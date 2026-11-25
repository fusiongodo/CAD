"""Getriebe 5 : 1 fuer das Exzenter-Quetschventil (Innenzahnring), Stand 22.11.2026.

Warum: Der NEMA 17 (17 Ncm) direkt auf dem Exzenter bleibt am Schlauch 6/12 haengen. Das Getriebe sitzt
hinter der Rahmenplatte; Rahmen, Steg, Griffscheibe und Lager aus quetschventil_v1.fs bleiben unveraendert.

Koordinaten wie quetschventil_v1.fs: Z = Hauptachse (Exzenterwelle), Rahmenplatte z -6 .. 0, Lager z 3 .. 27.
Alles Neue liegt bei z < -6. Der Motor sitzt 26 mm unter der Hauptachse (x 0, y -26), Flansch bei z -34.

Teile (alle ohne Stuetzen):
  Welle       ein Teil: Zapfen Ø 14 (laeuft in der Getriebeplatte), Zweiflach 17 (Mitnahme Hohlrad),
              Lagersitz Ø 21,8 (laeuft in der Pilotbohrung Ø 22,4 des Rahmens), Exzenterzapfen Ø 16,9 fuer 2 x 6203.
              Von vorn durch den Rahmen stecken. Druck: Zapfen Ø 14 unten, mit Brim.
  Hohlrad     65 Zaehne innen, Modul 1, Scheibe zum Rahmen, Hals mit radialer M3x8 (klemmt in die Senkung der Welle).
              Druck: Scheibe unten.
  Ritzel      13 Zaehne auf der NEMA-Welle, Passung nach Konsens (ihht-pi CAD/passungen): Ø 5,50 / 2,25 + 2 Stege 0,45 / 0,2.
              Seit 24.11.: radiale M5x8-Madenschraube in der Nabe auf die Abflachung (Kernloch 4,2, selbstschneidend).
              Druck: Nabe unten.
  Getriebeplatte  traegt Motor und hinteres Wellenlager, 4 Pfosten an die Befestigungsloecher des Rahmens (x ±40, y ±15).
              Druck: Plattenrueckseite unten, Pfosten nach oben.

         python3 build_getriebe.py          prueft die Netze (trimesh + manifold3d) und schreibt STL / 3MF nach druck/
"""
import math
import os

HERE = os.path.dirname(os.path.abspath(__file__))

# ---------------- Masse aus quetschventil_v1.fs (Rahmen bleibt) ----------------
PT = 6.0                    # Rahmenplatte z -PT .. 0
E = float(os.environ.get('QV_E', 3.5))      # Exzentrizitaet (Rahmen v1 ist fuer 3,5 gebaut: Spalt zu = 8,5 - E, offen = 8,5 + E)
DOTS = int(os.environ.get('QV_DOTS', 0))    # Kennpunkte auf der Zapfenstirn (Wellen-Varianten)
PIN_D = 16.9                # Zapfen fuer Lager 6203 (Bohrung 17)
ZB0, BW = 3.0, 12.0         # Lager ab z 3, je 12 breit
ZB1 = ZB0 + 2 * BW
PILOT = 22.4                # Pilotbohrung im Rahmen (war fuer den NEMA-Bund)
FIX_X, FIX_Y = 40.0, 15.0   # Befestigungsloecher des Rahmens (Ø 3,4)
M3_CLR, M3_SELF = 3.4, 2.6
NEMA_D, NEMA_FLAT, RIB_T, RIB_H = 5.50, 2.25, 0.45, 0.2   # Konsens 21.11.2026
SHAFT_TIP = 19.0            # NEMA-Wellenspitze ab Flansch (wie v1); bis 24 mm passt auch

# ---------------- Getriebe ----------------
MOD = 1.0
ZP, ZR = 13, 65             # 5 : 1
A = MOD * (ZR - ZP) / 2     # Achsabstand 26
BACKLASH = 0.3              # Flankenspiel gesamt (je Rad 0,15)
RP_TIP = ZP * MOD / 2 + 0.9             # Ritzel Kopfkreis 7,4
RP_ROOT = ZP * MOD / 2 - 1.25           # 5,25
RR_TIP = ZR * MOD / 2 - 0.65            # Hohlrad Kopfkreis 31,85 (gekuerzt: kein Eingriff unter dem Ritzel-Grundkreis)
RR_ROOT = ZR * MOD / 2 + 1.25           # 33,75
RIM_R = RR_ROOT + 2.75                  # Hohlrad aussen 36,5
FACE = 8.0                  # Zahnbreite
GAP = 0.6                   # Luft axial
DISK = 3.0

