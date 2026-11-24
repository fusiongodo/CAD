FeatureScript 3112;
import(path : "onshape/std/geometry.fs", version : "3112.0");
// Development-Wand (Lochraster, 2 QV): erzeugt von build.py aus teile/wand_platte.py, teile/wand_grundplatte.py, teile/wand_knotenblech.py, teile/wand_traeger.py, teile/wand_halter.py, teile/wand_dummies.py (Bibliothek ihht/). Nicht von Hand aendern.

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

function dw_dwWand(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "dwWand_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-123, 0, 6) * millimeter, "corner2" : vector(123, 6, 252) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-123, 5.99, 248) * millimeter, "corner2" : vector(123, 16, 252) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(-123, 5.99, 6) * millimeter, "corner2" : vector(123, 16, 18) * millimeter });
    fCuboid(context, A0 + "s3", { "corner1" : vector(-123, 5.99, 6) * millimeter, "corner2" : vector(-119, 16, 252) * millimeter });
    fCuboid(context, A0 + "s4", { "corner1" : vector(119, 5.99, 6) * millimeter, "corner2" : vector(123, 16, 252) * millimeter });
    fCuboid(context, A0 + "s5", { "corner1" : vector(-119.01, 5.99, 140) * millimeter, "corner2" : vector(119.01, 16, 143) * millimeter });
    prismOp(context, A0 + "s6", vector(-123, 5.99, 252) * millimeter, [[37, 0], [43.657, 0], [0, -43.657], [0, -37]], 10.01);
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(-73, 5.99, 198) * millimeter, "topCenter" : vector(-73, 16, 198) * millimeter, "radius" : 49.5 * millimeter });
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(73, 5.99, 198) * millimeter, "topCenter" : vector(73, 16, 198) * millimeter, "radius" : 49.5 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "dwWand_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "dwWand_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-73, 6, 198) * millimeter, "topCenter" : vector(-73, 17, 198) * millimeter, "radius" : 47 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(73, 6, 198) * millimeter, "topCenter" : vector(73, 17, 198) * millimeter, "radius" : 47 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(-73, -1, 198) * millimeter, "topCenter" : vector(-73, 17, 198) * millimeter, "radius" : 37.5 * millimeter });
    fCuboid(context, C1 + "o3", { "corner1" : vector(-118.5, -1, 177.5) * millimeter, "corner2" : vector(-27.5, 17, 218.5) * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(-99, -1, 164.5) * millimeter, "topCenter" : vector(-99, 7, 164.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(-99, -1, 231.5) * millimeter, "topCenter" : vector(-99, 7, 231.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(-47, -1, 164.5) * millimeter, "topCenter" : vector(-47, 7, 164.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(-47, -1, 231.5) * millimeter, "topCenter" : vector(-47, 7, 231.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(73, -1, 198) * millimeter, "topCenter" : vector(73, 17, 198) * millimeter, "radius" : 37.5 * millimeter });
    fCuboid(context, C1 + "o9", { "corner1" : vector(27.5, -1, 177.5) * millimeter, "corner2" : vector(118.5, 17, 218.5) * millimeter });
    fCylinder(context, C1 + "o10", { "bottomCenter" : vector(47, -1, 164.5) * millimeter, "topCenter" : vector(47, 7, 164.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o11", { "bottomCenter" : vector(47, -1, 231.5) * millimeter, "topCenter" : vector(47, 7, 231.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o12", { "bottomCenter" : vector(99, -1, 164.5) * millimeter, "topCenter" : vector(99, 7, 164.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o13", { "bottomCenter" : vector(99, -1, 231.5) * millimeter, "topCenter" : vector(99, 7, 231.5) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o14", { "bottomCenter" : vector(-13.5, -1, 222.5) * millimeter, "topCenter" : vector(-13.5, 7, 222.5) * millimeter, "radius" : 9 * millimeter });
    fCylinder(context, C1 + "o15", { "bottomCenter" : vector(13.5, -1, 222.5) * millimeter, "topCenter" : vector(13.5, 7, 222.5) * millimeter, "radius" : 9 * millimeter });
    fCylinder(context, C1 + "o16", { "bottomCenter" : vector(-95, 8, 5) * millimeter, "topCenter" : vector(-95, 8, 16) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o17", { "bottomCenter" : vector(-55, 8, 5) * millimeter, "topCenter" : vector(-55, 8, 16) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o18", { "bottomCenter" : vector(-15, 8, 5) * millimeter, "topCenter" : vector(-15, 8, 16) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o19", { "bottomCenter" : vector(15, 8, 5) * millimeter, "topCenter" : vector(15, 8, 16) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o20", { "bottomCenter" : vector(55, 8, 5) * millimeter, "topCenter" : vector(55, 8, 16) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o21", { "bottomCenter" : vector(95, 8, 5) * millimeter, "topCenter" : vector(95, 8, 16) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o22", { "bottomCenter" : vector(-105, -1, 26) * millimeter, "topCenter" : vector(-105, 7, 26) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o23", { "bottomCenter" : vector(-95, -1, 26) * millimeter, "topCenter" : vector(-95, 7, 26) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o24", { "bottomCenter" : vector(-85, -1, 26) * millimeter, "topCenter" : vector(-85, 7, 26) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o25", { "bottomCenter" : vector(-75, -1, 26) * millimeter, "topCenter" : vector(-75, 7, 26) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o26", { "bottomCenter" : vector(-65, -1, 26) * millimeter, "topCenter" : vector(-65, 7, 26) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o27", { "bottomCenter" : vector(-55, -1, 26) * millimeter, "topCenter" : vector(-55, 7, 26) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o28", { "bottomCenter" : vector(-45, -1, 26) * millimeter, "topCenter" : vector(-45, 7, 26) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o29", { "bottomCenter" : vector(-35, -1, 26) * millimeter, "topCenter" : vector(-35, 7, 26) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o30", { "bottomCenter" : vector(-25, -1, 26) * millimeter, "topCenter" : vector(-25, 7, 26) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o31", { "bottomCenter" : vector(-15, -1, 26) * millimeter, "topCenter" : vector(-15, 7, 26) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o32", { "bottomCenter" : vector(-5, -1, 26) * millimeter, "topCenter" : vector(-5, 7, 26) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o33", { "bottomCenter" : vector(5, -1, 26) * millimeter, "topCenter" : vector(5, 7, 26) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o34", { "bottomCenter" : vector(15, -1, 26) * millimeter, "topCenter" : vector(15, 7, 26) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o35", { "bottomCenter" : vector(25, -1, 26) * millimeter, "topCenter" : vector(25, 7, 26) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o36", { "bottomCenter" : vector(35, -1, 26) * millimeter, "topCenter" : vector(35, 7, 26) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o37", { "bottomCenter" : vector(45, -1, 26) * millimeter, "topCenter" : vector(45, 7, 26) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o38", { "bottomCenter" : vector(55, -1, 26) * millimeter, "topCenter" : vector(55, 7, 26) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o39", { "bottomCenter" : vector(65, -1, 26) * millimeter, "topCenter" : vector(65, 7, 26) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o40", { "bottomCenter" : vector(75, -1, 26) * millimeter, "topCenter" : vector(75, 7, 26) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o41", { "bottomCenter" : vector(85, -1, 26) * millimeter, "topCenter" : vector(85, 7, 26) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o42", { "bottomCenter" : vector(95, -1, 26) * millimeter, "topCenter" : vector(95, 7, 26) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o43", { "bottomCenter" : vector(105, -1, 26) * millimeter, "topCenter" : vector(105, 7, 26) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o44", { "bottomCenter" : vector(-105, -1, 36) * millimeter, "topCenter" : vector(-105, 7, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o45", { "bottomCenter" : vector(-95, -1, 36) * millimeter, "topCenter" : vector(-95, 7, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o46", { "bottomCenter" : vector(-85, -1, 36) * millimeter, "topCenter" : vector(-85, 7, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o47", { "bottomCenter" : vector(-75, -1, 36) * millimeter, "topCenter" : vector(-75, 7, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o48", { "bottomCenter" : vector(-65, -1, 36) * millimeter, "topCenter" : vector(-65, 7, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o49", { "bottomCenter" : vector(-55, -1, 36) * millimeter, "topCenter" : vector(-55, 7, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o50", { "bottomCenter" : vector(-45, -1, 36) * millimeter, "topCenter" : vector(-45, 7, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o51", { "bottomCenter" : vector(-35, -1, 36) * millimeter, "topCenter" : vector(-35, 7, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o52", { "bottomCenter" : vector(-25, -1, 36) * millimeter, "topCenter" : vector(-25, 7, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o53", { "bottomCenter" : vector(-15, -1, 36) * millimeter, "topCenter" : vector(-15, 7, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o54", { "bottomCenter" : vector(-5, -1, 36) * millimeter, "topCenter" : vector(-5, 7, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o55", { "bottomCenter" : vector(5, -1, 36) * millimeter, "topCenter" : vector(5, 7, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o56", { "bottomCenter" : vector(15, -1, 36) * millimeter, "topCenter" : vector(15, 7, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o57", { "bottomCenter" : vector(25, -1, 36) * millimeter, "topCenter" : vector(25, 7, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o58", { "bottomCenter" : vector(35, -1, 36) * millimeter, "topCenter" : vector(35, 7, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o59", { "bottomCenter" : vector(45, -1, 36) * millimeter, "topCenter" : vector(45, 7, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o60", { "bottomCenter" : vector(55, -1, 36) * millimeter, "topCenter" : vector(55, 7, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o61", { "bottomCenter" : vector(65, -1, 36) * millimeter, "topCenter" : vector(65, 7, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o62", { "bottomCenter" : vector(75, -1, 36) * millimeter, "topCenter" : vector(75, 7, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o63", { "bottomCenter" : vector(85, -1, 36) * millimeter, "topCenter" : vector(85, 7, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o64", { "bottomCenter" : vector(95, -1, 36) * millimeter, "topCenter" : vector(95, 7, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o65", { "bottomCenter" : vector(105, -1, 36) * millimeter, "topCenter" : vector(105, 7, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o66", { "bottomCenter" : vector(-105, -1, 46) * millimeter, "topCenter" : vector(-105, 7, 46) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o67", { "bottomCenter" : vector(-95, -1, 46) * millimeter, "topCenter" : vector(-95, 7, 46) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o68", { "bottomCenter" : vector(-85, -1, 46) * millimeter, "topCenter" : vector(-85, 7, 46) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o69", { "bottomCenter" : vector(-75, -1, 46) * millimeter, "topCenter" : vector(-75, 7, 46) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o70", { "bottomCenter" : vector(-65, -1, 46) * millimeter, "topCenter" : vector(-65, 7, 46) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o71", { "bottomCenter" : vector(-55, -1, 46) * millimeter, "topCenter" : vector(-55, 7, 46) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o72", { "bottomCenter" : vector(-45, -1, 46) * millimeter, "topCenter" : vector(-45, 7, 46) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o73", { "bottomCenter" : vector(-35, -1, 46) * millimeter, "topCenter" : vector(-35, 7, 46) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o74", { "bottomCenter" : vector(-25, -1, 46) * millimeter, "topCenter" : vector(-25, 7, 46) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o75", { "bottomCenter" : vector(-15, -1, 46) * millimeter, "topCenter" : vector(-15, 7, 46) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o76", { "bottomCenter" : vector(-5, -1, 46) * millimeter, "topCenter" : vector(-5, 7, 46) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o77", { "bottomCenter" : vector(5, -1, 46) * millimeter, "topCenter" : vector(5, 7, 46) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o78", { "bottomCenter" : vector(15, -1, 46) * millimeter, "topCenter" : vector(15, 7, 46) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o79", { "bottomCenter" : vector(25, -1, 46) * millimeter, "topCenter" : vector(25, 7, 46) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o80", { "bottomCenter" : vector(35, -1, 46) * millimeter, "topCenter" : vector(35, 7, 46) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o81", { "bottomCenter" : vector(45, -1, 46) * millimeter, "topCenter" : vector(45, 7, 46) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o82", { "bottomCenter" : vector(55, -1, 46) * millimeter, "topCenter" : vector(55, 7, 46) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o83", { "bottomCenter" : vector(65, -1, 46) * millimeter, "topCenter" : vector(65, 7, 46) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o84", { "bottomCenter" : vector(75, -1, 46) * millimeter, "topCenter" : vector(75, 7, 46) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o85", { "bottomCenter" : vector(85, -1, 46) * millimeter, "topCenter" : vector(85, 7, 46) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o86", { "bottomCenter" : vector(95, -1, 46) * millimeter, "topCenter" : vector(95, 7, 46) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o87", { "bottomCenter" : vector(105, -1, 46) * millimeter, "topCenter" : vector(105, 7, 46) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o88", { "bottomCenter" : vector(-105, -1, 56) * millimeter, "topCenter" : vector(-105, 7, 56) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o89", { "bottomCenter" : vector(-95, -1, 56) * millimeter, "topCenter" : vector(-95, 7, 56) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o90", { "bottomCenter" : vector(-85, -1, 56) * millimeter, "topCenter" : vector(-85, 7, 56) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o91", { "bottomCenter" : vector(-75, -1, 56) * millimeter, "topCenter" : vector(-75, 7, 56) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o92", { "bottomCenter" : vector(-65, -1, 56) * millimeter, "topCenter" : vector(-65, 7, 56) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o93", { "bottomCenter" : vector(-55, -1, 56) * millimeter, "topCenter" : vector(-55, 7, 56) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o94", { "bottomCenter" : vector(-45, -1, 56) * millimeter, "topCenter" : vector(-45, 7, 56) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o95", { "bottomCenter" : vector(-35, -1, 56) * millimeter, "topCenter" : vector(-35, 7, 56) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o96", { "bottomCenter" : vector(-25, -1, 56) * millimeter, "topCenter" : vector(-25, 7, 56) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o97", { "bottomCenter" : vector(-15, -1, 56) * millimeter, "topCenter" : vector(-15, 7, 56) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o98", { "bottomCenter" : vector(-5, -1, 56) * millimeter, "topCenter" : vector(-5, 7, 56) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o99", { "bottomCenter" : vector(5, -1, 56) * millimeter, "topCenter" : vector(5, 7, 56) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o100", { "bottomCenter" : vector(15, -1, 56) * millimeter, "topCenter" : vector(15, 7, 56) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o101", { "bottomCenter" : vector(25, -1, 56) * millimeter, "topCenter" : vector(25, 7, 56) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o102", { "bottomCenter" : vector(35, -1, 56) * millimeter, "topCenter" : vector(35, 7, 56) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o103", { "bottomCenter" : vector(45, -1, 56) * millimeter, "topCenter" : vector(45, 7, 56) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o104", { "bottomCenter" : vector(55, -1, 56) * millimeter, "topCenter" : vector(55, 7, 56) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o105", { "bottomCenter" : vector(65, -1, 56) * millimeter, "topCenter" : vector(65, 7, 56) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o106", { "bottomCenter" : vector(75, -1, 56) * millimeter, "topCenter" : vector(75, 7, 56) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o107", { "bottomCenter" : vector(85, -1, 56) * millimeter, "topCenter" : vector(85, 7, 56) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o108", { "bottomCenter" : vector(95, -1, 56) * millimeter, "topCenter" : vector(95, 7, 56) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o109", { "bottomCenter" : vector(105, -1, 56) * millimeter, "topCenter" : vector(105, 7, 56) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o110", { "bottomCenter" : vector(-105, -1, 66) * millimeter, "topCenter" : vector(-105, 7, 66) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o111", { "bottomCenter" : vector(-95, -1, 66) * millimeter, "topCenter" : vector(-95, 7, 66) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o112", { "bottomCenter" : vector(-85, -1, 66) * millimeter, "topCenter" : vector(-85, 7, 66) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o113", { "bottomCenter" : vector(-75, -1, 66) * millimeter, "topCenter" : vector(-75, 7, 66) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o114", { "bottomCenter" : vector(-65, -1, 66) * millimeter, "topCenter" : vector(-65, 7, 66) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o115", { "bottomCenter" : vector(-55, -1, 66) * millimeter, "topCenter" : vector(-55, 7, 66) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o116", { "bottomCenter" : vector(-45, -1, 66) * millimeter, "topCenter" : vector(-45, 7, 66) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o117", { "bottomCenter" : vector(-35, -1, 66) * millimeter, "topCenter" : vector(-35, 7, 66) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o118", { "bottomCenter" : vector(-25, -1, 66) * millimeter, "topCenter" : vector(-25, 7, 66) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o119", { "bottomCenter" : vector(-15, -1, 66) * millimeter, "topCenter" : vector(-15, 7, 66) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o120", { "bottomCenter" : vector(-5, -1, 66) * millimeter, "topCenter" : vector(-5, 7, 66) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o121", { "bottomCenter" : vector(5, -1, 66) * millimeter, "topCenter" : vector(5, 7, 66) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o122", { "bottomCenter" : vector(15, -1, 66) * millimeter, "topCenter" : vector(15, 7, 66) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o123", { "bottomCenter" : vector(25, -1, 66) * millimeter, "topCenter" : vector(25, 7, 66) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o124", { "bottomCenter" : vector(35, -1, 66) * millimeter, "topCenter" : vector(35, 7, 66) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o125", { "bottomCenter" : vector(45, -1, 66) * millimeter, "topCenter" : vector(45, 7, 66) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o126", { "bottomCenter" : vector(55, -1, 66) * millimeter, "topCenter" : vector(55, 7, 66) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o127", { "bottomCenter" : vector(65, -1, 66) * millimeter, "topCenter" : vector(65, 7, 66) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o128", { "bottomCenter" : vector(75, -1, 66) * millimeter, "topCenter" : vector(75, 7, 66) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o129", { "bottomCenter" : vector(85, -1, 66) * millimeter, "topCenter" : vector(85, 7, 66) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o130", { "bottomCenter" : vector(95, -1, 66) * millimeter, "topCenter" : vector(95, 7, 66) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o131", { "bottomCenter" : vector(105, -1, 66) * millimeter, "topCenter" : vector(105, 7, 66) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o132", { "bottomCenter" : vector(-105, -1, 76) * millimeter, "topCenter" : vector(-105, 7, 76) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o133", { "bottomCenter" : vector(-95, -1, 76) * millimeter, "topCenter" : vector(-95, 7, 76) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o134", { "bottomCenter" : vector(-85, -1, 76) * millimeter, "topCenter" : vector(-85, 7, 76) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o135", { "bottomCenter" : vector(-75, -1, 76) * millimeter, "topCenter" : vector(-75, 7, 76) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o136", { "bottomCenter" : vector(-65, -1, 76) * millimeter, "topCenter" : vector(-65, 7, 76) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o137", { "bottomCenter" : vector(-55, -1, 76) * millimeter, "topCenter" : vector(-55, 7, 76) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o138", { "bottomCenter" : vector(-45, -1, 76) * millimeter, "topCenter" : vector(-45, 7, 76) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o139", { "bottomCenter" : vector(-35, -1, 76) * millimeter, "topCenter" : vector(-35, 7, 76) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o140", { "bottomCenter" : vector(-25, -1, 76) * millimeter, "topCenter" : vector(-25, 7, 76) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o141", { "bottomCenter" : vector(-15, -1, 76) * millimeter, "topCenter" : vector(-15, 7, 76) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o142", { "bottomCenter" : vector(-5, -1, 76) * millimeter, "topCenter" : vector(-5, 7, 76) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o143", { "bottomCenter" : vector(5, -1, 76) * millimeter, "topCenter" : vector(5, 7, 76) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o144", { "bottomCenter" : vector(15, -1, 76) * millimeter, "topCenter" : vector(15, 7, 76) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o145", { "bottomCenter" : vector(25, -1, 76) * millimeter, "topCenter" : vector(25, 7, 76) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o146", { "bottomCenter" : vector(35, -1, 76) * millimeter, "topCenter" : vector(35, 7, 76) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o147", { "bottomCenter" : vector(45, -1, 76) * millimeter, "topCenter" : vector(45, 7, 76) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o148", { "bottomCenter" : vector(55, -1, 76) * millimeter, "topCenter" : vector(55, 7, 76) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o149", { "bottomCenter" : vector(65, -1, 76) * millimeter, "topCenter" : vector(65, 7, 76) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o150", { "bottomCenter" : vector(75, -1, 76) * millimeter, "topCenter" : vector(75, 7, 76) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o151", { "bottomCenter" : vector(85, -1, 76) * millimeter, "topCenter" : vector(85, 7, 76) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o152", { "bottomCenter" : vector(95, -1, 76) * millimeter, "topCenter" : vector(95, 7, 76) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o153", { "bottomCenter" : vector(105, -1, 76) * millimeter, "topCenter" : vector(105, 7, 76) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o154", { "bottomCenter" : vector(-105, -1, 86) * millimeter, "topCenter" : vector(-105, 7, 86) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o155", { "bottomCenter" : vector(-95, -1, 86) * millimeter, "topCenter" : vector(-95, 7, 86) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o156", { "bottomCenter" : vector(-85, -1, 86) * millimeter, "topCenter" : vector(-85, 7, 86) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o157", { "bottomCenter" : vector(-75, -1, 86) * millimeter, "topCenter" : vector(-75, 7, 86) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o158", { "bottomCenter" : vector(-65, -1, 86) * millimeter, "topCenter" : vector(-65, 7, 86) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o159", { "bottomCenter" : vector(-55, -1, 86) * millimeter, "topCenter" : vector(-55, 7, 86) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o160", { "bottomCenter" : vector(-45, -1, 86) * millimeter, "topCenter" : vector(-45, 7, 86) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o161", { "bottomCenter" : vector(-35, -1, 86) * millimeter, "topCenter" : vector(-35, 7, 86) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o162", { "bottomCenter" : vector(-25, -1, 86) * millimeter, "topCenter" : vector(-25, 7, 86) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o163", { "bottomCenter" : vector(-15, -1, 86) * millimeter, "topCenter" : vector(-15, 7, 86) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o164", { "bottomCenter" : vector(-5, -1, 86) * millimeter, "topCenter" : vector(-5, 7, 86) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o165", { "bottomCenter" : vector(5, -1, 86) * millimeter, "topCenter" : vector(5, 7, 86) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o166", { "bottomCenter" : vector(15, -1, 86) * millimeter, "topCenter" : vector(15, 7, 86) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o167", { "bottomCenter" : vector(25, -1, 86) * millimeter, "topCenter" : vector(25, 7, 86) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o168", { "bottomCenter" : vector(35, -1, 86) * millimeter, "topCenter" : vector(35, 7, 86) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o169", { "bottomCenter" : vector(45, -1, 86) * millimeter, "topCenter" : vector(45, 7, 86) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o170", { "bottomCenter" : vector(55, -1, 86) * millimeter, "topCenter" : vector(55, 7, 86) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o171", { "bottomCenter" : vector(65, -1, 86) * millimeter, "topCenter" : vector(65, 7, 86) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o172", { "bottomCenter" : vector(75, -1, 86) * millimeter, "topCenter" : vector(75, 7, 86) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o173", { "bottomCenter" : vector(85, -1, 86) * millimeter, "topCenter" : vector(85, 7, 86) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o174", { "bottomCenter" : vector(95, -1, 86) * millimeter, "topCenter" : vector(95, 7, 86) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o175", { "bottomCenter" : vector(105, -1, 86) * millimeter, "topCenter" : vector(105, 7, 86) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o176", { "bottomCenter" : vector(-105, -1, 96) * millimeter, "topCenter" : vector(-105, 7, 96) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o177", { "bottomCenter" : vector(-95, -1, 96) * millimeter, "topCenter" : vector(-95, 7, 96) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o178", { "bottomCenter" : vector(-85, -1, 96) * millimeter, "topCenter" : vector(-85, 7, 96) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o179", { "bottomCenter" : vector(-75, -1, 96) * millimeter, "topCenter" : vector(-75, 7, 96) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o180", { "bottomCenter" : vector(-65, -1, 96) * millimeter, "topCenter" : vector(-65, 7, 96) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o181", { "bottomCenter" : vector(-55, -1, 96) * millimeter, "topCenter" : vector(-55, 7, 96) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o182", { "bottomCenter" : vector(-45, -1, 96) * millimeter, "topCenter" : vector(-45, 7, 96) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o183", { "bottomCenter" : vector(-35, -1, 96) * millimeter, "topCenter" : vector(-35, 7, 96) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o184", { "bottomCenter" : vector(-25, -1, 96) * millimeter, "topCenter" : vector(-25, 7, 96) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o185", { "bottomCenter" : vector(-15, -1, 96) * millimeter, "topCenter" : vector(-15, 7, 96) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o186", { "bottomCenter" : vector(-5, -1, 96) * millimeter, "topCenter" : vector(-5, 7, 96) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o187", { "bottomCenter" : vector(5, -1, 96) * millimeter, "topCenter" : vector(5, 7, 96) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o188", { "bottomCenter" : vector(15, -1, 96) * millimeter, "topCenter" : vector(15, 7, 96) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o189", { "bottomCenter" : vector(25, -1, 96) * millimeter, "topCenter" : vector(25, 7, 96) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o190", { "bottomCenter" : vector(35, -1, 96) * millimeter, "topCenter" : vector(35, 7, 96) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o191", { "bottomCenter" : vector(45, -1, 96) * millimeter, "topCenter" : vector(45, 7, 96) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o192", { "bottomCenter" : vector(55, -1, 96) * millimeter, "topCenter" : vector(55, 7, 96) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o193", { "bottomCenter" : vector(65, -1, 96) * millimeter, "topCenter" : vector(65, 7, 96) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o194", { "bottomCenter" : vector(75, -1, 96) * millimeter, "topCenter" : vector(75, 7, 96) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o195", { "bottomCenter" : vector(85, -1, 96) * millimeter, "topCenter" : vector(85, 7, 96) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o196", { "bottomCenter" : vector(95, -1, 96) * millimeter, "topCenter" : vector(95, 7, 96) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o197", { "bottomCenter" : vector(105, -1, 96) * millimeter, "topCenter" : vector(105, 7, 96) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o198", { "bottomCenter" : vector(-105, -1, 106) * millimeter, "topCenter" : vector(-105, 7, 106) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o199", { "bottomCenter" : vector(-95, -1, 106) * millimeter, "topCenter" : vector(-95, 7, 106) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o200", { "bottomCenter" : vector(-85, -1, 106) * millimeter, "topCenter" : vector(-85, 7, 106) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o201", { "bottomCenter" : vector(-75, -1, 106) * millimeter, "topCenter" : vector(-75, 7, 106) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o202", { "bottomCenter" : vector(-65, -1, 106) * millimeter, "topCenter" : vector(-65, 7, 106) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o203", { "bottomCenter" : vector(-55, -1, 106) * millimeter, "topCenter" : vector(-55, 7, 106) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o204", { "bottomCenter" : vector(-45, -1, 106) * millimeter, "topCenter" : vector(-45, 7, 106) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o205", { "bottomCenter" : vector(-35, -1, 106) * millimeter, "topCenter" : vector(-35, 7, 106) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o206", { "bottomCenter" : vector(-25, -1, 106) * millimeter, "topCenter" : vector(-25, 7, 106) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o207", { "bottomCenter" : vector(-15, -1, 106) * millimeter, "topCenter" : vector(-15, 7, 106) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o208", { "bottomCenter" : vector(-5, -1, 106) * millimeter, "topCenter" : vector(-5, 7, 106) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o209", { "bottomCenter" : vector(5, -1, 106) * millimeter, "topCenter" : vector(5, 7, 106) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o210", { "bottomCenter" : vector(15, -1, 106) * millimeter, "topCenter" : vector(15, 7, 106) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o211", { "bottomCenter" : vector(25, -1, 106) * millimeter, "topCenter" : vector(25, 7, 106) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o212", { "bottomCenter" : vector(35, -1, 106) * millimeter, "topCenter" : vector(35, 7, 106) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o213", { "bottomCenter" : vector(45, -1, 106) * millimeter, "topCenter" : vector(45, 7, 106) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o214", { "bottomCenter" : vector(55, -1, 106) * millimeter, "topCenter" : vector(55, 7, 106) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o215", { "bottomCenter" : vector(65, -1, 106) * millimeter, "topCenter" : vector(65, 7, 106) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o216", { "bottomCenter" : vector(75, -1, 106) * millimeter, "topCenter" : vector(75, 7, 106) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o217", { "bottomCenter" : vector(85, -1, 106) * millimeter, "topCenter" : vector(85, 7, 106) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o218", { "bottomCenter" : vector(95, -1, 106) * millimeter, "topCenter" : vector(95, 7, 106) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o219", { "bottomCenter" : vector(105, -1, 106) * millimeter, "topCenter" : vector(105, 7, 106) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o220", { "bottomCenter" : vector(-105, -1, 116) * millimeter, "topCenter" : vector(-105, 7, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o221", { "bottomCenter" : vector(-95, -1, 116) * millimeter, "topCenter" : vector(-95, 7, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o222", { "bottomCenter" : vector(-85, -1, 116) * millimeter, "topCenter" : vector(-85, 7, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o223", { "bottomCenter" : vector(-75, -1, 116) * millimeter, "topCenter" : vector(-75, 7, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o224", { "bottomCenter" : vector(-65, -1, 116) * millimeter, "topCenter" : vector(-65, 7, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o225", { "bottomCenter" : vector(-55, -1, 116) * millimeter, "topCenter" : vector(-55, 7, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o226", { "bottomCenter" : vector(-45, -1, 116) * millimeter, "topCenter" : vector(-45, 7, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o227", { "bottomCenter" : vector(-35, -1, 116) * millimeter, "topCenter" : vector(-35, 7, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o228", { "bottomCenter" : vector(-25, -1, 116) * millimeter, "topCenter" : vector(-25, 7, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o229", { "bottomCenter" : vector(-15, -1, 116) * millimeter, "topCenter" : vector(-15, 7, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o230", { "bottomCenter" : vector(-5, -1, 116) * millimeter, "topCenter" : vector(-5, 7, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o231", { "bottomCenter" : vector(5, -1, 116) * millimeter, "topCenter" : vector(5, 7, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o232", { "bottomCenter" : vector(15, -1, 116) * millimeter, "topCenter" : vector(15, 7, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o233", { "bottomCenter" : vector(25, -1, 116) * millimeter, "topCenter" : vector(25, 7, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o234", { "bottomCenter" : vector(35, -1, 116) * millimeter, "topCenter" : vector(35, 7, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o235", { "bottomCenter" : vector(45, -1, 116) * millimeter, "topCenter" : vector(45, 7, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o236", { "bottomCenter" : vector(55, -1, 116) * millimeter, "topCenter" : vector(55, 7, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o237", { "bottomCenter" : vector(65, -1, 116) * millimeter, "topCenter" : vector(65, 7, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o238", { "bottomCenter" : vector(75, -1, 116) * millimeter, "topCenter" : vector(75, 7, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o239", { "bottomCenter" : vector(85, -1, 116) * millimeter, "topCenter" : vector(85, 7, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o240", { "bottomCenter" : vector(95, -1, 116) * millimeter, "topCenter" : vector(95, 7, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o241", { "bottomCenter" : vector(105, -1, 116) * millimeter, "topCenter" : vector(105, 7, 116) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o242", { "bottomCenter" : vector(-105, -1, 126) * millimeter, "topCenter" : vector(-105, 7, 126) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o243", { "bottomCenter" : vector(-95, -1, 126) * millimeter, "topCenter" : vector(-95, 7, 126) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o244", { "bottomCenter" : vector(-85, -1, 126) * millimeter, "topCenter" : vector(-85, 7, 126) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o245", { "bottomCenter" : vector(-75, -1, 126) * millimeter, "topCenter" : vector(-75, 7, 126) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o246", { "bottomCenter" : vector(-65, -1, 126) * millimeter, "topCenter" : vector(-65, 7, 126) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o247", { "bottomCenter" : vector(-55, -1, 126) * millimeter, "topCenter" : vector(-55, 7, 126) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o248", { "bottomCenter" : vector(-45, -1, 126) * millimeter, "topCenter" : vector(-45, 7, 126) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o249", { "bottomCenter" : vector(-35, -1, 126) * millimeter, "topCenter" : vector(-35, 7, 126) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o250", { "bottomCenter" : vector(-25, -1, 126) * millimeter, "topCenter" : vector(-25, 7, 126) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o251", { "bottomCenter" : vector(-15, -1, 126) * millimeter, "topCenter" : vector(-15, 7, 126) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o252", { "bottomCenter" : vector(-5, -1, 126) * millimeter, "topCenter" : vector(-5, 7, 126) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o253", { "bottomCenter" : vector(5, -1, 126) * millimeter, "topCenter" : vector(5, 7, 126) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o254", { "bottomCenter" : vector(15, -1, 126) * millimeter, "topCenter" : vector(15, 7, 126) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o255", { "bottomCenter" : vector(25, -1, 126) * millimeter, "topCenter" : vector(25, 7, 126) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o256", { "bottomCenter" : vector(35, -1, 126) * millimeter, "topCenter" : vector(35, 7, 126) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o257", { "bottomCenter" : vector(45, -1, 126) * millimeter, "topCenter" : vector(45, 7, 126) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o258", { "bottomCenter" : vector(55, -1, 126) * millimeter, "topCenter" : vector(55, 7, 126) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o259", { "bottomCenter" : vector(65, -1, 126) * millimeter, "topCenter" : vector(65, 7, 126) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o260", { "bottomCenter" : vector(75, -1, 126) * millimeter, "topCenter" : vector(75, 7, 126) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o261", { "bottomCenter" : vector(85, -1, 126) * millimeter, "topCenter" : vector(85, 7, 126) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o262", { "bottomCenter" : vector(95, -1, 126) * millimeter, "topCenter" : vector(95, 7, 126) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o263", { "bottomCenter" : vector(105, -1, 126) * millimeter, "topCenter" : vector(105, 7, 126) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o264", { "bottomCenter" : vector(-35, -1, 156) * millimeter, "topCenter" : vector(-35, 7, 156) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o265", { "bottomCenter" : vector(-25, -1, 156) * millimeter, "topCenter" : vector(-25, 7, 156) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o266", { "bottomCenter" : vector(-15, -1, 156) * millimeter, "topCenter" : vector(-15, 7, 156) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o267", { "bottomCenter" : vector(-5, -1, 156) * millimeter, "topCenter" : vector(-5, 7, 156) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o268", { "bottomCenter" : vector(5, -1, 156) * millimeter, "topCenter" : vector(5, 7, 156) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o269", { "bottomCenter" : vector(15, -1, 156) * millimeter, "topCenter" : vector(15, 7, 156) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o270", { "bottomCenter" : vector(25, -1, 156) * millimeter, "topCenter" : vector(25, 7, 156) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o271", { "bottomCenter" : vector(35, -1, 156) * millimeter, "topCenter" : vector(35, 7, 156) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o272", { "bottomCenter" : vector(-25, -1, 166) * millimeter, "topCenter" : vector(-25, 7, 166) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o273", { "bottomCenter" : vector(-15, -1, 166) * millimeter, "topCenter" : vector(-15, 7, 166) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o274", { "bottomCenter" : vector(-5, -1, 166) * millimeter, "topCenter" : vector(-5, 7, 166) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o275", { "bottomCenter" : vector(5, -1, 166) * millimeter, "topCenter" : vector(5, 7, 166) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o276", { "bottomCenter" : vector(15, -1, 166) * millimeter, "topCenter" : vector(15, 7, 166) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o277", { "bottomCenter" : vector(25, -1, 166) * millimeter, "topCenter" : vector(25, 7, 166) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o278", { "bottomCenter" : vector(-15, -1, 176) * millimeter, "topCenter" : vector(-15, 7, 176) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o279", { "bottomCenter" : vector(-5, -1, 176) * millimeter, "topCenter" : vector(-5, 7, 176) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o280", { "bottomCenter" : vector(5, -1, 176) * millimeter, "topCenter" : vector(5, 7, 176) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o281", { "bottomCenter" : vector(15, -1, 176) * millimeter, "topCenter" : vector(15, 7, 176) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o282", { "bottomCenter" : vector(-15, -1, 186) * millimeter, "topCenter" : vector(-15, 7, 186) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o283", { "bottomCenter" : vector(-5, -1, 186) * millimeter, "topCenter" : vector(-5, 7, 186) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o284", { "bottomCenter" : vector(5, -1, 186) * millimeter, "topCenter" : vector(5, 7, 186) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o285", { "bottomCenter" : vector(15, -1, 186) * millimeter, "topCenter" : vector(15, 7, 186) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o286", { "bottomCenter" : vector(-15, -1, 196) * millimeter, "topCenter" : vector(-15, 7, 196) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o287", { "bottomCenter" : vector(-5, -1, 196) * millimeter, "topCenter" : vector(-5, 7, 196) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o288", { "bottomCenter" : vector(5, -1, 196) * millimeter, "topCenter" : vector(5, 7, 196) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o289", { "bottomCenter" : vector(15, -1, 196) * millimeter, "topCenter" : vector(15, 7, 196) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o290", { "bottomCenter" : vector(-15, -1, 206) * millimeter, "topCenter" : vector(-15, 7, 206) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o291", { "bottomCenter" : vector(-5, -1, 206) * millimeter, "topCenter" : vector(-5, 7, 206) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o292", { "bottomCenter" : vector(5, -1, 206) * millimeter, "topCenter" : vector(5, 7, 206) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o293", { "bottomCenter" : vector(15, -1, 206) * millimeter, "topCenter" : vector(15, 7, 206) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o294", { "bottomCenter" : vector(-25, -1, 236) * millimeter, "topCenter" : vector(-25, 7, 236) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o295", { "bottomCenter" : vector(-15, -1, 236) * millimeter, "topCenter" : vector(-15, 7, 236) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o296", { "bottomCenter" : vector(-5, -1, 236) * millimeter, "topCenter" : vector(-5, 7, 236) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o297", { "bottomCenter" : vector(5, -1, 236) * millimeter, "topCenter" : vector(5, 7, 236) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o298", { "bottomCenter" : vector(15, -1, 236) * millimeter, "topCenter" : vector(15, 7, 236) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o299", { "bottomCenter" : vector(25, -1, 236) * millimeter, "topCenter" : vector(25, 7, 236) * millimeter, "radius" : 1.7 * millimeter });
    prismOp(context, C1 + "o300", vector(-123, -1, 252) * millimeter, [[-1, 1], [39, 1], [-1, -39]], 18);
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "dwWand_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Dev-Wand Wandplatte (6 dick, Randsteg, M3-Raster 10)", color(0.87, 0.81, 0.69));
}

function dw_dwGrund(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "dwGrund_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-123, -80, 0) * millimeter, "corner2" : vector(123, 80, 6) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "dwGrund_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "dwGrund_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-95, 8, -1) * millimeter, "topCenter" : vector(-95, 8, 7) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-95, 8, -1) * millimeter, "topCenter" : vector(-95, 8, 3.2) * millimeter, "radius" : 3.25 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(-55, 8, -1) * millimeter, "topCenter" : vector(-55, 8, 7) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(-55, 8, -1) * millimeter, "topCenter" : vector(-55, 8, 3.2) * millimeter, "radius" : 3.25 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(-15, 8, -1) * millimeter, "topCenter" : vector(-15, 8, 7) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(-15, 8, -1) * millimeter, "topCenter" : vector(-15, 8, 3.2) * millimeter, "radius" : 3.25 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(15, 8, -1) * millimeter, "topCenter" : vector(15, 8, 7) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(15, 8, -1) * millimeter, "topCenter" : vector(15, 8, 3.2) * millimeter, "radius" : 3.25 * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(55, 8, -1) * millimeter, "topCenter" : vector(55, 8, 7) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(55, 8, -1) * millimeter, "topCenter" : vector(55, 8, 3.2) * millimeter, "radius" : 3.25 * millimeter });
    fCylinder(context, C1 + "o10", { "bottomCenter" : vector(95, 8, -1) * millimeter, "topCenter" : vector(95, 8, 7) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o11", { "bottomCenter" : vector(95, 8, -1) * millimeter, "topCenter" : vector(95, 8, 3.2) * millimeter, "radius" : 3.25 * millimeter });
    fCylinder(context, C1 + "o12", { "bottomCenter" : vector(-105, -30, -1) * millimeter, "topCenter" : vector(-105, -30, 7) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o13", { "bottomCenter" : vector(-105, -30, -1) * millimeter, "topCenter" : vector(-105, -30, 3.2) * millimeter, "radius" : 3.25 * millimeter });
    fCylinder(context, C1 + "o14", { "bottomCenter" : vector(-105, -60, -1) * millimeter, "topCenter" : vector(-105, -60, 7) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o15", { "bottomCenter" : vector(-105, -60, -1) * millimeter, "topCenter" : vector(-105, -60, 3.2) * millimeter, "radius" : 3.25 * millimeter });
    fCylinder(context, C1 + "o16", { "bottomCenter" : vector(105, -30, -1) * millimeter, "topCenter" : vector(105, -30, 7) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o17", { "bottomCenter" : vector(105, -30, -1) * millimeter, "topCenter" : vector(105, -30, 3.2) * millimeter, "radius" : 3.25 * millimeter });
    fCylinder(context, C1 + "o18", { "bottomCenter" : vector(105, -60, -1) * millimeter, "topCenter" : vector(105, -60, 7) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o19", { "bottomCenter" : vector(105, -60, -1) * millimeter, "topCenter" : vector(105, -60, 3.2) * millimeter, "radius" : 3.25 * millimeter });
    fCylinder(context, C1 + "o20", { "bottomCenter" : vector(-105, 36, -1) * millimeter, "topCenter" : vector(-105, 36, 7) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o21", { "bottomCenter" : vector(-105, 36, -1) * millimeter, "topCenter" : vector(-105, 36, 3.2) * millimeter, "radius" : 3.25 * millimeter });
    fCylinder(context, C1 + "o22", { "bottomCenter" : vector(-105, 66, -1) * millimeter, "topCenter" : vector(-105, 66, 7) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o23", { "bottomCenter" : vector(-105, 66, -1) * millimeter, "topCenter" : vector(-105, 66, 3.2) * millimeter, "radius" : 3.25 * millimeter });
    fCylinder(context, C1 + "o24", { "bottomCenter" : vector(105, 36, -1) * millimeter, "topCenter" : vector(105, 36, 7) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o25", { "bottomCenter" : vector(105, 36, -1) * millimeter, "topCenter" : vector(105, 36, 3.2) * millimeter, "radius" : 3.25 * millimeter });
    fCylinder(context, C1 + "o26", { "bottomCenter" : vector(105, 66, -1) * millimeter, "topCenter" : vector(105, 66, 7) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o27", { "bottomCenter" : vector(105, 66, -1) * millimeter, "topCenter" : vector(105, 66, 3.2) * millimeter, "radius" : 3.25 * millimeter });
    fCuboid(context, C1 + "o28", { "corner1" : vector(-14.5, -41.6, -1) * millimeter, "corner2" : vector(-12.5, -38.4, 7) * millimeter });
    fCuboid(context, C1 + "o29", { "corner1" : vector(12.5, -41.6, -1) * millimeter, "corner2" : vector(14.5, -38.4, 7) * millimeter });
    fCuboid(context, C1 + "o30", { "corner1" : vector(-13.5, -41.6, -1) * millimeter, "corner2" : vector(13.5, -38.4, 1.5) * millimeter });
    fCuboid(context, C1 + "o31", { "corner1" : vector(-14.5, -73.6, -1) * millimeter, "corner2" : vector(-12.5, -70.4, 7) * millimeter });
    fCuboid(context, C1 + "o32", { "corner1" : vector(12.5, -73.6, -1) * millimeter, "corner2" : vector(14.5, -70.4, 7) * millimeter });
    fCuboid(context, C1 + "o33", { "corner1" : vector(-13.5, -73.6, -1) * millimeter, "corner2" : vector(13.5, -70.4, 1.5) * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "dwGrund_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Dev-Wand Grundplatte 246 x 160 (Schrauben von unten)", color(0.87, 0.81, 0.69));
}

function dw_dwKnotenVL(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "dwKnotenVL_a0";
    prismXOp(context, A0 + "s0", vector(-118.5, 0, 0) * millimeter, [[0, 81], [-5, 81], [-70, 11], [-70, 6], [-10.5, 6], [-10.5, 18.5], [0, 18.5]], 5);
    fCuboid(context, A0 + "s1", { "corner1" : vector(-118.5, -5, 18.5) * millimeter, "corner2" : vector(-100.5, 0, 81) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(-118.5, -70, 6) * millimeter, "corner2" : vector(-100.5, -10.5, 11) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "dwKnotenVL_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "dwKnotenVL_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-105, 1, 36) * millimeter, "topCenter" : vector(-105, -6, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-105, 1, 66) * millimeter, "topCenter" : vector(-105, -6, 66) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(-105, -30, 5) * millimeter, "topCenter" : vector(-105, -30, 12) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(-105, -60, 5) * millimeter, "topCenter" : vector(-105, -60, 12) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "dwKnotenVL_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Dev-Wand Knotenblech vorn links", color(0.78, 0.8, 0.82));
}

function dw_dwKnotenVR(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "dwKnotenVR_a0";
    prismXOp(context, A0 + "s0", vector(113.5, 0, 0) * millimeter, [[0, 81], [-5, 81], [-70, 11], [-70, 6], [-10.5, 6], [-10.5, 18.5], [0, 18.5]], 5);
    fCuboid(context, A0 + "s1", { "corner1" : vector(100.5, -5, 18.5) * millimeter, "corner2" : vector(118.5, 0, 81) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(100.5, -70, 6) * millimeter, "corner2" : vector(118.5, -10.5, 11) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "dwKnotenVR_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "dwKnotenVR_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(105, 1, 36) * millimeter, "topCenter" : vector(105, -6, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(105, 1, 66) * millimeter, "topCenter" : vector(105, -6, 66) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(105, -30, 5) * millimeter, "topCenter" : vector(105, -30, 12) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(105, -60, 5) * millimeter, "topCenter" : vector(105, -60, 12) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "dwKnotenVR_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Dev-Wand Knotenblech vorn rechts", color(0.78, 0.8, 0.82));
}

function dw_dwKnotenHL(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "dwKnotenHL_a0";
    prismXOp(context, A0 + "s0", vector(-118.5, 0, 0) * millimeter, [[6, 18.5], [16.5, 18.5], [16.5, 6], [76, 6], [76, 11], [11, 81], [6, 81]], 5);
    fCuboid(context, A0 + "s1", { "corner1" : vector(-118.5, 6, 18.5) * millimeter, "corner2" : vector(-100.5, 11, 81) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(-118.5, 16.5, 6) * millimeter, "corner2" : vector(-100.5, 76, 11) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "dwKnotenHL_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "dwKnotenHL_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-105, 5, 36) * millimeter, "topCenter" : vector(-105, 12, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-105, 5, 66) * millimeter, "topCenter" : vector(-105, 12, 66) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(-105, 36, 5) * millimeter, "topCenter" : vector(-105, 36, 12) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(-105, 66, 5) * millimeter, "topCenter" : vector(-105, 66, 12) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "dwKnotenHL_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Dev-Wand Knotenblech hinten links", color(0.78, 0.8, 0.82));
}

function dw_dwKnotenHR(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "dwKnotenHR_a0";
    prismXOp(context, A0 + "s0", vector(113.5, 0, 0) * millimeter, [[6, 18.5], [16.5, 18.5], [16.5, 6], [76, 6], [76, 11], [11, 81], [6, 81]], 5);
    fCuboid(context, A0 + "s1", { "corner1" : vector(100.5, 6, 18.5) * millimeter, "corner2" : vector(118.5, 11, 81) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(100.5, 16.5, 6) * millimeter, "corner2" : vector(118.5, 76, 11) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "dwKnotenHR_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "dwKnotenHR_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(105, 5, 36) * millimeter, "topCenter" : vector(105, 12, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(105, 5, 66) * millimeter, "topCenter" : vector(105, 12, 66) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(105, 36, 5) * millimeter, "topCenter" : vector(105, 36, 12) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(105, 66, 5) * millimeter, "topCenter" : vector(105, 66, 12) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "dwKnotenHR_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Dev-Wand Knotenblech hinten rechts", color(0.78, 0.8, 0.82));
}

function dw_dwTraegerPi(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "dwTraegerPi_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-91, 6, 26) * millimeter, "corner2" : vector(-19, 9, 86) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(-84, 8.99, 31.5) * millimeter, "topCenter" : vector(-84, 17, 31.5) * millimeter, "radius" : 3 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-84, 8.99, 80.5) * millimeter, "topCenter" : vector(-84, 17, 80.5) * millimeter, "radius" : 3 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(-26, 8.99, 31.5) * millimeter, "topCenter" : vector(-26, 17, 31.5) * millimeter, "radius" : 3 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(-26, 8.99, 80.5) * millimeter, "topCenter" : vector(-26, 17, 80.5) * millimeter, "radius" : 3 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "dwTraegerPi_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "dwTraegerPi_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-84, 7, 31.5) * millimeter, "topCenter" : vector(-84, 18, 31.5) * millimeter, "radius" : 1.1 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-84, 7, 80.5) * millimeter, "topCenter" : vector(-84, 18, 80.5) * millimeter, "radius" : 1.1 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(-26, 7, 31.5) * millimeter, "topCenter" : vector(-26, 18, 31.5) * millimeter, "radius" : 1.1 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(-26, 7, 80.5) * millimeter, "topCenter" : vector(-26, 18, 80.5) * millimeter, "radius" : 1.1 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(-75, 5, 36) * millimeter, "topCenter" : vector(-75, 10, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(-75, 5, 76) * millimeter, "topCenter" : vector(-75, 10, 76) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(-35, 5, 36) * millimeter, "topCenter" : vector(-35, 10, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(-35, 5, 76) * millimeter, "topCenter" : vector(-35, 10, 76) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "dwTraegerPi_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Dev-Wand Träger Pi (58 x 49, M2,5)", color(0.78, 0.8, 0.82));
}

function dw_dwTraegerPico(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "dwTraegerPico_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(19, 6, 26) * millimeter, "corner2" : vector(91, 9, 86) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(26, 8.99, 31.5) * millimeter, "topCenter" : vector(26, 17, 31.5) * millimeter, "radius" : 3 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(26, 8.99, 80.5) * millimeter, "topCenter" : vector(26, 17, 80.5) * millimeter, "radius" : 3 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(84, 8.99, 31.5) * millimeter, "topCenter" : vector(84, 17, 31.5) * millimeter, "radius" : 3 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(84, 8.99, 80.5) * millimeter, "topCenter" : vector(84, 17, 80.5) * millimeter, "radius" : 3 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "dwTraegerPico_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "dwTraegerPico_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(26, 7, 31.5) * millimeter, "topCenter" : vector(26, 18, 31.5) * millimeter, "radius" : 1.1 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(26, 7, 80.5) * millimeter, "topCenter" : vector(26, 18, 80.5) * millimeter, "radius" : 1.1 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(84, 7, 31.5) * millimeter, "topCenter" : vector(84, 18, 31.5) * millimeter, "radius" : 1.1 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(84, 7, 80.5) * millimeter, "topCenter" : vector(84, 18, 80.5) * millimeter, "radius" : 1.1 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(35, 5, 36) * millimeter, "topCenter" : vector(35, 10, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(35, 5, 76) * millimeter, "topCenter" : vector(35, 10, 76) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(75, 5, 36) * millimeter, "topCenter" : vector(75, 10, 36) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(75, 5, 76) * millimeter, "topCenter" : vector(75, 10, 76) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "dwTraegerPico_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Dev-Wand Träger Pico (58 x 49, M2,5)", color(0.78, 0.8, 0.82));
}

function dw_dwHalterHi(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "dwHalterHi_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-27, -4, 111) * millimeter, "corner2" : vector(-3, 0, 171) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-27, -14, 111) * millimeter, "corner2" : vector(-24, -3.99, 171) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(-6, -14, 111) * millimeter, "corner2" : vector(-3, -3.99, 171) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "dwHalterHi_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "dwHalterHi_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-15, -5, 126) * millimeter, "topCenter" : vector(-15, 1, 126) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-15, -5, 156) * millimeter, "topCenter" : vector(-15, 1, 156) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o2", { "corner1" : vector(-28, -9, 117.4) * millimeter, "corner2" : vector(-2, -7, 120.6) * millimeter });
    fCuboid(context, C1 + "o3", { "corner1" : vector(-28, -9, 131.4) * millimeter, "corner2" : vector(-2, -7, 134.6) * millimeter });
    fCuboid(context, C1 + "o4", { "corner1" : vector(-28, -9, 147.4) * millimeter, "corner2" : vector(-2, -7, 150.6) * millimeter });
    fCuboid(context, C1 + "o5", { "corner1" : vector(-28, -9, 161.4) * millimeter, "corner2" : vector(-2, -7, 164.6) * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "dwHalterHi_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Dev-Wand Rückenhalter OCS hi (Abstand 14, Kabelbinder)", color(0.78, 0.8, 0.82));
}

function dw_dwHalterLo(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "dwHalterLo_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(3, -4, 111) * millimeter, "corner2" : vector(27, 0, 171) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(3, -14, 111) * millimeter, "corner2" : vector(6, -3.99, 171) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(24, -14, 111) * millimeter, "corner2" : vector(27, -3.99, 171) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "dwHalterLo_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "dwHalterLo_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(15, -5, 126) * millimeter, "topCenter" : vector(15, 1, 126) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(15, -5, 156) * millimeter, "topCenter" : vector(15, 1, 156) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o2", { "corner1" : vector(2, -9, 117.4) * millimeter, "corner2" : vector(28, -7, 120.6) * millimeter });
    fCuboid(context, C1 + "o3", { "corner1" : vector(2, -9, 131.4) * millimeter, "corner2" : vector(28, -7, 134.6) * millimeter });
    fCuboid(context, C1 + "o4", { "corner1" : vector(2, -9, 147.4) * millimeter, "corner2" : vector(28, -7, 150.6) * millimeter });
    fCuboid(context, C1 + "o5", { "corner1" : vector(2, -9, 161.4) * millimeter, "corner2" : vector(28, -7, 164.6) * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "dwHalterLo_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Dev-Wand Rückenhalter OCS lo (Abstand 14, Kabelbinder)", color(0.78, 0.8, 0.82));
}

function dw_dwHalterT(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "dwHalterT_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-20, -4, 44) * millimeter, "corner2" : vector(20, 0, 68) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-9, -30, 44) * millimeter, "corner2" : vector(9, -3.99, 48) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(-9, -30, 64) * millimeter, "corner2" : vector(9, -3.99, 68) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "dwHalterT_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "dwHalterT_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-15, -5, 56) * millimeter, "topCenter" : vector(-15, 1, 56) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(15, -5, 56) * millimeter, "topCenter" : vector(15, 1, 56) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(0, -22, 43) * millimeter, "topCenter" : vector(0, -22, 49) * millimeter, "radius" : 4.2 * millimeter });
    fCuboid(context, C1 + "o3", { "corner1" : vector(-4.2, -31, 43) * millimeter, "corner2" : vector(4.2, -22, 49) * millimeter });
    fCuboid(context, C1 + "o4", { "corner1" : vector(-8.1, -11, 43) * millimeter, "corner2" : vector(-4.9, -9, 69) * millimeter });
    fCuboid(context, C1 + "o5", { "corner1" : vector(4.9, -11, 43) * millimeter, "corner2" : vector(8.1, -9, 69) * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "dwHalterT_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Dev-Wand T-Halter (Gabel für den Abgang, Kabelbinder)", color(0.78, 0.8, 0.82));
}

function dw_dwQVL(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "dwQVL_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-117.5, -6, 156.35) * millimeter, "corner2" : vector(-28.5, 0, 239.65) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-108.5, -40, 226.5) * millimeter, "corner2" : vector(-37.5, -5.99, 236.5) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(-108.5, -40, 159.5) * millimeter, "corner2" : vector(-37.5, -5.99, 169.5) * millimeter });
    fCuboid(context, A0 + "s3", { "corner1" : vector(-99, -11, 236.65) * millimeter, "corner2" : vector(-47, -5.99, 239.65) * millimeter });
    fCuboid(context, A0 + "s4", { "corner1" : vector(-99, -11, 156.35) * millimeter, "corner2" : vector(-47, -5.99, 159.35) * millimeter });
    fCuboid(context, A0 + "s5", { "corner1" : vector(-108.5, -47.2, 156.35) * millimeter, "corner2" : vector(-37.5, -39.99, 239.65) * millimeter });
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(-73, -0.1, 198) * millimeter, "topCenter" : vector(-73, 11.6, 198) * millimeter, "radius" : 36.5 * millimeter });
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(-73, 11.5, 198) * millimeter, "topCenter" : vector(-73, 21, 198) * millimeter, "radius" : 12 * millimeter });
    fCuboid(context, A0 + "s8", { "corner1" : vector(-118, 24, 149.5) * millimeter, "corner2" : vector(-28, 28, 218) * millimeter });
    fCylinder(context, A0 + "s9", { "bottomCenter" : vector(-113, -0.1, 183) * millimeter, "topCenter" : vector(-113, 24.01, 183) * millimeter, "radius" : 4.5 * millimeter });
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(-113, -0.1, 213) * millimeter, "topCenter" : vector(-113, 24.01, 213) * millimeter, "radius" : 4.5 * millimeter });
    fCylinder(context, A0 + "s11", { "bottomCenter" : vector(-33, -0.1, 183) * millimeter, "topCenter" : vector(-33, 24.01, 183) * millimeter, "radius" : 4.5 * millimeter });
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(-33, -0.1, 213) * millimeter, "topCenter" : vector(-33, 24.01, 213) * millimeter, "radius" : 4.5 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "dwQVL_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "QV hi Rahmen v2 + Getriebe v1 (Dummy)", color(0.93, 0.55, 0.2));
}

function dw_dwNemaL(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "dwNemaL_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-94.15, 28, 150.85) * millimeter, "corner2" : vector(-51.85, 68, 193.15) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-81, 56, 140.85) * millimeter, "corner2" : vector(-65, 68, 150.95) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "dwNemaL_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "NEMA 17 QV hi (Dummy)", color(0.3, 0.3, 0.32));
}

function dw_dwQVR(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "dwQVR_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(28.5, -6, 156.35) * millimeter, "corner2" : vector(117.5, 0, 239.65) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(37.5, -40, 226.5) * millimeter, "corner2" : vector(108.5, -5.99, 236.5) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(37.5, -40, 159.5) * millimeter, "corner2" : vector(108.5, -5.99, 169.5) * millimeter });
    fCuboid(context, A0 + "s3", { "corner1" : vector(47, -11, 236.65) * millimeter, "corner2" : vector(99, -5.99, 239.65) * millimeter });
    fCuboid(context, A0 + "s4", { "corner1" : vector(47, -11, 156.35) * millimeter, "corner2" : vector(99, -5.99, 159.35) * millimeter });
    fCuboid(context, A0 + "s5", { "corner1" : vector(37.5, -47.2, 156.35) * millimeter, "corner2" : vector(108.5, -39.99, 239.65) * millimeter });
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(73, -0.1, 198) * millimeter, "topCenter" : vector(73, 11.6, 198) * millimeter, "radius" : 36.5 * millimeter });
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(73, 11.5, 198) * millimeter, "topCenter" : vector(73, 21, 198) * millimeter, "radius" : 12 * millimeter });
    fCuboid(context, A0 + "s8", { "corner1" : vector(28, 24, 149.5) * millimeter, "corner2" : vector(118, 28, 218) * millimeter });
    fCylinder(context, A0 + "s9", { "bottomCenter" : vector(33, -0.1, 183) * millimeter, "topCenter" : vector(33, 24.01, 183) * millimeter, "radius" : 4.5 * millimeter });
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(33, -0.1, 213) * millimeter, "topCenter" : vector(33, 24.01, 213) * millimeter, "radius" : 4.5 * millimeter });
    fCylinder(context, A0 + "s11", { "bottomCenter" : vector(113, -0.1, 183) * millimeter, "topCenter" : vector(113, 24.01, 183) * millimeter, "radius" : 4.5 * millimeter });
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(113, -0.1, 213) * millimeter, "topCenter" : vector(113, 24.01, 213) * millimeter, "radius" : 4.5 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "dwQVR_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "QV lo Rahmen v2 + Getriebe v1 (Dummy)", color(0.93, 0.55, 0.2));
}

function dw_dwNemaR(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "dwNemaR_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(51.85, 28, 150.85) * millimeter, "corner2" : vector(94.15, 68, 193.15) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(65, 56, 140.85) * millimeter, "corner2" : vector(81, 68, 150.95) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "dwNemaR_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "NEMA 17 QV lo (Dummy)", color(0.3, 0.3, 0.32));
}

function dw_dwOCSHi(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "dwOCSHi_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-24, -37, 84.75) * millimeter, "corner2" : vector(-1, -14, 197.25) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(-23.99, -22, 173.5) * millimeter, "topCenter" : vector(-31.5, -22, 173.5) * millimeter, "radius" : 3.2 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-23.99, -22, 105.2) * millimeter, "topCenter" : vector(-31.5, -22, 105.2) * millimeter, "radius" : 3.2 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "dwOCSHi_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "OCS hi (Dummy, Tüllen zum QV)", color(0.55, 0.75, 0.92));
}

function dw_dwOCSLo(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "dwOCSLo_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(1, -37, 116) * millimeter, "corner2" : vector(25, -14, 195) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(24.99, -22, 173.5) * millimeter, "topCenter" : vector(30, -22, 173.5) * millimeter, "radius" : 3.2 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(24.99, -22, 135) * millimeter, "topCenter" : vector(30, -22, 135) * millimeter, "radius" : 3.2 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "dwOCSLo_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "OCS lo (Dummy, Tüllen zum QV)", color(0.55, 0.75, 0.92));
}

function dw_dwOCSMix(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "dwOCSMix_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-11.5, -79.5, 6) * millimeter, "corner2" : vector(11.5, -0.5, 30) * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(0, -22, 29.99) * millimeter, "topCenter" : vector(0, -22, 35) * millimeter, "radius" : 3.2 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(0, -60.5, 29.99) * millimeter, "topCenter" : vector(0, -60.5, 35) * millimeter, "radius" : 3.2 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "dwOCSMix_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "OCS Mix (Dummy, quer zur Wand, Tüllen oben)", color(0.55, 0.75, 0.92));
}

function dw_dwTStueck(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "dwTStueck_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(-18, -22, 53) * millimeter, "topCenter" : vector(18, -22, 53) * millimeter, "radius" : 4 * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(-6, -22, 53) * millimeter, "topCenter" : vector(6, -22, 53) * millimeter, "radius" : 5 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(0, -22, 38) * millimeter, "topCenter" : vector(0, -22, 53) * millimeter, "radius" : 4 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "dwTStueck_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "T-Stück 6 mm (Dummy)", color(0.78, 0.8, 0.82));
}

function dw_dwSchlauchZuHi(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "dwSchlauchZuHi_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(-37, -22, 173.5) * millimeter, "topCenter" : vector(-31.5, -22, 173.5) * millimeter, "radius" : 6 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "dwSchlauchZuHi_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Schlauch 6/12 ZuHi (Dummy)", color(0.95, 0.95, 0.85));
}

function dw_dwSchlauchZuLo(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "dwSchlauchZuLo_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(30, -22, 173.5) * millimeter, "topCenter" : vector(37, -22, 173.5) * millimeter, "radius" : 6 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "dwSchlauchZuLo_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Schlauch 6/12 ZuLo (Dummy)", color(0.95, 0.95, 0.85));
}

function dw_dwSchlauchHiT(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "dwSchlauchHiT_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(-31.5, -22, 105.2) * millimeter, "topCenter" : vector(-34.1, -22, 105.2) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(-32.905, -22, 105.278) * millimeter, "topCenter" : vector(-40.85, -22, 104.232) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-39.701, -22, 104.54) * millimeter, "topCenter" : vector(-47.104, -22, 101.474) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(-46.074, -22, 102.069) * millimeter, "topCenter" : vector(-52.431, -22, 97.19) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(-51.59, -22, 98.031) * millimeter, "topCenter" : vector(-56.469, -22, 91.674) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(-55.874, -22, 92.704) * millimeter, "topCenter" : vector(-58.94, -22, 85.301) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(-58.632, -22, 86.45) * millimeter, "topCenter" : vector(-59.678, -22, 78.505) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(-59.678, -22, 79.695) * millimeter, "topCenter" : vector(-58.632, -22, 71.75) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(-58.94, -22, 72.899) * millimeter, "topCenter" : vector(-55.874, -22, 65.496) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s9", { "bottomCenter" : vector(-56.469, -22, 66.526) * millimeter, "topCenter" : vector(-51.59, -22, 60.169) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(-52.431, -22, 61.01) * millimeter, "topCenter" : vector(-46.074, -22, 56.131) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s11", { "bottomCenter" : vector(-47.104, -22, 56.726) * millimeter, "topCenter" : vector(-39.701, -22, 53.66) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(-40.85, -22, 53.968) * millimeter, "topCenter" : vector(-32.905, -22, 52.922) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s13", { "bottomCenter" : vector(-34.1, -22, 53) * millimeter, "topCenter" : vector(-18, -22, 53) * millimeter, "radius" : 6 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "dwSchlauchHiT_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Schlauch 6/12 HiT, R 26 (Dummy)", color(0.95, 0.95, 0.85));
}

function dw_dwSchlauchLoT(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "dwSchlauchLoT_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(30, -22, 135) * millimeter, "topCenter" : vector(32.6, -22, 135) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(31.403, -22, 135.059) * millimeter, "topCenter" : vector(37.474, -22, 134.461) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(36.303, -22, 134.694) * millimeter, "topCenter" : vector(42.141, -22, 132.923) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(41.038, -22, 133.38) * millimeter, "topCenter" : vector(46.418, -22, 130.504) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(45.425, -22, 131.167) * millimeter, "topCenter" : vector(50.141, -22, 127.297) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(49.297, -22, 128.141) * millimeter, "topCenter" : vector(53.167, -22, 123.425) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(52.504, -22, 124.418) * millimeter, "topCenter" : vector(55.38, -22, 119.038) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(54.923, -22, 120.141) * millimeter, "topCenter" : vector(56.694, -22, 114.303) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(56.461, -22, 115.474) * millimeter, "topCenter" : vector(57.059, -22, 109.403) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s9", { "bottomCenter" : vector(57, -22, 110.6) * millimeter, "topCenter" : vector(57, -22, 77.4) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(57.059, -22, 78.597) * millimeter, "topCenter" : vector(56.461, -22, 72.526) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s11", { "bottomCenter" : vector(56.694, -22, 73.697) * millimeter, "topCenter" : vector(54.923, -22, 67.859) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(55.38, -22, 68.962) * millimeter, "topCenter" : vector(52.504, -22, 63.582) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s13", { "bottomCenter" : vector(53.167, -22, 64.575) * millimeter, "topCenter" : vector(49.297, -22, 59.859) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(50.141, -22, 60.703) * millimeter, "topCenter" : vector(45.425, -22, 56.833) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s15", { "bottomCenter" : vector(46.418, -22, 57.496) * millimeter, "topCenter" : vector(41.038, -22, 54.62) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s16", { "bottomCenter" : vector(42.141, -22, 55.077) * millimeter, "topCenter" : vector(36.303, -22, 53.306) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s17", { "bottomCenter" : vector(37.474, -22, 53.539) * millimeter, "topCenter" : vector(31.403, -22, 52.941) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s18", { "bottomCenter" : vector(32.6, -22, 53) * millimeter, "topCenter" : vector(18, -22, 53) * millimeter, "radius" : 6 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "dwSchlauchLoT_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Schlauch 6/12 LoT, R 25 (Dummy)", color(0.95, 0.95, 0.85));
}

function dw_dwSchlauchTMix(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "dwSchlauchTMix_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(0, -22, 38) * millimeter, "topCenter" : vector(0, -22, 35) * millimeter, "radius" : 6 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "dwSchlauchTMix_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Schlauch 6/12 TMix (Dummy)", color(0.95, 0.95, 0.85));
}

function dw_dwSchlauchAbHi(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "dwSchlauchAbHi_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(-37, -22, 222.5) * millimeter, "topCenter" : vector(-31.956, -21.548, 222.5) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(-33.133, -21.76, 222.5) * millimeter, "topCenter" : vector(-27.671, -20.26, 222.5) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-28.791, -20.678, 222.5) * millimeter, "topCenter" : vector(-23.683, -18.23, 222.5) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(-24.711, -18.841, 222.5) * millimeter, "topCenter" : vector(-20.12, -15.523, 222.5) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(-21.022, -16.307, 222.5) * millimeter, "topCenter" : vector(-17.095, -12.226, 222.5) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(-17.844, -13.158, 222.5) * millimeter, "topCenter" : vector(-14.705, -8.443, 222.5) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(-15.275, -9.493, 222.5) * millimeter, "topCenter" : vector(-13.025, -4.296, 222.5) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(-13.4, -5.431, 222.5) * millimeter, "topCenter" : vector(-12.246, -0.5, 222.5) * millimeter, "radius" : 6 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "dwSchlauchAbHi_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Schlauch 6/12 AbHi, R 25 (Dummy)", color(0.95, 0.95, 0.85));
}

function dw_dwSchlauchAbLo(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "dwSchlauchAbLo_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(37, -22, 222.5) * millimeter, "topCenter" : vector(31.956, -21.548, 222.5) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(33.133, -21.76, 222.5) * millimeter, "topCenter" : vector(27.671, -20.26, 222.5) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(28.791, -20.678, 222.5) * millimeter, "topCenter" : vector(23.683, -18.23, 222.5) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(24.711, -18.841, 222.5) * millimeter, "topCenter" : vector(20.12, -15.523, 222.5) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(21.022, -16.307, 222.5) * millimeter, "topCenter" : vector(17.095, -12.226, 222.5) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(17.844, -13.158, 222.5) * millimeter, "topCenter" : vector(14.705, -8.443, 222.5) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(15.275, -9.493, 222.5) * millimeter, "topCenter" : vector(13.025, -4.296, 222.5) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(13.4, -5.431, 222.5) * millimeter, "topCenter" : vector(12.246, -0.5, 222.5) * millimeter, "radius" : 6 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "dwSchlauchAbLo_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Schlauch 6/12 AbLo, R 25 (Dummy)", color(0.95, 0.95, 0.85));
}

function dw_dwSchlauchPatient(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "dwSchlauchPatient_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(0, -60.5, 35) * millimeter, "topCenter" : vector(0, -60.5, 45.6) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(0, -60.441, 44.403) * millimeter, "topCenter" : vector(0, -61.039, 50.474) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(0, -60.806, 49.303) * millimeter, "topCenter" : vector(0, -62.577, 55.141) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(0, -62.12, 54.038) * millimeter, "topCenter" : vector(0, -64.996, 59.418) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(0, -64.333, 58.425) * millimeter, "topCenter" : vector(0, -68.203, 63.141) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(0, -67.359, 62.297) * millimeter, "topCenter" : vector(0, -72.075, 66.167) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(0, -71.082, 65.504) * millimeter, "topCenter" : vector(0, -76.462, 68.38) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(0, -75.359, 67.923) * millimeter, "topCenter" : vector(0, -81.197, 69.694) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(0, -80.026, 69.461) * millimeter, "topCenter" : vector(0, -86.097, 70.059) * millimeter, "radius" : 6 * millimeter });
    fCylinder(context, A0 + "s9", { "bottomCenter" : vector(0, -84.9, 70) * millimeter, "topCenter" : vector(0, -100.5, 70) * millimeter, "radius" : 6 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "dwSchlauchPatient_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Schlauch 6/12 Patient, R 25 (Dummy)", color(0.95, 0.95, 0.85));
}

function dw_dwPi(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "dwPi_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-97.5, 17, 21) * millimeter, "corner2" : vector(-12.5, 47, 91) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "dwPi_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Raspberry Pi (Dummy)", color(0.1, 0.45, 0.2));
}

function dw_dwPico(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "dwPico_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(10, 17, 24) * millimeter, "corner2" : vector(100, 29, 88) * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "dwPico_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "BTT SKR Pico (Dummy)", color(0.1, 0.45, 0.2));
}

annotation { "Feature Type Name" : "Development-Wand (Lochraster, 2 QV)" }
export const developmentWand = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
    }
    {
        const prog = new box("");
        var failed = [];
        var okCount = 0;
        try
        {
            dw_dwWand(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "dwWand @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dw_dwGrund(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "dwGrund @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dw_dwKnotenVL(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "dwKnotenVL @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dw_dwKnotenVR(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "dwKnotenVR @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dw_dwKnotenHL(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "dwKnotenHL @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dw_dwKnotenHR(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "dwKnotenHR @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dw_dwTraegerPi(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "dwTraegerPi @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dw_dwTraegerPico(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "dwTraegerPico @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dw_dwHalterHi(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "dwHalterHi @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dw_dwHalterLo(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "dwHalterLo @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dw_dwHalterT(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "dwHalterT @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dw_dwQVL(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "dwQVL @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dw_dwNemaL(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "dwNemaL @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dw_dwQVR(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "dwQVR @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dw_dwNemaR(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "dwNemaR @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dw_dwOCSHi(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "dwOCSHi @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dw_dwOCSLo(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "dwOCSLo @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dw_dwOCSMix(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "dwOCSMix @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dw_dwTStueck(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "dwTStueck @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dw_dwSchlauchZuHi(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "dwSchlauchZuHi @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dw_dwSchlauchZuLo(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "dwSchlauchZuLo @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dw_dwSchlauchHiT(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "dwSchlauchHiT @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dw_dwSchlauchLoT(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "dwSchlauchLoT @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dw_dwSchlauchTMix(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "dwSchlauchTMix @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dw_dwSchlauchAbHi(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "dwSchlauchAbHi @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dw_dwSchlauchAbLo(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "dwSchlauchAbLo @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dw_dwSchlauchPatient(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "dwSchlauchPatient @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dw_dwPi(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "dwPi @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            dw_dwPico(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "dwPico @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        if (size(failed) > 0)
        {
            var msg = "OK: " ~ toString(okCount) ~ " von 29 Teilen. Fehler: ";
            for (var s in failed)
                msg = msg ~ s ~ " || ";
            reportFeatureWarning(context, id, msg);
        }
    });

