#!/usr/bin/env python3
"""Release valve A v4 - mesh twin of release_valve_a_v4.fs + Bambu Studio 3MF writer.

One script for the whole series: pass the thread major diameters (and optional neck diameters)
on the command line, get one 3MF with all variants side by side.

  python3 release_valve_a_v4_3mf.py out.3mf 26.1 25.8 25.5
  python3 release_valve_a_v4_3mf.py out.3mf 26.1:23.2 25.8:22.9      (major:neck)

Neck diameter defaults to minor - 0.3 mm. Print settings written into the 3MF:
  layer height 0.28 mm for the head (z 0 .. 6), 0.08 mm from z 6 to the top (height range modifier),
  sparse infill 20 %. Printer / filament profile copied from the CAD repo's YValveSitzring.3mf (P1S, PLA).
z = 0 is the bed (head face). Thread is right-hand (checked).
"""
import io, json, math, sys, uuid, zipfile
from pathlib import Path

import numpy as np
from manifold3d import CrossSection, Manifold, Mesh

# ---------------- parameters (same as release_valve_a_v4.fs) ----------------
HEAD_H = 6.0
KNOB_R0, KNOB_AMP, KNOB_N, EDGE_CH = 24.5, 4.0, 6, 0.5
BORE_D, BORE_FLOOR = 18.0, 2.0
NECK_LEN = 10.0
P = 2.117
DEPTH = 0.6134 * P
TURNS = 11
LEAD = 0.5
FLANK = math.radians(30)
# openings
SLOT_H = 1.5                 # slot height (axial) - fits the thread groove, crests stay intact
NECK_SLOT_L, NECK_SLOTS_PER_ROW = 7.0, 4
NECK_ROW_PITCH = 2.25
THR_SLOT_L, THR_SLOTS_PER_TURN = 8.0, 2.5
LAYER_HEAD, LAYER_CYL, INFILL = 0.28, 0.08, "20%"
SEG = 360                    # segments per turn for helix / revolve


def tri_mesh(verts, faces):
    return Manifold(Mesh(vert_properties=np.asarray(verts, np.float32), tri_verts=np.asarray(faces, np.uint32)))


def knob():
    """Wavy star r = r0 + amp sin(n phi), z 0..h, 45 deg chamfer EDGE_CH top and bottom (normal offset)."""
    m = KNOB_N * 32
    phi = np.arange(m) * 2 * math.pi / m
    r = KNOB_R0 + KNOB_AMP * np.sin(KNOB_N * phi)
    pts = np.stack([r * np.cos(phi), r * np.sin(phi)], 1)
    tang = np.roll(pts, -1, 0) - np.roll(pts, 1, 0)
    nrm = np.stack([tang[:, 1], -tang[:, 0]], 1)
    nrm /= np.linalg.norm(nrm, axis=1)[:, None]          # outward (curve runs CCW)
    inner = pts - EDGE_CH * nrm
    rings = [(inner, 0.0), (pts, EDGE_CH), (pts, HEAD_H - EDGE_CH), (inner, HEAD_H)]
    V, F = [], []
    for ring, z in rings:
        V += [(x, y, z) for x, y in ring]
    for k in range(3):
        a, b = k * m, (k + 1) * m
        for i in range(m):
            j = (i + 1) % m
            F += [(a + i, a + j, b + j), (a + i, b + j, b + i)]
    c0, c1 = len(V), len(V) + 1
    V += [(0, 0, 0), (0, 0, HEAD_H)]
    top = 3 * m
    for i in range(m):
        j = (i + 1) % m
        F += [(c0, j, i), (c1, top + i, top + j)]
    return tri_mesh(V, F)


def revolve_profile(rz):
    """Solid of revolution from a closed (r, z) polygon (r >= 0)."""
    return Manifold.revolve(CrossSection([[(r, z) for r, z in rz]]), SEG)


