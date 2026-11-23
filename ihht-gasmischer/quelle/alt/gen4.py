# v4 Entwicklungs-Setup: 2 statt 4 NEMA. Je Leitung (hi/lo) ein Ventilpaar: Zufuhr-LZQ direkt am NEMA (Zahnradkupplung),
# Ablass-LZQ 22 mm daneben über ein 1:1-Stirnradpaar gegenläufig mitgedreht (Zahnradnabe auf Lagerzapfen der Motorbrücke).
# Sonst wie v3 (Kamera, Höhen, Schraubstellen). Rahmen 134 statt 188 mm breit.
# Aufruf: python3 gen4.py chassis_v4.fs parts_v4.json
import sys, math, json, copy
import gen
from gen import B, C, Cy, Cz, Cx, rrect, barbY, emit_op, f

gen.parts.clear()        # v1-Teile aus gen.py werden hier nicht gebraucht
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
Y_DF, Y_DB = -158.0, 95.0     # vorne +10 mm (Kamera-Verstellweg); Rahmen 253 mm lang (P1S-Bett 256)

Z0 = 13.0 + LIFT                              # +3 mm, damit die NEMAs (unten) über dem Boden liegen
Z_PB, Z_PT = Z0 + LZQ_PORT_FROM_END, Z0 + LZQ_H - LZQ_PORT_FROM_END   # 22, 118
Z_V = Z0 + LZQ_VALVE_Z                                                # 121
Y_STEM_TIP = -LZQ_DEPTH - LZQ_STEM_TIP                                # -42
Y_NEMA_TIP = Y_STEM_TIP - 2.0                                         # -44
Y_NEMA_FACE = Y_NEMA_TIP - NEMA_SHAFT_L                               # -63
Y_NEMA_BACK = Y_NEMA_FACE - NEMA_H                                    # -86

HOLE_WALL = 11.0
BALL_Z0, BALL_Z1 = Z_PB - HOLE_WALL / 2, Z_PT + HOLE_WALL / 2         # Kugelbereich
Z_CAM = 84.0 + LIFT                                  # über der NEMA-Oberkante hinweg bis zum unteren Kugelbereich
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
SHELF_TOP = 103.0 + LIFT
SHELF_Z0 = SHELF_TOP - WALL
# ================= RAHMEN =================
frame = Part('frame4', 'Rahmen v4 (Entwicklung, 2 NEMA)', BEIGE)
frame.add(*rrect(-X_OUT, X_OUT, Y_DF, Y_DB, 0, DECK_T, 6.0))
for sx in (-1, 1):
    for y0, y1, zt in ((Y_DF, Y_DB, 15), (-66, -30, 25 + LIFT), (-25, 30, 135 + LIFT), (38, Y_DB, 101 + LIFT)):
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
    frame.add(B(c - 0.5, c + 0.5, WALL - 0.5, 20, DECK_T - 0.5, 130 + LIFT))       # längere, dünne Rippen
    frame.add(B(c - 0.5, c + 0.5, 19, 40.6, SHELF_Z0 - 6, SHELF_Z0))       # Steg oben zur Regalkante
# Schienen Motorbrücke (2 Säulen + Steg je Seite)
RAIL_Y0, RAIL_Y1, RAIL_HOLE_Y = -62.0, -32.0, -47.0
BR_RAIL_TOP = 9.0                 # Schienen-Oberkante = Unterkante Brückenfuß
BR_FOOT_T = 5.5                   # Fußdicke (Oberkante 14,5 bleibt unter den NEMA-Schraubdomen)
BR_FOOT_W = 12.6                  # Fußbreite (v2: 8) -> 4,6 mm Material je Seite des Langlochs (v2: 2,3)
BR_HOLE_X = X_IN - 0.3 - BR_FOOT_W / 2
for sx in (-1, 1):
    frame.add(B(sx * (X_IN - 13), sx * (X_IN + 0.5), RAIL_Y0, RAIL_Y1, DECK_T - 0.5, BR_RAIL_TOP))   # Schiene/Schraubdom am Boden
# Säulen Regal
SHELF_POSTS = [(sx * (X_IN - 4.0), yc) for sx in (-1, 1) for yc in (45.0, 88.0)]
for (px, py) in SHELF_POSTS:
    frame.add(B(px - math.copysign(4, px), px + math.copysign(4.5, px), py - 4, py + 4, DECK_T - 0.5, SHELF_Z0))

