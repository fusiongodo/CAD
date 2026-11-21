FeatureScript 3112;
import(path : "onshape/std/geometry.fs", version : "3112.0");

// =====================================================================
// Exzenter-Quetschventil v1 (Doppel-Quetschventil, Vorbild MakAir / Cole)
//
// Ansicht von vorn (Blick gegen -Z): Schlaeuche laufen waagerecht (X), einer
// unten auf dem unteren Amboss, einer oben unter dem oberen Amboss.
// Dazwischen zwei Kugellager 6203-2RS (40 x 17 x 12) nebeneinander auf einem
// Exzenterzapfen, der direkt auf der NEMA-17-Welle sitzt (Achse Z).
// Zapfen ganz unten -> unterer Schlauch auf "Quetschhoehe" zu, oberer offen
// (Spalt = Quetschhoehe + 2 x Exzentrizitaet); ganz oben umgekehrt.
// Standard: Quetschhoehe 5, Exzentrizitaet 3,5 -> zu 5 / offen 12 (= Schlauch aussen),
// Neutralstellung beide 8,5 = halber Weg zwischen zu und offen. Ambossabstand 40 + 2 x 8,5 = 57.
// Summe beider Spalte ist immer konstant (2 x Neutralspalt) und muss >= zu + offen sein.
//
// Teile (in Einbaulage, Exzenter in Neutralstellung bei +X):
//   Rahmen       Standard: EIN Rahmen, Quetschhoehe 5 (= 1 mm Ueberpressung bei Schlauch 6/12,
//                offener Spalt 12). Optional Variantenreihe ("Anzahl Rahmen" > 1, je Schritt
//                kleiner, Kennung 1..n Punkte vorn rechts; Steg bekommt dann Langloecher). Je Variante EIN Teil: Platte mit NEMA-17-Aufnahme (Pilot 22, 4 x M3 31 mm,
//                versenkt) + unterer und oberer Amboss, Fuehrungsstege fuer den
//                Schlauch ausserhalb der Lagerbahn, je Amboss 2 Kabelbinder-
//                Schlitzpaare (4 x 1,8 mm), 4 x M3 zur Befestigung.
//                Schlaeuche von vorn einlegen, dann Exzenter + Lager, dann Steg.
//                Drucken: Rueckseite (Motorseite) unten.
//   Steg vorn    6 mm Platte mit Fenster und Langloechern (passt auf alle Rahmen), verbindet die Ambosse vorn, damit sie
//                nicht auseinanderfedern. 4 x M3 x 12 in die Amboss-Stirnseiten.
//                Drucken: flach.
//   Exzenter     D-Bohrung fuer die NEMA-Welle nach Konsens (ihht-pi CAD/passungen, 21.11.2026): Grundbohrung
//                LOCKER Ø 5,50 / Abflachung 2,25 + genau 2 duenne Quetschstege 0,45 / 0,2 (eine Schicht) auf der Abflachung,
//                als Variantenreihe im Zapfen-Ø (Kennung 1..n Punkte auf der Unterseite);
//                Variante 1 sitzt in Einbaulage, die weiteren liegen darunter.
//                Schulter fuer den Lager-Innenring, Zapfen fuer 2 Lager,
//                M3 (selbstschneidend) mittig im Zapfenende.
//                Drucken: Schulter unten.
//   Griffscheibe haelt die Lager (M3x12 in den Zapfen, 2 Mitnehmerstifte), Knauf ueber dem Steg zum Drehen von Hand.
//   Exzenter     Reihe in der Exzentrizitaet (Standard 3,25 / 3,0 / 2,75 = Ueberpressung 0,75 / 0,5 / 0,25 auf Rahmen h 5).
//   Lager        Dummies, nicht drucken.
//
// Drehmoment (Abschaetzung): Hebelarm beim Schliessen = e x cos(Winkel);
// bei 3,5 mm Exzentrizitaet und 5 mm Quetschhoehe entsprechen 17 Ncm etwa
// 49 N (Spalt 8), 69 N (Spalt 6), 94 N (Spalt 5,5). Am Totpunkt null.
// =====================================================================

