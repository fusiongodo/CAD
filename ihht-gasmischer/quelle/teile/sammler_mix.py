# Sammler Mix v4 (Altstand mit CPAP-Tuelle, ersetzt durch sammler_mix_acryl.py): runder Kanal Ø 5, O2-Stecktuellen.
from ihht.masse import Z_OUT
from ihht.splitter import splitter
def bauen():
    return [splitter('splMix', 'Sammler Mix (Ausgang)', (0.6, 0.4, 0.8, 1.0), 'mix', innen='o2_alt', aussen='cpap', z_aus=Z_OUT['mix'], kanal=('rund', 5.0))]