# Splitter-Definition (für Schienen auf Deck/Regal)
SPL = [
    ('splHi',  'Splitter hi v4 (Eingang hyperoxisch, links, Tüllen 22 mm)', [XS[0], XS[1]], (XS[0] + XS[1]) / 2, Z_PB, 'hi',  (0.85, 0.35, 0.3, 1.0)),
    ('splLo',  'Splitter lo v4 (Eingang hypoxisch, rechts, Tüllen 22 mm)',  [XS[2], XS[3]], (XS[2] + XS[3]) / 2, Z_PB, 'lo',  (0.35, 0.55, 0.85, 1.0)),
    ('splMix', 'Sammler Mix (Ausgang)',             [XS[1], XS[2]], 0.0,                 Z_PT, 'mix', (0.6, 0.4, 0.8, 1.0)),
    # Ablass-Adapter entfallen in v4: bei 22 mm Achsabstand kollidieren sie mit dem Sammler Mix; die obere Tülle des Ablass-LZQ bläst frei ab
]
def spl_extent(xin, xout):
    xa, xb = min(xin + [xout]) - 8, max(xin + [xout]) + 8
    if xb - xa < 26: xa, xb = xout - 13, xout + 13
    return xa, xb
SPL_Y0, SPL_Y1, SPL_CH = 60.0, 72.0, 66.0
def rails_for(p, base_z, xa, xb, bottom):
    for hx in (xa - 5, xb + 5):
        p.add(B(hx - 4, hx + 4, SPL_Y0 - 12, SPL_Y1 + 12, bottom, base_z + RAIL_H))
SPL_HOLE_EXTRA = SCREW_L - 3.0 + 1.5 - RAIL_H      # Sackloch 10,5 mm ab Schienen-Oberkante (Flansch 3 mm + 1,5 mm Reserve)
def rail_holes(p, base_z, xa, xb, extra_depth):
    for hx in (xa - 5, xb + 5):
        p.cut(Cz(hx, SPL_CH, base_z - extra_depth, base_z + RAIL_H + 1, M3_SELF))

# Kamera-Schienen auf Deck
CAM_RAIL_X = 22.5
CAM_HOLES_Y = [-152.0 + 8.0 * k for k in range(7)]      # -152 .. -104, Raster 8 mm
for sx in (-1, 1):
    frame.add(B(sx * CAM_RAIL_X - 4, sx * CAM_RAIL_X + 4, Y_DF + 1, -99, DECK_T - 0.5, DECK_REF + RAIL_H))      # endet 13 mm vor den NEMAs (Brücke bleibt 10 mm nach vorn verschiebbar)
for name, label, xin, xout, zc, kind, col in SPL:
    if zc == Z_PB:
        xa, xb = spl_extent(xin, xout)
        rails_for(frame, DECK_REF, xa, xb, DECK_T - 0.5)
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
    hx = sx * BR_HOLE_X
    frame.cut(Cz(hx, RAIL_HOLE_Y, BR_RAIL_TOP + BR_FOOT_T - SCREW_L - 1.0, BR_RAIL_TOP + 0.5, M3_SELF))   # Sackloch: Schraubenspitze z 2,5, Lochgrund z 1,5
for (px, py) in SHELF_POSTS:
    frame.cut(Cz(px, py, SHELF_Z0 - (SCREW_L - WALL + 1.5), SHELF_Z0 + 1, M3_SELF))
for name, label, xin, xout, zc, kind, col in SPL:
    if zc == Z_PB:
        xa, xb = spl_extent(xin, xout)
        rail_holes(frame, DECK_REF, xa, xb, SPL_HOLE_EXTRA)
for sx in (-1, 1):
    for yy in CAM_HOLES_Y:
        frame.cut(Cz(sx * CAM_RAIL_X, yy, DECK_REF + RAIL_H - (SCREW_L - 3.0 + 1.5), DECK_REF + RAIL_H + 1, M3_SELF))

# ================= REGAL (oben, für Mix + Ablässe) =================
shelf = Part('shelf4', 'Regal oben v4 (nur Sammler Mix)', BEIGE)
SHELF_X = X_IN - 2.15
shelf.add(B(-SHELF_X, SHELF_X, 41, 92, SHELF_Z0, SHELF_TOP))
for (ya, yb) in ((41, 41 + WALL), (92 - WALL, 92)):
    shelf.add(B(-(X_IN - 13), X_IN - 13, ya, yb, SHELF_Z0 - 6, SHELF_Z0 + 0.5))     # Kantenrippen nach unten
