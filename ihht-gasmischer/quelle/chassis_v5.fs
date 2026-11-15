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

function dev_frame5(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "frame5_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-61, -168, 0) * millimeter, "corner2" : vector(61, 82, 1) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-67, -162, 0) * millimeter, "corner2" : vector(67, 76, 1) * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-61, -162, 0) * millimeter, "topCenter" : vector(-61, -162, 1) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(61, -162, 0) * millimeter, "topCenter" : vector(61, -162, 1) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(-61, 76, 0) * millimeter, "topCenter" : vector(-61, 76, 1) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(61, 76, 0) * millimeter, "topCenter" : vector(61, 76, 1) * millimeter, "radius" : 6 * millimeter });
    fCuboid(context, A0 + "s6", { "corner1" : vector(-67, -162, 0.5) * millimeter, "corner2" : vector(-65.95, 76, 15) * millimeter });
    fCuboid(context, A0 + "s7", { "corner1" : vector(-67, -90, 0.5) * millimeter, "corner2" : vector(-65.95, -54, 35) * millimeter });
    fCuboid(context, A0 + "s8", { "corner1" : vector(-67, -25, 0.5) * millimeter, "corner2" : vector(-65.95, 30, 145) * millimeter });
    fCuboid(context, A0 + "s9", { "corner1" : vector(-67, 38, 0.5) * millimeter, "corner2" : vector(-65.95, 76, 111) * millimeter });
    fCuboid(context, A0 + "s10", { "corner1" : vector(-67, -162, 0.5) * millimeter, "corner2" : vector(-65.95, 76, 10) * millimeter });
    fCuboid(context, A0 + "s11", { "corner1" : vector(-67, 28, 106) * millimeter, "corner2" : vector(-65.95, 40, 112) * millimeter });
    fCuboid(context, A0 + "s12", { "corner1" : vector(65.95, -162, 0.5) * millimeter, "corner2" : vector(67, 76, 15) * millimeter });
    fCuboid(context, A0 + "s13", { "corner1" : vector(65.95, -90, 0.5) * millimeter, "corner2" : vector(67, -54, 35) * millimeter });
    fCuboid(context, A0 + "s14", { "corner1" : vector(65.95, -25, 0.5) * millimeter, "corner2" : vector(67, 30, 145) * millimeter });
    fCuboid(context, A0 + "s15", { "corner1" : vector(65.95, 38, 0.5) * millimeter, "corner2" : vector(67, 76, 111) * millimeter });
    fCuboid(context, A0 + "s16", { "corner1" : vector(65.95, -162, 0.5) * millimeter, "corner2" : vector(67, 76, 10) * millimeter });
    fCuboid(context, A0 + "s17", { "corner1" : vector(65.95, 28, 106) * millimeter, "corner2" : vector(67, 40, 112) * millimeter });
    fCuboid(context, A0 + "s18", { "corner1" : vector(-61, -168, 0.5) * millimeter, "corner2" : vector(61, -167, 10) * millimeter });
    fCuboid(context, A0 + "s19", { "corner1" : vector(-61, 81, 0.5) * millimeter, "corner2" : vector(61, 82, 10) * millimeter });
    fCuboid(context, A0 + "s20", { "corner1" : vector(-66, 0, 0.5) * millimeter, "corner2" : vector(66, 1, 145) * millimeter });
    fCuboid(context, A0 + "s21", { "corner1" : vector(-34.15, 0.5, 0.5) * millimeter, "corner2" : vector(-33.15, 20, 140) * millimeter });
    fCuboid(context, A0 + "s22", { "corner1" : vector(-34.15, 19, 106) * millimeter, "corner2" : vector(-33.15, 26.6, 112) * millimeter });
    fCuboid(context, A0 + "s23", { "corner1" : vector(-0.5, 0.5, 0.5) * millimeter, "corner2" : vector(0.5, 20, 140) * millimeter });
    fCuboid(context, A0 + "s24", { "corner1" : vector(-0.5, 19, 106) * millimeter, "corner2" : vector(0.5, 26.6, 112) * millimeter });
    fCuboid(context, A0 + "s25", { "corner1" : vector(33.15, 0.5, 0.5) * millimeter, "corner2" : vector(34.15, 20, 140) * millimeter });
    fCuboid(context, A0 + "s26", { "corner1" : vector(33.15, 19, 106) * millimeter, "corner2" : vector(34.15, 26.6, 112) * millimeter });
    fCuboid(context, A0 + "s27", { "corner1" : vector(-66.5, -86, 0.5) * millimeter, "corner2" : vector(-53, -56, 9) * millimeter });
    fCuboid(context, A0 + "s28", { "corner1" : vector(53, -86, 0.5) * millimeter, "corner2" : vector(66.5, -56, 9) * millimeter });
    fCuboid(context, A0 + "s29", { "corner1" : vector(-66.5, 27, 0.5) * millimeter, "corner2" : vector(-58, 35, 112) * millimeter });
    fCuboid(context, A0 + "s30", { "corner1" : vector(-66.5, 71, 0.5) * millimeter, "corner2" : vector(-58, 79, 112) * millimeter });
    fCuboid(context, A0 + "s31", { "corner1" : vector(58, 27, 0.5) * millimeter, "corner2" : vector(66.5, 35, 112) * millimeter });
    fCuboid(context, A0 + "s32", { "corner1" : vector(58, 71, 0.5) * millimeter, "corner2" : vector(66.5, 79, 112) * millimeter });
    fCuboid(context, A0 + "s33", { "corner1" : vector(-59, -167, 0.5) * millimeter, "corner2" : vector(-51, -93, 17) * millimeter });
    fCuboid(context, A0 + "s34", { "corner1" : vector(51, -167, 0.5) * millimeter, "corner2" : vector(59, -93, 17) * millimeter });
    fCuboid(context, A0 + "s35", { "corner1" : vector(-61.65, 35, 0.5) * millimeter, "corner2" : vector(-53.65, 71, 17) * millimeter });
    fCuboid(context, A0 + "s36", { "corner1" : vector(-13.65, 35, 0.5) * millimeter, "corner2" : vector(-5.65, 71, 17) * millimeter });
    fCuboid(context, A0 + "s37", { "corner1" : vector(5.65, 35, 0.5) * millimeter, "corner2" : vector(13.65, 71, 17) * millimeter });
    fCuboid(context, A0 + "s38", { "corner1" : vector(53.65, 35, 0.5) * millimeter, "corner2" : vector(61.65, 71, 17) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "frame5_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "frame5_c1";
    fCuboid(context, C1 + "o0", { "corner1" : vector(-49, -153, -1) * millimeter, "corner2" : vector(-6, -117, 1.4) * millimeter });
    fCuboid(context, C1 + "o1", { "corner1" : vector(6, -153, -1) * millimeter, "corner2" : vector(49, -117, 1.4) * millimeter });
    fCuboid(context, C1 + "o2", { "corner1" : vector(-49, -110, -1) * millimeter, "corner2" : vector(-6, -74, 1.4) * millimeter });
    fCuboid(context, C1 + "o3", { "corner1" : vector(6, -110, -1) * millimeter, "corner2" : vector(49, -74, 1.4) * millimeter });
    fCuboid(context, C1 + "o4", { "corner1" : vector(-49, -67, -1) * millimeter, "corner2" : vector(-6, -30, 1.4) * millimeter });
    fCuboid(context, C1 + "o5", { "corner1" : vector(6, -67, -1) * millimeter, "corner2" : vector(49, -30, 1.4) * millimeter });
    fCuboid(context, C1 + "o6", { "corner1" : vector(-52, 20, -1) * millimeter, "corner2" : vector(-6, 33, 1.4) * millimeter });
    fCuboid(context, C1 + "o7", { "corner1" : vector(6, 20, -1) * millimeter, "corner2" : vector(52, 33, 1.4) * millimeter });
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
    fCylinder(context, C1 + "o18", { "bottomCenter" : vector(-62, 31, 99.5) * millimeter, "topCenter" : vector(-62, 31, 113) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o19", { "bottomCenter" : vector(-62, 75, 99.5) * millimeter, "topCenter" : vector(-62, 75, 113) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o20", { "bottomCenter" : vector(62, 31, 99.5) * millimeter, "topCenter" : vector(62, 31, 113) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o21", { "bottomCenter" : vector(62, 75, 99.5) * millimeter, "topCenter" : vector(62, 75, 113) * millimeter, "radius" : 1.3 * millimeter });
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
    cutAll(context, id + "frame5_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Rahmen v5 (Entwicklung, 2 NEMA, 250 mm)", color(0.87, 0.81, 0.69));
}

function dev_shelf5(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "shelf5_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-63.85, 27, 112) * millimeter, "corner2" : vector(63.85, 79, 113) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-53, 27, 106) * millimeter, "corner2" : vector(53, 28, 112.5) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(-53, 78, 106) * millimeter, "corner2" : vector(53, 79, 112.5) * millimeter });
    fCuboid(context, A0 + "s3", { "corner1" : vector(-39.65, 33, 112.5) * millimeter, "corner2" : vector(-31.65, 69, 118) * millimeter });
    fCuboid(context, A0 + "s4", { "corner1" : vector(31.65, 33, 112.5) * millimeter, "corner2" : vector(39.65, 69, 118) * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(-35.65, 51, 106) * millimeter, "topCenter" : vector(-35.65, 51, 112.5) * millimeter, "radius" : 4 * millimeter });
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(35.65, 51, 106) * millimeter, "topCenter" : vector(35.65, 51, 112.5) * millimeter, "radius" : 4 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "shelf5_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "shelf5_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-62, 31, 111) * millimeter, "topCenter" : vector(-62, 31, 114) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-62, 75, 111) * millimeter, "topCenter" : vector(-62, 75, 114) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(62, 31, 111) * millimeter, "topCenter" : vector(62, 31, 114) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(62, 75, 111) * millimeter, "topCenter" : vector(62, 75, 114) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o4", { "corner1" : vector(-55, 34, 111) * millimeter, "corner2" : vector(-43.65, 72, 113.4) * millimeter });
    fCuboid(context, C1 + "o5", { "corner1" : vector(-28.65, 34, 111) * millimeter, "corner2" : vector(28.65, 72, 113.4) * millimeter });
    fCuboid(context, C1 + "o6", { "corner1" : vector(43.65, 34, 111) * millimeter, "corner2" : vector(55, 72, 113.4) * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(-35.65, 51, 107.5) * millimeter, "topCenter" : vector(-35.65, 51, 119) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(35.65, 51, 107.5) * millimeter, "topCenter" : vector(35.65, 51, 119) * millimeter, "radius" : 1.3 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "shelf5_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Regal oben v5 (nur Sammler Mix)", color(0.87, 0.81, 0.69));
}

function dev_splHi(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "splHi_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-52.65, 47, 17) * millimeter, "corner2" : vector(-14.65, 59, 43.5) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-62.65, 39, 17) * millimeter, "corner2" : vector(-4.65, 67, 20) * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-44.65, 47.5, 35) * millimeter, "topCenter" : vector(-44.65, 35, 35) * millimeter, "radius" : 5.375 * millimeter });
    coneOp(context, A0 + "s3", vector(-44.65, 35.3, 35) * millimeter, vector(-44.65, 37.1, 35) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(-44.65, 37.1, 35) * millimeter, "topCenter" : vector(-44.65, 37.4, 35) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s5", vector(-44.65, 37.7, 35) * millimeter, vector(-44.65, 39.5, 35) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(-44.65, 39.5, 35) * millimeter, "topCenter" : vector(-44.65, 39.8, 35) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s7", vector(-44.65, 40.1, 35) * millimeter, vector(-44.65, 41.9, 35) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(-44.65, 41.9, 35) * millimeter, "topCenter" : vector(-44.65, 42.2, 35) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s9", vector(-44.65, 47.8, 35) * millimeter, vector(-44.65, 45.3, 35) * millimeter, 7.375, 5.375);
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(-22.65, 47.5, 35) * millimeter, "topCenter" : vector(-22.65, 35, 35) * millimeter, "radius" : 5.375 * millimeter });
    coneOp(context, A0 + "s11", vector(-22.65, 35.3, 35) * millimeter, vector(-22.65, 37.1, 35) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(-22.65, 37.1, 35) * millimeter, "topCenter" : vector(-22.65, 37.4, 35) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s13", vector(-22.65, 37.7, 35) * millimeter, vector(-22.65, 39.5, 35) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(-22.65, 39.5, 35) * millimeter, "topCenter" : vector(-22.65, 39.8, 35) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s15", vector(-22.65, 40.1, 35) * millimeter, vector(-22.65, 41.9, 35) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s16", { "bottomCenter" : vector(-22.65, 41.9, 35) * millimeter, "topCenter" : vector(-22.65, 42.2, 35) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s17", vector(-22.65, 47.8, 35) * millimeter, vector(-22.65, 45.3, 35) * millimeter, 7.375, 5.375);
    fCylinder(context, A0 + "s18", { "bottomCenter" : vector(-33.65, 58.5, 35) * millimeter, "topCenter" : vector(-33.65, 71, 35) * millimeter, "radius" : 3.5 * millimeter });
    fCylinder(context, A0 + "s19", { "bottomCenter" : vector(-33.65, 61, 35) * millimeter, "topCenter" : vector(-33.65, 61.5, 35) * millimeter, "radius" : 3.75 * millimeter });
    coneOp(context, A0 + "s20", vector(-33.65, 61.5, 35) * millimeter, vector(-33.65, 63.5, 35) * millimeter, 3.75, 3.5);
    fCylinder(context, A0 + "s21", { "bottomCenter" : vector(-33.65, 63.5, 35) * millimeter, "topCenter" : vector(-33.65, 64, 35) * millimeter, "radius" : 3.75 * millimeter });
    coneOp(context, A0 + "s22", vector(-33.65, 64, 35) * millimeter, vector(-33.65, 66, 35) * millimeter, 3.75, 3.5);
    fCylinder(context, A0 + "s23", { "bottomCenter" : vector(-33.65, 66, 35) * millimeter, "topCenter" : vector(-33.65, 66.5, 35) * millimeter, "radius" : 3.75 * millimeter });
    coneOp(context, A0 + "s24", vector(-33.65, 66.5, 35) * millimeter, vector(-33.65, 68.5, 35) * millimeter, 3.75, 3.5);
    fCylinder(context, A0 + "s25", { "bottomCenter" : vector(-33.65, 68.5, 35) * millimeter, "topCenter" : vector(-33.65, 69, 35) * millimeter, "radius" : 3.75 * millimeter });
    coneOp(context, A0 + "s26", vector(-33.65, 69, 35) * millimeter, vector(-33.65, 71, 35) * millimeter, 3.75, 3.5);
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "splHi_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "splHi_c1";
    tearOp(context, C1 + "o0", vector(-48.15, 53, 35) * millimeter, vector(-19.15, 53, 35) * millimeter, 4);
    tearOp(context, C1 + "o1", vector(-44.65, 34, 35) * millimeter, vector(-44.65, 53, 35) * millimeter, 3.25);
    tearOp(context, C1 + "o2", vector(-22.65, 34, 35) * millimeter, vector(-22.65, 53, 35) * millimeter, 3.25);
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(-33.65, 52.5, 35) * millimeter, "topCenter" : vector(-33.65, 72, 35) * millimeter, "radius" : 2.6 * millimeter });
    fCuboid(context, C1 + "o4", { "corner1" : vector(-59.35, 43, 16) * millimeter, "corner2" : vector(-55.95, 63, 21) * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(-57.65, 43, 16) * millimeter, "topCenter" : vector(-57.65, 43, 21) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(-57.65, 63, 16) * millimeter, "topCenter" : vector(-57.65, 63, 21) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o7", { "corner1" : vector(-11.35, 43, 16) * millimeter, "corner2" : vector(-7.95, 63, 21) * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(-9.65, 43, 16) * millimeter, "topCenter" : vector(-9.65, 43, 21) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(-9.65, 63, 16) * millimeter, "topCenter" : vector(-9.65, 63, 21) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "splHi_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Splitter hi v4 (Eingang hyperoxisch, links, Tüllen 22 mm)", color(0.85, 0.35, 0.3));
}

function dev_splLo(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "splLo_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(14.65, 47, 17) * millimeter, "corner2" : vector(52.65, 59, 54.7) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(4.65, 39, 17) * millimeter, "corner2" : vector(62.65, 67, 20) * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(22.65, 47.5, 35) * millimeter, "topCenter" : vector(22.65, 35, 35) * millimeter, "radius" : 5.375 * millimeter });
    coneOp(context, A0 + "s3", vector(22.65, 35.3, 35) * millimeter, vector(22.65, 37.1, 35) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(22.65, 37.1, 35) * millimeter, "topCenter" : vector(22.65, 37.4, 35) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s5", vector(22.65, 37.7, 35) * millimeter, vector(22.65, 39.5, 35) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(22.65, 39.5, 35) * millimeter, "topCenter" : vector(22.65, 39.8, 35) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s7", vector(22.65, 40.1, 35) * millimeter, vector(22.65, 41.9, 35) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(22.65, 41.9, 35) * millimeter, "topCenter" : vector(22.65, 42.2, 35) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s9", vector(22.65, 47.8, 35) * millimeter, vector(22.65, 45.3, 35) * millimeter, 7.375, 5.375);
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(44.65, 47.5, 35) * millimeter, "topCenter" : vector(44.65, 35, 35) * millimeter, "radius" : 5.375 * millimeter });
    coneOp(context, A0 + "s11", vector(44.65, 35.3, 35) * millimeter, vector(44.65, 37.1, 35) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(44.65, 37.1, 35) * millimeter, "topCenter" : vector(44.65, 37.4, 35) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s13", vector(44.65, 37.7, 35) * millimeter, vector(44.65, 39.5, 35) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(44.65, 39.5, 35) * millimeter, "topCenter" : vector(44.65, 39.8, 35) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s15", vector(44.65, 40.1, 35) * millimeter, vector(44.65, 41.9, 35) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s16", { "bottomCenter" : vector(44.65, 41.9, 35) * millimeter, "topCenter" : vector(44.65, 42.2, 35) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s17", vector(44.65, 47.8, 35) * millimeter, vector(44.65, 45.3, 35) * millimeter, 7.375, 5.375);
    fCylinder(context, A0 + "s18", { "bottomCenter" : vector(33.65, 58.5, 40) * millimeter, "topCenter" : vector(33.65, 75.2, 40) * millimeter, "radius" : 11.4 * millimeter });
    fCylinder(context, A0 + "s19", { "bottomCenter" : vector(33.65, 62, 40) * millimeter, "topCenter" : vector(33.65, 62.8, 40) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s20", vector(33.65, 62.8, 40) * millimeter, vector(33.65, 65.3, 40) * millimeter, 12.2, 11.4);
    fCylinder(context, A0 + "s21", { "bottomCenter" : vector(33.65, 65.3, 40) * millimeter, "topCenter" : vector(33.65, 66.1, 40) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s22", vector(33.65, 66.1, 40) * millimeter, vector(33.65, 68.6, 40) * millimeter, 12.2, 11.4);
    fCylinder(context, A0 + "s23", { "bottomCenter" : vector(33.65, 68.6, 40) * millimeter, "topCenter" : vector(33.65, 69.4, 40) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s24", vector(33.65, 69.4, 40) * millimeter, vector(33.65, 71.9, 40) * millimeter, 12.2, 11.4);
    fCylinder(context, A0 + "s25", { "bottomCenter" : vector(33.65, 71.9, 40) * millimeter, "topCenter" : vector(33.65, 72.7, 40) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s26", vector(33.65, 72.7, 40) * millimeter, vector(33.65, 75.2, 40) * millimeter, 12.2, 11.4);
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "splLo_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "splLo_c1";
    tearOp(context, C1 + "o0", vector(19.15, 53, 35) * millimeter, vector(48.15, 53, 35) * millimeter, 4);
    tearOp(context, C1 + "o1", vector(22.65, 34, 35) * millimeter, vector(22.65, 53, 35) * millimeter, 3.25);
    tearOp(context, C1 + "o2", vector(44.65, 34, 35) * millimeter, vector(44.65, 53, 35) * millimeter, 3.25);
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(33.65, 51, 40) * millimeter, "topCenter" : vector(33.65, 76.2, 40) * millimeter, "radius" : 10.2 * millimeter });
    fCuboid(context, C1 + "o4", { "corner1" : vector(7.95, 43, 16) * millimeter, "corner2" : vector(11.35, 63, 21) * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(9.65, 43, 16) * millimeter, "topCenter" : vector(9.65, 43, 21) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(9.65, 63, 16) * millimeter, "topCenter" : vector(9.65, 63, 21) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o7", { "corner1" : vector(55.95, 43, 16) * millimeter, "corner2" : vector(59.35, 63, 21) * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(57.65, 43, 16) * millimeter, "topCenter" : vector(57.65, 43, 21) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(57.65, 63, 16) * millimeter, "topCenter" : vector(57.65, 63, 21) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "splLo_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Splitter lo v4 (Eingang hypoxisch, rechts, Tüllen 22 mm)", color(0.35, 0.55, 0.85));
}

function dev_splMix(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "splMix_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-30.65, 45, 118) * millimeter, "corner2" : vector(30.65, 57, 152.7) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-40.65, 37, 118) * millimeter, "corner2" : vector(40.65, 65, 121) * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-22.65, 45.5, 131) * millimeter, "topCenter" : vector(-22.65, 35, 131) * millimeter, "radius" : 3.5 * millimeter });
    coneOp(context, A0 + "s3", vector(-22.65, 35.3, 131) * millimeter, vector(-22.65, 37.1, 131) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(-22.65, 37.1, 131) * millimeter, "topCenter" : vector(-22.65, 37.4, 131) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s5", vector(-22.65, 37.7, 131) * millimeter, vector(-22.65, 39.5, 131) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(-22.65, 39.5, 131) * millimeter, "topCenter" : vector(-22.65, 39.8, 131) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s7", vector(-22.65, 40.1, 131) * millimeter, vector(-22.65, 41.9, 131) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(-22.65, 41.9, 131) * millimeter, "topCenter" : vector(-22.65, 42.2, 131) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s9", vector(-22.65, 45.8, 131) * millimeter, vector(-22.65, 43.3, 131) * millimeter, 5.5, 3.5);
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(22.65, 45.5, 131) * millimeter, "topCenter" : vector(22.65, 35, 131) * millimeter, "radius" : 3.5 * millimeter });
    coneOp(context, A0 + "s11", vector(22.65, 35.3, 131) * millimeter, vector(22.65, 37.1, 131) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(22.65, 37.1, 131) * millimeter, "topCenter" : vector(22.65, 37.4, 131) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s13", vector(22.65, 37.7, 131) * millimeter, vector(22.65, 39.5, 131) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(22.65, 39.5, 131) * millimeter, "topCenter" : vector(22.65, 39.8, 131) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s15", vector(22.65, 40.1, 131) * millimeter, vector(22.65, 41.9, 131) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s16", { "bottomCenter" : vector(22.65, 41.9, 131) * millimeter, "topCenter" : vector(22.65, 42.2, 131) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s17", vector(22.65, 45.8, 131) * millimeter, vector(22.65, 43.3, 131) * millimeter, 5.5, 3.5);
    fCylinder(context, A0 + "s18", { "bottomCenter" : vector(0, 56.5, 138) * millimeter, "topCenter" : vector(0, 73.2, 138) * millimeter, "radius" : 11.4 * millimeter });
    fCylinder(context, A0 + "s19", { "bottomCenter" : vector(0, 60, 138) * millimeter, "topCenter" : vector(0, 60.8, 138) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s20", vector(0, 60.8, 138) * millimeter, vector(0, 63.3, 138) * millimeter, 12.2, 11.4);
    fCylinder(context, A0 + "s21", { "bottomCenter" : vector(0, 63.3, 138) * millimeter, "topCenter" : vector(0, 64.1, 138) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s22", vector(0, 64.1, 138) * millimeter, vector(0, 66.6, 138) * millimeter, 12.2, 11.4);
    fCylinder(context, A0 + "s23", { "bottomCenter" : vector(0, 66.6, 138) * millimeter, "topCenter" : vector(0, 67.4, 138) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s24", vector(0, 67.4, 138) * millimeter, vector(0, 69.9, 138) * millimeter, 12.2, 11.4);
    fCylinder(context, A0 + "s25", { "bottomCenter" : vector(0, 69.9, 138) * millimeter, "topCenter" : vector(0, 70.7, 138) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s26", vector(0, 70.7, 138) * millimeter, vector(0, 73.2, 138) * millimeter, 12.2, 11.4);
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "splMix_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "splMix_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-22.65, 51, 131) * millimeter, "topCenter" : vector(22.65, 51, 131) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-22.65, 34, 131) * millimeter, "topCenter" : vector(-22.65, 51.5, 131) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(22.65, 34, 131) * millimeter, "topCenter" : vector(22.65, 51.5, 131) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(0, 49, 138) * millimeter, "topCenter" : vector(0, 74.2, 138) * millimeter, "radius" : 10.2 * millimeter });
    fCuboid(context, C1 + "o4", { "corner1" : vector(-37.35, 41, 117) * millimeter, "corner2" : vector(-33.95, 61, 122) * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(-35.65, 41, 117) * millimeter, "topCenter" : vector(-35.65, 41, 122) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(-35.65, 61, 117) * millimeter, "topCenter" : vector(-35.65, 61, 122) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o7", { "corner1" : vector(33.95, 41, 117) * millimeter, "corner2" : vector(37.35, 61, 122) * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(35.65, 41, 117) * millimeter, "topCenter" : vector(35.65, 41, 122) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(35.65, 61, 117) * millimeter, "topCenter" : vector(35.65, 61, 122) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "splMix_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Sammler Mix (Ausgang)", color(0.6, 0.4, 0.8));
}

function dev_bridge5(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "bridge5_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-65.7, -87, 9) * millimeter, "corner2" : vector(65.7, -86, 54) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-65.7, -87, 9) * millimeter, "corner2" : vector(-53.1, -55, 14.5) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(-65.7, -87, 9) * millimeter, "corner2" : vector(-63.2, -55, 26.5) * millimeter });
    fCuboid(context, A0 + "s3", { "corner1" : vector(53.1, -87, 9) * millimeter, "corner2" : vector(65.7, -55, 14.5) * millimeter });
    fCuboid(context, A0 + "s4", { "corner1" : vector(63.2, -87, 9) * millimeter, "corner2" : vector(65.7, -55, 26.5) * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(-38.15, -86.1, 19.5) * millimeter, "topCenter" : vector(-38.15, -84, 19.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(-38.15, -86.1, 50.5) * millimeter, "topCenter" : vector(-38.15, -84, 50.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(-7.15, -86.1, 19.5) * millimeter, "topCenter" : vector(-7.15, -84, 19.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(-7.15, -86.1, 50.5) * millimeter, "topCenter" : vector(-7.15, -84, 50.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s9", { "bottomCenter" : vector(7.15, -86.1, 19.5) * millimeter, "topCenter" : vector(7.15, -84, 19.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(7.15, -86.1, 50.5) * millimeter, "topCenter" : vector(7.15, -84, 50.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s11", { "bottomCenter" : vector(38.15, -86.1, 19.5) * millimeter, "topCenter" : vector(38.15, -84, 19.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(38.15, -86.1, 50.5) * millimeter, "topCenter" : vector(38.15, -84, 50.5) * millimeter, "radius" : 3.75 * millimeter });
    fCuboid(context, A0 + "s13", { "corner1" : vector(-65.2, -86.1, 9) * millimeter, "corner2" : vector(-64.2, -83, 54) * millimeter });
    fCuboid(context, A0 + "s14", { "corner1" : vector(-52.5, -86.1, 9) * millimeter, "corner2" : vector(-51.5, -83, 54) * millimeter });
    fCuboid(context, A0 + "s15", { "corner1" : vector(-0.5, -86.1, 9) * millimeter, "corner2" : vector(0.5, -83, 54) * millimeter });
    fCuboid(context, A0 + "s16", { "corner1" : vector(51.5, -86.1, 9) * millimeter, "corner2" : vector(52.5, -83, 54) * millimeter });
    fCuboid(context, A0 + "s17", { "corner1" : vector(64.2, -86.1, 9) * millimeter, "corner2" : vector(65.2, -83, 54) * millimeter });
    fCylinder(context, A0 + "s18", { "bottomCenter" : vector(-44.65, -86.1, 35) * millimeter, "topCenter" : vector(-44.65, -70, 35) * millimeter, "radius" : 3 * millimeter });
    coneOp(context, A0 + "s19", vector(-44.65, -86.01, 35) * millimeter, vector(-44.65, -84.5, 35) * millimeter, 4.5, 3);
    fCuboid(context, A0 + "s20", { "corner1" : vector(-45.15, -86.1, 9) * millimeter, "corner2" : vector(-44.15, -84.5, 54) * millimeter });
    fCylinder(context, A0 + "s21", { "bottomCenter" : vector(44.65, -86.1, 35) * millimeter, "topCenter" : vector(44.65, -70, 35) * millimeter, "radius" : 3 * millimeter });
    coneOp(context, A0 + "s22", vector(44.65, -86.01, 35) * millimeter, vector(44.65, -84.5, 35) * millimeter, 4.5, 3);
    fCuboid(context, A0 + "s23", { "corner1" : vector(44.15, -86.1, 9) * millimeter, "corner2" : vector(45.15, -84.5, 54) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "bridge5_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "bridge5_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-22.65, -88, 35) * millimeter, "topCenter" : vector(-22.65, -85, 35) * millimeter, "radius" : 12 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-38.15, -88, 19.5) * millimeter, "topCenter" : vector(-38.15, -83, 19.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(-38.15, -88, 50.5) * millimeter, "topCenter" : vector(-38.15, -83, 50.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(-7.15, -88, 19.5) * millimeter, "topCenter" : vector(-7.15, -83, 19.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(-7.15, -88, 50.5) * millimeter, "topCenter" : vector(-7.15, -83, 50.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(22.65, -88, 35) * millimeter, "topCenter" : vector(22.65, -85, 35) * millimeter, "radius" : 12 * millimeter });
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
    cutAll(context, id + "bridge5_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Motorbrücke v5 (2 NEMA + 2 Lagerzapfen 16 mm)", color(0.87, 0.81, 0.69));
}

function dev_gearDrive1(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "gearDrive1_a0";
    prismOp(context, A0 + "s0", vector(-22.65, -82, 35) * millimeter, [[9.225, -1.016], [10.275, -1.131], [10.619, -1.11], [10.967, -1.037], [11.318, -0.927], [11.669, -0.785], [12.02, -0.614], [12.368, -0.415], [12.375, 0], [12.368, 0.415], [12.02, 0.614], [11.669, 0.785], [11.318, 0.927], [10.967, 1.037], [10.619, 1.11], [10.275, 1.131], [9.225, 1.016], [9.103, 1.811], [8.912, 2.592], [9.925, 2.887], [10.235, 3.038], [10.529, 3.239], [10.811, 3.475], [11.081, 3.74], [11.34, 4.032], [11.586, 4.349], [11.433, 4.736], [11.268, 5.117], [10.87, 5.167], [10.48, 5.191], [10.102, 5.187], [9.736, 5.155], [9.386, 5.089], [9.059, 4.977], [8.134, 4.469], [7.717, 5.156], [7.242, 5.805], [8.065, 6.465], [8.293, 6.724], [8.488, 7.022], [8.658, 7.348], [8.806, 7.696], [8.933, 8.065], [9.039, 8.452], [8.75, 8.75], [8.452, 9.039], [8.065, 8.933], [7.696, 8.806], [7.348, 8.658], [7.022, 8.488], [6.724, 8.293], [6.465, 8.065], [5.805, 7.242], [5.156, 7.717], [4.469, 8.134], [4.977, 9.059], [5.089, 9.386], [5.155, 9.736], [5.187, 10.102], [5.191, 10.48], [5.167, 10.87], [5.117, 11.268], [4.736, 11.433], [4.349, 11.586], [4.032, 11.34], [3.74, 11.081], [3.475, 10.811], [3.239, 10.529], [3.038, 10.235], [2.887, 9.925], [2.592, 8.912], [1.811, 9.103], [1.016, 9.225], [1.131, 10.275], [1.11, 10.619], [1.037, 10.967], [0.927, 11.318], [0.785, 11.669], [0.614, 12.02], [0.415, 12.368], [0, 12.375], [-0.415, 12.368], [-0.614, 12.02], [-0.785, 11.669], [-0.927, 11.318], [-1.037, 10.967], [-1.11, 10.619], [-1.131, 10.275], [-1.016, 9.225], [-1.811, 9.103], [-2.592, 8.912], [-2.887, 9.925], [-3.038, 10.235], [-3.239, 10.529], [-3.475, 10.811], [-3.74, 11.081], [-4.032, 11.34], [-4.349, 11.586], [-4.736, 11.433], [-5.117, 11.268], [-5.167, 10.87], [-5.191, 10.48], [-5.187, 10.102], [-5.155, 9.736], [-5.089, 9.386], [-4.977, 9.059], [-4.469, 8.134], [-5.156, 7.717], [-5.805, 7.242], [-6.465, 8.065], [-6.724, 8.293], [-7.022, 8.488], [-7.348, 8.658], [-7.696, 8.806], [-8.065, 8.933], [-8.452, 9.039], [-8.75, 8.75], [-9.039, 8.452], [-8.933, 8.065], [-8.806, 7.696], [-8.658, 7.348], [-8.488, 7.022], [-8.293, 6.724], [-8.065, 6.465], [-7.242, 5.805], [-7.717, 5.156], [-8.134, 4.469], [-9.059, 4.977], [-9.386, 5.089], [-9.736, 5.155], [-10.102, 5.187], [-10.48, 5.191], [-10.87, 5.167], [-11.268, 5.117], [-11.433, 4.736], [-11.586, 4.349], [-11.34, 4.032], [-11.081, 3.74], [-10.811, 3.475], [-10.529, 3.239], [-10.235, 3.038], [-9.925, 2.887], [-8.912, 2.592], [-9.103, 1.811], [-9.225, 1.016], [-10.275, 1.131], [-10.619, 1.11], [-10.967, 1.037], [-11.318, 0.927], [-11.669, 0.785], [-12.02, 0.614], [-12.368, 0.415], [-12.375, 0], [-12.368, -0.415], [-12.02, -0.614], [-11.669, -0.785], [-11.318, -0.927], [-10.967, -1.037], [-10.619, -1.11], [-10.275, -1.131], [-9.225, -1.016], [-9.103, -1.811], [-8.912, -2.592], [-9.925, -2.887], [-10.235, -3.038], [-10.529, -3.239], [-10.811, -3.475], [-11.081, -3.74], [-11.34, -4.032], [-11.586, -4.349], [-11.433, -4.736], [-11.268, -5.117], [-10.87, -5.167], [-10.48, -5.191], [-10.102, -5.187], [-9.736, -5.155], [-9.386, -5.089], [-9.059, -4.977], [-8.134, -4.469], [-7.717, -5.156], [-7.242, -5.805], [-8.065, -6.465], [-8.293, -6.724], [-8.488, -7.022], [-8.658, -7.348], [-8.806, -7.696], [-8.933, -8.065], [-9.039, -8.452], [-8.75, -8.75], [-8.452, -9.039], [-8.065, -8.933], [-7.696, -8.806], [-7.348, -8.658], [-7.022, -8.488], [-6.724, -8.293], [-6.465, -8.065], [-5.805, -7.242], [-5.156, -7.717], [-4.469, -8.134], [-4.977, -9.059], [-5.089, -9.386], [-5.155, -9.736], [-5.187, -10.102], [-5.191, -10.48], [-5.167, -10.87], [-5.117, -11.268], [-4.736, -11.433], [-4.349, -11.586], [-4.032, -11.34], [-3.74, -11.081], [-3.475, -10.811], [-3.239, -10.529], [-3.038, -10.235], [-2.887, -9.925], [-2.592, -8.912], [-1.811, -9.103], [-1.016, -9.225], [-1.131, -10.275], [-1.11, -10.619], [-1.037, -10.967], [-0.927, -11.318], [-0.785, -11.669], [-0.614, -12.02], [-0.415, -12.368], [0, -12.375], [0.415, -12.368], [0.614, -12.02], [0.785, -11.669], [0.927, -11.318], [1.037, -10.967], [1.11, -10.619], [1.131, -10.275], [1.016, -9.225], [1.811, -9.103], [2.592, -8.912], [2.887, -9.925], [3.038, -10.235], [3.239, -10.529], [3.475, -10.811], [3.74, -11.081], [4.032, -11.34], [4.349, -11.586], [4.736, -11.433], [5.117, -11.268], [5.167, -10.87], [5.191, -10.48], [5.187, -10.102], [5.155, -9.736], [5.089, -9.386], [4.977, -9.059], [4.469, -8.134], [5.156, -7.717], [5.805, -7.242], [6.465, -8.065], [6.724, -8.293], [7.022, -8.488], [7.348, -8.658], [7.696, -8.806], [8.065, -8.933], [8.452, -9.039], [8.75, -8.75], [9.039, -8.452], [8.933, -8.065], [8.806, -7.696], [8.658, -7.348], [8.488, -7.022], [8.293, -6.724], [8.065, -6.465], [7.242, -5.805], [7.717, -5.156], [8.134, -4.469], [9.059, -4.977], [9.386, -5.089], [9.736, -5.155], [10.102, -5.187], [10.48, -5.191], [10.87, -5.167], [11.268, -5.117], [11.433, -4.736], [11.586, -4.349], [11.34, -4.032], [11.081, -3.74], [10.811, -3.475], [10.529, -3.239], [10.235, -3.038], [9.925, -2.887], [8.912, -2.592], [9.103, -1.811]], 14);
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(-22.65, -68.1, 35) * millimeter, "topCenter" : vector(-22.65, -46, 35) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-22.65, -46.1, 35) * millimeter, "topCenter" : vector(-22.65, -34, 35) * millimeter, "radius" : 4.5 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "gearDrive1_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "gearDrive1_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-22.65, -83, 35) * millimeter, "topCenter" : vector(-22.65, -66, 35) * millimeter, "radius" : 2.66 * millimeter });
    coneOp(context, C1 + "o1", vector(-22.65, -66.01, 35) * millimeter, vector(-22.65, -63.94, 35) * millimeter, 2.66, 0.6);
    coneOp(context, C1 + "o2", vector(-22.65, -82.01, 35) * millimeter, vector(-22.65, -81.4, 35) * millimeter, 3.26, 2.66);
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(-22.65, -54, 35) * millimeter, "topCenter" : vector(-22.65, -33, 35) * millimeter, "radius" : 2.25 * millimeter });
    coneOp(context, C1 + "o4", vector(-22.65, -34.6, 35) * millimeter, vector(-22.65, -33.99, 35) * millimeter, 2.25, 2.85);
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "gearDrive1_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Körper Stufe 2";
    const A2 = id + "gearDrive1_a2";
    fCuboid(context, A2 + "s0", { "corner1" : vector(-25.65, -82, 37.22) * millimeter, "corner2" : vector(-19.65, -66, 38.6) * millimeter });
    fCuboid(context, A2 + "s1", { "corner1" : vector(-25.25, -43.5, 36.06) * millimeter, "corner2" : vector(-20.05, -35.5, 38.4) * millimeter });
    prog[] = "Vereinigen Stufe 2";
    unionAll(context, id + "gearDrive1_u2", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]), "Zahnradkupplung v5 hi (NEMA -> Zufuhr, 48 mm)", color(0.93, 0.55, 0.2));
}

function dev_gearDrive2(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "gearDrive2_a0";
    prismOp(context, A0 + "s0", vector(22.65, -82, 35) * millimeter, [[9.225, -1.016], [10.275, -1.131], [10.619, -1.11], [10.967, -1.037], [11.318, -0.927], [11.669, -0.785], [12.02, -0.614], [12.368, -0.415], [12.375, 0], [12.368, 0.415], [12.02, 0.614], [11.669, 0.785], [11.318, 0.927], [10.967, 1.037], [10.619, 1.11], [10.275, 1.131], [9.225, 1.016], [9.103, 1.811], [8.912, 2.592], [9.925, 2.887], [10.235, 3.038], [10.529, 3.239], [10.811, 3.475], [11.081, 3.74], [11.34, 4.032], [11.586, 4.349], [11.433, 4.736], [11.268, 5.117], [10.87, 5.167], [10.48, 5.191], [10.102, 5.187], [9.736, 5.155], [9.386, 5.089], [9.059, 4.977], [8.134, 4.469], [7.717, 5.156], [7.242, 5.805], [8.065, 6.465], [8.293, 6.724], [8.488, 7.022], [8.658, 7.348], [8.806, 7.696], [8.933, 8.065], [9.039, 8.452], [8.75, 8.75], [8.452, 9.039], [8.065, 8.933], [7.696, 8.806], [7.348, 8.658], [7.022, 8.488], [6.724, 8.293], [6.465, 8.065], [5.805, 7.242], [5.156, 7.717], [4.469, 8.134], [4.977, 9.059], [5.089, 9.386], [5.155, 9.736], [5.187, 10.102], [5.191, 10.48], [5.167, 10.87], [5.117, 11.268], [4.736, 11.433], [4.349, 11.586], [4.032, 11.34], [3.74, 11.081], [3.475, 10.811], [3.239, 10.529], [3.038, 10.235], [2.887, 9.925], [2.592, 8.912], [1.811, 9.103], [1.016, 9.225], [1.131, 10.275], [1.11, 10.619], [1.037, 10.967], [0.927, 11.318], [0.785, 11.669], [0.614, 12.02], [0.415, 12.368], [0, 12.375], [-0.415, 12.368], [-0.614, 12.02], [-0.785, 11.669], [-0.927, 11.318], [-1.037, 10.967], [-1.11, 10.619], [-1.131, 10.275], [-1.016, 9.225], [-1.811, 9.103], [-2.592, 8.912], [-2.887, 9.925], [-3.038, 10.235], [-3.239, 10.529], [-3.475, 10.811], [-3.74, 11.081], [-4.032, 11.34], [-4.349, 11.586], [-4.736, 11.433], [-5.117, 11.268], [-5.167, 10.87], [-5.191, 10.48], [-5.187, 10.102], [-5.155, 9.736], [-5.089, 9.386], [-4.977, 9.059], [-4.469, 8.134], [-5.156, 7.717], [-5.805, 7.242], [-6.465, 8.065], [-6.724, 8.293], [-7.022, 8.488], [-7.348, 8.658], [-7.696, 8.806], [-8.065, 8.933], [-8.452, 9.039], [-8.75, 8.75], [-9.039, 8.452], [-8.933, 8.065], [-8.806, 7.696], [-8.658, 7.348], [-8.488, 7.022], [-8.293, 6.724], [-8.065, 6.465], [-7.242, 5.805], [-7.717, 5.156], [-8.134, 4.469], [-9.059, 4.977], [-9.386, 5.089], [-9.736, 5.155], [-10.102, 5.187], [-10.48, 5.191], [-10.87, 5.167], [-11.268, 5.117], [-11.433, 4.736], [-11.586, 4.349], [-11.34, 4.032], [-11.081, 3.74], [-10.811, 3.475], [-10.529, 3.239], [-10.235, 3.038], [-9.925, 2.887], [-8.912, 2.592], [-9.103, 1.811], [-9.225, 1.016], [-10.275, 1.131], [-10.619, 1.11], [-10.967, 1.037], [-11.318, 0.927], [-11.669, 0.785], [-12.02, 0.614], [-12.368, 0.415], [-12.375, 0], [-12.368, -0.415], [-12.02, -0.614], [-11.669, -0.785], [-11.318, -0.927], [-10.967, -1.037], [-10.619, -1.11], [-10.275, -1.131], [-9.225, -1.016], [-9.103, -1.811], [-8.912, -2.592], [-9.925, -2.887], [-10.235, -3.038], [-10.529, -3.239], [-10.811, -3.475], [-11.081, -3.74], [-11.34, -4.032], [-11.586, -4.349], [-11.433, -4.736], [-11.268, -5.117], [-10.87, -5.167], [-10.48, -5.191], [-10.102, -5.187], [-9.736, -5.155], [-9.386, -5.089], [-9.059, -4.977], [-8.134, -4.469], [-7.717, -5.156], [-7.242, -5.805], [-8.065, -6.465], [-8.293, -6.724], [-8.488, -7.022], [-8.658, -7.348], [-8.806, -7.696], [-8.933, -8.065], [-9.039, -8.452], [-8.75, -8.75], [-8.452, -9.039], [-8.065, -8.933], [-7.696, -8.806], [-7.348, -8.658], [-7.022, -8.488], [-6.724, -8.293], [-6.465, -8.065], [-5.805, -7.242], [-5.156, -7.717], [-4.469, -8.134], [-4.977, -9.059], [-5.089, -9.386], [-5.155, -9.736], [-5.187, -10.102], [-5.191, -10.48], [-5.167, -10.87], [-5.117, -11.268], [-4.736, -11.433], [-4.349, -11.586], [-4.032, -11.34], [-3.74, -11.081], [-3.475, -10.811], [-3.239, -10.529], [-3.038, -10.235], [-2.887, -9.925], [-2.592, -8.912], [-1.811, -9.103], [-1.016, -9.225], [-1.131, -10.275], [-1.11, -10.619], [-1.037, -10.967], [-0.927, -11.318], [-0.785, -11.669], [-0.614, -12.02], [-0.415, -12.368], [0, -12.375], [0.415, -12.368], [0.614, -12.02], [0.785, -11.669], [0.927, -11.318], [1.037, -10.967], [1.11, -10.619], [1.131, -10.275], [1.016, -9.225], [1.811, -9.103], [2.592, -8.912], [2.887, -9.925], [3.038, -10.235], [3.239, -10.529], [3.475, -10.811], [3.74, -11.081], [4.032, -11.34], [4.349, -11.586], [4.736, -11.433], [5.117, -11.268], [5.167, -10.87], [5.191, -10.48], [5.187, -10.102], [5.155, -9.736], [5.089, -9.386], [4.977, -9.059], [4.469, -8.134], [5.156, -7.717], [5.805, -7.242], [6.465, -8.065], [6.724, -8.293], [7.022, -8.488], [7.348, -8.658], [7.696, -8.806], [8.065, -8.933], [8.452, -9.039], [8.75, -8.75], [9.039, -8.452], [8.933, -8.065], [8.806, -7.696], [8.658, -7.348], [8.488, -7.022], [8.293, -6.724], [8.065, -6.465], [7.242, -5.805], [7.717, -5.156], [8.134, -4.469], [9.059, -4.977], [9.386, -5.089], [9.736, -5.155], [10.102, -5.187], [10.48, -5.191], [10.87, -5.167], [11.268, -5.117], [11.433, -4.736], [11.586, -4.349], [11.34, -4.032], [11.081, -3.74], [10.811, -3.475], [10.529, -3.239], [10.235, -3.038], [9.925, -2.887], [8.912, -2.592], [9.103, -1.811]], 14);
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(22.65, -68.1, 35) * millimeter, "topCenter" : vector(22.65, -46, 35) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(22.65, -46.1, 35) * millimeter, "topCenter" : vector(22.65, -34, 35) * millimeter, "radius" : 4.5 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "gearDrive2_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "gearDrive2_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(22.65, -83, 35) * millimeter, "topCenter" : vector(22.65, -66, 35) * millimeter, "radius" : 2.66 * millimeter });
    coneOp(context, C1 + "o1", vector(22.65, -66.01, 35) * millimeter, vector(22.65, -63.94, 35) * millimeter, 2.66, 0.6);
    coneOp(context, C1 + "o2", vector(22.65, -82.01, 35) * millimeter, vector(22.65, -81.4, 35) * millimeter, 3.26, 2.66);
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(22.65, -54, 35) * millimeter, "topCenter" : vector(22.65, -33, 35) * millimeter, "radius" : 2.25 * millimeter });
    coneOp(context, C1 + "o4", vector(22.65, -34.6, 35) * millimeter, vector(22.65, -33.99, 35) * millimeter, 2.25, 2.85);
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "gearDrive2_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Körper Stufe 2";
    const A2 = id + "gearDrive2_a2";
    fCuboid(context, A2 + "s0", { "corner1" : vector(19.65, -82, 37.22) * millimeter, "corner2" : vector(25.65, -66, 38.6) * millimeter });
    fCuboid(context, A2 + "s1", { "corner1" : vector(20.05, -43.5, 36.06) * millimeter, "corner2" : vector(25.25, -35.5, 38.4) * millimeter });
    prog[] = "Vereinigen Stufe 2";
    unionAll(context, id + "gearDrive2_u2", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]), "Zahnradkupplung v5 lo (NEMA -> Zufuhr, 48 mm)", color(0.93, 0.55, 0.2));
}

function dev_gearIdle1(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "gearIdle1_a0";
    prismOp(context, A0 + "s0", vector(-44.65, -82, 35) * millimeter, [[9.246, 0.803], [10.298, 0.895], [10.631, 0.983], [10.959, 1.123], [11.281, 1.299], [11.598, 1.507], [11.908, 1.743], [12.211, 2.005], [12.137, 2.414], [12.049, 2.82], [11.669, 2.947], [11.292, 3.046], [10.92, 3.117], [10.554, 3.156], [10.198, 3.16], [9.856, 3.114], [8.85, 2.796], [8.575, 3.552], [8.235, 4.281], [9.171, 4.768], [9.446, 4.977], [9.695, 5.231], [9.925, 5.517], [10.139, 5.83], [10.335, 6.167], [10.514, 6.526], [10.289, 6.875], [10.053, 7.217], [9.653, 7.188], [9.266, 7.136], [8.896, 7.058], [8.543, 6.955], [8.212, 6.822], [7.914, 6.649], [7.106, 5.97], [6.563, 6.563], [5.97, 7.106], [6.649, 7.914], [6.822, 8.212], [6.955, 8.543], [7.058, 8.896], [7.136, 9.266], [7.188, 9.653], [7.217, 10.053], [6.875, 10.289], [6.526, 10.514], [6.167, 10.335], [5.83, 10.139], [5.517, 9.925], [5.231, 9.695], [4.977, 9.446], [4.768, 9.171], [4.281, 8.235], [3.552, 8.575], [2.796, 8.85], [3.114, 9.856], [3.16, 10.198], [3.156, 10.554], [3.117, 10.92], [3.046, 11.292], [2.947, 11.669], [2.82, 12.049], [2.414, 12.137], [2.005, 12.211], [1.743, 11.908], [1.507, 11.598], [1.299, 11.281], [1.123, 10.959], [0.983, 10.631], [0.895, 10.298], [0.803, 9.246], [0, 9.281], [-0.803, 9.246], [-0.895, 10.298], [-0.983, 10.631], [-1.123, 10.959], [-1.299, 11.281], [-1.507, 11.598], [-1.743, 11.908], [-2.005, 12.211], [-2.414, 12.137], [-2.82, 12.049], [-2.947, 11.669], [-3.046, 11.292], [-3.117, 10.92], [-3.156, 10.554], [-3.16, 10.198], [-3.114, 9.856], [-2.796, 8.85], [-3.552, 8.575], [-4.281, 8.235], [-4.768, 9.171], [-4.977, 9.446], [-5.231, 9.695], [-5.517, 9.925], [-5.83, 10.139], [-6.167, 10.335], [-6.526, 10.514], [-6.875, 10.289], [-7.217, 10.053], [-7.188, 9.653], [-7.136, 9.266], [-7.058, 8.896], [-6.955, 8.543], [-6.822, 8.212], [-6.649, 7.914], [-5.97, 7.106], [-6.563, 6.563], [-7.106, 5.97], [-7.914, 6.649], [-8.212, 6.822], [-8.543, 6.955], [-8.896, 7.058], [-9.266, 7.136], [-9.653, 7.188], [-10.053, 7.217], [-10.289, 6.875], [-10.514, 6.526], [-10.335, 6.167], [-10.139, 5.83], [-9.925, 5.517], [-9.695, 5.231], [-9.446, 4.977], [-9.171, 4.768], [-8.235, 4.281], [-8.575, 3.552], [-8.85, 2.796], [-9.856, 3.114], [-10.198, 3.16], [-10.554, 3.156], [-10.92, 3.117], [-11.292, 3.046], [-11.669, 2.947], [-12.049, 2.82], [-12.137, 2.414], [-12.211, 2.005], [-11.908, 1.743], [-11.598, 1.507], [-11.281, 1.299], [-10.959, 1.123], [-10.631, 0.983], [-10.298, 0.895], [-9.246, 0.803], [-9.281, 0], [-9.246, -0.803], [-10.298, -0.895], [-10.631, -0.983], [-10.959, -1.123], [-11.281, -1.299], [-11.598, -1.507], [-11.908, -1.743], [-12.211, -2.005], [-12.137, -2.414], [-12.049, -2.82], [-11.669, -2.947], [-11.292, -3.046], [-10.92, -3.117], [-10.554, -3.156], [-10.198, -3.16], [-9.856, -3.114], [-8.85, -2.796], [-8.575, -3.552], [-8.235, -4.281], [-9.171, -4.768], [-9.446, -4.977], [-9.695, -5.231], [-9.925, -5.517], [-10.139, -5.83], [-10.335, -6.167], [-10.514, -6.526], [-10.289, -6.875], [-10.053, -7.217], [-9.653, -7.188], [-9.266, -7.136], [-8.896, -7.058], [-8.543, -6.955], [-8.212, -6.822], [-7.914, -6.649], [-7.106, -5.97], [-6.563, -6.563], [-5.97, -7.106], [-6.649, -7.914], [-6.822, -8.212], [-6.955, -8.543], [-7.058, -8.896], [-7.136, -9.266], [-7.188, -9.653], [-7.217, -10.053], [-6.875, -10.289], [-6.526, -10.514], [-6.167, -10.335], [-5.83, -10.139], [-5.517, -9.925], [-5.231, -9.695], [-4.977, -9.446], [-4.768, -9.171], [-4.281, -8.235], [-3.552, -8.575], [-2.796, -8.85], [-3.114, -9.856], [-3.16, -10.198], [-3.156, -10.554], [-3.117, -10.92], [-3.046, -11.292], [-2.947, -11.669], [-2.82, -12.049], [-2.414, -12.137], [-2.005, -12.211], [-1.743, -11.908], [-1.507, -11.598], [-1.299, -11.281], [-1.123, -10.959], [-0.983, -10.631], [-0.895, -10.298], [-0.803, -9.246], [0, -9.281], [0.803, -9.246], [0.895, -10.298], [0.983, -10.631], [1.123, -10.959], [1.299, -11.281], [1.507, -11.598], [1.743, -11.908], [2.005, -12.211], [2.414, -12.137], [2.82, -12.049], [2.947, -11.669], [3.046, -11.292], [3.117, -10.92], [3.156, -10.554], [3.16, -10.198], [3.114, -9.856], [2.796, -8.85], [3.552, -8.575], [4.281, -8.235], [4.768, -9.171], [4.977, -9.446], [5.231, -9.695], [5.517, -9.925], [5.83, -10.139], [6.167, -10.335], [6.526, -10.514], [6.875, -10.289], [7.217, -10.053], [7.188, -9.653], [7.136, -9.266], [7.058, -8.896], [6.955, -8.543], [6.822, -8.212], [6.649, -7.914], [5.97, -7.106], [6.563, -6.563], [7.106, -5.97], [7.914, -6.649], [8.212, -6.822], [8.543, -6.955], [8.896, -7.058], [9.266, -7.136], [9.653, -7.188], [10.053, -7.217], [10.289, -6.875], [10.514, -6.526], [10.335, -6.167], [10.139, -5.83], [9.925, -5.517], [9.695, -5.231], [9.446, -4.977], [9.171, -4.768], [8.235, -4.281], [8.575, -3.552], [8.85, -2.796], [9.856, -3.114], [10.198, -3.16], [10.554, -3.156], [10.92, -3.117], [11.292, -3.046], [11.669, -2.947], [12.049, -2.82], [12.137, -2.414], [12.211, -2.005], [11.908, -1.743], [11.598, -1.507], [11.281, -1.299], [10.959, -1.123], [10.631, -0.983], [10.298, -0.895], [9.246, -0.803], [9.281, 0]], 14);
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(-44.65, -68.1, 35) * millimeter, "topCenter" : vector(-44.65, -46, 35) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-44.65, -46.1, 35) * millimeter, "topCenter" : vector(-44.65, -34, 35) * millimeter, "radius" : 4.5 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "gearIdle1_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "gearIdle1_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-44.65, -83, 35) * millimeter, "topCenter" : vector(-44.65, -68.5, 35) * millimeter, "radius" : 3.5 * millimeter });
    coneOp(context, C1 + "o1", vector(-44.65, -68.51, 35) * millimeter, vector(-44.65, -65.6, 35) * millimeter, 3.5, 0.6);
    coneOp(context, C1 + "o2", vector(-44.65, -82.01, 35) * millimeter, vector(-44.65, -81.2, 35) * millimeter, 4.3, 3.5);
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(-44.65, -54, 35) * millimeter, "topCenter" : vector(-44.65, -33, 35) * millimeter, "radius" : 2.25 * millimeter });
    coneOp(context, C1 + "o4", vector(-44.65, -34.6, 35) * millimeter, vector(-44.65, -33.99, 35) * millimeter, 2.25, 2.85);
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "gearIdle1_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Körper Stufe 2";
    const A2 = id + "gearIdle1_a2";
    fCuboid(context, A2 + "s0", { "corner1" : vector(-47.25, -43.5, 36.06) * millimeter, "corner2" : vector(-42.05, -35.5, 38.4) * millimeter });
    prog[] = "Vereinigen Stufe 2";
    unionAll(context, id + "gearIdle1_u2", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]), "Zahnradnabe v5 hi (Ablass, läuft auf Lagerzapfen, 48 mm)", color(0.95, 0.7, 0.25));
}

function dev_gearIdle2(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "gearIdle2_a0";
    prismOp(context, A0 + "s0", vector(44.65, -82, 35) * millimeter, [[9.246, 0.803], [10.298, 0.895], [10.631, 0.983], [10.959, 1.123], [11.281, 1.299], [11.598, 1.507], [11.908, 1.743], [12.211, 2.005], [12.137, 2.414], [12.049, 2.82], [11.669, 2.947], [11.292, 3.046], [10.92, 3.117], [10.554, 3.156], [10.198, 3.16], [9.856, 3.114], [8.85, 2.796], [8.575, 3.552], [8.235, 4.281], [9.171, 4.768], [9.446, 4.977], [9.695, 5.231], [9.925, 5.517], [10.139, 5.83], [10.335, 6.167], [10.514, 6.526], [10.289, 6.875], [10.053, 7.217], [9.653, 7.188], [9.266, 7.136], [8.896, 7.058], [8.543, 6.955], [8.212, 6.822], [7.914, 6.649], [7.106, 5.97], [6.563, 6.563], [5.97, 7.106], [6.649, 7.914], [6.822, 8.212], [6.955, 8.543], [7.058, 8.896], [7.136, 9.266], [7.188, 9.653], [7.217, 10.053], [6.875, 10.289], [6.526, 10.514], [6.167, 10.335], [5.83, 10.139], [5.517, 9.925], [5.231, 9.695], [4.977, 9.446], [4.768, 9.171], [4.281, 8.235], [3.552, 8.575], [2.796, 8.85], [3.114, 9.856], [3.16, 10.198], [3.156, 10.554], [3.117, 10.92], [3.046, 11.292], [2.947, 11.669], [2.82, 12.049], [2.414, 12.137], [2.005, 12.211], [1.743, 11.908], [1.507, 11.598], [1.299, 11.281], [1.123, 10.959], [0.983, 10.631], [0.895, 10.298], [0.803, 9.246], [0, 9.281], [-0.803, 9.246], [-0.895, 10.298], [-0.983, 10.631], [-1.123, 10.959], [-1.299, 11.281], [-1.507, 11.598], [-1.743, 11.908], [-2.005, 12.211], [-2.414, 12.137], [-2.82, 12.049], [-2.947, 11.669], [-3.046, 11.292], [-3.117, 10.92], [-3.156, 10.554], [-3.16, 10.198], [-3.114, 9.856], [-2.796, 8.85], [-3.552, 8.575], [-4.281, 8.235], [-4.768, 9.171], [-4.977, 9.446], [-5.231, 9.695], [-5.517, 9.925], [-5.83, 10.139], [-6.167, 10.335], [-6.526, 10.514], [-6.875, 10.289], [-7.217, 10.053], [-7.188, 9.653], [-7.136, 9.266], [-7.058, 8.896], [-6.955, 8.543], [-6.822, 8.212], [-6.649, 7.914], [-5.97, 7.106], [-6.563, 6.563], [-7.106, 5.97], [-7.914, 6.649], [-8.212, 6.822], [-8.543, 6.955], [-8.896, 7.058], [-9.266, 7.136], [-9.653, 7.188], [-10.053, 7.217], [-10.289, 6.875], [-10.514, 6.526], [-10.335, 6.167], [-10.139, 5.83], [-9.925, 5.517], [-9.695, 5.231], [-9.446, 4.977], [-9.171, 4.768], [-8.235, 4.281], [-8.575, 3.552], [-8.85, 2.796], [-9.856, 3.114], [-10.198, 3.16], [-10.554, 3.156], [-10.92, 3.117], [-11.292, 3.046], [-11.669, 2.947], [-12.049, 2.82], [-12.137, 2.414], [-12.211, 2.005], [-11.908, 1.743], [-11.598, 1.507], [-11.281, 1.299], [-10.959, 1.123], [-10.631, 0.983], [-10.298, 0.895], [-9.246, 0.803], [-9.281, 0], [-9.246, -0.803], [-10.298, -0.895], [-10.631, -0.983], [-10.959, -1.123], [-11.281, -1.299], [-11.598, -1.507], [-11.908, -1.743], [-12.211, -2.005], [-12.137, -2.414], [-12.049, -2.82], [-11.669, -2.947], [-11.292, -3.046], [-10.92, -3.117], [-10.554, -3.156], [-10.198, -3.16], [-9.856, -3.114], [-8.85, -2.796], [-8.575, -3.552], [-8.235, -4.281], [-9.171, -4.768], [-9.446, -4.977], [-9.695, -5.231], [-9.925, -5.517], [-10.139, -5.83], [-10.335, -6.167], [-10.514, -6.526], [-10.289, -6.875], [-10.053, -7.217], [-9.653, -7.188], [-9.266, -7.136], [-8.896, -7.058], [-8.543, -6.955], [-8.212, -6.822], [-7.914, -6.649], [-7.106, -5.97], [-6.563, -6.563], [-5.97, -7.106], [-6.649, -7.914], [-6.822, -8.212], [-6.955, -8.543], [-7.058, -8.896], [-7.136, -9.266], [-7.188, -9.653], [-7.217, -10.053], [-6.875, -10.289], [-6.526, -10.514], [-6.167, -10.335], [-5.83, -10.139], [-5.517, -9.925], [-5.231, -9.695], [-4.977, -9.446], [-4.768, -9.171], [-4.281, -8.235], [-3.552, -8.575], [-2.796, -8.85], [-3.114, -9.856], [-3.16, -10.198], [-3.156, -10.554], [-3.117, -10.92], [-3.046, -11.292], [-2.947, -11.669], [-2.82, -12.049], [-2.414, -12.137], [-2.005, -12.211], [-1.743, -11.908], [-1.507, -11.598], [-1.299, -11.281], [-1.123, -10.959], [-0.983, -10.631], [-0.895, -10.298], [-0.803, -9.246], [0, -9.281], [0.803, -9.246], [0.895, -10.298], [0.983, -10.631], [1.123, -10.959], [1.299, -11.281], [1.507, -11.598], [1.743, -11.908], [2.005, -12.211], [2.414, -12.137], [2.82, -12.049], [2.947, -11.669], [3.046, -11.292], [3.117, -10.92], [3.156, -10.554], [3.16, -10.198], [3.114, -9.856], [2.796, -8.85], [3.552, -8.575], [4.281, -8.235], [4.768, -9.171], [4.977, -9.446], [5.231, -9.695], [5.517, -9.925], [5.83, -10.139], [6.167, -10.335], [6.526, -10.514], [6.875, -10.289], [7.217, -10.053], [7.188, -9.653], [7.136, -9.266], [7.058, -8.896], [6.955, -8.543], [6.822, -8.212], [6.649, -7.914], [5.97, -7.106], [6.563, -6.563], [7.106, -5.97], [7.914, -6.649], [8.212, -6.822], [8.543, -6.955], [8.896, -7.058], [9.266, -7.136], [9.653, -7.188], [10.053, -7.217], [10.289, -6.875], [10.514, -6.526], [10.335, -6.167], [10.139, -5.83], [9.925, -5.517], [9.695, -5.231], [9.446, -4.977], [9.171, -4.768], [8.235, -4.281], [8.575, -3.552], [8.85, -2.796], [9.856, -3.114], [10.198, -3.16], [10.554, -3.156], [10.92, -3.117], [11.292, -3.046], [11.669, -2.947], [12.049, -2.82], [12.137, -2.414], [12.211, -2.005], [11.908, -1.743], [11.598, -1.507], [11.281, -1.299], [10.959, -1.123], [10.631, -0.983], [10.298, -0.895], [9.246, -0.803], [9.281, 0]], 14);
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(44.65, -68.1, 35) * millimeter, "topCenter" : vector(44.65, -46, 35) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(44.65, -46.1, 35) * millimeter, "topCenter" : vector(44.65, -34, 35) * millimeter, "radius" : 4.5 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "gearIdle2_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "gearIdle2_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(44.65, -83, 35) * millimeter, "topCenter" : vector(44.65, -68.5, 35) * millimeter, "radius" : 3.5 * millimeter });
    coneOp(context, C1 + "o1", vector(44.65, -68.51, 35) * millimeter, vector(44.65, -65.6, 35) * millimeter, 3.5, 0.6);
    coneOp(context, C1 + "o2", vector(44.65, -82.01, 35) * millimeter, vector(44.65, -81.2, 35) * millimeter, 4.3, 3.5);
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(44.65, -54, 35) * millimeter, "topCenter" : vector(44.65, -33, 35) * millimeter, "radius" : 2.25 * millimeter });
    coneOp(context, C1 + "o4", vector(44.65, -34.6, 35) * millimeter, vector(44.65, -33.99, 35) * millimeter, 2.25, 2.85);
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "gearIdle2_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Körper Stufe 2";
    const A2 = id + "gearIdle2_a2";
    fCuboid(context, A2 + "s0", { "corner1" : vector(42.05, -43.5, 36.06) * millimeter, "corner2" : vector(47.25, -35.5, 38.4) * millimeter });
    prog[] = "Vereinigen Stufe 2";
    unionAll(context, id + "gearIdle2_u2", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]), "Zahnradnabe v5 lo (Ablass, läuft auf Lagerzapfen, 48 mm)", color(0.95, 0.7, 0.25));
}

