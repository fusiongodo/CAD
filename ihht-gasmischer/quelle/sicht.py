# Kamera: benoetigter Bildwinkel und Sichtlinien (Linse -> Spindelachse) ueber den Verstellbereich.
import math
from ihht.masse import *
from teile.zahnrad import GEAR_Y1, GEAR_RA, HUB_YS, HUB_D

half = XS[-1] + LZQ_W / 2
d = -LZQ_DEPTH - Y_LENS
print('Kamera: Abstand %.0f mm, braucht HFOV %.1f°, VFOV %.1f°; Kugelbereich z %.1f..%.1f, Kamera z %.1f' % (
    d, 2 * math.degrees(math.atan(half / d)), 2 * math.degrees(math.atan((BALL_Z1 - BALL_Z0) / 2 / d)), BALL_Z0, BALL_Z1, Z_CAM))

# Sichtlinie Linse -> Punkt, an dem die Spindelachse in den LZQ-Körper eintritt (y -22, z Z_V)
def dmax(zc, d_obst, h):
    return (zc - Z_V) * d_obst / (h - Z_V)
obst = [('NEMA-Oberkante', -LZQ_DEPTH - Y_NEMA_FACE, Z_V + NEMA_W / 2), ('Brücke/Distanzringe', -LZQ_DEPTH - BR_Y1 - BOSS_H, Z_V + NEMA_HOLE_PITCH / 2 + BOSS_D / 2),
        ('Zahnrad', -LZQ_DEPTH - GEAR_Y1, Z_V + GEAR_RA), ('Nabe Ø11', -LZQ_DEPTH - HUB_YS, Z_V + HUB_D / 2)]
D_NEAR = -LZQ_DEPTH - (Y_NEMA_FACE - 1.0 - FOOT_L + 16.0)
D_FAR = -LZQ_DEPTH - (CAM_HOLES_Y[1] - (FOOT_L - 4.0) + 16.0)
print('Kamera-Abstand Linse–LZQ-Front: %.0f .. %.0f mm (Nennlage %.0f)' % (D_NEAR, D_FAR, -LZQ_DEPTH - Y_LENS))
for zc in (Z_CAM - CAM_DZ, Z_CAM, Z_CAM + CAM_DZ):
    print('  Kamerahöhe z %.0f: freie Sicht bis Abstand %s' % (zc, ', '.join('%s %.0f' % (n, dmax(zc, dd, h)) for n, dd, h in obst)))
