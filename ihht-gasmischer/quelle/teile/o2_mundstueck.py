# O2-Mundstueck mit Reservoirbeutel ("Pfeife", 26.11.2026, Alex): fuer die hyperoxische Phase. Nur einatmen, zum Ausatmen
# aus dem Mund nehmen: kein Ausatemventil. Achse = Z, Mund oben.
#   unten (Z = 0)     Kegeltuelle 5 -> 7 fuer den duennen O2-Schlauch, Bohrung 3
#   seitlich 45°      Beutelring Ø 20 mit Wulst, zeigt zur Schlauchseite (Beutel liegt neben dem O2-Schlauch)
#   Mitte             loser Ventilring wie im CPAP-Reservoir (Sitz Ø 19, Kreuz, Pilzknopf, Membran Ø 21,5), nur 10 hoch
#   oben              Mundstueck B: in A eingeschraubt (Rechtsgewinde P 2,5 wie CPAP-Reservoir), drueckt den Ring auf den Konus
# Die Klappe haelt den Sauerstoff zurueck, solange niemand zieht: der Beutel fuellt sich. Ist er voll, drueckt der
# Ueberschuss die Membran auf und entweicht durchs Mundstueck. Beim Zug kommt der Beutelinhalt auf einmal.
# Druck ohne Stuetzen: A mit der Oeffnung nach unten (Tuelle oben), Ring Kreuz unten, B Stecker unten.
import math
from ihht.geo import Part, C, Cz, P
from ihht.emit import f
from ihht.gewinde import GEWINDE_FS

S = math.sqrt(0.5)
RC = 10.2                                          # Kammer innen (wie CPAP-Reservoir: Ring sitzt auf demselben Konus)
MEM_D, SEAT_D, MEM_SP = 21.5, 19.0, 0.5
SEAT_H, RING_H = 3.0, 10.0
TS = 1.5
PIN = dict(neck=2.5, gap=1.0, head=6.5, flare=60.0, land=0.3, lead=1.5, tip=1.0)
RING_R = MEM_D / 2 + MEM_SP + 1.2                  # 12,45
RING_FASE = RING_R - RC
# Gewinde A (innen) / B (aussen): Profil und Masse wie CPAP-Reservoir
TH_P, TH_CLR = 2.5, 0.25
TH_DEPTH = 0.6134 * TH_P
rFmin = RING_R + TH_CLR                            # Innengewinde Kern = Ring passt mit 0,25 Spiel durch
rR = rFmin - TH_CLR                                # Aussengewinde Kern (B)
rC = rR + TH_DEPTH                                 # Aussengewinde Kamm
rFmaj = rC + TH_CLR
R_SOCK = rFmin
R_BODY = 16.0                                      # Aussen Ø 32 (Wand am Innengewinde 1,8)

TU_R0, TU_R1, TU_L, TU_BOHR = 2.5, 3.5, 12.0, 3.0  # O2-Tuelle
Z_K0 = TU_L + (R_BODY - TU_R1)                     # Aussenkegel Tuelle -> Rohr endet hier (22,75)
ZJ = 34.0                                          # Beutel-Achse schneidet die Hauptachse
BAG_R, BAG_RI, BAG_L = 10.0, 8.0, 42.0             # Beutelring Ø 20 / 16, Laenge ab Hauptachse
NB = (S, 0.0, -S)
Z_L = ZJ + 6.0
Z_R0 = Z_L + 0.15
Z_R1 = Z_R0 + RING_H
GEW_L = 8.0
ZA = Z_R1 + GEW_L                                 # Oberkante A
ZF0, FL_H = ZA + 0.3, 5.0
ZF1 = ZF0 + FL_H
MU_R, MU_RI, MU_L = 7.5, 5.5, 14.0                 # Mundrohr Ø 15 / 11
ZM0 = ZF1 + (R_BODY - MU_R)
ZM1 = ZM0 + MU_L

COL_A, COL_B = (0.3, 0.6, 0.85), (0.85, 0.55, 0.3)
LABEL_A = 'O2-Mundstück A: Grundkörper (Schlauchtülle, Beutelring 45°, Innengewinde)'
LABEL_B = 'O2-Mundstück B: Mundrohr (Außengewinde, drückt den Ring an)'

# Gewinde: eine Quelle fuer FeatureScript (fs_zusatz) und Drucknetz (o2_mundstueck_3mf.py, ihht.netz.gewinde).
# Gleiche Phase: Innengewinde beginnt genau eine Steigung unter dem Aussengewinde.
GEW_A = dict(r_root=rFmin, r_crest=rFmaj, overlap=rFmin - 0.4, z0=Z_R1 - 1.5 * TH_P, pitch=TH_P)
GEW_A['turns'] = (ZA + TH_P - GEW_A['z0']) / TH_P
GEW_B = dict(r_root=rR, r_crest=rC, overlap=rR - 0.4, z0=Z_R1 - 0.5 * TH_P, pitch=TH_P)
GEW_B['turns'] = (ZF0 + TH_P - GEW_B['z0']) / TH_P
TRIM_Z1 = Z_R1 + (rC + 1 - (rR - 0.5))            # 45°-Fase am Gewindeende von B
TRIM = dict(z0=Z_R1 - 2 * TH_P - 2, z1=TRIM_Z1, r_big=rC + 1, keep_z0=Z_R1 - 0.01, keep_z1=TRIM_Z1 + 0.01, keep_r0=rR - 0.5, keep_r1=rC + 1.01)


