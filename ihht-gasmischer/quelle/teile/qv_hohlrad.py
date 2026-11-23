# Quetschventil v3: Hohlrad 65 Zähne innen, Modul 1. Sechskant-Tasche für den Kopf der M8 (Drehmoment geht in den
# Stahlkern). Druck: Scheibe (vorn) unten, ohne Stützen.
import math
from ihht.geo import Part
from ihht.qv import *


def bauen():
    h = Part('qvHohlrad', 'QV v3 Hohlrad 65 Zähne innen (Sechskant für M8-Kopf)', ORANGE)
    h.add(QCz(0, 0, Z_RING1, Z_DISK0, 2 * RIM_R))
    h.cut(QP(0, 0, Z_RING1 - 0.1, Z_DISK1, gear_profile(ZR, RR_ROOT, RR_TIP, math.pi * MOD / 2 + BACKLASH / 2, -math.pi / 2)))
    h.add(QCz(0, 0, Z_RING1, Z_DISK1 + 0.1, HUB_D))
    h.cut(QP(0, 0, Z_RING1 - 0.1, Z_HEAD, hex_profile(NUT_AF)),          # Kopf DIN 933 M8 (SW 13)
          QCz(0, 0, Z_RING1, Z_DISK0 + 1, ROD_D),
          QK(0, 0, Z_DISK0 + 0.01, Z_DISK0 - 0.6, ROD_D / 2 + 0.6, ROD_D / 2))    # Fase am Bett
    return [h]
