# Quetschventil v3: 2 Distanzscheiben 8,4 / 11,5 x 1,2. Drücken nur auf den 608-Innenring (Kopf, Mutter und
# Hohlrad würden sonst an der Dichtscheibe schleifen). Hinten zwischen Hohlrad und 608, vorn zwischen 608 und Mutter.
from ihht.geo import Part
from ihht.qv import *


def bauen():
    out = []
    for name, (z0, z1) in (('qvDistanzH', Z_DS_H), ('qvDistanzV', Z_DS_V)):
        d = Part(name, 'QV v3 Distanzscheibe 8,4 / 11,5 x 1,2 (%s)' % ('hinten' if z0 < 0 else 'vorn'), ORANGE)
        d.add(QCz(0, 0, z0, z1, COLLAR_D))
        d.cut(QCz(0, 0, z0 - 1, z1 + 1, ROD_D))
        out.append(d)
    return out
