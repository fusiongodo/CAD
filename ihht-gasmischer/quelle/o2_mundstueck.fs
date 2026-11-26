FeatureScript 3112;
import(path : "onshape/std/geometry.fs", version : "3112.0");
// O2-Mundstück mit Reservoirbeutel: erzeugt von build.py aus teile/o2_mundstueck.py (Bibliothek ihht/). Nicht von Hand aendern.

// IHHT-Gasmischer – v5 Entwicklungs-Setup: 2 NEMA, Ventilpaare über Zahnräder gekoppelt, Kameraportal, Rahmen 250 mm (generiert). Änderungen bitte per Chat.
// Koordinaten: X = links/rechts, Y = vorne(-)/hinten(+), Z = oben. Y=0: Vorderseite LZQ-Montagewand, Z=0: Unterseite Boden.

function unionAll(context is Context, opId is Id, q is Query)
{
    const bodies = qBodyType(q, BodyType.SOLID);
    if (size(evaluateQuery(context, bodies)) > 1)
        opBoolean(context, opId, { "tools" : bodies, "operationType" : BooleanOperationType.UNION });
}

function cutAll(context is Context, opId is Id, targets is Query, tools is Query)
{
    if (size(evaluateQuery(context, tools)) > 0)
        opBoolean(context, opId, { "tools" : tools, "targets" : qBodyType(targets, BodyType.SOLID), "operationType" : BooleanOperationType.SUBTRACTION });
}

function coneOp(context is Context, id is Id, p0 is Vector, p1 is Vector, r0 is number, r1 is number)
{
    // Kegelstumpf: Profil skizzieren und um die Achse rotieren; Fallback: gestufte Zylinder
    const axisDir = normalize(p1 - p0);
    const len = norm(p1 - p0) / millimeter;
    var refDir = vector(0, 0, 1);
    if (abs(dot(axisDir, refDir)) > 0.9)
        refDir = vector(1, 0, 0);
    const xd = normalize(cross(axisDir, refDir));
    try
    {
        const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(p0, cross(axisDir, xd), xd) });
        skLineSegment(sk, "l0", { "start" : vector(0, 0) * millimeter, "end" : vector(r0, 0) * millimeter });
        skLineSegment(sk, "l1", { "start" : vector(r0, 0) * millimeter, "end" : vector(r1, -len) * millimeter });
        skLineSegment(sk, "l2", { "start" : vector(r1, -len) * millimeter, "end" : vector(0, -len) * millimeter });
        skLineSegment(sk, "l3", { "start" : vector(0, -len) * millimeter, "end" : vector(0, 0) * millimeter });
        skSolve(sk);
        opRevolve(context, id + "rev", { "entities" : qSketchRegion(id + "sk"), "axis" : line(p0, axisDir), "angleForward" : 2 * PI * radian });
        opDeleteBodies(context, id + "del", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
    }
    catch (e)
    {
        try silent
        {
            opDeleteBodies(context, id + "del2", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
        }
        for (var k = 0; k < 8; k += 1)
        {
            fCylinder(context, id + ("fb" ~ toString(k)), { "bottomCenter" : p0 + (p1 - p0) * (k / 8), "topCenter" : p0 + (p1 - p0) * ((k + 1) / 8), "radius" : (r0 + (r1 - r0) * (k + 0.5) / 8) * millimeter });
        }
    }
}

function finishPart(context is Context, q is Query, label is string, c is Color)
{
    const b = qBodyType(q, BodyType.SOLID);
    setProperty(context, { "entities" : b, "propertyType" : PropertyType.NAME, "value" : label });
    setProperty(context, { "entities" : b, "propertyType" : PropertyType.APPEARANCE, "value" : c });
}

function tearOp(context is Context, id is Id, p0 is Vector, p1 is Vector, r is number)
{
    // Waagerechter Kanal mit vollem Tropfenprofil (45°-Flanken bis zur Spitze, kein Dach) -> druckt ohne Support
    fCylinder(context, id + "cyl", { "bottomCenter" : p0, "topCenter" : p1, "radius" : r * millimeter });
    const n = normalize(p1 - p0);
    const len = norm(p1 - p0);
    const xd = normalize(cross(vector(0, 0, 1), n));
    const s2 = r / sqrt(2);
    const w = r * (sqrt(2) - 1);
    try
    {
        const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(p0, n, xd) });
        skLineSegment(sk, "a", { "start" : vector(0, 0) * millimeter, "end" : vector(-s2, s2) * millimeter });
        skLineSegment(sk, "b", { "start" : vector(-s2, s2) * millimeter, "end" : vector(0, r * sqrt(2)) * millimeter });
        skLineSegment(sk, "d", { "start" : vector(0, r * sqrt(2)) * millimeter, "end" : vector(s2, s2) * millimeter });
        skLineSegment(sk, "e", { "start" : vector(s2, s2) * millimeter, "end" : vector(0, 0) * millimeter });
        skSolve(sk);
        opExtrude(context, id + "ext", { "entities" : qSketchRegion(id + "sk"), "direction" : n, "endBound" : BoundingType.BLIND, "endDepth" : len });
        opDeleteBodies(context, id + "del", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
    }
    catch (e)
    {
        // Fallback: nur runder Kanal
    }
}