for name, label, xin, xout, zc, kind, col in SPL:
    if zc == Z_PT:
        xa, xb = spl_extent(xin, xout)
        rails_for(shelf, SHELF_TOP, xa, xb, SHELF_TOP - 0.5)
        for hx in (xa - 5, xb + 5):
            shelf.add(Cz(hx, SPL_CH, SHELF_Z0 - 6, SHELF_Z0 + 0.5, 8.0))      # Schraubdom nach unten (so tief wie die Kantenrippen)
for (px, py) in SHELF_POSTS:
    shelf.cut(Cz(px, py, SHELF_Z0 - 1, SHELF_TOP + 1, M3_CLR))
for (x0, x1) in ((-55.0, -43.65), (-28.65, 28.65), (43.65, 55.0)):
    shelf.cut(B(x0, x1, 48, 85, SHELF_Z0 - 1, SHELF_TOP + 0.4))
for name, label, xin, xout, zc, kind, col in SPL:
    if zc == Z_PT:
        xa, xb = spl_extent(xin, xout)
        rail_holes(shelf, SHELF_TOP, xa, xb, SPL_HOLE_EXTRA)

# ================= SPLITTER =================
# Außentüllen nach Handoff: (r_innen, r_tal, r_spitze, ansatz, wellen, plateau, rampe, länge)
TUELLE_PROF = {
    'lo': (10.2, 11.4, 12.2, 3.0, 4, 0.8, 2.5, 16.2),     # CPAP, hypoxisch
    'hi': (2.6, 3.5, 3.75, 2.0, 4, 0.5, 2.0, 12.0),       # O2, hyperoxisch
    'mix': (10.2, 11.4, 12.2, 3.0, 4, 0.8, 2.5, 16.2),    # Mischausgang = gleiche CPAP-Tülle
}
Z_OUT = {'lo': 30.0 + LIFT, 'mix': Z_PT + 7.0}   # lo +4 mm (Platz für übergestülpten Schlauch)
TUELLE_AD = 10.75    # Kern-Ø Innentülle; Wellenscheitel = 10,75 + 2*0,25 = 11,25 (Ring-Test: zwischen Nr. 3 und 4)
KANAL = 8.0          # Innenkanal Splitter (Tropfenprofil)
def T(p0, p1, d): return ('tear', tuple(p0), tuple(p1), d)                                     # große Tülle höher, damit sie über dem Flansch liegt
def K(x, z, y0, y1, r0, r1):
    return ('cone', (x, y0, z), (x, y1, z), r0, r1)
def barb_inner(x, z, ytip, ybase, od):
    # Innentülle: Kern od, 3 glatte Wellen +0,15 mm, glatte Fase am Sockel
    r = od / 2
    ops = [Cy(x, ybase, ytip, z, od)]
    for k in range(3):
        a = ytip + 0.3 + k * 2.4
        rd = 0.25 if od > 9 else 0.15
        ops.append(K(x, z, a, a + 1.8, r, r + rd))            # Rampe (von der Spitze her)
        ops.append(Cy(x, a + 1.8, a + 2.1, z, od + 2 * rd))    # kurzes Plateau, dann Stufe
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
    top = max(zc + (8.5 if zc == Z_PB else 7), z_out + (prof[2] if prof else 4) + 2.5)
    m.add(B(xa, xb, SPL_Y0, SPL_Y1, floor, top))
    m.add(B(xa - 10, xb + 10, SPL_Y0 - 8, SPL_Y1 + 8, floor, floor + 3))
    low = (zc == Z_PB)
    for x in xin:
        if low: m.add(*barb_inner(x, zc, SPL_Y0 - 12, SPL_Y0 + 0.5, TUELLE_AD))
        else:   m.add(*barb_inner(x, zc, SPL_Y0 - 10, SPL_Y0 + 0.5, O2_BARB[0]))
    od_out, bore_out = OUTER[kind]
    if prof:
        m.add(*tuelle_ops(xout, z_out, SPL_Y1, prof))
        bore_out = 2 * prof[0]
    else:
        m.add(*barb_smooth(xout, zc, SPL_Y1 + 16, SPL_Y1 - 0.5, od_out))
    xs_all = xin + [xout]
    if max(xs_all) - min(xs_all) > 0.1:
        if low: m.cut(T((min(xs_all) - KANAL / 2 + 0.5, SPL_CH, zc), (max(xs_all) + KANAL / 2 - 0.5, SPL_CH, zc), KANAL))
        else:   m.cut(Cx(min(xs_all), max(xs_all), SPL_CH, zc, 5.0))
    for x in xin:
        if low: m.cut(T((x, SPL_Y0 - 13, zc), (x, SPL_CH, zc), 6.5))       # voller Tropfen passt in die Tülle
        else:   m.cut(Cy(x, SPL_Y0 - 11, SPL_CH + 0.5, zc, O2_BARB[1]))
    if prof and z_out != zc:
        m.cut(Cy(xout, SPL_CH - 2, SPL_Y1 + prof[7] + 1, z_out, bore_out))   # Kammer schneidet den Kanal
    else:
        m.cut(Cy(xout, SPL_CH - 0.5, SPL_Y1 + (prof[7] if prof else 16) + 1, zc, bore_out))
    for hx in (xa - 5, xb + 5):
        m.cut(*slotY(hx, SPL_CH - 10, SPL_CH + 10, floor - 1, floor + 4))

