# v2 Entwicklungs-Setup + Ablage (v1-Teile). Nutzt Part-Modell + Emitter-Bausteine aus gen.py
import sys, math, json, copy
import gen
from gen import B, C, Cy, Cz, Cx, rrect, barbY, emit_op, f

V1 = list(gen.parts)
gen.parts.clear()
Part = gen.Part

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

O2_BARB = (7.0, 5.0)                         # innen: Standard-O2-Schlauch (Kern-Ø, Bohrung)
OUTER = {'lo': (8.0, 5.0), 'hi': (7.0, 5.0), 'mix': (8.0, 5.0), 'vent': (8.0, 5.0)}

WALL, DECK_T = 1.0, 1.0
DECK_REF = 2.0      # Bezugshöhe für Schienen/Splitter/Kamerafuß (bleibt wie v2.8)
RIM_H = 10.0        # umlaufender Randsteg (Flächenträgheitsmoment)
PITCH = NEMA_W + 3.0
XS = [-1.5 * PITCH, -0.5 * PITCH, 0.5 * PITCH, 1.5 * PITCH]
X_IN = 93.15
X_OUT = X_IN + WALL
Y_DF, Y_DB = -148.0, 95.0

Z0 = 13.0                              # +3 mm, damit die NEMAs (unten) über dem Boden liegen
Z_PB, Z_PT = Z0 + LZQ_PORT_FROM_END, Z0 + LZQ_H - LZQ_PORT_FROM_END   # 22, 118
Z_V = Z0 + LZQ_VALVE_Z                                                # 121
Y_STEM_TIP = -LZQ_DEPTH - LZQ_STEM_TIP                                # -42
Y_NEMA_TIP = Y_STEM_TIP - 2.0                                         # -44
Y_NEMA_FACE = Y_NEMA_TIP - NEMA_SHAFT_L                               # -63
Y_NEMA_BACK = Y_NEMA_FACE - NEMA_H                                    # -86

HOLE_WALL = 11.0
BALL_Z0, BALL_Z1 = Z_PB - HOLE_WALL / 2, Z_PT + HOLE_WALL / 2         # Kugelbereich
Z_CAM = 84.0                                  # über der NEMA-Oberkante hinweg bis zum unteren Kugelbereich
Y_LENS = -LZQ_DEPTH - 90.0                                            # -112

M3_CLR, M3_PILOT, M3_SELF, M3_NUT_W, M3_NUT_T = 3.4, 3.2, 2.6, 5.9, 2.8
M2_PILOT = 1.8

BEIGE = (0.87, 0.81, 0.69, 1.0)
ANTH = (0.22, 0.22, 0.24, 1.0)
ORANGE = (0.93, 0.55, 0.2, 1.0)

def slotY(x, y0, y1, z0, z1, w=M3_CLR):
    return [B(x - w / 2, x + w / 2, y0, y1, z0, z1), Cz(x, y0, z0, z1, w), Cz(x, y1, z0, z1, w)]
def slotZ(x, y0, y1, z0, z1, w=M3_CLR):
    return [B(x - w / 2, x + w / 2, y0, y1, z0, z1), Cy(x, y0, y1, z0, w), Cy(x, y0, y1, z1, w)]

RAIL_H = 5.0
SHELF_TOP = 103.0
SHELF_Z0 = SHELF_TOP - WALL
# ================= RAHMEN =================
frame = Part('frame2', 'Rahmen (Entwicklung)', BEIGE)
frame.add(*rrect(-X_OUT, X_OUT, Y_DF, Y_DB, 0, DECK_T, 6.0))
for sx in (-1, 1):
    for y0, y1, zt in ((Y_DF, Y_DB, 15), (-66, -30, 25), (-25, 30, 135), (38, Y_DB, 101)):
        frame.add(B(sx * (X_IN - 0.05), sx * X_OUT, y0 + (6 if y0 == Y_DF else 0), y1 - (6 if y1 == Y_DB else 0), DECK_T - 0.5, zt))
    frame.add(B(sx * (X_IN - 0.05), sx * X_OUT, Y_DF + 6, Y_DB - 6, DECK_T - 0.5, RIM_H))
    # Steg oben: Rotameter-Sektion <-> Splitter-Sektion
    frame.add(B(sx * (X_IN - 0.05), sx * X_OUT, 28, 40, SHELF_Z0 - 6, SHELF_Z0))