function dev_portal5(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "portal5_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-61, -138, 58) * millimeter, "corner2" : vector(61, -135, 108) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-61, -138, 17) * millimeter, "corner2" : vector(-49, -135, 58.1) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(-61, -138, 17) * millimeter, "corner2" : vector(-49, -104, 20) * millimeter });
    prismXOp(context, A0 + "s3", vector(-50.5, -135.1, 19.9) * millimeter, [[0, 0], [31, 0], [0, 30]], 1.5);
    prismXOp(context, A0 + "s4", vector(-61, -135.1, 19.9) * millimeter, [[0, 0], [31, 0], [0, 30]], 1.5);
    fCuboid(context, A0 + "s5", { "corner1" : vector(49, -138, 17) * millimeter, "corner2" : vector(61, -135, 58.1) * millimeter });
    fCuboid(context, A0 + "s6", { "corner1" : vector(49, -138, 17) * millimeter, "corner2" : vector(61, -104, 20) * millimeter });
    prismXOp(context, A0 + "s7", vector(49, -135.1, 19.9) * millimeter, [[0, 0], [31, 0], [0, 30]], 1.5);
    prismXOp(context, A0 + "s8", vector(59.5, -135.1, 19.9) * millimeter, [[0, 0], [31, 0], [0, 30]], 1.5);
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "portal5_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "portal5_c1";
    fCuboid(context, C1 + "o0", { "corner1" : vector(-15.5, -139, 61) * millimeter, "corner2" : vector(15.5, -134, 105) * millimeter });
    fCuboid(context, C1 + "o1", { "corner1" : vector(-45, -139, 64) * millimeter, "corner2" : vector(-29, -134, 102) * millimeter });
    fCuboid(context, C1 + "o2", { "corner1" : vector(-21.7, -139, 71) * millimeter, "corner2" : vector(-18.3, -134, 95) * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(-20, -139, 71) * millimeter, "topCenter" : vector(-20, -134, 71) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(-20, -139, 95) * millimeter, "topCenter" : vector(-20, -134, 95) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o5", { "corner1" : vector(-56.7, -131, 16) * millimeter, "corner2" : vector(-53.3, -108, 21) * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(-55, -131, 16) * millimeter, "topCenter" : vector(-55, -131, 21) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(-55, -108, 16) * millimeter, "topCenter" : vector(-55, -108, 21) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o8", { "corner1" : vector(29, -139, 64) * millimeter, "corner2" : vector(45, -134, 102) * millimeter });
    fCuboid(context, C1 + "o9", { "corner1" : vector(18.3, -139, 71) * millimeter, "corner2" : vector(21.7, -134, 95) * millimeter });
    fCylinder(context, C1 + "o10", { "bottomCenter" : vector(20, -139, 71) * millimeter, "topCenter" : vector(20, -134, 71) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o11", { "bottomCenter" : vector(20, -139, 95) * millimeter, "topCenter" : vector(20, -134, 95) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o12", { "corner1" : vector(53.3, -131, 16) * millimeter, "corner2" : vector(56.7, -108, 21) * millimeter });
    fCylinder(context, C1 + "o13", { "bottomCenter" : vector(55, -131, 16) * millimeter, "topCenter" : vector(55, -131, 21) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o14", { "bottomCenter" : vector(55, -108, 16) * millimeter, "topCenter" : vector(55, -108, 21) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "portal5_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Kameraportal v5 (Y-verstellbar, Halter ±12 mm in Z)", color(0.87, 0.81, 0.69));
}

function dev_camMount5(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "camMount5_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-25, -135, 70.5) * millimeter, "corner2" : vector(25, -134, 95.5) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(-10.5, -134.5, 76.5) * millimeter, "topCenter" : vector(-10.5, -128, 76.5) * millimeter, "radius" : 5 * millimeter });
    coneOp(context, A0 + "s2", vector(-10.5, -134.01, 76.5) * millimeter, vector(-10.5, -133.2, 76.5) * millimeter, 5.8, 5);
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(-10.5, -134.5, 89.5) * millimeter, "topCenter" : vector(-10.5, -128, 89.5) * millimeter, "radius" : 5 * millimeter });
    coneOp(context, A0 + "s4", vector(-10.5, -134.01, 89.5) * millimeter, vector(-10.5, -133.2, 89.5) * millimeter, 5.8, 5);
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(10.5, -134.5, 76.5) * millimeter, "topCenter" : vector(10.5, -128, 76.5) * millimeter, "radius" : 5 * millimeter });
    coneOp(context, A0 + "s6", vector(10.5, -134.01, 76.5) * millimeter, vector(10.5, -133.2, 76.5) * millimeter, 5.8, 5);
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(10.5, -134.5, 89.5) * millimeter, "topCenter" : vector(10.5, -128, 89.5) * millimeter, "radius" : 5 * millimeter });
    coneOp(context, A0 + "s8", vector(10.5, -134.01, 89.5) * millimeter, vector(10.5, -133.2, 89.5) * millimeter, 5.8, 5);
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "camMount5_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "camMount5_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-20, -136, 83) * millimeter, "topCenter" : vector(-20, -133, 83) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-10.5, -136, 76.5) * millimeter, "topCenter" : vector(-10.5, -126, 76.5) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(-10.5, -136, 89.5) * millimeter, "topCenter" : vector(-10.5, -126, 89.5) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(20, -136, 83) * millimeter, "topCenter" : vector(20, -133, 83) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(10.5, -136, 76.5) * millimeter, "topCenter" : vector(10.5, -126, 76.5) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(10.5, -136, 89.5) * millimeter, "topCenter" : vector(10.5, -126, 89.5) * millimeter, "radius" : 1.5 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "camMount5_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Kamerahalter v5 (nur Kamera, 1 mm, M2 durch + Mutter)", color(0.22, 0.22, 0.24));
}

