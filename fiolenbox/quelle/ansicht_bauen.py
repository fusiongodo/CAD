# Baut ansicht/index.html: STL + three.js (Bundle) eingebettet, läuft offline.
import base64, json, sys, pathlib
q = pathlib.Path(__file__).parent; root = q.parent
bundle = pathlib.Path(sys.argv[1]).read_text()
INFO = sys.argv[2] if len(sys.argv) > 2 else 'info.json'      # z. B. info_11.json
info = json.loads((root/'druck'/INFO).read_text())
t = (q/'ansicht_template.html').read_text()
t = t.replace('__MASSE__', f"Außen {info['L']:.1f} × {info['W']:.1f} × {info['H']:.1f} mm · Loch Ø {info['hole_d']:g} · Raster {info['pitch']:g} mm")
t = t.replace('__RASTER__', f"{len({p[0] for p in info['pts']})} × {len({p[1] for p in info['pts']})}")
t = t.replace('__INFO__', json.dumps(info)).replace('__VER__', info.get('version', 'v1'))
for k, f in (('__STL_BASE__', info.get('base_stl', f"fiolenbox_{info.get('version','v1')}_unterteil.stl")), ('__STL_LID__', info.get('lid_stl', f"fiolenbox_{info.get('version','v1')}_deckel.stl"))):
    t = t.replace(k, base64.b64encode((root/'druck'/f).read_bytes()).decode())
t = t.replace('__THREE__', bundle.replace('</script', '<\\/script'))
out = root/'ansicht'/(sys.argv[3] if len(sys.argv) > 3 else 'index.html'); out.write_text(t)
print('ok', len(t)//1024, 'KB')
