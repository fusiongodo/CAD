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
QV_KOPF_D = 7.0                 # Freiraum hinten um jede QV-Schraube (Kopf M3 Ø 5,5 + Werkzeug); klinkt den Fasen-Steg aus
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


def _ocs(x0, x1, z1, L, d, mitte, vorn, ueber, tu, seite, stecker, loecher, rohr_b, rohr_t, clip_u, rohr_wand=None):
    """seite -1: Tüllen nach links (Tüllenkante x0), +1: nach rechts (x1). stecker / loecher in (v, u), Blick von vorn.
    Rohrseite: Rohr rohr_b breit mittig, rohr_t tief; Clip mit Muttern über die volle Breite bei clip_u (u0, u1), vorn tief
    (dickste Stelle). Ohne rohr_wand: Bauteilseite zur Wand, Rohr vorn, Tüllenachse auf YS. rohr_wand = Y der Platinen-
    Rohrseite: Rohr zur Wand, Bauteilseite vorn, Tüllenachse mitte davor. y = (Platinenfläche zur Wand, Fläche vorn)."""
    if rohr_wand is None:
        yo = YS + mitte
        yb, ry, cy, sy, ty = yo + d, (yo - rohr_t, yo), (yo - vorn, yo), (yo + d, yo + d + STECKER_H), YS
    else:
        yb = rohr_wand
        yo = yb - d
        ry, cy, sy, ty = (yb, yb + rohr_t), (yb, yb + vorn), (yo - STECKER_H, yo), yb + mitte
    vx = (lambda v: x0 + v) if seite < 0 else (lambda v: x1 - v)
    kante = x0 if seite < 0 else x1
    xm = (x0 + x1) / 2
    return dict(x=(x0, x1), z=(z1 - L, z1), y=(yb, yo), rohr=(xm - rohr_b / 2, xm + rohr_b / 2) + ry, clip_y=cy, stecker_y=sy,
                ty=ty, clip=(z1 - clip_u[1], z1 - clip_u[0]), tz=tuple(z1 - u for u in tu), tx=(kante, kante + seite * ueber),
                stecker=[(min(vx(v0), vx(v1)), max(vx(v0), vx(v1)), z1 - u1, z1 - u0) for v0, v1, u0, u1 in stecker],
                loch={k: (vx(v), z1 - u) for k, (v, u) in loecher.items()})


# hi 3F 3.0: 125 x 22 x 1,5, Rohr bis 12,5 vor der Platine (Mitte 6), Tüllen 20 von beiden Enden, 7 über die Kante.
# Löcher (Foto): Ecke u 2,8; Paar A u 25,4 (v 3,2 / v 20,3); Paar B versetzt u 96,8 (v 3,2) / u 100,9 (v 20,3).
# v2.8 (Alex 25.11. am Aufbau): hi sitzt mit dem Rohr zur Wand, Platine vorn. Von vorn: oben das Einzelloch E, darunter
# Paar A auf einer Höhe, weit unten Paar B links höher -> Fotodaten ohne Drehung. Rohrrückseite 1 vor dem Flansch (Y -4).
# Rohr 10 statt 12,5 breit angenommen: Stützen Ø 5 an A / B passten real daneben.
# v3.1 (Alex 25.11., Foto am Aufbau): doch Rohr nach vorn, Bauteilseite (Stecker) zur Wand, Tüllen links, J5 oben, J2 unten.
# Gegenüber v2.8 um die Tüllenachse gekippt: u -> 125 - u, v bleibt. Tüllenachse wieder auf YS (Y -22, wie QV-Schlauch und lo),
# Platine Y -14,5 .. -16 (3 näher an der Wand als die Stützen von v2.8).
# v3.2 (Alex 25.11., Foto von oben am Aufbau): montiert ist es mit dem Rohr zur Wand, Stecker vorn (wie v2.8, Lochbild ohne
# Drehung, J2 oben). Im v3.1-Halter lag das Rohr fast am Flansch, die Tülle 13 tiefer als beim lo. Jetzt Rohr zur Wand UND
# Tüllenachse auf YS: Platinen-Rohrseite Y -28, Rohr 12 vor dem Flansch frei (Schraubenköpfe passen darunter).
_HI_ARGS = (125.0, 1.5, 6.0, 12.5, 7.0, (20.0, 105.0), -1,
            [(0, 5.7, 6, 17.6), (0, 5.7, 108, 117), (17.2, 22, 73, 84)],                     # J2, J5, J3
            {'E': (3.2, 2.8), 'A1': (3.2, 25.2), 'A2': (20.3, 25.6), 'B1': (3.2, 96.7), 'B2': (20.3, 100.9)},
            10.0, 12.0, (55.0, 70.0))