function dev_lzqD1(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "lzqD1_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-54.65, -22, 23) * millimeter, "corner2" : vector(-34.65, 0, 143) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(-44.65, -0.1, 35) * millimeter, "topCenter" : vector(-44.65, 11, 35) * millimeter, "radius" : 4.75 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-44.65, 2, 35) * millimeter, "topCenter" : vector(-44.65, 5, 35) * millimeter, "radius" : 7.5 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(-44.65, 10.9, 35) * millimeter, "topCenter" : vector(-44.65, 25, 35) * millimeter, "radius" : 3.5 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(-44.65, 21, 35) * millimeter, "topCenter" : vector(-44.65, 24.5, 35) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(-44.65, 16.5, 35) * millimeter, "topCenter" : vector(-44.65, 20, 35) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(-44.65, 12, 35) * millimeter, "topCenter" : vector(-44.65, 15.5, 35) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(-44.65, -0.1, 131) * millimeter, "topCenter" : vector(-44.65, 11, 131) * millimeter, "radius" : 4.75 * millimeter });
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(-44.65, 2, 131) * millimeter, "topCenter" : vector(-44.65, 5, 131) * millimeter, "radius" : 7.5 * millimeter });
    fCylinder(context, A0 + "s9", { "bottomCenter" : vector(-44.65, 10.9, 131) * millimeter, "topCenter" : vector(-44.65, 25, 131) * millimeter, "radius" : 3.5 * millimeter });
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(-44.65, 21, 131) * millimeter, "topCenter" : vector(-44.65, 24.5, 131) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s11", { "bottomCenter" : vector(-44.65, 16.5, 131) * millimeter, "topCenter" : vector(-44.65, 20, 131) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(-44.65, 12, 131) * millimeter, "topCenter" : vector(-44.65, 15.5, 131) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s13", { "bottomCenter" : vector(-44.65, -24.5, 35) * millimeter, "topCenter" : vector(-44.65, -21.9, 35) * millimeter, "radius" : 6.928 * millimeter });
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(-44.65, -42, 35) * millimeter, "topCenter" : vector(-44.65, -24.4, 35) * millimeter, "radius" : 2 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "lzqD1_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "LZQ Ablass hi (Dummy)", color(0.7, 0.85, 0.95));
}

