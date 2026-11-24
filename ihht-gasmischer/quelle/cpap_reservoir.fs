FeatureScript 3112;
import(path : "onshape/std/geometry.fs", version : "3112.0");
// CPAP-Inline-Reservoir-Adapter (Einwegventil): erzeugt von build.py aus teile/cpap_reservoir.py (Bibliothek ihht/). Nicht von Hand aendern.

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

// ===================== CPAP-Reservoir: Innengewinde A, Aussengewinde B =====================
function crGewinde(context is Context, id is Id)
{
    const a = gewKoerper(context, id, vector(-15.2, 0, 64) * millimeter);
    makeThread(context, id + "giA", 12.7 * millimeter, 14.233 * millimeter, 12.3 * millimeter, 72.4 * millimeter, 2.5 * millimeter, 5.7);
    opBoolean(context, id + "giAx", { "tools" : qCreatedBy(id + "giA" + "sweep", EntityType.BODY), "targets" : a,
                "operationType" : BooleanOperationType.SUBTRACTION });
    finishPart(context, a, "CPAP-Reservoir A: Grundkörper (Quelle, Beutel 45°, Innengewinde)", color(0.3, 0.6, 0.85));

    const b = gewKoerper(context, id, vector(-15.2, 0, 87.45) * millimeter);
    makeThread(context, id + "gaB", 12.45 * millimeter, 13.983 * millimeter, 12.05 * millimeter, 74.9 * millimeter, 2.5 * millimeter, 4.82);
    fCylinder(context, id + "gaBtrim" + "big", { "bottomCenter" : vector(0, 0, 69.15) * millimeter,
                "topCenter" : vector(0, 0, 79.184) * millimeter, "radius" : 14.983 * millimeter });
    fCone(context, id + "gaBtrim" + "keep", { "bottomCenter" : vector(0, 0, 76.14) * millimeter,
                "topCenter" : vector(0, 0, 79.194) * millimeter, "bottomRadius" : 11.95 * millimeter, "topRadius" : 14.993 * millimeter });
    opBoolean(context, id + "gaBtrim" + "sub", { "tools" : qCreatedBy(id + "gaBtrim" + "keep", EntityType.BODY),
                "targets" : qCreatedBy(id + "gaBtrim" + "big", EntityType.BODY), "operationType" : BooleanOperationType.SUBTRACTION });
    opBoolean(context, id + "gaBu", { "tools" : qUnion([b, qCreatedBy(id + "gaB" + "sweep", EntityType.BODY)]),
                "operationType" : BooleanOperationType.UNION });
    opBoolean(context, id + "gaBx", { "tools" : qCreatedBy(id + "gaBtrim", EntityType.BODY), "targets" : b,
                "operationType" : BooleanOperationType.SUBTRACTION });
    finishPart(context, b, "CPAP-Reservoir B: Maskenkappe (Außengewinde, Ring andrücken)", color(0.85, 0.55, 0.3));
}

