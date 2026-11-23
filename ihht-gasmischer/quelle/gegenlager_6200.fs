FeatureScript 3112;
import(path : "onshape/std/geometry.fs", version : "3112.0");

// Gegenlager 6200 vorn im Steg fuer das Exzenter-Quetschventil (Rahmen v2 + Getriebe v1). Erzeugt von gen_gegenlager.py /
// build_gegenlager.py (ihht-pi CAD/quetschventil), nicht von Hand aendern. Koordinaten wie rahmen_v2.fs / getriebe_v1.fs.
// Im Einbau bei e 3,5 mit Platte, Ambossen, Welle, Huelse und Lager-Dummies; Scheiben e 4,5 / e 5,0 daneben bei x 70 / 110.
// Neu zu drucken: Steg 6200 und die Scheibe passend zur Welle. 6200 von vorn in den Steg druecken, Steg mit 4 x M3 x 12.

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

function qvg_scheibe_e3_5(context is Context, id is Id, err is box)
{
    const A0 = id + "scheibe_e3_5_a0";
    prismZ(context, A0 + "s0", vector(0, 0, 27) * millimeter, [[3.5, -11], [3.1401, -10.9941], [2.7806, -10.9764], [2.4218, -10.947], [2.0642, -10.9059], [1.7081, -10.8531], [1.354, -10.7886], [1.0022, -10.7126], [0.653, -10.6252], [0.3069, -10.5263], [-0.0358, -10.4162], [-0.3748, -10.295], [-0.7095, -10.1627], [-1.0398, -10.0195], [-1.3652, -9.8656], [-1.6854, -9.7011], [-2, -9.5263], [-2.3087, -9.3412], [-2.6113, -9.1462], [-2.9073, -8.9413], [-3.1964, -8.7269], [-3.4783, -8.5031], [-3.7528, -8.2702], [-4.0195, -8.0285], [-4.2782, -7.7782], [-4.5285, -7.5195], [-4.7702, -7.2528], [-5.0031, -6.9783], [-5.2269, -6.6964], [-5.4413, -6.4073], [-5.6462, -6.1113], [-5.8412, -5.8087], [-6.0263, -5.5], [-6.2011, -5.1854], [-6.3656, -4.8652], [-6.5195, -4.5398], [-6.6627, -4.2095], [-6.795, -3.8748], [-6.9162, -3.5358], [-7.0263, -3.1931], [-7.1252, -2.847], [-7.2126, -2.4978], [-7.2886, -2.146], [-7.3531, -1.7919], [-7.4059, -1.4358], [-7.447, -1.0782], [-7.4764, -0.7194], [-7.4941, -0.3599], [-7.5, 0], [-7.4941, 0.3599], [-7.4764, 0.7194], [-7.447, 1.0782], [-7.4059, 1.4358], [-7.3531, 1.7919], [-7.2886, 2.146], [-7.2126, 2.4978], [-7.1252, 2.847], [-7.0263, 3.1931], [-6.9162, 3.5358], [-6.795, 3.8748], [-6.6627, 4.2095], [-6.5195, 4.5398], [-6.3656, 4.8652], [-6.2011, 5.1854], [-6.0263, 5.5], [-5.8412, 5.8087], [-5.6462, 6.1113], [-5.4413, 6.4073], [-5.2269, 6.6964], [-5.0031, 6.9783], [-4.7702, 7.2528], [-4.5285, 7.5195], [-4.2782, 7.7782], [-4.0195, 8.0285], [-3.7528, 8.2702], [-3.4783, 8.5031], [-3.1964, 8.7269], [-2.9073, 8.9413], [-2.6113, 9.1462], [-2.3087, 9.3412], [-2, 9.5263], [-1.6854, 9.7011], [-1.3652, 9.8656], [-1.0398, 10.0195], [-0.7095, 10.1627], [-0.3748, 10.295], [-0.0358, 10.4162], [0.3069, 10.5263], [0.653, 10.6252], [1.0022, 10.7126], [1.354, 10.7886], [1.7081, 10.8531], [2.0642, 10.9059], [2.4218, 10.947], [2.7806, 10.9764], [3.1401, 10.9941], [3.5, 11], [3.8599, 10.9941], [4.2194, 10.9764], [4.5782, 10.947], [4.9358, 10.9059], [5.2919, 10.8531], [5.646, 10.7886], [5.9978, 10.7126], [6.347, 10.6252], [6.6931, 10.5263], [7.0358, 10.4162], [7.3748, 10.295], [7.7095, 10.1627], [8.0398, 10.0195], [8.3652, 9.8656], [8.6854, 9.7011], [9, 9.5263], [9.3087, 9.3412], [9.6113, 9.1462], [9.9073, 8.9413], [10.1964, 8.7269], [10.4783, 8.5031], [10.7528, 8.2702], [11.0195, 8.0285], [11.2782, 7.7782], [11.5285, 7.5195], [11.7702, 7.2528], [12.0031, 6.9783], [12.2269, 6.6964], [12.4413, 6.4073], [12.6462, 6.1113], [12.8412, 5.8087], [13.0263, 5.5], [13.2011, 5.1854], [13.3656, 4.8652], [13.5195, 4.5398], [13.6627, 4.2095], [13.795, 3.8748], [13.9162, 3.5358], [14.0263, 3.1931], [14.1252, 2.847], [14.2126, 2.4978], [14.2886, 2.146], [14.3531, 1.7919], [14.4059, 1.4358], [14.447, 1.0782], [14.4764, 0.7194], [14.4941, 0.3599], [14.5, 0], [14.4941, -0.3599], [14.4764, -0.7194], [14.447, -1.0782], [14.4059, -1.4358], [14.3531, -1.7919], [14.2886, -2.146], [14.2126, -2.4978], [14.1252, -2.847], [14.0263, -3.1931], [13.9162, -3.5358], [13.795, -3.8748], [13.6627, -4.2095], [13.5195, -4.5398], [13.3656, -4.8652], [13.2011, -5.1854], [13.0263, -5.5], [12.8412, -5.8087], [12.6462, -6.1113], [12.4413, -6.4073], [12.2269, -6.6964], [12.0031, -6.9783], [11.7702, -7.2528], [11.5285, -7.5195], [11.2782, -7.7782], [11.0195, -8.0285], [10.7528, -8.2702], [10.4783, -8.5031], [10.1964, -8.7269], [9.9073, -8.9413], [9.6113, -9.1462], [9.3087, -9.3412], [9, -9.5263], [8.6854, -9.7011], [8.3652, -9.8656], [8.0398, -10.0195], [7.7095, -10.1627], [7.3748, -10.295], [7.0358, -10.4162], [6.6931, -10.5263], [6.347, -10.6252], [5.9978, -10.7126], [5.646, -10.7886], [5.2919, -10.8531], [4.9358, -10.9059], [4.5782, -10.947], [4.2194, -10.9764], [3.8599, -10.9941]], 3.3);
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(0, 0, 30.29) * millimeter, "topCenter" : vector(0, 0, 32) * millimeter, "radius" : 6.75 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(0, 0, 31.99) * millimeter, "topCenter" : vector(0, 0, 40.4) * millimeter, "radius" : 4.975 * millimeter });
    fCone(context, A0 + "s3", { "bottomCenter" : vector(0, 0, 40.39) * millimeter, "topCenter" : vector(0, 0, 40.9) * millimeter, "bottomRadius" : 4.975 * millimeter, "topRadius" : 4.525 * millimeter });
    prismZ(context, A0 + "s4", vector(0, 0, 32.6) * millimeter, [[0.45, 4.775], [0.45, 5.095], [-0.45, 5.095], [-0.45, 4.775]], 7.5);
    prismZ(context, A0 + "s5", vector(0, 0, 32.6) * millimeter, [[-4.3603, -1.9978], [-4.6374, -2.1578], [-4.1874, -2.9372], [-3.9103, -2.7772]], 7.5);
    prismZ(context, A0 + "s6", vector(0, 0, 32.6) * millimeter, [[3.9103, -2.7772], [4.1874, -2.9372], [4.6374, -2.1578], [4.3603, -1.9978]], 7.5);
    try { unionAll(context, id + "scheibe_e3_5_u0", qUnion([qCreatedBy(A0, EntityType.BODY)])); } catch (e) { err[] = err[] ~ "scheibe_e3_5 Vereinigung Stufe 0; "; }
    const C1 = id + "scheibe_e3_5_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(3.5, 5.5, 26) * millimeter, "topCenter" : vector(3.5, 5.5, 29.6) * millimeter, "radius" : 1.65 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(3.5, -5.5, 26) * millimeter, "topCenter" : vector(3.5, -5.5, 29.6) * millimeter, "radius" : 1.65 * millimeter });
    fCone(context, C1 + "o2", { "bottomCenter" : vector(3.5, 5.5, 26.99) * millimeter, "topCenter" : vector(3.5, 5.5, 27.4) * millimeter, "bottomRadius" : 2.05 * millimeter, "topRadius" : 1.65 * millimeter });
    fCone(context, C1 + "o3", { "bottomCenter" : vector(3.5, -5.5, 26.99) * millimeter, "topCenter" : vector(3.5, -5.5, 27.4) * millimeter, "bottomRadius" : 2.05 * millimeter, "topRadius" : 1.65 * millimeter });
    try { cutAll(context, id + "scheibe_e3_5_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY)); } catch (e) { err[] = err[] ~ "scheibe_e3_5 Abzug Stufe 1; "; }
    setProperty(context, { "entities" : qBodyType(qUnion([qCreatedBy(A0, EntityType.BODY)]), BodyType.SOLID), "propertyType" : PropertyType.NAME, "value" : "QV Gegenlagerscheibe 6200 fuer Welle e 3,5 (0 Punkte)" });
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

function qvg_scheibe_e4_5(context is Context, id is Id, err is box)
{
    const A0 = id + "scheibe_e4_5_a0";
    prismZ(context, A0 + "s0", vector(70, 0, 27) * millimeter, [[4.5, -11], [4.1401, -10.9941], [3.7806, -10.9764], [3.4218, -10.947], [3.0642, -10.9059], [2.7081, -10.8531], [2.354, -10.7886], [2.0022, -10.7126], [1.653, -10.6252], [1.3069, -10.5263], [0.9642, -10.4162], [0.6252, -10.295], [0.2905, -10.1627], [-0.0398, -10.0195], [-0.3652, -9.8656], [-0.6854, -9.7011], [-1, -9.5263], [-1.3087, -9.3412], [-1.6113, -9.1462], [-1.9073, -8.9413], [-2.1964, -8.7269], [-2.4783, -8.5031], [-2.7528, -8.2702], [-3.0195, -8.0285], [-3.2782, -7.7782], [-3.5285, -7.5195], [-3.7702, -7.2528], [-4.0031, -6.9783], [-4.2269, -6.6964], [-4.4413, -6.4073], [-4.6462, -6.1113], [-4.8412, -5.8087], [-5.0263, -5.5], [-5.2011, -5.1854], [-5.3656, -4.8652], [-5.5195, -4.5398], [-5.6627, -4.2095], [-5.795, -3.8748], [-5.9162, -3.5358], [-6.3918, -2.1697], [-6.4593, -1.9594], [-6.52, -1.747], [-6.5737, -1.5328], [-6.6203, -1.3169], [-6.6598, -1.0995], [-6.6923, -0.8811], [-6.7175, -0.6616], [-6.7355, -0.4415], [-6.7464, -0.2209], [-6.75, 0], [-6.7464, 0.2209], [-6.7355, 0.4415], [-6.7175, 0.6616], [-6.6923, 0.8811], [-6.6598, 1.0995], [-6.6203, 1.3169], [-6.5737, 1.5328], [-6.52, 1.747], [-6.4593, 1.9594], [-6.3918, 2.1697], [-5.9162, 3.5358], [-5.795, 3.8748], [-5.6627, 4.2095], [-5.5195, 4.5398], [-5.3656, 4.8652], [-5.2011, 5.1854], [-5.0263, 5.5], [-4.8412, 5.8087], [-4.6462, 6.1113], [-4.4413, 6.4073], [-4.2269, 6.6964], [-4.0031, 6.9783], [-3.7702, 7.2528], [-3.5285, 7.5195], [-3.2782, 7.7782], [-3.0195, 8.0285], [-2.7528, 8.2702], [-2.4783, 8.5031], [-2.1964, 8.7269], [-1.9073, 8.9413], [-1.6113, 9.1462], [-1.3087, 9.3412], [-1, 9.5263], [-0.6854, 9.7011], [-0.3652, 9.8656], [-0.0398, 10.0195], [0.2905, 10.1627], [0.6252, 10.295], [0.9642, 10.4162], [1.3069, 10.5263], [1.653, 10.6252], [2.0022, 10.7126], [2.354, 10.7886], [2.7081, 10.8531], [3.0642, 10.9059], [3.4218, 10.947], [3.7806, 10.9764], [4.1401, 10.9941], [4.5, 11], [4.8599, 10.9941], [5.2194, 10.9764], [5.5782, 10.947], [5.9358, 10.9059], [6.2919, 10.8531], [6.646, 10.7886], [6.9978, 10.7126], [7.347, 10.6252], [7.6931, 10.5263], [8.0358, 10.4162], [8.3748, 10.295], [8.7095, 10.1627], [9.0398, 10.0195], [9.3652, 9.8656], [9.6854, 9.7011], [10, 9.5263], [10.3087, 9.3412], [10.6113, 9.1462], [10.9073, 8.9413], [11.1964, 8.7269], [11.4783, 8.5031], [11.7528, 8.2702], [12.0195, 8.0285], [12.2782, 7.7782], [12.5285, 7.5195], [12.7702, 7.2528], [13.0031, 6.9783], [13.2269, 6.6964], [13.4413, 6.4073], [13.6462, 6.1113], [13.8412, 5.8087], [14.0263, 5.5], [14.2011, 5.1854], [14.3656, 4.8652], [14.5195, 4.5398], [14.6627, 4.2095], [14.795, 3.8748], [14.9162, 3.5358], [15.0263, 3.1931], [15.1252, 2.847], [15.2126, 2.4978], [15.2886, 2.146], [15.3531, 1.7919], [15.4059, 1.4358], [15.447, 1.0782], [15.4764, 0.7194], [15.4941, 0.3599], [15.5, 0], [15.4941, -0.3599], [15.4764, -0.7194], [15.447, -1.0782], [15.4059, -1.4358], [15.3531, -1.7919], [15.2886, -2.146], [15.2126, -2.4978], [15.1252, -2.847], [15.0263, -3.1931], [14.9162, -3.5358], [14.795, -3.8748], [14.6627, -4.2095], [14.5195, -4.5398], [14.3656, -4.8652], [14.2011, -5.1854], [14.0263, -5.5], [13.8412, -5.8087], [13.6462, -6.1113], [13.4413, -6.4073], [13.2269, -6.6964], [13.0031, -6.9783], [12.7702, -7.2528], [12.5285, -7.5195], [12.2782, -7.7782], [12.0195, -8.0285], [11.7528, -8.2702], [11.4783, -8.5031], [11.1964, -8.7269], [10.9073, -8.9413], [10.6113, -9.1462], [10.3087, -9.3412], [10, -9.5263], [9.6854, -9.7011], [9.3652, -9.8656], [9.0398, -10.0195], [8.7095, -10.1627], [8.3748, -10.295], [8.0358, -10.4162], [7.6931, -10.5263], [7.347, -10.6252], [6.9978, -10.7126], [6.646, -10.7886], [6.2919, -10.8531], [5.9358, -10.9059], [5.5782, -10.947], [5.2194, -10.9764], [4.8599, -10.9941]], 3.3);
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(70, 0, 30.29) * millimeter, "topCenter" : vector(70, 0, 32) * millimeter, "radius" : 6.75 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(70, 0, 31.99) * millimeter, "topCenter" : vector(70, 0, 40.4) * millimeter, "radius" : 4.975 * millimeter });
    fCone(context, A0 + "s3", { "bottomCenter" : vector(70, 0, 40.39) * millimeter, "topCenter" : vector(70, 0, 40.9) * millimeter, "bottomRadius" : 4.975 * millimeter, "topRadius" : 4.525 * millimeter });
    prismZ(context, A0 + "s4", vector(70, 0, 32.6) * millimeter, [[0.45, 4.775], [0.45, 5.095], [-0.45, 5.095], [-0.45, 4.775]], 7.5);
    prismZ(context, A0 + "s5", vector(70, 0, 32.6) * millimeter, [[-4.3603, -1.9978], [-4.6374, -2.1578], [-4.1874, -2.9372], [-3.9103, -2.7772]], 7.5);
    prismZ(context, A0 + "s6", vector(70, 0, 32.6) * millimeter, [[3.9103, -2.7772], [4.1874, -2.9372], [4.6374, -2.1578], [4.3603, -1.9978]], 7.5);
    try { unionAll(context, id + "scheibe_e4_5_u0", qUnion([qCreatedBy(A0, EntityType.BODY)])); } catch (e) { err[] = err[] ~ "scheibe_e4_5 Vereinigung Stufe 0; "; }
    const C1 = id + "scheibe_e4_5_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(74.5, 5.5, 26) * millimeter, "topCenter" : vector(74.5, 5.5, 29.6) * millimeter, "radius" : 1.65 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(74.5, -5.5, 26) * millimeter, "topCenter" : vector(74.5, -5.5, 29.6) * millimeter, "radius" : 1.65 * millimeter });
    fCone(context, C1 + "o2", { "bottomCenter" : vector(74.5, 5.5, 26.99) * millimeter, "topCenter" : vector(74.5, 5.5, 27.4) * millimeter, "bottomRadius" : 2.05 * millimeter, "topRadius" : 1.65 * millimeter });
    fCone(context, C1 + "o3", { "bottomCenter" : vector(74.5, -5.5, 26.99) * millimeter, "topCenter" : vector(74.5, -5.5, 27.4) * millimeter, "bottomRadius" : 2.05 * millimeter, "topRadius" : 1.65 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(82.5, 0, 29.7) * millimeter, "topCenter" : vector(82.5, 0, 31.3) * millimeter, "radius" : 0.6 * millimeter });
    try { cutAll(context, id + "scheibe_e4_5_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY)); } catch (e) { err[] = err[] ~ "scheibe_e4_5 Abzug Stufe 1; "; }
    setProperty(context, { "entities" : qBodyType(qUnion([qCreatedBy(A0, EntityType.BODY)]), BodyType.SOLID), "propertyType" : PropertyType.NAME, "value" : "QV Gegenlagerscheibe 6200 fuer Welle e 4,5 (1 Punkte)" });
}

function qvg_scheibe_e5(context is Context, id is Id, err is box)
{
    const A0 = id + "scheibe_e5_a0";
    prismZ(context, A0 + "s0", vector(110, 0, 27) * millimeter, [[5, -11], [4.6401, -10.9941], [4.2806, -10.9764], [3.9218, -10.947], [3.5642, -10.9059], [3.2081, -10.8531], [2.854, -10.7886], [2.5022, -10.7126], [2.153, -10.6252], [1.8069, -10.5263], [1.4642, -10.4162], [1.1252, -10.295], [0.7905, -10.1627], [0.4602, -10.0195], [0.1348, -9.8656], [-0.1854, -9.7011], [-0.5, -9.5263], [-0.8087, -9.3412], [-1.1113, -9.1462], [-1.4073, -8.9413], [-1.6964, -8.7269], [-1.9783, -8.5031], [-2.2528, -8.2702], [-2.5195, -8.0285], [-2.7782, -7.7782], [-3.0285, -7.5195], [-3.2702, -7.2528], [-3.5031, -6.9783], [-3.7269, -6.6964], [-3.9413, -6.4073], [-4.1462, -6.1113], [-4.3412, -5.8087], [-5.7321, -3.5645], [-5.8457, -3.375], [-5.953, -3.1819], [-6.0539, -2.9854], [-6.1483, -2.7858], [-6.2362, -2.5831], [-6.3174, -2.3777], [-6.3918, -2.1697], [-6.4593, -1.9594], [-6.52, -1.747], [-6.5737, -1.5328], [-6.6203, -1.3169], [-6.6598, -1.0995], [-6.6923, -0.8811], [-6.7175, -0.6616], [-6.7355, -0.4415], [-6.7464, -0.2209], [-6.75, 0], [-6.7464, 0.2209], [-6.7355, 0.4415], [-6.7175, 0.6616], [-6.6923, 0.8811], [-6.6598, 1.0995], [-6.6203, 1.3169], [-6.5737, 1.5328], [-6.52, 1.747], [-6.4593, 1.9594], [-6.3918, 2.1697], [-6.3174, 2.3777], [-6.2362, 2.5831], [-6.1483, 2.7858], [-6.0539, 2.9854], [-5.953, 3.1819], [-5.8457, 3.375], [-5.7321, 3.5645], [-4.3412, 5.8087], [-4.1462, 6.1113], [-3.9413, 6.4073], [-3.7269, 6.6964], [-3.5031, 6.9783], [-3.2702, 7.2528], [-3.0285, 7.5195], [-2.7782, 7.7782], [-2.5195, 8.0285], [-2.2528, 8.2702], [-1.9783, 8.5031], [-1.6964, 8.7269], [-1.4073, 8.9413], [-1.1113, 9.1462], [-0.8087, 9.3412], [-0.5, 9.5263], [-0.1854, 9.7011], [0.1348, 9.8656], [0.4602, 10.0195], [0.7905, 10.1627], [1.1252, 10.295], [1.4642, 10.4162], [1.8069, 10.5263], [2.153, 10.6252], [2.5022, 10.7126], [2.854, 10.7886], [3.2081, 10.8531], [3.5642, 10.9059], [3.9218, 10.947], [4.2806, 10.9764], [4.6401, 10.9941], [5, 11], [5.3599, 10.9941], [5.7194, 10.9764], [6.0782, 10.947], [6.4358, 10.9059], [6.7919, 10.8531], [7.146, 10.7886], [7.4978, 10.7126], [7.847, 10.6252], [8.1931, 10.5263], [8.5358, 10.4162], [8.8748, 10.295], [9.2095, 10.1627], [9.5398, 10.0195], [9.8652, 9.8656], [10.1854, 9.7011], [10.5, 9.5263], [10.8087, 9.3412], [11.1113, 9.1462], [11.4073, 8.9413], [11.6964, 8.7269], [11.9783, 8.5031], [12.2528, 8.2702], [12.5195, 8.0285], [12.7782, 7.7782], [13.0285, 7.5195], [13.2702, 7.2528], [13.5031, 6.9783], [13.7269, 6.6964], [13.9413, 6.4073], [14.1462, 6.1113], [14.3412, 5.8087], [14.5263, 5.5], [14.7011, 5.1854], [14.8656, 4.8652], [15.0195, 4.5398], [15.1627, 4.2095], [15.295, 3.8748], [15.4162, 3.5358], [15.5263, 3.1931], [15.6252, 2.847], [15.7126, 2.4978], [15.7886, 2.146], [15.8531, 1.7919], [15.9059, 1.4358], [15.947, 1.0782], [15.9764, 0.7194], [15.9941, 0.3599], [16, 0], [15.9941, -0.3599], [15.9764, -0.7194], [15.947, -1.0782], [15.9059, -1.4358], [15.8531, -1.7919], [15.7886, -2.146], [15.7126, -2.4978], [15.6252, -2.847], [15.5263, -3.1931], [15.4162, -3.5358], [15.295, -3.8748], [15.1627, -4.2095], [15.0195, -4.5398], [14.8656, -4.8652], [14.7011, -5.1854], [14.5263, -5.5], [14.3412, -5.8087], [14.1462, -6.1113], [13.9413, -6.4073], [13.7269, -6.6964], [13.5031, -6.9783], [13.2702, -7.2528], [13.0285, -7.5195], [12.7782, -7.7782], [12.5195, -8.0285], [12.2528, -8.2702], [11.9783, -8.5031], [11.6964, -8.7269], [11.4073, -8.9413], [11.1113, -9.1462], [10.8087, -9.3412], [10.5, -9.5263], [10.1854, -9.7011], [9.8652, -9.8656], [9.5398, -10.0195], [9.2095, -10.1627], [8.8748, -10.295], [8.5358, -10.4162], [8.1931, -10.5263], [7.847, -10.6252], [7.4978, -10.7126], [7.146, -10.7886], [6.7919, -10.8531], [6.4358, -10.9059], [6.0782, -10.947], [5.7194, -10.9764], [5.3599, -10.9941]], 3.3);
    fCylinder(context, A0 + "s1", { "bottomCenter" : vector(110, 0, 30.29) * millimeter, "topCenter" : vector(110, 0, 32) * millimeter, "radius" : 6.75 * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(110, 0, 31.99) * millimeter, "topCenter" : vector(110, 0, 40.4) * millimeter, "radius" : 4.975 * millimeter });
    fCone(context, A0 + "s3", { "bottomCenter" : vector(110, 0, 40.39) * millimeter, "topCenter" : vector(110, 0, 40.9) * millimeter, "bottomRadius" : 4.975 * millimeter, "topRadius" : 4.525 * millimeter });
    prismZ(context, A0 + "s4", vector(110, 0, 32.6) * millimeter, [[0.45, 4.775], [0.45, 5.095], [-0.45, 5.095], [-0.45, 4.775]], 7.5);
    prismZ(context, A0 + "s5", vector(110, 0, 32.6) * millimeter, [[-4.3603, -1.9978], [-4.6374, -2.1578], [-4.1874, -2.9372], [-3.9103, -2.7772]], 7.5);
    prismZ(context, A0 + "s6", vector(110, 0, 32.6) * millimeter, [[3.9103, -2.7772], [4.1874, -2.9372], [4.6374, -2.1578], [4.3603, -1.9978]], 7.5);
    try { unionAll(context, id + "scheibe_e5_u0", qUnion([qCreatedBy(A0, EntityType.BODY)])); } catch (e) { err[] = err[] ~ "scheibe_e5 Vereinigung Stufe 0; "; }
    const C1 = id + "scheibe_e5_c1";
    fCylinder(context, C1 + "o0", { "bottomCenter" : vector(115, 5.5, 26) * millimeter, "topCenter" : vector(115, 5.5, 29.6) * millimeter, "radius" : 1.65 * millimeter });
    fCylinder(context, C1 + "o1", { "bottomCenter" : vector(115, -5.5, 26) * millimeter, "topCenter" : vector(115, -5.5, 29.6) * millimeter, "radius" : 1.65 * millimeter });
    fCone(context, C1 + "o2", { "bottomCenter" : vector(115, 5.5, 26.99) * millimeter, "topCenter" : vector(115, 5.5, 27.4) * millimeter, "bottomRadius" : 2.05 * millimeter, "topRadius" : 1.65 * millimeter });
    fCone(context, C1 + "o3", { "bottomCenter" : vector(115, -5.5, 26.99) * millimeter, "topCenter" : vector(115, -5.5, 27.4) * millimeter, "bottomRadius" : 2.05 * millimeter, "topRadius" : 1.65 * millimeter });
    fCylinder(context, C1 + "o4", { "bottomCenter" : vector(123, -2.5, 29.7) * millimeter, "topCenter" : vector(123, -2.5, 31.3) * millimeter, "radius" : 0.6 * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(123, 2.5, 29.7) * millimeter, "topCenter" : vector(123, 2.5, 31.3) * millimeter, "radius" : 0.6 * millimeter });
    try { cutAll(context, id + "scheibe_e5_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY)); } catch (e) { err[] = err[] ~ "scheibe_e5 Abzug Stufe 1; "; }
    setProperty(context, { "entities" : qBodyType(qUnion([qCreatedBy(A0, EntityType.BODY)]), BodyType.SOLID), "propertyType" : PropertyType.NAME, "value" : "QV Gegenlagerscheibe 6200 fuer Welle e 5 (2 Punkte)" });
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
        try { qvg_scheibe_e3_5(context, id + "scheibe_e3_5", err); } catch (e) { err[] = err[] ~ "scheibe_e3_5 abgebrochen; "; }
        try { qvg_huelse(context, id + "huelse", err); } catch (e) { err[] = err[] ~ "huelse abgebrochen; "; }
        try { qvg_l6203a(context, id + "l6203a", err); } catch (e) { err[] = err[] ~ "l6203a abgebrochen; "; }
        try { qvg_l6203b(context, id + "l6203b", err); } catch (e) { err[] = err[] ~ "l6203b abgebrochen; "; }
        try { qvg_lager6200(context, id + "lager6200", err); } catch (e) { err[] = err[] ~ "lager6200 abgebrochen; "; }
        try { qvg_scheibe_e4_5(context, id + "scheibe_e4_5", err); } catch (e) { err[] = err[] ~ "scheibe_e4_5 abgebrochen; "; }
        try { qvg_scheibe_e5(context, id + "scheibe_e5", err); } catch (e) { err[] = err[] ~ "scheibe_e5 abgebrochen; "; }
        if (err[] != "")
            reportFeatureWarning(context, id, err[]);
    });
