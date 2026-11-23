FeatureScript 3112;
import(path : "onshape/std/geometry.fs", version : "3112.0");
// Splitter lo O2: erzeugt von gen_splitter_lo_o2.py (Basis gen6.py, Anschluesse aus anschluesse.py). Lage wie Splitter lo v4.

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

function lo2_splLoO2(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "splLoO2_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(14.65, 47, 17) * millimeter, "corner2" : vector(52.65, 59, 54.7) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(4.65, 39, 17) * millimeter, "corner2" : vector(62.65, 67, 20) * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(22.65, 47.5, 35) * millimeter, "topCenter" : vector(22.65, 35, 35) * millimeter, "radius" : 3.5 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(22.65, 45, 35) * millimeter, "topCenter" : vector(22.65, 44.5, 35) * millimeter, "radius" : 3.75 * millimeter });
    coneOp(context, A0 + "s4", vector(22.65, 44.5, 35) * millimeter, vector(22.65, 42.5, 35) * millimeter, 3.75, 3.5);
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(22.65, 42.5, 35) * millimeter, "topCenter" : vector(22.65, 42, 35) * millimeter, "radius" : 3.75 * millimeter });
    coneOp(context, A0 + "s6", vector(22.65, 42, 35) * millimeter, vector(22.65, 40, 35) * millimeter, 3.75, 3.5);
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(22.65, 40, 35) * millimeter, "topCenter" : vector(22.65, 39.5, 35) * millimeter, "radius" : 3.75 * millimeter });
    coneOp(context, A0 + "s8", vector(22.65, 39.5, 35) * millimeter, vector(22.65, 37.5, 35) * millimeter, 3.75, 3.5);
    fCylinder(context, A0 + "s9", { "bottomCenter" : vector(22.65, 37.5, 35) * millimeter, "topCenter" : vector(22.65, 37, 35) * millimeter, "radius" : 3.75 * millimeter });
    coneOp(context, A0 + "s10", vector(22.65, 37, 35) * millimeter, vector(22.65, 35, 35) * millimeter, 3.75, 3.5);
    fCylinder(context, A0 + "s11", { "bottomCenter" : vector(44.65, 47.5, 35) * millimeter, "topCenter" : vector(44.65, 35, 35) * millimeter, "radius" : 3.5 * millimeter });
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(44.65, 45, 35) * millimeter, "topCenter" : vector(44.65, 44.5, 35) * millimeter, "radius" : 3.75 * millimeter });
    coneOp(context, A0 + "s13", vector(44.65, 44.5, 35) * millimeter, vector(44.65, 42.5, 35) * millimeter, 3.75, 3.5);
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(44.65, 42.5, 35) * millimeter, "topCenter" : vector(44.65, 42, 35) * millimeter, "radius" : 3.75 * millimeter });
    coneOp(context, A0 + "s15", vector(44.65, 42, 35) * millimeter, vector(44.65, 40, 35) * millimeter, 3.75, 3.5);
    fCylinder(context, A0 + "s16", { "bottomCenter" : vector(44.65, 40, 35) * millimeter, "topCenter" : vector(44.65, 39.5, 35) * millimeter, "radius" : 3.75 * millimeter });
    coneOp(context, A0 + "s17", vector(44.65, 39.5, 35) * millimeter, vector(44.65, 37.5, 35) * millimeter, 3.75, 3.5);
    fCylinder(context, A0 + "s18", { "bottomCenter" : vector(44.65, 37.5, 35) * millimeter, "topCenter" : vector(44.65, 37, 35) * millimeter, "radius" : 3.75 * millimeter });
    coneOp(context, A0 + "s19", vector(44.65, 37, 35) * millimeter, vector(44.65, 35, 35) * millimeter, 3.75, 3.5);
    fCylinder(context, A0 + "s20", { "bottomCenter" : vector(33.65, 58.5, 40) * millimeter, "topCenter" : vector(33.65, 75.2, 40) * millimeter, "radius" : 11.4 * millimeter });
    fCylinder(context, A0 + "s21", { "bottomCenter" : vector(33.65, 62, 40) * millimeter, "topCenter" : vector(33.65, 62.8, 40) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s22", vector(33.65, 62.8, 40) * millimeter, vector(33.65, 65.3, 40) * millimeter, 12.2, 11.4);
    fCylinder(context, A0 + "s23", { "bottomCenter" : vector(33.65, 65.3, 40) * millimeter, "topCenter" : vector(33.65, 66.1, 40) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s24", vector(33.65, 66.1, 40) * millimeter, vector(33.65, 68.6, 40) * millimeter, 12.2, 11.4);
    fCylinder(context, A0 + "s25", { "bottomCenter" : vector(33.65, 68.6, 40) * millimeter, "topCenter" : vector(33.65, 69.4, 40) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s26", vector(33.65, 69.4, 40) * millimeter, vector(33.65, 71.9, 40) * millimeter, 12.2, 11.4);
    fCylinder(context, A0 + "s27", { "bottomCenter" : vector(33.65, 71.9, 40) * millimeter, "topCenter" : vector(33.65, 72.7, 40) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s28", vector(33.65, 72.7, 40) * millimeter, vector(33.65, 75.2, 40) * millimeter, 12.2, 11.4);
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "splLoO2_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "splLoO2_c1";
    tearOp(context, C1 + "o0", vector(19.15, 53, 35) * millimeter, vector(48.15, 53, 35) * millimeter, 4);
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(22.65, 34, 35) * millimeter, "topCenter" : vector(22.65, 53, 35) * millimeter, "radius" : 2.6 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(44.65, 34, 35) * millimeter, "topCenter" : vector(44.65, 53, 35) * millimeter, "radius" : 2.6 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(33.65, 51, 40) * millimeter, "topCenter" : vector(33.65, 76.2, 40) * millimeter, "radius" : 10.2 * millimeter });
    fCuboid(context, C1 + "o4", { "corner1" : vector(7.95, 43, 16) * millimeter, "corner2" : vector(11.35, 63, 21) * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(9.65, 43, 16) * millimeter, "topCenter" : vector(9.65, 43, 21) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(9.65, 63, 16) * millimeter, "topCenter" : vector(9.65, 63, 21) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o7", { "corner1" : vector(55.95, 43, 16) * millimeter, "corner2" : vector(59.35, 63, 21) * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(57.65, 43, 16) * millimeter, "topCenter" : vector(57.65, 43, 21) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(57.65, 63, 16) * millimeter, "topCenter" : vector(57.65, 63, 21) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "splLoO2_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Splitter lo O2 (innen 2 x dünne O2-Tülle wie Eingang hi, außen CPAP)", color(0.35, 0.55, 0.85));
}

annotation { "Feature Type Name" : "Splitter lo O2-Tüllen" }
export const splitterLoO2 = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
    }
    {
        const prog = new box("");
        var failed = [];
        var okCount = 0;
        try
        {
            lo2_splLoO2(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "splLoO2 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        if (size(failed) > 0)
        {
            var msg = "OK: " ~ toString(okCount) ~ " von 1 Teilen. Fehler: ";
            for (var s in failed)
                msg = msg ~ s ~ " || ";
            reportFeatureWarning(context, id, msg);
        }
    });