# ================= MOTORBRÜCKE (verschiebbar) =================
bridge = Part('bridge4', 'Motorbrücke v4 (2 NEMA + 2 Lagerzapfen)', BEIGE)
BR_Z0 = BR_RAIL_TOP
BR_Y0, BR_Y1 = Y_NEMA_FACE, Y_NEMA_FACE + 1.0       # -63 .. -62: Platte 1 mm
BOSS_H, BOSS_D = 2.0, 7.5                            # Distanzringe: Platte + Ring = 3 mm Klemmlänge (Schrauben zu lang)
PIN_D, PIN_Y1 = 6.0, Y_NEMA_FACE + 13.0              # Lagerzapfen Ø 6 für die Zahnradnabe, 12 mm lang (bis y -50)
bridge.add(B(-(X_IN - 0.3), X_IN - 0.3, BR_Y0, BR_Y1, BR_Z0, Z_V + 19))   # Oberkante niedrig: freie Sicht auf Kugelbereich
for sx in (-1, 1):
    bridge.add(B(sx * (X_IN - 0.3 - BR_FOOT_W), sx * (X_IN - 0.3), BR_Y0, RAIL_HOLE_Y + 16, BR_Z0, BR_Z0 + BR_FOOT_T))      # Fuß wie v3
    bridge.add(B(sx * (X_IN - 2.8), sx * (X_IN - 0.3), BR_Y0, RAIL_HOLE_Y + 16, BR_Z0, BR_Z0 + BR_FOOT_T + 12))            # Außenwange (L-Profil)
for x in XS_MOTOR:
    for sx in (-1, 1):
        for sz in (-1, 1):
            bridge.add(Cy(x + sx * NEMA_HOLE_PITCH / 2, BR_Y1 - 0.1, BR_Y1 + BOSS_H, Z_V + sz * NEMA_HOLE_PITCH / 2, BOSS_D))
for xr in (-(X_IN - 1.3), -52.0, 0.0, 52.0, X_IN - 1.3):                    # senkrechte Rippen (frei von Schraubköpfen, Zapfen und Zahnrädern)
    bridge.add(B(xr - 0.5, xr + 0.5, BR_Y1 - 0.1, BR_Y1 + 3.0, BR_Z0, Z_V + 19))
for x in XS_GEAR:
    bridge.add(Cy(x, BR_Y1 - 0.1, PIN_Y1, Z_V, PIN_D),
               K(x, Z_V, BR_Y1 - 0.01, BR_Y1 + 1.5, PIN_D / 2 + 1.5, PIN_D / 2),      # Fuß-Fase am Zapfen
               B(x - 0.5, x + 0.5, BR_Y1 - 0.1, BR_Y1 + 1.5, BR_Z0, Z_V + 19))        # flache Rippe durch den Zapfenfuß (steift die 1-mm-Platte)
for x in XS_MOTOR:
    bridge.cut(Cy(x, BR_Y0 - 1, BR_Y1 + 1, Z_V, 24.0))
    for sx in (-1, 1):
        for sz in (-1, 1):
            hx, hz = x + sx * NEMA_HOLE_PITCH / 2, Z_V + sz * NEMA_HOLE_PITCH / 2
            bridge.cut(Cy(hx, BR_Y0 - 1, BR_Y1 + BOSS_H + 1, hz, M3_CLR))
