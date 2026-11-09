"""
Gesichtsfoto-Gestell v4 (alle Verbindungselemente gedruckt, M10)  (iPhone 14 Pro Max, Hauptkamera, 250 mm Objektiv -> Stirnpolster)

Koordinaten der Baugruppe:
  X = links/rechts, Y = Kameraachse (Y=0 Objektiv-Vorderkante, +Y Richtung Gesicht), Z = hoch (Z=0 Tisch)

Teile (alle ohne Stützmaterial druckbar):
  1 kopfrahmen   – Grundplatte hinten + 2 verjüngte Säulen + Kinnsockel (Zungen-Auflage, Gelenk, Anschlag) + Gabel
  2 kopfbuegel   – Bügelplatte mit Stirnpolster + federnden Schläfenpolstern (kopfüber gedruckt)
  3 kinnhebel    – klappbare Kinnschale (aufrecht gedruckt)
  4 traeger      – T-Träger mit Handy-Wiege und Zunge für die Gabel
Verbindungselemente (gedruckt, M10 x 1,5): Gelenkbolzen + Mutter, 2x Gabelschraube + Mutter, 2x Bügelschraube (Gewinde in der Säule)
"""
import math
import cadquery as cq
from shapely.geometry import box as sbox, Point, Polygon
from shapely.ops import unary_union
from shapely import affinity

# ================= Parameter =================
D_STIRN = 250.0          # Objektiv -> Kontaktfläche Stirnpolster
FACE_BELOW_PAD = 70.0    # Objektivhöhe unter Stirnpolster-Mitte
CHIN_DROP = 150.0        # Stirnpolster-Mitte -> Kinnauflage (Gesicht-Modus)
CHIN_BACK = 25.0         # Kinnauflage-Mitte liegt so weit hinter der Stirn-Kontaktfläche
TONGUE_DROP = 32.0       # Zungen-Modus: Kinn liegt so viel tiefer ...
TONGUE_BACK = 12.0       # ... und so viel weiter hinten (Unterkiefer schwenkt nach hinten-unten)

# iPhone 14 Pro Max ohne Hülle
PH_W, PH_H, PH_T, BUMP = 77.6, 160.7, 7.85, 4.17
LENS_DX, LENS_DZ = 15.0, 22.0   # Hauptkamera: Abstand von (Rückseite gesehen) linker Kante / Oberkante – geschätzt
CLR = 0.4                       # Spiel im Handyschacht je Seite
WALL = 2.5                      # Wandstärke Wiege

BASE_T = 6.0
FIT = 0.2

# Säulen (verjüngt: unten tief, oben schlank)
POST_X0, POST_X1 = 88.0, 106.0  # 18 breit: Platz für M10-Innengewinde  # X-Bereich (+X-Seite, gespiegelt)
POST_Y_REAR = 312.0
POST_D_BOT, POST_D_TOP = 46.0, 18.0

# Kopfbügel
HALO_T = 5.0                    # Plattenstärke
IN_AX, IN_AY = 78.0, 110.0      # innere Freikontur (Ellipse)
OUT_AX, OUT_AY = 97.0, 122.0    # äußere Kontur
HALO_END_Y = 335.0
PAD_R, PAD_T, PAD_W = 90.0, 5.0, 40.0     # Stirnpolster: Krümmungsradius, Stärke, Breite
PAD_DOWN = 28.0                 # Polster reicht so weit unter die Platte
TEMPLE_GAP = 124.0              # lichter Abstand Schläfenpolster
TEMPLE_Y0, TEMPLE_Y1 = 305.0, 327.0
TEMPLE_T = 2.4                  # federnd

# Kinnschale / Zungenauflage: doppelt gekrümmte Mulde (quer eng, längs flach) -> zentriert jede Kinnbreite
DISH_RX, DISH_RY = 45.0, 110.0
CUP_W, CUP_FRONT, CUP_BACK = 72.0, 21.0, 25.0   # Schale: Breite, Länge vor/hinter Gelenkachse
CUP_RIM = 10.0                  # Randhöhe über Muldengrund
CUP_FLOOR = 3.0                 # Material unter Muldengrund
LEAN = 5.0                      # Hebel lehnt 5° nach hinten gegen Anschlag (Übertotpunkt)
LEV_BW, LEV_BT = 36.0, 20.0     # Hebelarm Breite (X) / Dicke (Y)
PIVOT_ABOVE_BASE = 18.0
LEV_BOT = -10.0                 # Hebel-Unterkante unter der Achse
DISH_OFF = 2.0                  # Muldenmitte hinter Gelenkachse -> Last drückt in den Anschlag
HOLE10 = 10.6                   # Durchgangsloch M10 (gedruckt)
PIVOT_HOLE = 10.4               # Lagerbohrung Gelenk
TAP10 = 2 * ((10 - 1.0825 * 1.5) / 2 + 0.15)  # Kernloch Innengewinde

