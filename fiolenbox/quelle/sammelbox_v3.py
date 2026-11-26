# Sammelbox v3 – Wanne + Stülpdeckel für 3 × 3 große Fiolenboxen, Verschluss: Klappe mit Mund am Deckel, Zunge an der Wanne.
# Idee Alex 26./27.11.2026: Am Deckel hängt eine frei drehbare Klappe mit einem Fenster (Mund). An der Wanne steht eine
#   Zunge heraus (Steg 3 dick, 15 breit). Klappe herunterschwenken: die Zunge steckt im Mund. Die Zunge ist ein eigenes
#   Teil und wird variiert, bis es gut sitzt. Befestigung mit M2-Schrauben (max. 20 lang), nichts ist lose.
# Umsetzung:
#   - Deckel und Wanne tragen nur je 2 Laschen pro Verschluss. Eine M2 x 20 geht quer durch Lasche 1 (Durchgang 2,5),
#     das Teil dazwischen (12 breit) und schneidet sich in Lasche 2 (Kernloch 1,8) ihr Gewinde.
#   - Klappe: dreht auf der M2 am Deckel. Oben ein Hebel: Hebel zum Deckel drücken = Klappe schwenkt unten auf.
#   - Zunge: Klotz zwischen den Wannen-Laschen, daraus der Steg. Die Unterseite des Stegs folgt der Bahn des Klappen-Stegs
#     (Hüllkurve + Luft) und trägt eine Raste. Rasthöhe = der Parameter der Testserie.
#   - Hält der Deckel? Der untere Steg der Klappe liegt unter der Zunge: Formschluss. Die Raste hält nur die Klappe zu.
# Aufruf: ~/.cache/cq312/bin/python sammelbox_v3.py   → druck/sammelbox_16_3x3_v3_*.stl / .step
# Koordinaten: z = 0 Boden außen, Deckel in geschlossener Lage. u = Abstand nach außen von der Außenfläche der langen Seite.
import cadquery as cq, math, os
HERE = os.path.dirname(os.path.abspath(__file__)); DRUCK = os.path.join(HERE, '..', 'druck')
NAME = 'sammelbox_16_3x3_v3'

# --- Inhalt ---
BOX_L, BOX_W, BOX_H = 82.4, 63.4, 42.1      # Fiolenbox v2 (Unterteil außen) mit Deckel v3
NX, NY = 3, 3
FUGE = 0.4                                  # Luft je Box und Richtung
# --- Wanne ---
FLOOR, H_BASE, NECK_H = 1.6, 14.0, 7.0      # Boden geschlossen (Alex 26.11.)
NECK_T, NECK_CHAMFER = 1.4, 0.6
R_IN = 3.0
# --- Deckel ---
WALL, TOP, HEAD = 1.2, 2.0, 0.6
LCLR = 0.20                                 # Spiel Deckel ↔ Hals je Seite
# --- Verschluss ---
M2_FREI, M2_KERN = 2.5, 1.8                 # Durchgang / Kernloch (liegende Löcher fallen enger aus)
TEIL_B, SPALT_B = 12.0, 12.3                # Breite von Klappenauge und Zungenklotz / lichte Weite zwischen den Laschen
LA1, LA2 = 3.0, 5.0                         # Lasche Kopfseite / Gewindeseite (3 + 12,3 + 4,7 = 20: M2 x 20; ab M2 x 16 greift sie)
ACH_U, ACH_Z = 8.0, 15.5                    # Klappenachse
DL_U, DL_Z0, DL_Z1 = 11.5, 12.0, 19.0       # Deckel-Laschen: Außenkante, unten, oben (darüber 45°-Stütze)
WL_U, WL_Z1 = 6.0, 7.0                      # Wannen-Laschen: Außenkante, oben (= Absatz)
ZS_U, ZS_Z = 3.0, 3.5                       # Schraube der Zunge
KL_U0, KL_U1 = 8.5, 11.5                    # Klappenplatte innen / außen (3 dick)
KL_B = 21.6                                 # Klappe unten breit (Mund 15,6 + 2 x 3)
KL_Z0, KL_STEG = 2.1, 1.2                   # Unterkante Klappe, Dicke des unteren Stegs (federt über die Raste)
ZU_B, ZU_D, ZU_TOP, ZU_TIP = 15.0, 3.0, 6.6, 14.0   # Zunge: Breite, Dicke, Oberseite z, Spitze u
LUFT = 0.3                                  # Luft Klappensteg ↔ Zungen-Unterseite
RASTE = [(0.6, 1), (0.8, 2), (1.0, 3)]      # Rasthöhe unter der Zunge, Kennpunkte. Überdeckung = Höhe - LUFT
RASTE_U, RASTE_B, RASTE_X = 12.5, 0.75, 3.0  # Raste: Mitte (u), halbe Länge (u), halbe Breite (x): nur 6 breit in der Mitte

