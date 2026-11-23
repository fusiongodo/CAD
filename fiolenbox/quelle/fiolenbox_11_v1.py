# Fiolenbox 11 v1 – wie Fiolenbox v2/v3, aber für Fiolen Ø 11,6 × 32 mm. Raster 4×3 oder 5×4 (Aufruf: python fiolenbox_11_v1.py 4x3), Deckel klippt außen über Hals. CadQuery.
# Unterteil = Bauweise v2 (Lochebene + Hals + Rille), Deckel = Rastung v3 (Ring 6: Spiel 0,06, Nase 0,41, 0,40 über Rillenmitte).
# Koordinaten: z=0 Boden außen, Deckel in geschlossener Lage.
import cadquery as cq, json, sys, os
HERE = os.path.dirname(os.path.abspath(__file__)); DRUCK = os.path.join(HERE, '..', 'druck')

# --- Fiole ---
VIAL_D, VIAL_H = 11.6, 32.0
CLR = 0.5                     # radiales Spiel Fiole↔Loch (wie v2)
# --- Raster (Steg zwischen Löchern 2 mm wie v2) ---
COLS, ROWS = map(int, (sys.argv[1] if len(sys.argv) > 1 else '5x4').split('x'))
NAME = f'fiolenbox_11_{COLS}x{ROWS}_v1'
hole_d = VIAL_D + 2*CLR
PITCH = hole_d + 2.0
EDGE, OUTER = 2.6, 1.6        # wie v2
FLOOR, HOLE_CHAMFER = 1.5, 0.8
# --- Unterteil ---
H_BASE, R_OUT, NECK_H = 20.0, 4.0, 7.0      # Fiolen ragen 1,5+32-20 = 13,5 mm heraus
NECK_CHAMFER = 0.8
NECK_IN = 1.85                              # Hals zurückgesetzt (wie v2: Wand 1,6 + Spiel 0,25)
GROOVE_D, GROOVE_H, GROOVE_L = 0.45, 2.2, 32.0
# --- Deckel (v3 = Ring 6) ---
WALL, TOP, HEAD = 1.6, 1.6, 1.0
LCLR, P = 0.06, 0.40
NOSE = LCLR + 0.35
SNAP_H, SNAP_L = 2.0, 30.0

L = (COLS-1)*PITCH + hole_d + 2*EDGE + 2*OUTER
W = (ROWS-1)*PITCH + hole_d + 2*EDGE + 2*OUTER
neck_L, neck_W, r_neck = L - 2*NECK_IN, W - 2*NECK_IN, R_OUT - NECK_IN
ledge_z = H_BASE - NECK_H
SNAP_Z = ledge_z + 3.0
top_z = FLOOR + VIAL_H + HEAD + TOP

def rrect(l, w, r, h, z0=0):
    return cq.Workplane("XY").workplane(offset=z0).sketch().rect(l, w).vertices().fillet(r).finalize().extrude(h)
def ridge(sign_y, y_face, z_peak, depth, length, h):
    d = depth * sign_y
    pts = [(y_face, z_peak - h/2), (y_face + d, z_peak), (y_face, z_peak + h/2)]
    return cq.Workplane("YZ").workplane(offset=-length/2).polyline(pts).close().extrude(length)

base = rrect(L, W, R_OUT, ledge_z).union(rrect(neck_L, neck_W, r_neck, NECK_H + 0.01, ledge_z - 0.01))
base = base.faces(">Z").edges().chamfer(NECK_CHAMFER)
pts = [((c-(COLS-1)/2)*PITCH, (r-(ROWS-1)/2)*PITCH) for c in range(COLS) for r in range(ROWS)]
for x, y in pts:
    base = base.cut(cq.Workplane("XY").workplane(offset=FLOOR).center(x, y).circle(hole_d/2).extrude(H_BASE))
    base = base.cut(cq.Workplane("XY").workplane(offset=H_BASE - HOLE_CHAMFER).center(x, y)
                    .circle(hole_d/2).workplane(offset=HOLE_CHAMFER + 0.01).circle(hole_d/2 + HOLE_CHAMFER + 0.01).loft())
for s in (1, -1):
    base = base.cut(ridge(s, s*(neck_W/2 + 0.01), SNAP_Z, -(GROOVE_D + 0.01), GROOVE_L, GROOVE_H))

iL, iW, ri = neck_L + 2*LCLR, neck_W + 2*LCLR, r_neck + LCLR
oL, oW, ro = iL + 2*WALL, iW + 2*WALL, ri + WALL
lid = rrect(oL, oW, ro, top_z - ledge_z, ledge_z).cut(rrect(iL, iW, ri, top_z - TOP - ledge_z + 0.01, ledge_z - 0.01))
for s in (1, -1):
    lid = lid.union(ridge(s, s*(iW/2 + 0.01), SNAP_Z + P, -(NOSE + 0.01), SNAP_L, SNAP_H))

ov = lambda dz: sum(x.Volume() for x in base.intersect(lid.translate((0, 0, dz))).solids().vals())
info = dict(L=round(L,2), W=round(W,2), H=round(top_z,2), H_BASE=H_BASE, pitch=PITCH, pts=pts, vial_d=VIAL_D, vial_h=VIAL_H,
            floor=FLOOR, hole_d=hole_d, ledge_z=ledge_z, snap_z=SNAP_Z, version=f'11_{COLS}x{ROWS}_v1', cut_x=PITCH/2,
            base_stl=f'{NAME}_unterteil.stl', lid_stl=f'{NAME}_deckel.stl')
print(json.dumps(info))
print('Deckel außen %.2f × %.2f mm (Unterteil %.2f × %.2f)' % (oL, oW, L, W))
for n, sh in (('Unterteil', base), ('Deckel', lid)):
    v = sh.val(); print(n, 'Volumen %.1f cm³' % (v.Volume()/1000), 'gültig', v.isValid(), 'Körper', len(sh.solids().vals()))
print('Überdeckung: sitzend %.2f mm³ (Vorspannung), 1,5 mm Hub %.2f, 5 mm Hub %.2f' % (ov(0), ov(1.5), ov(5)))
out = DRUCK
for n, sh in (('unterteil', base), ('deckel', lid)):
    cq.exporters.export(sh, f"{out}/{NAME}_{n}.stl", tolerance=0.02, angularTolerance=0.1)
    cq.exporters.export(sh, f"{out}/{NAME}_{n}.step")
json.dump(info, open(f"{out}/info_11_{COLS}x{ROWS}.json", "w"))
