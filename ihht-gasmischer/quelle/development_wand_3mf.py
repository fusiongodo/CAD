"""Development-Wand: Netze aus teile/wand_*.py bauen, prüfen und als Bambu-3MF (3 Druckplatten) schreiben.

Prüfung: jedes gedruckte Teil wasserdicht und ein Körper; keine Überschneidung (> 0,2 mm3) zwischen allen Teilen und
Dummies des Moduls; dazu die echten QV-Teile an beiden Plätzen gegen Wand, Grundplatte, Knotenbleche, Träger und
die übrigen Dummies:
  QV v3   teile/qv_*.py (dieses Repo), Rückwand-Rückseite auf Y = 0
  QV v2   ihht-pi CAD/quetschventil gen_rahmen.py + gen_getriebe.py (Pfad IHHT_QV, sonst übersprungen), mit NEMA-Dummy
Ausgabe ../druck/development_wand_1_wand.3mf, _2_grundplatte_traeger.3mf, _3_knotenbleche.3mf (0,20 mm, 4 Wände, 40 %, Brim 5, Wand Brim 3).

Braucht numpy trimesh manifold3d shapely mapbox_earcut:   <venv>/bin/python development_wand_3mf.py
"""
import itertools, math, os, sys
import trimesh
import build
from ihht.netz import build as netz
from ihht.bambu import write_3mf
from ihht.wand import *

HERE = os.path.dirname(os.path.abspath(__file__))
DRUCK = os.path.join(HERE, '..', 'druck')
IHHT_QV = os.environ.get('IHHT_QV', os.path.expanduser('~/Desktop/projects/ihht-retrofit/ihht-pi/CAD/quetschventil'))
BED = 256.0

mods = ['wand_platte', 'wand_grundplatte', 'wand_knotenblech', 'wand_traeger', 'wand_halter', 'wand_dummies']
P = {p.name: p for m in mods for p in build.teil(m).bauen()}
M = {n: netz(p) for n, p in P.items()}
ok = True
for n, m in M.items():
    b = m.bounds
    k = len(m.split(only_watertight=False))
    print('%-14s %s vol %10.1f  wasserdicht %-5s Körper %d  x %6.1f..%6.1f  y %6.1f..%6.1f  z %6.1f..%6.1f' % (
        n, 'D' if P[n].printed else 'T', m.volume, m.is_watertight, k, b[0][0], b[1][0], b[0][1], b[1][1], b[0][2], b[1][2]))
    ok &= m.is_watertight and k == 1


def inter(a, b):
    ba, bb = a.bounds, b.bounds
    if (ba[0] > bb[1]).any() or (bb[0] > ba[1]).any():
        return 0.0
    return trimesh.boolean.intersection([a, b], engine='manifold').volume


def pruefe(paare, titel):
    print('--- %s ---' % titel)
    n = 0
    for a, b, ma, mb in paare:
        v = inter(ma, mb)
        if v > 0.2:
            n += 1
            print('  %s x %s: %.2f mm3' % (a, b, v))
    print('  keine' if n == 0 else '  %d Kollisionen' % n)
    return n == 0


G = set(grid_points())
noetig = [(sx * K_HOLE_X, z) for sx in (-1, 1) for z in K_HOLE_Z] + [(cx + x, cz + z) for cx, cz in TR_POS.values() for x, z in TR_MOUNT]
noetig += [halter_punkt(art, a, sg, u0, u, H_HOLE_V) for _, art, a, sg, u0 in HALTER for u in H_HOLE_U]
fehlt = [p for p in noetig if p not in G]
print('Rasterbohrungen %d, Knotenblech/Träger/Halter auf dem Raster: %s' % (len(G), 'ja' if not fehlt else 'FEHLT %s' % fehlt))
ok &= not fehlt
ok &= pruefe([(a, b, M[a], M[b]) for a, b in itertools.combinations(M, 2)], 'Überschneidungen im Modul (> 0,2 mm3)')

