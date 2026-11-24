# Bambu-3MF aus trimesh-Netzen (eine Druckplatte), übernommen aus quetschventil_v3_3mf.py.
# PRINT = [(name, netz in Drucklage, x, y)], Druckeinstellungen über proc (Vorgabe PROC).
import json, os, uuid, zipfile

TEMPLATE = os.environ.get('BAMBU_VORLAGE', os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..', 'druck', 'passungsraster.3mf'))
PROC = {'layer_height': '0.2', 'wall_loops': '4', 'sparse_infill_density': '40%', 'enable_support': '0',
        'brim_type': 'outer_only', 'brim_width': '5'}
FIL_PREFIX = ('close_fan_the_first_x_layers', 'textured_plate_temp', 'hot_plate_temp')


def write_3mf(PRINT, out3, proc=None, template=TEMPLATE):
    NS = ('xmlns="http://schemas.microsoft.com/3dmanufacturing/core/2015/02" xmlns:BambuStudio="http://schemas.bambulab.com/package/2021" '
          'xmlns:p="http://schemas.microsoft.com/3dmanufacturing/production/2015/06" requiredextensions="p"')

    def mesh_xml(m, oid):
        vv = '\n'.join('     <vertex x="%.5f" y="%.5f" z="%.5f"/>' % tuple(p) for p in m.vertices)
        tt = '\n'.join('     <triangle v1="%d" v2="%d" v3="%d"/>' % tuple(t) for t in m.faces)
        return ('<?xml version="1.0" encoding="UTF-8"?>\n<model unit="millimeter" xml:lang="en-US" %s>\n <metadata name="BambuStudio:3mfVersion">1</metadata>\n <resources>\n'
                '  <object id="%d" p:UUID="%s" type="model">\n   <mesh>\n    <vertices>\n%s\n    </vertices>\n    <triangles>\n%s\n    </triangles>\n   </mesh>\n  </object>\n </resources>\n <build/>\n</model>\n') % (NS, oid, uuid.uuid4(), vv, tt)

    zin = zipfile.ZipFile(template)
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
    proc = dict(PROC, **(proc or {}))
    d.update(proc)
    nfil = len(d['filament_settings_id'])
    fil = {k for k in proc if k.startswith(FIL_PREFIX)}                 # Filament-Werte (Liste je Filament) -> Filament-Diff
    diff = list(d['different_settings_to_system'])
    diff[0] = ';'.join(sorted((set(x for x in diff[0].split(';') if x) | set(proc) - fil) - {'support_on_build_plate_only', 'support_remove_small_overhang'}))
    diff[1] = ';'.join(sorted(set(x for x in diff[1].split(';') if x) | fil))
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

