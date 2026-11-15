# Generator: builds the IHHT chassis as a list of parts (primitive ops + boolean stages),
# emits an Onshape FeatureScript custom feature AND a trimesh preview from the same data.
import sys, math, json

# ---------------- Dummy-Maße (aus ihht-pi Part Studio) ----------------
NEMA_W, NEMA_H = 42.3, 23.0
NEMA_SHAFT_D, NEMA_SHAFT_L = 5.0, 19.0
NEMA_RING_D, NEMA_RING_H = 22.0, 2.0
NEMA_HOLE_PITCH = 31.0

LZQ_D, LZQ_FLATCUT, LZQ_A, LZQ_B = 23.0, 2.0, 120.0, 96.0   # D, D-C, A, B
LZQ_PORT_D, LZQ_PORT_L, LZQ_PORT_TOP = 10.5, 23.0, 16.0      # E, Stutzenlänge, Abstand oberer Stutzen zur Oberkante
LZQ_STEM_D, LZQ_STEM_L, LZQ_VALVE_Z = 4.0, 21.0, 111.0       # G, F, Höhe Ventilachse über Unterkante
LZQ_NUT_AF, LZQ_NUT_H = 12.0, 2.5

CAM_W, CAM_L = 25.0, 24.0
CAM_HOLE_W, CAM_HOLE_L = 21.0, 12.5
KEYRING_D, KEYRING_TUBE_L = 14.5, 30.0

PI = (85.0, 56.0, 17.0)
UNO = (68.6, 53.3, 35.0)
PSU = (120.0, 50.0, 30.0)
FOV = 120.0

# Tüllen (Außendurchmesser Kern / Bohrung) – leicht änderbar
TUELLE = {
    'lzq':  (8.0, 4.0),   # Richtung Rotameter-Stutzen (Schlauchstück)
    'lo':   (8.0, 5.0),   # hypoxischer Eingang
    'hi':   (7.0, 5.0),   # hyperoxischer Eingang (O2, dünner)
    'mix':  (8.0, 5.0),   # Mischausgang (noch offen)
    'vent': (8.0, 5.0),   # Ablass
}

# ---------------- Layout ----------------
PITCH = NEMA_W + 3.0
XS = [-1.5 * PITCH, -0.5 * PITCH, 0.5 * PITCH, 1.5 * PITCH]
R_LZQ = LZQ_D / 2
Y_FLAT = R_LZQ - LZQ_FLATCUT          # 9.5  Rückseite LZQ = Vorderseite Montagewand
Y_LZQ_FRONT = -R_LZQ                  # -11.5
Z_DECK = 54.0
Z0 = 58.0                              # LZQ Unterkante
Z_PORT_TOP = Z0 + LZQ_A - LZQ_PORT_TOP # 162
Z_PORT_BOT = Z_PORT_TOP - LZQ_B        # 66
Z_V = Z0 + LZQ_VALVE_Z                 # 169
Y_STEM_TIP = Y_LZQ_FRONT - LZQ_STEM_L  # -32.5
Y_NEMA_TIP = -39.5
Y_NEMA_FACE = Y_NEMA_TIP - NEMA_SHAFT_L  # -58.5
Y_NEMA_BACK = Y_NEMA_FACE - NEMA_H       # -81.5
Z_CAM = Z0 + 55.0                       # 113

X_IN, X_OUT = 93.15, 96.65            # Seitenwand innen/außen
Y_FRONT_END, Y_BACK_END = -90.2, 65.7
SHELL = dict(x=99.15, y0=-92.7, y1=68.2, r=8.0)
Z_TOP = 200.0
WALL_T = 3.5

def ch(yc, t):  # channel: slot + ribs
    a, b = yc - t / 2 - 0.2, yc + t / 2 + 0.2
    return (a, b), [(a - 2.0, a), (b, b + 2.0)]
FRONT_YC, BACK_YC, BRIDGE_YC = -86.25, 61.75, Y_NEMA_FACE + 2.0
PANEL_X = X_IN - 0.15

