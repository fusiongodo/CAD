# Fiolenbox mit beliebigem Raster, auch groß (z. B. 10×10). Bauweise wie v2/v3 bzw. Fiolenbox 11:
# Unterteil = Lochebene + Hals + Rille, Deckel = Rastung Ring 6 (Spiel 0,06, Nase 0,41, 0,40 über Rillenmitte).
# Neu für große Boxen: Seiten ≥ 100 mm bekommen 2 Rastnasen (bei ±¼ Seitenlänge), dann auf allen vier Seiten;
# Deckelplatte 2,0 statt 1,6 (große freie Spannweite).
# Aufruf: python fiolenbox_raster_v1.py <16|11> <SPALTENxREIHEN>   z. B. 16 10x10
import cadquery as cq, json, sys, os
HERE = os.path.dirname(os.path.abspath(__file__)); DRUCK = os.path.join(HERE, '..', 'druck')

FIOLE = sys.argv[1] if len(sys.argv) > 1 else '16'
VIAL_D, VIAL_H, H_BASE = {'16': (16.0, 38.0, 24.0), '11': (11.6, 32.0, 20.0)}[FIOLE]
COLS, ROWS = map(int, (sys.argv[2] if len(sys.argv) > 2 else '10x10').split('x'))
NAME = f'fiolenbox_{FIOLE}_{COLS}x{ROWS}_v1'

CLR = 0.5                                   # radiales Spiel Fiole↔Loch
hole_d = VIAL_D + 2*CLR
PITCH = hole_d + 2.0                        # Steg 2 mm
EDGE, OUTER = 2.6, 1.6
FLOOR, HOLE_CHAMFER = 1.5, 0.8
R_OUT, NECK_H, NECK_CHAMFER, NECK_IN = 4.0, 7.0, 0.8, 1.85
GROOVE_D, GROOVE_H, GROOVE_L = 0.45, 2.2, 32.0
WALL, HEAD = 1.6, 1.0
LCLR, P = 0.06, 0.40
NOSE = LCLR + 0.35
SNAP_H, SNAP_L = 2.0, 30.0

L = (COLS-1)*PITCH + hole_d + 2*EDGE + 2*OUTER
W = (ROWS-1)*PITCH + hole_d + 2*EDGE + 2*OUTER
TOP = 2.0 if max(L, W) >= 100 else 1.6
neck_L, neck_W, r_neck = L - 2*NECK_IN, W - 2*NECK_IN, R_OUT - NECK_IN
ledge_z = H_BASE - NECK_H
SNAP_Z = ledge_z + 3.0
top_z = FLOOR + VIAL_H + HEAD + TOP

def rrect(l, w, r, h, z0=0):
    return cq.Workplane("XY").workplane(offset=z0).sketch().rect(l, w).vertices().fillet(r).finalize().extrude(h)
def ridge(sign_y, y_face, z_peak, depth, length, h, x0=0.0):
    d = depth * sign_y
    pts = [(y_face, z_peak - h/2), (y_face + d, z_peak), (y_face, z_peak + h/2)]
    return cq.Workplane("YZ").workplane(offset=x0 - length/2).polyline(pts).close().extrude(length)

# Rastnasen-Plan: (Seite, Positionen entlang der Seite)
def snap_plan():
    big = max(L, W) >= 100
    plan = []
    for side, length, face_half in (('y', neck_L, neck_W/2), ('x', neck_W, neck_L/2)):
        if not big and side == 'x':
            continue                       # kleine Box: nur Längsseiten (wie v2/v3)
        pos = [-length/4, length/4] if length >= 100 else [0.0]
        plan.append((side, face_half, pos))
    return plan

def place(solid, side):                    # 'y' = Seiten bei ±y; 'x' = um 90° gedreht → Seiten bei ±x
    return solid if side == 'y' else solid.rotate((0, 0, 0), (0, 0, 1), 90)

base = rrect(L, W, R_OUT, ledge_z).union(rrect(neck_L, neck_W, r_neck, NECK_H + 0.01, ledge_z - 0.01))
base = base.faces(">Z").edges().chamfer(NECK_CHAMFER)
pts = [((c-(COLS-1)/2)*PITCH, (r-(ROWS-1)/2)*PITCH) for c in range(COLS) for r in range(ROWS)]
holes = None
for x, y in pts:
    h = cq.Workplane("XY").workplane(offset=FLOOR).center(x, y).circle(hole_d/2).extrude(H_BASE)
    h = h.union(cq.Workplane("XY").workplane(offset=H_BASE - HOLE_CHAMFER).center(x, y)
                .circle(hole_d/2).workplane(offset=HOLE_CHAMFER + 0.01).circle(hole_d/2 + HOLE_CHAMFER + 0.01).loft())
    holes = h if holes is None else holes.union(h)
base = base.cut(holes)

iL, iW, ri = neck_L + 2*LCLR, neck_W + 2*LCLR, r_neck + LCLR
oL, oW, ro = iL + 2*WALL, iW + 2*WALL, ri + WALL
lid = rrect(oL, oW, ro, top_z - ledge_z, ledge_z).cut(rrect(iL, iW, ri, top_z - TOP - ledge_z + 0.01, ledge_z - 0.01))

n_snaps = 0
for side, face_half, pos in snap_plan():
    for x0 in pos:
        for s in (1, -1):
            g = ridge(s, s*(face_half + 0.01), SNAP_Z, -(GROOVE_D + 0.01), GROOVE_L, GROOVE_H, x0 if side == 'y' else -x0)
            n = ridge(s, s*(face_half + LCLR + 0.01), SNAP_Z + P, -(NOSE + 0.01), SNAP_L, SNAP_H, x0 if side == 'y' else -x0)
            base = base.cut(place(g, side)); lid = lid.union(place(n, side)); n_snaps += 1

ov = lambda dz: sum(x.Volume() for x in base.intersect(lid.translate((0, 0, dz))).solids().vals())
xs = sorted({p[0] for p in pts}); target = neck_L/4 if neck_L >= 100 else 0
cut_x = min(xs, key=lambda v: abs(v - target))
info = dict(L=round(L,2), W=round(W,2), H=round(top_z,2), H_BASE=H_BASE, pitch=PITCH, pts=pts, vial_d=VIAL_D, vial_h=VIAL_H,
            floor=FLOOR, hole_d=hole_d, ledge_z=ledge_z, snap_z=SNAP_Z, version=f'{FIOLE} mm v1', cut_x=cut_x,
            base_stl=f'{NAME}_unterteil.stl', lid_stl=f'{NAME}_deckel.stl')
print('%s: außen %.1f × %.1f × %.1f mm, Deckelplatte %.1f, Rastnasen %d' % (NAME, L, W, top_z, TOP, n_snaps))
for nme, sh in (('Unterteil', base), ('Deckel', lid)):
    v = sh.val(); print(' ', nme, 'Volumen %.1f cm³' % (v.Volume()/1000), 'gültig', v.isValid(), 'Körper', len(sh.solids().vals()))
print('  Überdeckung: sitzend %.2f mm³ (Vorspannung), 1,5 mm Hub %.2f, 5 mm Hub %.2f' % (ov(0), ov(1.5), ov(5)))
for nme, sh in (('unterteil', base), ('deckel', lid)):
    cq.exporters.export(sh, f"{DRUCK}/{NAME}_{nme}.stl", tolerance=0.02, angularTolerance=0.1)
    cq.exporters.export(sh, f"{DRUCK}/{NAME}_{nme}.step")
json.dump(info, open(f"{DRUCK}/info_{FIOLE}_{COLS}x{ROWS}.json", "w"))
