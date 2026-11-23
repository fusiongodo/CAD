# Alle Lage- und Grundmasse des Aufbaus an EINER Stelle. Koordinaten: X = links/rechts, Y = vorne(-)/hinten(+), Z = oben.
# Y = 0: Vorderseite LZQ-Montagewand, Z = 0: Unterseite Boden. Teile-Dateien importieren von hier, nie umgekehrt.
import math

# ---------------- Maße ----------------
NEMA_W, NEMA_H = 42.3, 23.0
NEMA_SHAFT_D, NEMA_SHAFT_L = 5.0, 19.0
NEMA_RING_D, NEMA_RING_H = 22.0, 2.0
NEMA_HOLE_PITCH = 31.0

LZQ_W, LZQ_DEPTH, LZQ_H = 20.0, 22.0, 120.0
LZQ_PORT_FROM_END = 12.0
LZQ_THREAD_D, LZQ_THREAD_L = 9.5, 11.0
LZQ_BARB_D0, LZQ_BARB_D1, LZQ_TUELLE_L = 7.0, 8.5, 25.0
LZQ_STEM_D, LZQ_STEM_TIP = 4.0, 20.0         # Spitze 20 mm vor Body-Vorderseite
LZQ_STEM_FLAT_L, LZQ_STEM_FLAT = 6.5, 0.5
LZQ_VALVE_Z = 12.0                           # Spindel = Achse der unteren Tülle (Rotameter mit Drehknopf unten)


WALL, DECK_T = 2.0, 2.0     # v6: alle Wände, Boden, Rippen, Brückenplatte, Kamerahalter 2 mm (v5: 1)
LIFT = 10.0         # Anhebung gegenüber v2: Platz für NEMA-Stecker + Kabelbogen unter den Motoren
SCREW_L = 12.0      # Spax 3x12 an allen Chassis-Schraubstellen
DECK_REF = 2.0 + LIFT      # Bezugshöhe für Schienen/Splitter/Kamerafuß (bleibt wie v2.8)
RIM_H = 10.0        # umlaufender Randsteg (Flächenträgheitsmoment)
PITCH = NEMA_W + 3.0          # Abstand der beiden NEMAs = Abstand der beiden Zufuhr-LZQ (wie v3 -> Sammler Mix passt weiter)
PAIR = 22.0                   # Achsabstand Zufuhr-LZQ <-> Ablass-LZQ = Achsabstand der Zahnräder (2 mm Luft zwischen den LZQ-Körpern)
X_FEED = PITCH / 2
XS = [-(X_FEED + PAIR), -X_FEED, X_FEED, X_FEED + PAIR]      # Ablass hi, Zufuhr hi, Zufuhr lo, Ablass lo
XS_MOTOR = [XS[1], XS[2]]     # NEMA direkt auf der Zufuhr
XS_GEAR = [XS[0], XS[3]]      # Ablass läuft über das Zahnrad mit
X_IN = 66.0
X_OUT = X_IN + WALL
Y_DF, Y_DB = -168.0, 82.0     # Rahmen 250 mm lang (P1S-Bett 256): hinten 13 mm kürzer, vorne 10 mm länger

Z0 = 13.0 + LIFT                              # +3 mm, damit die NEMAs (unten) über dem Boden liegen
Z_PB, Z_PT = Z0 + LZQ_PORT_FROM_END, Z0 + LZQ_H - LZQ_PORT_FROM_END   # 22, 118
Z_V = Z0 + LZQ_VALVE_Z                                                # 121
Y_STEM_TIP = -LZQ_DEPTH - LZQ_STEM_TIP                                # -42
NEMA_BACK_OFF = 24.0                                                  # NEMAs weiter weg vom LZQ (Sichtlinie der Kamera)
Y_NEMA_TIP = Y_STEM_TIP - 2.0 - NEMA_BACK_OFF                                         # -44
Y_NEMA_FACE = Y_NEMA_TIP - NEMA_SHAFT_L                               # -63
Y_NEMA_BACK = Y_NEMA_FACE - NEMA_H                                    # -86

HOLE_WALL = 11.0
BALL_Z0, BALL_Z1 = Z_PB - HOLE_WALL / 2, Z_PT + HOLE_WALL / 2         # Kugelbereich
Z_CAM = (Z_PB + Z_PT) / 2                            # 83: Mitte zwischen unterer und oberer Tüllenachse
CAM_DZ = 0.1 * LZQ_H                                 # ±12 mm Höhenverstellung (10 % der LZQ-Höhe)
Y_LENS = -LZQ_DEPTH - 100.0                                           # Nennlage: 100 mm vor der LZQ-Front

M3_CLR, M3_PILOT, M3_SELF, M3_NUT_W, M3_NUT_T = 3.4, 3.2, 2.6, 5.9, 2.8
M2_PILOT = 1.8

BEIGE = (0.87, 0.81, 0.69, 1.0)
ANTH = (0.22, 0.22, 0.24, 1.0)
ORANGE = (0.93, 0.55, 0.2, 1.0)

RAIL_H = 5.0
SHELF_TOP = 103.0 + LIFT
SHELF_Z0 = SHELF_TOP - 1.0   # v6: Regalhöhe fest (wie v5), nicht an WALL gekoppelt
SHELF_Y0, SHELF_Y1 = 27.0, 79.0

