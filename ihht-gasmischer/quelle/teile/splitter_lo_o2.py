# Splitter lo mit duennen O2-Tuellen innen (wie der Eingang des Splitter hi), aussen CPAP.
from ihht.masse import Z_OUT
from ihht.splitter import splitter
def bauen():
    return [splitter('splLoO2', 'Splitter lo O2 (innen 2 x dünne O2-Tülle wie Eingang hi, außen CPAP)', (0.35, 0.55, 0.85, 1.0), 'lo', innen='o2', aussen='cpap', z_aus=Z_OUT['lo'])]
