# Development-Wand, nicht gedruckt: OCS hi / lo / Mix, Schläuche, Raspberry Pi, BTT SKR Pico. Für Ansicht und
# Kollisionsprüfung. Die Quetschventile stehen als echte Teile in teile/wand_qv.py.
import math
from ihht.geo import Part, B, C, Cx, Cy, Cz
from ihht.wand import *

def bauen():
    t = []
    for name, o, sx, typ in (('Hi', OCS_HI, -1, '3F 3.0'), ('Lo', OCS_LO, 1, '3FRL 2.0'), ('Mix', OCS_MIX, -1, '3F 3.0')):   # Platine, Rohrseite, Stecker, Tüllen
        q = Part('dwOCS' + name, 'OCS %s %s (Dummy)' % (name.lower(), typ), BLAU, printed=False)
        (x0, x1), (z0, z1), (yb, yo) = o['x'], o['z'], o['y']
        r0, r1, ya, yz = o['rohr']
        q.add(B(x0, x1, yo, yb, z0, z1), B(r0, r1, ya - 0.01, yz + 0.01, z0, z1), B(x0, x1, o['clip_y'][0] - 0.01, o['clip_y'][1] + 0.01, *o['clip']))
        q.cut(*[Cy(x, yb + 1, yo - 1, z, 3.5) for x, z in o['loch'].values()])      # Löcher für die Zapfen
        q.add(*[B(a, b, o['stecker_y'][0] - 0.01, o['stecker_y'][1] + 0.01, c, d) for a, b, c, d in o['stecker']])
        q.add(*[Cx((x0 + x1) / 2, o['tx'][1], o['ty'], z, TUELLE_D) for z in o['tz']])
        t.append(q)
    for name, (pts, r) in schlaeuche().items():                            # Schläuche 6/12 als Kette aus Zylindern
        h = Part('dwSchlauch' + name, 'Schlauch 6/12 %s%s (Dummy)' % (name, ', R %.0f' % r if r else ''), (0.95, 0.95, 0.85, 1.0), printed=False)
        n = len(pts) - 1
        for k, (a_, b_) in enumerate(zip(pts, pts[1:])):
            d = [b_[i] - a_[i] for i in range(3)]
            L = math.sqrt(sum(c * c for c in d))
            e0 = 0.6 if k > 0 else 0.0                                     # innen überlappen, an den Tüllen bündig enden
            e1 = 0.6 if k < n - 1 else 0.0
            h.add(C(tuple(a_[i] - d[i] / L * e0 for i in range(3)), tuple(b_[i] + d[i] / L * e1 for i in range(3)), SCHLAUCH_D))
        t.append(h)
    for name, (w, h, d) in (('Pi', (85.0, 70.0, 30.0)), ('Pico', (90.0, 64.0, 12.0))):
        cx, cz = TR_POS[name]
        y0 = WT + TR_T + TR_BOSS_H
        t.append(Part('dw' + name, '%s (Dummy)' % ('Raspberry Pi' if name == 'Pi' else 'BTT SKR Pico'), GRUEN, printed=False).add(
            B(cx - w / 2, cx + w / 2, y0, y0 + d, cz - h / 2, cz + h / 2)))
    return t