# ---------------- part model ----------------
parts = []
class Part:
    def __init__(s, name, label, col, printed=True):
        s.name, s.label, s.col, s.printed = name, label, col, printed
        s.stages = []
        parts.append(s)
    def add(s, *ops):
        if not s.stages or s.stages[-1][0] != 'add': s.stages.append(['add', []])
        s.stages[-1][1].extend(ops); return s
    def cut(s, *ops):
        if not s.stages or s.stages[-1][0] != 'cut': s.stages.append(['cut', []])
        s.stages[-1][1].extend(ops); return s

def B(x0, x1, y0, y1, z0, z1):
    return ('box', min(x0, x1), max(x0, x1), min(y0, y1), max(y0, y1), min(z0, z1), max(z0, z1))
def C(p0, p1, d):
    return ('cyl', tuple(p0), tuple(p1), d)
def Cy(x, y0, y1, z, d): return C((x, y0, z), (x, y1, z), d)   # axis Y
def Cz(x, y, z0, z1, d): return C((x, y, z0), (x, y, z1), d)   # axis Z
def Cx(x0, x1, y, z, d): return C((x0, y, z), (x1, y, z), d)   # axis X

def rrect(x0, x1, y0, y1, z0, z1, r):
    return [B(x0 + r, x1 - r, y0, y1, z0, z1), B(x0, x1, y0 + r, y1 - r, z0, z1),
            Cz(x0 + r, y0 + r, z0, z1, 2 * r), Cz(x1 - r, y0 + r, z0, z1, 2 * r),
            Cz(x0 + r, y1 - r, z0, z1, 2 * r), Cz(x1 - r, y1 - r, z0, z1, 2 * r)]

def barbY(x, z, ytip, ybase, od, bore_note=None):
    """hose barb along Y from ybase to ytip (tip = smaller end of ridges)."""
    s = 1 if ytip > ybase else -1
    r0 = od / 2
    ops = [Cy(x, ybase, ytip, z, od)]
    for k in range(2):                      # two ridges, saw-tooth from stacked cylinders
        start = ytip - s * (0.6 + k * 3.6)
        for j, dr in enumerate([0.15, 0.3, 0.45, 0.6]):
            a = start - s * j * 0.75
            ops.append(Cy(x, a, a - s * 0.75, z, 2 * (r0 + dr)))
    return ops

SCREW_M3_CLR, SCREW_M3_PILOT, M3_NUT_W, M3_NUT_T = 3.4, 3.2, 5.9, 2.8
M2_PILOT, M2_CLR = 1.8, 2.4

BEIGE = (0.87, 0.81, 0.69, 1.0)
BEIGE_D = (0.78, 0.72, 0.60, 1.0)
ANTH = (0.22, 0.22, 0.24, 1.0)

# ================= BASE (Elektronik-Wanne) =================
base = Part('base', 'Unterteil (Elektronik)', BEIGE)
base.add(*rrect(-SHELL['x'], SHELL['x'], SHELL['y0'], SHELL['y1'], 0, 50, SHELL['r']))
for sx in (-1, 1):
    for sy, yy in ((-1, SHELL['y0'] + 14), (1, SHELL['y1'] - 14)):
        base.add(Cz(sx * (SHELL['x'] - 14), yy, -2, 0.5, 14))      # Füße
BASE_POSTS = [(sx * 88.15, yc) for sx in (-1, 1) for yc in (-78.5, 54.0)]
inner = rrect(-SHELL['x'] + WALL_T, SHELL['x'] - WALL_T, SHELL['y0'] + WALL_T, SHELL['y1'] - WALL_T, 3, 51, SHELL['r'] - WALL_T)
base.cut(*inner)
# Posts, Standoffs, PSU-Stopper
for (px, py) in BASE_POSTS:
    base.add(B(px - 4, px + 4, py - 4, py + 4, 2, 50))