frame.add(B(-(X_OUT - 6), X_OUT - 6, Y_DF, Y_DF + WALL, DECK_T - 0.5, RIM_H))     # Randsteg vorne
frame.add(B(-(X_OUT - 6), X_OUT - 6, Y_DB - WALL, Y_DB, DECK_T - 0.5, RIM_H))     # Randsteg hinten
# LZQ-Montagewand + Rippen
frame.add(B(-X_IN, X_IN, 0, WALL, DECK_T - 0.5, 135))
for c in (-PITCH, 0, PITCH):
    frame.add(B(c - 0.5, c + 0.5, WALL - 0.5, 20, DECK_T - 0.5, 130))       # längere, dünne Rippen
    frame.add(B(c - 0.5, c + 0.5, 19, 40.6, SHELF_Z0 - 6, SHELF_Z0))       # Steg oben zur Regalkante
# Schienen Motorbrücke (2 Säulen + Steg je Seite)
RAIL_Y0, RAIL_Y1, RAIL_HOLE_Y = -62.0, -34.0, -49.0
for sx in (-1, 1):
    frame.add(B(sx * (X_IN - 8), sx * (X_IN + 0.5), RAIL_Y0, RAIL_Y1, DECK_T - 0.5, 4.0))   # flache Schiene am Boden
# Säulen Regal
SHELF_POSTS = [(sx * 89.15, yc) for sx in (-1, 1) for yc in (45.0, 88.0)]
for (px, py) in SHELF_POSTS:
    frame.add(B(px - math.copysign(4, px), px + math.copysign(4.5, px), py - 4, py + 4, DECK_T - 0.5, SHELF_Z0))

# Splitter-Definition (für Schienen auf Deck/Regal)
SPL = [
    ('splHi',  'Splitter hi (Eingang hyperoxisch, links)', [XS[0], XS[1]], (XS[0] + XS[1]) / 2, Z_PB, 'hi',  (0.85, 0.35, 0.3, 1.0)),
    ('splLo',  'Splitter lo (Eingang hypoxisch, rechts)',  [XS[2], XS[3]], (XS[2] + XS[3]) / 2, Z_PB, 'lo',  (0.35, 0.55, 0.85, 1.0)),
    ('splMix', 'Sammler Mix (Ausgang)',             [XS[1], XS[2]], 0.0,                 Z_PT, 'mix', (0.6, 0.4, 0.8, 1.0)),
    ('ventL',  'Ablass links (hi)',                 [XS[0]],        XS[0],               Z_PT, 'vent', (0.55, 0.6, 0.65, 1.0)),
    ('ventR',  'Ablass rechts (lo)',                [XS[3]],        XS[3],               Z_PT, 'vent', (0.55, 0.6, 0.65, 1.0)),
]
def spl_extent(xin, xout):
    xa, xb = min(xin + [xout]) - 8, max(xin + [xout]) + 8
    if xb - xa < 26: xa, xb = xout - 13, xout + 13
    return xa, xb
SPL_Y0, SPL_Y1, SPL_CH = 60.0, 72.0, 66.0
def rails_for(p, base_z, xa, xb):
    for hx in (xa - 5, xb + 5):
        p.add(B(hx - 4, hx + 4, SPL_Y0 - 12, SPL_Y1 + 12, base_z - (1.5 if base_z < 10 else 0.5), base_z + RAIL_H))
def rail_holes(p, base_z, xa, xb, extra_depth):
    for hx in (xa - 5, xb + 5):
        p.cut(Cz(hx, SPL_CH, base_z - extra_depth, base_z + RAIL_H + 1, M3_SELF))

# Kamera-Schienen auf Deck
CAM_RAIL_X = 22.5
for sx in (-1, 1):
    frame.add(B(sx * CAM_RAIL_X - 4, sx * CAM_RAIL_X + 4, Y_DF + 1, -89, DECK_T - 0.5, DECK_REF + RAIL_H))
for name, label, xin, xout, zc, kind, col in SPL:
    if zc == Z_PB:
        xa, xb = spl_extent(xin, xout)
        rails_for(frame, DECK_REF, xa, xb)
# --- Schnitte Rahmen
XI = X_OUT - 15.0                       # Rand 15 mm
DECK_WIN = [
    (-XI, -31, Y_DF + 15, -85), (-14, 14, Y_DF + 15, -85), (31, XI, Y_DF + 15, -85),
    (-XI, -6, -75, -30), (6, XI, -75, -30),
    (-XI, -6, 20, 42), (6, XI, 20, 42),
]
for (x0, x1, y0, y1) in DECK_WIN:
    frame.cut(B(x0, x1, y0, y1, -1, DECK_T + 0.4))