Z_PLATE = -PT                           # Rahmen-Rueckseite -6
Z_DISK0 = Z_PLATE - GAP                 # Hohlrad vorn -6,6
Z_DISK1 = Z_DISK0 - DISK                # -9,6
Z_RING1 = Z_DISK1 - FACE                # -17,6
Z_NECK1 = -26.4                         # Hals-Ende
Z_BOSS = Z_NECK1 - GAP                  # Bund der Getriebeplatte -27
Z_C0, Z_C1 = -30.0, -34.0               # Getriebeplatte vorn / hinten (= Motorflansch)

JOURNAL_D = 21.8            # Lagersitz in der Pilotbohrung 22,4
KEY_AF = 17.0               # Zweiflach (Schluesselweite)
STUB_D = 14.0               # hinterer Zapfen
FIT = 0.5                   # Spiel Welle/Hohlrad und Zapfen/Getriebeplatte (Durchmesser; gedruckte Bohrungen fallen enger aus)
Z_KEY1 = -15.0              # Ende Zweiflach / Beginn Kegel
Z_CONE1 = Z_KEY1 - (JOURNAL_D - STUB_D) / 2   # -18,9
Z_SCREW = -23.0             # radiale M3 im Hals
HUB_D, NECK_D = 30.0, 24.0


# ---------------- Grundkoerper ----------------
def B(x0, x1, y0, y1, z0, z1):
    return ('box', min(x0, x1), max(x0, x1), min(y0, y1), max(y0, y1), min(z0, z1), max(z0, z1))


def C(p0, p1, d):
    return ('cyl', tuple(p0), tuple(p1), d)


def Cz(x, y, z0, z1, d):
    return C((x, y, min(z0, z1)), (x, y, max(z0, z1)), d)


def K(x, y, z0, z1, r0, r1):
    """Kegelstumpf entlang Z: Radius r0 bei z0, r1 bei z1 (z0 < z1)."""
    return ('cone', (x, y, z0), (x, y, z1), r0, r1)


def P(x, y, z0, z1, pts):
    """Polygon (dx, dy) um (x, y), von z0 bis z1 extrudiert."""
    return ('prism', (x, y, min(z0, z1)), tuple(pts), abs(z1 - z0))


def Gz(x, y, z0, z1, n, ra, rf, thick, phase):
    """Zahnprofil (siehe gear_profile) um (x, y), von z0 bis z1. Im FeatureScript rechnet gearZ dieselben Punkte."""
    return ('gear', (x, y, min(z0, z1)), (n, ra, rf, thick, phase), abs(z1 - z0))


class Part:
    def __init__(s, name, label):
        s.name, s.label, s.stages = name, label, []

    def add(s, *ops):
        if not s.stages or s.stages[-1][0] != 'add':
            s.stages.append(['add', []])
        s.stages[-1][1].extend(ops)
        return s

    def cut(s, *ops):
        if not s.stages or s.stages[-1][0] != 'cut':
            s.stages.append(['cut', []])
        s.stages[-1][1].extend(ops)
        return s


# ---------------- Profile ----------------
def gear_profile(n, ra, rf, thick, phase, steps=5):   # gleiche Rechnung wie gearPts im FeatureScript
    """Evolventenprofil: n Zaehne, Kopf ra, Fuss rf, Zahndicke am Teilkreis thick, ein Zahn mittig bei phase."""
    r = n * MOD / 2
    pa = math.radians(20.0)
    rb = r * math.cos(pa)
    inv = lambda a: math.tan(a) - a
    half = thick / (2 * r)

    def psi(rho):
        return half + inv(pa) - inv(math.acos(min(1.0, rb / rho)))

    r0 = max(rb, rf)
    rad = [r0 + (ra - r0) * k / steps for k in range(steps + 1)]
    assert psi(ra) > 0.004, 'Zahn laeuft spitz zu'
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
    return [(round(rho * math.cos(a), 4), round(rho * math.sin(a), 4)) for rho, a in pts]


