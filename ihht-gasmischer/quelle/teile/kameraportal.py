# Kameraportal: 2 Beine + Quertraeger, in Y auf den Kameraschienen verstellbar.
from ihht.geo import *
from ihht.masse import *

def bauen():
    portal = Part('portal6', 'Kameraportal v6 (Y-verstellbar, Halter ±12 mm in Z)', BEIGE)
    portal.add(B(-LEG_X1, LEG_X1, PT_Y0, PT_Y1, BEAM_Z0, BEAM_Z1))
    for sx in (-1, 1):
        portal.add(B(sx * LEG_X0, sx * LEG_X1, PT_Y0, PT_Y1, FOOT_Z0, BEAM_Z0 + 0.1),                       # Bein
                   B(sx * LEG_X0, sx * LEG_X1, PT_Y0, PT_Y0 + FOOT_L, FOOT_Z0, FOOT_Z0 + 3))               # Fuß
        for gx in (LEG_X0, LEG_X1 - 1.5):                                                                   # Knotenbleche beidseits des Langlochs
            portal.add(('prismx', (PT_Y1 - 0.1, FOOT_Z0 + 2.9), ((0, 0), (FOOT_L - PT_T, 0), (0, 30.0)), min(sx * gx, sx * (gx + 1.5)), max(sx * gx, sx * (gx + 1.5))))
    portal.cut(B(-15.5, 15.5, PT_Y0 - 1, PT_Y1 + 1, Z_CAM - CAM_DZ - 10, Z_CAM + CAM_DZ + 10))             # Fenster für die M2-Muttern hinter dem Halter
    for sx in (-1, 1):
        portal.cut(B(sx * 29, sx * 45, PT_Y0 - 1, PT_Y1 + 1, BEAM_Z0 + 6, BEAM_Z1 - 6))                     # Sparfenster
        portal.cut(*slotZ(sx * SLOT_X, PT_Y0 - 1, PT_Y1 + 1, Z_CAM - CAM_DZ, Z_CAM + CAM_DZ))
        portal.cut(*slotY(sx * CAM_RAIL_X, PT_Y1 + 4, PT_Y0 + FOOT_L - 4, FOOT_Z0 - 1, FOOT_Z0 + 4))
    return [portal]
