"""Personaje Mateo Rivas: Blender -b --factory-startup -P generate_mateo.py -- --output <raíz del proyecto> [--previews]."""
import argparse, json, math, sys
from pathlib import Path
import bpy, bmesh
from mathutils import Vector, Matrix, Quaternion, Euler

PALETTE = {'Casaca':'6B6B3E','Chalina':'8E8C8A','Piel':'C99A7A','Pelo':'3A2A22','Pantalon':'2F3340','Zapatillas':'E3DED4','Morral':'A08B6A'}
MATS = {}
PARTS = []
RULES = []
FPS = 30
TAU = math.tau
# A-pose ligera: brazos 35° bajo la horizontal. Mateo mira hacia -Y; su izquierda es +X.
ARM_DOWN = math.radians(35)
SHOULDER_X, SHOULDER_Z = .178, 1.385
UPPER_ARM, LOWER_ARM, HAND = .28, .25, .10
HIP_X = .09

def materials():
    for name,color in PALETTE.items():
        mat = bpy.data.materials.new('M_'+name)
        rgb = [int(color[i:i+2],16)/255 for i in (0,2,4)]
        rgb = [v/12.92 if v<.04045 else ((v+.055)/1.055)**2.4 for v in rgb]
        mat.diffuse_color = (*rgb,1)
        mat.use_nodes = True
        node = mat.node_tree.nodes.get('Principled BSDF')
        node.inputs['Base Color'].default_value = (*rgb,1)
        node.inputs['Roughness'].default_value = .78
        MATS[name] = mat

# ---------------------------------------------------------------- geometría

def from_bm(name, bm, mat, rule, smooth=True):
    """Cada pieza lleva un grupo PART:<regla> que decide qué huesos pueden moverla."""
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    mesh = bpy.data.meshes.new(name)
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(MATS[mat])
    for p in mesh.polygons: p.use_smooth = smooth
    # Los nombres de grupo se truncan a 63 caracteres: se guarda un índice a RULES.
    if rule not in RULES: RULES.append(rule)
    group = obj.vertex_groups.new(name='PART:%d' % RULES.index(rule))
    group.add(range(len(mesh.vertices)), 1, 'REPLACE')
    PARTS.append(obj)
    return obj

def mesh_part(name, verts, faces, mat, rule, smooth=True):
    bm = bmesh.new()
    vs = [bm.verts.new(v) for v in verts]
    for f in faces: bm.faces.new([vs[i] for i in f])
    return from_bm(name, bm, mat, rule, smooth)

def ellipsoid(name, center, size, mat, rule, rot=(0,0,0), basis=None, seg=12, rings=8):
    bm = bmesh.new()
    bmesh.ops.create_uvsphere(bm, u_segments=seg, v_segments=rings, radius=1)
    m = (basis if basis is not None else Euler(rot).to_matrix()) @ Matrix.Diagonal(Vector(size)/2)
    bmesh.ops.transform(bm, matrix=m.to_4x4(), verts=bm.verts)
    bmesh.ops.translate(bm, vec=Vector(center), verts=bm.verts)
    return from_bm(name, bm, mat, rule, True)

def box(name, center, size, mat, rule, bevel=.01, rot=(0,0,0), basis=None):
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1)
    bmesh.ops.scale(bm, vec=Vector(size), verts=bm.verts)
    if bevel:
        bmesh.ops.bevel(bm, geom=list(bm.edges), offset=min(bevel, min(size)*.3), segments=2, profile=.5, affect='EDGES', clamp_overlap=True)
    m = basis if basis is not None else Euler(rot).to_matrix()
    bmesh.ops.transform(bm, matrix=m.to_4x4(), verts=bm.verts)
    bmesh.ops.translate(bm, vec=Vector(center), verts=bm.verts)
    return from_bm(name, bm, mat, rule, False)

def loft(name, points, radii, mat, rule, seg=12, refs=(0,-1,0), closed=False, cap=.5, smooth=True):
    """Tubo por anillos elípticos: rx sobre t×ref, ry hacia ref. Tapas abovedadas."""
    pts = [Vector(p) for p in points]
    n = len(pts)
    verts, faces, frames = [], [], []
    for i,p in enumerate(pts):
        a, b = (pts[i-1], pts[(i+1)%n]) if closed else (pts[max(i-1,0)], pts[min(i+1,n-1)])
        t = (b-a).normalized()
        ref = Vector(refs[i]) if isinstance(refs, list) else Vector(refs)
        x = t.cross(ref)
        if x.length < 1e-6: x = t.orthogonal()
        x.normalize()
        y = x.cross(t).normalized()
        rx, ry = radii[i] if isinstance(radii[i], (tuple, list)) else (radii[i], radii[i])
        frames.append((t, x, y, rx, ry))
        for k in range(seg):
            ang = TAU*k/seg
            verts.append(p + x*rx*math.cos(ang) + y*ry*math.sin(ang))
    for i in range(n if closed else n-1):
        j = (i+1) % n
        for k in range(seg):
            k1 = (k+1) % seg
            faces.append((i*seg+k, i*seg+k1, j*seg+k1, j*seg+k))
    if not closed:
        for idx, sign in ((0,-1), (n-1,1)):
            t, x, y, rx, ry = frames[idx]
            p = pts[idx]
            base = len(verts)
            for k in range(seg):
                ang = TAU*k/seg
                verts.append(p + t*sign*cap*min(rx,ry)*.75 + x*rx*.68*math.cos(ang) + y*ry*.68*math.sin(ang))
            verts.append(p + t*sign*cap*min(rx,ry))
            c = len(verts)-1
            for k in range(seg):
                k1 = (k+1) % seg
                faces.append((idx*seg+k, idx*seg+k1, base+k1, base+k))
                faces.append((base+k, base+k1, c))
    return mesh_part(name, verts, faces, mat, rule, smooth)

