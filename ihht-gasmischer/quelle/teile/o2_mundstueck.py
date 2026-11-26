# O2-Mundstueck mit Reservoirbeutel ("Pfeife", 26.11.2026, Alex): fuer die hyperoxische Phase. Nur einatmen, zum Ausatmen
# aus dem Mund nehmen: kein Ausatemventil. Achse = Z, Mund oben.
#   unten (Z = 0)     Kegeltuelle 5 -> 7 fuer den duennen O2-Schlauch, Bohrung 3
#   seitlich 45°      Beutelring Ø 20 mit Wulst, zeigt zur Schlauchseite (Beutel liegt neben dem O2-Schlauch)
#   Mitte             loser Ventilring wie im CPAP-Reservoir (Sitz Ø 19, Kreuz, Pilzknopf, Membran Ø 21,5), nur 10 hoch
#   oben              Mundstueck B: steckt im Grundkoerper (Klemmsitz, kein Gewinde) und drueckt den Ring auf den Konus
# Die Klappe haelt den Sauerstoff zurueck, solange niemand zieht: der Beutel fuellt sich. Ist er voll, drueckt der
# Ueberschuss die Membran auf und entweicht durchs Mundstueck. Beim Zug kommt der Beutelinhalt auf einmal.
# Druck ohne Stuetzen: A mit der Oeffnung nach unten (Tuelle oben), Ring Kreuz unten, B Stecker unten.
import math
from ihht.geo import Part, C, Cz, P

S = math.sqrt(0.5)
RC = 10.2                                          # Kammer innen (wie CPAP-Reservoir: Ring sitzt auf demselben Konus)
MEM_D, SEAT_D, MEM_SP = 21.5, 19.0, 0.5
SEAT_H, RING_H = 3.0, 10.0
TS = 1.5
PIN = dict(neck=2.5, gap=1.0, head=6.5, flare=60.0, land=0.3, lead=1.5, tip=1.0)
RING_R = MEM_D / 2 + MEM_SP + 1.2                  # 12,45
RING_FASE = RING_R - RC
R_SOCK = RING_R + 0.2                              # Aufnahme in A
R_BODY = R_SOCK + 1.6                              # 14,25: Aussen Ø 28,5
PLUG_SP = 0.08                                     # Klemmsitz B in A (Radius)

TU_R0, TU_R1, TU_L, TU_BOHR = 2.5, 3.5, 12.0, 3.0  # O2-Tuelle
Z_K0 = TU_L + (R_BODY - TU_R1)                     # Aussenkegel Tuelle -> Rohr endet hier (22,75)
ZJ = 34.0                                          # Beutel-Achse schneidet die Hauptachse
BAG_R, BAG_RI, BAG_L = 10.0, 8.0, 40.0             # Beutelring Ø 20 / 16, Laenge ab Hauptachse
NB = (S, 0.0, -S)
Z_L = ZJ + 6.0
Z_R0 = Z_L + 0.15
Z_R1 = Z_R0 + RING_H
PLUG_L = 7.0
ZA = Z_R1 + PLUG_L                                 # Oberkante A
ZF0, FL_H = ZA + 0.3, 3.0
ZF1 = ZF0 + FL_H
MU_R, MU_RI, MU_L = 7.5, 5.5, 14.0                 # Mundrohr Ø 15 / 11
ZM0 = ZF1 + (R_BODY - MU_R)
ZM1 = ZM0 + MU_L

COL_A, COL_B = (0.3, 0.6, 0.85), (0.85, 0.55, 0.3)


def _bag(t):
    return (NB[0] * t, 0.0, ZJ + NB[2] * t)