def thread_sweep(r_root, r_crest, overlap_r, z_start, turns, right_hand=True):
    """Trapezoid tooth (as in the FeatureScript) swept along a helix. Right hand: z rises CCW (from +Z)."""
    crest = P / 8
    w_root = crest + 2 * (r_crest - r_root) * math.tan(FLANK)
    prof = [(overlap_r, -w_root / 2), (r_root, -w_root / 2), (r_crest, -crest / 2),
            (r_crest, crest / 2), (r_root, w_root / 2), (overlap_r, w_root / 2)]
    n = int(round(turns * SEG))
    s = 1 if right_hand else -1
    V, F = [], []
    for i in range(n + 1):
        t = i / SEG
        a = s * 2 * math.pi * t
        z0 = z_start + P * t
        V += [(r * math.cos(a), r * math.sin(a), z0 + dz) for r, dz in prof]
    k = len(prof)
    for i in range(n):
        for j in range(k):
            a0, a1 = i * k + j, i * k + (j + 1) % k
            b0, b1 = a0 + k, a1 + k
            F += [(a0, b0, b1), (a0, b1, a1)] if s > 0 else [(a0, b1, b0), (a0, a1, b1)]
    for cap, rev in ((0, s > 0), (n * k, s < 0)):
        for j in range(1, k - 1):
            tri = (cap, cap + j, cap + j + 1)
            F.append(tri if rev else tri[::-1])
    mf = tri_mesh(V, F)
    if mf.volume() < 0 or mf.status().name != "NoError":
        raise RuntimeError("thread sweep not manifold: " + mf.status().name)
    return mf


def radial_slot(phi_deg, zc, length, height, tilt, out_len):
    """Hexagonal slot (flat bridge top, 45 deg pointed ends) in the (tangent, z) plane, extruded radially
    from the axis outwards; tilted by `tilt` (rad) to follow the helix."""
    h2, l2 = height / 2, length / 2
    pts = [(-l2 + h2, -h2), (l2 - h2, -h2), (l2, 0), (l2 - h2, h2), (-l2 + h2, h2), (-l2, 0)]
    c, s = math.cos(tilt), math.sin(tilt)
    pts = [(x * c - y * s, x * s + y * c) for x, y in pts]
    ex = CrossSection([pts]).extrude(out_len)            # local x = tangent, y = up, z = radial
    phi = math.radians(phi_deg)
    d = (math.cos(phi), math.sin(phi), 0.0)
    t = (-math.sin(phi), math.cos(phi), 0.0)
    # world = x*t + y*Z + z*d + (0,0,zc)
    mtx = [[t[0], 0.0, d[0], 0.0], [t[1], 0.0, d[1], 0.0], [t[2], 1.0, d[2], zc]]
    return ex.transform(mtx)


