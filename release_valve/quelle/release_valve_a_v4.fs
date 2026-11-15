FeatureScript 3112;
import(path : "onshape/std/common.fs", version : "3112.0");

// =====================================================================
// THREAD HAND: right-hand by default (turn right = screw in / close), checked at regen.
// Release valve A v4: slotted neck + slotted thread root, parametric diameters
// (manual bleed valve, test vessel port 1-1/16"-12 UN male)
//
//   Head (inside of the vessel): wavy star knob, outline r = r0 + amp * sin(n * phi), 0.5 mm chamfers.
//         Top of the head = O-ring face (O-ring as on the O2 adapter, ~5 mm cord),
//         squeezed against the inner port face -> closed = tight.
//   Neck: 10 mm. Lower ~5.5 mm plain (O-ring zone), above it 2 staggered rows of slots.
//   Thread: 60°, pitch 2.117 (12 TPI), OD 26.1 / root 23.5 (slim: ~0.29 mm axial flank play,
//         turns easily under chamber pressure; only the O-ring seals), 11 turns, 45° fade-in, 45° lead-in.
//   Openings only in the upper half of the cylinder (neck + thread); the lower half is plain thread.
//         ~50 % of the area in the neck (open right after the O-ring lifts), ~50 % as slots in the thread
//         root following the helix. Slots are 1.5 mm high: they fit the groove, crests stay intact
//         (fewer edges -> grease stays on the flanks, less wear). Flat bridged top, 45 deg pointed ends.
//   Parameters in the feature dialog: thread major diameter, neck diameter (minor follows: depth 0.6134 P).
// Print: head face down, no support. z = 0 is the head face (bed).
// =====================================================================

const RV_VARIABLES = {
    // --- head ---
    "rv_head_h"      : 6 * millimeter,
    "rv_knob_r0"     : 24.5 * millimeter,   // mean radius of the wavy knob
    "rv_knob_amp"    : 4 * millimeter,      // wave amplitude (mean -> crest); trough radius = r0 - amp
    "rv_knob_n"      : 6,                   // whole waves per turn
    "rv_edge_ch"     : 0.5 * millimeter,    // 45° chamfer, top + bottom edge

    // --- bore ---
    "rv_bore_d"      : 18 * millimeter,     // 18: keeps >= 0.35 mm between neighbouring holes at the bore wall
    "rv_bore_floor"  : 2 * millimeter,      // closed end at the head (z = 0 .. floor)

    // --- neck (O-ring seat) ---

    // --- male thread 1-1/16"-12 UN, printed fit (values are final, no extra clearance) ---
    "rv_m_depth_k"   : 0.6134,              // thread depth = k x pitch; minor = major - 2 x depth
    "rv_m_pitch"     : 2.117 * millimeter,    // 12 TPI - critical, do not change
    "rv_m_turns"     : 11,
    "rv_m_lead"      : 0.5 * millimeter,    // 45° lead-in reaches this far below the root
    "rv_flank"       : 30 * degree,         // half flank angle: 60° thread

    // --- v4 openings ---
    "rv_a_neck_len"  : 10 * millimeter,     // neck under the head (O-ring zone + slot rows)
    "rv_slot_h"      : 1.5 * millimeter,    // slot height (axial), fits the thread groove
    "rv_neck_slot_l" : 7 * millimeter,      // neck slots: length (tangential chord)
    "rv_neck_slots"  : 4,                   // per row, 2 rows staggered
    "rv_neck_row_p"  : 2.25 * millimeter,   // row pitch
    "rv_thr_slot_l"  : 8 * millimeter,      // thread-root slots: length
    "rv_thr_per_turn": 2.5                  // slots per turn (non-integer -> staggered from turn to turn)
};

// Single feature: values come from this table. To override one without editing the file,
// create a Part Studio #variable with the same name (e.g. #rv_slot_h) before this feature.
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

