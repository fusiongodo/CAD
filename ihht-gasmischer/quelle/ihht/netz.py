# Netze (trimesh/manifold) aus denselben Teil-Daten wie das FeatureScript: fuer Pruefung und STL.
from shapely.geometry import Polygon
import numpy as np, trimesh
def _mk(o):
    if o[0] == 'tear':
        _, p0, p1, d = o
        p0, p1 = np.array(p0, float), np.array(p1, float); r = d / 2; L = np.linalg.norm(p1 - p0); n = (p1 - p0) / L
        c = trimesh.creation.cylinder(radius=r, segment=[p0, p1], sections=48)
        s2, w = r / 2**0.5, r * (2**0.5 - 1)
        xd = np.cross([0, 0, 1], n); xd /= np.linalg.norm(xd)
        pts2 = [(0, 0), (-s2, s2), (0, r * 2**0.5), (s2, s2)]
        verts = []
        for t in (0, L):
            for (u, v) in pts2: verts.append(p0 + n * t + xd * u + np.array([0, 0, v]))
        roof = trimesh.convex.convex_hull(np.array(verts))
        return trimesh.boolean.union([c, roof], engine='manifold')
    if o[0] == 'box':
        _, x0, x1, y0, y1, z0, z1 = o
        m = trimesh.creation.box(extents=[x1-x0, y1-y0, z1-z0]); m.apply_translation([(x0+x1)/2, (y0+y1)/2, (z0+z1)/2]); return m
    if o[0] == 'cone':
        _, p0, p1, r0, r1 = o
        p0, p1 = np.array(p0, float), np.array(p1, float); L = np.linalg.norm(p1 - p0)
        m = trimesh.creation.revolve(np.array([[0, 0], [r0, 0], [r1, L], [0, L]]), sections=48)
        T = trimesh.geometry.align_vectors([0, 0, 1], (p1 - p0) / L); m.apply_transform(T); m.apply_translation(p0)
        if m.volume < 0: m.invert()
        return m
    _, p0, p1, d = o
    return trimesh.creation.cylinder(radius=d/2, segment=[p0, p1], sections=48)

def mk(o):
    if o[0] == 'prism':
        _, c, pts, y0, y1 = o
        m = trimesh.creation.extrude_polygon(Polygon(pts), y1 - y0)          # Profil in (u, v), Höhe in w
        v = m.vertices.copy()
        m.vertices = np.column_stack([c[0] + v[:, 0], y0 + v[:, 2], c[1] + v[:, 1]])   # (u, v, w) -> (x, y, z) = (u, w, v)
        m.invert()                                                           # Achsentausch spiegelt -> Flächen umdrehen
        return m
    if o[0] == 'prismx':
        _, c, pts, x0, x1 = o
        m = trimesh.creation.extrude_polygon(Polygon(pts), x1 - x0)          # Profil in (u, v) = (y, z), Höhe in w = x
        v = m.vertices.copy()
        m.vertices = np.column_stack([x0 + v[:, 2], c[0] + v[:, 0], c[1] + v[:, 1]])   # zyklischer Tausch -> keine Spiegelung
        return m
    return _mk(tuple(o) if not isinstance(o, tuple) else o)

def build(p):
    body = None
    for kind, ops in p.stages:
        ms = [mk(o) for o in ops]
        if kind == 'add':
            body = trimesh.boolean.union(([body] if body is not None else []) + ms, engine='manifold')
        else:
            body = trimesh.boolean.difference([body] + ms, engine='manifold')
    return body



def gewinde(r_root, r_crest, overlap, z0, pitch, turns, seg=72):
    """Gewindezahn als Netz, gleiche Masse wie makeThread in ihht/gewinde.py: 60°-Trapez entlang einer Rechts-Helix um Z,
    Start bei Winkel 0 auf Hoehe z0. Aussengewinde: mit dem Teil vereinigen. Innengewinde: vom Teil abziehen."""
    import math
    depth = abs(r_crest - r_root); crest = pitch / 8; w = crest + 2 * depth * math.tan(math.radians(30))
    prof = [(overlap, -w / 2), (r_root, -w / 2), (r_crest, -crest / 2), (r_crest, crest / 2), (r_root, w / 2), (overlap, w / 2)]
    n = int(math.ceil(turns * seg)); k = len(prof)
    V, F = [], []
    for i in range(n + 1):
        t = turns * i / n; a = 2 * math.pi * t
        V += [(r * math.cos(a), r * math.sin(a), z0 + pitch * t + dz) for r, dz in prof]
    for i in range(n):
        for j in range(k):
            a0, a1, b0, b1 = i * k + j, i * k + (j + 1) % k, (i + 1) * k + j, (i + 1) * k + (j + 1) % k
            F += [(a0, a1, b1), (a0, b1, b0)]
    for j in range(1, k - 1):
        F += [(0, j + 1, j), (n * k, n * k + j, n * k + j + 1)]
    m = trimesh.Trimesh(np.array(V, float), np.array(F, int), process=True)
    if m.volume < 0: m.invert()
    assert m.is_watertight, 'Gewindenetz offen'
    return m
