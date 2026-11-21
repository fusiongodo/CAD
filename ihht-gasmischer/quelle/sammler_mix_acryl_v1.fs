FeatureScript 3112;
import(path : "onshape/std/geometry.fs", version : "3112.0");
// Sammler Mix Acryl: erzeugt von gen_sammler_acryl.py (Basis gen5.py). Gewindestutzen 20 x 2.5 rechts, Huelse ID 12.4, 45 mm, aussen 26 -> 13.6.

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


// ===================== Gewindestutzen + Huelse (Sammler Mix Acryl) =====================
// Lokales System fuer Gewinde und Huelse: Achse +Z, z = 0 = Blockvorderseite.
// Welt: Achse +Y bei x = XO, z = ZO; lokales z -> Welt y - Y1.
const SM_XO = 0.0000;
const SM_Y1 = 57.0000;
const SM_ZO = 135.0000;

function smXf() returns Transform
{
    return transform(vector(SM_XO, SM_Y1, SM_ZO) * millimeter) *
        rotationAround(line(vector(0, 0, 0) * millimeter, vector(1, 0, 0)), -90 * degree);
}

// Rechtsgewinde-Pruefung (aus release_valve_a_v4): von +Z gesehen steigt z bei Drehung gegen den Uhrzeigersinn.
function helixIsRightHand(context is Context, edge is Query, turns is number) returns boolean
{
    const p0 = evEdgeTangentLine(context, { "edge" : edge, "parameter" : 0 }).origin;
    const p1 = evEdgeTangentLine(context, { "edge" : edge, "parameter" : 0.25 / turns }).origin;
    const crossZ = (p0[0] * p1[1] - p0[1] * p1[0]) / (millimeter * millimeter);
    const dz = (p1[2] - p0[2]) / millimeter;
    return crossZ * dz > 0;
}

// Trapez-Gewindezahn (60°), entlang einer Helix um Z gezogen. Immer RECHTSGEWINDE (rechts drehen = zu).
function makeThread(context is Context, id is Id, rRoot is ValueWithUnits, rCrest is ValueWithUnits,
    overlapR is ValueWithUnits, zStart is ValueWithUnits, pitch is ValueWithUnits, turns is number)
{
    const depth = abs(rCrest - rRoot);
    const crest = pitch / 8;
    const wRoot = crest + 2 * depth * tan(30 * degree);
    const rMid = (rRoot + rCrest) / 2;

    var helixId = id + "helix";
    for (var attempt = 0; attempt < 2; attempt += 1)
    {
        helixId = id + "helix" + ("a" ~ attempt);
        opHelix(context, helixId, {
                    "direction" : vector(0, 0, 1),
                    "axisStart" : vector(0, 0, 0) * millimeter,
                    "startPoint" : vector(rMid, 0 * millimeter, zStart),
                    "interval" : vector(0, turns),
                    "clockwise" : attempt == 0,
                    "helicalPitch" : pitch,
                    "spiralPitch" : 0 * millimeter
                });
        if (helixIsRightHand(context, qCreatedBy(helixId, EntityType.EDGE), turns))
            break;
        opDeleteBodies(context, id + "helix" + ("del" ~ attempt), { "entities" : qCreatedBy(helixId, EntityType.BODY) });
        if (attempt == 1)
            throw regenError("Rechtsgewinde konnte nicht erzeugt werden");
    }

    const sk = newSketchOnPlane(context, id + "profile", {
                "sketchPlane" : plane(vector(0, 0, 0) * millimeter, vector(0, -1, 0), vector(1, 0, 0))
            });
    const pts = [
            vector(overlapR, zStart - wRoot / 2),
            vector(rRoot, zStart - wRoot / 2),
            vector(rCrest, zStart - crest / 2),
            vector(rCrest, zStart + crest / 2),
            vector(rRoot, zStart + wRoot / 2),
            vector(overlapR, zStart + wRoot / 2)
        ];
    for (var i = 0; i < size(pts); i += 1)
        skLineSegment(sk, "l" ~ i, { "start" : pts[i], "end" : pts[(i + 1) % size(pts)] });
    skSolve(sk);
    opSweep(context, id + "sweep", { "profiles" : qSketchRegion(id + "profile"), "path" : qCreatedBy(helixId, EntityType.EDGE) });
}

function lz(z is number) returns Vector
{
    return vector(0, 0, z) * millimeter;
}