for sx in (-1, 1):
    bridge.cut(*slotY(sx * BR_HOLE_X, RAIL_HOLE_Y - 10, RAIL_HOLE_Y + 10, BR_Z0 - 1, BR_Z0 + BR_FOOT_T + 1))

# ================= ZAHNRADKUPPLUNG (NEMA -> Zufuhr-LZQ) + ZAHNRADNABE (Ablass-LZQ) =================
# Stirnräder 1:1, direkt kämmend -> Ablass dreht gegenläufig. z 16, Modul 22/16 = 1,375, Eingriffswinkel 20°.
GEAR_N = 16
GEAR_M = PAIR / GEAR_N
GEAR_BACKLASH = 0.15                 # Flankenspiel gesamt am Teilkreis (je Rad 0,075 dünner)
GEAR_RA = PAIR / 2 + GEAR_M          # Kopfkreis 12,375
GEAR_Y0, GEAR_Y1 = Y_NEMA_FACE + 5.0, Y_NEMA_TIP      # -58 .. -44: 14 mm breit (Spindeln dürfen ±5 mm gegeneinander wandern)
HUB_D, HUB_Y1 = 11.0, Y_STEM_TIP + 8.0                # Nabe Ø 11 bis y -34
FIT_NEMA_D, FIT_NEMA_FLAT = 5.32, 2.22                # aus den Kupplungs-Testdrucken
FIT_LZQ_D, FIT_LZQ_FLAT = 4.50, 1.06
PIN_BORE = 7.0                       # Nennmaß; gedruckt ca. 6,4–6,7 -> Laufsitz auf dem Zapfen Ø 6
def gear_profile(n, m, phase, backlash, steps=6):
    r = n * m / 2; pa = math.radians(20.0); rb = r * math.cos(pa); ra = r + m; rf = r - 1.25 * m
    inv = lambda a: math.tan(a) - a
    half = (math.pi * m / 2 - backlash / 2) / (2 * r)             # halber Zahnwinkel am Teilkreis
    def psi(rho): return half + inv(pa) - inv(math.acos(min(1.0, rb / rho)))
    r0 = max(rb, rf)
    rad = [r0 + (ra - r0) * k / steps for k in range(steps + 1)]
    pts = []
    for t in range(n):
        c = phase + 2 * math.pi * t / n
        if rf < rb: pts.append((rf, c - psi(rb)))
        pts += [(rho, c - psi(rho)) for rho in rad]              # rechte Flanke hoch
        pts.append((ra, c))                                      # Kopf
        pts += [(rho, c + psi(rho)) for rho in reversed(rad)]    # linke Flanke runter
        if rf < rb: pts.append((rf, c + psi(rb)))
        pts.append((rf, c + math.pi / n))                        # Zahngrund
    return [(round(rho * math.cos(a), 3), round(rho * math.sin(a), 3)) for rho, a in pts]
def P(x, z, y0, y1, pts): return ('prism', (x, z), tuple(pts), y0, y1)
for i, x in enumerate(XS_MOTOR):
    g = Part('gearDrive%d' % (i + 1), 'Zahnradkupplung %s (NEMA -> Zufuhr)' % ('hi', 'lo')[i], ORANGE)
    g.add(P(x, Z_V, GEAR_Y0, GEAR_Y1, gear_profile(GEAR_N, GEAR_M, 0.0, GEAR_BACKLASH)),
          Cy(x, GEAR_Y1 - 0.1, HUB_Y1, Z_V, HUB_D))
    g.cut(Cy(x, GEAR_Y0 - 1, Y_NEMA_TIP, Z_V, FIT_NEMA_D),
          Cy(x, Y_NEMA_TIP - 0.01, HUB_Y1 + 1, Z_V, FIT_LZQ_D),
          K(x, Z_V, GEAR_Y0 - 0.01, GEAR_Y0 + 0.6, FIT_NEMA_D / 2 + 0.6, FIT_NEMA_D / 2),          # Einführfasen
          K(x, Z_V, HUB_Y1 - 0.6, HUB_Y1 + 0.01, FIT_LZQ_D / 2, FIT_LZQ_D / 2 + 0.6))
    g.add(B(x - 3, x + 3, GEAR_Y0, Y_NEMA_TIP, Z_V + FIT_NEMA_FLAT, Z_V + 3.6),                    # D-Abflachungen wie Kupplung v1
          B(x - 2.6, x + 2.6, Y_STEM_TIP - 1.5, Y_STEM_TIP + LZQ_STEM_FLAT_L, Z_V + FIT_LZQ_FLAT, Z_V + 3.4))
