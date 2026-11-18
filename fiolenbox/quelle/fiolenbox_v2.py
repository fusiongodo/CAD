# Fiolenbox v2 – 4×3 Steckplätze in durchgehender Ebene, Deckel klippt AUSSEN über einen Hals am Unterteil. CadQuery.
# Koordinaten: z=0 Boden außen, Deckel in geschlossener Lage modelliert.
import cadquery as cq, json, sys

# --- Fiole ---
VIAL_D, VIAL_H = 16.0, 38.0
CLR = 0.5                     # radiales Spiel Fiole↔Loch
# --- Raster ---
COLS, ROWS, PITCH = 4, 3, 19.0
EDGE = 2.6                    # Material Lochrand ↔ Außenwand (wie v1: Röhrchen 1.2 + Luft 1.0 + Wand… gerundet)
FLOOR = 1.5                   # Boden unter den Löchern
HOLE_CHAMFER = 0.8            # Fase oben am Loch (Einführhilfe)
# --- Unterteil ---
H_BASE, R_OUT = 24.0, 4.0     # Oberkante Ebene = Oberkante Hals
NECK_H = 7.0                  # Hals, über den der Deckel greift
# --- Deckel ---
LID_WALL, LID_TOP, HEAD = 1.6, 1.6, 1.0
LID_CLR = 0.25                # Spiel Deckel ↔ Hals je Seite
NECK_CHAMFER = 0.8            # Fase Halsoberkante außen (Einführschräge)
# --- Rastung (nur Längsseiten): Nase innen am Deckelrand rastet in Rille am Hals ---
SNAP_Z = H_BASE - NECK_H + 3.0             # Mitte Nase/Rille (z, geschlossen)
SNAP_H, SNAP_L = 2.0, 30.0
SNAP_LID = 0.6                             # Überdeckung beim Aufschieben: 0.6 - 0.25 = 0.35 mm
GROOVE_D, GROOVE_H, GROOVE_L = 0.45, 2.2, 32.0

hole_d = VIAL_D + 2*CLR
L = (COLS-1)*PITCH + hole_d + 2*EDGE + 2*1.6   # Außenmaß gleich v1
W = (ROWS-1)*PITCH + hole_d + 2*EDGE + 2*1.6
neck_L, neck_W = L - 2*(LID_WALL + LID_CLR), W - 2*(LID_WALL + LID_CLR)
r_neck = R_OUT - LID_WALL - LID_CLR
ledge_z = H_BASE - NECK_H
lid_top_z = FLOOR + VIAL_H + HEAD + LID_TOP

def rrect(l, w, r, h, z0=0):
    return cq.Workplane("XY").workplane(offset=z0).sketch().rect(l, w).vertices().fillet(r).finalize().extrude(h)

def ridge(sign_y, y_face, z_peak, depth, length, h):
    d = depth * sign_y
    pts = [(y_face, z_peak - h/2), (y_face + d, z_peak), (y_face, z_peak + h/2)]
    return cq.Workplane("YZ").workplane(offset=-length/2).polyline(pts).close().extrude(length)

# Unterteil: Körper bis Absatz + Hals, Ebene mit Löchern durchgehend (im Slicer: Wände + Infill)
base = rrect(L, W, R_OUT, ledge_z).union(rrect(neck_L, neck_W, r_neck, NECK_H + 0.01, ledge_z - 0.01))
base = base.faces(">Z").edges().chamfer(NECK_CHAMFER)          # Einführschräge Halskante
pts = [((c-(COLS-1)/2)*PITCH, (r-(ROWS-1)/2)*PITCH) for c in range(COLS) for r in range(ROWS)]
for x, y in pts:
    base = base.cut(cq.Workplane("XY").workplane(offset=FLOOR).center(x, y).circle(hole_d/2).extrude(H_BASE))
    base = base.cut(cq.Workplane("XY").workplane(offset=H_BASE - HOLE_CHAMFER).center(x, y)
                    .circle(hole_d/2).workplane(offset=HOLE_CHAMFER + 0.01).circle(hole_d/2 + HOLE_CHAMFER + 0.01).loft())
for s in (1, -1):
    base = base.cut(ridge(s, s*(neck_W/2 + 0.01), SNAP_Z, -(GROOVE_D + 0.01), GROOVE_L, GROOVE_H))

# Deckel: Haube, Rand sitzt auf dem Absatz auf
lid = rrect(L, W, R_OUT, lid_top_z - ledge_z, ledge_z)
lid = lid.cut(rrect(neck_L + 2*LID_CLR, neck_W + 2*LID_CLR, r_neck + LID_CLR, lid_top_z - LID_TOP - ledge_z + 0.01, ledge_z - 0.01))
for s in (1, -1):
    lid = lid.union(ridge(s, s*(neck_W/2 + LID_CLR + 0.01), SNAP_Z, -(SNAP_LID + 0.01), SNAP_L, SNAP_H))

info = dict(L=round(L,2), W=round(W,2), H=round(lid_top_z,2), H_BASE=H_BASE, pitch=PITCH, pts=pts,
            vial_d=VIAL_D, vial_h=VIAL_H, floor=FLOOR, hole_d=hole_d, ledge_z=ledge_z, snap_z=SNAP_Z, version='v2', cut_x=9.5)
print(json.dumps(info))
for n, sh in (('Unterteil', base), ('Deckel', lid)):
    v = sh.val(); print(n, 'Volumen %.1f cm³' % (v.Volume()/1000), 'gültig', v.isValid(), 'Körper', len(sh.solids().vals()))
out = sys.argv[1] if len(sys.argv) > 1 else "."
for n, sh in (('unterteil', base), ('deckel', lid)):
    cq.exporters.export(sh, f"{out}/fiolenbox_v2_{n}.stl", tolerance=0.02, angularTolerance=0.1)
    cq.exporters.export(sh, f"{out}/fiolenbox_v2_{n}.step")
json.dump(info, open(f"{out}/info.json", "w"))
