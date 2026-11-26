"""Ritzel v2 für die neuen Motoren mit längerem Schaft (Alex 26.11.), 2 Stück. Geometrie: qv2/gen_getriebe.py (RITZEL_LANG).
Prüft den Eingriff ins Hohlrad über eine Zahnteilung und das Ritzel gegen Getriebeplatte und Motor-Dummy (Schaft 24).
<venv>/bin/python qv_ritzel_lang_3mf.py  ->  ../druck/qv_ritzel_lang.3mf (Motorseite unten, ohne Stützen)."""
import math, os
import trimesh
from ihht import qv2
from ihht.netz import build as netz
from ihht.bambu import write_3mf

G = qv2.G
HERE = os.path.dirname(os.path.abspath(__file__))
T = lambda p: netz(qv2._teil(p, p.name, qv2.ORANGE, True, 0.0, 0.0))           # Builder: X = x, Y = -(z + 6), Z = y
M = {'ritzel': T(G.ritzel), 'hohlrad': T(G.hohlrad), 'platte': T(G.platte), 'nema': T(qv2._nema())}
r = M['ritzel']; k = len(r.split(only_watertight=False)); e = r.extents
print('Ritzel wasserdicht %s, Körper %d, %.1f x %.1f x %.1f' % (r.is_watertight, k, e[0], e[1], e[2]))
ok = r.is_watertight and k == 1
inter = lambda a, b: trimesh.boolean.intersection([a, b], engine='manifold').volume
rot = lambda m, w, c: m.copy().apply_transform(trimesh.transformations.rotation_matrix(w, [0, 1, 0], [c[0], 0, c[1]]))
g = 0.0
for i in range(24):                                                             # eine Teilung des Hohlrads, Ritzel ZR/ZP-mal so schnell, gleiche Richtung
    th = 2 * math.pi / G.ZR * i / 24
    g = max(g, inter(rot(M['hohlrad'], th, (0, 0)), rot(r, th * G.ZR / G.ZP, (0, -G.A))))
print('Eingriff Ritzel x Hohlrad, größte Überschneidung: %.3f mm3' % g); ok &= g < 0.2
for f in ('platte',):
    v = inter(r, M[f]); print('Ritzel x %s: %.3f mm3' % (f, v)); ok &= v < 0.2
v = inter(M['nema'], M['hohlrad']); print('Motorschaft (24 ab Flansch) x Hohlrad: %.3f mm3' % v); ok &= v < 0.2
print('PRÜFUNG', 'OK' if ok else 'FEHLER')
m = r.copy(); m.apply_transform(trimesh.transformations.rotation_matrix(-math.pi / 2, [1, 0, 0]))    # Motorseite (Y max) aufs Bett
c = m.bounds.mean(axis=0); m.apply_translation([-c[0], -c[1], -m.bounds[0][2]])
print('Drucklage %.1f x %.1f x %.1f' % tuple(m.extents))
PRINT = [('Ritzel lang a', m, 113.0, 128.0), ('Ritzel lang b', m.copy(), 143.0, 128.0)]
out = os.path.join(HERE, '..', 'druck', 'qv_ritzel_lang.3mf')
write_3mf(PRINT, out, proc=dict(layer_height='0.2', wall_loops='6', sparse_infill_density='40%', brim_type='outer_only', brim_width='5', enable_support='0'))
print('3MF', out)
# ---------- Alles für die neuen Motoren in einer Datei (Alex 26.11.): 2 Ritzel + 2 Distanzplatten ----------
cyl = lambda d, x, y: trimesh.creation.cylinder(radius=d / 2, segment=[[x, y, -1], [x, y, 3]], sections=64)
dp = trimesh.creation.box(extents=[42.3, 42.3, 2.0]); dp.apply_translation([0, 0, 1.0])              # wie qv_motor_distanz_3mf.py
dp = trimesh.boolean.difference([dp, cyl(22.4, 0, 0)] + [cyl(3.4, sx * 15.5, sy * 15.5) for sx in (-1, 1) for sy in (-1, 1)], engine='manifold')
assert dp.is_watertight
P2 = [('Ritzel lang a', m, 108.0, 165.0), ('Ritzel lang b', m.copy(), 148.0, 165.0), ('Motor-Distanz a', dp, 100.0, 110.0), ('Motor-Distanz b', dp.copy(), 156.0, 110.0)]
out2 = os.path.join(HERE, '..', 'druck', 'qv_neue_motoren.3mf')
write_3mf(P2, out2, proc=dict(layer_height='0.2', wall_loops='6', sparse_infill_density='40%', brim_type='outer_only', brim_width='5', enable_support='0'))
print('3MF', out2)
raise SystemExit(0 if ok else 1)
