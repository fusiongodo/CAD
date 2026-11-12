FeatureScript 3112;
import(path : "onshape/std/common.fs", version : "3112.0");

// =====================================================================
// THREAD HAND: right-hand by default (turn right = screw in / close), checked at regen.
// Release valve A: perforated thread  (manual bleed valve, test vessel port 1-1/16"-12 UN male)
//
//   Head (inside of the vessel): flat solid hex SW 40, rounded corners, 0.5 mm chamfers.
//         Top of the head = O-ring face (O-ring as on the O2 adapter, ~5 mm cord),
//         squeezed against the inner port face -> closed = tight.
//   Neck: plain, 5 mm, O-ring seat.
//   Thread: 60°, pitch 2.117 (12 TPI), OD 26.6, 11 turns, 45° fade-in, 45° lead-in.
//   Flow: many small flat-topped teardrop holes along the thread root (helix) into the bore,
//         bore closed at the head, open at the outer tip. Unscrewing exposes one more turn
//         of holes per turn -> progressive; many thin jets -> high-pitched, quieter.
// Print: head face down, no support. z = 0 is the head face (bed).
// =====================================================================

const RV_VARIABLES = {
    // --- head ---
    "rv_head_h"      : 6 * millimeter,
    "rv_hex_af"      : 40 * millimeter,     // width across flats (SW)
    "rv_corner_r"    : 21.9 * millimeter,   // hex corners cut by this cylinder (sharp corner = af / sqrt(3))
    "rv_edge_ch"     : 0.5 * millimeter,    // 45° chamfer, top + bottom edge

    // --- bore ---
    "rv_bore_d"      : 18 * millimeter,     // 18: keeps >= 0.35 mm between neighbouring holes at the bore wall
    "rv_bore_floor"  : 2 * millimeter,      // closed end at the head (z = 0 .. floor)

    // --- neck (O-ring seat) ---
    "rv_neck_d"      : 23.6 * millimeter,   // below the port's thread minor diameter

    // --- male thread 1-1/16"-12 UN, printed fit (values are final, no extra clearance) ---
    "rv_m_major"     : 26.6 * millimeter,
    "rv_m_minor"     : 24.003 * millimeter,   // major - 2 x 0.6134 x pitch
    "rv_m_pitch"     : 2.117 * millimeter,    // 12 TPI
    "rv_m_turns"     : 11,
    "rv_m_lead"      : 0.5 * millimeter,    // 45° lead-in reaches this far below the root
    "rv_flank"       : 30 * degree,         // half flank angle: 60° thread

    // --- A: perforated thread ---
    "rv_a_neck_len"  : 5 * millimeter,      // plain neck under the head
    "rv_a_hole_d"    : 1.5 * millimeter,    // teardrop holes in the thread root
    "rv_a_per_turn"  : 30,                  // holes per turn (along the root helix)
    "rv_a_turns"     : 8                    // turns with holes
};

// Single feature: values come from this table. To override one without editing the file,
// create a Part Studio #variable with the same name (e.g. #rv_a_hole_d) before this feature.
function cv(context is Context, name is string)
{
    var v = RV_VARIABLES[name];
    try silent
    {
        v = getVariable(context, name);
    }
    return v;
}

function pz(z is ValueWithUnits) returns Vector
{
    return vector(0 * millimeter, 0 * millimeter, z);
}

// Right-hand check of a helix edge: looking down from +Z, z must rise while it turns
// counter-clockwise (= screwing in when turned clockwise from the head side).
// Independent of how opHelix defines "clockwise" (never guess that flag).
function helixIsRightHand(context is Context, edge is Query, turns is number) returns boolean
{
    const p0 = evEdgeTangentLine(context, { "edge" : edge, "parameter" : 0 }).origin;
    const p1 = evEdgeTangentLine(context, { "edge" : edge, "parameter" : 0.25 / turns }).origin;
    const crossZ = (p0[0] * p1[1] - p0[1] * p1[0]) / (millimeter * millimeter);
    const dz = (p1[2] - p0[2]) / millimeter;
    return crossZ * dz > 0;
}