PI_X0, PI_Y0 = -84.0, -80.0
PI_HOLES = [(PI_X0 + PI[0] - hx, PI_Y0 + PI[1] - hy) for hx, hy in ((3.5, 3.5), (61.5, 3.5), (3.5, 52.5), (61.5, 52.5))]
UNO_X0, UNO_Y0 = 15.0, -80.0
UNO_HOLES = [(UNO_X0 + UNO[0] - hx, UNO_Y0 + UNO[1] - hy) for hx, hy in ((13.97, 2.54), (15.24, 50.8), (66.04, 7.62), (66.04, 35.56))]
for (hx, hy) in PI_HOLES + UNO_HOLES:
    base.add(Cz(hx, hy, 2, 9, 6.0))
PSU_X0, PSU_X1, PSU_Y0, PSU_Y1 = -60.0, 60.0, 8.0, 58.0
for sx, px in ((-1, PSU_X0), (1, PSU_X1)):
    for sy, py in ((-1, PSU_Y0), (1, PSU_Y1)):
        base.add(B(px, px + sx * 4, py - sy * 10, py + sy * 4, 2, 14),
                 B(px - sx * 10, px + sx * 4, py, py + sy * 4, 2, 14))
# Schnitte Stufe 2
for (px, py) in BASE_POSTS:
    base.cut(Cz(px, py, 34, 51, SCREW_M3_PILOT),
             B(px - math.copysign(4.6, px), px + math.copysign(2.95, px), py - M3_NUT_W / 2, py + M3_NUT_W / 2, 42, 42 + M3_NUT_T))
for (hx, hy) in PI_HOLES + UNO_HOLES:
    base.cut(Cz(hx, hy, 4, 10, M2_PILOT))
base.cut(B(-101, -94, -74, -26, 9, 28))      # Pi USB/LAN
base.cut(B(94, 101, -74, -34, 9, 26))        # Uno USB/DC
base.cut(Cy(-80, 63, 70, 25, 12))            # Kabel Netzteil / 5V
for k in range(7):
    c = -45 + 15 * k
    base.cut(B(c - 2, c + 2, 63, 70, 10, 34))
for sx in (-1, 1):
    for k in range(6):
        c = 10 + 9 * k
        base.cut(B(sx * 94, sx * 101, c - 2, c + 2, 12, 36))

# ================= FRAME (Deck + Seitenwände + LZQ-Montagewand) =================
frame = Part('frame', 'Rahmen', BEIGE)
frame.add(*rrect(-SHELL['x'], SHELL['x'], SHELL['y0'], SHELL['y1'], 50, Z_DECK, SHELL['r']))
for sx in (-1, 1):
    frame.add(B(sx * X_IN, sx * X_OUT, Y_FRONT_END, Y_BACK_END, Z_DECK - 0.5, Z_TOP))
frame.add(B(-X_IN - 0.05, X_IN + 0.05, Y_FLAT, Y_FLAT + 4.0, Z_DECK - 0.5, Z_TOP))
for yc, t, z0, z1, deck in ((FRONT_YC, 3.5, Z_DECK - 0.5, Z_TOP, True), (BACK_YC, 3.5, Z_DECK - 0.5, Z_TOP, True),
                            (BRIDGE_YC, 4.0, 136, 190, False)):
    slot, ribs = ch(yc, t)
    for (a, b) in ribs:
        for sx in (-1, 1):
            frame.add(B(sx * (X_IN - 2.0), sx * (X_IN + 0.5), a, b, z0, z1))
        if deck:
            frame.add(B(-X_IN - 0.05, X_IN + 0.05, a, b, Z_DECK - 0.5, Z_DECK + 2.0))
bslot, bribs = ch(BRIDGE_YC, 4.0)
for sx in (-1, 1):
    frame.add(B(sx * (X_IN - 5.0), sx * (X_IN + 0.5), bribs[0][0], bribs[1][1], 136, 142))   # Auflage Motorbrücke
LID_POSTS = [(sx * 89.15, yp) for sx in (-1, 1) for yp in (-40.0, 40.0)]
for (px, py) in LID_POSTS:
    frame.add(B(px - math.copysign(4, px), px + math.copysign(4.5, px), py - 4, py + 4, Z_DECK - 0.5, Z_TOP))
for x in XS:
    for z in (Z_PORT_BOT, Z_PORT_TOP):
        frame.cut(Cy(x, Y_FLAT - 0.5, Y_FLAT + 4.5, z, 11.0))
