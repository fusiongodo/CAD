# Development-Wand (Variante A „Senkrechte Wand“, 23.11.2026): Maße, Lage und Lochraster für teile/wand_*.py.
#
# Vorlage: ihht-pi CAD/produktkasten-skizzen/development_wand_3d.html, Funktion A(). Mit Alex beschlossen (Stabilität):
#   Grundplatte als L statt 2 Füße, je Ende 2 Knotenbleche (vorn + hinten), Wand 6 dick mit Randsteg hinten,
#   Wand als M3-Lochraster 10 mm, QV-Ausschnitte für Hohlrad und Getriebepfosten. Bambu 256 x 256, alles ohne Stützen.
#
# Anordnung v2 (23.11.2026, Alex): Fluss von außen nach innen und von oben nach unten, kein Schlauch kreuzt.
#   Gas kommt außen an (hi links, lo rechts) und läuft waagerecht durch das QV zur Mitte. Oberer QV-Schlauch = Ablass:
#   oben mittig durch 2 Durchlässe nach hinten. QV nach außen (X ±73), Wand 246 hoch.
# v2.1 (24.11.2026, Alex: Knickgefahr): OCS hi / lo stehen hochkant parallel zur Wand, Tüllen seitlich zum QV
#   -> der untere QV-Schlauch läuft gerade auf den Eingang. Ausgänge mit U-Bogen (R ≥ 25) zu einem T-Stück in der Mitte,
#   T-Ausgang gerade nach unten auf OCS Mix. OCS Mix steht quer zur Wand auf der Grundplatte (Tüllen oben), sein
#   Ausgang liegt 38,5 vor dem Eingang -> Patientenschlauch geht vor allen anderen Schläuchen nach vorn weg.
#   Alle Schlauchachsen in einer Ebene Y -22 (Schlauchmitte im QV); Schläuche als Dummies mit Biegeradius.

# Builder-Koordinaten: X quer, Y Tiefe (vorn = -Y), Z oben. Wand-Vorderseite Y = 0, Oberseite Grundplatte Z = 6.
#
# QV-Aufnahme (gilt für Rahmen v2 + Getriebe v1 und für QV v3, beide haben dieselbe Rückplatte 89 x 83,3 mit
# Amboss-Schrauben x ±26 / y ±33,5 und Getriebepfosten Ø 9 bei x ±40 / y ±15, Hohlrad Ø 73):
#   Die Rückplatte des QV liegt vorn flach an der Wand. Hohlrad und Pfosten stecken im Ausschnitt, Getriebeplatte und
#   NEMA sitzen hinter der Wand. Gehalten wird das QV von 4 x M3 x 20 von hinten durch die Wand in die Ambosse
#   (ersetzen die 4 äußeren M3 x 12 der Ambosse). Getriebe bleibt abnehmbar (Pfosten-Schrauben von vorn).
import math

# ---------------- Grundmaße ----------------
BT = 6.0                        # Grundplatte Z 0 .. 6
BY0, BY1 = -80.0, 80.0          # Grundplatte Tiefe (160, vorn trägt sie OCS Mix)
WX = 123.0                      # halbe Breite Wand und Grundplatte (246: mit Brim 5 genau 256)
WZ0, WZ1 = BT, BT + 246.0       # Wand Z 6 .. 252
WT = 6.0                        # Wanddicke, Y 0 .. 6
RW, RD = 4.0, 10.0              # Randsteg: Breite, Tiefe (hinten, Y 6 .. 16)
RB = 12.0                       # Randsteg unten höher (Z 6 .. 18): nimmt die Schrauben aus der Grundplatte
RIB_Z = (140.0, 143.0)          # Quersteg hinten zwischen QV-Feld und Elektronikfeld
YR = WT + RD                    # Rückseite der Stege 16