// Aussengewinde auf den Stutzen des Sammlers, vorne 45° angefast.
function smMaleThread(context is Context, id is Id)
{
    const P = 2.5000;
    const rC = 10.0000;
    const rR = 8.4665;
    const L = 8.0000;
    const sammler = qContainsPoint(qBodyType(qCreatedBy(id, EntityType.BODY), BodyType.SOLID), vector(SM_XO, SM_Y1 + 5, 119.5000) * millimeter);
    if (size(evaluateQuery(context, sammler)) != 1)
        throw regenError("Sammler-Koerper nicht gefunden");
    const g = id + "gew";
    makeThread(context, g + "thr", rR * millimeter, rC * millimeter, (rR - 0.4) * millimeter, -0.5 * P * millimeter,
        P * millimeter, (L + 1.5 * P) / P);
    const z0 = L - (rC + 1 - (rR - 0.5));     // 45°-Fase: von Kern - 0,5 an der Spitze bis ueber den Gewindekamm
    fCylinder(context, g + "trim" + "big", { "bottomCenter" : lz(z0), "topCenter" : lz(L + 2 * P + 2), "radius" : (rC + 1) * millimeter });
    fCone(context, g + "trim" + "keep", { "bottomCenter" : lz(z0 - 0.01), "topCenter" : lz(L),
                "bottomRadius" : (rC + 1.01) * millimeter, "topRadius" : (rR - 0.5) * millimeter });
    opBoolean(context, g + "trim" + "sub", { "tools" : qCreatedBy(g + "trim" + "keep", EntityType.BODY),
                "targets" : qCreatedBy(g + "trim" + "big", EntityType.BODY), "operationType" : BooleanOperationType.SUBTRACTION });
    opDeleteBodies(context, g + "clean", { "entities" : qUnion([qCreatedBy(g + "thr" + "helix", EntityType.BODY),
                    qCreatedBy(g + "thr" + "profile", EntityType.BODY)]) });
    opTransform(context, g + "xf", { "bodies" : qUnion([qCreatedBy(g + "thr" + "sweep", EntityType.BODY),
                    qCreatedBy(g + "trim", EntityType.BODY)]), "transform" : smXf() });
    opBoolean(context, g + "u", { "tools" : qUnion([sammler, qCreatedBy(g + "thr" + "sweep", EntityType.BODY)]),
                "operationType" : BooleanOperationType.UNION });
    opBoolean(context, g + "x", { "tools" : qCreatedBy(g + "trim", EntityType.BODY), "targets" : sammler,
                "operationType" : BooleanOperationType.SUBTRACTION });
    finishPart(context, sammler, "Sammler Mix Acryl (Gewindestutzen)", color(0.6000, 0.4000, 0.8000, 1.0000));
}

