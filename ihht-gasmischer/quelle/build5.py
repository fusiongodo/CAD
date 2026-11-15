# Baut aus parts_v5.json die Netze (Manifold), prüft Überschneidungen und schreibt STL + Viewer-Daten.
# Aufruf: python3 build5.py parts_v5.json <ausgabe-ordner>   (parts_v5_view.json aus gen5.py liegt daneben)
import sys, json, itertools
import numpy as np, trimesh
from shapely.geometry import Polygon
import mkgeom

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
    return mkgeom.mk(tuple(o) if not isinstance(o, tuple) else o)

def build(p):
    body = None
    for kind, ops in p['stages']:
        ms = [mk([tuple(x) if isinstance(x, list) else x for x in o]) for o in ops]
        if kind == 'add':
            body = trimesh.boolean.union(([body] if body is not None else []) + ms, engine='manifold')
        else:
            body = trimesh.boolean.difference([body] + ms, engine='manifold')
    return body

if __name__ == '__main__':
    parts = json.load(open(sys.argv[1]))
    out = sys.argv[2]
    M = {}
    for p in parts:
        m = build(p)
        M[p['name']] = m
        b = m.bounds
        print('%-11s vol %8.1f mm3  watertight %s  x %.1f..%.1f  y %.1f..%.1f  z %.1f..%.1f' % (
            p['name'], m.volume, m.is_watertight, b[0][0], b[1][0], b[0][1], b[1][1], b[0][2], b[1][2]))
    print('--- Überschneidungen (> 0,5 mm3) ---')
    n = 0
    for a, b in itertools.combinations(M, 2):
        ba, bb = M[a].bounds, M[b].bounds
        if (ba[0] > bb[1]).any() or (bb[0] > ba[1]).any():
            continue
        v = trimesh.boolean.intersection([M[a], M[b]], engine='manifold').volume
        if v > 0.5:
            n += 1
            print('  %s x %s: %.1f mm3' % (a, b, v))
    print('  keine' if n == 0 else '  %d Paare' % n)
    json.dump({k: dict(v=np.round(m.vertices, 3).tolist(), f=m.faces.tolist()) for k, m in M.items()}, open(out + '/meshes_v5.json', 'w'))
    stl = {'frame5': 'v5_rahmen', 'shelf5': 'v5_regal', 'bridge5': 'v5_motorbruecke', 'portal5': 'v5_kameraportal', 'camMount5': 'v5_kamerahalter',
           'gearDrive1': 'v5_zahnradkupplung', 'gearIdle1': 'v5_zahnradnabe'}
    for k, fn in stl.items():
        m = M[k].copy()
        if k.startswith(('gear', 'portal', 'camMount')):   # Drucklage: Rückseite (kleinstes y) unten -> Zahnrad bzw. Plattenrückseite auf dem Bett
            c = m.bounds.mean(axis=0)
            v = m.vertices
            m.vertices = np.column_stack([v[:, 0] - c[0], -(v[:, 2] - c[2]), v[:, 1] - m.bounds[0][1]])
        m.export(out + '/' + fn + '.stl')
    models = []
    for p in parts:
        m = M[p['name']]
        models.append(dict(name=p['name'], label=p['label'], col=[round(c, 2) for c in p['col'][:3]], dummy=p['name'].endswith(('D', 'D1', 'D2', 'D3', 'D4')),
                           pos=np.round(m.vertices[m.faces].reshape(-1), 2).tolist()))
    view = json.load(open(sys.argv[1].replace('.json', '_view.json')))
    open(out + '/models.js', 'w').write('const MODELS = ' + json.dumps(models, separators=(',', ':')) + ';\nconst VIEW = ' + json.dumps(view) + ';\n')