for (px, py) in LID_POSTS:
    frame.cut(Cz(px, py, 188, Z_TOP + 1, SCREW_M3_PILOT),
              B(px - math.copysign(4.2, px), px + math.copysign(2.95, px), py - M3_NUT_W / 2, py + M3_NUT_W / 2, 192, 192 + M3_NUT_T))
for (px, py) in BASE_POSTS:
    frame.cut(Cz(px, py, 49, Z_DECK + 1, SCREW_M3_CLR), Cz(px, py, Z_DECK - 1.5, Z_DECK + 1, 6.2))
for x0, x1 in ((-75, -8), (8, 75)):
    frame.cut(B(x0, x1, -81.5, -74.5, 49, Z_DECK + 3))     # Kabel NEMA + Kamera

# ================= LID =================
lid = Part('lid', 'Deckel', BEIGE_D)
lid.add(*rrect(-SHELL['x'], SHELL['x'], SHELL['y0'], SHELL['y1'], Z_TOP, Z_TOP + 4, SHELL['r']))
for (px, py) in LID_POSTS:
    lid.cut(Cz(px, py, Z_TOP - 1, Z_TOP + 5, SCREW_M3_CLR), Cz(px, py, Z_TOP + 2.2, Z_TOP + 5, 6.2))
# dezente Lüftungsrillen über dem Rückraum
for k in range(5):
    x = -40 + 20 * k
    lid.cut(B(x - 6, x + 6, 30, 50, Z_TOP - 1, Z_TOP + 5))

# ================= FRONT PANEL =================
fslot, _ = ch(FRONT_YC, 3.5)
FP_Y0, FP_Y1 = FRONT_YC - 1.75, FRONT_YC + 1.75     # -88 .. -84.5
front = Part('front', 'Frontplatte', BEIGE)
front.add(B(-PANEL_X, PANEL_X, FP_Y0, FP_Y1, Z_DECK, Z_TOP))
INS = 19.0
for sx in (-1, 1):
    for sz in (-1, 1):
        front.add(Cy(sx * INS, FP_Y1 - 0.5, FP_Y1 + 2.5, Z_CAM + sz * INS, 7.0))
front.cut(B(-15, 15, FP_Y0 - 1, FP_Y1 + 1, Z_CAM - 15, Z_CAM + 15))
front.cut(Cy(0, FP_Y0 - 1, FP_Y1 + 1, Z_CAM - 34, 5.2))     # LED
for sx in (-1, 1):
    for sz in (-1, 1):
        front.cut(Cy(sx * INS, FP_Y0 - 1, FP_Y1 + 3, Z_CAM + sz * INS, M2_PILOT))

def insert_plate(p, dy):
    p.add(B(-23, 23, FP_Y0 - 3 + dy, FP_Y0 + dy, Z_CAM - 23, Z_CAM + 23))
def insert_holes(p, dy):
    for sx in (-1, 1):
        for sz in (-1, 1):
            p.cut(Cy(sx * INS, FP_Y0 - 4 + dy, FP_Y0 + 1 + dy, Z_CAM + sz * INS, M2_CLR))

insA = Part('insKey', 'Kamera-Einsatz Röhre (Schlüsselanhänger-Cam)', ANTH)
insert_plate(insA, 0)
insA.add(Cy(0, FP_Y0 - 0.5, FP_Y0 + KEYRING_TUBE_L, Z_CAM, KEYRING_D + 4.0))
insert_holes(insA, 0)
insA.cut(Cy(0, FP_Y0 - 4, FP_Y0 + KEYRING_TUBE_L + 1, Z_CAM, KEYRING_D))

DY_EXP = -60.0
insB = Part('insPi', 'Kamera-Einsatz Pi-Cam (explodiert)', ANTH)
insert_plate(insB, DY_EXP)
for sx in (-1, 1):
    for sz in (-1, 1):
        insB.add(Cy(sx * CAM_HOLE_W / 2, FP_Y0 - 0.5 + DY_EXP, -79.5 + DY_EXP, Z_CAM + sz * CAM_HOLE_L / 2, 5.0))
