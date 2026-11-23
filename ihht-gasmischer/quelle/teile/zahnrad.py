# Zahnradkupplung (NEMA -> Zufuhr-LZQ) und Zahnradnabe (Ablass-LZQ), je 2 Stueck. Passmasse Stand 19.11.2026.
from ihht.geo import *
from ihht.masse import *

# Stirnräder 1:1, direkt kämmend -> Ablass dreht gegenläufig. z 16, Modul 22/16 = 1,375, Eingriffswinkel 20°.
GEAR_N = 16
GEAR_M = PAIR / GEAR_N
GEAR_BACKLASH = 0.15                 # Flankenspiel gesamt am Teilkreis (je Rad 0,075 dünner)
GEAR_RA = PAIR / 2 + GEAR_M          # Kopfkreis 12,375
GEAR_Y0, GEAR_Y1 = Y_NEMA_FACE + 5.0, Y_NEMA_FACE + 19.0   # 14 mm breit, direkt vor dem Motor (Spindeln dürfen ±5 mm gegeneinander wandern)
HUB_D, HUB_D1, HUB_Y1 = 11.0, 9.0, Y_STEM_TIP + 8.0        # Nabe Ø 11, vorderes Stück Ø 9 (wie Kupplung v1) bis y -34
HUB_YS = Y_STEM_TIP - 4.0                                  # Übergang Ø 11 -> Ø 9
STEM_BORE_Y0 = Y_STEM_TIP - 12.0                           # LZQ-Bohrung 12 mm tiefer als die Spindelspitze (Spindelhub)
NEMA_BORE_Y1 = Y_NEMA_TIP + 2.0                            # NEMA-Bohrung 2 mm über die Wellenspitze hinaus
# Passmaße aus den Steg-Zylindern vom 17.11.2026 (ihht-pi CAD/passungen, gelten nur für 0,20-mm-Schichten, Bohrung stehend).
# Quetschrillen (Opferstege) = waagerechte Rippen auf der Abflachung, alle 2 mm; bewusst die lockerere der guten Varianten.
WIDEN_D, WIDEN_FLAT = 0.15, 0.08                         # 19.11.: Druck vom 18.11. ging nicht auf die LZQ -> beide Seiten gleich weiter
FIT_NEMA_D, FIT_NEMA_FLAT = 5.26 + WIDEN_D, 2.10 + WIDEN_FLAT
FIT_LZQ_D, FIT_LZQ_FLAT = 4.34 + WIDEN_D, 1.07 + WIDEN_FLAT   # Schlüsselweite 3,24 + 0,155
KEY_LZQ_L = 4.5                                            # LZQ-Abflachung nur bis 4,5 mm hinter der Spindelspitze (vorher 6,5): falls die echte Spindel-Abflachung kürzer ist, klemmt der runde Teil sonst
RIB_NEMA = (0.15, 0.4)                                     # Tiefe, Höhe (18.11.: 0,20 / 0,4)
RIB_LZQ = (0.15, 0.4)                                      # Tiefe, Höhe (unverändert, flacher druckt die Düse nicht)
LAYER = 0.2
PIN_BORE = 7.0                                             # Nennmaß; gedruckt ca. 6,4–6,7 -> Laufsitz auf dem Zapfen Ø 6
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
def hub_body(x, phase):
    return [P(x, Z_V, GEAR_Y0, GEAR_Y1, gear_profile(GEAR_N, GEAR_M, phase, GEAR_BACKLASH)),
            Cy(x, GEAR_Y1 - 0.1, HUB_YS, Z_V, HUB_D), Cy(x, HUB_YS - 0.1, HUB_Y1, Z_V, HUB_D1)]
def stem_cut(x):
    return [Cy(x, STEM_BORE_Y0, HUB_Y1 + 1, Z_V, FIT_LZQ_D),
            K(x, Z_V, HUB_Y1 - 0.6, HUB_Y1 + 0.01, FIT_LZQ_D / 2, FIT_LZQ_D / 2 + 0.6)]                # Einführfase
