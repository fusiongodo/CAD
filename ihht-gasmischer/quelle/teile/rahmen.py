# Rahmen: Boden, Seitenwaende, LZQ-Montagewand, Schienen fuer Motorbruecke, Kamera, Splitter, Saeulen fuers Regal.
from ihht.geo import *
from ihht.masse import *
from ihht.splitter import schienen, schienen_loecher

def bauen():
    frame = Part('frame6', 'Rahmen v6 (2 mm, 2 NEMA, 250 mm)', BEIGE)
    frame.add(*rrect(-X_OUT, X_OUT, Y_DF, Y_DB, 0, DECK_T, 6.0))
    for sx in (-1, 1):
        for y0, y1, zt in ((Y_DF, Y_DB, 15), (Y_NEMA_FACE - 3, Y_NEMA_FACE + 33, 25 + LIFT), (-25, 30, 135 + LIFT), (38, Y_DB, 101 + LIFT)):
            frame.add(B(sx * (X_IN - 0.05), sx * X_OUT, y0 + (6 if y0 == Y_DF else 0), y1 - (6 if y1 == Y_DB else 0), DECK_T - 0.5, zt))
        frame.add(B(sx * (X_IN - 0.05), sx * X_OUT, Y_DF + 6, Y_DB - 6, DECK_T - 0.5, RIM_H))
        # Steg oben: Rotameter-Sektion <-> Splitter-Sektion
        frame.add(B(sx * (X_IN - 0.05), sx * X_OUT, 28, 40, SHELF_Z0 - 6, SHELF_Z0))
    frame.add(B(-(X_OUT - 6), X_OUT - 6, Y_DF, Y_DF + WALL, DECK_T - 0.5, RIM_H))     # Randsteg vorne
    frame.add(B(-(X_OUT - 6), X_OUT - 6, Y_DB - WALL, Y_DB, DECK_T - 0.5, RIM_H))     # Randsteg hinten
    # LZQ-Montagewand + Rippen
    frame.add(B(-X_IN, X_IN, 0, WALL, DECK_T - 0.5, 135 + LIFT))
    RIB_X = (-(X_FEED + PAIR / 2), 0, X_FEED + PAIR / 2)      # zwischen den LZQ-Muttern (Ø 15)
    for c in RIB_X:
        frame.add(B(c - WALL / 2, c + WALL / 2, WALL - 0.5, 20, DECK_T - 0.5, 130 + LIFT))       # längere, dünne Rippen
        frame.add(B(c - WALL / 2, c + WALL / 2, 19, SHELF_Y0 - 0.4, SHELF_Z0 - 6, SHELF_Z0))       # Steg oben zur Regalkante
    # Schienen Motorbrücke (2 Säulen + Steg je Seite)
    for sx in (-1, 1):
        frame.add(B(sx * (X_IN - 13), sx * (X_IN + 0.5), RAIL_Y0, RAIL_Y1, DECK_T - 0.5, BR_RAIL_TOP))   # Schiene/Schraubdom am Boden
    for (px, py) in SHELF_POSTS:
        frame.add(B(px - math.copysign(3, px), px + math.copysign(5.5, px), py - 4, py + 4, DECK_T - 0.5, SHELF_Z0))   # Säule unverändert X_IN-8 .. X_IN+0.5
    for sx in (-1, 1):
        frame.add(B(sx * CAM_RAIL_X - 4, sx * CAM_RAIL_X + 4, Y_DF + 1, CAM_RAIL_Y1, DECK_T - 0.5, DECK_REF + RAIL_H))
    for xin, xout, zc in SPL_PLATZ.values():
        if zc == Z_PB:
            xa, xb = spl_extent(xin, xout)
            schienen(frame, DECK_REF, xa, xb, DECK_T - 0.5, zc)
    # --- Schnitte Rahmen
    XI = X_OUT - 15.0                       # Rand 15 mm
    DECK_WIN = [
        (-49, -6, Y_DF + 15, Y_NEMA_BACK - 7), (6, 49, Y_DF + 15, Y_NEMA_BACK - 7),            # vorne, zwischen den Kameraschienen
        (-49, -6, Y_NEMA_BACK, Y_NEMA_FACE + 13), (6, 49, Y_NEMA_BACK, Y_NEMA_FACE + 13),      # unter den Motoren
        (-49, -6, Y_NEMA_FACE + 20, -30), (6, 49, Y_NEMA_FACE + 20, -30),                      # unter Zahnrädern und Kupplungen
        (-XI, -6, 20, 33), (6, XI, 20, 33),
    ]
    for (x0, x1, y0, y1) in DECK_WIN:
        frame.cut(B(x0, x1, y0, y1, -1, DECK_T + 0.4))
    for x in XS:
        for z in (Z_PB, Z_PT):
            frame.cut(Cy(x, -1, 4, z, HOLE_WALL))
    for sx in (-1, 1):
        hx = sx * BR_HOLE_X
        frame.cut(Cz(hx, RAIL_HOLE_Y, BR_RAIL_TOP + BR_FOOT_T - SCREW_L - 1.0, BR_RAIL_TOP + 0.5, M3_SELF))   # Sackloch: Schraubenspitze z 2,5, Lochgrund z 1,5
    for (px, py) in SHELF_POSTS:
        frame.cut(Cz(px, py, SHELF_Z0 - (SCREW_L - WALL + 1.5), SHELF_Z0 + 1, M3_SELF))
    for xin, xout, zc in SPL_PLATZ.values():
        if zc == Z_PB:
            xa, xb = spl_extent(xin, xout)
            schienen_loecher(frame, DECK_REF, xa, xb, SPL_HOLE_EXTRA, zc)
    for sx in (-1, 1):
        for yy in CAM_HOLES_Y:
            frame.cut(Cz(sx * CAM_RAIL_X, yy, DECK_REF + RAIL_H - (SCREW_L - 3.0 + 1.5), DECK_REF + RAIL_H + 1, M3_SELF))
    return [frame]
