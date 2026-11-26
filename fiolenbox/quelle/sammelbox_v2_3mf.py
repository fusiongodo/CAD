"""Druckdateien Sammelbox v2 (Bambu P1S, 0,2 mm, 3 Wände, 15 %, ohne Stützen, ohne Brim). Vorher sammelbox_v2.py laufen lassen.
  sammelbox_16_3x3_v2_probe.3mf      Probestück: 1 Riegelplatz aus Wanne und Deckel + Riegel 1, 2, 3 (Kennpunkte)
  sammelbox_16_3x3_v2_unterteil.3mf  Wanne + 6 Riegel Nr. RIEGEL_NR (4 + 2 Reserve)
  sammelbox_16_3x3_v2_deckel.3mf     Deckel kopfüber
Aufruf: <venv mit trimesh>/bin/python sammelbox_v2_3mf.py [Riegel-Nr]   (nutzt ihht-gasmischer/quelle/ihht/bambu.py)"""
import os, sys
import numpy as np, trimesh
HERE = os.path.dirname(os.path.abspath(__file__)); DRUCK = os.path.join(HERE, '..', 'druck')
sys.path.insert(0, os.path.join(HERE, '..', '..', 'ihht-gasmischer', 'quelle'))
from ihht.bambu import write_3mf

NAME = 'sammelbox_16_3x3_v2'
RIEGEL_NR = int(sys.argv[1]) if len(sys.argv) > 1 else 1
PROC = dict(layer_height='0.2', wall_loops='3', sparse_infill_density='15%', brim_type='no_brim', enable_support='0')


def load(n, flip=False):
    m = trimesh.load(os.path.join(DRUCK, '%s_%s.stl' % (NAME, n)), force='mesh'); m.merge_vertices()
    if flip: m.apply_transform(trimesh.transformations.rotation_matrix(np.pi, [1, 0, 0]))
    c = m.bounds.mean(axis=0); m.apply_translation([-c[0], -c[1], -m.bounds[0][2]])
    assert m.is_watertight, n
    return m


def job(datei, teile):
    for n, m, x, y in teile:
        e = m.extents
        assert x - e[0] / 2 >= 0.5 and x + e[0] / 2 <= 255.5 and y - e[1] / 2 >= 0.5 and y + e[1] / 2 <= 255.5, n
        assert not (x - e[0] / 2 < 20 and y - e[1] / 2 < 30) or e[0] > 100, n + ' liegt in der Sperrzone vorn links'
        print('  %-22s %6.1f × %6.1f × %5.1f' % (n, *e))
    out = os.path.join(DRUCK, datei); write_3mf(teile, out, proc=PROC); print('3MF', datei)


r = {k: load('riegel%d' % k) for k in (1, 2, 3)}
job(NAME + '_probe.3mf', [('Probe Wanne', load('probe_unterteil'), 100.0, 150.0), ('Probe Deckel', load('probe_deckel', True), 100.0, 110.0)]
    + [('Riegel %d' % k, r[k], 180.0, 110.0 + 12 * k) for k in (1, 2, 3)])
job(NAME + '_unterteil.3mf', [('Wanne', load('unterteil'), 128.0, 136.0)]
    + [('Riegel %d %s' % (RIEGEL_NR, c), r[RIEGEL_NR].copy(), x, y) for c, x, y in
       (('a', 60.0, 12.0), ('b', 110.0, 12.0), ('c', 160.0, 12.0), ('d', 210.0, 12.0), ('e', 60.0, 249.5), ('f', 110.0, 249.5))])
job(NAME + '_deckel.3mf', [('Deckel', load('deckel', True), 128.0, 138.0)])   # nach hinten gerückt: die Ösen kämen sonst der Sperrzone vorn links zu nah