# ---------------- QV ----------------
QX, QZ = 73.0, 198.0            # Achsen bei X ±73 (Pfosten außen bis 117,5, Seitensteg ab 119), Z 198
QV_CUT_R = 37.5                 # Hohlrad Ø 73 + 1 Luft radial
QV_CUT_BOX = (45.5, 20.5)       # Pfosten Ø 9 bei x ±40 / y ±15 + 1 Luft
QV_SCREWS = [(sx * 26.0, sy * 33.5) for sx in (-1, 1) for sy in (-1, 1)]   # M3 x 20 in die Ambosse
QV_RING = (47.0, 49.5)          # Ringsteg hinten um den Ausschnitt (frei von Schraubenköpfen und Getriebeplatte, innerhalb der Randstege)
TUBE_DY = 24.5                  # Schlauchmitte über/unter der Achse (Amboss 28,5, Rolle Ø 40..42)
TUBE_Y = -22.0                  # Schlauchmitte vor der Wand (Schlauchmitte z 15 + Rückplatte 6 bzw. 8)
TUBE_X = QX - 35.5              # Schlauch verlässt das QV am Amboss-Ende (X ±37,5)
Z_ABLASS = QZ + TUBE_DY         # 222,5 oberer Schlauch
Z_ZUFUHR = QZ - TUBE_DY         # 173,5 unterer Schlauch -> OCS
ABLASS = [(-13.5, Z_ABLASS), (13.5, Z_ABLASS)]          # 2 Durchlässe nach hinten (je ein Schlauch 6/12, Bogen R 25)
ABLASS_D = 18.0

# ---------------- Lochraster ----------------
PITCH = 10.0
GX = [5.0 + PITCH * k for k in range(-11, 11)]          # X ±5 .. ±105 (±115 liegt im Seitensteg)
GZ = [26.0 + PITCH * k for k in range(22)]              # Z 26 .. 236
M3_CLR, M3_SELF, HEAD_D, HEAD_T = 3.4, 2.6, 6.5, 3.2
M25_SELF = 2.2

# ---------------- Knotenbleche ----------------
KX0, KX1 = 100.5, 118.5         # |X|: Flansche 18 breit, 0,5 Luft zum Seitensteg
KW = 5.0                        # Steg (außen) und Flansche
KL, KH = 70.0, 81.0             # Länge auf der Grundplatte ab Wandfläche, Höhe an der Wand (Z)
KN = (RD + 0.5, BT + RB + 0.5)  # Aussparung für den unteren Randsteg (hinten), d 0 .. 10,5 / Z 6 .. 18,5
K_HOLE_X = 105.0                # Rasterspalte
K_HOLE_Z = (36.0, 66.0)         # Rasterzeilen: Wand-Flansch, gemeinsam für vorn + hinten (M3 x 20)
K_HOLE_D = (30.0, 60.0)         # Abstand ab Wandfläche: Grundplatten-Flansch (M3 x 12 von unten + Mutter)
FACE = {'v': 0.0, 'h': WT}      # Anlagefläche vorn / hinten

# ---------------- Wand auf Grundplatte ----------------
FUSS_X = (-95.0, -55.0, -15.0, 15.0, 55.0, 95.0)       # M3 x 12 von unten in den unteren Randsteg
FUSS_Y = 8.0
FUSS_TIEF = 10.0

# ---------------- Träger Pi / Pico (Pi-Lochbild 58 x 49, laut Händler auch SKR Pico) ----------------
TR_W, TR_H, TR_T = 72.0, 60.0, 3.0
TR_BOSS = [(sx * 29.0, sz * 24.5) for sx in (-1, 1) for sz in (-1, 1)]
TR_BOSS_D, TR_BOSS_H = 6.0, 8.0
TR_MOUNT = [(sx * 20.0, sz * 20.0) for sx in (-1, 1) for sz in (-1, 1)]   # auf Rasterpunkte, M3 x 12 + Mutter
TR_POS = {'Pi': (-55.0, 56.0), 'Pico': (55.0, 56.0)}