PX, PY = BOX_L + FUGE, BOX_W + FUGE
IL, IW = NX * PX, NY * PY
neck_L, neck_W, r_neck = IL + 2 * NECK_T, IW + 2 * NECK_T, R_IN + NECK_T
NECK_IN = WALL + LCLR
L, W, R_OUT = neck_L + 2 * NECK_IN, neck_W + 2 * NECK_IN, r_neck + NECK_IN
ledge_z = H_BASE - NECK_H
top_z = FLOOR + BOX_H + HEAD + TOP
PLAETZE = [(sx * L / 4, sy) for sx in (-1, 1) for sy in (-1, 1)]     # Verschlussmitte x, Seite ±y
LASCHEN = ((-SPALT_B / 2 - LA1, -SPALT_B / 2, M2_FREI), (SPALT_B / 2, SPALT_B / 2 + LA2, M2_KERN))
STEG_Z = KL_Z0 + KL_STEG                    # Oberkante des Klappenstegs = Unterkante Mund
assert L <= 255.0 and W + 2 * DL_U <= 255.0, 'passt nicht aufs Druckbett (256)'


def rrect(l, w, r, h, z0=0):
    return cq.Workplane("XY").workplane(offset=z0).sketch().rect(l, w).vertices().fillet(r).finalize().extrude(h)


def box(x0, x1, u0, u1, z0, z1):            # lokal: y = u
    return cq.Workplane("XY").box(x1 - x0, u1 - u0, z1 - z0, centered=False).translate((x0, u0, z0))


def xloch(x0, x1, u, z, d):                 # Bohrung längs x, lokal
    return cq.Workplane("YZ").workplane(offset=x0).center(u, z).circle(d / 2).extrude(x1 - x0)


def profil(pts, x0, x1):                    # Profil in (u, z), längs x
    return cq.Workplane("YZ").workplane(offset=x0).polyline(pts).close().extrude(x1 - x0)


def setze(teil, xc, s):                     # lokales Teil (y = u) an die Seite s (±1) bei xc
    if s < 0: teil = teil.mirror("XZ")
    return teil.translate((xc, s * W / 2, 0))


def huellkurve():
    """Höchste Lage der Klappensteg-Oberkante beim Aufschwenken, als z über u (Raster 0,25)."""
    env = {}
    r = ACH_Z - STEG_Z
    for k in range(0, 4 * 45 + 1):
        t = math.radians(k / 4)
        for j in range(13):
            du = KL_U0 - ACH_U + (KL_U1 - KL_U0) * j / 12
            u, z = ACH_U + du * math.cos(t) + r * math.sin(t), ACH_Z + du * math.sin(t) - r * math.cos(t)
            q = round(u * 4) / 4
            env[q] = max(env.get(q, -9), z)
    return env


ENV = huellkurve()


def zunge(h, punkte):
    us = [KL_U0 + 0.25 * k for k in range(int((ZU_TIP - KL_U0) * 4) + 1)]
    unter = lambda u, hh: min(max(ENV.get(u, STEG_Z), STEG_Z) + LUFT - hh * max(0.0, 1 - abs(u - RASTE_U) / RASTE_B), ZU_TOP - 1.0)
    glatt = [(u, unter(u, 0.0)) for u in us]
    steg = profil([(WL_U + 0.3, ZU_TOP), (ZU_TIP, ZU_TOP)] + glatt[::-1] + [(WL_U + 0.3, ZU_TOP - ZU_D)], -ZU_B / 2, ZU_B / 2)
    z = box(-TEIL_B / 2, TEIL_B / 2, 0.15, WL_U + 0.4, 0.4, ZU_TOP).union(steg)
    if h > 0:                                                                 # Raste nur in der Mitte: der Klappensteg federt dort am weichsten
        ur = [u for u in us if abs(u - RASTE_U) <= RASTE_B + 0.01]
        z = z.union(profil([(u, unter(u, 0.0) + 0.3) for u in ur] + [(u, unter(u, h)) for u in ur[::-1]], -RASTE_X, RASTE_X))
    z = z.cut(xloch(-TEIL_B / 2 - 1, TEIL_B / 2 + 1, ZS_U, ZS_Z, M2_FREI))
    for k in range(punkte):                                                   # Kennpunkte oben auf dem Klotz
        z = z.cut(cq.Workplane("XY").center(-2.6 * (punkte - 1) / 2 + 2.6 * k, 1.6).circle(0.8).extrude(1.0).translate((0, 0, ZU_TOP - 0.6)))
    return z