const E_BOUNDS = { (meter) : [0.001, 0.0035, 0.010], (millimeter) : 3.5, (inch) : 0.14 } as LengthBoundSpec;
const H_BOUNDS = { (meter) : [0.002, 0.005, 0.012], (millimeter) : 5.0, (inch) : 0.2 } as LengthBoundSpec;
const HSTEP_BOUNDS = { (meter) : [0, 0.0005, 0.003], (millimeter) : 0.5, (inch) : 0.02 } as LengthBoundSpec;
const BD_BOUNDS = { (meter) : [0.010, 0.040, 0.080], (millimeter) : 40, (inch) : 1.6 } as LengthBoundSpec;
const BW_BOUNDS = { (meter) : [0.004, 0.012, 0.030], (millimeter) : 12, (inch) : 0.47 } as LengthBoundSpec;
const PIN_BOUNDS = { (meter) : [0.005, 0.0169, 0.030], (millimeter) : 16.9, (inch) : 0.665 } as LengthBoundSpec;
const STEP_BOUNDS = { (meter) : [0, 0, 0.002], (millimeter) : 0, (inch) : 0 } as LengthBoundSpec;
const COUNT_BOUNDS = { (unitless) : [1, 4, 10] } as IntegerBoundSpec;
const FCOUNT_BOUNDS = { (unitless) : [1, 1, 10] } as IntegerBoundSpec;
const EE_BOUNDS = { (meter) : [0.001, 0.00325, 0.010], (millimeter) : 3.25, (inch) : 0.128 } as LengthBoundSpec;
const ECOUNT_BOUNDS = { (unitless) : [1, 3, 10] } as IntegerBoundSpec;
const ESTEP_BOUNDS = { (meter) : [0, 0.00025, 0.002], (millimeter) : 0.25, (inch) : 0.01 } as LengthBoundSpec;
const RIB_BOUNDS = { (meter) : [0, 0.00045, 0.001], (millimeter) : 0.45, (inch) : 0.018 } as LengthBoundSpec;
const TOD_BOUNDS = { (meter) : [0.004, 0.012, 0.030], (millimeter) : 12, (inch) : 0.47 } as LengthBoundSpec;
const TID_BOUNDS = { (meter) : [0.001, 0.006, 0.025], (millimeter) : 6, (inch) : 0.24 } as LengthBoundSpec;
const ND_BOUNDS = { (meter) : [0.004, 0.0055, 0.007], (millimeter) : 5.5, (inch) : 0.217 } as LengthBoundSpec;
const NF_BOUNDS = { (meter) : [0.001, 0.00225, 0.003], (millimeter) : 2.25, (inch) : 0.089 } as LengthBoundSpec;

const M3_CLR = 3.4;
const M3_SELF = 2.6;

function mmv(x is number, y is number, z is number) returns Vector
{
    return vector(x, y, z) * millimeter;
}

function bx(context is Context, id is Id, x0 is number, x1 is number, y0 is number, y1 is number, z0 is number, z1 is number)
{
    fCuboid(context, id, { "corner1" : mmv(min(x0, x1), min(y0, y1), min(z0, z1)),
                "corner2" : mmv(max(x0, x1), max(y0, y1), max(z0, z1)) });
}

function cz(context is Context, id is Id, x is number, y is number, z0 is number, z1 is number, d is number)
{
    fCylinder(context, id, { "bottomCenter" : mmv(x, y, z0), "topCenter" : mmv(x, y, z1), "radius" : d / 2 * millimeter });
}

