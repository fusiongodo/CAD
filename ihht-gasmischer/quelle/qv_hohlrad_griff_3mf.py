"""Hohlrad mit Steckgriff für das gebaute Quetschventil (Alex 26.11.): 2 x Hohlrad mit Kragen + 1 Griff Variante 2.
Der Griff wird axial von hinten über den Kragen geschoben (Variante 1, radial gesteckt, ist verworfen).
Geometrie in qv2/gen_getriebe.py (GRIFF, KR_*, GR_*). Prüft Hohlrad und Griff gegen Getriebeplatte, Ritzel, Welle, Rahmenplatte,
Motor; dazu den Schwenkbereich und das gerade Aufschieben.
<venv>/bin/python qv_hohlrad_griff_3mf.py  ->  ../druck/qv_hohlrad_griff_v2.3mf (ohne Stützen)."""
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
v = inter(rot(M['hohlrad'], 22.5), M['griff']); print('Griff x Hohlrad (gesteckt): %.2f mm3' % v); ok &= v < 0.2
FEST = ('platte', 'rahmen', 'nema', 'ritzel', 'welle')
frei = [w for w in range(-90, 91, 2) if all(inter(rot(M['griff'], w), M[f]) < 0.2 for f in FEST)]   # Griff mit Hohlrad geschwenkt
print('Griff frei beim Schwenken (0 = oben gesteckt): %d bis %d Grad' % (min(frei), max(frei)))
ok &= min(frei) <= -26 and max(frei) >= 26
def ab(w):                                                                     # bei Winkel w gerade nach hinten abziehen (Builder +Y = Ventil -z)
    for dy in (2, 4, 6, 8, 12, 20, 40):
        g = rot(M['griff'], w); g.apply_translation([0, dy, 0])
        if sum(inter(g, M[f]) for f in FEST) + inter(g, rot(M['hohlrad'], 22.5 + w)) >= 0.2: return False
    return True
zieh = [w for w in range(-60, 61, 2) if ab(w)]
print('Abziehen / Aufstecken frei: %d bis %d Grad' % (min(zieh), max(zieh)))
ok &= min(zieh) <= -26 and max(zieh) >= 26                                     # Schlitzpaare alle 45 Grad: je Seite mehr als 22,5 noetig
print('PRÜFUNG', 'OK' if ok else 'FEHLER')


def lay(m, vorn):                                                               # vorn = Seite Y min (zur Wand) aufs Bett
    m = m.copy()
    m.apply_transform(trimesh.transformations.rotation_matrix(math.pi / 2 if vorn else -math.pi / 2, [1, 0, 0]))
    c = m.bounds.mean(axis=0); m.apply_translation([-c[0], -c[1], -m.bounds[0][2]])
    return m


h = lay(M['hohlrad'], True)
g = M['griff'].copy(); g.apply_transform(trimesh.transformations.rotation_matrix(math.pi / 2, [0, 1, 0]))   # auf die Seite: Griffbreite nach oben
c = g.bounds.mean(axis=0); g.apply_translation([-c[0], -c[1], -g.bounds[0][2]])
PRINT = [('Hohlrad a', h, 80.0, 160.0), ('Hohlrad b', h.copy(), 176.0, 160.0), ('Steckgriff v2', g, 128.0, 70.0)]
for n, m, x, y in PRINT:
    print('%-10s Drucklage %5.1f x %5.1f x %5.1f' % ((n,) + tuple(m.extents)))
out = os.path.join(HERE, '..', 'druck', 'qv_hohlrad_griff_v2.3mf')
write_3mf(PRINT, out, proc=dict(layer_height='0.2', wall_loops='4', sparse_infill_density='40%', brim_type='no_brim', enable_support='0'))
print('3MF', out)
raise SystemExit(0 if ok else 1)