// Trapezoid thread tooth in the XZ plane, swept along a helix around Z.
// THREAD HAND: rightHand = true is the default for every part (turn right = screw in / close).
// The helix is built, measured and rebuilt the other way if the hand is wrong.
function makeThread(context is Context, id is Id, rRoot is ValueWithUnits, rCrest is ValueWithUnits,
    overlapR is ValueWithUnits, zStart is ValueWithUnits, pitch is ValueWithUnits, turns is number,
    flank is ValueWithUnits, rightHand is boolean)
{
    const depth = abs(rCrest - rRoot);
    const crest = pitch / 8;              // crest flat as in the test plugs
    const wRoot = crest + 2 * depth * tan(flank);
    const rMid = (rRoot + rCrest) / 2;

    var helixId = id + "helix";
    for (var attempt = 0; attempt < 2; attempt += 1)
    {
        helixId = id + "helix" + ("a" ~ attempt);   // sub-id: cleanup via qCreatedBy(id + "helix") still works
        opHelix(context, helixId, {
                    "direction" : vector(0, 0, 1),
                    "axisStart" : vector(0, 0, 0) * millimeter,
                    "startPoint" : vector(rMid, 0 * millimeter, zStart),
                    "interval" : vector(0, turns),
                    "clockwise" : attempt == 0,   // measured: clockwise = true gives a right-hand helix here
                    "helicalPitch" : pitch,
                    "spiralPitch" : 0 * millimeter
                });
        if (helixIsRightHand(context, qCreatedBy(helixId, EntityType.EDGE), turns) == rightHand)
        {
            break;
        }
        opDeleteBodies(context, id + "helix" + ("del" ~ attempt), {   // same parent id as the helices (Onshape id rule)
                    "entities" : qCreatedBy(helixId, EntityType.BODY)
                });
        if (attempt == 1)
        {
            throw regenError("Thread hand check failed: could not build the requested thread hand");
        }
    }

    // plane normal -Y, x = +X  ->  sketch y = world +Z
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
    {
        skLineSegment(sk, "l" ~ i, {
                    "start" : pts[i],
                    "end" : pts[(i + 1) % size(pts)]
                });
    }
    skSolve(sk);

    opSweep(context, id + "sweep", {
                "profiles" : qSketchRegion(id + "profile"),
                "path" : qCreatedBy(helixId, EntityType.EDGE)
            });
}

export enum ThreadHand
{
    annotation { "Name" : "Right (standard: turn right = screw in)" }
    RIGHT,
    annotation { "Name" : "Left" }
    LEFT
}

// Ring tool: cylinder minus cone (used for 45° chamfers / fade-ins).
function ringTool(context is Context, id is Id, z0 is ValueWithUnits, z1 is ValueWithUnits,
    rOuter is ValueWithUnits, rCone0 is ValueWithUnits, rCone1 is ValueWithUnits)
{
    fCylinder(context, id + "c", {
                "bottomCenter" : pz(z0),
                "topCenter" : pz(z1),
                "radius" : rOuter
            });
    fCone(context, id + "k", {
                "bottomCenter" : pz(z0),
                "topCenter" : pz(z1),
                "bottomRadius" : rCone0,
                "topRadius" : rCone1
            });
    opBoolean(context, id + "cut", {
                "targets" : qCreatedBy(id + "c", EntityType.BODY),
                "tools" : qCreatedBy(id + "k", EntityType.BODY),
                "operationType" : BooleanOperationType.SUBTRACTION
            });
    return qCreatedBy(id + "c", EntityType.BODY);
}

// 2D polygon (x = tangential, y = up) on a plane through the axis, extruded radially outwards.
function radialCut(context is Context, id is Id, phi is ValueWithUnits, z0 is ValueWithUnits,
    pts is array, arcMid, len is ValueWithUnits) returns Query
{
    const d = vector(cos(phi), sin(phi), 0);
    const t = vector(-sin(phi), cos(phi), 0);
    const sk = newSketchOnPlane(context, id + "sk", {
                "sketchPlane" : plane(pz(z0), d, t)
            });
    const n = size(pts);
    for (var i = 0; i < n; i += 1)
    {
        if (i == 0 && arcMid != undefined)
        {
            skArc(sk, "a", { "start" : pts[0], "mid" : arcMid, "end" : pts[1] });
        }
        else
        {
            skLineSegment(sk, "l" ~ i, { "start" : pts[i], "end" : pts[(i + 1) % n] });
        }
    }
    skSolve(sk);
    opExtrude(context, id + "ex", {
                "entities" : qSketchRegion(id + "sk"),
                "direction" : d,
                "endBound" : BoundingType.BLIND,
                "endDepth" : len
            });
    opDeleteBodies(context, id + "clean", {
                "entities" : qCreatedBy(id + "sk", EntityType.BODY)
            });
    return qCreatedBy(id + "ex", EntityType.BODY);
}

