"""Nachtdruck 16./17.11.2026: Passungs-Testteile für NEMA-Welle und LZQ-Spindel als eine Bambu-3MF (0,20-mm-Schichten).

Teile
  - LZQ-Raster A + B   70 x 70 x 6 mm, 7 x 7 D-Löcher (Ø 3,70-4,18 / Abflachung 0,82-1,18)
  - NEMA-Platte A + B  64 x 64 x 5 mm, 5 x 5 D-Löcher (Ø 5,10-5,26 / Abflachung 1,90-2,10)
  - 27 NEMA-Zylinder mit Sockel, Methode 2 (Opferstege nur auf der Abflachung):
      Grundbohrung Ø 5,26 / Abflachung 2,10; Stege: Tiefe 0,05/0,10/0,15 x Abstand 1/2/3 mm x Höhe 0,2/0,4/0,6 mm

Lesen der Platten: abgeschrägte Ecke = Reihe 1 / Spalte 1. Einzelkerben = Reihen = Bohrungs-Ø, Doppelkerben = Spalten = Abstand
  Abflachung-Achse. Abflachungen zeigen von der Doppelkerben-Kante weg. Exemplar A: 1 kleines Loch in der Ecke gegenüber der
  Schräge, Exemplar B: 2 Löcher.
Lesen der Zylinder (am Sockel, Lage egal): Einzelkerben = Tiefe-Stufe, Doppelkerben = Abstand-Stufe, kleine Löcher = Höhe-Stufe.

Aufruf:  python3 nachtdruck_3mf.py          (Vorlage: ../druck/passungsraster.3mf, Ausgabe: ../druck/nachtdruck_2026-11-16.3mf)
"""
import numpy as np, trimesh, json, zipfile, uuid, os, math

HERE = os.path.dirname(os.path.abspath(__file__))
TEMPLATE = os.environ.get('BAMBU_VORLAGE', os.path.join(HERE, '..', 'druck', 'passungsraster.3mf'))
OUT = os.path.join(HERE, '..', 'druck', 'nachtdruck_2026-11-16.3mf')

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

# ---------- Zylinder mit Sockel ----------
PED, PED_T = 18.0, 2.4      # Sockel 18 x 18 x 2,4
CYL_OD, CYL_H = 11.0, 8.0   # Zylinder darüber
BASE_D, BASE_F = 5.26, 2.10 # Grundbohrung (lockerer als bester Sitz Ø 5,18 / 2,05)
TIEFE = [0.05, 0.10, 0.15]
ABSTAND = [1.0, 2.0, 3.0]
HOEHE = [0.2, 0.4, 0.6]
WEB_Z0 = 1.0                # erster Steg ab z 1,0 (über der Fase, auf Schichtraster 0,2)

def cylinder_part(it, ia, ih):
    c = PED / 2; top = PED_T + CYL_H; r = BASE_D / 2
    body = U([box(0, PED, 0, PED, 0, PED_T), cyl(CYL_OD / 2, PED_T - 0.01, top, c, c)])
    cuts, keys = d_hole(c, c, BASE_D, BASE_F, top)
    t, a, h = TIEFE[it], ABSTAND[ia], HOEHE[ih]
    z = WEB_Z0
    while z + h <= top - 0.4 + 1e-9:                       # Opferstege: Fläche bei BASE_F - t, nur über die Breite der Abflachung
        keys.append(box(c - r - 0.3, c + r + 0.3, c + BASE_F - t, c + BASE_F + 0.01, z, z + h))
        z = round(z + a, 3)
    lv = lambda k: [PED / 2 + (q - (k - 1) / 2) * 5.0 for q in range(k)]
    for x in lv(it + 1): cuts += notches_single(1, [x], PED_T, 'x0')        # Tiefe: Einzelkerben links
    for x in lv(ia + 1): cuts += notch_double(x, PED_T, 'y0')               # Abstand: Doppelkerben vorn
    for x in lv(ih + 1): cuts.append(cyl(MARK_D / 2, -1, PED_T + 1, x, PED - 2.2, n=32))   # Höhe: Löcher hinten
    m = U([D(body, cuts)] + keys)
    assert m.is_watertight and len(m.split()) == 1, (it, ia, ih)
    return m

# ---------- Teile bauen + prüfen ----------
LZQ = ([3.70, 3.78, 3.86, 3.94, 4.02, 4.10, 4.18], [0.82, 0.88, 0.94, 1.00, 1.06, 1.12, 1.18], 9.0, 6.0)
NEMA = ([5.10, 5.14, 5.18, 5.22, 5.26], [1.90, 1.95, 2.00, 2.05, 2.10], 12.0, 5.0)
parts = []                   # (Name, Mesh, x, y)
for copy, cn in ((1, 'A'), (2, 'B')):
    m, W = plate(*LZQ, copy); parts.append(('LZQ-Raster ' + cn, m, 4.0 + 76.0 * (copy - 1), 182.0))
for copy, cn, y in ((1, 'A', 188.0), (2, 'B', 116.0)):
    m, W = plate(*NEMA, copy); parts.append(('NEMA-Platte ' + cn, m, 158.0, y))
