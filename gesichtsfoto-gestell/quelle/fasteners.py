"""Gedruckte Verbindungselemente M10 x 1,5 – Gewinde als Netz (r(θ,z)-Fläche), Boolesche Operationen mit manifold3d."""
import math
import numpy as np
import trimesh
from shapely.geometry import Point, Polygon

P, D = 1.5, 10.0
CLR_EXT, CLR_INT = 0.15, 0.15
TAN30 = math.tan(math.radians(30))

# Außengewinde (Bolzen)
RC = (D - 1.2269 * P) / 2 - CLR_EXT          # Kern
RM = D / 2 - CLR_EXT                          # Spitze
W_CREST = 0.25
W_ROOT = W_CREST + 2 * (RM - RC) * TAN30
# Innengewinde (Mutter / Säule)
RB = (D - 1.0825 * P) / 2 + CLR_INT           # Kernloch
RO = D / 2 + 0.2                              # Gewindegrund
G_OUT = 0.25
G_IN = G_OUT + 2 * (RO - RB) * TAN30
SHANK_R = 4.85                                # glatter Schaft (Gelenk)

def _u(theta, z):
    return np.mod(z - P * theta / (2 * math.pi), P) - P / 2   # -P/2..P/2, Mitte der Leiste bei 0

def _trap(u, r_lo, r_hi, w_lo, w_hi):
    a = np.abs(u)
    t = np.clip((w_lo / 2 - a) / ((w_lo - w_hi) / 2), 0, 1)
    return r_lo + (r_hi - r_lo) * t

def ext_r(theta, z):
    return _trap(_u(theta, z), RC, RM, W_ROOT, W_CREST)

def int_r(theta, z):
    return _trap(_u(theta, z), RB, RO, G_IN, G_OUT)

def rod(rfunc, z0, z1, n_theta=144, dz=P / 18):
    nz = max(2, int(math.ceil((z1 - z0) / dz)) + 1)
    zs = np.linspace(z0, z1, nz)
    th = np.linspace(0, 2 * math.pi, n_theta, endpoint=False)
    T, Z = np.meshgrid(th, zs)
    R = rfunc(T, Z)
    V = np.stack([R * np.cos(T), R * np.sin(T), Z], -1).reshape(-1, 3)
    F = []
    idx = lambda i, j: i * n_theta + (j % n_theta)
    for i in range(nz - 1):
        for j in range(n_theta):
            a, b, c, d = idx(i, j), idx(i, j + 1), idx(i + 1, j + 1), idx(i + 1, j)
            F += [(a, b, c), (a, c, d)]
    nb = len(V)
    V = np.vstack([V, [0, 0, z0], [0, 0, z1]])
    for j in range(n_theta):
        F.append((nb, idx(0, j + 1), idx(0, j)))
        F.append((nb + 1, idx(nz - 1, j), idx(nz - 1, j + 1)))
    m = trimesh.Trimesh(V, np.array(F), process=True)
    m.fix_normals()
    return m

def knurl(r_out=11.0, r_in=10.2, lobes=18):
    pts = []
    for k in range(lobes * 2):
        a = math.pi * k / lobes
        r = r_out if k % 2 == 0 else r_in
        pts.append((r * math.cos(a), r * math.sin(a)))
    return Polygon(pts).buffer(0.9, quad_segs=6).buffer(-1.6, quad_segs=6).buffer(0.7, quad_segs=6)

def prism(poly, z0, z1):
    m = trimesh.creation.extrude_polygon(poly, z1 - z0)
    m.apply_translation([0, 0, z0])
    return m

def U(*ms):
    return trimesh.boolean.union(ms, engine="manifold")

def Dif(a, *bs):
    return trimesh.boolean.difference([a, *bs], engine="manifold")

def head():
    """Rändelkopf mit Bund, Auflagefläche bei z=0, Kopf nach unten (Druckbett)."""
    k = prism(knurl(), -8.0, -1.4)
    fl = prism(Point(0, 0).buffer(10.0, quad_segs=32), -1.5, 0.0)
    return U(k, fl)

def bolt(length, smooth_to=0.0):
    """Schraube: Kopf unten, Schaft von z=0 bis length. Bis smooth_to glatter Schaft, danach Gewinde."""
    tip = length
    def r(theta, z):
        rr = np.where(z < smooth_to, SHANK_R, ext_r(theta, z))
        cone = (RC - 0.4) + (tip - z)        # 45°-Fase an der Spitze
        return np.minimum(rr, cone)
    shaft = rod(r, -0.6, length)
    return U(head(), shaft)

def nut(h=8.0):
    body = U(prism(knurl(), 0, h - 1.4), prism(Point(0, 0).buffer(10.0, quad_segs=32), h - 1.5, h))
    return Dif(body, thread_cutter(0, h))

def thread_cutter(z0, z1):
    def r(theta, z):
        rr = int_r(theta, z)
        sink = np.maximum(RO + 0.5 - (z - z0), RO + 0.5 - (z1 - z))   # Senkung an beiden Enden
        return np.maximum(rr, np.minimum(sink, RO + 0.5))
    return rod(r, z0 - 0.8, z1 + 0.8)