function prismOp(context is Context, id is Id, origin is Vector, pts is array, depth is number)
{
    // Polygon [dx, dz] in der XZ-Ebene durch origin, Extrusion um depth in +Y (Zahnradprofil)
    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(origin, vector(0, 1, 0), vector(1, 0, 0)) });   // Skizzen-y zeigt nach -Z
    const n = size(pts);
    for (var i = 0; i < n; i += 1)
    {
        const a = pts[i];
        const b = pts[(i + 1) % n];
        skLineSegment(sk, "l" ~ i, { "start" : vector(a[0], -a[1]) * millimeter, "end" : vector(b[0], -b[1]) * millimeter });
    }
    skSolve(sk);
    opExtrude(context, id + "ext", { "entities" : qSketchRegion(id + "sk"), "direction" : vector(0, 1, 0), "endBound" : BoundingType.BLIND, "endDepth" : depth * millimeter });
    opDeleteBodies(context, id + "del", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
}

function prismXOp(context is Context, id is Id, origin is Vector, pts is array, depth is number)
{
    // Polygon [dy, dz] in der YZ-Ebene durch origin, Extrusion um depth in +X (Knotenbleche)
    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(origin, vector(1, 0, 0), vector(0, 1, 0)) });   // Skizzen-y zeigt nach +Z
    const n = size(pts);
    for (var i = 0; i < n; i += 1)
    {
        const a = pts[i];
        const b = pts[(i + 1) % n];
        skLineSegment(sk, "l" ~ i, { "start" : vector(a[0], a[1]) * millimeter, "end" : vector(b[0], b[1]) * millimeter });
    }
    skSolve(sk);
    opExtrude(context, id + "ext", { "entities" : qSketchRegion(id + "sk"), "direction" : vector(1, 0, 0), "endBound" : BoundingType.BLIND, "endDepth" : depth * millimeter });
    opDeleteBodies(context, id + "del", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
}


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

// ===================== O2-Mundstueck: Innengewinde A, Aussengewinde B =====================
function o2mGewinde(context is Context, id is Id)
{
    const a = gewKoerper(context, id, vector(-15.2, 0, 44) * millimeter);
    makeThread(context, id + "giA", 12.7 * millimeter, 14.233 * millimeter, 12.3 * millimeter, 46.4 * millimeter, 2.5 * millimeter, 5.7);
    opBoolean(context, id + "giAx", { "tools" : qCreatedBy(id + "giA" + "sweep", EntityType.BODY), "targets" : a,
                "operationType" : BooleanOperationType.SUBTRACTION });
    finishPart(context, a, "O2-Mundstück A: Grundkörper (Schlauchtülle, Beutelring 45°, Innengewinde)", color(0.3, 0.6, 0.85));

    const b = gewKoerper(context, id, vector(-15.2, 0, 62.45) * millimeter);
    makeThread(context, id + "gaB", 12.45 * millimeter, 13.983 * millimeter, 12.05 * millimeter, 48.9 * millimeter, 2.5 * millimeter, 4.82);
    fCylinder(context, id + "gaBtrim" + "big", { "bottomCenter" : vector(0, 0, 43.15) * millimeter,
                "topCenter" : vector(0, 0, 53.183) * millimeter, "radius" : 14.983 * millimeter });
    fCone(context, id + "gaBtrim" + "keep", { "bottomCenter" : vector(0, 0, 50.14) * millimeter,
                "topCenter" : vector(0, 0, 53.193) * millimeter, "bottomRadius" : 11.95 * millimeter, "topRadius" : 14.993 * millimeter });
    opBoolean(context, id + "gaBtrim" + "sub", { "tools" : qCreatedBy(id + "gaBtrim" + "keep", EntityType.BODY),
                "targets" : qCreatedBy(id + "gaBtrim" + "big", EntityType.BODY), "operationType" : BooleanOperationType.SUBTRACTION });
    opBoolean(context, id + "gaBu", { "tools" : qUnion([b, qCreatedBy(id + "gaB" + "sweep", EntityType.BODY)]),
                "operationType" : BooleanOperationType.UNION });
    opBoolean(context, id + "gaBx", { "tools" : qCreatedBy(id + "gaBtrim", EntityType.BODY), "targets" : b,
                "operationType" : BooleanOperationType.SUBTRACTION });
    finishPart(context, b, "O2-Mundstück B: Mundrohr (Außengewinde, drückt den Ring an)", color(0.85, 0.55, 0.3));
}

