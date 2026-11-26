# Sammelbox v2 – Wanne + Stülpdeckel für 3 × 3 große Fiolenboxen, Verschluss über 4 Schubriegel (wie ein Türriegel).
# Alex 26.11.2026: kein fester Clip (Stärke unbekannt, teurer Druck), nichts Loses, Verschluss tauschbar.
#   Deckel und Wanne haben keine Rastnasen mehr, der Deckel liegt mit 0,20 Spiel auf dem Hals.
#   Am Deckel sitzen je Riegel 2 Ösen (A, B), an der Wanne 1 Öse (P). Der Riegel ist ein Vierkantstab durch A und B;
#   zugeschoben steckt er auch in P und hält den Deckel formschlüssig. Eine M3-Schraube (Kernloch 2,6) im Riegel
#   zwischen A und B ist Griff und Anschlag: der Riegel kann nicht herausfallen. Tauschen: Schraube raus, Riegel ziehen.
#   Alle Ösen sind geschlossene Rahmen, ihre Decke druckt als kurze Brücke (4,8). Keine Stützen.
# Aufruf: ~/.cache/cq312/bin/python sammelbox_v2.py   → druck/sammelbox_16_3x3_v2_*.stl (+ .step für Wanne und Deckel)
# Koordinaten: z = 0 Boden außen, Deckel in geschlossener Lage. u = Abstand nach außen von der Außenfläche der langen Seite.
import cadquery as cq, os
HERE = os.path.dirname(os.path.abspath(__file__)); DRUCK = os.path.join(HERE, '..', 'druck')
NAME = 'sammelbox_16_3x3_v2'

# --- Inhalt ---
BOX_L, BOX_W, BOX_H = 82.4, 63.4, 42.1      # Fiolenbox v2 (Unterteil außen) mit Deckel v3
NX, NY = 3, 3
FUGE = 0.4                                  # Luft je Box und Richtung
# --- Wanne ---
FLOOR, H_BASE, NECK_H = 1.6, 14.0, 7.0
NECK_T, NECK_CHAMFER = 1.4, 0.6
R_IN = 3.0
FINGER_D = 0.0                              # Loch im Boden unter jeder Box (22 = von unten herausdrücken); Alex 26.11.: Boden geschlossen
# --- Deckel ---
WALL, TOP, HEAD = 1.2, 2.0, 0.6
LCLR = 0.20                                 # Spiel Deckel ↔ Hals je Seite
# --- Schubriegel ---
RIEGEL = [(4.4, 4.0, 1), (4.2, 3.8, 2), (4.6, 4.2, 3)]   # Breite (u), Höhe (z), Kennpunkte; Nr. 1 = Nennmaß
R_LEN, R_LOCH, R_Q = 41.0, 2.6, 20.4        # Länge, Kernloch M3, Abstand Loch ↔ Riegelspitze
OE_U0, OE_U1, OE_UA = 1.6, 6.4, 7.8         # Ösenloch innen / außen, Außenkante der Öse
OE_Z0, OE_Z1 = 9.0, 13.6                    # Ösenloch unten / oben (Riegel 4,0 hoch: 0,6 Luft für die durchhängende Brücke)
OE_ZA0, OE_ZA1 = 7.2, 15.4                  # Öse außen unten / oben
A_X, B_X, P_X = (-22.0, -16.0), (1.0, 7.0), (9.0, 17.0)   # Ösen entlang der Seite, bezogen auf die Riegelmitte
P_SPALT = 0.4                               # Luft Wannen-Öse ↔ Deckelwand

PX, PY = BOX_L + FUGE, BOX_W + FUGE
IL, IW = NX * PX, NY * PY
neck_L, neck_W, r_neck = IL + 2 * NECK_T, IW + 2 * NECK_T, R_IN + NECK_T
NECK_IN = WALL + LCLR
L, W, R_OUT = neck_L + 2 * NECK_IN, neck_W + 2 * NECK_IN, r_neck + NECK_IN
ledge_z = H_BASE - NECK_H
top_z = FLOOR + BOX_H + HEAD + TOP
PLAETZE = [(sx * L / 4, sy) for sx in (-1, 1) for sy in (-1, 1)]     # Riegelmitte x, Seite ±y
assert L <= 255.0 and W + 2 * OE_UA + 6 <= 255.0, 'passt nicht aufs Druckbett (256)'


def rrect(l, w, r, h, z0=0):
    return cq.Workplane("XY").workplane(offset=z0).sketch().rect(l, w).vertices().fillet(r).finalize().extrude(h)


