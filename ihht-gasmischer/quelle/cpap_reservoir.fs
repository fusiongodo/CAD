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
    coneOp(context, A0 + "s9", vector(0, 0, 16.2) * millimeter, vector(0, 0, 18.7) * millimeter, 12, 14.5);
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(0, 0, 18.69) * millimeter, "topCenter" : vector(0, 0, 78) * millimeter, "radius" : 14.5 * millimeter });
    fCylinder(context, A0 + "s11", { "bottomCenter" : vector(0, 0, 26) * millimeter, "topCenter" : vector(25.456, 0, 51.456) * millimeter, "radius" : 14.5 * millimeter });
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(25.102, 0, 51.102) * millimeter, "topCenter" : vector(36.911, 0, 62.911) * millimeter, "radius" : 11.25 * millimeter });
    fCylinder(context, A0 + "s13", { "bottomCenter" : vector(27.577, 0, 53.577) * millimeter, "topCenter" : vector(28.143, 0, 54.143) * millimeter, "radius" : 12 * millimeter });
    coneOp(context, A0 + "s14", vector(28.143, 0, 54.143) * millimeter, vector(29.911, 0, 55.911) * millimeter, 12, 11.25);
    fCylinder(context, A0 + "s15", { "bottomCenter" : vector(29.911, 0, 55.911) * millimeter, "topCenter" : vector(30.476, 0, 56.476) * millimeter, "radius" : 12 * millimeter });
    coneOp(context, A0 + "s16", vector(30.476, 0, 56.476) * millimeter, vector(32.244, 0, 58.244) * millimeter, 12, 11.25);
    fCylinder(context, A0 + "s17", { "bottomCenter" : vector(32.244, 0, 58.244) * millimeter, "topCenter" : vector(32.81, 0, 58.81) * millimeter, "radius" : 12 * millimeter });
    coneOp(context, A0 + "s18", vector(32.81, 0, 58.81) * millimeter, vector(34.578, 0, 60.578) * millimeter, 12, 11.25);
    fCylinder(context, A0 + "s19", { "bottomCenter" : vector(34.578, 0, 60.578) * millimeter, "topCenter" : vector(35.143, 0, 61.143) * millimeter, "radius" : 12 * millimeter });
    coneOp(context, A0 + "s20", vector(35.143, 0, 61.143) * millimeter, vector(36.911, 0, 62.911) * millimeter, 12, 11.25);
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "cpapResA_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "cpapResA_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(0, 0, -1) * millimeter, "topCenter" : vector(0, 0, 51.3) * millimeter, "radius" : 10.2 * millimeter });
    coneOp(context, C1 + "o1", vector(0, 0, 51.3) * millimeter, vector(0, 0, 52) * millimeter, 10.2, 9.5);
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(0, 0, 51.99) * millimeter, "topCenter" : vector(0, 0, 55.01) * millimeter, "radius" : 9.5 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(0, 0, 55) * millimeter, "topCenter" : vector(0, 0, 68.01) * millimeter, "radius" : 11.25 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(0, 0, 68) * millimeter, "topCenter" : vector(0, 0, 79) * millimeter, "radius" : 12.6 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(0, 0, 26) * millimeter, "topCenter" : vector(37.618, 0, 63.618) * millimeter, "radius" : 10.2 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "cpapResA_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Körper Stufe 2";
    const A2 = id + "cpapResA_a2";
    prismOp(context, A2 + "s0", vector(0, -0.75, 55) * millimeter, [[-10.5, 0], [10.5, 0], [10.5, -3], [-1.5, -3], [-10.5, -12]], 1.5);
    prismXOp(context, A2 + "s1", vector(-0.75, 0, 55) * millimeter, [[-10.5, 0], [10.5, 0], [10.5, -12], [0, -1.5], [-10.5, -12]], 1.5);
    fCylinder(context, A2 + "s2", { "bottomCenter" : vector(0, 0, 54.25) * millimeter, "topCenter" : vector(0, 0, 56) * millimeter, "radius" : 1.25 * millimeter });
    coneOp(context, A2 + "s3", vector(0, 0, 56) * millimeter, vector(0, 0, 59.464) * millimeter, 1.25, 3.25);
    fCylinder(context, A2 + "s4", { "bottomCenter" : vector(0, 0, 59.464) * millimeter, "topCenter" : vector(0, 0, 59.764) * millimeter, "radius" : 3.25 * millimeter });
    coneOp(context, A2 + "s5", vector(0, 0, 59.764) * millimeter, vector(0, 0, 61.264) * millimeter, 3.25, 0.5);
    prog[] = "Vereinigen Stufe 2";
    unionAll(context, id + "cpapResA_u2", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]), "CPAP-Reservoir A: Quelle + Beutel 45° + Ventilsitz (Membran Ø 21,5)", color(0.3, 0.6, 0.85));
}

function cr_cpapResB(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "cpapResB_a0";
    coneOp(context, A0 + "s0", vector(0, 0, 68) * millimeter, vector(0, 0, 78) * millimeter, 12.3, 12.6);
    coneOp(context, A0 + "s1", vector(0, 0, 78) * millimeter, vector(0, 0, 79.9) * millimeter, 12.6, 14.5);
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(0, 0, 79.89) * millimeter, "topCenter" : vector(0, 0, 81) * millimeter, "radius" : 14.5 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(0, 0, 80.5) * millimeter, "topCenter" : vector(0, 0, 97.2) * millimeter, "radius" : 11.25 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(0, 0, 84) * millimeter, "topCenter" : vector(0, 0, 84.8) * millimeter, "radius" : 12 * millimeter });
    coneOp(context, A0 + "s5", vector(0, 0, 84.8) * millimeter, vector(0, 0, 87.3) * millimeter, 12, 11.25);
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(0, 0, 87.3) * millimeter, "topCenter" : vector(0, 0, 88.1) * millimeter, "radius" : 12 * millimeter });
    coneOp(context, A0 + "s7", vector(0, 0, 88.1) * millimeter, vector(0, 0, 90.6) * millimeter, 12, 11.25);
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(0, 0, 90.6) * millimeter, "topCenter" : vector(0, 0, 91.4) * millimeter, "radius" : 12 * millimeter });
    coneOp(context, A0 + "s9", vector(0, 0, 91.4) * millimeter, vector(0, 0, 93.9) * millimeter, 12, 11.25);
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(0, 0, 93.9) * millimeter, "topCenter" : vector(0, 0, 94.7) * millimeter, "radius" : 12 * millimeter });
    coneOp(context, A0 + "s11", vector(0, 0, 94.7) * millimeter, vector(0, 0, 97.2) * millimeter, 12, 11.25);
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "cpapResB_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "cpapResB_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(0, 0, 67) * millimeter, "topCenter" : vector(0, 0, 98.2) * millimeter, "radius" : 10.2 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "cpapResB_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "CPAP-Reservoir B: Maskenkappe (Zapfen konisch Ø 24,6-25,2)", color(0.85, 0.55, 0.3));
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
            cr_cpapResB(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "cpapResB @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        if (size(failed) > 0)
        {
            var msg = "OK: " ~ toString(okCount) ~ " von 2 Teilen. Fehler: ";
            for (var s in failed)
                msg = msg ~ s ~ " || ";
            reportFeatureWarning(context, id, msg);
        }
    });