# ---------------- Schlauchebene, OCS, T-Stück, OCS Mix ----------------
YS = -22.0                      # alle Schlauch- und Tüllenachsen
SCHLAUCH_D = 12.0               # Schlauch 6/12
R_MIN = 25.0                    # kleinster Biegeradius (ca. 2 x Außen-Ø)
# OCS hochkant, Platine parallel zur Wand, Rohrseite zur Wand (Rohrmitte ≈ 8 vor der Rohr-Oberkante), Bauteile vorn.
OCS_Y = (-37.0, -14.0)
TUELLE_D = 6.4                  # Tüllenfuß
OCS_HI = dict(x=(-24.0, -1.0), z=(Z_ZUFUHR - 32.5 - 56.25, Z_ZUFUHR - 32.5 + 56.25),   # 3F: Platine 112,5 x 22
              tz=(Z_ZUFUHR, Z_ZUFUHR - 68.3), tx=(-24.0, -31.5))                        # Tüllen nach links, 7,5 lang
OCS_LO = dict(x=(1.0, 25.0), z=(Z_ZUFUHR + 21.5 - 79.0, Z_ZUFUHR + 21.5),               # 3FRL2.0: 79 x 24, Tüllen 21,5 / 60 vom Ende
              tz=(Z_ZUFUHR, Z_ZUFUHR - 38.5), tx=(25.0, 30.0))                          # Tüllen nach rechts, 5 lang
T_Z = 53.0                      # T-Stück 6 mm: Arme ±18 (Ø 8), Körper Ø 10, Abgang 15 nach unten
T_ARM, T_STEM = 18.0, 15.0
MIX = dict(x=(-11.5, 11.5), y=(-79.5, -0.5), z=(BT, BT + 24.0), ty=(YS, YS - 38.5), tip=BT + 29.0)   # quer zur Wand, Tüllen oben

# ---------------- Halter ----------------
# Rückenhalter (OCS hi / lo): Flansch 24 x 60 an der Wand, 2 Finnen an den Rändern als Abstand (OCS liegt bei Y -14 an),
# 4 Schlitze für Kabelbinder 2,5 x 150 quer durch beide Finnen. 2 x M3 x 16 + Mutter auf dem Raster (Mitte, ±15).
H_L, H_W, H_FT, H_FIN_T, H_STAND = 60.0, 24.0, 4.0, 3.0, 14.0
H_HOLE_U = (15.0, 45.0)
H_SLOT_U = (8.0, 22.0, 38.0, 52.0)
H_SLOT_W = (7.0, 9.0)
RUECKEN = [('Hi', -15.0, 111.0), ('Lo', 15.0, 111.0)]      # Name, Mitte X, Unterkante Z -> Löcher (±15, 126 / 156)
# T-Halter: Flansch 40 x 24, 2 waagerechte Finnen (4 dick, 30 tief) über und unter dem T-Körper, unten Gabel für den Abgang.
TH_X, TH_Z, TH_FIN_X, TH_FIN_T, TH_D = (-20.0, 20.0), (44.0, 68.0), (-9.0, 9.0), 4.0, 30.0
TH_HOLES = [(-15.0, 56.0), (15.0, 56.0)]
TH_SLOT_X, TH_SLOT_W = (-6.5, 6.5), (9.0, 11.0)
# OCS Mix: Kabelbinder durch Schlitze in der Grundplatte, Nut auf der Unterseite.
MIX_BAND_Y = (-40.0, -72.0)

BEIGE = (0.87, 0.81, 0.69, 1.0)
HELL = (0.78, 0.8, 0.82, 1.0)
GRAU = (0.3, 0.3, 0.32, 1.0)
GRUEN = (0.1, 0.45, 0.2, 1.0)
BLAU = (0.55, 0.75, 0.92, 1.0)


def grid_points():
    """Rasterpunkte, deren Bohrung frei bleibt von QV-Ausschnitten (2 Rand), Stegen hinten (3,3 für die Mutter),
    QV-Schrauben (4,5) und Ablass-Durchlässen (2)."""
    r = M3_CLR / 2
    bx, bz = QV_CUT_BOX
    out = []
    for z in GZ:
        for x in GX:
            frei = WZ0 + RB + 3.3 + r <= z <= WZ1 - RW - 3.3 - r and abs(x) <= WX - RW - 3.3 - r
            frei &= not (RIB_Z[0] - 3.3 - r < z < RIB_Z[1] + 3.3 + r)
            for cx in (-QX, QX):
                d = math.hypot(x - cx, z - QZ)
                frei &= d >= QV_CUT_R + 2 + r and not (QV_RING[0] - 3.3 - r < d < QV_RING[1] + 3.3 + r)
                frei &= not (abs(x - cx) < bx + 2 + r and abs(z - QZ) < bz + 2 + r)
                frei &= all(math.hypot(x - cx - sx, z - QZ - sz) >= 4.5 + r for sx, sz in QV_SCREWS)
            frei &= all(math.hypot(x - ax, z - az) >= ABLASS_D / 2 + 2 + r for ax, az in ABLASS)
            if frei:
                out.append((x, z))
    return out


