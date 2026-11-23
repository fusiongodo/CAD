# Regal oben: traegt den Sammler Mix, sitzt auf den vier Saeulen des Rahmens.
from ihht.geo import *
from ihht.masse import *
from ihht.splitter import schienen, schienen_loecher

def bauen():
    shelf = Part('shelf6', 'Regal oben v6 (2 mm Chassis, verstärkt, Säulen-Fassungen geschlossen)', BEIGE)
    SHELF_X = X_IN - 0.35          # v6: bis 0,3 mm an die Rahmenwand (v5: X_IN - 2,15 -> Säulenloch nur 0,15 mm vom Rand = offen)
    PLATE, RIB_T, RIB_D, DOME_D, RAIL_W = 3.0, 3.0, 8.0, 10.0, 10.0   # wie Regal oben v6 (gen_regal_v6.py)
    SHELF_TOP6 = SHELF_Z0 + PLATE
    shelf.add(B(-SHELF_X, SHELF_X, SHELF_Y0, SHELF_Y1, SHELF_Z0, SHELF_TOP6))
    for (ya, yb) in ((SHELF_Y0, SHELF_Y0 + RIB_T), (SHELF_Y1 - RIB_T, SHELF_Y1)):
        shelf.add(B(-(X_IN - 13), X_IN - 13, ya, yb, SHELF_Z0 - RIB_D, SHELF_Z0 + 0.5))        # Kantenrippen
    for sx in (-1, 1):
        shelf.add(B(sx * 55.0, sx * 57.8, SHELF_Y0, SHELF_Y1, SHELF_Z0 - RIB_D, SHELF_Z0 + 0.5))  # Seitenrippen (innen neben den Säulen)
        shelf.add(B(sx * 53.0, sx * 55.5, SHELF_Y0, SHELF_Y0 + RIB_T, SHELF_Z0 - RIB_D, SHELF_Z0 + 0.5))
        shelf.add(B(sx * 53.0, sx * 55.5, SHELF_Y1 - RIB_T, SHELF_Y1, SHELF_Z0 - RIB_D, SHELF_Z0 + 0.5))
    for xin, xout, zc in SPL_PLATZ.values():
        SPL_Y0, SPL_Y1, SPL_CH = spy(zc)
        if zc == Z_PT:
            xa, xb = spl_extent(xin, xout)
            for hx in (xa - 5, xb + 5):
                shelf.add(B(hx - RAIL_W / 2, hx + RAIL_W / 2, SPL_Y0 - 12, SPL_Y1 + 12, SHELF_TOP6 - 0.5, SHELF_TOP6 + RAIL_H))   # Schiene
                shelf.add(Cz(hx, SPL_CH, SHELF_Z0 - RIB_D, SHELF_Z0 + 0.5, DOME_D))                                      # Schraubdom
    for (px, py) in SHELF_POSTS:
        shelf.cut(Cz(px, py, SHELF_Z0 - 1, SHELF_TOP6 + 1, M3_CLR))
    for (x0, x1) in ((-53.0, -43.65), (-28.65, 28.65), (43.65, 53.0)):
        shelf.cut(B(x0, x1, SHELF_Y0 + 7, SHELF_Y1 - 7, SHELF_Z0 - 1, SHELF_TOP6 + 0.4))
    for xin, xout, zc in SPL_PLATZ.values():
        SPL_Y0, SPL_Y1, SPL_CH = spy(zc)
        if zc == Z_PT:
            xa, xb = spl_extent(xin, xout)
            schienen_loecher(shelf, SHELF_TOP6, xa, xb, SPL_HOLE_EXTRA, zc)
    return [shelf]
