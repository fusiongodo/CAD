import time, os, json, base64, glob, math
import cadquery as cq, trimesh, numpy as np
import gestell_v4 as g, fasteners as F
t0=time.time(); os.makedirs("out",exist_ok=True); os.makedirs("print",exist_ok=True); os.makedirs("viewer/m",exist_ok=True)
ys=g.find_stop(); print("Anschlag", round(ys,2))
P={"kopfrahmen":g.kopfrahmen(ys),"kopfbuegel":g.kopfbuegel(),"traeger":g.traeger()}
lev=g.kinnhebel_local(); phone=g.dummy_phone()
for k,v in P.items(): print(k, v.val().isValid(), round(v.val().Volume()/1000,1),"cm3")
def iv(a,b):
    try: return round(a.intersect(b).val().Volume(),2)
    except Exception: return 0.0
names=list(P)
for i in range(3):
    for j in range(i+1,3):
        v=iv(P[names[i]],P[names[j]]); 
        if v>0.01: print("UEBERLAPPUNG",names[i],names[j],v)
L0=g.place_lever(lev,-g.LEAN)
for n in names: 
    v=iv(L0,P[n]); 
    if v>0.01: print("HEBEL-UEBERLAPPUNG",n,v)
    v=iv(phone,P[n])
    if v>0.01: print("HANDY",n,v)
fold={a:(iv(g.place_lever(lev,a),P["kopfrahmen"]),iv(g.place_lever(lev,a),P["traeger"])) for a in [-5.5,-5,10,20,30,40,50,60,70,75,78,80,82]}
print("Klappweg:",fold)
# --- Verbindungselemente: Platzierung (Transformationsmatrizen) ---
def T(rot_axis, ang, origin):
    M=trimesh.transformations.rotation_matrix(math.radians(ang), rot_axis) if ang else np.eye(4)
    M=M.copy(); M[:3,3]=origin; return M
PX=g.PED_X; FX=g.TONGUE_T/2+g.FIT+g.FORK_PLATE
place=[]  # (name, mesh_key, matrix)
place.append(("gelenkbolzen","gelenkbolzen",T([0,1,0],90,[-PX,g.Y_PIV,g.Z_PIV])))
place.append(("gelenkmutter","mutter",T([0,1,0],90,[PX,g.Y_PIV,g.Z_PIV])))
y1,y2=g.FORK_BOLTS_Y
place.append(("gabelschraube1","gabelschraube",T([0,1,0],-90,[FX,y1,g.FORK_BOLT_Z])))
place.append(("gabelmutter1","mutter",T([0,1,0],-90,[-FX,y1,g.FORK_BOLT_Z])))
place.append(("gabelschraube2","gabelschraube",T([0,1,0],90,[-FX,y2,g.FORK_BOLT_Z])))
place.append(("gabelmutter2","mutter",T([0,1,0],90,[FX,y2,g.FORK_BOLT_Z])))
for sx in (1,-1):
    place.append((f"buegelschraube{'R' if sx>0 else 'L'}","buegelschraube",T([1,0,0],180,[sx*g.POST_X,g.POST_SCREW_Y,g.Z_TOP+g.HALO_T])))
FM={"gelenkbolzen":F.bolt(82,smooth_to=58),"gabelschraube":F.bolt(31),"buegelschraube":F.bolt(20),"mutter":F.nut()}
for k,m in FM.items(): print(k,"dicht",m.is_watertight, round(m.volume/1000,1),"cm3")
# --- Teile als Netze (Rahmen mit Innengewinde) ---
def mesh_of(w,tol=0.05):
    cq.exporters.export(w,"out/_tmp.stl",tolerance=tol,angularTolerance=0.1); return trimesh.load("out/_tmp.stl")
frame_m=mesh_of(P["kopfrahmen"])
cuts=[]
for sx in (1,-1):
    c=F.thread_cutter(0,18); c.apply_translation([sx*g.POST_X,g.POST_SCREW_Y,g.Z_TOP-18]); cuts.append(c)