# Casaca: (z, rx, ry). El centro avanza hacia -Y con la altura: espalda algo encorvada.
JACKET = [(.79,.172,.152),(.85,.164,.138),(.95,.152,.116),(1.05,.147,.106),(1.15,.152,.11),(1.25,.157,.116),(1.32,.16,.116),(1.38,.153,.106),(1.42,.128,.088),(1.455,.085,.068),(1.47,.06,.055)]
def hunch(z): return -.03*max(0,(z-1.0)/.47)**2
def jacket_ring(z):
    for (z0,a0,b0),(z1,a1,b1) in zip(JACKET, JACKET[1:]):
        if z <= z1:
            f = min(max((z-z0)/(z1-z0),0),1)
            return a0+(a1-a0)*f, b0+(b1-b0)*f, hunch(z)
    return JACKET[-1][1], JACKET[-1][2], hunch(z)
def on_jacket(z, deg, off):
    rx, ry, cy = jacket_ring(z)
    a = math.radians(deg)
    n = Vector((math.cos(a)/rx, math.sin(a)/ry, 0)).normalized()
    return Vector((rx*math.cos(a), cy+ry*math.sin(a), z)) + n*off, n

def arm_dir(side):
    return Vector((side*math.cos(ARM_DOWN), 0, -math.sin(ARM_DOWN)))
def arm_point(side, s):
    return Vector((side*SHOULDER_X, 0, SHOULDER_Z)) + arm_dir(side)*s

def body():
    # Rostro sin rasgos: cabeza ovalada lisa, solo orejas y pelo.
    ellipsoid('Cabeza', (0,-.036,1.574), (.18,.212,.252), 'Piel', 'rigid:Head', seg=20, rings=14)
    loft('Cuello', [(0,-.005,1.37),(0,-.018,1.45),(0,-.03,1.53)], [.046,.044,.043], 'Piel', 'auto:UpperChest,Neck,Head', seg=12)
    for sx in (1,-1):
        ellipsoid('Oreja', (sx*.087,-.004,1.572), (.018,.036,.052), 'Piel', 'rigid:Head', rot=(0,0,sx*.25), seg=10, rings=6)
    hair()
    # Casaca holgada.
    loft('Casaca', [(0,hunch(z),z) for z,_,_ in JACKET], [(a,b) for _,a,b in JACKET], 'Casaca', 'skirt:Hips,Spine,Chest,UpperChest,Neck,LeftShoulder,RightShoulder', seg=20, refs=(0,-1,0), cap=.15)
    hem = [on_jacket(.805, d, .004)[0] for d in range(0,360,18)]
    loft('Basta casaca', hem, [(.012,.022)]*len(hem), 'Casaca', 'skirt:Hips,Spine', seg=6, refs=(0,0,1), closed=True)
    zipper = [on_jacket(z,-90,.006)[0] for z in (.80,.9,1.0,1.1,1.2,1.3,1.38,1.43)]
    loft('Cierre', zipper, [(.007,.003)]*len(zipper), 'Chalina', 'skirt:Hips,Spine,Chest,UpperChest', seg=6, refs=[on_jacket(z,-90,0)[1] for z in (.80,.9,1.0,1.1,1.2,1.3,1.38,1.43)], cap=0)
    for sx in (1,-1):
        p, n = on_jacket(.93, -90+sx*42, .008)
        t = Vector((0,0,1)).cross(n).normalized()
        box('Bolsillo', p, (.10,.014,.10), 'Casaca', 'skirt:Hips,Spine', bevel=.006, basis=Matrix((t, n, (0,0,1))).transposed())
    for side, label in ((1,'Left'),(-1,'Right')):
        d = arm_dir(side)
        s_list = [-.03,.0,.05,.11,.17,.215,.25,.28,.31,.34,.39,.44,.49,.515]
        radii = [.047,.052,.054,.053,.052,.051,.05,.049,.049,.048,.048,.047,.047,.048]
        loft('Manga', [arm_point(side,s) for s in s_list], radii, 'Casaca', f'auto:{label}Shoulder,{label}UpperArm,{label}LowerArm', seg=14, cap=.9)
        cuff = [arm_point(side,s) for s in (.47,.495,.52)]
        loft('Puño', cuff, [.051,.053,.052], 'Casaca', f'auto:{label}LowerArm', seg=12, cap=.05)
        hand(side, label)
        leg(side, label)
    pants_top = [(0,0,z) for z in (.75,.80,.88,.97)]
    loft('Pantalon pelvis', pants_top, [(.11,.085),(.13,.095),(.135,.1),(.13,.095)], 'Pantalon', 'rigid:Hips', seg=16, cap=.4)
    scarf()
    bag()

