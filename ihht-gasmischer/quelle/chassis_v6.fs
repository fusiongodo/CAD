FeatureScript 3112;
import(path : "onshape/std/geometry.fs", version : "3112.0");

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

function c6_frame6(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "frame6_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-62, -168, 0) * millimeter, "corner2" : vector(62, 82, 2) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-68, -162, 0) * millimeter, "corner2" : vector(68, 76, 2) * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-62, -162, 0) * millimeter, "topCenter" : vector(-62, -162, 2) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(62, -162, 0) * millimeter, "topCenter" : vector(62, -162, 2) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(-62, 76, 0) * millimeter, "topCenter" : vector(-62, 76, 2) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(62, 76, 0) * millimeter, "topCenter" : vector(62, 76, 2) * millimeter, "radius" : 6 * millimeter });
    fCuboid(context, A0 + "s6", { "corner1" : vector(-68, -162, 1.5) * millimeter, "corner2" : vector(-65.95, 76, 15) * millimeter });
    fCuboid(context, A0 + "s7", { "corner1" : vector(-68, -90, 1.5) * millimeter, "corner2" : vector(-65.95, -54, 35) * millimeter });
    fCuboid(context, A0 + "s8", { "corner1" : vector(-68, -25, 1.5) * millimeter, "corner2" : vector(-65.95, 30, 145) * millimeter });
    fCuboid(context, A0 + "s9", { "corner1" : vector(-68, 38, 1.5) * millimeter, "corner2" : vector(-65.95, 76, 111) * millimeter });
    fCuboid(context, A0 + "s10", { "corner1" : vector(-68, -162, 1.5) * millimeter, "corner2" : vector(-65.95, 76, 10) * millimeter });
    fCuboid(context, A0 + "s11", { "corner1" : vector(-68, 28, 106) * millimeter, "corner2" : vector(-65.95, 40, 112) * millimeter });
    fCuboid(context, A0 + "s12", { "corner1" : vector(65.95, -162, 1.5) * millimeter, "corner2" : vector(68, 76, 15) * millimeter });
    fCuboid(context, A0 + "s13", { "corner1" : vector(65.95, -90, 1.5) * millimeter, "corner2" : vector(68, -54, 35) * millimeter });
    fCuboid(context, A0 + "s14", { "corner1" : vector(65.95, -25, 1.5) * millimeter, "corner2" : vector(68, 30, 145) * millimeter });
    fCuboid(context, A0 + "s15", { "corner1" : vector(65.95, 38, 1.5) * millimeter, "corner2" : vector(68, 76, 111) * millimeter });
    fCuboid(context, A0 + "s16", { "corner1" : vector(65.95, -162, 1.5) * millimeter, "corner2" : vector(68, 76, 10) * millimeter });
    fCuboid(context, A0 + "s17", { "corner1" : vector(65.95, 28, 106) * millimeter, "corner2" : vector(68, 40, 112) * millimeter });
    fCuboid(context, A0 + "s18", { "corner1" : vector(-62, -168, 1.5) * millimeter, "corner2" : vector(62, -166, 10) * millimeter });
    fCuboid(context, A0 + "s19", { "corner1" : vector(-62, 80, 1.5) * millimeter, "corner2" : vector(62, 82, 10) * millimeter });
    fCuboid(context, A0 + "s20", { "corner1" : vector(-66, 0, 1.5) * millimeter, "corner2" : vector(66, 2, 145) * millimeter });
    fCuboid(context, A0 + "s21", { "corner1" : vector(-34.65, 1.5, 1.5) * millimeter, "corner2" : vector(-32.65, 20, 140) * millimeter });
    fCuboid(context, A0 + "s22", { "corner1" : vector(-34.65, 19, 106) * millimeter, "corner2" : vector(-32.65, 26.6, 112) * millimeter });
    fCuboid(context, A0 + "s23", { "corner1" : vector(-1, 1.5, 1.5) * millimeter, "corner2" : vector(1, 20, 140) * millimeter });
    fCuboid(context, A0 + "s24", { "corner1" : vector(-1, 19, 106) * millimeter, "corner2" : vector(1, 26.6, 112) * millimeter });
    fCuboid(context, A0 + "s25", { "corner1" : vector(32.65, 1.5, 1.5) * millimeter, "corner2" : vector(34.65, 20, 140) * millimeter });
    fCuboid(context, A0 + "s26", { "corner1" : vector(32.65, 19, 106) * millimeter, "corner2" : vector(34.65, 26.6, 112) * millimeter });
    fCuboid(context, A0 + "s27", { "corner1" : vector(-66.5, -86, 1.5) * millimeter, "corner2" : vector(-53, -56, 9) * millimeter });
    fCuboid(context, A0 + "s28", { "corner1" : vector(53, -86, 1.5) * millimeter, "corner2" : vector(66.5, -56, 9) * millimeter });
    fCuboid(context, A0 + "s29", { "corner1" : vector(-66.5, 27, 1.5) * millimeter, "corner2" : vector(-58, 35, 112) * millimeter });
    fCuboid(context, A0 + "s30", { "corner1" : vector(-66.5, 71, 1.5) * millimeter, "corner2" : vector(-58, 79, 112) * millimeter });
    fCuboid(context, A0 + "s31", { "corner1" : vector(58, 27, 1.5) * millimeter, "corner2" : vector(66.5, 35, 112) * millimeter });
    fCuboid(context, A0 + "s32", { "corner1" : vector(58, 71, 1.5) * millimeter, "corner2" : vector(66.5, 79, 112) * millimeter });
    fCuboid(context, A0 + "s33", { "corner1" : vector(-59, -167, 1.5) * millimeter, "corner2" : vector(-51, -93, 17) * millimeter });
    fCuboid(context, A0 + "s34", { "corner1" : vector(51, -167, 1.5) * millimeter, "corner2" : vector(59, -93, 17) * millimeter });
    fCuboid(context, A0 + "s35", { "corner1" : vector(-61.65, 35, 1.5) * millimeter, "corner2" : vector(-53.65, 71, 17) * millimeter });
    fCuboid(context, A0 + "s36", { "corner1" : vector(-13.65, 35, 1.5) * millimeter, "corner2" : vector(-5.65, 71, 17) * millimeter });
    fCuboid(context, A0 + "s37", { "corner1" : vector(5.65, 35, 1.5) * millimeter, "corner2" : vector(13.65, 71, 17) * millimeter });
    fCuboid(context, A0 + "s38", { "corner1" : vector(53.65, 35, 1.5) * millimeter, "corner2" : vector(61.65, 71, 17) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "frame6_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "frame6_c1";
    fCuboid(context, C1 + "o0", { "corner1" : vector(-49, -153, -1) * millimeter, "corner2" : vector(-6, -117, 2.4) * millimeter });
    fCuboid(context, C1 + "o1", { "corner1" : vector(6, -153, -1) * millimeter, "corner2" : vector(49, -117, 2.4) * millimeter });
    fCuboid(context, C1 + "o2", { "corner1" : vector(-49, -110, -1) * millimeter, "corner2" : vector(-6, -74, 2.4) * millimeter });
    fCuboid(context, C1 + "o3", { "corner1" : vector(6, -110, -1) * millimeter, "corner2" : vector(49, -74, 2.4) * millimeter });
    fCuboid(context, C1 + "o4", { "corner1" : vector(-49, -67, -1) * millimeter, "corner2" : vector(-6, -30, 2.4) * millimeter });
    fCuboid(context, C1 + "o5", { "corner1" : vector(6, -67, -1) * millimeter, "corner2" : vector(49, -30, 2.4) * millimeter });
    fCuboid(context, C1 + "o6", { "corner1" : vector(-53, 20, -1) * millimeter, "corner2" : vector(-6, 33, 2.4) * millimeter });
    fCuboid(context, C1 + "o7", { "corner1" : vector(6, 20, -1) * millimeter, "corner2" : vector(53, 33, 2.4) * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(-44.65, -1, 35) * millimeter, "topCenter" : vector(-44.65, 4, 35) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(-44.65, -1, 131) * millimeter, "topCenter" : vector(-44.65, 4, 131) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o10", { "bottomCenter" : vector(-22.65, -1, 35) * millimeter, "topCenter" : vector(-22.65, 4, 35) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o11", { "bottomCenter" : vector(-22.65, -1, 131) * millimeter, "topCenter" : vector(-22.65, 4, 131) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o12", { "bottomCenter" : vector(22.65, -1, 35) * millimeter, "topCenter" : vector(22.65, 4, 35) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o13", { "bottomCenter" : vector(22.65, -1, 131) * millimeter, "topCenter" : vector(22.65, 4, 131) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o14", { "bottomCenter" : vector(44.65, -1, 35) * millimeter, "topCenter" : vector(44.65, 4, 35) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o15", { "bottomCenter" : vector(44.65, -1, 131) * millimeter, "topCenter" : vector(44.65, 4, 131) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o16", { "bottomCenter" : vector(-59.4, -71, 1.5) * millimeter, "topCenter" : vector(-59.4, -71, 9.5) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o17", { "bottomCenter" : vector(59.4, -71, 1.5) * millimeter, "topCenter" : vector(59.4, -71, 9.5) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o18", { "bottomCenter" : vector(-61, 31, 100.5) * millimeter, "topCenter" : vector(-61, 31, 113) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o19", { "bottomCenter" : vector(-61, 75, 100.5) * millimeter, "topCenter" : vector(-61, 75, 113) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o20", { "bottomCenter" : vector(61, 31, 100.5) * millimeter, "topCenter" : vector(61, 31, 113) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o21", { "bottomCenter" : vector(61, 75, 100.5) * millimeter, "topCenter" : vector(61, 75, 113) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o22", { "bottomCenter" : vector(-57.65, 53, 6.5) * millimeter, "topCenter" : vector(-57.65, 53, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o23", { "bottomCenter" : vector(-9.65, 53, 6.5) * millimeter, "topCenter" : vector(-9.65, 53, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o24", { "bottomCenter" : vector(9.65, 53, 6.5) * millimeter, "topCenter" : vector(9.65, 53, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o25", { "bottomCenter" : vector(57.65, 53, 6.5) * millimeter, "topCenter" : vector(57.65, 53, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o26", { "bottomCenter" : vector(-55, -162, 6.5) * millimeter, "topCenter" : vector(-55, -162, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o27", { "bottomCenter" : vector(-55, -154, 6.5) * millimeter, "topCenter" : vector(-55, -154, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o28", { "bottomCenter" : vector(-55, -146, 6.5) * millimeter, "topCenter" : vector(-55, -146, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o29", { "bottomCenter" : vector(-55, -138, 6.5) * millimeter, "topCenter" : vector(-55, -138, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o30", { "bottomCenter" : vector(-55, -130, 6.5) * millimeter, "topCenter" : vector(-55, -130, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o31", { "bottomCenter" : vector(-55, -122, 6.5) * millimeter, "topCenter" : vector(-55, -122, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o32", { "bottomCenter" : vector(-55, -114, 6.5) * millimeter, "topCenter" : vector(-55, -114, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o33", { "bottomCenter" : vector(-55, -106, 6.5) * millimeter, "topCenter" : vector(-55, -106, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o34", { "bottomCenter" : vector(-55, -98, 6.5) * millimeter, "topCenter" : vector(-55, -98, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o35", { "bottomCenter" : vector(55, -162, 6.5) * millimeter, "topCenter" : vector(55, -162, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o36", { "bottomCenter" : vector(55, -154, 6.5) * millimeter, "topCenter" : vector(55, -154, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o37", { "bottomCenter" : vector(55, -146, 6.5) * millimeter, "topCenter" : vector(55, -146, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o38", { "bottomCenter" : vector(55, -138, 6.5) * millimeter, "topCenter" : vector(55, -138, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o39", { "bottomCenter" : vector(55, -130, 6.5) * millimeter, "topCenter" : vector(55, -130, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o40", { "bottomCenter" : vector(55, -122, 6.5) * millimeter, "topCenter" : vector(55, -122, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o41", { "bottomCenter" : vector(55, -114, 6.5) * millimeter, "topCenter" : vector(55, -114, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o42", { "bottomCenter" : vector(55, -106, 6.5) * millimeter, "topCenter" : vector(55, -106, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o43", { "bottomCenter" : vector(55, -98, 6.5) * millimeter, "topCenter" : vector(55, -98, 18) * millimeter, "radius" : 1.3 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "frame6_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Rahmen v6 (2 mm, 2 NEMA, 250 mm)", color(0.87, 0.81, 0.69));
}

function c6_shelf6(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "shelf6_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-65.65, 27, 112) * millimeter, "corner2" : vector(65.65, 79, 115) * millimeter });
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
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-61, 31, 111) * millimeter, "topCenter" : vector(-61, 31, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-61, 75, 111) * millimeter, "topCenter" : vector(-61, 75, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(61, 31, 111) * millimeter, "topCenter" : vector(61, 31, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(61, 75, 111) * millimeter, "topCenter" : vector(61, 75, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o4", { "corner1" : vector(-53, 34, 111) * millimeter, "corner2" : vector(-43.65, 72, 115.4) * millimeter });
    fCuboid(context, C1 + "o5", { "corner1" : vector(-28.65, 34, 111) * millimeter, "corner2" : vector(28.65, 72, 115.4) * millimeter });
    fCuboid(context, C1 + "o6", { "corner1" : vector(43.65, 34, 111) * millimeter, "corner2" : vector(53, 72, 115.4) * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(-35.65, 51, 109.5) * millimeter, "topCenter" : vector(-35.65, 51, 121) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(35.65, 51, 109.5) * millimeter, "topCenter" : vector(35.65, 51, 121) * millimeter, "radius" : 1.3 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "shelf6_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Regal oben v6 (2 mm Chassis, verstärkt, Säulen-Fassungen geschlossen)", color(0.87, 0.81, 0.69));
}

function c6_bridge6(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "bridge6_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-65.7, -87, 9) * millimeter, "corner2" : vector(65.7, -85, 54) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-65.7, -87, 9) * millimeter, "corner2" : vector(-53.1, -55, 14.5) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(-65.7, -87, 9) * millimeter, "corner2" : vector(-63.2, -55, 26.5) * millimeter });
    fCuboid(context, A0 + "s3", { "corner1" : vector(53.1, -87, 9) * millimeter, "corner2" : vector(65.7, -55, 14.5) * millimeter });
    fCuboid(context, A0 + "s4", { "corner1" : vector(63.2, -87, 9) * millimeter, "corner2" : vector(65.7, -55, 26.5) * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(-38.15, -85.1, 19.5) * millimeter, "topCenter" : vector(-38.15, -84, 19.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(-38.15, -85.1, 50.5) * millimeter, "topCenter" : vector(-38.15, -84, 50.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(-7.15, -85.1, 19.5) * millimeter, "topCenter" : vector(-7.15, -84, 19.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(-7.15, -85.1, 50.5) * millimeter, "topCenter" : vector(-7.15, -84, 50.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s9", { "bottomCenter" : vector(7.15, -85.1, 19.5) * millimeter, "topCenter" : vector(7.15, -84, 19.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(7.15, -85.1, 50.5) * millimeter, "topCenter" : vector(7.15, -84, 50.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s11", { "bottomCenter" : vector(38.15, -85.1, 19.5) * millimeter, "topCenter" : vector(38.15, -84, 19.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(38.15, -85.1, 50.5) * millimeter, "topCenter" : vector(38.15, -84, 50.5) * millimeter, "radius" : 3.75 * millimeter });
    fCuboid(context, A0 + "s13", { "corner1" : vector(-65.7, -85.1, 9) * millimeter, "corner2" : vector(-63.7, -83, 54) * millimeter });
    fCuboid(context, A0 + "s14", { "corner1" : vector(-53, -85.1, 9) * millimeter, "corner2" : vector(-51, -83, 54) * millimeter });
    fCuboid(context, A0 + "s15", { "corner1" : vector(-1, -85.1, 9) * millimeter, "corner2" : vector(1, -83, 54) * millimeter });
    fCuboid(context, A0 + "s16", { "corner1" : vector(51, -85.1, 9) * millimeter, "corner2" : vector(53, -83, 54) * millimeter });
    fCuboid(context, A0 + "s17", { "corner1" : vector(63.7, -85.1, 9) * millimeter, "corner2" : vector(65.7, -83, 54) * millimeter });
    fCylinder(context, A0 + "s18", { "bottomCenter" : vector(-44.65, -85.1, 35) * millimeter, "topCenter" : vector(-44.65, -70, 35) * millimeter, "radius" : 3 * millimeter });
    coneOp(context, A0 + "s19", vector(-44.65, -85.01, 35) * millimeter, vector(-44.65, -83.5, 35) * millimeter, 4.5, 3);
    fCuboid(context, A0 + "s20", { "corner1" : vector(-45.65, -85.1, 9) * millimeter, "corner2" : vector(-43.65, -83.5, 54) * millimeter });
    fCylinder(context, A0 + "s21", { "bottomCenter" : vector(44.65, -85.1, 35) * millimeter, "topCenter" : vector(44.65, -70, 35) * millimeter, "radius" : 3 * millimeter });
    coneOp(context, A0 + "s22", vector(44.65, -85.01, 35) * millimeter, vector(44.65, -83.5, 35) * millimeter, 4.5, 3);
    fCuboid(context, A0 + "s23", { "corner1" : vector(43.65, -85.1, 9) * millimeter, "corner2" : vector(45.65, -83.5, 54) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "bridge6_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "bridge6_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-22.65, -88, 35) * millimeter, "topCenter" : vector(-22.65, -84, 35) * millimeter, "radius" : 12 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-38.15, -88, 19.5) * millimeter, "topCenter" : vector(-38.15, -83, 19.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(-38.15, -88, 50.5) * millimeter, "topCenter" : vector(-38.15, -83, 50.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(-7.15, -88, 19.5) * millimeter, "topCenter" : vector(-7.15, -83, 19.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(-7.15, -88, 50.5) * millimeter, "topCenter" : vector(-7.15, -83, 50.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(22.65, -88, 35) * millimeter, "topCenter" : vector(22.65, -84, 35) * millimeter, "radius" : 12 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(7.15, -88, 19.5) * millimeter, "topCenter" : vector(7.15, -83, 19.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(7.15, -88, 50.5) * millimeter, "topCenter" : vector(7.15, -83, 50.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(38.15, -88, 19.5) * millimeter, "topCenter" : vector(38.15, -83, 19.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(38.15, -88, 50.5) * millimeter, "topCenter" : vector(38.15, -83, 50.5) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o10", { "corner1" : vector(-61.1, -81, 8) * millimeter, "corner2" : vector(-57.7, -61, 15.5) * millimeter });
    fCylinder(context, C1 + "o11", { "bottomCenter" : vector(-59.4, -81, 8) * millimeter, "topCenter" : vector(-59.4, -81, 15.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o12", { "bottomCenter" : vector(-59.4, -61, 8) * millimeter, "topCenter" : vector(-59.4, -61, 15.5) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o13", { "corner1" : vector(57.7, -81, 8) * millimeter, "corner2" : vector(61.1, -61, 15.5) * millimeter });
    fCylinder(context, C1 + "o14", { "bottomCenter" : vector(59.4, -81, 8) * millimeter, "topCenter" : vector(59.4, -81, 15.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o15", { "bottomCenter" : vector(59.4, -61, 8) * millimeter, "topCenter" : vector(59.4, -61, 15.5) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "bridge6_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Motorbrücke v6 (2 mm, 2 NEMA + 2 Lagerzapfen 16 mm)", color(0.87, 0.81, 0.69));
}

function c6_portal6(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "portal6_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-61, -139, 58) * millimeter, "corner2" : vector(61, -136, 108) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-61, -139, 17) * millimeter, "corner2" : vector(-49, -136, 58.1) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(-61, -139, 17) * millimeter, "corner2" : vector(-49, -105, 20) * millimeter });
    prismXOp(context, A0 + "s3", vector(-50.5, -136.1, 19.9) * millimeter, [[0, 0], [31, 0], [0, 30]], 1.5);
    prismXOp(context, A0 + "s4", vector(-61, -136.1, 19.9) * millimeter, [[0, 0], [31, 0], [0, 30]], 1.5);
    fCuboid(context, A0 + "s5", { "corner1" : vector(49, -139, 17) * millimeter, "corner2" : vector(61, -136, 58.1) * millimeter });
    fCuboid(context, A0 + "s6", { "corner1" : vector(49, -139, 17) * millimeter, "corner2" : vector(61, -105, 20) * millimeter });
    prismXOp(context, A0 + "s7", vector(49, -136.1, 19.9) * millimeter, [[0, 0], [31, 0], [0, 30]], 1.5);
    prismXOp(context, A0 + "s8", vector(59.5, -136.1, 19.9) * millimeter, [[0, 0], [31, 0], [0, 30]], 1.5);
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "portal6_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "portal6_c1";
    fCuboid(context, C1 + "o0", { "corner1" : vector(-15.5, -140, 61) * millimeter, "corner2" : vector(15.5, -135, 105) * millimeter });
    fCuboid(context, C1 + "o1", { "corner1" : vector(-45, -140, 64) * millimeter, "corner2" : vector(-29, -135, 102) * millimeter });
    fCuboid(context, C1 + "o2", { "corner1" : vector(-21.7, -140, 71) * millimeter, "corner2" : vector(-18.3, -135, 95) * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(-20, -140, 71) * millimeter, "topCenter" : vector(-20, -135, 71) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(-20, -140, 95) * millimeter, "topCenter" : vector(-20, -135, 95) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o5", { "corner1" : vector(-56.7, -132, 16) * millimeter, "corner2" : vector(-53.3, -109, 21) * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(-55, -132, 16) * millimeter, "topCenter" : vector(-55, -132, 21) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(-55, -109, 16) * millimeter, "topCenter" : vector(-55, -109, 21) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o8", { "corner1" : vector(29, -140, 64) * millimeter, "corner2" : vector(45, -135, 102) * millimeter });
    fCuboid(context, C1 + "o9", { "corner1" : vector(18.3, -140, 71) * millimeter, "corner2" : vector(21.7, -135, 95) * millimeter });
    fCylinder(context, C1 + "o10", { "bottomCenter" : vector(20, -140, 71) * millimeter, "topCenter" : vector(20, -135, 71) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o11", { "bottomCenter" : vector(20, -140, 95) * millimeter, "topCenter" : vector(20, -135, 95) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o12", { "corner1" : vector(53.3, -132, 16) * millimeter, "corner2" : vector(56.7, -109, 21) * millimeter });
    fCylinder(context, C1 + "o13", { "bottomCenter" : vector(55, -132, 16) * millimeter, "topCenter" : vector(55, -132, 21) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o14", { "bottomCenter" : vector(55, -109, 16) * millimeter, "topCenter" : vector(55, -109, 21) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "portal6_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Kameraportal v6 (Y-verstellbar, Halter ±12 mm in Z)", color(0.87, 0.81, 0.69));
}

function c6_camMount6(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "camMount6_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-25, -136, 70.5) * millimeter, "corner2" : vector(25, -134, 95.5) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(-10.5, -134.5, 76.5) * millimeter, "topCenter" : vector(-10.5, -128, 76.5) * millimeter, "radius" : 5 * millimeter });
    coneOp(context, A0 + "s2", vector(-10.5, -134.01, 76.5) * millimeter, vector(-10.5, -133.2, 76.5) * millimeter, 5.8, 5);
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(-10.5, -134.5, 89.5) * millimeter, "topCenter" : vector(-10.5, -128, 89.5) * millimeter, "radius" : 5 * millimeter });
    coneOp(context, A0 + "s4", vector(-10.5, -134.01, 89.5) * millimeter, vector(-10.5, -133.2, 89.5) * millimeter, 5.8, 5);
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(10.5, -134.5, 76.5) * millimeter, "topCenter" : vector(10.5, -128, 76.5) * millimeter, "radius" : 5 * millimeter });
    coneOp(context, A0 + "s6", vector(10.5, -134.01, 76.5) * millimeter, vector(10.5, -133.2, 76.5) * millimeter, 5.8, 5);
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(10.5, -134.5, 89.5) * millimeter, "topCenter" : vector(10.5, -128, 89.5) * millimeter, "radius" : 5 * millimeter });
    coneOp(context, A0 + "s8", vector(10.5, -134.01, 89.5) * millimeter, vector(10.5, -133.2, 89.5) * millimeter, 5.8, 5);
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "camMount6_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "camMount6_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-20, -137, 83) * millimeter, "topCenter" : vector(-20, -133, 83) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-10.5, -137, 76.5) * millimeter, "topCenter" : vector(-10.5, -126, 76.5) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(-10.5, -137, 89.5) * millimeter, "topCenter" : vector(-10.5, -126, 89.5) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(20, -137, 83) * millimeter, "topCenter" : vector(20, -133, 83) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(10.5, -137, 76.5) * millimeter, "topCenter" : vector(10.5, -126, 76.5) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(10.5, -137, 89.5) * millimeter, "topCenter" : vector(10.5, -126, 89.5) * millimeter, "radius" : 1.5 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "camMount6_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Kamerahalter v6 (nur Kamera, 2 mm, M2 durch + Mutter)", color(0.22, 0.22, 0.24));
}

annotation { "Feature Type Name" : "Chassis v6 (2 mm)" }
export const chassisV6 = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
    }
    {
        const prog = new box("");
        var failed = [];
        var okCount = 0;
        try
        {
            c6_frame6(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "frame6 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            c6_shelf6(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "shelf6 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            c6_bridge6(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "bridge6 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            c6_portal6(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "portal6 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            c6_camMount6(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "camMount6 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        if (size(failed) > 0)
        {
            var msg = "OK: " ~ toString(okCount) ~ " von 5 Teilen. Fehler: ";
            for (var s in failed)
                msg = msg ~ s ~ " || ";
            reportFeatureWarning(context, id, msg);
        }
    });