def d_profile(d, flat, n=48):
    """Kreis Ø d mit Abflachung bei x = -flat (wie v1: duenne Wand auf -X)."""
    r = d / 2
    t0 = math.acos(-flat / r)
    return [(round(r * math.cos(-t0 + 2 * t0 * k / n), 4), round(r * math.sin(-t0 + 2 * t0 * k / n), 4)) for k in range(n + 1)]


def dd_profile(d, af, n=24):
    """Zweiflach: Kreis Ø d, Flaechen bei x = ±af/2."""
    r = d / 2
    t0 = math.acos((af / 2) / r)
    pts = []
    for base in (0.0, math.pi):
        a0, a1 = base + t0, base + math.pi - t0
        pts += [(round(r * math.cos(a0 + (a1 - a0) * k / n), 4), round(r * math.sin(a0 + (a1 - a0) * k / n), 4)) for k in range(n + 1)]
    return pts


def crescent(r1, r2, e, n=40):
    """Kreis r1 um (0,0) minus Kreis r2 um (e,0): Sichel auf der -X-Seite."""
    x = (r1 * r1 - r2 * r2 + e * e) / (2 * e)
    y = math.sqrt(r1 * r1 - x * x)
    a1 = math.atan2(y, x)
    a2 = math.atan2(y, x - e)
    pts = [(r1 * math.cos(a1 + (2 * math.pi - 2 * a1) * k / n), r1 * math.sin(a1 + (2 * math.pi - 2 * a1) * k / n)) for k in range(n + 1)]
    pts += [(e + r2 * math.cos(-a2 + 2 * math.pi - (2 * math.pi - 2 * a2) * k / n), r2 * math.sin(-a2 + 2 * math.pi - (2 * math.pi - 2 * a2) * k / n))
            for k in range(1, n)]
    return [(round(px, 4), round(py, 4)) for px, py in pts]


# ---------------- Teile ----------------
pin_r = PIN_D / 2
jr, sr = JOURNAL_D / 2, STUB_D / 2

OV = max(1.4, E + pin_r - jr + 0.2)     # Ueberstand des Zapfens ueber den Lagersitz (45° abgestuetzt)
welle = Part('welle', 'QV-Getriebe Welle (Zapfen 14, Zweiflach 17, Lagersitz 21,8, Exzenter e %s)' % ('%g' % E).replace('.', ','))
welle.add(
    K(0, 0, Z_C1 + 0.2, Z_C1 + 0.6, sr - 0.4, sr),                 # Fase gegen Elefantenfuss
    Cz(0, 0, Z_C1 + 0.59, Z_CONE1 + 0.1, STUB_D),                   # Zapfen hinten
    K(0, 0, Z_CONE1, Z_KEY1, sr, jr),                               # 45°-Kegel statt Stufe
    Cz(0, 0, Z_KEY1 - 0.01, ZB0, JOURNAL_D),                        # Zweiflach + Lagersitz (bis zur Lagerschulter)
    K(E, 0, ZB0 - OV, ZB0, pin_r - OV, pin_r),                      # Stuetzkegel unter dem ueberstehenden Zapfenrand
    Cz(E, 0, ZB0 - 0.01, ZB1 - 0.7, PIN_D),                         # Exzenterzapfen
    K(E, 0, ZB1 - 0.71, ZB1 - 0.2, pin_r, pin_r - 0.5),
    Cz(E, 5.5, ZB1 - 1.0, ZB1 + 2, 2.8), Cz(E, -5.5, ZB1 - 1.0, ZB1 + 2, 2.8))   # Mitnehmerstifte wie v1 (Griffscheibe)
welle.cut(
    B(KEY_AF / 2, 14, -14, 14, Z_CONE1 - 0.6, Z_DISK0),             # Zweiflach
    B(-14, -KEY_AF / 2, -14, 14, Z_CONE1 - 0.6, Z_DISK0),
    P(0, 0, ZB0 - 0.6, ZB0 + 0.01, crescent(12.5, 11.0, E)),        # Lagerschulter nur Ø 22 um den Zapfen (Innenring)
    Cz(E, 0, ZB1 - 11, ZB1 + 1, M3_SELF),                           # M3 der Griffscheibe
    C((0, sr - 3.2, Z_SCREW), (0, sr + 1, Z_SCREW), 3.2))           # Senkung fuer die radiale M3 im Hohlrad-Hals