def hair():
    rule = 'rigid:Head'
    # Corto y algo desordenado: casco amplio y pocos mechones planos que rompen la silueta.
    ellipsoid('Pelo casco', (0,-.02,1.632), (.198,.226,.17), 'Pelo', rule, seg=18, rings=10)
    ellipsoid('Pelo nuca', (0,.018,1.58), (.168,.13,.14), 'Pelo', rule, seg=14, rings=8)
    tufts = [((.0,-.112,1.66),(.13,.05,.062),(.45,0,.05)),
             ((.05,-.103,1.652),(.07,.04,.06),(.35,.1,.45)),
             ((-.052,-.104,1.656),(.072,.04,.058),(.3,-.1,-.5)),
             ((.03,-.05,1.69),(.1,.09,.026),(.2,.12,.4)),
             ((-.035,.02,1.693),(.1,.09,.026),(.1,-.15,-.3)),
             ((.03,.07,1.68),(.09,.07,.04),(-.6,.2,.3)),
             ((.088,-.03,1.625),(.03,.11,.07),(0,.12,.1)),
             ((-.088,-.03,1.625),(.03,.11,.07),(0,-.12,-.1))]
    for i,(c,s,r) in enumerate(tufts):
        ellipsoid('Mechon %d'%i, c, s, 'Pelo', rule, rot=r, seg=10, rings=6)

def hand(side, label):
    d = arm_dir(side)
    w = arm_point(side, .52)
    across = Vector((0,1,0))
    n = d.cross(across).normalized()
    basis = Matrix((d, across, n)).transposed()
    loft('Muñeca', [w-d*.03, w+d*.025], [.027,.03], 'Piel', f'auto:{label}LowerArm,{label}Hand', seg=10, cap=.3)
    ellipsoid('Palma', w+d*.06, (.105,.088,.036), 'Piel', f'rigid:{label}Hand', basis=basis, seg=12, rings=8)
    ellipsoid('Dedos', w+d*.125+n*.004, (.085,.082,.03), 'Piel', f'rigid:{label}Hand', basis=basis @ Euler((0,math.radians(10*side),0)).to_matrix(), seg=12, rings=6)
    thumb_dir = (d*.8 + Vector((0,-1,0))).normalized()
    tn = thumb_dir.cross(n).normalized()
    ellipsoid('Pulgar', w+d*.055+Vector((0,-.035,0)), (.055,.024,.024), 'Piel', f'rigid:{label}Hand', basis=Matrix((thumb_dir, n.cross(thumb_dir), n)).transposed(), seg=10, rings=6)

def leg(side, label):
    x = side*HIP_X
    zs = [.89,.84,.76,.66,.58,.53,.50,.47,.43,.35,.25,.16,.105]
    rs = [.058,.066,.068,.062,.057,.055,.054,.054,.055,.054,.05,.049,.052]
    pts = [(x*(1-.03*(1-z)), -.006*math.exp(-((z-.5)/.08)**2), z) for z in zs]
    loft('Pierna', pts, rs, 'Pantalon', f'auto:Hips,{label}UpperLeg,{label}LowerLeg', seg=14, cap=.1)
    # Zapatilla: perfil a lo largo de -Y, suela plana en z=0.
    fx = x*.98
    prof = [(.07,.032,.028),(.055,.043,.042),(.015,.048,.055),(-.035,.05,.048),(-.09,.051,.037),(-.14,.047,.03),(-.17,.036,.024),(-.185,.02,.018)]
    loft('Zapatilla', [(fx,y,.012+ry) for y,rx,ry in prof], [(rx,ry) for _,rx,ry in prof], 'Zapatillas', f'auto:{label}Foot,{label}Toes', seg=12, refs=(0,0,1), cap=.35)
    box('Suela', (fx,-.058,.009), (.082,.225,.018), 'Zapatillas', f'auto:{label}Foot,{label}Toes', bevel=.008)

def scarf():
    rule = 'auto:Neck,UpperChest,Chest'
    loop1 = [Vector((.098*math.cos(a), -.02+.09*math.sin(a), 1.432-.012*math.sin(a))) for a in [TAU*i/20 for i in range(20)]]
    loft('Chalina vuelta', loop1, [(.034,.036)]*20, 'Chalina', rule, seg=10, refs=(0,0,1), closed=True)
    loop2 = [Vector((.084*math.cos(a), -.024+.078*math.sin(a), 1.47-.018*math.sin(a))) for a in [TAU*i/20 for i in range(20)]]
    loft('Chalina vuelta alta', loop2, [(.028,.03)]*20, 'Chalina', rule, seg=10, refs=(0,0,1), closed=True)
    # Punta delantera que cae sobre el pecho, algo a la izquierda.
    front = [(1.42,-72,.045),(1.34,-74,.03),(1.25,-76,.026),(1.16,-77,.026),(1.09,-78,.026)]
    pts, refs = [], []
    for z,deg,off in front:
        p, n = on_jacket(z, deg, off); pts.append(p); refs.append(n)
    loft('Chalina punta', pts, [(.042,.012),(.045,.012),(.046,.012),(.046,.012),(.044,.012)], 'Chalina', rule, seg=8, refs=refs, cap=.2)
    for i,(z,deg,off) in enumerate(front[-1:]):
        for k in range(3):
            p, n = on_jacket(z-.03, deg-6+k*6, off)
            ellipsoid('Fleco', p, (.01,.01,.04), 'Chalina', rule, seg=6, rings=4)
    # Punta trasera por encima del hombro derecho.
    back = [(1.43,110,.04),(1.35,112,.028),(1.26,114,.024),(1.2,115,.024)]
    pts, refs = [], []
    for z,deg,off in back:
        p, n = on_jacket(z, deg, off); pts.append(p); refs.append(n)
    loft('Chalina espalda', pts, [(.044,.012)]*len(pts), 'Chalina', rule, seg=8, refs=refs, cap=.2)