insert_holes(insB, DY_EXP)
for sx in (-1, 1):
    for sz in (-1, 1):
        insB.cut(Cy(sx * CAM_HOLE_W / 2, -85 + DY_EXP, -79 + DY_EXP, Z_CAM + sz * CAM_HOLE_L / 2, M2_PILOT))

# ================= NEMA-BRÜCKE =================
bridge = Part('bridge', 'Motorbrücke', BEIGE)
BR_Y0, BR_Y1 = Y_NEMA_FACE, Y_NEMA_FACE + 4.0
bridge.add(B(-PANEL_X, PANEL_X, BR_Y0, BR_Y1, 142, 187))
for x in XS:
    bridge.cut(Cy(x, BR_Y0 - 1, BR_Y1 + 1, Z_V, 24.0))
    for sx in (-1, 1):
        for sz in (-1, 1):
            hx, hz = x + sx * NEMA_HOLE_PITCH / 2, Z_V + sz * NEMA_HOLE_PITCH / 2
            bridge.cut(Cy(hx, BR_Y0 - 1, BR_Y1 + 1, hz, SCREW_M3_CLR), Cy(hx, BR_Y1 - 1.5, BR_Y1 + 1, hz, 6.2))

# ================= KUPPLUNGEN (geteilte Klemmkupplung) =================
CP_Y0, CP_Y1 = Y_NEMA_TIP - 11.0, Y_NEMA_TIP + 16.0     # -50.5 .. -23.5
for i, x in enumerate(XS):
    for half, (z0, z1) in (('o', (Z_V + 0.2, Z_V + 7)), ('u', (Z_V - 7, Z_V - 0.2))):
        p = Part('cpl%d%s' % (i + 1, half), 'Kupplung %d %s' % (i + 1, 'oben' if half == 'o' else 'unten'), (0.93, 0.55, 0.2, 1.0))
        p.add(B(x - 10, x + 10, CP_Y0, CP_Y1, z0, z1))
        p.cut(Cy(x, CP_Y0 - 1, Y_NEMA_TIP + 0.5, Z_V, NEMA_SHAFT_D + 0.1),
              Cy(x, Y_NEMA_TIP, CP_Y1 + 1, Z_V, LZQ_STEM_D + 0.1))
        for sx in (-1, 1):
            for yy in (CP_Y0 + 4.5, CP_Y1 - 5.0):
                d = M2_CLR if half == 'o' else M2_PILOT
                p.cut(Cz(x + sx * 7.0, yy, Z_V - 8, Z_V + 8, d))
                if half == 'o':
                    p.cut(Cz(x + sx * 7.0, yy, Z_V + 5.5, Z_V + 8, 4.2))

