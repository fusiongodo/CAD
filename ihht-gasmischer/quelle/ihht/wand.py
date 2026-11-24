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
# v2.2 (24.11.2026, Alex hat beide OCS ausgemessen): OCS hi = 3F 3.0 (125 x 22, Tüllen 85 auseinander), OCS lo =
#   3FRL 2.0 (79 x 24). Beide mit der Bauteilseite (Stecker) zur Wand, Rohr nach vorn, auf Steckhaltern (Platine wird auf
#   Zapfen gesteckt). Untere hi-Tülle liegt jetzt 85 tiefer -> U-Bogen R 25 geht links weit aus und läuft mit einer
#   flachen S-Kurve von unten in den T-Arm (gerader U-Bogen hätte nur R 18).

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
FASE = 38.0                     # 45°-Fase oben links (X -123 / Z 252), Schenkel 38: Bambu P1S sperrt vorn links 18 x 28
                                # und prüft die Hülle des Teils (Rechteck-Ausklinkung reicht nicht). Wand liegt mit dieser
                                # Ecke vorn links auf dem Bett. 38 geht gerade durch, QV-Schraube oben außen bleibt (4,6 Abstand).


def fase_abstand(x, z):
    """Abstand eines Punkts (x, z) der Wand von der Fasenkante (positiv = im Material)."""
    return ((x + WX) + (WZ1 - z) - FASE) / math.sqrt(2)

# ---------------- QV ----------------
QX, QZ = 73.0, 198.0            # Achsen bei X ±73 (Pfosten außen bis 117,5, Seitensteg ab 119), Z 198
QV_CUT_R = 37.5                 # Hohlrad Ø 73 + 1 Luft radial
QV_CUT_BOX = (45.5, 20.5)       # Pfosten Ø 9 bei x ±40 / y ±15 + 1 Luft
QV_CUT_ECKE = 5.5                # Feldecken mit r 5,5 um die Pfostenmitten gerundet
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
# OCS hochkant, Platine parallel zur Wand, Bauteilseite (Stecker) zur Wand, Rohr nach vorn: Rohr- und Tüllenachse auf YS.
# Maße von Alex (Geodreieck / Schieblehre), Löcher und Stecker vom Foto. Platinenmaß u ab dem J2-Ende (oben), v ab der
# Tüllenkante. Dummy = Platine + Hüllquader Rohrseite (dickste Stelle mit Clip) + Steckergehäuse 6 hoch auf der Rückseite.
TUELLE_D = 5.5                  # Tüllenhals
STECKER_H = 6.0


def _ocs(x0, x1, z1, L, d, mitte, vorn, ueber, tu, seite, stecker, loecher, rohr_b, rohr_t, clip_u, dreh=False):
    """seite -1: Tüllen nach links (Tüllenkante x0), +1: nach rechts (x1). stecker / loecher in (v, u).
    Rohrseite: Rohr rohr_b breit mittig, rohr_t vor der Platine; Clip mit Muttern über die volle Breite bei clip_u (u0, u1),
    vorn bis zur dicksten Stelle. dreh: Platine (Löcher, Stecker) 180° um ihre Mitte gedreht, Tüllen bleiben."""
    if dreh:
        W = x1 - x0
        stecker = [(W - v1, W - v0, L - u1, L - u0) for v0, v1, u0, u1 in stecker]
        loecher = {k: (W - v, L - u) for k, (v, u) in loecher.items()}
    ob = YS + mitte                                                       # Platinen-Vorderseite (Rohrseite)
    vx = (lambda v: x0 + v) if seite < 0 else (lambda v: x1 - v)
    kante = x0 if seite < 0 else x1
    xm = (x0 + x1) / 2
    return dict(x=(x0, x1), z=(z1 - L, z1), y=(ob + d, ob), vorn=ob - vorn, hinten=ob + d + STECKER_H,
                rohr=(xm - rohr_b / 2, xm + rohr_b / 2, ob - rohr_t), clip=(z1 - clip_u[1], z1 - clip_u[0]),
                tz=tuple(z1 - u for u in tu), tx=(kante, kante + seite * ueber),
                stecker=[(min(vx(v0), vx(v1)), max(vx(v0), vx(v1)), z1 - u1, z1 - u0) for v0, v1, u0, u1 in stecker],
                loch={k: (vx(v), z1 - u) for k, (v, u) in loecher.items()})


# hi 3F 3.0: 125 x 22 x 1,5, Rohr bis 12,5 vor der Platine (Mitte 6), Tüllen 20 von beiden Enden, 7 über die Kante.
# Löcher (Foto): Ecke u 2,8; Paar A u 25,4 (v 3,2 / v 20,3); Paar B versetzt u 96,8 (v 3,2) / u 100,9 (v 20,3).
# Alex 24.11.: so stünde das Lochbild spiegelverkehrt -> Platine 180° um die Mitte gedreht (dreh=True), Tüllen bleiben links.
OCS_HI = _ocs(-24.0, -2.0, Z_ZUFUHR + 20.0, 125.0, 1.5, 6.0, 12.5, 7.0, (20.0, 105.0), -1,
              [(0, 5.7, 6, 17.6), (0, 5.7, 108, 117), (17.2, 22, 73, 84)],                     # J2, J5, J3
              {'E': (3.2, 2.8), 'A1': (3.2, 25.2), 'A2': (20.3, 25.6), 'B1': (3.2, 96.7), 'B2': (20.3, 100.9)},
              12.5, 12.0, (55.0, 70.0), dreh=True)
