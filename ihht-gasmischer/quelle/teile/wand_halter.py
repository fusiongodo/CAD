# Development-Wand: T-Stück als Druckteil (die OCS-Steckhalter stehen in wand_ocshalter.py). Maße in ihht/wand.py (TS_*).
#   Block mit Bohrungen Ø 5 (v3.3): lo von links, Mix nach rechts (3 tiefer), hi von unten, alle bei Y -22.
#   3 Tüllen Ø 7 mit je 3 Sägezahn-Wellen bis Ø 7,5 für Schlauch 6 innen (v3.3).
#   v3.4: druckt ohne Stützen. Vorderseite aufs Bett, Tüllen liegen mit Flachstelle auf, Beine unter den
#   Schraubstellen, Flansch nur noch über Beinen + Block (Brücke 16,5 über den hi-Schlauch).
#   2 x M3 x 16 von hinten durch die Wand, Mutter von vorn in den Sechskant-Schacht.
import math
from ihht.geo import Part, B, C, Cx, Cy, Cz, P
from ihht.wand import *


def t_stueck():
    p = Part('dwTStueck', 'Dev-Wand T-Stück (Druckteil, 3 Tüllen, Flansch, druckt ohne Stützen)', HELL)
    bx0, bx1, by, bz0, bz1 = TS_BLOCK
    fx0, fx1, fz0, fz1 = TS_FL
    (yl, zl), (ym, zm) = TS_LO, TS_MIX
    (lx0, lx1, lz1), (rx0, rx1, rz1) = TS_BEINE
    p.add(B(lx0, bx1, -H_FT, 0, fz0, lz1), B(bx1 - 0.01, rx1, -H_FT, 0, fz0, rz1),  # Flansch: nur über Beinen + Brücke über den hi-Schlauch
          B(bx0, bx1, by, 0, bz0, bz1))                                             # Block bis an die Wand
    for x0, x1, z1 in TS_BEINE:
        p.add(B(x0, x1, by, -H_FT + 0.01, fz0, z1))
    tuellen = (((bx0, yl, zl), (-1, 0, 0)), ((bx1, ym, zm), (1, 0, 0)), ((TS_X, ym, bz0), (0, 0, -1)))   # Wurzel am Block, Richtung zur Spitze
    for p0, n in tuellen:
        pt = lambda t: tuple(p0[i] + n[i] * t for i in range(3))
        for a, b, d in TS_BARB_D:
            p.add(C(pt(a - 0.01), pt(b), d))
        for a, b in TS_WELLEN:
            p.add(('cone', pt(a), pt(b), TS_WELLE_D / 2, TS_BARB_D[-1][2] / 2))
        p.add(('cone', pt(TS_BARB_D[-1][1] - 0.01), pt(TS_BARB), TS_BARB_D[-1][2] / 2, TS_SPITZE_D / 2))
    t, e = TS_BARB + 1, TS_BORE / 2                                         # Bohrungen, an den Ecken um den Radius verlängert
    p.cut(B(fx0 - 20, fx1 + 20, by - 10, by, fz0 - 20, fz1 + 20))           # Druckbett-Ebene: Flachstelle unter den Tüllen
    p.cut(Cx(bx0 - t, TS_X + e, yl, zl, TS_BORE), Cy(TS_X, yl - e, ym + e, zl, TS_BORE),
          Cz(TS_X, ym, bz0 - t, zl + e, TS_BORE), Cx(TS_X - e, bx1 + t, ym, zm, TS_BORE))
    r = TS_MUTTER_SW / 2 / math.cos(math.pi / 6)
    sechs = [(r * math.cos(k * math.pi / 3), r * math.sin(k * math.pi / 3)) for k in range(6)]
    for x, z in TS_HOLES:
        p.cut(Cy(x, -H_FT - 1, 1, z, M3_CLR), P(x, z, by - 1, -H_FT, sechs))
    return p


def bauen():
    return [t_stueck()]
