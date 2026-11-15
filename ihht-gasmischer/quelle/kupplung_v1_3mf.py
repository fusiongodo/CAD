import numpy as np, trimesh, json, zipfile, uuid, math, os
# Finale Kupplungen: 4 Varianten x 4 Stück. (NEMA-Ø, NEMA-Abstand Fläche-Achse, LZQ-Ø, LZQ-Abstand Fläche-Achse)
VAR = [(5.32, 2.22, 4.5, 1.06)]   # final: NEMA wie Reihe 2, LZQ wie Reihe 3 (K2) minimal lockerer
COPIES = 4
OD, L, ZS = 9.0, 20.0, 10.0
KEY_LZQ = (10.5, 18.5)
CH = 0.6
DX = 14.0
def cyl(r, z0, z1, x=0, y=0, n=96):
    m = trimesh.creation.cylinder(radius=r, height=z1 - z0, sections=n); m.apply_translation([x, y, (z0 + z1) / 2]); return m
def cone(r0, r1, z0, z1, n=96):
    m = trimesh.creation.revolve(np.array([(0, z0), (r0, z0), (r1, z1), (0, z1)]), sections=n)
    if m.volume < 0: m.invert()
    return m
def box(x0, x1, y0, y1, z0, z1):
    b = trimesh.creation.box(extents=[x1 - x0, y1 - y0, z1 - z0]); b.apply_translation([(x0 + x1) / 2, (y0 + y1) / 2, (z0 + z1) / 2]); return b

def plug(v, k, c=None):
    c = k if c is None else c
    U = lambda ms: trimesh.boolean.union(ms, engine='manifold')
    D = lambda a, ms: trimesh.boolean.difference([a] + ms, engine='manifold')
    dn, fn, dl, fl = v
    rn, rl = dn / 2, dl / 2
    m = D(cyl(OD / 2, 0, L), [cyl(rn, -1, ZS), cyl(rl, ZS - 0.01, L + 1), cone(rn + CH, rn, -0.01, CH), cone(rl, rl + CH, L - CH, L + 0.01)])
    m = U([m, box(-3, 3, fn, 3.6, 0, ZS), box(-2.6, 2.6, fl, 3.4, KEY_LZQ[0], KEY_LZQ[1])])
    grooves = []
    for i in range(c + 1):                                  # Rillen außen = Stück-Nr. (1-4)
        a = math.radians(90 + (i - c / 2) * 25)
        g = box(-0.4, 0.4, -0.5, 0.5, L - 5, L + 1)
        g.apply_transform(trimesh.transformations.rotation_matrix(a + math.pi / 2, [0, 0, 1]))
        g.apply_translation([OD / 2 * math.cos(a), OD / 2 * math.sin(a), 0]); grooves.append(g)
    for i in range(k + 1):                                  # Punkte oben (LZQ-Stirnseite): Anzahl = Variante
        a = math.radians(-90 + (i - k / 2) * 34)
        grooves.append(cyl(0.5, L - 0.8, L + 1, 3.45 * math.cos(a), 3.45 * math.sin(a), n=24))
    m = D(m, grooves)
    assert m.is_watertight and len(m.split()) == 1, k
    return m

meshes = [plug(v, k, c) for k, v in enumerate(VAR) for c in range(COPIES)]

# ---------- FeatureScript ----------
F = lambda v: ('%.4f' % v).rstrip('0').rstrip('.')
def v3(x, y, z): return 'vector(%s, %s, %s) * millimeter' % (F(x), F(y), F(z))
L_ = ['FeatureScript 3112;', 'import(path : "onshape/std/geometry.fs", version : "3112.0");', '',
      '// Steckkupplung NEMA 17 <-> LZQ, final: 4 Varianten (K1-K4, 1-4 Rillen am LZQ-Ende). Stehend drucken, NEMA-Seite unten.', '',
      'annotation { "Feature Type Name" : "Kupplung final" }',
      'export const kupplungFinal = defineFeature(function(context is Context, id is Id, definition is map)', '    precondition', '    {', '    }', '    {']