def klappe():
    k = box(-TEIL_B / 2, TEIL_B / 2, KL_U0, KL_U1, 10.5, DL_Z1)               # Hals
    k = k.union(box(-5.0, 5.0, KL_U0, KL_U1, DL_Z1 - 0.1, 27.0))              # Hebel über der Achse
    k = k.union(box(-KL_B / 2, KL_B / 2, KL_U0, KL_U1, KL_Z0, 10.6))          # Platte mit Mund
    k = k.union(box(-TEIL_B / 2, TEIL_B / 2, ACH_U - 3.5, KL_U1, DL_Z0, DL_Z1))   # Auge
    k = k.cut(xloch(-TEIL_B / 2 - 1, TEIL_B / 2 + 1, ACH_U, ACH_Z, M2_FREI))
    return k.cut(box(-ZU_B / 2 - 0.3, ZU_B / 2 + 0.3, KL_U0 - 1, KL_U1 + 1, STEG_Z, ZU_TOP + 0.4))   # Mund


base = rrect(L, W, R_OUT, ledge_z).union(rrect(neck_L, neck_W, r_neck, NECK_H + 0.01, ledge_z - 0.01))
base = base.faces(">Z").edges().chamfer(NECK_CHAMFER)
base = base.cut(rrect(IL, IW, R_IN, H_BASE, FLOOR))
iL, iW, ri = neck_L + 2 * LCLR, neck_W + 2 * LCLR, r_neck + LCLR
oL, oW, ro = iL + 2 * WALL, iW + 2 * WALL, ri + WALL
lid = rrect(oL, oW, ro, top_z - ledge_z, ledge_z).cut(rrect(iL, iW, ri, top_z - TOP - ledge_z + 0.01, ledge_z - 0.01))

for xc, s in PLAETZE:
    for x0, x1, d in LASCHEN:
        dl = box(x0, x1, -0.2, DL_U, DL_Z0, DL_Z1).union(profil([(-0.2, DL_Z1 - 0.01), (DL_U, DL_Z1 - 0.01), (-0.2, DL_Z1 + DL_U + 0.2)], x0, x1))
        lid = lid.union(setze(dl.cut(xloch(x0 - 1, x1 + 1, ACH_U, ACH_Z, d)), xc, s))
        wl = box(x0, x1, -0.2, WL_U, 0, WL_Z1).cut(xloch(x0 - 1, x1 + 1, ZS_U, ZS_Z, d))
        base = base.union(setze(wl, xc, s))

vol = lambda a, b: sum(x.Volume() for x in a.intersect(b).solids().vals())
xc0 = L / 4
kl_zu = setze(klappe(), xc0, 1)
dreh = lambda sh, w: sh.rotate((0, W / 2 + ACH_U, ACH_Z), (1, W / 2 + ACH_U, ACH_Z), w)     # +w = unten nach außen (Seite +y)
hoch = lambda sh, dz: sh.translate((0, 0, dz))
boxen = None
for i in range(NX):
    for j in range(NY):
        b = rrect(BOX_L, BOX_W, 4.0, BOX_H, FLOOR).translate(((i - (NX - 1) / 2) * PX, (j - (NY - 1) / 2) * PY, 0))
        boxen = b if boxen is None else boxen.union(b)
print('%s: Körper außen %.1f × %.1f × %.1f mm, Deckel mit Laschen %.1f breit, Wanne mit Laschen %.1f breit' % (NAME, L, W, top_z, W + 2 * DL_U, W + 2 * WL_U))
print('  Zungen-Unterseite z über u:', ' '.join('%.1f:%.2f' % (u, ENV.get(u, STEG_Z) + LUFT) for u in (8.5, 10.0, 11.5, 12.5, 13.0, 14.0)))
ok = True
for nme, sh in (('Unterteil', base), ('Deckel', lid), ('Klappe', klappe())):
    v = sh.val(); k = len(sh.solids().vals()); ok &= v.isValid() and k == 1
    print('  %-9s Volumen %.1f cm³, gültig %s, Körper %d' % (nme, v.Volume() / 1000, v.isValid(), k))