def valve(major, neck_d=None, right_hand=True):
    minor = major - 2 * DEPTH
    neck_d = neck_d if neck_d else round(minor - 0.3, 3)
    rmC, rmR, rN, rB = major / 2, minor / 2, neck_d / 2, BORE_D / 2
    assert rN < rmR, "neck must be smaller than thread minor"
    z_neck = HEAD_H + NECK_LEN
    z_tip = z_neck + TURNS * P
    body = knob() + Manifold.cylinder(z_neck + 0.5 - (HEAD_H - 0.5), rN, rN, SEG).translate((0, 0, HEAD_H - 0.5))

    # shank: core + swept tooth, trimmed by 45 deg fade-in above the neck and 45 deg lead-in at the tip
    z_s = z_neck - P
    core = Manifold.cylinder(z_tip + 1 - (z_neck - 0.5), rmR, rmR, SEG).translate((0, 0, z_neck - 0.5))
    turns = (z_tip + 1 - z_s + 2 * P) / P
    male = core + thread_sweep(rmR, rmC, rN - 0.1, z_s, turns, right_hand)
    hF = (rmC + 1) - rN
    r_top = rmR - LEAD
    h_tip = (rmC + 1) - r_top
    keep = revolve_profile([(0, z_neck - 1), (rN, z_neck - 1), (rN, z_neck), (rmC + 1, z_neck + hF),
                            (rmC + 1, z_tip - h_tip), (r_top, z_tip), (0, z_tip)])
    male = male ^ keep
    body = body + male

    # bore, closed at the head
    body = body - Manifold.cylinder(z_tip + 1 - BORE_FLOOR, rB, rB, SEG).translate((0, 0, BORE_FLOOR))

    # openings: only in the upper half of the cylinder (neck + thread), lower half is plain thread
    cyl_len = NECK_LEN + TURNS * P
    z_zone = HEAD_H + cyl_len / 2
    out = rmC + 2
    cuts, info = [], {"area_neck_mm2": 0.0, "area_thread_mm2": 0.0}
    hex_area = lambda L, h: (L - h) * h + h * h / 2
    # neck: two staggered rows directly below the thread start (above the O-ring zone)
    rows = [z_neck - 1.25, z_neck - 1.25 - NECK_ROW_PITCH]
    for ri, zc in enumerate(rows):
        for i in range(NECK_SLOTS_PER_ROW):
            phi = i * 360 / NECK_SLOTS_PER_ROW + ri * 180 / NECK_SLOTS_PER_ROW
            cuts.append(radial_slot(phi, zc, NECK_SLOT_L, SLOT_H, 0.0, out))
            info["area_neck_mm2"] += hex_area(NECK_SLOT_L, SLOT_H)
    # thread: slots centred in the root groove, following the helix
    hand = 1 if right_hand else -1
    tilt = hand * math.atan(P / (2 * math.pi * (rmR + rmC) / 2))
    k = 0
    while True:
        zc = z_neck + P / 2 + k * P / THR_SLOTS_PER_TURN
        if zc + SLOT_H / 2 + 0.2 > z_zone:
            break
        cuts.append(radial_slot(hand * k * 360 / THR_SLOTS_PER_TURN, zc, THR_SLOT_L, SLOT_H, tilt, out))
        info["area_thread_mm2"] += hex_area(THR_SLOT_L, SLOT_H)
        k += 1
    info["thread_slots"] = k
    body = body - Manifold.batch_boolean(cuts, __import__("manifold3d").OpType.Add)

    info.update(major=major, minor=round(minor, 3), neck_d=neck_d, height=round(z_tip, 3), z_zone=round(z_zone, 2),
                z_neck=z_neck, neck_slot_z=(round(rows[1] - SLOT_H / 2, 2), round(rows[0] + SLOT_H / 2, 2)),
                thread_slot_z=(round(z_neck + P / 2 - SLOT_H / 2, 2),
                               round(z_neck + P / 2 + (k - 1) * P / THR_SLOTS_PER_TURN + SLOT_H / 2, 2)))
    return body, info


def helix_is_right_hand(right_hand=True):
    """Independent check on the swept tooth mesh: looking down from +Z, z must rise while the tooth
    turns counter-clockwise (angle measured CCW, first quarter turn)."""
    m = thread_sweep(11.0, 12.0, 10.5, 0.0, 0.25, right_hand).to_mesh()
    v = np.asarray(m.vert_properties)[:, :3]
    a = np.arctan2(v[:, 1], v[:, 0])
    if not right_hand:
        a = -a                      # left-hand sweep runs clockwise: unwrap into the same quarter
    sel = (a > -0.1) & (a < math.pi / 2 + 0.1)
    a = np.arctan2(v[sel, 1], v[sel, 0])
    slope = np.polyfit(a, v[sel, 2], 1)[0]          # mm per rad
    return abs(slope / (P / (2 * math.pi)) - 1) < 0.1