function dev_nemaD1(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "nemaD1_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-43.8, -110, 13.85) * millimeter, "corner2" : vector(-1.5, -87, 56.15) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(-22.65, -87.1, 35) * millimeter, "topCenter" : vector(-22.65, -85, 35) * millimeter, "radius" : 11 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-22.65, -87.1, 35) * millimeter, "topCenter" : vector(-22.65, -68, 35) * millimeter, "radius" : 2.5 * millimeter });
    fCuboid(context, A0 + "s3", { "corner1" : vector(-30.65, -110, 3.85) * millimeter, "corner2" : vector(-14.65, -98, 13.95) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "nemaD1_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "NEMA 17 hi (Dummy)", color(0.3, 0.3, 0.32));
}

function dev_lzqD2(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "lzqD2_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-32.65, -22, 23) * millimeter, "corner2" : vector(-12.65, 0, 143) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(-22.65, -0.1, 35) * millimeter, "topCenter" : vector(-22.65, 11, 35) * millimeter, "radius" : 4.75 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-22.65, 2, 35) * millimeter, "topCenter" : vector(-22.65, 5, 35) * millimeter, "radius" : 7.5 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(-22.65, 10.9, 35) * millimeter, "topCenter" : vector(-22.65, 25, 35) * millimeter, "radius" : 3.5 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(-22.65, 21, 35) * millimeter, "topCenter" : vector(-22.65, 24.5, 35) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(-22.65, 16.5, 35) * millimeter, "topCenter" : vector(-22.65, 20, 35) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(-22.65, 12, 35) * millimeter, "topCenter" : vector(-22.65, 15.5, 35) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(-22.65, -0.1, 131) * millimeter, "topCenter" : vector(-22.65, 11, 131) * millimeter, "radius" : 4.75 * millimeter });
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(-22.65, 2, 131) * millimeter, "topCenter" : vector(-22.65, 5, 131) * millimeter, "radius" : 7.5 * millimeter });
    fCylinder(context, A0 + "s9", { "bottomCenter" : vector(-22.65, 10.9, 131) * millimeter, "topCenter" : vector(-22.65, 25, 131) * millimeter, "radius" : 3.5 * millimeter });
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(-22.65, 21, 131) * millimeter, "topCenter" : vector(-22.65, 24.5, 131) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s11", { "bottomCenter" : vector(-22.65, 16.5, 131) * millimeter, "topCenter" : vector(-22.65, 20, 131) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(-22.65, 12, 131) * millimeter, "topCenter" : vector(-22.65, 15.5, 131) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s13", { "bottomCenter" : vector(-22.65, -24.5, 35) * millimeter, "topCenter" : vector(-22.65, -21.9, 35) * millimeter, "radius" : 6.928 * millimeter });
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(-22.65, -42, 35) * millimeter, "topCenter" : vector(-22.65, -24.4, 35) * millimeter, "radius" : 2 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "lzqD2_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "LZQ Zufuhr hi (Dummy)", color(0.7, 0.85, 0.95));
}