t = [('9 Boxen × Unterteil', vol(base, boxen), 0), ('9 Boxen × Deckel', vol(lid, boxen), 0), ('Deckel × Unterteil', vol(lid, base), 0),
     ('Klappe zu × Deckel', vol(kl_zu, lid), 0), ('Klappe zu × Unterteil', vol(kl_zu, base), 0)]
z_ohne = setze(zunge(0.0, 0), xc0, 1)
t += [('Zunge × Unterteil', vol(z_ohne, base), 0), ('Zunge × Deckel', vol(z_ohne, lid), 0), ('Klappe zu × Zunge', vol(kl_zu, z_ohne), 0)]
t += [('Klappe %2d° × Zunge ohne Raste / Deckel / Unterteil' % w, vol(dreh(kl_zu, w), z_ohne) + vol(dreh(kl_zu, w), lid) + vol(dreh(kl_zu, w), base), 0) for w in (5, 10, 15, 20, 25, 30, 35, 40)]   # ab etwa 45° stößt der Hebel an die Deckelwand
t += [('Klappe zu, Deckel 1 hoch: Klappe × Zunge (muss sperren)', vol(hoch(kl_zu, 1.0), z_ohne), 1)]
t += [('Klappe 40° auf, Deckel %d hoch: Klappe + Deckel × Zunge + Unterteil' % dz, vol(hoch(dreh(kl_zu, 40), dz), z_ohne.union(base)) + vol(hoch(lid, dz), z_ohne.union(base)), 0) for dz in (1, 4, 10, 20)]
for name, v, sperrt in t:
    gut = v > 3 if sperrt else v < 0.1; ok &= gut
    print('  %-66s %8.2f mm³ %s' % (name, v, '' if gut else '<-- FEHLER'))
for h, p in RASTE:                                                            # Raste: wie stark greift sie, bei welchem Winkel
    zr = setze(zunge(h, p), xc0, 1)
    v = [(w, vol(dreh(kl_zu, w), zr)) for w in range(0, 31, 2)]
    print('  Zunge %d (Raste %.1f, Überdeckung %.1f): zu %.2f mm³, größte Überschneidung %.2f mm³ bei %d°' % ((p, h, h - LUFT, v[0][1]) + max((b, a) for a, b in v)))
    ok &= v[0][1] < 0.1

exp = lambda sh, n, step=False: (cq.exporters.export(sh, f"{DRUCK}/{NAME}_{n}.stl", tolerance=0.02, angularTolerance=0.1),
                                 step and cq.exporters.export(sh, f"{DRUCK}/{NAME}_{n}.step"))
exp(base, 'unterteil', True); exp(lid, 'deckel', True)
exp(klappe().rotate((0, 0, 0), (1, 0, 0), -90), 'klappe')                     # Drucklage: Außenseite unten
for h, p in RASTE:
    exp(zunge(h, p).rotate((0, 0, 0), (1, 0, 0), 180), 'zunge%d' % p)         # Drucklage: Oberseite unten
ges = cq.Assembly(name=NAME)
ges.add(base, name='Wanne', color=cq.Color(0.85, 0.85, 0.88))
ges.add(lid, name='Deckel', color=cq.Color(0.35, 0.6, 0.9, 0.6))
for k, (xc, s) in enumerate(PLAETZE):
    ges.add(setze(klappe(), xc, s), name='Klappe_%d' % (k + 1), color=cq.Color(0.93, 0.55, 0.2))
    ges.add(setze(zunge(*RASTE[1]), xc, s), name='Zunge_%d' % (k + 1), color=cq.Color(0.36, 0.79, 0.65))
ges.save(f"{DRUCK}/{NAME}_gesamt.step")
# Probestück: ein Verschlussplatz (+x, +y) aus Wanne und Deckel geschnitten, Deckel nur bis z 34 und mit Standfuß
fenster = lambda z0, z1: cq.Workplane("XY").box(64, 30, z1 - z0, centered=False).translate((xc0 - 32, W / 2 - 14, z0))
exp(base.intersect(fenster(-1, 30)), 'probe_unterteil')
fuss = cq.Workplane("XY").box(64, 14, 1.2, centered=False).translate((xc0 - 32, W / 2 - 14, 32.8))
exp(lid.intersect(fenster(0, 34)).union(fuss), 'probe_deckel')
print('PRÜFUNG', 'OK' if ok else 'FEHLER')
raise SystemExit(0 if ok else 1)
