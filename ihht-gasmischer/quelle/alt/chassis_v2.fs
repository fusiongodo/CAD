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

function dev_frame2(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "frame2_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-88.15, -148, 0) * millimeter, "corner2" : vector(88.15, 95, 1) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-94.15, -142, 0) * millimeter, "corner2" : vector(94.15, 89, 1) * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-88.15, -142, 0) * millimeter, "topCenter" : vector(-88.15, -142, 1) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(88.15, -142, 0) * millimeter, "topCenter" : vector(88.15, -142, 1) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(-88.15, 89, 0) * millimeter, "topCenter" : vector(-88.15, 89, 1) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(88.15, 89, 0) * millimeter, "topCenter" : vector(88.15, 89, 1) * millimeter, "radius" : 6 * millimeter });
    fCuboid(context, A0 + "s6", { "corner1" : vector(-94.15, -142, 0.5) * millimeter, "corner2" : vector(-93.1, 89, 15) * millimeter });
    fCuboid(context, A0 + "s7", { "corner1" : vector(-94.15, -66, 0.5) * millimeter, "corner2" : vector(-93.1, -30, 25) * millimeter });
    fCuboid(context, A0 + "s8", { "corner1" : vector(-94.15, -25, 0.5) * millimeter, "corner2" : vector(-93.1, 30, 135) * millimeter });
    fCuboid(context, A0 + "s9", { "corner1" : vector(-94.15, 38, 0.5) * millimeter, "corner2" : vector(-93.1, 89, 101) * millimeter });
    fCuboid(context, A0 + "s10", { "corner1" : vector(-94.15, -142, 0.5) * millimeter, "corner2" : vector(-93.1, 89, 10) * millimeter });
    fCuboid(context, A0 + "s11", { "corner1" : vector(-94.15, 28, 96) * millimeter, "corner2" : vector(-93.1, 40, 102) * millimeter });
    fCuboid(context, A0 + "s12", { "corner1" : vector(93.1, -142, 0.5) * millimeter, "corner2" : vector(94.15, 89, 15) * millimeter });
    fCuboid(context, A0 + "s13", { "corner1" : vector(93.1, -66, 0.5) * millimeter, "corner2" : vector(94.15, -30, 25) * millimeter });
    fCuboid(context, A0 + "s14", { "corner1" : vector(93.1, -25, 0.5) * millimeter, "corner2" : vector(94.15, 30, 135) * millimeter });
    fCuboid(context, A0 + "s15", { "corner1" : vector(93.1, 38, 0.5) * millimeter, "corner2" : vector(94.15, 89, 101) * millimeter });
    fCuboid(context, A0 + "s16", { "corner1" : vector(93.1, -142, 0.5) * millimeter, "corner2" : vector(94.15, 89, 10) * millimeter });
    fCuboid(context, A0 + "s17", { "corner1" : vector(93.1, 28, 96) * millimeter, "corner2" : vector(94.15, 40, 102) * millimeter });
    fCuboid(context, A0 + "s18", { "corner1" : vector(-88.15, -148, 0.5) * millimeter, "corner2" : vector(88.15, -147, 10) * millimeter });
    fCuboid(context, A0 + "s19", { "corner1" : vector(-88.15, 94, 0.5) * millimeter, "corner2" : vector(88.15, 95, 10) * millimeter });
    fCuboid(context, A0 + "s20", { "corner1" : vector(-93.15, 0, 0.5) * millimeter, "corner2" : vector(93.15, 1, 135) * millimeter });
    fCuboid(context, A0 + "s21", { "corner1" : vector(-45.8, 0.5, 0.5) * millimeter, "corner2" : vector(-44.8, 20, 130) * millimeter });
    fCuboid(context, A0 + "s22", { "corner1" : vector(-45.8, 19, 96) * millimeter, "corner2" : vector(-44.8, 40.6, 102) * millimeter });
    fCuboid(context, A0 + "s23", { "corner1" : vector(-0.5, 0.5, 0.5) * millimeter, "corner2" : vector(0.5, 20, 130) * millimeter });
    fCuboid(context, A0 + "s24", { "corner1" : vector(-0.5, 19, 96) * millimeter, "corner2" : vector(0.5, 40.6, 102) * millimeter });
    fCuboid(context, A0 + "s25", { "corner1" : vector(44.8, 0.5, 0.5) * millimeter, "corner2" : vector(45.8, 20, 130) * millimeter });
    fCuboid(context, A0 + "s26", { "corner1" : vector(44.8, 19, 96) * millimeter, "corner2" : vector(45.8, 40.6, 102) * millimeter });
    fCuboid(context, A0 + "s27", { "corner1" : vector(-93.65, -62, 0.5) * millimeter, "corner2" : vector(-85.15, -34, 4) * millimeter });
    fCuboid(context, A0 + "s28", { "corner1" : vector(85.15, -62, 0.5) * millimeter, "corner2" : vector(93.65, -34, 4) * millimeter });
    fCuboid(context, A0 + "s29", { "corner1" : vector(-93.65, 41, 0.5) * millimeter, "corner2" : vector(-85.15, 49, 102) * millimeter });
    fCuboid(context, A0 + "s30", { "corner1" : vector(-93.65, 84, 0.5) * millimeter, "corner2" : vector(-85.15, 92, 102) * millimeter });
    fCuboid(context, A0 + "s31", { "corner1" : vector(85.15, 41, 0.5) * millimeter, "corner2" : vector(93.65, 49, 102) * millimeter });
    fCuboid(context, A0 + "s32", { "corner1" : vector(85.15, 84, 0.5) * millimeter, "corner2" : vector(93.65, 92, 102) * millimeter });
    fCuboid(context, A0 + "s33", { "corner1" : vector(-26.5, -147, 0.5) * millimeter, "corner2" : vector(-18.5, -89, 7) * millimeter });
    fCuboid(context, A0 + "s34", { "corner1" : vector(18.5, -147, 0.5) * millimeter, "corner2" : vector(26.5, -89, 7) * millimeter });
    fCuboid(context, A0 + "s35", { "corner1" : vector(-84.95, 48, 0.5) * millimeter, "corner2" : vector(-76.95, 84, 7) * millimeter });
    fCuboid(context, A0 + "s36", { "corner1" : vector(-13.65, 48, 0.5) * millimeter, "corner2" : vector(-5.65, 84, 7) * millimeter });
    fCuboid(context, A0 + "s37", { "corner1" : vector(5.65, 48, 0.5) * millimeter, "corner2" : vector(13.65, 84, 7) * millimeter });
    fCuboid(context, A0 + "s38", { "corner1" : vector(76.95, 48, 0.5) * millimeter, "corner2" : vector(84.95, 84, 7) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "frame2_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "frame2_c1";
    fCuboid(context, C1 + "o0", { "corner1" : vector(-79.15, -133, -1) * millimeter, "corner2" : vector(-31, -85, 1.4) * millimeter });
    fCuboid(context, C1 + "o1", { "corner1" : vector(-14, -133, -1) * millimeter, "corner2" : vector(14, -85, 1.4) * millimeter });
    fCuboid(context, C1 + "o2", { "corner1" : vector(31, -133, -1) * millimeter, "corner2" : vector(79.15, -85, 1.4) * millimeter });
    fCuboid(context, C1 + "o3", { "corner1" : vector(-79.15, -75, -1) * millimeter, "corner2" : vector(-6, -30, 1.4) * millimeter });
    fCuboid(context, C1 + "o4", { "corner1" : vector(6, -75, -1) * millimeter, "corner2" : vector(79.15, -30, 1.4) * millimeter });
    fCuboid(context, C1 + "o5", { "corner1" : vector(-79.15, 20, -1) * millimeter, "corner2" : vector(-6, 42, 1.4) * millimeter });
    fCuboid(context, C1 + "o6", { "corner1" : vector(6, 20, -1) * millimeter, "corner2" : vector(79.15, 42, 1.4) * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(-67.95, -1, 25) * millimeter, "topCenter" : vector(-67.95, 4, 25) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(-67.95, -1, 121) * millimeter, "topCenter" : vector(-67.95, 4, 121) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(-22.65, -1, 25) * millimeter, "topCenter" : vector(-22.65, 4, 25) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o10", { "bottomCenter" : vector(-22.65, -1, 121) * millimeter, "topCenter" : vector(-22.65, 4, 121) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o11", { "bottomCenter" : vector(22.65, -1, 25) * millimeter, "topCenter" : vector(22.65, 4, 25) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o12", { "bottomCenter" : vector(22.65, -1, 121) * millimeter, "topCenter" : vector(22.65, 4, 121) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o13", { "bottomCenter" : vector(67.95, -1, 25) * millimeter, "topCenter" : vector(67.95, 4, 25) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o14", { "bottomCenter" : vector(67.95, -1, 121) * millimeter, "topCenter" : vector(67.95, 4, 121) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o15", { "bottomCenter" : vector(-89.15, -49, 0.5) * millimeter, "topCenter" : vector(-89.15, -49, 4.5) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o16", { "bottomCenter" : vector(89.15, -49, 0.5) * millimeter, "topCenter" : vector(89.15, -49, 4.5) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o17", { "bottomCenter" : vector(-89.15, 45, 90) * millimeter, "topCenter" : vector(-89.15, 45, 103) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o18", { "bottomCenter" : vector(-89.15, 88, 90) * millimeter, "topCenter" : vector(-89.15, 88, 103) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o19", { "bottomCenter" : vector(89.15, 45, 90) * millimeter, "topCenter" : vector(89.15, 45, 103) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o20", { "bottomCenter" : vector(89.15, 88, 90) * millimeter, "topCenter" : vector(89.15, 88, 103) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o21", { "bottomCenter" : vector(-80.95, 66, -0.5) * millimeter, "topCenter" : vector(-80.95, 66, 8) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o22", { "bottomCenter" : vector(-9.65, 66, -0.5) * millimeter, "topCenter" : vector(-9.65, 66, 8) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o23", { "bottomCenter" : vector(9.65, 66, -0.5) * millimeter, "topCenter" : vector(9.65, 66, 8) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o24", { "bottomCenter" : vector(80.95, 66, -0.5) * millimeter, "topCenter" : vector(80.95, 66, 8) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o25", { "bottomCenter" : vector(-22.5, -141, 0.5) * millimeter, "topCenter" : vector(-22.5, -141, 8) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o26", { "bottomCenter" : vector(-22.5, -126, 0.5) * millimeter, "topCenter" : vector(-22.5, -126, 8) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o27", { "bottomCenter" : vector(-22.5, -111, 0.5) * millimeter, "topCenter" : vector(-22.5, -111, 8) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o28", { "bottomCenter" : vector(22.5, -141, 0.5) * millimeter, "topCenter" : vector(22.5, -141, 8) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o29", { "bottomCenter" : vector(22.5, -126, 0.5) * millimeter, "topCenter" : vector(22.5, -126, 8) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o30", { "bottomCenter" : vector(22.5, -111, 0.5) * millimeter, "topCenter" : vector(22.5, -111, 8) * millimeter, "radius" : 1.3 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "frame2_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Rahmen (Entwicklung)", color(0.87, 0.81, 0.69));
}

function dev_shelf(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "shelf_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-91, 41, 102) * millimeter, "corner2" : vector(91, 92, 103) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-80, 41, 96) * millimeter, "corner2" : vector(80, 42, 102.5) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(-80, 91, 96) * millimeter, "corner2" : vector(80, 92, 102.5) * millimeter });
    fCuboid(context, A0 + "s3", { "corner1" : vector(-39.65, 48, 102.5) * millimeter, "corner2" : vector(-31.65, 84, 108) * millimeter });
    fCuboid(context, A0 + "s4", { "corner1" : vector(31.65, 48, 102.5) * millimeter, "corner2" : vector(39.65, 84, 108) * millimeter });
    fCuboid(context, A0 + "s5", { "corner1" : vector(-89.95, 48, 102.5) * millimeter, "corner2" : vector(-81.95, 84, 108) * millimeter });
    fCuboid(context, A0 + "s6", { "corner1" : vector(-53.95, 48, 102.5) * millimeter, "corner2" : vector(-45.95, 84, 108) * millimeter });
    fCuboid(context, A0 + "s7", { "corner1" : vector(45.95, 48, 102.5) * millimeter, "corner2" : vector(53.95, 84, 108) * millimeter });
    fCuboid(context, A0 + "s8", { "corner1" : vector(81.95, 48, 102.5) * millimeter, "corner2" : vector(89.95, 84, 108) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "shelf_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "shelf_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-89.15, 45, 101) * millimeter, "topCenter" : vector(-89.15, 45, 104) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-89.15, 88, 101) * millimeter, "topCenter" : vector(-89.15, 88, 104) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(89.15, 45, 101) * millimeter, "topCenter" : vector(89.15, 45, 104) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(89.15, 88, 101) * millimeter, "topCenter" : vector(89.15, 88, 104) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o4", { "corner1" : vector(-78.95, 48, 101) * millimeter, "corner2" : vector(-56.95, 85, 103.4) * millimeter });
    fCuboid(context, C1 + "o5", { "corner1" : vector(-28.65, 48, 101) * millimeter, "corner2" : vector(28.65, 85, 103.4) * millimeter });
    fCuboid(context, C1 + "o6", { "corner1" : vector(56.95, 48, 101) * millimeter, "corner2" : vector(78.95, 85, 103.4) * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(-35.65, 66, 100.5) * millimeter, "topCenter" : vector(-35.65, 66, 109) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(35.65, 66, 100.5) * millimeter, "topCenter" : vector(35.65, 66, 109) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(-85.95, 66, 100.5) * millimeter, "topCenter" : vector(-85.95, 66, 109) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o10", { "bottomCenter" : vector(-49.95, 66, 100.5) * millimeter, "topCenter" : vector(-49.95, 66, 109) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o11", { "bottomCenter" : vector(49.95, 66, 100.5) * millimeter, "topCenter" : vector(49.95, 66, 109) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o12", { "bottomCenter" : vector(85.95, 66, 100.5) * millimeter, "topCenter" : vector(85.95, 66, 109) * millimeter, "radius" : 1.3 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "shelf_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Regal oben", color(0.87, 0.81, 0.69));
}

function dev_splHi(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "splHi_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-75.95, 60, 7) * millimeter, "corner2" : vector(-14.65, 72, 32) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-85.95, 52, 7) * millimeter, "corner2" : vector(-4.65, 80, 10) * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-67.95, 60.5, 25) * millimeter, "topCenter" : vector(-67.95, 50, 25) * millimeter, "radius" : 3.5 * millimeter });
    coneOp(context, A0 + "s3", vector(-67.95, 50.3, 25) * millimeter, vector(-67.95, 52.1, 25) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(-67.95, 52.1, 25) * millimeter, "topCenter" : vector(-67.95, 52.4, 25) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s5", vector(-67.95, 52.7, 25) * millimeter, vector(-67.95, 54.5, 25) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(-67.95, 54.5, 25) * millimeter, "topCenter" : vector(-67.95, 54.8, 25) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s7", vector(-67.95, 55.1, 25) * millimeter, vector(-67.95, 56.9, 25) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(-67.95, 56.9, 25) * millimeter, "topCenter" : vector(-67.95, 57.2, 25) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s9", vector(-67.95, 60.8, 25) * millimeter, vector(-67.95, 58.3, 25) * millimeter, 5.5, 3.5);
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(-22.65, 60.5, 25) * millimeter, "topCenter" : vector(-22.65, 50, 25) * millimeter, "radius" : 3.5 * millimeter });
    coneOp(context, A0 + "s11", vector(-22.65, 50.3, 25) * millimeter, vector(-22.65, 52.1, 25) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(-22.65, 52.1, 25) * millimeter, "topCenter" : vector(-22.65, 52.4, 25) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s13", vector(-22.65, 52.7, 25) * millimeter, vector(-22.65, 54.5, 25) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(-22.65, 54.5, 25) * millimeter, "topCenter" : vector(-22.65, 54.8, 25) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s15", vector(-22.65, 55.1, 25) * millimeter, vector(-22.65, 56.9, 25) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s16", { "bottomCenter" : vector(-22.65, 56.9, 25) * millimeter, "topCenter" : vector(-22.65, 57.2, 25) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s17", vector(-22.65, 60.8, 25) * millimeter, vector(-22.65, 58.3, 25) * millimeter, 5.5, 3.5);
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
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-67.95, 66, 25) * millimeter, "topCenter" : vector(-22.65, 66, 25) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-67.95, 49, 25) * millimeter, "topCenter" : vector(-67.95, 66.5, 25) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(-22.65, 49, 25) * millimeter, "topCenter" : vector(-22.65, 66.5, 25) * millimeter, "radius" : 2.5 * millimeter });
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
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Splitter hi (Eingang hyperoxisch, links)", color(0.85, 0.35, 0.3));
}

function dev_splLo(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "splLo_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(14.65, 60, 7) * millimeter, "corner2" : vector(75.95, 72, 40.7) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(4.65, 52, 7) * millimeter, "corner2" : vector(85.95, 80, 10) * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(22.65, 60.5, 25) * millimeter, "topCenter" : vector(22.65, 50, 25) * millimeter, "radius" : 3.5 * millimeter });
    coneOp(context, A0 + "s3", vector(22.65, 50.3, 25) * millimeter, vector(22.65, 52.1, 25) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(22.65, 52.1, 25) * millimeter, "topCenter" : vector(22.65, 52.4, 25) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s5", vector(22.65, 52.7, 25) * millimeter, vector(22.65, 54.5, 25) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(22.65, 54.5, 25) * millimeter, "topCenter" : vector(22.65, 54.8, 25) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s7", vector(22.65, 55.1, 25) * millimeter, vector(22.65, 56.9, 25) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(22.65, 56.9, 25) * millimeter, "topCenter" : vector(22.65, 57.2, 25) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s9", vector(22.65, 60.8, 25) * millimeter, vector(22.65, 58.3, 25) * millimeter, 5.5, 3.5);
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(67.95, 60.5, 25) * millimeter, "topCenter" : vector(67.95, 50, 25) * millimeter, "radius" : 3.5 * millimeter });
    coneOp(context, A0 + "s11", vector(67.95, 50.3, 25) * millimeter, vector(67.95, 52.1, 25) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(67.95, 52.1, 25) * millimeter, "topCenter" : vector(67.95, 52.4, 25) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s13", vector(67.95, 52.7, 25) * millimeter, vector(67.95, 54.5, 25) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(67.95, 54.5, 25) * millimeter, "topCenter" : vector(67.95, 54.8, 25) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s15", vector(67.95, 55.1, 25) * millimeter, vector(67.95, 56.9, 25) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s16", { "bottomCenter" : vector(67.95, 56.9, 25) * millimeter, "topCenter" : vector(67.95, 57.2, 25) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s17", vector(67.95, 60.8, 25) * millimeter, vector(67.95, 58.3, 25) * millimeter, 5.5, 3.5);
    fCylinder(context, A0 + "s18", { "bottomCenter" : vector(45.3, 71.5, 26) * millimeter, "topCenter" : vector(45.3, 88.2, 26) * millimeter, "radius" : 11.4 * millimeter });
    fCylinder(context, A0 + "s19", { "bottomCenter" : vector(45.3, 75, 26) * millimeter, "topCenter" : vector(45.3, 75.8, 26) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s20", vector(45.3, 75.8, 26) * millimeter, vector(45.3, 78.3, 26) * millimeter, 12.2, 11.4);
    fCylinder(context, A0 + "s21", { "bottomCenter" : vector(45.3, 78.3, 26) * millimeter, "topCenter" : vector(45.3, 79.1, 26) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s22", vector(45.3, 79.1, 26) * millimeter, vector(45.3, 81.6, 26) * millimeter, 12.2, 11.4);
    fCylinder(context, A0 + "s23", { "bottomCenter" : vector(45.3, 81.6, 26) * millimeter, "topCenter" : vector(45.3, 82.4, 26) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s24", vector(45.3, 82.4, 26) * millimeter, vector(45.3, 84.9, 26) * millimeter, 12.2, 11.4);
    fCylinder(context, A0 + "s25", { "bottomCenter" : vector(45.3, 84.9, 26) * millimeter, "topCenter" : vector(45.3, 85.7, 26) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s26", vector(45.3, 85.7, 26) * millimeter, vector(45.3, 88.2, 26) * millimeter, 12.2, 11.4);
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "splLo_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "splLo_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(22.65, 66, 25) * millimeter, "topCenter" : vector(67.95, 66, 25) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(22.65, 49, 25) * millimeter, "topCenter" : vector(22.65, 66.5, 25) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(67.95, 49, 25) * millimeter, "topCenter" : vector(67.95, 66.5, 25) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(45.3, 64, 26) * millimeter, "topCenter" : vector(45.3, 89.2, 26) * millimeter, "radius" : 10.2 * millimeter });
    fCuboid(context, C1 + "o4", { "corner1" : vector(7.95, 56, 6) * millimeter, "corner2" : vector(11.35, 76, 11) * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(9.65, 56, 6) * millimeter, "topCenter" : vector(9.65, 56, 11) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(9.65, 76, 6) * millimeter, "topCenter" : vector(9.65, 76, 11) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o7", { "corner1" : vector(79.25, 56, 6) * millimeter, "corner2" : vector(82.65, 76, 11) * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(80.95, 56, 6) * millimeter, "topCenter" : vector(80.95, 56, 11) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(80.95, 76, 6) * millimeter, "topCenter" : vector(80.95, 76, 11) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "splLo_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Splitter lo (Eingang hypoxisch, rechts)", color(0.35, 0.55, 0.85));
}

function dev_splMix(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "splMix_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-30.65, 60, 108) * millimeter, "corner2" : vector(30.65, 72, 142.7) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-40.65, 52, 108) * millimeter, "corner2" : vector(40.65, 80, 111) * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-22.65, 60.5, 121) * millimeter, "topCenter" : vector(-22.65, 50, 121) * millimeter, "radius" : 3.5 * millimeter });
    coneOp(context, A0 + "s3", vector(-22.65, 50.3, 121) * millimeter, vector(-22.65, 52.1, 121) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(-22.65, 52.1, 121) * millimeter, "topCenter" : vector(-22.65, 52.4, 121) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s5", vector(-22.65, 52.7, 121) * millimeter, vector(-22.65, 54.5, 121) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(-22.65, 54.5, 121) * millimeter, "topCenter" : vector(-22.65, 54.8, 121) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s7", vector(-22.65, 55.1, 121) * millimeter, vector(-22.65, 56.9, 121) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(-22.65, 56.9, 121) * millimeter, "topCenter" : vector(-22.65, 57.2, 121) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s9", vector(-22.65, 60.8, 121) * millimeter, vector(-22.65, 58.3, 121) * millimeter, 5.5, 3.5);
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(22.65, 60.5, 121) * millimeter, "topCenter" : vector(22.65, 50, 121) * millimeter, "radius" : 3.5 * millimeter });
    coneOp(context, A0 + "s11", vector(22.65, 50.3, 121) * millimeter, vector(22.65, 52.1, 121) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(22.65, 52.1, 121) * millimeter, "topCenter" : vector(22.65, 52.4, 121) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s13", vector(22.65, 52.7, 121) * millimeter, vector(22.65, 54.5, 121) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(22.65, 54.5, 121) * millimeter, "topCenter" : vector(22.65, 54.8, 121) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s15", vector(22.65, 55.1, 121) * millimeter, vector(22.65, 56.9, 121) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s16", { "bottomCenter" : vector(22.65, 56.9, 121) * millimeter, "topCenter" : vector(22.65, 57.2, 121) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s17", vector(22.65, 60.8, 121) * millimeter, vector(22.65, 58.3, 121) * millimeter, 5.5, 3.5);
    fCylinder(context, A0 + "s18", { "bottomCenter" : vector(0, 71.5, 128) * millimeter, "topCenter" : vector(0, 88.2, 128) * millimeter, "radius" : 11.4 * millimeter });
    fCylinder(context, A0 + "s19", { "bottomCenter" : vector(0, 75, 128) * millimeter, "topCenter" : vector(0, 75.8, 128) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s20", vector(0, 75.8, 128) * millimeter, vector(0, 78.3, 128) * millimeter, 12.2, 11.4);
    fCylinder(context, A0 + "s21", { "bottomCenter" : vector(0, 78.3, 128) * millimeter, "topCenter" : vector(0, 79.1, 128) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s22", vector(0, 79.1, 128) * millimeter, vector(0, 81.6, 128) * millimeter, 12.2, 11.4);
    fCylinder(context, A0 + "s23", { "bottomCenter" : vector(0, 81.6, 128) * millimeter, "topCenter" : vector(0, 82.4, 128) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s24", vector(0, 82.4, 128) * millimeter, vector(0, 84.9, 128) * millimeter, 12.2, 11.4);
    fCylinder(context, A0 + "s25", { "bottomCenter" : vector(0, 84.9, 128) * millimeter, "topCenter" : vector(0, 85.7, 128) * millimeter, "radius" : 12.2 * millimeter });
    coneOp(context, A0 + "s26", vector(0, 85.7, 128) * millimeter, vector(0, 88.2, 128) * millimeter, 12.2, 11.4);
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "splMix_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "splMix_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-22.65, 66, 121) * millimeter, "topCenter" : vector(22.65, 66, 121) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-22.65, 49, 121) * millimeter, "topCenter" : vector(-22.65, 66.5, 121) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(22.65, 49, 121) * millimeter, "topCenter" : vector(22.65, 66.5, 121) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(0, 64, 128) * millimeter, "topCenter" : vector(0, 89.2, 128) * millimeter, "radius" : 10.2 * millimeter });
    fCuboid(context, C1 + "o4", { "corner1" : vector(-37.35, 56, 107) * millimeter, "corner2" : vector(-33.95, 76, 112) * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(-35.65, 56, 107) * millimeter, "topCenter" : vector(-35.65, 56, 112) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(-35.65, 76, 107) * millimeter, "topCenter" : vector(-35.65, 76, 112) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o7", { "corner1" : vector(33.95, 56, 107) * millimeter, "corner2" : vector(37.35, 76, 112) * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(35.65, 56, 107) * millimeter, "topCenter" : vector(35.65, 56, 112) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(35.65, 76, 107) * millimeter, "topCenter" : vector(35.65, 76, 112) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "splMix_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Sammler Mix (Ausgang)", color(0.6, 0.4, 0.8));
}

function dev_ventL(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "ventL_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-80.95, 60, 108) * millimeter, "corner2" : vector(-54.95, 72, 128) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-90.95, 52, 108) * millimeter, "corner2" : vector(-44.95, 80, 111) * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-67.95, 60.5, 121) * millimeter, "topCenter" : vector(-67.95, 50, 121) * millimeter, "radius" : 3.5 * millimeter });
    coneOp(context, A0 + "s3", vector(-67.95, 50.3, 121) * millimeter, vector(-67.95, 52.1, 121) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(-67.95, 52.1, 121) * millimeter, "topCenter" : vector(-67.95, 52.4, 121) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s5", vector(-67.95, 52.7, 121) * millimeter, vector(-67.95, 54.5, 121) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(-67.95, 54.5, 121) * millimeter, "topCenter" : vector(-67.95, 54.8, 121) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s7", vector(-67.95, 55.1, 121) * millimeter, vector(-67.95, 56.9, 121) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(-67.95, 56.9, 121) * millimeter, "topCenter" : vector(-67.95, 57.2, 121) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s9", vector(-67.95, 60.8, 121) * millimeter, vector(-67.95, 58.3, 121) * millimeter, 5.5, 3.5);
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(-67.95, 71.5, 121) * millimeter, "topCenter" : vector(-67.95, 88, 121) * millimeter, "radius" : 4 * millimeter });
    coneOp(context, A0 + "s11", vector(-67.95, 87.6, 121) * millimeter, vector(-67.95, 85, 121) * millimeter, 4, 4.5);
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(-67.95, 85, 121) * millimeter, "topCenter" : vector(-67.95, 84.6, 121) * millimeter, "radius" : 4.5 * millimeter });
    coneOp(context, A0 + "s13", vector(-67.95, 84, 121) * millimeter, vector(-67.95, 81.4, 121) * millimeter, 4, 4.5);
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(-67.95, 81.4, 121) * millimeter, "topCenter" : vector(-67.95, 81, 121) * millimeter, "radius" : 4.5 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "ventL_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "ventL_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-67.95, 49, 121) * millimeter, "topCenter" : vector(-67.95, 66.5, 121) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-67.95, 65.5, 121) * millimeter, "topCenter" : vector(-67.95, 89, 121) * millimeter, "radius" : 2.5 * millimeter });
    fCuboid(context, C1 + "o2", { "corner1" : vector(-87.65, 56, 107) * millimeter, "corner2" : vector(-84.25, 76, 112) * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(-85.95, 56, 107) * millimeter, "topCenter" : vector(-85.95, 56, 112) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(-85.95, 76, 107) * millimeter, "topCenter" : vector(-85.95, 76, 112) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o5", { "corner1" : vector(-51.65, 56, 107) * millimeter, "corner2" : vector(-48.25, 76, 112) * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(-49.95, 56, 107) * millimeter, "topCenter" : vector(-49.95, 56, 112) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(-49.95, 76, 107) * millimeter, "topCenter" : vector(-49.95, 76, 112) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "ventL_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Ablass links (hi)", color(0.55, 0.6, 0.65));
}

function dev_ventR(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "ventR_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(54.95, 60, 108) * millimeter, "corner2" : vector(80.95, 72, 128) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(44.95, 52, 108) * millimeter, "corner2" : vector(90.95, 80, 111) * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(67.95, 60.5, 121) * millimeter, "topCenter" : vector(67.95, 50, 121) * millimeter, "radius" : 3.5 * millimeter });
    coneOp(context, A0 + "s3", vector(67.95, 50.3, 121) * millimeter, vector(67.95, 52.1, 121) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(67.95, 52.1, 121) * millimeter, "topCenter" : vector(67.95, 52.4, 121) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s5", vector(67.95, 52.7, 121) * millimeter, vector(67.95, 54.5, 121) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(67.95, 54.5, 121) * millimeter, "topCenter" : vector(67.95, 54.8, 121) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s7", vector(67.95, 55.1, 121) * millimeter, vector(67.95, 56.9, 121) * millimeter, 3.5, 3.65);
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(67.95, 56.9, 121) * millimeter, "topCenter" : vector(67.95, 57.2, 121) * millimeter, "radius" : 3.65 * millimeter });
    coneOp(context, A0 + "s9", vector(67.95, 60.8, 121) * millimeter, vector(67.95, 58.3, 121) * millimeter, 5.5, 3.5);
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(67.95, 71.5, 121) * millimeter, "topCenter" : vector(67.95, 88, 121) * millimeter, "radius" : 4 * millimeter });
    coneOp(context, A0 + "s11", vector(67.95, 87.6, 121) * millimeter, vector(67.95, 85, 121) * millimeter, 4, 4.5);
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(67.95, 85, 121) * millimeter, "topCenter" : vector(67.95, 84.6, 121) * millimeter, "radius" : 4.5 * millimeter });
    coneOp(context, A0 + "s13", vector(67.95, 84, 121) * millimeter, vector(67.95, 81.4, 121) * millimeter, 4, 4.5);
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(67.95, 81.4, 121) * millimeter, "topCenter" : vector(67.95, 81, 121) * millimeter, "radius" : 4.5 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "ventR_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "ventR_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(67.95, 49, 121) * millimeter, "topCenter" : vector(67.95, 66.5, 121) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(67.95, 65.5, 121) * millimeter, "topCenter" : vector(67.95, 89, 121) * millimeter, "radius" : 2.5 * millimeter });
    fCuboid(context, C1 + "o2", { "corner1" : vector(48.25, 56, 107) * millimeter, "corner2" : vector(51.65, 76, 112) * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(49.95, 56, 107) * millimeter, "topCenter" : vector(49.95, 56, 112) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(49.95, 76, 107) * millimeter, "topCenter" : vector(49.95, 76, 112) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o5", { "corner1" : vector(84.25, 56, 107) * millimeter, "corner2" : vector(87.65, 76, 112) * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(85.95, 56, 107) * millimeter, "topCenter" : vector(85.95, 56, 112) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(85.95, 76, 107) * millimeter, "topCenter" : vector(85.95, 76, 112) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "ventR_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Ablass rechts (lo)", color(0.55, 0.6, 0.65));
}

function dev_bridge2(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "bridge2_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-92.85, -63, 4) * millimeter, "corner2" : vector(92.85, -62, 44) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-92.85, -63, 4) * millimeter, "corner2" : vector(-84.85, -36, 8) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(84.85, -63, 4) * millimeter, "corner2" : vector(92.85, -36, 8) * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(-83.45, -62.1, 9.5) * millimeter, "topCenter" : vector(-83.45, -60, 9.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(-83.45, -62.1, 40.5) * millimeter, "topCenter" : vector(-83.45, -60, 40.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(-52.45, -62.1, 9.5) * millimeter, "topCenter" : vector(-52.45, -60, 9.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(-52.45, -62.1, 40.5) * millimeter, "topCenter" : vector(-52.45, -60, 40.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(-38.15, -62.1, 9.5) * millimeter, "topCenter" : vector(-38.15, -60, 9.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(-38.15, -62.1, 40.5) * millimeter, "topCenter" : vector(-38.15, -60, 40.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s9", { "bottomCenter" : vector(-7.15, -62.1, 9.5) * millimeter, "topCenter" : vector(-7.15, -60, 9.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(-7.15, -62.1, 40.5) * millimeter, "topCenter" : vector(-7.15, -60, 40.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s11", { "bottomCenter" : vector(7.15, -62.1, 9.5) * millimeter, "topCenter" : vector(7.15, -60, 9.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(7.15, -62.1, 40.5) * millimeter, "topCenter" : vector(7.15, -60, 40.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s13", { "bottomCenter" : vector(38.15, -62.1, 9.5) * millimeter, "topCenter" : vector(38.15, -60, 9.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(38.15, -62.1, 40.5) * millimeter, "topCenter" : vector(38.15, -60, 40.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s15", { "bottomCenter" : vector(52.45, -62.1, 9.5) * millimeter, "topCenter" : vector(52.45, -60, 9.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s16", { "bottomCenter" : vector(52.45, -62.1, 40.5) * millimeter, "topCenter" : vector(52.45, -60, 40.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s17", { "bottomCenter" : vector(83.45, -62.1, 9.5) * millimeter, "topCenter" : vector(83.45, -60, 9.5) * millimeter, "radius" : 3.75 * millimeter });
    fCylinder(context, A0 + "s18", { "bottomCenter" : vector(83.45, -62.1, 40.5) * millimeter, "topCenter" : vector(83.45, -60, 40.5) * millimeter, "radius" : 3.75 * millimeter });
    fCuboid(context, A0 + "s19", { "corner1" : vector(-91.1, -62.1, 4) * millimeter, "corner2" : vector(-90.1, -59, 44) * millimeter });
    fCuboid(context, A0 + "s20", { "corner1" : vector(-45.8, -62.1, 4) * millimeter, "corner2" : vector(-44.8, -59, 44) * millimeter });
    fCuboid(context, A0 + "s21", { "corner1" : vector(-0.5, -62.1, 4) * millimeter, "corner2" : vector(0.5, -59, 44) * millimeter });
    fCuboid(context, A0 + "s22", { "corner1" : vector(44.8, -62.1, 4) * millimeter, "corner2" : vector(45.8, -59, 44) * millimeter });
    fCuboid(context, A0 + "s23", { "corner1" : vector(90.1, -62.1, 4) * millimeter, "corner2" : vector(91.1, -59, 44) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "bridge2_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "bridge2_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-67.95, -64, 25) * millimeter, "topCenter" : vector(-67.95, -61, 25) * millimeter, "radius" : 12 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-83.45, -64, 9.5) * millimeter, "topCenter" : vector(-83.45, -59, 9.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(-83.45, -64, 40.5) * millimeter, "topCenter" : vector(-83.45, -59, 40.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(-52.45, -64, 9.5) * millimeter, "topCenter" : vector(-52.45, -59, 9.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(-52.45, -64, 40.5) * millimeter, "topCenter" : vector(-52.45, -59, 40.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(-22.65, -64, 25) * millimeter, "topCenter" : vector(-22.65, -61, 25) * millimeter, "radius" : 12 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(-38.15, -64, 9.5) * millimeter, "topCenter" : vector(-38.15, -59, 9.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(-38.15, -64, 40.5) * millimeter, "topCenter" : vector(-38.15, -59, 40.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(-7.15, -64, 9.5) * millimeter, "topCenter" : vector(-7.15, -59, 9.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(-7.15, -64, 40.5) * millimeter, "topCenter" : vector(-7.15, -59, 40.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o10", { "bottomCenter" : vector(22.65, -64, 25) * millimeter, "topCenter" : vector(22.65, -61, 25) * millimeter, "radius" : 12 * millimeter });
    fCylinder(context, C1 + "o11", { "bottomCenter" : vector(7.15, -64, 9.5) * millimeter, "topCenter" : vector(7.15, -59, 9.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o12", { "bottomCenter" : vector(7.15, -64, 40.5) * millimeter, "topCenter" : vector(7.15, -59, 40.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o13", { "bottomCenter" : vector(38.15, -64, 9.5) * millimeter, "topCenter" : vector(38.15, -59, 9.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o14", { "bottomCenter" : vector(38.15, -64, 40.5) * millimeter, "topCenter" : vector(38.15, -59, 40.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o15", { "bottomCenter" : vector(67.95, -64, 25) * millimeter, "topCenter" : vector(67.95, -61, 25) * millimeter, "radius" : 12 * millimeter });
    fCylinder(context, C1 + "o16", { "bottomCenter" : vector(52.45, -64, 9.5) * millimeter, "topCenter" : vector(52.45, -59, 9.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o17", { "bottomCenter" : vector(52.45, -64, 40.5) * millimeter, "topCenter" : vector(52.45, -59, 40.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o18", { "bottomCenter" : vector(83.45, -64, 9.5) * millimeter, "topCenter" : vector(83.45, -59, 9.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o19", { "bottomCenter" : vector(83.45, -64, 40.5) * millimeter, "topCenter" : vector(83.45, -59, 40.5) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o20", { "corner1" : vector(-90.85, -59, 3) * millimeter, "corner2" : vector(-87.45, -39, 9) * millimeter });
    fCylinder(context, C1 + "o21", { "bottomCenter" : vector(-89.15, -59, 3) * millimeter, "topCenter" : vector(-89.15, -59, 9) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o22", { "bottomCenter" : vector(-89.15, -39, 3) * millimeter, "topCenter" : vector(-89.15, -39, 9) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o23", { "corner1" : vector(87.45, -59, 3) * millimeter, "corner2" : vector(90.85, -39, 9) * millimeter });
    fCylinder(context, C1 + "o24", { "bottomCenter" : vector(89.15, -59, 3) * millimeter, "topCenter" : vector(89.15, -59, 9) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o25", { "bottomCenter" : vector(89.15, -39, 3) * millimeter, "topCenter" : vector(89.15, -39, 9) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "bridge2_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Motorbrücke (verschiebbar)", color(0.87, 0.81, 0.69));
}

function dev_plug1(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "plug1_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(-67.95, -54, 25) * millimeter, "topCenter" : vector(-67.95, -34, 25) * millimeter, "radius" : 5.5 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "plug1_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "plug1_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-67.95, -55, 25) * millimeter, "topCenter" : vector(-67.95, -44, 25) * millimeter, "radius" : 2.575 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-67.95, -44.01, 25) * millimeter, "topCenter" : vector(-67.95, -33, 25) * millimeter, "radius" : 2.075 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "plug1_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Körper Stufe 2";
    const A2 = id + "plug1_a2";
    fCuboid(context, A2 + "s0", { "corner1" : vector(-70.95, -54, 27.1) * millimeter, "corner2" : vector(-64.95, -44, 28.5) * millimeter });
    fCuboid(context, A2 + "s1", { "corner1" : vector(-70.95, -43.5, 26.6) * millimeter, "corner2" : vector(-64.95, -35.5, 28.5) * millimeter });
    prog[] = "Vereinigen Stufe 2";
    unionAll(context, id + "plug1_u2", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]), "Steckkupplung 1", color(0.93, 0.55, 0.2));
}

function dev_plug2(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "plug2_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(-22.65, -54, 25) * millimeter, "topCenter" : vector(-22.65, -34, 25) * millimeter, "radius" : 5.5 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "plug2_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "plug2_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-22.65, -55, 25) * millimeter, "topCenter" : vector(-22.65, -44, 25) * millimeter, "radius" : 2.575 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-22.65, -44.01, 25) * millimeter, "topCenter" : vector(-22.65, -33, 25) * millimeter, "radius" : 2.075 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "plug2_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Körper Stufe 2";
    const A2 = id + "plug2_a2";
    fCuboid(context, A2 + "s0", { "corner1" : vector(-25.65, -54, 27.1) * millimeter, "corner2" : vector(-19.65, -44, 28.5) * millimeter });
    fCuboid(context, A2 + "s1", { "corner1" : vector(-25.65, -43.5, 26.6) * millimeter, "corner2" : vector(-19.65, -35.5, 28.5) * millimeter });
    prog[] = "Vereinigen Stufe 2";
    unionAll(context, id + "plug2_u2", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]), "Steckkupplung 2", color(0.93, 0.55, 0.2));
}

function dev_plug3(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "plug3_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(22.65, -54, 25) * millimeter, "topCenter" : vector(22.65, -34, 25) * millimeter, "radius" : 5.5 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "plug3_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "plug3_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(22.65, -55, 25) * millimeter, "topCenter" : vector(22.65, -44, 25) * millimeter, "radius" : 2.575 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(22.65, -44.01, 25) * millimeter, "topCenter" : vector(22.65, -33, 25) * millimeter, "radius" : 2.075 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "plug3_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Körper Stufe 2";
    const A2 = id + "plug3_a2";
    fCuboid(context, A2 + "s0", { "corner1" : vector(19.65, -54, 27.1) * millimeter, "corner2" : vector(25.65, -44, 28.5) * millimeter });
    fCuboid(context, A2 + "s1", { "corner1" : vector(19.65, -43.5, 26.6) * millimeter, "corner2" : vector(25.65, -35.5, 28.5) * millimeter });
    prog[] = "Vereinigen Stufe 2";
    unionAll(context, id + "plug3_u2", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]), "Steckkupplung 3", color(0.93, 0.55, 0.2));
}

function dev_plug4(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "plug4_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(67.95, -54, 25) * millimeter, "topCenter" : vector(67.95, -34, 25) * millimeter, "radius" : 5.5 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "plug4_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "plug4_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(67.95, -55, 25) * millimeter, "topCenter" : vector(67.95, -44, 25) * millimeter, "radius" : 2.575 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(67.95, -44.01, 25) * millimeter, "topCenter" : vector(67.95, -33, 25) * millimeter, "radius" : 2.075 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "plug4_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Körper Stufe 2";
    const A2 = id + "plug4_a2";
    fCuboid(context, A2 + "s0", { "corner1" : vector(64.95, -54, 27.1) * millimeter, "corner2" : vector(70.95, -44, 28.5) * millimeter });
    fCuboid(context, A2 + "s1", { "corner1" : vector(64.95, -43.5, 26.6) * millimeter, "corner2" : vector(70.95, -35.5, 28.5) * millimeter });
    prog[] = "Vereinigen Stufe 2";
    unionAll(context, id + "plug4_u2", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]), "Steckkupplung 4", color(0.93, 0.55, 0.2));
}

function dev_camcol(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "camcol_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-27, -140, 7) * millimeter, "corner2" : vector(27, -116, 10) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-20, -128, 7) * millimeter, "corner2" : vector(-10, -125, 126) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(-16, -140, 7) * millimeter, "corner2" : vector(-14, -125, 40) * millimeter });
    fCuboid(context, A0 + "s3", { "corner1" : vector(10, -128, 7) * millimeter, "corner2" : vector(20, -125, 126) * millimeter });
    fCuboid(context, A0 + "s4", { "corner1" : vector(14, -140, 7) * millimeter, "corner2" : vector(16, -125, 40) * millimeter });
    fCuboid(context, A0 + "s5", { "corner1" : vector(-20, -128, 123) * millimeter, "corner2" : vector(20, -125, 126) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "camcol_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "camcol_c1";
    fCuboid(context, C1 + "o0", { "corner1" : vector(-16.7, -129, 25) * millimeter, "corner2" : vector(-13.3, -124, 119) * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-15, -129, 25) * millimeter, "topCenter" : vector(-15, -124, 25) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(-15, -129, 119) * millimeter, "topCenter" : vector(-15, -124, 119) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o3", { "corner1" : vector(-24.2, -137, 6) * millimeter, "corner2" : vector(-20.8, -119, 11) * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(-22.5, -137, 6) * millimeter, "topCenter" : vector(-22.5, -137, 11) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(-22.5, -119, 6) * millimeter, "topCenter" : vector(-22.5, -119, 11) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o6", { "corner1" : vector(13.3, -129, 25) * millimeter, "corner2" : vector(16.7, -124, 119) * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(15, -129, 25) * millimeter, "topCenter" : vector(15, -124, 25) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(15, -129, 119) * millimeter, "topCenter" : vector(15, -124, 119) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o9", { "corner1" : vector(20.8, -137, 6) * millimeter, "corner2" : vector(24.2, -119, 11) * millimeter });
    fCylinder(context, C1 + "o10", { "bottomCenter" : vector(22.5, -137, 6) * millimeter, "topCenter" : vector(22.5, -137, 11) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o11", { "bottomCenter" : vector(22.5, -119, 6) * millimeter, "topCenter" : vector(22.5, -119, 11) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "camcol_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Kamerasäule (Y- und Z-verstellbar)", color(0.87, 0.81, 0.69));
}

function dev_camMount(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "camMount_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-20, -125, 64) * millimeter, "corner2" : vector(20, -122, 122) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(-10.5, -122.5, 77.5) * millimeter, "topCenter" : vector(-10.5, -118, 77.5) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-10.5, -122.5, 90.5) * millimeter, "topCenter" : vector(-10.5, -118, 90.5) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(10.5, -122.5, 77.5) * millimeter, "topCenter" : vector(10.5, -118, 77.5) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(10.5, -122.5, 90.5) * millimeter, "topCenter" : vector(10.5, -118, 90.5) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(0, -122.5, 110) * millimeter, "topCenter" : vector(0, -92, 110) * millimeter, "radius" : 9.25 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "camMount_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "camMount_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-15, -126, 70) * millimeter, "topCenter" : vector(-15, -121, 70) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-15, -126, 116) * millimeter, "topCenter" : vector(-15, -121, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(-10.5, -124, 77.5) * millimeter, "topCenter" : vector(-10.5, -117, 77.5) * millimeter, "radius" : 0.9 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(-10.5, -124, 90.5) * millimeter, "topCenter" : vector(-10.5, -117, 90.5) * millimeter, "radius" : 0.9 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(15, -126, 70) * millimeter, "topCenter" : vector(15, -121, 70) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(15, -126, 116) * millimeter, "topCenter" : vector(15, -121, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(10.5, -124, 77.5) * millimeter, "topCenter" : vector(10.5, -117, 77.5) * millimeter, "radius" : 0.9 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(10.5, -124, 90.5) * millimeter, "topCenter" : vector(10.5, -117, 90.5) * millimeter, "radius" : 0.9 * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(0, -126, 110) * millimeter, "topCenter" : vector(0, -91, 110) * millimeter, "radius" : 7.25 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "camMount_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Kamerahalter (Pi-Cam + LED-Röhre)", color(0.22, 0.22, 0.24));
}

function dev_nemaD1(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "nemaD1_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-89.1, -86, 3.85) * millimeter, "corner2" : vector(-46.8, -63, 46.15) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(-67.95, -63.1, 25) * millimeter, "topCenter" : vector(-67.95, -61, 25) * millimeter, "radius" : 11 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-67.95, -63.1, 25) * millimeter, "topCenter" : vector(-67.95, -44, 25) * millimeter, "radius" : 2.5 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "nemaD1_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "NEMA 17 #1 (Dummy)", color(0.3, 0.3, 0.32));
}

function dev_lzqD1(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "lzqD1_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-77.95, -22, 13) * millimeter, "corner2" : vector(-57.95, 0, 133) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(-67.95, -0.1, 25) * millimeter, "topCenter" : vector(-67.95, 11, 25) * millimeter, "radius" : 4.75 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-67.95, 2, 25) * millimeter, "topCenter" : vector(-67.95, 5, 25) * millimeter, "radius" : 7.5 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(-67.95, 10.9, 25) * millimeter, "topCenter" : vector(-67.95, 25, 25) * millimeter, "radius" : 3.5 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(-67.95, 21, 25) * millimeter, "topCenter" : vector(-67.95, 24.5, 25) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(-67.95, 16.5, 25) * millimeter, "topCenter" : vector(-67.95, 20, 25) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(-67.95, 12, 25) * millimeter, "topCenter" : vector(-67.95, 15.5, 25) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(-67.95, -0.1, 121) * millimeter, "topCenter" : vector(-67.95, 11, 121) * millimeter, "radius" : 4.75 * millimeter });
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(-67.95, 2, 121) * millimeter, "topCenter" : vector(-67.95, 5, 121) * millimeter, "radius" : 7.5 * millimeter });
    fCylinder(context, A0 + "s9", { "bottomCenter" : vector(-67.95, 10.9, 121) * millimeter, "topCenter" : vector(-67.95, 25, 121) * millimeter, "radius" : 3.5 * millimeter });
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(-67.95, 21, 121) * millimeter, "topCenter" : vector(-67.95, 24.5, 121) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s11", { "bottomCenter" : vector(-67.95, 16.5, 121) * millimeter, "topCenter" : vector(-67.95, 20, 121) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(-67.95, 12, 121) * millimeter, "topCenter" : vector(-67.95, 15.5, 121) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s13", { "bottomCenter" : vector(-67.95, -24.5, 25) * millimeter, "topCenter" : vector(-67.95, -21.9, 25) * millimeter, "radius" : 6.928 * millimeter });
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(-67.95, -42, 25) * millimeter, "topCenter" : vector(-67.95, -24.4, 25) * millimeter, "radius" : 2 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "lzqD1_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "LZQ #1 (Dummy)", color(0.7, 0.85, 0.95));
}

function dev_nemaD2(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "nemaD2_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-43.8, -86, 3.85) * millimeter, "corner2" : vector(-1.5, -63, 46.15) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(-22.65, -63.1, 25) * millimeter, "topCenter" : vector(-22.65, -61, 25) * millimeter, "radius" : 11 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-22.65, -63.1, 25) * millimeter, "topCenter" : vector(-22.65, -44, 25) * millimeter, "radius" : 2.5 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "nemaD2_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "NEMA 17 #2 (Dummy)", color(0.3, 0.3, 0.32));
}

function dev_lzqD2(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "lzqD2_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-32.65, -22, 13) * millimeter, "corner2" : vector(-12.65, 0, 133) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(-22.65, -0.1, 25) * millimeter, "topCenter" : vector(-22.65, 11, 25) * millimeter, "radius" : 4.75 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-22.65, 2, 25) * millimeter, "topCenter" : vector(-22.65, 5, 25) * millimeter, "radius" : 7.5 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(-22.65, 10.9, 25) * millimeter, "topCenter" : vector(-22.65, 25, 25) * millimeter, "radius" : 3.5 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(-22.65, 21, 25) * millimeter, "topCenter" : vector(-22.65, 24.5, 25) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(-22.65, 16.5, 25) * millimeter, "topCenter" : vector(-22.65, 20, 25) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(-22.65, 12, 25) * millimeter, "topCenter" : vector(-22.65, 15.5, 25) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(-22.65, -0.1, 121) * millimeter, "topCenter" : vector(-22.65, 11, 121) * millimeter, "radius" : 4.75 * millimeter });
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(-22.65, 2, 121) * millimeter, "topCenter" : vector(-22.65, 5, 121) * millimeter, "radius" : 7.5 * millimeter });
    fCylinder(context, A0 + "s9", { "bottomCenter" : vector(-22.65, 10.9, 121) * millimeter, "topCenter" : vector(-22.65, 25, 121) * millimeter, "radius" : 3.5 * millimeter });
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(-22.65, 21, 121) * millimeter, "topCenter" : vector(-22.65, 24.5, 121) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s11", { "bottomCenter" : vector(-22.65, 16.5, 121) * millimeter, "topCenter" : vector(-22.65, 20, 121) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(-22.65, 12, 121) * millimeter, "topCenter" : vector(-22.65, 15.5, 121) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s13", { "bottomCenter" : vector(-22.65, -24.5, 25) * millimeter, "topCenter" : vector(-22.65, -21.9, 25) * millimeter, "radius" : 6.928 * millimeter });
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(-22.65, -42, 25) * millimeter, "topCenter" : vector(-22.65, -24.4, 25) * millimeter, "radius" : 2 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "lzqD2_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "LZQ #2 (Dummy)", color(0.7, 0.85, 0.95));
}

function dev_nemaD3(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "nemaD3_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(1.5, -86, 3.85) * millimeter, "corner2" : vector(43.8, -63, 46.15) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(22.65, -63.1, 25) * millimeter, "topCenter" : vector(22.65, -61, 25) * millimeter, "radius" : 11 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(22.65, -63.1, 25) * millimeter, "topCenter" : vector(22.65, -44, 25) * millimeter, "radius" : 2.5 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "nemaD3_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "NEMA 17 #3 (Dummy)", color(0.3, 0.3, 0.32));
}

function dev_lzqD3(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "lzqD3_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(12.65, -22, 13) * millimeter, "corner2" : vector(32.65, 0, 133) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(22.65, -0.1, 25) * millimeter, "topCenter" : vector(22.65, 11, 25) * millimeter, "radius" : 4.75 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(22.65, 2, 25) * millimeter, "topCenter" : vector(22.65, 5, 25) * millimeter, "radius" : 7.5 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(22.65, 10.9, 25) * millimeter, "topCenter" : vector(22.65, 25, 25) * millimeter, "radius" : 3.5 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(22.65, 21, 25) * millimeter, "topCenter" : vector(22.65, 24.5, 25) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(22.65, 16.5, 25) * millimeter, "topCenter" : vector(22.65, 20, 25) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(22.65, 12, 25) * millimeter, "topCenter" : vector(22.65, 15.5, 25) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(22.65, -0.1, 121) * millimeter, "topCenter" : vector(22.65, 11, 121) * millimeter, "radius" : 4.75 * millimeter });
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(22.65, 2, 121) * millimeter, "topCenter" : vector(22.65, 5, 121) * millimeter, "radius" : 7.5 * millimeter });
    fCylinder(context, A0 + "s9", { "bottomCenter" : vector(22.65, 10.9, 121) * millimeter, "topCenter" : vector(22.65, 25, 121) * millimeter, "radius" : 3.5 * millimeter });
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(22.65, 21, 121) * millimeter, "topCenter" : vector(22.65, 24.5, 121) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s11", { "bottomCenter" : vector(22.65, 16.5, 121) * millimeter, "topCenter" : vector(22.65, 20, 121) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(22.65, 12, 121) * millimeter, "topCenter" : vector(22.65, 15.5, 121) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s13", { "bottomCenter" : vector(22.65, -24.5, 25) * millimeter, "topCenter" : vector(22.65, -21.9, 25) * millimeter, "radius" : 6.928 * millimeter });
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(22.65, -42, 25) * millimeter, "topCenter" : vector(22.65, -24.4, 25) * millimeter, "radius" : 2 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "lzqD3_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "LZQ #3 (Dummy)", color(0.7, 0.85, 0.95));
}

function dev_nemaD4(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "nemaD4_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(46.8, -86, 3.85) * millimeter, "corner2" : vector(89.1, -63, 46.15) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(67.95, -63.1, 25) * millimeter, "topCenter" : vector(67.95, -61, 25) * millimeter, "radius" : 11 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(67.95, -63.1, 25) * millimeter, "topCenter" : vector(67.95, -44, 25) * millimeter, "radius" : 2.5 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "nemaD4_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "NEMA 17 #4 (Dummy)", color(0.3, 0.3, 0.32));
}

function dev_lzqD4(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "lzqD4_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(57.95, -22, 13) * millimeter, "corner2" : vector(77.95, 0, 133) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(67.95, -0.1, 25) * millimeter, "topCenter" : vector(67.95, 11, 25) * millimeter, "radius" : 4.75 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(67.95, 2, 25) * millimeter, "topCenter" : vector(67.95, 5, 25) * millimeter, "radius" : 7.5 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(67.95, 10.9, 25) * millimeter, "topCenter" : vector(67.95, 25, 25) * millimeter, "radius" : 3.5 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(67.95, 21, 25) * millimeter, "topCenter" : vector(67.95, 24.5, 25) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(67.95, 16.5, 25) * millimeter, "topCenter" : vector(67.95, 20, 25) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(67.95, 12, 25) * millimeter, "topCenter" : vector(67.95, 15.5, 25) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(67.95, -0.1, 121) * millimeter, "topCenter" : vector(67.95, 11, 121) * millimeter, "radius" : 4.75 * millimeter });
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(67.95, 2, 121) * millimeter, "topCenter" : vector(67.95, 5, 121) * millimeter, "radius" : 7.5 * millimeter });
    fCylinder(context, A0 + "s9", { "bottomCenter" : vector(67.95, 10.9, 121) * millimeter, "topCenter" : vector(67.95, 25, 121) * millimeter, "radius" : 3.5 * millimeter });
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(67.95, 21, 121) * millimeter, "topCenter" : vector(67.95, 24.5, 121) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s11", { "bottomCenter" : vector(67.95, 16.5, 121) * millimeter, "topCenter" : vector(67.95, 20, 121) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(67.95, 12, 121) * millimeter, "topCenter" : vector(67.95, 15.5, 121) * millimeter, "radius" : 4.25 * millimeter });
    fCylinder(context, A0 + "s13", { "bottomCenter" : vector(67.95, -24.5, 25) * millimeter, "topCenter" : vector(67.95, -21.9, 25) * millimeter, "radius" : 6.928 * millimeter });
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(67.95, -42, 25) * millimeter, "topCenter" : vector(67.95, -24.4, 25) * millimeter, "radius" : 2 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "lzqD4_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "LZQ #4 (Dummy)", color(0.7, 0.85, 0.95));
}

function dev_ledD(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "ledD_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(0, -150, 110) * millimeter, "topCenter" : vector(0, -92.5, 110) * millimeter, "radius" : 7 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "ledD_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Schlüsselanhänger-LED (Dummy)", color(0.1, 0.1, 0.1));
}

function dev_camD(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "camD_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-12.5, -118, 72) * millimeter, "corner2" : vector(12.5, -117, 96) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-4, -117.1, 80) * millimeter, "corner2" : vector(4, -112, 88) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "camD_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Pi-Cam OV5647 (Dummy)", color(0.1, 0.35, 0.15));
}

annotation { "Feature Type Name" : "IHHT Entwicklung v2" }
export const ihhtDevV2 = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
    }
    {
        const prog = new box("");
        var failed = [];
        var okCount = 0;
        try
        {
            dev_frame2(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "frame2 @ " ~ prog[] ~ " -> " ~ toString(error));
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
            dev_bridge2(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "bridge2 @ " ~ prog[] ~ " -> " ~ toString(error));
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
            dev_camcol(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "camcol @ " ~ prog[] ~ " -> " ~ toString(error));
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

function arc_v1_base(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "v1_base_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(358.85, -92.7, 0) * millimeter, "corner2" : vector(541.15, 68.2, 50) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(350.85, -84.7, 0) * millimeter, "corner2" : vector(549.15, 60.2, 50) * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(358.85, -84.7, 0) * millimeter, "topCenter" : vector(358.85, -84.7, 50) * millimeter, "radius" : 8 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(541.15, -84.7, 0) * millimeter, "topCenter" : vector(541.15, -84.7, 50) * millimeter, "radius" : 8 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(358.85, 60.2, 0) * millimeter, "topCenter" : vector(358.85, 60.2, 50) * millimeter, "radius" : 8 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(541.15, 60.2, 0) * millimeter, "topCenter" : vector(541.15, 60.2, 50) * millimeter, "radius" : 8 * millimeter });
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(364.85, -78.7, -2) * millimeter, "topCenter" : vector(364.85, -78.7, 0.5) * millimeter, "radius" : 7 * millimeter });
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(364.85, 54.2, -2) * millimeter, "topCenter" : vector(364.85, 54.2, 0.5) * millimeter, "radius" : 7 * millimeter });
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(535.15, -78.7, -2) * millimeter, "topCenter" : vector(535.15, -78.7, 0.5) * millimeter, "radius" : 7 * millimeter });
    fCylinder(context, A0 + "s9", { "bottomCenter" : vector(535.15, 54.2, -2) * millimeter, "topCenter" : vector(535.15, 54.2, 0.5) * millimeter, "radius" : 7 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "v1_base_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "v1_base_c1";
    fCuboid(context, C1 + "o0", { "corner1" : vector(358.85, -89.2, 3) * millimeter, "corner2" : vector(541.15, 64.7, 51) * millimeter });
    fCuboid(context, C1 + "o1", { "corner1" : vector(354.35, -84.7, 3) * millimeter, "corner2" : vector(545.65, 60.2, 51) * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(358.85, -84.7, 3) * millimeter, "topCenter" : vector(358.85, -84.7, 51) * millimeter, "radius" : 4.5 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(541.15, -84.7, 3) * millimeter, "topCenter" : vector(541.15, -84.7, 51) * millimeter, "radius" : 4.5 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(358.85, 60.2, 3) * millimeter, "topCenter" : vector(358.85, 60.2, 51) * millimeter, "radius" : 4.5 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(541.15, 60.2, 3) * millimeter, "topCenter" : vector(541.15, 60.2, 51) * millimeter, "radius" : 4.5 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "v1_base_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Körper Stufe 2";
    const A2 = id + "v1_base_a2";
    fCuboid(context, A2 + "s0", { "corner1" : vector(357.85, -82.5, 2) * millimeter, "corner2" : vector(365.85, -74.5, 50) * millimeter });
    fCuboid(context, A2 + "s1", { "corner1" : vector(357.85, 50, 2) * millimeter, "corner2" : vector(365.85, 58, 50) * millimeter });
    fCuboid(context, A2 + "s2", { "corner1" : vector(534.15, -82.5, 2) * millimeter, "corner2" : vector(542.15, -74.5, 50) * millimeter });
    fCuboid(context, A2 + "s3", { "corner1" : vector(534.15, 50, 2) * millimeter, "corner2" : vector(542.15, 58, 50) * millimeter });
    fCylinder(context, A2 + "s4", { "bottomCenter" : vector(447.5, -27.5, 2) * millimeter, "topCenter" : vector(447.5, -27.5, 9) * millimeter, "radius" : 3 * millimeter });
    fCylinder(context, A2 + "s5", { "bottomCenter" : vector(389.5, -27.5, 2) * millimeter, "topCenter" : vector(389.5, -27.5, 9) * millimeter, "radius" : 3 * millimeter });
    fCylinder(context, A2 + "s6", { "bottomCenter" : vector(447.5, -76.5, 2) * millimeter, "topCenter" : vector(447.5, -76.5, 9) * millimeter, "radius" : 3 * millimeter });
    fCylinder(context, A2 + "s7", { "bottomCenter" : vector(389.5, -76.5, 2) * millimeter, "topCenter" : vector(389.5, -76.5, 9) * millimeter, "radius" : 3 * millimeter });
    fCylinder(context, A2 + "s8", { "bottomCenter" : vector(519.63, -29.24, 2) * millimeter, "topCenter" : vector(519.63, -29.24, 9) * millimeter, "radius" : 3 * millimeter });
    fCylinder(context, A2 + "s9", { "bottomCenter" : vector(518.36, -77.5, 2) * millimeter, "topCenter" : vector(518.36, -77.5, 9) * millimeter, "radius" : 3 * millimeter });
    fCylinder(context, A2 + "s10", { "bottomCenter" : vector(467.56, -34.32, 2) * millimeter, "topCenter" : vector(467.56, -34.32, 9) * millimeter, "radius" : 3 * millimeter });
    fCylinder(context, A2 + "s11", { "bottomCenter" : vector(467.56, -62.26, 2) * millimeter, "topCenter" : vector(467.56, -62.26, 9) * millimeter, "radius" : 3 * millimeter });
    fCuboid(context, A2 + "s12", { "corner1" : vector(386, 4, 2) * millimeter, "corner2" : vector(390, 18, 14) * millimeter });
    fCuboid(context, A2 + "s13", { "corner1" : vector(386, 4, 2) * millimeter, "corner2" : vector(400, 8, 14) * millimeter });
    fCuboid(context, A2 + "s14", { "corner1" : vector(386, 48, 2) * millimeter, "corner2" : vector(390, 62, 14) * millimeter });
    fCuboid(context, A2 + "s15", { "corner1" : vector(386, 58, 2) * millimeter, "corner2" : vector(400, 62, 14) * millimeter });
    fCuboid(context, A2 + "s16", { "corner1" : vector(510, 4, 2) * millimeter, "corner2" : vector(514, 18, 14) * millimeter });
    fCuboid(context, A2 + "s17", { "corner1" : vector(500, 4, 2) * millimeter, "corner2" : vector(514, 8, 14) * millimeter });
    fCuboid(context, A2 + "s18", { "corner1" : vector(510, 48, 2) * millimeter, "corner2" : vector(514, 62, 14) * millimeter });
    fCuboid(context, A2 + "s19", { "corner1" : vector(500, 58, 2) * millimeter, "corner2" : vector(514, 62, 14) * millimeter });
    prog[] = "Vereinigen Stufe 2";
    unionAll(context, id + "v1_base_u2", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 3";
    const C3 = id + "v1_base_c3";
    fCylinder(context, C3 + "o0", { "bottomCenter" : vector(361.85, -78.5, 34) * millimeter, "topCenter" : vector(361.85, -78.5, 51) * millimeter, "radius" : 1.6 * millimeter });
    fCuboid(context, C3 + "o1", { "corner1" : vector(358.9, -81.45, 42) * millimeter, "corner2" : vector(366.45, -75.55, 44.8) * millimeter });
    fCylinder(context, C3 + "o2", { "bottomCenter" : vector(361.85, 54, 34) * millimeter, "topCenter" : vector(361.85, 54, 51) * millimeter, "radius" : 1.6 * millimeter });
    fCuboid(context, C3 + "o3", { "corner1" : vector(358.9, 51.05, 42) * millimeter, "corner2" : vector(366.45, 56.95, 44.8) * millimeter });
    fCylinder(context, C3 + "o4", { "bottomCenter" : vector(538.15, -78.5, 34) * millimeter, "topCenter" : vector(538.15, -78.5, 51) * millimeter, "radius" : 1.6 * millimeter });
    fCuboid(context, C3 + "o5", { "corner1" : vector(533.55, -81.45, 42) * millimeter, "corner2" : vector(541.1, -75.55, 44.8) * millimeter });
    fCylinder(context, C3 + "o6", { "bottomCenter" : vector(538.15, 54, 34) * millimeter, "topCenter" : vector(538.15, 54, 51) * millimeter, "radius" : 1.6 * millimeter });
    fCuboid(context, C3 + "o7", { "corner1" : vector(533.55, 51.05, 42) * millimeter, "corner2" : vector(541.1, 56.95, 44.8) * millimeter });
    fCylinder(context, C3 + "o8", { "bottomCenter" : vector(447.5, -27.5, 4) * millimeter, "topCenter" : vector(447.5, -27.5, 10) * millimeter, "radius" : 0.9 * millimeter });
    fCylinder(context, C3 + "o9", { "bottomCenter" : vector(389.5, -27.5, 4) * millimeter, "topCenter" : vector(389.5, -27.5, 10) * millimeter, "radius" : 0.9 * millimeter });
    fCylinder(context, C3 + "o10", { "bottomCenter" : vector(447.5, -76.5, 4) * millimeter, "topCenter" : vector(447.5, -76.5, 10) * millimeter, "radius" : 0.9 * millimeter });
    fCylinder(context, C3 + "o11", { "bottomCenter" : vector(389.5, -76.5, 4) * millimeter, "topCenter" : vector(389.5, -76.5, 10) * millimeter, "radius" : 0.9 * millimeter });
    fCylinder(context, C3 + "o12", { "bottomCenter" : vector(519.63, -29.24, 4) * millimeter, "topCenter" : vector(519.63, -29.24, 10) * millimeter, "radius" : 0.9 * millimeter });
    fCylinder(context, C3 + "o13", { "bottomCenter" : vector(518.36, -77.5, 4) * millimeter, "topCenter" : vector(518.36, -77.5, 10) * millimeter, "radius" : 0.9 * millimeter });
    fCylinder(context, C3 + "o14", { "bottomCenter" : vector(467.56, -34.32, 4) * millimeter, "topCenter" : vector(467.56, -34.32, 10) * millimeter, "radius" : 0.9 * millimeter });
    fCylinder(context, C3 + "o15", { "bottomCenter" : vector(467.56, -62.26, 4) * millimeter, "topCenter" : vector(467.56, -62.26, 10) * millimeter, "radius" : 0.9 * millimeter });
    fCuboid(context, C3 + "o16", { "corner1" : vector(349, -74, 9) * millimeter, "corner2" : vector(356, -26, 28) * millimeter });
    fCuboid(context, C3 + "o17", { "corner1" : vector(544, -74, 9) * millimeter, "corner2" : vector(551, -34, 26) * millimeter });
    fCylinder(context, C3 + "o18", { "bottomCenter" : vector(370, 63, 25) * millimeter, "topCenter" : vector(370, 70, 25) * millimeter, "radius" : 6 * millimeter });
    fCuboid(context, C3 + "o19", { "corner1" : vector(403, 63, 10) * millimeter, "corner2" : vector(407, 70, 34) * millimeter });
    fCuboid(context, C3 + "o20", { "corner1" : vector(418, 63, 10) * millimeter, "corner2" : vector(422, 70, 34) * millimeter });
    fCuboid(context, C3 + "o21", { "corner1" : vector(433, 63, 10) * millimeter, "corner2" : vector(437, 70, 34) * millimeter });
    fCuboid(context, C3 + "o22", { "corner1" : vector(448, 63, 10) * millimeter, "corner2" : vector(452, 70, 34) * millimeter });
    fCuboid(context, C3 + "o23", { "corner1" : vector(463, 63, 10) * millimeter, "corner2" : vector(467, 70, 34) * millimeter });
    fCuboid(context, C3 + "o24", { "corner1" : vector(478, 63, 10) * millimeter, "corner2" : vector(482, 70, 34) * millimeter });
    fCuboid(context, C3 + "o25", { "corner1" : vector(493, 63, 10) * millimeter, "corner2" : vector(497, 70, 34) * millimeter });
    fCuboid(context, C3 + "o26", { "corner1" : vector(349, 8, 12) * millimeter, "corner2" : vector(356, 12, 36) * millimeter });
    fCuboid(context, C3 + "o27", { "corner1" : vector(349, 17, 12) * millimeter, "corner2" : vector(356, 21, 36) * millimeter });
    fCuboid(context, C3 + "o28", { "corner1" : vector(349, 26, 12) * millimeter, "corner2" : vector(356, 30, 36) * millimeter });
    fCuboid(context, C3 + "o29", { "corner1" : vector(349, 35, 12) * millimeter, "corner2" : vector(356, 39, 36) * millimeter });
    fCuboid(context, C3 + "o30", { "corner1" : vector(349, 44, 12) * millimeter, "corner2" : vector(356, 48, 36) * millimeter });
    fCuboid(context, C3 + "o31", { "corner1" : vector(349, 53, 12) * millimeter, "corner2" : vector(356, 57, 36) * millimeter });
    fCuboid(context, C3 + "o32", { "corner1" : vector(544, 8, 12) * millimeter, "corner2" : vector(551, 12, 36) * millimeter });
    fCuboid(context, C3 + "o33", { "corner1" : vector(544, 17, 12) * millimeter, "corner2" : vector(551, 21, 36) * millimeter });
    fCuboid(context, C3 + "o34", { "corner1" : vector(544, 26, 12) * millimeter, "corner2" : vector(551, 30, 36) * millimeter });
    fCuboid(context, C3 + "o35", { "corner1" : vector(544, 35, 12) * millimeter, "corner2" : vector(551, 39, 36) * millimeter });
    fCuboid(context, C3 + "o36", { "corner1" : vector(544, 44, 12) * millimeter, "corner2" : vector(551, 48, 36) * millimeter });
    fCuboid(context, C3 + "o37", { "corner1" : vector(544, 53, 12) * millimeter, "corner2" : vector(551, 57, 36) * millimeter });
    prog[] = "Abziehen Stufe 3";
    cutAll(context, id + "v1_base_x3", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]), qCreatedBy(C3, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]), "[v1] Unterteil (Elektronik)", color(0.87, 0.81, 0.69));
}

function arc_v1_frame(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "v1_frame_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(358.85, -92.7, 50) * millimeter, "corner2" : vector(541.15, 68.2, 54) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(350.85, -84.7, 50) * millimeter, "corner2" : vector(549.15, 60.2, 54) * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(358.85, -84.7, 50) * millimeter, "topCenter" : vector(358.85, -84.7, 54) * millimeter, "radius" : 8 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(541.15, -84.7, 50) * millimeter, "topCenter" : vector(541.15, -84.7, 54) * millimeter, "radius" : 8 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(358.85, 60.2, 50) * millimeter, "topCenter" : vector(358.85, 60.2, 54) * millimeter, "radius" : 8 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(541.15, 60.2, 50) * millimeter, "topCenter" : vector(541.15, 60.2, 54) * millimeter, "radius" : 8 * millimeter });
    fCuboid(context, A0 + "s6", { "corner1" : vector(353.35, -90.2, 53.5) * millimeter, "corner2" : vector(356.85, 65.7, 200) * millimeter });
    fCuboid(context, A0 + "s7", { "corner1" : vector(543.15, -90.2, 53.5) * millimeter, "corner2" : vector(546.65, 65.7, 200) * millimeter });
    fCuboid(context, A0 + "s8", { "corner1" : vector(356.8, 9.5, 53.5) * millimeter, "corner2" : vector(543.2, 13.5, 200) * millimeter });
    fCuboid(context, A0 + "s9", { "corner1" : vector(356.35, -90.2, 53.5) * millimeter, "corner2" : vector(358.85, -88.2, 200) * millimeter });
    fCuboid(context, A0 + "s10", { "corner1" : vector(541.15, -90.2, 53.5) * millimeter, "corner2" : vector(543.65, -88.2, 200) * millimeter });
    fCuboid(context, A0 + "s11", { "corner1" : vector(356.8, -90.2, 53.5) * millimeter, "corner2" : vector(543.2, -88.2, 56) * millimeter });
    fCuboid(context, A0 + "s12", { "corner1" : vector(356.35, -84.3, 53.5) * millimeter, "corner2" : vector(358.85, -82.3, 200) * millimeter });
    fCuboid(context, A0 + "s13", { "corner1" : vector(541.15, -84.3, 53.5) * millimeter, "corner2" : vector(543.65, -82.3, 200) * millimeter });
    fCuboid(context, A0 + "s14", { "corner1" : vector(356.8, -84.3, 53.5) * millimeter, "corner2" : vector(543.2, -82.3, 56) * millimeter });
    fCuboid(context, A0 + "s15", { "corner1" : vector(356.35, 57.8, 53.5) * millimeter, "corner2" : vector(358.85, 59.8, 200) * millimeter });
    fCuboid(context, A0 + "s16", { "corner1" : vector(541.15, 57.8, 53.5) * millimeter, "corner2" : vector(543.65, 59.8, 200) * millimeter });
    fCuboid(context, A0 + "s17", { "corner1" : vector(356.8, 57.8, 53.5) * millimeter, "corner2" : vector(543.2, 59.8, 56) * millimeter });
    fCuboid(context, A0 + "s18", { "corner1" : vector(356.35, 63.7, 53.5) * millimeter, "corner2" : vector(358.85, 65.7, 200) * millimeter });
    fCuboid(context, A0 + "s19", { "corner1" : vector(541.15, 63.7, 53.5) * millimeter, "corner2" : vector(543.65, 65.7, 200) * millimeter });
    fCuboid(context, A0 + "s20", { "corner1" : vector(356.8, 63.7, 53.5) * millimeter, "corner2" : vector(543.2, 65.7, 56) * millimeter });
    fCuboid(context, A0 + "s21", { "corner1" : vector(356.35, -60.7, 136) * millimeter, "corner2" : vector(358.85, -58.7, 190) * millimeter });
    fCuboid(context, A0 + "s22", { "corner1" : vector(541.15, -60.7, 136) * millimeter, "corner2" : vector(543.65, -58.7, 190) * millimeter });
    fCuboid(context, A0 + "s23", { "corner1" : vector(356.35, -54.3, 136) * millimeter, "corner2" : vector(358.85, -52.3, 190) * millimeter });
    fCuboid(context, A0 + "s24", { "corner1" : vector(541.15, -54.3, 136) * millimeter, "corner2" : vector(543.65, -52.3, 190) * millimeter });
    fCuboid(context, A0 + "s25", { "corner1" : vector(356.35, -60.7, 136) * millimeter, "corner2" : vector(361.85, -52.3, 142) * millimeter });
    fCuboid(context, A0 + "s26", { "corner1" : vector(538.15, -60.7, 136) * millimeter, "corner2" : vector(543.65, -52.3, 142) * millimeter });
    fCuboid(context, A0 + "s27", { "corner1" : vector(356.35, -44, 53.5) * millimeter, "corner2" : vector(364.85, -36, 200) * millimeter });
    fCuboid(context, A0 + "s28", { "corner1" : vector(356.35, 36, 53.5) * millimeter, "corner2" : vector(364.85, 44, 200) * millimeter });
    fCuboid(context, A0 + "s29", { "corner1" : vector(535.15, -44, 53.5) * millimeter, "corner2" : vector(543.65, -36, 200) * millimeter });
    fCuboid(context, A0 + "s30", { "corner1" : vector(535.15, 36, 53.5) * millimeter, "corner2" : vector(543.65, 44, 200) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "v1_frame_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "v1_frame_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(382.05, 9, 66) * millimeter, "topCenter" : vector(382.05, 14, 66) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(382.05, 9, 162) * millimeter, "topCenter" : vector(382.05, 14, 162) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(427.35, 9, 66) * millimeter, "topCenter" : vector(427.35, 14, 66) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(427.35, 9, 162) * millimeter, "topCenter" : vector(427.35, 14, 162) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(472.65, 9, 66) * millimeter, "topCenter" : vector(472.65, 14, 66) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(472.65, 9, 162) * millimeter, "topCenter" : vector(472.65, 14, 162) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(517.95, 9, 66) * millimeter, "topCenter" : vector(517.95, 14, 66) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(517.95, 9, 162) * millimeter, "topCenter" : vector(517.95, 14, 162) * millimeter, "radius" : 5.5 * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(360.85, -40, 188) * millimeter, "topCenter" : vector(360.85, -40, 201) * millimeter, "radius" : 1.6 * millimeter });
    fCuboid(context, C1 + "o9", { "corner1" : vector(357.9, -42.95, 192) * millimeter, "corner2" : vector(365.05, -37.05, 194.8) * millimeter });
    fCylinder(context, C1 + "o10", { "bottomCenter" : vector(360.85, 40, 188) * millimeter, "topCenter" : vector(360.85, 40, 201) * millimeter, "radius" : 1.6 * millimeter });
    fCuboid(context, C1 + "o11", { "corner1" : vector(357.9, 37.05, 192) * millimeter, "corner2" : vector(365.05, 42.95, 194.8) * millimeter });
    fCylinder(context, C1 + "o12", { "bottomCenter" : vector(539.15, -40, 188) * millimeter, "topCenter" : vector(539.15, -40, 201) * millimeter, "radius" : 1.6 * millimeter });
    fCuboid(context, C1 + "o13", { "corner1" : vector(534.95, -42.95, 192) * millimeter, "corner2" : vector(542.1, -37.05, 194.8) * millimeter });
    fCylinder(context, C1 + "o14", { "bottomCenter" : vector(539.15, 40, 188) * millimeter, "topCenter" : vector(539.15, 40, 201) * millimeter, "radius" : 1.6 * millimeter });
    fCuboid(context, C1 + "o15", { "corner1" : vector(534.95, 37.05, 192) * millimeter, "corner2" : vector(542.1, 42.95, 194.8) * millimeter });
    fCylinder(context, C1 + "o16", { "bottomCenter" : vector(361.85, -78.5, 49) * millimeter, "topCenter" : vector(361.85, -78.5, 55) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o17", { "bottomCenter" : vector(361.85, -78.5, 52.5) * millimeter, "topCenter" : vector(361.85, -78.5, 55) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o18", { "bottomCenter" : vector(361.85, 54, 49) * millimeter, "topCenter" : vector(361.85, 54, 55) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o19", { "bottomCenter" : vector(361.85, 54, 52.5) * millimeter, "topCenter" : vector(361.85, 54, 55) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o20", { "bottomCenter" : vector(538.15, -78.5, 49) * millimeter, "topCenter" : vector(538.15, -78.5, 55) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o21", { "bottomCenter" : vector(538.15, -78.5, 52.5) * millimeter, "topCenter" : vector(538.15, -78.5, 55) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o22", { "bottomCenter" : vector(538.15, 54, 49) * millimeter, "topCenter" : vector(538.15, 54, 55) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o23", { "bottomCenter" : vector(538.15, 54, 52.5) * millimeter, "topCenter" : vector(538.15, 54, 55) * millimeter, "radius" : 3.1 * millimeter });
    fCuboid(context, C1 + "o24", { "corner1" : vector(375, -81.5, 49) * millimeter, "corner2" : vector(442, -74.5, 57) * millimeter });
    fCuboid(context, C1 + "o25", { "corner1" : vector(458, -81.5, 49) * millimeter, "corner2" : vector(525, -74.5, 57) * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "v1_frame_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "[v1] Rahmen", color(0.87, 0.81, 0.69));
}

function arc_v1_lid(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "v1_lid_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(358.85, -92.7, 200) * millimeter, "corner2" : vector(541.15, 68.2, 204) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(350.85, -84.7, 200) * millimeter, "corner2" : vector(549.15, 60.2, 204) * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(358.85, -84.7, 200) * millimeter, "topCenter" : vector(358.85, -84.7, 204) * millimeter, "radius" : 8 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(541.15, -84.7, 200) * millimeter, "topCenter" : vector(541.15, -84.7, 204) * millimeter, "radius" : 8 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(358.85, 60.2, 200) * millimeter, "topCenter" : vector(358.85, 60.2, 204) * millimeter, "radius" : 8 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(541.15, 60.2, 200) * millimeter, "topCenter" : vector(541.15, 60.2, 204) * millimeter, "radius" : 8 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "v1_lid_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "v1_lid_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(360.85, -40, 199) * millimeter, "topCenter" : vector(360.85, -40, 205) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(360.85, -40, 202.2) * millimeter, "topCenter" : vector(360.85, -40, 205) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(360.85, 40, 199) * millimeter, "topCenter" : vector(360.85, 40, 205) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(360.85, 40, 202.2) * millimeter, "topCenter" : vector(360.85, 40, 205) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(539.15, -40, 199) * millimeter, "topCenter" : vector(539.15, -40, 205) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(539.15, -40, 202.2) * millimeter, "topCenter" : vector(539.15, -40, 205) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(539.15, 40, 199) * millimeter, "topCenter" : vector(539.15, 40, 205) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(539.15, 40, 202.2) * millimeter, "topCenter" : vector(539.15, 40, 205) * millimeter, "radius" : 3.1 * millimeter });
    fCuboid(context, C1 + "o8", { "corner1" : vector(404, 30, 199) * millimeter, "corner2" : vector(416, 50, 205) * millimeter });
    fCuboid(context, C1 + "o9", { "corner1" : vector(424, 30, 199) * millimeter, "corner2" : vector(436, 50, 205) * millimeter });
    fCuboid(context, C1 + "o10", { "corner1" : vector(444, 30, 199) * millimeter, "corner2" : vector(456, 50, 205) * millimeter });
    fCuboid(context, C1 + "o11", { "corner1" : vector(464, 30, 199) * millimeter, "corner2" : vector(476, 50, 205) * millimeter });
    fCuboid(context, C1 + "o12", { "corner1" : vector(484, 30, 199) * millimeter, "corner2" : vector(496, 50, 205) * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "v1_lid_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "[v1] Deckel", color(0.78, 0.72, 0.6));
}

function arc_v1_front(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "v1_front_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(357, -88, 54) * millimeter, "corner2" : vector(543, -84.5, 200) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(431, -85, 94) * millimeter, "topCenter" : vector(431, -82, 94) * millimeter, "radius" : 3.5 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(431, -85, 132) * millimeter, "topCenter" : vector(431, -82, 132) * millimeter, "radius" : 3.5 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(469, -85, 94) * millimeter, "topCenter" : vector(469, -82, 94) * millimeter, "radius" : 3.5 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(469, -85, 132) * millimeter, "topCenter" : vector(469, -82, 132) * millimeter, "radius" : 3.5 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "v1_front_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "v1_front_c1";
    fCuboid(context, C1 + "o0", { "corner1" : vector(435, -89, 98) * millimeter, "corner2" : vector(465, -83.5, 128) * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(450, -89, 79) * millimeter, "topCenter" : vector(450, -83.5, 79) * millimeter, "radius" : 2.6 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(431, -89, 94) * millimeter, "topCenter" : vector(431, -81.5, 94) * millimeter, "radius" : 0.9 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(431, -89, 132) * millimeter, "topCenter" : vector(431, -81.5, 132) * millimeter, "radius" : 0.9 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(469, -89, 94) * millimeter, "topCenter" : vector(469, -81.5, 94) * millimeter, "radius" : 0.9 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(469, -89, 132) * millimeter, "topCenter" : vector(469, -81.5, 132) * millimeter, "radius" : 0.9 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "v1_front_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "[v1] Frontplatte", color(0.87, 0.81, 0.69));
}

function arc_v1_insKey(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "v1_insKey_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(427, -91, 90) * millimeter, "corner2" : vector(473, -88, 136) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(450, -88.5, 113) * millimeter, "topCenter" : vector(450, -58, 113) * millimeter, "radius" : 9.25 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "v1_insKey_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "v1_insKey_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(431, -92, 94) * millimeter, "topCenter" : vector(431, -87, 94) * millimeter, "radius" : 1.2 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(431, -92, 132) * millimeter, "topCenter" : vector(431, -87, 132) * millimeter, "radius" : 1.2 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(469, -92, 94) * millimeter, "topCenter" : vector(469, -87, 94) * millimeter, "radius" : 1.2 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(469, -92, 132) * millimeter, "topCenter" : vector(469, -87, 132) * millimeter, "radius" : 1.2 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(450, -92, 113) * millimeter, "topCenter" : vector(450, -57, 113) * millimeter, "radius" : 7.25 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "v1_insKey_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "[v1] Kamera-Einsatz Röhre (Schlüsselanhänger-Cam)", color(0.22, 0.22, 0.24));
}

function arc_v1_insPi(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "v1_insPi_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(427, -151, 90) * millimeter, "corner2" : vector(473, -148, 136) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(439.5, -148.5, 106.75) * millimeter, "topCenter" : vector(439.5, -139.5, 106.75) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(439.5, -148.5, 119.25) * millimeter, "topCenter" : vector(439.5, -139.5, 119.25) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(460.5, -148.5, 106.75) * millimeter, "topCenter" : vector(460.5, -139.5, 106.75) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(460.5, -148.5, 119.25) * millimeter, "topCenter" : vector(460.5, -139.5, 119.25) * millimeter, "radius" : 2.5 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "v1_insPi_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "v1_insPi_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(431, -152, 94) * millimeter, "topCenter" : vector(431, -147, 94) * millimeter, "radius" : 1.2 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(431, -152, 132) * millimeter, "topCenter" : vector(431, -147, 132) * millimeter, "radius" : 1.2 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(469, -152, 94) * millimeter, "topCenter" : vector(469, -147, 94) * millimeter, "radius" : 1.2 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(469, -152, 132) * millimeter, "topCenter" : vector(469, -147, 132) * millimeter, "radius" : 1.2 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(439.5, -145, 106.75) * millimeter, "topCenter" : vector(439.5, -139, 106.75) * millimeter, "radius" : 0.9 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(439.5, -145, 119.25) * millimeter, "topCenter" : vector(439.5, -139, 119.25) * millimeter, "radius" : 0.9 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(460.5, -145, 106.75) * millimeter, "topCenter" : vector(460.5, -139, 106.75) * millimeter, "radius" : 0.9 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(460.5, -145, 119.25) * millimeter, "topCenter" : vector(460.5, -139, 119.25) * millimeter, "radius" : 0.9 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "v1_insPi_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "[v1] Kamera-Einsatz Pi-Cam (explodiert)", color(0.22, 0.22, 0.24));
}

function arc_v1_bridge(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "v1_bridge_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(357, -58.5, 142) * millimeter, "corner2" : vector(543, -54.5, 187) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "v1_bridge_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "v1_bridge_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(382.05, -59.5, 169) * millimeter, "topCenter" : vector(382.05, -53.5, 169) * millimeter, "radius" : 12 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(366.55, -59.5, 153.5) * millimeter, "topCenter" : vector(366.55, -53.5, 153.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(366.55, -56, 153.5) * millimeter, "topCenter" : vector(366.55, -53.5, 153.5) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(366.55, -59.5, 184.5) * millimeter, "topCenter" : vector(366.55, -53.5, 184.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(366.55, -56, 184.5) * millimeter, "topCenter" : vector(366.55, -53.5, 184.5) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(397.55, -59.5, 153.5) * millimeter, "topCenter" : vector(397.55, -53.5, 153.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(397.55, -56, 153.5) * millimeter, "topCenter" : vector(397.55, -53.5, 153.5) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(397.55, -59.5, 184.5) * millimeter, "topCenter" : vector(397.55, -53.5, 184.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(397.55, -56, 184.5) * millimeter, "topCenter" : vector(397.55, -53.5, 184.5) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(427.35, -59.5, 169) * millimeter, "topCenter" : vector(427.35, -53.5, 169) * millimeter, "radius" : 12 * millimeter });
    fCylinder(context, C1 + "o10", { "bottomCenter" : vector(411.85, -59.5, 153.5) * millimeter, "topCenter" : vector(411.85, -53.5, 153.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o11", { "bottomCenter" : vector(411.85, -56, 153.5) * millimeter, "topCenter" : vector(411.85, -53.5, 153.5) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o12", { "bottomCenter" : vector(411.85, -59.5, 184.5) * millimeter, "topCenter" : vector(411.85, -53.5, 184.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o13", { "bottomCenter" : vector(411.85, -56, 184.5) * millimeter, "topCenter" : vector(411.85, -53.5, 184.5) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o14", { "bottomCenter" : vector(442.85, -59.5, 153.5) * millimeter, "topCenter" : vector(442.85, -53.5, 153.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o15", { "bottomCenter" : vector(442.85, -56, 153.5) * millimeter, "topCenter" : vector(442.85, -53.5, 153.5) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o16", { "bottomCenter" : vector(442.85, -59.5, 184.5) * millimeter, "topCenter" : vector(442.85, -53.5, 184.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o17", { "bottomCenter" : vector(442.85, -56, 184.5) * millimeter, "topCenter" : vector(442.85, -53.5, 184.5) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o18", { "bottomCenter" : vector(472.65, -59.5, 169) * millimeter, "topCenter" : vector(472.65, -53.5, 169) * millimeter, "radius" : 12 * millimeter });
    fCylinder(context, C1 + "o19", { "bottomCenter" : vector(457.15, -59.5, 153.5) * millimeter, "topCenter" : vector(457.15, -53.5, 153.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o20", { "bottomCenter" : vector(457.15, -56, 153.5) * millimeter, "topCenter" : vector(457.15, -53.5, 153.5) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o21", { "bottomCenter" : vector(457.15, -59.5, 184.5) * millimeter, "topCenter" : vector(457.15, -53.5, 184.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o22", { "bottomCenter" : vector(457.15, -56, 184.5) * millimeter, "topCenter" : vector(457.15, -53.5, 184.5) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o23", { "bottomCenter" : vector(488.15, -59.5, 153.5) * millimeter, "topCenter" : vector(488.15, -53.5, 153.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o24", { "bottomCenter" : vector(488.15, -56, 153.5) * millimeter, "topCenter" : vector(488.15, -53.5, 153.5) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o25", { "bottomCenter" : vector(488.15, -59.5, 184.5) * millimeter, "topCenter" : vector(488.15, -53.5, 184.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o26", { "bottomCenter" : vector(488.15, -56, 184.5) * millimeter, "topCenter" : vector(488.15, -53.5, 184.5) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o27", { "bottomCenter" : vector(517.95, -59.5, 169) * millimeter, "topCenter" : vector(517.95, -53.5, 169) * millimeter, "radius" : 12 * millimeter });
    fCylinder(context, C1 + "o28", { "bottomCenter" : vector(502.45, -59.5, 153.5) * millimeter, "topCenter" : vector(502.45, -53.5, 153.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o29", { "bottomCenter" : vector(502.45, -56, 153.5) * millimeter, "topCenter" : vector(502.45, -53.5, 153.5) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o30", { "bottomCenter" : vector(502.45, -59.5, 184.5) * millimeter, "topCenter" : vector(502.45, -53.5, 184.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o31", { "bottomCenter" : vector(502.45, -56, 184.5) * millimeter, "topCenter" : vector(502.45, -53.5, 184.5) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o32", { "bottomCenter" : vector(533.45, -59.5, 153.5) * millimeter, "topCenter" : vector(533.45, -53.5, 153.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o33", { "bottomCenter" : vector(533.45, -56, 153.5) * millimeter, "topCenter" : vector(533.45, -53.5, 153.5) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o34", { "bottomCenter" : vector(533.45, -59.5, 184.5) * millimeter, "topCenter" : vector(533.45, -53.5, 184.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o35", { "bottomCenter" : vector(533.45, -56, 184.5) * millimeter, "topCenter" : vector(533.45, -53.5, 184.5) * millimeter, "radius" : 3.1 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "v1_bridge_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "[v1] Motorbrücke", color(0.87, 0.81, 0.69));
}

function arc_v1_cpl1o(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "v1_cpl1o_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(372.05, -50.5, 169.2) * millimeter, "corner2" : vector(392.05, -23.5, 176) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "v1_cpl1o_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "v1_cpl1o_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(382.05, -51.5, 169) * millimeter, "topCenter" : vector(382.05, -39, 169) * millimeter, "radius" : 2.55 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(382.05, -39.5, 169) * millimeter, "topCenter" : vector(382.05, -22.5, 169) * millimeter, "radius" : 2.05 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(375.05, -46, 161) * millimeter, "topCenter" : vector(375.05, -46, 177) * millimeter, "radius" : 1.2 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(375.05, -46, 174.5) * millimeter, "topCenter" : vector(375.05, -46, 177) * millimeter, "radius" : 2.1 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(375.05, -28.5, 161) * millimeter, "topCenter" : vector(375.05, -28.5, 177) * millimeter, "radius" : 1.2 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(375.05, -28.5, 174.5) * millimeter, "topCenter" : vector(375.05, -28.5, 177) * millimeter, "radius" : 2.1 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(389.05, -46, 161) * millimeter, "topCenter" : vector(389.05, -46, 177) * millimeter, "radius" : 1.2 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(389.05, -46, 174.5) * millimeter, "topCenter" : vector(389.05, -46, 177) * millimeter, "radius" : 2.1 * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(389.05, -28.5, 161) * millimeter, "topCenter" : vector(389.05, -28.5, 177) * millimeter, "radius" : 1.2 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(389.05, -28.5, 174.5) * millimeter, "topCenter" : vector(389.05, -28.5, 177) * millimeter, "radius" : 2.1 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "v1_cpl1o_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "[v1] Kupplung 1 oben", color(0.93, 0.55, 0.2));
}

function arc_v1_cpl1u(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "v1_cpl1u_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(372.05, -50.5, 162) * millimeter, "corner2" : vector(392.05, -23.5, 168.8) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "v1_cpl1u_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "v1_cpl1u_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(382.05, -51.5, 169) * millimeter, "topCenter" : vector(382.05, -39, 169) * millimeter, "radius" : 2.55 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(382.05, -39.5, 169) * millimeter, "topCenter" : vector(382.05, -22.5, 169) * millimeter, "radius" : 2.05 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(375.05, -46, 161) * millimeter, "topCenter" : vector(375.05, -46, 177) * millimeter, "radius" : 0.9 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(375.05, -28.5, 161) * millimeter, "topCenter" : vector(375.05, -28.5, 177) * millimeter, "radius" : 0.9 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(389.05, -46, 161) * millimeter, "topCenter" : vector(389.05, -46, 177) * millimeter, "radius" : 0.9 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(389.05, -28.5, 161) * millimeter, "topCenter" : vector(389.05, -28.5, 177) * millimeter, "radius" : 0.9 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "v1_cpl1u_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "[v1] Kupplung 1 unten", color(0.93, 0.55, 0.2));
}

function arc_v1_cpl2o(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "v1_cpl2o_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(417.35, -50.5, 169.2) * millimeter, "corner2" : vector(437.35, -23.5, 176) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "v1_cpl2o_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "v1_cpl2o_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(427.35, -51.5, 169) * millimeter, "topCenter" : vector(427.35, -39, 169) * millimeter, "radius" : 2.55 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(427.35, -39.5, 169) * millimeter, "topCenter" : vector(427.35, -22.5, 169) * millimeter, "radius" : 2.05 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(420.35, -46, 161) * millimeter, "topCenter" : vector(420.35, -46, 177) * millimeter, "radius" : 1.2 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(420.35, -46, 174.5) * millimeter, "topCenter" : vector(420.35, -46, 177) * millimeter, "radius" : 2.1 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(420.35, -28.5, 161) * millimeter, "topCenter" : vector(420.35, -28.5, 177) * millimeter, "radius" : 1.2 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(420.35, -28.5, 174.5) * millimeter, "topCenter" : vector(420.35, -28.5, 177) * millimeter, "radius" : 2.1 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(434.35, -46, 161) * millimeter, "topCenter" : vector(434.35, -46, 177) * millimeter, "radius" : 1.2 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(434.35, -46, 174.5) * millimeter, "topCenter" : vector(434.35, -46, 177) * millimeter, "radius" : 2.1 * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(434.35, -28.5, 161) * millimeter, "topCenter" : vector(434.35, -28.5, 177) * millimeter, "radius" : 1.2 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(434.35, -28.5, 174.5) * millimeter, "topCenter" : vector(434.35, -28.5, 177) * millimeter, "radius" : 2.1 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "v1_cpl2o_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "[v1] Kupplung 2 oben", color(0.93, 0.55, 0.2));
}

function arc_v1_cpl2u(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "v1_cpl2u_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(417.35, -50.5, 162) * millimeter, "corner2" : vector(437.35, -23.5, 168.8) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "v1_cpl2u_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "v1_cpl2u_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(427.35, -51.5, 169) * millimeter, "topCenter" : vector(427.35, -39, 169) * millimeter, "radius" : 2.55 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(427.35, -39.5, 169) * millimeter, "topCenter" : vector(427.35, -22.5, 169) * millimeter, "radius" : 2.05 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(420.35, -46, 161) * millimeter, "topCenter" : vector(420.35, -46, 177) * millimeter, "radius" : 0.9 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(420.35, -28.5, 161) * millimeter, "topCenter" : vector(420.35, -28.5, 177) * millimeter, "radius" : 0.9 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(434.35, -46, 161) * millimeter, "topCenter" : vector(434.35, -46, 177) * millimeter, "radius" : 0.9 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(434.35, -28.5, 161) * millimeter, "topCenter" : vector(434.35, -28.5, 177) * millimeter, "radius" : 0.9 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "v1_cpl2u_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "[v1] Kupplung 2 unten", color(0.93, 0.55, 0.2));
}

function arc_v1_cpl3o(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "v1_cpl3o_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(462.65, -50.5, 169.2) * millimeter, "corner2" : vector(482.65, -23.5, 176) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "v1_cpl3o_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "v1_cpl3o_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(472.65, -51.5, 169) * millimeter, "topCenter" : vector(472.65, -39, 169) * millimeter, "radius" : 2.55 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(472.65, -39.5, 169) * millimeter, "topCenter" : vector(472.65, -22.5, 169) * millimeter, "radius" : 2.05 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(465.65, -46, 161) * millimeter, "topCenter" : vector(465.65, -46, 177) * millimeter, "radius" : 1.2 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(465.65, -46, 174.5) * millimeter, "topCenter" : vector(465.65, -46, 177) * millimeter, "radius" : 2.1 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(465.65, -28.5, 161) * millimeter, "topCenter" : vector(465.65, -28.5, 177) * millimeter, "radius" : 1.2 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(465.65, -28.5, 174.5) * millimeter, "topCenter" : vector(465.65, -28.5, 177) * millimeter, "radius" : 2.1 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(479.65, -46, 161) * millimeter, "topCenter" : vector(479.65, -46, 177) * millimeter, "radius" : 1.2 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(479.65, -46, 174.5) * millimeter, "topCenter" : vector(479.65, -46, 177) * millimeter, "radius" : 2.1 * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(479.65, -28.5, 161) * millimeter, "topCenter" : vector(479.65, -28.5, 177) * millimeter, "radius" : 1.2 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(479.65, -28.5, 174.5) * millimeter, "topCenter" : vector(479.65, -28.5, 177) * millimeter, "radius" : 2.1 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "v1_cpl3o_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "[v1] Kupplung 3 oben", color(0.93, 0.55, 0.2));
}

function arc_v1_cpl3u(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "v1_cpl3u_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(462.65, -50.5, 162) * millimeter, "corner2" : vector(482.65, -23.5, 168.8) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "v1_cpl3u_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "v1_cpl3u_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(472.65, -51.5, 169) * millimeter, "topCenter" : vector(472.65, -39, 169) * millimeter, "radius" : 2.55 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(472.65, -39.5, 169) * millimeter, "topCenter" : vector(472.65, -22.5, 169) * millimeter, "radius" : 2.05 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(465.65, -46, 161) * millimeter, "topCenter" : vector(465.65, -46, 177) * millimeter, "radius" : 0.9 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(465.65, -28.5, 161) * millimeter, "topCenter" : vector(465.65, -28.5, 177) * millimeter, "radius" : 0.9 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(479.65, -46, 161) * millimeter, "topCenter" : vector(479.65, -46, 177) * millimeter, "radius" : 0.9 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(479.65, -28.5, 161) * millimeter, "topCenter" : vector(479.65, -28.5, 177) * millimeter, "radius" : 0.9 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "v1_cpl3u_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "[v1] Kupplung 3 unten", color(0.93, 0.55, 0.2));
}

function arc_v1_cpl4o(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "v1_cpl4o_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(507.95, -50.5, 169.2) * millimeter, "corner2" : vector(527.95, -23.5, 176) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "v1_cpl4o_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "v1_cpl4o_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(517.95, -51.5, 169) * millimeter, "topCenter" : vector(517.95, -39, 169) * millimeter, "radius" : 2.55 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(517.95, -39.5, 169) * millimeter, "topCenter" : vector(517.95, -22.5, 169) * millimeter, "radius" : 2.05 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(510.95, -46, 161) * millimeter, "topCenter" : vector(510.95, -46, 177) * millimeter, "radius" : 1.2 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(510.95, -46, 174.5) * millimeter, "topCenter" : vector(510.95, -46, 177) * millimeter, "radius" : 2.1 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(510.95, -28.5, 161) * millimeter, "topCenter" : vector(510.95, -28.5, 177) * millimeter, "radius" : 1.2 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(510.95, -28.5, 174.5) * millimeter, "topCenter" : vector(510.95, -28.5, 177) * millimeter, "radius" : 2.1 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(524.95, -46, 161) * millimeter, "topCenter" : vector(524.95, -46, 177) * millimeter, "radius" : 1.2 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(524.95, -46, 174.5) * millimeter, "topCenter" : vector(524.95, -46, 177) * millimeter, "radius" : 2.1 * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(524.95, -28.5, 161) * millimeter, "topCenter" : vector(524.95, -28.5, 177) * millimeter, "radius" : 1.2 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(524.95, -28.5, 174.5) * millimeter, "topCenter" : vector(524.95, -28.5, 177) * millimeter, "radius" : 2.1 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "v1_cpl4o_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "[v1] Kupplung 4 oben", color(0.93, 0.55, 0.2));
}

function arc_v1_cpl4u(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "v1_cpl4u_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(507.95, -50.5, 162) * millimeter, "corner2" : vector(527.95, -23.5, 168.8) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "v1_cpl4u_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "v1_cpl4u_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(517.95, -51.5, 169) * millimeter, "topCenter" : vector(517.95, -39, 169) * millimeter, "radius" : 2.55 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(517.95, -39.5, 169) * millimeter, "topCenter" : vector(517.95, -22.5, 169) * millimeter, "radius" : 2.05 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(510.95, -46, 161) * millimeter, "topCenter" : vector(510.95, -46, 177) * millimeter, "radius" : 0.9 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(510.95, -28.5, 161) * millimeter, "topCenter" : vector(510.95, -28.5, 177) * millimeter, "radius" : 0.9 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(524.95, -46, 161) * millimeter, "topCenter" : vector(524.95, -46, 177) * millimeter, "radius" : 0.9 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(524.95, -28.5, 161) * millimeter, "topCenter" : vector(524.95, -28.5, 177) * millimeter, "radius" : 0.9 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "v1_cpl4u_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "[v1] Kupplung 4 unten", color(0.93, 0.55, 0.2));
}

function arc_v1_back(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "v1_back_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(357, 60, 54) * millimeter, "corner2" : vector(543, 63.5, 200) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "v1_back_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "v1_back_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(392.7, 59, 75) * millimeter, "topCenter" : vector(392.7, 64.5, 75) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(392.7, 62, 75) * millimeter, "topCenter" : vector(392.7, 64.5, 75) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(416.7, 59, 75) * millimeter, "topCenter" : vector(416.7, 64.5, 75) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(416.7, 62, 75) * millimeter, "topCenter" : vector(416.7, 64.5, 75) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(404.7, 59, 66) * millimeter, "topCenter" : vector(404.7, 64.5, 66) * millimeter, "radius" : 5.1 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(483.3, 59, 75) * millimeter, "topCenter" : vector(483.3, 64.5, 75) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(483.3, 62, 75) * millimeter, "topCenter" : vector(483.3, 64.5, 75) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(507.3, 59, 75) * millimeter, "topCenter" : vector(507.3, 64.5, 75) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(507.3, 62, 75) * millimeter, "topCenter" : vector(507.3, 64.5, 75) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(495.3, 59, 66) * millimeter, "topCenter" : vector(495.3, 64.5, 66) * millimeter, "radius" : 4.6 * millimeter });
    fCylinder(context, C1 + "o10", { "bottomCenter" : vector(438, 59, 171) * millimeter, "topCenter" : vector(438, 64.5, 171) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o11", { "bottomCenter" : vector(438, 62, 171) * millimeter, "topCenter" : vector(438, 64.5, 171) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o12", { "bottomCenter" : vector(462, 59, 171) * millimeter, "topCenter" : vector(462, 64.5, 171) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o13", { "bottomCenter" : vector(462, 62, 171) * millimeter, "topCenter" : vector(462, 64.5, 171) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o14", { "bottomCenter" : vector(450, 59, 162) * millimeter, "topCenter" : vector(450, 64.5, 162) * millimeter, "radius" : 5.1 * millimeter });
    fCylinder(context, C1 + "o15", { "bottomCenter" : vector(373.05, 59, 171) * millimeter, "topCenter" : vector(373.05, 64.5, 171) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o16", { "bottomCenter" : vector(373.05, 62, 171) * millimeter, "topCenter" : vector(373.05, 64.5, 171) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o17", { "bottomCenter" : vector(391.05, 59, 171) * millimeter, "topCenter" : vector(391.05, 64.5, 171) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o18", { "bottomCenter" : vector(391.05, 62, 171) * millimeter, "topCenter" : vector(391.05, 64.5, 171) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o19", { "bottomCenter" : vector(382.05, 59, 162) * millimeter, "topCenter" : vector(382.05, 64.5, 162) * millimeter, "radius" : 5.1 * millimeter });
    fCylinder(context, C1 + "o20", { "bottomCenter" : vector(508.95, 59, 171) * millimeter, "topCenter" : vector(508.95, 64.5, 171) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o21", { "bottomCenter" : vector(508.95, 62, 171) * millimeter, "topCenter" : vector(508.95, 64.5, 171) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o22", { "bottomCenter" : vector(526.95, 59, 171) * millimeter, "topCenter" : vector(526.95, 64.5, 171) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o23", { "bottomCenter" : vector(526.95, 62, 171) * millimeter, "topCenter" : vector(526.95, 64.5, 171) * millimeter, "radius" : 3.1 * millimeter });
    fCylinder(context, C1 + "o24", { "bottomCenter" : vector(517.95, 59, 162) * millimeter, "topCenter" : vector(517.95, 64.5, 162) * millimeter, "radius" : 5.1 * millimeter });
    fCuboid(context, C1 + "o25", { "corner1" : vector(420, 59, 183) * millimeter, "corner2" : vector(480, 64.5, 186) * millimeter });
    fCuboid(context, C1 + "o26", { "corner1" : vector(420, 59, 189) * millimeter, "corner2" : vector(480, 64.5, 192) * millimeter });
    fCuboid(context, C1 + "o27", { "corner1" : vector(420, 59, 195) * millimeter, "corner2" : vector(480, 64.5, 198) * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "v1_back_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "[v1] Rückwand", color(0.87, 0.81, 0.69));
}

function arc_v1_maniLo(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "v1_maniLo_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(374.05, 48, 58) * millimeter, "corner2" : vector(435.35, 60, 80) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(382.05, 48.5, 66) * millimeter, "topCenter" : vector(382.05, 40, 66) * millimeter, "radius" : 4 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(382.05, 40.6, 66) * millimeter, "topCenter" : vector(382.05, 41.35, 66) * millimeter, "radius" : 4.15 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(382.05, 41.35, 66) * millimeter, "topCenter" : vector(382.05, 42.1, 66) * millimeter, "radius" : 4.3 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(382.05, 42.1, 66) * millimeter, "topCenter" : vector(382.05, 42.85, 66) * millimeter, "radius" : 4.45 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(382.05, 42.85, 66) * millimeter, "topCenter" : vector(382.05, 43.6, 66) * millimeter, "radius" : 4.6 * millimeter });
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(382.05, 44.2, 66) * millimeter, "topCenter" : vector(382.05, 44.95, 66) * millimeter, "radius" : 4.15 * millimeter });
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(382.05, 44.95, 66) * millimeter, "topCenter" : vector(382.05, 45.7, 66) * millimeter, "radius" : 4.3 * millimeter });
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(382.05, 45.7, 66) * millimeter, "topCenter" : vector(382.05, 46.45, 66) * millimeter, "radius" : 4.45 * millimeter });
    fCylinder(context, A0 + "s9", { "bottomCenter" : vector(382.05, 46.45, 66) * millimeter, "topCenter" : vector(382.05, 47.2, 66) * millimeter, "radius" : 4.6 * millimeter });
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(427.35, 48.5, 66) * millimeter, "topCenter" : vector(427.35, 40, 66) * millimeter, "radius" : 4 * millimeter });
    fCylinder(context, A0 + "s11", { "bottomCenter" : vector(427.35, 40.6, 66) * millimeter, "topCenter" : vector(427.35, 41.35, 66) * millimeter, "radius" : 4.15 * millimeter });
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(427.35, 41.35, 66) * millimeter, "topCenter" : vector(427.35, 42.1, 66) * millimeter, "radius" : 4.3 * millimeter });
    fCylinder(context, A0 + "s13", { "bottomCenter" : vector(427.35, 42.1, 66) * millimeter, "topCenter" : vector(427.35, 42.85, 66) * millimeter, "radius" : 4.45 * millimeter });
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(427.35, 42.85, 66) * millimeter, "topCenter" : vector(427.35, 43.6, 66) * millimeter, "radius" : 4.6 * millimeter });
    fCylinder(context, A0 + "s15", { "bottomCenter" : vector(427.35, 44.2, 66) * millimeter, "topCenter" : vector(427.35, 44.95, 66) * millimeter, "radius" : 4.15 * millimeter });
    fCylinder(context, A0 + "s16", { "bottomCenter" : vector(427.35, 44.95, 66) * millimeter, "topCenter" : vector(427.35, 45.7, 66) * millimeter, "radius" : 4.3 * millimeter });
    fCylinder(context, A0 + "s17", { "bottomCenter" : vector(427.35, 45.7, 66) * millimeter, "topCenter" : vector(427.35, 46.45, 66) * millimeter, "radius" : 4.45 * millimeter });
    fCylinder(context, A0 + "s18", { "bottomCenter" : vector(427.35, 46.45, 66) * millimeter, "topCenter" : vector(427.35, 47.2, 66) * millimeter, "radius" : 4.6 * millimeter });
    fCylinder(context, A0 + "s19", { "bottomCenter" : vector(404.7, 59.5, 66) * millimeter, "topCenter" : vector(404.7, 77.5, 66) * millimeter, "radius" : 4 * millimeter });
    fCylinder(context, A0 + "s20", { "bottomCenter" : vector(404.7, 76.9, 66) * millimeter, "topCenter" : vector(404.7, 76.15, 66) * millimeter, "radius" : 4.15 * millimeter });
    fCylinder(context, A0 + "s21", { "bottomCenter" : vector(404.7, 76.15, 66) * millimeter, "topCenter" : vector(404.7, 75.4, 66) * millimeter, "radius" : 4.3 * millimeter });
    fCylinder(context, A0 + "s22", { "bottomCenter" : vector(404.7, 75.4, 66) * millimeter, "topCenter" : vector(404.7, 74.65, 66) * millimeter, "radius" : 4.45 * millimeter });
    fCylinder(context, A0 + "s23", { "bottomCenter" : vector(404.7, 74.65, 66) * millimeter, "topCenter" : vector(404.7, 73.9, 66) * millimeter, "radius" : 4.6 * millimeter });
    fCylinder(context, A0 + "s24", { "bottomCenter" : vector(404.7, 73.3, 66) * millimeter, "topCenter" : vector(404.7, 72.55, 66) * millimeter, "radius" : 4.15 * millimeter });
    fCylinder(context, A0 + "s25", { "bottomCenter" : vector(404.7, 72.55, 66) * millimeter, "topCenter" : vector(404.7, 71.8, 66) * millimeter, "radius" : 4.3 * millimeter });
    fCylinder(context, A0 + "s26", { "bottomCenter" : vector(404.7, 71.8, 66) * millimeter, "topCenter" : vector(404.7, 71.05, 66) * millimeter, "radius" : 4.45 * millimeter });
    fCylinder(context, A0 + "s27", { "bottomCenter" : vector(404.7, 71.05, 66) * millimeter, "topCenter" : vector(404.7, 70.3, 66) * millimeter, "radius" : 4.6 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "v1_maniLo_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "v1_maniLo_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(382.05, 54, 66) * millimeter, "topCenter" : vector(427.35, 54, 66) * millimeter, "radius" : 2.75 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(382.05, 39, 66) * millimeter, "topCenter" : vector(382.05, 54.5, 66) * millimeter, "radius" : 2 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(427.35, 39, 66) * millimeter, "topCenter" : vector(427.35, 54.5, 66) * millimeter, "radius" : 2 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(404.7, 53.5, 66) * millimeter, "topCenter" : vector(404.7, 78.5, 66) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(392.7, 50, 75) * millimeter, "topCenter" : vector(392.7, 60.5, 75) * millimeter, "radius" : 1.6 * millimeter });
    fCuboid(context, C1 + "o5", { "corner1" : vector(389.75, 53.6, 71.8) * millimeter, "corner2" : vector(395.65, 56.4, 80.5) * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(416.7, 50, 75) * millimeter, "topCenter" : vector(416.7, 60.5, 75) * millimeter, "radius" : 1.6 * millimeter });
    fCuboid(context, C1 + "o7", { "corner1" : vector(413.75, 53.6, 71.8) * millimeter, "corner2" : vector(419.65, 56.4, 80.5) * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "v1_maniLo_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "[v1] Verteiler lo (hypoxisch, Eingang)", color(0.35, 0.55, 0.85));
}

function arc_v1_maniHi(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "v1_maniHi_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(464.65, 48, 58) * millimeter, "corner2" : vector(525.95, 60, 80) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(472.65, 48.5, 66) * millimeter, "topCenter" : vector(472.65, 40, 66) * millimeter, "radius" : 4 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(472.65, 40.6, 66) * millimeter, "topCenter" : vector(472.65, 41.35, 66) * millimeter, "radius" : 4.15 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(472.65, 41.35, 66) * millimeter, "topCenter" : vector(472.65, 42.1, 66) * millimeter, "radius" : 4.3 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(472.65, 42.1, 66) * millimeter, "topCenter" : vector(472.65, 42.85, 66) * millimeter, "radius" : 4.45 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(472.65, 42.85, 66) * millimeter, "topCenter" : vector(472.65, 43.6, 66) * millimeter, "radius" : 4.6 * millimeter });
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(472.65, 44.2, 66) * millimeter, "topCenter" : vector(472.65, 44.95, 66) * millimeter, "radius" : 4.15 * millimeter });
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(472.65, 44.95, 66) * millimeter, "topCenter" : vector(472.65, 45.7, 66) * millimeter, "radius" : 4.3 * millimeter });
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(472.65, 45.7, 66) * millimeter, "topCenter" : vector(472.65, 46.45, 66) * millimeter, "radius" : 4.45 * millimeter });
    fCylinder(context, A0 + "s9", { "bottomCenter" : vector(472.65, 46.45, 66) * millimeter, "topCenter" : vector(472.65, 47.2, 66) * millimeter, "radius" : 4.6 * millimeter });
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(517.95, 48.5, 66) * millimeter, "topCenter" : vector(517.95, 40, 66) * millimeter, "radius" : 4 * millimeter });
    fCylinder(context, A0 + "s11", { "bottomCenter" : vector(517.95, 40.6, 66) * millimeter, "topCenter" : vector(517.95, 41.35, 66) * millimeter, "radius" : 4.15 * millimeter });
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(517.95, 41.35, 66) * millimeter, "topCenter" : vector(517.95, 42.1, 66) * millimeter, "radius" : 4.3 * millimeter });
    fCylinder(context, A0 + "s13", { "bottomCenter" : vector(517.95, 42.1, 66) * millimeter, "topCenter" : vector(517.95, 42.85, 66) * millimeter, "radius" : 4.45 * millimeter });
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(517.95, 42.85, 66) * millimeter, "topCenter" : vector(517.95, 43.6, 66) * millimeter, "radius" : 4.6 * millimeter });
    fCylinder(context, A0 + "s15", { "bottomCenter" : vector(517.95, 44.2, 66) * millimeter, "topCenter" : vector(517.95, 44.95, 66) * millimeter, "radius" : 4.15 * millimeter });
    fCylinder(context, A0 + "s16", { "bottomCenter" : vector(517.95, 44.95, 66) * millimeter, "topCenter" : vector(517.95, 45.7, 66) * millimeter, "radius" : 4.3 * millimeter });
    fCylinder(context, A0 + "s17", { "bottomCenter" : vector(517.95, 45.7, 66) * millimeter, "topCenter" : vector(517.95, 46.45, 66) * millimeter, "radius" : 4.45 * millimeter });
    fCylinder(context, A0 + "s18", { "bottomCenter" : vector(517.95, 46.45, 66) * millimeter, "topCenter" : vector(517.95, 47.2, 66) * millimeter, "radius" : 4.6 * millimeter });
    fCylinder(context, A0 + "s19", { "bottomCenter" : vector(495.3, 59.5, 66) * millimeter, "topCenter" : vector(495.3, 77.5, 66) * millimeter, "radius" : 3.5 * millimeter });
    fCylinder(context, A0 + "s20", { "bottomCenter" : vector(495.3, 76.9, 66) * millimeter, "topCenter" : vector(495.3, 76.15, 66) * millimeter, "radius" : 3.65 * millimeter });
    fCylinder(context, A0 + "s21", { "bottomCenter" : vector(495.3, 76.15, 66) * millimeter, "topCenter" : vector(495.3, 75.4, 66) * millimeter, "radius" : 3.8 * millimeter });
    fCylinder(context, A0 + "s22", { "bottomCenter" : vector(495.3, 75.4, 66) * millimeter, "topCenter" : vector(495.3, 74.65, 66) * millimeter, "radius" : 3.95 * millimeter });
    fCylinder(context, A0 + "s23", { "bottomCenter" : vector(495.3, 74.65, 66) * millimeter, "topCenter" : vector(495.3, 73.9, 66) * millimeter, "radius" : 4.1 * millimeter });
    fCylinder(context, A0 + "s24", { "bottomCenter" : vector(495.3, 73.3, 66) * millimeter, "topCenter" : vector(495.3, 72.55, 66) * millimeter, "radius" : 3.65 * millimeter });
    fCylinder(context, A0 + "s25", { "bottomCenter" : vector(495.3, 72.55, 66) * millimeter, "topCenter" : vector(495.3, 71.8, 66) * millimeter, "radius" : 3.8 * millimeter });
    fCylinder(context, A0 + "s26", { "bottomCenter" : vector(495.3, 71.8, 66) * millimeter, "topCenter" : vector(495.3, 71.05, 66) * millimeter, "radius" : 3.95 * millimeter });
    fCylinder(context, A0 + "s27", { "bottomCenter" : vector(495.3, 71.05, 66) * millimeter, "topCenter" : vector(495.3, 70.3, 66) * millimeter, "radius" : 4.1 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "v1_maniHi_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "v1_maniHi_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(472.65, 54, 66) * millimeter, "topCenter" : vector(517.95, 54, 66) * millimeter, "radius" : 2.75 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(472.65, 39, 66) * millimeter, "topCenter" : vector(472.65, 54.5, 66) * millimeter, "radius" : 2 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(517.95, 39, 66) * millimeter, "topCenter" : vector(517.95, 54.5, 66) * millimeter, "radius" : 2 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(495.3, 53.5, 66) * millimeter, "topCenter" : vector(495.3, 78.5, 66) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(483.3, 50, 75) * millimeter, "topCenter" : vector(483.3, 60.5, 75) * millimeter, "radius" : 1.6 * millimeter });
    fCuboid(context, C1 + "o5", { "corner1" : vector(480.35, 53.6, 71.8) * millimeter, "corner2" : vector(486.25, 56.4, 80.5) * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(507.3, 50, 75) * millimeter, "topCenter" : vector(507.3, 60.5, 75) * millimeter, "radius" : 1.6 * millimeter });
    fCuboid(context, C1 + "o7", { "corner1" : vector(504.35, 53.6, 71.8) * millimeter, "corner2" : vector(510.25, 56.4, 80.5) * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "v1_maniHi_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "[v1] Verteiler hi (hyperoxisch, Eingang)", color(0.85, 0.35, 0.3));
}

function arc_v1_maniMix(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "v1_maniMix_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(419.35, 48, 154) * millimeter, "corner2" : vector(480.65, 60, 176) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(427.35, 48.5, 162) * millimeter, "topCenter" : vector(427.35, 40, 162) * millimeter, "radius" : 4 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(427.35, 40.6, 162) * millimeter, "topCenter" : vector(427.35, 41.35, 162) * millimeter, "radius" : 4.15 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(427.35, 41.35, 162) * millimeter, "topCenter" : vector(427.35, 42.1, 162) * millimeter, "radius" : 4.3 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(427.35, 42.1, 162) * millimeter, "topCenter" : vector(427.35, 42.85, 162) * millimeter, "radius" : 4.45 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(427.35, 42.85, 162) * millimeter, "topCenter" : vector(427.35, 43.6, 162) * millimeter, "radius" : 4.6 * millimeter });
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(427.35, 44.2, 162) * millimeter, "topCenter" : vector(427.35, 44.95, 162) * millimeter, "radius" : 4.15 * millimeter });
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(427.35, 44.95, 162) * millimeter, "topCenter" : vector(427.35, 45.7, 162) * millimeter, "radius" : 4.3 * millimeter });
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(427.35, 45.7, 162) * millimeter, "topCenter" : vector(427.35, 46.45, 162) * millimeter, "radius" : 4.45 * millimeter });
    fCylinder(context, A0 + "s9", { "bottomCenter" : vector(427.35, 46.45, 162) * millimeter, "topCenter" : vector(427.35, 47.2, 162) * millimeter, "radius" : 4.6 * millimeter });
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(472.65, 48.5, 162) * millimeter, "topCenter" : vector(472.65, 40, 162) * millimeter, "radius" : 4 * millimeter });
    fCylinder(context, A0 + "s11", { "bottomCenter" : vector(472.65, 40.6, 162) * millimeter, "topCenter" : vector(472.65, 41.35, 162) * millimeter, "radius" : 4.15 * millimeter });
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(472.65, 41.35, 162) * millimeter, "topCenter" : vector(472.65, 42.1, 162) * millimeter, "radius" : 4.3 * millimeter });
    fCylinder(context, A0 + "s13", { "bottomCenter" : vector(472.65, 42.1, 162) * millimeter, "topCenter" : vector(472.65, 42.85, 162) * millimeter, "radius" : 4.45 * millimeter });
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(472.65, 42.85, 162) * millimeter, "topCenter" : vector(472.65, 43.6, 162) * millimeter, "radius" : 4.6 * millimeter });
    fCylinder(context, A0 + "s15", { "bottomCenter" : vector(472.65, 44.2, 162) * millimeter, "topCenter" : vector(472.65, 44.95, 162) * millimeter, "radius" : 4.15 * millimeter });
    fCylinder(context, A0 + "s16", { "bottomCenter" : vector(472.65, 44.95, 162) * millimeter, "topCenter" : vector(472.65, 45.7, 162) * millimeter, "radius" : 4.3 * millimeter });
    fCylinder(context, A0 + "s17", { "bottomCenter" : vector(472.65, 45.7, 162) * millimeter, "topCenter" : vector(472.65, 46.45, 162) * millimeter, "radius" : 4.45 * millimeter });
    fCylinder(context, A0 + "s18", { "bottomCenter" : vector(472.65, 46.45, 162) * millimeter, "topCenter" : vector(472.65, 47.2, 162) * millimeter, "radius" : 4.6 * millimeter });
    fCylinder(context, A0 + "s19", { "bottomCenter" : vector(450, 59.5, 162) * millimeter, "topCenter" : vector(450, 77.5, 162) * millimeter, "radius" : 4 * millimeter });
    fCylinder(context, A0 + "s20", { "bottomCenter" : vector(450, 76.9, 162) * millimeter, "topCenter" : vector(450, 76.15, 162) * millimeter, "radius" : 4.15 * millimeter });
    fCylinder(context, A0 + "s21", { "bottomCenter" : vector(450, 76.15, 162) * millimeter, "topCenter" : vector(450, 75.4, 162) * millimeter, "radius" : 4.3 * millimeter });
    fCylinder(context, A0 + "s22", { "bottomCenter" : vector(450, 75.4, 162) * millimeter, "topCenter" : vector(450, 74.65, 162) * millimeter, "radius" : 4.45 * millimeter });
    fCylinder(context, A0 + "s23", { "bottomCenter" : vector(450, 74.65, 162) * millimeter, "topCenter" : vector(450, 73.9, 162) * millimeter, "radius" : 4.6 * millimeter });
    fCylinder(context, A0 + "s24", { "bottomCenter" : vector(450, 73.3, 162) * millimeter, "topCenter" : vector(450, 72.55, 162) * millimeter, "radius" : 4.15 * millimeter });
    fCylinder(context, A0 + "s25", { "bottomCenter" : vector(450, 72.55, 162) * millimeter, "topCenter" : vector(450, 71.8, 162) * millimeter, "radius" : 4.3 * millimeter });
    fCylinder(context, A0 + "s26", { "bottomCenter" : vector(450, 71.8, 162) * millimeter, "topCenter" : vector(450, 71.05, 162) * millimeter, "radius" : 4.45 * millimeter });
    fCylinder(context, A0 + "s27", { "bottomCenter" : vector(450, 71.05, 162) * millimeter, "topCenter" : vector(450, 70.3, 162) * millimeter, "radius" : 4.6 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "v1_maniMix_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "v1_maniMix_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(427.35, 54, 162) * millimeter, "topCenter" : vector(472.65, 54, 162) * millimeter, "radius" : 2.75 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(427.35, 39, 162) * millimeter, "topCenter" : vector(427.35, 54.5, 162) * millimeter, "radius" : 2 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(472.65, 39, 162) * millimeter, "topCenter" : vector(472.65, 54.5, 162) * millimeter, "radius" : 2 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(450, 53.5, 162) * millimeter, "topCenter" : vector(450, 78.5, 162) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(438, 50, 171) * millimeter, "topCenter" : vector(438, 60.5, 171) * millimeter, "radius" : 1.6 * millimeter });
    fCuboid(context, C1 + "o5", { "corner1" : vector(435.05, 53.6, 167.8) * millimeter, "corner2" : vector(440.95, 56.4, 176.5) * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(462, 50, 171) * millimeter, "topCenter" : vector(462, 60.5, 171) * millimeter, "radius" : 1.6 * millimeter });
    fCuboid(context, C1 + "o7", { "corner1" : vector(459.05, 53.6, 167.8) * millimeter, "corner2" : vector(464.95, 56.4, 176.5) * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "v1_maniMix_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "[v1] Sammler Mix (Ausgang)", color(0.6, 0.4, 0.8));
}

function arc_v1_ventL(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "v1_ventL_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(367.05, 48, 154) * millimeter, "corner2" : vector(397.05, 60, 176) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(382.05, 48.5, 162) * millimeter, "topCenter" : vector(382.05, 40, 162) * millimeter, "radius" : 4 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(382.05, 40.6, 162) * millimeter, "topCenter" : vector(382.05, 41.35, 162) * millimeter, "radius" : 4.15 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(382.05, 41.35, 162) * millimeter, "topCenter" : vector(382.05, 42.1, 162) * millimeter, "radius" : 4.3 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(382.05, 42.1, 162) * millimeter, "topCenter" : vector(382.05, 42.85, 162) * millimeter, "radius" : 4.45 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(382.05, 42.85, 162) * millimeter, "topCenter" : vector(382.05, 43.6, 162) * millimeter, "radius" : 4.6 * millimeter });
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(382.05, 44.2, 162) * millimeter, "topCenter" : vector(382.05, 44.95, 162) * millimeter, "radius" : 4.15 * millimeter });
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(382.05, 44.95, 162) * millimeter, "topCenter" : vector(382.05, 45.7, 162) * millimeter, "radius" : 4.3 * millimeter });
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(382.05, 45.7, 162) * millimeter, "topCenter" : vector(382.05, 46.45, 162) * millimeter, "radius" : 4.45 * millimeter });
    fCylinder(context, A0 + "s9", { "bottomCenter" : vector(382.05, 46.45, 162) * millimeter, "topCenter" : vector(382.05, 47.2, 162) * millimeter, "radius" : 4.6 * millimeter });
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(382.05, 59.5, 162) * millimeter, "topCenter" : vector(382.05, 77.5, 162) * millimeter, "radius" : 4 * millimeter });
    fCylinder(context, A0 + "s11", { "bottomCenter" : vector(382.05, 76.9, 162) * millimeter, "topCenter" : vector(382.05, 76.15, 162) * millimeter, "radius" : 4.15 * millimeter });
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(382.05, 76.15, 162) * millimeter, "topCenter" : vector(382.05, 75.4, 162) * millimeter, "radius" : 4.3 * millimeter });
    fCylinder(context, A0 + "s13", { "bottomCenter" : vector(382.05, 75.4, 162) * millimeter, "topCenter" : vector(382.05, 74.65, 162) * millimeter, "radius" : 4.45 * millimeter });
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(382.05, 74.65, 162) * millimeter, "topCenter" : vector(382.05, 73.9, 162) * millimeter, "radius" : 4.6 * millimeter });
    fCylinder(context, A0 + "s15", { "bottomCenter" : vector(382.05, 73.3, 162) * millimeter, "topCenter" : vector(382.05, 72.55, 162) * millimeter, "radius" : 4.15 * millimeter });
    fCylinder(context, A0 + "s16", { "bottomCenter" : vector(382.05, 72.55, 162) * millimeter, "topCenter" : vector(382.05, 71.8, 162) * millimeter, "radius" : 4.3 * millimeter });
    fCylinder(context, A0 + "s17", { "bottomCenter" : vector(382.05, 71.8, 162) * millimeter, "topCenter" : vector(382.05, 71.05, 162) * millimeter, "radius" : 4.45 * millimeter });
    fCylinder(context, A0 + "s18", { "bottomCenter" : vector(382.05, 71.05, 162) * millimeter, "topCenter" : vector(382.05, 70.3, 162) * millimeter, "radius" : 4.6 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "v1_ventL_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "v1_ventL_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(382.05, 39, 162) * millimeter, "topCenter" : vector(382.05, 54.5, 162) * millimeter, "radius" : 2 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(382.05, 53.5, 162) * millimeter, "topCenter" : vector(382.05, 78.5, 162) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(373.05, 50, 171) * millimeter, "topCenter" : vector(373.05, 60.5, 171) * millimeter, "radius" : 1.6 * millimeter });
    fCuboid(context, C1 + "o3", { "corner1" : vector(370.1, 53.6, 167.8) * millimeter, "corner2" : vector(376, 56.4, 176.5) * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(391.05, 50, 171) * millimeter, "topCenter" : vector(391.05, 60.5, 171) * millimeter, "radius" : 1.6 * millimeter });
    fCuboid(context, C1 + "o5", { "corner1" : vector(388.1, 53.6, 167.8) * millimeter, "corner2" : vector(394, 56.4, 176.5) * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "v1_ventL_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "[v1] Ablass links (lo)", color(0.55, 0.6, 0.65));
}

function arc_v1_ventR(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "v1_ventR_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(502.95, 48, 154) * millimeter, "corner2" : vector(532.95, 60, 176) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(517.95, 48.5, 162) * millimeter, "topCenter" : vector(517.95, 40, 162) * millimeter, "radius" : 4 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(517.95, 40.6, 162) * millimeter, "topCenter" : vector(517.95, 41.35, 162) * millimeter, "radius" : 4.15 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(517.95, 41.35, 162) * millimeter, "topCenter" : vector(517.95, 42.1, 162) * millimeter, "radius" : 4.3 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(517.95, 42.1, 162) * millimeter, "topCenter" : vector(517.95, 42.85, 162) * millimeter, "radius" : 4.45 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(517.95, 42.85, 162) * millimeter, "topCenter" : vector(517.95, 43.6, 162) * millimeter, "radius" : 4.6 * millimeter });
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(517.95, 44.2, 162) * millimeter, "topCenter" : vector(517.95, 44.95, 162) * millimeter, "radius" : 4.15 * millimeter });
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(517.95, 44.95, 162) * millimeter, "topCenter" : vector(517.95, 45.7, 162) * millimeter, "radius" : 4.3 * millimeter });
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(517.95, 45.7, 162) * millimeter, "topCenter" : vector(517.95, 46.45, 162) * millimeter, "radius" : 4.45 * millimeter });
    fCylinder(context, A0 + "s9", { "bottomCenter" : vector(517.95, 46.45, 162) * millimeter, "topCenter" : vector(517.95, 47.2, 162) * millimeter, "radius" : 4.6 * millimeter });
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(517.95, 59.5, 162) * millimeter, "topCenter" : vector(517.95, 77.5, 162) * millimeter, "radius" : 4 * millimeter });
    fCylinder(context, A0 + "s11", { "bottomCenter" : vector(517.95, 76.9, 162) * millimeter, "topCenter" : vector(517.95, 76.15, 162) * millimeter, "radius" : 4.15 * millimeter });
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(517.95, 76.15, 162) * millimeter, "topCenter" : vector(517.95, 75.4, 162) * millimeter, "radius" : 4.3 * millimeter });
    fCylinder(context, A0 + "s13", { "bottomCenter" : vector(517.95, 75.4, 162) * millimeter, "topCenter" : vector(517.95, 74.65, 162) * millimeter, "radius" : 4.45 * millimeter });
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(517.95, 74.65, 162) * millimeter, "topCenter" : vector(517.95, 73.9, 162) * millimeter, "radius" : 4.6 * millimeter });
    fCylinder(context, A0 + "s15", { "bottomCenter" : vector(517.95, 73.3, 162) * millimeter, "topCenter" : vector(517.95, 72.55, 162) * millimeter, "radius" : 4.15 * millimeter });
    fCylinder(context, A0 + "s16", { "bottomCenter" : vector(517.95, 72.55, 162) * millimeter, "topCenter" : vector(517.95, 71.8, 162) * millimeter, "radius" : 4.3 * millimeter });
    fCylinder(context, A0 + "s17", { "bottomCenter" : vector(517.95, 71.8, 162) * millimeter, "topCenter" : vector(517.95, 71.05, 162) * millimeter, "radius" : 4.45 * millimeter });
    fCylinder(context, A0 + "s18", { "bottomCenter" : vector(517.95, 71.05, 162) * millimeter, "topCenter" : vector(517.95, 70.3, 162) * millimeter, "radius" : 4.6 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "v1_ventR_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "v1_ventR_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(517.95, 39, 162) * millimeter, "topCenter" : vector(517.95, 54.5, 162) * millimeter, "radius" : 2 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(517.95, 53.5, 162) * millimeter, "topCenter" : vector(517.95, 78.5, 162) * millimeter, "radius" : 2.5 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(508.95, 50, 171) * millimeter, "topCenter" : vector(508.95, 60.5, 171) * millimeter, "radius" : 1.6 * millimeter });
    fCuboid(context, C1 + "o3", { "corner1" : vector(506, 53.6, 167.8) * millimeter, "corner2" : vector(511.9, 56.4, 176.5) * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(526.95, 50, 171) * millimeter, "topCenter" : vector(526.95, 60.5, 171) * millimeter, "radius" : 1.6 * millimeter });
    fCuboid(context, C1 + "o5", { "corner1" : vector(524, 53.6, 167.8) * millimeter, "corner2" : vector(529.9, 56.4, 176.5) * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "v1_ventR_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "[v1] Ablass rechts (hi)", color(0.55, 0.6, 0.65));
}

function arc_v1_pi(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "v1_pi_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(366, -80, 9) * millimeter, "corner2" : vector(451, -24, 26) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "v1_pi_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "[v1] Raspberry Pi 3B+ (Dummy)", color(0.2, 0.6, 0.3));
}

function arc_v1_uno(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "v1_uno_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(465, -80, 9) * millimeter, "corner2" : vector(533.6, -26.7, 44) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "v1_uno_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "[v1] Arduino Uno + CNC Shield (Dummy)", color(0.15, 0.45, 0.65));
}