# ---------- echte QV-Teile an beiden Plätzen ----------
FREMD = [n for n in M if not n.startswith(('dwQV', 'dwNema'))]          # alles außer den v2-Dummies


def plaetze(qv, name):
    paare = []
    for cx in (-QX, QX):
        for qn, qm in qv.items():
            m = qm.copy()
            m.apply_translation([cx, 0, QZ])
            paare += [('%s %+d %s' % (name, cx, qn), f, m, M[f]) for f in FREMD]
    return paare


v3mods = ['qv_welle', 'qv_klemmhuelse', 'qv_hohlrad', 'qv_ritzel', 'qv_getriebeplatte', 'qv_rueckwand', 'qv_steg',
          'qv_amboss', 'qv_distanzscheibe', 'qv_dummies']
V3 = {}
for p in (p for m in v3mods for p in build.teil(m).bauen()):
    m = netz(p)
    m.apply_translation([0, -8.0, 0])                                   # Rückwand z -8 .. 0 -> Rückseite auf Y = 0
    V3[p.name] = m
ok &= pruefe(plaetze(V3, 'v3'), 'QV v3 (echte Teile) gegen Wand und Module')

if os.path.isdir(IHHT_QV):
    sys.path.insert(0, IHHT_QV)
    import gen_getriebe, gen_rahmen
    import netz as qvnetz
    R = trimesh.transformations.rotation_matrix(math.pi / 2, [1, 0, 0])   # (x, y, z) -> (x, -z, y)
    V2 = {}
    for p in gen_rahmen.PARTS + gen_getriebe.PARTS:
        m = qvnetz.build(p)
        m.apply_transform(R)
        m.apply_translation([0, -6.0, 0])                               # Rahmenplatte z -6 .. 0 -> Rückseite auf Y = 0
        V2['v2' + p.name] = m
    nema = M['dwNemaR'].copy()
    nema.apply_translation([-QX, 0, -QZ])
    V2['dwNema'] = nema
    ok &= pruefe(plaetze(V2, 'v2'), 'QV v2 (echte Teile gen_rahmen + gen_getriebe) gegen Wand und Module')
else:
    print('--- QV v2: %s fehlt, übersprungen ---' % IHHT_QV)

# ---------- Fluss: Tüllenhöhen und ungefähre Schlauchlängen (Mittellinie, Bogen als Viertelkreis R 15) ----------
def weg(*pts):
    return sum(math.dist(a, b) for a, b in zip(pts, pts[1:]))


yt = OCS['hi'][2] - 3.0                                                  # Tüllenspitze vor dem Bauraum
xh, xl = sum(OCS['hi'][:2]) / 2, sum(OCS['lo'][:2]) / 2
ty, tm = YSTUECK, OCS['mix']
print('--- Fluss (X, Z; Tüllen nach vorn bei Y %.0f) ---' % yt)
print('  Ablass oben          Z %.1f  QV-Ende X ±%.1f -> Durchlass X ±%.0f: ca. %.0f mm bis zur Wand, dann nach hinten' % (
    Z_ABLASS, QX - TUBE_X + 2, abs(ABLASS[0][0]), weg((QX - TUBE_X, TUBE_Y), (abs(ABLASS[0][0]), TUBE_Y), (abs(ABLASS[0][0]), 6)) + 20))
for k, xm in (('hi', xh), ('lo', xl)):
    ein, aus = TUELLEN[k]
    print('  OCS %-3s Eingang Z %.1f (X %+.1f), Ausgang Z %.1f; QV -> OCS ca. %.0f mm, OCS -> Y ca. %.0f mm' % (
        k, ein, xm, aus, weg((-(QX - TUBE_X) if k == 'hi' else QX - TUBE_X, 0, TUBE_Y), (xm, 0, TUBE_Y), (xm, 0, yt)) + 10,
        weg((xm, aus, yt), (xm, ty[5] + 8, yt), (math.copysign(7, xm), ty[5], ty[2] - 3)) + 10))