# Gabel / Zunge
FORK_Y0, FORK_Y1 = 150.0, 203.0
TONGUE_T, FORK_PLATE, FORK_H = 8.0, 6.0, 24.0
FORK_BOLTS_Y, FORK_BOLT_Z = (162.0, 185.0), 12.0   # 23 mm Abstand: Rändelköpfe Ø22 wechselseitig

# ================= abgeleitet =================
LENS_Z = BASE_T + PH_H - LENS_DZ
Z_PAD = LENS_Z + FACE_BELOW_PAD
Z_TOP = Z_PAD + 10.0                    # Säulenoberkante = Unterseite Bügelplatte
CHIN_Z, CHIN_Y = Z_PAD - CHIN_DROP, D_STIRN + CHIN_BACK
T_Z, T_Y = CHIN_Z - TONGUE_DROP, CHIN_Y + TONGUE_BACK
Z_PIV = BASE_T + PIVOT_ABOVE_BASE
HG = CHIN_Z - Z_PIV                      # Gelenk -> Muldengrund (lokal)
Y_PIV = CHIN_Y - DISH_OFF - HG * math.sin(math.radians(LEAN))
POST_X = (POST_X0 + POST_X1) / 2
POST_SCREW_Y = POST_Y_REAR - POST_D_TOP / 2
HALO_CY = D_STIRN - PAD_T - 1.0 + IN_AY   # innere Freikontur vorn 1 mm vor der Polster-Rückseite
PH_X1, PH_X0 = LENS_DX, LENS_DX - PH_W
PH_Y_BACK, PH_Y_SCREEN = -BUMP, -BUMP - PH_T
SLOT_HW = LEV_BW / 2 + 1.0
PED_X = CUP_W / 2
PED_Y0, PED_Y1 = Y_PIV - 15.0, 302.0
PED_TOP = T_Z + 8.0
Y_STOP = None   # wird in find_stop() bestimmt

# ================= Helfer =================
def smooth(p, r):
    return p.buffer(r, quad_segs=12).buffer(-2 * r, quad_segs=12).buffer(r, quad_segs=12)

def extrude2d(poly, z0, z1):
    pts = list(poly.exterior.coords)[:-1]
    w = cq.Workplane("XY").workplane(offset=z0).polyline(pts).close().extrude(z1 - z0)
    for hole in poly.interiors:
        hp = list(hole.coords)[:-1]
        w = w.cut(cq.Workplane("XY").workplane(offset=z0 - 1).polyline(hp).close().extrude(z1 - z0 + 2))
    return w

def ellipse(cx, cy, ax, ay):
    return affinity.translate(affinity.scale(Point(0, 0).buffer(1, quad_segs=64), ax, ay), cx, cy)

def box(x0, x1, y0, y1, z0, z1):
    return cq.Workplane("XY").box(x1 - x0, y1 - y0, z1 - z0, centered=False).translate((x0, y0, z0))

def xcyl(r, y, z, x0=-200, x1=200):
    return cq.Workplane("YZ").workplane(offset=x0).center(y, z).circle(r).extrude(x1 - x0)

def dish_cutter(yc, z_bottom):
    """Torus mit Achse X: quer Radius DISH_RX, längs Radius DISH_RY, tiefster Punkt (0, yc, z_bottom)."""
    s = cq.Solid.makeTorus(DISH_RY - DISH_RX, DISH_RX, cq.Vector(0, yc, z_bottom + DISH_RY), cq.Vector(1, 0, 0))
    return cq.Workplane("XY").add(s)

def rrect_sketch(w, h, r, cy=0.0):
    return cq.Sketch().push([(0, cy)]).rect(w, h).reset().vertices().fillet(r)

