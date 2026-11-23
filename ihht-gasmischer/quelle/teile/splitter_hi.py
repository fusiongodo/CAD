# Splitter hi v4 (LZQ-Bau): O2 kommt aussen an, innen zwei LZQ-Stecktuellen.
from ihht.splitter import splitter
def bauen():
    return [splitter('splHi', 'Splitter hi v4 (Eingang hyperoxisch, links, Tüllen 22 mm)', (0.85, 0.35, 0.3, 1.0), 'hi', innen='lzq', aussen='o2')]
