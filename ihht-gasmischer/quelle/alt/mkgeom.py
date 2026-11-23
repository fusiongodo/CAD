import numpy as np, trimesh
def mk(o):
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
