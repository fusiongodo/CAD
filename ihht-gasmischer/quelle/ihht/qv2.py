# Quetschventil, wie es gebaut ist (Rahmen v2 + Getriebe v1 + Gegenlager 6200 + Welle mit angedrucktem Zapfen).
# Quelle der Geometrie: qv2/gen_rahmen.py, qv2/gen_getriebe.py, qv2/gen_gegenlager.py (aus ihht-pi CAD/quetschventil
# hierher umgezogen, 25.11.2026). Dieses Modul rechnet deren Körper aus den Ventil-Koordinaten (z = Hauptachse,
# Rahmenplatte z -6 .. 0, y zu den Ambossen) in die Builder-Koordinaten (X = x, Y = -(z + 6), Z = y): Rückseite der
# Rahmenplatte auf Y = 0, vorn = -Y. teile() liefert das ganze Ventil als Builder-Teile, verschoben um (dx, dz).
# Wer das Ventil braucht (eigenes Modul, Development-Wand), ruft teile() auf: eine Änderung in qv2/ wirkt überall.
import os, sys
from ihht.geo import Part

sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'qv2'))
E_BAU = 3.5                                # eingebaute Exzentrizität (drucklog.md)
os.environ.setdefault('QV_E', str(E_BAU))
import gen_getriebe as G
import gen_rahmen as RV
import gen_gegenlager as GL

ORANGE = (0.93, 0.55, 0.2, 1.0)
STAHL = (0.65, 0.67, 0.7, 1.0)
GRAU = (0.3, 0.3, 0.32, 1.0)
PT = RV.PT


def _op(o, dx, dz):
    m = lambda p: (p[0] + dx, -(p[2] + PT), p[1] + dz)
    k = o[0]
    if k == 'box':
        _, x0, x1, y0, y1, z0, z1 = o
        return ('box', x0 + dx, x1 + dx, -(z1 + PT), -(z0 + PT), y0 + dz, y1 + dz)
    if k == 'cyl':
        return ('cyl', m(o[1]), m(o[2]), o[3])
    if k == 'cone':
        return ('cone', m(o[1]), m(o[2]), o[3], o[4])
    if k in ('prism', 'gear'):
        (x, y, z0), h = o[1], o[3]
        pts = G.gear_profile(*o[2]) if k == 'gear' else o[2]
        return ('prism', (x + dx, y + dz), tuple(pts), -(z0 + h + PT), -(z0 + PT))
    raise ValueError(k)


def _teil(p, name, col, printed, dx, dz):
    t = Part(name, p.label, col, printed)
    t.stages = [[kind, [_op(o, dx, dz) for o in ops]] for kind, ops in p.stages]
    return t


def _nema():
    n = G.Part('nema', 'NEMA 17 (Dummy, mit Stecker und Kabelbogen nach unten)')
    n.add(G.B(-21.15, 21.15, -G.A - 21.15, -G.A + 21.15, G.Z_C1 - 40, G.Z_C1),
          G.B(-8, 8, -G.A - 21.15 - 10, -G.A - 21.15 + 0.1, G.Z_C1 - 40, G.Z_C1 - 28),
          G.Cz(0, -G.A, G.Z_C1 - 0.1, G.Z_C1 + 2, 22), G.Cz(0, -G.A, G.Z_C1, G.Z_C1 + G.SHAFT_TIP, 5.0))
    return n


def teile(prefix, dx=0.0, dz=0.0, printed=True, nema=None):
    """Alle Teile des gebauten Ventils. prefix vor jedem Teilnamen, nema = Name des Motor-Dummys (sonst prefix + 'Nema')."""
    e = G.E
    gedruckt = [('Platte', RV.platte), ('AmbossO', RV.amboss_o), ('AmbossU', RV.amboss_u), ('Steg', GL.STEG),
                ('Welle', G.welle), ('Ring', GL.distanzring(e)), ('Huelse', GL.huelse(e)),
                ('Hohlrad', G.hohlrad), ('Ritzel', G.ritzel), ('Getriebeplatte', G.platte)]
    out = [_teil(p, prefix + n, ORANGE, printed, dx, dz) for n, p in gedruckt]
    out += [_teil(p, prefix + n, STAHL, False, dx, dz) for n, p in (('L6203H', GL.lager6203(e, 0)), ('L6203V', GL.lager6203(e, 1)),
                                                                    ('L6200', GL.lager6200()))]
    out.append(_teil(_nema(), nema or prefix + 'Nema', GRAU, False, dx, dz))
    return out
