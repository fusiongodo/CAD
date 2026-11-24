# Development-Wand, nicht gedruckt: QV (Rahmen v2 + Getriebe v1 + Gegenlager 6200) vereinfacht, NEMA 17,
# OCS hi / lo / Mix, T-Stück, Schläuche, Raspberry Pi, BTT SKR Pico. Für Ansicht und Kollisionsprüfung.
# QV in Ventil-Koordinaten wie ihht-pi CAD/quetschventil (z = Achse, Rahmenplatte z -6 .. 0) -> Rückseite auf Y = 0.
import math
from ihht.geo import Part, B, C, Cx, Cy, Cz
from ihht.wand import *

PT2 = 6.0                               # Rahmenplatte v2


def VB(cx, x0, x1, y0, y1, z0, z1):
    return B(cx + x0, cx + x1, -(z0 + PT2), -(z1 + PT2), QZ + y0, QZ + y1)


def VC(cx, x, y, z0, z1, d):
    return Cy(cx + x, -(z1 + PT2), -(z0 + PT2), QZ + y, d)


def qv(cx, s):
    q = Part('dwQV' + s, 'QV %s Rahmen v2 + Getriebe v1 (Dummy)' % ('hi' if s == 'L' else 'lo'), (0.93, 0.55, 0.2, 1.0), printed=False)
    q.add(VB(cx, -44.5, 44.5, -41.65, 41.65, -6, 0),                              # Rahmenplatte
          VB(cx, -35.5, 35.5, 28.5, 38.5, -0.01, 34), VB(cx, -35.5, 35.5, -38.5, -28.5, -0.01, 34),   # Ambosse
          VB(cx, -26, 26, 38.65, 41.65, -0.01, 5), VB(cx, -26, 26, -41.65, -38.65, -0.01, 5),       # Anschlagleisten
          VB(cx, -35.5, 35.5, -41.65, 41.65, 33.99, 41.2),                        # Steg mit 6200
          VC(cx, 0, 0, -17.6, -5.9, 73), VC(cx, 0, 0, -27, -17.5, 24),           # Hohlrad, Hals
          VB(cx, -45, 45, -48.5, 20, -34, -30))                                   # Getriebeplatte
    for sx in (-1, 1):
        for sy in (-1, 1):
            q.add(VC(cx, sx * 40, sy * 15, -30.01, -5.9, 9))                      # Pfosten
    n = Part('dwNema' + s, 'NEMA 17 QV %s (Dummy)' % ('hi' if s == 'L' else 'lo'), GRAU, printed=False)
    n.add(VB(cx, -21.15, 21.15, -26 - 21.15, -26 + 21.15, -74, -34),
          VB(cx, -8, 8, -26 - 21.15 - 10, -26 - 21.15 + 0.1, -74, -62))           # Stecker + Kabelbogen nach unten
    return [q, n]


def bauen():
    t = qv(-QX, 'L') + qv(QX, 'R')
    for name, o, sx, typ in (('Hi', OCS_HI, -1, '3F 3.0'), ('Lo', OCS_LO, 1, '3FRL 2.0')):   # Platine, Rohrseite, Stecker, Tüllen
        q = Part('dwOCS' + name, 'OCS %s %s (Dummy, Tüllen zum QV)' % (name.lower(), typ), BLAU, printed=False)
        (x0, x1), (z0, z1), (yb, yo) = o['x'], o['z'], o['y']
        r0, r1, yr = o['rohr']
        q.add(B(x0, x1, yo, yb, z0, z1), B(r0, r1, yr, yo + 0.01, z0, z1), B(x0, x1, o['vorn'], yo + 0.01, *o['clip']))
        q.cut(*[Cy(x, yb + 1, yo - 1, z, 3.5) for x, z in o['loch'].values()])      # Löcher für die Zapfen
        q.add(*[B(a, b, yb - 0.01, o['hinten'], c, d) for a, b, c, d in o['stecker']])
        q.add(*[Cx((x0 + x1) / 2, o['tx'][1], YS, z, TUELLE_D) for z in o['tz']])
        t.append(q)
    m = Part('dwOCSMix', 'OCS Mix (Dummy, quer zur Wand, Tüllen oben)', BLAU, printed=False)
    m.add(B(MIX['x'][0], MIX['x'][1], MIX['y'][0], MIX['y'][1], MIX['z'][0], MIX['z'][1]))
    for y in MIX['ty']:
        m.add(Cz(0, y, MIX['z'][1] - 0.01, MIX['tip'], TUELLE_D))
    t.append(m)
    tt = Part('dwTStueck', 'T-Stück 6 mm (Dummy)', HELL, printed=False)
    tt.add(Cx(-T_ARM, T_ARM, YS, T_Z, 8.0), Cx(-6.0, 6.0, YS, T_Z, 10.0), Cz(0, YS, T_Z - T_STEM, T_Z, 8.0))
    t.append(tt)
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
