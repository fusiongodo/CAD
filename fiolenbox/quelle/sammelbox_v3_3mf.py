"""Druckdatei Testserie Sammelbox v3 + Hohlrad-Griff (Alex 27.11.2026: alles in einer Datei). Vorher sammelbox_v3.py laufen lassen.
  testserie_klappe_hohlradgriff.3mf: Probestück Wanne + Deckel (ein Verschlussplatz), 2 Klappen, Zungen 1 bis 3 (Kennpunkte),
                                     dazu 2 x Hohlrad mit Kragen + Steckgriff v2 aus ihht-gasmischer (qv2/gen_getriebe.py)
Bambu P1S, 0,2 mm, 4 Wände, 40 %, ohne Stützen, ohne Brim.
Aufruf: <venv mit trimesh>/bin/python sammelbox_v3_3mf.py"""
import math, os, sys
import numpy as np, trimesh
HERE = os.path.dirname(os.path.abspath(__file__)); DRUCK = os.path.join(HERE, '..', 'druck')
GM = os.path.join(HERE, '..', '..', 'ihht-gasmischer', 'quelle')
sys.path.insert(0, GM); os.chdir(GM)
from ihht.bambu import write_3mf
from ihht.netz import build as netz
from ihht import qv2

NAME = 'sammelbox_16_3x3_v3'
PROC = dict(layer_height='0.2', wall_loops='4', sparse_infill_density='40%', brim_type='no_brim', enable_support='0')
RX = lambda w: trimesh.transformations.rotation_matrix(w, [1, 0, 0])


def auf_bett(m):
    c = m.bounds.mean(axis=0); m.apply_translation([-c[0], -c[1], -m.bounds[0][2]]); return m


def load(n, flip=False):
    m = trimesh.load(os.path.join(DRUCK, '%s_%s.stl' % (NAME, n)), force='mesh'); m.merge_vertices()
    if flip: m.apply_transform(RX(np.pi))
    assert m.is_watertight, n
    return auf_bett(m)


T = lambda p: netz(qv2._teil(p, p.name, qv2.ORANGE, True, 0.0, 0.0))            # Builder-Koordinaten: Y = -(z + 6)
hohlrad = auf_bett(T(qv2.G.hohlrad).apply_transform(RX(math.pi / 2)))           # Scheibe aufs Bett (wie qv_hohlrad_griff_3mf.py)
griff = T(qv2.G.griff()); griff.apply_transform(trimesh.transformations.rotation_matrix(math.pi / 2, [0, 1, 0]))   # auf die Seite
griff = auf_bett(griff.apply_transform(trimesh.transformations.rotation_matrix(math.pi / 2, [0, 0, 1])))            # längs Y
for n, m in (('Hohlrad', hohlrad), ('Griff', griff)):
    assert m.is_watertight and abs(m.bounds[0][2]) < 1e-6, n

teile = [('Hohlrad a', hohlrad, 44.0, 212.0), ('Hohlrad b', hohlrad.copy(), 124.0, 212.0), ('Steckgriff v2', griff, 236.0, 195.0),
         ('Probe Wanne', load('probe_unterteil'), 62.0, 132.0), ('Probe Deckel', load('probe_deckel', True), 62.0, 92.0),
         ('Klappe a', load('klappe'), 130.0, 130.0), ('Klappe b', load('klappe'), 130.0, 92.0)]
teile += [('Zunge %d' % k, load('zunge%d' % k), 178.0, 60.0 + 26.0 * k) for k in (1, 2, 3)]
box = []
for n, m, x, y in teile:
    e = m.extents; b = (x - e[0] / 2, y - e[1] / 2, x + e[0] / 2, y + e[1] / 2)
    assert b[0] >= 1 and b[1] >= 1 and b[2] <= 255 and b[3] <= 255, n + ' außerhalb des Betts'
    assert not (b[0] < 20 and b[1] < 30), n + ' in der Sperrzone vorn links'
    for n2, b2 in box:
        assert b[0] > b2[2] + 2 or b[2] < b2[0] - 2 or b[1] > b2[3] + 2 or b[3] < b2[1] - 2, '%s überlappt %s' % (n, n2)
    box.append((n, b)); print('  %-14s %6.1f × %6.1f × %5.1f' % (n, *e))
out = os.path.join(DRUCK, 'testserie_klappe_hohlradgriff.3mf')
write_3mf(teile, out, proc=PROC); print('3MF', os.path.basename(out))
