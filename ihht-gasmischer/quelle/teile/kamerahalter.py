# Kamerahalter: Platte mit vier Haelsen fuer die Pi-Cam, in Z am Portal verstellbar.
from ihht.geo import *
from ihht.masse import *

def bauen():
    mount = Part('camMount6', 'Kamerahalter v6 (nur Kamera, 2 mm, M2 durch + Mutter)', ANTH)
    PLF = PT_Y1 + MNT_T
    HALS_D, M2_DURCH = 10.0, 3.0                       # Hals Ø10; Loch nominal 3,0 (gedruckt ≈ 2,2–2,4)
    mount.add(B(-25, 25, PT_Y1, PLF, Z_CAM - 12.5, Z_CAM + 12.5))
    for sx in (-1, 1):
        for sz in (-1, 1):
            hx, hz = sx * CAM_HW / 2, Z_CAM + sz * CAM_HL / 2
            mount.add(Cy(hx, PLF - 0.5, BOARD_Y - 1.0, hz, HALS_D),
                      K(hx, hz, PLF - 0.01, PLF + 0.8, HALS_D / 2 + 0.8, HALS_D / 2))     # Fase am Fuß
    for sx in (-1, 1):
        mount.cut(Cy(sx * SLOT_X, PT_Y1 - 1, PLF + 1, Z_CAM, M3_CLR))
        for sz in (-1, 1):
            mount.cut(Cy(sx * CAM_HW / 2, PT_Y1 - 1, BOARD_Y + 1, Z_CAM + sz * CAM_HL / 2, M2_DURCH))   # komplett durch
    return [mount]
