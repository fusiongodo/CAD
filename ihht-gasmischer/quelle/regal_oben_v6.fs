FeatureScript 3112;
import(path : "onshape/std/geometry.fs", version : "3112.0");
// Regal oben v6 (verstaerkt): erzeugt von gen_regal_v6.py (Basis gen5.py). Platte 3, Rippen 3 x 8, Dome Ø 10.

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

function rg6_shelf6(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "shelf6_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-63.85, 27, 112) * millimeter, "corner2" : vector(63.85, 79, 115) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-53, 27, 104) * millimeter, "corner2" : vector(53, 30, 112.5) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(-53, 76, 104) * millimeter, "corner2" : vector(53, 79, 112.5) * millimeter });
    fCuboid(context, A0 + "s3", { "corner1" : vector(-57.8, 27, 104) * millimeter, "corner2" : vector(-55, 79, 112.5) * millimeter });
    fCuboid(context, A0 + "s4", { "corner1" : vector(-55.5, 27, 104) * millimeter, "corner2" : vector(-53, 30, 112.5) * millimeter });
    fCuboid(context, A0 + "s5", { "corner1" : vector(-55.5, 76, 104) * millimeter, "corner2" : vector(-53, 79, 112.5) * millimeter });
    fCuboid(context, A0 + "s6", { "corner1" : vector(55, 27, 104) * millimeter, "corner2" : vector(57.8, 79, 112.5) * millimeter });
    fCuboid(context, A0 + "s7", { "corner1" : vector(53, 27, 104) * millimeter, "corner2" : vector(55.5, 30, 112.5) * millimeter });
    fCuboid(context, A0 + "s8", { "corner1" : vector(53, 76, 104) * millimeter, "corner2" : vector(55.5, 79, 112.5) * millimeter });
    fCuboid(context, A0 + "s9", { "corner1" : vector(-40.65, 33, 114.5) * millimeter, "corner2" : vector(-30.65, 69, 120) * millimeter });
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(-35.65, 51, 104) * millimeter, "topCenter" : vector(-35.65, 51, 112.5) * millimeter, "radius" : 5 * millimeter });
    fCuboid(context, A0 + "s11", { "corner1" : vector(30.65, 33, 114.5) * millimeter, "corner2" : vector(40.65, 69, 120) * millimeter });
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(35.65, 51, 104) * millimeter, "topCenter" : vector(35.65, 51, 112.5) * millimeter, "radius" : 5 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "shelf6_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "shelf6_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-62, 31, 111) * millimeter, "topCenter" : vector(-62, 31, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-62, 75, 111) * millimeter, "topCenter" : vector(-62, 75, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(62, 31, 111) * millimeter, "topCenter" : vector(62, 31, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(62, 75, 111) * millimeter, "topCenter" : vector(62, 75, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o4", { "corner1" : vector(-53, 34, 111) * millimeter, "corner2" : vector(-43.65, 72, 115.4) * millimeter });
    fCuboid(context, C1 + "o5", { "corner1" : vector(-28.65, 34, 111) * millimeter, "corner2" : vector(28.65, 72, 115.4) * millimeter });
    fCuboid(context, C1 + "o6", { "corner1" : vector(43.65, 34, 111) * millimeter, "corner2" : vector(53, 72, 115.4) * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(-35.65, 51, 109.5) * millimeter, "topCenter" : vector(-35.65, 51, 121) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(35.65, 51, 109.5) * millimeter, "topCenter" : vector(35.65, 51, 121) * millimeter, "radius" : 1.3 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "shelf6_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Regal oben v6 (nur Sammler Mix, verstaerkt)", color(0.87, 0.81, 0.69));
}

annotation { "Feature Type Name" : "Regal oben v6" }
export const regalObenV6 = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
    }
    {
        const prog = new box("");
        var failed = [];
        var okCount = 0;
        try
        {
            rg6_shelf6(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "shelf6 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        if (size(failed) > 0)
        {
            var msg = "OK: " ~ toString(okCount) ~ " von 1 Teilen. Fehler: ";
            for (var s in failed)
                msg = msg ~ s ~ " || ";
            reportFeatureWarning(context, id, msg);
        }
    });

