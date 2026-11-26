"""Akkuschrauber-Aufsatz für die M8-Sechskantschraube (DIN 933, SW 13), Alex 26.11.: M8 mit dem Akkuschrauber
in die gedruckte Welle drehen statt 200 Handbewegungen. Zwei Varianten auf einer Platte:
  A "Bohrfutter": Sechskantschaft SW 8,5 (Ecken 9,8) zum Einspannen ins 3-Backen-Futter (10 oder 13 mm).
  B "Bit":        Sechskantschaft SW 6,25 (1/4 Zoll = 6,35) für den Bithalter.
Kopf Ø 20 x 10 mit Sechskant-Tasche SW 13,4 x 6,5 tief (Kopf M8: SW 13, 5,3 hoch). Kegel 45° zum Schaft.
Druck: Tasche unten (Dach der Tasche ist eine 13-mm-Brücke), Schaft steht nach oben, ohne Stützen.
<venv>/bin/python m8_akkuschrauber_aufsatz_3mf.py  ->  ../druck/m8_akkuschrauber_aufsatz.3mf"""
import math, os
import trimesh
from ihht.bambu import write_3mf

HERE = os.path.dirname(os.path.abspath(__file__))
KOPF_D, KOPF_H, SW, TIEFE = 20.0, 10.0, 13.4, 6.5
cyl = lambda r, z0, z1, n=96: trimesh.creation.cylinder(radius=r, segment=[[0, 0, z0], [0, 0, z1]], sections=n)
hexa = lambda sw, z0, z1: cyl(sw / 2 / math.cos(math.pi / 6), z0, z1, 6)


def aufsatz(schaft_sw, schaft_l):
    rs = schaft_sw / 2 / math.cos(math.pi / 6)
    kegel = trimesh.creation.revolve([[0, KOPF_H], [KOPF_D / 2, KOPF_H], [rs, KOPF_H + KOPF_D / 2 - rs], [0, KOPF_H + KOPF_D / 2 - rs]], sections=96)
    if kegel.volume < 0: kegel.invert()
    zs = KOPF_H + KOPF_D / 2 - rs
    m = trimesh.boolean.union([cyl(KOPF_D / 2, 0, KOPF_H + 0.01), kegel, hexa(schaft_sw, KOPF_H, zs + schaft_l)], engine='manifold')
    m = trimesh.boolean.difference([m, hexa(SW, -1, TIEFE)], engine='manifold')
    assert m.is_watertight and len(m.split(only_watertight=False)) == 1
    print('Schaft SW %.2f: Höhe %.1f, Schaft frei %.0f lang' % (schaft_sw, m.extents[2], schaft_l))
    return m


PRINT = [('M8-Aufsatz Bohrfutter SW 8,5', aufsatz(8.5, 28.0), 108.0, 128.0), ('M8-Aufsatz Bit SW 6,25', aufsatz(6.25, 26.0), 148.0, 128.0)]
out = os.path.join(HERE, '..', 'druck', 'm8_akkuschrauber_aufsatz.3mf')
write_3mf(PRINT, out, proc=dict(layer_height='0.2', wall_loops='8', sparse_infill_density='40%', brim_type='no_brim', enable_support='0'))
print('3MF', out)
