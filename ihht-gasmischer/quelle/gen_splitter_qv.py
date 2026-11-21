# Splitter hi + lo fuer das Exzenter-Quetschventil (Silikonschlauch 6/12 statt LZQ):
#   Wie Splitter hi/lo v4 aus gen5.py (Block, Flansch, Schienen-Langloecher, Tropfenkanal Ø 8, Aussenseite unveraendert),
#   nur die je zwei Innen-Tuellen (vorher Ø 10,75 fuer LZQ, Tropfen Ø 6,5) sind jetzt glatte Tuellen fuer den
#   Quetschventil-Schlauch 6/12: aussen Ø 8, innen Ø 6 (mind. 6 mm Durchgang), ohne Wellen, 12 mm lang,
#   an der Wurzel 45°-Fase 2 mm (gegen Abbrechen beim Ruckeln), an der Spitze 0,5-mm-Einfuehrfase.
#   Lage identisch mit v4 -> die neuen Teile sitzen exakt an der Stelle der alten.
# Aufruf: python3 gen_splitter_qv.py <ausgabe.fs> <parts.json>
import sys, os, tempfile
here = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, here)
out_fs, out_json = sys.argv[1], sys.argv[2]
tmp = tempfile.mkdtemp()
sys.argv = ['gen5.py', os.path.join(tmp, 'c.fs'), os.path.join(tmp, 'p.json')]
import gen5
from gen5 import *
import json

IN_OD, IN_ID, IN_L = 8.0, 6.0, 12.0    # Innen-Tuelle: aussen, Bohrung, Laenge ab Block
ROOT_CH, TIP_CH = 2.0, 0.5             # 45°-Fase an der Wurzel (radial = axial), Einfuehrfase an der Spitze

def tuelle_glatt(x, z, y0):
    # glatte Tuelle nach -Y (vom Block nach vorn Richtung Ventil)
    r = IN_OD / 2
    return [Cy(x, y0 + 0.5, y0 - IN_L + TIP_CH, z, IN_OD),
            K(x, z, y0 - IN_L + TIP_CH, y0 - IN_L, r, r - TIP_CH),                 # Einfuehrfase
            K(x, z, y0 + 0.3, y0 - ROOT_CH, r + ROOT_CH + 0.3, r)]                 # Wurzel-Fase 45°

LABELS = {'splHi': 'Splitter hi v5 QV (Quetschventil, innen 2 x Tuelle 8/6 glatt)',
          'splLo': 'Splitter lo v5 QV (Quetschventil, innen 2 x Tuelle 8/6 glatt)'}
gen.parts.clear()
parts = []
for name, label, xin, xout, zc, kind, col in SPL:
    if name not in LABELS:
        continue
    SPL_Y0, SPL_Y1, SPL_CH = spy(zc)
    m = Part(name + 'QV', LABELS[name], col)
    floor = DECK_REF + RAIL_H
    xa, xb = spl_extent(xin, xout)
    z_out = Z_OUT.get(kind, zc)
    prof = TUELLE_PROF.get(kind)
    top = max(zc + 8.5, z_out + prof[2] + 2.5)
    m.add(B(xa, xb, SPL_Y0, SPL_Y1, floor, top))
    m.add(B(xa - 10, xb + 10, SPL_Y0 - 8, SPL_Y1 + 8, floor, floor + 3))
    for x in xin:
        m.add(*tuelle_glatt(x, zc, SPL_Y0))
    m.add(*tuelle_ops(xout, z_out, SPL_Y1, prof))
    bore_out = 2 * prof[0]
    xs_all = xin + [xout]
    m.cut(T((min(xs_all) - KANAL / 2 + 0.5, SPL_CH, zc), (max(xs_all) + KANAL / 2 - 0.5, SPL_CH, zc), KANAL))
    for x in xin:
        m.cut(Cy(x, SPL_Y0 - IN_L - 1, SPL_CH, zc, IN_ID))                  # Einlass Ø 6 bis in den Kanal
    if z_out != zc:
        m.cut(Cy(xout, SPL_CH - 2, SPL_Y1 + prof[7] + 1, z_out, bore_out))  # Kammer schneidet den Kanal (lo, CPAP)
    else:
        m.cut(Cy(xout, SPL_CH - 0.5, SPL_Y1 + prof[7] + 1, zc, bore_out))
    for hx in (xa - 5, xb + 5):
        m.cut(*slotY(hx, SPL_CH - 10, SPL_CH + 10, floor - 1, floor + 4))
    parts.append(m)

L = list(gen5.L)
cut = next(i for i, l in enumerate(L) if l.startswith('function dev_'))
L = L[:cut]
L.insert(2, '// Splitter hi + lo v5 QV (Exzenter-Quetschventil): erzeugt von gen_splitter_qv.py (Basis gen5.py). '
         'Innen je 2 glatte Tuellen aussen 8 / innen 6 / 12 mm, Wurzel-Fase 2 mm, statt LZQ-Tuelle 10,75. Lage wie v4.')
emit_feature(L, parts, 'qv_', 'Splitter v5 Quetschventil', 'splitterV5QV')
open(out_fs, 'w').write('\n'.join(L) + '\n')
json.dump([dict(name=p.name, label=p.label, col=p.col, printed=True, stages=p.stages, group='dev') for p in parts], open(out_json, 'w'))
print('ok', [p.name for p in parts])