BAG_ANGLE, BAG_RADIUS, BAG_Z = 141, .205, .88
def bag():
    a = math.radians(BAG_ANGLE)
    out = Vector((math.cos(a), math.sin(a), 0))
    tx = Vector((0,0,1)).cross(out)  # tangente: +X local
    center = out*BAG_RADIUS + Vector((0,0,BAG_Z))
    basis = Matrix((tx, out, (0,0,1))).transposed()
    box('Morral', center, (.25,.075,.22), 'Morral', 'rigid:Hips', bevel=.022, basis=basis)
    box('Solapa', center+out*.043+Vector((0,0,.045)), (.256,.016,.14), 'Morral', 'rigid:Hips', bevel=.007, basis=basis)
    box('Hebilla', center+out*.054+Vector((0,0,-.01)), (.035,.01,.05), 'Morral', 'rigid:Hips', bevel=.004, basis=basis)
    # Correa cruzada: hombro izquierdo -> cadera derecha (delante y detrás).
    ends = (center - tx*.105 + Vector((0,0,.10)), center + tx*.105 + Vector((0,0,.10)))
    front = [(.99,-172),(1.04,-150),(1.12,-128),(1.22,-104),(1.31,-82),(1.385,-60),(1.43,-38),(1.452,-14)]
    back = [(1.452,14),(1.43,38),(1.385,60),(1.30,78),(1.20,92),(1.10,104),(1.02,114)]
    pts, refs = [ends[0]], []
    for z,deg in front+back:
        p, n = on_jacket(z, deg, .011); pts.append(p); refs.append(n)
    pts.append(ends[1])
    refs = [refs[0]] + refs + [refs[-1]]
    loft('Correa', pts, [(.021,.005)]*len(pts), 'Morral', 'auto:Hips,Spine,Chest,UpperChest,LeftShoulder', seg=8, refs=refs, cap=0, smooth=False)

# ---------------------------------------------------------------- rig

BONE_NAMES = []
def build_rig():
    arm = bpy.data.armatures.new('CH_Mateo_Rig')
    rig = bpy.data.objects.new('CH_Mateo_Rig', arm)
    bpy.context.collection.objects.link(rig)
    bpy.context.view_layer.objects.active = rig
    rig.select_set(True)
    bpy.ops.object.mode_set(mode='EDIT')
    eb = arm.edit_bones
    def bone(name, head, tail, parent=None, connect=False):
        b = eb.new(name)
        b.head, b.tail = Vector(head), Vector(tail)
        along = (Vector(tail)-Vector(head)).normalized()
        b.align_roll(Vector((0,0,1)) if abs(along.y) > .7 else Vector((0,-1,0)))
        if parent:
            b.parent = eb[parent]
            b.use_connect = connect
        BONE_NAMES.append(name)
    bone('Hips', (0,0,.95), (0,0,1.04))
    bone('Spine', (0,0,1.04), (0,-.002,1.15), 'Hips', True)
    bone('Chest', (0,-.002,1.15), (0,-.008,1.26), 'Spine', True)
    bone('UpperChest', (0,-.008,1.26), (0,-.02,1.385), 'Chest', True)
    bone('Neck', (0,-.02,1.385), (0,-.03,1.50), 'UpperChest', True)
    bone('Head', (0,-.03,1.50), (0,-.036,1.70), 'Neck', True)
    for side, label in ((1,'Left'),(-1,'Right')):
        d = arm_dir(side)
        sh = arm_point(side,0)
        elbow, wrist = arm_point(side,UPPER_ARM), arm_point(side,UPPER_ARM+LOWER_ARM)
        bone(label+'Shoulder', (side*.03,-.012,1.365), sh, 'UpperChest')
        bone(label+'UpperArm', sh, elbow, label+'Shoulder', True)
        bone(label+'LowerArm', elbow, wrist, label+'UpperArm', True)
        bone(label+'Hand', wrist, wrist+d*HAND, label+'LowerArm', True)
        x = side*HIP_X
        bone(label+'UpperLeg', (x,0,.92), (x,-.008,.495), 'Hips')
        bone(label+'LowerLeg', (x,-.008,.495), (x*.98,.006,.09), label+'UpperLeg', True)
        bone(label+'Foot', (x*.98,.006,.09), (x*.98,-.10,.03), label+'LowerLeg', True)
        bone(label+'Toes', (x*.98,-.10,.03), (x*.98,-.17,.03), label+'Foot', True)
    bpy.ops.object.mode_set(mode='OBJECT')
    return rig

def parse_rule(rule):
    kind, names = rule.split(':')
    names = [n.strip() for n in names.split(',')]
    return kind, names

def seg_distance(p, a, b):
    ab = b-a
    t = min(max((p-a).dot(ab)/ab.length_squared, 0), 1)
    return (a+ab*t-p).length