for x in XS:
    for z in (Z_PB, Z_PT):
        frame.cut(Cy(x, -1, 4, z, HOLE_WALL))
for sx in (-1, 1):
    hx = sx * (X_IN - 4)
    frame.cut(Cz(hx, RAIL_HOLE_Y, 0.5, 4.5, M3_SELF))
for (px, py) in SHELF_POSTS:
    frame.cut(Cz(px, py, 90, SHELF_Z0 + 1, M3_SELF))
for name, label, xin, xout, zc, kind, col in SPL:
    if zc == Z_PB:
        xa, xb = spl_extent(xin, xout)
        rail_holes(frame, DECK_REF, xa, xb, 2.5)
for sx in (-1, 1):
    for yy in (-141.0, -126.0, -111.0):
        frame.cut(Cz(sx * CAM_RAIL_X, yy, 0.5, DECK_REF + RAIL_H + 1, M3_SELF))

# ================= REGAL (oben, für Mix + Ablässe) =================
shelf = Part('shelf', 'Regal oben', BEIGE)
shelf.add(B(-91, 91, 41, 92, SHELF_Z0, SHELF_TOP))
for (ya, yb) in ((41, 41 + WALL), (92 - WALL, 92)):
    shelf.add(B(-80, 80, ya, yb, SHELF_Z0 - 6, SHELF_Z0 + 0.5))     # Kantenrippen nach unten
for name, label, xin, xout, zc, kind, col in SPL:
    if zc == Z_PT:
        xa, xb = spl_extent(xin, xout)
        rails_for(shelf, SHELF_TOP, xa, xb)
for (px, py) in SHELF_POSTS:
    shelf.cut(Cz(px, py, SHELF_Z0 - 1, SHELF_TOP + 1, M3_CLR))
for (x0, x1) in ((-78.95, -56.95), (-28.65, 28.65), (56.95, 78.95)):
    shelf.cut(B(x0, x1, 48, 85, SHELF_Z0 - 1, SHELF_TOP + 0.4))
for name, label, xin, xout, zc, kind, col in SPL:
    if zc == Z_PT:
        xa, xb = spl_extent(xin, xout)
        rail_holes(shelf, SHELF_TOP, xa, xb, 2.5)

# ================= SPLITTER =================
# Außentüllen nach Handoff: (r_innen, r_tal, r_spitze, ansatz, wellen, plateau, rampe, länge)
TUELLE_PROF = {
    'lo': (10.2, 11.4, 12.2, 3.0, 4, 0.8, 2.5, 16.2),     # CPAP, hypoxisch
    'hi': (2.6, 3.5, 3.75, 2.0, 4, 0.5, 2.0, 12.0),       # O2, hyperoxisch
    'mix': (10.2, 11.4, 12.2, 3.0, 4, 0.8, 2.5, 16.2),    # Mischausgang = gleiche CPAP-Tülle
}
Z_OUT = {'lo': 26.0, 'mix': Z_PT + 7.0}                                     # große Tülle höher, damit sie über dem Flansch liegt
def K(x, z, y0, y1, r0, r1):
    return ('cone', (x, y0, z), (x, y1, z), r0, r1)
def barb_inner(x, z, ytip, ybase, od):
    # Innentülle: Kern od, 3 glatte Wellen +0,15 mm, glatte Fase am Sockel
    r = od / 2
    ops = [Cy(x, ybase, ytip, z, od)]
    for k in range(3):
        a = ytip + 0.3 + k * 2.4
        ops.append(K(x, z, a, a + 1.8, r, r + 0.15))          # Rampe (von der Spitze her)
        ops.append(Cy(x, a + 1.8, a + 2.1, z, od + 0.3))       # kurzes Plateau, dann Stufe
    ops.append(K(x, z, ybase + 0.3, ybase - 2.2, r + 2.0, r))  # Fase 45°
    return ops
def barb_smooth(x, z, ytip, ybase, od):
    r = od / 2
    s = 1 if ytip > ybase else -1
    ops = [Cy(x, ybase, ytip, z, od)]
    for k in range(2):
        a = ytip - s * (0.4 + k * 3.6)
        ops.append(K(x, z, a, a - s * 2.6, r, r + 0.5))
        ops.append(Cy(x, a - s * 2.6, a - s * 3.0, z, od + 1.0))
    return ops
