FeatureScript 3112;
import(path : "onshape/std/geometry.fs", version : "3112.0");

// Gegenlager vorn fuer das Exzenter-Quetschventil (Rahmen v2 + Getriebe v1): Lager 6200 im neuen Steg, Lagerscheibe mit Zapfen 10.
// Erzeugt von build_gegenlager.py (ihht-pi CAD/quetschventil), nicht von Hand aendern.
// Im Einbau bei e 3,5 mit Platte, Ambossen, Welle, Huelse und Lager-Dummies; rechts daneben die Scheiben fuer e 4,5 und e 5,0.
// Neu zu drucken: Steg 6200 und die Lagerscheibe passend zur Welle. 6200 von vorn in den Steg druecken.

function gearPsi(rho is number, rb is number, half is number, pa is number) returns number
{
    const a = acos(min(1, rb / rho)) / radian;
    return half + (tan(pa * radian) - pa) - (tan(a * radian) - a);
}

function gearPts(n is number, ra is number, rf is number, thick is number, phase is number) returns array
{
    // Evolventenprofil Modul 1, Eingriffswinkel 20 Grad: n Zaehne, Kopf ra, Fuss rf, Zahndicke am Teilkreis thick, ein Zahn mittig bei phase
    const steps = 5;
    const r = n * 1 / 2;
    const pa = 20 * PI / 180;
    const rb = r * cos(pa * radian);
    const half = thick / (2 * r);
    const r0 = max(rb, rf);
    var pts = [];
    for (var t = 0; t < n; t += 1)
    {
        const c = phase + 2 * PI * t / n;
        var pol = [];
        if (rf < rb)
            pol = append(pol, [rf, c - gearPsi(rb, rb, half, pa)]);
        for (var k = 0; k <= steps; k += 1)
        {
            const rho = r0 + (ra - r0) * k / steps;
            pol = append(pol, [rho, c - gearPsi(rho, rb, half, pa)]);
        }
        pol = append(pol, [ra, c]);
        for (var k = steps; k >= 0; k -= 1)
        {
            const rho = r0 + (ra - r0) * k / steps;
            pol = append(pol, [rho, c + gearPsi(rho, rb, half, pa)]);
        }
        if (rf < rb)
            pol = append(pol, [rf, c + gearPsi(rb, rb, half, pa)]);
        pol = append(pol, [rf, c + PI / n]);
        for (var q in pol)
            pts = append(pts, [q[0] * cos(q[1] * radian), q[0] * sin(q[1] * radian)]);
    }
    return pts;
}

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

function prismZ(context is Context, id is Id, origin is Vector, pts is array, depth is number)
{
    // Polygon [dx, dy] in der XY-Ebene durch origin, Extrusion um depth in +Z
    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(origin, vector(0, 0, 1), vector(1, 0, 0)) });
    const n = size(pts);
    for (var i = 0; i < n; i += 1)
    {
        const a = pts[i];
        const b = pts[(i + 1) % n];
        skLineSegment(sk, "l" ~ i, { "start" : vector(a[0], a[1]) * millimeter, "end" : vector(b[0], b[1]) * millimeter });
    }
    skSolve(sk);
    opExtrude(context, id + "ext", { "entities" : qSketchRegion(id + "sk"), "direction" : vector(0, 0, 1), "endBound" : BoundingType.BLIND, "endDepth" : depth * millimeter });
    opDeleteBodies(context, id + "del", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
}

