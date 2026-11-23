# Development-Wand, nicht gedruckt: QV (Rahmen v2 + Getriebe v1 + Gegenlager 6200) vereinfacht, NEMA 17,
# OCS hi / lo / Mix (Bauraum), Y-Stück, Raspberry Pi, BTT SKR Pico. Für Ansicht und Kollisionsprüfung.
# QV in Ventil-Koordinaten wie ihht-pi CAD/quetschventil (z = Achse, Rahmenplatte z -6 .. 0) -> Rückseite auf Y = 0.
from ihht.geo import Part, B, Cy
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
    for k, (x0, x1, y0, y1, z0, z1) in OCS.items():
        t.append(Part('dwOCS' + k.capitalize(), 'OCS %s Bauraum (Dummy)' % k, BLAU, printed=False).add(B(x0, x1, y0, y1, z0, z1)))
    t.append(Part('dwYStueck', 'Y-Stück Bauraum 30 x 30 x 15 (Dummy)', HELL, printed=False).add(B(*YSTUECK)))
    for name, (w, h, d) in (('Pi', (85.0, 70.0, 30.0)), ('Pico', (90.0, 64.0, 12.0))):
        cx, cz = TR_POS[name]
        y0 = WT + TR_T + TR_BOSS_H
        t.append(Part('dw' + name, '%s (Dummy)' % ('Raspberry Pi' if name == 'Pi' else 'BTT SKR Pico'), GRUEN, printed=False).add(
            B(cx - w / 2, cx + w / 2, y0, y0 + d, cz - h / 2, cz + h / 2)))
    return t
