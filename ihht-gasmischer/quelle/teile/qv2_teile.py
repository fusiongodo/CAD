# Quetschventil wie gebaut (Rahmen v2 + Getriebe v1 + Gegenlager 6200, Welle mit Zapfen): alle Teile im Einbau.
# Geometrie in qv2/gen_*.py, Umrechnung in ihht/qv2.py. Dieselben Teile hängen zweimal in der Development-Wand (teile/wand_qv.py).
from ihht import qv2


def bauen():
    return qv2.teile('q2')