# ---------------- Bambu 3MF writer ----------------
def object_model_xml(mf):
    m = mf.to_mesh()
    v = np.asarray(m.vert_properties)[:, :3]
    f = np.asarray(m.tri_verts)
    out = io.StringIO()
    out.write('<?xml version="1.0" encoding="UTF-8"?>\n<model unit="millimeter" xml:lang="en-US" '
              'xmlns="http://schemas.microsoft.com/3dmanufacturing/core/2015/02" '
              'xmlns:BambuStudio="http://schemas.bambulab.com/package/2021" '
              'xmlns:p="http://schemas.microsoft.com/3dmanufacturing/production/2015/06" requiredextensions="p">\n'
              ' <metadata name="BambuStudio:3mfVersion">1</metadata>\n <resources>\n'
              f'  <object id="1" p:UUID="{uuid.uuid4()}" type="model">\n   <mesh>\n    <vertices>\n')
    out.writelines(f'     <vertex x="{x:.5f}" y="{y:.5f}" z="{z:.5f}"/>\n' for x, y, z in v)
    out.write('    </vertices>\n    <triangles>\n')
    out.writelines(f'     <triangle v1="{a}" v2="{b}" v3="{c}"/>\n' for a, b, c in f)
    out.write('    </triangles>\n   </mesh>\n  </object>\n </resources>\n <build/>\n</model>\n')
    return out.getvalue(), len(f)


