# Quetschventil v3: Rückwand (ersetzt die Bodenplatte v2). 8 dick, 608-Sitz mit Lippe vorn (Lager von hinten
# eindrücken), Anschlagleisten und Amboss-Schrauben wie v2. Druck: Rückseite unten, Leisten nach oben.
from ihht.geo import Part
from ihht.qv import *


def bauen():
    w = Part('qvRueckwand', 'QV v3 Rückwand (608-Sitz, Anschlagleisten, 6 x M3 in die Ambosse)', BEIGE)
    w.add(QB(-XP, XP, -YP, YP, -PT, 0))
    for s in (-1, 1):
        w.add(QB(-LIP_X, LIP_X, s * (YO + PLAY), s * YP, -0.01, LIP_H))
    w.cut(*seat_cut(-PT, Z_R608[1], 0.0, -PT - 1))
    for sx in (-1, 1):
        for sy in (-1, 1):
            w.cut(QCz(sx * FIX_X, sy * FIX_Y, -PT - 1, 1, M3_CLR), QCz(sx * FIX_X, sy * FIX_Y, -2.0, 1, HEAD_D))   # Getriebeplatte, Kopf vorn versenkt
    for x in (-SX, 0, SX):
        for s in (-1, 1):
            w.cut(QCz(x, s * SY, -PT - 1, 1, M3_CLR), QCz(x, s * SY, -PT - 1, -PT + HEAD_T + 2.0, HEAD_D))  # Ambosse, Kopf hinten versenkt
    w.add(*seat_ribs(-PT + 0.6, Z_R608[1]))
    return [w]