k = 0
for it in range(3):
    for ia in range(3):
        for ih in range(3):
            row, col = divmod(k, 7)
            parts.append(('Zyl T%d A%d H%d (Tiefe %.2f, Abstand %g, Höhe %.1f)' % (it + 1, ia + 1, ih + 1, TIEFE[it], ABSTAND[ia], HOEHE[ih]),
                          cylinder_part(it, ia, ih), 4.0 + col * 21.0, 157.0 - row * 21.0))
            k += 1

def check_holes(m, dias, flats, pitch, T):
    sec = m.section(plane_origin=[0, 0, T / 2], plane_normal=[0, 0, 1])
    holes = [np.asarray(sec.vertices)[e.points][:, :2] for e in sec.entities]
    holes = [p for p in holes if 3 < np.ptp(p[:, 0]) < 6]
    assert len(holes) == len(dias) * len(flats), len(holes)
    for p in holes:
        cx, cy = p.mean(axis=0); j, i = int(round((cx - 8) / pitch)), int(round((cy - 8) / pitch))
        x0, y0 = 8 + j * pitch, 8 + i * pitch
        assert abs(np.ptp(p[:, 0]) - dias[i]) < 0.01 and abs(p[:, 1].max() - y0 - flats[j]) < 1e-4, (i, j)
check_holes(parts[0][1], *LZQ); check_holes(parts[2][1], *NEMA)
for name, m, x, y in parts[4:]:                              # Zylinder: Steg-Fläche und Grundabflachung nachmessen
    t = TIEFE[int(name[5]) - 1]; c = PED / 2
    s1 = m.section(plane_origin=[0, 0, WEB_Z0 + 0.1], plane_normal=[0, 0, 1])     # im ersten Steg
    s2 = m.section(plane_origin=[0, 0, WEB_Z0 - 0.3], plane_normal=[0, 0, 1])     # unter dem ersten Steg
    f1 = max(np.asarray(s1.vertices)[e.points][:, 1].max() for e in s1.entities if 3 < np.ptp(np.asarray(s1.vertices)[e.points][:, 0]) < 6) - c
    f2 = max(np.asarray(s2.vertices)[e.points][:, 1].max() for e in s2.entities if 3 < np.ptp(np.asarray(s2.vertices)[e.points][:, 0]) < 6) - c
    assert abs(f1 - (BASE_F - t)) < 1e-3 and abs(f2 - BASE_F) < 1e-3, (name, f1, f2)
for name, m, x, y in parts:
    b = m.bounds; assert x + b[0][0] >= 0 and y + b[0][1] >= 0 and x + b[1][0] <= 256 and y + b[1][1] <= 256, name
print('Prüfung ok:', len(parts), 'Teile')

# ---------- 3MF (Aufbau wie passungsraster_3mf.py) ----------
NS = 'xmlns="http://schemas.microsoft.com/3dmanufacturing/core/2015/02" xmlns:BambuStudio="http://schemas.bambulab.com/package/2021" xmlns:p="http://schemas.microsoft.com/3dmanufacturing/production/2015/06" requiredextensions="p"'
def mesh_xml(m):
    vv = '\n'.join('     <vertex x="%.5f" y="%.5f" z="%.5f"/>' % tuple(p) for p in m.vertices)
    tt = '\n'.join('     <triangle v1="%d" v2="%d" v3="%d"/>' % tuple(f) for f in m.faces)
    return ('<?xml version="1.0" encoding="UTF-8"?>\n<model unit="millimeter" xml:lang="en-US" %s>\n <metadata name="BambuStudio:3mfVersion">1</metadata>\n <resources>\n'
            '  <object id="1" p:UUID="%s" type="model">\n   <mesh>\n    <vertices>\n%s\n    </vertices>\n    <triangles>\n%s\n    </triangles>\n   </mesh>\n  </object>\n </resources>\n <build/>\n</model>\n') % (NS, uuid.uuid4(), vv, tt)
zin = zipfile.ZipFile(TEMPLATE); files = {}
objs = []
for i, (name, m, x, y) in enumerate(parts):
    files['3D/Objects/part_%d.model' % i] = mesh_xml(m)
    objs.append((i + 2, i, x, y, name, len(m.faces)))
tpl = zin.read('3D/3dmodel.model').decode(); head = tpl[:tpl.index(' <resources>')]
res = [' <resources>']
for o, vi, x, y, name, nf in objs:
    res.append('  <object id="%d" p:UUID="%s" type="model">\n   <components>\n    <component p:path="/3D/Objects/part_%d.model" objectid="1" p:UUID="%s" transform="1 0 0 0 1 0 0 0 1 0 0 0"/>\n   </components>\n  </object>' % (o, uuid.uuid4(), vi, uuid.uuid4()))
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
json.dump([dict(name=n, x=x, y=y, v=np.round(m.vertices, 3).tolist(), f=m.faces.tolist()) for n, m, x, y in parts], open('/tmp/nachtdruck_meshes.json', 'w'))
