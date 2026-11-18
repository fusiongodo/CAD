"""Testdruck 18.11.2026: 7 Deckelringe für das gedruckte Unterteil v2 (Hals + Rille bleiben).

Parameter je Ring: Spiel c (je Seite) und Versatz p der Rastnase nach oben (Vorspannung auf den Absatz).
Nasenhöhe h = c + 0,35 → Überdeckung beim Aufschieben bleibt 0,35 mm wie v2 (Haltekraft war gut).
Ring = Deckelrand 9 mm hoch (Wand 1,6) + 6 mm breiter Kragen oben (1,6) als Ersatz für die Deckelplatte.
Kennung: Kerben am freien Rand der kurzen Seite (+x) = Ringnummer.
Aufruf: python3 deckelringe_3mf.py  → ../druck/deckelringe_2026-11-18.3mf
"""
import cadquery as cq, numpy as np, trimesh, json, zipfile, uuid, os, io, tempfile

HERE = os.path.dirname(os.path.abspath(__file__)); DRUCK = os.path.join(HERE, '..', 'druck')
TEMPLATE = os.path.join(HERE, '..', '..', 'ihht-gasmischer', 'druck', 'passungsraster.3mf')
OUT = os.path.join(DRUCK, 'deckelringe_2026-11-18.3mf')

# Maße Unterteil v2 (gedruckt)
L, W, R_OUT, H_BASE, NECK_H = 82.4, 63.4, 4.0, 24.0, 7.0
LID_WALL0, LID_CLR0 = 1.6, 0.25
neck_L, neck_W = L - 2*(LID_WALL0 + LID_CLR0), W - 2*(LID_WALL0 + LID_CLR0)
r_neck = R_OUT - LID_WALL0 - LID_CLR0
ledge_z, SNAP_Z = H_BASE - NECK_H, H_BASE - NECK_H + 3.0
SNAP_H, SNAP_L, OVERLAP = 2.0, 30.0, 0.35
WALL, RING_H, COLLAR_W, COLLAR_T = 1.6, 9.0, 6.0, 1.6

RINGS = [  # Nr, Spiel c, Versatz p
    (1, 0.18, 0.25), (2, 0.12, 0.25), (3, 0.06, 0.25),
    (4, 0.18, 0.40), (5, 0.12, 0.40), (6, 0.06, 0.40),
    (7, 0.25, 0.00),   # Referenz = Deckel v2
]

def rrect(l, w, r, h, z0=0):
    return cq.Workplane("XY").workplane(offset=z0).sketch().rect(l, w).vertices().fillet(r).finalize().extrude(h)
def ridge(sign_y, y_face, z_peak, depth, length, h):
    d = depth * sign_y
    pts = [(y_face, z_peak - h/2), (y_face + d, z_peak), (y_face, z_peak + h/2)]
    return cq.Workplane("YZ").workplane(offset=-length/2).polyline(pts).close().extrude(length)

def ring(nr, c, p):
    iL, iW, ri = neck_L + 2*c, neck_W + 2*c, r_neck + c
    oL, oW, ro = iL + 2*WALL, iW + 2*WALL, ri + WALL
    top = ledge_z + RING_H
    m = rrect(oL, oW, ro, RING_H + COLLAR_T, ledge_z).cut(rrect(iL, iW, ri, RING_H + 0.01, ledge_z - 0.01))
    m = m.cut(rrect(iL - 2*COLLAR_W, iW - 2*COLLAR_W, max(ri - COLLAR_W, 0.5), COLLAR_T + 0.2, top - 0.1))
    for s in (1, -1):
        m = m.union(ridge(s, s*(iW/2 + 0.01), SNAP_Z + p, -(c + OVERLAP + 0.01), SNAP_L, SNAP_H))
    for k in range(nr):   # Kerben 1 × 2 mm am freien Rand, kurze Seite +x
        y = (k - (nr - 1)/2) * 3.0
        m = m.cut(cq.Workplane("XY").box(WALL + 1, 1.0, 2.0).translate((oL/2 - WALL/2, y, ledge_z + 1.0 - 0.01)))
    return m

base = cq.importers.importStep(os.path.join(DRUCK, 'fiolenbox_v2_unterteil.step'))
def ov(a, b, dz):
    i = a.intersect(b.translate((0, 0, dz))); return sum(s.Volume() for s in i.solids().vals())

