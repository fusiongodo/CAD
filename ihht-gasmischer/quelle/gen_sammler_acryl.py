# Sammler Mix (Ausgang) fuer den Acrylschlauch, zweiteilig:
#   1. Sammler: Block, LZQ-Innentuellen 10,75 mit Tropfenbohrung (wie Splitter LZQ-Bau), Kanal, Schienen-Langloecher wie gen5.py; statt der CPAP-Tuelle ein kurzer
#      Gewindestutzen (Aussengewinde M20 x 2,5-artig, 60°, RECHTSGEWINDE). Luftkanal wie Splitter lo: Tropfen Ø 8,
#      Ausgang Tropfen Ø 10 direkt durch den Kanal (keine Kammer, keine Engstelle). Druckt ohne Support.
#   2. Huelse: wird senkrecht gedruckt (Gewindeseite auf dem Bett) und auf den Stutzen geschraubt:
#      Innengewinde unten, 45°-Sitz, darueber Bohrung ACR_ID fuer den Acrylschlauch (Ø 12 aussen),
#      aussen konisch bis auf ACR_TIP_WALL an der Spitze (duenn auslaufend zum Abtapen).
#   Rechts drehen = zuschrauben, links = aufschrauben. Die Gewinderichtung wird in Onshape beim Regenerieren gemessen.
# Aufruf: python3 gen_sammler_acryl.py <ausgabe.fs> <parts.json>   (gen5.py wird dabei mit Wegwerf-Ausgaben ausgefuehrt)
import sys, os, tempfile
here = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, here)
out_fs, out_json = sys.argv[1], sys.argv[2]
tmp = tempfile.mkdtemp()
sys.argv = ['gen5.py', os.path.join(tmp, 'c.fs'), os.path.join(tmp, 'p.json')]
import gen5
from gen5 import *
import json

# --- Gewinde (gedruckt auf gedruckt) ---
TH_MAJ = 20.0                 # Aussengewinde aussen
TH_P = 2.5                    # Steigung
TH_DEPTH = 0.6134 * TH_P      # Gewindetiefe (wie Release-Valve / O2-Adapter)
TH_CLR = 0.25                 # radiales Spiel Innengewinde
STUB_L = 8.0                  # Stutzenlaenge ab Blockvorderseite
STUB_BORE = 10.0              # Ausgangsbohrung (Tropfen), groesser als der Kanal -> keine Engstelle
OUT_DROP = 3.0                # Ausgang 3 mm tiefer als bei der CPAP-Fassung: Bohrung ueberdeckt den Kanal ganz
# --- Huelse ---
ACR_ID = 12.4                 # Bohrung fuer Acrylschlauch 12 (getestet: 12,4 passt am besten)
ACR_LEN = 45.0                # Acryl-Bohrung ab Sitz
ACR_TIP_WALL = 0.6            # Wand an der Spitze
H_TH_L = 10.0                 # Innengewinde-Tiefe (laenger als der Stutzen -> Stirnflaeche liegt am Block an)
H_OD = 26.0                   # Griff aussen
H_FLUTES = 12                 # Griffrillen

rC = TH_MAJ / 2
rR = rC - TH_DEPTH
rFmin = rR + TH_CLR           # Innengewinde Kern
rFmaj = rC + TH_CLR           # Innengewinde aussen

name, label, xin, xout, zc, kind, col = [s for s in SPL if s[0] == 'splMix'][0]
gen.parts.clear()
SPL_Y0, SPL_Y1, SPL_CH = spy(zc)
m = Part('splMixAcr', 'Sammler Mix Acryl (Gewindestutzen)', col)
floor = SHELF_TOP + RAIL_H
xa, xb = spl_extent(xin, xout)
z_out = Z_OUT[kind] - OUT_DROP                      # 135: Huelse (r 13) bleibt 1 mm ueber dem Flansch
prof = TUELLE_PROF[kind]
top = max(zc + 7, Z_OUT[kind] + prof[2] + 2.5)         # Blockhoehe wie CPAP-Fassung
m.add(B(xa, xb, SPL_Y0, SPL_Y1, floor, top))
m.add(B(xa - 10, xb + 10, SPL_Y0 - 8, SPL_Y1 + 8, floor, floor + 3))
for x in xin:
    m.add(*barb_inner(x, zc, SPL_Y0 - 12, SPL_Y0 + 0.5, TUELLE_AD))     # LZQ-Innentuelle 10,75 wie Splitter (LZQ-Bau)
