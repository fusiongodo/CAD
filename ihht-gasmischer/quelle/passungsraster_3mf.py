"""Passungs-Raster für die D-Bohrungen (NEMA-Welle Ø 5, LZQ-Spindel Ø 4) als Bambu-3MF.

Zwei Platten 64 x 64 x 5 mm, je 5 x 5 D-Löcher, Lochachsen senkrecht.
  Reihe  (1-5, Kerben am linken Rand)   = Bohrungs-Ø
  Spalte (1-5, Kerben am vorderen Rand) = Abstand Abflachung-Achse
  Abgeschrägte Ecke vorn links = Reihe 1 / Spalte 1.
  NEMA-Platte: eine abgeschrägte Ecke. LZQ-Platte: zweite Schräge hinten rechts.
  Abflachungen zeigen alle nach hinten (+y), weg von den Spaltenkerben.

Aufruf:  python3 passungsraster_3mf.py            (braucht trimesh, manifold3d, numpy)
Vorlage: ~/Desktop/projects/CAD/m3_schrauben_original.3mf  (oder Umgebungsvariable BAMBU_VORLAGE)
Ausgabe: ../druck/passungsraster.3mf
"""
import numpy as np, trimesh, json, zipfile, uuid, os, math

HERE = os.path.dirname(os.path.abspath(__file__))
TEMPLATE = os.environ.get('BAMBU_VORLAGE', os.path.expanduser('~/Desktop/projects/CAD/m3_schrauben_original.3mf'))
OUT = os.path.join(HERE, '..', 'druck', 'passungsraster.3mf')

PLATES = [  # Name, Bohrungs-Ø je Reihe, Abstand Fläche-Achse je Spalte, zweite Schräge
    ('Passung NEMA', [5.02, 5.10, 5.18, 5.26, 5.34], [2.05, 2.10, 2.15, 2.20, 2.25], False),
    ('Passung LZQ',  [4.17, 4.25, 4.33, 4.41, 4.49], [0.90, 0.95, 1.00, 1.05, 1.10], True),
]
W, T, N = 64.0, 5.0, 5      # Plattenkante, Dicke, Löcher je Reihe/Spalte
PITCH, FIRST = 12.0, 8.0    # Lochraster, erste Lochmitte ab Kante
CH = 0.5                    # Fase unten (Elefantenfuß)
NW, ND, NP = 1.0, 1.5, 2.0  # Kerbe: Breite, Tiefe, Raster innerhalb einer Gruppe
CORNER = 5.0                # Schenkel der Eckschräge

def cyl(r, z0, z1, x, y, n=128):
    m = trimesh.creation.cylinder(radius=r, height=z1 - z0, sections=n); m.apply_translation([x, y, (z0 + z1) / 2]); return m
def cone(r0, r1, z0, z1, x, y, n=128):
    m = trimesh.creation.revolve(np.array([(0, z0), (r0, z0), (r1, z1), (0, z1)]), sections=n)
    if m.volume < 0: m.invert()
    m.apply_translation([x, y, 0]); return m
def box(x0, x1, y0, y1, z0, z1):
    b = trimesh.creation.box(extents=[x1 - x0, y1 - y0, z1 - z0]); b.apply_translation([(x0 + x1) / 2, (y0 + y1) / 2, (z0 + z1) / 2]); return b
def corner_cut(cx, cy):
    """Dreiecksprisma, das die Ecke (cx, cy) mit 45° und Schenkel CORNER abschneidet."""
    s = CORNER * math.sqrt(2)
    b = box(-s, s, -s, 0, -1, T + 1)                       # Halbraum unterhalb der Schnittlinie
    sx, sy = (1 if cx == 0 else -1), (1 if cy == 0 else -1)
    ang = math.atan2(sy, sx) - math.pi / 2                 # Normale der Schnittlinie zeigt ins Platteninnere
    b.apply_transform(trimesh.transformations.rotation_matrix(ang, [0, 0, 1]))
    b.apply_translation([cx + sx * CORNER / 2, cy + sy * CORNER / 2, 0]); return b