# ================= RÜCKWAND + VERTEILER =================
BC_Y0, BC_Y1 = BACK_YC - 1.75, BACK_YC + 1.75      # 60 .. 63.5
back = Part('back', 'Rückwand', BEIGE)
back.add(B(-PANEL_X, PANEL_X, BC_Y0, BC_Y1, Z_DECK, Z_TOP))
MANIS = [
    ('maniLo',  'Verteiler lo (hypoxisch, Eingang)',  [XS[0], XS[1]], (XS[0] + XS[1]) / 2, Z_PORT_BOT, 'lo',  (0.35, 0.55, 0.85, 1.0)),
    ('maniHi',  'Verteiler hi (hyperoxisch, Eingang)', [XS[2], XS[3]], (XS[2] + XS[3]) / 2, Z_PORT_BOT, 'hi',  (0.85, 0.35, 0.3, 1.0)),
    ('maniMix', 'Sammler Mix (Ausgang)',               [XS[1], XS[2]], 0.0,                 Z_PORT_TOP, 'mix', (0.6, 0.4, 0.8, 1.0)),
    ('ventL',   'Ablass links (lo)',                   [XS[0]],        XS[0],               Z_PORT_TOP, 'vent', (0.55, 0.6, 0.65, 1.0)),
    ('ventR',   'Ablass rechts (hi)',                  [XS[3]],        XS[3],               Z_PORT_TOP, 'vent', (0.55, 0.6, 0.65, 1.0)),
]
MB_Y0, MB_Y1, MB_CH = 48.0, BC_Y0, 54.0
OUT_LEN = 14.0
for name, label, xin, xout, zc, kind, col in MANIS:
    m = Part(name, label, col)
    xa, xb = min(xin + [xout]) - 8, max(xin + [xout]) + 8
    if xb - xa < 30: xa, xb = xout - 15, xout + 15
    m.add(B(xa, xb, MB_Y0, MB_Y1, zc - 8, zc + 14))
    od_in, bore_in = TUELLE['lzq']
    od_out, bore_out = TUELLE[kind]
    for x in xin:
        m.add(*barbY(x, zc, MB_Y0 - 8, MB_Y0 + 0.5, od_in))
    m.add(*barbY(xout, zc, BC_Y1 + OUT_LEN, MB_Y1 - 0.5, od_out))
    xs_all = xin + [xout]
    if max(xs_all) - min(xs_all) > 0.1:
        m.cut(Cx(min(xs_all), max(xs_all), MB_CH, zc, 5.5))
    for x in xin:
        m.cut(Cy(x, MB_Y0 - 9, MB_CH + 0.5, zc, bore_in))
    m.cut(Cy(xout, MB_CH - 0.5, BC_Y1 + OUT_LEN + 1, zc, bore_out))
    sd = 12.0 if (xb - xa) > 40 else 9.0
    for sx in (-1, 1):
        x = xout + sx * sd
        m.cut(Cy(x, 50, MB_Y1 + 0.5, zc + 9, SCREW_M3_PILOT),
              B(x - M3_NUT_W / 2, x + M3_NUT_W / 2, 55 - M3_NUT_T / 2, 55 + M3_NUT_T / 2, zc + 9 - 3.2, zc + 14.5))
        back.cut(Cy(x, BC_Y0 - 1, BC_Y1 + 1, zc + 9, SCREW_M3_CLR), Cy(x, BC_Y1 - 1.5, BC_Y1 + 1, zc + 9, 6.2))
    back.cut(Cy(xout, BC_Y0 - 1, BC_Y1 + 1, zc, od_out + 2.2))
for k in range(3):
    z = 183 + 6 * k
    back.cut(B(-30, 30, BC_Y0 - 1, BC_Y1 + 1, z, z + 3))

# ================= DUMMIES =================
GREY = (0.3, 0.3, 0.32, 1.0)
for i, x in enumerate(XS):
    n = Part('nema%d' % (i + 1), 'NEMA 17 #%d (Dummy)' % (i + 1), GREY, printed=False)
    h = NEMA_W / 2
    n.add(B(x - h, x + h, Y_NEMA_BACK, Y_NEMA_FACE, Z_V - h, Z_V + h),
          Cy(x, Y_NEMA_FACE - 0.1, Y_NEMA_FACE + NEMA_RING_H, Z_V, NEMA_RING_D),
          Cy(x, Y_NEMA_FACE - 0.1, Y_NEMA_TIP, Z_V, NEMA_SHAFT_D))
    l = Part('lzq%d' % (i + 1), 'LZQ #%d (Dummy)' % (i + 1), (0.7, 0.85, 0.95, 0.45), printed=False)
    l.add(Cz(x, 0, Z0, Z0 + LZQ_A, LZQ_D))
    l.cut(B(x - 12, x + 12, Y_FLAT, Y_FLAT + 3, Z0 - 1, Z0 + LZQ_A + 1))
    for z in (Z_PORT_BOT, Z_PORT_TOP):
        l.add(Cy(x, Y_FLAT - 0.1, Y_FLAT + LZQ_PORT_L, z, LZQ_PORT_D))
    l.add(Cy(x, Y_LZQ_FRONT - LZQ_NUT_H, Y_LZQ_FRONT + 0.6, Z_V, LZQ_NUT_AF / math.cos(math.pi / 6)),
          Cy(x, Y_STEM_TIP, Y_LZQ_FRONT - LZQ_NUT_H + 0.1, Z_V, LZQ_STEM_D))