function o2m_o2mA(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "o2mA_a0";
    coneOp(context, A0 + "s0", vector(0, 0, 0) * millimeter, vector(0, 0, 12) * millimeter, 2.5, 3.5);
    coneOp(context, A0 + "s1", vector(0, 0, 11.99) * millimeter, vector(0, 0, 24.5) * millimeter, 3.5, 16);
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(0, 0, 24.49) * millimeter, "topCenter" : vector(0, 0, 58.15) * millimeter, "radius" : 16 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(0, 0, 34) * millimeter, "topCenter" : vector(29.698, 0, 4.302) * millimeter, "radius" : 10 * millimeter });
    coneOp(context, A0 + "s4", vector(26.87, 0, 7.13) * millimeter, vector(27.436, 0, 6.564) * millimeter, 10, 10.8);
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(27.429, 0, 6.571) * millimeter, "topCenter" : vector(28.567, 0, 5.433) * millimeter, "radius" : 10.8 * millimeter });
    coneOp(context, A0 + "s6", vector(28.56, 0, 5.44) * millimeter, vector(29.133, 0, 4.867) * millimeter, 10.8, 10);
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "o2mA_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "o2mA_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(0, 0, -1) * millimeter, "topCenter" : vector(0, 0, 16.81) * millimeter, "radius" : 1.5 * millimeter });
    coneOp(context, C1 + "o1", vector(0, 0, 16.8) * millimeter, vector(0, 0, 25.5) * millimeter, 1.5, 10.2);
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(0, 0, 25.49) * millimeter, "topCenter" : vector(0, 0, 40.01) * millimeter, "radius" : 10.2 * millimeter });
    coneOp(context, C1 + "o3", vector(0, 0, 40) * millimeter, vector(0, 0, 42.5) * millimeter, 10.2, 12.7);
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(0, 0, 42.49) * millimeter, "topCenter" : vector(0, 0, 59.15) * millimeter, "radius" : 12.7 * millimeter });
    coneOp(context, C1 + "o5", vector(0, 0, 56.267) * millimeter, vector(0, 0, 58.16) * millimeter, 12.7, 14.593);
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(0, 0, 34) * millimeter, "topCenter" : vector(30.406, 0, 3.594) * millimeter, "radius" : 8 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "o2mA_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "O2-Mundstück A: Grundkörper (Schlauchtülle, Beutelring 45°, Innengewinde)", color(0.3, 0.6, 0.85));
}

function o2m_o2mRing(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "o2mRing_a0";
    coneOp(context, A0 + "s0", vector(0, 0, 40.15) * millimeter, vector(0, 0, 42.4) * millimeter, 10.2, 12.45);
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(0, 0, 42.39) * millimeter, "topCenter" : vector(0, 0, 50.15) * millimeter, "radius" : 12.45 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "o2mRing_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "o2mRing_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(0, 0, 39.15) * millimeter, "topCenter" : vector(0, 0, 43.16) * millimeter, "radius" : 9.5 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(0, 0, 43.15) * millimeter, "topCenter" : vector(0, 0, 51.15) * millimeter, "radius" : 11.25 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "o2mRing_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Körper Stufe 2";
    const A2 = id + "o2mRing_a2";
    prismOp(context, A2 + "s0", vector(0, -0.75, 43.15) * millimeter, [[-9.8, 0], [9.8, 0], [9.8, -3], [1.5, -3], [0, -1.5], [-1.5, -3], [-9.8, -3]], 1.5);
    prismXOp(context, A2 + "s1", vector(-0.75, 0, 43.15) * millimeter, [[-9.8, 0], [9.8, 0], [9.8, -3], [1.5, -3], [0, -1.5], [-1.5, -3], [-9.8, -3]], 1.5);
    fCylinder(context, A2 + "s2", { "bottomCenter" : vector(0, 0, 42.4) * millimeter, "topCenter" : vector(0, 0, 44.15) * millimeter, "radius" : 1.25 * millimeter });
    coneOp(context, A2 + "s3", vector(0, 0, 44.15) * millimeter, vector(0, 0, 47.614) * millimeter, 1.25, 3.25);
    fCylinder(context, A2 + "s4", { "bottomCenter" : vector(0, 0, 47.614) * millimeter, "topCenter" : vector(0, 0, 47.914) * millimeter, "radius" : 3.25 * millimeter });
    coneOp(context, A2 + "s5", vector(0, 0, 47.914) * millimeter, vector(0, 0, 49.414) * millimeter, 3.25, 0.5);
    prog[] = "Vereinigen Stufe 2";
    unionAll(context, id + "o2mRing_u2", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]), "O2-Mundstück Ventilring (Sitz Ø 19, Membran Ø 21,5)", color(0.95, 0.95, 0.95));
}