if E > 5.5:     # der Stuetzkegel ragte sonst in der Pilotbohrung (z < 0) ueber den Lagersitz hinaus: dort auf Ø 21,8 kappen
    welle.cut(P(E, 0, ZB0 - OV - 0.1, 0.2, [(-x_, y_) for x_, y_ in reversed(crescent(14.0, jr, E))]))
for k in range(DOTS):
    welle.cut(Cz(E + (5.5 if k % 2 == 0 else -5.5), 0 if k < 2 else 3, ZB1 - 0.8, ZB1 + 1, 1.2))

# ---------------- Welle mit angedrucktem Zapfen fuer das Gegenlager 6200 (25.11.2026, Alex) ----------------
# Beobachtung: Die Gegenlagerscheibe sitzt nur auf den 2 Stiften Ø 2,8 in Loechern Ø 3,3, kippt und laesst die
# Exzenterachse in Kraftrichtung ausweichen (es fehlen 1 - 2 mm Quetschweg). Jetzt ein Teil: Zapfen Ø 9,95 auf der
# Hauptachse direkt auf der Stirn des Exzenterzapfens. Damit die 6203 (Bohrung 17) noch von vorn drueber passen, ist der
# Zapfen auf den Kreis Ø 16,8 um die Exzenterachse beschnitten (Linse): bei e 3,5 fehlen 31°, bei 4,5 110°, bei 5,0 130°
# des Umfangs auf der Seite gegenueber dem Exzenter. Unter Last liegt der Innenring des 6200 an den beiden Linsenkanten.
# 3 Quetschrippen bei 0° / ±105°. Bis e 4,5 M3-Kernloch 20 tief in der Hauptachse (M3 x 20 als Stahlkern, optional).
ZJ1 = 40.9                  # Zapfenende (Steg 6200: Lager z 32,2 .. 41,2), wie gen_gegenlager.Z_ST1
JZ_D, JZ_RIB, JZ_PASS = 9.95, 0.12, 16.8


def linse(r, rp, e, n=24):
    """Kreis r um (0,0) geschnitten mit Kreis rp um (e,0)."""
    x = (r * r - rp * rp + e * e) / (2 * e)
    if x <= -r:
        return [(round(r * math.cos(2 * math.pi * k / (4 * n)), 4), round(r * math.sin(2 * math.pi * k / (4 * n)), 4)) for k in range(4 * n)]
    a1 = math.acos(x / r)                                   # Bogen des Zapfenkreises -a1 .. a1
    a2 = math.atan2(math.sqrt(r * r - x * x), x - e)        # Bogen des Durchlasskreises a2 .. 2 pi - a2
    pts = [(r * math.cos(-a1 + 2 * a1 * k / (2 * n)), r * math.sin(-a1 + 2 * a1 * k / (2 * n))) for k in range(2 * n + 1)]
    pts += [(e + rp * math.cos(a2 + (2 * math.pi - 2 * a2) * k / n), rp * math.sin(a2 + (2 * math.pi - 2 * a2) * k / n)) for k in range(1, n)]
    return [(round(px, 4), round(py, 4)) for px, py in pts]


def _rippe(r0, r1, w, ang):
    c, s = math.cos(ang), math.sin(ang)
    return [(round(x * c - y * s, 4), round(x * s + y * c, 4)) for x, y in ((r0, -w / 2), (r1, -w / 2), (r1, w / 2), (r0, w / 2))]


