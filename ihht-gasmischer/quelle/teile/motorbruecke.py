# Motorbruecke: verschiebbare Platte mit 2 NEMA-Lochbildern und 2 Lagerzapfen.
from ihht.geo import *
from ihht.masse import *

def bauen():
    bridge = Part('bridge6', 'Motorbrücke v6 (2 mm, 2 NEMA + 2 Lagerzapfen 16 mm)', BEIGE)
    bridge.add(B(-(X_IN - 0.3), X_IN - 0.3, BR_Y0, BR_Y1, BR_Z0, Z_V + 19))   # Oberkante niedrig: freie Sicht auf Kugelbereich
    for sx in (-1, 1):
        bridge.add(B(sx * (X_IN - 0.3 - BR_FOOT_W), sx * (X_IN - 0.3), BR_Y0, RAIL_HOLE_Y + 16, BR_Z0, BR_Z0 + BR_FOOT_T))      # Fuß wie v3
        bridge.add(B(sx * (X_IN - 2.8), sx * (X_IN - 0.3), BR_Y0, RAIL_HOLE_Y + 16, BR_Z0, BR_Z0 + BR_FOOT_T + 12))            # Außenwange (L-Profil)
    for x in XS_MOTOR:
        for sx in (-1, 1):
            for sz in (-1, 1):
                bridge.add(Cy(x + sx * NEMA_HOLE_PITCH / 2, BR_Y1 - 0.1, BR_Y1 + BOSS_H, Z_V + sz * NEMA_HOLE_PITCH / 2, BOSS_D))
    for xr in (-(X_IN - 1.3), -52.0, 0.0, 52.0, X_IN - 1.3):                    # senkrechte Rippen (frei von Schraubköpfen, Zapfen und Zahnrädern)
        bridge.add(B(xr - WALL / 2, xr + WALL / 2, BR_Y1 - 0.1, BR_Y0 + 4.0, BR_Z0, Z_V + 19))   # Rippen enden wie v5
    for x in XS_GEAR:
        bridge.add(Cy(x, BR_Y1 - 0.1, PIN_Y1, Z_V, PIN_D),
                   K(x, Z_V, BR_Y1 - 0.01, BR_Y1 + 1.5, PIN_D / 2 + 1.5, PIN_D / 2),      # Fuß-Fase am Zapfen
                   B(x - WALL / 2, x + WALL / 2, BR_Y1 - 0.1, BR_Y1 + 1.5, BR_Z0, Z_V + 19))        # flache Rippe durch den Zapfenfuß (steift die 1-mm-Platte)
    for x in XS_MOTOR:
        bridge.cut(Cy(x, BR_Y0 - 1, BR_Y1 + 1, Z_V, 24.0))
        for sx in (-1, 1):
            for sz in (-1, 1):
                hx, hz = x + sx * NEMA_HOLE_PITCH / 2, Z_V + sz * NEMA_HOLE_PITCH / 2
                bridge.cut(Cy(hx, BR_Y0 - 1, BR_Y1 + BOSS_H + 1, hz, M3_CLR))
    for sx in (-1, 1):
        bridge.cut(*slotY(sx * BR_HOLE_X, RAIL_HOLE_Y - 10, RAIL_HOLE_Y + 10, BR_Z0 - 1, BR_Z0 + BR_FOOT_T + 1))
    return [bridge]