print('  Y-Stück Eingänge Z %.0f (X ±7), Ausgang Z %.0f -> OCS Mix Eingang X %.1f / Z %.1f: ca. %.0f mm' % (
    ty[5], ty[4], TUELLEN['mix'][0], (tm[4] + tm[5]) / 2, weg((0, ty[4]), (0, tm[5] + 6), (TUELLEN['mix'][0], (tm[4] + tm[5]) / 2)) + 15))
print('  OCS Mix Ausgang X %+.1f / Z %.1f -> rechts unten zum Patienten' % (TUELLEN['mix'][1], (tm[4] + tm[5]) / 2))
print('PRÜFUNG', 'OK' if ok else 'FEHLER')


# ---------- Drucklage ----------
def lay(m, ax, ang):
    m = m.copy()
    m.apply_transform(trimesh.transformations.rotation_matrix(ang, ax))
    c = m.bounds.mean(axis=0)
    m.apply_translation([-c[0], -c[1], -m.bounds[0][2]])
    return m


X, Y = [1, 0, 0], [0, 1, 0]
VORN = lambda n: lay(M[n], X, math.pi / 2)                              # Wand-Vorderseite (Y min) aufs Bett
Z_ = [0, 0, 1]


def HINTEN(n):                                                          # Halter: Flansch-Rückseite (Y = 0) aufs Bett, lang in Bett-Y
    m = lay(M[n], X, -math.pi / 2)
    if m.extents[0] > m.extents[1]:
        m = lay(m, Z_, math.pi / 2)
    return m
PLATTEN = {
    'development_wand_1_wand.3mf': [('Wandplatte', VORN('dwWand'), 128.0, 128.0)],
    'development_wand_2_grundplatte_traeger.3mf': [('Grundplatte', lay(M['dwGrund'], X, 0.0), 128.0, 84.0),
                                                   ('Traeger Pi', VORN('dwTraegerPi'), 62.0, 205.0),
                                                   ('Traeger Pico', VORN('dwTraegerPico'), 154.0, 205.0),
                                                   ('Halter Y', HINTEN('dwHalterY'), 214.0, 205.0),
                                                   ('Halter Mix', HINTEN('dwHalterMix'), 238.0, 205.0)],
    'development_wand_3_knotenbleche.3mf': [('Knotenblech VL', lay(M['dwKnotenVL'], Y, -math.pi / 2), 64.0, 64.0),
                                            ('Knotenblech VR', lay(M['dwKnotenVR'], Y, math.pi / 2), 192.0, 64.0),
                                            ('Knotenblech HL', lay(M['dwKnotenHL'], Y, -math.pi / 2), 64.0, 192.0),
                                            ('Knotenblech HR', lay(M['dwKnotenHR'], Y, math.pi / 2), 192.0, 192.0),
                                            ('Halter Hi', HINTEN('dwHalterHi'), 128.0, 64.0),
                                            ('Halter Lo', HINTEN('dwHalterLo'), 128.0, 192.0)]}
for datei, PRINT in PLATTEN.items():
    for name, m, x, y in PRINT:
        e = m.extents
        x0, x1, y0, y1 = x - e[0] / 2, x + e[0] / 2, y - e[1] / 2, y + e[1] / 2
        print('%-15s Drucklage %5.1f x %5.1f x %5.1f  Bett x %5.1f..%5.1f  y %5.1f..%5.1f' % (name, e[0], e[1], e[2], x0, x1, y0, y1))
        assert m.is_watertight and abs(m.bounds[0][2]) < 1e-6, name
        assert x0 >= 5 and y0 >= 5 and x1 <= BED - 5 and y1 <= BED - 5, name + ': passt mit Brim nicht aufs Bett'
    write_3mf(PRINT, os.path.join(DRUCK, datei), {'brim_width': '3'} if 'wand_1' in datei else None)
raise SystemExit(0 if ok else 1)