m.add(Cy(xout, SPL_Y1 - 0.5, SPL_Y1 + STUB_L, z_out, 2 * rR))                 # Stutzen-Kern (Gewinde folgt im FS)
xs_all = xin + [xout]
m.cut(T((min(xs_all) - KANAL / 2 + 0.5, SPL_CH, zc), (max(xs_all) + KANAL / 2 - 0.5, SPL_CH, zc), KANAL))   # Kanal wie Splitter lo: Tropfen Ø 8
for x in xin:
    m.cut(T((x, SPL_Y0 - 13, zc), (x, SPL_CH, zc), 6.5))              # Tropfen Ø 6,5 durch die Tuelle bis in den Kanal
m.cut(T((xout, SPL_CH - KANAL / 2 + 0.5, z_out), (xout, SPL_Y1 + STUB_L + 1, z_out), STUB_BORE))   # Ausgang: Tropfen Ø 10 durch Kanal und Stutzen
for hx in (xa - 5, xb + 5):
    m.cut(*slotY(hx, SPL_CH - 10, SPL_CH + 10, floor - 1, floor + 4))

z_seat = H_TH_L + (rFmin - ACR_ID / 2)          # Ende 45°-Sitz
z_top = z_seat + ACR_LEN
r_tip = ACR_ID / 2 + ACR_TIP_WALL
g = lambda v: '%.4f' % v

