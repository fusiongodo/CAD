# Baut ansicht/index.html: STL + three.js (Bundle) eingebettet, läuft offline.
import base64, json, sys, pathlib
q = pathlib.Path(__file__).parent; root = q.parent
bundle = pathlib.Path(sys.argv[1]).read_text()
info = json.loads((root/'druck/info.json').read_text())
t = (q/'ansicht_template.html').read_text()
t = t.replace('__MASSE__', f"Außen {info['L']:.1f} × {info['W']:.1f} × {info['H']:.1f} mm · Loch Ø {info['hole_d']:.0f} · Raster {info['pitch']:.0f} mm")
t = t.replace('__INFO__', json.dumps(info)).replace('__VER__', info.get('version', 'v1'))
for k, f in (('__STL_BASE__', f"fiolenbox_{info.get('version','v1')}_unterteil.stl"), ('__STL_LID__', f"fiolenbox_{info.get('version','v1')}_deckel.stl")):
    t = t.replace(k, base64.b64encode((root/'druck'/f).read_bytes()).decode())
t = t.replace('__THREE__', bundle.replace('</script', '<\\/script'))
(root/'ansicht/index.html').write_text(t)
print('ok', len(t)//1024, 'KB')
