"""Druck 17.11.2026: LZQ-Platte A+B (11 x 7 Löcher) + 18 NEMA-Steg-Zylinder + 18 LZQ-Steg-Zylinder, Bambu-3MF, 0,20-mm-Schichten.

LZQ-Platte 64 x 96 x 6 mm: Reihen (Einzelkerben, 11) = großer Ø 4,14-4,34 (0,02), Spalten (Doppelkerben, 7) = Schlüsselweite 3,08-3,20 (0,02).
  Abgeschrägte Ecke = Reihe 1 / Spalte 1. Exemplar A: 1 Loch, B: 2 Löcher in der Gegenecke.
Zylinder: Opferstege auf der Abflachung, Abstand 2 mm, Tiefe 0,15-0,40 (0,05) x Höhe 0,2/0,4/0,6.
  NEMA: Zylinder Ø 11, Grundbohrung Ø 5,26 / Abflachung 2,10.   LZQ: Zylinder Ø 9, Grundbohrung Ø 4,34 / SW 3,24.
  Sockel 20 x 20: Einzelkerben = Tiefe-Stufe (1-6), Löcher = Höhe-Stufe (1-3).

Aufruf:  python3 druck2_3mf.py   (Vorlage ../druck/passungsraster.3mf, Ausgabe ../druck/druck2_2026-11-17.3mf)
"""
import numpy as np, trimesh, json, zipfile, uuid, os, math

HERE = os.path.dirname(os.path.abspath(__file__))
TEMPLATE = os.environ.get('BAMBU_VORLAGE', os.path.join(HERE, '..', 'druck', 'passungsraster.3mf'))
OUT = os.path.join(HERE, '..', 'druck', 'druck2_2026-11-17.3mf')

CH = 0.5                    # Fase unten an jeder Bohrung (Elefantenfuß)
NW, ND, NP = 1.0, 1.5, 2.0  # Kerbe: Breite, Tiefe, Abstand der beiden Kerben einer Doppelkerbe
CORNER = 5.0                # Schenkel der Eckschräge (Platten)
MARK_D = 1.5                # Exemplar-/Höhe-Löcher

def cyl(r, z0, z1, x, y, n=128):
    m = trimesh.creation.cylinder(radius=r, height=z1 - z0, sections=n); m.apply_translation([x, y, (z0 + z1) / 2]); return m
def cone(r0, r1, z0, z1, x, y, n=128):
    m = trimesh.creation.revolve(np.array([(0, z0), (r0, z0), (r1, z1), (0, z1)]), sections=n)
    if m.volume < 0: m.invert()
    m.apply_translation([x, y, 0]); return m
def box(x0, x1, y0, y1, z0, z1):
    b = trimesh.creation.box(extents=[x1 - x0, y1 - y0, z1 - z0]); b.apply_translation([(x0 + x1) / 2, (y0 + y1) / 2, (z0 + z1) / 2]); return b
U = lambda ms: trimesh.boolean.union(ms, engine='manifold')
D = lambda a, ms: trimesh.boolean.difference([a] + ms, engine='manifold')

def notches_single(n, centres, T, edge):        # edge 'x0' = linker Rand (Kerben quer zu y), 'y0' = vorderer Rand
    out = []
    for c in centres[:n] if isinstance(centres, list) else centres:
        out.append(box(-1, ND, c - NW / 2, c + NW / 2, -1, T + 1) if edge == 'x0' else box(c - NW / 2, c + NW / 2, -1, ND, -1, T + 1))
    return out
def notch_double(c, T, edge):
    return [box(-1, ND, o - NW / 2, o + NW / 2, -1, T + 1) if edge == 'x0' else box(o - NW / 2, o + NW / 2, -1, ND, -1, T + 1) for o in (c - NP / 2, c + NP / 2)]

def corner_cut(T):
    s = CORNER * math.sqrt(2)
    b = box(-s, s, -s, 0, -1, T + 1)
    b.apply_transform(trimesh.transformations.rotation_matrix(math.radians(-45), [0, 0, 1]))
    b.apply_translation([CORNER / 2, CORNER / 2, 0]); return b

def d_hole(x, y, d, f, z1):
    """Bohrung Ø d mit Abflachung im Abstand f (nach +y), Fase unten. Rückgabe: (Schnittkörper, Keile)."""
    r = d / 2
    return [cyl(r, -1, z1 + 1, x, y), cone(r + CH, r, -0.01, CH, x, y)], [box(x - r - 0.3, x + r + 0.3, y + f, y + r + 0.6, CH, z1)]

