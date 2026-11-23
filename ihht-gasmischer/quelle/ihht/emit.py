# FeatureScript-Ausgabe: gemeinsamer Kopf (Hilfsfunktionen) + je Teil eine Funktion + ein Feature je Modul.
def f(v): return ('%.3f' % v).rstrip('0').rstrip('.') if abs(v) > 1e-9 else '0'
def vec(p): return 'vector(%s, %s, %s) * millimeter' % (f(p[0]), f(p[1]), f(p[2]))
def emit_op(o, idexpr):
    if o[0] == 'box':
        _, x0, x1, y0, y1, z0, z1 = o
        return 'fCuboid(context, %s, { "corner1" : %s, "corner2" : %s });' % (idexpr, vec((x0, y0, z0)), vec((x1, y1, z1)))
    _, p0, p1, d = o
    return 'fCylinder(context, %s, { "bottomCenter" : %s, "topCenter" : %s, "radius" : %s * millimeter });' % (idexpr, vec(p0), vec(p1), f(d / 2))

def emit_any(o, idexpr):
    if o[0] == 'tear':
        _, p0, p1, d = o
        return 'tearOp(context, %s, vector(%s, %s, %s) * millimeter, vector(%s, %s, %s) * millimeter, %s);' % (
            idexpr, f(p0[0]), f(p0[1]), f(p0[2]), f(p1[0]), f(p1[1]), f(p1[2]), f(d / 2))
    if o[0] == 'prism':
        _, c, pts, y0, y1 = o
        return 'prismOp(context, %s, vector(%s, %s, %s) * millimeter, [%s], %s);' % (
            idexpr, f(c[0]), f(y0), f(c[1]), ', '.join('[%s, %s]' % (f(a), f(b)) for a, b in pts), f(y1 - y0))
    if o[0] == 'prismx':
        _, c, pts, x0, x1 = o
        return 'prismXOp(context, %s, vector(%s, %s, %s) * millimeter, [%s], %s);' % (
            idexpr, f(x0), f(c[0]), f(c[1]), ', '.join('[%s, %s]' % (f(a), f(b)) for a, b in pts), f(x1 - x0))
    if o[0] == 'cone':
        _, p0, p1, r0, r1 = o
        return 'coneOp(context, %s, vector(%s, %s, %s) * millimeter, vector(%s, %s, %s) * millimeter, %s, %s);' % (
            idexpr, f(p0[0]), f(p0[1]), f(p0[2]), f(p1[0]), f(p1[1]), f(p1[2]), f(r0), f(r1))
    return emit_op(o, idexpr)
def emit_part(L, p, fn):
    L.append('function %s(context is Context, id is Id, prog is box)' % fn)
    L.append('{')
    adds = []
    tq = lambda: 'qUnion([' + ', '.join('qCreatedBy(%s, EntityType.BODY)' % a for a in adds) + '])'
    for si, (kind, ops) in enumerate(p.stages):
        if kind == 'add':
            L.append('    prog[] = "Körper Stufe %d";' % si)
            L.append('    const A%d = id + "%s_a%d";' % (si, p.name, si))
            adds.append('A%d' % si)
            for j, o in enumerate(ops):
                L.append('    ' + emit_any(o, 'A%d + "s%d"' % (si, j)))
            L.append('    prog[] = "Vereinigen Stufe %d";' % si)
            L.append('    unionAll(context, id + "%s_u%d", %s);' % (p.name, si, tq()))
        else:
            L.append('    prog[] = "Schnittkörper Stufe %d";' % si)
            L.append('    const C%d = id + "%s_c%d";' % (si, p.name, si))
            for j, o in enumerate(ops):
                L.append('    ' + emit_any(o, 'C%d + "o%d"' % (si, j)))
            L.append('    prog[] = "Abziehen Stufe %d";' % si)
            L.append('    cutAll(context, id + "%s_x%d", %s, qCreatedBy(C%d, EntityType.BODY));' % (p.name, si, tq(), si))
    c = p.col
    L.append('    prog[] = "Name/Farbe";')
    L.append('    finishPart(context, %s, "%s", color(%s, %s, %s));' % (tq(), p.label, f(c[0]), f(c[1]), f(c[2])))
    L.append('}')
    L.append('')