// Aufschraub-Huelse fuer den Acrylschlauch. Druck: Gewindeseite (z = 0) auf dem Bett, kein Support.
function smHuelse(context is Context, id is Id)
{
    const P = 2.5000;
    const rFmin = 8.7165;
    const rFmaj = 10.2500;
    const rA = 6.2000;
    const Lt = 10.0000;
    const zSeat = 12.5165;
    const zTop = 57.5165;
    const rTip = 6.8000;
    const rO = 13.0000;
    const h = id + "huelse";
    fCylinder(context, h + "a" + "grip", { "bottomCenter" : lz(0), "topCenter" : lz(Lt + 2), "radius" : rO * millimeter });
    fCone(context, h + "a" + "cone", { "bottomCenter" : lz(Lt + 1.99), "topCenter" : lz(zTop),
                "bottomRadius" : rO * millimeter, "topRadius" : rTip * millimeter });
    opBoolean(context, h + "u", { "tools" : qCreatedBy(h + "a", EntityType.BODY), "operationType" : BooleanOperationType.UNION });
    fCylinder(context, h + "c" + "core", { "bottomCenter" : lz(-1), "topCenter" : lz(Lt), "radius" : rFmin * millimeter });
    fCone(context, h + "c" + "seat", { "bottomCenter" : lz(Lt - 0.01), "topCenter" : lz(zSeat),
                "bottomRadius" : (rFmin + 0.01) * millimeter, "topRadius" : rA * millimeter });
    fCylinder(context, h + "c" + "bore", { "bottomCenter" : lz(zSeat - 0.01), "topCenter" : lz(zTop + 1), "radius" : rA * millimeter });
    fCone(context, h + "c" + "lead", { "bottomCenter" : lz(-0.01), "topCenter" : lz(rFmaj + 0.35 - rFmin),
                "bottomRadius" : (rFmaj + 0.36) * millimeter, "topRadius" : rFmin * millimeter });   // 45°-Einfuehrung
    for (var k = 0; k < 12; k += 1)
    {
        const a = k * 360 / 12 * degree;
        fCylinder(context, h + "c" + ("fl" ~ k), { "bottomCenter" : vector((rO + 0.6) * cos(a), (rO + 0.6) * sin(a), -1) * millimeter,
                    "topCenter" : vector((rO + 0.6) * cos(a), (rO + 0.6) * sin(a), Lt) * millimeter, "radius" : 1.5 * millimeter });
    }
    makeThread(context, h + "thr", rFmin * millimeter, rFmaj * millimeter, (rFmin - 0.4) * millimeter, -1.5 * P * millimeter,
        P * millimeter, (Lt + P) / P);     // Phase wie Stutzen (Start -0,5 P): in Einbaulage kollisionsfrei
    opBoolean(context, h + "x", { "tools" : qUnion([qCreatedBy(h + "c", EntityType.BODY), qCreatedBy(h + "thr" + "sweep", EntityType.BODY)]),
                "targets" : qCreatedBy(h + "a", EntityType.BODY), "operationType" : BooleanOperationType.SUBTRACTION });
    opDeleteBodies(context, h + "clean", { "entities" : qUnion([qCreatedBy(h + "thr" + "helix", EntityType.BODY),
                    qCreatedBy(h + "thr" + "profile", EntityType.BODY)]) });
    opTransform(context, h + "xf", { "bodies" : qCreatedBy(h + "a", EntityType.BODY), "transform" : smXf() });
    finishPart(context, qCreatedBy(h + "a", EntityType.BODY), "Sammler Mix Huelse Acryl ID 12.4 (senkrecht drucken, Gewinde unten)", color(0.6000, 0.4000, 0.8000, 1.0000));
}

