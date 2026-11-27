"""Druckdatei Testserie Sammelbox v3 (Klappe mit Mund + Zunge). Vorher sammelbox_v3.py laufen lassen.
  testserie_klappe_zunge.3mf: Mini-Box (Wanne + Deckel, 1 Verschluss je langer Seite),
                              Klappen 2 x Nr. 1 + 2 x Nr. 2 (Punkte auf dem Hebel), Zungen 2 x Nr. 1, 2, 3 (Punkte auf dem Klotz)
Bambu P1S, 0,2 mm, 4 Wände, 40 %, ohne Stützen, ohne Brim.
Aufruf: <venv mit trimesh>/bin/python sammelbox_v3_3mf.py   (nutzt ihht-gasmischer/quelle/ihht/bambu.py)"""
import os, sys
import numpy as np, trimesh
HERE = os.path.dirname(os.path.abspath(__file__)); DRUCK = os.path.join(HERE, '..', 'druck')
sys.path.insert(0, os.path.join(HERE, '..', '..', 'ihht-gasmischer', 'quelle'))
from ihht.bambu import write_3mf

NAME = 'sammelbox_16_3x3_v3'
PROC = dict(layer_height='0.2', wall_loops='4', sparse_infill_density='40%', brim_type='no_brim', enable_support='0')


def load(n, flip=False):
    m = trimesh.load(os.path.join(DRUCK, '%s_%s.stl' % (NAME, n)), force='mesh'); m.merge_vertices()
    if flip: m.apply_transform(trimesh.transformations.rotation_matrix(np.pi, [1, 0, 0]))
    assert m.is_watertight, n
    c = m.bounds.mean(axis=0); m.apply_translation([-c[0], -c[1], -m.bounds[0][2]])
    return m


teile = [('Mini-Wanne', load('mini_unterteil'), 80.0, 150.0), ('Mini-Deckel', load('mini_deckel', True), 80.0, 90.0)]
teile += [('Klappe %d%s' % (k, c), load('klappe%d' % k), 140.0 + 30.0 * i, 160.0 - 34.0 * (k - 1)) for k in (1, 2) for i, c in enumerate('ab')]
teile += [('Zunge %d%s' % (k, c), load('zunge%d' % k), 140.0 + 22.0 * (k - 1), 92.0 - 20.0 * i) for k in (1, 2, 3) for i, c in enumerate('ab')]
box = []
for n, m, x, y in teile:
    e = m.extents; b = (x - e[0] / 2, y - e[1] / 2, x + e[0] / 2, y + e[1] / 2)
    assert b[0] >= 1 and b[1] >= 1 and b[2] <= 255 and b[3] <= 255, n + ' außerhalb des Betts'
    assert not (b[0] < 20 and b[1] < 30), n + ' in der Sperrzone vorn links'
    for n2, b2 in box:
        assert b[0] > b2[2] + 2 or b[2] < b2[0] - 2 or b[1] > b2[3] + 2 or b[3] < b2[1] - 2, '%s überlappt %s' % (n, n2)
    box.append((n, b)); print('  %-12s %6.1f × %6.1f × %5.1f  Volumen %6.2f cm³' % (n, *e, m.volume / 1000))
out = os.path.join(DRUCK, 'testserie_klappe_zunge.3mf')
write_3mf(teile, out, proc=PROC); print('3MF', os.path.basename(out))