def emit_feature(L, plist, fnprefix, title, var, nach_teilen=()):
    names = []
    for p in plist:
        fn = fnprefix + p.name
        emit_part(L, p, fn)
        names.append((fn, p))
    L.append('annotation { "Feature Type Name" : "%s" }' % title)
    L.append('export const %s = defineFeature(function(context is Context, id is Id, definition is map)' % var)
    L.append('    precondition')
    L.append('    {')
    L.append('    }')
    L.append('    {')
    L.append('        const prog = new box("");')
    L.append('        var failed = [];')
    L.append('        var okCount = 0;')
    for fn, p in names:
        L.append('        try')
        L.append('        {')
        L.append('            %s(context, id, prog);' % fn)
        L.append('            okCount += 1;')
        L.append('        }')
        L.append('        catch (error)')
        L.append('        {')
        L.append('            failed = append(failed, "%s @ " ~ prog[] ~ " -> " ~ toString(error));' % p.name)
        L.append('        }')
    L += list(nach_teilen)
    L.append('        if (size(failed) > 0)')
    L.append('        {')
    L.append('            var msg = "OK: " ~ toString(okCount) ~ " von %d Teilen. Fehler: ";' % len(names))
    L.append('            for (var s in failed)')
    L.append('                msg = msg ~ s ~ " || ";')
    L.append('            reportFeatureWarning(context, id, msg);')
    L.append('        }')
    L.append('    });')
    L.append('')
KOPF = []
KOPF += ['FeatureScript 3112;', 'import(path : "onshape/std/geometry.fs", version : "3112.0");', '',
      '// IHHT-Gasmischer – v5 Entwicklungs-Setup: 2 NEMA, Ventilpaare über Zahnräder gekoppelt, Kameraportal, Rahmen 250 mm (generiert). Änderungen bitte per Chat.',
      '// Koordinaten: X = links/rechts, Y = vorne(-)/hinten(+), Z = oben. Y=0: Vorderseite LZQ-Montagewand, Z=0: Unterseite Boden.', '']
KOPF += ['function unionAll(context is Context, opId is Id, q is Query)', '{',
      '    const bodies = qBodyType(q, BodyType.SOLID);',
      '    if (size(evaluateQuery(context, bodies)) > 1)',
      '        opBoolean(context, opId, { "tools" : bodies, "operationType" : BooleanOperationType.UNION });', '}', '',
      'function cutAll(context is Context, opId is Id, targets is Query, tools is Query)', '{',
      '    if (size(evaluateQuery(context, tools)) > 0)',
      '        opBoolean(context, opId, { "tools" : tools, "targets" : qBodyType(targets, BodyType.SOLID), "operationType" : BooleanOperationType.SUBTRACTION });', '}', '',
      'function coneOp(context is Context, id is Id, p0 is Vector, p1 is Vector, r0 is number, r1 is number)', '{',
      '    // Kegelstumpf: Profil skizzieren und um die Achse rotieren; Fallback: gestufte Zylinder',
      '    const axisDir = normalize(p1 - p0);',
      '    const len = norm(p1 - p0) / millimeter;',
      '    var refDir = vector(0, 0, 1);',
      '    if (abs(dot(axisDir, refDir)) > 0.9)',
      '        refDir = vector(1, 0, 0);',
      '    const xd = normalize(cross(axisDir, refDir));',
      '    try',
      '    {',
      '        const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(p0, cross(axisDir, xd), xd) });',
      '        skLineSegment(sk, "l0", { "start" : vector(0, 0) * millimeter, "end" : vector(r0, 0) * millimeter });',
      '        skLineSegment(sk, "l1", { "start" : vector(r0, 0) * millimeter, "end" : vector(r1, -len) * millimeter });',
      '        skLineSegment(sk, "l2", { "start" : vector(r1, -len) * millimeter, "end" : vector(0, -len) * millimeter });',
      '        skLineSegment(sk, "l3", { "start" : vector(0, -len) * millimeter, "end" : vector(0, 0) * millimeter });',
      '        skSolve(sk);',
      '        opRevolve(context, id + "rev", { "entities" : qSketchRegion(id + "sk"), "axis" : line(p0, axisDir), "angleForward" : 2 * PI * radian });',
      '        opDeleteBodies(context, id + "del", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });',
      '    }',
      '    catch (e)',
      '    {',
      '        try silent',
      '        {',
      '            opDeleteBodies(context, id + "del2", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });',
      '        }',
      '        for (var k = 0; k < 8; k += 1)',
      '        {',
      '            fCylinder(context, id + ("fb" ~ toString(k)), { "bottomCenter" : p0 + (p1 - p0) * (k / 8), "topCenter" : p0 + (p1 - p0) * ((k + 1) / 8), "radius" : (r0 + (r1 - r0) * (k + 0.5) / 8) * millimeter });',
      '        }',
      '    }',
      '}', '',
      'function finishPart(context is Context, q is Query, label is string, c is Color)', '{',
      '    const b = qBodyType(q, BodyType.SOLID);',
      '    setProperty(context, { "entities" : b, "propertyType" : PropertyType.NAME, "value" : label });',
      '    setProperty(context, { "entities" : b, "propertyType" : PropertyType.APPEARANCE, "value" : c });', '}', '']
