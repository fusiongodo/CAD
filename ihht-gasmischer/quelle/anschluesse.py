# Zentrale Anschluss-Bibliothek: jeder Schlauchanschluss hat hier GENAU EINE Definition.
# Teile nennen nur den Typ ('o2', 'cpap', ...). Wer ein Profil hier aendert und die Generatoren
# neu laufen laesst, aendert den Anschluss an allen Teilen, die ihn verwenden.
#   tuelle(typ, x, z, y0, s)        -> Koerper-Ops; Tuelle ab Wandflaeche y0 in Richtung s (+1 = +Y, -1 = -Y)
#   bohrung(typ, x, z, y_a, y_b)    -> Schnitt-Op; Durchgang des Typs zwischen y_a und y_b
#   laenge(typ)                     -> Laenge der Tuelle ab Wandflaeche
# Profil: (r_innen, r_tal, r_spitze, ansatz, wellen, plateau, rampe, laenge)
TUELLE_PROF = {
    'lo': (10.2, 11.4, 12.2, 3.0, 4, 0.8, 2.5, 16.2),     # CPAP, hypoxisch
    'hi': (2.6, 3.5, 3.75, 2.0, 4, 0.5, 2.0, 12.0),       # O2, hyperoxisch
    'mix': (10.2, 11.4, 12.2, 3.0, 4, 0.8, 2.5, 16.2),    # Mischausgang = gleiche CPAP-Tuelle
}
TYP = {'o2': 'hi', 'cpap': 'lo'}                           # sprechende Namen -> Profil

def _cy(x, y0, y1, z, d): return ('cyl', (x, y0, z), (x, y1, z), d)
def _cone(x, z, y0, y1, r0, r1): return ('cone', (x, y0, z), (x, y1, z), r0, r1)
def prof(typ): return TUELLE_PROF[TYP.get(typ, typ)]
def laenge(typ): return prof(typ)[7]

def tuelle(typ, x, z, y0, s=1):
    ri, rt, rs, base, n, pl, ramp, L = prof(typ)
    ops = [_cy(x, y0 - s * 0.5, y0 + s * L, z, 2 * rt)]
    for w in range(n):
        a = y0 + s * (base + w * (pl + ramp))
        ops.append(_cy(x, a, a + s * pl, z, 2 * rs))                          # Stufe + Plateau
        ops.append(_cone(x, z, a + s * pl, a + s * (pl + ramp), rs, rt))      # Rampe zur Spitze hin
    return ops

def bohrung(typ, x, z, y_a, y_b):
    return _cy(x, y_a, y_b, z, 2 * prof(typ)[0])
