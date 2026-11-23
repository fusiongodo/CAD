"""Quetschventil v3: Netze aus teile/qv_*.py bauen, prüfen und als Bambu-3MF in Drucklage schreiben.

Prüfung: jedes Teil wasserdicht und ein Körper; keine Überschneidung zwischen Teilen und Dummies (Rotor in 12 Stellungen
um die Achse); Zahneingriff über eine Teilung. Ausgabe ../druck/quetschventil_v3.3mf (0,20 mm, 4 Wände, 40 %, Brim 5).
Die Ambosse sind Rahmen v2 und stehen nicht in der 3MF.

Braucht numpy trimesh manifold3d shapely mapbox_earcut:   <venv>/bin/python quetschventil_v3_3mf.py
"""
import itertools, json, math, os, uuid, zipfile
import trimesh
import build
from ihht.netz import build as netz
from ihht.qv import A, ZR, ZP

HERE = os.path.dirname(os.path.abspath(__file__))
DRUCK = os.path.join(HERE, '..', 'druck')
TEMPLATE = os.environ.get('BAMBU_VORLAGE', os.path.join(DRUCK, 'passungsraster.3mf'))
OUT = 'quetschventil_v3.3mf'

mods = ['qv_welle', 'qv_klemmhuelse', 'qv_hohlrad', 'qv_ritzel', 'qv_getriebeplatte', 'qv_rueckwand', 'qv_steg',
        'qv_amboss', 'qv_distanzscheibe', 'qv_dummies']
P = {p.name: p for m in mods for p in build.teil(m).bauen()}
M = {n: netz(p) for n, p in P.items()}
ok = True
for n, m in M.items():
    b = m.bounds
    k = len(m.split(only_watertight=False))
    print('%-16s vol %9.1f  wasserdicht %-5s Körper %d  x %6.1f..%6.1f  y %6.1f..%6.1f  z %6.1f..%6.1f' % (
        n, m.volume, m.is_watertight, k, b[0][0], b[1][0], b[0][1], b[1][1], b[0][2], b[1][2]))
    ok &= m.is_watertight and k == 1


def inter(a, b):
    ba, bb = a.bounds, b.bounds
    if (ba[0] > bb[1]).any() or (bb[0] > ba[1]).any():
        return 0.0
    return trimesh.boolean.intersection([a, b], engine='manifold').volume


def rot(m, ang, center=(0.0, 0.0)):                  # Drehung um eine Achse parallel zu Y durch (x, z) = center
    m = m.copy()
    m.apply_transform(trimesh.transformations.rotation_matrix(ang, [0, 1, 0], [center[0], 0, center[1]]))
    return m


ROTOR = ['qvWelle', 'qvKlemmhuelse', 'qvHohlrad', 'qvDistanzH', 'qvDistanzV', 'qv6905A', 'qv6905B', 'qvSchraube', 'qvMutterK', 'qvMutterV']
FEST = ['qvRueckwand', 'qvSteg', 'qvAmbossO', 'qvAmbossU', 'qvGetriebeplatte', 'qv608H', 'qv608V', 'qvNema']
ERLAUBT = {frozenset(p) for p in [('qv608H', 'qvRueckwand'), ('qv608V', 'qvSteg'), ('qvRitzel', 'qvNema')]}   # Presssitz / Passung

print('--- Überschneidungen (> 0,2 mm3) ---')
n_bad = 0
for a, b in itertools.combinations(M, 2):
    if frozenset((a, b)) in ERLAUBT:
        continue
    v = inter(M[a], M[b])
    if v > 0.2:
        n_bad += 1
        print('  %s x %s: %.2f mm3' % (a, b, v))
worst = 0.0
for k in range(1, 12):
    ang = 2 * math.pi * k / 12
    for r in ROTOR:
        mr = rot(M[r], ang)
        for f in FEST + ['qvRitzel']:
            if frozenset((r, f)) in ERLAUBT or (r, f) == ('qvHohlrad', 'qvRitzel'):   # Eingriff: eigene Prüfung unten
                continue
            v = inter(mr, M[f])
            if v > 0.2:
                worst = max(worst, v)
                print('  Rotor %d°: %s x %s: %.2f mm3' % (30 * k, r, f, v))
print('  keine' if n_bad == 0 and worst == 0 else '  Kollisionen')
ok &= n_bad == 0 and worst == 0

