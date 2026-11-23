# Exzenter-Quetschventil v3 (23.11.2026): Maße, Profile und Koordinaten-Helfer für teile/qv_*.py.
#
# Aufbau: M8-Schraube (DIN 933 M8 x 70) als Stahlkern durch den ganzen Rotor. Kopf im Sechskant des Hohlrads
# (nimmt das Drehmoment), Mutter gefangen in der Klemmhülse vorn am Exzenter. 608 hinten in der Rückwand,
# 608 vorn im Steg: die Schlauchlast liegt zwischen den Lagern. 2 x 6905 (25 x 42 x 9) gestapelt auf dem
# Exzenterzapfen (e 4,5), zwischen Wellenschulter und Klemmhülse über die M8 verspannt. Ambosse wie Rahmen v2.
#
# Die Teile sind in den Ventil-Koordinaten der alten Generatoren (ihht-pi CAD/quetschventil) beschrieben:
#   z = Hauptachse (vorn +), y = Richtung zu den Ambossen (Amboss oben +y), x = Schlauchrichtung.
#   Rückwand z -8 .. 0, Ambosse z 0 .. 34, Steg z 34 .. 42, Getriebe hinter der Rückwand, Motor bei y -26.
# Die Helfer Q* drehen das in die Builder-Koordinaten (X = x, Y = -z, Z = y): Achse läuft in Y, vorn = -Y.
import math
from ihht.geo import B, C

BEIGE = (0.87, 0.81, 0.69, 1.0)
ORANGE = (0.93, 0.55, 0.2, 1.0)
GRAU = (0.3, 0.3, 0.32, 1.0)
STAHL = (0.65, 0.67, 0.7, 1.0)


def _m(p):
    return (p[0], -p[2], p[1])


def QB(x0, x1, y0, y1, z0, z1):                       # Quader in Ventil-Koordinaten
    return B(x0, x1, -z0, -z1, y0, y1)


def QC(p0, p1, d):
    return C(_m(p0), _m(p1), d)


def QCz(x, y, z0, z1, d):                             # Zylinder entlang der Hauptachse
    return QC((x, y, z0), (x, y, z1), d)


def QK(x, y, z0, z1, r0, r1):                         # Kegelstumpf entlang der Hauptachse: r0 bei z0, r1 bei z1
    return ('cone', _m((x, y, z0)), _m((x, y, z1)), r0, r1)


def QP(x, y, z0, z1, pts):                            # Polygon (dx, dy) um (x, y), von z0 bis z1 extrudiert
    z0, z1 = min(z0, z1), max(z0, z1)
    return ('prism', (x, y), tuple((round(a, 4), round(b, 4)) for a, b in pts), -z1, -z0)


# ---------------- Getriebe 5 : 1 (wie Getriebe v1, nur 2,2 mm weiter hinten) ----------------
MOD, ZP, ZR = 1.0, 13, 65
A = MOD * (ZR - ZP) / 2                 # Achsabstand 26, Motor bei y -26
BACKLASH = 0.3
RP_TIP, RP_ROOT = ZP * MOD / 2 + 0.9, ZP * MOD / 2 - 1.25
RR_TIP, RR_ROOT = ZR * MOD / 2 - 0.65, ZR * MOD / 2 + 1.25      # Kopfkreis Hohlrad gekürzt (Ø 63,7)
RIM_R = RR_ROOT + 2.75                  # Hohlrad außen Ø 73
FACE, GAP, DISK, HUB_D = 8.0, 0.6, 3.0, 30.0