function dev_nemaD2(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "nemaD2_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(1.5, -110, 13.85) * millimeter, "corner2" : vector(43.8, -87, 56.15) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(22.65, -87.1, 35) * millimeter, "topCenter" : vector(22.65, -85, 35) * millimeter, "radius" : 11 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(22.65, -87.1, 35) * millimeter, "topCenter" : vector(22.65, -68, 35) * millimeter, "radius" : 2.5 * millimeter });
    fCuboid(context, A0 + "s3", { "corner1" : vector(14.65, -110, 3.85) * millimeter, "corner2" : vector(30.65, -98, 13.95) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "nemaD2_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "NEMA 17 lo (Dummy)", color(0.3, 0.3, 0.32));
}

function dev_lzqD3(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "lzqD3_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(12.65, -22, 23) * millimeter, "corner2" : vector(32.65, 0, 143) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(22.65, -0.1, 35) * millimeter, "topCenter" : vector(22.65, 11, 35) * millimeter, "radius" : 4.75 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(22.65, 2, 35) * millimeter, "topCenter" : vector(22.65, 5, 35) * millimeter, "radius" : 7.5 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(22.65, 10.9, 35) * millimeter, "topCenter" : vector(22.65, 25, 35) * millimeter, "radius" : 3.5 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(22.65, 21, 35) * millimeter, "topCenter" : vector(22.65, 24.5, 35) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(22.65, 16.5, 35) * millimeter, "topCenter" : vector(22.65, 20, 35) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(22.65, 12, 35) * millimeter, "topCenter" : vector(22.65, 15.5, 35) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(22.65, -0.1, 131) * millimeter, "topCenter" : vector(22.65, 11, 131) * millimeter, "radius" : 4.75 * millimeter });
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(22.65, 2, 131) * millimeter, "topCenter" : vector(22.65, 5, 131) * millimeter, "radius" : 7.5 * millimeter });
    fCylinder(context, A0 + "s9", { "bottomCenter" : vector(22.65, 10.9, 131) * millimeter, "topCenter" : vector(22.65, 25, 131) * millimeter, "radius" : 3.5 * millimeter });
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(22.65, 21, 131) * millimeter, "topCenter" : vector(22.65, 24.5, 131) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s11", { "bottomCenter" : vector(22.65, 16.5, 131) * millimeter, "topCenter" : vector(22.65, 20, 131) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(22.65, 12, 131) * millimeter, "topCenter" : vector(22.65, 15.5, 131) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s13", { "bottomCenter" : vector(22.65, -24.5, 35) * millimeter, "topCenter" : vector(22.65, -21.9, 35) * millimeter, "radius" : 6.928 * millimeter });
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(22.65, -42, 35) * millimeter, "topCenter" : vector(22.65, -24.4, 35) * millimeter, "radius" : 2 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "lzqD3_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "LZQ Zufuhr lo (Dummy)", color(0.7, 0.85, 0.95));
}

