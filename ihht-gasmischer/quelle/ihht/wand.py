# Development-Wand (Variante A „Senkrechte Wand“, 23.11.2026): Maße, Lage und Lochraster für teile/wand_*.py.
#
# Vorlage: ihht-pi CAD/produktkasten-skizzen/development_wand_3d.html, Funktion A(). Mit Alex beschlossen (Stabilität):
#   Grundplatte als L statt 2 Füße, je Ende 2 Knotenbleche (vorn + hinten), Wand 6 dick mit Randsteg hinten,
#   Wand als M3-Lochraster 10 mm, QV-Ausschnitte für Hohlrad und Getriebepfosten. Bambu 256 x 256, alles ohne Stützen.
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
WX = 120.0                      # halbe Breite Wand und Grundplatte (240)
WZ0, WZ1 = BT, BT + 210.0       # Wand Z 6 .. 216
WT = 6.0                        # Wanddicke, Y 0 .. 6
RW, RD = 4.0, 10.0              # Randsteg: Breite, Tiefe (hinten, Y 6 .. 16)
RB = 12.0                       # Randsteg unten höher (Z 6 .. 18): nimmt die Schrauben aus der Grundplatte
RIB_Z = (100.0, 103.0)          # Quersteg hinten zwischen QV-Feld und Elektronikfeld
YR = WT + RD                    # Rückseite der Stege 16

# ---------------- QV ----------------
QX, QZ = 58.0, 165.0            # Achsen bei X ±58, Z 165 (Vorlage: x ±58, Mitte y ≈ 165)
QV_CUT_R = 37.5                 # Hohlrad Ø 73 + 1 Luft radial
QV_CUT_BOX = (45.5, 20.5)       # Pfosten Ø 9 bei x ±40 / y ±15 + 1 Luft
QV_SCREWS = [(sx * 26.0, sy * 33.5) for sx in (-1, 1) for sy in (-1, 1)]   # M3 x 20 in die Ambosse
QV_RING = (47.0, 50.0)          # Ringsteg hinten um den Ausschnitt (frei von Schraubenköpfen und Getriebeplatte)

# ---------------- Lochraster ----------------
PITCH = 10.0
GX = [5.0 + PITCH * k for k in range(-11, 11)]          # X ±5 .. ±105
GZ = [26.0 + PITCH * k for k in range(19)]              # Z 26 .. 206
M3_CLR, M3_SELF, HEAD_D, HEAD_T = 3.4, 2.6, 6.5, 3.2
M25_SELF = 2.2

# ---------------- Knotenbleche ----------------
KX0, KX1 = 98.5, 115.5          # |X|: Flansche 17 breit, 0,5 Luft zum Seitenrand
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

# ---------------- Module vorn (Bauraum aus Materialliste/ocs-sensoren-masse.md) ----------------
OCS = {'hi': (-108.0, 14.0, 85.0, 116.0, 23.0),        # OCS-3F 122 x 31 x 23
       'lo': (24.0, 106.0, 85.0, 115.0, 23.0),         # OCS-3FRL2.0 82 x 30 x 23
       'mix': (-30.0, 52.0, 30.0, 60.0, 23.0)}
MISCHER = (-60.0, -40.0, 30.0, 60.0, 20.0)

BEIGE = (0.87, 0.81, 0.69, 1.0)
HELL = (0.78, 0.8, 0.82, 1.0)
GRAU = (0.3, 0.3, 0.32, 1.0)
GRUEN = (0.1, 0.45, 0.2, 1.0)
BLAU = (0.55, 0.75, 0.92, 1.0)


def grid_points():
    """Rasterpunkte, deren Bohrung frei bleibt von QV-Ausschnitten (2 Rand), Stegen hinten (3,3 für die Mutter)
    und QV-Schrauben (4,5)."""
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
            if frei:
                out.append((x, z))
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
