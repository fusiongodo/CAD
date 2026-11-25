"""Rahmen v2 fuer das Exzenter-Quetschventil: Baukasten. Erzeugt rahmen_v2.fs (Onshape-FeatureScript).

Anlass (22.11.2026): Rahmen v1 (Platte + beide Ambosse in einem Teil) ist ohne Steg unter dem Quetschdruck an der
Amboss-Wurzel gebrochen. v2 hat dieselbe Geometrie fuer Schlauch, Lager, Motor und Getriebe, aber vier Einzelteile:

  Bodenplatte  z -6 .. 0, Pilot, Motor-Lochbild, Befestigung (x ±40, y ±15) wie v1; neu: Anschlagleiste aussen hinter
               jedem Amboss (nimmt die Quetschkraft auf) und je 3 x M3 von hinten in den Amboss (Koepfe versenkt,
               sonst streifen sie am Hohlrad).
  Amboss       2 x dasselbe Teil (oben/unten um 180° gedreht), 10 statt 8 mm dick, liegend gedruckt (Schichten laengs).
  Steg vorn    wie v1, zusaetzlich Anschlagleisten; 4 x M3 x 12 in die Amboss-Stirnseiten.

Koordinaten wie quetschventil_v1.fs: Z = Hauptachse, Schlauchmitte z 15, Ambossflaeche y ±28,5 (e 3,5, Quetschhoehe 5).
Start: python3 gen_rahmen.py   (danach build_rahmen.py fuer Pruefung + 3MF)
"""
import math
import os

import gen_getriebe as G
from gen_getriebe import B, Cz, Part

HERE = os.path.dirname(os.path.abspath(__file__))

E_R, H, R = 3.5, 5.0, 20.0          # Exzentrizitaet, Quetschhoehe, Lagerradius (6203: Ø 40)
YA = R + E_R + H                    # Ambossflaeche 28,5
TA = 10.0                           # Ambossdicke (v1: 8)
YO = YA + TA                        # Amboss-Aussenflaeche 38,5
PLAY = 0.15                         # Luft Amboss / Anschlagleiste
LIP, LIP_H, LIP_X = 3.0, 5.0, 26.0  # Anschlagleiste: Dicke, Hoehe, halbe Laenge (aussen frei fuer die Kabelbinder)
YP = YO + PLAY + LIP                # halbe Hoehe Platte und Steg 41,65
XA, XP, PT = 35.5, 44.5, 6.0        # Amboss halbe Laenge, Platte halbe Breite, Plattendicke
ZC, LA, TS = 15.0, 34.0, 6.0        # Schlauchmitte, Ambosslaenge in Z, Stegdicke
GH = math.pi * (12 + 6) / 4 / 2 + 0.75   # Fuehrungsstege: halber Innenabstand (Schlauch 6/12 flach ≈ 14,1)
X_RIB = R + E_R + 2                 # Fuehrungsstege ab hier (Lagerbahn + 2)
SX, SY = 26.0, YA + TA / 2          # Schrauben Platte/Steg -> Amboss
M3_CLR, M3_SELF, HEAD_D, HEAD_T = 3.4, 2.6, 6.4, 3.2

platte = Part('platte', 'QV-Rahmen v2 Bodenplatte (Anschlagleisten, 6 x M3 von hinten in die Ambosse)')
platte.add(B(-XP, XP, -YP, YP, -PT, 0))
for s in (-1, 1):
    platte.add(B(-LIP_X, LIP_X, s * (YO + PLAY), s * YP, -0.01, LIP_H))
platte.cut(Cz(0, 0, -PT - 1, 1, 22.4))
for sx in (-1, 1):
    for sy in (-1, 1):
        platte.cut(Cz(sx * 15.5, sy * 15.5, -PT - 1, 1, M3_CLR), Cz(sx * 15.5, sy * 15.5, -HEAD_T, 1, HEAD_D),   # Motor direkt (ohne Getriebe)
                   Cz(sx * 40, sy * 15, -PT - 1, 1, M3_CLR))                                                      # Befestigung / Getriebe-Pfosten