# ================= 1 Kopfrahmen =================
def post_solid(x0):
    prof = [(POST_Y_REAR - POST_D_BOT, 0), (POST_Y_REAR, 0), (POST_Y_REAR, Z_TOP), (POST_Y_REAR - POST_D_TOP, Z_TOP)]
    w = cq.Workplane("YZ").workplane(offset=x0).polyline(prof).close().extrude(POST_X1 - POST_X0)
    s = w.val()
    edges = [e for e in s.Edges()
             if abs(e.tangentAt(0.5).x) < 0.5 and 8 < e.Center().z < Z_TOP - 1]
    s = s.fillet(3.0, edges)
    w = cq.Workplane("XY").add(s)
    # Fachwerk-Fenster (Warren-Fachwerk): spart Material, Gurte vorn/hinten tragen die Biegung
    t, sw = 5.0, 5.0
    za, zb = 22.0, Z_TOP - 32.0
    yf = lambda z: POST_Y_REAR - POST_D_BOT + (POST_D_BOT - POST_D_TOP) * z / Z_TOP
    inner = Polygon([(yf(za) + t, za), (POST_Y_REAR - t, za), (POST_Y_REAR - t, zb), (yf(zb) + t, zb)])
    n = 4
    zs = [za + (zb - za) * i / n for i in range(n + 1)]
    pts = [((POST_Y_REAR - t) if i % 2 == 0 else (yf(z) + t), z) for i, z in enumerate(zs)]
    from shapely.geometry import LineString
    struts = LineString(pts).buffer(sw / 2, cap_style="flat", join_style="mitre")
    wins = inner.difference(struts)
    wins = [wins] if wins.geom_type == "Polygon" else list(wins.geoms)
    for wpoly in wins:
        if wpoly.area < 30:
            continue
        wpoly = wpoly.buffer(-1.5, quad_segs=8).buffer(1.5, quad_segs=8)
        if wpoly.is_empty:
            continue
        pts2 = list(wpoly.exterior.coords)[:-1]
        cut = cq.Workplane("YZ").workplane(offset=x0 - 1).polyline(pts2).close().extrude(POST_X1 - POST_X0 + 2)
        w = w.cut(cut)
    return w

def fiducial(xc):
    """Fadenkreuz (Passmarke) 0,8 mm tief in die geneigte Säulen-Vorderseite, auf Objektivhöhe."""
    phi = math.atan2(POST_D_BOT - POST_D_TOP, Z_TOP)
    y_face = POST_Y_REAR - POST_D_BOT + (POST_D_BOT - POST_D_TOP) * LENS_Z / Z_TOP
    h = cq.Workplane("XY").box(10, 1.6, 1.2)
    v = cq.Workplane("XY").box(1.2, 1.6, 10)
    c = h.union(v).rotate((0, 0, 0), (1, 0, 0), -math.degrees(phi))
    return c.translate((xc, y_face, LENS_Z))

def find_stop():
    """Rückwand des Schlitzes so legen, dass der um LEAN zurückgelehnte Hebel genau anliegt."""
    lev = place_lever(kinnhebel_local(), -LEAN)
    lo, hi = Y_PIV + 4, Y_PIV + 20
    for _ in range(18):
        mid = (lo + hi) / 2
        ped = kinnsockel(mid)
        try:
            v = lev.intersect(ped).val().Volume()
        except Exception:
            v = 0.0
        if v > 0.02:
            lo = mid
        else:
            hi = mid
    return hi + 0.05

def kinnsockel(y_stop):
    foot = smooth(sbox(-PED_X, PED_Y0, PED_X, PED_Y1), 6)
    p = extrude2d(foot, 0, PED_TOP)
    p = p.cut(dish_cutter(T_Y, T_Z))
    # Schlitz für den Hebel (vorn offen), Rückwand = Anschlag
    p = p.cut(box(-SLOT_HW, SLOT_HW, PED_Y0 - 5, y_stop, 9.0, 200))
    # Freistich hinter dem Gelenk: untere Hebelecke schwenkt beim Klappen nach hinten
    p = p.cut(box(-SLOT_HW, SLOT_HW, y_stop - 1, y_stop + 7, 9.0, Z_PIV + 8.0))
    # 45°-Fase an der Vorderkante oben (Klappweg der Schale + Optik)
    c = 6.0
    wedge = (cq.Workplane("YZ").workplane(offset=-PED_X - 1)
             .polyline([(PED_Y0 - 1, PED_TOP + 1), (PED_Y0 - 1, PED_TOP - c - 1), (PED_Y0 + c + 1, PED_TOP + 1)])
             .close().extrude(2 * PED_X + 2))
    return p.cut(wedge)

