# Development-Wand: Steckhalter OCS hi (3F 3.0) und OCS lo (3FRL 2.0), Maße und Lage in ihht/wand.py (OH).
#   Flansch 4 an der Wand, 2 x M3 x 16 + Mutter auf dem Raster (vor dem Aufstecken einschrauben). Stützen Ø 5 bis an die
#   Platinen-Rückseite (Bauteilseite zur Wand, Steckergehäuse 6 frei), oben Zapfen durch die Platinenlöcher mit Fase.
#   hi zusätzlich 2 Randleisten mit Rastwulst: Platine einfach von vorn eindrücken.
# Druck: lo Flansch-Rückseite unten; hi auf der Seite (linke Leiste unten), damit die Schichten längs durch die Leisten laufen.
from ihht.geo import Part, B, Cy, Cz, K
from ihht.wand import *


def halter(name, h):
    o = h['ocs']
    (fx0, fx1, fz0, fz1) = h['flansch']
    yb, yo = o['y']
    p = Part('dwOCSHalter' + name, 'Dev-Wand Steckhalter OCS %s' % name.lower(), HELL)
    p.add(B(fx0, fx1, -H_FT, 0, fz0, fz1))
    p.add(*[Cy(o['loch'][k][0], -H_FT + 0.01, yb, o['loch'][k][1], OH_POST_D) for k in h['stuetzen']])
    for k, d in h['zapfen'].items():
        x, z = o['loch'][k]
        y1 = yo - 1.2
        p.add(Cy(x, yb + 0.01, y1, z, d), K(x, z, y1 + 0.01, y1 - 0.8, d / 2, d / 2 - 0.5))
    if h['leisten']:
        z0, z1 = h['leisten']
        x0, x1 = o['x']
        for xi, sx in ((x0 - LEISTE_LUFT, -1), (x1 + LEISTE_LUFT, 1)):            # Innenkante, nach außen dick
            yw = yo - WULST_D / 2                                                   # Wulst liegt vorn auf der Platine
            p.add(B(xi, xi + sx * LEISTE_T, -H_FT + 0.01, yw, z0, z1),
                  Cz(xi + sx * 0.4, yw, z0, z1, WULST_D))                         # ragt 0,6 über die Platinenkante
    for x, z in h['schrauben']:
        p.cut(Cy(x, -H_FT - 1, 1, z, M3_CLR))
    return p


def bauen():
    return [halter(n, h) for n, h in OH.items()]
