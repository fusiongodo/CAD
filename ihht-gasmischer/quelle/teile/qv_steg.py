# Quetschventil v3: Steg vorn (zweite Wand). 8 dick, 608-Sitz mit Lippe innen (Lager von vorn eindrücken),
# Mittelsteg trägt das Lager nach oben/unten in die Ambosse, Fenster links/rechts. Druck: Vorderseite unten.
from ihht.geo import Part
from ihht.qv import *


def bauen():
    s = Part('qvSteg', 'QV v3 Steg vorn (608-Sitz, Anschlagleisten)', BEIGE)
    z1 = LA + TS
    s.add(QB(-XA, XA, -YP, YP, LA, z1))
    for sg in (-1, 1):
        s.add(QB(-LIP_X, LIP_X, sg * (YO + PLAY), sg * YP, LA - LIP_H, LA + 0.01))
    for sx in (-1, 1):
        s.cut(QB(sx * 15.5, sx * (XA - 11), -(YA - 0.5), YA - 0.5, LA - 1, z1 + 1))        # Fenster
    s.cut(*seat_cut(z1, Z_F608[0] - 0.2, LA, z1 + 1))
    for sx in (-1, 1):
        for sg in (-1, 1):
            s.cut(QCz(sx * SX, sg * SY, LA - 1, z1 + 1, M3_CLR), QCz(sx * SX, sg * SY, z1 - 2.0, z1 + 1, HEAD_D))
    s.add(*seat_ribs(Z_F608[0] - 0.2, z1 - 0.6))
    return [s]
