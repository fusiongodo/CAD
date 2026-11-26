# Development-Wand: T-Stück als Druckteil (die OCS-Steckhalter stehen in wand_ocshalter.py). Maße in ihht/wand.py (TS_*).
#   Block mit Bohrungen Ø 5 (v3.3): lo von links (Y -22), Mix nach rechts (Y -11,5, 3 tiefer), hi von unten (Y -11,5).
#   3 Tüllen Ø 7 mit je 3 Sägezahn-Wellen bis Ø 7,5 für Schlauch 6 innen (v3.3). Flansch 2 x M3 x 16 + Mutter.
# Druck: Flansch-Rückseite unten, Tüllen liegen waagerecht (Schichten längs), Baumstützen unter den Tüllen.
from ihht.geo import Part, B, C, Cx, Cy, Cz
from ihht.wand import *


def t_stueck():
    p = Part('dwTStueck', 'Dev-Wand T-Stück (Druckteil, 3 Tüllen, Flansch)', HELL)
    bx0, bx1, by, bz0, bz1 = TS_BLOCK
    fx0, fx1, fz0, fz1 = TS_FL
    (yl, zl), (ym, zm) = TS_LO, TS_MIX
    p.add(B(fx0, fx1, -H_FT, 0, fz0, fz1), B(bx0, bx1, by, -H_FT + 0.01, bz0, bz1))
    tuellen = (((bx0, yl, zl), (-1, 0, 0)), ((bx1, ym, zm), (1, 0, 0)), ((TS_X, ym, bz0), (0, 0, -1)))   # Wurzel am Block, Richtung zur Spitze
    for p0, n in tuellen:
        pt = lambda t: tuple(p0[i] + n[i] * t for i in range(3))
        for a, b, d in TS_BARB_D:
            p.add(C(pt(a - 0.01), pt(b), d))
        for a, b in TS_WELLEN:
            p.add(('cone', pt(a), pt(b), TS_WELLE_D / 2, TS_BARB_D[-1][2] / 2))
        p.add(('cone', pt(TS_BARB_D[-1][1] - 0.01), pt(TS_BARB), TS_BARB_D[-1][2] / 2, TS_SPITZE_D / 2))
    t, e = TS_BARB + 1, TS_BORE / 2                                         # Bohrungen, an den Ecken um den Radius verlängert
    p.cut(Cx(bx0 - t, TS_X + e, yl, zl, TS_BORE), Cy(TS_X, yl - e, ym + e, zl, TS_BORE),
          Cz(TS_X, ym, bz0 - t, zl + e, TS_BORE), Cx(TS_X - e, bx1 + t, ym, zm, TS_BORE))
    for x, z in TS_HOLES:
        p.cut(Cy(x, -H_FT - 1, 1, z, M3_CLR))
    return p


def bauen():
    return [t_stueck()]
