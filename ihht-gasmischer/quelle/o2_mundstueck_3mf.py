"""O2-Mundstück mit Reservoirbeutel: Netze aus teile/o2_mundstueck.py bauen, prüfen, Bambu-3MF in Drucklage schreiben.
<venv>/bin/python o2_mundstueck_3mf.py  ->  ../druck/o2_mundstueck.3mf (0,20 mm, 4 Wände, Brim 5, ohne Stützen)."""
import math, os
import trimesh
import build
from ihht.netz import build as netz
from ihht.bambu import write_3mf

HERE = os.path.dirname(os.path.abspath(__file__))
M = {p.name: netz(p) for p in build.teil('o2_mundstueck').bauen()}
ok = True
for n, m in M.items():
    b = m.bounds; k = len(m.split(only_watertight=False))
    print('%-8s vol %8.1f wasserdicht %-5s Körper %d  x %6.1f..%6.1f y %6.1f..%6.1f z %6.1f..%6.1f' % (n, m.volume, m.is_watertight, k, b[0][0], b[1][0], b[0][1], b[1][1], b[0][2], b[1][2]))
    ok &= m.is_watertight and k == 1
for a, b in (('o2mA', 'o2mRing'), ('o2mA', 'o2mB'), ('o2mRing', 'o2mB')):
    v = trimesh.boolean.intersection([M[a], M[b]], engine='manifold').volume
    print('%s x %s: %.3f mm3' % (a, b, v)); ok &= v < 0.2
print('PRÜFUNG', 'OK' if ok else 'FEHLER')


def lay(m, flip):
    m = m.copy()
    if flip:
        m.apply_transform(trimesh.transformations.rotation_matrix(math.pi, [1, 0, 0]))
    c = m.bounds.mean(axis=0)
    m.apply_translation([-c[0], -c[1], -m.bounds[0][2]])
    return m


PRINT = [('O2-Mundstueck A', lay(M['o2mA'], True), 100.0, 128.0), ('Ventilring', lay(M['o2mRing'], False), 150.0, 128.0),
         ('O2-Mundstueck B', lay(M['o2mB'], False), 195.0, 128.0)]
for name, m, x, y in PRINT:
    print('%-16s Drucklage %5.1f x %5.1f x %5.1f' % ((name,) + tuple(m.extents)))
write_3mf(PRINT, os.path.join(HERE, '..', 'druck', 'o2_mundstueck.3mf'))
raise SystemExit(0 if ok else 1)
