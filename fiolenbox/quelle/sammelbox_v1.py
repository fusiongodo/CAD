# Sammelbox v1 – Wanne + Stülpdeckel für 3 × 3 Fiolenboxen (große Box v2/v3: 82,4 × 63,4 × 42,1), einlagig, Deckel nach oben.
# Bauweise wie die Fiolenbox: Hals am Unterteil, Deckel klippt außen darüber. Rastung wie Raster-Skript (2 Nasen je Seite,
# alle 4 Seiten), aber mit 0,20 Spiel statt 0,06: 0,06 war nur auf EIN gedrucktes Unterteil abgestimmt, bei der Fiolenbox 11
# saß der Deckel damit bombenfest (Alex 26.11.2026, siehe passungen.html).
# Aufruf: ~/.cache/cq312/bin/python sammelbox_v1.py   → druck/sammelbox_16_3x3_v1_unterteil/_deckel .stl/.step
# Koordinaten: z = 0 Boden außen, Deckel in geschlossener Lage.
import cadquery as cq, os
HERE = os.path.dirname(os.path.abspath(__file__)); DRUCK = os.path.join(HERE, '..', 'druck')
NAME = 'sammelbox_16_3x3_v1'

# --- Inhalt ---
BOX_L, BOX_W, BOX_H = 82.4, 63.4, 42.1      # Fiolenbox v2 (Unterteil außen) mit Deckel v3
NX, NY = 3, 3
FUGE = 0.4                                  # Luft je Box und Richtung (gesamt 1,2 je Reihe): ohne Druck, ohne Klappern
# --- Wanne ---
FLOOR, H_BASE, NECK_H = 1.6, 14.0, 7.0      # Mulde 12,4 tief: die Unterteile der Fiolenboxen (bis z 17) schauen 4,6 heraus
NECK_T, NECK_CHAMFER = 1.4, 0.6
R_IN = 3.0                                  # Innenradius (Fiolenbox außen R 4)
FINGER_D = 22.0                             # Loch im Boden unter jeder Box: von unten herausdrücken
# --- Deckel ---
WALL, TOP, HEAD = 1.2, 2.0, 0.6
LCLR, P = 0.20, 0.40                        # Spiel Deckel ↔ Hals je Seite; Nase so viel über Rillenmitte (zieht auf den Absatz)
NOSE = LCLR + 0.35
SNAP_H, SNAP_L = 2.0, 30.0
GROOVE_D, GROOVE_H, GROOVE_L = 0.45, 2.2, 32.0

PX, PY = BOX_L + FUGE, BOX_W + FUGE
IL, IW = NX * PX, NY * PY                   # Mulde
neck_L, neck_W, r_neck = IL + 2 * NECK_T, IW + 2 * NECK_T, R_IN + NECK_T
NECK_IN = WALL + LCLR
L, W, R_OUT = neck_L + 2 * NECK_IN, neck_W + 2 * NECK_IN, r_neck + NECK_IN
ledge_z = H_BASE - NECK_H
SNAP_Z = ledge_z + 3.0
top_z = FLOOR + BOX_H + HEAD + TOP
assert max(L, W) <= 255.0, 'passt nicht aufs Druckbett (256)'


def rrect(l, w, r, h, z0=0):
    return cq.Workplane("XY").workplane(offset=z0).sketch().rect(l, w).vertices().fillet(r).finalize().extrude(h)


def ridge(sign_y, y_face, z_peak, depth, length, h, x0=0.0):
    d = depth * sign_y
    pts = [(y_face, z_peak - h / 2), (y_face + d, z_peak), (y_face, z_peak + h / 2)]
    return cq.Workplane("YZ").workplane(offset=x0 - length / 2).polyline(pts).close().extrude(length)


def place(solid, side):                     # 'y' = Seiten bei ±y; 'x' = um 90° gedreht → Seiten bei ±x
    return solid if side == 'y' else solid.rotate((0, 0, 0), (0, 0, 1), 90)


base = rrect(L, W, R_OUT, ledge_z).union(rrect(neck_L, neck_W, r_neck, NECK_H + 0.01, ledge_z - 0.01))
base = base.faces(">Z").edges().chamfer(NECK_CHAMFER)
base = base.cut(rrect(IL, IW, R_IN, H_BASE, FLOOR))
plaetze = [((i - (NX - 1) / 2) * PX, (j - (NY - 1) / 2) * PY) for i in range(NX) for j in range(NY)]
for x, y in plaetze:
    base = base.cut(cq.Workplane("XY").workplane(offset=-1).center(x, y).circle(FINGER_D / 2).extrude(FLOOR + 2))

iL, iW, ri = neck_L + 2 * LCLR, neck_W + 2 * LCLR, r_neck + LCLR
oL, oW, ro = iL + 2 * WALL, iW + 2 * WALL, ri + WALL
lid = rrect(oL, oW, ro, top_z - ledge_z, ledge_z).cut(rrect(iL, iW, ri, top_z - TOP - ledge_z + 0.01, ledge_z - 0.01))

n_snaps = 0
for side, length, face_half in (('y', neck_L, neck_W / 2), ('x', neck_W, neck_L / 2)):
    for x0 in (-length / 4, length / 4):
        for s in (1, -1):
            g = ridge(s, s * (face_half + 0.01), SNAP_Z, -(GROOVE_D + 0.01), GROOVE_L, GROOVE_H, x0 if side == 'y' else -x0)
            n = ridge(s, s * (face_half + LCLR + 0.01), SNAP_Z + P, -(NOSE + 0.01), SNAP_L, SNAP_H, x0 if side == 'y' else -x0)
            base = base.cut(place(g, side)); lid = lid.union(place(n, side)); n_snaps += 1

vol = lambda a, b: sum(x.Volume() for x in a.intersect(b).solids().vals())
ov = lambda dz: vol(base, lid.translate((0, 0, dz)))
boxen = None
for x, y in plaetze:
    b = rrect(BOX_L, BOX_W, 4.0, BOX_H, FLOOR).translate((x, y, 0))          # Fiolenbox außen R 4
    boxen = b if boxen is None else boxen.union(b)
print('%s: außen %.1f × %.1f × %.1f mm (Deckel %.1f × %.1f), Mulde %.1f × %.1f × %.1f, Rastnasen %d' % (NAME, L, W, top_z, oL, oW, IL, IW, H_BASE - FLOOR, n_snaps))
ok = True
for nme, sh in (('Unterteil', base), ('Deckel', lid)):
    v = sh.val(); k = len(sh.solids().vals()); ok &= v.isValid() and k == 1
    print('  %-9s Volumen %.1f cm³, gültig %s, Körper %d' % (nme, v.Volume() / 1000, v.isValid(), k))
vb, vl = vol(base, boxen), vol(lid, boxen); ok &= vb < 0.1 and vl < 0.1
print('  9 Boxen (Nennmaß) × Unterteil %.2f mm³, × Deckel %.2f mm³' % (vb, vl))
print('  Überdeckung Deckel/Unterteil: sitzend %.2f mm³ (Vorspannung), 1,5 mm Hub %.2f, 5 mm Hub %.2f' % (ov(0), ov(1.5), ov(5)))
for nme, sh in (('unterteil', base), ('deckel', lid)):
    cq.exporters.export(sh, f"{DRUCK}/{NAME}_{nme}.stl", tolerance=0.02, angularTolerance=0.1)
    cq.exporters.export(sh, f"{DRUCK}/{NAME}_{nme}.step")
print('PRÜFUNG', 'OK' if ok else 'FEHLER')
raise SystemExit(0 if ok else 1)