EXTRA = r'''
// ===================== Gewindestutzen + Huelse (Sammler Mix Acryl) =====================
// Lokales System fuer Gewinde und Huelse: Achse +Z, z = 0 = Blockvorderseite.
// Welt: Achse +Y bei x = XO, z = ZO; lokales z -> Welt y - Y1.
const SM_XO = @XO@;
const SM_Y1 = @Y1@;
const SM_ZO = @ZO@;

function smXf() returns Transform
{
    return transform(vector(SM_XO, SM_Y1, SM_ZO) * millimeter) *
        rotationAround(line(vector(0, 0, 0) * millimeter, vector(1, 0, 0)), -90 * degree);
}

// Rechtsgewinde-Pruefung (aus release_valve_a_v4): von +Z gesehen steigt z bei Drehung gegen den Uhrzeigersinn.
function helixIsRightHand(context is Context, edge is Query, turns is number) returns boolean
{
    const p0 = evEdgeTangentLine(context, { "edge" : edge, "parameter" : 0 }).origin;
    const p1 = evEdgeTangentLine(context, { "edge" : edge, "parameter" : 0.25 / turns }).origin;
    const crossZ = (p0[0] * p1[1] - p0[1] * p1[0]) / (millimeter * millimeter);
    const dz = (p1[2] - p0[2]) / millimeter;
    return crossZ * dz > 0;
}

// Trapez-Gewindezahn (60°), entlang einer Helix um Z gezogen. Immer RECHTSGEWINDE (rechts drehen = zu).
function makeThread(context is Context, id is Id, rRoot is ValueWithUnits, rCrest is ValueWithUnits,
    overlapR is ValueWithUnits, zStart is ValueWithUnits, pitch is ValueWithUnits, turns is number)
{
    const depth = abs(rCrest - rRoot);
    const crest = pitch / 8;
    const wRoot = crest + 2 * depth * tan(30 * degree);
    const rMid = (rRoot + rCrest) / 2;

    var helixId = id + "helix";
    for (var attempt = 0; attempt < 2; attempt += 1)
    {
        helixId = id + "helix" + ("a" ~ attempt);
        opHelix(context, helixId, {
                    "direction" : vector(0, 0, 1),
                    "axisStart" : vector(0, 0, 0) * millimeter,
                    "startPoint" : vector(rMid, 0 * millimeter, zStart),
                    "interval" : vector(0, turns),
                    "clockwise" : attempt == 0,
                    "helicalPitch" : pitch,
                    "spiralPitch" : 0 * millimeter
                });
        if (helixIsRightHand(context, qCreatedBy(helixId, EntityType.EDGE), turns))
            break;
        opDeleteBodies(context, id + "helix" + ("del" ~ attempt), { "entities" : qCreatedBy(helixId, EntityType.BODY) });
        if (attempt == 1)
            throw regenError("Rechtsgewinde konnte nicht erzeugt werden");
    }

    const sk = newSketchOnPlane(context, id + "profile", {
                "sketchPlane" : plane(vector(0, 0, 0) * millimeter, vector(0, -1, 0), vector(1, 0, 0))
            });
    const pts = [
            vector(overlapR, zStart - wRoot / 2),
            vector(rRoot, zStart - wRoot / 2),
            vector(rCrest, zStart - crest / 2),
            vector(rCrest, zStart + crest / 2),
            vector(rRoot, zStart + wRoot / 2),
            vector(overlapR, zStart + wRoot / 2)
        ];
    for (var i = 0; i < size(pts); i += 1)
        skLineSegment(sk, "l" ~ i, { "start" : pts[i], "end" : pts[(i + 1) % size(pts)] });
    skSolve(sk);
    opSweep(context, id + "sweep", { "profiles" : qSketchRegion(id + "profile"), "path" : qCreatedBy(helixId, EntityType.EDGE) });
}

function lz(z is number) returns Vector
{
    return vector(0, 0, z) * millimeter;
}

// Aussengewinde auf den Stutzen des Sammlers, vorne 45° angefast.
function smMaleThread(context is Context, id is Id)
{
    const P = @P@;
    const rC = @RC@;
    const rR = @RR@;
    const L = @STUB_L@;
    const sammler = qContainsPoint(qBodyType(qCreatedBy(id, EntityType.BODY), BodyType.SOLID), vector(SM_XO, SM_Y1 + 5, @ZFL@) * millimeter);
    if (size(evaluateQuery(context, sammler)) != 1)
        throw regenError("Sammler-Koerper nicht gefunden");
    const g = id + "gew";
    makeThread(context, g + "thr", rR * millimeter, rC * millimeter, (rR - 0.4) * millimeter, -0.5 * P * millimeter,
        P * millimeter, (L + 1.5 * P) / P);
    const z0 = L - (rC + 1 - (rR - 0.5));     // 45°-Fase: von Kern - 0,5 an der Spitze bis ueber den Gewindekamm
    fCylinder(context, g + "trim" + "big", { "bottomCenter" : lz(z0), "topCenter" : lz(L + 2 * P + 2), "radius" : (rC + 1) * millimeter });
    fCone(context, g + "trim" + "keep", { "bottomCenter" : lz(z0 - 0.01), "topCenter" : lz(L),
                "bottomRadius" : (rC + 1.01) * millimeter, "topRadius" : (rR - 0.5) * millimeter });
    opBoolean(context, g + "trim" + "sub", { "tools" : qCreatedBy(g + "trim" + "keep", EntityType.BODY),
                "targets" : qCreatedBy(g + "trim" + "big", EntityType.BODY), "operationType" : BooleanOperationType.SUBTRACTION });
    opDeleteBodies(context, g + "clean", { "entities" : qUnion([qCreatedBy(g + "thr" + "helix", EntityType.BODY),
                    qCreatedBy(g + "thr" + "profile", EntityType.BODY)]) });
    opTransform(context, g + "xf", { "bodies" : qUnion([qCreatedBy(g + "thr" + "sweep", EntityType.BODY),
                    qCreatedBy(g + "trim", EntityType.BODY)]), "transform" : smXf() });
    opBoolean(context, g + "u", { "tools" : qUnion([sammler, qCreatedBy(g + "thr" + "sweep", EntityType.BODY)]),
                "operationType" : BooleanOperationType.UNION });
    opBoolean(context, g + "x", { "tools" : qCreatedBy(g + "trim", EntityType.BODY), "targets" : sammler,
                "operationType" : BooleanOperationType.SUBTRACTION });
    finishPart(context, sammler, "@LABEL@", color(@COL@));
}

// Aufschraub-Huelse fuer den Acrylschlauch. Druck: Gewindeseite (z = 0) auf dem Bett, kein Support.
function smHuelse(context is Context, id is Id)
{
    const P = @P@;
    const rFmin = @RFMIN@;
    const rFmaj = @RFMAJ@;
    const rA = @RA@;
    const Lt = @HTL@;
    const zSeat = @ZSEAT@;
    const zTop = @ZTOP@;
    const rTip = @RTIP@;
    const rO = @RO@;
    const h = id + "huelse";
    fCylinder(context, h + "a" + "grip", { "bottomCenter" : lz(0), "topCenter" : lz(Lt + 2), "radius" : rO * millimeter });
    fCone(context, h + "a" + "cone", { "bottomCenter" : lz(Lt + 1.99), "topCenter" : lz(zTop),
                "bottomRadius" : rO * millimeter, "topRadius" : rTip * millimeter });
    opBoolean(context, h + "u", { "tools" : qCreatedBy(h + "a", EntityType.BODY), "operationType" : BooleanOperationType.UNION });
    fCylinder(context, h + "c" + "core", { "bottomCenter" : lz(-1), "topCenter" : lz(Lt), "radius" : rFmin * millimeter });
    fCone(context, h + "c" + "seat", { "bottomCenter" : lz(Lt - 0.01), "topCenter" : lz(zSeat),
                "bottomRadius" : (rFmin + 0.01) * millimeter, "topRadius" : rA * millimeter });
    fCylinder(context, h + "c" + "bore", { "bottomCenter" : lz(zSeat - 0.01), "topCenter" : lz(zTop + 1), "radius" : rA * millimeter });
    fCone(context, h + "c" + "lead", { "bottomCenter" : lz(-0.01), "topCenter" : lz(rFmaj + 0.35 - rFmin),
                "bottomRadius" : (rFmaj + 0.36) * millimeter, "topRadius" : rFmin * millimeter });   // 45°-Einfuehrung
    for (var k = 0; k < @NFL@; k += 1)
    {
        const a = k * 360 / @NFL@ * degree;
        fCylinder(context, h + "c" + ("fl" ~ k), { "bottomCenter" : vector((rO + 0.6) * cos(a), (rO + 0.6) * sin(a), -1) * millimeter,
                    "topCenter" : vector((rO + 0.6) * cos(a), (rO + 0.6) * sin(a), Lt) * millimeter, "radius" : 1.5 * millimeter });
    }
    makeThread(context, h + "thr", rFmin * millimeter, rFmaj * millimeter, (rFmin - 0.4) * millimeter, -1.5 * P * millimeter,
        P * millimeter, (Lt + P) / P);     // Phase wie Stutzen (Start -0,5 P): in Einbaulage kollisionsfrei
    opBoolean(context, h + "x", { "tools" : qUnion([qCreatedBy(h + "c", EntityType.BODY), qCreatedBy(h + "thr" + "sweep", EntityType.BODY)]),
                "targets" : qCreatedBy(h + "a", EntityType.BODY), "operationType" : BooleanOperationType.SUBTRACTION });
    opDeleteBodies(context, h + "clean", { "entities" : qUnion([qCreatedBy(h + "thr" + "helix", EntityType.BODY),
                    qCreatedBy(h + "thr" + "profile", EntityType.BODY)]) });
    opTransform(context, h + "xf", { "bodies" : qCreatedBy(h + "a", EntityType.BODY), "transform" : smXf() });
    finishPart(context, qCreatedBy(h + "a", EntityType.BODY), "Sammler Mix Huelse Acryl ID @ACRID@ (senkrecht drucken, Gewinde unten)", color(@COL@));
}
'''
rep = {'@XO@': g(xout), '@Y1@': g(SPL_Y1), '@ZO@': g(z_out), '@P@': g(TH_P), '@RC@': g(rC), '@RR@': g(rR),
       '@STUB_L@': g(STUB_L), '@ZFL@': g(floor + 1.5), '@LABEL@': m.label, '@COL@': ', '.join(g(c) for c in col),
       '@RFMIN@': g(rFmin), '@RFMAJ@': g(rFmaj), '@RA@': g(ACR_ID / 2), '@HTL@': g(H_TH_L), '@ZSEAT@': g(z_seat),
       '@ZTOP@': g(z_top), '@RTIP@': g(r_tip), '@RO@': g(H_OD / 2), '@NFL@': str(H_FLUTES), '@ACRID@': '%.1f' % ACR_ID}
