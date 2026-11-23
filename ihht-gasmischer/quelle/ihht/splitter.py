# Gemeinsamer Bauplan aller Splitter/Sammler: Block + Flansch mit Langloechern, Querkanal,
# innen je Platz zwei Anschluesse Richtung Ventile (-Y), aussen ein Anschluss (+Y).
# Ein konkretes Teil waehlt nur Platz ('hi', 'lo', 'mix') und die Anschluss-Typen.
from .geo import Part, B, Cx, Cz, T, slotY
from .masse import *
from .anschluesse import anschluss

def splitter(name, label, col, platz, innen, aussen, z_aus=None, top=None, kanal=('tropfen', KANAL)):
    xin, xout, zc = SPL_PLATZ[platz]
    y0, y1, ch = spy(zc)
    inn, aus = anschluss(innen), anschluss(aussen)
    unten = (zc == Z_PB)
    floor = (DECK_REF if unten else SHELF_TOP) + RAIL_H
    xa, xb = spl_extent(xin, xout)
    z_out = zc if z_aus is None else z_aus                 # Aussenanschluss ggf. hoeher (grosse Tuelle ueber dem Flansch)
    if top is None:
        top = max(zc + (8.5 if unten else 7), z_out + aus.r_max + 2.5)
    m = Part(name, label, col)
    m.add(B(xa, xb, y0, y1, floor, top))                                  # Block
    m.add(B(xa - 10, xb + 10, y0 - 8, y1 + 8, floor, floor + 3))          # Flansch
    for x in xin:
        m.add(*inn.koerper(x, zc, y0, -1))
    m.add(*aus.koerper(xout, z_out, y1, +1))
    xs = xin + [xout]
    if kanal[0] == 'tropfen':
        m.cut(T((min(xs) - kanal[1] / 2 + 0.5, ch, zc), (max(xs) + kanal[1] / 2 - 0.5, ch, zc), kanal[1]))
    else:
        m.cut(Cx(min(xs), max(xs), ch, zc, kanal[1]))
    for x in xin:
        m.cut(*inn.bohrung(x, zc, y0, -1, ch))
    m.cut(*aus.bohrung(xout, z_out, y1, +1, ch, 2.0 if z_out != zc else 0.5))   # versetzter Ausgang: Kammer schneidet den Kanal
    for hx in (xa - 5, xb + 5):
        m.cut(*slotY(hx, ch - 10, ch + 10, floor - 1, floor + 4))
    return m

def schienen(p, base_z, xa, xb, bottom, zc):               # Schienen unter einem Splitter-Platz
    y0, y1, ch = spy(zc)
    for hx in (xa - 5, xb + 5):
        p.add(B(hx - 4, hx + 4, y0 - 12, y1 + 12, bottom, base_z + RAIL_H))
def schienen_loecher(p, base_z, xa, xb, extra_depth, zc):  # Sackloecher Spax 3x12 in den Schienen
    y0, y1, ch = spy(zc)
    for hx in (xa - 5, xb + 5):
        p.cut(Cz(hx, ch, base_z - extra_depth, base_z + RAIL_H + 1, M3_SELF))