for i, x in enumerate(XS_GEAR):
    g = Part('gearIdle%d' % (i + 1), 'Zahnradnabe %s (Ablass, läuft auf Lagerzapfen)' % ('hi', 'lo')[i], (0.95, 0.7, 0.25, 1.0))
    g.add(P(x, Z_V, GEAR_Y0, GEAR_Y1, gear_profile(GEAR_N, GEAR_M, math.pi / GEAR_N, GEAR_BACKLASH)),
          Cy(x, GEAR_Y1 - 0.1, HUB_Y1, Z_V, HUB_D))
    g.cut(Cy(x, GEAR_Y0 - 1, PIN_Y1 + 1.5, Z_V, PIN_BORE),                                          # Zapfenbohrung bis y -48,5
          K(x, Z_V, PIN_Y1 + 1.49, PIN_Y1 + 1.5 + (PIN_BORE - FIT_LZQ_D) / 2, PIN_BORE / 2, FIT_LZQ_D / 2),   # 45°-Übergang (druckt ohne Brücke)
          Cy(x, PIN_Y1 + 2.7, HUB_Y1 + 1, Z_V, FIT_LZQ_D),
          K(x, Z_V, GEAR_Y0 - 0.01, GEAR_Y0 + 0.8, PIN_BORE / 2 + 0.8, PIN_BORE / 2),
          K(x, Z_V, HUB_Y1 - 0.6, HUB_Y1 + 0.01, FIT_LZQ_D / 2, FIT_LZQ_D / 2 + 0.6))
    g.add(B(x - 2.6, x + 2.6, Y_STEM_TIP - 1.5, Y_STEM_TIP + LZQ_STEM_FLAT_L, Z_V + FIT_LZQ_FLAT, Z_V + 3.4))

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
FOOT_Y0 = CC_Y0 - 6
FOOT_Y1 = FOOT_Y0 + 46      # Fuß nach vorn verlängert: Langloch 38 mm statt 18 mm
camcol = Part('camcol3', 'Kamerasäule v3 (Y- und Z-verstellbar)', BEIGE)
camcol.add(B(-28, 28, FOOT_Y0, FOOT_Y1, FOOT_Z0, FOOT_Z0 + 3))
for sx in (-1, 1):
    camcol.add(B(sx * 10, sx * 20, CC_Y0, CC_Y1, FOOT_Z0, PL_Z1 + 4))
    camcol.add(B(sx * 14, sx * 16, FOOT_Y0, CC_Y1, FOOT_Z0, 40 + LIFT))        # Stützrippe hinten
    camcol.add(B(sx * 14, sx * 16, CC_Y1 - 0.1, CC_Y1 + 12, FOOT_Z0, 40))        # Stützrippe vorn (bis z 40)
    camcol.add(B(sx * 14, sx * 16, CC_Y1 + 11.9, FOOT_Y1, FOOT_Z0, FOOT_Z0 + 8))  # flache Rippe über die Fußlänge
camcol.add(B(-20, 20, CC_Y0, CC_Y1, PL_Z1 + 1, PL_Z1 + 4))
for sx in (-1, 1):
    camcol.cut(*slotZ(sx * 15, CC_Y0 - 1, CC_Y1 + 1, 46, PL_Z1 - 3))
    camcol.cut(*slotY(sx * CAM_RAIL_X, FOOT_Y0 + 4, FOOT_Y1 - 4, FOOT_Z0 - 1, FOOT_Z0 + 4))

mount = Part('camMount', 'Kamerahalter v3 (1 mm, M2 durch + Mutter)', ANTH)
PLF = PL_Y0 + 1.0                                  # Platte 1 mm (Rückseite bleibt an der Säule)
HALS_D, M2_DURCH = 10.0, 3.0                       # Hals Ø10; Loch nominal 3,0 (gedruckt ≈ 2,2–2,4)
mount.add(B(-20, 20, PL_Y0, PLF, PL_Z0, PL_Z1))
for sx in (-1, 1):
    for sz in (-1, 1):
        hx, hz = sx * CAM_HW / 2, Z_CAM + sz * CAM_HL / 2
        mount.add(Cy(hx, PLF - 0.5, BOARD_Y - 1.0, hz, HALS_D),
                  K(hx, hz, PLF - 0.01, PLF + 0.8, HALS_D / 2 + 0.8, HALS_D / 2))     # Fase am Fuß
