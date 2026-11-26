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

function o2m_o2mA(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "o2mA_a0";
    coneOp(context, A0 + "s0", vector(0, 0, 0) * millimeter, vector(0, 0, 12) * millimeter, 2.5, 3.5);
    coneOp(context, A0 + "s1", vector(0, 0, 11.99) * millimeter, vector(0, 0, 22.75) * millimeter, 3.5, 14.25);
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(0, 0, 22.74) * millimeter, "topCenter" : vector(0, 0, 57.15) * millimeter, "radius" : 14.25 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(0, 0, 34) * millimeter, "topCenter" : vector(28.284, 0, 5.716) * millimeter, "radius" : 10 * millimeter });
    coneOp(context, A0 + "s4", vector(25.456, 0, 8.544) * millimeter, vector(26.022, 0, 7.978) * millimeter, 10, 10.8);
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(26.014, 0, 7.986) * millimeter, "topCenter" : vector(27.153, 0, 6.847) * millimeter, "radius" : 10.8 * millimeter });
    coneOp(context, A0 + "s6", vector(27.146, 0, 6.854) * millimeter, vector(27.719, 0, 6.281) * millimeter, 10.8, 10);
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "o2mA_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "o2mA_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(0, 0, -1) * millimeter, "topCenter" : vector(0, 0, 15.06) * millimeter, "radius" : 1.5 * millimeter });
    coneOp(context, C1 + "o1", vector(0, 0, 15.05) * millimeter, vector(0, 0, 23.75) * millimeter, 1.5, 10.2);
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(0, 0, 23.74) * millimeter, "topCenter" : vector(0, 0, 40.01) * millimeter, "radius" : 10.2 * millimeter });
    coneOp(context, C1 + "o3", vector(0, 0, 40) * millimeter, vector(0, 0, 42.45) * millimeter, 10.2, 12.65);
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(0, 0, 42.44) * millimeter, "topCenter" : vector(0, 0, 58.15) * millimeter, "radius" : 12.65 * millimeter });
    coneOp(context, C1 + "o5", vector(0, 0, 56.55) * millimeter, vector(0, 0, 57.16) * millimeter, 12.65, 13.26);
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(0, 0, 34) * millimeter, "topCenter" : vector(28.991, 0, 5.009) * millimeter, "radius" : 8 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "o2mA_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "O2-Mundstück A: Grundkörper (Schlauchtülle, Beutelring 45°, Ringaufnahme)", color(0.3, 0.6, 0.85));
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
    coneOp(context, A0 + "s0", vector(0, 0, 50.15) * millimeter, vector(0, 0, 50.75) * millimeter, 11.97, 12.57);
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(0, 0, 50.74) * millimeter, "topCenter" : vector(0, 0, 57.46) * millimeter, "radius" : 12.57 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(0, 0, 57.45) * millimeter, "topCenter" : vector(0, 0, 60.45) * millimeter, "radius" : 14.25 * millimeter });
    coneOp(context, A0 + "s3", vector(0, 0, 60.44) * millimeter, vector(0, 0, 67.2) * millimeter, 14.25, 7.5);
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(0, 0, 67.19) * millimeter, "topCenter" : vector(0, 0, 81.2) * millimeter, "radius" : 7.5 * millimeter });
    coneOp(context, A0 + "s5", vector(0, 0, 77.6) * millimeter, vector(0, 0, 78.6) * millimeter, 7.5, 8.5);
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(0, 0, 78.59) * millimeter, "topCenter" : vector(0, 0, 80.2) * millimeter, "radius" : 8.5 * millimeter });
    coneOp(context, A0 + "s7", vector(0, 0, 80.19) * millimeter, vector(0, 0, 81.2) * millimeter, 8.5, 7.5);
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "o2mB_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "o2mB_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(0, 0, 49.15) * millimeter, "topCenter" : vector(0, 0, 60.96) * millimeter, "radius" : 10.2 * millimeter });
    coneOp(context, C1 + "o1", vector(0, 0, 60.95) * millimeter, vector(0, 0, 65.65) * millimeter, 10.2, 5.5);
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(0, 0, 65.64) * millimeter, "topCenter" : vector(0, 0, 82.2) * millimeter, "radius" : 5.5 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "o2mB_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "O2-Mundstück B: Mundrohr (steckt in A, drückt den Ring an)", color(0.85, 0.55, 0.3));
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
        if (size(failed) > 0)
        {
            var msg = "OK: " ~ toString(okCount) ~ " von 3 Teilen. Fehler: ";
            for (var s in failed)
                msg = msg ~ s ~ " || ";
            reportFeatureWarning(context, id, msg);
        }
    });