g = 0.0
for k in range(24):                                   # eine Teilung des Hohlrads; Ritzel 5-mal so schnell, gleiche Richtung
    th = 2 * math.pi / ZR * k / 24
    g = max(g, inter(rot(M['qvHohlrad'], th), rot(M['qvRitzel'], th * ZR / ZP, (0.0, -A))))
print('Zahneingriff: größte Durchdringung %.3f mm3' % g)
ok &= g < 0.05
print('PRÜFUNG', 'OK' if ok else 'FEHLER')


# ---------- Drucklage: Builder-Koordinaten, Achse = Y, vorn = -Y ----------
def lay(m, unten):                                    # unten: 'vorn' (-Y aufs Bett) oder 'hinten' (+Y aufs Bett)
    m = m.copy()
    m.apply_transform(trimesh.transformations.rotation_matrix(math.pi / 2 if unten == 'vorn' else -math.pi / 2, [1, 0, 0]))
    c = m.bounds.mean(axis=0)
    m.apply_translation([-c[0], -c[1], -m.bounds[0][2]])
    return m


PRINT = [('Rueckwand', lay(M['qvRueckwand'], 'hinten'), 60.0, 58.0),
         ('Steg', lay(M['qvSteg'], 'vorn'), 160.0, 58.0),
         ('Getriebeplatte', lay(M['qvGetriebeplatte'], 'hinten'), 60.0, 170.0),
         ('Hohlrad', lay(M['qvHohlrad'], 'vorn'), 160.0, 170.0),
         ('Welle', lay(M['qvWelle'], 'vorn'), 228.0, 40.0),
         ('Klemmhuelse', lay(M['qvKlemmhuelse'], 'hinten'), 228.0, 85.0),
         ('Ritzel', lay(M['qvRitzel'], 'hinten'), 228.0, 125.0),
         ('Distanz hinten', lay(M['qvDistanzH'], 'vorn'), 222.0, 160.0),
         ('Distanz vorn', lay(M['qvDistanzV'], 'vorn'), 238.0, 160.0)]
for name, m, x, y in PRINT:
    e = m.extents
    print('%-15s Drucklage %5.1f x %5.1f x %5.1f  Bett x %5.1f..%5.1f  y %5.1f..%5.1f' % (name, e[0], e[1], e[2], x - e[0] / 2, x + e[0] / 2, y - e[1] / 2, y + e[1] / 2))
    assert m.is_watertight and abs(m.bounds[0][2]) < 1e-6, name


