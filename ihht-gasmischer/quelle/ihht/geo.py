# Geometrie-Bausteine: ein Teil = Liste von Stufen ('add' / 'cut') aus einfachen Koerpern.
# Dieselben Daten gehen nach FeatureScript (emit.py) und in die Netz-Pruefung (netz.py).
class Part:
    def __init__(s, name, label, col, printed=True):
        s.name, s.label, s.col, s.printed = name, label, col, printed
        s.stages = []
    def add(s, *ops):
        if not s.stages or s.stages[-1][0] != 'add': s.stages.append(['add', []])
        s.stages[-1][1].extend(ops); return s
    def cut(s, *ops):
        if not s.stages or s.stages[-1][0] != 'cut': s.stages.append(['cut', []])
        s.stages[-1][1].extend(ops); return s

def B(x0, x1, y0, y1, z0, z1):                                   # Quader
    return ('box', min(x0, x1), max(x0, x1), min(y0, y1), max(y0, y1), min(z0, z1), max(z0, z1))
def C(p0, p1, d): return ('cyl', tuple(p0), tuple(p1), d)        # Zylinder zwischen zwei Punkten
def Cy(x, y0, y1, z, d): return C((x, y0, z), (x, y1, z), d)     # Achse Y
def Cz(x, y, z0, z1, d): return C((x, y, z0), (x, y, z1), d)     # Achse Z
def Cx(x0, x1, y, z, d): return C((x0, y, z), (x1, y, z), d)     # Achse X
def T(p0, p1, d): return ('tear', tuple(p0), tuple(p1), d)       # waagerechter Kanal mit Tropfenprofil (druckt ohne Support)
def K(x, z, y0, y1, r0, r1): return ('cone', (x, y0, z), (x, y1, z), r0, r1)     # Kegelstumpf, Achse Y
def P(x, z, y0, y1, pts): return ('prism', (x, z), tuple(pts), y0, y1)           # Polygon in XZ, Extrusion in Y

def rrect(x0, x1, y0, y1, z0, z1, r):
    return [B(x0 + r, x1 - r, y0, y1, z0, z1), B(x0, x1, y0 + r, y1 - r, z0, z1),
            Cz(x0 + r, y0 + r, z0, z1, 2 * r), Cz(x1 - r, y0 + r, z0, z1, 2 * r),
            Cz(x0 + r, y1 - r, z0, z1, 2 * r), Cz(x1 - r, y1 - r, z0, z1, 2 * r)]
def slotY(x, y0, y1, z0, z1, w=3.4):                             # Langloch in Y, senkrecht durch (Standard: M3 Durchgang)
    return [B(x - w / 2, x + w / 2, y0, y1, z0, z1), Cz(x, y0, z0, z1, w), Cz(x, y1, z0, z1, w)]
def slotZ(x, y0, y1, z0, z1, w=3.4):                             # Langloch in Z, waagerecht durch
    return [B(x - w / 2, x + w / 2, y0, y1, z0, z1), Cy(x, y0, y1, z0, w), Cy(x, y0, y1, z1, w)]