def plate(dias, flats, second_corner):
    U = lambda ms: trimesh.boolean.union(ms, engine='manifold')
    D = lambda a, ms: trimesh.boolean.difference([a] + ms, engine='manifold')
    cuts, keys = [], []
    for i, d in enumerate(dias):                           # Reihe i -> y
        for j, f in enumerate(flats):                      # Spalte j -> x
            x, y, r = FIRST + j * PITCH, FIRST + i * PITCH, d / 2
            cuts += [cyl(r, -1, T + 1, x, y), cone(r + CH, r, -0.01, CH, x, y)]
            keys.append(box(x - r - 0.3, x + r + 0.3, y + f, y + r + 0.6, CH, T))   # Keil: Fläche bei y+f, bis hinter die Bohrungswand, ab Fasenoberkante
    for i in range(N):                                     # Kerben: i+1 Stück je Reihe (links) und Spalte (vorn)
        c = FIRST + i * PITCH
        for k in range(i + 1):
            o = c + (k - i / 2) * NP
            cuts.append(box(-1, ND, o - NW / 2, o + NW / 2, -1, T + 1))
            cuts.append(box(o - NW / 2, o + NW / 2, -1, ND, -1, T + 1))
    cuts.append(corner_cut(0, 0))
    if second_corner: cuts.append(corner_cut(W, W))
    m = U([D(box(0, W, 0, W, 0, T), cuts)] + keys)
    assert m.is_watertight and len(m.split()) == 1
    return m

meshes = [plate(d, f, c) for _, d, f, c in PLATES]

# ---------- Selbstprüfung: jedes Loch im Schnitt z = T/2 nachmessen ----------
for (name, dias, flats, _), m in zip(PLATES, meshes):
    assert np.allclose(m.bounds, [[0, 0, 0], [W, W, T]], atol=1e-6), m.bounds
    sec = m.section(plane_origin=[0, 0, T / 2], plane_normal=[0, 0, 1])
    loops = [np.asarray(sec.vertices)[e.points][:, :2] for e in sec.entities]
    holes = [p for p in loops if np.ptp(p[:, 0]) < 10]
    assert len(holes) == N * N and len(loops) == N * N + 1, (len(holes), len(loops))
    outer = max(loops, key=lambda p: np.ptp(p[:, 0])); xo, yo = outer[:, 0], outer[:, 1]
    area = abs(np.dot(xo, np.roll(yo, 1)) - np.dot(yo, np.roll(xo, 1))) / 2
    assert abs(area - (W * W - N * (N + 1) * NW * ND - (2 if PLATES[[n for n, *_ in PLATES].index(name)][3] else 1) * CORNER ** 2 / 2)) < 1e-3, area   # Kerben + Eckschrägen
    low = m.section(plane_origin=[0, 0, CH / 2], plane_normal=[0, 0, 1])                                  # in der Fase: Loch rund, kein Keil
    for e in low.entities:
        q = np.asarray(low.vertices)[e.points][:, :2]
        if np.ptp(q[:, 0]) < 10: assert abs(np.ptp(q[:, 1]) - np.ptp(q[:, 0])) < 0.01 and abs(np.ptp(q[:, 0]) - dias[int(round((q[:, 1].mean() - FIRST) / PITCH))] - CH) < 0.01
    for p in holes:
        c = p.mean(axis=0); j, i = int(round((c[0] - FIRST) / PITCH)), int(round((c[1] - FIRST) / PITCH))
        cx, cy = FIRST + j * PITCH, FIRST + i * PITCH
        assert abs(np.ptp(p[:, 0]) - dias[i]) < 0.01, (name, i, j, 'Ø')                 # Bohrungs-Ø quer zur Fläche
        assert abs(p[:, 1].max() - cy - flats[j]) < 1e-4, (name, i, j, 'Fläche')        # Abstand Fläche-Achse
        assert abs(cy - p[:, 1].min() - dias[i] / 2) < 0.01, (name, i, j, 'Radius')
    web = PITCH - max(dias) - 2 * CH
    print('%s: alle Löcher geprüft, Steg min. %.2f mm, Rand min. %.2f mm' % (name, web, min(FIRST - max(dias) / 2 - CH - ND, W - FIRST - (N - 1) * PITCH - max(dias) / 2 - CH)))