function qvg_platte(context is Context, id is Id, err is box)
{
    const A0 = id + "platte_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-44.5, -41.65, -6) * millimeter, "corner2" : vector(44.5, 41.65, 0) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-26, -41.65, -0.01) * millimeter, "corner2" : vector(26, -38.65, 5) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(-26, 38.65, -0.01) * millimeter, "corner2" : vector(26, 41.65, 5) * millimeter });
    try { unionAll(context, id + "platte_u0", qUnion([qCreatedBy(A0, EntityType.BODY)])); } catch (e) { err[] = err[] ~ "platte Vereinigung Stufe 0; "; }
    const C1 = id + "platte_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(0, 0, -7) * millimeter, "topCenter" : vector(0, 0, 1) * millimeter, "radius" : 11.2 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-15.5, -15.5, -7) * millimeter, "topCenter" : vector(-15.5, -15.5, 1) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(-15.5, -15.5, -3.2) * millimeter, "topCenter" : vector(-15.5, -15.5, 1) * millimeter, "radius" : 3.2 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(-40, -15, -7) * millimeter, "topCenter" : vector(-40, -15, 1) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(-15.5, 15.5, -7) * millimeter, "topCenter" : vector(-15.5, 15.5, 1) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(-15.5, 15.5, -3.2) * millimeter, "topCenter" : vector(-15.5, 15.5, 1) * millimeter, "radius" : 3.2 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(-40, 15, -7) * millimeter, "topCenter" : vector(-40, 15, 1) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o7", { "bottomCenter" : vector(15.5, -15.5, -7) * millimeter, "topCenter" : vector(15.5, -15.5, 1) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(15.5, -15.5, -3.2) * millimeter, "topCenter" : vector(15.5, -15.5, 1) * millimeter, "radius" : 3.2 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(40, -15, -7) * millimeter, "topCenter" : vector(40, -15, 1) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o10", { "bottomCenter" : vector(15.5, 15.5, -7) * millimeter, "topCenter" : vector(15.5, 15.5, 1) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o11", { "bottomCenter" : vector(15.5, 15.5, -3.2) * millimeter, "topCenter" : vector(15.5, 15.5, 1) * millimeter, "radius" : 3.2 * millimeter });
    fCylinder(context, C1 + "o12", { "bottomCenter" : vector(40, 15, -7) * millimeter, "topCenter" : vector(40, 15, 1) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o13", { "bottomCenter" : vector(-26, -33.5, -7) * millimeter, "topCenter" : vector(-26, -33.5, 1) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o14", { "bottomCenter" : vector(-26, -33.5, -7) * millimeter, "topCenter" : vector(-26, -33.5, -2.8) * millimeter, "radius" : 3.2 * millimeter });
    fCylinder(context, C1 + "o15", { "bottomCenter" : vector(-26, 33.5, -7) * millimeter, "topCenter" : vector(-26, 33.5, 1) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o16", { "bottomCenter" : vector(-26, 33.5, -7) * millimeter, "topCenter" : vector(-26, 33.5, -2.8) * millimeter, "radius" : 3.2 * millimeter });
    fCylinder(context, C1 + "o17", { "bottomCenter" : vector(0, -33.5, -7) * millimeter, "topCenter" : vector(0, -33.5, 1) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o18", { "bottomCenter" : vector(0, -33.5, -7) * millimeter, "topCenter" : vector(0, -33.5, -2.8) * millimeter, "radius" : 3.2 * millimeter });
    fCylinder(context, C1 + "o19", { "bottomCenter" : vector(0, 33.5, -7) * millimeter, "topCenter" : vector(0, 33.5, 1) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o20", { "bottomCenter" : vector(0, 33.5, -7) * millimeter, "topCenter" : vector(0, 33.5, -2.8) * millimeter, "radius" : 3.2 * millimeter });
    fCylinder(context, C1 + "o21", { "bottomCenter" : vector(26, -33.5, -7) * millimeter, "topCenter" : vector(26, -33.5, 1) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o22", { "bottomCenter" : vector(26, -33.5, -7) * millimeter, "topCenter" : vector(26, -33.5, -2.8) * millimeter, "radius" : 3.2 * millimeter });
    fCylinder(context, C1 + "o23", { "bottomCenter" : vector(26, 33.5, -7) * millimeter, "topCenter" : vector(26, 33.5, 1) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o24", { "bottomCenter" : vector(26, 33.5, -7) * millimeter, "topCenter" : vector(26, 33.5, -2.8) * millimeter, "radius" : 3.2 * millimeter });
    try { cutAll(context, id + "platte_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY)); } catch (e) { err[] = err[] ~ "platte Abzug Stufe 1; "; }
    setProperty(context, { "entities" : qBodyType(qUnion([qCreatedBy(A0, EntityType.BODY)]), BodyType.SOLID), "propertyType" : PropertyType.NAME, "value" : "QV-Rahmen v2 Bodenplatte (Anschlagleisten, 6 x M3 von hinten in die Ambosse)" });
}

function qvg_amboss_o(context is Context, id is Id, err is box)
{
    const A0 = id + "amboss_o_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-35.5, 28.5, 0) * millimeter, "corner2" : vector(35.5, 38.5, 34) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-35.5, 24.5, 5.1814) * millimeter, "corner2" : vector(-25.5, 28.51, 7.1814) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(-35.5, 24.5, 22.8186) * millimeter, "corner2" : vector(-25.5, 28.51, 24.8186) * millimeter });
    fCuboid(context, A0 + "s3", { "corner1" : vector(25.5, 24.5, 5.1814) * millimeter, "corner2" : vector(35.5, 28.51, 7.1814) * millimeter });
    fCuboid(context, A0 + "s4", { "corner1" : vector(25.5, 24.5, 22.8186) * millimeter, "corner2" : vector(35.5, 28.51, 24.8186) * millimeter });
    try { unionAll(context, id + "amboss_o_u0", qUnion([qCreatedBy(A0, EntityType.BODY)])); } catch (e) { err[] = err[] ~ "amboss_o Vereinigung Stufe 0; "; }
    const C1 = id + "amboss_o_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-26, 33.5, -1) * millimeter, "topCenter" : vector(-26, 33.5, 12) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(0, 33.5, -1) * millimeter, "topCenter" : vector(0, 33.5, 12) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(26, 33.5, -1) * millimeter, "topCenter" : vector(26, 33.5, 12) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(-26, 33.5, 22) * millimeter, "topCenter" : vector(-26, 33.5, 35) * millimeter, "radius" : 1.3 * millimeter });
    fCuboid(context, C1 + "o4", { "corner1" : vector(-34, 27.5, 2.8814) * millimeter, "corner2" : vector(-30, 39.5, 4.6814) * millimeter });
    fCuboid(context, C1 + "o5", { "corner1" : vector(-34, 27.5, 25.3186) * millimeter, "corner2" : vector(-30, 39.5, 27.1186) * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(26, 33.5, 22) * millimeter, "topCenter" : vector(26, 33.5, 35) * millimeter, "radius" : 1.3 * millimeter });
    fCuboid(context, C1 + "o7", { "corner1" : vector(30, 27.5, 2.8814) * millimeter, "corner2" : vector(34, 39.5, 4.6814) * millimeter });
    fCuboid(context, C1 + "o8", { "corner1" : vector(30, 27.5, 25.3186) * millimeter, "corner2" : vector(34, 39.5, 27.1186) * millimeter });
    try { cutAll(context, id + "amboss_o_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY)); } catch (e) { err[] = err[] ~ "amboss_o Abzug Stufe 1; "; }
    setProperty(context, { "entities" : qBodyType(qUnion([qCreatedBy(A0, EntityType.BODY)]), BodyType.SOLID), "propertyType" : PropertyType.NAME, "value" : "QV-Rahmen v2 Amboss oben (10 dick)" });
}

function qvg_amboss_u(context is Context, id is Id, err is box)
{
    const A0 = id + "amboss_u_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-35.5, -38.5, 0) * millimeter, "corner2" : vector(35.5, -28.5, 34) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-35.5, -28.51, 5.1814) * millimeter, "corner2" : vector(-25.5, -24.5, 7.1814) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(-35.5, -28.51, 22.8186) * millimeter, "corner2" : vector(-25.5, -24.5, 24.8186) * millimeter });
    fCuboid(context, A0 + "s3", { "corner1" : vector(25.5, -28.51, 5.1814) * millimeter, "corner2" : vector(35.5, -24.5, 7.1814) * millimeter });
    fCuboid(context, A0 + "s4", { "corner1" : vector(25.5, -28.51, 22.8186) * millimeter, "corner2" : vector(35.5, -24.5, 24.8186) * millimeter });
    try { unionAll(context, id + "amboss_u_u0", qUnion([qCreatedBy(A0, EntityType.BODY)])); } catch (e) { err[] = err[] ~ "amboss_u Vereinigung Stufe 0; "; }
    const C1 = id + "amboss_u_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(-26, -33.5, -1) * millimeter, "topCenter" : vector(-26, -33.5, 12) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(0, -33.5, -1) * millimeter, "topCenter" : vector(0, -33.5, 12) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(26, -33.5, -1) * millimeter, "topCenter" : vector(26, -33.5, 12) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(-26, -33.5, 22) * millimeter, "topCenter" : vector(-26, -33.5, 35) * millimeter, "radius" : 1.3 * millimeter });
    fCuboid(context, C1 + "o4", { "corner1" : vector(-34, -39.5, 2.8814) * millimeter, "corner2" : vector(-30, -27.5, 4.6814) * millimeter });
    fCuboid(context, C1 + "o5", { "corner1" : vector(-34, -39.5, 25.3186) * millimeter, "corner2" : vector(-30, -27.5, 27.1186) * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(26, -33.5, 22) * millimeter, "topCenter" : vector(26, -33.5, 35) * millimeter, "radius" : 1.3 * millimeter });
    fCuboid(context, C1 + "o7", { "corner1" : vector(30, -39.5, 2.8814) * millimeter, "corner2" : vector(34, -27.5, 4.6814) * millimeter });
    fCuboid(context, C1 + "o8", { "corner1" : vector(30, -39.5, 25.3186) * millimeter, "corner2" : vector(34, -27.5, 27.1186) * millimeter });
    try { cutAll(context, id + "amboss_u_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY)); } catch (e) { err[] = err[] ~ "amboss_u Abzug Stufe 1; "; }
    setProperty(context, { "entities" : qBodyType(qUnion([qCreatedBy(A0, EntityType.BODY)]), BodyType.SOLID), "propertyType" : PropertyType.NAME, "value" : "QV-Rahmen v2 Amboss unten (gleiches Teil, 180° gedreht)" });
}

function qvg_steg6200(context is Context, id is Id, err is box)
{
    const A0 = id + "steg6200_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-35.5, -41.65, 34) * millimeter, "corner2" : vector(35.5, 41.65, 41.2) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-26, -41.65, 29) * millimeter, "corner2" : vector(26, -38.65, 34.01) * millimeter });
    fCuboid(context, A0 + "s2", { "corner1" : vector(-26, 38.65, 29) * millimeter, "corner2" : vector(26, 41.65, 34.01) * millimeter });
    try { unionAll(context, id + "steg6200_u0", qUnion([qCreatedBy(A0, EntityType.BODY)])); } catch (e) { err[] = err[] ~ "steg6200 Vereinigung Stufe 0; "; }
    const C1 = id + "steg6200_c1";
    fCuboid(context, C1 + "o0", { "corner1" : vector(-24.5, -28, 33) * millimeter, "corner2" : vector(24.5, 28, 42.2) * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(-26, -33.5, 33) * millimeter, "topCenter" : vector(-26, -33.5, 42.2) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o2", { "bottomCenter" : vector(-26, 33.5, 33) * millimeter, "topCenter" : vector(-26, 33.5, 42.2) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(26, -33.5, 33) * millimeter, "topCenter" : vector(26, -33.5, 42.2) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(26, 33.5, 33) * millimeter, "topCenter" : vector(26, 33.5, 42.2) * millimeter, "radius" : 1.7 * millimeter });
    try { cutAll(context, id + "steg6200_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY)); } catch (e) { err[] = err[] ~ "steg6200 Abzug Stufe 1; "; }
    const A2 = id + "steg6200_a2";
    fCuboid(context, A2 + "s0", { "corner1" : vector(-9, -28.1, 34) * millimeter, "corner2" : vector(9, 28.1, 41.2) * millimeter });
    fCylinder(context, A2 + "s1", { "bottomCenter" : vector(0, 0, 31) * millimeter, "topCenter" : vector(0, 0, 41.2) * millimeter, "radius" : 18 * millimeter });
    try { unionAll(context, id + "steg6200_u2", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)])); } catch (e) { err[] = err[] ~ "steg6200 Vereinigung Stufe 2; "; }
    const C3 = id + "steg6200_c3";
    fCylinder(context, C3 + "o0", { "bottomCenter" : vector(0, 0, 32.2) * millimeter, "topCenter" : vector(0, 0, 42.2) * millimeter, "radius" : 15.15 * millimeter });
    fCylinder(context, C3 + "o1", { "bottomCenter" : vector(0, 0, 30) * millimeter, "topCenter" : vector(0, 0, 32.21) * millimeter, "radius" : 12.25 * millimeter });
    fCone(context, C3 + "o2", { "bottomCenter" : vector(0, 0, 40.6) * millimeter, "topCenter" : vector(0, 0, 41.21) * millimeter, "bottomRadius" : 15.15 * millimeter, "topRadius" : 15.75 * millimeter });
    try { cutAll(context, id + "steg6200_x3", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]), qCreatedBy(C3, EntityType.BODY)); } catch (e) { err[] = err[] ~ "steg6200 Abzug Stufe 3; "; }
    const A4 = id + "steg6200_a4";
    prismZ(context, A4 + "s0", vector(0, 0, 32.2) * millimeter, [[14.9, -0.6], [15.2, -0.6], [15.2, 0.6], [14.9, 0.6]], 8.2);
    prismZ(context, A4 + "s1", vector(0, 0, 32.2) * millimeter, [[7.9696, 12.6038], [8.1196, 12.8636], [7.0804, 13.4636], [6.9304, 13.2038]], 8.2);
    prismZ(context, A4 + "s2", vector(0, 0, 32.2) * millimeter, [[-6.9304, 13.2038], [-7.0804, 13.4636], [-8.1196, 12.8636], [-7.9696, 12.6038]], 8.2);
    prismZ(context, A4 + "s3", vector(0, 0, 32.2) * millimeter, [[-14.9, 0.6], [-15.2, 0.6], [-15.2, -0.6], [-14.9, -0.6]], 8.2);
    prismZ(context, A4 + "s4", vector(0, 0, 32.2) * millimeter, [[-7.9696, -12.6038], [-8.1196, -12.8636], [-7.0804, -13.4636], [-6.9304, -13.2038]], 8.2);
    prismZ(context, A4 + "s5", vector(0, 0, 32.2) * millimeter, [[6.9304, -13.2038], [7.0804, -13.4636], [8.1196, -12.8636], [7.9696, -12.6038]], 8.2);
    try { unionAll(context, id + "steg6200_u4", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY), qCreatedBy(A4, EntityType.BODY)])); } catch (e) { err[] = err[] ~ "steg6200 Vereinigung Stufe 4; "; }
    setProperty(context, { "entities" : qBodyType(qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY), qCreatedBy(A4, EntityType.BODY)]), BodyType.SOLID), "propertyType" : PropertyType.NAME, "value" : "QV-Rahmen v2 Steg vorn mit Lager 6200 (7,2 dick, Mittelsteg)" });
}

function qvg_welle(context is Context, id is Id, err is box)
{
    const A0 = id + "welle_a0";
    fCone(context, A0 + "s0", { "bottomCenter" : vector(0, 0, -33.8) * millimeter, "topCenter" : vector(0, 0, -33.4) * millimeter, "bottomRadius" : 6.6 * millimeter, "topRadius" : 7 * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(0, 0, -33.41) * millimeter, "topCenter" : vector(0, 0, -18.8) * millimeter, "radius" : 7 * millimeter });
    fCone(context, A0 + "s2", { "bottomCenter" : vector(0, 0, -18.9) * millimeter, "topCenter" : vector(0, 0, -15) * millimeter, "bottomRadius" : 7 * millimeter, "topRadius" : 10.9 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(0, 0, -15.01) * millimeter, "topCenter" : vector(0, 0, 3) * millimeter, "radius" : 10.9 * millimeter });
    fCone(context, A0 + "s4", { "bottomCenter" : vector(3.5, 0, 1.6) * millimeter, "topCenter" : vector(3.5, 0, 3) * millimeter, "bottomRadius" : 7.05 * millimeter, "topRadius" : 8.45 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(3.5, 0, 2.99) * millimeter, "topCenter" : vector(3.5, 0, 26.3) * millimeter, "radius" : 8.45 * millimeter });
    fCone(context, A0 + "s6", { "bottomCenter" : vector(3.5, 0, 26.29) * millimeter, "topCenter" : vector(3.5, 0, 26.8) * millimeter, "bottomRadius" : 8.45 * millimeter, "topRadius" : 7.95 * millimeter });
    fCylinder(context, A0 + "s7", { "bottomCenter" : vector(3.5, 5.5, 26) * millimeter, "topCenter" : vector(3.5, 5.5, 29) * millimeter, "radius" : 1.4 * millimeter });
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(3.5, -5.5, 26) * millimeter, "topCenter" : vector(3.5, -5.5, 29) * millimeter, "radius" : 1.4 * millimeter });
    try { unionAll(context, id + "welle_u0", qUnion([qCreatedBy(A0, EntityType.BODY)])); } catch (e) { err[] = err[] ~ "welle Vereinigung Stufe 0; "; }
    const C1 = id + "welle_c1";
    fCuboid(context, C1 + "o0", { "corner1" : vector(8.5, -14, -19.5) * millimeter, "corner2" : vector(14, 14, -6.6) * millimeter });
    fCuboid(context, C1 + "o1", { "corner1" : vector(-14, -14, -19.5) * millimeter, "corner2" : vector(-8.5, 14, -6.6) * millimeter });
    prismZ(context, C1 + "o2", vector(0, 0, 2.4) * millimeter, [[6.7857, 10.4978], [5.6232, 11.1638], [4.3961, 11.7015], [3.1185, 12.1048], [1.805, 12.369], [0.4709, 12.4911], [-0.8687, 12.4698], [-2.1983, 12.3052], [-3.5027, 11.9992], [-4.7668, 11.5554], [-5.9761, 10.9789], [-7.1168, 10.2762], [-8.1758, 9.4555], [-9.1408, 8.5262], [-10.0008, 7.4989], [-10.746, 6.3855], [-11.3677, 5.1987], [-11.8588, 3.9522], [-12.2136, 2.6603], [-12.4282, 1.3378], [-12.5, 0], [-12.4282, -1.3378], [-12.2136, -2.6603], [-11.8588, -3.9522], [-11.3677, -5.1987], [-10.746, -6.3855], [-10.0008, -7.4989], [-9.1408, -8.5262], [-8.1758, -9.4555], [-7.1168, -10.2762], [-5.9761, -10.9789], [-4.7668, -11.5554], [-3.5027, -11.9992], [-2.1983, -12.3052], [-0.8687, -12.4698], [0.4709, -12.4911], [1.805, -12.369], [3.1185, -12.1048], [4.3961, -11.7015], [5.6232, -11.1638], [6.7857, -10.4978], [5.789, -10.7592], [4.7723, -10.9262], [3.7443, -10.9973], [2.7142, -10.9719], [1.691, -10.8502], [0.6837, -10.6334], [-0.2989, -10.3232], [-1.2481, -9.9225], [-2.1557, -9.4346], [-3.0137, -8.8641], [-3.8145, -8.2157], [-4.5512, -7.4952], [-5.2172, -6.709], [-5.8067, -5.8639], [-6.3146, -4.9673], [-6.7363, -4.0272], [-7.0682, -3.0517], [-7.3074, -2.0495], [-7.4517, -1.0293], [-7.5, 0], [-7.4517, 1.0293], [-7.3074, 2.0495], [-7.0682, 3.0517], [-6.7363, 4.0272], [-6.3146, 4.9673], [-5.8067, 5.8639], [-5.2172, 6.709], [-4.5512, 7.4952], [-3.8145, 8.2157], [-3.0137, 8.8641], [-2.1557, 9.4346], [-1.2481, 9.9225], [-0.2989, 10.3232], [0.6837, 10.6334], [1.691, 10.8502], [2.7142, 10.9719], [3.7443, 10.9973], [4.7723, 10.9262], [5.789, 10.7592]], 0.61);
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(3.5, 0, 16) * millimeter, "topCenter" : vector(3.5, 0, 28) * millimeter, "radius" : 1.3 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(0, 3.8, -23) * millimeter, "topCenter" : vector(0, 8, -23) * millimeter, "radius" : 1.6 * millimeter });
    try { cutAll(context, id + "welle_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY)); } catch (e) { err[] = err[] ~ "welle Abzug Stufe 1; "; }
    setProperty(context, { "entities" : qBodyType(qUnion([qCreatedBy(A0, EntityType.BODY)]), BodyType.SOLID), "propertyType" : PropertyType.NAME, "value" : "QV-Getriebe Welle (Zapfen 14, Zweiflach 17, Lagersitz 21,8, Exzenter e 3,5)" });
}

function qvg_scheibe_e35(context is Context, id is Id, err is box)
{
    const A0 = id + "scheibe_e35_a0";
    prismZ(context, A0 + "s0", vector(0, 0, 27) * millimeter, [[3.5, -11], [2.5413, -10.9581], [1.5899, -10.8329], [0.653, -10.6252], [-0.2622, -10.3366], [-1.1488, -9.9694], [-2, -9.5263], [-2.8093, -9.0107], [-3.5707, -8.4265], [-4.2782, -7.7782], [-4.9265, -7.0707], [-5.5107, -6.3093], [-6.0263, -5.5], [-6.4694, -4.6488], [-6.8366, -3.7622], [-7.1252, -2.847], [-7.3329, -1.9101], [-7.4581, -0.9587], [-7.5, 0], [-7.4581, 0.9587], [-7.3329, 1.9101], [-7.1252, 2.847], [-6.8366, 3.7622], [-6.4694, 4.6488], [-6.0263, 5.5], [-5.5107, 6.3093], [-4.9265, 7.0707], [-4.2782, 7.7782], [-3.5707, 8.4265], [-2.8093, 9.0107], [-2, 9.5263], [-1.1488, 9.9694], [-0.2622, 10.3366], [0.653, 10.6252], [1.5899, 10.8329], [2.5413, 10.9581], [3.5, 11], [4.4587, 10.9581], [5.4101, 10.8329], [6.347, 10.6252], [7.2622, 10.3366], [8.1488, 9.9694], [9, 9.5263], [9.8093, 9.0107], [10.5707, 8.4265], [11.2782, 7.7782], [11.9265, 7.0707], [12.5107, 6.3093], [13.0263, 5.5], [13.4694, 4.6488], [13.8366, 3.7622], [14.1252, 2.847], [14.3329, 1.9101], [14.4581, 0.9587], [14.5, 0], [14.4581, -0.9587], [14.3329, -1.9101], [14.1252, -2.847], [13.8366, -3.7622], [13.4694, -4.6488], [13.0263, -5.5], [12.5107, -6.3093], [11.9265, -7.0707], [11.2782, -7.7782], [10.5707, -8.4265], [9.8093, -9.0107], [9, -9.5263], [8.1488, -9.9694], [7.2622, -10.3366], [6.347, -10.6252], [5.4101, -10.8329], [4.4587, -10.9581]], 3.3);
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(0, 0, 30.29) * millimeter, "topCenter" : vector(0, 0, 32) * millimeter, "radius" : 6.75 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(0, 0, 31.99) * millimeter, "topCenter" : vector(0, 0, 40.3) * millimeter, "radius" : 4.975 * millimeter });
    fCone(context, A0 + "s3", { "bottomCenter" : vector(0, 0, 40.29) * millimeter, "topCenter" : vector(0, 0, 40.8) * millimeter, "bottomRadius" : 4.975 * millimeter, "topRadius" : 4.475 * millimeter });
    prismZ(context, A0 + "s4", vector(0, 0, 32.6) * millimeter, [[0.4, 4.875], [0.4, 5.095], [-0.4, 5.095], [-0.4, 4.875]], 7.4);
    prismZ(context, A0 + "s5", vector(0, 0, 32.6) * millimeter, [[-4.4219, -2.0911], [-4.6124, -2.2011], [-4.2124, -2.8939], [-4.0219, -2.7839]], 7.4);
    prismZ(context, A0 + "s6", vector(0, 0, 32.6) * millimeter, [[4.0219, -2.7839], [4.2124, -2.8939], [4.6124, -2.2011], [4.4219, -2.0911]], 7.4);
    try { unionAll(context, id + "scheibe_e35_u0", qUnion([qCreatedBy(A0, EntityType.BODY)])); } catch (e) { err[] = err[] ~ "scheibe_e35 Vereinigung Stufe 0; "; }
    const C1 = id + "scheibe_e35_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(3.5, 5.5, 26) * millimeter, "topCenter" : vector(3.5, 5.5, 29.4) * millimeter, "radius" : 1.65 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(3.5, -5.5, 26) * millimeter, "topCenter" : vector(3.5, -5.5, 29.4) * millimeter, "radius" : 1.65 * millimeter });
    fCone(context, C1 + "o2", { "bottomCenter" : vector(3.5, 5.5, 26.99) * millimeter, "topCenter" : vector(3.5, 5.5, 27.5) * millimeter, "bottomRadius" : 2.15 * millimeter, "topRadius" : 1.65 * millimeter });
    fCone(context, C1 + "o3", { "bottomCenter" : vector(3.5, -5.5, 26.99) * millimeter, "topCenter" : vector(3.5, -5.5, 27.5) * millimeter, "bottomRadius" : 2.15 * millimeter, "topRadius" : 1.65 * millimeter });
    try { cutAll(context, id + "scheibe_e35_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY)); } catch (e) { err[] = err[] ~ "scheibe_e35 Abzug Stufe 1; "; }
    setProperty(context, { "entities" : qBodyType(qUnion([qCreatedBy(A0, EntityType.BODY)]), BodyType.SOLID), "propertyType" : PropertyType.NAME, "value" : "QV Lagerscheibe e 3,5 (Zapfen 10 fuer 6200, ersetzt Klemmscheibe)" });
}

function qvg_huelse(context is Context, id is Id, err is box)
{
    const A0 = id + "huelse_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(3.5, 0, 3) * millimeter, "topCenter" : vector(3.5, 0, 28) * millimeter, "radius" : 22.7 * millimeter });
    try { unionAll(context, id + "huelse_u0", qUnion([qCreatedBy(A0, EntityType.BODY)])); } catch (e) { err[] = err[] ~ "huelse Vereinigung Stufe 0; "; }
    const C1 = id + "huelse_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(3.5, 0, 2) * millimeter, "topCenter" : vector(3.5, 0, 27) * millimeter, "radius" : 20.2 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(3.5, 0, 26) * millimeter, "topCenter" : vector(3.5, 0, 29) * millimeter, "radius" : 17.5 * millimeter });
    try { cutAll(context, id + "huelse_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY)); } catch (e) { err[] = err[] ~ "huelse Abzug Stufe 1; "; }
    setProperty(context, { "entities" : qBodyType(qUnion([qCreatedBy(A0, EntityType.BODY)]), BodyType.SOLID), "propertyType" : PropertyType.NAME, "value" : "QV Lagerhuelse weit (vorhanden)" });
}

function qvg_l6203a(context is Context, id is Id, err is box)
{
    const A0 = id + "l6203a_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(3.5, 0, 3) * millimeter, "topCenter" : vector(3.5, 0, 15) * millimeter, "radius" : 20 * millimeter });
    try { unionAll(context, id + "l6203a_u0", qUnion([qCreatedBy(A0, EntityType.BODY)])); } catch (e) { err[] = err[] ~ "l6203a Vereinigung Stufe 0; "; }
    const C1 = id + "l6203a_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(3.5, 0, 2) * millimeter, "topCenter" : vector(3.5, 0, 16) * millimeter, "radius" : 8.5 * millimeter });
    try { cutAll(context, id + "l6203a_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY)); } catch (e) { err[] = err[] ~ "l6203a Abzug Stufe 1; "; }
    setProperty(context, { "entities" : qBodyType(qUnion([qCreatedBy(A0, EntityType.BODY)]), BodyType.SOLID), "propertyType" : PropertyType.NAME, "value" : "6203 (Dummy)" });
}

function qvg_l6203b(context is Context, id is Id, err is box)
{
    const A0 = id + "l6203b_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(3.5, 0, 15) * millimeter, "topCenter" : vector(3.5, 0, 27) * millimeter, "radius" : 20 * millimeter });
    try { unionAll(context, id + "l6203b_u0", qUnion([qCreatedBy(A0, EntityType.BODY)])); } catch (e) { err[] = err[] ~ "l6203b Vereinigung Stufe 0; "; }
    const C1 = id + "l6203b_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(3.5, 0, 14) * millimeter, "topCenter" : vector(3.5, 0, 28) * millimeter, "radius" : 8.5 * millimeter });
    try { cutAll(context, id + "l6203b_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY)); } catch (e) { err[] = err[] ~ "l6203b Abzug Stufe 1; "; }
    setProperty(context, { "entities" : qBodyType(qUnion([qCreatedBy(A0, EntityType.BODY)]), BodyType.SOLID), "propertyType" : PropertyType.NAME, "value" : "6203 (Dummy)" });
}

function qvg_l6200(context is Context, id is Id, err is box)
{
    const A0 = id + "l6200_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(0, 0, 32.2) * millimeter, "topCenter" : vector(0, 0, 41.2) * millimeter, "radius" : 15 * millimeter });
    try { unionAll(context, id + "l6200_u0", qUnion([qCreatedBy(A0, EntityType.BODY)])); } catch (e) { err[] = err[] ~ "l6200 Vereinigung Stufe 0; "; }
    const C1 = id + "l6200_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(0, 0, 31.2) * millimeter, "topCenter" : vector(0, 0, 42.2) * millimeter, "radius" : 5 * millimeter });
    try { cutAll(context, id + "l6200_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY)); } catch (e) { err[] = err[] ~ "l6200 Abzug Stufe 1; "; }
    setProperty(context, { "entities" : qBodyType(qUnion([qCreatedBy(A0, EntityType.BODY)]), BodyType.SOLID), "propertyType" : PropertyType.NAME, "value" : "6200 (Dummy)" });
}

function qvg_scheibe_e45(context is Context, id is Id, err is box)
{
    const A0 = id + "scheibe_e45_a0";
    prismZ(context, A0 + "s0", vector(70, 0, 27) * millimeter, [[4.5, -11], [3.5413, -10.9581], [2.5899, -10.8329], [1.653, -10.6252], [0.7378, -10.3366], [-0.1488, -9.9694], [-1, -9.5263], [-1.8093, -9.0107], [-2.5707, -8.4265], [-3.2782, -7.7782], [-3.9265, -7.0707], [-4.5107, -6.3093], [-5.0263, -5.5], [-5.4694, -4.6488], [-5.8366, -3.7622], [-6.3429, -2.3086], [-6.52, -1.747], [-6.6475, -1.1721], [-6.7243, -0.5883], [-6.75, 0], [-6.7243, 0.5883], [-6.6475, 1.1721], [-6.52, 1.747], [-6.3429, 2.3086], [-5.8366, 3.7622], [-5.4694, 4.6488], [-5.0263, 5.5], [-4.5107, 6.3093], [-3.9265, 7.0707], [-3.2782, 7.7782], [-2.5707, 8.4265], [-1.8093, 9.0107], [-1, 9.5263], [-0.1488, 9.9694], [0.7378, 10.3366], [1.653, 10.6252], [2.5899, 10.8329], [3.5413, 10.9581], [4.5, 11], [5.4587, 10.9581], [6.4101, 10.8329], [7.347, 10.6252], [8.2622, 10.3366], [9.1488, 9.9694], [10, 9.5263], [10.8093, 9.0107], [11.5707, 8.4265], [12.2782, 7.7782], [12.9265, 7.0707], [13.5107, 6.3093], [14.0263, 5.5], [14.4694, 4.6488], [14.8366, 3.7622], [15.1252, 2.847], [15.3329, 1.9101], [15.4581, 0.9587], [15.5, 0], [15.4581, -0.9587], [15.3329, -1.9101], [15.1252, -2.847], [14.8366, -3.7622], [14.4694, -4.6488], [14.0263, -5.5], [13.5107, -6.3093], [12.9265, -7.0707], [12.2782, -7.7782], [11.5707, -8.4265], [10.8093, -9.0107], [10, -9.5263], [9.1488, -9.9694], [8.2622, -10.3366], [7.347, -10.6252], [6.4101, -10.8329], [5.4587, -10.9581]], 3.3);
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(70, 0, 30.29) * millimeter, "topCenter" : vector(70, 0, 32) * millimeter, "radius" : 6.75 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(70, 0, 31.99) * millimeter, "topCenter" : vector(70, 0, 40.3) * millimeter, "radius" : 4.975 * millimeter });
    fCone(context, A0 + "s3", { "bottomCenter" : vector(70, 0, 40.29) * millimeter, "topCenter" : vector(70, 0, 40.8) * millimeter, "bottomRadius" : 4.975 * millimeter, "topRadius" : 4.475 * millimeter });
    prismZ(context, A0 + "s4", vector(70, 0, 32.6) * millimeter, [[0.4, 4.875], [0.4, 5.095], [-0.4, 5.095], [-0.4, 4.875]], 7.4);
    prismZ(context, A0 + "s5", vector(70, 0, 32.6) * millimeter, [[-4.4219, -2.0911], [-4.6124, -2.2011], [-4.2124, -2.8939], [-4.0219, -2.7839]], 7.4);
    prismZ(context, A0 + "s6", vector(70, 0, 32.6) * millimeter, [[4.0219, -2.7839], [4.2124, -2.8939], [4.6124, -2.2011], [4.4219, -2.0911]], 7.4);
    try { unionAll(context, id + "scheibe_e45_u0", qUnion([qCreatedBy(A0, EntityType.BODY)])); } catch (e) { err[] = err[] ~ "scheibe_e45 Vereinigung Stufe 0; "; }
    const C1 = id + "scheibe_e45_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(74.5, 5.5, 26) * millimeter, "topCenter" : vector(74.5, 5.5, 29.4) * millimeter, "radius" : 1.65 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(74.5, -5.5, 26) * millimeter, "topCenter" : vector(74.5, -5.5, 29.4) * millimeter, "radius" : 1.65 * millimeter });
    fCone(context, C1 + "o2", { "bottomCenter" : vector(74.5, 5.5, 26.99) * millimeter, "topCenter" : vector(74.5, 5.5, 27.5) * millimeter, "bottomRadius" : 2.15 * millimeter, "topRadius" : 1.65 * millimeter });
    fCone(context, C1 + "o3", { "bottomCenter" : vector(74.5, -5.5, 26.99) * millimeter, "topCenter" : vector(74.5, -5.5, 27.5) * millimeter, "bottomRadius" : 2.15 * millimeter, "topRadius" : 1.65 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(82.5, 0, 29.7) * millimeter, "topCenter" : vector(82.5, 0, 31.3) * millimeter, "radius" : 0.7 * millimeter });
    try { cutAll(context, id + "scheibe_e45_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY)); } catch (e) { err[] = err[] ~ "scheibe_e45 Abzug Stufe 1; "; }
    setProperty(context, { "entities" : qBodyType(qUnion([qCreatedBy(A0, EntityType.BODY)]), BodyType.SOLID), "propertyType" : PropertyType.NAME, "value" : "QV Lagerscheibe e 4,5 (Zapfen 10 fuer 6200, ersetzt Klemmscheibe)" });
}

function qvg_scheibe_e5(context is Context, id is Id, err is box)
{
    const A0 = id + "scheibe_e5_a0";
    prismZ(context, A0 + "s0", vector(110, 0, 27) * millimeter, [[5, -11], [4.0413, -10.9581], [3.0899, -10.8329], [2.153, -10.6252], [1.2378, -10.3366], [0.3512, -9.9694], [-0.5, -9.5263], [-1.3093, -9.0107], [-2.0707, -8.4265], [-2.7782, -7.7782], [-3.4265, -7.0707], [-4.0107, -6.3093], [-4.5263, -5.5], [-5.8457, -3.375], [-6.1176, -2.8527], [-6.3429, -2.3086], [-6.52, -1.747], [-6.6475, -1.1721], [-6.7243, -0.5883], [-6.75, 0], [-6.7243, 0.5883], [-6.6475, 1.1721], [-6.52, 1.747], [-6.3429, 2.3086], [-6.1176, 2.8527], [-5.8457, 3.375], [-4.5263, 5.5], [-4.0107, 6.3093], [-3.4265, 7.0707], [-2.7782, 7.7782], [-2.0707, 8.4265], [-1.3093, 9.0107], [-0.5, 9.5263], [0.3512, 9.9694], [1.2378, 10.3366], [2.153, 10.6252], [3.0899, 10.8329], [4.0413, 10.9581], [5, 11], [5.9587, 10.9581], [6.9101, 10.8329], [7.847, 10.6252], [8.7622, 10.3366], [9.6488, 9.9694], [10.5, 9.5263], [11.3093, 9.0107], [12.0707, 8.4265], [12.7782, 7.7782], [13.4265, 7.0707], [14.0107, 6.3093], [14.5263, 5.5], [14.9694, 4.6488], [15.3366, 3.7622], [15.6252, 2.847], [15.8329, 1.9101], [15.9581, 0.9587], [16, 0], [15.9581, -0.9587], [15.8329, -1.9101], [15.6252, -2.847], [15.3366, -3.7622], [14.9694, -4.6488], [14.5263, -5.5], [14.0107, -6.3093], [13.4265, -7.0707], [12.7782, -7.7782], [12.0707, -8.4265], [11.3093, -9.0107], [10.5, -9.5263], [9.6488, -9.9694], [8.7622, -10.3366], [7.847, -10.6252], [6.9101, -10.8329], [5.9587, -10.9581]], 3.3);
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(110, 0, 30.29) * millimeter, "topCenter" : vector(110, 0, 32) * millimeter, "radius" : 6.75 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(110, 0, 31.99) * millimeter, "topCenter" : vector(110, 0, 40.3) * millimeter, "radius" : 4.975 * millimeter });
    fCone(context, A0 + "s3", { "bottomCenter" : vector(110, 0, 40.29) * millimeter, "topCenter" : vector(110, 0, 40.8) * millimeter, "bottomRadius" : 4.975 * millimeter, "topRadius" : 4.475 * millimeter });
    prismZ(context, A0 + "s4", vector(110, 0, 32.6) * millimeter, [[0.4, 4.875], [0.4, 5.095], [-0.4, 5.095], [-0.4, 4.875]], 7.4);
    prismZ(context, A0 + "s5", vector(110, 0, 32.6) * millimeter, [[-4.4219, -2.0911], [-4.6124, -2.2011], [-4.2124, -2.8939], [-4.0219, -2.7839]], 7.4);
    prismZ(context, A0 + "s6", vector(110, 0, 32.6) * millimeter, [[4.0219, -2.7839], [4.2124, -2.8939], [4.6124, -2.2011], [4.4219, -2.0911]], 7.4);
    try { unionAll(context, id + "scheibe_e5_u0", qUnion([qCreatedBy(A0, EntityType.BODY)])); } catch (e) { err[] = err[] ~ "scheibe_e5 Vereinigung Stufe 0; "; }
    const C1 = id + "scheibe_e5_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(115, 5.5, 26) * millimeter, "topCenter" : vector(115, 5.5, 29.4) * millimeter, "radius" : 1.65 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(115, -5.5, 26) * millimeter, "topCenter" : vector(115, -5.5, 29.4) * millimeter, "radius" : 1.65 * millimeter });
    fCone(context, C1 + "o2", { "bottomCenter" : vector(115, 5.5, 26.99) * millimeter, "topCenter" : vector(115, 5.5, 27.5) * millimeter, "bottomRadius" : 2.15 * millimeter, "topRadius" : 1.65 * millimeter });
    fCone(context, C1 + "o3", { "bottomCenter" : vector(115, -5.5, 26.99) * millimeter, "topCenter" : vector(115, -5.5, 27.5) * millimeter, "bottomRadius" : 2.15 * millimeter, "topRadius" : 1.65 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(123, -1.5, 29.7) * millimeter, "topCenter" : vector(123, -1.5, 31.3) * millimeter, "radius" : 0.7 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(123, 1.5, 29.7) * millimeter, "topCenter" : vector(123, 1.5, 31.3) * millimeter, "radius" : 0.7 * millimeter });
    try { cutAll(context, id + "scheibe_e5_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY)); } catch (e) { err[] = err[] ~ "scheibe_e5 Abzug Stufe 1; "; }
    setProperty(context, { "entities" : qBodyType(qUnion([qCreatedBy(A0, EntityType.BODY)]), BodyType.SOLID), "propertyType" : PropertyType.NAME, "value" : "QV Lagerscheibe e 5 (Zapfen 10 fuer 6200, ersetzt Klemmscheibe)" });
}

annotation { "Feature Type Name" : "Quetschventil Gegenlager 6200" }
export const quetschventilGegenlager6200 = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
    }
    {
        var err = new box("");
        try { qvg_platte(context, id + "platte", err); } catch (e) { err[] = err[] ~ "platte abgebrochen; "; }
        try { qvg_amboss_o(context, id + "amboss_o", err); } catch (e) { err[] = err[] ~ "amboss_o abgebrochen; "; }
        try { qvg_amboss_u(context, id + "amboss_u", err); } catch (e) { err[] = err[] ~ "amboss_u abgebrochen; "; }
        try { qvg_steg6200(context, id + "steg6200", err); } catch (e) { err[] = err[] ~ "steg6200 abgebrochen; "; }
        try { qvg_welle(context, id + "welle", err); } catch (e) { err[] = err[] ~ "welle abgebrochen; "; }
        try { qvg_scheibe_e35(context, id + "scheibe_e35", err); } catch (e) { err[] = err[] ~ "scheibe_e35 abgebrochen; "; }
        try { qvg_huelse(context, id + "huelse", err); } catch (e) { err[] = err[] ~ "huelse abgebrochen; "; }
        try { qvg_l6203a(context, id + "l6203a", err); } catch (e) { err[] = err[] ~ "l6203a abgebrochen; "; }
        try { qvg_l6203b(context, id + "l6203b", err); } catch (e) { err[] = err[] ~ "l6203b abgebrochen; "; }
        try { qvg_l6200(context, id + "l6200", err); } catch (e) { err[] = err[] ~ "l6200 abgebrochen; "; }
        try { qvg_scheibe_e45(context, id + "scheibe_e45", err); } catch (e) { err[] = err[] ~ "scheibe_e45 abgebrochen; "; }
        try { qvg_scheibe_e5(context, id + "scheibe_e5", err); } catch (e) { err[] = err[] ~ "scheibe_e5 abgebrochen; "; }
        if (err[] != "")
            reportFeatureWarning(context, id, err[]);
    });