annotation { "Feature Type Name" : "Release valve A (perforated)" }
export const releaseValveA = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Offset X (e.g. to place A and B side by side)" }
        isLength(definition.offsetX, LENGTH_BOUNDS);

        annotation { "Name" : "Model threads", "Default" : true }
        definition.modelThreads is boolean;

        annotation { "Name" : "Thread hand", "Default" : ThreadHand.RIGHT }
        definition.threadHand is ThreadHand;
    }
    {
        const mm = millimeter;

        // ---------- variables ----------
        const headH = cv(context, "rv_head_h");
        const af = cv(context, "rv_hex_af");
        const cornerR = cv(context, "rv_corner_r");
        const edgeCh = cv(context, "rv_edge_ch");

        const boreD = cv(context, "rv_bore_d");
        const boreFloor = cv(context, "rv_bore_floor");
        const neckD = cv(context, "rv_neck_d");

        const mMajor = cv(context, "rv_m_major");
        const mMinor = cv(context, "rv_m_minor");
        const mPitch = cv(context, "rv_m_pitch");
        const mTurns = cv(context, "rv_m_turns");
        const mLead = cv(context, "rv_m_lead");
        const flank = cv(context, "rv_flank");

        const aNeckLen = cv(context, "rv_a_neck_len");
        const aHoleD = cv(context, "rv_a_hole_d");
        const aPerTurn = cv(context, "rv_a_per_turn");
        const aTurns = cv(context, "rv_a_turns");

        const rightHand = definition.threadHand == ThreadHand.RIGHT;   // default RIGHT

        // ---------- derived ----------
        const R = af / sqrt(3);                  // sharp hex corner radius
        const zNeck = headH + aNeckLen;   // thread starts here
        const zTip = zNeck + mTurns * mPitch;
        const rmC = mMajor / 2;
        const rmR = mMinor / 2;
        const rN = neckD / 2;
        const rB = boreD / 2;
        if (rN >= rmR)
        {
            throw regenError("rv_neck_d must be smaller than rv_m_minor");
        }
        const rc0 = af / 2 - edgeCh;
        const zc = cornerR - rc0 + 0.5 * mm;

        const solids = qBodyType(qCreatedBy(id, EntityType.BODY), BodyType.SOLID);
        const partQ = qContainsPoint(solids, vector(af / 2 - 1 * mm, 0 * mm, headH / 2));
        const maleQ = qContainsPoint(solids, pz(zNeck + (zTip - zNeck) / 2));

        // =============================== HEAD ===============================
        const skHex = newSketchOnPlane(context, id + "hexSk", {
                    "sketchPlane" : plane(pz(0 * mm), vector(0, 0, 1), vector(1, 0, 0))
                });
        var hv = [];
        for (var i = 0; i < 6; i += 1)
        {
            const a = (i * 60) * degree;
            hv = append(hv, vector(R * cos(a), R * sin(a)));
        }
        for (var i = 0; i < 6; i += 1)
        {
            skLineSegment(skHex, "e" ~ i, {
                        "start" : hv[i],
                        "end" : hv[(i + 1) % 6]
                    });
        }
        skSolve(skHex);
        opExtrude(context, id + "hexEx", {
                    "entities" : qSketchRegion(id + "hexSk"),
                    "direction" : vector(0, 0, 1),
                    "endBound" : BoundingType.BLIND,
                    "endDepth" : headH
                });
        opDeleteBodies(context, id + "hexClean", {
                    "entities" : qCreatedBy(id + "hexSk", EntityType.BODY)
                });

        // rounded corners: intersect with a cylinder
        fCylinder(context, id + "cornerCyl", {
                    "bottomCenter" : pz(-1 * mm),
                    "topCenter" : pz(headH + 1 * mm),
                    "radius" : cornerR
                });
        opBoolean(context, id + "corners", {
                    "tools" : qUnion([qCreatedBy(id + "hexEx", EntityType.BODY), qCreatedBy(id + "cornerCyl", EntityType.BODY)]),
                    "operationType" : BooleanOperationType.INTERSECTION
                });

        // 45° edge chamfers top + bottom (flats get edgeCh, corners a bit more)
        const chB = ringTool(context, id + "chB", -1 * mm, zc, cornerR + 1 * mm, rc0 - 1 * mm, rc0 + zc);
        const chT = ringTool(context, id + "chT", headH - zc, headH + 1 * mm, cornerR + 1 * mm, rc0 + zc, rc0 - 1 * mm);
        opBoolean(context, id + "chamfer", {
                    "targets" : partQ,
                    "tools" : qUnion([chB, chT]),
                    "operationType" : BooleanOperationType.SUBTRACTION
                });

        // neck
        fCylinder(context, id + "neck", {
                    "bottomCenter" : pz(headH - 0.5 * mm),
                    "topCenter" : pz(zNeck + 0.5 * mm),
                    "radius" : rN
                });
        const neckTools = [partQ, qCreatedBy(id + "neck", EntityType.BODY)];
        opBoolean(context, id + "neckUnion", {
                    "tools" : qUnion(neckTools),
                    "operationType" : BooleanOperationType.UNION
                });

        // =============================== SHANK ===============================
        fCylinder(context, id + "mCore", {
                    "bottomCenter" : pz(zNeck - 0.5 * mm),
                    "topCenter" : pz(zTip + 1 * mm),
                    "radius" : rmR
                });

        const zS = zNeck - mPitch;
        if (definition.modelThreads)
        {
            const turns = (zTip + 1 * mm - zS + 2 * mPitch) / mPitch;
            try
            {
                makeThread(context, id + "thrM", rmR, rmC, rN - 0.1 * mm, zS, mPitch, turns, flank, rightHand);
            }
            catch (e)
            {
                throw regenError("Stage THREAD failed: " ~ toString(e));
            }
            opBoolean(context, id + "thrMUnion", {
                        "tools" : qUnion([maleQ, qCreatedBy(id + "thrM" + "sweep", EntityType.BODY)]),
                        "operationType" : BooleanOperationType.UNION
                    });
            opDeleteBodies(context, id + "thrMClean", {
                        "entities" : qUnion([qCreatedBy(id + "thrM" + "helix", EntityType.BODY),
                                    qCreatedBy(id + "thrM" + "profile", EntityType.BODY)])
                    });
        }

        // fade-in above the neck: 45° cone from the neck radius outwards (no overhang at the first turn)
        const hF = (rmC + 1 * mm) - rN;
        fCylinder(context, id + "fadeCyl", {
                    "bottomCenter" : pz(zS - mPitch - 1 * mm),
                    "topCenter" : pz(zNeck + hF),
                    "radius" : rmC + 2 * mm
                });
        fCylinder(context, id + "fadeKeepN", {
                    "bottomCenter" : pz(zS - mPitch - 2 * mm),
                    "topCenter" : pz(zNeck),
                    "radius" : rN
                });
        fCone(context, id + "fadeKeepC", {
                    "bottomCenter" : pz(zNeck),
                    "topCenter" : pz(zNeck + hF),
                    "bottomRadius" : rN,
                    "topRadius" : rmC + 1 * mm
                });
        opBoolean(context, id + "fadeTool", {
                    "targets" : qCreatedBy(id + "fadeCyl", EntityType.BODY),
                    "tools" : qUnion([qCreatedBy(id + "fadeKeepN", EntityType.BODY), qCreatedBy(id + "fadeKeepC", EntityType.BODY)]),
                    "operationType" : BooleanOperationType.SUBTRACTION
                });

        // tip: flat at zTip with a 45° lead-in cone
        const rTop = rmR - mLead;
        const hTip = (rmC + 1 * mm) - rTop;
        const tipTool = ringTool(context, id + "tip", zTip - hTip, zTip, rmC + 2 * mm, rmC + 1 * mm, rTop);
        fCylinder(context, id + "tipCap", {
                    "bottomCenter" : pz(zTip),
                    "topCenter" : pz(zTip + 3 * mPitch),
                    "radius" : rmC + 2 * mm
                });

        opBoolean(context, id + "maleTrim", {
                    "targets" : maleQ,
                    "tools" : qUnion([qCreatedBy(id + "fadeCyl", EntityType.BODY), tipTool,
                                qCreatedBy(id + "tipCap", EntityType.BODY)]),
                    "operationType" : BooleanOperationType.SUBTRACTION
                });

        // join everything (thin thread remnants below zNeck lie inside the neck and vanish here)
        opBoolean(context, id + "join", {
                    "tools" : solids,
                    "operationType" : BooleanOperationType.UNION
                });

        // =============================== BORE ===============================
        fCylinder(context, id + "bore", {
                    "bottomCenter" : pz(boreFloor),
                    "topCenter" : pz(zTip + 1 * mm),
                    "radius" : rB
                });

        // =============================== FLOW OPENINGS ===============================
        var cutTools = [];
        var info = "";
        const len = rmC + 2 * mm;
        // flat-topped teardrop holes centred on the thread root, following the helix
        const r = aHoleD / 2;
        const s2 = r / sqrt(2);
        const nH = aPerTurn * aTurns;
        const hand = rightHand ? 1 : -1;   // holes follow the root: right hand = z rises counter-clockwise
        // one hole, then patterned along the root helix (much faster than one sketch per hole)
        const z0h = zNeck + mPitch / 2;
        try
        {
            const hole0 = radialCut(context, id + "hole0", 0 * degree, z0h,
                [vector(s2, s2), vector(-s2, s2), vector(-r * (sqrt(2) - 1), r), vector(r * (sqrt(2) - 1), r)],
                vector(0 * mm, -r), len);   // teardrop with flat top at y = r (short bridge instead of a point)
            var trs = [];
            var names = [];
            for (var k = 1; k < nH; k += 1)
            {
                const ang = (hand * 360 * k / aPerTurn) * degree;
                trs = append(trs, transform(vector(0 * mm, 0 * mm, mPitch * k / aPerTurn)) *
                            rotationAround(line(pz(0 * mm), vector(0, 0, 1)), ang));
                names = append(names, "h" ~ k);
            }
            opPattern(context, id + "holePat", {
                        "entities" : hole0,
                        "transforms" : trs,
                        "instanceNames" : names
                    });
            cutTools = [hole0, qCreatedBy(id + "holePat", EntityType.BODY)];
            info = "" ~ nH ~ " holes d" ~ roundToPrecision(aHoleD / mm, 2) ~ " = " ~
                roundToPrecision(nH * PI * r * r / (mm * mm), 1) ~ " mm2, z " ~
                roundToPrecision((zNeck + mPitch / 2 - r) / mm, 1) ~ " - " ~
                roundToPrecision((zNeck + mPitch / 2 + mPitch * (nH - 1) / aPerTurn + r) / mm, 1) ~ " mm";
            opBoolean(context, id + "flowCut", {
                        "targets" : partQ,
                        "tools" : qUnion(concatenateArrays([[qCreatedBy(id + "bore", EntityType.BODY)], cutTools])),
                        "operationType" : BooleanOperationType.SUBTRACTION
                    });
        }
        catch (e)
        {
            throw regenError("Stage HOLES failed (" ~ nH ~ " holes d" ~ roundToPrecision(aHoleD / mm, 2) ~ "): " ~ toString(e));
        }

        reportFeatureInfo(context, id, info ~ "; height " ~ roundToPrecision(zTip / mm, 2) ~ " mm");

        setProperty(context, {
                    "entities" : partQ,
                    "propertyType" : PropertyType.NAME,
                    "value" : "Release valve A (perforated)"
                });

        if (abs(definition.offsetX) > 0.001 * mm)
        {
            opTransform(context, id + "offset", {
                        "bodies" : partQ,
                        "transform" : transform(vector(definition.offsetX, 0 * mm, 0 * mm))
                    });
        }
    });