HI_ROHR_WAND = YS - 6.0         # Tüllenachse (6 vor der Platine, Rohrseite) auf YS
OCS_HI = _ocs(-24.0, -2.0, Z_ZUFUHR + 20.0, *_HI_ARGS, rohr_wand=HI_ROHR_WAND)
# v3.0 (Alex 25.11.): OCS Mix ist ein zweiter 3F 3.0. Hochkant rechts unter dem QV lo, gleiche Lage wie hi (Rohr zur Wand,
# Tüllen nach links, Einlass oben Z 132, Auslass unten Z 47). 4,35 unter der QV-Platte, 1 neben dem Knotenblech-Flansch.
OCS_MIX = _ocs(73.0, 95.0, 152.0, *_HI_ARGS, rohr_wand=HI_ROHR_WAND)
# lo 3FRL 2.0: 79 x 24 x 1,6, 25 hoch (Stecker bis Clip), Rohrmitte 6,7 vor der Platine, Tüllen 21,5 (J2-Ende) / 60,
# 5,5 über die Kante. Löcher Ø 3,5 quer 18,5: Ende ohne J2 rund (u 75,6), J2-Ende Langloch 4,5 (u 3,6) + rund (u 10,1).
OCS_LO = _ocs(1.0, 25.0, Z_ZUFUHR + 21.5, 79.0, 1.6, 6.7, 17.4, 5.5, (21.5, 60.0), 1,
              [(18.0, 24.0, 20.0, 30.0)],                                                        # J2 (Gegenkante)
              {'L1': (2.75, 3.6), 'L2': (21.25, 3.6), 'R1': (2.75, 75.6), 'R2': (21.25, 75.6)},
              13.4, 13.4, (34.0, 46.0))
# v3.0: T-Stück als Druckteil zwischen lo-Auslass (X 30,5 / Y -22 / Z 135) und Mix-Einlass (X 66 / Y -11,5 / Z 132),
# hi kommt von unten (Y -11,5). Block mit Bohrungen Ø 4, 3 Tüllen (Hals 6,6 / Wulst 7,5 / Spitze 6,2) für Schlauch 6 innen,
# Flansch mit 2 x M3 x 16 auf dem Raster (unter dem Block, neben dem hi-Schlauch).
# v3.1: hi und Mix mit Rohr vorn -> alle drei Anschlüsse auf Y -22, nur noch 3 Höhenversatz lo / Mix im Block.
TS_X = 48.25                                   # Abgang nach unten (hi)
TS_BLOCK = (42.5, 54.0, -25.3, 127.5, 140.5)   # x0, x1, y vorn (= Druckbett, 3,3 vor der Tüllenachse: Tüllen liegen mit Flachstelle auf), z0, z1
TS_BEINE = ((31.0, 40.0, 127.5), (56.5, 69.0, 126.0))   # v3.4: Beine (x0, x1, z1) unter den Schraubstellen bis aufs Druckbett; dazwischen der hi-Schlauch, rechts 1 Luft zum Mix-Schlauch
TS_MUTTER_SW = 5.9                             # Sechskant-Schacht von vorn: M3-Mutter fällt bis auf den Flansch
TS_FL = (31.0, 69.0, 111.0, 140.5)             # Flansch x0, x1, z0, z1
TS_HOLES = [(35.0, 116.0), (65.0, 116.0)]
# v3.3 (26.11., Alex): Luftkanäle größer. Bohrung 5 statt 4, Tülle außen 7, 3 Sägezahn-Wellen bis 7,5 (steile Flanke zum Block).
TS_BARB, TS_BORE = 10.0, 5.0
TS_BARB_D = ((0.0, 9.2, 7.0),)                                      # (ab Block, bis, Ø): glatter Kern
TS_WELLEN = ((1.5, 3.5), (4.5, 6.5), (7.5, 9.2))                    # (ab Block, bis): Ø 7,5 am Block-Ende, Ø 7,0 zur Spitze
TS_WELLE_D, TS_SPITZE_D = 7.5, 6.2                                  # Wellenscheitel, Spitze (Fase über die letzten 0,8)
TS_LO = (OCS_LO['ty'], OCS_LO['tz'][1])        # Y, Z des lo-Anschlusses
TS_MIX = (OCS_MIX['ty'], OCS_MIX['tz'][0])     # Y, Z des Mix-Anschlusses
# MIX: alter Platz des kleinen OCS Mix auf der Grundplatte. Seit v3.0 unbenutzt, die Kabelbinder-Schlitze bleiben (gedruckt).
MIX = dict(x=(-11.5, 11.5), y=(-79.5, -0.5), z=(BT, BT + 24.0), ty=(YS, YS - 38.5), tip=BT + 29.0)   # quer zur Wand, Tüllen oben