function cr_cpapResA(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "cpapResA_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(0, 0, 16.7) * millimeter, "topCenter" : vector(0, 0, 0) * millimeter, "radius" : 11.25 * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(0, 0, 13.2) * millimeter, "topCenter" : vector(0, 0, 12.4) * millimeter, "radius" : 12 * millimeter });
    coneOp(context, A0 + "s2", vector(0, 0, 12.4) * millimeter, vector(0, 0, 9.9) * millimeter, 12, 11.25);
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(0, 0, 9.9) * millimeter, "topCenter" : vector(0, 0, 9.1) * millimeter, "radius" : 12 * millimeter });
    coneOp(context, A0 + "s4", vector(0, 0, 9.1) * millimeter, vector(0, 0, 6.6) * millimeter, 12, 11.25);
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(0, 0, 6.6) * millimeter, "topCenter" : vector(0, 0, 5.8) * millimeter, "radius" : 12 * millimeter });
    coneOp(context, A0 + "s6", vector(0, 0, 5.8) * millimeter, vector(0, 0, 3.3) * millimeter, 12, 11.25);
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(0, 0, 3.3) * millimeter, "topCenter" : vector(0, 0, 2.5) * millimeter, "radius" : 12 * millimeter });
    coneOp(context, A0 + "s8", vector(0, 0, 2.5) * millimeter, vector(0, 0, 0) * millimeter, 12, 11.25);
    coneOp(context, A0 + "s9", vector(0, 0, 16.2) * millimeter, vector(0, 0, 20.2) * millimeter, 12, 16);
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(0, 0, 20.19) * millimeter, "topCenter" : vector(0, 0, 84.15) * millimeter, "radius" : 16 * millimeter });
    fCylinder(context, A0 + "s11", { "bottomCenter" : vector(0, 0, 54) * millimeter, "topCenter" : vector(26.87, 0, 27.13) * millimeter, "radius" : 16 * millimeter });
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(26.517, 0, 27.483) * millimeter, "topCenter" : vector(38.325, 0, 15.675) * millimeter, "radius" : 11.25 * millimeter });
    fCylinder(context, A0 + "s13", { "bottomCenter" : vector(28.991, 0, 25.009) * millimeter, "topCenter" : vector(29.557, 0, 24.443) * millimeter, "radius" : 12 * millimeter });
    coneOp(context, A0 + "s14", vector(29.557, 0, 24.443) * millimeter, vector(31.325, 0, 22.675) * millimeter, 12, 11.25);
    fCylinder(context, A0 + "s15", { "bottomCenter" : vector(31.325, 0, 22.675) * millimeter, "topCenter" : vector(31.891, 0, 22.109) * millimeter, "radius" : 12 * millimeter });
    coneOp(context, A0 + "s16", vector(31.891, 0, 22.109) * millimeter, vector(33.658, 0, 20.342) * millimeter, 12, 11.25);
    fCylinder(context, A0 + "s17", { "bottomCenter" : vector(33.658, 0, 20.342) * millimeter, "topCenter" : vector(34.224, 0, 19.776) * millimeter, "radius" : 12 * millimeter });
    coneOp(context, A0 + "s18", vector(34.224, 0, 19.776) * millimeter, vector(35.992, 0, 18.008) * millimeter, 12, 11.25);
    fCylinder(context, A0 + "s19", { "bottomCenter" : vector(35.992, 0, 18.008) * millimeter, "topCenter" : vector(36.557, 0, 17.443) * millimeter, "radius" : 12 * millimeter });
    coneOp(context, A0 + "s20", vector(36.557, 0, 17.443) * millimeter, vector(38.325, 0, 15.675) * millimeter, 12, 11.25);
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "cpapResA_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "cpapResA_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(0, 0, -1) * millimeter, "topCenter" : vector(0, 0, 60.01) * millimeter, "radius" : 10.2 * millimeter });
    coneOp(context, C1 + "o1", vector(0, 0, 60) * millimeter, vector(0, 0, 62.5) * millimeter, 10.2, 12.7);
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(0, 0, 62.49) * millimeter, "topCenter" : vector(0, 0, 85.15) * millimeter, "radius" : 12.7 * millimeter });
    coneOp(context, C1 + "o3", vector(0, 0, 82.267) * millimeter, vector(0, 0, 84.16) * millimeter, 12.7, 14.593);
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(0, 0, 54) * millimeter, "topCenter" : vector(39.032, 0, 14.968) * millimeter, "radius" : 10.2 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "cpapResA_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "CPAP-Reservoir A: Grundkörper (Quelle, Beutel 45°, Innengewinde)", color(0.3, 0.6, 0.85));
}

function cr_cpapResRing(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "cpapResRing_a0";
    coneOp(context, A0 + "s0", vector(0, 0, 60.15) * millimeter, vector(0, 0, 62.4) * millimeter, 10.2, 12.45);
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(0, 0, 62.39) * millimeter, "topCenter" : vector(0, 0, 76.15) * millimeter, "radius" : 12.45 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "cpapResRing_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "cpapResRing_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(0, 0, 59.15) * millimeter, "topCenter" : vector(0, 0, 63.16) * millimeter, "radius" : 9.5 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(0, 0, 63.15) * millimeter, "topCenter" : vector(0, 0, 77.15) * millimeter, "radius" : 11.25 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "cpapResRing_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Körper Stufe 2";
    const A2 = id + "cpapResRing_a2";
    prismOp(context, A2 + "s0", vector(0, -0.75, 63.15) * millimeter, [[-9.8, 0], [9.8, 0], [9.8, -3], [1.5, -3], [0, -1.5], [-1.5, -3], [-9.8, -3]], 1.5);
    prismXOp(context, A2 + "s1", vector(-0.75, 0, 63.15) * millimeter, [[-9.8, 0], [9.8, 0], [9.8, -3], [1.5, -3], [0, -1.5], [-1.5, -3], [-9.8, -3]], 1.5);
    fCylinder(context, A2 + "s2", { "bottomCenter" : vector(0, 0, 62.4) * millimeter, "topCenter" : vector(0, 0, 64.15) * millimeter, "radius" : 1.25 * millimeter });
    coneOp(context, A2 + "s3", vector(0, 0, 64.15) * millimeter, vector(0, 0, 67.614) * millimeter, 1.25, 3.25);
    fCylinder(context, A2 + "s4", { "bottomCenter" : vector(0, 0, 67.614) * millimeter, "topCenter" : vector(0, 0, 67.914) * millimeter, "radius" : 3.25 * millimeter });
    coneOp(context, A2 + "s5", vector(0, 0, 67.914) * millimeter, vector(0, 0, 69.414) * millimeter, 3.25, 0.5);
    prog[] = "Vereinigen Stufe 2";
    unionAll(context, id + "cpapResRing_u2", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]), "CPAP-Reservoir Ventilring (Y-Split-Sitz, Membran Ø 21,5)", color(0.95, 0.95, 0.95));
}