function o2m_o2mB(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "o2mB_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(0, 0, 50.15) * millimeter, "topCenter" : vector(0, 0, 58.46) * millimeter, "radius" : 12.45 * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(0, 0, 58.45) * millimeter, "topCenter" : vector(0, 0, 63.45) * millimeter, "radius" : 16 * millimeter });
    coneOp(context, A0 + "s2", vector(0, 0, 63.44) * millimeter, vector(0, 0, 71.95) * millimeter, 16, 7.5);
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(0, 0, 71.94) * millimeter, "topCenter" : vector(0, 0, 85.95) * millimeter, "radius" : 7.5 * millimeter });
    coneOp(context, A0 + "s4", vector(0, 0, 82.35) * millimeter, vector(0, 0, 83.35) * millimeter, 7.5, 8.5);
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(0, 0, 83.34) * millimeter, "topCenter" : vector(0, 0, 84.95) * millimeter, "radius" : 8.5 * millimeter });
    coneOp(context, A0 + "s6", vector(0, 0, 84.94) * millimeter, vector(0, 0, 85.95) * millimeter, 8.5, 7.5);
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "o2mB_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "o2mB_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(0, 0, 49.15) * millimeter, "topCenter" : vector(0, 0, 63.96) * millimeter, "radius" : 10.2 * millimeter });
    coneOp(context, C1 + "o1", vector(0, 0, 63.95) * millimeter, vector(0, 0, 68.65) * millimeter, 10.2, 5.5);
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(0, 0, 68.64) * millimeter, "topCenter" : vector(0, 0, 86.95) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(16.034, 4.296, 57.45) * millimeter, "topCenter" : vector(16.034, 4.296, 63.45) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(11.738, 11.738, 57.45) * millimeter, "topCenter" : vector(11.738, 11.738, 63.45) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(4.296, 16.034, 57.45) * millimeter, "topCenter" : vector(4.296, 16.034, 63.45) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(-4.296, 16.034, 57.45) * millimeter, "topCenter" : vector(-4.296, 16.034, 63.45) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(-11.738, 11.738, 57.45) * millimeter, "topCenter" : vector(-11.738, 11.738, 63.45) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(-16.034, 4.296, 57.45) * millimeter, "topCenter" : vector(-16.034, 4.296, 63.45) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(-16.034, -4.296, 57.45) * millimeter, "topCenter" : vector(-16.034, -4.296, 63.45) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o10", { "bottomCenter" : vector(-11.738, -11.738, 57.45) * millimeter, "topCenter" : vector(-11.738, -11.738, 63.45) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o11", { "bottomCenter" : vector(-4.296, -16.034, 57.45) * millimeter, "topCenter" : vector(-4.296, -16.034, 63.45) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o12", { "bottomCenter" : vector(4.296, -16.034, 57.45) * millimeter, "topCenter" : vector(4.296, -16.034, 63.45) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o13", { "bottomCenter" : vector(11.738, -11.738, 57.45) * millimeter, "topCenter" : vector(11.738, -11.738, 63.45) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o14", { "bottomCenter" : vector(16.034, -4.296, 57.45) * millimeter, "topCenter" : vector(16.034, -4.296, 63.45) * millimeter, "radius" : 1.5 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "o2mB_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "O2-Mundstück B: Mundrohr (Außengewinde, drückt den Ring an)", color(0.85, 0.55, 0.3));
}

annotation { "Feature Type Name" : "O2-Mundstück mit Reservoirbeutel" }
export const o2Mundstueck = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
    }
    {
        const prog = new box("");
        var failed = [];
        var okCount = 0;
        try
        {
            o2m_o2mA(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "o2mA @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            o2m_o2mRing(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "o2mRing @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            o2m_o2mB(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "o2mB @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            o2mGewinde(context, id);
        }
        catch (error)
        {
            failed = append(failed, "Gewinde A/B -> " ~ toString(error));
        }
        if (size(failed) > 0)
        {
            var msg = "OK: " ~ toString(okCount) ~ " von 3 Teilen. Fehler: ";
            for (var s in failed)
                msg = msg ~ s ~ " || ";
            reportFeatureWarning(context, id, msg);
        }
    });