function arc_v1_psu(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "v1_psu_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(390, 8, 3) * millimeter, "corner2" : vector(510, 58, 33) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "v1_psu_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "[v1] Netzteil 24 V (Dummy)", color(0.7, 0.7, 0.72));
}

annotation { "Feature Type Name" : "IHHT Ablage" }
export const ihhtAblage = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
    }
    {
        const prog = new box("");
        var failed = [];
        var okCount = 0;
        try
        {
            arc_v1_base(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "v1_base @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            arc_v1_frame(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "v1_frame @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            arc_v1_lid(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "v1_lid @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            arc_v1_front(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "v1_front @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            arc_v1_insKey(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "v1_insKey @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            arc_v1_insPi(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "v1_insPi @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            arc_v1_bridge(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "v1_bridge @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            arc_v1_cpl1o(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "v1_cpl1o @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            arc_v1_cpl1u(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "v1_cpl1u @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            arc_v1_cpl2o(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "v1_cpl2o @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            arc_v1_cpl2u(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "v1_cpl2u @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            arc_v1_cpl3o(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "v1_cpl3o @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            arc_v1_cpl3u(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "v1_cpl3u @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            arc_v1_cpl4o(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "v1_cpl4o @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            arc_v1_cpl4u(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "v1_cpl4u @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            arc_v1_back(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "v1_back @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            arc_v1_maniLo(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "v1_maniLo @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            arc_v1_maniHi(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "v1_maniHi @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            arc_v1_maniMix(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "v1_maniMix @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            arc_v1_ventL(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "v1_ventL @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            arc_v1_ventR(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "v1_ventR @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            arc_v1_pi(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "v1_pi @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            arc_v1_uno(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "v1_uno @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            arc_v1_psu(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "v1_psu @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        if (size(failed) > 0)
        {
            var msg = "OK: " ~ toString(okCount) ~ " von 24 Teilen. Fehler: ";
            for (var s in failed)
                msg = msg ~ s ~ " || ";
            reportFeatureWarning(context, id, msg);
        }
    });