def barb_inner_old(x, z, ytip, ybase, od):
    # Innentülle: Kern od, flache Wellen +0,15 mm radial, gestufte Fase am Sockel
    ops = [Cy(x, ybase, ytip, z, od)]
    for k in range(3):
        start = ytip + 0.6 + k * 2.4
        for j, dr in enumerate([0.05, 0.10, 0.15]):
            a = start + j * 0.6
            ops.append(Cy(x, a, a + 0.6, z, od + 2 * dr))
    for j, extra in enumerate([3.0, 2.0, 1.0]):          # Fase am Übergang zum Block (Treppe)
        ops.append(Cy(x, ybase - 0.5 - j * 0.7, ybase - 0.5 - (j + 1) * 0.7, z, od + extra))
    return ops
def tuelle_ops(x, z, y0, prof, steps=6):
    ri, rt, rs, base, n, pl, ramp, L = prof
    ops = [Cy(x, y0 - 0.5, y0 + L, z, 2 * rt)]
    for w in range(n):
        z0 = y0 + base + w * (pl + ramp)
        ops.append(Cy(x, z0, z0 + pl, z, 2 * rs))           # Stufe + Plateau
        for k in range(steps):                               # Rampe als Treppe
            pass
        ops.append(K(x, z, z0 + pl, z0 + pl + ramp, rs, rt))   # glatte Rampe
    return ops
for name, label, xin, xout, zc, kind, col in SPL:
    m = Part(name, label, col)
    floor = (DECK_REF if zc == Z_PB else SHELF_TOP) + RAIL_H
    xa, xb = spl_extent(xin, xout)
    z_out = Z_OUT.get(kind, zc)
    prof = TUELLE_PROF.get(kind)
    top = max(zc + 7, z_out + (prof[2] if prof else 4) + 2.5)
    m.add(B(xa, xb, SPL_Y0, SPL_Y1, floor, top))
    m.add(B(xa - 10, xb + 10, SPL_Y0 - 8, SPL_Y1 + 8, floor, floor + 3))
    for x in xin:
        m.add(*barb_inner(x, zc, SPL_Y0 - 10, SPL_Y0 + 0.5, O2_BARB[0]))
    od_out, bore_out = OUTER[kind]
    if prof:
        m.add(*tuelle_ops(xout, z_out, SPL_Y1, prof))
        bore_out = 2 * prof[0]
    else:
        m.add(*barb_smooth(xout, zc, SPL_Y1 + 16, SPL_Y1 - 0.5, od_out))
    xs_all = xin + [xout]
    if max(xs_all) - min(xs_all) > 0.1:
        m.cut(Cx(min(xs_all), max(xs_all), SPL_CH, zc, 5.0))
    for x in xin:
        m.cut(Cy(x, SPL_Y0 - 11, SPL_CH + 0.5, zc, O2_BARB[1]))
    if prof and z_out != zc:
        m.cut(Cy(xout, SPL_CH - 2, SPL_Y1 + prof[7] + 1, z_out, bore_out))   # Kammer schneidet den Kanal
    else:
        m.cut(Cy(xout, SPL_CH - 0.5, SPL_Y1 + (prof[7] if prof else 16) + 1, zc, bore_out))
    for hx in (xa - 5, xb + 5):
        m.cut(*slotY(hx, SPL_CH - 10, SPL_CH + 10, floor - 1, floor + 4))

# ================= MOTORBRÜCKE (verschiebbar) =================
bridge = Part('bridge2', 'Motorbrücke (verschiebbar)', BEIGE)
BR_Y0, BR_Y1 = Y_NEMA_FACE, Y_NEMA_FACE + 1.0       # -63 .. -62: Platte 1 mm
BOSS_H, BOSS_D = 2.0, 7.5                            # Distanzringe: Platte + Ring = 3 mm Klemmlänge (Schrauben zu lang)
bridge.add(B(-(X_IN - 0.3), X_IN - 0.3, BR_Y0, BR_Y1, 4, Z_V + 19))   # Oberkante niedrig: freie Sicht auf Kugelbereich
for sx in (-1, 1):
    bridge.add(B(sx * (X_IN - 8.3), sx * (X_IN - 0.3), BR_Y0, -36, 4, 8))