for k, v in rep.items():
    EXTRA = EXTRA.replace(k, v)
assert '@' not in EXTRA

# FeatureScript: Kopf (Hilfsfunktionen) aus gen5 uebernehmen, dann dieses Teil + Gewinde + Huelse
L = list(gen5.L)
cut = next(i for i, l in enumerate(L) if l.startswith('function dev_'))
L = L[:cut]
L.insert(2, '// Sammler Mix Acryl: erzeugt von gen_sammler_acryl.py (Basis gen5.py). Gewindestutzen %.0f x %.1f rechts, '
         'Huelse ID %.1f, %.0f mm, aussen %.0f -> %.1f.' % (TH_MAJ, TH_P, ACR_ID, ACR_LEN, H_OD, 2 * r_tip))
L += EXTRA.split('\n')
emit_feature(L, [m], 'acr_', 'Sammler Mix Acryl', 'sammlerMixAcrylV1')
i = max(j for j, l in enumerate(L) if l.strip() == 'if (size(failed) > 0)')
L[i:i] = ['        try', '        {', '            smMaleThread(context, id);', '        }', '        catch (error)', '        {',
          '            failed = append(failed, "Gewinde Stutzen -> " ~ toString(error));', '        }',
          '        try', '        {', '            smHuelse(context, id);', '        }', '        catch (error)', '        {',
          '            failed = append(failed, "Huelse -> " ~ toString(error));', '        }']

open(out_fs, 'w').write('\n'.join(L) + '\n')
json.dump([dict(name=m.name, label=m.label, col=m.col, printed=True, stages=m.stages, group='dev')], open(out_json, 'w'))
print('z_out', z_out, 'Y1', SPL_Y1, 'top', top, 'floor', floor, 'rR', round(rR, 3), 'rFmin', round(rFmin, 3),
      'rFmaj', rFmaj, 'z_seat', round(z_seat, 3), 'z_top', round(z_top, 3), 'tip OD', 2 * r_tip)
