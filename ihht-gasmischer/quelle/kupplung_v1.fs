FeatureScript 3112;
import(path : "onshape/std/geometry.fs", version : "3112.0");

// Steckkupplung NEMA 17 <-> LZQ, final (NEMA Ø5,32 / Fläche 2,22 | LZQ Ø4,5 / Fläche 1,06), 4 Stück. Stehend drucken, NEMA-Seite unten.
// Punkte auf der oberen Stirnseite = Variante K1-K4 (Zeile), Kerben am Rand gegenüber = Stück 1-4 (Spalte).

annotation { "Feature Type Name" : "Kupplung NEMA-LZQ" }
export const kupplungNemaLzq = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
    }
    {
        // K1 Stück 1: NEMA Ø5.32 Fläche 2.22 | LZQ Ø4.5 Fläche 1.06
        fCylinder(context, id + "p0_0" + "body", { "bottomCenter" : vector(0, 0, 0) * millimeter, "topCenter" : vector(0, 0, 20) * millimeter, "radius" : 4.5 * millimeter });
        fCylinder(context, id + "p0_0" + "t" + "bn", { "bottomCenter" : vector(0, 0, -1) * millimeter, "topCenter" : vector(0, 0, 10) * millimeter, "radius" : 2.66 * millimeter });
        fCylinder(context, id + "p0_0" + "t" + "bl", { "bottomCenter" : vector(0, 0, 9.99) * millimeter, "topCenter" : vector(0, 0, 21) * millimeter, "radius" : 2.25 * millimeter });
        fCone(context, id + "p0_0" + "t" + "cn", { "bottomCenter" : vector(0, 0, -0.01) * millimeter, "topCenter" : vector(0, 0, 0.6) * millimeter, "bottomRadius" : 3.26 * millimeter, "topRadius" : 2.66 * millimeter });
        fCone(context, id + "p0_0" + "t" + "cl", { "bottomCenter" : vector(0, 0, 19.4) * millimeter, "topCenter" : vector(0, 0, 20.01) * millimeter, "bottomRadius" : 2.25 * millimeter, "topRadius" : 2.85 * millimeter });
        opBoolean(context, id + "p0_0" + "x", { "tools" : qCreatedBy(id + "p0_0" + "t", EntityType.BODY), "targets" : qCreatedBy(id + "p0_0" + "body", EntityType.BODY), "operationType" : BooleanOperationType.SUBTRACTION });
        fCuboid(context, id + "p0_0" + "k" + "n", { "corner1" : vector(-3, 2.22, 0) * millimeter, "corner2" : vector(3, 3.6, 10) * millimeter });
        fCuboid(context, id + "p0_0" + "k" + "l", { "corner1" : vector(-2.6, 1.06, 10.5) * millimeter, "corner2" : vector(2.6, 3.4, 18.5) * millimeter });
        opBoolean(context, id + "p0_0" + "u", { "tools" : qUnion([qCreatedBy(id + "p0_0" + "body", EntityType.BODY), qCreatedBy(id + "p0_0" + "k", EntityType.BODY)]), "operationType" : BooleanOperationType.UNION });
        fCylinder(context, id + "p0_0" + "g" + "g0", { "bottomCenter" : vector(0, 4.5, 15) * millimeter, "topCenter" : vector(0, 4.5, 21) * millimeter, "radius" : 0.45 * millimeter });
        fCylinder(context, id + "p0_0" + "g" + "d0", { "bottomCenter" : vector(0, -3.45, 19.2) * millimeter, "topCenter" : vector(0, -3.45, 21) * millimeter, "radius" : 0.5 * millimeter });
        opBoolean(context, id + "p0_0" + "gx", { "tools" : qCreatedBy(id + "p0_0" + "g", EntityType.BODY), "targets" : qCreatedBy(id + "p0_0" + "body", EntityType.BODY), "operationType" : BooleanOperationType.SUBTRACTION });
        setProperty(context, { "entities" : qCreatedBy(id + "p0_0" + "body", EntityType.BODY), "propertyType" : PropertyType.NAME, "value" : "K1 Stueck 1" });
        // K1 Stück 2: NEMA Ø5.32 Fläche 2.22 | LZQ Ø4.5 Fläche 1.06
        fCylinder(context, id + "p0_1" + "body", { "bottomCenter" : vector(14, 0, 0) * millimeter, "topCenter" : vector(14, 0, 20) * millimeter, "radius" : 4.5 * millimeter });
        fCylinder(context, id + "p0_1" + "t" + "bn", { "bottomCenter" : vector(14, 0, -1) * millimeter, "topCenter" : vector(14, 0, 10) * millimeter, "radius" : 2.66 * millimeter });
        fCylinder(context, id + "p0_1" + "t" + "bl", { "bottomCenter" : vector(14, 0, 9.99) * millimeter, "topCenter" : vector(14, 0, 21) * millimeter, "radius" : 2.25 * millimeter });
        fCone(context, id + "p0_1" + "t" + "cn", { "bottomCenter" : vector(14, 0, -0.01) * millimeter, "topCenter" : vector(14, 0, 0.6) * millimeter, "bottomRadius" : 3.26 * millimeter, "topRadius" : 2.66 * millimeter });
        fCone(context, id + "p0_1" + "t" + "cl", { "bottomCenter" : vector(14, 0, 19.4) * millimeter, "topCenter" : vector(14, 0, 20.01) * millimeter, "bottomRadius" : 2.25 * millimeter, "topRadius" : 2.85 * millimeter });
        opBoolean(context, id + "p0_1" + "x", { "tools" : qCreatedBy(id + "p0_1" + "t", EntityType.BODY), "targets" : qCreatedBy(id + "p0_1" + "body", EntityType.BODY), "operationType" : BooleanOperationType.SUBTRACTION });
        fCuboid(context, id + "p0_1" + "k" + "n", { "corner1" : vector(11, 2.22, 0) * millimeter, "corner2" : vector(17, 3.6, 10) * millimeter });
        fCuboid(context, id + "p0_1" + "k" + "l", { "corner1" : vector(11.4, 1.06, 10.5) * millimeter, "corner2" : vector(16.6, 3.4, 18.5) * millimeter });
        opBoolean(context, id + "p0_1" + "u", { "tools" : qUnion([qCreatedBy(id + "p0_1" + "body", EntityType.BODY), qCreatedBy(id + "p0_1" + "k", EntityType.BODY)]), "operationType" : BooleanOperationType.UNION });
        fCylinder(context, id + "p0_1" + "g" + "g0", { "bottomCenter" : vector(14.974, 4.3933, 15) * millimeter, "topCenter" : vector(14.974, 4.3933, 21) * millimeter, "radius" : 0.45 * millimeter });
        fCylinder(context, id + "p0_1" + "g" + "g1", { "bottomCenter" : vector(13.026, 4.3933, 15) * millimeter, "topCenter" : vector(13.026, 4.3933, 21) * millimeter, "radius" : 0.45 * millimeter });
        fCylinder(context, id + "p0_1" + "g" + "d0", { "bottomCenter" : vector(14, -3.45, 19.2) * millimeter, "topCenter" : vector(14, -3.45, 21) * millimeter, "radius" : 0.5 * millimeter });
        opBoolean(context, id + "p0_1" + "gx", { "tools" : qCreatedBy(id + "p0_1" + "g", EntityType.BODY), "targets" : qCreatedBy(id + "p0_1" + "body", EntityType.BODY), "operationType" : BooleanOperationType.SUBTRACTION });
        setProperty(context, { "entities" : qCreatedBy(id + "p0_1" + "body", EntityType.BODY), "propertyType" : PropertyType.NAME, "value" : "K1 Stueck 2" });
        // K1 Stück 3: NEMA Ø5.32 Fläche 2.22 | LZQ Ø4.5 Fläche 1.06
        fCylinder(context, id + "p0_2" + "body", { "bottomCenter" : vector(28, 0, 0) * millimeter, "topCenter" : vector(28, 0, 20) * millimeter, "radius" : 4.5 * millimeter });
        fCylinder(context, id + "p0_2" + "t" + "bn", { "bottomCenter" : vector(28, 0, -1) * millimeter, "topCenter" : vector(28, 0, 10) * millimeter, "radius" : 2.66 * millimeter });
        fCylinder(context, id + "p0_2" + "t" + "bl", { "bottomCenter" : vector(28, 0, 9.99) * millimeter, "topCenter" : vector(28, 0, 21) * millimeter, "radius" : 2.25 * millimeter });
        fCone(context, id + "p0_2" + "t" + "cn", { "bottomCenter" : vector(28, 0, -0.01) * millimeter, "topCenter" : vector(28, 0, 0.6) * millimeter, "bottomRadius" : 3.26 * millimeter, "topRadius" : 2.66 * millimeter });
        fCone(context, id + "p0_2" + "t" + "cl", { "bottomCenter" : vector(28, 0, 19.4) * millimeter, "topCenter" : vector(28, 0, 20.01) * millimeter, "bottomRadius" : 2.25 * millimeter, "topRadius" : 2.85 * millimeter });
        opBoolean(context, id + "p0_2" + "x", { "tools" : qCreatedBy(id + "p0_2" + "t", EntityType.BODY), "targets" : qCreatedBy(id + "p0_2" + "body", EntityType.BODY), "operationType" : BooleanOperationType.SUBTRACTION });
        fCuboid(context, id + "p0_2" + "k" + "n", { "corner1" : vector(25, 2.22, 0) * millimeter, "corner2" : vector(31, 3.6, 10) * millimeter });
        fCuboid(context, id + "p0_2" + "k" + "l", { "corner1" : vector(25.4, 1.06, 10.5) * millimeter, "corner2" : vector(30.6, 3.4, 18.5) * millimeter });
        opBoolean(context, id + "p0_2" + "u", { "tools" : qUnion([qCreatedBy(id + "p0_2" + "body", EntityType.BODY), qCreatedBy(id + "p0_2" + "k", EntityType.BODY)]), "operationType" : BooleanOperationType.UNION });
        fCylinder(context, id + "p0_2" + "g" + "g0", { "bottomCenter" : vector(29.9018, 4.0784, 15) * millimeter, "topCenter" : vector(29.9018, 4.0784, 21) * millimeter, "radius" : 0.45 * millimeter });
        fCylinder(context, id + "p0_2" + "g" + "g1", { "bottomCenter" : vector(28, 4.5, 15) * millimeter, "topCenter" : vector(28, 4.5, 21) * millimeter, "radius" : 0.45 * millimeter });
        fCylinder(context, id + "p0_2" + "g" + "g2", { "bottomCenter" : vector(26.0982, 4.0784, 15) * millimeter, "topCenter" : vector(26.0982, 4.0784, 21) * millimeter, "radius" : 0.45 * millimeter });
        fCylinder(context, id + "p0_2" + "g" + "d0", { "bottomCenter" : vector(28, -3.45, 19.2) * millimeter, "topCenter" : vector(28, -3.45, 21) * millimeter, "radius" : 0.5 * millimeter });
        opBoolean(context, id + "p0_2" + "gx", { "tools" : qCreatedBy(id + "p0_2" + "g", EntityType.BODY), "targets" : qCreatedBy(id + "p0_2" + "body", EntityType.BODY), "operationType" : BooleanOperationType.SUBTRACTION });
        setProperty(context, { "entities" : qCreatedBy(id + "p0_2" + "body", EntityType.BODY), "propertyType" : PropertyType.NAME, "value" : "K1 Stueck 3" });
        // K1 Stück 4: NEMA Ø5.32 Fläche 2.22 | LZQ Ø4.5 Fläche 1.06
        fCylinder(context, id + "p0_3" + "body", { "bottomCenter" : vector(42, 0, 0) * millimeter, "topCenter" : vector(42, 0, 20) * millimeter, "radius" : 4.5 * millimeter });
        fCylinder(context, id + "p0_3" + "t" + "bn", { "bottomCenter" : vector(42, 0, -1) * millimeter, "topCenter" : vector(42, 0, 10) * millimeter, "radius" : 2.66 * millimeter });
        fCylinder(context, id + "p0_3" + "t" + "bl", { "bottomCenter" : vector(42, 0, 9.99) * millimeter, "topCenter" : vector(42, 0, 21) * millimeter, "radius" : 2.25 * millimeter });
        fCone(context, id + "p0_3" + "t" + "cn", { "bottomCenter" : vector(42, 0, -0.01) * millimeter, "topCenter" : vector(42, 0, 0.6) * millimeter, "bottomRadius" : 3.26 * millimeter, "topRadius" : 2.66 * millimeter });
        fCone(context, id + "p0_3" + "t" + "cl", { "bottomCenter" : vector(42, 0, 19.4) * millimeter, "topCenter" : vector(42, 0, 20.01) * millimeter, "bottomRadius" : 2.25 * millimeter, "topRadius" : 2.85 * millimeter });
        opBoolean(context, id + "p0_3" + "x", { "tools" : qCreatedBy(id + "p0_3" + "t", EntityType.BODY), "targets" : qCreatedBy(id + "p0_3" + "body", EntityType.BODY), "operationType" : BooleanOperationType.SUBTRACTION });
        fCuboid(context, id + "p0_3" + "k" + "n", { "corner1" : vector(39, 2.22, 0) * millimeter, "corner2" : vector(45, 3.6, 10) * millimeter });
        fCuboid(context, id + "p0_3" + "k" + "l", { "corner1" : vector(39.4, 1.06, 10.5) * millimeter, "corner2" : vector(44.6, 3.4, 18.5) * millimeter });
        opBoolean(context, id + "p0_3" + "u", { "tools" : qUnion([qCreatedBy(id + "p0_3" + "body", EntityType.BODY), qCreatedBy(id + "p0_3" + "k", EntityType.BODY)]), "operationType" : BooleanOperationType.UNION });
        fCylinder(context, id + "p0_3" + "g" + "g0", { "bottomCenter" : vector(44.7394, 3.5701, 15) * millimeter, "topCenter" : vector(44.7394, 3.5701, 21) * millimeter, "radius" : 0.45 * millimeter });
        fCylinder(context, id + "p0_3" + "g" + "g1", { "bottomCenter" : vector(42.974, 4.3933, 15) * millimeter, "topCenter" : vector(42.974, 4.3933, 21) * millimeter, "radius" : 0.45 * millimeter });
        fCylinder(context, id + "p0_3" + "g" + "g2", { "bottomCenter" : vector(41.026, 4.3933, 15) * millimeter, "topCenter" : vector(41.026, 4.3933, 21) * millimeter, "radius" : 0.45 * millimeter });
        fCylinder(context, id + "p0_3" + "g" + "g3", { "bottomCenter" : vector(39.2606, 3.5701, 15) * millimeter, "topCenter" : vector(39.2606, 3.5701, 21) * millimeter, "radius" : 0.45 * millimeter });
        fCylinder(context, id + "p0_3" + "g" + "d0", { "bottomCenter" : vector(42, -3.45, 19.2) * millimeter, "topCenter" : vector(42, -3.45, 21) * millimeter, "radius" : 0.5 * millimeter });
        opBoolean(context, id + "p0_3" + "gx", { "tools" : qCreatedBy(id + "p0_3" + "g", EntityType.BODY), "targets" : qCreatedBy(id + "p0_3" + "body", EntityType.BODY), "operationType" : BooleanOperationType.SUBTRACTION });
        setProperty(context, { "entities" : qCreatedBy(id + "p0_3" + "body", EntityType.BODY), "propertyType" : PropertyType.NAME, "value" : "K1 Stueck 4" });
    });