def write_3mf(PRINT, out3):
    NS = ('xmlns="http://schemas.microsoft.com/3dmanufacturing/core/2015/02" xmlns:BambuStudio="http://schemas.bambulab.com/package/2021" '
          'xmlns:p="http://schemas.microsoft.com/3dmanufacturing/production/2015/06" requiredextensions="p"')

    def mesh_xml(m, oid):
        vv = '\n'.join('     <vertex x="%.5f" y="%.5f" z="%.5f"/>' % tuple(p) for p in m.vertices)
        tt = '\n'.join('     <triangle v1="%d" v2="%d" v3="%d"/>' % tuple(t) for t in m.faces)
        return ('<?xml version="1.0" encoding="UTF-8"?>\n<model unit="millimeter" xml:lang="en-US" %s>\n <metadata name="BambuStudio:3mfVersion">1</metadata>\n <resources>\n'
                '  <object id="%d" p:UUID="%s" type="model">\n   <mesh>\n    <vertices>\n%s\n    </vertices>\n    <triangles>\n%s\n    </triangles>\n   </mesh>\n  </object>\n </resources>\n <build/>\n</model>\n') % (NS, oid, uuid.uuid4(), vv, tt)

    zin = zipfile.ZipFile(TEMPLATE)
    files, objs = {}, []
    for i, (name, m, x, y) in enumerate(PRINT):
        files['3D/Objects/part_%d.model' % i] = mesh_xml(m, 2 * i + 1)
        objs.append((2 * i + 2, i, x, y, name, len(m.faces)))
    tpl = zin.read('3D/3dmodel.model').decode()
    res = [tpl[:tpl.index(' <resources>')] + ' <resources>']
    for o, vi, x, y, name, nf in objs:
        res.append('  <object id="%d" p:UUID="%s" type="model">\n   <components>\n    <component p:path="/3D/Objects/part_%d.model" objectid="%d" p:UUID="%s" transform="1 0 0 0 1 0 0 0 1 0 0 0"/>\n   </components>\n  </object>' % (o, uuid.uuid4(), vi, 2 * vi + 1, uuid.uuid4()))
    res += [' </resources>', ' <build p:UUID="%s">' % uuid.uuid4()]
    for o, vi, x, y, name, nf in objs:
        res.append('  <item objectid="%d" p:UUID="%s" transform="1 0 0 0 1 0 0 0 1 %.4f %.4f 0" printable="1"/>' % (o, uuid.uuid4(), x, y))
    res.append(' </build>\n</model>\n')
    files['3D/3dmodel.model'] = '\n'.join(res)
    files['3D/_rels/3dmodel.model.rels'] = ('<?xml version="1.0" encoding="UTF-8"?>\n<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">\n' +
                                            '\n'.join(' <Relationship Target="/3D/Objects/part_%d.model" Id="rel-%d" Type="http://schemas.microsoft.com/3dmanufacturing/2013/01/3dmodel"/>' % (i, i + 1) for i in range(len(PRINT))) + '\n</Relationships>\n')
    ms = ['<?xml version="1.0" encoding="UTF-8"?>', '<config>']
    for o, vi, x, y, name, nf in objs:
        ms += ['  <object id="%d">' % o, '    <metadata key="name" value="%s"/>' % name, '    <metadata key="extruder" value="1"/>', '    <metadata face_count="%d"/>' % nf,
               '    <part id="1" subtype="normal_part" uuid="%s">' % uuid.uuid4(), '      <metadata key="name" value="%s"/>' % name,
               '      <metadata key="matrix" value="1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1"/>',
               '      <mesh_stat face_count="%d" edges_fixed="0" degenerate_facets="0" facets_removed="0" facets_reversed="0" backwards_edges="0"/>' % nf,
               '    </part>', '  </object>']
    ms += ['  <plate>', '    <metadata key="plater_id" value="1"/>', '    <metadata key="plater_name" value=""/>', '    <metadata key="locked" value="false"/>',
           '    <metadata key="filament_map_mode" value="Auto For Flush"/>', '    <metadata key="filament_maps" value="1 1 1"/>', '    <metadata key="filament_volume_maps" value="0 0 0"/>']
    for i, (o, vi, x, y, name, nf) in enumerate(objs):
        ms += ['    <model_instance>', '      <metadata key="object_id" value="%d"/>' % o, '      <metadata key="instance_id" value="0"/>',
               '      <metadata key="identify_id" value="%d"/>' % (2000 + i), '    </model_instance>']
    ms += ['  </plate>', '  <assemble>']
    for o, vi, x, y, name, nf in objs:
        ms += ['   <assemble_item object_id="%d" instance_id="0" transform="1 0 0 0 1 0 0 0 1 %.4f %.4f 0" offset="0 0 0" />' % (o, x, y)]
    ms += ['  </assemble>', '</config>', '']
    files['Metadata/model_settings.config'] = '\n'.join(ms)
    files['Metadata/cut_information.xml'] = ('<?xml version="1.0" encoding="utf-8"?>\n<objects>\n' +
                                             ''.join(' <object id="%d">\n  <cut_id id="0" check_sum="1" connectors_cnt="0"/>\n </object>\n' % (i + 1) for i in range(len(objs))) + '</objects>\n')
    d = json.loads(zin.read('Metadata/project_settings.config'))
    proc = {'layer_height': '0.2', 'wall_loops': '4', 'sparse_infill_density': '40%', 'enable_support': '0',
            'brim_type': 'outer_only', 'brim_width': '5'}
    d.update(proc)
    diff = list(d['different_settings_to_system'])
    diff[0] = ';'.join(sorted((set(x for x in diff[0].split(';') if x) | set(proc)) - {'support_on_build_plate_only', 'support_remove_small_overhang'}))
    d['different_settings_to_system'] = diff
    files['Metadata/project_settings.config'] = json.dumps(d, indent=4, ensure_ascii=False)
    zout = zipfile.ZipFile(out3, 'w', zipfile.ZIP_DEFLATED, compresslevel=9)
    done = set()
    for it in zin.infolist():
        nme = it.filename
        if nme.startswith('3D/Objects/') and nme not in files:
            continue
        if nme in files:
            zout.writestr(nme, files[nme]); done.add(nme)
        else:
            zout.writestr(it, zin.read(nme))
    for nme, c in files.items():
        if nme not in done:
            zout.writestr(nme, c)
    zout.close()
    print('3MF', out3, os.path.getsize(out3))


write_3mf(PRINT, os.path.join(DRUCK, OUT))
raise SystemExit(0 if ok else 1)