mount.add(Cy(0, PLF - 0.5, PL_Y1 + 30, Z_LED, 16.5))   # LED-Röhre, Wand 1 mm
for sx in (-1, 1):
    for zz in (PL_Z0 + 6, PL_Z1 - 6):
        mount.cut(Cy(sx * 15, PL_Y0 - 1, PLF + 1, zz, M3_CLR))
    for sz in (-1, 1):
        mount.cut(Cy(sx * CAM_HW / 2, PL_Y0 - 1, BOARD_Y + 1, Z_CAM + sz * CAM_HL / 2, M2_DURCH))   # komplett durch
mount.cut(Cy(0, PL_Y0 - 1, PL_Y1 + 31, Z_LED, 14.5))

# Dummies
for i, x in enumerate(XS):
    if x in XS_MOTOR:
        n = Part('nemaD%d' % (XS_MOTOR.index(x) + 1), 'NEMA 17 %s (Dummy)' % ('hi', 'lo')[XS_MOTOR.index(x)], (0.3, 0.3, 0.32, 1.0))
        h = NEMA_W / 2
        n.add(B(x - h, x + h, Y_NEMA_BACK, Y_NEMA_FACE, Z_V - h, Z_V + h),
              Cy(x, Y_NEMA_FACE - 0.1, Y_NEMA_FACE + NEMA_RING_H, Z_V, NEMA_RING_D),
              Cy(x, Y_NEMA_FACE - 0.1, Y_NEMA_TIP, Z_V, NEMA_SHAFT_D),
              B(x - 8, x + 8, Y_NEMA_BACK, Y_NEMA_BACK + 12, Z_V - h - 10.0, Z_V - h + 0.1))      # 6-Pin-Stecker + Kabelbogen: 10 mm nach unten
    l = Part('lzqD%d' % (i + 1), 'LZQ %s (Dummy)' % ('Ablass hi', 'Zufuhr hi', 'Zufuhr lo', 'Ablass lo')[i], (0.7, 0.85, 0.95, 1.0))
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