# ---------------- Lager, Stahl, Passungen ----------------
E = 4.5                                 # Exzentrizität: Spalt zu 3,0 / Neutral 7,5 / offen 12,0 (Amboss 28,5, Rolle Ø 42)
L608 = (8.0, 22.0, 7.0)                 # d, D, B
L6905 = (25.0, 42.0, 9.0)
SEAT_D, SEAT_RIB, SEAT_RIB_W = 22.3, 0.25, 1.2   # 608-Sitz: Grundbohrung + 6 Quetschrippen -> Innenkreis 21,8 (Schätzwert)
LIP_D = 19.5                            # Lippe hält den Außenring axial
COLLAR_D = 11.5                         # Anlage nur am Innenring des 608
SHOULDER_D = 28.4                       # Anlage nur am Innenring des 6905
PIN_D = 24.9                            # Exzenterzapfen für 6905 (Bohrung 25), Schiebesitz
ROD_D = 8.4                             # M8 Durchgang
NUT_AF, NUT_H = 13.3, 6.7               # Tasche Mutter DIN 934 (SW 13, 6,5)
HEAD_POCKET = 5.6                       # Tasche Kopf DIN 933 (SW 13, 5,3)
DS_T = 1.2                              # Distanzscheibe 8,4 / 11,5 zwischen 608-Innenring und Hohlrad bzw. Mutter
NEMA_D, NEMA_FLAT, RIB_T, RIB_H = 5.50, 2.25, 0.45, 0.2      # NEMA-Konsens (ihht-pi CAD/passungen, 21.11.2026)
SHAFT_TIP = 19.0                        # NEMA-Wellenspitze ab Flansch; bis 24 passt auch
M3_CLR, M3_SELF, HEAD_D, HEAD_T = 3.4, 2.6, 6.4, 3.2

# ---------------- Rahmen (Ambosse = Rahmen v2) ----------------
YA, TA = 28.5, 10.0                     # Ambossfläche, Ambossdicke
YO = YA + TA
PLAY, LIP, LIP_H, LIP_X = 0.15, 3.0, 5.0, 26.0
YP = YO + PLAY + LIP                    # halbe Höhe Rückwand und Steg 41,65
XA, XP = 35.5, 44.5
ZC, LA = 15.0, 34.0                     # Schlauchmitte, Ambosslänge
GH = math.pi * (12 + 6) / 4 / 2 + 0.75  # Führungsstege: halber Innenabstand
X_RIB = 25.5                            # Führungsstege ab hier (wie v2)
SX, SY = 26.0, YA + TA / 2
FIX_X, FIX_Y = 40.0, 15.0
PT, TS = 8.0, 8.0                       # Rückwand z -8 .. 0, Steg z 34 .. 42 (beide 8 statt 6: 608 ist 7 breit)

# ---------------- Lage entlang der Achse ----------------
ZB0, ZB1 = 5.0, 23.0                    # 2 x 6905 (Mitte 14, Schlauchmitte 15)
Z_R608 = (-7.6, -0.6)                   # hinteres 608 liegt vorn an der Lippe der Rückwand
Z_F608 = (34.8, 41.8)                   # vorderes 608: Rotor 0,2 länger als der Lippenabstand (Außenringe rutschen nach außen statt vorzuspannen)
Z_DS_H = (Z_R608[0] - DS_T, Z_R608[0])
Z_DS_V = (Z_F608[1], Z_F608[1] + DS_T)
Z_NUT_V = (Z_DS_V[1], Z_DS_V[1] + 6.5)  # Mutter vorn (außen)
Z_HUB0, Z_SHOULDER = 0.6, ZB0 - 0.6     # Welle: Bund 11,5 bis 0,6, Nabe Ø 18, Schulter
Z_PIN1 = ZB1 - 0.3                      # Zapfen endet knapp vor der Lagerkante
Z_NOSE1 = Z_PIN1 + 1.2                  # Nase (45°) greift exzentrisch in die Klemmhülse -> Mitnahme
Z_KPOCK1 = Z_NOSE1 + 0.35               # Tasche für die Nase
Z_KNUT = (Z_KPOCK1 + 1.5, Z_KPOCK1 + 1.5 + NUT_H)   # gefangene Mutter in der Klemmhülse
Z_KHUB1 = 33.15                         # Klemmhülse: Nabe Ø 21 bis hier, dann Bund 11,5 bis zum 608
Z_DISK0 = Z_DS_H[0]                     # Hohlrad vorn
Z_DISK1 = Z_DISK0 - DISK
Z_RING1 = Z_DISK1 - FACE
Z_HEAD = Z_RING1 + HEAD_POCKET          # Auflage M8-Kopf
BOLT_L = 70.0
Z_C0 = Z_DISK0 - 23.4                   # Getriebeplatte vorn (Abstand zum Hohlrad wie v1)
Z_C1 = Z_C0 - 4.0                       # Motorflansch
Z_PH0, Z_PH1, Z_PG1 = Z_C0 + 0.5, Z_RING1 - GAP, Z_DISK1 - GAP     # Ritzel: Nabe, Zähne
Z_D0 = Z_PH0 + 3.0                      # D-Bohrung erst ab 7,5 mm vor dem Flansch (davor rund)


