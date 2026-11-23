# Quetschventil v3: Klemmhülse. Drückt die beiden 6905-Innenringe gegen die Wellenschulter, nimmt die Welle über die
# exzentrische Nase mit, hält die M8-Mutter gefangen (seitlich einschieben) und stützt vorn den Innenring des 608.
# Druck: Scheibe (hinten) unten, ohne Stützen.
from ihht.geo import Part
from ihht.qv import *


def bauen():
    k = Part('qvKlemmhuelse', 'QV v3 Klemmhülse (6905 verspannen, M8-Mutter gefangen)', ORANGE)
    pr = PIN_D / 2
    k.add(QCz(E, 0, ZB1, ZB1 + 2.0, SHOULDER_D),                         # Scheibe: nur Innenring 6905
          QCz(0, 0, ZB1, Z_KHUB1 + 0.01, 21.0),                          # Nabe um die Mutter
          QCz(0, 0, Z_KHUB1, Z_F608[0], COLLAR_D))                       # Bund: nur Innenring des vorderen 608
    r_at = lambda z: pr - (z - Z_PIN1) + 0.35                            # Nase der Welle + 0,35 Luft
    k.cut(QK(E, 0, ZB1 - 0.01, Z_KPOCK1, r_at(ZB1 - 0.01), r_at(Z_KPOCK1)),   # Tasche für die Nase
          QP(0, 0, Z_KNUT[0], Z_KNUT[1], hex_profile(NUT_AF)),           # Mutter M8
          QB(-15, 0, -NUT_AF / 2, NUT_AF / 2, Z_KNUT[0], Z_KNUT[1]),     # Einschub von -x
          QCz(0, 0, ZB1 - 1, Z_F608[0] + 1, ROD_D))
    return [k]
