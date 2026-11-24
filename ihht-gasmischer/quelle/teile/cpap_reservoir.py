# CPAP-Inline-Reservoir-Adapter: sitzt irgendwo im 22-mm-CPAP-Schlauch zwischen Gasquelle und Maske.
#   unten (Z = 0)   CPAP-Tuelle Quelle (Zufuhr)
#   seitlich 45°    CPAP-Tuelle Reservoirbeutel (direkt oder ueber ~30 cm CPAP-Stueck), VOR der Membran
#   Mitte           Einweg-Klappenventil: Sitz + Kreuz + Pilzknopf aus dem Y-Split (Influx-Ring, getestet)
#   oben            Maskenkappe B mit CPAP-Tuelle zur Maske
# Fluss nur Quelle/Beutel -> Maske; Rueckstrom (Ausatmen) drueckt die Membran auf den Sitz.
# Zwei Teile, damit die Membran ueber den Knopf gezogen werden kann: B steckt mit konischem Zapfen in A (Klemmsitz,
# bei Bedarf mit Klebeband abdichten). Druck: beide Teile so wie modelliert (Z oben), ohne Stuetzen.
import math
from ihht.geo import Part, C, Cz, P
from ihht.anschluesse import ANSCHLUSS

T = ANSCHLUSS['cpap_pi']
S = math.sqrt(0.5)

# Ventil (Y-Split, VALVE_VARIABLES / valveRing, Influx-Seite)
MEM_D, SEAT_D, MEM_SP = 21.5, 19.0, 0.5           # Membran (Flap), Sitzloch, Luft rundum
SEAT_H = 3.0                                       # Sitzbund-Hoehe unter der Sitzflaeche
MEM_H = 13.0                                       # freier Membranraum ueber dem Sitz (Ring 16 - Sitz 3)
TS = 1.5                                           # Kreuz: Armbreite = Tiefe in der Mitte
PIN = dict(neck=2.5, gap=1.0, head=6.5, flare=60.0, land=0.3, lead=1.5, tip=1.0)

# Lage (mm, Achse = Z)
Z_W1 = 16.2                       # Wand der Quell-Tuelle (Spitze bei Z = 0)
ZJ = 26.0                         # Beutel-Achse schneidet die Hauptachse
ZS = ZJ + 29.0                    # Sitzflaeche (Beutel-Oeffnung endet darunter, Kreuzarme liegen frei)
ZC = ZS + MEM_H                   # Membranraum oben = Unterkante Zapfen B
SOCK_T, SOCK_D = 10.0, 25.2       # Steckbuchse in A
ZA = ZC + SOCK_T                  # Oberkante A
FL_H = 3.0                        # Bund B
ZB_W = ZA + FL_H                  # Wand der Masken-Tuelle
R_BODY = 14.5                     # Aussenradius Rohr A / Beutel-Arm / Bund B
L0 = 36.0                         # Beutel-Tuelle: Wandabstand von der Hauptachse (Schlauchmuffe frei vom Rohr)
NB = (S, 0.0, S)                  # Richtung Beutel-Tuelle (45° nach oben, zur Maskenseite)
P_BAG = (S * L0, 0.0, ZJ + S * L0)


def kreuz_und_knopf():
    rw = SEAT_D / 2 + 1.0                            # Arme reichen in die Wand
    v = lambda: [(-rw, 0), (rw, 0), (rw, -(TS + rw)), (0, -TS), (-rw, -(TS + rw))]   # Steg, unten 45°-V (stuetzenfrei)
    # Arm in X: auf der +X-Seite (Beutel-Oeffnung) nur 3 mm tief, haengt am Sitzbund
    arm_x = P(0, ZS, -TS / 2, TS / 2, [(-rw, 0), (rw, 0), (rw, -SEAT_H), (-TS, -SEAT_H), (-rw, -(TS + rw))])
    arm_y = ('prismx', (0, ZS), tuple(v()), -TS / 2, TS / 2)
    p = PIN
    fl = (p['head'] - p['neck']) / 2 * math.tan(math.radians(p['flare']))
    z1 = ZS + p['gap']; z2 = z1 + fl; z3 = z2 + p['land']; z4 = z3 + p['lead']
    knopf = [Cz(0, 0, ZS - TS / 2, z1, p['neck']),
             ('cone', (0, 0, z1), (0, 0, z2), p['neck'] / 2, p['head'] / 2),
             Cz(0, 0, z2, z3, p['head']),
             ('cone', (0, 0, z3), (0, 0, z4), p['head'] / 2, p['tip'] / 2)]
    return [arm_x, arm_y] + knopf


def teil_a():
    a = Part('cpapResA', 'CPAP-Reservoir A: Quelle + Beutel 45° + Ventilsitz (Membran Ø 21,5)', (0.3, 0.6, 0.85))
    a.add(*T.koerper_achse((0, 0, Z_W1), (0, 0, -1)),
          ('cone', (0, 0, Z_W1), (0, 0, Z_W1 + R_BODY - T.r_spitze), T.r_spitze, R_BODY),   # 45°-Uebergang Tuelle -> Rohr
          Cz(0, 0, Z_W1 + R_BODY - T.r_spitze - 0.01, ZA, 2 * R_BODY),
          C((0, 0, ZJ), P_BAG, 2 * R_BODY),
          *T.koerper_achse(P_BAG, NB))
    a.cut(Cz(0, 0, -1, ZS - SEAT_H - 0.7, 2 * T.r_innen),
          ('cone', (0, 0, ZS - SEAT_H - 0.7), (0, 0, ZS - SEAT_H), T.r_innen, SEAT_D / 2),  # 45° auf das Sitzloch
          Cz(0, 0, ZS - SEAT_H - 0.01, ZS + 0.01, SEAT_D),
          Cz(0, 0, ZS, ZC + 0.01, MEM_D + 2 * MEM_SP),
          Cz(0, 0, ZC, ZA + 1, SOCK_D),
          *T.bohrung_achse(P_BAG, NB, -L0))
    a.add(*kreuz_und_knopf())
    return a


def teil_b():
    b = Part('cpapResB', 'CPAP-Reservoir B: Maskenkappe (Zapfen konisch Ø 24,6-25,2)', (0.85, 0.55, 0.3))
    b.add(('cone', (0, 0, ZC), (0, 0, ZA), SOCK_D / 2 - 0.3, SOCK_D / 2),
          ('cone', (0, 0, ZA), (0, 0, ZA + R_BODY - SOCK_D / 2), SOCK_D / 2, R_BODY),         # 45°-Fase unter dem Bund
          Cz(0, 0, ZA + R_BODY - SOCK_D / 2 - 0.01, ZB_W, 2 * R_BODY),
          *T.koerper_achse((0, 0, ZB_W), (0, 0, 1)))
    b.cut(Cz(0, 0, ZC - 1, ZB_W + T.laenge + 1, 2 * T.r_innen))
    return b


def bauen():
    return [teil_a(), teil_b()]