def _bogen(c, r, a0, a1, n=8):
    """Kreisbogen in der XZ-Ebene (Y = YS): Mittelpunkt c = (x, z), Winkel a0 -> a1 in Grad."""
    return [(c[0] + r * math.cos(math.radians(a0 + (a1 - a0) * k / n)), YS,
             c[1] + r * math.sin(math.radians(a0 + (a1 - a0) * k / n))) for k in range(n + 1)]


def schlaeuche():
    """Schlauch-Mittellinien als Punktlisten (X, Y, Z) und kleinster Biegeradius je Strecke."""
    hz0, hz1 = OCS_HI['tz']
    lz0, lz1 = OCS_LO['tz']
    hx, lx = OCS_HI['tx'][1], OCS_LO['tx'][1]
    rh = (hz1 - T_Z) / 2                                                  # U-Bogen hi
    out = {
        'ZuHi': ([(-TUBE_X + 0.5, YS, Z_ZUFUHR), (hx, YS, Z_ZUFUHR)], None),
        'ZuLo': ([(lx, YS, Z_ZUFUHR), (TUBE_X - 0.5, YS, Z_ZUFUHR)], None),
        'HiT': ([(hx, YS, hz1)] + _bogen((hx - 2, T_Z + rh), rh, 90, 270, 12) + [(-T_ARM, YS, T_Z)], rh),   # 2 gerade ab Tülle
        'LoT': ([(lx, YS, lz1)] + _bogen((lx + 2, lz1 - R_MIN), R_MIN, 90, 0) + _bogen((lx + 2, T_Z + R_MIN), R_MIN, 0, -90)
                + [(T_ARM, YS, T_Z)], R_MIN),
        'TMix': ([(0.0, YS, T_Z - T_STEM), (0.0, YS, MIX['tip'])], None),
    }
    for sx, n in ((-1, 'AbHi'), (1, 'AbLo')):                             # Ablass: Bogen in XY bis an die Wand
        cx, cy, r = sx * (TUBE_X - 0.5), YS + R_MIN, R_MIN
        f1 = math.acos((cy + 0.5) / r)                                    # bis 0,5 vor die Wand (dann im Durchlass)
        pts = []
        for k in range(9):
            f = f1 * k / 8
            pts.append((cx - sx * r * math.sin(f), cy - r * math.cos(f), Z_ABLASS))
        out[n] = (pts, r)
    y0, z0 = MIX['ty'][1], MIX['tip']                                     # Patient: hoch, dann nach vorn
    out['Patient'] = ([(0.0, y0, z0), (0.0, y0, z0 + 10)] +
                      [(0.0, y0 - R_MIN + R_MIN * math.cos(math.radians(90 * k / 8)), z0 + 10 + R_MIN * math.sin(math.radians(90 * k / 8)))
                       for k in range(1, 9)] + [(0.0, y0 - R_MIN - 15, z0 + 10 + R_MIN)], R_MIN)
    return out


def kb_profile(seite):
    """Knotenblech-Profil (Y, Z) als Liste (Steg, Wand-Flansch, Grundplatten-Flansch). seite 'v' oder 'h'."""
    sy = -1 if seite == 'v' else 1
    y = lambda d: FACE[seite] + sy * d
    dn, zn = KN
    steg = [(0, zn), (dn, zn), (dn, BT), (KL, BT), (KL, BT + KW), (KW, KH), (0, KH)]
    steg = [(y(d), z) for d, z in steg]
    if sy < 0:
        steg = steg[::-1]
    return steg, (y(0), y(KW), zn, KH), (y(dn), y(KL), BT, BT + KW)
