FeatureScript 3112;
import(path : "onshape/std/geometry.fs", version : "3112.0");

// Gegenlager 6200 vorn im Steg fuer das Exzenter-Quetschventil (Rahmen v2 + Getriebe v1). Erzeugt von build_welle_zapfen.py
// (ihht-pi CAD/quetschventil), nicht von Hand aendern. Stand 25.11.2026: Welle mit angedrucktem Zapfen fuer das 6200 (ein Teil)
// und loser Distanzring zwischen 6203 und 6200, im Einbau bei e 3,5 mit Platte, Ambossen, Steg 6200, Huelse und Lager-Dummies.
// Reihenfolge: Welle von vorn durch den Rahmen, 2 x 6203 (+ Huelse) auf den Exzenter, Distanzring ueber den Zapfen, Steg mit 6200 drauf.

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
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(-26, 33.5, -1) * millimeter, "topCenter" : vector(-26, 33.5, 35) * millimeter, "radius" : 1.3 * millimeter });
    fCuboid(context, C1 + "o4", { "corner1" : vector(-34, 27.5, 2.8814) * millimeter, "corner2" : vector(-30, 39.5, 4.6814) * millimeter });
    fCuboid(context, C1 + "o5", { "corner1" : vector(-34, 27.5, 25.3186) * millimeter, "corner2" : vector(-30, 39.5, 27.1186) * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(26, 33.5, -1) * millimeter, "topCenter" : vector(26, 33.5, 35) * millimeter, "radius" : 1.3 * millimeter });
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
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(-26, -33.5, -1) * millimeter, "topCenter" : vector(-26, -33.5, 35) * millimeter, "radius" : 1.3 * millimeter });
    fCuboid(context, C1 + "o4", { "corner1" : vector(-34, -39.5, 2.8814) * millimeter, "corner2" : vector(-30, -27.5, 4.6814) * millimeter });
    fCuboid(context, C1 + "o5", { "corner1" : vector(-34, -39.5, 25.3186) * millimeter, "corner2" : vector(-30, -27.5, 27.1186) * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(26, -33.5, -1) * millimeter, "topCenter" : vector(26, -33.5, 35) * millimeter, "radius" : 1.3 * millimeter });
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
    try { cutAll(context, id + "steg6200_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY)); } catch (e) { err[] = err[] ~ "steg6200 Abzug Stufe 1; "; }
    const A2 = id + "steg6200_a2";
    fCuboid(context, A2 + "s0", { "corner1" : vector(-9, -28.1, 34) * millimeter, "corner2" : vector(9, 28.1, 41.2) * millimeter });
    fCylinder(context, A2 + "s1", { "bottomCenter" : vector(0, 0, 31) * millimeter, "topCenter" : vector(0, 0, 41.2) * millimeter, "radius" : 18 * millimeter });
    try { unionAll(context, id + "steg6200_u2", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)])); } catch (e) { err[] = err[] ~ "steg6200 Vereinigung Stufe 2; "; }
    const C3 = id + "steg6200_c3";
    fCylinder(context, C3 + "o0", { "bottomCenter" : vector(0, 0, 32.2) * millimeter, "topCenter" : vector(0, 0, 42.2) * millimeter, "radius" : 15.15 * millimeter });
    fCylinder(context, C3 + "o1", { "bottomCenter" : vector(0, 0, 30) * millimeter, "topCenter" : vector(0, 0, 32.21) * millimeter, "radius" : 12.25 * millimeter });
    fCone(context, C3 + "o2", { "bottomCenter" : vector(0, 0, 40.6) * millimeter, "topCenter" : vector(0, 0, 41.21) * millimeter, "bottomRadius" : 15.15 * millimeter, "topRadius" : 15.75 * millimeter });
    fCylinder(context, C3 + "o3", { "bottomCenter" : vector(-26, -33.5, 33) * millimeter, "topCenter" : vector(-26, -33.5, 42.2) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C3 + "o4", { "bottomCenter" : vector(-26, 33.5, 33) * millimeter, "topCenter" : vector(-26, 33.5, 42.2) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C3 + "o5", { "bottomCenter" : vector(26, -33.5, 33) * millimeter, "topCenter" : vector(26, -33.5, 42.2) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C3 + "o6", { "bottomCenter" : vector(26, 33.5, 33) * millimeter, "topCenter" : vector(26, 33.5, 42.2) * millimeter, "radius" : 1.7 * millimeter });
    try { cutAll(context, id + "steg6200_x3", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY)]), qCreatedBy(C3, EntityType.BODY)); } catch (e) { err[] = err[] ~ "steg6200 Abzug Stufe 3; "; }
    const A4 = id + "steg6200_a4";
    prismZ(context, A4 + "s0", vector(0, 0, 32.2) * millimeter, [[13.1538, 7.017], [13.5435, 7.242], [13.0435, 8.108], [12.6538, 7.883]], 8.2);
    prismZ(context, A4 + "s1", vector(0, 0, 32.2) * millimeter, [[0.5, 14.9], [0.5, 15.35], [-0.5, 15.35], [-0.5, 14.9]], 8.2);
    prismZ(context, A4 + "s2", vector(0, 0, 32.2) * millimeter, [[-12.6538, 7.883], [-13.0435, 8.108], [-13.5435, 7.242], [-13.1538, 7.017]], 8.2);
    prismZ(context, A4 + "s3", vector(0, 0, 32.2) * millimeter, [[-13.1538, -7.017], [-13.5435, -7.242], [-13.0435, -8.108], [-12.6538, -7.883]], 8.2);
    prismZ(context, A4 + "s4", vector(0, 0, 32.2) * millimeter, [[-0.5, -14.9], [-0.5, -15.35], [0.5, -15.35], [0.5, -14.9]], 8.2);
    prismZ(context, A4 + "s5", vector(0, 0, 32.2) * millimeter, [[12.6538, -7.883], [13.0435, -8.108], [13.5435, -7.242], [13.1538, -7.017]], 8.2);
    try { unionAll(context, id + "steg6200_u4", qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY), qCreatedBy(A4, EntityType.BODY)])); } catch (e) { err[] = err[] ~ "steg6200 Vereinigung Stufe 4; "; }
    setProperty(context, { "entities" : qBodyType(qUnion([qCreatedBy(A0, EntityType.BODY), qCreatedBy(A2, EntityType.BODY), qCreatedBy(A4, EntityType.BODY)]), BodyType.SOLID), "propertyType" : PropertyType.NAME, "value" : "QV-Rahmen v2 Steg vorn mit Gegenlager 6200 (7,2 dick)" });
}