Part('pi', 'Raspberry Pi 3B+ (Dummy)', (0.2, 0.6, 0.3, 1.0), printed=False).add(B(PI_X0, PI_X0 + PI[0], PI_Y0, PI_Y0 + PI[1], 9, 9 + PI[2]))
Part('uno', 'Arduino Uno + CNC Shield (Dummy)', (0.15, 0.45, 0.65, 1.0), printed=False).add(B(UNO_X0, UNO_X0 + UNO[0], UNO_Y0, UNO_Y0 + UNO[1], 9, 9 + UNO[2]))
Part('psu', 'Netzteil 24 V (Dummy)', (0.7, 0.7, 0.72, 1.0), printed=False).add(B(PSU_X0, PSU_X1, PSU_Y0, PSU_Y1, 3, 3 + PSU[2]))
Part('keycam', 'Schlüsselanhänger-Cam (Dummy)', (0.1, 0.1, 0.1, 1.0), printed=False).add(Cy(0, FP_Y0 - 30, FP_Y0 + KEYRING_TUBE_L - 0.5, Z_CAM, KEYRING_D - 0.5))
pc = Part('picam', 'Pi-Kamera OV5647 (Dummy, explodiert)', (0.1, 0.35, 0.15, 1.0), printed=False)
pc.add(B(-CAM_W / 2, CAM_W / 2, -79.5 + DY_EXP, -78.5 + DY_EXP, Z_CAM - 8.25, Z_CAM - 8.25 + CAM_L),
       B(-4, 4, -78.6 + DY_EXP, -73.5 + DY_EXP, Z_CAM + 3.75 - 4, Z_CAM + 3.75 + 4))

# ---------------- checks ----------------
half_span = XS[-1] + R_LZQ
for dist, what in ((FP_Y0 * -1, 'Front'), (79.0, 'PiCam-Platine')):
    need = 2 * math.degrees(math.atan(half_span / dist))
    print('FOV benötigt bei Abstand %.1f (%s): %.1f°' % (dist, what, need), file=sys.stderr)

# ---------------- FeatureScript emit ----------------
def f(v): return ('%.3f' % v).rstrip('0').rstrip('.') if abs(v) > 1e-9 else '0'
def vec(p): return 'vector(%s, %s, %s) * millimeter' % (f(p[0]), f(p[1]), f(p[2]))

def emit_op(o, idexpr):
    if o[0] == 'box':
        _, x0, x1, y0, y1, z0, z1 = o
        return 'fCuboid(context, %s, { "corner1" : %s, "corner2" : %s });' % (idexpr, vec((x0, y0, z0)), vec((x1, y1, z1)))
    _, p0, p1, d = o
    return 'fCylinder(context, %s, { "bottomCenter" : %s, "topCenter" : %s, "radius" : %s * millimeter });' % (idexpr, vec(p0), vec(p1), f(d / 2))

