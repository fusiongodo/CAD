# CPAP-Inline-Reservoir-Adapter: sitzt irgendwo im 22-mm-CPAP-Schlauch zwischen Gasquelle und Maske, liegt am Boden.
#   unten (Z = 0)        CPAP-Tuelle Quelle (Zufuhr)
#   seitlich 45°         CPAP-Tuelle Reservoirbeutel, zeigt zur Quellseite (Beutel/Schlauch liegen neben der Zufuhr)
#   Mitte                loser Ventilring wie im Y-Split (Influx, getestet): Sitz Ø 19, Kreuz, Pilzknopf, Membran Ø 21,5
#   oben                 Maskenkappe B mit CPAP-Tuelle zur Maske
# Fluss nur Quelle/Beutel -> Maske; Rueckstrom (Ausatmen) drueckt die Membran auf den Sitz. Beutel haengt VOR der Membran.
# Montage: Membran ueber den Knopf ziehen, Ring (Kreuz voraus) in A legen, B einschrauben (Rechtsgewinde P 2,5):
# B drueckt den Ring auf den 45°-Konus in A -> Ring dichtet unten (Konus) und oben (Stirnflaeche B).
# Druck ohne Stuetzen: A Gewindeseite unten, Ring Kreuz unten, B Tuelle unten.
import math
from ihht.geo import Part, C, Cz, P
from ihht.anschluesse import ANSCHLUSS
from ihht.emit import f
from ihht.gewinde import GEWINDE_FS

T = ANSCHLUSS['cpap_pi']
S = math.sqrt(0.5)

# Ventil (Y-Split, VALVE_VARIABLES / valveRing, Influx-Ring)
MEM_D, SEAT_D, MEM_SP = 21.5, 19.0, 0.5           # Membran (Flap), Sitzloch, Luft rundum
SEAT_H, RING_H = 3.0, 16.0                         # Sitzbund, Ringhoehe (Membranraum 13)
TS = 1.5                                           # Kreuz: Armbreite = Tiefe in der Mitte
PIN = dict(neck=2.5, gap=1.0, head=6.5, flare=60.0, land=0.3, lead=1.5, tip=1.0)
RING_R = MEM_D / 2 + MEM_SP + 1.2                  # 12,45: Wand 1,2 um den Membranraum
RING_FASE = RING_R - T.r_innen                     # 45°-Fase unten (sitzt auf dem Konus in A)

# Gewinde A (innen) / B (aussen): Profil wie Sammler Mix Acryl
TH_P, TH_CLR = 2.5, 0.25
TH_DEPTH = 0.6134 * TH_P
rFmin = RING_R + TH_CLR                            # Innengewinde Kern = Ring passt mit 0,25 Spiel durch
rR = rFmin - TH_CLR                                # Aussengewinde Kern (B)
rC = rR + TH_DEPTH                                 # Aussengewinde Kamm
rFmaj = rC + TH_CLR
GEW_L = 8.0                                        # Gewindelaenge
LUFT_BUND = 0.3                                    # B-Bund bleibt ueber A -> B presst sicher auf den Ring

# Lage (mm, Achse = Z)
R_BODY = 16.0                     # Aussenradius A / Beutel-Arm / Bund B (Wand am Innengewinde 1,8)
Z_W1 = 16.2                       # Wand der Quell-Tuelle (Spitze bei Z = 0)
ZJ = 54.0                         # Beutel-Achse schneidet die Hauptachse; Arm bleibt ueber der Muffe des Quellschlauchs
L0 = 38.0                         # Beutel-Tuelle: Wandabstand von der Hauptachse (Muffe frei vom Rohr)
NB = (S, 0.0, -S)                 # 45° nach unten, zur Quellseite
P_BAG = (S * L0, 0.0, ZJ - S * L0)
Z_L = ZJ + 6.0                    # Konus-Unterkante (Beutel-Oeffnung endet darunter)
Z_R0 = Z_L + 0.15                 # Ring unten
Z_R1 = Z_R0 + RING_H              # Ring oben = Stirnflaeche B
ZA = Z_R1 + GEW_L                 # Oberkante A
FL_H = 6.0                        # Griffbund B
ZF0 = ZA + LUFT_BUND
ZF1 = ZF0 + FL_H
ZB_W = ZF1 + (R_BODY - T.r_spitze)   # Wand der Masken-Tuelle (45°-Uebergang darunter)

LABEL_A = 'CPAP-Reservoir A: Grundkörper (Quelle, Beutel 45°, Innengewinde)'
LABEL_B = 'CPAP-Reservoir B: Maskenkappe (Außengewinde, Ring andrücken)'
COL_A, COL_B = (0.3, 0.6, 0.85), (0.85, 0.55, 0.3)


