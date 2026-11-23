# Quetschventil v3: Getriebeplatte. Trägt nur noch den Motor (kein Wellenlager mehr), 4 Pfosten an die Rückwand.
# Druck: Plattenrückseite unten, Pfosten nach oben.
from ihht.geo import Part
from ihht.qv import *


def bauen():
    p = Part('qvGetriebeplatte', 'QV v3 Getriebeplatte (Motor, 4 Pfosten an die Rückwand)', BEIGE)
    p.add(QB(-45, 45, -A - 22.5, 20, Z_C1, Z_C0))
    for sx in (-1, 1):
        for sy in (-1, 1):
            p.add(QCz(sx * FIX_X, sy * FIX_Y, Z_C0 - 0.01, -PT, 9))
    p.cut(QCz(0, -A, Z_C1 - 1, Z_C0 + 1, 22.4))                          # Motorbund
    for sx in (-1, 1):
        for sy in (-1, 1):
            p.cut(QCz(sx * 15.5, -A + sy * 15.5, Z_C1 - 1, Z_C0 + 1, M3_CLR),
                  QCz(sx * FIX_X, sy * FIX_Y, -PT - 14, -PT + 1, M3_SELF))
    return [p]