if os.environ.get('QV_ZAPFEN', '1') != '0':      # Standard seit 25.11.2026; QV_ZAPFEN=0 baut die alte Welle mit Stiften
    welle = Part('welle_zapfen', 'QV-Getriebe Welle mit Zapfen fuer 6200 (ein Teil, Exzenter e %s)' % ('%g' % E).replace('.', ','))
    welle.add(
        K(0, 0, Z_C1 + 0.2, Z_C1 + 0.6, sr - 0.4, sr),
        Cz(0, 0, Z_C1 + 0.59, Z_CONE1 + 0.1, STUB_D),
        K(0, 0, Z_CONE1, Z_KEY1, sr, jr),
        Cz(0, 0, Z_KEY1 - 0.01, ZB0, JOURNAL_D),
        K(E, 0, ZB0 - OV, ZB0, pin_r - OV, pin_r),
        Cz(E, 0, ZB0 - 0.01, ZB1 - 0.2, PIN_D),
        P(0, 0, ZB1 - 0.21, ZJ1, linse(JZ_D / 2, JZ_PASS / 2, E)))
    for a_ in (0.0, math.radians(105), math.radians(-105)):
        welle.add(P(0, 0, ZB1 + 6.0, ZJ1 - 0.8, _rippe(JZ_D / 2 - 0.2, JZ_D / 2 + JZ_RIB, 0.9, a_)))
    welle.cut(
        B(KEY_AF / 2, 14, -14, 14, Z_CONE1 - 0.6, Z_DISK0),
        B(-14, -KEY_AF / 2, -14, 14, Z_CONE1 - 0.6, Z_DISK0),
        P(0, 0, ZB0 - 0.6, ZB0 + 0.01, crescent(12.5, 11.0, E)),
        C((0, sr - 3.2, Z_SCREW), (0, sr + 1, Z_SCREW), 3.2))
    if E <= 4.6:
        welle.cut(Cz(0, 0, ZJ1 - 20, ZJ1 + 1, M3_SELF))
    for k in range(DOTS):                                   # Kennpunkte auf der Zapfenstirn neben dem Lagerzapfen
        welle.cut(Cz(E + 6.0, -2.5 + 5 * k if DOTS > 1 else 0, ZB1 - 0.8, ZB1 + 1, 1.2))

hohlrad = Part('hohlrad', 'QV-Getriebe Hohlrad 65 Zaehne innen, Modul 1 (radial M3x8)')
hohlrad.add(Cz(0, 0, Z_RING1, Z_DISK0, 2 * RIM_R))
hohlrad.cut(Gz(0, 0, Z_RING1 - 0.1, Z_DISK1, ZR, RR_ROOT, RR_TIP, math.pi * MOD / 2 + BACKLASH / 2, -math.pi / 2))
hohlrad.add(Cz(0, 0, Z_RING1 - 0.01, Z_DISK1 + 0.1, HUB_D), Cz(0, 0, Z_NECK1, Z_RING1, NECK_D))
pr = (JOURNAL_D + FIT) / 2
hohlrad.cut(
    P(0, 0, Z_KEY1 - 0.2, Z_DISK0 + 0.1, dd_profile(JOURNAL_D + FIT, KEY_AF + FIT)),      # Zweiflach-Tasche
    K(0, 0, Z_KEY1 - 0.2 - (pr + 0.3 - (STUB_D + FIT) / 2 + 0.1), Z_KEY1 - 0.19, (STUB_D + FIT) / 2 - 0.1, pr + 0.3),   # Kegeldach statt Bruecke; etwas groesser als die Tasche und kleiner als die Bohrung, damit keine Kante genau auf einer Wand endet
    Cz(0, 0, Z_NECK1 - 1, Z_KEY1 - 3.5, STUB_D + FIT),                                    # Bohrung im Hals
    K(0, 0, Z_DISK0 - 0.7, Z_DISK0 + 0.01, pr - 0.2, pr + 0.51),                          # Fase gegen Elefantenfuss (beginnt innerhalb der Tasche)
    C((0, sr - 0.5, Z_SCREW), (0, NECK_D / 2 + 1, Z_SCREW), M3_SELF))                      # radiale M3 (selbstschneidend)

Z_PH0, Z_PH1, Z_PG1 = Z_C0 + 0.5, Z_RING1 - GAP, Z_DISK1 - GAP     # Nabe -29,5 .. -18,2, Zaehne bis -10,2
Z_D0 = Z_PH0 + 3.0                                                  # D-Bohrung erst ab 7,5 mm vor dem Flansch (davor rund)
HUB_R = RP_TIP + 0.3                                                # Nabe Ø 15,4
Z_SET, SET_D, SET_L = (Z_D0 + Z_PH1) / 2, 4.2, 8.0                  # M5x8-Madenschraube radial auf die Abflachung (-X), z -22,35;
                                                                    # Kernloch 4,2, die Schraube schneidet ihr Gewinde selbst