def plate(dias, flats, pitch, T, copy):
    n = len(dias); W = 16.0 + pitch * (n - 1)
    cuts, keys = [], []
    for i, d in enumerate(dias):
        for j, f in enumerate(flats):
            c, k = d_hole(8 + j * pitch, 8 + i * pitch, d, f, T); cuts += c; keys += k
    for i in range(n):
        c = 8 + i * pitch
        cuts += notches_single(1, [c], T, 'x0')          # Reihen: eine Kerbe je Reihe (links)
        cuts += notch_double(c, T, 'y0')                  # Spalten: Doppelkerbe je Spalte (vorn)
    cuts.append(corner_cut(T))
    for k in range(copy):                                 # Exemplar: 1 oder 2 Löcher in der Ecke gegenüber der Schräge
        cuts.append(cyl(MARK_D / 2, -1, T + 1, W - 3.5 - 3.0 * k, W - 3.5, n=32))
    m = U([D(box(0, W, 0, W, 0, T), cuts)] + keys)
    assert m.is_watertight and len(m.split()) == 1
    return m, W

def plate_rc(dias, sws, pitch, T, copy):
    """Rechteckige Platte: Reihen = dias (y), Spalten = Schlüsselweiten (x). Abflachung bei SW - Ø/2 nach +y."""
    W, H = 16.0 + pitch * (len(sws) - 1), 16.0 + pitch * (len(dias) - 1)
    cuts, keys = [], []
    for i, d in enumerate(dias):
        for j, sw in enumerate(sws):
            c, k = d_hole(8 + j * pitch, 8 + i * pitch, d, sw - d / 2, T); cuts += c; keys += k
    for i in range(len(dias)): cuts += notches_single(1, [8 + i * pitch], T, 'x0')
    for j in range(len(sws)): cuts += notch_double(8 + j * pitch, T, 'y0')
    cuts.append(corner_cut(T))
    for k in range(copy): cuts.append(cyl(MARK_D / 2, -1, T + 1, W - 3.5 - 3.0 * k, H - 3.5, n=32))
    m = U([D(box(0, W, 0, H, 0, T), cuts)] + keys)
    assert m.is_watertight and len(m.split()) == 1
    return m

# ---------- Zylinder mit Sockel ----------
PED, PED_T, CYL_H = 20.0, 2.4, 8.0
TIEFE = [0.15, 0.20, 0.25, 0.30, 0.35, 0.40]
HOEHE = [0.2, 0.4, 0.6]
ABSTAND = 2.0
WEB_Z0 = 1.0
KINDS = {'NEMA': (11.0, 5.26, 2.10), 'LZQ': (9.0, 4.34, 3.24 - 4.34 / 2)}    # Zylinder-Ø, Bohrungs-Ø, Abflachung (Abstand zur Achse)

def cylinder_part(kind, it, ih):
    od, bd, bf = KINDS[kind]; c = PED / 2; top = PED_T + CYL_H; r = bd / 2
    body = U([box(0, PED, 0, PED, 0, PED_T), cyl(od / 2, PED_T - 0.01, top, c, c)])
    cuts, keys = d_hole(c, c, bd, bf, top)
    t, h = TIEFE[it], HOEHE[ih]
    z = WEB_Z0
    while z + h <= top - 0.4 + 1e-9:
        keys.append(box(c - r - 0.3, c + r + 0.3, c + bf - t, c + bf + 0.01, z, z + h))
        z = round(z + ABSTAND, 3)
    lv = lambda k, sp: [PED / 2 + (q - (k - 1) / 2) * sp for q in range(k)]
    for x in lv(it + 1, 3.0): cuts += notches_single(1, [x], PED_T, 'x0')                     # Tiefe: Einzelkerben links
    for x in lv(ih + 1, 4.0): cuts.append(cyl(MARK_D / 2, -1, PED_T + 1, x, PED - 2.2, n=32))  # Höhe: Löcher hinten
    m = U([D(body, cuts)] + keys)
    assert m.is_watertight and len(m.split()) == 1, (kind, it, ih)
    return m

# ---------- Teile ----------
DIAS = [round(4.14 + 0.02 * i, 2) for i in range(11)]
SWS = [round(3.08 + 0.02 * j, 2) for j in range(7)]
PITCH, PT = 8.0, 6.0
parts = []
for copy, cn, x in ((1, 'A', 4.0), (2, 'B', 74.0)):
    parts.append(('LZQ-Platte ' + cn, plate_rc(DIAS, SWS, PITCH, PT, copy), x, 156.0))
k = 0
for kind in ('NEMA', 'LZQ'):
    for it in range(6):
        for ih in range(3):
            row, col = divmod(k, 9)
            parts.append(('%s T%d H%d (Tiefe %.2f, Höhe %.1f)' % (kind, it + 1, ih + 1, TIEFE[it], HOEHE[ih]),
                          cylinder_part(kind, it, ih), 4.0 + col * 23.0, 126.0 - row * 23.0))
            k += 1

# Prüfung Platte: jedes Loch Ø und Abflachung nachmessen
m = parts[0][1]
sec = m.section(plane_origin=[0, 0, PT / 2], plane_normal=[0, 0, 1])
holes = [np.asarray(sec.vertices)[e.points][:, :2] for e in sec.entities]
holes = [p for p in holes if 3 < np.ptp(p[:, 0]) < 6]
assert len(holes) == len(DIAS) * len(SWS), len(holes)
for p in holes:
    cx, cy = p.mean(axis=0); j, i = int(round((cx - 8) / PITCH)), int(round((cy - 8) / PITCH))
    y0 = 8 + i * PITCH
    assert abs(np.ptp(p[:, 0]) - DIAS[i]) < 0.01 and abs(p[:, 1].max() - y0 + DIAS[i] / 2 - SWS[j]) < 1e-3, (i, j)
