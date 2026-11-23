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

function cam3_camMount(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "camMount_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-20, -125, 64) * millimeter, "corner2" : vector(20, -124, 122) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(-10.5, -124.5, 77.5) * millimeter, "topCenter" : vector(-10.5, -118, 77.5) * millimeter, "radius" : 5 * millimeter });
    coneOp(context, A0 + "s2", vector(-10.5, -124.01, 77.5) * millimeter, vector(-10.5, -123.2, 77.5) * millimeter, 5.8, 5);
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(-10.5, -124.5, 90.5) * millimeter, "topCenter" : vector(-10.5, -118, 90.5) * millimeter, "radius" : 5 * millimeter });
    coneOp(context, A0 + "s4", vector(-10.5, -124.01, 90.5) * millimeter, vector(-10.5, -123.2, 90.5) * millimeter, 5.8, 5);
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(10.5, -124.5, 77.5) * millimeter, "topCenter" : vector(10.5, -118, 77.5) * millimeter, "radius" : 5 * millimeter });
    coneOp(context, A0 + "s6", vector(10.5, -124.01, 77.5) * millimeter, vector(10.5, -123.2, 77.5) * millimeter, 5.8, 5);
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(10.5, -124.5, 90.5) * millimeter, "topCenter" : vector(10.5, -118, 90.5) * millimeter, "radius" : 5 * millimeter });
    coneOp(context, A0 + "s8", vector(10.5, -124.01, 90.5) * millimeter, vector(10.5, -123.2, 90.5) * millimeter, 5.8, 5);
    fCylinder(context, A0 + "s9", { "bottomCenter" : vector(0, -124.5, 110) * millimeter, "topCenter" : vector(0, -92, 110) * millimeter, "radius" : 8.25 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "camMount_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "camMount_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-15, -126, 70) * millimeter, "topCenter" : vector(-15, -123, 70) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-15, -126, 116) * millimeter, "topCenter" : vector(-15, -123, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(-10.5, -126, 77.5) * millimeter, "topCenter" : vector(-10.5, -116, 77.5) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(-10.5, -126, 90.5) * millimeter, "topCenter" : vector(-10.5, -116, 90.5) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(15, -126, 70) * millimeter, "topCenter" : vector(15, -123, 70) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(15, -126, 116) * millimeter, "topCenter" : vector(15, -123, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(10.5, -126, 77.5) * millimeter, "topCenter" : vector(10.5, -116, 77.5) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(10.5, -126, 90.5) * millimeter, "topCenter" : vector(10.5, -116, 90.5) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(0, -126, 110) * millimeter, "topCenter" : vector(0, -91, 110) * millimeter, "radius" : 7.25 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "camMount_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Kamerahalter v3 (1 mm, M2 durch + Mutter)", color(0.22, 0.22, 0.24));
}

annotation { "Feature Type Name" : "IHHT Kamerahalter v3" }
export const ihhtKamerahalterV3 = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
    }
    {
        const prog = new box("");
        var failed = [];
        var okCount = 0;
        try
        {
            cam3_camMount(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "camMount @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        if (size(failed) > 0)
        {
            var msg = "OK: " ~ toString(okCount) ~ " von 1 Teilen. Fehler: ";
            for (var s in failed)
                msg = msg ~ s ~ " || ";
            reportFeatureWarning(context, id, msg);
        }
    });