# ---------- 3MF (Aufbau wie kupplung_v1_3mf.py) ----------
NS = 'xmlns="http://schemas.microsoft.com/3dmanufacturing/core/2015/02" xmlns:BambuStudio="http://schemas.bambulab.com/package/2021" xmlns:p="http://schemas.microsoft.com/3dmanufacturing/production/2015/06" requiredextensions="p"'
def mesh_xml(m):
    vv = '\n'.join('     <vertex x="%.5f" y="%.5f" z="%.5f"/>' % tuple(p) for p in m.vertices)
    tt = '\n'.join('     <triangle v1="%d" v2="%d" v3="%d"/>' % tuple(f) for f in m.faces)
    return ('<?xml version="1.0" encoding="UTF-8"?>\n<model unit="millimeter" xml:lang="en-US" %s>\n <metadata name="BambuStudio:3mfVersion">1</metadata>\n <resources>\n'
            '  <object id="1" p:UUID="%s" type="model">\n   <mesh>\n    <vertices>\n%s\n    </vertices>\n    <triangles>\n%s\n    </triangles>\n   </mesh>\n  </object>\n </resources>\n <build/>\n</model>\n') % (NS, uuid.uuid4(), vv, tt)
zin = zipfile.ZipFile(TEMPLATE); files = {}
GAP = 8.0
objs = []                                                   # (Objekt-ID, Mesh-Index, x, y, Name, Dreiecke); Platten nebeneinander um die Bettmitte (128, 128)
for i, m in enumerate(meshes):
    files['3D/Objects/variant_%d.model' % i] = mesh_xml(m)
    objs.append((i + 2, i, 128 - W - GAP / 2 + i * (W + GAP), 128 - W / 2, PLATES[i][0], len(m.faces)))
tpl = zin.read('3D/3dmodel.model').decode(); head = tpl[:tpl.index(' <resources>')]
res = [' <resources>']
for o, vi, x, y, name, nf in objs:
    res.append('  <object id="%d" p:UUID="%s" type="model">\n   <components>\n    <component p:path="/3D/Objects/variant_%d.model" objectid="1" p:UUID="%s" transform="1 0 0 0 1 0 0 0 1 0 0 0"/>\n   </components>\n  </object>' % (o, uuid.uuid4(), vi, uuid.uuid4()))
res += [' </resources>', ' <build p:UUID="%s">' % uuid.uuid4()]
for o, vi, x, y, name, nf in objs:
    res.append('  <item objectid="%d" p:UUID="%s" transform="1 0 0 0 1 0 0 0 1 %.4f %.4f 0" printable="1"/>' % (o, uuid.uuid4(), x, y))
res.append(' </build>\n</model>\n')
files['3D/3dmodel.model'] = head + '\n'.join(res)
files['3D/_rels/3dmodel.model.rels'] = '<?xml version="1.0" encoding="UTF-8"?>\n<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">\n' + \
    '\n'.join(' <Relationship Target="/3D/Objects/variant_%d.model" Id="rel-%d" Type="http://schemas.microsoft.com/3dmanufacturing/2013/01/3dmodel"/>' % (i, i + 1) for i in range(len(meshes))) + '\n</Relationships>\n'
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
proc = {'layer_height': '0.2', 'wall_loops': '3', 'enable_support': '0', 'brim_type': 'no_brim'}   # Rest wie Vorlage (u. a. Elefantenfuß-Kompensation 0,15, Naht ausgerichtet)
d.update(proc)
diff = list(d['different_settings_to_system'])
diff[0] = ';'.join(sorted((set(x for x in diff[0].split(';') if x) | set(proc)) - {'support_on_build_plate_only', 'support_remove_small_overhang'}))
d['different_settings_to_system'] = diff
files['Metadata/project_settings.config'] = json.dumps(d, indent=4, ensure_ascii=False)
skip = {'3D/Objects/object_%s.model' % s for s in ('19', '20', '21', '36', '38', '39')}
zout = zipfile.ZipFile(OUT, 'w', zipfile.ZIP_DEFLATED, compresslevel=9); done = set()
for it in zin.infolist():
    n = it.filename
    if n in skip: continue
    if n in files: zout.writestr(n, files[n]); done.add(n)
    else: zout.writestr(it, zin.read(n))
for n, c in files.items():
    if n not in done: zout.writestr(n, c)
zout.close()
print('ok', os.path.normpath(OUT), os.path.getsize(OUT), [len(m.faces) for m in meshes])
