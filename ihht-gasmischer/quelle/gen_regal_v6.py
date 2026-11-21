# Regal oben v6: wie shelf5 aus gen5.py, nur kraeftiger.
#   Platte 1 -> 3 mm (waechst nach oben, Unterseite/Pfosten bleiben -> Sammler sitzt 2 mm hoeher),
#   Kantenrippen 1 -> 3 mm dick, 6 -> 8 mm tief, neu: Seitenrippen (geschlossener Rahmen),
#   Schraubdome Ø 8 -> 10, Schienen 8 -> 10 mm breit.
#   Gleich bleiben: Grundmass (Breite/Tiefe), Pfosten-Schraubloecher, Schienen-Schraubloecher (x/y).
# Aufruf: python3 gen_regal_v6.py <ausgabe.fs> <parts.json>
import sys, os, tempfile
here = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, here)
out_fs, out_json = sys.argv[1], sys.argv[2]
tmp = tempfile.mkdtemp()
sys.argv = ['gen5.py', os.path.join(tmp, 'c.fs'), os.path.join(tmp, 'p.json')]
import gen5
from gen5 import *
import json

PLATE = 3.0      # Plattenstaerke (v5: 1)
RIB_T = 3.0      # Rippendicke (v5: 1)
RIB_D = 8.0      # Rippentiefe unter der Platte (v5: 6)
DOME_D = 10.0    # Schraubdom (v5: 8)
RAIL_W = 10.0    # Schienenbreite (v5: 8)

gen.parts.clear()
top = SHELF_Z0 + PLATE
s = Part('shelf6', 'Regal oben v6 (nur Sammler Mix, verstaerkt)', BEIGE)
s.add(B(-SHELF_X, SHELF_X, SHELF_Y0, SHELF_Y1, SHELF_Z0, top))
for (ya, yb) in ((SHELF_Y0, SHELF_Y0 + RIB_T), (SHELF_Y1 - RIB_T, SHELF_Y1)):
    s.add(B(-(X_IN - 13), X_IN - 13, ya, yb, SHELF_Z0 - RIB_D, SHELF_Z0 + 0.5))        # Kantenrippen
for sx in (-1, 1):
    s.add(B(sx * 55.0, sx * 57.8, SHELF_Y0, SHELF_Y1, SHELF_Z0 - RIB_D, SHELF_Z0 + 0.5))  # Seitenrippen (innen neben den Pfosten)
    s.add(B(sx * 53.0, sx * 55.5, SHELF_Y0, SHELF_Y0 + RIB_T, SHELF_Z0 - RIB_D, SHELF_Z0 + 0.5))   # Ecke Kante/Seite schliessen
    s.add(B(sx * 53.0, sx * 55.5, SHELF_Y1 - RIB_T, SHELF_Y1, SHELF_Z0 - RIB_D, SHELF_Z0 + 0.5))
for name, label, xin, xout, zc, kind, col in SPL:
    SPL_Y0, SPL_Y1, SPL_CH = spy(zc)
    if zc == Z_PT:
        xa, xb = spl_extent(xin, xout)
        for hx in (xa - 5, xb + 5):
            s.add(B(hx - RAIL_W / 2, hx + RAIL_W / 2, SPL_Y0 - 12, SPL_Y1 + 12, top - 0.5, top + RAIL_H))   # Schiene
            s.add(Cz(hx, SPL_CH, SHELF_Z0 - RIB_D, SHELF_Z0 + 0.5, DOME_D))                                # Schraubdom
for (px, py) in SHELF_POSTS:
    s.cut(Cz(px, py, SHELF_Z0 - 1, top + 1, M3_CLR))
for (x0, x1) in ((-53.0, -43.65), (-28.65, 28.65), (43.65, 53.0)):                     # Fenster wie v5 (aussen bis an die Seitenrippe)
    s.cut(B(x0, x1, SHELF_Y0 + 7, SHELF_Y1 - 7, SHELF_Z0 - 1, top + 0.4))
for name, label, xin, xout, zc, kind, col in SPL:
    SPL_Y0, SPL_Y1, SPL_CH = spy(zc)
    if zc == Z_PT:
        xa, xb = spl_extent(xin, xout)
        rail_holes(s, top, xa, xb, SPL_HOLE_EXTRA)

L = list(gen5.L)
cut = next(i for i, l in enumerate(L) if l.startswith('function dev_'))
L = L[:cut]
L.insert(2, '// Regal oben v6 (verstaerkt): erzeugt von gen_regal_v6.py (Basis gen5.py). Platte %.0f, Rippen %.0f x %.0f, Dome Ø %.0f.'
         % (PLATE, RIB_T, RIB_D, DOME_D))
emit_feature(L, [s], 'rg6_', 'Regal oben v6', 'regalObenV6')
open(out_fs, 'w').write('\n'.join(L) + '\n')
json.dump([dict(name=s.name, label=s.label, col=s.col, printed=True, stages=s.stages, group='dev')], open(out_json, 'w'))
os.replace(os.path.join(tmp, 'p.json'), out_json.replace('.json', '_v5all.json'))
print('top', top, 'z0', SHELF_Z0)