# Prüfung Zylinder: Stegfläche und Grundabflachung
for name, m, x, y in parts[2:]:
    kind = name.split()[0]; it = int(name.split()[1][1]) - 1; od, bd, bf = KINDS[kind]; c = PED / 2
    def flat_at(z):
        s = m.section(plane_origin=[0, 0, z], plane_normal=[0, 0, 1])
        return max(np.asarray(s.vertices)[e.points][:, 1].max() for e in s.entities if 3 < np.ptp(np.asarray(s.vertices)[e.points][:, 0]) < 6) - c
    assert abs(flat_at(WEB_Z0 + 0.1) - (bf - TIEFE[it])) < 1e-3 and abs(flat_at(WEB_Z0 - 0.3) - bf) < 1e-3, name
for name, m, x, y in parts:
    b = m.bounds; assert x + b[0][0] >= 0 and y + b[0][1] >= 0 and x + b[1][0] <= 256 and y + b[1][1] <= 256, name
print('Prüfung ok:', len(parts), 'Teile')

# ---------- 3MF (Aufbau wie passungsraster_3mf.py) ----------
NS = 'xmlns="http://schemas.microsoft.com/3dmanufacturing/core/2015/02" xmlns:BambuStudio="http://schemas.bambulab.com/package/2021" xmlns:p="http://schemas.microsoft.com/3dmanufacturing/production/2015/06" requiredextensions="p"'
def mesh_xml(m, oid=1):
    vv = '\n'.join('     <vertex x="%.5f" y="%.5f" z="%.5f"/>' % tuple(p) for p in m.vertices)
    tt = '\n'.join('     <triangle v1="%d" v2="%d" v3="%d"/>' % tuple(f) for f in m.faces)
    return ('<?xml version="1.0" encoding="UTF-8"?>\n<model unit="millimeter" xml:lang="en-US" %s>\n <metadata name="BambuStudio:3mfVersion">1</metadata>\n <resources>\n'
            '  <object id="%d" p:UUID="%s" type="model">\n   <mesh>\n    <vertices>\n%s\n    </vertices>\n    <triangles>\n%s\n    </triangles>\n   </mesh>\n  </object>\n </resources>\n <build/>\n</model>\n') % (NS, oid, uuid.uuid4(), vv, tt)
zin = zipfile.ZipFile(TEMPLATE); files = {}
objs = []
for i, (name, m, x, y) in enumerate(parts):
    files['3D/Objects/part_%d.model' % i] = mesh_xml(m, 2 * i + 1)   # eigene ID je Datei (sonst lädt Bambu überall das erste Netz)
    objs.append((2 * i + 2, i, x, y, name, len(m.faces)))
tpl = zin.read('3D/3dmodel.model').decode(); head = tpl[:tpl.index(' <resources>')]
res = [' <resources>']
for o, vi, x, y, name, nf in objs:
    res.append('  <object id="%d" p:UUID="%s" type="model">\n   <components>\n    <component p:path="/3D/Objects/part_%d.model" objectid="%d" p:UUID="%s" transform="1 0 0 0 1 0 0 0 1 0 0 0"/>\n   </components>\n  </object>' % (o, uuid.uuid4(), vi, 2 * vi + 1, uuid.uuid4()))
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
proc = {'layer_height': '0.2', 'wall_loops': '3', 'enable_support': '0', 'brim_type': 'no_brim'}
d.update(proc)
diff = list(d['different_settings_to_system'])
diff[0] = ';'.join(sorted((set(x for x in diff[0].split(';') if x) | set(proc)) - {'support_on_build_plate_only', 'support_remove_small_overhang'}))
d['different_settings_to_system'] = diff
files['Metadata/project_settings.config'] = json.dumps(d, indent=4, ensure_ascii=False)
skip_prefix = ('3D/Objects/',)                               # alte Objekte der Vorlage weglassen
zout = zipfile.ZipFile(OUT, 'w', zipfile.ZIP_DEFLATED, compresslevel=9); done = set()
for it in zin.infolist():
    n = it.filename
    if n.startswith(skip_prefix) and n not in files: continue
    if n in files: zout.writestr(n, files[n]); done.add(n)
    else: zout.writestr(it, zin.read(n))
for n, c in files.items():
    if n not in done: zout.writestr(n, c)
zout.close()
print('ok', os.path.normpath(OUT), os.path.getsize(OUT))
json.dump([dict(name=n, x=x, y=y, v=np.round(m.vertices, 3).tolist(), f=m.faces.tolist()) for n, m, x, y in parts], open('/tmp/druck2_meshes.json', 'w'))