def blk(x0, x1, s, u0, u1, z0, z1):         # Quader an der Seite s (±1), u nach außen
    y0, y1 = sorted((s * (W / 2 + u0), s * (W / 2 + u1)))
    return cq.Workplane("XY").box(x1 - x0, y1 - y0, z1 - z0, centered=False).translate((x0, y0, z0))


def keil(x0, x1, s):                        # 45°-Stütze über der Deckel-Öse (im Druck liegt sie unter der Öse)
    pts = [(s * (W / 2 - 0.2), OE_ZA1 - 0.01), (s * (W / 2 + OE_UA), OE_ZA1 - 0.01), (s * (W / 2 - 0.2), OE_ZA1 + OE_UA + 0.2)]
    return cq.Workplane("YZ").workplane(offset=x0).polyline(pts).close().extrude(x1 - x0)


loch = lambda x0, x1, s: blk(x0 - 1, x1 + 1, s, OE_U0, OE_U1, OE_Z0, OE_Z1)

base = rrect(L, W, R_OUT, ledge_z).union(rrect(neck_L, neck_W, r_neck, NECK_H + 0.01, ledge_z - 0.01))
base = base.faces(">Z").edges().chamfer(NECK_CHAMFER)
base = base.cut(rrect(IL, IW, R_IN, H_BASE, FLOOR))
plaetze = [((i - (NX - 1) / 2) * PX, (j - (NY - 1) / 2) * PY) for i in range(NX) for j in range(NY)]
for x, y in plaetze if FINGER_D > 0 else []:
    base = base.cut(cq.Workplane("XY").workplane(offset=-1).center(x, y).circle(FINGER_D / 2).extrude(FLOOR + 2))

iL, iW, ri = neck_L + 2 * LCLR, neck_W + 2 * LCLR, r_neck + LCLR
oL, oW, ro = iL + 2 * WALL, iW + 2 * WALL, ri + WALL
lid = rrect(oL, oW, ro, top_z - ledge_z, ledge_z).cut(rrect(iL, iW, ri, top_z - TOP - ledge_z + 0.01, ledge_z - 0.01))


def riegel(b, h, punkte, x_spitze=0.0):
    """Riegel liegend: x von der Spitze (x_spitze) nach hinten, u ab 0, z ab 0. Druck in dieser Lage."""
    r = cq.Workplane("XY").box(R_LEN, b, h, centered=False).edges("|Y").chamfer(0.6)
    r = r.cut(cq.Workplane("XY").center(R_LEN - R_Q, b / 2).circle(R_LOCH / 2).extrude(h + 1).translate((0, 0, -0.5)))
    for k in range(punkte):                                                   # Kennpunkte oben am hinteren Ende
        r = r.cut(cq.Workplane("XY").center(3.0 + 2.6 * k, b / 2).circle(0.8).extrude(1.0).translate((0, 0, h - 0.5)))
    return r.translate((x_spitze - R_LEN, 0, 0))


def riegel_im_bau(xc, s, tip):              # Riegel Nr. 1 an seinem Platz, Spitze bei xc + tip
    b, h, _ = RIEGEL[0]
    r = riegel(b, h, 0, 0.0).translate((0, -b / 2, 0))
    if s < 0: r = r.mirror("XZ")
    return r.translate((xc + tip, s * (W / 2 + (OE_U0 + OE_U1) / 2), OE_Z0))


SCHRAUBE_KOPF = 5.6
TIP_ZU = B_X[0] - SCHRAUBE_KOPF / 2 + R_Q   # Schraubenkopf liegt an B an
TIP_AUF = A_X[1] + SCHRAUBE_KOPF / 2 + R_Q  # Schraubenkopf liegt an A an
for xc, s in PLAETZE:
    for x0, x1 in (A_X, B_X):
        lid = lid.union(blk(xc + x0, xc + x1, s, -0.2, OE_UA, OE_ZA0, OE_ZA1)).union(keil(xc + x0, xc + x1, s))
        lid = lid.cut(loch(xc + x0, xc + x1, s))
    x0, x1 = P_X
    base = base.union(blk(xc + x0, xc + x1, s, -0.2, OE_UA, 0, ledge_z)).union(blk(xc + x0, xc + x1, s, P_SPALT, OE_UA, ledge_z - 0.01, OE_ZA1))
    base = base.cut(loch(xc + x0, xc + x1, s))