for x in XS:
    for sx in (-1, 1):
        for sz in (-1, 1):
            bridge.add(Cy(x + sx * NEMA_HOLE_PITCH / 2, BR_Y1 - 0.1, BR_Y1 + BOSS_H, Z_V + sz * NEMA_HOLE_PITCH / 2, BOSS_D))
for xr in [XS[0] - PITCH / 2] + [x + PITCH / 2 for x in XS]:                # senkrechte Rippen zwischen den Motoren (frei von den Köpfen)
    xr = max(min(xr, X_IN - 1.3), -(X_IN - 1.3))
    bridge.add(B(xr - 0.5, xr + 0.5, BR_Y1 - 0.1, BR_Y1 + 3.0, 4, Z_V + 19))
for x in XS:
    bridge.cut(Cy(x, BR_Y0 - 1, BR_Y1 + 1, Z_V, 24.0))
    for sx in (-1, 1):
        for sz in (-1, 1):
            hx, hz = x + sx * NEMA_HOLE_PITCH / 2, Z_V + sz * NEMA_HOLE_PITCH / 2
            bridge.cut(Cy(hx, BR_Y0 - 1, BR_Y1 + BOSS_H + 1, hz, M3_CLR))
for sx in (-1, 1):
    bridge.cut(*slotY(sx * (X_IN - 4), RAIL_HOLE_Y - 10, RAIL_HOLE_Y + 10, 3, 9))

# ================= STECKKUPPLUNGEN =================
CP_Y0, CP_Y1 = Y_NEMA_TIP - 10.0, Y_STEM_TIP + 8.0      # -54 .. -34
for i, x in enumerate(XS):
    p = Part('plug%d' % (i + 1), 'Steckkupplung %d' % (i + 1), ORANGE)
    p.add(Cy(x, CP_Y0, CP_Y1, Z_V, 11.0))
    p.cut(Cy(x, CP_Y0 - 1, Y_NEMA_TIP, Z_V, NEMA_SHAFT_D + 0.15),
          Cy(x, Y_NEMA_TIP - 0.01, CP_Y1 + 1, Z_V, LZQ_STEM_D + 0.15))
    # D-Abflachungen (Keile wieder einsetzen)
    p.add(B(x - 3, x + 3, CP_Y0, Y_NEMA_TIP, Z_V + (NEMA_SHAFT_D / 2 - 0.5) + 0.1, Z_V + 3.5),
          B(x - 3, x + 3, Y_STEM_TIP - 1.5, Y_STEM_TIP + LZQ_STEM_FLAT_L, Z_V + (LZQ_STEM_D / 2 - LZQ_STEM_FLAT) + 0.1, Z_V + 3.5))

# ================= KAMERA =================
# Pi-Cam OV5647: Lochbild 21 x 13 mm (aus deinem Modell), Löcher 2,5 mm -> M2
CAM_HW, CAM_HL = 21.0, 13.0
Z_LED = Z_CAM + 26.0                         # LED-Röhre über der Kamera (unten begrenzen die NEMAs den Lichtkegel)
BOARD_Y = Y_LENS - 5.0                       # Platinen-Vorderseite (Linse ~5 mm hoch)
PL_Y1 = BOARD_Y - 1.0 - 4.0                  # Halterplatte Vorderseite (4 mm Abstandshalter)
PL_Y0 = PL_Y1 - 3.0
CC_Y1, CC_Y0 = PL_Y0, PL_Y0 - 3.0            # Säulenbeine
PL_Z0, PL_Z1 = Z_CAM - 20.0, Z_LED + 12.0
FOOT_Z0 = DECK_REF + RAIL_H
FOOT_Y0, FOOT_Y1 = CC_Y0 - 12, CC_Y1 + 9
camcol = Part('camcol', 'Kamerasäule (Y- und Z-verstellbar)', BEIGE)
camcol.add(B(-27, 27, FOOT_Y0, FOOT_Y1, FOOT_Z0, FOOT_Z0 + 3))
for sx in (-1, 1):
    camcol.add(B(sx * 10, sx * 20, CC_Y0, CC_Y1, FOOT_Z0, PL_Z1 + 4))
    camcol.add(B(sx * 14, sx * 16, FOOT_Y0, CC_Y1, FOOT_Z0, 40))        # Stützrippe
