# Quetschventil v3: Ritzel 13 Zähne auf der NEMA-Welle (wie Getriebe v1). NEMA-Konsens: Ø 5,50 / 2,25 + genau 2 Stege
# 0,45 / 0,2. Druck: Nabe unten, ohne Stützen.
import math
from ihht.geo import Part
from ihht.qv import *


def bauen():
    r = Part('qvRitzel', 'QV v3 Ritzel 13 Zähne (NEMA-Welle, 2 Quetschstege)', ORANGE)
    r.add(QCz(0, -A, Z_PH0, Z_PH1 + 0.01, 2 * RP_TIP + 0.6),
          QP(0, -A, Z_PH1, Z_PG1, gear_profile(ZP, RP_TIP, RP_ROOT, math.pi * MOD / 2 - BACKLASH / 2, -math.pi / 2)))
    r.cut(QP(0, -A, Z_D0 - 0.01, Z_PG1 + 1, d_profile(NEMA_D, NEMA_FLAT)),
          QCz(0, -A, Z_PH0 - 1, Z_D0, NEMA_D + 0.3),                     # runder Einlauf (Welle ohne Abflachung)
          QK(0, -A, Z_PH0 - 0.01, Z_PH0 + 0.6, NEMA_D / 2 + 0.75, NEMA_D / 2 + 0.15))
    z_tip = Z_C1 + SHAFT_TIP
    rib_top = Z_PH0 + math.floor((z_tip - 0.7 - Z_PH0) / 0.2) * 0.2
    for zr in (Z_D0 + 1.0, rib_top):                                     # genau 2 Stege, je eine Schicht
        r.add(QB(-NEMA_D / 2 - 0.3, -(NEMA_FLAT - RIB_T), -A - NEMA_D / 2 - 0.3, -A + NEMA_D / 2 + 0.3, round(zr, 3), round(zr + RIB_H, 3)))
    return [r]