vol = lambda a, b: sum(x.Volume() for x in a.intersect(b).solids().vals())
zu = auf = None
for xc, s in PLAETZE:
    a, b = riegel_im_bau(xc, s, TIP_ZU), riegel_im_bau(xc, s, TIP_AUF)
    zu = a if zu is None else zu.union(a); auf = b if auf is None else auf.union(b)
boxen = None
for x, y in plaetze:
    b = rrect(BOX_L, BOX_W, 4.0, BOX_H, FLOOR).translate((x, y, 0))
    boxen = b if boxen is None else boxen.union(b)
hoch = lambda sh, dz: sh.translate((0, 0, dz))
print('%s: Körper außen %.1f × %.1f × %.1f mm, mit Ösen %.1f breit, Mulde %.1f × %.1f × %.1f' % (NAME, L, W, top_z, W + 2 * OE_UA, IL, IW, H_BASE - FLOOR))
print('  Riegel: Spitze zu %.1f (Wannen-Öse %.0f..%.0f), offen %.1f, Hub %.1f' % (TIP_ZU, P_X[0], P_X[1], TIP_AUF, TIP_ZU - TIP_AUF))
ok = TIP_AUF < P_X[0] - 0.5 and TIP_ZU > P_X[1] + 0.5 and TIP_ZU - R_LEN <= A_X[0]
for nme, sh in (('Unterteil', base), ('Deckel', lid)):
    v = sh.val(); k = len(sh.solids().vals()); ok &= v.isValid() and k == 1
    print('  %-9s Volumen %.1f cm³, gültig %s, Körper %d' % (nme, v.Volume() / 1000, v.isValid(), k))
t = [('9 Boxen × Unterteil', vol(base, boxen), 0), ('9 Boxen × Deckel', vol(lid, boxen), 0), ('Deckel × Unterteil', vol(lid, base), 0),
     ('Riegel zu × Deckel', vol(zu, lid), 0), ('Riegel zu × Unterteil', vol(zu, base), 0), ('Riegel offen × Deckel', vol(auf, lid), 0),
     ('Riegel offen × Unterteil', vol(auf, base), 0)]
t += [('offen, Deckel %d hoch: Deckel + Riegel × Unterteil' % dz, vol(hoch(lid, dz), base) + vol(hoch(auf, dz), base), 0) for dz in (1, 3, 8, 16)]
t += [('zu, Deckel 1 hoch: Riegel × Unterteil (muss sperren)', vol(hoch(zu, 1.0), base), 1)]
for name, v, sperrt in t:
    gut = v > 5 if sperrt else v < 0.1; ok &= gut
    print('  %-52s %8.2f mm³ %s' % (name, v, '' if gut else '<-- FEHLER'))

exp = lambda sh, n, step=False: (cq.exporters.export(sh, f"{DRUCK}/{NAME}_{n}.stl", tolerance=0.02, angularTolerance=0.1),
                                 step and cq.exporters.export(sh, f"{DRUCK}/{NAME}_{n}.step"))
exp(base, 'unterteil', True); exp(lid, 'deckel', True)
for b, h, p in RIEGEL:
    exp(riegel(b, h, p), 'riegel%d' % p)
# Gesamt-STEP für Onshape: Wanne, Deckel und 4 Riegel (zugeschoben) als eigene Körper in Einbaulage
ges = cq.Assembly(name=NAME)
ges.add(base, name='Wanne', color=cq.Color(0.85, 0.85, 0.88))
ges.add(lid, name='Deckel', color=cq.Color(0.35, 0.6, 0.9, 0.6))
for k, (xc, s) in enumerate(PLAETZE):
    ges.add(riegel_im_bau(xc, s, TIP_ZU), name='Riegel_%d' % (k + 1), color=cq.Color(0.93, 0.55, 0.2))
ges.save(f"{DRUCK}/{NAME}_gesamt.step")
# Probestück: ein Riegelplatz (+x, +y) aus Wanne und Deckel geschnitten, Deckel nur bis z 26 und mit Standfuß
xc = L / 4
fenster = lambda z0, z1: cq.Workplane("XY").box(64, 26, z1 - z0, centered=False).translate((xc - 39, W / 2 - 14, z0))
exp(base.intersect(fenster(-1, 30)), 'probe_unterteil')
fuss = cq.Workplane("XY").box(64, 14, 1.2, centered=False).translate((xc - 39, W / 2 - 14, 24.8))
exp(lid.intersect(fenster(0, 26)).union(fuss), 'probe_deckel')
print('PRÜFUNG', 'OK' if ok else 'FEHLER')
raise SystemExit(0 if ok else 1)
