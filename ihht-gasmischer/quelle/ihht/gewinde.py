# Gewinde fuer FeatureScript (ueber fs_zusatz eines Teils): 60°-Trapezzahn entlang einer Helix um die Welt-Z-Achse
# (x = y = 0), immer Rechtsgewinde. Herkunft: Sammler Mix Acryl (teile/sammler_mix_acryl.py, dort lokal transformiert).
#   makeThread(context, id, rRoot, rCrest, overlapR, zStart, pitch, turns)
#     Aussengewinde: rRoot = Kern, rCrest = Kamm, overlapR < rRoot (Ueberlappung in den Kern) -> mit dem Teil vereinigen
#     Innengewinde:  rRoot = Bohrung (Kern + Spiel), rCrest = Gewinde aussen + Spiel, overlapR < rRoot -> vom Teil abziehen
#   Gleiche Phase (zStart modulo pitch) bei Aussen- und Innengewinde -> passt in Einbaulage kollisionsfrei.
GEWINDE_FS = r'''
// ===================== Gewinde (Helix um Z, Rechtsgewinde) =====================
function helixIsRightHand(context is Context, edge is Query, turns is number) returns boolean
{
    const p0 = evEdgeTangentLine(context, { "edge" : edge, "parameter" : 0 }).origin;
    const p1 = evEdgeTangentLine(context, { "edge" : edge, "parameter" : 0.25 / turns }).origin;
    const crossZ = (p0[0] * p1[1] - p0[1] * p1[0]) / (millimeter * millimeter);
    const dz = (p1[2] - p0[2]) / millimeter;
    return crossZ * dz > 0;
}

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
    opDeleteBodies(context, id + "clean", { "entities" : qUnion([qCreatedBy(helixId, EntityType.BODY),
                    qCreatedBy(id + "profile", EntityType.BODY)]) });
}

function gewKoerper(context is Context, id is Id, p is Vector) returns Query
{
    const q = qContainsPoint(qBodyType(qCreatedBy(id, EntityType.BODY), BodyType.SOLID), p);
    if (size(evaluateQuery(context, q)) != 1)
        throw regenError("Koerper fuer Gewinde nicht gefunden");
    return q;
}
'''