def teil_a():
    a = Part('cpapResA', LABEL_A, COL_A)
    a.add(*T.koerper_achse((0, 0, Z_W1), (0, 0, -1)),
          ('cone', (0, 0, Z_W1), (0, 0, Z_W1 + R_BODY - T.r_spitze), T.r_spitze, R_BODY),   # 45°-Uebergang Tuelle -> Rohr
          Cz(0, 0, Z_W1 + R_BODY - T.r_spitze - 0.01, ZA, 2 * R_BODY),
          C((0, 0, ZJ), P_BAG, 2 * R_BODY),
          *T.koerper_achse(P_BAG, NB))
    a.cut(Cz(0, 0, -1, Z_L + 0.01, 2 * T.r_innen),
          ('cone', (0, 0, Z_L), (0, 0, Z_L + rFmin - T.r_innen), T.r_innen, rFmin),          # 45°-Konus, Ringauflage
          Cz(0, 0, Z_L + rFmin - T.r_innen - 0.01, ZA + 1, 2 * rFmin),
          ('cone', (0, 0, ZA - (rFmaj + 0.35 - rFmin)), (0, 0, ZA + 0.01), rFmin, rFmaj + 0.36),   # Einfuehrfase
          *T.bohrung_achse(P_BAG, NB, -L0))
    return a


def ventilring():
    r = Part('cpapResRing', 'CPAP-Reservoir Ventilring (Y-Split-Sitz, Membran Ø 21,5)', (0.95, 0.95, 0.95))
    zs = Z_R0 + SEAT_H
    r.add(('cone', (0, 0, Z_R0), (0, 0, Z_R0 + RING_FASE), T.r_innen, RING_R),
          Cz(0, 0, Z_R0 + RING_FASE - 0.01, Z_R1, 2 * RING_R))
    r.cut(Cz(0, 0, Z_R0 - 1, zs + 0.01, SEAT_D),
          Cz(0, 0, zs, Z_R1 + 1, MEM_D + 2 * MEM_SP))
    rw = SEAT_D / 2 + 0.3
    kreuz = [(-rw, 0), (rw, 0), (rw, -SEAT_H), (TS, -SEAT_H), (0, -TS), (-TS, -SEAT_H), (-rw, -SEAT_H)]   # V, unten auf Ringboden
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
    b = Part('cpapResB', LABEL_B, COL_B)
    b.add(Cz(0, 0, Z_R1, ZF0 + 0.01, 2 * rR),                                                   # Gewindekern (Gewinde: FS)
          Cz(0, 0, ZF0, ZF1, 2 * R_BODY),
          ('cone', (0, 0, ZF1 - 0.01), (0, 0, ZB_W), R_BODY, T.r_spitze),
          *T.koerper_achse((0, 0, ZB_W), (0, 0, 1)))
    b.cut(Cz(0, 0, Z_R1 - 1, ZB_W + T.laenge + 1, 2 * T.r_innen),
          *[Cz((R_BODY + 0.6) * math.cos(math.radians(15 + 30 * k)), (R_BODY + 0.6) * math.sin(math.radians(15 + 30 * k)),
               ZF0 - 1, ZF1, 3.0) for k in range(12)])                                          # Griffrillen
    return b


def bauen():
    return [teil_a(), ventilring(), teil_b()]


def fs_zusatz():
    mm = lambda v: '%s * millimeter' % f(v)
    zf = Z_R1 - 1.5 * TH_P                         # gleiche Phase wie B (Z_R1 - 0,5 P), siehe Sammler Acryl
    zm = Z_R1 - 0.5 * TH_P
    z1 = Z_R1 + (rC + 1 - (rR - 0.5))              # 45°-Fase am Gewindeende von B (unten)
    col = lambda c: 'color(%s, %s, %s)' % tuple(f(v) for v in c)
    fs = GEWINDE_FS + (r'''
// ===================== CPAP-Reservoir: Innengewinde A, Aussengewinde B =====================
function crGewinde(context is Context, id is Id)
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
''' % dict(ax=f(-(R_BODY - 0.8)), az=f(Z_L + 4), bz=f(ZF0 + FL_H / 2),
           rFmin=mm(rFmin), rFmaj=mm(rFmaj), rFminO=mm(rFmin - 0.4), zf=mm(zf), P=mm(TH_P), nf=f((ZA + TH_P - zf) / TH_P),
           rR=mm(rR), rC=mm(rC), rRO=mm(rR - 0.4), zm=mm(zm), nm=f((ZF0 + TH_P - zm) / TH_P),
           tz0=f(Z_R1 - 2 * TH_P - 2), z1=f(z1), rbig=mm(rC + 1), zr1=f(Z_R1 - 0.01), z1k=f(z1 + 0.01),
           rk0=mm(rR - 0.5), rk1=mm(rC + 1.01), la=LABEL_A, lb=LABEL_B, ca=col(COL_A), cb=col(COL_B)))
    nach = ['        try', '        {', '            crGewinde(context, id);', '        }', '        catch (error)', '        {',
            '            failed = append(failed, "Gewinde A/B -> " ~ toString(error));', '        }']
    return fs.split('\n'), nach
