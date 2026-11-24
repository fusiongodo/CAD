# Development-Wand: Grundplatte 246 x 160 x 6 (L-Fuß statt 2 Füße). Schrauben von unten, Köpfe versenkt.
# Druck: Unterseite unten.
from ihht.geo import Part, B, Cz
from ihht.wand import *


def bauen():
    g = Part('dwGrund', 'Dev-Wand Grundplatte 246 x 160 (Schrauben von unten)', BEIGE)
    g.add(B(-WX, WX, BY0, BY1, 0, BT))
    pts = [(x, FUSS_Y) for x in FUSS_X]
    for s in ('v', 'h'):
        sy = -1 if s == 'v' else 1
        pts += [(sx * K_HOLE_X, FACE[s] + sy * d) for sx in (-1, 1) for d in K_HOLE_D]
    for x, y in pts:
        g.cut(Cz(x, y, -1, BT + 1, M3_CLR), Cz(x, y, -1, HEAD_T, HEAD_D))
    for y in MIX_BAND_Y:                                                  # OCS Mix: Kabelbinder durch die Platte, Nut unten
        xs = MIX['x'][1] + 2.0
        g.cut(B(-xs - 1, -xs + 1, y - 1.6, y + 1.6, -1, BT + 1), B(xs - 1, xs + 1, y - 1.6, y + 1.6, -1, BT + 1),
              B(-xs, xs, y - 1.6, y + 1.6, -1, 1.5))
    return [g]
