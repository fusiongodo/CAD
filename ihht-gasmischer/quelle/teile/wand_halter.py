# Development-Wand: Modulhalter (ein Teil, 4 x) für OCS hi, OCS lo, Y-Stück und OCS Mix. L-Winkel: Flansch an der
# Wand mit 2 x M3 x 12 + Mutter auf dem Raster, Finne quer zur Wand mit 4 Schlitzen für Kabelbinder 2,5 x 150.
# OCS hi / lo hängen hochkant neben der Finne, Y-Stück und OCS Mix liegen auf der waagerechten Finne.
# Druck: Flansch-Rückseite unten, Finne nach oben, ohne Stützen.
from ihht.geo import Part, B, Cy
from ihht.wand import *


def halter(name, art, a, s, u0):
    p = Part('dwHalter' + name, 'Dev-Wand Modulhalter %s (L 60, Kabelbinder)' % name, HELL)
    hb = lambda u, v, w: B(*halter_box(art, a, s, u0, u, v, w))
    p.add(hb((0, H_L), (0, H_FW), (0, H_FT)), hb((0, H_L), (0, H_FIN_T), (H_FT - 0.01, H_FD)))
    for u in H_HOLE_U:
        x, z = halter_punkt(art, a, s, u0, u, H_HOLE_V)
        p.cut(Cy(x, -H_FT - 1, 1, z, M3_CLR))
    for u in H_SLOT_U:
        p.cut(hb((u - 1.6, u + 1.6), (-1, H_FIN_T + 1), H_SLOT_W))
    return p


def bauen():
    return [halter(*h) for h in HALTER]