for k, (dn, fn, dl, fl) in enumerate(VAR):
    x = k * DX; P = 'id + "p%d"' % k; rn, rl = dn / 2, dl / 2
    L_ += ['        // K%d: NEMA Ø%s Fläche %s | LZQ Ø%s Fläche %s' % (k + 1, F(dn), F(fn), F(dl), F(fl)),
           '        fCylinder(context, %s + "body", { "bottomCenter" : %s, "topCenter" : %s, "radius" : %s * millimeter });' % (P, v3(x, 0, 0), v3(x, 0, L), F(OD / 2)),
           '        fCylinder(context, %s + "t" + "bn", { "bottomCenter" : %s, "topCenter" : %s, "radius" : %s * millimeter });' % (P, v3(x, 0, -1), v3(x, 0, ZS), F(rn)),
           '        fCylinder(context, %s + "t" + "bl", { "bottomCenter" : %s, "topCenter" : %s, "radius" : %s * millimeter });' % (P, v3(x, 0, ZS - 0.01), v3(x, 0, L + 1), F(rl)),
           '        fCone(context, %s + "t" + "cn", { "bottomCenter" : %s, "topCenter" : %s, "bottomRadius" : %s * millimeter, "topRadius" : %s * millimeter });' % (P, v3(x, 0, -0.01), v3(x, 0, CH), F(rn + CH), F(rn)),
           '        fCone(context, %s + "t" + "cl", { "bottomCenter" : %s, "topCenter" : %s, "bottomRadius" : %s * millimeter, "topRadius" : %s * millimeter });' % (P, v3(x, 0, L - CH), v3(x, 0, L + 0.01), F(rl), F(rl + CH)),
           '        opBoolean(context, %s + "x", { "tools" : qCreatedBy(%s + "t", EntityType.BODY), "targets" : qCreatedBy(%s + "body", EntityType.BODY), "operationType" : BooleanOperationType.SUBTRACTION });' % (P, P, P),
           '        fCuboid(context, %s + "k" + "n", { "corner1" : %s, "corner2" : %s });' % (P, v3(x - 3, fn, 0), v3(x + 3, 3.6, ZS)),
           '        fCuboid(context, %s + "k" + "l", { "corner1" : %s, "corner2" : %s });' % (P, v3(x - 2.6, fl, KEY_LZQ[0]), v3(x + 2.6, 3.4, KEY_LZQ[1])),
           '        opBoolean(context, %s + "u", { "tools" : qUnion([qCreatedBy(%s + "body", EntityType.BODY), qCreatedBy(%s + "k", EntityType.BODY)]), "operationType" : BooleanOperationType.UNION });' % (P, P, P)]
    for i in range(k + 1):
        a = math.radians(-90 + (i - k / 2) * 25)
        L_.append('        fCylinder(context, %s + "g" + "g%d", { "bottomCenter" : %s, "topCenter" : %s, "radius" : 0.45 * millimeter });' % (P, i, v3(x + OD / 2 * math.cos(a), OD / 2 * math.sin(a), L - 5), v3(x + OD / 2 * math.cos(a), OD / 2 * math.sin(a), L + 1)))
    for i in range(k + 1):
        a = math.radians(-90 + (i - k / 2) * 34)
        L_.append('        fCylinder(context, %s + "g" + "d%d", { "bottomCenter" : %s, "topCenter" : %s, "radius" : 0.5 * millimeter });' % (P, i, v3(x + 3.45 * math.cos(a), 3.45 * math.sin(a), L - 0.8), v3(x + 3.45 * math.cos(a), 3.45 * math.sin(a), L + 1)))
    L_ += ['        opBoolean(context, %s + "gx", { "tools" : qCreatedBy(%s + "g", EntityType.BODY), "targets" : qCreatedBy(%s + "body", EntityType.BODY), "operationType" : BooleanOperationType.SUBTRACTION });' % (P, P, P),
           '        setProperty(context, { "entities" : qCreatedBy(%s + "body", EntityType.BODY), "propertyType" : PropertyType.NAME, "value" : "Kupplung K%d" });' % (P, k + 1)]
