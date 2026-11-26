"""Distanzplatte 2 mm zwischen NEMA 17 und Getriebeplatte (Alex 26.11.): neue Motoren mit längerem Schaft, die
Schaftspitze hätte sonst nur 0,4 Luft zur Hohlrad-Scheibe. Mit der Platte rückt der Motor 2 nach hinten (2,4 Luft bei
Schaft 22 ab Passungsring). 42,3 x 42,3 x 2, Mittelloch Ø 22,4 (Passungsring Ø 22), 4 x Ø 3,4 im Raster 31.
Motorschrauben 2 länger nehmen. 2 Stück. <venv>/bin/python qv_motor_distanz_3mf.py -> ../druck/qv_motor_distanz.3mf"""
import os
import trimesh
from ihht.bambu import write_3mf

HERE = os.path.dirname(os.path.abspath(__file__))
SEITE, DICKE, MITTE_D, LOCH_D, RASTER = 42.3, 2.0, 22.4, 3.4, 15.5
cyl = lambda d, x, y: trimesh.creation.cylinder(radius=d / 2, segment=[[x, y, -1], [x, y, DICKE + 1]], sections=64)
m = trimesh.creation.box(extents=[SEITE, SEITE, DICKE]); m.apply_translation([0, 0, DICKE / 2])
m = trimesh.boolean.difference([m, cyl(MITTE_D, 0, 0)] + [cyl(LOCH_D, sx * RASTER, sy * RASTER) for sx in (-1, 1) for sy in (-1, 1)], engine='manifold')
assert m.is_watertight and len(m.split(only_watertight=False)) == 1
print('Distanzplatte %.1f x %.1f x %.1f' % tuple(m.extents))
PRINT = [('Motor-Distanz a', m, 100.0, 128.0), ('Motor-Distanz b', m.copy(), 156.0, 128.0)]
out = os.path.join(HERE, '..', 'druck', 'qv_motor_distanz.3mf')
write_3mf(PRINT, out, proc=dict(layer_height='0.2', wall_loops='3', sparse_infill_density='40%', brim_type='no_brim', enable_support='0'))
print('3MF', out)