camcol.add(B(-20, 20, CC_Y0, CC_Y1, PL_Z1 + 1, PL_Z1 + 4))
for sx in (-1, 1):
    camcol.cut(*slotZ(sx * 15, CC_Y0 - 1, CC_Y1 + 1, 25, PL_Z1 - 3))
    camcol.cut(*slotY(sx * CAM_RAIL_X, FOOT_Y0 + 3, FOOT_Y0 + 21, FOOT_Z0 - 1, FOOT_Z0 + 4))

mount = Part('camMount', 'Kamerahalter (Pi-Cam + LED-Röhre)', ANTH)
mount.add(B(-20, 20, PL_Y0, PL_Y1, PL_Z0, PL_Z1))
for sx in (-1, 1):
    for sz in (-1, 1):
        mount.add(Cy(sx * CAM_HW / 2, PL_Y1 - 0.5, BOARD_Y - 1.0, Z_CAM + sz * CAM_HL / 2, 5.0))
mount.add(Cy(0, PL_Y1 - 0.5, PL_Y1 + 30, Z_LED, 18.5))
for sx in (-1, 1):
    for zz in (PL_Z0 + 6, PL_Z1 - 6):
        mount.cut(Cy(sx * 15, PL_Y0 - 1, PL_Y1 + 1, zz, M3_CLR))
    for sz in (-1, 1):
        mount.cut(Cy(sx * CAM_HW / 2, PL_Y1 - 2, BOARD_Y, Z_CAM + sz * CAM_HL / 2, M2_PILOT))
mount.cut(Cy(0, PL_Y0 - 1, PL_Y1 + 31, Z_LED, 14.5))

# Dummies
for i, x in enumerate(XS):
    n = Part('nemaD%d' % (i + 1), 'NEMA 17 #%d (Dummy)' % (i + 1), (0.3, 0.3, 0.32, 1.0))
    h = NEMA_W / 2
    n.add(B(x - h, x + h, Y_NEMA_BACK, Y_NEMA_FACE, Z_V - h, Z_V + h),
          Cy(x, Y_NEMA_FACE - 0.1, Y_NEMA_FACE + NEMA_RING_H, Z_V, NEMA_RING_D),
          Cy(x, Y_NEMA_FACE - 0.1, Y_NEMA_TIP, Z_V, NEMA_SHAFT_D))
    l = Part('lzqD%d' % (i + 1), 'LZQ #%d (Dummy)' % (i + 1), (0.7, 0.85, 0.95, 1.0))
    l.add(B(x - LZQ_W / 2, x + LZQ_W / 2, -LZQ_DEPTH, 0, Z0, Z0 + LZQ_H))
    for z in (Z_PB, Z_PT):
        l.add(Cy(x, -0.1, LZQ_THREAD_L, z, LZQ_THREAD_D),
              Cy(x, 2.0, 5.0, z, 15.0),                                   # Mutter hinter der Wand
              Cy(x, LZQ_THREAD_L - 0.1, LZQ_TUELLE_L, z, LZQ_BARB_D0))
        for k in range(3):
            a = LZQ_TUELLE_L - 0.5 - 4.5 * k
            l.add(Cy(x, a - 3.5, a, z, LZQ_BARB_D1))
    l.add(Cy(x, -LZQ_DEPTH - 2.5, 0 - LZQ_DEPTH + 0.1, Z_V, 12.0 / math.cos(math.pi / 6)),
          Cy(x, Y_STEM_TIP, -LZQ_DEPTH - 2.4, Z_V, LZQ_STEM_D))
Part('ledD', 'Schlüsselanhänger-LED (Dummy)', (0.1, 0.1, 0.1, 1.0)).add(Cy(0, PL_Y0 - 25, PL_Y1 + 29.5, Z_LED, 14.0))
Part('camD', 'Pi-Cam OV5647 (Dummy)', (0.1, 0.35, 0.15, 1.0)).add(
    B(-12.5, 12.5, BOARD_Y - 1.0, BOARD_Y, Z_CAM - 12, Z_CAM + 12), B(-4, 4, BOARD_Y - 0.1, Y_LENS, Z_CAM - 4, Z_CAM + 4))

V2 = list(gen.parts)
gen.parts.clear()

# ================= ABLAGE =================
DX_ARC = 450.0
def shift_op(o, dx, dy=0.0):
    if o[0] == 'box':
        _, x0, x1, y0, y1, z0, z1 = o
        return ('box', x0 + dx, x1 + dx, y0 + dy, y1 + dy, z0, z1)
    _, p0, p1, d = o
    return ('cyl', (p0[0] + dx, p0[1] + dy, p0[2]), (p1[0] + dx, p1[1] + dy, p1[2]), d)