# ---------------- Emit ----------------
def emit_any(o, idexpr):
    if o[0] == 'tear':
        _, p0, p1, d = o
        return 'tearOp(context, %s, vector(%s, %s, %s) * millimeter, vector(%s, %s, %s) * millimeter, %s);' % (
            idexpr, f(p0[0]), f(p0[1]), f(p0[2]), f(p1[0]), f(p1[1]), f(p1[2]), f(d / 2))
    if o[0] == 'prism':
        _, c, pts, y0, y1 = o
        return 'prismOp(context, %s, vector(%s, %s, %s) * millimeter, [%s], %s);' % (
            idexpr, f(c[0]), f(y0), f(c[1]), ', '.join('[%s, %s]' % (f(a), f(b)) for a, b in pts), f(y1 - y0))
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
    if var == 'ihhtSplitterV3':
        L += ['        // ===== SCHNITTE START (zum Entfernen alles bis SCHNITTE ENDE löschen) =====',
              '        try silent',
              '        {',
              '            const cutPlanes = [plane(vector(0, 0, 25) * millimeter, vector(0, 0, 1)),        // waagerecht durch die Kanalachse (z 25)',
              '                               plane(vector(0, 66, 0) * millimeter, vector(0, 1, 0)),        // senkrecht durch die Längsachse des Querkanals (y 66)',
              '                               plane(vector(-67.95, 0, 0) * millimeter, vector(1, 0, 0)),    // senkrecht durch die Achse der äußeren Innentülle (hi)',
              '                               plane(vector(67.95, 0, 0) * millimeter, vector(1, 0, 0))];    // senkrecht durch die Achse der äußeren Innentülle (lo)',
              '            for (var i = 0; i < size(cutPlanes); i += 1)',
              '            {',
              '                opPlane(context, id + ("cutPl" ~ i), { "plane" : cutPlanes[i], "width" : 400 * millimeter, "height" : 400 * millimeter });',
              '                opSplitPart(context, id + ("cutSp" ~ i), { "targets" : qBodyType(qCreatedBy(id, EntityType.BODY), BodyType.SOLID), "tool" : qCreatedBy(id + ("cutPl" ~ i), EntityType.FACE) });',
              '                opDeleteBodies(context, id + ("cutDel" ~ i), { "entities" : qCreatedBy(id + ("cutPl" ~ i), EntityType.BODY) });',
              '            }',
              '        }',
              '        // ===== SCHNITTE ENDE =====']
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
      '// IHHT-Gasmischer – v4 Entwicklungs-Setup: 2 NEMA, Ventilpaare über Zahnräder gegenläufig gekoppelt (generiert). Änderungen bitte per Chat.',
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
L += ['function tearOp(context is Context, id is Id, p0 is Vector, p1 is Vector, r is number)', '{',
      '    // Waagerechter Kanal mit vollem Tropfenprofil (45°-Flanken bis zur Spitze, kein Dach) -> druckt ohne Support',
      '    fCylinder(context, id + "cyl", { "bottomCenter" : p0, "topCenter" : p1, "radius" : r * millimeter });',
      '    const n = normalize(p1 - p0);',
      '    const len = norm(p1 - p0);',
      '    const xd = normalize(cross(vector(0, 0, 1), n));',
      '    const s2 = r / sqrt(2);',
      '    const w = r * (sqrt(2) - 1);',
      '    try',
      '    {',
      '        const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(p0, n, xd) });',
      '        skLineSegment(sk, "a", { "start" : vector(0, 0) * millimeter, "end" : vector(-s2, s2) * millimeter });',
      '        skLineSegment(sk, "b", { "start" : vector(-s2, s2) * millimeter, "end" : vector(0, r * sqrt(2)) * millimeter });',
      '        skLineSegment(sk, "d", { "start" : vector(0, r * sqrt(2)) * millimeter, "end" : vector(s2, s2) * millimeter });',
      '        skLineSegment(sk, "e", { "start" : vector(s2, s2) * millimeter, "end" : vector(0, 0) * millimeter });',
      '        skSolve(sk);',
      '        opExtrude(context, id + "ext", { "entities" : qSketchRegion(id + "sk"), "direction" : n, "endBound" : BoundingType.BLIND, "endDepth" : len });',
      '        opDeleteBodies(context, id + "del", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });',
      '    }',
      '    catch (e)',
      '    {',
      '        // Fallback: nur runder Kanal',
      '    }',
      '}', '']
L += ['function prismOp(context is Context, id is Id, origin is Vector, pts is array, depth is number)', '{',
      '    // Polygon [dx, dz] in der XZ-Ebene durch origin, Extrusion um depth in +Y (Zahnradprofil)',
      '    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(origin, vector(0, 1, 0), vector(1, 0, 0)) });   // Skizzen-y zeigt nach -Z',
      '    const n = size(pts);',
      '    for (var i = 0; i < n; i += 1)',
      '    {',
      '        const a = pts[i];',
      '        const b = pts[(i + 1) % n];',
      '        skLineSegment(sk, "l" ~ i, { "start" : vector(a[0], -a[1]) * millimeter, "end" : vector(b[0], -b[1]) * millimeter });',
      '    }',
      '    skSolve(sk);',
      '    opExtrude(context, id + "ext", { "entities" : qSketchRegion(id + "sk"), "direction" : vector(0, 1, 0), "endBound" : BoundingType.BLIND, "endDepth" : depth * millimeter });',
      '    opDeleteBodies(context, id + "del", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });',
      '}', '']
emit_feature(L, V2, 'dev_', 'IHHT Entwicklung v4', 'ihhtDevV4')
open(sys.argv[1], 'w').write('\n'.join(L) + '\n')

def dump(pl, grp):
    return [dict(name=p.name, label=p.label, col=p.col, printed=p.printed, stages=p.stages, group=grp) for p in pl]
json.dump(dump(V2, 'dev'), open(sys.argv[2], 'w'))

half = XS[-1] + LZQ_W / 2
d = -LZQ_DEPTH - Y_LENS
print('Kamera: Abstand %.0f mm, braucht HFOV %.1f°, VFOV %.1f°; Kugelbereich z %.1f..%.1f, Kamera z %.1f' % (
    d, 2 * math.degrees(math.atan(half / d)), 2 * math.degrees(math.atan((BALL_Z1 - BALL_Z0) / 2 / d)), BALL_Z0, BALL_Z1, Z_CAM))
