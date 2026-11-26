"""Hohlrad mit Steckgriff für das gebaute Quetschventil (Alex 26.11.): 2 x Hohlrad mit Kragen + 1 Gabelgriff.
Geometrie in qv2/gen_getriebe.py (GRIFF, KR_*, GR_*). Prüft Hohlrad und Griff gegen Getriebeplatte, Ritzel, Welle, Rahmenplatte.
<venv>/bin/python qv_hohlrad_griff_3mf.py  ->  ../druck/qv_hohlrad_griff.3mf (ohne Stützen)."""
import math, os
import trimesh
from ihht import qv2
from ihht.netz import build as netz
from ihht.bambu import write_3mf

G, RV = qv2.G, qv2.RV
HERE = os.path.dirname(os.path.abspath(__file__))
T = lambda p: netz(qv2._teil(p, p.name, qv2.ORANGE, True, 0.0, 0.0))           # Builder-Koordinaten: Y = -(z + 6)
M = {'hohlrad': T(G.hohlrad), 'griff': T(G.griff()), 'platte': T(G.platte), 'ritzel': T(G.ritzel), 'welle': T(G.welle),
     'rahmen': T(RV.platte), 'nema': T(qv2._nema())}
ok = True
for n in ('hohlrad', 'griff'):
    m = M[n]; k = len(m.split(only_watertight=False)); e = m.extents
    print('%-8s wasserdicht %-5s Körper %d  %5.1f x %5.1f x %5.1f' % (n, m.is_watertight, k, e[0], e[1], e[2])); ok &= m.is_watertight and k == 1
rot = lambda m, w: m.copy().apply_transform(trimesh.transformations.rotation_matrix(math.radians(w), [0, 1, 0]))   # um die Hauptachse (Builder-Y)
inter = lambda a, b: trimesh.boolean.intersection([a, b], engine='manifold').volume
for w in range(0, 60, 5):                                                       # Hohlrad dreht: Kragen und Schlitze gegen alles Feste
    for f in ('platte', 'rahmen', 'nema'):
        v = inter(rot(M['hohlrad'], w), M[f]); ok &= v < 0.2
        if v >= 0.2: print('Hohlrad %3d° x %s: %.1f mm3' % (w, f, v))
v = inter(M['hohlrad'], M['griff']); print('Griff x Hohlrad (gesteckt): %.2f mm3' % v); ok &= v < 0.2
frei = []
for w in range(0, 360, 5):                                                      # Griff mit Hohlrad gedreht: wo geht er frei?
    g = rot(M['griff'], w)
    if all(inter(g, M[f]) < 0.2 for f in ('platte', 'rahmen', 'nema', 'ritzel')): frei.append(w)
print('Griff frei bei Drehung (0 = nach oben gesteckt, Grad):', frei)
print('PRÜFUNG', 'OK' if ok else 'FEHLER')


def lay(m, vorn):                                                               # vorn = Seite Y min (zur Wand) aufs Bett
    m = m.copy()
    m.apply_transform(trimesh.transformations.rotation_matrix(math.pi / 2 if vorn else -math.pi / 2, [1, 0, 0]))
    c = m.bounds.mean(axis=0); m.apply_translation([-c[0], -c[1], -m.bounds[0][2]])
    return m


h, g = lay(M['hohlrad'], True), lay(M['griff'], True)
g.apply_transform(trimesh.transformations.rotation_matrix(math.pi / 2, [0, 0, 1])); c = g.bounds.mean(axis=0); g.apply_translation([-c[0], -c[1], 0])
PRINT = [('Hohlrad a', h, 80.0, 150.0), ('Hohlrad b', h.copy(), 176.0, 150.0), ('Steckgriff', g, 128.0, 70.0)]
for n, m, x, y in PRINT:
    print('%-10s Drucklage %5.1f x %5.1f x %5.1f' % ((n,) + tuple(m.extents)))
out = os.path.join(HERE, '..', 'druck', 'qv_hohlrad_griff.3mf')
write_3mf(PRINT, out, proc=dict(layer_height='0.2', wall_loops='4', sparse_infill_density='40%', brim_type='no_brim', enable_support='0'))
print('3MF', out)
raise SystemExit(0 if ok else 1)
