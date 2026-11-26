"""Dünnere Lagerhülsen für das gebaute Quetschventil (über beide 6203), je 2 Stück mit Wand 1,5 und 1,0 (erste Fassung 2,0 + 1,5: 2,0 ist laut Alex zu viel).
Anlass (Alex 26.11.): von den zwei vorhandenen Hülsen ist die mit der dünneren Wand besser, aber noch zu stark.
Maße wie ihht-pi build_huelse.py, Passung "weit": Bohrung 40,4, Länge 24 + Bund 1 (innen Ø 35), Einführfase.
Kennung: vertiefte Punkte oben auf dem Bodensaum (innen), 1 Punkt = Wand 1,5, 3 Punkte = Wand 1,0 (2 Punkte = Wand 2,0, nur in der ersten Fassung).
Spalt zu = 8,5 - e - Wand: mit e 3,5 also 3,0 (Wand 2,0) bzw. 3,5 (Wand 1,5) statt 2,5.
<venv>/bin/python qv_huelsen_3mf.py  ->  ../druck/qv_huelsen_duenn.3mf (Bund unten, ohne Stützen, ohne Brim)."""
import math, os
import trimesh
from ihht.bambu import write_3mf

HERE = os.path.dirname(os.path.abspath(__file__))
D, L, LIP, LIP_D = 40.4, 24.0, 1.0, 35.0


def huelse(wand, kerben):
    ri, ra, h = D / 2, D / 2 + wand, L + LIP
    cyl = lambda r, z0, z1: trimesh.creation.cylinder(radius=r, segment=[[0, 0, z0], [0, 0, z1]], sections=180)
    fase = trimesh.creation.revolve([[0, h - 0.6], [ri, h - 0.6], [ri + 0.8, h + 0.2], [0, h + 0.2]], sections=180)
    if fase.volume < 0: fase.invert()
    m = trimesh.boolean.difference([cyl(ra, 0, h), cyl(LIP_D / 2, -1, LIP + 0.5), cyl(ri, LIP, h + 1), fase], engine='manifold')   # Drucklage: Bund unten
    for k in range(kerben):                                                  # Punkte oben auf dem Bodensaum, Ø 1,6, 0,5 tief (vertieft: das Lager liegt dort an)
        b = trimesh.creation.cylinder(radius=0.8, segment=[[18.8, 0, LIP - 0.5], [18.8, 0, LIP + 0.5]], sections=24)
        b.apply_transform(trimesh.transformations.rotation_matrix(math.radians(12 * k), [0, 0, 1]))
        m = trimesh.boolean.difference([m, b], engine='manifold')
    assert m.is_watertight and len(m.split(only_watertight=False)) == 1
    print('Wand %.1f: außen Ø %.1f, Punkte %d, Volumen %.0f mm3' % (wand, 2 * ra, kerben, m.volume))
    return m


PRINT = [('Huelse Wand 1,5 a', huelse(1.5, 1), 98.0, 100.0), ('Huelse Wand 1,5 b', huelse(1.5, 1), 158.0, 100.0),      # Alex 26.11.: 2,0 ist zu viel, nur 1,5 und weniger
         ('Huelse Wand 1,0 a', huelse(1.0, 3), 98.0, 160.0), ('Huelse Wand 1,0 b', huelse(1.0, 3), 158.0, 160.0)]
out = os.path.join(HERE, '..', 'druck', 'qv_huelsen_duenn.3mf')
write_3mf(PRINT, out, proc=dict(layer_height='0.2', wall_loops='4', sparse_infill_density='40%', brim_type='no_brim',
                                wall_generator='arachne', enable_support='0'))
print('3MF', out)