def skin(mesh, rig):
    """Pesos automáticos (calor de huesos) y luego reglas por pieza: sin ropa arrastrada por huesos ajenos."""
    bpy.ops.object.select_all(action='DESELECT')
    mesh.select_set(True); rig.select_set(True)
    bpy.context.view_layer.objects.active = rig
    bpy.ops.object.parent_set(type='ARMATURE_AUTO')
    groups = {g.index: g.name for g in mesh.vertex_groups}
    bones = {b.name: (b.head_local.copy(), b.tail_local.copy()) for b in rig.data.bones}
    stats = {'fallback_vertices':0, 'max_influences':0}
    raw = []
    for v in mesh.data.vertices:
        rule, weights = None, {}
        for g in v.groups:
            name = groups[g.group]
            if name.startswith('PART:'): rule = RULES[int(name[5:])]
            elif g.weight > 0: weights[name] = g.weight
        raw.append((rule, weights))
    # Piezas finas (correa, chalina, basta) donde el calor de huesos no llega: copian el peso
    # del vértice de ropa más cercano que sí lo tiene, así se pegan a la casaca al animar.
    from mathutils.kdtree import KDTree
    donors = [i for i,(rule,w) in enumerate(raw) if not rule.startswith('rigid') and w]
    tree = KDTree(len(donors))
    for k,i in enumerate(donors): tree.insert(mesh.data.vertices[i].co, i)
    tree.balance()
    new_weights = []
    for v,(rule,weights) in zip(mesh.data.vertices, raw):
        kind, allowed = parse_rule(rule)
        if kind == 'rigid':
            result = {allowed[0]: 1.0}
            new_weights.append(result)
            continue
        else:
            result = {k:w for k,w in weights.items() if k in allowed}
            if sum(result.values()) < 1e-4:
                stats['fallback_vertices'] += 1
                stats.setdefault('fallback_by_rule',{}).setdefault(rule,0)
                stats['fallback_by_rule'][rule] += 1
                for co, idx, dist in tree.find_n(v.co, 24):
                    cand = {k:w for k,w in raw[idx][1].items() if k in allowed}
                    if sum(cand.values()) > 1e-3:
                        result = cand
                        break
                if sum(result.values()) < 1e-4:
                    dist = sorted((seg_distance(v.co, *bones[b]), b) for b in allowed)[:2]
                    result = {b: 1/max(dd,1e-3)**4 for dd,b in dist}
            top = sorted(result.items(), key=lambda kv: -kv[1])[:4]
            top = [(k,w) for k,w in top if w > .01*top[0][1]]
            total = sum(w for _,w in top)
            result = {k: w/total for k,w in top}
            if kind == 'skirt' and v.co.z < .95:
                # Faldón de la casaca: cede parte del peso al muslo del mismo lado.
                f = min(1, (.95-v.co.z)/.14) * .7 * min(1, abs(v.co.x)/.06)
                leg = ('Left' if v.co.x > 0 else 'Right') + 'UpperLeg'
                result = {k: w*(1-f) for k,w in result.items()}
                result[leg] = result.get(leg, 0) + f
                top = sorted(result.items(), key=lambda kv: -kv[1])[:4]
                total = sum(w for _,w in top)
                result = {k: w/total for k,w in top}
        stats['max_influences'] = max(stats['max_influences'], len(result))
        new_weights.append(result)
    for g in list(mesh.vertex_groups): mesh.vertex_groups.remove(g)
    for name in BONE_NAMES: mesh.vertex_groups.new(name=name)
    for i, result in enumerate(new_weights):
        for name, w in result.items(): mesh.vertex_groups[name].add([i], w, 'REPLACE')
    return stats

# ---------------------------------------------------------------- animación

def bump(t, c, w):
    d = (t-c+.5) % 1 - .5
    return math.exp(-(d/w)**2)

SOLE = {}  # puntos de suela en reposo por hueso
def sole_points(rig):
    for label in ('Left','Right'):
        x = rig.data.bones[label+'Foot'].head_local.x
        SOLE[label] = [(label+'Foot', Vector((x,.075,0))), (label+'Foot', Vector((x,-.10,0))), (label+'Toes', Vector((x,-.18,0)))]

def hinge(side, along):
    """Eje de flexión del codo en reposo: lleva el antebrazo hacia delante (-Y)."""
    return along.cross(Vector((0,-1,0))).normalized()

X, Y, Z = (1,0,0), (0,1,0), (0,0,1)
def mirror(ops, side):
    return [(ax, deg if (ax==X or side==1 or isinstance(ax,Vector)) else -deg) for ax,deg in ops]

def arm_ops(R, side, label, down, pitch, elbow, wrist=8, twist=0, inward=0):
    d = arm_dir(side)
    h = hinge(side, d)
    R[label+'UpperArm'] = mirror([(Y,down),(Z,twist),(X,pitch)], side)
    R[label+'LowerArm'] = [(h, elbow), (Z, -side*inward)]
    R[label+'Hand'] = [(h, wrist)]

def leg_ops(R, label, thigh, knee, foot_world, toes=0, splay=0):
    R[label+'UpperLeg'] = [(Y, splay), (X, thigh)]
    R[label+'LowerLeg'] = [(X, knee)]
    R[label+'Foot'] = [(X, foot_world-(thigh+knee))]
    R[label+'Toes'] = [(X, toes)]