function kz(context is Context, id is Id, x is number, y is number, z0 is number, z1 is number, r0 is number, r1 is number)
{
    fCone(context, id, { "bottomCenter" : mmv(x, y, z0), "topCenter" : mmv(x, y, z1),
                "bottomRadius" : r0 * millimeter, "topRadius" : r1 * millimeter });
}

// Alle Koerper unter id + "a" vereinigen, alle unter id + "c" abziehen, benennen.
function finish(context is Context, id is Id, name is string, hasCuts is boolean)
{
    const body = qCreatedBy(id + "a", EntityType.BODY);
    if (size(evaluateQuery(context, body)) > 1)
        opBoolean(context, id + "u", { "tools" : body, "operationType" : BooleanOperationType.UNION });
    if (hasCuts)
    {
        opBoolean(context, id + "x", {
                    "tools" : qCreatedBy(id + "c", EntityType.BODY),
                    "targets" : body,
                    "operationType" : BooleanOperationType.SUBTRACTION
                });
    }
    setProperty(context, { "entities" : body, "propertyType" : PropertyType.NAME, "value" : name });
}

// Exzenter: Nabe + Schulter + Zapfen, D-Bohrung mit genau 2 duennen Quetschstegen auf der Abflachung.
// Druck: Unterseite (z0) auf dem Bett; Stege je eine 0,2-mm-Schicht: 1,0 mm ueber dem Bett und kurz unter der Wellenspitze.
function buildEccentric(context is Context, p is Id, xo is number, yo is number, e is number, pinD is number,
    nD is number, nF is number, ribT is number, zB0 is number, zB1 is number, zTip is number, marks is number, name is string)
{
    const z0 = 0.8;
    cz(context, p + "a" + "hub", xo, yo, z0, zB0, 12);
    cz(context, p + "a" + "collar", xo + e, yo, z0, zB0, 22);
    cz(context, p + "a" + "pin", xo + e, yo, zB0 - 0.01, zB1 - 0.7, pinD);
    kz(context, p + "a" + "pinTop", xo + e, yo, zB1 - 0.71, zB1 - 0.2, pinD / 2, pinD / 2 - 0.5);
    for (var k = 0; k < 2; k += 1)                                       // Mitnehmerstifte fuer die Griffscheibe
        cz(context, p + "a" + ("peg" ~ k), xo + e, yo + (k == 0 ? 5.5 : -5.5), zB1 - 0.7, zB1 + 2, 2.8);
    // D-Bohrung: Zylinder minus Abflachung (Abflachung auf -X = Seite der duennen Wand)
    const t = p + "tool";
    cz(context, t + "bore", xo, yo, z0 - 1, zTip + 1, nD);
    bx(context, t + "flat", xo - nD, xo - nF, yo - nD, yo + nD, z0 - 2, zTip + 2);
    opBoolean(context, t + "sub", {
                "tools" : qCreatedBy(t + "flat", EntityType.BODY),
                "targets" : qCreatedBy(t + "bore", EntityType.BODY),
                "operationType" : BooleanOperationType.SUBTRACTION
            });
    opBoolean(context, p + "boreCut", {
                "tools" : qCreatedBy(t + "bore", EntityType.BODY),
                "targets" : qCreatedBy(p + "a", EntityType.BODY),
                "operationType" : BooleanOperationType.SUBTRACTION
            });
    // Genau 2 Quetschstege auf der Abflachung, je eine Schicht (0,2): unten und kurz unter der Wellenspitze
    if (ribT > 0)
    {
        const zTop = z0 + floor((zTip - 0.7 - z0) / 0.2) * 0.2;
        for (var zr in [z0 + 1.0, zTop])
            bx(context, p + "rib" + ("r" ~ round(zr * 10)), xo - nD / 2 - 0.3, xo - (nF - ribT), yo - nD / 2 - 0.3, yo + nD / 2 + 0.3, zr, zr + 0.2);
    }
    const body = qUnion([qCreatedBy(p + "a", EntityType.BODY), qCreatedBy(p + "rib", EntityType.BODY)]);
    opBoolean(context, p + "u", { "tools" : body, "operationType" : BooleanOperationType.UNION });
    kz(context, p + "c" + "roof", xo, yo, zTip + 0.99, zTip + 1 + nD / 2 - 0.6, nD / 2, 0.6);   // Kegeldach statt Bruecke
    kz(context, p + "c" + "foot", xo, yo, z0 - 0.01, z0 + 0.6, nD / 2 + 0.6, nD / 2);          // Fase gegen Elefantenfuss
    cz(context, p + "c" + "m3", xo + e, yo, zB1 - 11, zB1 + 1, M3_SELF);
    for (var k = 0; k < marks; k += 1)                                                       // Kennung: Punkte auf der Unterseite
    {
        const a = (-60 + k * 30) * degree;
        cz(context, p + "c" + ("mk" ~ k), xo + 4.4 * cos(a), yo + 4.4 * sin(a), z0 - 1, z0 + 0.6, 0.9);
    }
    opBoolean(context, p + "x", {
                "tools" : qCreatedBy(p + "c", EntityType.BODY),
                "targets" : body,
                "operationType" : BooleanOperationType.SUBTRACTION
            });
    setProperty(context, { "entities" : body, "propertyType" : PropertyType.NAME, "value" : name });
}

