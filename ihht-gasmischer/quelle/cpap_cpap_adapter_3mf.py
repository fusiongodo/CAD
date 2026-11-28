"""Adapter CPAP-Schlauch auf CPAP-Schlauch (22 mm), gerade (Alex 28.11., eilig): zwei CPAP-Tüllen Rücken an Rücken.
Maße aus ihht/anschluesse.py 'cpap_pi' (Kern Ø 22,5, Wellen Ø 24, Bohrung Ø 20,4, je 16,2 lang), dazwischen 4 mm Bund Ø 24.
Druck: stehend, ohne Stützen. An der oberen Tülle sind die Wellen-Stufen als 45°-Schräge ausgeführt (sonst Überhang).
<venv>/bin/python cpap_cpap_adapter_3mf.py  ->  ../druck/cpap_cpap_adapter.3mf   (venv: ~/.cache/ihht_venv)"""
import os
import trimesh
from ihht.anschluesse import ANSCHLUSS
from ihht.bambu import write_3mf

HERE = os.path.dirname(os.path.abspath(__file__))
C = ANSCHLUSS['cpap_pi']
BUND = 4.0
ZC = C.laenge; Z2 = ZC + BUND; ZT = Z2 + C.laenge; H = C.r_spitze - C.r_tal

aussen = [(0.0, 0.0), (C.r_tal, 0.0)]
for w in reversed(range(C.wellen)):         # untere Tülle, Spitze unten: Rampe, Plateau, Stufe
    a = C.ansatz + w * (C.plateau + C.rampe)
    aussen += [(C.r_tal, ZC - a - C.plateau - C.rampe), (C.r_spitze, ZC - a - C.plateau), (C.r_spitze, ZC - a), (C.r_tal, ZC - a)]
aussen += [(C.r_tal, ZC - H), (C.r_spitze, ZC), (C.r_spitze, Z2), (C.r_tal, Z2)]           # Bund
for w in range(C.wellen):                   # obere Tülle, Spitze oben: 45°-Schräge statt Stufe, Plateau, Rampe
    a = C.ansatz + w * (C.plateau + C.rampe)
    aussen += [(C.r_tal, Z2 + a - H), (C.r_spitze, Z2 + a), (C.r_spitze, Z2 + a + C.plateau), (C.r_tal, Z2 + a + C.plateau + C.rampe)]
aussen += [(0.0, ZT)]
innen = [(0.0, -1.0), (C.r_innen, -1.0), (C.r_innen, ZT + 1.0), (0.0, ZT + 1.0)]


def dreh(pts):
    pts = [q for i, q in enumerate(pts) if i == 0 or abs(q[0] - pts[i - 1][0]) + abs(q[1] - pts[i - 1][1]) > 1e-9]
    m = trimesh.creation.revolve(pts, sections=128)
    if m.volume < 0: m.invert()
    return m


m = trimesh.boolean.difference([dreh(aussen), dreh(innen)], engine='manifold')
ok = m.is_watertight and len(m.split(only_watertight=False)) == 1
print('Adapter: Höhe %.1f, größter Ø %.1f, Bohrung Ø %.1f, wasserdicht %s' % (m.extents[2], m.extents[0], 2 * C.r_innen, m.is_watertight))
out = os.path.join(HERE, '..', 'druck', 'cpap_cpap_adapter.3mf')
write_3mf([('CPAP auf CPAP', m, 128.0, 128.0)], out, proc=dict(layer_height='0.2', wall_loops='4', sparse_infill_density='40%', brim_type='no_brim', enable_support='0'))
print('3MF', out, 'PRÜFUNG', 'OK' if ok else 'FEHLER')
raise SystemExit(0 if ok else 1)
