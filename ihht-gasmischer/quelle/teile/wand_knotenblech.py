# Development-Wand: 4 Knotenbleche (je Ende vorn + hinten), L-Winkel mit dreieckigem Steg außen.
# Wand-Flansch: 2 x M3 x 20 gemeinsam durch vorn, Wand und hinten. Grundplatten-Flansch: 2 x M3 x 12 von unten + Mutter.
# Hinten mit Aussparung für den unteren Randsteg. VR = HL gedreht, VL = HR gedreht (je 2 Spiegelpaare).
# Druck: Steg-Außenseite unten, Flansche nach oben, ohne Stützen.
from ihht.geo import Part, B, Cy, Cz
from ihht.wand import *


def knotenblech(seite, sx):
    name = 'dwKnoten%s%s' % (seite.upper(), 'R' if sx > 0 else 'L')
    p = Part(name, 'Dev-Wand Knotenblech %s %s' % ('vorn' if seite == 'v' else 'hinten', 'rechts' if sx > 0 else 'links'), HELL)
    steg, wf, gf = kb_profile(seite)
    xa, xb = sorted((sx * KX0, sx * KX1))
    sa, sb = sorted((sx * (KX1 - KW), sx * KX1))
    p.add(('prismx', (0.0, 0.0), tuple(steg), sa, sb),
          B(xa, xb, wf[0], wf[1], wf[2], wf[3]),
          B(xa, xb, gf[0], gf[1], gf[2], gf[3]))
    sy = -1 if seite == 'v' else 1
    for z in K_HOLE_Z:
        p.cut(Cy(sx * K_HOLE_X, wf[0] - sy, wf[1] + sy, z, M3_CLR))
    for d in K_HOLE_D:
        p.cut(Cz(sx * K_HOLE_X, FACE[seite] + sy * d, BT - 1, BT + KW + 1, M3_CLR))
    return p


def bauen():
    return [knotenblech(s, sx) for s in ('v', 'h') for sx in (-1, 1)]