# ---------------- Halter ----------------
H_FT = 4.0                      # Flanschdicke aller Halter (Y -4 .. 0)
# OCS-Steckhalter: Flansch an der Wand (2 x M3 x 16 + Mutter auf dem Raster, vor dem Aufstecken), Stützen Ø 5 bis an die
# Platinen-Rückseite, darauf Zapfen durch die Platinenlöcher. lo: 4 Zapfen Ø 3,4 stramm (Löcher Ø 3,5, am J2-Ende die
# Langlöcher -> Längenmaß unkritisch). hi: Löcher nur vom Foto -> 1 Zapfen Ø 2,6 in A1 (Lage), Stützen ohne Zapfen in
# A2 / B1 / B2, 2 Randleisten mit Rastwulst (u 40 .. 50, frei von Rohr, Clip und Tüllen) halten die Platine.
OH_POST_D = 5.0
OH = {'Hi': dict(ocs=OCS_HI, flansch=(-26.4, 0.4, 88.0, 173.0), schrauben=[(-15.0, 116.0), (-15.0, 156.0)],
                 zapfen={'A1': 2.6}, stuetzen=('A1', 'A2', 'B1', 'B2'), leisten=(OCS_HI['z'][1] - 50.0, OCS_HI['z'][1] - 40.0), fuss=8.0),
      'Lo': dict(ocs=OCS_LO, flansch=(0.6, 26.0, 116.0, 195.0), schrauben=[(15.0, 126.0), (15.0, 156.0)],
                 zapfen={}, stuetzen=('L1', 'L2', 'R1', 'R2'), leisten=None, post_d=7.0, m2=('L1', 'L2', 'R1', 'R2'))}
# v3.0 Mix: wie Hi, aber Schrauben neben dem Rohr (Kopf 3 hoch passt nicht unter das Rohr, 1,5 Luft) und Ausschnitt um die
# Träger-Mutter (75, 76). Flansch rechts bis 99,5 (Knotenblech ab 100,5).
OH['Mix'] = dict(ocs=OCS_MIX, flansch=(70.6, 99.5, 46.5, 131.5), schrauben=[(75.0, 96.0), (95.0, 66.0)],
                 zapfen={'A1': 2.6}, stuetzen=('A1', 'A2', 'B1', 'B2'), leisten=(OCS_MIX['z'][1] - 50.0, OCS_MIX['z'][1] - 40.0),
                 frei=[(75.0, 76.0)], fuss=8.0)   # fuss (v3.2): Stützen 24 lang -> je ein Steg 8 lang zur Platinenmitte hin
# v2.7 lo: Zapfen brachen beim Abnehmen ab -> M2 von vorn durch Platine und Stütze, Mutter seitlich in einen Schlitz 1,2 unter
# der Platine eingeschoben (M2 x 6 reicht, bis M2 x 14 geht). Stützen Ø 7 für die Mutter (SW 4).
M2_CLR, M2_MUTTER_SW, M2_MUTTER_H, M2_MUTTER_AB = 2.4, 4.2, 1.9, 1.2
M2_FLANSCH_T = 3.0              # v2.9: Sechskanttasche hinten im lo-Flansch, M2 x 14 greift voll, Spitze bleibt in der Tasche
LEISTE_T, LEISTE_LUFT, WULST_D = 2.2, 0.2, 2.4   # v2.4: 2,2 statt 1,6 (Leisten brachen ab), Druck auf der Seite
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