function qvg_welle_zapfen(context is Context, id is Id, err is box)
{
    const A0 = id + "welle_zapfen_a0";
    fCone(context, A0 + "s0", { "bottomCenter" : vector(0, 0, -33.8) * millimeter, "topCenter" : vector(0, 0, -33.4) * millimeter, "bottomRadius" : 6.6 * millimeter, "topRadius" : 7 * millimeter });
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(0, 0, -33.41) * millimeter, "topCenter" : vector(0, 0, -18.8) * millimeter, "radius" : 7 * millimeter });
    fCone(context, A0 + "s2", { "bottomCenter" : vector(0, 0, -18.9) * millimeter, "topCenter" : vector(0, 0, -15) * millimeter, "bottomRadius" : 7 * millimeter, "topRadius" : 10.9 * millimeter });
    fCylinder(context, A0 + "s3", { "bottomCenter" : vector(0, 0, -15.01) * millimeter, "topCenter" : vector(0, 0, 3) * millimeter, "radius" : 10.9 * millimeter });
    fCone(context, A0 + "s4", { "bottomCenter" : vector(3.5, 0, 1.6) * millimeter, "topCenter" : vector(3.5, 0, 3) * millimeter, "bottomRadius" : 7.05 * millimeter, "topRadius" : 8.45 * millimeter });
    fCylinder(context, A0 + "s5", { "bottomCenter" : vector(3.5, 0, 2.99) * millimeter, "topCenter" : vector(3.5, 0, 26.8) * millimeter, "radius" : 8.45 * millimeter });
    prismZ(context, A0 + "s6", vector(0, 0, 26.79) * millimeter, [[-4.7942, -1.329], [-4.6013, -1.8917], [-4.3427, -2.4273], [-4.0219, -2.9283], [-3.6437, -3.3873], [-3.2134, -3.798], [-2.7371, -4.1544], [-2.2218, -4.4513], [-1.6746, -4.6847], [-1.1036, -4.8511], [-0.5167, -4.9481], [0.0775, -4.9744], [0.6706, -4.9296], [1.2542, -4.8143], [1.8198, -4.6302], [2.3594, -4.3799], [2.8652, -4.0671], [3.3302, -3.696], [3.7475, -3.2722], [4.1112, -2.8015], [4.4162, -2.2909], [4.658, -1.7474], [4.8333, -1.179], [4.9394, -0.5938], [4.975, 0], [4.9394, 0.5938], [4.8333, 1.179], [4.658, 1.7474], [4.4162, 2.2909], [4.1112, 2.8015], [3.7475, 3.2722], [3.3302, 3.696], [2.8652, 4.0671], [2.3594, 4.3799], [1.8198, 4.6302], [1.2542, 4.8143], [0.6706, 4.9296], [0.0775, 4.9744], [-0.5167, 4.9481], [-1.1036, 4.8511], [-1.6746, 4.6847], [-2.2218, 4.4513], [-2.7371, 4.1544], [-3.2134, 3.798], [-3.6437, 3.3873], [-4.0219, 2.9283], [-4.3427, 2.4273], [-4.6013, 1.8917], [-4.7942, 1.329], [-4.8111, 1.2191], [-4.8265, 1.1089], [-4.8404, 0.9986], [-4.8529, 0.8881], [-4.8639, 0.7774], [-4.8735, 0.6666], [-4.8816, 0.5557], [-4.8882, 0.4447], [-4.8934, 0.3336], [-4.8971, 0.2224], [-4.8993, 0.1112], [-4.9, 0], [-4.8993, -0.1112], [-4.8971, -0.2224], [-4.8934, -0.3336], [-4.8882, -0.4447], [-4.8816, -0.5557], [-4.8735, -0.6666], [-4.8639, -0.7774], [-4.8529, -0.8881], [-4.8404, -0.9986], [-4.8265, -1.1089], [-4.8111, -1.2191]], 14.11);
    prismZ(context, A0 + "s7", vector(0, 0, 33) * millimeter, [[4.775, -0.45], [5.095, -0.45], [5.095, 0.45], [4.775, 0.45]], 7.1);
    prismZ(context, A0 + "s8", vector(0, 0, 33) * millimeter, [[-0.8012, 4.7288], [-0.884, 5.0379], [-1.7533, 4.8049], [-1.6705, 4.4958]], 7.1);
    prismZ(context, A0 + "s9", vector(0, 0, 33) * millimeter, [[-1.6705, -4.4958], [-1.7533, -4.8049], [-0.884, -5.0379], [-0.8012, -4.7288]], 7.1);
    try { unionAll(context, id + "welle_zapfen_u0", qUnion([qCreatedBy(A0, EntityType.BODY)])); } catch (e) { err[] = err[] ~ "welle_zapfen Vereinigung Stufe 0; "; }
    const C1 = id + "welle_zapfen_c1";
    fCuboid(context, C1 + "o0", { "corner1" : vector(8.5, -14, -19.5) * millimeter, "corner2" : vector(14, 14, -6.6) * millimeter });
    fCuboid(context, C1 + "o1", { "corner1" : vector(-14, -14, -19.5) * millimeter, "corner2" : vector(-8.5, 14, -6.6) * millimeter });
    prismZ(context, C1 + "o2", vector(0, 0, 2.4) * millimeter, [[6.7857, 10.4978], [5.6232, 11.1638], [4.3961, 11.7015], [3.1185, 12.1048], [1.805, 12.369], [0.4709, 12.4911], [-0.8687, 12.4698], [-2.1983, 12.3052], [-3.5027, 11.9992], [-4.7668, 11.5554], [-5.9761, 10.9789], [-7.1168, 10.2762], [-8.1758, 9.4555], [-9.1408, 8.5262], [-10.0008, 7.4989], [-10.746, 6.3855], [-11.3677, 5.1987], [-11.8588, 3.9522], [-12.2136, 2.6603], [-12.4282, 1.3378], [-12.5, 0], [-12.4282, -1.3378], [-12.2136, -2.6603], [-11.8588, -3.9522], [-11.3677, -5.1987], [-10.746, -6.3855], [-10.0008, -7.4989], [-9.1408, -8.5262], [-8.1758, -9.4555], [-7.1168, -10.2762], [-5.9761, -10.9789], [-4.7668, -11.5554], [-3.5027, -11.9992], [-2.1983, -12.3052], [-0.8687, -12.4698], [0.4709, -12.4911], [1.805, -12.369], [3.1185, -12.1048], [4.3961, -11.7015], [5.6232, -11.1638], [6.7857, -10.4978], [5.789, -10.7592], [4.7723, -10.9262], [3.7443, -10.9973], [2.7142, -10.9719], [1.691, -10.8502], [0.6837, -10.6334], [-0.2989, -10.3232], [-1.2481, -9.9225], [-2.1557, -9.4346], [-3.0137, -8.8641], [-3.8145, -8.2157], [-4.5512, -7.4952], [-5.2172, -6.709], [-5.8067, -5.8639], [-6.3146, -4.9673], [-6.7363, -4.0272], [-7.0682, -3.0517], [-7.3074, -2.0495], [-7.4517, -1.0293], [-7.5, 0], [-7.4517, 1.0293], [-7.3074, 2.0495], [-7.0682, 3.0517], [-6.7363, 4.0272], [-6.3146, 4.9673], [-5.8067, 5.8639], [-5.2172, 6.709], [-4.5512, 7.4952], [-3.8145, 8.2157], [-3.0137, 8.8641], [-2.1557, 9.4346], [-1.2481, 9.9225], [-0.2989, 10.3232], [0.6837, 10.6334], [1.691, 10.8502], [2.7142, 10.9719], [3.7443, 10.9973], [4.7723, 10.9262], [5.789, 10.7592]], 0.61);
    fCylinder(context, C1 + "o3", { "bottomCenter" : vector(0, 3.8, -23) * millimeter, "topCenter" : vector(0, 8, -23) * millimeter, "radius" : 1.6 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(0, 0, 20.9) * millimeter, "topCenter" : vector(0, 0, 41.9) * millimeter, "radius" : 1.3 * millimeter });
    try { cutAll(context, id + "welle_zapfen_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY)); } catch (e) { err[] = err[] ~ "welle_zapfen Abzug Stufe 1; "; }
    setProperty(context, { "entities" : qBodyType(qUnion([qCreatedBy(A0, EntityType.BODY)]), BodyType.SOLID), "propertyType" : PropertyType.NAME, "value" : "QV-Getriebe Welle mit Zapfen fuer 6200 (ein Teil, Exzenter e 3,5)" });
}

function qvg_distanzring_e3_5(context is Context, id is Id, err is box)
{
    const A0 = id + "distanzring_e3_5_a0";
    prismZ(context, A0 + "s0", vector(0, 0, 27) * millimeter, [[3.5, -11], [3.1401, -10.9941], [2.7806, -10.9764], [2.4218, -10.947], [2.0642, -10.9059], [1.7081, -10.8531], [1.354, -10.7886], [1.0022, -10.7126], [0.653, -10.6252], [0.3069, -10.5263], [-0.0358, -10.4162], [-0.3748, -10.295], [-0.7095, -10.1627], [-1.0398, -10.0195], [-1.3652, -9.8656], [-1.6854, -9.7011], [-2, -9.5263], [-2.3087, -9.3412], [-2.6113, -9.1462], [-2.9073, -8.9413], [-3.1964, -8.7269], [-3.4783, -8.5031], [-3.7528, -8.2702], [-4.0195, -8.0285], [-4.2782, -7.7782], [-4.5285, -7.5195], [-4.7702, -7.2528], [-5.0031, -6.9783], [-5.2269, -6.6964], [-5.4413, -6.4073], [-5.6462, -6.1113], [-5.8412, -5.8087], [-6.0263, -5.5], [-6.2011, -5.1854], [-6.3656, -4.8652], [-6.5195, -4.5398], [-6.6627, -4.2095], [-6.795, -3.8748], [-6.9162, -3.5358], [-7.0263, -3.1931], [-7.1252, -2.847], [-7.2126, -2.4978], [-7.2886, -2.146], [-7.3531, -1.7919], [-7.4059, -1.4358], [-7.447, -1.0782], [-7.4764, -0.7194], [-7.4941, -0.3599], [-7.5, 0], [-7.4941, 0.3599], [-7.4764, 0.7194], [-7.447, 1.0782], [-7.4059, 1.4358], [-7.3531, 1.7919], [-7.2886, 2.146], [-7.2126, 2.4978], [-7.1252, 2.847], [-7.0263, 3.1931], [-6.9162, 3.5358], [-6.795, 3.8748], [-6.6627, 4.2095], [-6.5195, 4.5398], [-6.3656, 4.8652], [-6.2011, 5.1854], [-6.0263, 5.5], [-5.8412, 5.8087], [-5.6462, 6.1113], [-5.4413, 6.4073], [-5.2269, 6.6964], [-5.0031, 6.9783], [-4.7702, 7.2528], [-4.5285, 7.5195], [-4.2782, 7.7782], [-4.0195, 8.0285], [-3.7528, 8.2702], [-3.4783, 8.5031], [-3.1964, 8.7269], [-2.9073, 8.9413], [-2.6113, 9.1462], [-2.3087, 9.3412], [-2, 9.5263], [-1.6854, 9.7011], [-1.3652, 9.8656], [-1.0398, 10.0195], [-0.7095, 10.1627], [-0.3748, 10.295], [-0.0358, 10.4162], [0.3069, 10.5263], [0.653, 10.6252], [1.0022, 10.7126], [1.354, 10.7886], [1.7081, 10.8531], [2.0642, 10.9059], [2.4218, 10.947], [2.7806, 10.9764], [3.1401, 10.9941], [3.5, 11], [3.8599, 10.9941], [4.2194, 10.9764], [4.5782, 10.947], [4.9358, 10.9059], [5.2919, 10.8531], [5.646, 10.7886], [5.9978, 10.7126], [6.347, 10.6252], [6.6931, 10.5263], [7.0358, 10.4162], [7.3748, 10.295], [7.7095, 10.1627], [8.0398, 10.0195], [8.3652, 9.8656], [8.6854, 9.7011], [9, 9.5263], [9.3087, 9.3412], [9.6113, 9.1462], [9.9073, 8.9413], [10.1964, 8.7269], [10.4783, 8.5031], [10.7528, 8.2702], [11.0195, 8.0285], [11.2782, 7.7782], [11.5285, 7.5195], [11.7702, 7.2528], [12.0031, 6.9783], [12.2269, 6.6964], [12.4413, 6.4073], [12.6462, 6.1113], [12.8412, 5.8087], [13.0263, 5.5], [13.2011, 5.1854], [13.3656, 4.8652], [13.5195, 4.5398], [13.6627, 4.2095], [13.795, 3.8748], [13.9162, 3.5358], [14.0263, 3.1931], [14.1252, 2.847], [14.2126, 2.4978], [14.2886, 2.146], [14.3531, 1.7919], [14.4059, 1.4358], [14.447, 1.0782], [14.4764, 0.7194], [14.4941, 0.3599], [14.5, 0], [14.4941, -0.3599], [14.4764, -0.7194], [14.447, -1.0782], [14.4059, -1.4358], [14.3531, -1.7919], [14.2886, -2.146], [14.2126, -2.4978], [14.1252, -2.847], [14.0263, -3.1931], [13.9162, -3.5358], [13.795, -3.8748], [13.6627, -4.2095], [13.5195, -4.5398], [13.3656, -4.8652], [13.2011, -5.1854], [13.0263, -5.5], [12.8412, -5.8087], [12.6462, -6.1113], [12.4413, -6.4073], [12.2269, -6.6964], [12.0031, -6.9783], [11.7702, -7.2528], [11.5285, -7.5195], [11.2782, -7.7782], [11.0195, -8.0285], [10.7528, -8.2702], [10.4783, -8.5031], [10.1964, -8.7269], [9.9073, -8.9413], [9.6113, -9.1462], [9.3087, -9.3412], [9, -9.5263], [8.6854, -9.7011], [8.3652, -9.8656], [8.0398, -10.0195], [7.7095, -10.1627], [7.3748, -10.295], [7.0358, -10.4162], [6.6931, -10.5263], [6.347, -10.6252], [5.9978, -10.7126], [5.646, -10.7886], [5.2919, -10.8531], [4.9358, -10.9059], [4.5782, -10.947], [4.2194, -10.9764], [3.8599, -10.9941]], 3.3);
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(0, 0, 30.29) * millimeter, "topCenter" : vector(0, 0, 32) * millimeter, "radius" : 6.75 * millimeter });
    try { unionAll(context, id + "distanzring_e3_5_u0", qUnion([qCreatedBy(A0, EntityType.BODY)])); } catch (e) { err[] = err[] ~ "distanzring_e3_5 Vereinigung Stufe 0; "; }
    const C1 = id + "distanzring_e3_5_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(0, 0, 26) * millimeter, "topCenter" : vector(0, 0, 33) * millimeter, "radius" : 5.3 * millimeter });
    try { cutAll(context, id + "distanzring_e3_5_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY)); } catch (e) { err[] = err[] ~ "distanzring_e3_5 Abzug Stufe 1; "; }
    setProperty(context, { "entities" : qBodyType(qUnion([qCreatedBy(A0, EntityType.BODY)]), BodyType.SOLID), "propertyType" : PropertyType.NAME, "value" : "QV Distanzring zwischen 6203 und 6200 (lose, e 3.5)" });
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
    setProperty(context, { "entities" : qBodyType(qUnion([qCreatedBy(A0, EntityType.BODY)]), BodyType.SOLID), "propertyType" : PropertyType.NAME, "value" : "QV Lagerhuelse weit (vorhanden, Wand 2,5)" });
}

function qvg_l6203a(context is Context, id is Id, err is box)
{
    const A0 = id + "l6203a_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(3.5, 0, 3) * millimeter, "topCenter" : vector(3.5, 0, 15) * millimeter, "radius" : 20 * millimeter });
    try { unionAll(context, id + "l6203a_u0", qUnion([qCreatedBy(A0, EntityType.BODY)])); } catch (e) { err[] = err[] ~ "l6203a Vereinigung Stufe 0; "; }
    const C1 = id + "l6203a_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(3.5, 0, 2) * millimeter, "topCenter" : vector(3.5, 0, 16) * millimeter, "radius" : 8.5 * millimeter });
    try { cutAll(context, id + "l6203a_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY)); } catch (e) { err[] = err[] ~ "l6203a Abzug Stufe 1; "; }
    setProperty(context, { "entities" : qBodyType(qUnion([qCreatedBy(A0, EntityType.BODY)]), BodyType.SOLID), "propertyType" : PropertyType.NAME, "value" : "6203 vorn (Dummy)" });
}

function qvg_l6203b(context is Context, id is Id, err is box)
{
    const A0 = id + "l6203b_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(3.5, 0, 15) * millimeter, "topCenter" : vector(3.5, 0, 27) * millimeter, "radius" : 20 * millimeter });
    try { unionAll(context, id + "l6203b_u0", qUnion([qCreatedBy(A0, EntityType.BODY)])); } catch (e) { err[] = err[] ~ "l6203b Vereinigung Stufe 0; "; }
    const C1 = id + "l6203b_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(3.5, 0, 14) * millimeter, "topCenter" : vector(3.5, 0, 28) * millimeter, "radius" : 8.5 * millimeter });
    try { cutAll(context, id + "l6203b_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY)); } catch (e) { err[] = err[] ~ "l6203b Abzug Stufe 1; "; }
    setProperty(context, { "entities" : qBodyType(qUnion([qCreatedBy(A0, EntityType.BODY)]), BodyType.SOLID), "propertyType" : PropertyType.NAME, "value" : "6203 hinten (Dummy)" });
}

function qvg_lager6200(context is Context, id is Id, err is box)
{
    const A0 = id + "lager6200_a0";
    fCylinder(context, A0 + "s0", { "bottomCenter" : vector(0, 0, 32.2) * millimeter, "topCenter" : vector(0, 0, 41.2) * millimeter, "radius" : 15 * millimeter });
    try { unionAll(context, id + "lager6200_u0", qUnion([qCreatedBy(A0, EntityType.BODY)])); } catch (e) { err[] = err[] ~ "lager6200 Vereinigung Stufe 0; "; }
    const C1 = id + "lager6200_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(0, 0, 31.2) * millimeter, "topCenter" : vector(0, 0, 42.2) * millimeter, "radius" : 5 * millimeter });
    try { cutAll(context, id + "lager6200_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY)); } catch (e) { err[] = err[] ~ "lager6200 Abzug Stufe 1; "; }
    setProperty(context, { "entities" : qBodyType(qUnion([qCreatedBy(A0, EntityType.BODY)]), BodyType.SOLID), "propertyType" : PropertyType.NAME, "value" : "Lager 6200 (Dummy)" });
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
        try { qvg_welle_zapfen(context, id + "welle_zapfen", err); } catch (e) { err[] = err[] ~ "welle_zapfen abgebrochen; "; }
        try { qvg_distanzring_e3_5(context, id + "distanzring_e3_5", err); } catch (e) { err[] = err[] ~ "distanzring_e3_5 abgebrochen; "; }
        try { qvg_huelse(context, id + "huelse", err); } catch (e) { err[] = err[] ~ "huelse abgebrochen; "; }
        try { qvg_l6203a(context, id + "l6203a", err); } catch (e) { err[] = err[] ~ "l6203a abgebrochen; "; }
        try { qvg_l6203b(context, id + "l6203b", err); } catch (e) { err[] = err[] ~ "l6203b abgebrochen; "; }
        try { qvg_lager6200(context, id + "lager6200", err); } catch (e) { err[] = err[] ~ "lager6200 abgebrochen; "; }
        if (err[] != "")
            reportFeatureWarning(context, id, err[]);
    });