for x in (-SX, 0, SX):
    for s in (-1, 1):
        platte.cut(Cz(x, s * SY, -PT - 1, 1, M3_CLR), Cz(x, s * SY, -PT - 1, -PT + HEAD_T, HEAD_D))


def amboss(s, name, label):
    p = Part(name, label)
    p.add(B(-XA, XA, s * YA, s * YO, 0, LA))
    for sx in (-1, 1):
        for sz in (-1, 1):
            za = ZC + sz * GH
            p.add(B(sx * X_RIB, sx * XA, s * (YA + 0.01), s * (YA - 4), za if sz > 0 else za - 2, za + 2 if sz > 0 else za))
    for x in (-SX, 0, SX):
        p.cut(Cz(x, s * SY, -1, 12, M3_SELF))                       # von der Platte
    for sx in (-1, 1):
        p.cut(Cz(sx * SX, s * SY, -1, LA + 1, M3_SELF))             # Eckbohrungen durchgehend (25.11.: M3 x 20 vom Steg passte nicht in 12 tief)
        for sz in (-1, 1):                                          # Kabelbinder 4 x 1,8
            zk = ZC + sz * (GH + 2 + 1.4)
            p.cut(B(sx * (XA - 3.5) - 2, sx * (XA - 3.5) + 2, s * (YA - 1), s * (YO + 1), zk - 0.9, zk + 0.9))
    return p


amboss_o = amboss(1, 'amboss_o', 'QV-Rahmen v2 Amboss oben (10 dick)')
amboss_u = amboss(-1, 'amboss_u', 'QV-Rahmen v2 Amboss unten (gleiches Teil, 180° gedreht)')

steg = Part('steg', 'QV-Rahmen v2 Steg vorn (Anschlagleisten)')
steg.add(B(-XA, XA, -YP, YP, LA, LA + TS))
for s in (-1, 1):
    steg.add(B(-LIP_X, LIP_X, s * (YO + PLAY), s * YP, LA - LIP_H, LA + 0.01))
steg.cut(B(-(XA - 11), XA - 11, -(YA - 0.5), YA - 0.5, LA - 1, LA + TS + 1))
for sx in (-1, 1):
    for s in (-1, 1):
        steg.cut(Cz(sx * SX, s * SY, LA - 1, LA + TS + 1, M3_CLR))

PARTS = [platte, amboss_o, amboss_u, steg]

HEAD = ['Rahmen v2 fuer das Exzenter-Quetschventil als Baukasten: Bodenplatte, 2 x Amboss, Steg vorn. Erzeugt von gen_rahmen.py',
        '(ihht-pi CAD/quetschventil), nicht von Hand aendern: Konstante im Generator aendern und neu erzeugen.',
        'Koordinaten wie quetschventil_v1.fs: Z = Hauptachse, Platte z -6 .. 0, Ambossflaeche y ±28,5, Schlauchmitte z 15.',
        'Ambosse mit je 3 x M3 x 12 von hinten durch die Platte (Koepfe versenkt), Steg mit 4 x M3 x 12 von vorn.',
        'Die Anschlagleisten an Platte und Steg nehmen die Quetschkraft auf; die Schrauben halten nur zusammen.',
        'Ohne Steg nicht fahren: dann haengen die Ambosse nur an der Platte.']

if __name__ == '__main__':
    open(os.path.join(HERE, 'rahmen_v2.fs'), 'w').write(G.emit(PARTS, 'Quetschventil Rahmen v2', 'quetschventilRahmenV2', HEAD))
    print('ok', [p.name for p in PARTS], 'Ambossabstand', 2 * YA, 'Platte', 2 * XP, 'x', round(2 * YP, 2))