def _bogen(c, r, a0, a1, n=8, y=YS):
    """Kreisbogen in der XZ-Ebene (Y = y): Mittelpunkt c = (x, z), Winkel a0 -> a1 in Grad."""
    return [(c[0] + r * math.cos(math.radians(a0 + (a1 - a0) * k / n)), y,
             c[1] + r * math.sin(math.radians(a0 + (a1 - a0) * k / n))) for k in range(n + 1)]


HIT_X = 36.0                    # hi-Schlauch steigt bei X 36 (links am Patientenschlauch vorbei), S-Kurve auf den T-Abgang


def schlaeuche():
    """Schlauch-Mittellinien als Punktlisten (X, Y, Z) und kleinster Biegeradius je Strecke."""
    hz1, hx, hy = OCS_HI['tz'][1], OCS_HI['tx'][1], OCS_HI['ty']
    lx, mx, my = OCS_LO['tx'][1], OCS_MIX['tx'][1], OCS_MIX['ty']
    R = R_MIN
    # hi: U-Bogen nach links unten, waagerecht unter hi / lo durch, Bogen hoch bei HIT_X, S-Kurve auf den T-Abgang
    zu = hz1 - 2 * R                                                    # 38,5 waagerecht
    ze = TS_BLOCK[3] - TS_BARB - 2.0                                    # 2 gerade vor der Tülle
    th = math.acos(1 - (TS_X - HIT_X) / (2 * R))
    zs = ze - 2 * R * math.sin(th)
    assert zs >= zu + R and HIT_X - R > hx - 3.0, 'hi-Schlauch passt nicht'
    w = math.degrees(th)
    hi_t = ([(hx, hy, hz1)] + _bogen((hx - 3.0, hz1 - R), R, 90, 270, 12, hy)   # 3 gerade auf der Tülle
            + _bogen((HIT_X - R, zu + R), R, -90, 0, 8, hy)
            + _bogen((HIT_X + R, zs), R, 180, 180 - w, 4, hy) + _bogen((TS_X - R, ze), R, -w, 0, 4, hy)[1:]
            + [(TS_X, hy, ze + 2.0)])
    hi_t = [q for k, q in enumerate(hi_t) if k == 0 or math.dist(q, hi_t[k - 1]) > 1e-6]
    out = {
        'ZuHi': ([(-TUBE_X + 0.5, YS, Z_ZUFUHR), (hx, YS, Z_ZUFUHR)], None),   # hi-Tülle liegt 10,5 weiter hinten, Schlauch versetzt real
        'ZuLo': ([(lx, YS, Z_ZUFUHR), (TUBE_X - 0.5, YS, Z_ZUFUHR)], None),
        'HiT': (hi_t, R),
        'LoT': ([(lx, TS_LO[0], TS_LO[1]), (TS_BLOCK[0] - TS_BARB, TS_LO[0], TS_LO[1])], None),
        'TMix': ([(TS_BLOCK[1] + TS_BARB, TS_MIX[0], TS_MIX[1]), (mx, TS_MIX[0], TS_MIX[1])], None),
    }
    for sx, n in ((-1, 'AbHi'), (1, 'AbLo')):                             # Ablass: Bogen in XY bis an die Wand
        cx, cy, r = sx * (TUBE_X - 0.5), YS + R_MIN, R_MIN
        f1 = math.acos((cy + 0.5) / r)                                    # bis 0,5 vor die Wand (dann im Durchlass)
        pts = []
        for k in range(9):
            f = f1 * k / 8
            pts.append((cx - sx * r * math.sin(f), cy - r * math.cos(f), Z_ABLASS))
        out[n] = (pts, r)
    pz = OCS_MIX['tz'][1]                                                 # Patient: Mix-Auslass unten, Bogen in XY nach vorn
    px = mx - 3.0                                                         # 3 gerade auf der Tülle
    out['Patient'] = ([(mx, my, pz)] + [(px - R * math.sin(math.radians(90 * k / 8)), my - R + R * math.cos(math.radians(90 * k / 8)), pz) for k in range(9)]
                      + [(px - R, my - R - 15, pz)], R)
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