// Wavy knob: outline r(phi) = r0 + amp * sin(n * phi), n whole waves per turn (closed curve),
// extruded z = 0 .. h, equal 45-degree chamfer (edgeCh) on the bottom and top outline.
// periodic = true: closed periodic spline. false: fallback, closed spline through a repeated first point.
function makeKnob(context is Context, id is Id, r0 is ValueWithUnits, amp is ValueWithUnits, n is number,
    h is ValueWithUnits, edgeCh is ValueWithUnits, periodic is boolean)
{
    const sk = newSketchOnPlane(context, id + "sk", {
                "sketchPlane" : plane(vector(0, 0, 0) * millimeter, vector(0, 0, 1), vector(1, 0, 0))
            });
    const m = n * 16;   // 16 interpolation points per wave
    var pts = [];
    for (var i = 0; i < m; i += 1)
    {
        const a = (i / m) * 360 * degree;
        const r = r0 + amp * sin(n * a);
        pts = append(pts, vector(r * cos(a), r * sin(a)));
    }
    if (!periodic)
    {
        pts = append(pts, pts[0]);
    }
    skFitSpline(sk, "wave", { "points" : pts, "isPeriodic" : periodic });
    skSolve(sk);
    opExtrude(context, id + "ex", {
                "entities" : qSketchRegion(id + "sk"),
                "direction" : vector(0, 0, 1),
                "endBound" : BoundingType.BLIND,
                "endDepth" : h
            });
    opDeleteBodies(context, id + "clean", {
                "entities" : qCreatedBy(id + "sk", EntityType.BODY)
            });
    const capFaces = qUnion([qCapEntity(id + "ex", CapType.START, EntityType.FACE),
                qCapEntity(id + "ex", CapType.END, EntityType.FACE)]);
    opChamfer(context, id + "ch", {
                "entities" : qAdjacent(capFaces, AdjacencyType.EDGE, EntityType.EDGE),
                "chamferType" : ChamferType.EQUAL_OFFSETS,
                "width" : edgeCh
            });
}


const RV_MAJOR_BOUNDS = { (millimeter) : [20, 26.1, 30] } as LengthBoundSpec;
const RV_NECK_BOUNDS = { (millimeter) : [18, 23.2, 28] } as LengthBoundSpec;

// Hexagonal slot (x = tangential, y = up): flat bridged top/bottom, 45-degree pointed ends,
// rotated by tilt (follow the helix). Length l, height h.
function slotPts(l is ValueWithUnits, h is ValueWithUnits, tilt is ValueWithUnits) returns array
{
    const h2 = h / 2;
    const l2 = l / 2;
    const raw = [[-l2 + h2, -h2], [l2 - h2, -h2], [l2, 0 * millimeter], [l2 - h2, h2], [-l2 + h2, h2], [-l2, 0 * millimeter]];
    var pts = [];
    for (var p in raw)
    {
        pts = append(pts, vector(p[0] * cos(tilt) - p[1] * sin(tilt), p[0] * sin(tilt) + p[1] * cos(tilt)));
    }
    return pts;
}