def walk_pose(t):
    p = TAU*t
    R = {}
    for side, label, ph in ((1,'Left',0.0), (-1,'Right',.5)):
        tt = (t+ph) % 1
        q = TAU*tt
        thigh = -23*math.sin(q)
        knee = 5 + 42*max(0, math.cos(q+.45))**1.6 + 7*bump(tt,.31,.06)
        foot = -13*bump(tt,.25,.05) + 30*bump(tt,.70,.065) - 7*bump(tt,.93,.08)
        toes = -max(0, foot)*bump(tt,.70,.09)
        leg_ops(R, label, thigh, knee, foot, toes)
        arm_ops(R, side, label, 47, 15*math.sin(q) - 2, 14 + 14*max(0,-math.sin(q)), 10)
        R[label+'Shoulder'] = mirror([(Z,-5),(Y,3)], side)
    R['Hips'] = [(Z, -5*math.sin(p)), (Y, 2*math.cos(p))]
    R['Spine'] = [(X, 2.5), (Z, 3*math.sin(p))]
    R['Chest'] = [(X, 2), (Z, 4*math.sin(p))]
    R['UpperChest'] = [(X, 1.5)]
    R['Neck'] = [(X, 2), (Z, -1*math.sin(p))]
    R['Head'] = [(X, 6 + 1.2*math.cos(2*p)), (Z, -1*math.sin(p))]
    return R, Vector((-.012*math.cos(p), 0, 0))

def idle_pose(t):
    p = TAU*t
    breath = math.sin(p)
    sway = .007*math.sin(p+1.0)
    roll = 1.0*math.sin(p+1.0)
    comp = math.degrees(math.atan(sway/.85))
    R = {}
    for side, label in ((1,'Left'),(-1,'Right')):
        thigh, knee = (1.5, 2.5) if side == 1 else (-3.5, 8)
        leg_ops(R, label, thigh, knee, 0, 0, splay=comp-roll)
        arm_ops(R, side, label, 46, -2 + .8*breath, 11, 9)
        R[label+'Shoulder'] = mirror([(Z,-6),(Y,3.5 - 1.6*breath)], side)
    R['Hips'] = [(Y, roll)]
    R['Spine'] = [(X, 2.5)]
    R['Chest'] = [(X, 1.5 - 1.3*breath)]
    R['UpperChest'] = [(X, 1.5 - 1.0*breath)]
    R['Neck'] = [(X, 3 + 1.0*math.sin(p-.6))]
    R['Head'] = [(X, 8 + 3.0*math.sin(p-.9)), (Z, 2.0*math.sin(p+.4))]
    return R, Vector((sway, 0, 0))

def phone_pose(t):
    p = TAU*t
    breath = math.sin(p)
    R = {}
    leg_ops(R, 'Left', 1.5, 3, 0)
    leg_ops(R, 'Right', -3, 7, 0)
    arm_ops(R, 1, 'Left', 46, -2 + .8*breath, 12, 9)
    # Mano derecha a la altura del pecho, mirando el celular.
    arm_ops(R, -1, 'Right', 52, -30 + 1.2*breath, 84, -15, inward=34)
    R['LeftShoulder'] = mirror([(Z,-6),(Y,3.5 - 1.5*breath)], 1)
    R['RightShoulder'] = mirror([(Z,-9),(Y,2 - 1.5*breath)], -1)
    R['Hips'] = [(X, 0)]
    R['Spine'] = [(X, 3)]
    R['Chest'] = [(X, 2 - 1.2*breath)]
    R['UpperChest'] = [(X, 2 - 1.0*breath)]
    R['Neck'] = [(X, 8)]
    R['Head'] = [(X, 16 + 2*math.sin(p)), (Z, -6 + 2.5*math.sin(p+1))]
    return R, Vector((0,0,0))

def apply_pose(rig, R, hips_offset):
    for pb in rig.pose.bones:
        pb.rotation_mode = 'QUATERNION'
        pb.rotation_quaternion = (1,0,0,0)
        pb.location = (0,0,0)
    for name, ops in R.items():
        q = Quaternion()
        for axis, deg in ops: q = Quaternion(Vector(axis), math.radians(deg)) @ q
        m = rig.data.bones[name].matrix_local.to_quaternion()
        rig.pose.bones[name].rotation_quaternion = m.inverted() @ q @ m
    hips = rig.pose.bones['Hips']
    rest = rig.data.bones['Hips'].matrix_local.to_3x3()
    hips.location = rest.inverted() @ hips_offset
    bpy.context.view_layer.update()
    # Apoyo automático: la suela más baja toca el suelo en cada fotograma.
    low = min((rig.pose.bones[b].matrix @ rig.data.bones[b].matrix_local.inverted() @ pnt).z for pts in SOLE.values() for b,pnt in pts)
    hips.location = rest.inverted() @ (hips_offset - Vector((0,0,low)))
    bpy.context.view_layer.update()