def kopfrahmen(y_stop):
    base2d = unary_union([
        sbox(-107, 274, 107, POST_Y_REAR),
        sbox(POST_X0 - 8, 256, POST_X1 + 8, 318),
        sbox(-(POST_X1 + 8), 256, -(POST_X0 - 8), 318),
        sbox(-20, FORK_Y1 - 4, 20, 276),
        sbox(-PED_X, PED_Y0, PED_X, PED_Y1),
    ])
    f = extrude2d(smooth(base2d, 5), 0, BASE_T)
    for x0 in (POST_X0, -POST_X1):
        f = f.union(post_solid(x0))
    f = f.union(kinnsockel(y_stop))
    # Gabel
    for sx in (1, -1):
        x0 = TONGUE_T / 2 + FIT if sx > 0 else -(TONGUE_T / 2 + FIT + FORK_PLATE)
        plate = box(x0, x0 + FORK_PLATE, FORK_Y0, FORK_Y1 + 2, 0, FORK_H).edges("|Z").fillet(2.0)
        f = f.union(plate)
    for yb in FORK_BOLTS_Y:
        f = f.cut(xcyl(HOLE10 / 2, yb, FORK_BOLT_Z, -20, 20))
    # Gelenkbohrung
    f = f.cut(xcyl(PIVOT_HOLE / 2, Y_PIV, Z_PIV, -60, 60))
    # Gewinde-Vorbohrungen oben in den Säulen
    for sx in (1, -1):
        f = f.cut(cq.Workplane("XY").workplane(offset=Z_TOP - 18).center(sx * POST_X, POST_SCREW_Y)
                  .circle(TAP10 / 2).extrude(25))   # Gewindegänge werden im Netz geschnitten
        f = f.cut(fiducial(sx * POST_X))
    return f

# ================= 2 Kopfbügel =================
def kopfbuegel():
    inner = ellipse(0, HALO_CY, IN_AX, IN_AY)
    outer = ellipse(0, HALO_CY, OUT_AX, OUT_AY)
    lugs = [Point(sx * POST_X, POST_SCREW_Y).buffer(14, quad_segs=24) for sx in (1, -1)]
    plate = unary_union([outer] + lugs).difference(inner)
    plate = plate.intersection(sbox(-300, 0, 300, HALO_END_Y))
    plate = smooth(plate, 4)
    z0, z1 = Z_TOP, Z_TOP + HALO_T
    h = extrude2d(plate, z0, z1)
    # Stirnpolster: Kreisring-Segment, Kontaktfläche genau bei Y = D_STIRN
    c = (0, D_STIRN + PAD_R)
    ring = Point(c).buffer(PAD_R + PAD_T + 4, quad_segs=64).difference(Point(c).buffer(PAD_R, quad_segs=64))
    pad2d = smooth(ring.intersection(sbox(-PAD_W / 2, 0, PAD_W / 2, D_STIRN + 30)), 1.5)
    pad = extrude2d(pad2d, z0 - PAD_DOWN, z1)
    pad = pad.cut(box(-100, 100, 0, D_STIRN - PAD_T, z0 - PAD_DOWN - 1, z0 - 0.001))  # unter der Platte nur PAD_T stark
    h = h.union(pad)
    # Schläfenpolster: federnde Laschen
    for sx in (1, -1):
        xi = TEMPLE_GAP / 2
        tab2d = smooth(sbox(xi, TEMPLE_Y0, xi + TEMPLE_T, TEMPLE_Y1), 1.0)
        web2d = smooth(sbox(xi, TEMPLE_Y0, xi + 16, TEMPLE_Y1), 1.0)
        if sx < 0:
            tab2d = affinity.scale(tab2d, -1, 1, origin=(0, 0))
            web2d = affinity.scale(web2d, -1, 1, origin=(0, 0))
        h = h.union(extrude2d(tab2d, z0 - PAD_DOWN, z1)).union(extrude2d(web2d, z0, z1))
    # Schraublöcher + Flachsenkung oben
    for sx in (1, -1):
        h = h.cut(cq.Workplane("XY").workplane(offset=z0 - 1).center(sx * POST_X, POST_SCREW_Y).circle(HOLE10 / 2).extrude(10))
    return h

