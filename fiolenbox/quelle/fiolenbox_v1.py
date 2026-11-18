# Fiolenbox v1 – 4×3 Steckplätze, Stülpdeckel mit Rastnasen. CadQuery.
# Koordinaten: z=0 Boden außen, Deckel in geschlossener Lage modelliert.
import cadquery as cq, json, sys

# --- Fiole ---
VIAL_D, VIAL_H = 16.0, 38.0
CLR = 0.5                    # radiales Spiel Fiole↔Röhrchen
# --- Raster ---
COLS, ROWS, PITCH = 4, 3, 19.0
TUBE_W, TUBE_TOP = 1.2, 15.5 # Röhrchenwand, Oberkante Röhrchen (z)
GAP = 1.0                    # Luft Röhrchen ↔ Innenwand
# --- Unterteil ---
WALL, FLOOR, H_BASE, R_OUT = 1.6, 1.5, 24.0, 4.0
# --- Deckel ---
LID_WALL, LID_TOP, HEAD = 1.2, 1.6, 1.0   # Außenwand, Deckelplatte, Luft über Fiole
PLUG_W, PLUG_D, PLUG_CLR = 1.2, 7.0, 0.25 # Einsteckrand: Wand, Tiefe, Spiel je Seite
# --- Rastung (nur Längsseiten): Nase am Einsteckrand rastet in Rille der Unterteil-Innenwand ---
SNAP_Z = H_BASE - 3.5                      # Mitte Nase/Rille (z, geschlossen)
SNAP_H, SNAP_L = 2.0, 30.0                 # Breite (z), Länge (x)
SNAP_LID = 0.6                             # Nasenhöhe → beim Aufschieben 0.6 - PLUG_CLR = 0.35 mm Überdeckung
GROOVE_D, GROOVE_H, GROOVE_L = 0.45, 2.2, 32.0  # Rille: Tiefe, Breite (z), Länge
tube_od = VIAL_D + 2*CLR + 2*TUBE_W
in_L = (COLS-1)*PITCH + tube_od + 2*GAP
in_W = (ROWS-1)*PITCH + tube_od + 2*GAP
L, W = in_L + 2*WALL, in_W + 2*WALL
lid_top_z = FLOOR + VIAL_H + HEAD + LID_TOP

def rrect(l, w, r, h, z0=0):
    return cq.Workplane("XY").workplane(offset=z0).sketch().rect(l, w).vertices().fillet(r).finalize().extrude(h)

def ridge(sign_y, y_face, z_peak, depth, length, h=None):
    # dreieckiges Profil an der Fläche y_face, Spitze um depth nach außen (+|y|), entlang x extrudiert
    h = h or SNAP_H
    d = depth * sign_y
    pts = [(y_face, z_peak - h/2), (y_face + d, z_peak), (y_face, z_peak + h/2)]
    prof = cq.Workplane("YZ").workplane(offset=-length/2).polyline(pts).close().extrude(length)
    return prof

# Unterteil
r_in = R_OUT - WALL
base = rrect(L, W, R_OUT, H_BASE).cut(rrect(in_L, in_W, r_in, H_BASE, FLOOR))
pts = [((c-(COLS-1)/2)*PITCH, (r-(ROWS-1)/2)*PITCH) for c in range(COLS) for r in range(ROWS)]
for x, y in pts:   # Röhrchen einzeln (überlappende Kreise in einer Skizze ergeben kaputte Flächen)
    base = base.union(cq.Workplane("XY").workplane(offset=FLOOR - 0.01).center(x, y).circle(tube_od/2).extrude(TUBE_TOP - FLOOR + 0.01))
for x, y in pts:
    base = base.cut(cq.Workplane("XY").workplane(offset=FLOOR).center(x, y).circle(VIAL_D/2 + CLR).extrude(TUBE_TOP))
for s in (1, -1):
    base = base.cut(ridge(s, s*(in_W/2 - 0.01), SNAP_Z, GROOVE_D + 0.01, GROOVE_L, GROOVE_H))

# Deckel
p_out_L, p_out_W = in_L - 2*PLUG_CLR, in_W - 2*PLUG_CLR
r_p = r_in - PLUG_CLR
p_in_L, p_in_W = p_out_L - 2*PLUG_W, p_out_W - 2*PLUG_W
lid = rrect(L, W, R_OUT, lid_top_z - H_BASE, H_BASE)              # Außenkörper
lid = lid.union(rrect(p_out_L, p_out_W, r_p, PLUG_D + 0.01, H_BASE - PLUG_D))  # Einsteckrand
lid = lid.cut(rrect(p_in_L, p_in_W, r_p - PLUG_W, lid_top_z - LID_TOP - (H_BASE - PLUG_D), H_BASE - PLUG_D))
for s in (1, -1):
    lid = lid.union(ridge(s, s*(p_out_W/2 - 0.01), SNAP_Z, SNAP_LID + 0.01, SNAP_L))

info = dict(L=round(L,2), W=round(W,2), H=round(lid_top_z,2), H_BASE=H_BASE, pitch=PITCH, pts=pts,
            vial_d=VIAL_D, vial_h=VIAL_H, floor=FLOOR, hole_d=VIAL_D+2*CLR,
            plug_bottom=H_BASE-PLUG_D, tube_top=TUBE_TOP, snap_z=SNAP_Z)
print(json.dumps(info))
for n, sh in (('Unterteil', base), ('Deckel', lid)):
    v = sh.val(); print(n, 'Volumen %.1f cm³' % (v.Volume()/1000), 'gültig', v.isValid(), 'Körper', len(sh.solids().vals()))
out = sys.argv[1] if len(sys.argv) > 1 else "."
cq.exporters.export(base, f"{out}/fiolenbox_v1_unterteil.stl", tolerance=0.02, angularTolerance=0.1)
cq.exporters.export(lid,  f"{out}/fiolenbox_v1_deckel.stl",   tolerance=0.02, angularTolerance=0.1)
cq.exporters.export(base, f"{out}/fiolenbox_v1_unterteil.step")
cq.exporters.export(lid,  f"{out}/fiolenbox_v1_deckel.step")
json.dump(info, open(f"{out}/info.json", "w"))