def _bag(t):
    return (NB[0] * t, 0.0, ZJ + NB[2] * t)


def teil_a():
    a = Part('o2mA', LABEL_A, COL_A)
    a.add(('cone', (0, 0, 0), (0, 0, TU_L), TU_R0, TU_R1),
          ('cone', (0, 0, TU_L - 0.01), (0, 0, Z_K0), TU_R1, R_BODY),
          Cz(0, 0, Z_K0 - 0.01, ZA, 2 * R_BODY),
          C(_bag(0), _bag(BAG_L), 2 * BAG_R),
          ('cone', _bag(BAG_L - 4.0), _bag(BAG_L - 3.2), BAG_R, BAG_R + 0.8),          # Wulst: Beutel rutscht nicht ab
          C(_bag(BAG_L - 3.21), _bag(BAG_L - 1.6), 2 * (BAG_R + 0.8)),
          ('cone', _bag(BAG_L - 1.61), _bag(BAG_L - 0.8), BAG_R + 0.8, BAG_R))
    zc = Z_K0 + 1.0                                                                     # Kammerboden: 45°-Kegel auf die Bohrung
    a.cut(Cz(0, 0, -1, zc - (RC - TU_BOHR / 2) + 0.01, TU_BOHR),
          ('cone', (0, 0, zc - (RC - TU_BOHR / 2)), (0, 0, zc), TU_BOHR / 2, RC),
          Cz(0, 0, zc - 0.01, Z_L + 0.01, 2 * RC),
          ('cone', (0, 0, Z_L), (0, 0, Z_L + R_SOCK - RC), RC, R_SOCK),                 # 45°-Konus, Ringauflage
          Cz(0, 0, Z_L + R_SOCK - RC - 0.01, ZA + 1, 2 * R_SOCK),
          ('cone', (0, 0, ZA - (rFmaj + 0.35 - rFmin)), (0, 0, ZA + 0.01), rFmin, rFmaj + 0.36),   # Einfuehrfase
          C(_bag(0), _bag(BAG_L + 1), 2 * BAG_RI))
    return a


def ventilring():
    r = Part('o2mRing', 'O2-Mundstück Ventilring (Sitz Ø 19, Membran Ø 21,5)', (0.95, 0.95, 0.95))
    zs = Z_R0 + SEAT_H
    r.add(('cone', (0, 0, Z_R0), (0, 0, Z_R0 + RING_FASE), RC, RING_R),
          Cz(0, 0, Z_R0 + RING_FASE - 0.01, Z_R1, 2 * RING_R))
    r.cut(Cz(0, 0, Z_R0 - 1, zs + 0.01, SEAT_D),
          Cz(0, 0, zs, Z_R1 + 1, MEM_D + 2 * MEM_SP))
    rw = SEAT_D / 2 + 0.3
    kreuz = [(-rw, 0), (rw, 0), (rw, -SEAT_H), (TS, -SEAT_H), (0, -TS), (-TS, -SEAT_H), (-rw, -SEAT_H)]
    p = PIN
    fl = (p['head'] - p['neck']) / 2 * math.tan(math.radians(p['flare']))
    z1 = zs + p['gap']; z2 = z1 + fl; z3 = z2 + p['land']; z4 = z3 + p['lead']
    r.add(P(0, zs, -TS / 2, TS / 2, kreuz),
          ('prismx', (0, zs), tuple(kreuz), -TS / 2, TS / 2),
          Cz(0, 0, zs - TS / 2, z1, p['neck']),
          ('cone', (0, 0, z1), (0, 0, z2), p['neck'] / 2, p['head'] / 2),
          Cz(0, 0, z2, z3, p['head']),
          ('cone', (0, 0, z3), (0, 0, z4), p['head'] / 2, p['tip'] / 2))
    return r