# ---------------- Profile ----------------
def gear_profile(n, ra, rf, thick, phase, steps=5):
    """Evolventenprofil Modul MOD, 20°: n Zähne, Kopf ra, Fuß rf, Zahndicke am Teilkreis thick, ein Zahn mittig bei phase."""
    r = n * MOD / 2
    pa = math.radians(20.0)
    rb = r * math.cos(pa)
    inv = lambda a: math.tan(a) - a
    half = thick / (2 * r)

    def psi(rho):
        return half + inv(pa) - inv(math.acos(min(1.0, rb / rho)))

    r0 = max(rb, rf)
    rad = [r0 + (ra - r0) * k / steps for k in range(steps + 1)]
    assert psi(ra) > 0.004, 'Zahn läuft spitz zu'
    pts = []
    for t in range(n):
        c = phase + 2 * math.pi * t / n
        if rf < rb:
            pts.append((rf, c - psi(rb)))
        pts += [(rho, c - psi(rho)) for rho in rad]
        pts.append((ra, c))
        pts += [(rho, c + psi(rho)) for rho in reversed(rad)]
        if rf < rb:
            pts.append((rf, c + psi(rb)))
        pts.append((rf, c + math.pi / n))
    return [(rho * math.cos(a), rho * math.sin(a)) for rho, a in pts]


def d_profile(d, flat, n=48):
    """Kreis Ø d mit Abflachung bei x = -flat."""
    r = d / 2
    t0 = math.acos(-flat / r)
    return [(r * math.cos(-t0 + 2 * t0 * k / n), r * math.sin(-t0 + 2 * t0 * k / n)) for k in range(n + 1)]


def hex_profile(af):
    """Sechskant mit Schlüsselweite af, Flächen bei y = ±af/2 (Einschub in -x)."""
    R = af / math.sqrt(3)
    return [(R * math.cos(math.pi / 3 * k), R * math.sin(math.pi / 3 * k)) for k in range(6)]


def seat_ribs(z0, z1, n=6):
    """Quetschrippen im 608-Sitz (längs, je eine 1,2 breite Leiste, 0,25 tief)."""
    out = []
    r_in, r_out = SEAT_D / 2 - SEAT_RIB, SEAT_D / 2 + 0.3
    for k in range(n):
        a = 2 * math.pi * k / n + math.pi / 6
        c, s = math.cos(a), math.sin(a)
        w = SEAT_RIB_W / 2
        pts = [(r_in * c - w * s, r_in * s + w * c), (r_out * c - w * s, r_out * s + w * c),
               (r_out * c + w * s, r_out * s - w * c), (r_in * c + w * s, r_in * s - w * c)]
        out.append(QP(0, 0, z0, z1, pts))
    return out


def seat_cut(z_entry, z_lip, z_face, z_back):
    """608-Sitz: Einführfase am Eingang z_entry, Bohrung bis zur Lippe z_lip, Lippe bis zur Wandfläche z_face.
    z_back = Außenseite hinter dem Eingang (für den Werkzeugüberlauf)."""
    s = 1 if z_lip > z_entry else -1
    return [QCz(0, 0, z_back, z_lip, SEAT_D),
            QK(0, 0, z_entry - s * 0.01, z_entry + s * 0.6, SEAT_D / 2 + 0.6, SEAT_D / 2),
            QCz(0, 0, z_lip - s * 0.5, z_face + s * 1.0, LIP_D)]