ARC = []
for p in V1:
    if p.name.startswith(('lzq', 'nema', 'keycam', 'picam')):
        continue          # veraltete Dummies (durch v2-Dummies ersetzt)
    q = copy.copy(p)
    q.name = 'v1_' + p.name
    q.label = '[v1] ' + p.label
    q.stages = [[k, [shift_op(o, DX_ARC) for o in ops]] for k, ops in p.stages]
    ARC.append(q)
ARC += list(gen.parts)
gen.parts.clear()

# ---------------- Emit ----------------
def emit_any(o, idexpr):
    if o[0] == 'cone':
        _, p0, p1, r0, r1 = o
        return 'coneOp(context, %s, vector(%s, %s, %s) * millimeter, vector(%s, %s, %s) * millimeter, %s, %s);' % (
            idexpr, f(p0[0]), f(p0[1]), f(p0[2]), f(p1[0]), f(p1[1]), f(p1[2]), f(r0), f(r1))
    return emit_op(o, idexpr)
def emit_part(L, p, fn):
    L.append('function %s(context is Context, id is Id, prog is box)' % fn)
    L.append('{')
    adds = []
    tq = lambda: 'qUnion([' + ', '.join('qCreatedBy(%s, EntityType.BODY)' % a for a in adds) + '])'
    for si, (kind, ops) in enumerate(p.stages):
        if kind == 'add':
            L.append('    prog[] = "Körper Stufe %d";' % si)
            L.append('    const A%d = id + "%s_a%d";' % (si, p.name, si))
            adds.append('A%d' % si)
            for j, o in enumerate(ops):
                L.append('    ' + emit_any(o, 'A%d + "s%d"' % (si, j)))
            L.append('    prog[] = "Vereinigen Stufe %d";' % si)
            L.append('    unionAll(context, id + "%s_u%d", %s);' % (p.name, si, tq()))
        else:
            L.append('    prog[] = "Schnittkörper Stufe %d";' % si)
            L.append('    const C%d = id + "%s_c%d";' % (si, p.name, si))
            for j, o in enumerate(ops):
                L.append('    ' + emit_any(o, 'C%d + "o%d"' % (si, j)))
            L.append('    prog[] = "Abziehen Stufe %d";' % si)
            L.append('    cutAll(context, id + "%s_x%d", %s, qCreatedBy(C%d, EntityType.BODY));' % (p.name, si, tq(), si))
    c = p.col
    L.append('    prog[] = "Name/Farbe";')
    L.append('    finishPart(context, %s, "%s", color(%s, %s, %s));' % (tq(), p.label, f(c[0]), f(c[1]), f(c[2])))
    L.append('}')
    L.append('')

def emit_feature(L, plist, fnprefix, title, var):
    names = []
    for p in plist:
        fn = fnprefix + p.name
        emit_part(L, p, fn)
        names.append((fn, p))
    L.append('annotation { "Feature Type Name" : "%s" }' % title)
    L.append('export const %s = defineFeature(function(context is Context, id is Id, definition is map)' % var)
    L.append('    precondition')
    L.append('    {')
    L.append('    }')
    L.append('    {')
    L.append('        const prog = new box("");')
    L.append('        var failed = [];')
    L.append('        var okCount = 0;')
    for fn, p in names:
        L.append('        try')
        L.append('        {')
        L.append('            %s(context, id, prog);' % fn)
        L.append('            okCount += 1;')
        L.append('        }')
        L.append('        catch (error)')
        L.append('        {')
        L.append('            failed = append(failed, "%s @ " ~ prog[] ~ " -> " ~ toString(error));' % p.name)
        L.append('        }')
    L.append('        if (size(failed) > 0)')
    L.append('        {')
    L.append('            var msg = "OK: " ~ toString(okCount) ~ " von %d Teilen. Fehler: ";' % len(names))
    L.append('            for (var s in failed)')
    L.append('                msg = msg ~ s ~ " || ";')
    L.append('            reportFeatureWarning(context, id, msg);')
    L.append('        }')
    L.append('    });')
    L.append('')

L = []
L += ['FeatureScript 3112;', 'import(path : "onshape/std/geometry.fs", version : "3112.0");', '',
      '// IHHT-Gasmischer – v2 Entwicklungs-Setup + Ablage (generiert). Änderungen bitte per Chat.',
      '// Koordinaten: X = links/rechts, Y = vorne(-)/hinten(+), Z = oben. Y=0: Vorderseite LZQ-Montagewand, Z=0: Unterseite Boden.', '']
