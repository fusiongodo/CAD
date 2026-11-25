"""Gegenlager 6200 vorn im Steg fuer das Exzenter-Quetschventil (Rahmen v2 + Getriebe v1), Stand 23.11.2026.

Anlass: Steht der Exzenter unten, hebelt der Schlauch die Exzenterachse zur Mitte. Die Welle laeuft nur hinten
(Lagersitz 21,8 in Pilot 22,4, Zapfen 14 in 14,5), die Last sitzt fliegend davor. Abhilfe bis v3: vorn ein
Kugellager 6200 (10 x 30 x 9) im Steg, das die Welle auf der Hauptachse fuehrt. Rahmen, Ambosse, Getriebe, Welle
und die 6203 bleiben.

Teile:
  Gegenlagerscheibe  ersetzt Griff-/Klemmscheibe. Liegt auf den Innenringen der 6203 (z 27), greift mit 2 Loechern
                     ueber die Mitnehmerstifte der Welle (x e, y ±5,5) und traegt auf der Hauptachse Bund Ø 13,5 und
                     Zapfen Ø 10 (3 Quetschrippen) fuer das 6200. Keine Schraube: nach vorn haelt der Innenring des
                     6200 die Scheibe (0,2 Luft), nach hinten liegt sie auf den 6203. Je Welle eine Variante,
                     e 3,5 / 4,5 / 5,0 mit 0 / 1 / 2 Kennpunkten oben (wie auf den Wellen). Druck: Scheibe unten.
  Steg 6200          Steg v2 mit 7,2 statt 6 Dicke, Mittelsteg (bindet oberen und unteren Amboss direkt) und Nabe
                     Ø 36 fuer das 6200: von vorn eingepresst bis an den Rand hinten. Druck: Vorderseite unten.

Koordinaten wie quetschventil_v1.fs / gen_getriebe.py: Z = Hauptachse, Rahmenplatte z -6 .. 0, Lager 6203 z 3 .. 27,
Ambosse z 0 .. 34, Steg ab z 34.
         python3 build_gegenlager.py    prueft und schreibt druck/gegenlager_6200.3mf und gegenlager_6200.fs
"""
import math

import gen_getriebe as G
import gen_rahmen as RV
from gen_getriebe import B, Cz, K, P, Part

# ---------------- 6200 ----------------
L6200 = (10.0, 30.0, 9.0)       # d, D, B
SEAT_D, SEAT_RIB = 30.3, 0.25   # Sitz Ø 30,3 + 6 Quetschrippen 0,25 (Innenkreis 29,8), wie 608-Sitz v3: Schaetzwert
LIP_ID = 24.5                   # Rand hinten: nur am Aussenring (Aussenring innen ≈ 25,9, Innenring aussen ≈ 14,6)
STUB_D, STUB_RIB = 9.95, 0.12   # Zapfen fuer den Innenring 10, 3 Rippen bis Ø 10,19: Schaetzwert
SHOULDER_D = 13.5               # Bund: liegt nur am Innenring des 6200 an

# ---------------- Lagen in z ----------------
TS = 7.2                        # Stegdicke (v2: 6), damit das 6200 (9) + Rand (1,2) vorn buendig endet
Z_FRONT = RV.LA + TS            # 41,2 Vorderseite Steg und Lager
Z_B0 = Z_FRONT - L6200[2]       # 32,2 Lager hinten
Z_HUB0 = Z_B0 - 1.2             # 31,0 Nabe hinten (Rand)
HUB_D = 36.0
Z_D0, Z_D1 = G.ZB1, G.ZB1 + 3.3   # Scheibe 27 .. 30,3 (Stifte enden bei 29)
Z_SH1 = Z_B0 - 0.2              # Bund endet 0,2 vor dem Innenring
Z_ST1 = Z_FRONT - 0.3           # Zapfenende
BAR = 9.0                       # halbe Breite Mittelsteg

VARIANTEN = ((3.5, 0), (4.5, 1), (5.0, 2))    # (e, Kennpunkte)


def hull(e):
    """Konvexe Huelle aus Kreis r 11 um (e, 0) und Bund um (0, 0) (ohne shapely: Andrew-Kette ueber Stuetzpunkte)."""
    pts = sorted(set([(round(e + 11.0 * math.cos(2 * math.pi * k / 192), 4), round(11.0 * math.sin(2 * math.pi * k / 192), 4)) for k in range(192)]
                     + [(round(SHOULDER_D / 2 * math.cos(2 * math.pi * k / 192), 4), round(SHOULDER_D / 2 * math.sin(2 * math.pi * k / 192), 4)) for k in range(192)]))
    kreuz = lambda o, a_, b_: (a_[0] - o[0]) * (b_[1] - o[1]) - (a_[1] - o[1]) * (b_[0] - o[0])
    unten, oben = [], []
    for p in pts:
        while len(unten) >= 2 and kreuz(unten[-2], unten[-1], p) <= 0:
            unten.pop()
        unten.append(p)
    for p in reversed(pts):
        while len(oben) >= 2 and kreuz(oben[-2], oben[-1], p) <= 0:
            oben.pop()
        oben.append(p)
    return unten[:-1] + oben[:-1]


def rib(r0, r1, w, ang):
    import math
    c, s = math.cos(ang), math.sin(ang)
    pts = [(r0, -w / 2), (r1, -w / 2), (r1, w / 2), (r0, w / 2)]
    return [(round(x * c - y * s, 4), round(x * s + y * c, 4)) for x, y in pts]


