# Quetschventil v3, nicht gedruckt: NEMA 17, 2 x 608, 2 x 6905, M8 x 70 (DIN 933), 2 Muttern. Für Ansicht und Prüfung.
from ihht.geo import Part
from ihht.qv import *


def ring(name, label, x, z0, z1, d, D, col=STAHL):
    p = Part(name, label, col, printed=False)
    p.add(QCz(x, 0, z0, z1, D))
    p.cut(QCz(x, 0, z0 - 1, z1 + 1, d))
    return p


def bauen():
    t = []
    n = Part('qvNema', 'NEMA 17 (Dummy)', GRAU, printed=False)
    n.add(QB(-21.15, 21.15, -A - 21.15, -A + 21.15, Z_C1 - 40, Z_C1),
          QCz(0, -A, Z_C1 - 0.1, Z_C1 + 2, 22),
          QCz(0, -A, Z_C1, Z_C1 + 24, 5.0))                               # längste übliche Welle
    t.append(n)
    t.append(ring('qv608H', '608 hinten (Dummy)', 0, Z_R608[0], Z_R608[1], L608[0], L608[1]))
    t.append(ring('qv608V', '608 vorn (Dummy)', 0, Z_F608[0], Z_F608[1], L608[0], L608[1]))
    t.append(ring('qv6905A', '6905 hinten (Dummy)', E, ZB0, ZB0 + L6905[2], L6905[0], L6905[1]))
    t.append(ring('qv6905B', '6905 vorn (Dummy)', E, ZB0 + L6905[2], ZB1, L6905[0], L6905[1]))
    s = Part('qvSchraube', 'M8 x 70 DIN 933 (Dummy)', STAHL, printed=False)
    s.add(QP(0, 0, Z_HEAD - 5.3, Z_HEAD - 0.01, hex_profile(13.0)), QCz(0, 0, Z_HEAD - 0.02, Z_HEAD + BOLT_L, 7.9))
    t.append(s)
    for name, z0 in (('qvMutterK', Z_KNUT[0] + 0.1), ('qvMutterV', Z_NUT_V[0])):
        m = Part(name, 'Mutter M8 DIN 934 (Dummy)', STAHL, printed=False)
        m.add(QP(0, 0, z0, z0 + 6.5, hex_profile(13.0)))
        m.cut(QCz(0, 0, z0 - 1, z0 + 7.5, 7.9))
        t.append(m)
    return t