def write_3mf(path, parts, template):
    """parts: list of (name, manifold, x, y). Positions are plate coordinates (P1S bed 256 x 256)."""
    with zipfile.ZipFile(template) as z:
        proj = json.loads(z.read("Metadata/project_settings.config"))
        ctypes = z.read("[Content_Types].xml")
    proj["layer_height"] = f"{LAYER_HEAD}"
    proj["sparse_infill_density"] = INFILL
    zf = zipfile.ZipFile(path, "w", zipfile.ZIP_DEFLATED)
    res, build, rels, settings, ranges, insts, assem = [], [], [], [], [], [], []
    for i, (name, mf, x, y) in enumerate(parts, 1):
        xml, nf = object_model_xml(mf)
        fn = f"3D/Objects/object_{i}.model"
        zf.writestr(fn, xml)
        rels.append(f' <Relationship Target="/{fn}" Id="rel-{i}" '
                    'Type="http://schemas.microsoft.com/3dmanufacturing/2013/01/3dmodel"/>')
        res.append(f'  <object id="{i}" p:UUID="{uuid.uuid4()}" type="model">\n   <components>\n'
                   f'    <component p:path="/{fn}" objectid="1" p:UUID="{uuid.uuid4()}" '
                   'transform="1 0 0 0 1 0 0 0 1 0 0 0"/>\n   </components>\n  </object>')
        build.append(f'  <item objectid="{i}" p:UUID="{uuid.uuid4()}" '
                     f'transform="1 0 0 0 1 0 0 0 1 {x} {y} 0" printable="1"/>')
        settings.append(f'  <object id="{i}">\n    <metadata key="name" value="{name}"/>\n'
                        '    <metadata key="extruder" value="1"/>\n'
                        f'    <metadata key="layer_height" value="{LAYER_HEAD}"/>\n'
                        f'    <metadata key="sparse_infill_density" value="{INFILL}"/>\n'
                        f'    <metadata face_count="{nf}"/>\n'
                        f'    <part id="{i}" subtype="normal_part" uuid="{uuid.uuid4()}">\n'
                        f'      <metadata key="name" value="{name}"/>\n'
                        '      <metadata key="matrix" value="1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1"/>\n'
                        f'      <mesh_stat face_count="{nf}" edges_fixed="0" degenerate_facets="0" '
                        'facets_removed="0" facets_reversed="0" backwards_edges="0"/>\n    </part>\n  </object>')
        top = round(mf.bounding_box()[5] + 1, 3)
        ranges.append(f' <object id="{i}">\n  <range min_z="{HEAD_H}" max_z="{top}">\n'
                      '   <option opt_key="extruder">1</option>\n'
                      f'   <option opt_key="layer_height">{LAYER_CYL}</option>\n  </range>\n </object>')
        insts.append(f'    <model_instance>\n      <metadata key="object_id" value="{i}"/>\n'
                     f'      <metadata key="instance_id" value="0"/>\n'
                     f'      <metadata key="identify_id" value="{100 + i}"/>\n    </model_instance>')
        assem.append(f'   <assemble_item object_id="{i}" instance_id="0" '
                     f'transform="1 0 0 0 1 0 0 0 1 {x} {y} 0" offset="0 0 0" />')
    ns = ('xmlns="http://schemas.microsoft.com/3dmanufacturing/core/2015/02" '
          'xmlns:BambuStudio="http://schemas.bambulab.com/package/2021" '
          'xmlns:p="http://schemas.microsoft.com/3dmanufacturing/production/2015/06" requiredextensions="p"')
    zf.writestr("3D/3dmodel.model",
                f'<?xml version="1.0" encoding="UTF-8"?>\n<model unit="millimeter" xml:lang="en-US" {ns}>\n'
                ' <metadata name="Application">BambuStudio-02.09.01.70</metadata>\n'
                ' <metadata name="BambuStudio:3mfVersion">1</metadata>\n'
                ' <metadata name="Title">Release valve A v4 series</metadata>\n <resources>\n'
                + "\n".join(res) + f'\n </resources>\n <build p:UUID="{uuid.uuid4()}">\n' + "\n".join(build)
                + "\n </build>\n</model>\n")
    zf.writestr("3D/_rels/3dmodel.model.rels", '<?xml version="1.0" encoding="UTF-8"?>\n<Relationships '
                'xmlns="http://schemas.openxmlformats.org/package/2006/relationships">\n' + "\n".join(rels)
                + "\n</Relationships>\n")
    zf.writestr("[Content_Types].xml", ctypes)
    zf.writestr("_rels/.rels", '<?xml version="1.0" encoding="UTF-8"?>\n<Relationships '
                'xmlns="http://schemas.openxmlformats.org/package/2006/relationships">\n'
                ' <Relationship Target="/3D/3dmodel.model" Id="rel-1" '
                'Type="http://schemas.microsoft.com/3dmanufacturing/2013/01/3dmodel"/>\n</Relationships>\n')
    zf.writestr("Metadata/model_settings.config", '<?xml version="1.0" encoding="UTF-8"?>\n<config>\n'
                + "\n".join(settings) + '\n  <plate>\n    <metadata key="plater_id" value="1"/>\n'
                '    <metadata key="plater_name" value="Release valve A v4"/>\n'
                '    <metadata key="locked" value="false"/>\n' + "\n".join(insts)
                + "\n  </plate>\n  <assemble>\n" + "\n".join(assem) + "\n  </assemble>\n</config>\n")
    zf.writestr("Metadata/layer_config_ranges.xml", '<?xml version="1.0" encoding="utf-8"?>\n<objects>\n'
                + "\n".join(ranges) + "\n</objects>\n")
    zf.writestr("Metadata/project_settings.config", json.dumps(proj, indent=4))
    zf.close()


def main():
    out = Path(sys.argv[1])
    specs = []
    for a in sys.argv[2:]:
        major, _, neck = a.partition(":")
        specs.append((float(major), float(neck) if neck else None))
    if not helix_is_right_hand(True):
        raise SystemExit("thread hand check failed")
    template = Path(__file__).resolve().parents[2] / "YValveSitzring.3mf"
    parts, infos = [], []
    spacing = 2 * (KNOB_R0 + KNOB_AMP) + 8
    x0 = 128 - spacing * (len(specs) - 1) / 2
    for i, (major, neck) in enumerate(specs):
        mf, info = valve(major, neck)
        tag = chr(ord("a") + i)
        name = f"RV-A v4{tag} OD{major:g}"
        parts.append((name, mf, round(x0 + i * spacing, 3), 128))
        info.update(name=name, volume_cm3=round(mf.volume() / 1000, 2), genus=mf.genus(), status=mf.status().name)
        infos.append(info)
    write_3mf(out, parts, template)
    print(json.dumps(infos, indent=1))


if __name__ == "__main__":
    main()