function dev_lzqD4(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "lzqD4_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(34.65, -22, 23) * millimeter, "corner2" : vector(54.65, 0, 143) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(44.65, -0.1, 35) * millimeter, "topCenter" : vector(44.65, 11, 35) * millimeter, "radius" : 4.75 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(44.65, 2, 35) * millimeter, "topCenter" : vector(44.65, 5, 35) * millimeter, "radius" : 7.5 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(44.65, 10.9, 35) * millimeter, "topCenter" : vector(44.65, 25, 35) * millimeter, "radius" : 3.5 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(44.65, 21, 35) * millimeter, "topCenter" : vector(44.65, 24.5, 35) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(44.65, 16.5, 35) * millimeter, "topCenter" : vector(44.65, 20, 35) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(44.65, 12, 35) * millimeter, "topCenter" : vector(44.65, 15.5, 35) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(44.65, -0.1, 131) * millimeter, "topCenter" : vector(44.65, 11, 131) * millimeter, "radius" : 4.75 * millimeter });
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(44.65, 2, 131) * millimeter, "topCenter" : vector(44.65, 5, 131) * millimeter, "radius" : 7.5 * millimeter });
    fCylinder(context, A0 + "s9", { "bottomCenter" : vector(44.65, 10.9, 131) * millimeter, "topCenter" : vector(44.65, 25, 131) * millimeter, "radius" : 3.5 * millimeter });
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(44.65, 21, 131) * millimeter, "topCenter" : vector(44.65, 24.5, 131) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s11", { "bottomCenter" : vector(44.65, 16.5, 131) * millimeter, "topCenter" : vector(44.65, 20, 131) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(44.65, 12, 131) * millimeter, "topCenter" : vector(44.65, 15.5, 131) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s13", { "bottomCenter" : vector(44.65, -24.5, 35) * millimeter, "topCenter" : vector(44.65, -21.9, 35) * millimeter, "radius" : 6.928 * millimeter });
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(44.65, -42, 35) * millimeter, "topCenter" : vector(44.65, -24.4, 35) * millimeter, "radius" : 2 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "lzqD4_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "LZQ Ablass lo (Dummy)", color(0.7, 0.85, 0.95));
}