ritzel = Part('ritzel', 'QV-Getriebe Ritzel 13 Zaehne, Modul 1 (NEMA-Welle, 2 Quetschstege, M5-Madenschraube)')
ritzel.add(Cz(0, -A, Z_PH0, Z_PH1 + 0.01, 2 * HUB_R),
           Gz(0, -A, Z_PH1, Z_PG1, ZP, RP_TIP, RP_ROOT, math.pi * MOD / 2 - BACKLASH / 2, -math.pi / 2))
ritzel.cut(C((-(NEMA_FLAT - 0.3), -A, Z_SET), (-(HUB_R + 1.0), -A, Z_SET), SET_D),          # Kernloch M5
           C((-(HUB_R - 0.6), -A, Z_SET), (-(HUB_R + 1.0), -A, Z_SET), 5.0),                 # Einlauf 0,6 tief zum Ansetzen
           P(0, -A, Z_D0 - 0.01, Z_PG1 + 1, d_profile(NEMA_D, NEMA_FLAT)),
           Cz(0, -A, Z_PH0 - 1, Z_D0, NEMA_D + 0.3),                                       # runder Einlauf (Welle ohne Abflachung)
           K(0, -A, Z_PH0 - 0.01, Z_PH0 + 0.6, NEMA_D / 2 + 0.75, NEMA_D / 2 + 0.15))      # Fase gegen Elefantenfuss
z_tip = Z_C1 + SHAFT_TIP
rib_top = Z_PH0 + math.floor((z_tip - 0.7 - Z_PH0) / 0.2) * 0.2
for zr in (Z_D0 + 1.0, rib_top):                                    # genau 2 Stege, je eine Schicht
    ritzel.add(B(-NEMA_D / 2 - 0.3, -(NEMA_FLAT - RIB_T), -A - NEMA_D / 2 - 0.3, -A + NEMA_D / 2 + 0.3, round(zr, 3), round(zr + RIB_H, 3)))

platte = Part('platte', 'QV-Getriebe Getriebeplatte (Motor, Wellenlager, 4 Pfosten an den Rahmen)')
platte.add(B(-45, 45, -A - 22.5, 20, Z_C1, Z_C0), Cz(0, 0, Z_C0 - 0.01, Z_BOSS, 22))
for sx in (-1, 1):
    for sy in (-1, 1):
        platte.add(Cz(sx * FIX_X, sy * FIX_Y, Z_C0 - 0.01, Z_PLATE, 9))
platte.cut(Cz(0, 0, Z_C1 - 1, Z_BOSS + 1, STUB_D + FIT), Cz(0, -A, Z_C1 - 1, Z_C0 + 1, 22.4))
for sx in (-1, 1):
    for sy in (-1, 1):
        platte.cut(Cz(sx * 15.5, -A + sy * 15.5, Z_C1 - 1, Z_C0 + 1, M3_CLR),
                   Cz(sx * FIX_X, sy * FIX_Y, Z_PLATE - 14, Z_PLATE + 1, M3_SELF))

PARTS = [welle, hohlrad, ritzel, platte]


# ---------------- FeatureScript ----------------
def f(v):
    s = ('%.4f' % v).rstrip('0').rstrip('.')
    return '0' if s in ('-0', '') else s


def vec(p):
    return 'vector(%s, %s, %s) * millimeter' % (f(p[0]), f(p[1]), f(p[2]))