parts = []
tmp = tempfile.mkdtemp()
for nr, c, p in RINGS:
    m = ring(nr, c, p)
    assert m.val().isValid() and len(m.solids().vals()) == 1, nr
    o0, o1, o5 = ov(base, m, 0), ov(base, m, 1.5), ov(base, m, 5)
    print(f"Ring {nr}: Spiel {c:.2f}  Versatz {p:.2f}  Nase {c+OVERLAP:.2f}  | Überdeckung sitzend {o0:5.2f} mm³, bei 1,5 mm Hub {o1:5.2f}, bei 5 mm {o5:.2f}")
    assert o5 < 1e-3 and o1 > 1
    f = os.path.join(tmp, f'r{nr}.stl'); cq.exporters.export(m, f, tolerance=0.02, angularTolerance=0.1)
    t = trimesh.load(f, force='mesh'); t.merge_vertices()
    t.apply_transform(trimesh.transformations.rotation_matrix(np.pi, [1, 0, 0]))   # Kragen aufs Bett
    t.apply_translation(-t.bounds[0]); assert t.is_watertight
    parts.append((f'Deckelring {nr} (Spiel {c:.2f}, Versatz {p:.2f})', t))

# Anordnung: Spalte 1 quer (3 Ringe), Spalten 2+3 längs gedreht (je 2 Ringe)
G = 8.0
rot = trimesh.transformations.rotation_matrix(np.pi/2, [0, 0, 1])
placed = []
x0 = (256 - (parts[0][1].extents[0] + 2*parts[0][1].extents[1] + 2*G)) / 2
for i in range(3):
    n, t = parts[i]; placed.append((n, t, x0, (256 - 3*t.extents[1] - 2*G)/2 + i*(t.extents[1] + G)))
xc = x0 + parts[0][1].extents[0] + G
for j, i in enumerate(range(3, 7)):
    n, t = parts[i]; t = t.copy(); t.apply_transform(rot); t.apply_translation(-t.bounds[0])
    col, row = j // 2, j % 2
    placed.append((n, t, xc + col*(t.extents[0] + G), (256 - 2*t.extents[1] - G)/2 + row*(t.extents[1] + G)))
for n, t, x, y in placed:
    assert x >= 2 and y >= 2 and x + t.extents[0] <= 254 and y + t.extents[1] <= 254, (n, x, y, t.extents)
print('Volumen gesamt %.1f cm³' % (sum(t.volume for _, t, _, _ in placed) / 1000))
parts = [(n, t, x, y) for n, t, x, y in placed]

# ---------- 3MF (Aufbau wie fiolenbox_v2_3mf.py) ----------
NS = 'xmlns="http://schemas.microsoft.com/3dmanufacturing/core/2015/02" xmlns:BambuStudio="http://schemas.bambulab.com/package/2021" xmlns:p="http://schemas.microsoft.com/3dmanufacturing/production/2015/06" requiredextensions="p"'
def mesh_xml(m, oid):
    vv = '\n'.join('     <vertex x="%.5f" y="%.5f" z="%.5f"/>' % tuple(q) for q in m.vertices)
    tt = '\n'.join('     <triangle v1="%d" v2="%d" v3="%d"/>' % tuple(f) for f in m.faces)
    return ('<?xml version="1.0" encoding="UTF-8"?>\n<model unit="millimeter" xml:lang="en-US" %s>\n <metadata name="BambuStudio:3mfVersion">1</metadata>\n <resources>\n'
            '  <object id="%d" p:UUID="%s" type="model">\n   <mesh>\n    <vertices>\n%s\n    </vertices>\n    <triangles>\n%s\n    </triangles>\n   </mesh>\n  </object>\n </resources>\n <build/>\n</model>\n') % (NS, oid, uuid.uuid4(), vv, tt)
zin = zipfile.ZipFile(TEMPLATE); files = {}; objs = []
for i, (name, m, x, y) in enumerate(parts):
    files['3D/Objects/part_%d.model' % i] = mesh_xml(m, 2*i + 1); objs.append((2*i + 2, i, x, y, name, len(m.faces)))
tpl = zin.read('3D/3dmodel.model').decode(); head = tpl[:tpl.index(' <resources>')]
res = [' <resources>']
for o, vi, x, y, name, nf in objs:
    res.append('  <object id="%d" p:UUID="%s" type="model">\n   <components>\n    <component p:path="/3D/Objects/part_%d.model" objectid="%d" p:UUID="%s" transform="1 0 0 0 1 0 0 0 1 0 0 0"/>\n   </components>\n  </object>' % (o, uuid.uuid4(), vi, 2*vi + 1, uuid.uuid4()))