# lo 3FRL 2.0: 79 x 24 x 1,6, 25 hoch (Stecker bis Clip), Rohrmitte 6,7 vor der Platine, Tüllen 21,5 (J2-Ende) / 60,
# 5,5 über die Kante. Löcher Ø 3,5 quer 18,5: Ende ohne J2 rund (u 75,6), J2-Ende Langloch 4,5 (u 3,6) + rund (u 10,1).
OCS_LO = _ocs(1.0, 25.0, Z_ZUFUHR + 21.5, 79.0, 1.6, 6.7, 17.4, 5.5, (21.5, 60.0), 1,
              [(18.0, 24.0, 20.0, 30.0)],                                                        # J2 (Gegenkante)
              {'L1': (2.75, 3.6), 'L2': (21.25, 3.6), 'R1': (2.75, 75.6), 'R2': (21.25, 75.6)},
              13.4, 13.4, (34.0, 46.0))
T_Z = 53.0                      # T-Stück 6 mm: Arme ±18 (Ø 8), Körper Ø 10, Abgang 15 nach unten
T_ARM, T_STEM = 18.0, 15.0
MIX = dict(x=(-11.5, 11.5), y=(-79.5, -0.5), z=(BT, BT + 24.0), ty=(YS, YS - 38.5), tip=BT + 29.0)   # quer zur Wand, Tüllen oben

# ---------------- Halter ----------------
H_FT = 4.0                      # Flanschdicke aller Halter (Y -4 .. 0)
# OCS-Steckhalter: Flansch an der Wand (2 x M3 x 16 + Mutter auf dem Raster, vor dem Aufstecken), Stützen Ø 5 bis an die
# Platinen-Rückseite, darauf Zapfen durch die Platinenlöcher. lo: 4 Zapfen Ø 3,4 stramm (Löcher Ø 3,5, am J2-Ende die
# Langlöcher -> Längenmaß unkritisch). hi: Löcher nur vom Foto -> 1 Zapfen Ø 2,6 in A1 (Lage), Stützen ohne Zapfen in
# A2 / B1 / B2, 2 Randleisten mit Rastwulst (u 40 .. 50, frei von Rohr, Clip und Tüllen) halten die Platine.
OH_POST_D = 5.0
OH = {'Hi': dict(ocs=OCS_HI, flansch=(-25.8, -0.2, 88.0, 173.0), schrauben=[(-15.0, 116.0), (-15.0, 156.0)],
                 zapfen={'A1': 2.6}, stuetzen=('A1', 'A2', 'B1', 'B2'), leisten=(OCS_HI['z'][1] - 50.0, OCS_HI['z'][1] - 40.0)),
      'Lo': dict(ocs=OCS_LO, flansch=(1.0, 25.0, 116.0, 195.0), schrauben=[(15.0, 126.0), (15.0, 156.0)],
                 zapfen={'L1': 3.4, 'L2': 3.4, 'R1': 3.4, 'R2': 3.4}, stuetzen=('L1', 'L2', 'R1', 'R2'), leisten=None)}
LEISTE_T, LEISTE_LUFT, WULST_D = 1.6, 0.2, 2.4
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
            frei &= fase_abstand(x, z) >= RW + 3.3 + r                           # Randsteg entlang der Fase
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
    # hi: U-Bogen R 25 nach unten (links ausladend), dann flache S-Kurve R 25 hoch in den linken T-Arm
    zb = hz1 - 2 * R_MIN
    th = math.acos(1 - (T_Z - zb) / (2 * R_MIN))
    xs = -T_ARM - 3.0 - 2 * R_MIN * math.sin(th)                       # 3 gerade vor dem T-Arm
    assert T_Z > zb and xs < hx - 2, 'hi-Schlauch passt nicht'
    w = math.degrees(th)
    hi_t = ([(hx, YS, hz1)] + _bogen((xs, hz1 - R_MIN), R_MIN, 90, 270, 12) + _bogen((xs, zb + R_MIN), R_MIN, -90, -90 + w, 4)[1:]
            + _bogen((-T_ARM - 3.0, T_Z - R_MIN), R_MIN, 90 + w, 90, 4)[1:] + [(-T_ARM, YS, T_Z)])
    out = {
        'ZuHi': ([(-TUBE_X + 0.5, YS, Z_ZUFUHR), (hx, YS, Z_ZUFUHR)], None),
        'ZuLo': ([(lx, YS, Z_ZUFUHR), (TUBE_X - 0.5, YS, Z_ZUFUHR)], None),
        'HiT': (hi_t, R_MIN),
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
