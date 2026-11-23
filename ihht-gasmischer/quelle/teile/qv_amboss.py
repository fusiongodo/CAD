# Quetschventil v3: Ambosse = Rahmen v2 unverändert (2 x dasselbe Teil, 10 dick). Nur zur Vollständigkeit im Modul;
# die gedruckten v2-Ambosse passen weiter. Druck: liegend, Außenfläche aufs Bett.
from ihht.geo import Part
from ihht.qv import *


def amboss(s, name, label):
    p = Part(name, label, BEIGE)
    p.add(QB(-XA, XA, s * YA, s * YO, 0, LA))
    for sx in (-1, 1):
        for sz in (-1, 1):
            za = ZC + sz * GH
            p.add(QB(sx * X_RIB, sx * XA, s * (YA + 0.01), s * (YA - 4), za if sz > 0 else za - 2, za + 2 if sz > 0 else za))
    for x in (-SX, 0, SX):
        p.cut(QCz(x, s * SY, -1, 12, M3_SELF))                           # von der Rückwand
    for sx in (-1, 1):
        p.cut(QCz(sx * SX, s * SY, LA - 12, LA + 1, M3_SELF))            # vom Steg
        for sz in (-1, 1):                                               # Kabelbinder 4 x 1,8
            zk = ZC + sz * (GH + 2 + 1.4)
            p.cut(QB(sx * (XA - 3.5) - 2, sx * (XA - 3.5) + 2, s * (YA - 1), s * (YO + 1), zk - 0.9, zk + 0.9))
    return p


def bauen():
    return [amboss(1, 'qvAmbossO', 'QV Amboss oben (= Rahmen v2)'), amboss(-1, 'qvAmbossU', 'QV Amboss unten (= Rahmen v2, gleiches Teil)')]