function acr_splMixAcr(context is Context, id is Id, prog is box)
{
    prog[] = "Körper Stufe 0";
    const A0 = id + "splMixAcr_a0";
    fCuboid(context, A0 + "s0", { "corner1" : vector(-30.65, 45, 118) * millimeter, "corner2" : vector(30.65, 57, 152.7) * millimeter });
    fCuboid(context, A0 + "s1", { "corner1" : vector(-40.65, 37, 118) * millimeter, "corner2" : vector(40.65, 65, 121) * millimeter });
    fCylinder(context, A0 + "s2", { "bottomCenter" : vector(-22.65, 45.5, 131) * millimeter, "topCenter" : vector(-22.65, 33, 131) * millimeter, "radius" : 5.375 * millimeter });
    coneOp(context, A0 + "s3", vector(-22.65, 33.3, 131) * millimeter, vector(-22.65, 35.1, 131) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s4", { "bottomCenter" : vector(-22.65, 35.1, 131) * millimeter, "topCenter" : vector(-22.65, 35.4, 131) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s5", vector(-22.65, 35.7, 131) * millimeter, vector(-22.65, 37.5, 131) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s6", { "bottomCenter" : vector(-22.65, 37.5, 131) * millimeter, "topCenter" : vector(-22.65, 37.8, 131) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s7", vector(-22.65, 38.1, 131) * millimeter, vector(-22.65, 39.9, 131) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s8", { "bottomCenter" : vector(-22.65, 39.9, 131) * millimeter, "topCenter" : vector(-22.65, 40.2, 131) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s9", vector(-22.65, 45.8, 131) * millimeter, vector(-22.65, 43.3, 131) * millimeter, 7.375, 5.375);
    fCylinder(context, A0 + "s10", { "bottomCenter" : vector(22.65, 45.5, 131) * millimeter, "topCenter" : vector(22.65, 33, 131) * millimeter, "radius" : 5.375 * millimeter });
    coneOp(context, A0 + "s11", vector(22.65, 33.3, 131) * millimeter, vector(22.65, 35.1, 131) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s12", { "bottomCenter" : vector(22.65, 35.1, 131) * millimeter, "topCenter" : vector(22.65, 35.4, 131) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s13", vector(22.65, 35.7, 131) * millimeter, vector(22.65, 37.5, 131) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s14", { "bottomCenter" : vector(22.65, 37.5, 131) * millimeter, "topCenter" : vector(22.65, 37.8, 131) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s15", vector(22.65, 38.1, 131) * millimeter, vector(22.65, 39.9, 131) * millimeter, 5.375, 5.625);
    fCylinder(context, A0 + "s16", { "bottomCenter" : vector(22.65, 39.9, 131) * millimeter, "topCenter" : vector(22.65, 40.2, 131) * millimeter, "radius" : 5.625 * millimeter });
    coneOp(context, A0 + "s17", vector(22.65, 45.8, 131) * millimeter, vector(22.65, 43.3, 131) * millimeter, 7.375, 5.375);
    fCylinder(context, A0 + "s18", { "bottomCenter" : vector(0, 56.5, 135) * millimeter, "topCenter" : vector(0, 65, 135) * millimeter, "radius" : 8.466 * millimeter });
    prog[] = "Vereinigen Stufe 0";
    unionAll(context, id + "splMixAcr_u0", qUnion([qCreatedBy(A0, EntityType.BODY)]));
    prog[] = "Schnittkörper Stufe 1";
    const C1 = id + "splMixAcr_c1";
    tearOp(context, C1 + "o0", vector(-26.15, 51, 131) * millimeter, vector(26.15, 51, 131) * millimeter, 4);
    tearOp(context, C1 + "o1", vector(-22.65, 32, 131) * millimeter, vector(-22.65, 51, 131) * millimeter, 3.25);
    tearOp(context, C1 + "o2", vector(22.65, 32, 131) * millimeter, vector(22.65, 51, 131) * millimeter, 3.25);
    tearOp(context, C1 + "o3", vector(0, 47.5, 135) * millimeter, vector(0, 66, 135) * millimeter, 5);
    fCuboid(context, C1 + "o4", { "corner1" : vector(-37.35, 41, 117) * millimeter, "corner2" : vector(-33.95, 61, 122) * millimeter });
    fCylinder(context, C1 + "o5", { "bottomCenter" : vector(-35.65, 41, 117) * millimeter, "topCenter" : vector(-35.65, 41, 122) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o6", { "bottomCenter" : vector(-35.65, 61, 117) * millimeter, "topCenter" : vector(-35.65, 61, 122) * millimeter, "radius" : 1.7 * millimeter });
    fCuboid(context, C1 + "o7", { "corner1" : vector(33.95, 41, 117) * millimeter, "corner2" : vector(37.35, 61, 122) * millimeter });
    fCylinder(context, C1 + "o8", { "bottomCenter" : vector(35.65, 41, 117) * millimeter, "topCenter" : vector(35.65, 41, 122) * millimeter, "radius" : 1.7 * millimeter });
    fCylinder(context, C1 + "o9", { "bottomCenter" : vector(35.65, 61, 117) * millimeter, "topCenter" : vector(35.65, 61, 122) * millimeter, "radius" : 1.7 * millimeter });
    prog[] = "Abziehen Stufe 1";
    cutAll(context, id + "splMixAcr_x1", qUnion([qCreatedBy(A0, EntityType.BODY)]), qCreatedBy(C1, EntityType.BODY));
    prog[] = "Name/Farbe";
    finishPart(context, qUnion([qCreatedBy(A0, EntityType.BODY)]), "Sammler Mix Acryl (Gewindestutzen)", color(0.6, 0.4, 0.8));
}

annotation { "Feature Type Name" : "Sammler Mix Acryl" }
export const sammlerMixAcrylV1 = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
    }
    {
        const prog = new box("");
        var failed = [];
        var okCount = 0;
        try
        {
            acr_splMixAcr(context, id, prog);
            okCount += 1;
        }
        catch (error)
        {
            failed = append(failed, "splMixAcr @ " ~ prog[] ~ " -> " ~ toString(error));
        }
        try
        {
            smMaleThread(context, id);
        }
        catch (error)
        {
            failed = append(failed, "Gewinde Stutzen -> " ~ toString(error));
        }
        try
        {
            smHuelse(context, id);
        }
        catch (error)
        {
            failed = append(failed, "Huelse -> " ~ toString(error));
        }
        if (size(failed) > 0)
        {
            var msg = "OK: " ~ toString(okCount) ~ " von 1 Teilen. Fehler: ";
            for (var s in failed)
                msg = msg ~ s ~ " || ";
            reportFeatureWarning(context, id, msg);
        }
    });