# Schienen der Motorbruecke am Rahmen
RAIL_Y0, RAIL_Y1, RAIL_HOLE_Y = Y_NEMA_FACE + 1.0, Y_NEMA_FACE + 31.0, Y_NEMA_FACE + 16.0
BR_RAIL_TOP = 9.0                 # Schienen-Oberkante = Unterkante Brückenfuß
BR_FOOT_T = 5.5                   # Fußdicke (Oberkante 14,5 bleibt unter den NEMA-Schraubdomen)
BR_FOOT_W = 12.6                  # Fußbreite (v2: 8) -> 4,6 mm Material je Seite des Langlochs (v2: 2,3)
BR_HOLE_X = X_IN - 0.3 - BR_FOOT_W / 2
# Saeulen Regal
SHELF_POSTS = [(sx * (X_IN - 5.0), yc) for sx in (-1, 1) for yc in (SHELF_Y0 + 4.0, SHELF_Y1 - 4.0)]   # v6: Loch 1 mm weiter innen -> Fassung im Regal geschlossen

# Splitter-Plaetze: Innen-Achsen (x), Aussen-Achse (x), Hoehe der Kanalachse
SPL_PLATZ = {
    'hi':  ([XS[0], XS[1]], (XS[0] + XS[1]) / 2, Z_PB),      # links unten, Eingang hyperoxisch
    'lo':  ([XS[2], XS[3]], (XS[2] + XS[3]) / 2, Z_PB),      # rechts unten, Eingang hypoxisch
    'mix': ([XS[1], XS[2]], 0.0, Z_PT),                      # oben auf dem Regal, Mischausgang
}
def spl_extent(xin, xout):
    xa, xb = min(xin + [xout]) - 8, max(xin + [xout]) + 8
    if xb - xa < 26: xa, xb = xout - 13, xout + 13
    return xa, xb
GAP_TIP = 10.0                                     # freie Lücke Tüllenspitze LZQ <-> Tüllenspitze Splitter/Sammler
def spy(zc):                                       # Block-Vorderkante, -Hinterkante, Kanalachse je Ebene
    y0 = LZQ_TUELLE_L + GAP_TIP + (12.0 if zc == Z_PB else 10.0)      # unten 47, oben 45
    return y0, y0 + 12.0, y0 + 6.0
SPL_HOLE_EXTRA = SCREW_L - 3.0 + 1.5 - RAIL_H      # Sackloch 10,5 mm ab Schienen-Oberkante (Flansch 3 mm + 1,5 mm Reserve)
Z_OUT = {'lo': 30.0 + LIFT, 'mix': Z_PT + 7.0}   # lo +4 mm (Platz für übergestülpten Schlauch)
KANAL = 8.0          # Innenkanal Splitter (Tropfenprofil)

# Kamera-Schienen auf dem Deck
CAM_RAIL_X = 55.0                                        # Kameraschienen außen neben den NEMAs
CAM_RAIL_Y1 = Y_NEMA_FACE - 6.0                          # -93: endet 6 mm vor der Brückenplatte
CAM_HOLES_Y = [Y_DF + 6.0 + 8.0 * k for k in range(9)]   # -162 .. -98, Raster 8 mm

# Motorbruecke (auch fuer Zahnraeder und Sichtlinien)
BR_Z0 = BR_RAIL_TOP
BR_Y0, BR_Y1 = Y_NEMA_FACE, Y_NEMA_FACE + WALL      # v6: Platte 2 mm
BOSS_H, BOSS_D = 3.0 - WALL, 7.5   # v6: Klemmlänge Platte + Ring bleibt 3 mm                            # Distanzringe: Platte + Ring = 3 mm Klemmlänge (Schrauben zu lang)
PIN_D, PIN_Y1 = 6.0, Y_NEMA_FACE + 17.0              # Lagerzapfen Ø 6 für die Zahnradnabe, 16 mm lang

# Kamera: Pi-Cam OV5647, Lochbild 21 x 13 mm; Portal = flache 3-mm-Platte
CAM_HW, CAM_HL = 21.0, 13.0
PT_T = 3.0                                   # Plattendicke Portal
MNT_T, NECK_L = WALL, 6.0   # v6: Halterplatte 2 mm                     # Halterplatte, Hälse
PT_Y0 = Y_LENS - 5.0 - 1.0 - NECK_L - MNT_T - PT_T      # Rückseite Portal (Linse = PT_Y0 + 16)
PT_Y1 = PT_Y0 + PT_T
BOARD_Y = Y_LENS - 5.0                       # Platinen-Vorderseite (Linse ~5 mm hoch)
FOOT_Z0 = DECK_REF + RAIL_H                  # 17
FOOT_L = 34.0                                # Fuß nach vorn (zum LZQ), Langloch 23 mm
LEG_X0, LEG_X1 = CAM_RAIL_X - 6.0, CAM_RAIL_X + 6.0     # 49 .. 61
BEAM_Z0, BEAM_Z1 = Z_CAM - 25.0, Z_CAM + 25.0           # 58 .. 108 (Unterkante über der NEMA-Oberkante 56,2)
SLOT_X = 20.0                                # Höhen-Langlöcher für den Halter
