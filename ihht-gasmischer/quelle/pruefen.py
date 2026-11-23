# Prueft Teile als Netz: ein Koerper, wasserdicht, Volumen, Ausdehnung. Braucht trimesh, manifold3d, shapely.
#   python3 pruefen.py                         -> alle Teile einzeln
#   python3 pruefen.py splLoO2 frame6          -> nur diese Teile, dazu Ueberschneidungen untereinander (> 0,5 mm3)
#   python3 pruefen.py splLoO2 --stl ../druck  -> zusaetzlich STL schreiben (Lage wie im Modell)
import sys, itertools
import trimesh
import build
from ihht.netz import build as netz

args = sys.argv[1:]
stl = None
if '--stl' in args:
    i = args.index('--stl'); stl = args[i + 1]; del args[i:i + 2]
teile = build.alle_teile()
namen = args or list(teile)
M = {}
for n in namen:
    m = M[n] = netz(teile[n]); b = m.bounds
    print('%-11s vol %9.1f mm3  wasserdicht %-5s Körper %d  x %.1f..%.1f  y %.1f..%.1f  z %.1f..%.1f' % (
        n, m.volume, m.is_watertight, len(m.split()), b[0][0], b[1][0], b[0][1], b[1][1], b[0][2], b[1][2]))
    if stl: m.export('%s/%s.stl' % (stl, n))
if args and len(args) > 1:
    print('--- Überschneidungen (> 0,5 mm3) ---')
    k = 0
    for a, b in itertools.combinations(M, 2):
        ba, bb = M[a].bounds, M[b].bounds
        if (ba[0] > bb[1]).any() or (bb[0] > ba[1]).any(): continue
        v = trimesh.boolean.intersection([M[a], M[b]], engine='manifold').volume
        if v > 0.5: k += 1; print('  %s x %s: %.1f mm3' % (a, b, v))
    print('  keine' if k == 0 else '  %d Paare' % k)
