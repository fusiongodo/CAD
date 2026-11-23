# Development-Wand: Wandplatte 246 x 246 x 6 mit Randsteg hinten, Quersteg, Ringstegen um die QV-Ausschnitte und
# M3-Lochraster 10 mm und 2 Ablass-Durchlässen Ø 14 oben mittig. Ein Teil. Druck: Vorderseite unten, Stege nach oben, ohne Stützen.
from ihht.geo import Part, B, Cy, Cz
from ihht.wand import *


def bauen():
    w = Part('dwWand', 'Dev-Wand Wandplatte (6 dick, Randsteg, M3-Raster 10)', BEIGE)
    w.add(B(-WX, WX, 0, WT, WZ0, WZ1),
          B(-WX, WX, WT - 0.01, YR, WZ1 - RW, WZ1), B(-WX, WX, WT - 0.01, YR, WZ0, WZ0 + RB),
          B(-WX, -WX + RW, WT - 0.01, YR, WZ0, WZ1), B(WX - RW, WX, WT - 0.01, YR, WZ0, WZ1),
          B(-WX + RW - 0.01, WX - RW + 0.01, WT - 0.01, YR, RIB_Z[0], RIB_Z[1]))
    for cx in (-QX, QX):
        w.add(Cy(cx, WT - 0.01, YR, QZ, 2 * QV_RING[1]))
    w.cut(*[Cy(cx, WT, YR + 1, QZ, 2 * QV_RING[0]) for cx in (-QX, QX)])     # Ringsteg innen frei
    bx, bz = QV_CUT_BOX
    for cx in (-QX, QX):
        w.cut(Cy(cx, -1, YR + 1, QZ, 2 * QV_CUT_R), B(cx - bx, cx + bx, -1, YR + 1, QZ - bz, QZ + bz))
        for x, z in QV_SCREWS:
            w.cut(Cy(cx + x, -1, WT + 1, QZ + z, M3_CLR))
    for x, z in ABLASS:
        w.cut(Cy(x, -1, WT + 1, z, ABLASS_D))                                    # Ablass-Schläuche nach hinten
    for x in FUSS_X:
        w.cut(Cz(x, FUSS_Y, WZ0 - 1, WZ0 + FUSS_TIEF, M3_SELF))
    w.cut(*[Cy(x, -1, WT + 1, z, M3_CLR) for x, z in grid_points()])
    return [w]