annotation { "Feature Type Name" : "Exzenter-Quetschventil v1" }
export const exzenterQuetschventilV1 = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Exzentrizitaet Rahmen (Auslegung Ambossabstand)" }
        isLength(definition.ecc, E_BOUNDS);

        annotation { "Name" : "Quetschhoehe (Spalt zu, 5 = 1 mm Ueberpressung)" }
        isLength(definition.hMin, H_BOUNDS);

        annotation { "Name" : "Quetschhoehe Schritt (nur bei mehreren Rahmen)" }
        isLength(definition.hStep, HSTEP_BOUNDS);

        annotation { "Name" : "Anzahl Rahmen (1 = nur ein Rahmen)" }
        isInteger(definition.hCount, FCOUNT_BOUNDS);

        annotation { "Name" : "Lager aussen D" }
        isLength(definition.bearingD, BD_BOUNDS);

        annotation { "Name" : "Lager Breite (je Lager)" }
        isLength(definition.bearingW, BW_BOUNDS);

        annotation { "Name" : "Exzentrizitaet Exzenter (kleiner = weniger Ueberpressung + Drehmoment)" }
        isLength(definition.eccE, EE_BOUNDS);

        annotation { "Name" : "Exzentrizitaet Schritt (je weiterer Exzenter kleiner)" }
        isLength(definition.eccStep, ESTEP_BOUNDS);

        annotation { "Name" : "Zapfen D (Lager 17)" }
        isLength(definition.pinD, PIN_BOUNDS);

        annotation { "Name" : "Zapfen D Schritt (0 = alle gleich)" }
        isLength(definition.pinStep, STEP_BOUNDS);

        annotation { "Name" : "Anzahl Exzenter" }
        isInteger(definition.pinCount, ECOUNT_BOUNDS);

        annotation { "Name" : "Schlauch aussen D" }
        isLength(definition.tubeOD, TOD_BOUNDS);

        annotation { "Name" : "Schlauch innen D" }
        isLength(definition.tubeID, TID_BOUNDS);

        annotation { "Name" : "NEMA-Welle D" }
        isLength(definition.nemaD, ND_BOUNDS);

        annotation { "Name" : "NEMA-Abflachung (Abstand zur Achse)" }
        isLength(definition.nemaFlat, NF_BOUNDS);

        annotation { "Name" : "NEMA-Quetschsteg Tiefe (2 Stege, 1 Schicht; 0 = glatt)" }
        isLength(definition.ribT, RIB_BOUNDS);

        annotation { "Name" : "Lager-Dummies zeigen", "Default" : true }
        definition.dummies is boolean;
    }
    {
        const e = definition.ecc / millimeter;
        const h = definition.hMin / millimeter;
        const R = definition.bearingD / millimeter / 2;
        const bw = definition.bearingW / millimeter;
        const pinD = definition.pinD / millimeter;
        const tOD = definition.tubeOD / millimeter;
        const tID = definition.tubeID / millimeter;
        const nD = definition.nemaD / millimeter;
        const nF = definition.nemaFlat / millimeter;
        const ribT = definition.ribT / millimeter;

        const hStep = definition.hStep / millimeter;
        const nFrames = definition.hCount;
        const hLow = h - (nFrames - 1) * hStep;     // kleinste Quetschhoehe (letzte Variante)
        if (hLow < 1)
            throw regenError("Kleinste Quetschhoehe < 1 mm: Schritt oder Anzahl verringern");
        if (2 * e + h < tOD)
            throw regenError("Offener Spalt bei Rahmen V1 (Quetschhoehe + 2 x Exzentrizitaet) kleiner als Schlauch aussen D");
        if (e + nD / 2 > pinD / 2 - 1.2)
            throw regenError("Exzentrizitaet zu gross: NEMA-Bohrung ragt aus dem Zapfen");

        // ---------- abgeleitete Masse ----------
        const PT = 6.0;                      // Rahmenplatte (z -PT .. 0)
        const TA = 8.0;                      // Ambossdicke
        const yA = R + e + h;                // Ambossflaeche bei Rahmen V1 (groesster Abstand)
        const yALow = R + e + hLow;          // Ambossflaeche bei der letzten Variante
        const zB0 = 3.0;                     // Lager ab hier (Schulter 0,8 .. 3)
        const zB1 = zB0 + 2 * bw;
        const zc = (zB0 + zB1) / 2;          // Schlauchmitte
        const LA = zB1 + 7;                  // Ambosslaenge in Z (2 mm Luft vor Scheibe + Schraubenkopf)
        const TS = 6.0;                      // Steg vorn, Dicke
        const xSweep = R + e;                // Lagerbahn in X
        const XA = xSweep + 12;              // Amboss halbe Laenge
        const XP = XA + 9;                   // Rahmen halbe Breite
        const YP = yA + TA;                  // Rahmen halbe Hoehe
        const band = PI * (tOD + tID) / 4;   // Breite des flach gedrueckten Schlauchs
        const gH = band / 2 + 0.75;          // Fuehrungsstege: halber Innenabstand
        const xScrew = XA - 8;
        const zTip = -PT + 19;               // NEMA-Wellenspitze (19 mm ab Motorflansch)

        // ================= Rahmen-Varianten = Platte + beide Ambosse in einem Teil =================
        // V1 sitzt in Einbaulage, V2.. liegen rechts daneben. Nur die Quetschhoehe (Ambossabstand) aendert sich.
        for (var v = 0; v < nFrames; v += 1)
        {
            const hv = roundToPrecision(h - v * hStep, 3);
            const yv = R + e + hv;
            const xo = v * (2 * XP + 12);
            const p = id + ("rahmen" ~ v);
            bx(context, p + "a" + "plate", xo - XP, xo + XP, -(yv + TA), yv + TA, -PT, 0);
            for (var side in [-1, 1])
            {
                const yIn = side * yv;
                bx(context, p + "a" + ("blk" ~ (side < 0 ? "U" : "O")), xo - XA, xo + XA, yIn, side * (yv + TA), -0.01, LA);
                var k = 0;
                for (var sx in [-1, 1])
                {
                    for (var sz in [-1, 1])
                    {
                        const za = zc + sz * gH;
                        bx(context, p + "a" + ("g" ~ (side < 0 ? "U" : "O") ~ k), xo + sx * (xSweep + 2), xo + sx * XA, yIn, yIn - side * 4,
                            sz > 0 ? za : za - 2, sz > 0 ? za + 2 : za);
                        k += 1;
                    }
                }
            }
            cz(context, p + "c" + "pilot", xo, 0, -PT - 1, 1, 22.4);
            for (var sx in [-1, 1])
            {
                for (var sy in [-1, 1])
                {
                    const sfx = (sx < 0 ? "L" : "R") ~ (sy < 0 ? "U" : "O");
                    cz(context, p + "c" + ("m" ~ sfx), xo + sx * 15.5, sy * 15.5, -PT - 1, 1, M3_CLR);
                    cz(context, p + "c" + ("mk" ~ sfx), xo + sx * 15.5, sy * 15.5, -3.2, 1, 6.4);            // Motorschraube versenkt
                    cz(context, p + "c" + ("fx" ~ sfx), xo + sx * (XA + 4.5), sy * 15, -PT - 1, 1, M3_CLR);    // Befestigung
                    cz(context, p + "c" + ("st" ~ sfx), xo + sx * xScrew, sy * (yv + TA / 2), LA - 10, LA + 1, M3_SELF);   // Steg
                    for (var sz in [-1, 1])                                                                      // Kabelbinder
                    {
                        const zk = zc + sz * (gH + 2 + 1.4);
                        bx(context, p + "c" + ("kb" ~ sfx ~ (sz < 0 ? "v" : "h")),
                            xo + sx * (XA - 3.5) - 2, xo + sx * (XA - 3.5) + 2, sy * (yv - 1), sy * (yv + TA + 1), zk - 0.9, zk + 0.9);
                    }
                }
            }
            for (var k = 0; k <= v; k += 1)                                                                  // Kennung: 1..n Punkte vorn rechts
                cz(context, p + "c" + ("dot" ~ k), xo + XA + 4.5, -4.5 + k * 3, -0.6, 1, 1.0);
            finish(context, p, "Quetschventil Rahmen V" ~ (v + 1) ~ " Quetschhoehe " ~ toString(hv) ~
                " (Ueberpressung " ~ toString(roundToPrecision(tOD - tID - hv, 3)) ~ ")", true);
        }

        // ================= Steg vorn (ein Teil fuer alle Rahmen-Varianten, Langloecher) =================
        {
            const p = id + "steg";
            bx(context, p + "a" + "plate", -XA, XA, -YP, YP, LA, LA + TS);
            bx(context, p + "c" + "win", -(XA - 11), XA - 11, -(yALow - 0.5), yALow - 0.5, LA - 1, LA + TS + 1);   // Fenster
            for (var sx in [-1, 1])
            {
                for (var sy in [-1, 1])
                {
                    const sfx = (sx < 0 ? "L" : "R") ~ (sy < 0 ? "U" : "O");
                    const y0 = sy * (yALow + TA / 2);
                    const y1 = sy * (yA + TA / 2);
                    cz(context, p + "c" + ("h0" ~ sfx), sx * xScrew, y0, LA - 1, LA + TS + 1, M3_CLR);
                    if (nFrames > 1 && hStep > 0)                                                               // Langloch nur bei mehreren Rahmen
                    {
                        cz(context, p + "c" + ("h1" ~ sfx), sx * xScrew, y1, LA - 1, LA + TS + 1, M3_CLR);
                        bx(context, p + "c" + ("hs" ~ sfx), sx * xScrew - M3_CLR / 2, sx * xScrew + M3_CLR / 2, y0, y1, LA - 1, LA + TS + 1);
                    }
                }
            }
            finish(context, p, nFrames > 1 ? "Quetschventil Steg vorn (passt auf alle Rahmen)" : "Quetschventil Steg vorn", true);
        }

        // ================= Exzenter (eigene Exzentrizitaet, Rahmen bleibt) + Griffscheibe =================
        // Ambossabstand ist fest (R + e Rahmen + h). Kleinere Exzentrizitaet am Exzenter: Spalt zu = h + e - eE (groesser),
        // Spalt offen = h + e + eE (kleiner). Weniger Hub -> weniger Ueberpressung und weniger Drehmoment.
        const eE0 = definition.eccE / millimeter;
        for (var v = 0; v < definition.pinCount; v += 1)
        {
            const d = roundToPrecision(pinD + v * definition.pinStep / millimeter, 3);
            const ev = roundToPrecision(definition.eccE / millimeter - v * definition.eccStep / millimeter, 3);
            if (ev <= 0 || ev + nD / 2 > d / 2 - 1.2)
                throw regenError("Exzentrizitaet Exzenter V" ~ (v + 1) ~ " ungueltig (NEMA-Bohrung ragt aus dem Zapfen)");
            const zuV = h + e - ev;                                   // Spalt zu mit diesem Exzenter (Rahmen V1)
            const xo = v == 0 ? 0 : -XP + 15 + (v - 1) * 32;
            const yo = v == 0 ? 0 : -YP - 30;
            buildEccentric(context, id + ("exz" ~ v), xo, yo, ev, d, nD, nF, ribT, zB0, zB1, zTip, v + 1,
                "Quetschventil Exzenter V" ~ (v + 1) ~ " e " ~ toString(ev) ~ " Zapfen " ~ toString(d) ~
                " (Spalt zu " ~ toString(roundToPrecision(zuV, 3)) ~ ", Ueberpressung " ~ toString(roundToPrecision(tOD - tID - zuV, 3)) ~
                ", offen " ~ toString(roundToPrecision(h + e + ev, 3)) ~ ")");
        }
        {
            // Griffscheibe: haelt die Lager wie die alte Scheibe, Stiel durch das Stegfenster, Knauf ueber dem Steg.
            // Mitnahme ueber 2 Stifte am Zapfen, Halt mit M3x12 (Kopf unten im Knauf). Passt auf alle Exzenter.
            // Druck: Scheibe auf dem Bett, Knauf oben (45°-Kegel, kein Support).
            const w = id + "griff";
            const zK = LA + TS + 1;                       // Kegel beginnt 1 mm ueber dem Steg
            cz(context, w + "a" + "disc", eE0, 0, zB1, zB1 + 2, 22);
            cz(context, w + "a" + "stem", eE0, 0, zB1 + 1.99, zK + 0.01, 12);
            kz(context, w + "a" + "cone", eE0, 0, zK, zK + 9, 6, 15);
            cz(context, w + "a" + "knob", eE0, 0, zK + 8.99, zK + 16, 30);
            cz(context, w + "c" + "m3", eE0, 0, zB1 - 1, zK + 17, M3_CLR);
            cz(context, w + "c" + "head", eE0, 0, zB1 + 5, zK + 17, 6.5);
            for (var k = 0; k < 2; k += 1)
                cz(context, w + "c" + ("peg" ~ k), eE0, k == 0 ? 5.5 : -5.5, zB1 - 1, zB1 + 2.6, 3.3);
            for (var k = 0; k < 10; k += 1)
            {
                const a = k * 36 * degree;
                cz(context, w + "c" + ("fl" ~ k), eE0 + 15.8 * cos(a), 15.8 * sin(a), zK + 9, zK + 17, 4);
            }
            finish(context, w, "Quetschventil Griffscheibe (Knauf, M3x12)", true);
        }

        // ================= Lager-Dummies =================
        if (definition.dummies)
        {
            for (var i = 0; i < 2; i += 1)
            {
                const p = id + ("lager" ~ i);
                cz(context, p + "a" + "o", eE0, 0, zB0 + i * bw, zB0 + (i + 1) * bw, 2 * R);
                cz(context, p + "c" + "i", eE0, 0, zB0 + i * bw - 1, zB0 + (i + 1) * bw + 1, 17);
                finish(context, p, "Lager-Dummy (nicht drucken)", true);
            }
        }
    });