def teil_b():
    b = Part('o2mB', LABEL_B, COL_B)
    b.add(Cz(0, 0, Z_R1, ZF0 + 0.01, 2 * rR),                                           # Gewindekern (Gewinde: GEW_A / GEW_B)
          Cz(0, 0, ZF0, ZF1, 2 * R_BODY),
          ('cone', (0, 0, ZF1 - 0.01), (0, 0, ZM0), R_BODY, MU_R),
          Cz(0, 0, ZM0 - 0.01, ZM1, 2 * MU_R),
          ('cone', (0, 0, ZM1 - 3.6), (0, 0, ZM1 - 2.6), MU_R, MU_R + 1.0),             # Lippenwulst
          Cz(0, 0, ZM1 - 2.61, ZM1 - 1.0, 2 * (MU_R + 1.0)),
          ('cone', (0, 0, ZM1 - 1.01), (0, 0, ZM1), MU_R + 1.0, MU_R))
    zi = ZF1 + 0.5
    b.cut(Cz(0, 0, Z_R1 - 1, zi + 0.01, 2 * RC),
          ('cone', (0, 0, zi), (0, 0, zi + RC - MU_RI), RC, MU_RI),
          Cz(0, 0, zi + RC - MU_RI - 0.01, ZM1 + 1, 2 * MU_RI),
          *[Cz((R_BODY + 0.6) * math.cos(math.radians(15 + 30 * k)), (R_BODY + 0.6) * math.sin(math.radians(15 + 30 * k)),
               ZF0 - 1, ZF1, 3.0) for k in range(12)])                                  # Griffrillen
    return b


def bauen():
    return [teil_a(), ventilring(), teil_b()]


def fs_zusatz():
    mm = lambda v: '%s * millimeter' % f(v)
    col = lambda c: 'color(%s, %s, %s)' % tuple(f(v) for v in c)
    a, b, t = GEW_A, GEW_B, TRIM
    fs = GEWINDE_FS + (r"""
// ===================== O2-Mundstueck: Innengewinde A, Aussengewinde B =====================
function o2mGewinde(context is Context, id is Id)
{
    const a = gewKoerper(context, id, vector(%(ax)s, 0, %(az)s) * millimeter);
    makeThread(context, id + "giA", %(rFmin)s, %(rFmaj)s, %(rFminO)s, %(zf)s, %(P)s, %(nf)s);
    opBoolean(context, id + "giAx", { "tools" : qCreatedBy(id + "giA" + "sweep", EntityType.BODY), "targets" : a,
                "operationType" : BooleanOperationType.SUBTRACTION });
    finishPart(context, a, "%(la)s", %(ca)s);

    const b = gewKoerper(context, id, vector(%(ax)s, 0, %(bz)s) * millimeter);
    makeThread(context, id + "gaB", %(rR)s, %(rC)s, %(rRO)s, %(zm)s, %(P)s, %(nm)s);
    fCylinder(context, id + "gaBtrim" + "big", { "bottomCenter" : vector(0, 0, %(tz0)s) * millimeter,
                "topCenter" : vector(0, 0, %(z1)s) * millimeter, "radius" : %(rbig)s });
    fCone(context, id + "gaBtrim" + "keep", { "bottomCenter" : vector(0, 0, %(zr1)s) * millimeter,
                "topCenter" : vector(0, 0, %(z1k)s) * millimeter, "bottomRadius" : %(rk0)s, "topRadius" : %(rk1)s });
    opBoolean(context, id + "gaBtrim" + "sub", { "tools" : qCreatedBy(id + "gaBtrim" + "keep", EntityType.BODY),
                "targets" : qCreatedBy(id + "gaBtrim" + "big", EntityType.BODY), "operationType" : BooleanOperationType.SUBTRACTION });
    opBoolean(context, id + "gaBu", { "tools" : qUnion([b, qCreatedBy(id + "gaB" + "sweep", EntityType.BODY)]),
                "operationType" : BooleanOperationType.UNION });
    opBoolean(context, id + "gaBx", { "tools" : qCreatedBy(id + "gaBtrim", EntityType.BODY), "targets" : b,
                "operationType" : BooleanOperationType.SUBTRACTION });
    finishPart(context, b, "%(lb)s", %(cb)s);
}
""" % dict(ax=f(-(R_BODY - 0.8)), az=f(Z_L + 4), bz=f(ZF0 + FL_H / 2 + 1.5),
           rFmin=mm(a['r_root']), rFmaj=mm(a['r_crest']), rFminO=mm(a['overlap']), zf=mm(a['z0']), P=mm(TH_P), nf=f(a['turns']),
           rR=mm(b['r_root']), rC=mm(b['r_crest']), rRO=mm(b['overlap']), zm=mm(b['z0']), nm=f(b['turns']),
           tz0=f(t['z0']), z1=f(t['z1']), rbig=mm(t['r_big']), zr1=f(t['keep_z0']), z1k=f(t['keep_z1']),
           rk0=mm(t['keep_r0']), rk1=mm(t['keep_r1']), la=LABEL_A, lb=LABEL_B, ca=col(COL_A), cb=col(COL_B)))
    nach = ['        try', '        {', '            o2mGewinde(context, id);', '        }', '        catch (error)', '        {',
            '            failed = append(failed, "Gewinde A/B -> " ~ toString(error));', '        }']
    return fs.split('\n'), nach