def stem_flat(x):
    return B(x - 2.6, x + 2.6, Y_STEM_TIP - 1.5, Y_STEM_TIP + KEY_LZQ_L, Z_V + FIT_LZQ_FLAT, Z_V + 3.4)
def ribs(x, d, flat, rib, y0, y1):
    # 19.11.: nur 2 Quetschrillen je Bohrung (vorher alle 2 mm) - eine oben, eine in der Mitte des Steckbereichs y0..y1.
    # Druck steht auf GEAR_Y0 -> Rillen auf ganze Schichten legen
    t, h = rib; out = []
    snap = lambda y: GEAR_Y0 + round((y - GEAR_Y0) / LAYER) * LAYER
    for y in (snap((y0 + y1) / 2 - h / 2), snap(y1 - h)):
        out.append(B(x - d / 2 - 0.3, x + d / 2 + 0.3, round(y, 3), round(y + h, 3), Z_V + flat - t, Z_V + flat + 0.01))
    return out
def stem_ribs(x):                                          # nur dort, wo die Spindel-Abflachung sitzt
    return ribs(x, FIT_LZQ_D, FIT_LZQ_FLAT, RIB_LZQ, Y_STEM_TIP + 0.5, Y_STEM_TIP + KEY_LZQ_L - 0.3)

def bauen():
    teile = []
    for i, x in enumerate(XS_MOTOR):
        g = Part('gearDrive%d' % (i + 1), 'Zahnradkupplung v5 %s (NEMA -> Zufuhr, 48 mm)' % ('hi', 'lo')[i], ORANGE)
        teile.append(g)
        g.add(*hub_body(x, 0.0))
        g.cut(Cy(x, GEAR_Y0 - 1, NEMA_BORE_Y1, Z_V, FIT_NEMA_D),
              K(x, Z_V, NEMA_BORE_Y1 - 0.01, NEMA_BORE_Y1 + FIT_NEMA_D / 2 - 0.6, FIT_NEMA_D / 2, 0.6),            # Kegeldach (druckt ohne Brücke)
              K(x, Z_V, GEAR_Y0 - 0.01, GEAR_Y0 + 0.6, FIT_NEMA_D / 2 + 0.6, FIT_NEMA_D / 2),
              *stem_cut(x))
        g.add(B(x - 3, x + 3, GEAR_Y0, NEMA_BORE_Y1, Z_V + FIT_NEMA_FLAT, Z_V + 3.6), stem_flat(x))          # D-Abflachungen wie Kupplung v1
        g.add(*ribs(x, FIT_NEMA_D, FIT_NEMA_FLAT, RIB_NEMA, GEAR_Y0 + 1.0, Y_NEMA_TIP - 0.3), *stem_ribs(x))   # Quetschrillen NEMA + LZQ
    for i, x in enumerate(XS_GEAR):
        g = Part('gearIdle%d' % (i + 1), 'Zahnradnabe v5 %s (Ablass, läuft auf Lagerzapfen, 48 mm)' % ('hi', 'lo')[i], (0.95, 0.7, 0.25, 1.0))
        teile.append(g)
        g.add(*hub_body(x, math.pi / GEAR_N))
        g.cut(Cy(x, GEAR_Y0 - 1, PIN_Y1 + 1.5, Z_V, PIN_BORE),                                                # Zapfenbohrung
              K(x, Z_V, PIN_Y1 + 1.49, PIN_Y1 + 1.5 + PIN_BORE / 2 - 0.6, PIN_BORE / 2, 0.6),                      # Kegeldach
              K(x, Z_V, GEAR_Y0 - 0.01, GEAR_Y0 + 0.8, PIN_BORE / 2 + 0.8, PIN_BORE / 2),
              *stem_cut(x))
        g.add(stem_flat(x), *stem_ribs(x))                                                                   # Lagerzapfen bleibt glatt (Laufsitz)
    return teile
