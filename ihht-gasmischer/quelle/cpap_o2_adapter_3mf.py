"""Adapter CPAP-Schlauch (22 mm) auf dünne Tülle Ø 7, gerade 1:1 (Alex 28.11.): damit ein CPAP-Schlauch an den
Ausgang des Mischers (dünner Schlauch) angeschlossen werden kann. Wie die Splitter, aber ohne Verzweigung.
Maße aus ihht/anschluesse.py: 'cpap_pi' (Kern Ø 22,5, Wellen Ø 24, Bohrung Ø 20,4, 16,2 lang) und 'o2' (Kern Ø 7,
Wellen Ø 7,5, 12 lang). Bohrung der dünnen Tülle Ø 5,0 wie am T-Stück, Spitze auf Ø 6,2 angefast.
Dazwischen ein 45°-Kegel außen und innen (Wand 1,6), also keine Stufe im Gasweg.
Druck: CPAP-Seite unten, dünne Tülle oben, ohne Stützen.
<venv>/bin/python cpap_o2_adapter_3mf.py  ->  ../druck/cpap_o2_adapter.3mf"""
import os
import trimesh
from ihht.anschluesse import ANSCHLUSS
from ihht.bambu import write_3mf

HERE = os.path.dirname(os.path.abspath(__file__))
C, O = ANSCHLUSS['cpap_pi'], ANSCHLUSS['o2']
O_BOHR, O_SPITZE = 2.5, 3.1                 # Bohrung Ø 5,0, Spitze Ø 6,2 (wie T-Stück v3.4)
KEGEL_WAND = 2.3                            # Wand im Kegel, waagrecht gemessen (senkrecht zur Fläche 1,6)

ZC = C.laenge                               # Wandebene der CPAP-Tülle (Spitze bei z = 0)
ZO = ZC + (C.r_spitze - O.r_tal)            # Wandebene der dünnen Tülle = Kegelende
ZT = ZO + O.laenge                          # Spitze der dünnen Tülle

aussen = [(0.0, 0.0), (C.r_tal, 0.0)]
for w in reversed(range(C.wellen)):         # CPAP-Wellen von der Spitze (unten) zur Wand: Rampe, Plateau, Stufe
    a = C.ansatz + w * (C.plateau + C.rampe)
    aussen += [(C.r_tal, ZC - a - C.plateau - C.rampe), (C.r_spitze, ZC - a - C.plateau), (C.r_spitze, ZC - a), (C.r_tal, ZC - a)]
aussen += [(C.r_tal, ZC - (C.r_spitze - C.r_tal)), (C.r_spitze, ZC), (O.r_tal, ZO)]       # 45° hoch auf den Kegel, Kegel
for w in range(O.wellen):                   # dünne Tülle von der Wand zur Spitze (oben): Stufe, Plateau, Rampe
    a = O.ansatz + w * (O.plateau + O.rampe)
    letzte = w == O.wellen - 1
    aussen += [(O.r_tal, ZO + a), (O.r_spitze, ZO + a), (O.r_spitze, ZO + a + O.plateau),
               (O_SPITZE if letzte else O.r_tal, ZO + a + O.plateau + O.rampe)]
aussen += [(0.0, ZT)]
zk = ZC - (KEGEL_WAND - (C.r_spitze - C.r_innen))                                         # Beginn des Innenkegels
innen = [(0.0, -1.0), (C.r_innen, -1.0), (C.r_innen, zk), (O_BOHR, zk + C.r_innen - O_BOHR), (O_BOHR, ZT + 1.0), (0.0, ZT + 1.0)]


def dreh(pts):
    pts = [q for i, q in enumerate(pts) if i == 0 or abs(q[0] - pts[i - 1][0]) + abs(q[1] - pts[i - 1][1]) > 1e-9]   # doppelte Punkte weg
    m = trimesh.creation.revolve(pts, sections=128)
    if m.volume < 0: m.invert()
    return m


m = trimesh.boolean.difference([dreh(aussen), dreh(innen)], engine='manifold')
ok = m.is_watertight and len(m.split(only_watertight=False)) == 1
print('Adapter: Höhe %.1f, größter Ø %.1f, wasserdicht %s, Volumen %.2f cm3' % (m.extents[2], m.extents[0], m.is_watertight, m.volume / 1000))
print('CPAP-Seite: Kern Ø %.1f, Wellen Ø %.1f, Bohrung Ø %.1f, %.1f lang' % (2 * C.r_tal, 2 * C.r_spitze, 2 * C.r_innen, C.laenge))
print('dünne Tülle: Kern Ø %.1f, Wellen Ø %.1f, Bohrung Ø %.1f, Spitze Ø %.1f, %.1f lang' % (2 * O.r_tal, 2 * O.r_spitze, 2 * O_BOHR, 2 * O_SPITZE, O.laenge))
wand = min(C.r_tal - C.r_innen, O.r_tal - O_BOHR, KEGEL_WAND * 0.7071)
print('dünnste Wand %.2f' % wand); ok &= wand >= 0.95
out = os.path.join(HERE, '..', 'druck', 'cpap_o2_adapter.3mf')
write_3mf([('CPAP auf Tülle 7', m, 128.0, 128.0)], out, proc=dict(layer_height='0.2', wall_loops='4', sparse_infill_density='40%', brim_type='no_brim', enable_support='0'))
print('3MF', out, 'PRÜFUNG', 'OK' if ok else 'FEHLER')
raise SystemExit(0 if ok else 1)