def scheibe(e, dots, dx=0.0):
    import math
    p = Part('scheibe_e%s' % ('%g' % e).replace('.', '_'),
             'QV Gegenlagerscheibe 6200 fuer Welle e %s (%d Punkte)' % (('%g' % e).replace('.', ','), dots))
    p.add(P(dx, 0, Z_D0, Z_D1, hull(e)),
          Cz(dx, 0, Z_D1 - 0.01, Z_SH1, SHOULDER_D),
          Cz(dx, 0, Z_SH1 - 0.01, Z_ST1 - 0.5, STUB_D),
          K(dx, 0, Z_ST1 - 0.51, Z_ST1, STUB_D / 2, STUB_D / 2 - 0.45))
    for k in range(3):
        p.add(P(dx, 0, Z_SH1 + 0.6, Z_ST1 - 0.8, rib(STUB_D / 2 - 0.2, STUB_D / 2 + STUB_RIB, 0.9, 2 * math.pi * k / 3 + math.pi / 2)))
    p.cut(Cz(dx + e, 5.5, Z_D0 - 1, G.ZB1 + 2.6, 3.3), Cz(dx + e, -5.5, Z_D0 - 1, G.ZB1 + 2.6, 3.3),       # Mitnehmerstifte Ø 2,8
          K(dx + e, 5.5, Z_D0 - 0.01, Z_D0 + 0.4, 2.05, 1.65), K(dx + e, -5.5, Z_D0 - 0.01, Z_D0 + 0.4, 2.05, 1.65))
    for k in range(dots):
        p.cut(Cz(dx + e + 8, 0 if dots == 1 else (-2.5 + 5 * k), Z_D1 - 0.6, Z_D1 + 1, 1.2))
    return p


def steg():
    import math
    s_ = Part('steg6200', 'QV-Rahmen v2 Steg vorn mit Gegenlager 6200 (7,2 dick)')
    s_.add(B(-RV.XA, RV.XA, -RV.YP, RV.YP, RV.LA, Z_FRONT))
    for s in (-1, 1):
        s_.add(B(-RV.LIP_X, RV.LIP_X, s * (RV.YO + RV.PLAY), s * RV.YP, RV.LA - RV.LIP_H, RV.LA + 0.01))
    s_.cut(B(-(RV.XA - 11), RV.XA - 11, -(RV.YA - 0.5), RV.YA - 0.5, RV.LA - 1, Z_FRONT + 1))
    s_.add(B(-BAR, BAR, -(RV.YA - 0.4), RV.YA - 0.4, RV.LA, Z_FRONT), Cz(0, 0, Z_HUB0, Z_FRONT, HUB_D))
    s_.cut(Cz(0, 0, Z_B0, Z_FRONT + 1, SEAT_D),
           Cz(0, 0, Z_HUB0 - 1, Z_B0 + 0.01, LIP_ID),
           K(0, 0, Z_FRONT - 0.6, Z_FRONT + 0.01, SEAT_D / 2, SEAT_D / 2 + 0.6))         # Einfuehrfase
    for sx in (-1, 1):
        for s in (-1, 1):
            s_.cut(Cz(sx * RV.SX, s * RV.SY, RV.LA - 1, Z_FRONT + 1, RV.M3_CLR))
    for k in range(6):
        s_.add(P(0, 0, Z_B0, Z_FRONT - 0.8, rib(SEAT_D / 2 - SEAT_RIB, SEAT_D / 2 + 0.2, 1.0, 2 * math.pi * k / 6 + math.pi / 6)))
    return s_


def lager6200(dx=0.0):
    p = Part('lager6200', 'Lager 6200 (Dummy)')
    p.add(Cz(dx, 0, Z_B0, Z_FRONT, L6200[1]))
    p.cut(Cz(dx, 0, Z_B0 - 1, Z_FRONT + 1, L6200[0]))
    return p


STEG = steg()
SCHEIBEN = [scheibe(e, d) for e, d in VARIANTEN]


# ---------------- Welle mit Zapfen (gen_getriebe, QV_ZAPFEN=1): loser Distanzring, Huelse, Lager-Dummies ----------------
RING_Z1 = Z_B0 - 0.2            # 0,2 Luft zum Innenring des 6200


def distanzring(e):
    """Lose zwischen 6203 und 6200 (haelt die 6203 axial, traegt keine Last). Breite Seite zu den 6203, Bund nach vorn."""
    p = Part('distanzring_e%s' % ('%g' % e).replace('.', '_'), 'QV Distanzring zwischen 6203 und 6200 (lose, e %s)' % ('%g' % e).replace('.', ','))
    p.add(P(0, 0, G.ZB1, G.ZB1 + 3.3, hull(e)), Cz(0, 0, G.ZB1 + 3.29, RING_Z1, SHOULDER_D))
    p.cut(Cz(0, 0, G.ZB1 - 1, RING_Z1 + 1, 10.6))
    return p


def huelse(e):
    p = Part('huelse', 'QV Lagerhuelse weit (Wand 2,5, ueber beide 6203)')
    p.add(Cz(e, 0, G.ZB0, G.ZB1 + 1.0, 45.4))
    p.cut(Cz(e, 0, G.ZB0 - 1, G.ZB1, 40.4), Cz(e, 0, G.ZB1 - 1, G.ZB1 + 2, 35.0))
    return p


def lager6203(e, k):
    p = Part('lager6203_%d' % k, 'Lager 6203 %s (Dummy)' % ('hinten' if k == 0 else 'vorn'))
    z0 = G.ZB0 + k * G.BW
    p.add(Cz(e, 0, z0, z0 + G.BW, 40.0))
    p.cut(Cz(e, 0, z0 - 1, z0 + G.BW + 1, 17.0))
    return p
