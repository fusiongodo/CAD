# Development-Wand (Variante A „Senkrechte Wand“, 23.11.2026): Maße, Lage und Lochraster für teile/wand_*.py.
#
# Vorlage: ihht-pi CAD/produktkasten-skizzen/development_wand_3d.html, Funktion A(). Mit Alex beschlossen (Stabilität):
#   Grundplatte als L statt 2 Füße, je Ende 2 Knotenbleche (vorn + hinten), Wand 6 dick mit Randsteg hinten,
#   Wand als M3-Lochraster 10 mm, QV-Ausschnitte für Hohlrad und Getriebepfosten. Bambu 256 x 256, alles ohne Stützen.
#
# Anordnung v2 (23.11.2026, Alex): Fluss von außen nach innen und von oben nach unten, kein Schlauch kreuzt.
#   Gas kommt außen an (hi links, lo rechts) und läuft waagerecht durch das QV zur Mitte. Oberer QV-Schlauch = Ablass:
#   oben mittig durch 2 Durchlässe Ø 14 nach hinten. Unterer QV-Schlauch -> OCS hi / OCS lo hochkant in der Mitte
#   (Tüllen nach vorn, oben Eingang auf Höhe des unteren QV-Schlauchs) -> Y-Stück darunter -> OCS Mix waagerecht
#   unten (Eingang links vom Y, Ausgang rechts zum Patienten). Dafür QV nach außen (X ±73) und Wand 246 hoch.
#
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
BY0, BY1 = -70.0, 80.0          # Grundplatte Tiefe (150)
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
TUBE_X = 37.5                   # Schlauch verlässt das QV am Amboss-Ende (QX - 35,5)
Z_ABLASS = QZ + TUBE_DY         # 222,5 oberer Schlauch
Z_ZUFUHR = QZ - TUBE_DY         # 173,5 unterer Schlauch -> OCS
ABLASS = [(-10.0, Z_ABLASS), (10.0, Z_ABLASS)]          # 2 Durchlässe nach hinten (je ein Schlauch 6/12)
ABLASS_D = 14.0

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

# ---------------- Modulhalter (ein Teil, 4 x): L-Winkel, Flansch an der Wand, Finne quer, Kabelbinder ----------------
# Lokal: u entlang der Finne (Länge), v quer über den Flansch ab der Finnen-Außenseite, w nach vorn ab der Wand.
H_L, H_FW, H_FT = 60.0, 16.5, 4.0       # Länge, Flanschbreite, Flanschdicke
H_FIN_T, H_FD = 3.0, 30.0               # Finne: Dicke, Tiefe
H_HOLE_U, H_HOLE_V = (15.0, 45.0), 12.0 # 2 x M3 x 12 + Mutter, Abstand 30 = Raster
H_SLOT_U = (8.0, 22.0, 38.0, 52.0)      # Kabelbinder 2,5 (je Modul 2 davon nutzen, nicht auf Höhe einer Tülle)
H_SLOT_W = (4.5, 6.5)                   # Schlitz zwischen Flansch und Modul
H_MOD_W = 7.0                           # Modul-Rückseite ab Wand (Platz für die Schraubenköpfe)
# (Name, Art, Finnen-Außenseite, Richtung, Start entlang u): 'seite' = Finne senkrecht, 'boden' = Finne waagerecht
HALTER = [('Hi', 'seite', -27.0, 1, 81.0),         # OCS hi links neben der Finne, Löcher (-15, 96 / 126)
          ('Lo', 'seite', 27.0, -1, 111.0),        # OCS lo, Löcher (15, 126 / 156)
          ('Y', 'boden', 44.0, 1, -30.0),          # Y-Stück liegt auf der Finne, Löcher (±15, 56)
          ('Mix', 'boden', 14.0, 1, -30.0)]        # OCS Mix liegt auf der Finne, Löcher (±15, 26)

# ---------------- Module vorn (Bauraum aus Materialliste/ocs-sensoren-masse.md) ----------------
# OCS hochkant, Platine quer zur Wand, Tüllen nach vorn. Bauraum je (x0, x1, y0, y1, z0, z1).
Y_MOD = (-H_MOD_W - 30.0, -H_MOD_W)                     # Tiefe 30 (Platine 22/24 + Tüllen)
OCS = {'hi': (-24.0, -1.0) + Y_MOD + (Z_ZUFUHR - 32.5 - 61.0, Z_ZUFUHR - 32.5 + 61.0),   # 3F: 122 lang, Tüllen +32,5 / -35,8
       'lo': (1.0, 24.0) + Y_MOD + (Z_ZUFUHR - 20.5 - 41.0, Z_ZUFUHR - 20.5 + 41.0),     # 3FRL2.0: 82 lang, Tüllen +20,5 / -18
       'mix': (-41.0, 41.0) + Y_MOD + (17.0, 40.0)}                                      # waagerecht auf dem Halter
TUELLEN = {'hi': (Z_ZUFUHR, Z_ZUFUHR - 68.3), 'lo': (Z_ZUFUHR, Z_ZUFUHR - 38.5),     # Eingang oben, Ausgang unten (Z)
           'mix': (-18.0, 20.5)}                                                     # Eingang links, Ausgang rechts (X)
YSTUECK = (-15.0, 15.0, -H_MOD_W - 15.0, -H_MOD_W, 47.0, 77.0)                       # Dummy 30 x 30 x 15

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


def halter_box(art, a, s, u0, u, v, w):
    """Lokaler Quader (u0..u1, v0..v1, w0..w1) eines Modulhalters -> Builder-Quader-Grenzen (x0, x1, y0, y1, z0, z1)."""
    vv = sorted((a + s * v[0], a + s * v[1]))
    uu = (u0 + u[0], u0 + u[1])
    ww = (-w[1], -w[0])
    return (vv + list(ww) + list(uu)) if art == 'seite' else (list(uu) + list(ww) + vv)


def halter_punkt(art, a, s, u0, u, v):
    """Lokaler Punkt (u, v) auf der Wand -> (X, Z)."""
    return (a + s * v, u0 + u) if art == 'seite' else (u0 + u, a + s * v)


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
