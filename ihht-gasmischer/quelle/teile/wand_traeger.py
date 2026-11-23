# Development-Wand: Träger für Raspberry Pi und BTT SKR Pico (gleiches Teil, Pi-Lochbild 58 x 49, M2,5 selbstschneidend).
# Sitzt hinten auf dem Raster: 4 x M3 x 12 + Mutter (Rasterpunkte ±20 / ±20). Druck: Platte unten, Dome nach oben.
from ihht.geo import Part, B, Cy
from ihht.wand import *


def bauen():
    out = []
    for name, (cx, cz) in TR_POS.items():
        t = Part('dwTraeger' + name, 'Dev-Wand Träger %s (58 x 49, M2,5)' % name, HELL)
        t.add(B(cx - TR_W / 2, cx + TR_W / 2, WT, WT + TR_T, cz - TR_H / 2, cz + TR_H / 2))
        for x, z in TR_BOSS:
            t.add(Cy(cx + x, WT + TR_T - 0.01, WT + TR_BOSS_H + TR_T, cz + z, TR_BOSS_D))
        for x, z in TR_BOSS:
            t.cut(Cy(cx + x, WT + 1.0, WT + TR_BOSS_H + TR_T + 1, cz + z, M25_SELF))
        for x, z in TR_MOUNT:
            t.cut(Cy(cx + x, WT - 1, WT + TR_T + 1, cz + z, M3_CLR))
        out.append(t)
    return out
