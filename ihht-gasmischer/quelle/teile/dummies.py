# Dummies (nicht gedruckt): NEMA 17, LZQ-Rotameter, Pi-Cam. Nur fuer Ansicht und Kollisionspruefung.
from ihht.geo import *
from ihht.masse import *

def bauen():
    teile = []
    for i, x in enumerate(XS):
        if x in XS_MOTOR:
            n = Part('nemaD%d' % (XS_MOTOR.index(x) + 1), 'NEMA 17 %s (Dummy)' % ('hi', 'lo')[XS_MOTOR.index(x)], (0.3, 0.3, 0.32, 1.0))
            teile.append(n)
            h = NEMA_W / 2
            n.add(B(x - h, x + h, Y_NEMA_BACK, Y_NEMA_FACE, Z_V - h, Z_V + h),
                  Cy(x, Y_NEMA_FACE - 0.1, Y_NEMA_FACE + NEMA_RING_H, Z_V, NEMA_RING_D),
                  Cy(x, Y_NEMA_FACE - 0.1, Y_NEMA_TIP, Z_V, NEMA_SHAFT_D),
                  B(x - 8, x + 8, Y_NEMA_BACK, Y_NEMA_BACK + 12, Z_V - h - 10.0, Z_V - h + 0.1))      # 6-Pin-Stecker + Kabelbogen: 10 mm nach unten
        l = Part('lzqD%d' % (i + 1), 'LZQ %s (Dummy)' % ('Ablass hi', 'Zufuhr hi', 'Zufuhr lo', 'Ablass lo')[i], (0.7, 0.85, 0.95, 1.0))
        teile.append(l)
        l.add(B(x - LZQ_W / 2, x + LZQ_W / 2, -LZQ_DEPTH, 0, Z0, Z0 + LZQ_H))
        for z in (Z_PB, Z_PT):
            l.add(Cy(x, -0.1, LZQ_THREAD_L, z, LZQ_THREAD_D),
                  Cy(x, 2.0, 5.0, z, 15.0),                                   # Mutter hinter der Wand
                  Cy(x, LZQ_THREAD_L - 0.1, LZQ_TUELLE_L, z, LZQ_BARB_D0))
            for k in range(3):
                a = LZQ_TUELLE_L - 0.5 - 4.5 * k
                l.add(Cy(x, a - 3.5, a, z, LZQ_BARB_D1))
        l.add(Cy(x, -LZQ_DEPTH - 2.5, 0 - LZQ_DEPTH + 0.1, Z_V, 12.0 / math.cos(math.pi / 6)),
              Cy(x, Y_STEM_TIP, -LZQ_DEPTH - 2.4, Z_V, LZQ_STEM_D))
    teile.append(Part('camD', 'Pi-Cam OV5647 (Dummy)', (0.1, 0.35, 0.15, 1.0)).add(
        B(-12.5, 12.5, BOARD_Y - 1.0, BOARD_Y, Z_CAM - 12, Z_CAM + 12), B(-4, 4, BOARD_Y - 0.1, Y_LENS, Z_CAM - 4, Z_CAM + 4)))
    for t in teile: t.printed = False
    return teile