function cr_cpapResB(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "cpapResB_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(0, 0, 76.15) * millimeter, "topCenter" : vector(0, 0, 84.46) * millimeter, "radius" : 12.45 * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(0, 0, 84.45) * millimeter, "topCenter" : vector(0, 0, 90.45) * millimeter, "radius" : 16 * millimeter });
    coneOp(context, A0 + "s2", vector(0, 0, 90.44) * millimeter, vector(0, 0, 94.45) * millimeter, 16, 12);
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(0, 0, 93.95) * millimeter, "topCenter" : vector(0, 0, 110.65) * millimeter, "radius" : 11.25 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(0, 0, 97.45) * millimeter, "topCenter" : vector(0, 0, 98.25) * millimeter, "radius" : 12 * millimeter });
    coneOp(context, A0 + "s5", vector(0, 0, 98.25) * millimeter, vector(0, 0, 100.75) * millimeter, 12, 11.25);
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(0, 0, 100.75) * millimeter, "topCenter" : vector(0, 0, 101.55) * millimeter, "radius" : 12 * millimeter });
    coneOp(context, A0 + "s7", vector(0, 0, 101.55) * millimeter, vector(0, 0, 104.05) * millimeter, 12, 11.25);
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(0, 0, 104.05) * millimeter, "topCenter" : vector(0, 0, 104.85) * millimeter, "radius" : 12 * millimeter });
    coneOp(context, A0 + "s9", vector(0, 0, 104.85) * millimeter, vector(0, 0, 107.35) * millimeter, 12, 11.25);
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(0, 0, 107.35) * millimeter, "topCenter" : vector(0, 0, 108.15) * millimeter, "radius" : 12 * millimeter });
    coneOp(context, A0 + "s11", vector(0, 0, 108.15) * millimeter, vector(0, 0, 110.65) * millimeter, 12, 11.25);
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "cpapResB_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "cpapResB_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(0, 0, 75.15) * millimeter, "topCenter" : vector(0, 0, 111.65) * millimeter, "radius" : 10.2 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(16.034, 4.296, 83.45) * millimeter, "topCenter" : vector(16.034, 4.296, 90.45) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(11.738, 11.738, 83.45) * millimeter, "topCenter" : vector(11.738, 11.738, 90.45) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(4.296, 16.034, 83.45) * millimeter, "topCenter" : vector(4.296, 16.034, 90.45) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(-4.296, 16.034, 83.45) * millimeter, "topCenter" : vector(-4.296, 16.034, 90.45) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(-11.738, 11.738, 83.45) * millimeter, "topCenter" : vector(-11.738, 11.738, 90.45) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(-16.034, 4.296, 83.45) * millimeter, "topCenter" : vector(-16.034, 4.296, 90.45) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(-16.034, -4.296, 83.45) * millimeter, "topCenter" : vector(-16.034, -4.296, 90.45) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(-11.738, -11.738, 83.45) * millimeter, "topCenter" : vector(-11.738, -11.738, 90.45) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(-4.296, -16.034, 83.45) * millimeter, "topCenter" : vector(-4.296, -16.034, 90.45) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o10", { "bottomCenter" : vector(4.296, -16.034, 83.45) * millimeter, "topCenter" : vector(4.296, -16.034, 90.45) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o11", { "bottomCenter" : vector(11.738, -11.738, 83.45) * millimeter, "topCenter" : vector(11.738, -11.738, 90.45) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o12", { "bottomCenter" : vector(16.034, -4.296, 83.45) * millimeter, "topCenter" : vector(16.034, -4.296, 90.45) * millimeter, "radius" : 1.5 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "cpapResB_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "CPAP-Reservoir B: Maskenkappe (Außengewinde, Ring andrücken)", color(0.85, 0.55, 0.3));
}

annotation { "Feature Type Name" : "CPAP-Inline-Reservoir-Adapter (Einwegventil)" }
export const cpapReservoir = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
    }
    {
        const prog = new box("");
        var failed = [];
        var okCount = 0;
        try
        {
            cr_cpapResA(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "cpapResA @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            cr_cpapResRing(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "cpapResRing @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            cr_cpapResB(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "cpapResB @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            crGewinde(context, id);
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