L_ += ['    });', '']
if os.environ.get('MKFS'): open('kupplung_final.fs', 'w').write('\n'.join(L_))

# ---------- 3MF ----------
NS = 'xmlns="http://schemas.microsoft.com/3dmanufacturing/core/2015/02" xmlns:BambuStudio="http://schemas.bambulab.com/package/2021" xmlns:p="http://schemas.microsoft.com/3dmanufacturing/production/2015/06" requiredextensions="p"'
def mesh_xml(m):
    vv = '\n'.join('     <vertex x="%.5f" y="%.5f" z="%.5f"/>' % tuple(p) for p in m.vertices)
    tt = '\n'.join('     <triangle v1="%d" v2="%d" v3="%d"/>' % tuple(f) for f in m.faces)
    return ('<?xml version="1.0" encoding="UTF-8"?>\n<model unit="millimeter" xml:lang="en-US" %s>\n <metadata name="BambuStudio:3mfVersion">1</metadata>\n <resources>\n'
            '  <object id="1" p:UUID="%s" type="model">\n   <mesh>\n    <vertices>\n%s\n    </vertices>\n    <triangles>\n%s\n    </triangles>\n   </mesh>\n  </object>\n </resources>\n <build/>\n</model>\n') % (NS, uuid.uuid4(), vv, tt)
zin = zipfile.ZipFile('3mf_in/in.3mf'); files = {}
objs = []
for i, m in enumerate(meshes):
    files['3D/Objects/variant_%d.model' % i] = mesh_xml(m)
oid = 2
for k in range(len(VAR)):
    for c in range(COPIES):
        i = k * COPIES + c
        objs.append((oid, i, 128 - 1.5 * DX + c * DX, 128, 'Kupplung %d' % (c + 1), len(meshes[i].faces))); oid += 1
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
proc = {'layer_height': '0.28', 'wall_loops': '3', 'sparse_infill_density': '100%', 'sparse_infill_pattern': 'zig-zag', 'enable_support': '0',
        'brim_type': 'no_brim', 'seam_position': 'back', 'elefant_foot_compensation': '0.15',
        'outer_wall_speed': ['60'] * len(d['outer_wall_speed']), 'inner_wall_speed': ['120'] * len(d['inner_wall_speed'])}
d.update(proc)
for kk, vv in (('slow_down_layer_time', '8'), ('fan_max_speed', '100'), ('fan_min_speed', '100'), ('slow_down_for_layer_cooling', '1')):
    a = list(d[kk]); a[0] = vv; d[kk] = a
diff = list(d['different_settings_to_system'])
def add(i, keys):
    cur = [x for x in diff[i].split(';') if x]; diff[i] = ';'.join(sorted(set(cur) | set(keys)))
add(0, proc.keys()); add(1, ['slow_down_layer_time', 'fan_max_speed', 'fan_min_speed', 'slow_down_for_layer_cooling'])
d['different_settings_to_system'] = diff
files['Metadata/project_settings.config'] = json.dumps(d, indent=4, ensure_ascii=False)
skip = {'3D/Objects/object_%s.model' % s for s in ('19', '20', '21', '36', '38', '39')}
zout = zipfile.ZipFile('kupplung_v1.3mf', 'w', zipfile.ZIP_DEFLATED, compresslevel=9); done = set()
for it in zin.infolist():
    n = it.filename
    if n in skip: continue
    if n in files: zout.writestr(n, files[n]); done.add(n)
    else: zout.writestr(it, zin.read(n))
for n, c in files.items():
    if n not in done: zout.writestr(n, c)
zout.close()
print('ok', os.path.getsize('kupplung_v1.3mf'), [len(m.faces) for m in meshes])
