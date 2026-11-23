# Sammler Mix (Ausgang) fuer den Acrylschlauch, zweiteilig:
#   1. Sammler: Block, LZQ-Innentuellen 10,75 mit Tropfenbohrung (wie Splitter LZQ-Bau), Kanal, Schienen-Langloecher wie gen5.py; statt der CPAP-Tuelle ein kurzer
#      Gewindestutzen (Aussengewinde M20 x 2,5-artig, 60°, RECHTSGEWINDE). Luftkanal wie Splitter lo: Tropfen Ø 8,
#      Ausgang Tropfen Ø 10 direkt durch den Kanal (keine Kammer, keine Engstelle). Druckt ohne Support.
#   2. Huelse: wird senkrecht gedruckt (Gewindeseite auf dem Bett) und auf den Stutzen geschraubt:
#      Innengewinde unten, 45°-Sitz, darueber Bohrung ACR_ID fuer den Acrylschlauch (Ø 12 aussen),
#      aussen konisch bis auf ACR_TIP_WALL an der Spitze (duenn auslaufend zum Abtapen).
#   Rechts drehen = zuschrauben, links = aufschrauben. Die Gewinderichtung wird in Onshape beim Regenerieren gemessen.
from ihht.geo import *
from ihht.masse import *
from ihht.splitter import splitter
from ihht.anschluesse import ANSCHLUSS

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

FARBE = (0.6, 0.4, 0.8, 1.0)
LABEL = 'Sammler Mix Acryl (Gewindestutzen)'
XIN, XOUT, ZC = SPL_PLATZ['mix']
Y0, Y1, CH = spy(ZC)
Z_AUS = Z_OUT['mix'] - OUT_DROP                      # 135: Huelse (r 13) bleibt 1 mm ueber dem Flansch
FLOOR = SHELF_TOP + RAIL_H
TOP = max(ZC + 7, Z_OUT['mix'] + ANSCHLUSS['cpap'].r_spitze + 2.5)     # Blockhoehe wie CPAP-Fassung

class Gewindestutzen:
    """Aussen-Anschluss des Sammlers: glatter Stutzen-Kern, das Gewinde schneidet das FeatureScript (FS_EXTRA)."""
    laenge, r_max = STUB_L, rC
    def koerper(s, x, z, y0, d=1):
        return [Cy(x, y0 - 0.5, y0 + STUB_L, z, 2 * rR)]
    def bohrung(s, x, z, y0, d, y_kanal, tief=0.0):       # Tropfen Ø 10 durch Kanal und Stutzen, keine Engstelle
        return [T((x, y_kanal - KANAL / 2 + 0.5, z), (x, y0 + STUB_L + 1, z), STUB_BORE)]

def bauen():
    return [splitter('splMixAcr', LABEL, FARBE, 'mix', 'lzq', Gewindestutzen(), z_aus=Z_AUS, top=TOP)]

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
def fs_zusatz():
    rep = {'@XO@': g(XOUT), '@Y1@': g(Y1), '@ZO@': g(Z_AUS), '@P@': g(TH_P), '@RC@': g(rC), '@RR@': g(rR),
           '@STUB_L@': g(STUB_L), '@ZFL@': g(FLOOR + 1.5), '@LABEL@': LABEL, '@COL@': ', '.join(g(c) for c in FARBE),
           '@RFMIN@': g(rFmin), '@RFMAJ@': g(rFmaj), '@RA@': g(ACR_ID / 2), '@HTL@': g(H_TH_L), '@ZSEAT@': g(z_seat),
           '@ZTOP@': g(z_top), '@RTIP@': g(r_tip), '@RO@': g(H_OD / 2), '@NFL@': str(H_FLUTES), '@ACRID@': '%.1f' % ACR_ID}
    extra = EXTRA
    for k, v in rep.items():
        extra = extra.replace(k, v)
    assert '@' not in extra
    nach = ['        try', '        {', '            smMaleThread(context, id);', '        }', '        catch (error)', '        {',
            '            failed = append(failed, "Gewinde Stutzen -> " ~ toString(error));', '        }',
            '        try', '        {', '            smHuelse(context, id);', '        }', '        catch (error)', '        {',
            '            failed = append(failed, "Huelse -> " ~ toString(error));', '        }']
    return extra.split('\n'), nach
