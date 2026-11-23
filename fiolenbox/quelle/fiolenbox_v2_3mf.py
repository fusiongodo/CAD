"""Druck Fiolenbox v2: Unterteil (stehend) + Deckel (kopfüber), Bambu-3MF für P1S, 0,20-mm-Schichten, kein Support.

Aufruf:  python3 fiolenbox_v2_3mf.py [NAME] [masse]   (STL aus ../druck, Vorlage ../../ihht-gasmischer/druck/passungsraster.3mf)
         NAME = Dateipräfix, Standard fiolenbox_v2; z. B. fiolenbox_11_4x3_v1
         masse = viel PLA + isolierend: Deckel 3 Wände, Unterteil 2 Wände (Löcher), 6/5 Deck-/Bodenschichten, Cubic 35 % (geschlossene Luftkammern)
"""
import numpy as np, trimesh, json, zipfile, uuid, os

HERE = os.path.dirname(os.path.abspath(__file__))
DRUCK = os.path.join(HERE, '..', 'druck')
TEMPLATE = os.environ.get('BAMBU_VORLAGE', os.path.join(HERE, '..', '..', 'ihht-gasmischer', 'druck', 'passungsraster.3mf'))
import sys
NAME = sys.argv[1] if len(sys.argv) > 1 else 'fiolenbox_v2'
MASSE = 'masse' in sys.argv[2:]
OUT = os.path.join(DRUCK, NAME + '.3mf')

def load(name, flip=False):
    m = trimesh.load(os.path.join(DRUCK, name), force='mesh'); m.merge_vertices()
    if flip: m.apply_transform(trimesh.transformations.rotation_matrix(np.pi, [1, 0, 0]))   # Deckelplatte aufs Bett
    m.apply_translation(-m.bounds[0]); assert m.is_watertight, name
    return m

base, lid = load(NAME + '_unterteil.stl'), load(NAME + '_deckel.stl', flip=True)
gap = 12.0
x0 = (256 - (base.extents[0] + lid.extents[0] + gap)) / 2
parts = [(NAME + ' Unterteil', base, x0, (256 - base.extents[1]) / 2),
         (NAME + ' Deckel', lid, x0 + base.extents[0] + gap, (256 - lid.extents[1]) / 2)]
for name, m, x, y in parts:
    assert x >= 0 and y >= 0 and x + m.extents[0] <= 256 and y + m.extents[1] <= 256, name
    print('%-24s %5.1f × %5.1f × %5.1f mm  Volumen %.1f cm³' % (name, *m.extents, m.volume / 1000))
# Deckelplatte liegt flach auf (größte Fläche bei z=0)
zf = lid.vertices[lid.faces][:, :, 2].max(axis=1) < 1e-3
assert lid.area_faces[zf].sum() > 0.9 * lid.extents[0] * lid.extents[1] * 0.95, 'Deckel liegt nicht flach'

# ---------- 3MF (Aufbau wie ihht-gasmischer/quelle/druck2_3mf.py) ----------
NS = 'xmlns="http://schemas.microsoft.com/3dmanufacturing/core/2015/02" xmlns:BambuStudio="http://schemas.bambulab.com/package/2021" xmlns:p="http://schemas.microsoft.com/3dmanufacturing/production/2015/06" requiredextensions="p"'
def mesh_xml(m, oid):
    vv = '\n'.join('     <vertex x="%.5f" y="%.5f" z="%.5f"/>' % tuple(p) for p in m.vertices)
    tt = '\n'.join('     <triangle v1="%d" v2="%d" v3="%d"/>' % tuple(f) for f in m.faces)
    return ('<?xml version="1.0" encoding="UTF-8"?>\n<model unit="millimeter" xml:lang="en-US" %s>\n <metadata name="BambuStudio:3mfVersion">1</metadata>\n <resources>\n'
            '  <object id="%d" p:UUID="%s" type="model">\n   <mesh>\n    <vertices>\n%s\n    </vertices>\n    <triangles>\n%s\n    </triangles>\n   </mesh>\n  </object>\n </resources>\n <build/>\n</model>\n') % (NS, oid, uuid.uuid4(), vv, tt)
zin = zipfile.ZipFile(TEMPLATE); files = {}; objs = []
for i, (name, m, x, y) in enumerate(parts):
    files['3D/Objects/part_%d.model' % i] = mesh_xml(m, 2 * i + 1)
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
PER_OBJ = {0: {'wall_loops': '2'}} if MASSE else {}   # objekt 0 = Unterteil: 2 Wände um jedes Loch reichen
ms = ['<?xml version="1.0" encoding="UTF-8"?>', '<config>']
for o, vi, x, y, name, nf in objs:
    ms += ['  <object id="%d">' % o, '    <metadata key="name" value="%s"/>' % name, '    <metadata key="extruder" value="1"/>', '    <metadata face_count="%d"/>' % nf,
           *['    <metadata key="%s" value="%s"/>' % kv for kv in PER_OBJ.get(vi, {}).items()],
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
if MASSE: proc.update({'wall_loops': '3', 'top_shell_layers': '6', 'bottom_shell_layers': '5', 'sparse_infill_pattern': 'cubic', 'sparse_infill_density': '35%'})
d.update(proc)
diff = list(d['different_settings_to_system'])
diff[0] = ';'.join(sorted((set(x for x in diff[0].split(';') if x) | set(proc)) - {'support_on_build_plate_only', 'support_remove_small_overhang'}))
d['different_settings_to_system'] = diff
files['Metadata/project_settings.config'] = json.dumps(d, indent=4, ensure_ascii=False)
skip = ('3D/Objects/',)   # alte Objekte der Vorlage weglassen (Vorschaubilder bleiben, werden von .rels referenziert)
zout = zipfile.ZipFile(OUT, 'w', zipfile.ZIP_DEFLATED, compresslevel=9); done = set()
for it in zin.infolist():
    n = it.filename
    if n.startswith(skip) and n not in files: continue
    if n in files: zout.writestr(n, files[n]); done.add(n)
    else: zout.writestr(it, zin.read(n))
for n, c in files.items():
    if n not in done: zout.writestr(n, c)
zout.close()
print('ok', os.path.normpath(OUT), os.path.getsize(OUT) // 1024, 'KB')