KOPF += ['function tearOp(context is Context, id is Id, p0 is Vector, p1 is Vector, r is number)', '{',
      '    // Waagerechter Kanal mit vollem Tropfenprofil (45°-Flanken bis zur Spitze, kein Dach) -> druckt ohne Support',
      '    fCylinder(context, id + "cyl", { "bottomCenter" : p0, "topCenter" : p1, "radius" : r * millimeter });',
      '    const n = normalize(p1 - p0);',
      '    const len = norm(p1 - p0);',
      '    const xd = normalize(cross(vector(0, 0, 1), n));',
      '    const s2 = r / sqrt(2);',
      '    const w = r * (sqrt(2) - 1);',
      '    try',
      '    {',
      '        const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(p0, n, xd) });',
      '        skLineSegment(sk, "a", { "start" : vector(0, 0) * millimeter, "end" : vector(-s2, s2) * millimeter });',
      '        skLineSegment(sk, "b", { "start" : vector(-s2, s2) * millimeter, "end" : vector(0, r * sqrt(2)) * millimeter });',
      '        skLineSegment(sk, "d", { "start" : vector(0, r * sqrt(2)) * millimeter, "end" : vector(s2, s2) * millimeter });',
      '        skLineSegment(sk, "e", { "start" : vector(s2, s2) * millimeter, "end" : vector(0, 0) * millimeter });',
      '        skSolve(sk);',
      '        opExtrude(context, id + "ext", { "entities" : qSketchRegion(id + "sk"), "direction" : n, "endBound" : BoundingType.BLIND, "endDepth" : len });',
      '        opDeleteBodies(context, id + "del", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });',
      '    }',
      '    catch (e)',
      '    {',
      '        // Fallback: nur runder Kanal',
      '    }',
      '}', '']
KOPF += ['function prismOp(context is Context, id is Id, origin is Vector, pts is array, depth is number)', '{',
      '    // Polygon [dx, dz] in der XZ-Ebene durch origin, Extrusion um depth in +Y (Zahnradprofil)',
      '    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(origin, vector(0, 1, 0), vector(1, 0, 0)) });   // Skizzen-y zeigt nach -Z',
      '    const n = size(pts);',
      '    for (var i = 0; i < n; i += 1)',
      '    {',
      '        const a = pts[i];',
      '        const b = pts[(i + 1) % n];',
      '        skLineSegment(sk, "l" ~ i, { "start" : vector(a[0], -a[1]) * millimeter, "end" : vector(b[0], -b[1]) * millimeter });',
      '    }',
      '    skSolve(sk);',
      '    opExtrude(context, id + "ext", { "entities" : qSketchRegion(id + "sk"), "direction" : vector(0, 1, 0), "endBound" : BoundingType.BLIND, "endDepth" : depth * millimeter });',
      '    opDeleteBodies(context, id + "del", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });',
      '}', '']
KOPF += ['function prismXOp(context is Context, id is Id, origin is Vector, pts is array, depth is number)', '{',
      '    // Polygon [dy, dz] in der YZ-Ebene durch origin, Extrusion um depth in +X (Knotenbleche)',
      '    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(origin, vector(1, 0, 0), vector(0, 1, 0)) });   // Skizzen-y zeigt nach +Z',
      '    const n = size(pts);',
      '    for (var i = 0; i < n; i += 1)',
      '    {',
      '        const a = pts[i];',
      '        const b = pts[(i + 1) % n];',
      '        skLineSegment(sk, "l" ~ i, { "start" : vector(a[0], a[1]) * millimeter, "end" : vector(b[0], b[1]) * millimeter });',
      '    }',
      '    skSolve(sk);',
      '    opExtrude(context, id + "ext", { "entities" : qSketchRegion(id + "sk"), "direction" : vector(1, 0, 0), "endBound" : BoundingType.BLIND, "endDepth" : depth * millimeter });',
      '    opDeleteBodies(context, id + "del", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });',
      '}', '']

def fs_text(titel, var, prefix, teile, kommentar=None, extra=(), nach_teilen=()):
    L = list(KOPF)
    if kommentar: L.insert(2, kommentar)
    L += list(extra)
    emit_feature(L, teile, prefix, titel, var, nach_teilen)
    return '\n'.join(L) + '\n'