# ================= 3 Kinnhebel (lokal: Gelenkachse = X-Achse durch Ursprung, aufrecht) =================
def kinnhebel_local():
    cup_bot = HG - CUP_FLOOR
    cup_top = HG + CUP_RIM
    cy = (CUP_BACK - CUP_FRONT) / 2
    L1 = cup_bot - 6.0
    L0 = L1 - 12.0
    bar = box(-LEV_BW / 2, LEV_BW / 2, -LEV_BT / 2, LEV_BT / 2, LEV_BOT, L0 + 0.01)
    # 45°-Fasen an den unteren Kanten: kleinerer Schwenkradius, druckbar
    for sy in (1, -1):
        yb = sy * LEV_BT / 2
        tri = (cq.Workplane("YZ").workplane(offset=-LEV_BW / 2 - 1)
               .polyline([(yb + sy * 0.01, LEV_BOT - 0.01), (yb - sy * 5, LEV_BOT - 0.01), (yb + sy * 0.01, LEV_BOT + 5)])
               .close().extrude(LEV_BW + 2))
        bar = bar.cut(tri)
    s0 = rrect_sketch(LEV_BW, LEV_BT, 1.0)
    s1 = rrect_sketch(CUP_W - 12, CUP_FRONT + CUP_BACK - 12, 4, cy)
    s2 = rrect_sketch(CUP_W, CUP_FRONT + CUP_BACK, 7, cy)
    loft1 = cq.Workplane("XY").placeSketch(s0.moved(cq.Location(cq.Vector(0, 0, L0))),
                                           s1.moved(cq.Location(cq.Vector(0, 0, L1)))).loft()
    loft2 = cq.Workplane("XY").placeSketch(s1.moved(cq.Location(cq.Vector(0, 0, L1))),
                                           s2.moved(cq.Location(cq.Vector(0, 0, cup_bot)))).loft()
    cup = cq.Workplane("XY").workplane(offset=cup_bot).placeSketch(s2).extrude(cup_top - cup_bot)
    lev = bar.union(loft1).union(loft2).union(cup)
    lev = lev.cut(dish_cutter(DISH_OFF, HG))
    lev = lev.cut(xcyl(HOLE10 / 2, 0, 0, -50, 50))
    return lev

def place_lever(lev, angle):
    """angle um +X: -LEAN = Ruhelage (zurückgelehnt), positiv = nach vorn geklappt."""
    return lev.rotate((0, 0, 0), (1, 0, 0), angle).translate((0, Y_PIV, Z_PIV))

# ================= 4 Träger mit Wiege =================
def traeger():
    foot = extrude2d(smooth(sbox(-80, -25, 30, 25), 6), 0, BASE_T)
    cx0, cx1 = PH_X0 - 0.5, PH_X1 + 0.5
    cy0, cy1 = PH_Y_SCREEN - CLR, PH_Y_BACK + CLR
    outer = sbox(cx0 - WALL, cy0 - WALL, cx1 + WALL, cy1 + WALL)
    cav = sbox(cx0, cy0, cx1, cy1)
    mouth = sbox(cx0 + 4.5, cy1 - 1, cx1 - 4.5, cy1 + WALL + 1)
    u = smooth(outer.difference(cav).difference(mouth), 0.8)
    cradle = extrude2d(u, BASE_T - 0.01, BASE_T + 60)
    flange = extrude2d(smooth(sbox(-18, 20, 18, FORK_Y0 - 3), 4), 0, 5)
    y_rib0 = cy1 + WALL
    y_t1 = FORK_Y1 - 4.5   # Zungenende vor dem Rückgrat der Gabel
    rib_prof = [(y_rib0, 0), (y_rib0, 12), (150, FORK_H), (y_t1, FORK_H), (y_t1, 0)]
    rib = (cq.Workplane("YZ").workplane(offset=-TONGUE_T / 2).polyline(rib_prof).close().extrude(TONGUE_T))
    t = foot.union(cradle).union(flange).union(rib)
    for yb in FORK_BOLTS_Y:
        t = t.cut(xcyl(HOLE10 / 2, yb, FORK_BOLT_Z, -10, 10))
    return t

def dummy_phone():
    p = box(PH_X0, PH_X1, PH_Y_SCREEN, PH_Y_BACK, BASE_T, BASE_T + PH_H)
    bump = box(PH_X1 - 4 - 36.7, PH_X1 - 4, PH_Y_BACK, 0, BASE_T + PH_H - 4 - 38.2, BASE_T + PH_H - 4)
    return p.union(bump)