def teil_a():
    a = Part('o2mA', 'O2-Mundstück A: Grundkörper (Schlauchtülle, Beutelring 45°, Ringaufnahme)', COL_A)
    a.add(('cone', (0, 0, 0), (0, 0, TU_L), TU_R0, TU_R1),
          ('cone', (0, 0, TU_L - 0.01), (0, 0, Z_K0), TU_R1, R_BODY),
          Cz(0, 0, Z_K0 - 0.01, ZA, 2 * R_BODY),
          C(_bag(0), _bag(BAG_L), 2 * BAG_R),
          ('cone', _bag(BAG_L - 4.0), _bag(BAG_L - 3.2), BAG_R, BAG_R + 0.8),          # Wulst: Beutel rutscht nicht ab
          C(_bag(BAG_L - 3.21), _bag(BAG_L - 1.6), 2 * (BAG_R + 0.8)),
          ('cone', _bag(BAG_L - 1.61), _bag(BAG_L - 0.8), BAG_R + 0.8, BAG_R))
    zc = Z_K0 + 1.0                                                                     # Kammerboden: 45°-Kegel auf die Bohrung
    a.cut(Cz(0, 0, -1, zc - (RC - TU_BOHR / 2) + 0.01, TU_BOHR),
          ('cone', (0, 0, zc - (RC - TU_BOHR / 2)), (0, 0, zc), TU_BOHR / 2, RC),
          Cz(0, 0, zc - 0.01, Z_L + 0.01, 2 * RC),
          ('cone', (0, 0, Z_L), (0, 0, Z_L + R_SOCK - RC), RC, R_SOCK),                 # 45°-Konus, Ringauflage
          Cz(0, 0, Z_L + R_SOCK - RC - 0.01, ZA + 1, 2 * R_SOCK),
          ('cone', (0, 0, ZA - 0.6), (0, 0, ZA + 0.01), R_SOCK, R_SOCK + 0.61),          # Einfuehrfase
          C(_bag(0), _bag(BAG_L + 1), 2 * BAG_RI))
    return a


def ventilring():
    r = Part('o2mRing', 'O2-Mundstück Ventilring (Sitz Ø 19, Membran Ø 21,5)', (0.95, 0.95, 0.95))
    zs = Z_R0 + SEAT_H
    r.add(('cone', (0, 0, Z_R0), (0, 0, Z_R0 + RING_FASE), RC, RING_R),
          Cz(0, 0, Z_R0 + RING_FASE - 0.01, Z_R1, 2 * RING_R))
    r.cut(Cz(0, 0, Z_R0 - 1, zs + 0.01, SEAT_D),
          Cz(0, 0, zs, Z_R1 + 1, MEM_D + 2 * MEM_SP))
    rw = SEAT_D / 2 + 0.3
    kreuz = [(-rw, 0), (rw, 0), (rw, -SEAT_H), (TS, -SEAT_H), (0, -TS), (-TS, -SEAT_H), (-rw, -SEAT_H)]
    p = PIN
    fl = (p['head'] - p['neck']) / 2 * math.tan(math.radians(p['flare']))
    z1 = zs + p['gap']; z2 = z1 + fl; z3 = z2 + p['land']; z4 = z3 + p['lead']
    r.add(P(0, zs, -TS / 2, TS / 2, kreuz),
          ('prismx', (0, zs), tuple(kreuz), -TS / 2, TS / 2),
          Cz(0, 0, zs - TS / 2, z1, p['neck']),
          ('cone', (0, 0, z1), (0, 0, z2), p['neck'] / 2, p['head'] / 2),
          Cz(0, 0, z2, z3, p['head']),
          ('cone', (0, 0, z3), (0, 0, z4), p['head'] / 2, p['tip'] / 2))
    return r


def teil_b():
    b = Part('o2mB', 'O2-Mundstück B: Mundrohr (steckt in A, drückt den Ring an)', COL_B)
    rp = R_SOCK - PLUG_SP
    b.add(('cone', (0, 0, Z_R1), (0, 0, Z_R1 + 0.6), rp - 0.6, rp),                     # Einfuehrfase am Stecker
          Cz(0, 0, Z_R1 + 0.59, ZF0 + 0.01, 2 * rp),
          Cz(0, 0, ZF0, ZF1, 2 * R_BODY),
          ('cone', (0, 0, ZF1 - 0.01), (0, 0, ZM0), R_BODY, MU_R),
          Cz(0, 0, ZM0 - 0.01, ZM1, 2 * MU_R),
          ('cone', (0, 0, ZM1 - 3.6), (0, 0, ZM1 - 2.6), MU_R, MU_R + 1.0),             # Lippenwulst
          Cz(0, 0, ZM1 - 2.61, ZM1 - 1.0, 2 * (MU_R + 1.0)),
          ('cone', (0, 0, ZM1 - 1.01), (0, 0, ZM1), MU_R + 1.0, MU_R))
    zi = ZF1 + 0.5
    b.cut(Cz(0, 0, Z_R1 - 1, zi + 0.01, 2 * RC),
          ('cone', (0, 0, zi), (0, 0, zi + RC - MU_RI), RC, MU_RI),
          Cz(0, 0, zi + RC - MU_RI - 0.01, ZM1 + 1, 2 * MU_RI))
    return b


def bauen():
    return [teil_a(), ventilring(), teil_b()]