res += [' </resources>', ' <build p:UUID="%s">' % uuid.uuid4()]
for o, vi, x, y, name, nf in objs:
    res.append('  <item objectid="%d" p:UUID="%s" transform="1 0 0 0 1 0 0 0 1 %.4f %.4f 0" printable="1"/>' % (o, uuid.uuid4(), x, y))
res.append(' </build>\n</model>\n')
files['3D/3dmodel.model'] = head + '\n'.join(res)
files['3D/_rels/3dmodel.model.rels'] = '<?xml version="1.0" encoding="UTF-8"?>\n<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">\n' + \
    '\n'.join(' <Relationship Target="/3D/Objects/part_%d.model" Id="rel-%d" Type="http://schemas.microsoft.com/3dmanufacturing/2013/01/3dmodel"/>' % (i, i + 1) for i in range(len(parts))) + '\n</Relationships>\n'
ms = ['<?xml version="1.0" encoding="UTF-8"?>', '<config>']
for o, vi, x, y, name, nf in objs:
    ms += ['  <object id="%d">' % o, '    <metadata key="name" value="%s"/>' % name, '    <metadata key="extruder" value="1"/>', '    <metadata face_count="%d"/>' % nf,
           '    <part id="1" subtype="normal_part" uuid="%s">' % uuid.uuid4(), '      <metadata key="name" value="%s"/>' % name,
           '      <metadata key="matrix" value="1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1"/>', '      <mesh_stat face_count="%d" edges_fixed="0" degenerate_facets="0" facets_removed="0" facets_reversed="0" backwards_edges="0"/>' % nf,
           '    </part>', '  </object>']
ms += ['  <plate>', '    <metadata key="plater_id" value="1"/>', '    <metadata key="plater_name" value=""/>', '    <metadata key="locked" value="false"/>',
       '    <metadata key="filament_map_mode" value="Auto For Flush"/>', '    <metadata key="filament_maps" value="1 1 1"/>', '    <metadata key="filament_volume_maps" value="0 0 0"/>']
for i, (o, vi, x, y, name, nf) in enumerate(objs):
    ms += ['    <model_instance>', '      <metadata key="object_id" value="%d"/>' % o, '      <metadata key="instance_id" value="0"/>', '      <metadata key="identify_id" value="%d"/>' % (2000 + i), '    </model_instance>']
ms += ['  </plate>', '  <assemble>']
for o, vi, x, y, name, nf in objs:
    ms += ['   <assemble_item object_id="%d" instance_id="0" transform="1 0 0 0 1 0 0 0 1 %.4f %.4f 0" offset="0 0 0" />' % (o, x, y)]
ms += ['  </assemble>', '</config>', '']
files['Metadata/model_settings.config'] = '\n'.join(ms)
files['Metadata/cut_information.xml'] = '<?xml version="1.0" encoding="utf-8"?>\n<objects>\n' + ''.join(' <object id="%d">\n  <cut_id id="0" check_sum="1" connectors_cnt="0"/>\n </object>\n' % (i + 1) for i in range(len(objs))) + '</objects>\n'
d = json.loads(zin.read('Metadata/project_settings.config'))
proc = {'layer_height': '0.2', 'wall_loops': '3', 'sparse_infill_density': '15%', 'enable_support': '0', 'brim_type': 'no_brim'}
d.update(proc)
diff = list(d['different_settings_to_system'])
diff[0] = ';'.join(sorted((set(x for x in diff[0].split(';') if x) | set(proc)) - {'support_on_build_plate_only', 'support_remove_small_overhang'}))
d['different_settings_to_system'] = diff
files['Metadata/project_settings.config'] = json.dumps(d, indent=4, ensure_ascii=False)
zout = zipfile.ZipFile(OUT, 'w', zipfile.ZIP_DEFLATED, compresslevel=9); done = set()
for it in zin.infolist():
    n = it.filename
    if n.startswith('3D/Objects/') and n not in files: continue
    if n in files: zout.writestr(n, files[n]); done.add(n)
    else: zout.writestr(it, zin.read(n))
for n, c in files.items():
    if n not in done: zout.writestr(n, c)
zout.close()
print('ok', os.path.normpath(OUT), os.path.getsize(OUT) // 1024, 'KB')
json.dump([dict(name=n, x=x, y=y, ext=list(t.extents)) for n, t, x, y in parts], open('/tmp/ringe_layout.json', 'w'))