L += ['function unionAll(context is Context, opId is Id, q is Query)', '{',
      '    const bodies = qBodyType(q, BodyType.SOLID);',
      '    if (size(evaluateQuery(context, bodies)) > 1)',
      '        opBoolean(context, opId, { "tools" : bodies, "operationType" : BooleanOperationType.UNION });', '}', '',
      'function cutAll(context is Context, opId is Id, targets is Query, tools is Query)', '{',
      '    if (size(evaluateQuery(context, tools)) > 0)',
      '        opBoolean(context, opId, { "tools" : tools, "targets" : qBodyType(targets, BodyType.SOLID), "operationType" : BooleanOperationType.SUBTRACTION });', '}', '',
      'function coneOp(context is Context, id is Id, p0 is Vector, p1 is Vector, r0 is number, r1 is number)', '{',
      '    // Kegelstumpf: Profil skizzieren und um die Achse rotieren; Fallback: gestufte Zylinder',
      '    const axisDir = normalize(p1 - p0);',
      '    const len = norm(p1 - p0) / millimeter;',
      '    var refDir = vector(0, 0, 1);',
      '    if (abs(dot(axisDir, refDir)) > 0.9)',
      '        refDir = vector(1, 0, 0);',
      '    const xd = normalize(cross(axisDir, refDir));',
      '    try',
      '    {',
      '        const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(p0, cross(axisDir, xd), xd) });',
      '        skLineSegment(sk, "l0", { "start" : vector(0, 0) * millimeter, "end" : vector(r0, 0) * millimeter });',
      '        skLineSegment(sk, "l1", { "start" : vector(r0, 0) * millimeter, "end" : vector(r1, -len) * millimeter });',
      '        skLineSegment(sk, "l2", { "start" : vector(r1, -len) * millimeter, "end" : vector(0, -len) * millimeter });',
      '        skLineSegment(sk, "l3", { "start" : vector(0, -len) * millimeter, "end" : vector(0, 0) * millimeter });',
      '        skSolve(sk);',
      '        opRevolve(context, id + "rev", { "entities" : qSketchRegion(id + "sk"), "axis" : line(p0, axisDir), "angleForward" : 2 * PI * radian });',
      '        opDeleteBodies(context, id + "del", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });',
      '    }',
      '    catch (e)',
      '    {',
      '        try silent',
      '        {',
      '            opDeleteBodies(context, id + "del2", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });',
      '        }',
      '        for (var k = 0; k < 8; k += 1)',
      '        {',
      '            fCylinder(context, id + ("fb" ~ toString(k)), { "bottomCenter" : p0 + (p1 - p0) * (k / 8), "topCenter" : p0 + (p1 - p0) * ((k + 1) / 8), "radius" : (r0 + (r1 - r0) * (k + 0.5) / 8) * millimeter });',
      '        }',
      '    }',
      '}', '',
      'function finishPart(context is Context, q is Query, label is string, c is Color)', '{',
      '    const b = qBodyType(q, BodyType.SOLID);',
      '    setProperty(context, { "entities" : b, "propertyType" : PropertyType.NAME, "value" : label });',
      '    setProperty(context, { "entities" : b, "propertyType" : PropertyType.APPEARANCE, "value" : c });', '}', '']
emit_feature(L, V2, 'dev_', 'IHHT Entwicklung v2', 'ihhtDevV2')
emit_feature(L, ARC, 'arc_', 'IHHT Ablage', 'ihhtAblage')
open(sys.argv[1], 'w').write('\n'.join(L) + '\n')

def dump(pl, grp):
    return [dict(name=p.name, label=p.label, col=p.col, printed=p.printed, stages=p.stages, group=grp) for p in pl]
json.dump(dump(V2, 'dev') + dump(ARC, 'arc'), open(sys.argv[2], 'w'))

half = XS[-1] + LZQ_W / 2
d = -LZQ_DEPTH - Y_LENS
print('Kamera: Abstand %.0f mm, braucht HFOV %.1f°, VFOV %.1f°; Kugelbereich z %.1f..%.1f, Kamera z %.1f' % (
    d, 2 * math.degrees(math.atan(half / d)), 2 * math.degrees(math.atan((BALL_Z1 - BALL_Z0) / 2 / d)), BALL_Z0, BALL_Z1, Z_CAM))
