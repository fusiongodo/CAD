# Development-Wand: Steckhalter OCS hi (3F 3.0) und OCS lo (3FRL 2.0), Maße und Lage in ihht/wand.py (OH).
#   Flansch 4 an der Wand, 2 x M3 x 16 + Mutter auf dem Raster (vor dem Aufstecken einschrauben). Stützen Ø 5 bis an die
#   Platinen-Rückseite (Bauteilseite zur Wand, Steckergehäuse 6 frei), oben Zapfen durch die Platinenlöcher mit Fase.
#   hi sitzt umgekehrt (Rohr zur Wand, Bauteilseite vorn): Stützen neben dem Rohr bis an die Platine, 1 Luft zum Flansch.
#   hi zusätzlich 2 Randleisten mit Rastwulst: Platine einfach von vorn eindrücken.
#   lo ohne Zapfen: 4 x M2 von vorn durch Platine und Stütze, Mutter seitlich in den Schlitz unter der Platine.
# Druck: lo Flansch-Rückseite unten; hi auf der Seite (linke Leiste unten), damit die Schichten längs durch die Leisten laufen.
from ihht.geo import Part, B, Cy, Cz, K
from ihht.wand import *


def halter(name, h):
    o = h['ocs']
    (fx0, fx1, fz0, fz1) = h['flansch']
    yb, yo = o['y']
    p = Part('dwOCSHalter' + name, 'Dev-Wand Steckhalter OCS %s' % name.lower(), HELL)
    p.add(B(fx0, fx1, -H_FT, 0, fz0, fz1))
    p.add(*[Cy(o['loch'][k][0], -H_FT + 0.01, yb, o['loch'][k][1], h.get('post_d', OH_POST_D)) for k in h['stuetzen']])
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
    xm = (o['x'][0] + o['x'][1]) / 2
    for k in h.get('m2', ()):                                                   # M2 durch, Mutter seitlich von außen
        x, z = o['loch'][k]
        sx = 1 if x > xm else -1
        y0 = yb + M2_MUTTER_AB
        p.cut(Cy(x, 1, yb - 1, z, M2_CLR),
              B(min(x, x + sx * 6), max(x, x + sx * 6), y0, y0 + M2_MUTTER_H, z - M2_MUTTER_SW / 2, z + M2_MUTTER_SW / 2))
    for x, z in h['schrauben']:
        p.cut(Cy(x, -H_FT - 1, 1, z, M3_CLR))
    return p


def bauen():
    return [halter(n, h) for n, h in OH.items()]
