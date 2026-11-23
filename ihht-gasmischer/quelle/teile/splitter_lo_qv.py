# Splitter lo fuer das Exzenter-Quetschventil: aussen CPAP, innen zwei glatte Tuellen fuer Silikonschlauch 6/12.
from ihht.masse import Z_OUT
from ihht.splitter import splitter
def bauen():
    return [splitter('splLoQV', 'Splitter lo v5 QV (Quetschventil, innen 2 x Tuelle 8/6 glatt)', (0.35, 0.55, 0.85, 1.0), 'lo', innen='qv', aussen='cpap', z_aus=Z_OUT['lo'])]
