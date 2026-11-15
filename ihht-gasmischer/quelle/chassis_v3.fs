FeatureScript 3112;
import(path : "onshape/std/geometry.fs", version : "3112.0");

// IHHT-Gasmischer – v3 Entwicklungs-Setup (generiert). Änderungen bitte per Chat.
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

function dev_frame3(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "frame3_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-88.15, -158, 0) * millimeter, "corner2" : vector(88.15, 95, 1) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-94.15, -152, 0) * millimeter, "corner2" : vector(94.15, 89, 1) * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-88.15, -152, 0) * millimeter, "topCenter" : vector(-88.15, -152, 1) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(88.15, -152, 0) * millimeter, "topCenter" : vector(88.15, -152, 1) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(-88.15, 89, 0) * millimeter, "topCenter" : vector(-88.15, 89, 1) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(88.15, 89, 0) * millimeter, "topCenter" : vector(88.15, 89, 1) * millimeter, "radius" : 6 * millimeter });
    fCuboid(context, A0 + "s6", { "corner1" : vector(-94.15, -152, 0.5) * millimeter, "corner2" : vector(-93.1, 89, 15) * millimeter });
    fCuboid(context, A0 + "s7", { "corner1" : vector(-94.15, -66, 0.5) * millimeter, "corner2" : vector(-93.1, -30, 35) * millimeter });
    fCuboid(context, A0 + "s8", { "corner1" : vector(-94.15, -25, 0.5) * millimeter, "corner2" : vector(-93.1, 30, 145) * millimeter });
    fCuboid(context, A0 + "s9", { "corner1" : vector(-94.15, 38, 0.5) * millimeter, "corner2" : vector(-93.1, 89, 111) * millimeter });
    fCuboid(context, A0 + "s10", { "corner1" : vector(-94.15, -152, 0.5) * millimeter, "corner2" : vector(-93.1, 89, 10) * millimeter });
    fCuboid(context, A0 + "s11", { "corner1" : vector(-94.15, 28, 106) * millimeter, "corner2" : vector(-93.1, 40, 112) * millimeter });
    fCuboid(context, A0 + "s12", { "corner1" : vector(93.1, -152, 0.5) * millimeter, "corner2" : vector(94.15, 89, 15) * millimeter });
    fCuboid(context, A0 + "s13", { "corner1" : vector(93.1, -66, 0.5) * millimeter, "corner2" : vector(94.15, -30, 35) * millimeter });
    fCuboid(context, A0 + "s14", { "corner1" : vector(93.1, -25, 0.5) * millimeter, "corner2" : vector(94.15, 30, 145) * millimeter });
    fCuboid(context, A0 + "s15", { "corner1" : vector(93.1, 38, 0.5) * millimeter, "corner2" : vector(94.15, 89, 111) * millimeter });
    fCuboid(context, A0 + "s16", { "corner1" : vector(93.1, -152, 0.5) * millimeter, "corner2" : vector(94.15, 89, 10) * millimeter });
    fCuboid(context, A0 + "s17", { "corner1" : vector(93.1, 28, 106) * millimeter, "corner2" : vector(94.15, 40, 112) * millimeter });
    fCuboid(context, A0 + "s18", { "corner1" : vector(-88.15, -158, 0.5) * millimeter, "corner2" : vector(88.15, -157, 10) * millimeter });
    fCuboid(context, A0 + "s19", { "corner1" : vector(-88.15, 94, 0.5) * millimeter, "corner2" : vector(88.15, 95, 10) * millimeter });
    fCuboid(context, A0 + "s20", { "corner1" : vector(-93.15, 0, 0.5) * millimeter, "corner2" : vector(93.15, 1, 145) * millimeter });
    fCuboid(context, A0 + "s21", { "corner1" : vector(-45.8, 0.5, 0.5) * millimeter, "corner2" : vector(-44.8, 20, 140) * millimeter });
    fCuboid(context, A0 + "s22", { "corner1" : vector(-45.8, 19, 106) * millimeter, "corner2" : vector(-44.8, 40.6, 112) * millimeter });
    fCuboid(context, A0 + "s23", { "corner1" : vector(-0.5, 0.5, 0.5) * millimeter, "corner2" : vector(0.5, 20, 140) * millimeter });
    fCuboid(context, A0 + "s24", { "corner1" : vector(-0.5, 19, 106) * millimeter, "corner2" : vector(0.5, 40.6, 112) * millimeter });
    fCuboid(context, A0 + "s25", { "corner1" : vector(44.8, 0.5, 0.5) * millimeter, "corner2" : vector(45.8, 20, 140) * millimeter });
    fCuboid(context, A0 + "s26", { "corner1" : vector(44.8, 19, 106) * millimeter, "corner2" : vector(45.8, 40.6, 112) * millimeter });
    fCuboid(context, A0 + "s27", { "corner1" : vector(-93.65, -62, 0.5) * millimeter, "corner2" : vector(-80.15, -32, 9) * millimeter });
    fCuboid(context, A0 + "s28", { "corner1" : vector(80.15, -62, 0.5) * millimeter, "corner2" : vector(93.65, -32, 9) * millimeter });
    fCuboid(context, A0 + "s29", { "corner1" : vector(-93.65, 41, 0.5) * millimeter, "corner2" : vector(-85.15, 49, 112) * millimeter });
    fCuboid(context, A0 + "s30", { "corner1" : vector(-93.65, 84, 0.5) * millimeter, "corner2" : vector(-85.15, 92, 112) * millimeter });
    fCuboid(context, A0 + "s31", { "corner1" : vector(85.15, 41, 0.5) * millimeter, "corner2" : vector(93.65, 49, 112) * millimeter });
    fCuboid(context, A0 + "s32", { "corner1" : vector(85.15, 84, 0.5) * millimeter, "corner2" : vector(93.65, 92, 112) * millimeter });
    fCuboid(context, A0 + "s33", { "corner1" : vector(-26.5, -157, 0.5) * millimeter, "corner2" : vector(-18.5, -99, 17) * millimeter });
    fCuboid(context, A0 + "s34", { "corner1" : vector(18.5, -157, 0.5) * millimeter, "corner2" : vector(26.5, -99, 17) * millimeter });
    fCuboid(context, A0 + "s35", { "corner1" : vector(-84.95, 48, 0.5) * millimeter, "corner2" : vector(-76.95, 84, 17) * millimeter });
    fCuboid(context, A0 + "s36", { "corner1" : vector(-13.65, 48, 0.5) * millimeter, "corner2" : vector(-5.65, 84, 17) * millimeter });
    fCuboid(context, A0 + "s37", { "corner1" : vector(5.65, 48, 0.5) * millimeter, "corner2" : vector(13.65, 84, 17) * millimeter });
    fCuboid(context, A0 + "s38", { "corner1" : vector(76.95, 48, 0.5) * millimeter, "corner2" : vector(84.95, 84, 17) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "frame3_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "frame3_c1";
    fCuboid(context, C1 + "o0", { "corner1" : vector(-79.15, -143, -1) * millimeter, "corner2" : vector(-31, -85, 1.4) * millimeter });
    fCuboid(context, C1 + "o1", { "corner1" : vector(-14, -143, -1) * millimeter, "corner2" : vector(14, -85, 1.4) * millimeter });
    fCuboid(context, C1 + "o2", { "corner1" : vector(31, -143, -1) * millimeter, "corner2" : vector(79.15, -85, 1.4) * millimeter });
    fCuboid(context, C1 + "o3", { "corner1" : vector(-79.15, -75, -1) * millimeter, "corner2" : vector(-6, -30, 1.4) * millimeter });
    fCuboid(context, C1 + "o4", { "corner1" : vector(6, -75, -1) * millimeter, "corner2" : vector(79.15, -30, 1.4) * millimeter });
    fCuboid(context, C1 + "o5", { "corner1" : vector(-79.15, 20, -1) * millimeter, "corner2" : vector(-6, 42, 1.4) * millimeter });
    fCuboid(context, C1 + "o6", { "corner1" : vector(6, 20, -1) * millimeter, "corner2" : vector(79.15, 42, 1.4) * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(-67.95, -1, 35) * millimeter, "topCenter" : vector(-67.95, 4, 35) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(-67.95, -1, 131) * millimeter, "topCenter" : vector(-67.95, 4, 131) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(-22.65, -1, 35) * millimeter, "topCenter" : vector(-22.65, 4, 35) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o10", { "bottomCenter" : vector(-22.65, -1, 131) * millimeter, "topCenter" : vector(-22.65, 4, 131) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o11", { "bottomCenter" : vector(22.65, -1, 35) * millimeter, "topCenter" : vector(22.65, 4, 35) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o12", { "bottomCenter" : vector(22.65, -1, 131) * millimeter, "topCenter" : vector(22.65, 4, 131) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o13", { "bottomCenter" : vector(67.95, -1, 35) * millimeter, "topCenter" : vector(67.95, 4, 35) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o14", { "bottomCenter" : vector(67.95, -1, 131) * millimeter, "topCenter" : vector(67.95, 4, 131) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o15", { "bottomCenter" : vector(-86.55, -47, 1.5) * millimeter, "topCenter" : vector(-86.55, -47, 9.5) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o16", { "bottomCenter" : vector(86.55, -47, 1.5) * millimeter, "topCenter" : vector(86.55, -47, 9.5) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o17", { "bottomCenter" : vector(-89.15, 45, 99.5) * millimeter, "topCenter" : vector(-89.15, 45, 113) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o18", { "bottomCenter" : vector(-89.15, 88, 99.5) * millimeter, "topCenter" : vector(-89.15, 88, 113) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o19", { "bottomCenter" : vector(89.15, 45, 99.5) * millimeter, "topCenter" : vector(89.15, 45, 113) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o20", { "bottomCenter" : vector(89.15, 88, 99.5) * millimeter, "topCenter" : vector(89.15, 88, 113) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o21", { "bottomCenter" : vector(-80.95, 66, 6.5) * millimeter, "topCenter" : vector(-80.95, 66, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o22", { "bottomCenter" : vector(-9.65, 66, 6.5) * millimeter, "topCenter" : vector(-9.65, 66, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o23", { "bottomCenter" : vector(9.65, 66, 6.5) * millimeter, "topCenter" : vector(9.65, 66, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o24", { "bottomCenter" : vector(80.95, 66, 6.5) * millimeter, "topCenter" : vector(80.95, 66, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o25", { "bottomCenter" : vector(-22.5, -152, 6.5) * millimeter, "topCenter" : vector(-22.5, -152, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o26", { "bottomCenter" : vector(-22.5, -144, 6.5) * millimeter, "topCenter" : vector(-22.5, -144, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o27", { "bottomCenter" : vector(-22.5, -136, 6.5) * millimeter, "topCenter" : vector(-22.5, -136, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o28", { "bottomCenter" : vector(-22.5, -128, 6.5) * millimeter, "topCenter" : vector(-22.5, -128, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o29", { "bottomCenter" : vector(-22.5, -120, 6.5) * millimeter, "topCenter" : vector(-22.5, -120, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o30", { "bottomCenter" : vector(-22.5, -112, 6.5) * millimeter, "topCenter" : vector(-22.5, -112, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o31", { "bottomCenter" : vector(-22.5, -104, 6.5) * millimeter, "topCenter" : vector(-22.5, -104, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o32", { "bottomCenter" : vector(22.5, -152, 6.5) * millimeter, "topCenter" : vector(22.5, -152, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o33", { "bottomCenter" : vector(22.5, -144, 6.5) * millimeter, "topCenter" : vector(22.5, -144, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o34", { "bottomCenter" : vector(22.5, -136, 6.5) * millimeter, "topCenter" : vector(22.5, -136, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o35", { "bottomCenter" : vector(22.5, -128, 6.5) * millimeter, "topCenter" : vector(22.5, -128, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o36", { "bottomCenter" : vector(22.5, -120, 6.5) * millimeter, "topCenter" : vector(22.5, -120, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o37", { "bottomCenter" : vector(22.5, -112, 6.5) * millimeter, "topCenter" : vector(22.5, -112, 18) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o38", { "bottomCenter" : vector(22.5, -104, 6.5) * millimeter, "topCenter" : vector(22.5, -104, 18) * millimeter, "radius" : 1.3 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "frame3_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Rahmen v3 (Entwicklung)", color(0.87, 0.81, 0.69));
}

function dev_shelf(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "shelf_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-91, 41, 112) * millimeter, "corner2" : vector(91, 92, 113) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-80, 41, 106) * millimeter, "corner2" : vector(80, 42, 112.5) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(-80, 91, 106) * millimeter, "corner2" : vector(80, 92, 112.5) * millimeter });
    fCuboid(context, A0 + "s3", { "corner1" : vector(-39.65, 48, 112.5) * millimeter, "corner2" : vector(-31.65, 84, 118) * millimeter });
    fCuboid(context, A0 + "s4", { "corner1" : vector(31.65, 48, 112.5) * millimeter, "corner2" : vector(39.65, 84, 118) * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(-35.65, 66, 106) * millimeter, "topCenter" : vector(-35.65, 66, 112.5) * millimeter, "radius" : 4 * millimeter });
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(35.65, 66, 106) * millimeter, "topCenter" : vector(35.65, 66, 112.5) * millimeter, "radius" : 4 * millimeter });
    fCuboid(context, A0 + "s7", { "corner1" : vector(-89.95, 48, 112.5) * millimeter, "corner2" : vector(-81.95, 84, 118) * millimeter });
    fCuboid(context, A0 + "s8", { "corner1" : vector(-53.95, 48, 112.5) * millimeter, "corner2" : vector(-45.95, 84, 118) * millimeter });
    fCylinder(context, A0 + "s9", { "bottomCenter" : vector(-85.95, 66, 106) * millimeter, "topCenter" : vector(-85.95, 66, 112.5) * millimeter, "radius" : 4 * millimeter });
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(-49.95, 66, 106) * millimeter, "topCenter" : vector(-49.95, 66, 112.5) * millimeter, "radius" : 4 * millimeter });
    fCuboid(context, A0 + "s11", { "corner1" : vector(45.95, 48, 112.5) * millimeter, "corner2" : vector(53.95, 84, 118) * millimeter });
    fCuboid(context, A0 + "s12", { "corner1" : vector(81.95, 48, 112.5) * millimeter, "corner2" : vector(89.95, 84, 118) * millimeter });
    fCylinder(context, A0 + "s13", { "bottomCenter" : vector(49.95, 66, 106) * millimeter, "topCenter" : vector(49.95, 66, 112.5) * millimeter, "radius" : 4 * millimeter });
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(85.95, 66, 106) * millimeter, "topCenter" : vector(85.95, 66, 112.5) * millimeter, "radius" : 4 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "shelf_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "shelf_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-89.15, 45, 111) * millimeter, "topCenter" : vector(-89.15, 45, 114) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-89.15, 88, 111) * millimeter, "topCenter" : vector(-89.15, 88, 114) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(89.15, 45, 111) * millimeter, "topCenter" : vector(89.15, 45, 114) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(89.15, 88, 111) * millimeter, "topCenter" : vector(89.15, 88, 114) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o4", { "corner1" : vector(-78.95, 48, 111) * millimeter, "corner2" : vector(-56.95, 85, 113.4) * millimeter });
    fCuboid(context, C1 + "o5", { "corner1" : vector(-28.65, 48, 111) * millimeter, "corner2" : vector(28.65, 85, 113.4) * millimeter });
    fCuboid(context, C1 + "o6", { "corner1" : vector(56.95, 48, 111) * millimeter, "corner2" : vector(78.95, 85, 113.4) * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(-35.65, 66, 107.5) * millimeter, "topCenter" : vector(-35.65, 66, 119) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(35.65, 66, 107.5) * millimeter, "topCenter" : vector(35.65, 66, 119) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(-85.95, 66, 107.5) * millimeter, "topCenter" : vector(-85.95, 66, 119) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o10", { "bottomCenter" : vector(-49.95, 66, 107.5) * millimeter, "topCenter" : vector(-49.95, 66, 119) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o11", { "bottomCenter" : vector(49.95, 66, 107.5) * millimeter, "topCenter" : vector(49.95, 66, 119) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o12", { "bottomCenter" : vector(85.95, 66, 107.5) * millimeter, "topCenter" : vector(85.95, 66, 119) * millimeter, "radius" : 1.3 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "shelf_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Regal oben", color(0.87, 0.81, 0.69));
}

function dev_splHi(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "splHi_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-75.95, 60, 17) * millimeter, "corner2" : vector(-14.65, 72, 43.5) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-85.95, 52, 17) * millimeter, "corner2" : vector(-4.65, 80, 20) * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-67.95, 60.5, 35) * millimeter, "topCenter" : vector(-67.95, 48, 35) * millimeter, "radius" : 5.375 * millimeter });
    coneOp(context, A0 + "s3", vector(-67.95, 48.3, 35) * millimeter, vector(-67.95, 50.1, 35) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(-67.95, 50.1, 35) * millimeter, "topCenter" : vector(-67.95, 50.4, 35) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s5", vector(-67.95, 50.7, 35) * millimeter, vector(-67.95, 52.5, 35) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(-67.95, 52.5, 35) * millimeter, "topCenter" : vector(-67.95, 52.8, 35) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s7", vector(-67.95, 53.1, 35) * millimeter, vector(-67.95, 54.9, 35) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(-67.95, 54.9, 35) * millimeter, "topCenter" : vector(-67.95, 55.2, 35) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s9", vector(-67.95, 60.8, 35) * millimeter, vector(-67.95, 58.3, 35) * millimeter, 7.375, 5.375);
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(-22.65, 60.5, 35) * millimeter, "topCenter" : vector(-22.65, 48, 35) * millimeter, "radius" : 5.375 * millimeter });
    coneOp(context, A0 + "s11", vector(-22.65, 48.3, 35) * millimeter, vector(-22.65, 50.1, 35) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(-22.65, 50.1, 35) * millimeter, "topCenter" : vector(-22.65, 50.4, 35) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s13", vector(-22.65, 50.7, 35) * millimeter, vector(-22.65, 52.5, 35) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(-22.65, 52.5, 35) * millimeter, "topCenter" : vector(-22.65, 52.8, 35) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s15", vector(-22.65, 53.1, 35) * millimeter, vector(-22.65, 54.9, 35) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s16", { "bottomCenter" : vector(-22.65, 54.9, 35) * millimeter, "topCenter" : vector(-22.65, 55.2, 35) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s17", vector(-22.65, 60.8, 35) * millimeter, vector(-22.65, 58.3, 35) * millimeter, 7.375, 5.375);
    fCylinder(context, A0 + "s18", { "bottomCenter" : vector(-45.3, 71.5, 35) * millimeter, "topCenter" : vector(-45.3, 84, 35) * millimeter, "radius" : 3.5 * millimeter });
    fCylinder(context, A0 + "s19", { "bottomCenter" : vector(-45.3, 74, 35) * millimeter, "topCenter" : vector(-45.3, 74.5, 35) * millimeter, "radius" : 3.75 * millimeter });
    coneOp(context, A0 + "s20", vector(-45.3, 74.5, 35) * millimeter, vector(-45.3, 76.5, 35) * millimeter, 3.75, 3.5);
    fCylinder(context, A0 + "s21", { "bottomCenter" : vector(-45.3, 76.5, 35) * millimeter, "topCenter" : vector(-45.3, 77, 35) * millimeter, "radius" : 3.75 * millimeter });
    coneOp(context, A0 + "s22", vector(-45.3, 77, 35) * millimeter, vector(-45.3, 79, 35) * millimeter, 3.75, 3.5);
    fCylinder(context, A0 + "s23", { "bottomCenter" : vector(-45.3, 79, 35) * millimeter, "topCenter" : vector(-45.3, 79.5, 35) * millimeter, "radius" : 3.75 * millimeter });
    coneOp(context, A0 + "s24", vector(-45.3, 79.5, 35) * millimeter, vector(-45.3, 81.5, 35) * millimeter, 3.75, 3.5);
    fCylinder(context, A0 + "s25", { "bottomCenter" : vector(-45.3, 81.5, 35) * millimeter, "topCenter" : vector(-45.3, 82, 35) * millimeter, "radius" : 3.75 * millimeter });
    coneOp(context, A0 + "s26", vector(-45.3, 82, 35) * millimeter, vector(-45.3, 84, 35) * millimeter, 3.75, 3.5);
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "splHi_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "splHi_c1";
    tearOp(context, C1 + "o0", vector(-71.45, 66, 35) * millimeter, vector(-19.15, 66, 35) * millimeter, 4);
    tearOp(context, C1 + "o1", vector(-67.95, 47, 35) * millimeter, vector(-67.95, 66, 35) * millimeter, 3.25);
    tearOp(context, C1 + "o2", vector(-22.65, 47, 35) * millimeter, vector(-22.65, 66, 35) * millimeter, 3.25);
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(-45.3, 65.5, 35) * millimeter, "topCenter" : vector(-45.3, 85, 35) * millimeter, "radius" : 2.6 * millimeter });
    fCuboid(context, C1 + "o4", { "corner1" : vector(-82.65, 56, 16) * millimeter, "corner2" : vector(-79.25, 76, 21) * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(-80.95, 56, 16) * millimeter, "topCenter" : vector(-80.95, 56, 21) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(-80.95, 76, 16) * millimeter, "topCenter" : vector(-80.95, 76, 21) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o7", { "corner1" : vector(-11.35, 56, 16) * millimeter, "corner2" : vector(-7.95, 76, 21) * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(-9.65, 56, 16) * millimeter, "topCenter" : vector(-9.65, 56, 21) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(-9.65, 76, 16) * millimeter, "topCenter" : vector(-9.65, 76, 21) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "splHi_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Splitter hi (Eingang hyperoxisch, links) v3", color(0.85, 0.35, 0.3));
}

function dev_splLo(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "splLo_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(14.65, 60, 17) * millimeter, "corner2" : vector(75.95, 72, 54.7) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(4.65, 52, 17) * millimeter, "corner2" : vector(85.95, 80, 20) * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(22.65, 60.5, 35) * millimeter, "topCenter" : vector(22.65, 48, 35) * millimeter, "radius" : 5.375 * millimeter });
    coneOp(context, A0 + "s3", vector(22.65, 48.3, 35) * millimeter, vector(22.65, 50.1, 35) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(22.65, 50.1, 35) * millimeter, "topCenter" : vector(22.65, 50.4, 35) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s5", vector(22.65, 50.7, 35) * millimeter, vector(22.65, 52.5, 35) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(22.65, 52.5, 35) * millimeter, "topCenter" : vector(22.65, 52.8, 35) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s7", vector(22.65, 53.1, 35) * millimeter, vector(22.65, 54.9, 35) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(22.65, 54.9, 35) * millimeter, "topCenter" : vector(22.65, 55.2, 35) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s9", vector(22.65, 60.8, 35) * millimeter, vector(22.65, 58.3, 35) * millimeter, 7.375, 5.375);
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(67.95, 60.5, 35) * millimeter, "topCenter" : vector(67.95, 48, 35) * millimeter, "radius" : 5.375 * millimeter });
    coneOp(context, A0 + "s11", vector(67.95, 48.3, 35) * millimeter, vector(67.95, 50.1, 35) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(67.95, 50.1, 35) * millimeter, "topCenter" : vector(67.95, 50.4, 35) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s13", vector(67.95, 50.7, 35) * millimeter, vector(67.95, 52.5, 35) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(67.95, 52.5, 35) * millimeter, "topCenter" : vector(67.95, 52.8, 35) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s15", vector(67.95, 53.1, 35) * millimeter, vector(67.95, 54.9, 35) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s16", { "bottomCenter" : vector(67.95, 54.9, 35) * millimeter, "topCenter" : vector(67.95, 55.2, 35) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s17", vector(67.95, 60.8, 35) * millimeter, vector(67.95, 58.3, 35) * millimeter, 7.375, 5.375);
    fCylinder(context, A0 + "s18", { "bottomCenter" : vector(45.3, 71.5, 40) * millimeter, "topCenter" : vector(45.3, 88.2, 40) * millimeter, "radius" : 11.4 * millimeter });
    fCylinder(context, A0 + "s19", { "bottomCenter" : vector(45.3, 75, 40) * millimeter, "topCenter" : vector(45.3, 75.8, 40) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s20", vector(45.3, 75.8, 40) * millimeter, vector(45.3, 78.3, 40) * millimeter, 12.2, 11.4);
    fCylinder(context, A0 + "s21", { "bottomCenter" : vector(45.3, 78.3, 40) * millimeter, "topCenter" : vector(45.3, 79.1, 40) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s22", vector(45.3, 79.1, 40) * millimeter, vector(45.3, 81.6, 40) * millimeter, 12.2, 11.4);
    fCylinder(context, A0 + "s23", { "bottomCenter" : vector(45.3, 81.6, 40) * millimeter, "topCenter" : vector(45.3, 82.4, 40) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s24", vector(45.3, 82.4, 40) * millimeter, vector(45.3, 84.9, 40) * millimeter, 12.2, 11.4);
    fCylinder(context, A0 + "s25", { "bottomCenter" : vector(45.3, 84.9, 40) * millimeter, "topCenter" : vector(45.3, 85.7, 40) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s26", vector(45.3, 85.7, 40) * millimeter, vector(45.3, 88.2, 40) * millimeter, 12.2, 11.4);
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "splLo_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "splLo_c1";
    tearOp(context, C1 + "o0", vector(19.15, 66, 35) * millimeter, vector(71.45, 66, 35) * millimeter, 4);
    tearOp(context, C1 + "o1", vector(22.65, 47, 35) * millimeter, vector(22.65, 66, 35) * millimeter, 3.25);
    tearOp(context, C1 + "o2", vector(67.95, 47, 35) * millimeter, vector(67.95, 66, 35) * millimeter, 3.25);
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(45.3, 64, 40) * millimeter, "topCenter" : vector(45.3, 89.2, 40) * millimeter, "radius" : 10.2 * millimeter });
    fCuboid(context, C1 + "o4", { "corner1" : vector(7.95, 56, 16) * millimeter, "corner2" : vector(11.35, 76, 21) * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(9.65, 56, 16) * millimeter, "topCenter" : vector(9.65, 56, 21) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(9.65, 76, 16) * millimeter, "topCenter" : vector(9.65, 76, 21) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o7", { "corner1" : vector(79.25, 56, 16) * millimeter, "corner2" : vector(82.65, 76, 21) * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(80.95, 56, 16) * millimeter, "topCenter" : vector(80.95, 56, 21) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(80.95, 76, 16) * millimeter, "topCenter" : vector(80.95, 76, 21) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "splLo_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Splitter lo (Eingang hypoxisch, rechts) v3", color(0.35, 0.55, 0.85));
}

function dev_splMix(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "splMix_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-30.65, 60, 118) * millimeter, "corner2" : vector(30.65, 72, 152.7) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-40.65, 52, 118) * millimeter, "corner2" : vector(40.65, 80, 121) * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-22.65, 60.5, 131) * millimeter, "topCenter" : vector(-22.65, 50, 131) * millimeter, "radius" : 3.5 * millimeter });
    coneOp(context, A0 + "s3", vector(-22.65, 50.3, 131) * millimeter, vector(-22.65, 52.1, 131) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(-22.65, 52.1, 131) * millimeter, "topCenter" : vector(-22.65, 52.4, 131) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s5", vector(-22.65, 52.7, 131) * millimeter, vector(-22.65, 54.5, 131) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(-22.65, 54.5, 131) * millimeter, "topCenter" : vector(-22.65, 54.8, 131) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s7", vector(-22.65, 55.1, 131) * millimeter, vector(-22.65, 56.9, 131) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(-22.65, 56.9, 131) * millimeter, "topCenter" : vector(-22.65, 57.2, 131) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s9", vector(-22.65, 60.8, 131) * millimeter, vector(-22.65, 58.3, 131) * millimeter, 5.5, 3.5);
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(22.65, 60.5, 131) * millimeter, "topCenter" : vector(22.65, 50, 131) * millimeter, "radius" : 3.5 * millimeter });
    coneOp(context, A0 + "s11", vector(22.65, 50.3, 131) * millimeter, vector(22.65, 52.1, 131) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(22.65, 52.1, 131) * millimeter, "topCenter" : vector(22.65, 52.4, 131) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s13", vector(22.65, 52.7, 131) * millimeter, vector(22.65, 54.5, 131) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(22.65, 54.5, 131) * millimeter, "topCenter" : vector(22.65, 54.8, 131) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s15", vector(22.65, 55.1, 131) * millimeter, vector(22.65, 56.9, 131) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s16", { "bottomCenter" : vector(22.65, 56.9, 131) * millimeter, "topCenter" : vector(22.65, 57.2, 131) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s17", vector(22.65, 60.8, 131) * millimeter, vector(22.65, 58.3, 131) * millimeter, 5.5, 3.5);
    fCylinder(context, A0 + "s18", { "bottomCenter" : vector(0, 71.5, 138) * millimeter, "topCenter" : vector(0, 88.2, 138) * millimeter, "radius" : 11.4 * millimeter });
    fCylinder(context, A0 + "s19", { "bottomCenter" : vector(0, 75, 138) * millimeter, "topCenter" : vector(0, 75.8, 138) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s20", vector(0, 75.8, 138) * millimeter, vector(0, 78.3, 138) * millimeter, 12.2, 11.4);
    fCylinder(context, A0 + "s21", { "bottomCenter" : vector(0, 78.3, 138) * millimeter, "topCenter" : vector(0, 79.1, 138) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s22", vector(0, 79.1, 138) * millimeter, vector(0, 81.6, 138) * millimeter, 12.2, 11.4);
    fCylinder(context, A0 + "s23", { "bottomCenter" : vector(0, 81.6, 138) * millimeter, "topCenter" : vector(0, 82.4, 138) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s24", vector(0, 82.4, 138) * millimeter, vector(0, 84.9, 138) * millimeter, 12.2, 11.4);
    fCylinder(context, A0 + "s25", { "bottomCenter" : vector(0, 84.9, 138) * millimeter, "topCenter" : vector(0, 85.7, 138) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s26", vector(0, 85.7, 138) * millimeter, vector(0, 88.2, 138) * millimeter, 12.2, 11.4);
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "splMix_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "splMix_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-22.65, 66, 131) * millimeter, "topCenter" : vector(22.65, 66, 131) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-22.65, 49, 131) * millimeter, "topCenter" : vector(-22.65, 66.5, 131) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(22.65, 49, 131) * millimeter, "topCenter" : vector(22.65, 66.5, 131) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(0, 64, 138) * millimeter, "topCenter" : vector(0, 89.2, 138) * millimeter, "radius" : 10.2 * millimeter });
    fCuboid(context, C1 + "o4", { "corner1" : vector(-37.35, 56, 117) * millimeter, "corner2" : vector(-33.95, 76, 122) * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(-35.65, 56, 117) * millimeter, "topCenter" : vector(-35.65, 56, 122) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(-35.65, 76, 117) * millimeter, "topCenter" : vector(-35.65, 76, 122) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o7", { "corner1" : vector(33.95, 56, 117) * millimeter, "corner2" : vector(37.35, 76, 122) * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(35.65, 56, 117) * millimeter, "topCenter" : vector(35.65, 56, 122) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(35.65, 76, 117) * millimeter, "topCenter" : vector(35.65, 76, 122) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "splMix_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Sammler Mix (Ausgang)", color(0.6, 0.4, 0.8));
}

function dev_ventL(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "ventL_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-80.95, 60, 118) * millimeter, "corner2" : vector(-54.95, 72, 138) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-90.95, 52, 118) * millimeter, "corner2" : vector(-44.95, 80, 121) * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-67.95, 60.5, 131) * millimeter, "topCenter" : vector(-67.95, 50, 131) * millimeter, "radius" : 3.5 * millimeter });
    coneOp(context, A0 + "s3", vector(-67.95, 50.3, 131) * millimeter, vector(-67.95, 52.1, 131) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(-67.95, 52.1, 131) * millimeter, "topCenter" : vector(-67.95, 52.4, 131) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s5", vector(-67.95, 52.7, 131) * millimeter, vector(-67.95, 54.5, 131) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(-67.95, 54.5, 131) * millimeter, "topCenter" : vector(-67.95, 54.8, 131) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s7", vector(-67.95, 55.1, 131) * millimeter, vector(-67.95, 56.9, 131) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(-67.95, 56.9, 131) * millimeter, "topCenter" : vector(-67.95, 57.2, 131) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s9", vector(-67.95, 60.8, 131) * millimeter, vector(-67.95, 58.3, 131) * millimeter, 5.5, 3.5);
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(-67.95, 71.5, 131) * millimeter, "topCenter" : vector(-67.95, 88, 131) * millimeter, "radius" : 4 * millimeter });
    coneOp(context, A0 + "s11", vector(-67.95, 87.6, 131) * millimeter, vector(-67.95, 85, 131) * millimeter, 4, 4.5);
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(-67.95, 85, 131) * millimeter, "topCenter" : vector(-67.95, 84.6, 131) * millimeter, "radius" : 4.5 * millimeter });
    coneOp(context, A0 + "s13", vector(-67.95, 84, 131) * millimeter, vector(-67.95, 81.4, 131) * millimeter, 4, 4.5);
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(-67.95, 81.4, 131) * millimeter, "topCenter" : vector(-67.95, 81, 131) * millimeter, "radius" : 4.5 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "ventL_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "ventL_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-67.95, 49, 131) * millimeter, "topCenter" : vector(-67.95, 66.5, 131) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-67.95, 65.5, 131) * millimeter, "topCenter" : vector(-67.95, 89, 131) * millimeter, "radius" : 2.5 * millimeter });
    fCuboid(context, C1 + "o2", { "corner1" : vector(-87.65, 56, 117) * millimeter, "corner2" : vector(-84.25, 76, 122) * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(-85.95, 56, 117) * millimeter, "topCenter" : vector(-85.95, 56, 122) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(-85.95, 76, 117) * millimeter, "topCenter" : vector(-85.95, 76, 122) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o5", { "corner1" : vector(-51.65, 56, 117) * millimeter, "corner2" : vector(-48.25, 76, 122) * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(-49.95, 56, 117) * millimeter, "topCenter" : vector(-49.95, 56, 122) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(-49.95, 76, 117) * millimeter, "topCenter" : vector(-49.95, 76, 122) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "ventL_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Ablass links (hi)", color(0.55, 0.6, 0.65));
}

function dev_ventR(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "ventR_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(54.95, 60, 118) * millimeter, "corner2" : vector(80.95, 72, 138) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(44.95, 52, 118) * millimeter, "corner2" : vector(90.95, 80, 121) * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(67.95, 60.5, 131) * millimeter, "topCenter" : vector(67.95, 50, 131) * millimeter, "radius" : 3.5 * millimeter });
    coneOp(context, A0 + "s3", vector(67.95, 50.3, 131) * millimeter, vector(67.95, 52.1, 131) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(67.95, 52.1, 131) * millimeter, "topCenter" : vector(67.95, 52.4, 131) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s5", vector(67.95, 52.7, 131) * millimeter, vector(67.95, 54.5, 131) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(67.95, 54.5, 131) * millimeter, "topCenter" : vector(67.95, 54.8, 131) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s7", vector(67.95, 55.1, 131) * millimeter, vector(67.95, 56.9, 131) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(67.95, 56.9, 131) * millimeter, "topCenter" : vector(67.95, 57.2, 131) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s9", vector(67.95, 60.8, 131) * millimeter, vector(67.95, 58.3, 131) * millimeter, 5.5, 3.5);
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(67.95, 71.5, 131) * millimeter, "topCenter" : vector(67.95, 88, 131) * millimeter, "radius" : 4 * millimeter });
    coneOp(context, A0 + "s11", vector(67.95, 87.6, 131) * millimeter, vector(67.95, 85, 131) * millimeter, 4, 4.5);
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(67.95, 85, 131) * millimeter, "topCenter" : vector(67.95, 84.6, 131) * millimeter, "radius" : 4.5 * millimeter });
    coneOp(context, A0 + "s13", vector(67.95, 84, 131) * millimeter, vector(67.95, 81.4, 131) * millimeter, 4, 4.5);
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(67.95, 81.4, 131) * millimeter, "topCenter" : vector(67.95, 81, 131) * millimeter, "radius" : 4.5 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "ventR_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "ventR_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(67.95, 49, 131) * millimeter, "topCenter" : vector(67.95, 66.5, 131) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(67.95, 65.5, 131) * millimeter, "topCenter" : vector(67.95, 89, 131) * millimeter, "radius" : 2.5 * millimeter });
    fCuboid(context, C1 + "o2", { "corner1" : vector(48.25, 56, 117) * millimeter, "corner2" : vector(51.65, 76, 122) * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(49.95, 56, 117) * millimeter, "topCenter" : vector(49.95, 56, 122) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(49.95, 76, 117) * millimeter, "topCenter" : vector(49.95, 76, 122) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o5", { "corner1" : vector(84.25, 56, 117) * millimeter, "corner2" : vector(87.65, 76, 122) * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(85.95, 56, 117) * millimeter, "topCenter" : vector(85.95, 56, 122) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(85.95, 76, 117) * millimeter, "topCenter" : vector(85.95, 76, 122) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "ventR_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Ablass rechts (lo)", color(0.55, 0.6, 0.65));
}

function dev_bridge3(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "bridge3_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-92.85, -63, 9) * millimeter, "corner2" : vector(92.85, -62, 54) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-92.85, -63, 9) * millimeter, "corner2" : vector(-80.25, -31, 14.5) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(-92.85, -63, 9) * millimeter, "corner2" : vector(-90.35, -31, 26.5) * millimeter });
    fCuboid(context, A0 + "s3", { "corner1" : vector(80.25, -63, 9) * millimeter, "corner2" : vector(92.85, -31, 14.5) * millimeter });
    fCuboid(context, A0 + "s4", { "corner1" : vector(90.35, -63, 9) * millimeter, "corner2" : vector(92.85, -31, 26.5) * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(-83.45, -62.1, 19.5) * millimeter, "topCenter" : vector(-83.45, -60, 19.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(-83.45, -62.1, 50.5) * millimeter, "topCenter" : vector(-83.45, -60, 50.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(-52.45, -62.1, 19.5) * millimeter, "topCenter" : vector(-52.45, -60, 19.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(-52.45, -62.1, 50.5) * millimeter, "topCenter" : vector(-52.45, -60, 50.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s9", { "bottomCenter" : vector(-38.15, -62.1, 19.5) * millimeter, "topCenter" : vector(-38.15, -60, 19.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(-38.15, -62.1, 50.5) * millimeter, "topCenter" : vector(-38.15, -60, 50.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s11", { "bottomCenter" : vector(-7.15, -62.1, 19.5) * millimeter, "topCenter" : vector(-7.15, -60, 19.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(-7.15, -62.1, 50.5) * millimeter, "topCenter" : vector(-7.15, -60, 50.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s13", { "bottomCenter" : vector(7.15, -62.1, 19.5) * millimeter, "topCenter" : vector(7.15, -60, 19.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(7.15, -62.1, 50.5) * millimeter, "topCenter" : vector(7.15, -60, 50.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s15", { "bottomCenter" : vector(38.15, -62.1, 19.5) * millimeter, "topCenter" : vector(38.15, -60, 19.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s16", { "bottomCenter" : vector(38.15, -62.1, 50.5) * millimeter, "topCenter" : vector(38.15, -60, 50.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s17", { "bottomCenter" : vector(52.45, -62.1, 19.5) * millimeter, "topCenter" : vector(52.45, -60, 19.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s18", { "bottomCenter" : vector(52.45, -62.1, 50.5) * millimeter, "topCenter" : vector(52.45, -60, 50.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s19", { "bottomCenter" : vector(83.45, -62.1, 19.5) * millimeter, "topCenter" : vector(83.45, -60, 19.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s20", { "bottomCenter" : vector(83.45, -62.1, 50.5) * millimeter, "topCenter" : vector(83.45, -60, 50.5) * millimeter, "radius" : 3.75 * millimeter });
    fCuboid(context, A0 + "s21", { "corner1" : vector(-91.1, -62.1, 9) * millimeter, "corner2" : vector(-90.1, -59, 54) * millimeter });
    fCuboid(context, A0 + "s22", { "corner1" : vector(-45.8, -62.1, 9) * millimeter, "corner2" : vector(-44.8, -59, 54) * millimeter });
    fCuboid(context, A0 + "s23", { "corner1" : vector(-0.5, -62.1, 9) * millimeter, "corner2" : vector(0.5, -59, 54) * millimeter });
    fCuboid(context, A0 + "s24", { "corner1" : vector(44.8, -62.1, 9) * millimeter, "corner2" : vector(45.8, -59, 54) * millimeter });
    fCuboid(context, A0 + "s25", { "corner1" : vector(90.1, -62.1, 9) * millimeter, "corner2" : vector(91.1, -59, 54) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "bridge3_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "bridge3_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-67.95, -64, 35) * millimeter, "topCenter" : vector(-67.95, -61, 35) * millimeter, "radius" : 12 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-83.45, -64, 19.5) * millimeter, "topCenter" : vector(-83.45, -59, 19.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(-83.45, -64, 50.5) * millimeter, "topCenter" : vector(-83.45, -59, 50.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(-52.45, -64, 19.5) * millimeter, "topCenter" : vector(-52.45, -59, 19.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(-52.45, -64, 50.5) * millimeter, "topCenter" : vector(-52.45, -59, 50.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(-22.65, -64, 35) * millimeter, "topCenter" : vector(-22.65, -61, 35) * millimeter, "radius" : 12 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(-38.15, -64, 19.5) * millimeter, "topCenter" : vector(-38.15, -59, 19.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(-38.15, -64, 50.5) * millimeter, "topCenter" : vector(-38.15, -59, 50.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(-7.15, -64, 19.5) * millimeter, "topCenter" : vector(-7.15, -59, 19.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(-7.15, -64, 50.5) * millimeter, "topCenter" : vector(-7.15, -59, 50.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o10", { "bottomCenter" : vector(22.65, -64, 35) * millimeter, "topCenter" : vector(22.65, -61, 35) * millimeter, "radius" : 12 * millimeter });
    fCylinder(context, C1 + "o11", { "bottomCenter" : vector(7.15, -64, 19.5) * millimeter, "topCenter" : vector(7.15, -59, 19.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o12", { "bottomCenter" : vector(7.15, -64, 50.5) * millimeter, "topCenter" : vector(7.15, -59, 50.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o13", { "bottomCenter" : vector(38.15, -64, 19.5) * millimeter, "topCenter" : vector(38.15, -59, 19.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o14", { "bottomCenter" : vector(38.15, -64, 50.5) * millimeter, "topCenter" : vector(38.15, -59, 50.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o15", { "bottomCenter" : vector(67.95, -64, 35) * millimeter, "topCenter" : vector(67.95, -61, 35) * millimeter, "radius" : 12 * millimeter });
    fCylinder(context, C1 + "o16", { "bottomCenter" : vector(52.45, -64, 19.5) * millimeter, "topCenter" : vector(52.45, -59, 19.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o17", { "bottomCenter" : vector(52.45, -64, 50.5) * millimeter, "topCenter" : vector(52.45, -59, 50.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o18", { "bottomCenter" : vector(83.45, -64, 19.5) * millimeter, "topCenter" : vector(83.45, -59, 19.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o19", { "bottomCenter" : vector(83.45, -64, 50.5) * millimeter, "topCenter" : vector(83.45, -59, 50.5) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o20", { "corner1" : vector(-88.25, -57, 8) * millimeter, "corner2" : vector(-84.85, -37, 15.5) * millimeter });
    fCylinder(context, C1 + "o21", { "bottomCenter" : vector(-86.55, -57, 8) * millimeter, "topCenter" : vector(-86.55, -57, 15.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o22", { "bottomCenter" : vector(-86.55, -37, 8) * millimeter, "topCenter" : vector(-86.55, -37, 15.5) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o23", { "corner1" : vector(84.85, -57, 8) * millimeter, "corner2" : vector(88.25, -37, 15.5) * millimeter });
    fCylinder(context, C1 + "o24", { "bottomCenter" : vector(86.55, -57, 8) * millimeter, "topCenter" : vector(86.55, -57, 15.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o25", { "bottomCenter" : vector(86.55, -37, 8) * millimeter, "topCenter" : vector(86.55, -37, 15.5) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "bridge3_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Motorbrücke v3 (verschiebbar, Fuß verstärkt)", color(0.87, 0.81, 0.69));
}

function dev_plug1(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "plug1_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(-67.95, -54, 35) * millimeter, "topCenter" : vector(-67.95, -34, 35) * millimeter, "radius" : 5.5 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "plug1_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "plug1_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-67.95, -55, 35) * millimeter, "topCenter" : vector(-67.95, -44, 35) * millimeter, "radius" : 2.575 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-67.95, -44.01, 35) * millimeter, "topCenter" : vector(-67.95, -33, 35) * millimeter, "radius" : 2.075 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "plug1_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Körper Stufe 2";
    const A2 = id + "plug1_a2";
    fCuboid(context, A2 + "s0", { "corner1" : vector(-70.95, -54, 37.1) * millimeter, "corner2" : vector(-64.95, -44, 38.5) * millimeter });
    fCuboid(context, A2 + "s1", { "corner1" : vector(-70.95, -43.5, 36.6) * millimeter, "corner2" : vector(-64.95, -35.5, 38.5) * millimeter });
    prog[] = "Vereinigen Stufe 2";
    unionAll(context, id + "plug1_u2", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]), "Steckkupplung 1", color(0.93, 0.55, 0.2));
}

function dev_plug2(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "plug2_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(-22.65, -54, 35) * millimeter, "topCenter" : vector(-22.65, -34, 35) * millimeter, "radius" : 5.5 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "plug2_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "plug2_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-22.65, -55, 35) * millimeter, "topCenter" : vector(-22.65, -44, 35) * millimeter, "radius" : 2.575 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-22.65, -44.01, 35) * millimeter, "topCenter" : vector(-22.65, -33, 35) * millimeter, "radius" : 2.075 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "plug2_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Körper Stufe 2";
    const A2 = id + "plug2_a2";
    fCuboid(context, A2 + "s0", { "corner1" : vector(-25.65, -54, 37.1) * millimeter, "corner2" : vector(-19.65, -44, 38.5) * millimeter });
    fCuboid(context, A2 + "s1", { "corner1" : vector(-25.65, -43.5, 36.6) * millimeter, "corner2" : vector(-19.65, -35.5, 38.5) * millimeter });
    prog[] = "Vereinigen Stufe 2";
    unionAll(context, id + "plug2_u2", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]), "Steckkupplung 2", color(0.93, 0.55, 0.2));
}

function dev_plug3(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "plug3_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(22.65, -54, 35) * millimeter, "topCenter" : vector(22.65, -34, 35) * millimeter, "radius" : 5.5 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "plug3_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "plug3_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(22.65, -55, 35) * millimeter, "topCenter" : vector(22.65, -44, 35) * millimeter, "radius" : 2.575 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(22.65, -44.01, 35) * millimeter, "topCenter" : vector(22.65, -33, 35) * millimeter, "radius" : 2.075 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "plug3_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Körper Stufe 2";
    const A2 = id + "plug3_a2";
    fCuboid(context, A2 + "s0", { "corner1" : vector(19.65, -54, 37.1) * millimeter, "corner2" : vector(25.65, -44, 38.5) * millimeter });
    fCuboid(context, A2 + "s1", { "corner1" : vector(19.65, -43.5, 36.6) * millimeter, "corner2" : vector(25.65, -35.5, 38.5) * millimeter });
    prog[] = "Vereinigen Stufe 2";
    unionAll(context, id + "plug3_u2", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]), "Steckkupplung 3", color(0.93, 0.55, 0.2));
}

function dev_plug4(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "plug4_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(67.95, -54, 35) * millimeter, "topCenter" : vector(67.95, -34, 35) * millimeter, "radius" : 5.5 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "plug4_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "plug4_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(67.95, -55, 35) * millimeter, "topCenter" : vector(67.95, -44, 35) * millimeter, "radius" : 2.575 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(67.95, -44.01, 35) * millimeter, "topCenter" : vector(67.95, -33, 35) * millimeter, "radius" : 2.075 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "plug4_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Körper Stufe 2";
    const A2 = id + "plug4_a2";
    fCuboid(context, A2 + "s0", { "corner1" : vector(64.95, -54, 37.1) * millimeter, "corner2" : vector(70.95, -44, 38.5) * millimeter });
    fCuboid(context, A2 + "s1", { "corner1" : vector(64.95, -43.5, 36.6) * millimeter, "corner2" : vector(70.95, -35.5, 38.5) * millimeter });
    prog[] = "Vereinigen Stufe 2";
    unionAll(context, id + "plug4_u2", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]), "Steckkupplung 4", color(0.93, 0.55, 0.2));
}

function dev_camcol3(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "camcol3_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-28, -134, 17) * millimeter, "corner2" : vector(28, -88, 20) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-20, -128, 17) * millimeter, "corner2" : vector(-10, -125, 136) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(-16, -134, 17) * millimeter, "corner2" : vector(-14, -125, 50) * millimeter });
    fCuboid(context, A0 + "s3", { "corner1" : vector(-16, -125.1, 17) * millimeter, "corner2" : vector(-14, -113, 40) * millimeter });
    fCuboid(context, A0 + "s4", { "corner1" : vector(-16, -113.1, 17) * millimeter, "corner2" : vector(-14, -88, 25) * millimeter });
    fCuboid(context, A0 + "s5", { "corner1" : vector(10, -128, 17) * millimeter, "corner2" : vector(20, -125, 136) * millimeter });
    fCuboid(context, A0 + "s6", { "corner1" : vector(14, -134, 17) * millimeter, "corner2" : vector(16, -125, 50) * millimeter });
    fCuboid(context, A0 + "s7", { "corner1" : vector(14, -125.1, 17) * millimeter, "corner2" : vector(16, -113, 40) * millimeter });
    fCuboid(context, A0 + "s8", { "corner1" : vector(14, -113.1, 17) * millimeter, "corner2" : vector(16, -88, 25) * millimeter });
    fCuboid(context, A0 + "s9", { "corner1" : vector(-20, -128, 133) * millimeter, "corner2" : vector(20, -125, 136) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "camcol3_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "camcol3_c1";
    fCuboid(context, C1 + "o0", { "corner1" : vector(-16.7, -129, 46) * millimeter, "corner2" : vector(-13.3, -124, 129) * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-15, -129, 46) * millimeter, "topCenter" : vector(-15, -124, 46) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(-15, -129, 129) * millimeter, "topCenter" : vector(-15, -124, 129) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o3", { "corner1" : vector(-24.2, -130, 16) * millimeter, "corner2" : vector(-20.8, -92, 21) * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(-22.5, -130, 16) * millimeter, "topCenter" : vector(-22.5, -130, 21) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(-22.5, -92, 16) * millimeter, "topCenter" : vector(-22.5, -92, 21) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o6", { "corner1" : vector(13.3, -129, 46) * millimeter, "corner2" : vector(16.7, -124, 129) * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(15, -129, 46) * millimeter, "topCenter" : vector(15, -124, 46) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(15, -129, 129) * millimeter, "topCenter" : vector(15, -124, 129) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o9", { "corner1" : vector(20.8, -130, 16) * millimeter, "corner2" : vector(24.2, -92, 21) * millimeter });
    fCylinder(context, C1 + "o10", { "bottomCenter" : vector(22.5, -130, 16) * millimeter, "topCenter" : vector(22.5, -130, 21) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o11", { "bottomCenter" : vector(22.5, -92, 16) * millimeter, "topCenter" : vector(22.5, -92, 21) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "camcol3_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Kamerasäule v3 (Y- und Z-verstellbar)", color(0.87, 0.81, 0.69));
}

function dev_camMount(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "camMount_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-20, -125, 74) * millimeter, "corner2" : vector(20, -124, 132) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(-10.5, -124.5, 87.5) * millimeter, "topCenter" : vector(-10.5, -118, 87.5) * millimeter, "radius" : 5 * millimeter });
    coneOp(context, A0 + "s2", vector(-10.5, -124.01, 87.5) * millimeter, vector(-10.5, -123.2, 87.5) * millimeter, 5.8, 5);
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(-10.5, -124.5, 100.5) * millimeter, "topCenter" : vector(-10.5, -118, 100.5) * millimeter, "radius" : 5 * millimeter });
    coneOp(context, A0 + "s4", vector(-10.5, -124.01, 100.5) * millimeter, vector(-10.5, -123.2, 100.5) * millimeter, 5.8, 5);
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(10.5, -124.5, 87.5) * millimeter, "topCenter" : vector(10.5, -118, 87.5) * millimeter, "radius" : 5 * millimeter });
    coneOp(context, A0 + "s6", vector(10.5, -124.01, 87.5) * millimeter, vector(10.5, -123.2, 87.5) * millimeter, 5.8, 5);
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(10.5, -124.5, 100.5) * millimeter, "topCenter" : vector(10.5, -118, 100.5) * millimeter, "radius" : 5 * millimeter });
    coneOp(context, A0 + "s8", vector(10.5, -124.01, 100.5) * millimeter, vector(10.5, -123.2, 100.5) * millimeter, 5.8, 5);
    fCylinder(context, A0 + "s9", { "bottomCenter" : vector(0, -124.5, 120) * millimeter, "topCenter" : vector(0, -92, 120) * millimeter, "radius" : 8.25 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "camMount_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "camMount_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-15, -126, 80) * millimeter, "topCenter" : vector(-15, -123, 80) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-15, -126, 126) * millimeter, "topCenter" : vector(-15, -123, 126) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(-10.5, -126, 87.5) * millimeter, "topCenter" : vector(-10.5, -116, 87.5) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(-10.5, -126, 100.5) * millimeter, "topCenter" : vector(-10.5, -116, 100.5) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(15, -126, 80) * millimeter, "topCenter" : vector(15, -123, 80) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(15, -126, 126) * millimeter, "topCenter" : vector(15, -123, 126) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(10.5, -126, 87.5) * millimeter, "topCenter" : vector(10.5, -116, 87.5) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(10.5, -126, 100.5) * millimeter, "topCenter" : vector(10.5, -116, 100.5) * millimeter, "radius" : 1.5 * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(0, -126, 120) * millimeter, "topCenter" : vector(0, -91, 120) * millimeter, "radius" : 7.25 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "camMount_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Kamerahalter v3 (1 mm, M2 durch + Mutter)", color(0.22, 0.22, 0.24));
}

function dev_nemaD1(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "nemaD1_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-89.1, -86, 13.85) * millimeter, "corner2" : vector(-46.8, -63, 56.15) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(-67.95, -63.1, 35) * millimeter, "topCenter" : vector(-67.95, -61, 35) * millimeter, "radius" : 11 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-67.95, -63.1, 35) * millimeter, "topCenter" : vector(-67.95, -44, 35) * millimeter, "radius" : 2.5 * millimeter });
    fCuboid(context, A0 + "s3", { "corner1" : vector(-75.95, -86, 3.85) * millimeter, "corner2" : vector(-59.95, -74, 13.95) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "nemaD1_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "NEMA 17 #1 (Dummy)", color(0.3, 0.3, 0.32));
}

function dev_lzqD1(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "lzqD1_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-77.95, -22, 23) * millimeter, "corner2" : vector(-57.95, 0, 143) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(-67.95, -0.1, 35) * millimeter, "topCenter" : vector(-67.95, 11, 35) * millimeter, "radius" : 4.75 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-67.95, 2, 35) * millimeter, "topCenter" : vector(-67.95, 5, 35) * millimeter, "radius" : 7.5 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(-67.95, 10.9, 35) * millimeter, "topCenter" : vector(-67.95, 25, 35) * millimeter, "radius" : 3.5 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(-67.95, 21, 35) * millimeter, "topCenter" : vector(-67.95, 24.5, 35) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(-67.95, 16.5, 35) * millimeter, "topCenter" : vector(-67.95, 20, 35) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(-67.95, 12, 35) * millimeter, "topCenter" : vector(-67.95, 15.5, 35) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(-67.95, -0.1, 131) * millimeter, "topCenter" : vector(-67.95, 11, 131) * millimeter, "radius" : 4.75 * millimeter });
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(-67.95, 2, 131) * millimeter, "topCenter" : vector(-67.95, 5, 131) * millimeter, "radius" : 7.5 * millimeter });
    fCylinder(context, A0 + "s9", { "bottomCenter" : vector(-67.95, 10.9, 131) * millimeter, "topCenter" : vector(-67.95, 25, 131) * millimeter, "radius" : 3.5 * millimeter });
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(-67.95, 21, 131) * millimeter, "topCenter" : vector(-67.95, 24.5, 131) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s11", { "bottomCenter" : vector(-67.95, 16.5, 131) * millimeter, "topCenter" : vector(-67.95, 20, 131) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(-67.95, 12, 131) * millimeter, "topCenter" : vector(-67.95, 15.5, 131) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s13", { "bottomCenter" : vector(-67.95, -24.5, 35) * millimeter, "topCenter" : vector(-67.95, -21.9, 35) * millimeter, "radius" : 6.928 * millimeter });
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(-67.95, -42, 35) * millimeter, "topCenter" : vector(-67.95, -24.4, 35) * millimeter, "radius" : 2 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "lzqD1_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "LZQ #1 (Dummy)", color(0.7, 0.85, 0.95));
}

function dev_nemaD2(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "nemaD2_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-43.8, -86, 13.85) * millimeter, "corner2" : vector(-1.5, -63, 56.15) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(-22.65, -63.1, 35) * millimeter, "topCenter" : vector(-22.65, -61, 35) * millimeter, "radius" : 11 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-22.65, -63.1, 35) * millimeter, "topCenter" : vector(-22.65, -44, 35) * millimeter, "radius" : 2.5 * millimeter });
    fCuboid(context, A0 + "s3", { "corner1" : vector(-30.65, -86, 3.85) * millimeter, "corner2" : vector(-14.65, -74, 13.95) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "nemaD2_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "NEMA 17 #2 (Dummy)", color(0.3, 0.3, 0.32));
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
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "LZQ #2 (Dummy)", color(0.7, 0.85, 0.95));
}

function dev_nemaD3(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "nemaD3_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(1.5, -86, 13.85) * millimeter, "corner2" : vector(43.8, -63, 56.15) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(22.65, -63.1, 35) * millimeter, "topCenter" : vector(22.65, -61, 35) * millimeter, "radius" : 11 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(22.65, -63.1, 35) * millimeter, "topCenter" : vector(22.65, -44, 35) * millimeter, "radius" : 2.5 * millimeter });
    fCuboid(context, A0 + "s3", { "corner1" : vector(14.65, -86, 3.85) * millimeter, "corner2" : vector(30.65, -74, 13.95) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "nemaD3_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "NEMA 17 #3 (Dummy)", color(0.3, 0.3, 0.32));
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
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "LZQ #3 (Dummy)", color(0.7, 0.85, 0.95));
}

function dev_nemaD4(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "nemaD4_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(46.8, -86, 13.85) * millimeter, "corner2" : vector(89.1, -63, 56.15) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(67.95, -63.1, 35) * millimeter, "topCenter" : vector(67.95, -61, 35) * millimeter, "radius" : 11 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(67.95, -63.1, 35) * millimeter, "topCenter" : vector(67.95, -44, 35) * millimeter, "radius" : 2.5 * millimeter });
    fCuboid(context, A0 + "s3", { "corner1" : vector(59.95, -86, 3.85) * millimeter, "corner2" : vector(75.95, -74, 13.95) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "nemaD4_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "NEMA 17 #4 (Dummy)", color(0.3, 0.3, 0.32));
}

function dev_lzqD4(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "lzqD4_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(57.95, -22, 23) * millimeter, "corner2" : vector(77.95, 0, 143) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(67.95, -0.1, 35) * millimeter, "topCenter" : vector(67.95, 11, 35) * millimeter, "radius" : 4.75 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(67.95, 2, 35) * millimeter, "topCenter" : vector(67.95, 5, 35) * millimeter, "radius" : 7.5 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(67.95, 10.9, 35) * millimeter, "topCenter" : vector(67.95, 25, 35) * millimeter, "radius" : 3.5 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(67.95, 21, 35) * millimeter, "topCenter" : vector(67.95, 24.5, 35) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(67.95, 16.5, 35) * millimeter, "topCenter" : vector(67.95, 20, 35) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(67.95, 12, 35) * millimeter, "topCenter" : vector(67.95, 15.5, 35) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(67.95, -0.1, 131) * millimeter, "topCenter" : vector(67.95, 11, 131) * millimeter, "radius" : 4.75 * millimeter });
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(67.95, 2, 131) * millimeter, "topCenter" : vector(67.95, 5, 131) * millimeter, "radius" : 7.5 * millimeter });
    fCylinder(context, A0 + "s9", { "bottomCenter" : vector(67.95, 10.9, 131) * millimeter, "topCenter" : vector(67.95, 25, 131) * millimeter, "radius" : 3.5 * millimeter });
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(67.95, 21, 131) * millimeter, "topCenter" : vector(67.95, 24.5, 131) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s11", { "bottomCenter" : vector(67.95, 16.5, 131) * millimeter, "topCenter" : vector(67.95, 20, 131) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(67.95, 12, 131) * millimeter, "topCenter" : vector(67.95, 15.5, 131) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s13", { "bottomCenter" : vector(67.95, -24.5, 35) * millimeter, "topCenter" : vector(67.95, -21.9, 35) * millimeter, "radius" : 6.928 * millimeter });
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(67.95, -42, 35) * millimeter, "topCenter" : vector(67.95, -24.4, 35) * millimeter, "radius" : 2 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "lzqD4_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "LZQ #4 (Dummy)", color(0.7, 0.85, 0.95));
}

function dev_ledD(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "ledD_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(0, -150, 120) * millimeter, "topCenter" : vector(0, -92.5, 120) * millimeter, "radius" : 7 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "ledD_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Schlüsselanhänger-LED (Dummy)", color(0.1, 0.1, 0.1));
}

function dev_camD(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "camD_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-12.5, -118, 82) * millimeter, "corner2" : vector(12.5, -117, 106) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-4, -117.1, 90) * millimeter, "corner2" : vector(4, -112, 98) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "camD_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Pi-Cam OV5647 (Dummy)", color(0.1, 0.35, 0.15));
}

annotation { "Feature Type Name" : "IHHT Entwicklung v3" }
export const ihhtDevV3 = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
    }
    {
        const prog = new box("");
        var failed = [];
        var okCount = 0;
        try
        {
            dev_frame3(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "frame3 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dev_shelf(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "shelf @ " ~ prog[] ~ " -> " ~ toString(error));
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
            dev_ventL(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "ventL @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dev_ventR(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "ventR @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dev_bridge3(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "bridge3 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dev_plug1(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "plug1 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dev_plug2(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "plug2 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dev_plug3(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "plug3 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dev_plug4(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "plug4 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dev_camcol3(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "camcol3 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dev_camMount(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "camMount @ " ~ prog[] ~ " -> " ~ toString(error));
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
            dev_lzqD1(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "lzqD1 @ " ~ prog[] ~ " -> " ~ toString(error));
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
            dev_lzqD2(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "lzqD2 @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dev_nemaD3(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "nemaD3 @ " ~ prog[] ~ " -> " ~ toString(error));
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
            dev_nemaD4(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "nemaD4 @ " ~ prog[] ~ " -> " ~ toString(error));
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
            dev_ledD(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "ledD @ " ~ prog[] ~ " -> " ~ toString(error));
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
            var msg = "OK: " ~ toString(okCount) ~ " von 24 Teilen. Fehler: ";
            for (var s in failed)
                msg = msg ~ s ~ " || ";
            reportFeatureWarning(context, id, msg);
        }
    });

