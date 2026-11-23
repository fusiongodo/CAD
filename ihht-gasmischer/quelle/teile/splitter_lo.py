# Splitter lo v4 (LZQ-Bau): CPAP-Schlauch aussen (4 mm hoeher als die Kanalachse), innen zwei LZQ-Stecktuellen.
from ihht.masse import Z_OUT
from ihht.splitter import splitter
def bauen():
    return [splitter('splLo', 'Splitter lo v4 (Eingang hypoxisch, rechts, Tüllen 22 mm)', (0.35, 0.55, 0.85, 1.0), 'lo', innen='lzq', aussen='cpap', z_aus=Z_OUT['lo'])]