annotation { "Feature Type Name" : "Release valve A v4 (slots, parametric)" }
export const releaseValveA = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Thread major diameter (minor follows, depth 0.6134 P)" }
        isLength(definition.majorD, RV_MAJOR_BOUNDS);

        annotation { "Name" : "Neck diameter (must be below minor, e.g. minor - 0.3)" }
        isLength(definition.neckD, RV_NECK_BOUNDS);

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
        const knobR0 = cv(context, "rv_knob_r0");
        const knobAmp = cv(context, "rv_knob_amp");
        const knobN = cv(context, "rv_knob_n");
        const edgeCh = cv(context, "rv_edge_ch");

        const boreD = cv(context, "rv_bore_d");
        const boreFloor = cv(context, "rv_bore_floor");
        const neckD = definition.neckD;

        const mMajor = definition.majorD;
        const mPitch = cv(context, "rv_m_pitch");
        const mMinor = mMajor - 2 * cv(context, "rv_m_depth_k") * mPitch;
        const mTurns = cv(context, "rv_m_turns");
        const mLead = cv(context, "rv_m_lead");
        const flank = cv(context, "rv_flank");

        const aNeckLen = cv(context, "rv_a_neck_len");
        const slotH = cv(context, "rv_slot_h");
        const neckSlotL = cv(context, "rv_neck_slot_l");
        const neckSlots = cv(context, "rv_neck_slots");
        const neckRowP = cv(context, "rv_neck_row_p");
        const thrSlotL = cv(context, "rv_thr_slot_l");
        const thrPerTurn = cv(context, "rv_thr_per_turn");

        const rightHand = definition.threadHand == ThreadHand.RIGHT;   // default RIGHT

        // ---------- derived ----------
        const zNeck = headH + aNeckLen;   // thread starts here
        const zTip = zNeck + mTurns * mPitch;
        const rmC = mMajor / 2;
        const rmR = mMinor / 2;
        const rN = neckD / 2;
        const rB = boreD / 2;
        if (rN >= rmR)
        {
            throw regenError("Neck diameter must be smaller than the thread minor diameter (" ~ roundToPrecision(mMinor / mm, 3) ~ " mm)");
        }
        if (knobR0 - knobAmp <= rN + 1 * mm)
        {
            throw regenError("knob trough radius (rv_knob_r0 - rv_knob_amp) must be larger than the neck radius");
        }

        const solids = qBodyType(qCreatedBy(id, EntityType.BODY), BodyType.SOLID);
        const partQ = qContainsPoint(solids, vector((rN + (knobR0 - knobAmp)) / 2, 0 * mm, headH / 2));
        const maleQ = qContainsPoint(solids, pz(zNeck + (zTip - zNeck) / 2));

        // =============================== HEAD (wavy star knob) ===============================
        var knobOk = false;
        var knobErr = undefined;
        for (var attempt = 0; attempt < 2; attempt += 1)
        {
            const knobId = id + ("knob" ~ attempt);
            try
            {
                makeKnob(context, knobId, knobR0, knobAmp, knobN, headH, edgeCh, attempt == 0);
                knobOk = true;
                break;
            }
            catch (e)
            {
                knobErr = e;
                try silent
                {
                    opDeleteBodies(context, id + ("knobDel" ~ attempt), {
                                "entities" : qCreatedBy(knobId, EntityType.BODY)
                            });
                }
            }
        }
        if (!knobOk)
        {
            throw regenError("Stage KNOB failed: " ~ toString(knobErr));
        }

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
        // only in the upper half of the cylinder (neck + thread); lower half stays plain thread
        const zZone = headH + (aNeckLen + mTurns * mPitch) / 2;
        const len = rmC + 2 * mm;
        const h2 = slotH / 2;
        var cutTools = [qCreatedBy(id + "bore", EntityType.BODY)];
        var aNeck = 0 * mm * mm;
        var aThr = 0 * mm * mm;
        var nThr = 0;
        try
        {
            // neck: two staggered rows directly below the thread start (above the O-ring zone)
            const rowZ = [zNeck - 1.25 * mm, zNeck - 1.25 * mm - neckRowP];
            for (var ri = 0; ri < 2; ri += 1)
            {
                for (var i = 0; i < neckSlots; i += 1)
                {
                    const phi = (i * 360 / neckSlots + ri * 180 / neckSlots) * degree;
                    cutTools = append(cutTools, radialCut(context, id + ("ns" ~ ri ~ "x" ~ i), phi, rowZ[ri],
                                slotPts(neckSlotL, slotH, 0 * degree), undefined, len));
                    aNeck += (neckSlotL - slotH) * slotH + slotH * slotH / 2;
                }
            }
            // thread: slots centred in the root groove, tilted to follow the helix
            const hand = rightHand ? 1 : -1;   // right hand: z rises counter-clockwise
            const tilt = hand * atan(mPitch / (2 * PI * (rmR + rmC) / 2));
            for (var k = 0; k < 100; k += 1)
            {
                const zc = zNeck + mPitch / 2 + k * mPitch / thrPerTurn;
                if (zc + h2 + 0.2 * mm > zZone)
                {
                    break;
                }
                cutTools = append(cutTools, radialCut(context, id + ("ts" ~ k), (hand * k * 360 / thrPerTurn) * degree, zc,
                            slotPts(thrSlotL, slotH, tilt), undefined, len));
                aThr += (thrSlotL - slotH) * slotH + slotH * slotH / 2;
                nThr += 1;
            }
            opBoolean(context, id + "flowCut", {
                        "targets" : partQ,
                        "tools" : qUnion(cutTools),
                        "operationType" : BooleanOperationType.SUBTRACTION
                    });
        }
        catch (e)
        {
            throw regenError("Stage SLOTS failed: " ~ toString(e));
        }
        const info = "major " ~ roundToPrecision(mMajor / mm, 2) ~ " / minor " ~ roundToPrecision(mMinor / mm, 3) ~
            " / neck " ~ roundToPrecision(neckD / mm, 2) ~ "; slots: neck " ~ (2 * neckSlots) ~ " = " ~
            roundToPrecision(aNeck / (mm * mm), 1) ~ " mm2, thread " ~ nThr ~ " = " ~ roundToPrecision(aThr / (mm * mm), 1) ~
            " mm2; openings z " ~ roundToPrecision((zNeck - 1.25 * mm - neckRowP - h2) / mm, 2) ~ " - " ~
            roundToPrecision(zZone / mm, 2) ~ " mm";

        reportFeatureInfo(context, id, info ~ "; height " ~ roundToPrecision(zTip / mm, 2) ~ " mm");

        setProperty(context, {
                    "entities" : partQ,
                    "propertyType" : PropertyType.NAME,
                    "value" : "Release valve A v4 OD " ~ roundToPrecision(mMajor / mm, 2)
                });

        if (abs(definition.offsetX) > 0.001 * mm)
        {
            opTransform(context, id + "offset", {
                        "bodies" : partQ,
                        "transform" : transform(vector(definition.offsetX, 0 * mm, 0 * mm))
                    });
        }
    });