def emit_op(o, idexpr):
    if o[0] == 'box':
        _, x0, x1, y0, y1, z0, z1 = o
        return 'fCuboid(context, %s, { "corner1" : %s, "corner2" : %s });' % (idexpr, vec((x0, y0, z0)), vec((x1, y1, z1)))
    if o[0] == 'cone':
        _, p0, p1, r0, r1 = o
        return 'fCone(context, %s, { "bottomCenter" : %s, "topCenter" : %s, "bottomRadius" : %s * millimeter, "topRadius" : %s * millimeter });' % (
            idexpr, vec(p0), vec(p1), f(r0), f(r1))
    if o[0] == 'prism':
        _, c, pts, depth = o
        return 'prismZ(context, %s, %s, [%s], %s);' % (idexpr, vec(c), ', '.join('[%s, %s]' % (f(a), f(b)) for a, b in pts), f(depth))
    if o[0] == 'gear':
        _, c, (n, ra, rf, thick, phase), depth = o
        return 'prismZ(context, %s, %s, gearPts(%d, %s, %s, %s, %s), %s);' % (idexpr, vec(c), n, f(ra), f(rf), '%.6f' % thick, '%.6f' % phase, f(depth))
    _, p0, p1, d = o
    return 'fCylinder(context, %s, { "bottomCenter" : %s, "topCenter" : %s, "radius" : %s * millimeter });' % (idexpr, vec(p0), vec(p1), f(d / 2))


