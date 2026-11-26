FeatureScript 3112;
import(path : "onshape/std/geometry.fs", version : "3112.0");
// Quetschventil Gegenlager 6200 (Rahmen v2): erzeugt von build.py aus teile/qv2_teile.py (Bibliothek ihht/). Nicht von Hand aendern.

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

function q2_q2Platte(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "q2Platte_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-44.5, -6, -41.65) * millimeter, "corner2" : vector(44.5, 0, 41.65) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-26, -11, -41.65) * millimeter, "corner2" : vector(26, -5.99, -38.65) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(-26, -11, 38.65) * millimeter, "corner2" : vector(26, -5.99, 41.65) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "q2Platte_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "q2Platte_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(0, 1, 0) * millimeter, "topCenter" : vector(0, -7, 0) * millimeter, "radius" : 11.2 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-15.5, 1, -15.5) * millimeter, "topCenter" : vector(-15.5, -7, -15.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(-15.5, -2.8, -15.5) * millimeter, "topCenter" : vector(-15.5, -7, -15.5) * millimeter, "radius" : 3.2 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(-40, 1, -15) * millimeter, "topCenter" : vector(-40, -7, -15) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(-15.5, 1, 15.5) * millimeter, "topCenter" : vector(-15.5, -7, 15.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(-15.5, -2.8, 15.5) * millimeter, "topCenter" : vector(-15.5, -7, 15.5) * millimeter, "radius" : 3.2 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(-40, 1, 15) * millimeter, "topCenter" : vector(-40, -7, 15) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(15.5, 1, -15.5) * millimeter, "topCenter" : vector(15.5, -7, -15.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(15.5, -2.8, -15.5) * millimeter, "topCenter" : vector(15.5, -7, -15.5) * millimeter, "radius" : 3.2 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(40, 1, -15) * millimeter, "topCenter" : vector(40, -7, -15) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o10", { "bottomCenter" : vector(15.5, 1, 15.5) * millimeter, "topCenter" : vector(15.5, -7, 15.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o11", { "bottomCenter" : vector(15.5, -2.8, 15.5) * millimeter, "topCenter" : vector(15.5, -7, 15.5) * millimeter, "radius" : 3.2 * millimeter });
    fCylinder(context, C1 + "o12", { "bottomCenter" : vector(40, 1, 15) * millimeter, "topCenter" : vector(40, -7, 15) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o13", { "bottomCenter" : vector(-26, 1, -33.5) * millimeter, "topCenter" : vector(-26, -7, -33.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o14", { "bottomCenter" : vector(-26, 1, -33.5) * millimeter, "topCenter" : vector(-26, -3.2, -33.5) * millimeter, "radius" : 3.2 * millimeter });
    fCylinder(context, C1 + "o15", { "bottomCenter" : vector(-26, 1, 33.5) * millimeter, "topCenter" : vector(-26, -7, 33.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o16", { "bottomCenter" : vector(-26, 1, 33.5) * millimeter, "topCenter" : vector(-26, -3.2, 33.5) * millimeter, "radius" : 3.2 * millimeter });
    fCylinder(context, C1 + "o17", { "bottomCenter" : vector(0, 1, -33.5) * millimeter, "topCenter" : vector(0, -7, -33.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o18", { "bottomCenter" : vector(0, 1, -33.5) * millimeter, "topCenter" : vector(0, -3.2, -33.5) * millimeter, "radius" : 3.2 * millimeter });
    fCylinder(context, C1 + "o19", { "bottomCenter" : vector(0, 1, 33.5) * millimeter, "topCenter" : vector(0, -7, 33.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o20", { "bottomCenter" : vector(0, 1, 33.5) * millimeter, "topCenter" : vector(0, -3.2, 33.5) * millimeter, "radius" : 3.2 * millimeter });
    fCylinder(context, C1 + "o21", { "bottomCenter" : vector(26, 1, -33.5) * millimeter, "topCenter" : vector(26, -7, -33.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o22", { "bottomCenter" : vector(26, 1, -33.5) * millimeter, "topCenter" : vector(26, -3.2, -33.5) * millimeter, "radius" : 3.2 * millimeter });
    fCylinder(context, C1 + "o23", { "bottomCenter" : vector(26, 1, 33.5) * millimeter, "topCenter" : vector(26, -7, 33.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o24", { "bottomCenter" : vector(26, 1, 33.5) * millimeter, "topCenter" : vector(26, -3.2, 33.5) * millimeter, "radius" : 3.2 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "q2Platte_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "QV-Rahmen v2 Bodenplatte (Anschlagleisten, 6 x M3 von hinten in die Ambosse)", color(0.93, 0.55, 0.2));
}

function q2_q2AmbossO(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "q2AmbossO_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-35.5, -40, 28.5) * millimeter, "corner2" : vector(35.5, -6, 38.5) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-35.5, -13.181, 24.5) * millimeter, "corner2" : vector(-25.5, -11.181, 28.51) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(-35.5, -30.819, 24.5) * millimeter, "corner2" : vector(-25.5, -28.819, 28.51) * millimeter });
    fCuboid(context, A0 + "s3", { "corner1" : vector(25.5, -13.181, 24.5) * millimeter, "corner2" : vector(35.5, -11.181, 28.51) * millimeter });
    fCuboid(context, A0 + "s4", { "corner1" : vector(25.5, -30.819, 24.5) * millimeter, "corner2" : vector(35.5, -28.819, 28.51) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "q2AmbossO_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "q2AmbossO_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-26, -5, 33.5) * millimeter, "topCenter" : vector(-26, -18, 33.5) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(0, -5, 33.5) * millimeter, "topCenter" : vector(0, -18, 33.5) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(26, -5, 33.5) * millimeter, "topCenter" : vector(26, -18, 33.5) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(-26, -5, 33.5) * millimeter, "topCenter" : vector(-26, -41, 33.5) * millimeter, "radius" : 1.3 * millimeter });
    fCuboid(context, C1 + "o4", { "corner1" : vector(-34, -10.681, 27.5) * millimeter, "corner2" : vector(-30, -8.881, 39.5) * millimeter });
    fCuboid(context, C1 + "o5", { "corner1" : vector(-34, -33.119, 27.5) * millimeter, "corner2" : vector(-30, -31.319, 39.5) * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(26, -5, 33.5) * millimeter, "topCenter" : vector(26, -41, 33.5) * millimeter, "radius" : 1.3 * millimeter });
    fCuboid(context, C1 + "o7", { "corner1" : vector(30, -10.681, 27.5) * millimeter, "corner2" : vector(34, -8.881, 39.5) * millimeter });
    fCuboid(context, C1 + "o8", { "corner1" : vector(30, -33.119, 27.5) * millimeter, "corner2" : vector(34, -31.319, 39.5) * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "q2AmbossO_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "QV-Rahmen v2 Amboss oben (10 dick)", color(0.93, 0.55, 0.2));
}

function q2_q2AmbossU(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "q2AmbossU_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-35.5, -40, -38.5) * millimeter, "corner2" : vector(35.5, -6, -28.5) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-35.5, -13.181, -28.51) * millimeter, "corner2" : vector(-25.5, -11.181, -24.5) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(-35.5, -30.819, -28.51) * millimeter, "corner2" : vector(-25.5, -28.819, -24.5) * millimeter });
    fCuboid(context, A0 + "s3", { "corner1" : vector(25.5, -13.181, -28.51) * millimeter, "corner2" : vector(35.5, -11.181, -24.5) * millimeter });
    fCuboid(context, A0 + "s4", { "corner1" : vector(25.5, -30.819, -28.51) * millimeter, "corner2" : vector(35.5, -28.819, -24.5) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "q2AmbossU_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "q2AmbossU_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-26, -5, -33.5) * millimeter, "topCenter" : vector(-26, -18, -33.5) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(0, -5, -33.5) * millimeter, "topCenter" : vector(0, -18, -33.5) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(26, -5, -33.5) * millimeter, "topCenter" : vector(26, -18, -33.5) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(-26, -5, -33.5) * millimeter, "topCenter" : vector(-26, -41, -33.5) * millimeter, "radius" : 1.3 * millimeter });
    fCuboid(context, C1 + "o4", { "corner1" : vector(-34, -10.681, -39.5) * millimeter, "corner2" : vector(-30, -8.881, -27.5) * millimeter });
    fCuboid(context, C1 + "o5", { "corner1" : vector(-34, -33.119, -39.5) * millimeter, "corner2" : vector(-30, -31.319, -27.5) * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(26, -5, -33.5) * millimeter, "topCenter" : vector(26, -41, -33.5) * millimeter, "radius" : 1.3 * millimeter });
    fCuboid(context, C1 + "o7", { "corner1" : vector(30, -10.681, -39.5) * millimeter, "corner2" : vector(34, -8.881, -27.5) * millimeter });
    fCuboid(context, C1 + "o8", { "corner1" : vector(30, -33.119, -39.5) * millimeter, "corner2" : vector(34, -31.319, -27.5) * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "q2AmbossU_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "QV-Rahmen v2 Amboss unten (gleiches Teil, 180° gedreht)", color(0.93, 0.55, 0.2));
}

function q2_q2Steg(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "q2Steg_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-35.5, -47.2, -41.65) * millimeter, "corner2" : vector(35.5, -40, 41.65) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-26, -40.01, -41.65) * millimeter, "corner2" : vector(26, -35, -38.65) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(-26, -40.01, 38.65) * millimeter, "corner2" : vector(26, -35, 41.65) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "q2Steg_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "q2Steg_c1";
    fCuboid(context, C1 + "o0", { "corner1" : vector(-24.5, -48.2, -28) * millimeter, "corner2" : vector(24.5, -39, 28) * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "q2Steg_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Körper Stufe 2";
    const A2 = id + "q2Steg_a2";
    fCuboid(context, A2 + "s0", { "corner1" : vector(-9, -47.2, -28.1) * millimeter, "corner2" : vector(9, -40, 28.1) * millimeter });
    fCylinder(context, A2 + "s1", { "bottomCenter" : vector(0, -37, 0) * millimeter, "topCenter" : vector(0, -47.2, 0) * millimeter, "radius" : 18 * millimeter });
    prog[] = "Vereinigen Stufe 2";
    unionAll(context, id + "q2Steg_u2", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 3";
    const C3 = id + "q2Steg_c3";
    fCylinder(context, C3 + "o0", { "bottomCenter" : vector(0, -38.2, 0) * millimeter, "topCenter" : vector(0, -48.2, 0) * millimeter, "radius" : 15.15 * millimeter });
    fCylinder(context, C3 + "o1", { "bottomCenter" : vector(0, -36, 0) * millimeter, "topCenter" : vector(0, -38.21, 0) * millimeter, "radius" : 12.25 * millimeter });
    coneOp(context, C3 + "o2", vector(0, -46.6, 0) * millimeter, vector(0, -47.21, 0) * millimeter, 15.15, 15.75);
    fCylinder(context, C3 + "o3", { "bottomCenter" : vector(-26, -39, -33.5) * millimeter, "topCenter" : vector(-26, -48.2, -33.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C3 + "o4", { "bottomCenter" : vector(-26, -39, 33.5) * millimeter, "topCenter" : vector(-26, -48.2, 33.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C3 + "o5", { "bottomCenter" : vector(26, -39, -33.5) * millimeter, "topCenter" : vector(26, -48.2, -33.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C3 + "o6", { "bottomCenter" : vector(26, -39, 33.5) * millimeter, "topCenter" : vector(26, -48.2, 33.5) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 3";
    cutAll(context, id + "q2Steg_x3", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]), qCreatedBy(C3, EntityType.BODY));
    prog[] = "Körper Stufe 4";
    const A4 = id + "q2Steg_a4";
    prismOp(context, A4 + "s0", vector(0, -46.4, 0) * millimeter, [[13.154, 7.017], [13.543, 7.242], [13.043, 8.108], [12.654, 7.883]], 8.2);
    prismOp(context, A4 + "s1", vector(0, -46.4, 0) * millimeter, [[0.5, 14.9], [0.5, 15.35], [-0.5, 15.35], [-0.5, 14.9]], 8.2);
    prismOp(context, A4 + "s2", vector(0, -46.4, 0) * millimeter, [[-12.654, 7.883], [-13.043, 8.108], [-13.543, 7.242], [-13.154, 7.017]], 8.2);
    prismOp(context, A4 + "s3", vector(0, -46.4, 0) * millimeter, [[-13.154, -7.017], [-13.543, -7.242], [-13.043, -8.108], [-12.654, -7.883]], 8.2);
    prismOp(context, A4 + "s4", vector(0, -46.4, 0) * millimeter, [[-0.5, -14.9], [-0.5, -15.35], [0.5, -15.35], [0.5, -14.9]], 8.2);
    prismOp(context, A4 + "s5", vector(0, -46.4, 0) * millimeter, [[12.654, -7.883], [13.043, -8.108], [13.543, -7.242], [13.154, -7.017]], 8.2);
    prog[] = "Vereinigen Stufe 4";
    unionAll(context, id + "q2Steg_u4", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY), qCreatedBy(A4, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY), qCreatedBy(A4, EntityType.BODY)]), "QV-Rahmen v2 Steg vorn mit Gegenlager 6200 (7,2 dick)", color(0.93, 0.55, 0.2));
}

function q2_q2Welle(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "q2Welle_a0";
    coneOp(context, A0 + "s0", vector(0, 27.8, 0) * millimeter, vector(0, 27.4, 0) * millimeter, 6.6, 7);
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(0, 27.41, 0) * millimeter, "topCenter" : vector(0, 12.8, 0) * millimeter, "radius" : 7 * millimeter });
    coneOp(context, A0 + "s2", vector(0, 12.9, 0) * millimeter, vector(0, 9, 0) * millimeter, 7, 10.9);
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(0, 9.01, 0) * millimeter, "topCenter" : vector(0, -9, 0) * millimeter, "radius" : 10.9 * millimeter });
    coneOp(context, A0 + "s4", vector(3.5, -7.6, 0) * millimeter, vector(3.5, -9, 0) * millimeter, 7.05, 8.45);
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(3.5, -8.99, 0) * millimeter, "topCenter" : vector(3.5, -32.8, 0) * millimeter, "radius" : 8.45 * millimeter });
    prismOp(context, A0 + "s6", vector(0, -46.9, 0) * millimeter, [[-4.794, -1.329], [-4.601, -1.892], [-4.343, -2.427], [-4.022, -2.928], [-3.644, -3.387], [-3.213, -3.798], [-2.737, -4.154], [-2.222, -4.451], [-1.675, -4.685], [-1.104, -4.851], [-0.517, -4.948], [0.077, -4.974], [0.671, -4.93], [1.254, -4.814], [1.82, -4.63], [2.359, -4.38], [2.865, -4.067], [3.33, -3.696], [3.748, -3.272], [4.111, -2.801], [4.416, -2.291], [4.658, -1.747], [4.833, -1.179], [4.939, -0.594], [4.975, 0], [4.939, 0.594], [4.833, 1.179], [4.658, 1.747], [4.416, 2.291], [4.111, 2.801], [3.748, 3.272], [3.33, 3.696], [2.865, 4.067], [2.359, 4.38], [1.82, 4.63], [1.254, 4.814], [0.671, 4.93], [0.077, 4.974], [-0.517, 4.948], [-1.104, 4.851], [-1.675, 4.685], [-2.222, 4.451], [-2.737, 4.154], [-3.213, 3.798], [-3.644, 3.387], [-4.022, 2.928], [-4.343, 2.427], [-4.601, 1.892], [-4.794, 1.329], [-4.811, 1.219], [-4.827, 1.109], [-4.84, 0.999], [-4.853, 0.888], [-4.864, 0.777], [-4.873, 0.667], [-4.882, 0.556], [-4.888, 0.445], [-4.893, 0.334], [-4.897, 0.222], [-4.899, 0.111], [-4.9, 0], [-4.899, -0.111], [-4.897, -0.222], [-4.893, -0.334], [-4.888, -0.445], [-4.882, -0.556], [-4.873, -0.667], [-4.864, -0.777], [-4.853, -0.888], [-4.84, -0.999], [-4.827, -1.109], [-4.811, -1.219]], 14.11);
    prismOp(context, A0 + "s7", vector(0, -46.1, 0) * millimeter, [[4.775, -0.45], [5.095, -0.45], [5.095, 0.45], [4.775, 0.45]], 7.1);
    prismOp(context, A0 + "s8", vector(0, -46.1, 0) * millimeter, [[-0.801, 4.729], [-0.884, 5.038], [-1.753, 4.805], [-1.671, 4.496]], 7.1);
    prismOp(context, A0 + "s9", vector(0, -46.1, 0) * millimeter, [[-1.671, -4.496], [-1.753, -4.805], [-0.884, -5.038], [-0.801, -4.729]], 7.1);
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "q2Welle_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "q2Welle_c1";
    fCuboid(context, C1 + "o0", { "corner1" : vector(8.5, 0.6, -14) * millimeter, "corner2" : vector(14, 13.5, 14) * millimeter });
    fCuboid(context, C1 + "o1", { "corner1" : vector(-14, 0.6, -14) * millimeter, "corner2" : vector(-8.5, 13.5, 14) * millimeter });
    prismOp(context, C1 + "o2", vector(0, -9.01, 0) * millimeter, [[6.786, 10.498], [5.623, 11.164], [4.396, 11.701], [3.119, 12.105], [1.805, 12.369], [0.471, 12.491], [-0.869, 12.47], [-2.198, 12.305], [-3.503, 11.999], [-4.767, 11.555], [-5.976, 10.979], [-7.117, 10.276], [-8.176, 9.456], [-9.141, 8.526], [-10.001, 7.499], [-10.746, 6.386], [-11.368, 5.199], [-11.859, 3.952], [-12.214, 2.66], [-12.428, 1.338], [-12.5, 0], [-12.428, -1.338], [-12.214, -2.66], [-11.859, -3.952], [-11.368, -5.199], [-10.746, -6.386], [-10.001, -7.499], [-9.141, -8.526], [-8.176, -9.456], [-7.117, -10.276], [-5.976, -10.979], [-4.767, -11.555], [-3.503, -11.999], [-2.198, -12.305], [-0.869, -12.47], [0.471, -12.491], [1.805, -12.369], [3.119, -12.105], [4.396, -11.701], [5.623, -11.164], [6.786, -10.498], [5.789, -10.759], [4.772, -10.926], [3.744, -10.997], [2.714, -10.972], [1.691, -10.85], [0.684, -10.633], [-0.299, -10.323], [-1.248, -9.922], [-2.156, -9.435], [-3.014, -8.864], [-3.814, -8.216], [-4.551, -7.495], [-5.217, -6.709], [-5.807, -5.864], [-6.315, -4.967], [-6.736, -4.027], [-7.068, -3.052], [-7.307, -2.05], [-7.452, -1.029], [-7.5, 0], [-7.452, 1.029], [-7.307, 2.05], [-7.068, 3.052], [-6.736, 4.027], [-6.315, 4.967], [-5.807, 5.864], [-5.217, 6.709], [-4.551, 7.495], [-3.814, 8.216], [-3.014, 8.864], [-2.156, 9.435], [-1.248, 9.922], [-0.299, 10.323], [0.684, 10.633], [1.691, 10.85], [2.714, 10.972], [3.744, 10.997], [4.772, 10.926], [5.789, 10.759]], 0.61);
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(0, 17, 3.8) * millimeter, "topCenter" : vector(0, 17, 8) * millimeter, "radius" : 1.6 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(0, 12, 0) * millimeter, "topCenter" : vector(0, -33.01, 0) * millimeter, "radius" : 4.15 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(0, -33, 0) * millimeter, "topCenter" : vector(0, -47.9, 0) * millimeter, "radius" : 4 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(0, 24.25, 0) * millimeter, "topCenter" : vector(0, 11.99, 0) * millimeter, "radius" : 3.6 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "q2Welle_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "QV-Getriebe Welle mit Zapfen fuer 6200 (ein Teil, Exzenter e 3,5, M8-Kern)", color(0.93, 0.55, 0.2));
}

function q2_q2Ring(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "q2Ring_a0";
    prismOp(context, A0 + "s0", vector(0, -36.3, 0) * millimeter, [[-7.5, 0], [-7.494, -0.36], [-7.476, -0.719], [-7.447, -1.078], [-7.406, -1.436], [-7.353, -1.792], [-7.289, -2.146], [-7.213, -2.498], [-7.125, -2.847], [-7.026, -3.193], [-6.916, -3.536], [-6.795, -3.875], [-6.663, -4.21], [-6.519, -4.54], [-6.366, -4.865], [-6.201, -5.185], [-6.026, -5.5], [-5.841, -5.809], [-5.646, -6.111], [-5.441, -6.407], [-5.227, -6.696], [-5.003, -6.978], [-4.77, -7.253], [-4.529, -7.519], [-4.278, -7.778], [-4.019, -8.028], [-3.753, -8.27], [-3.478, -8.503], [-3.196, -8.727], [-2.907, -8.941], [-2.611, -9.146], [-2.309, -9.341], [-2, -9.526], [-1.685, -9.701], [-1.365, -9.866], [-1.04, -10.02], [-0.71, -10.163], [-0.375, -10.295], [-0.036, -10.416], [0.307, -10.526], [0.653, -10.625], [1.002, -10.713], [1.354, -10.789], [1.708, -10.853], [2.064, -10.906], [2.422, -10.947], [2.781, -10.976], [3.14, -10.994], [3.5, -11], [3.86, -10.994], [4.219, -10.976], [4.578, -10.947], [4.936, -10.906], [5.292, -10.853], [5.646, -10.789], [5.998, -10.713], [6.347, -10.625], [6.693, -10.526], [7.036, -10.416], [7.375, -10.295], [7.71, -10.163], [8.04, -10.02], [8.365, -9.866], [8.685, -9.701], [9, -9.526], [9.309, -9.341], [9.611, -9.146], [9.907, -8.941], [10.196, -8.727], [10.478, -8.503], [10.753, -8.27], [11.02, -8.028], [11.278, -7.778], [11.528, -7.519], [11.77, -7.253], [12.003, -6.978], [12.227, -6.696], [12.441, -6.407], [12.646, -6.111], [12.841, -5.809], [13.026, -5.5], [13.201, -5.185], [13.366, -4.865], [13.52, -4.54], [13.663, -4.21], [13.795, -3.875], [13.916, -3.536], [14.026, -3.193], [14.125, -2.847], [14.213, -2.498], [14.289, -2.146], [14.353, -1.792], [14.406, -1.436], [14.447, -1.078], [14.476, -0.719], [14.494, -0.36], [14.5, 0], [14.494, 0.36], [14.476, 0.719], [14.447, 1.078], [14.406, 1.436], [14.353, 1.792], [14.289, 2.146], [14.213, 2.498], [14.125, 2.847], [14.026, 3.193], [13.916, 3.536], [13.795, 3.875], [13.663, 4.21], [13.52, 4.54], [13.366, 4.865], [13.201, 5.185], [13.026, 5.5], [12.841, 5.809], [12.646, 6.111], [12.441, 6.407], [12.227, 6.696], [12.003, 6.978], [11.77, 7.253], [11.528, 7.519], [11.278, 7.778], [11.02, 8.028], [10.753, 8.27], [10.478, 8.503], [10.196, 8.727], [9.907, 8.941], [9.611, 9.146], [9.309, 9.341], [9, 9.526], [8.685, 9.701], [8.365, 9.866], [8.04, 10.02], [7.71, 10.163], [7.375, 10.295], [7.036, 10.416], [6.693, 10.526], [6.347, 10.625], [5.998, 10.713], [5.646, 10.789], [5.292, 10.853], [4.936, 10.906], [4.578, 10.947], [4.219, 10.976], [3.86, 10.994], [3.5, 11], [3.14, 10.994], [2.781, 10.976], [2.422, 10.947], [2.064, 10.906], [1.708, 10.853], [1.354, 10.789], [1.002, 10.713], [0.653, 10.625], [0.307, 10.526], [-0.036, 10.416], [-0.375, 10.295], [-0.71, 10.163], [-1.04, 10.02], [-1.365, 9.866], [-1.685, 9.701], [-2, 9.526], [-2.309, 9.341], [-2.611, 9.146], [-2.907, 8.941], [-3.196, 8.727], [-3.478, 8.503], [-3.753, 8.27], [-4.019, 8.028], [-4.278, 7.778], [-4.529, 7.519], [-4.77, 7.253], [-5.003, 6.978], [-5.227, 6.696], [-5.441, 6.407], [-5.646, 6.111], [-5.841, 5.809], [-6.026, 5.5], [-6.201, 5.185], [-6.366, 4.865], [-6.519, 4.54], [-6.663, 4.21], [-6.795, 3.875], [-6.916, 3.536], [-7.026, 3.193], [-7.125, 2.847], [-7.213, 2.498], [-7.289, 2.146], [-7.353, 1.792], [-7.406, 1.436], [-7.447, 1.078], [-7.476, 0.719], [-7.494, 0.36]], 3.3);
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(0, -36.29, 0) * millimeter, "topCenter" : vector(0, -38, 0) * millimeter, "radius" : 6.75 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "q2Ring_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "q2Ring_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(0, -32, 0) * millimeter, "topCenter" : vector(0, -39, 0) * millimeter, "radius" : 5.3 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "q2Ring_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "QV Distanzring zwischen 6203 und 6200 (lose, e 3,5)", color(0.93, 0.55, 0.2));
}

function q2_q2Huelse(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "q2Huelse_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(3.5, -9, 0) * millimeter, "topCenter" : vector(3.5, -34, 0) * millimeter, "radius" : 22.7 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "q2Huelse_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "q2Huelse_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(3.5, -8, 0) * millimeter, "topCenter" : vector(3.5, -33, 0) * millimeter, "radius" : 20.2 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(3.5, -32, 0) * millimeter, "topCenter" : vector(3.5, -35, 0) * millimeter, "radius" : 17.5 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "q2Huelse_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "QV Lagerhuelse weit (Wand 2,5, ueber beide 6203)", color(0.93, 0.55, 0.2));
}

function q2_q2Hohlrad(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "q2Hohlrad_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(0, 11.6, 0) * millimeter, "topCenter" : vector(0, 0.6, 0) * millimeter, "radius" : 36.5 * millimeter });
    coneOp(context, A0 + "s1", vector(0, 11.6, 0) * millimeter, vector(0, 10.5, 0) * millimeter, 37.6, 36.5);
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "q2Hohlrad_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "q2Hohlrad_c1";
    prismOp(context, C1 + "o0", vector(0, 3.6, 0) * millimeter, [[-1.056, -31.832], [-0.947, -32.216], [-0.823, -32.6], [-0.682, -32.983], [-0.528, -33.366], [-0.36, -33.748], [0, -33.75], [0.36, -33.748], [0.528, -33.366], [0.682, -32.983], [0.823, -32.6], [0.947, -32.216], [1.056, -31.832], [1.539, -31.813], [2.021, -31.786], [2.166, -32.157], [2.328, -32.527], [2.504, -32.895], [2.695, -33.261], [2.899, -33.625], [3.257, -33.592], [3.616, -33.556], [3.746, -33.159], [3.863, -32.763], [3.965, -32.368], [4.052, -31.974], [4.123, -31.582], [4.602, -31.516], [5.08, -31.442], [5.26, -31.798], [5.456, -32.15], [5.667, -32.5], [5.892, -32.846], [6.13, -33.188], [6.484, -33.121], [6.837, -33.05], [6.929, -32.643], [7.007, -32.237], [7.071, -31.834], [7.119, -31.434], [7.152, -31.037], [7.622, -30.924], [8.091, -30.805], [8.304, -31.142], [8.534, -31.474], [8.777, -31.801], [9.035, -32.124], [9.305, -32.442], [9.651, -32.341], [9.995, -32.236], [10.047, -31.822], [10.085, -31.411], [10.11, -31.003], [10.12, -30.6], [10.114, -30.201], [10.571, -30.044], [11.026, -29.881], [11.271, -30.195], [11.531, -30.503], [11.806, -30.805], [12.093, -31.102], [12.393, -31.392], [12.727, -31.258], [13.06, -31.121], [13.071, -30.703], [13.07, -30.291], [13.055, -29.883], [13.026, -29.48], [12.982, -29.084], [13.422, -28.884], [13.858, -28.677], [14.133, -28.966], [14.421, -29.248], [14.724, -29.522], [15.038, -29.789], [15.365, -30.05], [15.684, -29.884], [16.002, -29.715], [15.973, -29.299], [15.932, -28.888], [15.878, -28.483], [15.81, -28.086], [15.728, -27.696], [16.147, -27.454], [16.561, -27.206], [16.862, -27.467], [17.177, -27.719], [17.504, -27.963], [17.843, -28.199], [18.193, -28.427], [18.495, -28.231], [18.796, -28.032], [18.727, -27.62], [18.646, -27.215], [18.553, -26.818], [18.447, -26.429], [18.328, -26.048], [18.721, -25.767], [19.11, -25.48], [19.434, -25.711], [19.772, -25.932], [20.121, -26.143], [20.481, -26.345], [20.852, -26.538], [21.134, -26.314], [21.413, -26.087], [21.305, -25.684], [21.186, -25.289], [21.055, -24.902], [20.912, -24.525], [20.756, -24.158], [21.12, -23.84], [21.48, -23.517], [21.825, -23.716], [22.183, -23.903], [22.551, -24.079], [22.928, -24.245], [23.316, -24.402], [23.575, -24.151], [23.831, -23.899], [23.684, -23.508], [23.527, -23.126], [23.36, -22.754], [23.181, -22.392], [22.991, -22.042], [23.323, -21.69], [23.649, -21.334], [24.012, -21.498], [24.386, -21.65], [24.769, -21.79], [25.162, -21.919], [25.562, -22.037], [25.796, -21.763], [26.026, -21.487], [25.843, -21.112], [25.649, -20.747], [25.447, -20.393], [25.234, -20.05], [25.011, -19.72], [25.307, -19.338], [25.598, -18.952], [25.975, -19.081], [26.362, -19.196], [26.757, -19.298], [27.16, -19.389], [27.57, -19.467], [27.776, -19.172], [27.979, -18.875], [27.759, -18.519], [27.532, -18.175], [27.296, -17.842], [27.052, -17.521], [26.797, -17.214], [27.056, -16.805], [27.307, -16.393], [27.695, -16.485], [28.091, -16.562], [28.494, -16.626], [28.904, -16.677], [29.32, -16.716], [29.496, -16.402], [29.67, -16.086], [29.417, -15.754], [29.158, -15.433], [28.891, -15.124], [28.616, -14.829], [28.334, -14.547], [28.551, -14.116], [28.762, -13.681], [29.157, -13.735], [29.559, -13.773], [29.966, -13.798], [30.379, -13.809], [30.796, -13.808], [30.942, -13.479], [31.084, -13.148], [30.8, -12.841], [30.511, -12.546], [30.216, -12.265], [29.914, -11.997], [29.605, -11.745], [29.78, -11.294], [29.948, -10.841], [30.346, -10.856], [30.75, -10.856], [31.158, -10.841], [31.57, -10.813], [31.985, -10.771], [32.098, -10.429], [32.208, -10.086], [31.896, -9.808], [31.579, -9.543], [31.258, -9.292], [30.932, -9.054], [30.601, -8.832], [30.731, -8.367], [30.855, -7.9], [31.253, -7.877], [31.654, -7.838], [32.059, -7.784], [32.466, -7.715], [32.875, -7.634], [32.955, -7.283], [33.031, -6.931], [32.694, -6.684], [32.353, -6.451], [32.009, -6.231], [31.662, -6.027], [31.31, -5.838], [31.395, -5.362], [31.473, -4.885], [31.867, -4.824], [32.263, -4.746], [32.66, -4.653], [33.059, -4.546], [33.459, -4.425], [33.504, -4.068], [33.545, -3.71], [33.186, -3.498], [32.825, -3.298], [32.461, -3.113], [32.095, -2.943], [31.728, -2.789], [31.766, -2.307], [31.798, -1.825], [32.184, -1.726], [32.57, -1.61], [32.957, -1.479], [33.343, -1.334], [33.73, -1.175], [33.74, -0.816], [33.747, -0.456], [33.369, -0.278], [32.99, -0.115], [32.61, 0.035], [32.23, 0.169], [31.849, 0.286], [31.841, 0.77], [31.825, 1.253], [32.2, 1.389], [32.574, 1.541], [32.946, 1.708], [33.316, 1.89], [33.685, 2.085], [33.661, 2.445], [33.633, 2.804], [33.24, 2.944], [32.847, 3.07], [32.454, 3.182], [32.063, 3.279], [31.672, 3.359], [31.618, 3.839], [31.556, 4.318], [31.916, 4.49], [32.273, 4.678], [32.627, 4.88], [32.978, 5.097], [33.327, 5.327], [33.268, 5.682], [33.206, 6.037], [32.801, 6.138], [32.397, 6.226], [31.996, 6.299], [31.597, 6.358], [31.2, 6.4], [31.1, 6.873], [30.992, 7.344], [31.333, 7.549], [31.671, 7.771], [32.004, 8.006], [32.333, 8.256], [32.657, 8.518], [32.565, 8.866], [32.468, 9.213], [32.055, 9.275], [31.645, 9.323], [31.238, 9.358], [30.836, 9.377], [30.437, 9.382], [30.291, 9.842], [30.138, 10.301], [30.459, 10.538], [30.773, 10.791], [31.082, 11.058], [31.385, 11.338], [31.683, 11.631], [31.557, 11.968], [31.427, 12.304], [31.01, 12.325], [30.598, 12.334], [30.189, 12.329], [29.787, 12.31], [29.39, 12.275], [29.2, 12.72], [29.003, 13.161], [29.299, 13.429], [29.588, 13.71], [29.869, 14.006], [30.144, 14.314], [30.412, 14.634], [30.254, 14.958], [30.093, 15.28], [29.676, 15.261], [29.264, 15.23], [28.859, 15.185], [28.459, 15.127], [28.067, 15.055], [27.836, 15.479], [27.598, 15.899], [27.866, 16.194], [28.126, 16.502], [28.378, 16.823], [28.622, 17.157], [28.858, 17.501], [28.669, 17.808], [28.478, 18.113], [28.064, 18.054], [27.658, 17.983], [27.258, 17.9], [26.867, 17.803], [26.483, 17.693], [26.212, 18.093], [25.934, 18.488], [26.173, 18.808], [26.402, 19.14], [26.622, 19.484], [26.832, 19.839], [27.034, 20.204], [26.817, 20.492], [26.597, 20.777], [26.191, 20.678], [25.793, 20.568], [25.404, 20.447], [25.023, 20.313], [24.652, 20.166], [24.343, 20.538], [24.029, 20.905], [24.236, 21.246], [24.432, 21.599], [24.617, 21.962], [24.792, 22.336], [24.958, 22.719], [24.714, 22.984], [24.467, 23.247], [24.073, 23.109], [23.688, 22.962], [23.311, 22.803], [22.946, 22.633], [22.591, 22.452], [22.248, 22.792], [21.899, 23.127], [22.072, 23.486], [22.233, 23.856], [22.383, 24.235], [22.521, 24.625], [22.649, 25.022], [22.38, 25.262], [22.11, 25.5], [21.73, 25.325], [21.361, 25.141], [21.002, 24.947], [20.654, 24.742], [20.319, 24.527], [19.944, 24.833], [19.565, 25.132], [19.703, 25.506], [19.827, 25.89], [19.939, 26.283], [20.039, 26.683], [20.128, 27.091], [19.838, 27.304], [19.545, 27.514], [19.185, 27.304], [18.835, 27.085], [18.496, 26.857], [18.17, 26.62], [17.856, 26.374], [17.454, 26.642], [17.048, 26.903], [17.149, 27.289], [17.236, 27.683], [17.309, 28.084], [17.37, 28.493], [17.419, 28.907], [17.11, 29.091], [16.799, 29.272], [16.46, 29.028], [16.133, 28.776], [15.818, 28.517], [15.516, 28.25], [15.228, 27.974], [14.801, 28.202], [14.372, 28.423], [14.435, 28.817], [14.483, 29.217], [14.518, 29.624], [14.539, 30.036], [14.548, 30.453], [14.222, 30.607], [13.895, 30.757], [13.582, 30.481], [13.28, 30.199], [12.992, 29.91], [12.717, 29.615], [12.457, 29.313], [12.011, 29.499], [11.562, 29.677], [11.586, 30.075], [11.596, 30.479], [11.591, 30.887], [11.573, 31.299], [11.541, 31.715], [11.202, 31.837], [10.862, 31.954], [10.576, 31.65], [10.303, 31.34], [10.044, 31.025], [9.799, 30.704], [9.569, 30.378], [9.107, 30.52], [8.643, 30.655], [8.63, 31.053], [8.6, 31.456], [8.556, 31.861], [8.498, 32.27], [8.426, 32.681], [8.077, 32.769], [7.727, 32.854], [7.472, 32.523], [7.231, 32.188], [7.003, 31.849], [6.79, 31.507], [6.593, 31.16], [6.119, 31.257], [5.644, 31.346], [5.592, 31.741], [5.524, 32.139], [5.441, 32.538], [5.344, 32.939], [5.232, 33.342], [4.877, 33.396], [4.52, 33.446], [4.298, 33.092], [4.09, 32.735], [3.896, 32.376], [3.717, 32.015], [3.555, 31.651], [3.074, 31.701], [2.593, 31.744], [2.503, 32.133], [2.397, 32.522], [2.275, 32.911], [2.139, 33.301], [1.99, 33.691], [1.631, 33.711], [1.271, 33.726], [1.085, 33.352], [0.912, 32.977], [0.753, 32.601], [0.61, 32.224], [0.483, 31.846], [0, 31.85], [-0.483, 31.846], [-0.61, 32.224], [-0.753, 32.601], [-0.912, 32.977], [-1.085, 33.352], [-1.271, 33.726], [-1.631, 33.711], [-1.99, 33.691], [-2.139, 33.301], [-2.275, 32.911], [-2.397, 32.522], [-2.503, 32.133], [-2.593, 31.744], [-3.074, 31.701], [-3.555, 31.651], [-3.717, 32.015], [-3.896, 32.376], [-4.09, 32.735], [-4.298, 33.092], [-4.52, 33.446], [-4.877, 33.396], [-5.232, 33.342], [-5.344, 32.939], [-5.441, 32.538], [-5.524, 32.139], [-5.592, 31.741], [-5.644, 31.346], [-6.119, 31.257], [-6.593, 31.16], [-6.79, 31.507], [-7.003, 31.849], [-7.231, 32.188], [-7.472, 32.523], [-7.727, 32.854], [-8.077, 32.769], [-8.426, 32.681], [-8.498, 32.27], [-8.556, 31.861], [-8.6, 31.456], [-8.63, 31.053], [-8.643, 30.655], [-9.107, 30.52], [-9.569, 30.378], [-9.799, 30.704], [-10.044, 31.025], [-10.303, 31.34], [-10.576, 31.65], [-10.862, 31.954], [-11.202, 31.837], [-11.541, 31.715], [-11.573, 31.299], [-11.591, 30.887], [-11.596, 30.479], [-11.586, 30.075], [-11.562, 29.677], [-12.011, 29.499], [-12.457, 29.313], [-12.717, 29.615], [-12.992, 29.91], [-13.28, 30.199], [-13.582, 30.481], [-13.895, 30.757], [-14.222, 30.607], [-14.548, 30.453], [-14.539, 30.036], [-14.518, 29.624], [-14.483, 29.217], [-14.435, 28.817], [-14.372, 28.423], [-14.801, 28.202], [-15.228, 27.974], [-15.516, 28.25], [-15.818, 28.517], [-16.133, 28.776], [-16.46, 29.028], [-16.799, 29.272], [-17.11, 29.091], [-17.419, 28.907], [-17.37, 28.493], [-17.309, 28.084], [-17.236, 27.683], [-17.149, 27.289], [-17.048, 26.903], [-17.454, 26.642], [-17.856, 26.374], [-18.17, 26.62], [-18.496, 26.857], [-18.835, 27.085], [-19.185, 27.304], [-19.545, 27.514], [-19.838, 27.304], [-20.128, 27.091], [-20.039, 26.683], [-19.939, 26.283], [-19.827, 25.89], [-19.703, 25.506], [-19.565, 25.132], [-19.944, 24.833], [-20.319, 24.527], [-20.654, 24.742], [-21.002, 24.947], [-21.361, 25.141], [-21.73, 25.325], [-22.11, 25.5], [-22.38, 25.262], [-22.649, 25.022], [-22.521, 24.625], [-22.383, 24.235], [-22.233, 23.856], [-22.072, 23.486], [-21.899, 23.127], [-22.248, 22.792], [-22.591, 22.452], [-22.946, 22.633], [-23.311, 22.803], [-23.688, 22.962], [-24.073, 23.109], [-24.467, 23.247], [-24.714, 22.984], [-24.958, 22.719], [-24.792, 22.336], [-24.617, 21.962], [-24.432, 21.599], [-24.236, 21.246], [-24.029, 20.905], [-24.343, 20.538], [-24.652, 20.166], [-25.023, 20.313], [-25.404, 20.447], [-25.793, 20.568], [-26.191, 20.678], [-26.597, 20.777], [-26.817, 20.492], [-27.034, 20.204], [-26.832, 19.839], [-26.622, 19.484], [-26.402, 19.14], [-26.173, 18.808], [-25.934, 18.488], [-26.212, 18.093], [-26.483, 17.693], [-26.867, 17.803], [-27.258, 17.9], [-27.658, 17.983], [-28.064, 18.054], [-28.478, 18.113], [-28.669, 17.808], [-28.858, 17.501], [-28.622, 17.157], [-28.378, 16.823], [-28.126, 16.502], [-27.866, 16.194], [-27.598, 15.899], [-27.836, 15.479], [-28.067, 15.055], [-28.459, 15.127], [-28.859, 15.185], [-29.264, 15.23], [-29.676, 15.261], [-30.093, 15.28], [-30.254, 14.958], [-30.412, 14.634], [-30.144, 14.314], [-29.869, 14.006], [-29.588, 13.71], [-29.299, 13.429], [-29.003, 13.161], [-29.2, 12.72], [-29.39, 12.275], [-29.787, 12.31], [-30.189, 12.329], [-30.598, 12.334], [-31.01, 12.325], [-31.427, 12.304], [-31.557, 11.968], [-31.683, 11.631], [-31.385, 11.338], [-31.082, 11.058], [-30.773, 10.791], [-30.459, 10.538], [-30.138, 10.301], [-30.291, 9.842], [-30.437, 9.382], [-30.836, 9.377], [-31.238, 9.358], [-31.645, 9.323], [-32.055, 9.275], [-32.468, 9.213], [-32.565, 8.866], [-32.657, 8.518], [-32.333, 8.256], [-32.004, 8.006], [-31.671, 7.771], [-31.333, 7.549], [-30.992, 7.344], [-31.1, 6.873], [-31.2, 6.4], [-31.597, 6.358], [-31.996, 6.299], [-32.397, 6.226], [-32.801, 6.138], [-33.206, 6.037], [-33.268, 5.682], [-33.327, 5.327], [-32.978, 5.097], [-32.627, 4.88], [-32.273, 4.678], [-31.916, 4.49], [-31.556, 4.318], [-31.618, 3.839], [-31.672, 3.359], [-32.063, 3.279], [-32.454, 3.182], [-32.847, 3.07], [-33.24, 2.944], [-33.633, 2.804], [-33.661, 2.445], [-33.685, 2.085], [-33.316, 1.89], [-32.946, 1.708], [-32.574, 1.541], [-32.2, 1.389], [-31.825, 1.253], [-31.841, 0.77], [-31.849, 0.286], [-32.23, 0.169], [-32.61, 0.035], [-32.99, -0.115], [-33.369, -0.278], [-33.747, -0.456], [-33.74, -0.816], [-33.73, -1.175], [-33.343, -1.334], [-32.957, -1.479], [-32.57, -1.61], [-32.184, -1.726], [-31.798, -1.825], [-31.766, -2.307], [-31.728, -2.789], [-32.095, -2.943], [-32.461, -3.113], [-32.825, -3.298], [-33.186, -3.498], [-33.545, -3.71], [-33.504, -4.068], [-33.459, -4.425], [-33.059, -4.546], [-32.66, -4.653], [-32.263, -4.746], [-31.867, -4.824], [-31.473, -4.885], [-31.395, -5.362], [-31.31, -5.838], [-31.662, -6.027], [-32.009, -6.231], [-32.353, -6.451], [-32.694, -6.684], [-33.031, -6.931], [-32.955, -7.283], [-32.875, -7.634], [-32.466, -7.715], [-32.059, -7.784], [-31.654, -7.838], [-31.253, -7.877], [-30.855, -7.9], [-30.731, -8.367], [-30.601, -8.832], [-30.932, -9.054], [-31.258, -9.292], [-31.579, -9.543], [-31.896, -9.808], [-32.208, -10.086], [-32.098, -10.429], [-31.985, -10.771], [-31.57, -10.813], [-31.158, -10.841], [-30.75, -10.856], [-30.346, -10.856], [-29.948, -10.841], [-29.78, -11.294], [-29.605, -11.745], [-29.914, -11.997], [-30.216, -12.265], [-30.511, -12.546], [-30.8, -12.841], [-31.084, -13.148], [-30.942, -13.479], [-30.796, -13.808], [-30.379, -13.809], [-29.966, -13.798], [-29.559, -13.773], [-29.157, -13.735], [-28.762, -13.681], [-28.551, -14.116], [-28.334, -14.547], [-28.616, -14.829], [-28.891, -15.124], [-29.158, -15.433], [-29.417, -15.754], [-29.67, -16.086], [-29.496, -16.402], [-29.32, -16.716], [-28.904, -16.677], [-28.494, -16.626], [-28.091, -16.562], [-27.695, -16.485], [-27.307, -16.393], [-27.056, -16.805], [-26.797, -17.214], [-27.052, -17.521], [-27.296, -17.842], [-27.532, -18.175], [-27.759, -18.519], [-27.979, -18.875], [-27.776, -19.172], [-27.57, -19.467], [-27.16, -19.389], [-26.757, -19.298], [-26.362, -19.196], [-25.975, -19.081], [-25.598, -18.952], [-25.307, -19.338], [-25.011, -19.72], [-25.234, -20.05], [-25.447, -20.393], [-25.649, -20.747], [-25.843, -21.112], [-26.026, -21.487], [-25.796, -21.763], [-25.562, -22.037], [-25.162, -21.919], [-24.769, -21.79], [-24.386, -21.65], [-24.012, -21.498], [-23.649, -21.334], [-23.323, -21.69], [-22.991, -22.042], [-23.181, -22.392], [-23.36, -22.754], [-23.527, -23.126], [-23.684, -23.508], [-23.831, -23.899], [-23.575, -24.151], [-23.316, -24.402], [-22.928, -24.245], [-22.551, -24.079], [-22.183, -23.903], [-21.825, -23.716], [-21.48, -23.517], [-21.12, -23.84], [-20.756, -24.158], [-20.912, -24.525], [-21.055, -24.902], [-21.186, -25.289], [-21.305, -25.684], [-21.413, -26.087], [-21.134, -26.314], [-20.852, -26.538], [-20.481, -26.345], [-20.121, -26.143], [-19.772, -25.932], [-19.434, -25.711], [-19.11, -25.48], [-18.721, -25.767], [-18.328, -26.048], [-18.447, -26.429], [-18.553, -26.818], [-18.646, -27.215], [-18.727, -27.62], [-18.796, -28.032], [-18.495, -28.231], [-18.193, -28.427], [-17.843, -28.199], [-17.504, -27.963], [-17.177, -27.719], [-16.862, -27.467], [-16.561, -27.206], [-16.147, -27.454], [-15.728, -27.696], [-15.81, -28.086], [-15.878, -28.483], [-15.932, -28.888], [-15.973, -29.299], [-16.002, -29.715], [-15.684, -29.884], [-15.365, -30.05], [-15.038, -29.789], [-14.724, -29.522], [-14.421, -29.248], [-14.133, -28.966], [-13.858, -28.677], [-13.422, -28.884], [-12.982, -29.084], [-13.026, -29.48], [-13.055, -29.883], [-13.07, -30.291], [-13.071, -30.703], [-13.06, -31.121], [-12.727, -31.258], [-12.393, -31.392], [-12.093, -31.102], [-11.806, -30.805], [-11.531, -30.503], [-11.271, -30.195], [-11.026, -29.881], [-10.571, -30.044], [-10.114, -30.201], [-10.12, -30.6], [-10.11, -31.003], [-10.085, -31.411], [-10.047, -31.822], [-9.995, -32.236], [-9.651, -32.341], [-9.305, -32.442], [-9.035, -32.124], [-8.777, -31.801], [-8.534, -31.474], [-8.304, -31.142], [-8.091, -30.805], [-7.622, -30.924], [-7.152, -31.037], [-7.119, -31.434], [-7.071, -31.834], [-7.007, -32.237], [-6.929, -32.643], [-6.837, -33.05], [-6.484, -33.121], [-6.13, -33.188], [-5.892, -32.846], [-5.667, -32.5], [-5.456, -32.15], [-5.26, -31.798], [-5.08, -31.442], [-4.602, -31.516], [-4.123, -31.582], [-4.052, -31.974], [-3.965, -32.368], [-3.863, -32.763], [-3.746, -33.159], [-3.616, -33.556], [-3.257, -33.592], [-2.899, -33.625], [-2.695, -33.261], [-2.504, -32.895], [-2.328, -32.527], [-2.166, -32.157], [-2.021, -31.786], [-1.539, -31.813]], 8.1);
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "q2Hohlrad_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Körper Stufe 2";
    const A2 = id + "q2Hohlrad_a2";
    fCylinder(context, A2 + "s0", { "bottomCenter" : vector(0, 21.5, 0) * millimeter, "topCenter" : vector(0, 11.59, 0) * millimeter, "radius" : 37.6 * millimeter });
    prog[] = "Vereinigen Stufe 2";
    unionAll(context, id + "q2Hohlrad_u2", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 3";
    const C3 = id + "q2Hohlrad_c3";
    fCylinder(context, C3 + "o0", { "bottomCenter" : vector(0, 22.5, 0) * millimeter, "topCenter" : vector(0, 11.58, 0) * millimeter, "radius" : 34.4 * millimeter });
    prog[] = "Abziehen Stufe 3";
    cutAll(context, id + "q2Hohlrad_x3", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]), qCreatedBy(C3, EntityType.BODY));
    prog[] = "Körper Stufe 4";
    const A4 = id + "q2Hohlrad_a4";
    fCylinder(context, A4 + "s0", { "bottomCenter" : vector(0, 11.61, 0) * millimeter, "topCenter" : vector(0, 3.5, 0) * millimeter, "radius" : 15 * millimeter });
    fCylinder(context, A4 + "s1", { "bottomCenter" : vector(0, 20.4, 0) * millimeter, "topCenter" : vector(0, 11.6, 0) * millimeter, "radius" : 12 * millimeter });
    prog[] = "Vereinigen Stufe 4";
    unionAll(context, id + "q2Hohlrad_u4", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY), qCreatedBy(A4, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 5";
    const C5 = id + "q2Hohlrad_c5";
    prismOp(context, C5 + "o0", vector(0, 0.5, 0) * millimeter, [[8.75, 6.911], [8.206, 7.549], [7.616, 8.144], [6.983, 8.693], [6.31, 9.193], [5.601, 9.641], [4.861, 10.034], [4.094, 10.371], [3.303, 10.649], [2.494, 10.867], [1.671, 11.024], [0.838, 11.118], [0, 11.15], [-0.838, 11.118], [-1.671, 11.024], [-2.494, 10.867], [-3.303, 10.649], [-4.094, 10.371], [-4.861, 10.034], [-5.601, 9.641], [-6.31, 9.193], [-6.983, 8.693], [-7.616, 8.144], [-8.206, 7.549], [-8.75, 6.911], [-8.75, -6.911], [-8.206, -7.549], [-7.616, -8.144], [-6.983, -8.693], [-6.31, -9.193], [-5.601, -9.641], [-4.861, -10.034], [-4.094, -10.371], [-3.303, -10.649], [-2.494, -10.867], [-1.671, -11.024], [-0.838, -11.118], [0, -11.15], [0.838, -11.118], [1.671, -11.024], [2.494, -10.867], [3.303, -10.649], [4.094, -10.371], [4.861, -10.034], [5.601, -9.641], [6.31, -9.193], [6.983, -8.693], [7.616, -8.144], [8.206, -7.549], [8.75, -6.911]], 8.7);
    coneOp(context, C5 + "o1", vector(0, 13.5, 0) * millimeter, vector(0, 9.19, 0) * millimeter, 7.15, 11.45);
    fCylinder(context, C5 + "o2", { "bottomCenter" : vector(0, 21.4, 0) * millimeter, "topCenter" : vector(0, 12.5, 0) * millimeter, "radius" : 7.25 * millimeter });
    coneOp(context, C5 + "o3", vector(0, 1.3, 0) * millimeter, vector(0, 0.59, 0) * millimeter, 10.95, 11.66);
    fCylinder(context, C5 + "o4", { "bottomCenter" : vector(0, 17, 6.5) * millimeter, "topCenter" : vector(0, 17, 13) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C5 + "o5", { "bottomCenter" : vector(30.012, 19.3, 5.937) * millimeter, "topCenter" : vector(39.251, 19.3, 9.764) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o6", { "bottomCenter" : vector(30.012, 21.3, 5.937) * millimeter, "topCenter" : vector(39.251, 21.3, 9.764) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o7", { "bottomCenter" : vector(30.012, 23.3, 5.937) * millimeter, "topCenter" : vector(39.251, 23.3, 9.764) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o8", { "bottomCenter" : vector(25.42, 19.3, 17.024) * millimeter, "topCenter" : vector(34.659, 19.3, 20.851) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o9", { "bottomCenter" : vector(25.42, 21.3, 17.024) * millimeter, "topCenter" : vector(34.659, 21.3, 20.851) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o10", { "bottomCenter" : vector(25.42, 23.3, 17.024) * millimeter, "topCenter" : vector(34.659, 23.3, 20.851) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o11", { "bottomCenter" : vector(17.024, 19.3, 25.42) * millimeter, "topCenter" : vector(20.851, 19.3, 34.659) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o12", { "bottomCenter" : vector(17.024, 21.3, 25.42) * millimeter, "topCenter" : vector(20.851, 21.3, 34.659) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o13", { "bottomCenter" : vector(17.024, 23.3, 25.42) * millimeter, "topCenter" : vector(20.851, 23.3, 34.659) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o14", { "bottomCenter" : vector(5.937, 19.3, 30.012) * millimeter, "topCenter" : vector(9.764, 19.3, 39.251) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o15", { "bottomCenter" : vector(5.937, 21.3, 30.012) * millimeter, "topCenter" : vector(9.764, 21.3, 39.251) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o16", { "bottomCenter" : vector(5.937, 23.3, 30.012) * millimeter, "topCenter" : vector(9.764, 23.3, 39.251) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o17", { "bottomCenter" : vector(-5.937, 19.3, 30.012) * millimeter, "topCenter" : vector(-9.764, 19.3, 39.251) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o18", { "bottomCenter" : vector(-5.937, 21.3, 30.012) * millimeter, "topCenter" : vector(-9.764, 21.3, 39.251) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o19", { "bottomCenter" : vector(-5.937, 23.3, 30.012) * millimeter, "topCenter" : vector(-9.764, 23.3, 39.251) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o20", { "bottomCenter" : vector(-17.024, 19.3, 25.42) * millimeter, "topCenter" : vector(-20.851, 19.3, 34.659) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o21", { "bottomCenter" : vector(-17.024, 21.3, 25.42) * millimeter, "topCenter" : vector(-20.851, 21.3, 34.659) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o22", { "bottomCenter" : vector(-17.024, 23.3, 25.42) * millimeter, "topCenter" : vector(-20.851, 23.3, 34.659) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o23", { "bottomCenter" : vector(-25.42, 19.3, 17.024) * millimeter, "topCenter" : vector(-34.659, 19.3, 20.851) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o24", { "bottomCenter" : vector(-25.42, 21.3, 17.024) * millimeter, "topCenter" : vector(-34.659, 21.3, 20.851) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o25", { "bottomCenter" : vector(-25.42, 23.3, 17.024) * millimeter, "topCenter" : vector(-34.659, 23.3, 20.851) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o26", { "bottomCenter" : vector(-30.012, 19.3, 5.937) * millimeter, "topCenter" : vector(-39.251, 19.3, 9.764) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o27", { "bottomCenter" : vector(-30.012, 21.3, 5.937) * millimeter, "topCenter" : vector(-39.251, 21.3, 9.764) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o28", { "bottomCenter" : vector(-30.012, 23.3, 5.937) * millimeter, "topCenter" : vector(-39.251, 23.3, 9.764) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o29", { "bottomCenter" : vector(-30.012, 19.3, -5.937) * millimeter, "topCenter" : vector(-39.251, 19.3, -9.764) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o30", { "bottomCenter" : vector(-30.012, 21.3, -5.937) * millimeter, "topCenter" : vector(-39.251, 21.3, -9.764) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o31", { "bottomCenter" : vector(-30.012, 23.3, -5.937) * millimeter, "topCenter" : vector(-39.251, 23.3, -9.764) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o32", { "bottomCenter" : vector(-25.42, 19.3, -17.024) * millimeter, "topCenter" : vector(-34.659, 19.3, -20.851) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o33", { "bottomCenter" : vector(-25.42, 21.3, -17.024) * millimeter, "topCenter" : vector(-34.659, 21.3, -20.851) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o34", { "bottomCenter" : vector(-25.42, 23.3, -17.024) * millimeter, "topCenter" : vector(-34.659, 23.3, -20.851) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o35", { "bottomCenter" : vector(-17.024, 19.3, -25.42) * millimeter, "topCenter" : vector(-20.851, 19.3, -34.659) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o36", { "bottomCenter" : vector(-17.024, 21.3, -25.42) * millimeter, "topCenter" : vector(-20.851, 21.3, -34.659) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o37", { "bottomCenter" : vector(-17.024, 23.3, -25.42) * millimeter, "topCenter" : vector(-20.851, 23.3, -34.659) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o38", { "bottomCenter" : vector(-5.937, 19.3, -30.012) * millimeter, "topCenter" : vector(-9.764, 19.3, -39.251) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o39", { "bottomCenter" : vector(-5.937, 21.3, -30.012) * millimeter, "topCenter" : vector(-9.764, 21.3, -39.251) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o40", { "bottomCenter" : vector(-5.937, 23.3, -30.012) * millimeter, "topCenter" : vector(-9.764, 23.3, -39.251) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o41", { "bottomCenter" : vector(5.937, 19.3, -30.012) * millimeter, "topCenter" : vector(9.764, 19.3, -39.251) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o42", { "bottomCenter" : vector(5.937, 21.3, -30.012) * millimeter, "topCenter" : vector(9.764, 21.3, -39.251) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o43", { "bottomCenter" : vector(5.937, 23.3, -30.012) * millimeter, "topCenter" : vector(9.764, 23.3, -39.251) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o44", { "bottomCenter" : vector(17.024, 19.3, -25.42) * millimeter, "topCenter" : vector(20.851, 19.3, -34.659) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o45", { "bottomCenter" : vector(17.024, 21.3, -25.42) * millimeter, "topCenter" : vector(20.851, 21.3, -34.659) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o46", { "bottomCenter" : vector(17.024, 23.3, -25.42) * millimeter, "topCenter" : vector(20.851, 23.3, -34.659) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o47", { "bottomCenter" : vector(25.42, 19.3, -17.024) * millimeter, "topCenter" : vector(34.659, 19.3, -20.851) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o48", { "bottomCenter" : vector(25.42, 21.3, -17.024) * millimeter, "topCenter" : vector(34.659, 21.3, -20.851) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o49", { "bottomCenter" : vector(25.42, 23.3, -17.024) * millimeter, "topCenter" : vector(34.659, 23.3, -20.851) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o50", { "bottomCenter" : vector(30.012, 19.3, -5.937) * millimeter, "topCenter" : vector(39.251, 19.3, -9.764) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o51", { "bottomCenter" : vector(30.012, 21.3, -5.937) * millimeter, "topCenter" : vector(39.251, 21.3, -9.764) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o52", { "bottomCenter" : vector(30.012, 23.3, -5.937) * millimeter, "topCenter" : vector(39.251, 23.3, -9.764) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C5 + "o53", { "bottomCenter" : vector(0, 17, 33.4) * millimeter, "topCenter" : vector(0, 17, 38.6) * millimeter, "radius" : 3.1 * millimeter });
    prog[] = "Abziehen Stufe 5";
    cutAll(context, id + "q2Hohlrad_x5", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY), qCreatedBy(A4, EntityType.BODY)]), qCreatedBy(C5, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY), qCreatedBy(A4, EntityType.BODY)]), "QV-Getriebe Hohlrad 65 Zaehne innen, Modul 1 (radial M3x8, Kragen mit Steckloechern)", color(0.93, 0.55, 0.2));
}

function q2_q2Ritzel(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "q2Ritzel_a0";
    prismOp(context, A0 + "s0", vector(0, 4.2, -26) * millimeter, [[-0.65, -5.21], [-0.757, -6.061], [-0.738, -6.324], [-0.673, -6.59], [-0.576, -6.859], [-0.45, -7.127], [-0.297, -7.394], [0, -7.4], [0.297, -7.394], [0.45, -7.127], [0.576, -6.859], [0.673, -6.59], [0.738, -6.324], [0.757, -6.061], [0.65, -5.21], [1.256, -5.097], [1.845, -4.915], [2.147, -5.718], [2.285, -5.942], [2.466, -6.149], [2.677, -6.341], [2.914, -6.52], [3.173, -6.685], [3.439, -6.552], [3.699, -6.409], [3.711, -6.102], [3.698, -5.806], [3.659, -5.523], [3.592, -5.256], [3.487, -5.015], [2.997, -4.311], [3.481, -3.93], [3.918, -3.495], [4.558, -4.066], [4.785, -4.199], [5.041, -4.298], [5.318, -4.371], [5.61, -4.419], [5.917, -4.445], [6.09, -4.204], [6.254, -3.956], [6.121, -3.679], [5.972, -3.422], [5.806, -3.19], [5.623, -2.985], [5.418, -2.82], [4.657, -2.424], [4.909, -1.862], [5.093, -1.274], [5.926, -1.482], [6.189, -1.495], [6.461, -1.463], [6.74, -1.399], [7.021, -1.306], [7.304, -1.186], [7.346, -0.892], [7.376, -0.597], [7.13, -0.412], [6.878, -0.255], [6.624, -0.126], [6.366, -0.03], [6.108, 0.021], [5.25, 0.018], [5.212, 0.633], [5.102, 1.239], [5.935, 1.442], [6.174, 1.552], [6.401, 1.707], [6.618, 1.893], [6.824, 2.107], [7.019, 2.345], [6.919, 2.624], [6.808, 2.9], [6.505, 2.948], [6.209, 2.971], [5.923, 2.967], [5.651, 2.932], [5.399, 2.857], [4.64, 2.455], [4.321, 2.982], [3.941, 3.468], [4.585, 4.035], [4.745, 4.244], [4.874, 4.487], [4.98, 4.752], [5.063, 5.037], [5.125, 5.338], [4.907, 5.539], [4.681, 5.731], [4.389, 5.633], [4.117, 5.516], [3.866, 5.38], [3.641, 5.222], [3.453, 5.038], [2.968, 4.331], [2.44, 4.649], [1.878, 4.902], [2.185, 5.704], [2.23, 5.963], [2.231, 6.238], [2.201, 6.522], [2.143, 6.813], [2.058, 7.108], [1.771, 7.185], [1.481, 7.25], [1.269, 7.028], [1.082, 6.798], [0.923, 6.56], [0.797, 6.316], [0.716, 6.066], [0.615, 5.214], [0, 5.25], [-0.615, 5.214], [-0.716, 6.066], [-0.797, 6.316], [-0.923, 6.56], [-1.082, 6.798], [-1.269, 7.028], [-1.481, 7.25], [-1.771, 7.185], [-2.058, 7.108], [-2.143, 6.813], [-2.201, 6.522], [-2.231, 6.238], [-2.23, 5.963], [-2.185, 5.704], [-1.878, 4.902], [-2.44, 4.649], [-2.968, 4.331], [-3.453, 5.038], [-3.641, 5.222], [-3.866, 5.38], [-4.117, 5.516], [-4.389, 5.633], [-4.681, 5.731], [-4.907, 5.539], [-5.125, 5.338], [-5.063, 5.037], [-4.98, 4.752], [-4.874, 4.487], [-4.745, 4.244], [-4.585, 4.035], [-3.941, 3.468], [-4.321, 2.982], [-4.64, 2.455], [-5.399, 2.857], [-5.651, 2.932], [-5.923, 2.967], [-6.209, 2.971], [-6.505, 2.948], [-6.808, 2.9], [-6.919, 2.624], [-7.019, 2.345], [-6.824, 2.107], [-6.618, 1.893], [-6.401, 1.707], [-6.174, 1.552], [-5.935, 1.442], [-5.102, 1.239], [-5.212, 0.633], [-5.25, 0.018], [-6.108, 0.021], [-6.366, -0.03], [-6.624, -0.126], [-6.878, -0.255], [-7.13, -0.412], [-7.376, -0.597], [-7.346, -0.892], [-7.304, -1.186], [-7.021, -1.306], [-6.74, -1.399], [-6.461, -1.463], [-6.189, -1.495], [-5.926, -1.482], [-5.093, -1.274], [-4.909, -1.862], [-4.657, -2.424], [-5.418, -2.82], [-5.623, -2.985], [-5.806, -3.19], [-5.972, -3.422], [-6.121, -3.679], [-6.254, -3.956], [-6.09, -4.204], [-5.917, -4.445], [-5.61, -4.419], [-5.318, -4.371], [-5.041, -4.298], [-4.785, -4.199], [-4.558, -4.066], [-3.918, -3.495], [-3.481, -3.93], [-2.997, -4.311], [-3.487, -5.015], [-3.592, -5.256], [-3.659, -5.523], [-3.698, -5.806], [-3.711, -6.102], [-3.699, -6.409], [-3.439, -6.552], [-3.173, -6.685], [-2.914, -6.52], [-2.677, -6.341], [-2.466, -6.149], [-2.285, -5.942], [-2.147, -5.718], [-1.845, -4.915], [-1.256, -5.097]], 19.3);
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "q2Ritzel_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "q2Ritzel_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-1.95, 14.1, -26) * millimeter, "topCenter" : vector(-8.7, 14.1, -26) * millimeter, "radius" : 2.1 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-7.1, 14.1, -26) * millimeter, "topCenter" : vector(-8.7, 14.1, -26) * millimeter, "radius" : 2.5 * millimeter });
    prismOp(context, C1 + "o2", vector(0, 3.2, -26) * millimeter, [[-2.25, -1.581], [-2.071, -1.809], [-1.869, -2.017], [-1.647, -2.202], [-1.406, -2.363], [-1.15, -2.498], [-0.881, -2.605], [-0.602, -2.683], [-0.316, -2.732], [-0.027, -2.75], [0.262, -2.737], [0.549, -2.695], [0.829, -2.622], [1.1, -2.52], [1.359, -2.39], [1.603, -2.234], [1.829, -2.053], [2.035, -1.849], [2.218, -1.625], [2.377, -1.383], [2.509, -1.125], [2.614, -0.855], [2.689, -0.575], [2.735, -0.289], [2.75, 0], [2.735, 0.289], [2.689, 0.575], [2.614, 0.855], [2.509, 1.125], [2.377, 1.383], [2.218, 1.625], [2.035, 1.849], [1.829, 2.053], [1.603, 2.234], [1.359, 2.39], [1.1, 2.52], [0.829, 2.622], [0.549, 2.695], [0.262, 2.737], [-0.027, 2.75], [-0.316, 2.732], [-0.602, 2.683], [-0.881, 2.605], [-1.15, 2.498], [-1.406, 2.363], [-1.647, 2.202], [-1.869, 2.017], [-2.071, 1.809], [-2.25, 1.581]], 12.81);
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(0, 24.5, -26) * millimeter, "topCenter" : vector(0, 16, -26) * millimeter, "radius" : 2.9 * millimeter });
    coneOp(context, C1 + "o4", vector(0, 23.51, -26) * millimeter, vector(0, 22.9, -26) * millimeter, 3.5, 2.9);
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "q2Ritzel_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Körper Stufe 2";
    const A2 = id + "q2Ritzel_a2";
    fCuboid(context, A2 + "s0", { "corner1" : vector(-3.05, 14.8, -29.05) * millimeter, "corner2" : vector(-1.8, 15, -22.95) * millimeter });
    fCuboid(context, A2 + "s1", { "corner1" : vector(-3.05, 9.5, -29.05) * millimeter, "corner2" : vector(-1.8, 9.7, -22.95) * millimeter });
    prog[] = "Vereinigen Stufe 2";
    unionAll(context, id + "q2Ritzel_u2", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]), "QV-Getriebe Ritzel 13 Zaehne, Modul 1 (NEMA-Welle, 2 Quetschstege, M5-Madenschraube, v2 Zaehne durchgehend, langer Schaft)", color(0.93, 0.55, 0.2));
}

function q2_q2Getriebeplatte(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "q2Getriebeplatte_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-45, 24, -48.5) * millimeter, "corner2" : vector(45, 28, 20) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(0, 24.01, 0) * millimeter, "topCenter" : vector(0, 21, 0) * millimeter, "radius" : 11 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-40, 24.01, -15) * millimeter, "topCenter" : vector(-40, 0, -15) * millimeter, "radius" : 4.5 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(-40, 24.01, 15) * millimeter, "topCenter" : vector(-40, 0, 15) * millimeter, "radius" : 4.5 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(40, 24.01, -15) * millimeter, "topCenter" : vector(40, 0, -15) * millimeter, "radius" : 4.5 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(40, 24.01, 15) * millimeter, "topCenter" : vector(40, 0, 15) * millimeter, "radius" : 4.5 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "q2Getriebeplatte_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "q2Getriebeplatte_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(0, 29, 0) * millimeter, "topCenter" : vector(0, 20, 0) * millimeter, "radius" : 7.25 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(0, 29, -26) * millimeter, "topCenter" : vector(0, 23, -26) * millimeter, "radius" : 11.2 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(-15.5, 29, -41.5) * millimeter, "topCenter" : vector(-15.5, 23, -41.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(-40, 14, -15) * millimeter, "topCenter" : vector(-40, -1, -15) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(-15.5, 29, -10.5) * millimeter, "topCenter" : vector(-15.5, 23, -10.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(-40, 14, 15) * millimeter, "topCenter" : vector(-40, -1, 15) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(15.5, 29, -41.5) * millimeter, "topCenter" : vector(15.5, 23, -41.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(40, 14, -15) * millimeter, "topCenter" : vector(40, -1, -15) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(15.5, 29, -10.5) * millimeter, "topCenter" : vector(15.5, 23, -10.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(40, 14, 15) * millimeter, "topCenter" : vector(40, -1, 15) * millimeter, "radius" : 1.3 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "q2Getriebeplatte_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "QV-Getriebe Getriebeplatte (Motor, Wellenlager, 4 Pfosten an den Rahmen)", color(0.93, 0.55, 0.2));
}

function q2_q2L6203H(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "q2L6203H_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(3.5, -9, 0) * millimeter, "topCenter" : vector(3.5, -21, 0) * millimeter, "radius" : 20 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "q2L6203H_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "q2L6203H_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(3.5, -8, 0) * millimeter, "topCenter" : vector(3.5, -22, 0) * millimeter, "radius" : 8.5 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "q2L6203H_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Lager 6203 hinten (Dummy)", color(0.65, 0.67, 0.7));
}

function q2_q2L6203V(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "q2L6203V_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(3.5, -21, 0) * millimeter, "topCenter" : vector(3.5, -33, 0) * millimeter, "radius" : 20 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "q2L6203V_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "q2L6203V_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(3.5, -20, 0) * millimeter, "topCenter" : vector(3.5, -34, 0) * millimeter, "radius" : 8.5 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "q2L6203V_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Lager 6203 vorn (Dummy)", color(0.65, 0.67, 0.7));
}

function q2_q2L6200(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "q2L6200_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(0, -38.2, 0) * millimeter, "topCenter" : vector(0, -47.2, 0) * millimeter, "radius" : 15 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "q2L6200_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "q2L6200_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(0, -37.2, 0) * millimeter, "topCenter" : vector(0, -48.2, 0) * millimeter, "radius" : 5 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "q2L6200_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Lager 6200 (Dummy)", color(0.65, 0.67, 0.7));
}

function q2_q2M8(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "q2M8_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(0, 12, 0) * millimeter, "topCenter" : vector(0, -47.26, 0) * millimeter, "radius" : 4 * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(0, 22.75, 0) * millimeter, "topCenter" : vector(0, 11.99, 0) * millimeter, "radius" : 3.5 * millimeter });
    prismOp(context, A0 + "s2", vector(0, -52.55, 0) * millimeter, [[7.5, 0], [3.75, 6.495], [-3.75, 6.495], [-7.5, 0], [-3.75, -6.495], [3.75, -6.495]], 5.3);
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "q2M8_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "M8 x 70 DIN 933 (Stahlkern, Dummy)", color(0.65, 0.67, 0.7));
}

function q2_q2Nema(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "q2Nema_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-21.15, 28, -47.15) * millimeter, "corner2" : vector(21.15, 68, -4.85) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-8, 56, -57.15) * millimeter, "corner2" : vector(8, 68, -47.05) * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(0, 28.1, -26) * millimeter, "topCenter" : vector(0, 26, -26) * millimeter, "radius" : 11 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(0, 28, -26) * millimeter, "topCenter" : vector(0, 4, -26) * millimeter, "radius" : 2.5 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "q2Nema_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "NEMA 17 (Dummy, mit Stecker und Kabelbogen nach unten)", color(0.3, 0.3, 0.32));
}

annotation { "Feature Type Name" : "Quetschventil Gegenlager 6200 (Rahmen v2)" }
export const quetschventilGegenlager6200 = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
    }
    {
        const prog = new box("");
        var failed = [];
        var okCount = 0;
        try
        {
            q2_q2Platte(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "q2Platte @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            q2_q2AmbossO(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "q2AmbossO @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            q2_q2AmbossU(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "q2AmbossU @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            q2_q2Steg(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "q2Steg @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            q2_q2Welle(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "q2Welle @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            q2_q2Ring(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "q2Ring @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            q2_q2Huelse(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "q2Huelse @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            q2_q2Hohlrad(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "q2Hohlrad @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            q2_q2Ritzel(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "q2Ritzel @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            q2_q2Getriebeplatte(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "q2Getriebeplatte @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            q2_q2L6203H(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "q2L6203H @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            q2_q2L6203V(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "q2L6203V @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            q2_q2L6200(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "q2L6200 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            q2_q2M8(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "q2M8 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            q2_q2Nema(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "q2Nema @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        if (size(failed) > 0)
        {
            var msg = "OK: " ~ toString(okCount) ~ " von 15 Teilen. Fehler: ";
            for (var s in failed)
                msg = msg ~ s ~ " || ";
            reportFeatureWarning(context, id, msg);
        }
    });