frame_m=F.Dif(frame_m,*cuts); print("Rahmen mit Gewinde dicht", frame_m.is_watertight)
MESH={"kopfrahmen":frame_m,"kopfbuegel":mesh_of(P["kopfbuegel"]),"traeger":mesh_of(P["traeger"]),"kinnhebel_local":mesh_of(lev)}
# Kollision Köpfe/Muttern mit Teilen (Gewindeeingriff ausgenommen): Kopf-/Mutter-Hüllkörper
lev_rest=MESH["kinnhebel_local"].copy(); R=trimesh.transformations.rotation_matrix(math.radians(-g.LEAN),[1,0,0]); lev_rest.apply_transform(R); lev_rest.apply_translation([0,g.Y_PIV,g.Z_PIV])
parts_all={"kopfrahmen":frame_m,"kopfbuegel":MESH["kopfbuegel"],"traeger":MESH["traeger"],"kinnhebel":lev_rest}
for nm,key,M in place:
    hull=trimesh.creation.cylinder(radius=11.05,segment=[[0,0,-8.05],[0,0,-0.05]]) if key!="mutter" else trimesh.creation.cylinder(radius=11.05,segment=[[0,0,0.05],[0,0,8.0]])
    hull.apply_transform(M)
    for pn,pm in parts_all.items():
        v=trimesh.boolean.intersection([hull,pm],engine="manifold").volume
        if v>0.05: print("KOLLISION Kopf/Mutter",nm,pn,round(v,2))
    # Schaft (glatter Ersatz r=4.85) gegen Teile, Gewindebohrung in Säule ausgenommen
    if key!="mutter":
        L={"gelenkbolzen":82,"gabelschraube":31,"buegelschraube":20}[key]
        sh=trimesh.creation.cylinder(radius=4.85,segment=[[0,0,0.05],[0,0,L]]); sh.apply_transform(M)
        for pn,pm in parts_all.items():
            if key=="buegelschraube" and pn=="kopfrahmen": continue
            v=trimesh.boolean.intersection([sh,pm],engine="manifold").volume
            if v>0.05: print("KOLLISION Schaft",nm,pn,round(v,2))
# --- Druckdateien ---
def on_bed(m):
    m=m.copy(); b=m.bounds; m.apply_translation([-(b[0][0]+b[1][0])/2,-(b[0][1]+b[1][1])/2,-b[0][2]]); return m
halo_print=MESH["kopfbuegel"].copy(); halo_print.apply_transform(trimesh.transformations.rotation_matrix(math.pi,[0,1,0]))
prints={"1_kopfrahmen":frame_m,"2_kopfbuegel_kopfueber":halo_print,"3_kinnhebel":MESH["kinnhebel_local"],"4_traeger_mit_wiege":MESH["traeger"],
        "5_gelenkbolzen_M10x82":FM["gelenkbolzen"],"6_gabelschraube_M10x31_2x":FM["gabelschraube"],"7_buegelschraube_M10x20_2x":FM["buegelschraube"],"8_mutter_M10_3x":FM["mutter"]}
for k,m in prints.items():
    m=on_bed(m); e=m.extents; print("DRUCK",k,f"{e[0]:.0f} x {e[1]:.0f} x {e[2]:.0f} mm", "dicht" if m.is_watertight else "NICHT DICHT")
    m.export(f"print/{k}.stl")
# --- Viewer (grobe Netze) ---
F.rod.__defaults__=(40, F.P/4)
FMv={"gelenkbolzen":F.bolt(82,smooth_to=58),"gabelschraube":F.bolt(31),"buegelschraube":F.bolt(20),"mutter":F.nut()}
vm={"kopfrahmen":mesh_of(P["kopfrahmen"],0.15),"kopfbuegel":mesh_of(P["kopfbuegel"],0.15),"traeger":mesh_of(P["traeger"],0.15),"kinnhebel":mesh_of(lev,0.15)}
vm["phone"]=mesh_of(phone,0.2)
head=trimesh.creation.icosphere(subdivisions=4,radius=1.0); head.apply_scale([70,100,125]); head.apply_translation([0,350,g.Z_PAD-20]); vm["kopf"]=head
fast=[]
for nm,key,M in place:
    m=FMv[key].copy(); m.apply_transform(M); fast.append(m)
vm["verbinder"]=trimesh.util.concatenate(fast)
d={}
for k,m in vm.items():
    m.export(f"viewer/m/{k}.stl"); d[k]=base64.b64encode(open(f"viewer/m/{k}.stl","rb").read()).decode()
    print("viewer",k,len(m.faces))
open("viewer/models.js","w").write("window.MODELS="+json.dumps(d)+";")
print("viewer", round(os.path.getsize("viewer/models.js")/1e6,1),"MB", "pivot",round(g.Y_PIV,2),g.Z_PIV, f"{time.time()-t0:.0f}s")