def bake_action(rig, name, pose, frames, step):
    # Primero se calculan las poses sin animación activa; luego se insertan las claves.
    rig.animation_data_create()
    rig.animation_data.action = None
    rig.animation_data.use_nla = False
    baked, previous = [], {}
    for f in range(0, frames+1, step):
        R, offset = pose((f % frames)/frames)
        apply_pose(rig, R, offset)
        values = {}
        for pb in rig.pose.bones:
            q = pb.rotation_quaternion.copy()
            if pb.name in previous and previous[pb.name].dot(q) < 0: q.negate()
            previous[pb.name] = values[pb.name] = q
        baked.append((f, values, rig.pose.bones['Hips'].location.copy()))
    act = bpy.data.actions.new(name)
    act.use_fake_user = True
    rig.animation_data.action = act
    for f, values, hips in baked:
        for pb in rig.pose.bones:
            pb.rotation_quaternion = values[pb.name]
            pb.keyframe_insert('rotation_quaternion', frame=f, group=pb.name)
        rig.pose.bones['Hips'].location = hips
        rig.pose.bones['Hips'].keyframe_insert('location', frame=f, group='Hips')
    act.use_frame_range = True
    act.frame_start, act.frame_end = 0, frames
    act.use_cyclic = True
    rig.animation_data.action = None
    rig.animation_data.use_nla = True
    return act

CLIPS = [('Idle', idle_pose, 60, 2, 0), ('Walk', walk_pose, 30, 1, 100), ('LookPhone', phone_pose, 60, 2, 200)]
def animate(rig):
    sole_points(rig)
    info = []
    for name, pose, frames, step, start in CLIPS:
        act = bake_action(rig, name, pose, frames, step)
        track = rig.animation_data.nla_tracks.new()
        track.name = name
        strip = track.strips.new(name, start, act)
        strip.name = name
        strip.extrapolation = 'NOTHING'
        info.append({'name':name, 'frames':frames, 'seconds':frames/FPS, 'scene_start':start})
    for pb in rig.pose.bones:
        pb.rotation_quaternion = (1,0,0,0); pb.location = (0,0,0)
    return info

# ---------------------------------------------------------------- miniaturas

def point_at(obj, target): obj.rotation_euler = (Vector(target)-obj.location).to_track_quat('-Z','Y').to_euler()

def preview_scene():
    scene = bpy.context.scene
    scene.render.engine = 'BLENDER_EEVEE'
    scene.render.resolution_x = scene.render.resolution_y = 512
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = 'PNG'
    scene.render.image_settings.color_mode = 'RGBA'
    scene.render.film_transparent = False
    scene.view_settings.view_transform = 'AgX'
    scene.eevee.taa_render_samples = 64
    scene.world = bpy.data.worlds.new('Fondo neutro')
    scene.world.use_nodes = True
    scene.world.node_tree.nodes['Background'].inputs[0].default_value = (.22,.23,.25,1)
    scene.world.node_tree.nodes['Background'].inputs[1].default_value = .65
    center, span = Vector((0,0,.86)), 1.8
    bpy.ops.mesh.primitive_plane_add(size=40, location=(0,0,-.001))
    ground = bpy.context.object
    ground.name = 'PREVIEW_Suelo'
    mat = bpy.data.materials.new('PREVIEW_Neutro')
    mat.diffuse_color = (.23,.24,.26,1)
    ground.data.materials.append(mat)
    for label, position, power, size in [('Principal',(-3,-4,6),700,4),('Relleno',(4,-1,3),350,3),('Borde',(0,4,5),500,3)]:
        bpy.ops.object.light_add(type='AREA', location=center+Vector(position)*span)
        light = bpy.context.object
        light.name = 'PREVIEW_'+label
        light.data.energy = power*span*span
        light.data.shape = 'DISK'
        light.data.size = size*span
        point_at(light, center)
    bpy.ops.object.camera_add()
    camera = bpy.context.object
    camera.name = 'PREVIEW_Camera'
    camera.data.type = 'ORTHO'
    camera.data.ortho_scale = 1.95
    camera.data.clip_start, camera.data.clip_end = .01, 100
    scene.camera = camera
    return scene, camera, center

def shoot(scene, camera, center, direction, frame, path):
    scene.frame_set(frame)
    camera.location = center + Vector(direction).normalized()*6
    point_at(camera, center)
    scene.render.filepath = str(path)
    bpy.ops.render.render(write_still=True)
    print('PREVIEW_OK', path.name, flush=True)

def stitch(paths, out):
    import numpy as np
    arrays = []
    for p in paths:
        img = bpy.data.images.load(str(p))
        w, h = img.size
        px = np.empty(w*h*4, dtype=np.float32)
        img.pixels.foreach_get(px)
        arrays.append(px.reshape(h, w, 4))
        bpy.data.images.remove(img)
    strip = np.concatenate(arrays, axis=1)
    h, w = strip.shape[:2]
    img = bpy.data.images.new('Tira Walk', w, h, alpha=True)
    img.pixels.foreach_set(strip.ravel())
    img.filepath_raw = str(out)
    img.file_format = 'PNG'
    img.save()

