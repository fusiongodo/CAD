# Splitter lo mit duennen O2-Tuellen innen:
#   Block, Flansch, Langloecher, Tropfenkanal Ø 8 und CPAP-Aussentuelle wie Splitter lo v4/v5 QV, Lage identisch.
#   Die zwei Innen-Anschluesse sind jetzt der Typ 'o2' aus anschluesse.py (= Eingang des Splitter hi:
#   Kern Ø 7, Wellen Ø 7,5, Bohrung Ø 5,2, 12 mm lang).
# Aufruf: python3 gen_splitter_lo_o2.py <ausgabe.fs> <parts.json>
import sys, os, tempfile, json
here = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, here)
out_fs, out_json = sys.argv[1], sys.argv[2]
tmp = tempfile.mkdtemp()
sys.argv = ['gen6.py', os.path.join(tmp, 'c.fs'), os.path.join(tmp, 'p.json')]
import gen6
from gen6 import *
import anschluesse as an

IN_TYP, OUT_TYP = 'o2', 'cpap'

gen.parts.clear()
name, label, xin, xout, zc, kind, col = next(s for s in SPL if s[0] == 'splLo')
SPL_Y0, SPL_Y1, SPL_CH = spy(zc)
m = Part('splLoO2', 'Splitter lo O2 (innen 2 x dünne O2-Tülle wie Eingang hi, außen CPAP)', col)
floor = DECK_REF + RAIL_H
xa, xb = spl_extent(xin, xout)
z_out = Z_OUT[kind]
top = max(zc + 8.5, z_out + an.prof(OUT_TYP)[2] + 2.5)
m.add(B(xa, xb, SPL_Y0, SPL_Y1, floor, top))
m.add(B(xa - 10, xb + 10, SPL_Y0 - 8, SPL_Y1 + 8, floor, floor + 3))
for x in xin:
    m.add(*an.tuelle(IN_TYP, x, zc, SPL_Y0, -1))
m.add(*an.tuelle(OUT_TYP, xout, z_out, SPL_Y1, +1))
xs_all = xin + [xout]
m.cut(T((min(xs_all) - KANAL / 2 + 0.5, SPL_CH, zc), (max(xs_all) + KANAL / 2 - 0.5, SPL_CH, zc), KANAL))
for x in xin:
    m.cut(an.bohrung(IN_TYP, x, zc, SPL_Y0 - an.laenge(IN_TYP) - 1, SPL_CH))
m.cut(an.bohrung(OUT_TYP, xout, z_out, SPL_CH - 2, SPL_Y1 + an.laenge(OUT_TYP) + 1))   # Kammer schneidet den Kanal
for hx in (xa - 5, xb + 5):
    m.cut(*slotY(hx, SPL_CH - 10, SPL_CH + 10, floor - 1, floor + 4))
parts = [m]

L = list(gen6.L)
cut = next(i for i, l in enumerate(L) if l.startswith('function ') and not l.startswith(('function unionAll', 'function cutAll', 'function finishPart', 'function tearOp', 'function coneOp', 'function prismOp', 'function prismXOp', 'function boxOp', 'function cylOp')))
print('Kopf bis:', L[cut])
L = L[:cut]
L.insert(2, '// Splitter lo O2: erzeugt von gen_splitter_lo_o2.py (Basis gen6.py, Anschluesse aus anschluesse.py). Lage wie Splitter lo v4.')
emit_feature(L, parts, 'lo2_', 'Splitter lo O2-Tüllen', 'splitterLoO2')
open(out_fs, 'w').write('\n'.join(L) + '\n')
json.dump([dict(name=p.name, label=p.label, col=p.col, printed=True, stages=p.stages, group='dev') for p in parts], open(out_json, 'w'))
print('ok', [p.name for p in parts])
