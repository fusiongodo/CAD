# Development-Wand: Halter für die Module vorn.
#   Rückenhalter Hi / Lo (gleiches Teil): Flansch 24 x 60 mit 2 x M3 x 16 + Mutter auf dem Raster, 2 Finnen an den Rändern
#   als Abstand 14 (OCS liegt mit der Rohrseite an), 4 Schlitze für Kabelbinder 2,5 x 150 quer durch beide Finnen.
#   T-Halter: Flansch 40 x 24, waagerechte Finnen über und unter dem T-Stück, unten Gabel für den Abgang nach unten.
# Druck: Flansch-Rückseite unten, Finnen nach oben, ohne Stützen.
from ihht.geo import Part, B, Cy, Cz
from ihht.wand import *


def ruecken(name, cx, z0):
    p = Part('dwHalter' + name, 'Dev-Wand Rückenhalter OCS %s (Abstand 14, Kabelbinder)' % name.lower(), HELL)
    x0, x1 = cx - H_W / 2, cx + H_W / 2
    p.add(B(x0, x1, -H_FT, 0, z0, z0 + H_L),
          B(x0, x0 + H_FIN_T, -H_STAND, -H_FT + 0.01, z0, z0 + H_L), B(x1 - H_FIN_T, x1, -H_STAND, -H_FT + 0.01, z0, z0 + H_L))
    for u in H_HOLE_U:
        p.cut(Cy(cx, -H_FT - 1, 1, z0 + u, M3_CLR))
    for u in H_SLOT_U:
        p.cut(B(x0 - 1, x1 + 1, -H_SLOT_W[1], -H_SLOT_W[0], z0 + u - 1.6, z0 + u + 1.6))
    return p


def t_halter():
    p = Part('dwHalterT', 'Dev-Wand T-Halter (Gabel für den Abgang, Kabelbinder)', HELL)
    (x0, x1), (z0, z1), (f0, f1) = TH_X, TH_Z, TH_FIN_X
    p.add(B(x0, x1, -H_FT, 0, z0, z1),
          B(f0, f1, -TH_D, -H_FT + 0.01, z0, z0 + TH_FIN_T), B(f0, f1, -TH_D, -H_FT + 0.01, z1 - TH_FIN_T, z1))
    for x, z in TH_HOLES:
        p.cut(Cy(x, -H_FT - 1, 1, z, M3_CLR))
    p.cut(Cz(0, YS, z0 - 1, z0 + TH_FIN_T + 1, 8.4), B(-4.2, 4.2, -TH_D - 1, YS, z0 - 1, z0 + TH_FIN_T + 1))   # Gabel
    for x in TH_SLOT_X:
        p.cut(B(x - 1.6, x + 1.6, -TH_SLOT_W[1], -TH_SLOT_W[0], z0 - 1, z1 + 1))
    return p


def bauen():
    return [ruecken(*r) for r in RUECKEN] + [t_halter()]