def render_previews(source, temp):
    folder = source/'previews'
    folder.mkdir(parents=True, exist_ok=True)
    scene, camera, center = preview_scene()
    views = {'frente':(0,-1,.12), 'perfil':(1,0,.12), 'tres_cuartos':(1.35,-1.9,1.0)}
    for label, direction in views.items():
        shoot(scene, camera, center, direction, 0, folder/f'CH_Mateo_{label}.png')
    shoot(scene, camera, center, (0,-1,.12), 80, folder/'CH_Mateo_reposo_apose.png')
    shoot(scene, camera, center, (1.35,-1.9,1.0), 215, folder/'CH_Mateo_lookphone.png')
    frames = [100, 108, 115, 123]
    temp.mkdir(parents=True, exist_ok=True)
    parts = []
    for f in frames:
        path = temp/f'mateo_walk_{f}.png'
        shoot(scene, camera, center, (1,0,.12), f, path)
        parts.append(path)
    stitch(parts, folder/'CH_Mateo_walk_tira.png')
    # Primeros planos de control (solo temporales): busto y articulaciones en poses extremas.
    camera.data.ortho_scale = .9
    for label, direction, frame, c in [('busto_frente',(0,-1,.1),0,(0,0,1.35)),('busto_perfil',(1,0,.1),0,(0,0,1.35)),('busto_perfil_reposo',(1,0,.1),80,(0,0,1.35)),('busto_perfil_der',(-1,0,.1),0,(0,0,1.35)),('busto_espalda',(.4,1,.3),0,(0,0,1.35)),('cadera_walk',(1.2,-1,.2),108,(0,0,.85)),('phone_frente',(0,-1,.1),215,(0,0,1.3)),('cabeza_walk',(1,-.25,.1),108,(0,-.05,1.55))]:
        shoot(scene, camera, Vector(c), direction, frame, temp/f'mateo_{label}.png')
    camera.data.ortho_scale = 1.95

# ---------------------------------------------------------------- principal

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--output', type=Path, required=True, help='Raíz del proyecto')
    parser.add_argument('--previews', action='store_true', help='Renderizar miniaturas Eevee al terminar')
    parser.add_argument('--temp', type=Path, default=None, help='Carpeta temporal para fotogramas sueltos')
    args = parser.parse_args(sys.argv[sys.argv.index('--')+1:])
    root = args.output.resolve()
    source = root/'Art/Blender/Characters/Mateo'
    export = root/'Game/Assets/_Project/Art/Characters/Mateo'
    source.mkdir(parents=True, exist_ok=True)
    export.mkdir(parents=True, exist_ok=True)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.context.preferences.filepaths.save_version = 0
    bpy.context.preferences.filepaths.file_preview_type = 'NONE'
    scene = bpy.context.scene
    scene.unit_settings.system = 'METRIC'
    scene.unit_settings.scale_length = 1
    scene.render.fps = FPS
    scene.frame_start, scene.frame_end = 0, 260
    materials()
    body()
    bpy.ops.object.select_all(action='DESELECT')
    for obj in PARTS: obj.select_set(True)
    bpy.context.view_layer.objects.active = PARTS[0]
    bpy.ops.object.join()
    mesh = bpy.context.object
    mesh.name = mesh.data.name = 'CH_Mateo'
    mod = mesh.modifiers.new('Triangulacion estable para Unity', 'TRIANGULATE')
    bpy.ops.object.modifier_apply(modifier=mod.name)
    mesh.data.calc_loop_triangles()
    tris = len(mesh.data.loop_triangles)
    assert tris < 12000, tris
    bm = bmesh.new(); bm.from_mesh(mesh.data)
    non_manifold = sum(not e.is_manifold for e in bm.edges); bm.free()
    rig = build_rig()
    stats = skin(mesh, rig)
    clips = animate(rig)
    coords = [v.co for v in mesh.data.vertices]
    low = Vector(tuple(min(v[i] for v in coords) for i in range(3)))
    high = Vector(tuple(max(v[i] for v in coords) for i in range(3)))
    scene.frame_set(0)
    bpy.ops.object.select_all(action='DESELECT')
    mesh.select_set(True); rig.select_set(True)
    bpy.context.view_layer.objects.active = rig
    blend, fbx = source/'CH_Mateo.blend', export/'CH_Mateo.fbx'
    bpy.ops.wm.save_as_mainfile(filepath=str(blend))
    # Mismos ejes y escala que el kit de la azotea; cada pista NLA sale como un clip con su nombre.
    bpy.ops.export_scene.fbx(filepath=str(fbx), use_selection=True, object_types={'ARMATURE','MESH'}, axis_forward='-Z', axis_up='Y', apply_unit_scale=True, apply_scale_options='FBX_SCALE_ALL', use_mesh_modifiers=True, mesh_smooth_type='FACE', path_mode='AUTO', add_leaf_bones=False, primary_bone_axis='Y', secondary_bone_axis='X', armature_nodetype='NULL', use_armature_deform_only=True, bake_anim=True, bake_anim_use_all_bones=True, bake_anim_use_nla_strips=True, bake_anim_use_all_actions=False, bake_anim_force_startend_keying=True, bake_anim_step=1, bake_anim_simplify_factor=0)
    report = {'name':'CH_Mateo', 'triangles':tris, 'non_manifold_edges':non_manifold, 'height_m':high.z-low.z, 'dimensions_m_blender_xyz':list(high-low), 'min_z':low.z, 'bones':BONE_NAMES, 'clips':clips, 'fps':FPS, 'skin':stats, 'materials':sorted(m.name for m in mesh.data.materials), 'fbx_bytes':fbx.stat().st_size, 'blend':str(blend.relative_to(root)), 'fbx':str(fbx.relative_to(root))}
    (source/'character_report.json').write_text(json.dumps(report, indent=2, ensure_ascii=False), encoding='utf-8')
    print('CHARACTER_OK', json.dumps(report, ensure_ascii=False), flush=True)
    if args.previews:
        import os
        render_previews(source, args.temp or Path(os.environ.get('TEMP','.'))/'mateo_previews')

if __name__ == '__main__': main()
