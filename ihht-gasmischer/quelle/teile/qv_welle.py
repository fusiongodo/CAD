# Quetschventil v3: Welle mit Exzenterzapfen e 4,5 für 2 x 6905, M8-Kern. Druck: Nase (vorn) unten, ohne Stützen.
from ihht.geo import Part
from ihht.qv import *


def bauen():
    w = Part('qvWelle', 'QV v3 Welle (Exzenter e 4,5, 2 x 6905, M8-Kern)', ORANGE)
    pr = PIN_D / 2
    w.add(QCz(0, 0, Z_R608[1], Z_HUB0 + 0.01, COLLAR_D),                 # Bund: drückt nur auf den Innenring des hinteren 608
          QCz(0, 0, Z_HUB0, Z_SHOULDER + 0.01, 18.0),                    # Nabe (liegt innerhalb der Schulter: kein Überhang)
          QCz(E, 0, Z_SHOULDER, ZB0, SHOULDER_D),                        # Schulter: nur Innenring 6905
          QCz(E, 0, ZB0 - 0.01, Z_PIN1 + 0.01, PIN_D),                   # Exzenterzapfen
          QK(E, 0, Z_PIN1, Z_NOSE1, pr, pr - (Z_NOSE1 - Z_PIN1)))         # Nase 45°: greift exzentrisch in die Klemmhülse
    w.cut(QCz(0, 0, Z_R608[1] - 1, Z_NOSE1 + 1, ROD_D),
          QK(0, 0, Z_NOSE1 + 0.01, Z_NOSE1 - 0.5, ROD_D / 2 + 0.5, ROD_D / 2))   # Fase am Bett
    return [w]
