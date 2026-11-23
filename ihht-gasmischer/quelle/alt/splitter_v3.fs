FeatureScript 3112;
import(path : "onshape/std/geometry.fs", version : "3112.0");

// IHHT-Gasmischer – v2 Entwicklungs-Setup + Ablage (generiert). Änderungen bitte per Chat.
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

function spl3_splHi(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "splHi_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-75.95, 60, 7) * millimeter, "corner2" : vector(-14.65, 72, 33.5) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-85.95, 52, 7) * millimeter, "corner2" : vector(-4.65, 80, 10) * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-67.95, 60.5, 25) * millimeter, "topCenter" : vector(-67.95, 48, 25) * millimeter, "radius" : 5.375 * millimeter });
    coneOp(context, A0 + "s3", vector(-67.95, 48.3, 25) * millimeter, vector(-67.95, 50.1, 25) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(-67.95, 50.1, 25) * millimeter, "topCenter" : vector(-67.95, 50.4, 25) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s5", vector(-67.95, 50.7, 25) * millimeter, vector(-67.95, 52.5, 25) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(-67.95, 52.5, 25) * millimeter, "topCenter" : vector(-67.95, 52.8, 25) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s7", vector(-67.95, 53.1, 25) * millimeter, vector(-67.95, 54.9, 25) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(-67.95, 54.9, 25) * millimeter, "topCenter" : vector(-67.95, 55.2, 25) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s9", vector(-67.95, 60.8, 25) * millimeter, vector(-67.95, 58.3, 25) * millimeter, 7.375, 5.375);
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(-22.65, 60.5, 25) * millimeter, "topCenter" : vector(-22.65, 48, 25) * millimeter, "radius" : 5.375 * millimeter });
    coneOp(context, A0 + "s11", vector(-22.65, 48.3, 25) * millimeter, vector(-22.65, 50.1, 25) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(-22.65, 50.1, 25) * millimeter, "topCenter" : vector(-22.65, 50.4, 25) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s13", vector(-22.65, 50.7, 25) * millimeter, vector(-22.65, 52.5, 25) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(-22.65, 52.5, 25) * millimeter, "topCenter" : vector(-22.65, 52.8, 25) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s15", vector(-22.65, 53.1, 25) * millimeter, vector(-22.65, 54.9, 25) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s16", { "bottomCenter" : vector(-22.65, 54.9, 25) * millimeter, "topCenter" : vector(-22.65, 55.2, 25) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s17", vector(-22.65, 60.8, 25) * millimeter, vector(-22.65, 58.3, 25) * millimeter, 7.375, 5.375);
    fCylinder(context, A0 + "s18", { "bottomCenter" : vector(-45.3, 71.5, 25) * millimeter, "topCenter" : vector(-45.3, 84, 25) * millimeter, "radius" : 3.5 * millimeter });
    fCylinder(context, A0 + "s19", { "bottomCenter" : vector(-45.3, 74, 25) * millimeter, "topCenter" : vector(-45.3, 74.5, 25) * millimeter, "radius" : 3.75 * millimeter });
    coneOp(context, A0 + "s20", vector(-45.3, 74.5, 25) * millimeter, vector(-45.3, 76.5, 25) * millimeter, 3.75, 3.5);
    fCylinder(context, A0 + "s21", { "bottomCenter" : vector(-45.3, 76.5, 25) * millimeter, "topCenter" : vector(-45.3, 77, 25) * millimeter, "radius" : 3.75 * millimeter });
    coneOp(context, A0 + "s22", vector(-45.3, 77, 25) * millimeter, vector(-45.3, 79, 25) * millimeter, 3.75, 3.5);
    fCylinder(context, A0 + "s23", { "bottomCenter" : vector(-45.3, 79, 25) * millimeter, "topCenter" : vector(-45.3, 79.5, 25) * millimeter, "radius" : 3.75 * millimeter });
    coneOp(context, A0 + "s24", vector(-45.3, 79.5, 25) * millimeter, vector(-45.3, 81.5, 25) * millimeter, 3.75, 3.5);
    fCylinder(context, A0 + "s25", { "bottomCenter" : vector(-45.3, 81.5, 25) * millimeter, "topCenter" : vector(-45.3, 82, 25) * millimeter, "radius" : 3.75 * millimeter });
    coneOp(context, A0 + "s26", vector(-45.3, 82, 25) * millimeter, vector(-45.3, 84, 25) * millimeter, 3.75, 3.5);
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "splHi_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "splHi_c1";
    tearOp(context, C1 + "o0", vector(-71.45, 66, 25) * millimeter, vector(-19.15, 66, 25) * millimeter, 4);
    tearOp(context, C1 + "o1", vector(-67.95, 47, 25) * millimeter, vector(-67.95, 66, 25) * millimeter, 3.25);
    tearOp(context, C1 + "o2", vector(-22.65, 47, 25) * millimeter, vector(-22.65, 66, 25) * millimeter, 3.25);
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(-45.3, 65.5, 25) * millimeter, "topCenter" : vector(-45.3, 85, 25) * millimeter, "radius" : 2.6 * millimeter });
    fCuboid(context, C1 + "o4", { "corner1" : vector(-82.65, 56, 6) * millimeter, "corner2" : vector(-79.25, 76, 11) * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(-80.95, 56, 6) * millimeter, "topCenter" : vector(-80.95, 56, 11) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(-80.95, 76, 6) * millimeter, "topCenter" : vector(-80.95, 76, 11) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o7", { "corner1" : vector(-11.35, 56, 6) * millimeter, "corner2" : vector(-7.95, 76, 11) * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(-9.65, 56, 6) * millimeter, "topCenter" : vector(-9.65, 56, 11) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(-9.65, 76, 6) * millimeter, "topCenter" : vector(-9.65, 76, 11) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "splHi_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Splitter hi (Eingang hyperoxisch, links) v3", color(0.85, 0.35, 0.3));
}

function spl3_splLo(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "splLo_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(14.65, 60, 7) * millimeter, "corner2" : vector(75.95, 72, 44.7) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(4.65, 52, 7) * millimeter, "corner2" : vector(85.95, 80, 10) * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(22.65, 60.5, 25) * millimeter, "topCenter" : vector(22.65, 48, 25) * millimeter, "radius" : 5.375 * millimeter });
    coneOp(context, A0 + "s3", vector(22.65, 48.3, 25) * millimeter, vector(22.65, 50.1, 25) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(22.65, 50.1, 25) * millimeter, "topCenter" : vector(22.65, 50.4, 25) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s5", vector(22.65, 50.7, 25) * millimeter, vector(22.65, 52.5, 25) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(22.65, 52.5, 25) * millimeter, "topCenter" : vector(22.65, 52.8, 25) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s7", vector(22.65, 53.1, 25) * millimeter, vector(22.65, 54.9, 25) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(22.65, 54.9, 25) * millimeter, "topCenter" : vector(22.65, 55.2, 25) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s9", vector(22.65, 60.8, 25) * millimeter, vector(22.65, 58.3, 25) * millimeter, 7.375, 5.375);
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(67.95, 60.5, 25) * millimeter, "topCenter" : vector(67.95, 48, 25) * millimeter, "radius" : 5.375 * millimeter });
    coneOp(context, A0 + "s11", vector(67.95, 48.3, 25) * millimeter, vector(67.95, 50.1, 25) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(67.95, 50.1, 25) * millimeter, "topCenter" : vector(67.95, 50.4, 25) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s13", vector(67.95, 50.7, 25) * millimeter, vector(67.95, 52.5, 25) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(67.95, 52.5, 25) * millimeter, "topCenter" : vector(67.95, 52.8, 25) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s15", vector(67.95, 53.1, 25) * millimeter, vector(67.95, 54.9, 25) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s16", { "bottomCenter" : vector(67.95, 54.9, 25) * millimeter, "topCenter" : vector(67.95, 55.2, 25) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s17", vector(67.95, 60.8, 25) * millimeter, vector(67.95, 58.3, 25) * millimeter, 7.375, 5.375);
    fCylinder(context, A0 + "s18", { "bottomCenter" : vector(45.3, 71.5, 30) * millimeter, "topCenter" : vector(45.3, 88.2, 30) * millimeter, "radius" : 11.4 * millimeter });
    fCylinder(context, A0 + "s19", { "bottomCenter" : vector(45.3, 75, 30) * millimeter, "topCenter" : vector(45.3, 75.8, 30) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s20", vector(45.3, 75.8, 30) * millimeter, vector(45.3, 78.3, 30) * millimeter, 12.2, 11.4);
    fCylinder(context, A0 + "s21", { "bottomCenter" : vector(45.3, 78.3, 30) * millimeter, "topCenter" : vector(45.3, 79.1, 30) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s22", vector(45.3, 79.1, 30) * millimeter, vector(45.3, 81.6, 30) * millimeter, 12.2, 11.4);
    fCylinder(context, A0 + "s23", { "bottomCenter" : vector(45.3, 81.6, 30) * millimeter, "topCenter" : vector(45.3, 82.4, 30) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s24", vector(45.3, 82.4, 30) * millimeter, vector(45.3, 84.9, 30) * millimeter, 12.2, 11.4);
    fCylinder(context, A0 + "s25", { "bottomCenter" : vector(45.3, 84.9, 30) * millimeter, "topCenter" : vector(45.3, 85.7, 30) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s26", vector(45.3, 85.7, 30) * millimeter, vector(45.3, 88.2, 30) * millimeter, 12.2, 11.4);
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "splLo_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "splLo_c1";
    tearOp(context, C1 + "o0", vector(19.15, 66, 25) * millimeter, vector(71.45, 66, 25) * millimeter, 4);
    tearOp(context, C1 + "o1", vector(22.65, 47, 25) * millimeter, vector(22.65, 66, 25) * millimeter, 3.25);
    tearOp(context, C1 + "o2", vector(67.95, 47, 25) * millimeter, vector(67.95, 66, 25) * millimeter, 3.25);
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(45.3, 64, 30) * millimeter, "topCenter" : vector(45.3, 89.2, 30) * millimeter, "radius" : 10.2 * millimeter });
    fCuboid(context, C1 + "o4", { "corner1" : vector(7.95, 56, 6) * millimeter, "corner2" : vector(11.35, 76, 11) * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(9.65, 56, 6) * millimeter, "topCenter" : vector(9.65, 56, 11) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(9.65, 76, 6) * millimeter, "topCenter" : vector(9.65, 76, 11) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o7", { "corner1" : vector(79.25, 56, 6) * millimeter, "corner2" : vector(82.65, 76, 11) * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(80.95, 56, 6) * millimeter, "topCenter" : vector(80.95, 56, 11) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(80.95, 76, 6) * millimeter, "topCenter" : vector(80.95, 76, 11) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "splLo_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Splitter lo (Eingang hypoxisch, rechts) v3", color(0.35, 0.55, 0.85));
}

annotation { "Feature Type Name" : "IHHT Splitter v3" }
export const ihhtSplitterV3 = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
    }
    {
        const prog = new box("");
        var failed = [];
        var okCount = 0;
        try
        {
            spl3_splHi(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "splHi @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            spl3_splLo(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "splLo @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        if (size(failed) > 0)
        {
            var msg = "OK: " ~ toString(okCount) ~ " von 2 Teilen. Fehler: ";
            for (var s in failed)
                msg = msg ~ s ~ " || ";
            reportFeatureWarning(context, id, msg);
        }
    });