def emit(parts=None, title='Quetschventil Getriebe v1', export='quetschventilGetriebeV1', head=None):
    parts = parts or PARTS
    L = ['FeatureScript 3112;', 'import(path : "onshape/std/geometry.fs", version : "3112.0");', '',
         '// Getriebe 5 : 1 fuer das Exzenter-Quetschventil (Innenzahnring 65 / Ritzel 13, Modul 1). Erzeugt von gen_getriebe.py',
         '// (ihht-pi CAD/quetschventil), nicht von Hand aendern: Konstante im Generator aendern und neu erzeugen.',
         '// Koordinaten wie quetschventil_v1.fs: Z = Hauptachse, Rahmenplatte z -6 .. 0. Rahmen, Steg, Griffscheibe, Lager bleiben.',
         '// Welle von vorn durch die Pilotbohrung des Rahmens, Hohlrad von hinten aufstecken (radiale M3x8 im Hals),',
         '// Motor mit 4 x M3x8 an die Getriebeplatte, Ritzel auf die Motorwelle, Platte mit 4 x M3x12 von vorn durch die',
         '// Befestigungsloecher des Rahmens (x ±40, y ±15) in die Pfosten.', '',
         'function gearPsi(rho is number, rb is number, half is number, pa is number) returns number', '{',
         '    const a = acos(min(1, rb / rho)) / radian;',
         '    return half + (tan(pa * radian) - pa) - (tan(a * radian) - a);', '}', '',
         'function gearPts(n is number, ra is number, rf is number, thick is number, phase is number) returns array', '{',
         '    // Evolventenprofil Modul %s, Eingriffswinkel 20 Grad: n Zaehne, Kopf ra, Fuss rf, Zahndicke am Teilkreis thick, ein Zahn mittig bei phase' % f(MOD),
         '    const steps = 5;',
         '    const r = n * %s / 2;' % f(MOD),
         '    const pa = 20 * PI / 180;',
         '    const rb = r * cos(pa * radian);',
         '    const half = thick / (2 * r);',
         '    const r0 = max(rb, rf);',
         '    var pts = [];',
         '    for (var t = 0; t < n; t += 1)',
         '    {',
         '        const c = phase + 2 * PI * t / n;',
         '        var pol = [];',
         '        if (rf < rb)',
         '            pol = append(pol, [rf, c - gearPsi(rb, rb, half, pa)]);',
         '        for (var k = 0; k <= steps; k += 1)',
         '        {',
         '            const rho = r0 + (ra - r0) * k / steps;',
         '            pol = append(pol, [rho, c - gearPsi(rho, rb, half, pa)]);',
         '        }',
         '        pol = append(pol, [ra, c]);',
         '        for (var k = steps; k >= 0; k -= 1)',
         '        {',
         '            const rho = r0 + (ra - r0) * k / steps;',
         '            pol = append(pol, [rho, c + gearPsi(rho, rb, half, pa)]);',
         '        }',
         '        if (rf < rb)',
         '            pol = append(pol, [rf, c + gearPsi(rb, rb, half, pa)]);',
         '        pol = append(pol, [rf, c + PI / n]);',
         '        for (var q in pol)',
         '            pts = append(pts, [q[0] * cos(q[1] * radian), q[0] * sin(q[1] * radian)]);',
         '    }',
         '    return pts;', '}', '',
         'function unionAll(context is Context, opId is Id, q is Query)', '{',
         '    const bodies = qBodyType(q, BodyType.SOLID);',
         '    if (size(evaluateQuery(context, bodies)) > 1)',
         '        opBoolean(context, opId, { "tools" : bodies, "operationType" : BooleanOperationType.UNION });', '}', '',
         'function cutAll(context is Context, opId is Id, targets is Query, tools is Query)', '{',
         '    if (size(evaluateQuery(context, tools)) > 0)',
         '        opBoolean(context, opId, { "tools" : tools, "targets" : qBodyType(targets, BodyType.SOLID), "operationType" : BooleanOperationType.SUBTRACTION });', '}', '',
         'function prismZ(context is Context, id is Id, origin is Vector, pts is array, depth is number)', '{',
         '    // Polygon [dx, dy] in der XY-Ebene durch origin, Extrusion um depth in +Z',
         '    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(origin, vector(0, 0, 1), vector(1, 0, 0)) });',
         '    const n = size(pts);',
         '    for (var i = 0; i < n; i += 1)',
         '    {',
         '        const a = pts[i];',
         '        const b = pts[(i + 1) % n];',
         '        skLineSegment(sk, "l" ~ i, { "start" : vector(a[0], a[1]) * millimeter, "end" : vector(b[0], b[1]) * millimeter });',
         '    }',
         '    skSolve(sk);',
         '    opExtrude(context, id + "ext", { "entities" : qSketchRegion(id + "sk"), "direction" : vector(0, 0, 1), "endBound" : BoundingType.BLIND, "endDepth" : depth * millimeter });',
         '    opDeleteBodies(context, id + "del", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });', '}', '']
    if head:
        L[3:9] = ['// ' + h for h in head]
    for p in parts:
        L += ['function qvg_%s(context is Context, id is Id, err is box)' % p.name, '{']
        adds = []
        tq = lambda: 'qUnion([' + ', '.join('qCreatedBy(%s, EntityType.BODY)' % a for a in adds) + '])'
        for si, (kind, ops) in enumerate(p.stages):
            if kind == 'add':
                L.append('    const A%d = id + "%s_a%d";' % (si, p.name, si))
                adds.append('A%d' % si)
                for j, o in enumerate(ops):
                    L.append('    ' + emit_op(o, 'A%d + "s%d"' % (si, j)))
                L.append('    try { unionAll(context, id + "%s_u%d", %s); } catch (e) { err[] = err[] ~ "%s Vereinigung Stufe %d; "; }' % (p.name, si, tq(), p.name, si))
            else:
                L.append('    const C%d = id + "%s_c%d";' % (si, p.name, si))
                for j, o in enumerate(ops):
                    L.append('    ' + emit_op(o, 'C%d + "o%d"' % (si, j)))
                L.append('    try { cutAll(context, id + "%s_x%d", %s, qCreatedBy(C%d, EntityType.BODY)); } catch (e) { err[] = err[] ~ "%s Abzug Stufe %d; "; }' % (p.name, si, tq(), si, p.name, si))
        L.append('    setProperty(context, { "entities" : qBodyType(%s, BodyType.SOLID), "propertyType" : PropertyType.NAME, "value" : "%s" });' % (tq(), p.label))
        L += ['}', '']
    L += ['annotation { "Feature Type Name" : "%s" }' % title,
          'export const %s = defineFeature(' % export + 'function(context is Context, id is Id, definition is map)',
          '    precondition', '    {', '    }', '    {']
    L += ['        var err = new box("");']
    L += ['        try { qvg_%s(context, id + "%s", err); } catch (e) { err[] = err[] ~ "%s abgebrochen; "; }' % (p.name, p.name, p.name) for p in parts]
    L += ['        if (err[] != "")', '            reportFeatureWarning(context, id, err[]);', '    });', '']
    return '\n'.join(L)


if __name__ == '__main__':
    open(os.path.join(HERE, 'getriebe_v1.fs'), 'w').write(emit())
    print('ok', [p.name for p in PARTS], 'Achsabstand', A, 'Uebersetzung', ZR / ZP)
