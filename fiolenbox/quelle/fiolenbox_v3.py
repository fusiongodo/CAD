# Fiolenbox v3 – nur neuer Deckel (Maße von Deckelring 6, Testdruck 18.11.2026). Unterteil = v2 unverändert.
# Koordinaten wie v2: z=0 Boden außen, Deckel in geschlossener Lage.
import cadquery as cq, json, sys, os
HERE = os.path.dirname(os.path.abspath(__file__)); DRUCK = os.path.join(HERE, '..', 'druck')

# Unterteil v2 (gedruckt) – Hals und Rille
L, W, R_OUT, H_BASE, NECK_H = 82.4, 63.4, 4.0, 24.0, 7.0
neck_L, neck_W, r_neck = L - 2*1.85, W - 2*1.85, R_OUT - 1.85
ledge_z, SNAP_Z = H_BASE - NECK_H, H_BASE - NECK_H + 3.0
FLOOR, VIAL_H, HEAD = 1.5, 38.0, 1.0

# Deckel v3 = Ring 6
CLR = 0.06            # Spiel je Seite
P = 0.40              # Nase 0,40 über Rillenmitte → zieht Deckel auf den Absatz
NOSE = CLR + 0.35     # Nasenhöhe 0,41
SNAP_H, SNAP_L = 2.0, 30.0
WALL, TOP = 1.6, 1.6
top_z = FLOOR + VIAL_H + HEAD + TOP

def rrect(l, w, r, h, z0=0):
    return cq.Workplane("XY").workplane(offset=z0).sketch().rect(l, w).vertices().fillet(r).finalize().extrude(h)
def ridge(sign_y, y_face, z_peak, depth, length, h):
    d = depth * sign_y
    pts = [(y_face, z_peak - h/2), (y_face + d, z_peak), (y_face, z_peak + h/2)]
    return cq.Workplane("YZ").workplane(offset=-length/2).polyline(pts).close().extrude(length)

iL, iW, ri = neck_L + 2*CLR, neck_W + 2*CLR, r_neck + CLR
oL, oW, ro = iL + 2*WALL, iW + 2*WALL, ri + WALL
lid = rrect(oL, oW, ro, top_z - ledge_z, ledge_z).cut(rrect(iL, iW, ri, top_z - TOP - ledge_z + 0.01, ledge_z - 0.01))
for s in (1, -1):
    lid = lid.union(ridge(s, s*(iW/2 + 0.01), SNAP_Z + P, -(NOSE + 0.01), SNAP_L, SNAP_H))

base = cq.importers.importStep(os.path.join(DRUCK, 'fiolenbox_v2_unterteil.step'))
ov = lambda dz: sum(x.Volume() for x in base.intersect(lid.translate((0, 0, dz))).solids().vals())
print('Deckel v3: außen %.2f × %.2f × %.2f mm, Volumen %.1f cm³, gültig %s' % (oL, oW, top_z, lid.val().Volume()/1000, lid.val().isValid()))
print('Überdeckung mit Unterteil v2: sitzend %.2f mm³ (Vorspannung), 1,5 mm Hub %.2f, 5 mm Hub %.2f' % (ov(0), ov(1.5), ov(5)))
cq.exporters.export(lid, os.path.join(DRUCK, 'fiolenbox_v3_deckel.stl'), tolerance=0.02, angularTolerance=0.1)
cq.exporters.export(lid, os.path.join(DRUCK, 'fiolenbox_v3_deckel.step'))
info = json.load(open(os.path.join(DRUCK, 'info.json')))
info.update(version='v3', base_stl='fiolenbox_v2_unterteil.stl', lid_stl='fiolenbox_v3_deckel.stl')
json.dump(info, open(os.path.join(DRUCK, 'info.json'), 'w'))