function dev_camD(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "camD_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-12.5, -128, 71) * millimeter, "corner2" : vector(12.5, -127, 95) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-4, -127.1, 79) * millimeter, "corner2" : vector(4, -122, 87) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "camD_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Pi-Cam OV5647 (Dummy)", color(0.1, 0.35, 0.15));
}

annotation { "Feature Type Name" : "IHHT Entwicklung v5" }
export const ihhtDevV5 = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
    }
    {
        const prog = new box("");
        var failed = [];
        var okCount = 0;
        try
        {
            dev_frame5(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "frame5 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dev_shelf5(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "shelf5 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dev_splHi(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "splHi @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dev_splLo(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "splLo @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dev_splMix(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "splMix @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dev_bridge5(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "bridge5 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dev_gearDrive1(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "gearDrive1 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dev_gearDrive2(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "gearDrive2 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dev_gearIdle1(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "gearIdle1 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dev_gearIdle2(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "gearIdle2 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dev_portal5(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "portal5 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dev_camMount5(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "camMount5 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dev_lzqD1(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "lzqD1 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dev_nemaD1(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "nemaD1 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dev_lzqD2(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "lzqD2 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dev_nemaD2(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "nemaD2 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dev_lzqD3(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "lzqD3 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dev_lzqD4(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "lzqD4 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dev_camD(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "camD @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        if (size(failed) > 0)
        {
            var msg = "OK: " ~ toString(okCount) ~ " von 19 Teilen. Fehler: ";
            for (var s in failed)
                msg = msg ~ s ~ " || ";
            reportFeatureWarning(context, id, msg);
        }
    });