def emit():
    L = []
    L.append('FeatureScript 3112;')
    L.append('import(path : "onshape/std/geometry.fs", version : "3112.0");')
    L.append('')
    L.append('// IHHT-Gasmischer – Chassis v1 (generiert). Keine Parameter: Änderungen bitte per Chat.')
    L.append('// Koordinaten: X = links/rechts, Y = vorne(-)/hinten(+), Z = oben. Ursprung: Mitte, Unterkante Gehäuse.')
    L.append('')
    L.append('function unionAll(context is Context, opId is Id, q is Query)')
    L.append('{')
    L.append('    const bodies = qBodyType(q, BodyType.SOLID);')
    L.append('    if (size(evaluateQuery(context, bodies)) > 1)')
    L.append('        opBoolean(context, opId, { "tools" : bodies, "operationType" : BooleanOperationType.UNION });')
    L.append('}')
    L.append('')
    L.append('function cutAll(context is Context, opId is Id, targets is Query, tools is Query)')
    L.append('{')
    L.append('    if (size(evaluateQuery(context, tools)) > 0)')
    L.append('        opBoolean(context, opId, { "tools" : tools, "targets" : qBodyType(targets, BodyType.SOLID), "operationType" : BooleanOperationType.SUBTRACTION });')
    L.append('}')
    L.append('')
    L.append('function finishPart(context is Context, q is Query, label is string, c is Color)')
    L.append('{')
    L.append('    const b = qBodyType(q, BodyType.SOLID);')
    L.append('    setProperty(context, { "entities" : b, "propertyType" : PropertyType.NAME, "value" : label });')
    L.append('    setProperty(context, { "entities" : b, "propertyType" : PropertyType.APPEARANCE, "value" : c });')
    L.append('}')
    L.append('')
    fnames = []
    for p in parts:
        fn = 'build_' + p.name
        fnames.append(fn)
        L.append('function %s(context is Context, id is Id, prog is box)' % fn)
        L.append('{')
        adds = []
        def tq():
            return 'qUnion([' + ', '.join('qCreatedBy(%s, EntityType.BODY)' % a for a in adds) + '])'
        for si, (kind, ops) in enumerate(p.stages):
            if kind == 'add':
                L.append('    prog[] = "Körper Stufe %d";' % si)
                L.append('    const A%d = id + "%s_a%d";' % (si, p.name, si))
                adds.append('A%d' % si)
                for j, o in enumerate(ops):
                    L.append('    ' + emit_op(o, 'A%d + "s%d"' % (si, j)))
                L.append('    prog[] = "Vereinigen Stufe %d";' % si)
                L.append('    unionAll(context, id + "%s_u%d", %s);' % (p.name, si, tq()))
            else:
                L.append('    prog[] = "Schnittkörper Stufe %d";' % si)
                L.append('    const C%d = id + "%s_c%d";' % (si, p.name, si))
                for j, o in enumerate(ops):
                    L.append('    ' + emit_op(o, 'C%d + "o%d"' % (si, j)))
                L.append('    prog[] = "Abziehen Stufe %d";' % si)
                L.append('    cutAll(context, id + "%s_x%d", %s, qCreatedBy(C%d, EntityType.BODY));' % (p.name, si, tq(), si))
        c = p.col
        L.append('    prog[] = "Name/Farbe";')
        L.append('    finishPart(context, %s, "%s", color(%s, %s, %s));' % (tq(), p.label, f(c[0]), f(c[1]), f(c[2])))
        L.append('}')
        L.append('')
    L.append('annotation { "Feature Type Name" : "IHHT Chassis v1.2" }')
    L.append('export const ihhtChassis = defineFeature(function(context is Context, id is Id, definition is map)')
    L.append('    precondition')
    L.append('    {')
    L.append('    }')
    L.append('    {')
    L.append('        const prog = new box("");')
    L.append('        var failed = [];')
    L.append('        var okCount = 0;')
    for fn, p in zip(fnames, parts):
        L.append('        try')
        L.append('        {')
        L.append('            prog[] = "Start";')
        L.append('            %s(context, id, prog);' % fn)
        L.append('            okCount += 1;')
        L.append('        }')
        L.append('        catch (error)')
        L.append('        {')
        L.append('            failed = append(failed, "%s @ " ~ prog[] ~ " -> " ~ toString(error));' % p.name)
        L.append('            println("FEHLER %s @ " ~ prog[] ~ " -> " ~ toString(error));' % p.name)
        L.append('        }')
    L.append('        if (size(failed) > 0)')
    L.append('        {')
    L.append('            var msg = "OK: " ~ toString(okCount) ~ " von %d Teilen. Fehler: ";' % len(parts))
    L.append('            for (var s in failed)')
    L.append('                msg = msg ~ s ~ " || ";')
    L.append('            reportFeatureWarning(context, id, msg);')
    L.append('        }')
    L.append('    });')
    return '\n'.join(L) + '\n'

if __name__ == '__main__':
    open(sys.argv[1], 'w').write(emit())
    json.dump([dict(name=p.name, label=p.label, col=p.col, printed=p.printed, stages=p.stages) for p in parts], open(sys.argv[2], 'w'))
