"""Kits del capítulo 1: Blender -b --factory-startup -P generate_capitulo1.py -- --output <raíz del proyecto>.

Opciones: --only SM_A SM_B (solo esos modelos), --kits Libreria Tintero (solo esos kits),
--render (miniaturas Eevee en Art/Blender/Capitulo1/previews/), --verify (reimporta cada FBX).
Reutiliza las primitivas y la consolidación de generate_azotea.py: mismo estilo, mismos ejes.
"""
import argparse, json, math, random, sys
from pathlib import Path
import bpy, bmesh
from mathutils import Vector, Matrix, Euler

sys.path.insert(0, str(Path(__file__).resolve().parent))
import generate_azotea as az
from generate_azotea import box, ellipsoid, rod, mesh_object, consolidate

# Paleta: los nueve de la biblioteca de Unity + los nuevos del capítulo 1.
az.PALETTE.update({'Cemento':'B7AEA4','LibroA':'7A3B3B','LibroB':'3E5A6E','LibroC':'C2A15A',
                   'Ceramica':'E9E2D6','Pizarra':'2F3B35','Planta':'5E7A4A','GatoNegro':'1E1C22',
                   'Corcho':'B5865A','Mimbre':'C8A46B'})
NEW_MATERIALS = ['LibroA','LibroB','LibroC','Ceramica','Pizarra','Planta','GatoNegro','Corcho','Mimbre']

T_WALL = .30   # espesor de los muros de casona (sillar)
H_WALL = 3.2   # altura libre hasta el arranque de la bóveda

# ---------------------------------------------------------------- utilidades nuevas

def custom(name, verts, faces, mat, loc=(0,0,0), rot=(0,0,0), smooth=False, face_mats=None):
    """Malla cerrada a partir de vértices; normales coherentes antes de la unión booleana."""
    obj = mesh_object(name, verts, faces, mat)
    if face_mats:
        names = [mat]
        for i, m in enumerate(face_mats):
            if m not in names:
                names.append(m); obj.data.materials.append(az.MATS[m])
            obj.data.polygons[i].material_index = names.index(m)
    bm = bmesh.new(); bm.from_mesh(obj.data)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    bm.to_mesh(obj.data); bm.free()
    for p in obj.data.polygons: p.use_smooth = smooth
    obj.location = loc
    obj.rotation_euler = rot
    return obj

def lathe(name, profile, loc=(0,0,0), mat='Ceramica', seg=20, rot=(0,0,0), seg_mats=None, smooth=False, closed=False):
    """Sólido de revolución sobre Z. profile: [(radio, z)], de abajo hacia arriba; radio 0 = polo."""
    verts, rings = [], []
    for r, z in profile:
        if r < 1e-7:
            rings.append([len(verts)]); verts.append((0, 0, z))
        else:
            ring = []
            for i in range(seg):
                a = i*math.tau/seg
                ring.append(len(verts)); verts.append((r*math.cos(a), r*math.sin(a), z))
            rings.append(ring)
    faces, fm = [], []
    pairs = list(zip(rings, rings[1:])) + ([(rings[-1], rings[0])] if closed else [])
    for j, (A, B) in enumerate(pairs):
        m = seg_mats[j] if seg_mats else mat
        if len(A) == 1 and len(B) == 1: continue
        if len(A) == 1: new = [(A[0], B[(i+1)%seg], B[i]) for i in range(seg)]
        elif len(B) == 1: new = [(A[i], A[(i+1)%seg], B[0]) for i in range(seg)]
        else: new = [(A[i], A[(i+1)%seg], B[(i+1)%seg], B[i]) for i in range(seg)]
        faces += new; fm += [m]*len(new)
    if closed: return custom(name, verts, faces, mat, loc, rot, smooth, fm if seg_mats else None)
    if len(rings[0]) > 1: faces.append(tuple(reversed(rings[0]))); fm.append(seg_mats[0] if seg_mats else mat)
    if len(rings[-1]) > 1: faces.append(tuple(rings[-1])); fm.append(seg_mats[-1] if seg_mats else mat)
    return custom(name, verts, faces, mat, loc, rot, smooth, fm if seg_mats else None)

def prism(name, poly, a0, a1, mat, plane='XZ', loc=(0,0,0), rot=(0,0,0)):
    """Polígono 2D extruido. XZ: (x,z) a lo largo de Y; YZ: (y,z) a lo largo de X; XY: (x,y) a lo largo de Z."""
    def P(u, v, w):
        return {'XZ':(u, w, v), 'YZ':(w, u, v), 'XY':(u, v, w)}[plane]
    n = len(poly)
    verts = [P(u, v, a0) for u, v in poly] + [P(u, v, a1) for u, v in poly]
    faces = [tuple(range(n)), tuple(range(2*n-1, n-1, -1))]
    faces += [(i, (i+1)%n, (i+1)%n+n, i+n) for i in range(n)]
    return custom(name, verts, faces, mat, loc, rot)

def tube(name, pts, radius, mat='Metal', sides=8, closed=False, smooth=False, radii=None):
    """Barrido circular a lo largo de una polilínea (marcos por transporte paralelo), con tapas."""
    P = []
    for p in pts:
        if not P or (Vector(p) - P[-1]).length > 1e-6: P.append(Vector(p))
    n = len(P)
    T = []
    for i in range(n):
        a, b = (P[(i-1)%n], P[(i+1)%n]) if closed else (P[max(i-1,0)], P[min(i+1,n-1)])
        T.append((b-a).normalized())
    ref = Vector((0,0,1)) if abs(T[0].z) < .9 else Vector((1,0,0))
    N = T[0].cross(ref).normalized()
    verts = []
    for i in range(n):
        N = (N - T[i]*N.dot(T[i])).normalized()
        B = T[i].cross(N)
        r = radii[i] if radii else radius
        for k in range(sides):
            ang = k*math.tau/sides + math.pi/sides
            verts.append(tuple(P[i] + (N*math.cos(ang) + B*math.sin(ang))*r))
    faces = []
    for i in range(n if closed else n-1):
        j = (i+1) % n
        for k in range(sides):
            k2 = (k+1) % sides
            faces.append((i*sides+k, i*sides+k2, j*sides+k2, j*sides+k))
    if not closed:
        faces.append(tuple(reversed(range(sides))))
        faces.append(tuple(range((n-1)*sides, n*sides)))
    return custom(name, verts, faces, mat, smooth=smooth)

def ring_pts(center, radius, n=20, plane='XY', phase=0.):
    cx, cy, cz = center
    out = []
    for i in range(n):
        a = (i + phase)*math.tau/n
        c, s = radius*math.cos(a), radius*math.sin(a)
        out.append((cx+c, cy+s, cz) if plane == 'XY' else (cx+c, cy, cz+s))
    return out

def beam(name, a, b, w, h, mat='Madera', bevel=.006):
    """Listón de sección w × h orientado de a hacia b."""
    a, b = Vector(a), Vector(b); v = b - a
    obj = box(name, (a+b)/2, (w, v.length, h), mat, bevel)
    obj.rotation_euler = v.to_track_quat('Y', 'Z').to_euler()
    return obj

def placed_box(name, origin, rotation, local, size, mat, bevel=.004, extra_rot=(0,0,0)):
    """Caja en coordenadas locales de un plano girado (tablero inclinado, pila de libros...)."""
    R = rotation.to_matrix()
    obj = box(name, tuple(Vector(origin) + R @ Vector(local)), size, mat, bevel)
    obj.rotation_euler = (rotation.to_matrix() @ Euler(extra_rot).to_matrix()).to_euler()
    return obj

# ---------------------------------------------------------------- A) Librería «La Pluma Quieta»

BOOK_MATS = ('LibroA','LibroB','LibroC','LibroA','LibroB','LibroC','LibroA','LibroB','Tela','Lana','Papel','Pizarra')

def book_row(rng, x0, x1, z, y_front, depth, hmax):
    """Una balda de libros de pie: alturas, grosores y fondos distintos; remata con libro inclinado o pila."""
    tail = rng.choice(('lean', 'stack', 'lean', 'full'))
    reserve = {'lean': rng.uniform(.08, .15), 'stack': rng.uniform(.21, .25), 'full': 0}[tail]
    x, stop = x0 + .004, x1 - .004
    limit = stop - reserve
    last = None
    while True:
        w = rng.uniform(.024, .056)
        if x + w > limit: break
        h = rng.uniform(.6, 1.0)*hmax
        d = rng.uniform(.78, 1.0)*depth
        if last and rng.random() < .5: mat = last  # tomos de una misma colección
        else: mat = rng.choice(BOOK_MATS)
        last = mat if rng.random() < .4 else None
        box('Libro', (x + w/2, y_front + d/2, z + h/2 - .002), (w, d, h), mat, 0)
        x += w + .003
    room = stop - x
    if tail == 'lean' and room > .06:
        w, h = .032, rng.uniform(.8, .95)*hmax
        s = max(.08, min(.55, (room - w - .006)/h))
        th = math.asin(s)
        if w*math.cos(th) + h*s < room:
            cx = x + w/2*math.cos(th) + h/2*s - .002
            cz = z + w/2*s + h/2*math.cos(th) - .002
            obj = box('Libro inclinado', (cx, y_front + depth*.45, cz), (w, depth*.9, h), rng.choice(BOOK_MATS), 0)
            obj.rotation_euler.y = -th
    elif tail == 'stack' and room > .2:
        zb = z - .002
        for i in range(rng.randint(3, 5)):
            t, L = rng.uniform(.028, .045), rng.uniform(.165, min(.205, room - .02))
            obj = box('Libro tumbado', (x + room/2 + rng.uniform(-.006, .006), y_front + depth*.45, zb + t/2), (L, depth*rng.uniform(.75, .9), t), rng.choice(BOOK_MATS), 0)
            obj.rotation_euler.z = rng.uniform(-.05, .05)
            zb += t - .002

def bookcase(width, height, depth, rows, seed, top_items=False):
    rng = random.Random(seed)
    t, crown, plinth, board = .045, .06, .10, .03
    body = height - crown
    inner = width - 2*t + .01
    for s in (-1, 1): box('Costado', (s*(width/2 - t/2), 0, body/2), (t, depth, body), bevel=.01)
    box('Fondo', (0, depth/2 - .015, body/2), (inner, .02, body - .02), bevel=0)
    box('Zocalo', (0, -depth/2 + .04, plinth/2), (inner, .03, plinth), bevel=.004)
    box('Cornisa', (0, -.016, body + crown/2), (width + .07, depth + .045, crown), bevel=.014)
    box('Moldura', (0, -.008, body - .014), (width + .026, depth + .018, .03), bevel=.008)
    levels = [plinth + k*(body - board - plinth)/rows for k in range(rows + 1)]
    for zb in levels:
        box('Balda', (0, -.008, zb + board/2), (inner, depth - .02, board), bevel=.006)
    box('Division', (0, -.01, (plinth + body)/2), (.03, depth - .03, body - plinth), bevel=.006)
    y_front, bdepth = -depth/2 + .03, depth - .07
    for k in range(rows):
        z, clear = levels[k] + board, levels[k+1] - levels[k] - board
        for x0, x1 in ((-width/2 + t, -.015), (.015, width/2 - t)):
            book_row(rng, x0, x1, z, y_front, bdepth, clear*.88)
    if top_items:
        top = height
        zb = top - .002
        for i, (L, d, tk, mat, rz) in enumerate(((.24, .17, .04, 'LibroB', .05), (.22, .16, .035, 'LibroC', -.08), (.2, .15, .03, 'LibroA', .12))):
            obj = box('Libro sobre el mueble', (-width/2 + .26, -.01, zb + tk/2), (L, d, tk), mat, .004)
            obj.rotation_euler.z = rz
            zb += tk - .002
        lathe('Maceta', [(0, 0), (.055, 0), (.075, .12), (.08, .12), (.08, .135), (0, .135)], (width/2 - .2, 0, top - .002), 'Ceramica', 16)
        for (dx, dy, dz, s) in ((0, 0, .19, .13), (.045, -.02, .16, .09), (-.045, .02, .165, .1), (0, .03, .23, .08)):
            ellipsoid('Planta', (width/2 - .2 + dx, dy, top + dz), (s, s, s*.9), 'Planta', 10, 6)

def estante_libros(): bookcase(2.0, 2.4, .38, 5, 7)
def estante_libros_bajo(): bookcase(1.4, 1.2, .34, 2, 21, top_items=True)

def cash_register(o):
    ox, oy, oz = o
    box('Base caja', (ox, oy, oz + .05), (.42, .38, .1), 'Madera', .01)
    box('Frente cajon', (ox, oy - .191, oz + .05), (.36, .012, .064), 'Madera', .005)
    rod('Tirador cajon', (ox, oy - .195, oz + .05), (ox, oy - .215, oz + .05), .012, 'Dorado', 10)
    prism('Cuerpo registradora', [(-.17, .098), (.17, .098), (.17, .36), (.02, .36), (-.17, .17)], ox - .19, ox + .19, 'Dorado', 'YZ', (0, oy, oz))
    for s in (.2, .5, .8):
        y, zs = oy - .17 + .19*s, oz + .17 + .19*s
        for x in (-.13, -.065, 0, .065, .13):
            rod('Vastago tecla', (ox + x, y, zs - .02), (ox + x, y, zs + .03), .006, 'Metal', 6)
            rod('Tecla', (ox + x, y, zs + .03), (ox + x, y, zs + .042), .015, 'Papel', 8)
    box('Marquesina', (ox, oy + .1, oz + .41), (.26, .075, .11), 'Dorado', .012)
    box('Visor', (ox, oy + .061, oz + .415), (.17, .006, .05), 'Papel', .002)
    ellipsoid('Cresta', (ox, oy + .1, oz + .47), (.2, .05, .07), 'Dorado', 12, 6)
    rod('Eje manivela', (ox + .18, oy + .02, oz + .22), (ox + .25, oy + .02, oz + .22), .014, 'Metal', 8)
    rod('Brazo manivela', (ox + .245, oy + .02, oz + .22), (ox + .245, oy - .08, oz + .15), .01, 'Metal', 8)
    rod('Pomo manivela', (ox + .245, oy - .08, oz + .15), (ox + .29, oy - .08, oz + .15), .015, 'Madera', 10)

def mostrador():
    W, D = 1.8, .6
    box('Zocalo', (0, .02, .05), (W - .1, D - .12, .1), bevel=.006)
    box('Cuerpo', (0, .015, .52), (W - .06, D - .08, .86), bevel=.012)   # frente en y = -0.255
    box('Tablero', (0, 0, .975), (W, D, .05), bevel=.018)
    box('Faja alta', (0, -.262, .89), (W - .04, .03, .05), bevel=.008)
    box('Faja baja', (0, -.262, .15), (W - .04, .03, .05), bevel=.008)
    pw = (W - .06 - 4*.07)/3
    for i in range(3):
        x = -W/2 + .03 + .07 + pw/2 + i*(pw + .07)
        box('Panel', (x, -.262, .52), (pw, .03, .6), bevel=.016)
    cash_register((.42, .02, 1.0))
    lathe('Timbre', [(0, 0), (.05, 0), (.05, .008), (.045, .012), (.044, .03), (.032, .055), (.012, .066), (.008, .076), (.013, .086), (0, .09)], (-.55, -.08, .998), 'Dorado', 16)
    for i, (mat, tk) in enumerate((('LibroB', .035), ('LibroA', .03))):
        obj = box('Libro en mostrador', (-.2, -.02, 1.0 + i*.033 + tk/2 - .002), (.22, .16, tk), mat, .004)
        obj.rotation_euler.z = .1 - i*.18

def canasta_separadores():
    lathe('Canasta', [(0, 0), (.135, 0), (.17, .22), (.17, .226), (.157, .226), (.157, .22), (.123, .012), (0, .012)], mat='Mimbre', seg=20)
    for z in (.05, .095, .14, .185):
        r = .135 + (.17 - .135)*z/.22 + .002
        tube('Trenzado', ring_pts((0, 0, z), r - .001, 18, phase=.37), .0045, 'Mimbre', 5, closed=True)
    tube('Borde trenzado', ring_pts((0, 0, .224), .165, 18, phase=.21), .01, 'Mimbre', 6, closed=True)
    rng = random.Random(5)
    mats = ('Papel', 'LibroC', 'Papel', 'LibroA', 'Ceramica', 'LibroB', 'Papel', 'Lana', 'LibroC', 'Papel', 'Tela', 'Papel')
    for i, mat in enumerate(mats):
        phi = i*math.tau/len(mats) + rng.uniform(-.2, .2)
        r = .035 + rng.uniform(0, .03) if i % 2 else rng.uniform(0, .025)
        h, tilt = rng.uniform(.25, .31), rng.uniform(.1, .28)
        rotation = Euler((0, tilt, phi), 'XYZ')
        base = Vector((r*math.cos(phi), r*math.sin(phi), .008))
        top = base + rotation.to_matrix() @ Vector((0, 0, h))
        obj = box('Separador', tuple(base + rotation.to_matrix() @ Vector((0, 0, h/2))), (.004, .042, h), mat, .0008)
        obj.rotation_euler = rotation
        if i % 4 == 1:
            rod('Borla', tuple(top - Vector((0, 0, .004))), tuple(top + Vector((0, 0, -.06)) + rotation.to_matrix() @ Vector((.03, 0, 0))), .005, 'Lana', 6)

def sillon():
    for x in (-.33, .33):
        for y in (-.3, .3):
            lathe('Pata torneada', [(0, 0), (.02, 0), (.026, .03), (.018, .07), (.028, .1), (.03, .14), (0, .14)], (x, y, 0), 'Madera', 10)
    box('Faldon', (0, 0, .15), (.82, .78, .05), 'Madera', .015)
    box('Base tapizada', (0, 0, .26), (.8, .76, .24), 'Lana', .05)
    box('Cojin', (0, -.06, .44), (.55, .58, .12), 'Lana', .05)
    back = box('Respaldo', (0, .29, .66), (.64, .17, .6), 'Lana', .06)
    back.rotation_euler.x = -.1
    ellipsoid('Remate respaldo', (0, .33, .95), (.72, .22, .2), 'Lana', 16, 8)
    for s in (-1, 1):
        box('Brazo', (s*.345, -.02, .48), (.13, .7, .2), 'Lana', .04)
        ellipsoid('Rollo brazo', (s*.35, -.03, .6), (.15, .74, .15), 'Lana', 14, 8)
        ellipsoid('Oreja', (s*.33, .2, .78), (.11, .34, .44), 'Lana', 12, 8)
        for z in (.41, .46, .51, .56):
            ellipsoid('Tachuela', (s*.35, -.374, z), (.018, .012, .018), 'Dorado', 8, 4)

def lying_book(rng, center, w, d, t, rot, mat, side):
    R = Euler((0, 0, rot))
    placed_box('Hojas', center, R, (-side*.004, 0, t/2), (w - .012, d - .012, t - .008), 'Papel', .002)
    placed_box('Tapa', center, R, (0, 0, .0025), (w, d, .005), mat, .0015)
    placed_box('Tapa', center, R, (0, 0, t - .0025), (w, d, .005), mat, .0015)
    placed_box('Lomo', center, R, (side*(w/2 - .002), 0, t/2), (.007, d + .003, t + .002), mat, .002)

def pila_libros():
    rng = random.Random(11)
    z = 0
    sizes = [(.24, .31, .048), (.22, .29, .04), (.23, .3, .03), (.2, .27, .045), (.19, .26, .028), (.17, .24, .036), (.15, .21, .03)]
    for i, (w, d, t) in enumerate(sizes):
        lying_book(rng, (rng.uniform(-.015, .015), rng.uniform(-.015, .015), z), w, d, t, rng.uniform(-.22, .22), BOOK_MATS[(i*5) % len(BOOK_MATS)], 1 if i % 2 else -1)
        z += t - .001

def escalera_madera():
    n, rise, run, half = 13, .2, .26, .45
    L = n*run; y0 = -L/2; k = rise/run
    for i in range(1, n + 1):
        yf = y0 + (i-1)*run
        box('Huella', (0, yf + .115, i*rise - .02), (2*half, .29, .04), bevel=.008)
        zb = (i-1)*rise - (.004 if i > 1 else 0)
        zt = i*rise - .036
        box('Contrahuella', (0, yf + .012, (zb + zt)/2), (2*half - .04, .02, zt - zb), bevel=.003)
    ytop = y0 + (2.64 - .26)/k
    poly = [(y0 - .03, 0), (y0 + .1/k, 0), (L/2, .2 + L*k - .3), (L/2, 2.64), (ytop, 2.64), (y0 - .03, .26 - .03*k)]
    for s in (-1, 1): prism('Zanca', poly, s*(half - .01), s*(half + .045), 'Madera', 'YZ')
    xr = half - .04
    box('Pilar arranque', (xr, y0 + .1, .53), (.09, .09, 1.06), bevel=.012)
    ellipsoid('Remate pilar', (xr, y0 + .1, 1.09), (.075, .075, .075), 'Madera', 12, 6)
    box('Pilar llegada', (xr, L/2 - .1, 2.95), (.09, .09, 1.06), bevel=.012)
    ellipsoid('Remate pilar', (xr, L/2 - .1, 3.51), (.075, .075, .075), 'Madera', 12, 6)
    a, b = Vector((xr, y0 + .1, .98)), Vector((xr, L/2 - .1, 3.42))
    beam('Pasamanos', a, b, .065, .055, bevel=.012)
    for i in range(2, n):
        y = y0 + (i-1)*run + .13
        zr = a.z + (y - a.y)*(b.z - a.z)/(b.y - a.y)
        rod('Balaustre', (xr, y, i*rise - .01), (xr, y, zr), .015, 'Madera', 8)

def calendario_pared():
    W, H = .32, .46
    box('Hoja', (0, 0, H/2), (W, .006, H), 'Papel', .0015)
    box('Hojas pasadas', (0, .004, H/2 - .004), (W - .004, .004, H - .006), 'Ceramica', .001)
    box('Encuadernacion', (0, -.002, H - .006), (W - .01, .016, .02), 'Metal', .004)
    for x in (-.11, .11): rod('Cordel', (x*.6, -.001, H - .01), (0, -.001, H + .065), .0018, 'Lana', 6)
    rod('Clavo', (0, .0, H + .065), (0, -.02, H + .065), .006, 'Metal', 8)
    box('Ilustracion', (0, -.0035, .355), (.28, .002, .17), 'Tela', .0006)
    prism('Misti', [(-.11, .275), (.07, .275), (.0, .37), (-.04, .37)], -.0050, -.0042, 'Cemento')
    prism('Nieve', [(-.04, .37), (.0, .37), (.017, .345), (-.002, .352), (-.02, .342), (-.038, .352), (-.057, .345)], -.0058, -.0046, 'Papel')
    prism('Chachani', [(.02, .275), (.135, .275), (.12, .31), (.08, .325), (.05, .315)], -.0047, -.0043, 'Pizarra')
    ellipsoid('Sol', (.09, -.0047, .4), (.03, .002, .03), 'Dorado', 12, 4)
    box('Mes', (0, -.0035, .253), (.28, .002, .026), 'LibroA', .0006)
    x0, x1, z0, z1 = -.14, .14, .02, .232
    cw, ch = (x1 - x0)/7, (z1 - z0)/5
    for i in range(8): box('Linea', (x0 + i*cw, -.0034, (z0 + z1)/2), (.0016, .0016, z1 - z0), 'Cemento', 0)
    for j in range(6): box('Linea', (0, -.0036, z0 + j*ch), (x1 - x0, .002, .0016), 'Cemento', 0)
    rng = random.Random(3)
    inks = ('LibroA', 'LibroB', 'Metal', 'LibroB', 'Metal')
    for row in range(5):
        for col in range(7):
            if rng.random() < .3: continue
            cx, cz = x0 + (col + .5)*cw, z0 + (4 - row + .5)*ch
            ink = rng.choice(inks)
            for k in range(rng.randint(1, 3)):
                L = rng.uniform(.012, .03)
                obj = box('Anotacion', (cx + rng.uniform(-.005, .005), -.0047, cz + .009 - k*.008), (L, .0016, .0025), ink, 0)
                obj.rotation_euler.y = rng.uniform(-.25, .25)
            if rng.random() < .12:
                tube('Circulo', ring_pts((cx, -.0048, cz), .017, 10, 'XZ'), .0013, 'LibroA', 4, closed=True)
            elif rng.random() < .06:
                for sgn in (-1, 1):
                    obj = box('Tachadura', (cx, -.0049, cz), (.03, .0016, .0024), 'Metal', 0)
                    obj.rotation_euler.y = sgn*.6

def gato_tinta():
    """Gato negro dormido, enroscado: cuerpo redondo, cabeza apoyada al frente y cola rodeándolo."""
    M = 'GatoNegro'
    ellipsoid('Cuerpo', (0, .02, .072), (.36, .3, .145), M, 16, 10)
    ellipsoid('Cadera', (-.08, .06, .085), (.2, .22, .15), M, 12, 8)
    ellipsoid('Lomo', (.06, .08, .09), (.2, .16, .12), M, 12, 8)
    hx, hy, hz = .07, -.1, .1
    ellipsoid('Cabeza', (hx, hy, hz), (.15, .13, .12), M, 14, 8)
    ellipsoid('Mejillas', (hx, hy - .03, hz - .022), (.13, .08, .07), M, 12, 6)
    for s in (-1, 1):
        lathe('Oreja', [(0, 0), (.028, 0), (0, .058)], (hx + s*.042, hy + .01, hz + .04), M, 6, rot=(-.15, s*.42, 0))
        ellipsoid('Pata', (hx + s*.04 - .01, hy - .07, .022), (.05, .08, .04), M, 10, 6)
        ex = hx + s*.032
        tube('Ojo cerrado', [(ex - .016, hy - .061, hz + .012), (ex - .006, hy - .066, hz + .004), (ex + .006, hy - .066, hz + .004), (ex + .016, hy - .061, hz + .012)], .003, 'Metal', 5)
    ellipsoid('Nariz', (hx, hy - .072, hz - .018), (.016, .01, .011), 'Lana', 8, 4)
    pts, radii = [], []
    for i in range(14):
        t = math.radians(120 + i*(175/13))
        pts.append((.2*math.cos(t), .02 + .175*math.sin(t), .028 + .012*(i/13)))
        radii.append(.03 - .01*i/13)
    tube('Cola', pts, .03, M, 8, smooth=True, radii=radii)
    ellipsoid('Punta cola', pts[-1], (.04, .04, .04), M, 8, 6)

# ---------------------------------------------------------------- B) Cafetería «El Tintero»

def espresso(o):
    ox, oy, oz = o
    box('Cuerpo maquina', (ox, oy, oz + .2), (.6, .42, .38), 'Ceramica', .045)
    box('Bandeja tazas', (ox, oy + .02, oz + .395), (.56, .36, .02), 'Metal', .006)
    rail = [(ox - .27, oy - .15, oz + .43), (ox - .27, oy + .19, oz + .43), (ox + .27, oy + .19, oz + .43), (ox + .27, oy - .15, oz + .43)]
    tube('Baranda', rail, .006, 'Metal', 6)
    for x, y, z in rail: rod('Poste', (x, y, oz + .4), (x, y, oz + .431), .005, 'Metal', 6)
    box('Franja', (ox, oy - .212, oz + .3), (.5, .01, .04), 'Dorado', .004)
    for x in (-.14, .14):
        lathe('Grupo', [(0, 0), (.04, 0), (.045, .06), (.03, .08), (0, .08)], (ox + x, oy - .25, oz + .155), 'Metal', 12)
        lathe('Portafiltro', [(0, 0), (.038, 0), (.04, .03), (0, .03)], (ox + x, oy - .25, oz + .125), 'Metal', 12)
        rod('Mango', (ox + x, oy - .27, oz + .14), (ox + x, oy - .4, oz + .12), .013, 'Madera', 8)
    box('Bandeja goteo', (ox, oy - .2, oz + .02), (.5, .14, .04), 'Metal', .008)
    lathe('Manometro', [(0, 0), (.045, 0), (.045, .02), (.036, .024), (0, .024)], (ox, oy - .2, oz + .27), 'Dorado', 16, rot=(math.pi/2, 0, 0), seg_mats=['Dorado', 'Dorado', 'Papel', 'Papel'])
    rod('Vaporizador', (ox + .26, oy - .18, oz + .3), (ox + .3, oy - .26, oz + .12), .008, 'Metal', 8)
    for i, x in enumerate((-.2, 0, .2)):
        lathe('Taza', [(0, 0), (.025, 0), (.035, .05), (.03, .05), (.02, .012), (0, .012)], (ox + x, oy + .04 + (i % 2)*.05, oz + .404), 'Ceramica', 12)

def barra_cafe():
    W, D = 2.5, .65
    box('Zocalo', (0, .04, .05), (W - .1, D - .14, .1), 'Cemento', .006)
    box('Cuerpo', (0, .025, .53), (W - .06, D - .1, .88), bevel=.012)   # frente en y = -0.275
    box('Tablero', (0, 0, 1.0), (W, D, .06), bevel=.02)
    n = 12
    sw = (W - .1)/n
    for i in range(n):
        box('Duela', (-W/2 + .05 + (i + .5)*sw, -.258, .52), (sw - .012, .02, .74), bevel=.006)
    box('Faja', (0, -.262, .92), (W - .04, .03, .05), bevel=.008)
    rod('Reposapies', (-W/2 + .1, -.4, .2), (W/2 - .1, -.4, .2), .02, 'Dorado', 10)
    for x in (-1.0, 0, 1.0): rod('Soporte', (x, -.4, .2), (x, -.255, .25), .012, 'Dorado', 8)
    espresso((-.6, .05, 1.03))
    box('Molinillo base', (.1, .1, 1.07), (.16, .2, .08), 'Metal', .02)
    lathe('Molinillo', [(0, 0), (.06, 0), (.055, .2), (0, .2)], (.1, .12, 1.1), 'Metal', 12)
    lathe('Tolva', [(0, 0), (.04, 0), (.075, .14), (.07, .15), (0, .15)], (.1, .12, 1.299), 'Vidrio', 16)
    lathe('Plato', [(0, 0), (.12, 0), (.15, .015), (.14, .02), (0, .012)], (.8, .05, 1.029), 'Ceramica', 16)
    lathe('Torta', [(0, 0), (.1, 0), (.1, .07), (.095, .075), (0, .075)], (.8, .05, 1.04), 'LibroC', 16, seg_mats=['LibroC', 'LibroC', 'Papel', 'Papel'])
    lathe('Campana', [(0, .28), (.035, .272), (.035, .252), (.103, .222), (.143, .14), (.143, 0), (.137, 0), (.137, .138), (.098, .216), (0, .244)], (.8, .05, 1.045), 'Vidrio', 16)

def mesa_cafe():
    lathe('Cubierta', [(0, 0), (.29, 0), (.3, .015), (.29, .03), (0, .03)], (0, 0, .71), 'Ceramica', 24)
    tube('Canto', ring_pts((0, 0, .725), .298, 24), .009, 'Metal', 6, closed=True)
    lathe('Pedestal', [(0, 0), (.22, 0), (.22, .015), (.12, .035), (.04, .06), (.025, .12), (.025, .62), (.045, .66), (.1, .7), (.1, .712), (0, .712)], mat='Metal', seg=20)

def silla_cafe():
    lathe('Asiento', [(0, 0), (.2, 0), (.21, .015), (.2, .035), (0, .035)], (0, 0, .43), 'Madera', 20)
    tube('Aro asiento', ring_pts((0, 0, .415), .175, 20), .014, 'Madera', 6, closed=True)
    tube('Aro patas', ring_pts((0, 0, .2), .19, 20), .011, 'Madera', 6, closed=True)
    for i in range(4):
        a = math.pi/4 + i*math.pi/2
        c, s = math.cos(a), math.sin(a)
        top = (.12*c/abs(c)*.96, .11, .44) if s > 0 else (.15*c, .15*s, .44)
        rod('Pata', top, (.2*c, .2*s, 0), .016, 'Madera', 8)
    pts = []
    for i in range(15):
        t = i/14
        a = math.pi*t
        pts.append((-.12*math.cos(a)*(1 + .3*math.sin(a)), .11 + .06*math.sin(a), .44 + .42*math.sin(a)**.6))
    tube('Respaldo curvado', pts, .016, 'Madera', 8)
    inner = []
    for i in range(11):
        a = math.pi*i/10
        inner.append((-.07*math.cos(a), .11 + .06*math.sin(a), .46 + .25*math.sin(a)**.7))
    tube('Arco interior', inner, .011, 'Madera', 6)

def taza_cafe():
    lathe('Plato', [(0, 0), (.045, 0), (.075, .013), (.074, .018), (.04, .009), (0, .009)], mat='Ceramica', seg=24)
    lathe('Taza', [(0, .006), (.028, .006), (.031, .012), (.044, .04), (.047, .075), (.043, .075), (.04, .04), (.026, .017), (0, .017)], mat='Ceramica', seg=24)
    lathe('Cafe', [(0, .055), (.0425, .055), (.0425, .062), (0, .062)], mat='Madera', seg=24)
    pts = [(.040 + .0*0, 0, .064)]
    for i in range(9):
        a = math.pi/2 - i*math.pi/8
        pts.append((.052 + .016*math.cos(a), 0, .045 + .017*math.sin(a)))
    pts.append((.040, 0, .028))
    tube('Asa', pts, .0055, 'Ceramica', 8)

def tablon_corcho():
    W, H = 1.2, .8
    box('Corcho', (0, 0, H/2), (W - .06, .016, H - .06), 'Corcho', 0)
    for s in (-1, 1):
        box('Marco', (s*(W/2 - .025), -.004, H/2), (.05, .032, H), bevel=.008)
        box('Marco', (0, -.005, H/2 + s*(H/2 - .025)), (W - .07, .028, .046), bevel=.008)
    rng = random.Random(9)
    mats = ('Papel', 'LibroC', 'Ceramica', 'Papel', 'Lana', 'Papel', 'LibroB', 'Papel', 'LibroC', 'Ceramica', 'Papel', 'LibroA', 'Papel', 'Planta')
    pins = ('LibroA', 'LibroB', 'Dorado')
    i = 0
    for row in range(3):
        for col in range(5):
            if (row, col) == (0, 2): continue
            cx = -.42 + col*.21 + rng.uniform(-.03, .03)
            cz = .54 - row*.19 + rng.uniform(-.015, .015)
            w, h = rng.uniform(.09, .14), rng.uniform(.065, .1)
            y = -.0085 - .0012 - (i % 4)*.0011
            ang = rng.uniform(-.16, .16)
            card = box('Tarjeta', (cx, y, cz), (w, .0016, h), mats[i % len(mats)], .0005)
            card.rotation_euler.y = ang
            ink = 'Metal' if mats[i % len(mats)] != 'LibroB' else 'Papel'
            for k in range(rng.randint(1, 3)):
                stroke = box('Nombre', (cx - w*.12 + rng.uniform(-.01, .01), y - .0014, cz + h*.15 - k*.018), (w*rng.uniform(.4, .65), .0014, .0035), 'LibroA' if k == 0 else ink, 0)
                stroke.rotation_euler.y = ang + rng.uniform(-.06, .06)
            ellipsoid('Chinche', (cx + math.sin(-ang)*h*.38, y - .006, cz + h*.38), (.016, .012, .016), pins[i % 3], 8, 4)
            i += 1
    box('Tarjeta grande', (0, -.0105, .54), (.17, .0016, .12), 'Papel', .0005)
    box('Corazon', (0, -.0118, .55), (.035, .0016, .035), 'Lana', 0).rotation_euler.y = math.pi/4
    ellipsoid('Chinche', (0, -.016, .595), (.018, .012, .018), 'Dorado', 8, 4)
    pts = [(-W/2 + .06, -.02, H - .07)]
    for j in range(1, 12):
        t = j/12
        pts.append((-W/2 + .06 + t*(W - .12), -.02, H - .07 - .06*math.sin(math.pi*t)))
    pts.append((W/2 - .06, -.02, H - .07))
    tube('Guirnalda', pts, .0035, 'Lana', 4)
    for j in range(1, 12):
        x, _, z = pts[j]
        prism('Banderin', [(x - .03, z + .004), (x + .03, z + .004), (x, z - .055)], -.0215, -.0185, ('LibroA', 'LibroC', 'LibroB')[j % 3])
    for s in (-1, 1): ellipsoid('Clavo guirnalda', (s*(W/2 - .06), -.022, H - .07), (.016, .012, .016), 'Dorado', 8, 4)

def lampara_colgante():
    lathe('Pantalla', [(.185, 0), (.19, .006), (.12, .1), (.06, .16), (.03, .2), (.024, .21), (.018, .2), (.054, .155), (.112, .098), (.178, .006)], mat='Pizarra', seg=24,
          seg_mats=['Pizarra', 'Pizarra', 'Pizarra', 'Pizarra', 'Pizarra', 'Papel', 'Papel', 'Papel', 'Papel', 'Pizarra'], closed=True)
    tube('Aro', ring_pts((0, 0, .004), .187, 24), .006, 'Dorado', 6, closed=True)
    lathe('Portalampara', [(0, .15), (.026, .15), (.026, .26), (.012, .27), (0, .27)], mat='Metal', seg=12)
    ellipsoid('Bombilla', (0, 0, .11), (.07, .07, .09), 'Papel', 12, 8)
    rod('Cable', (0, 0, .265), (0, 0, .97), .005, 'Metal', 6)
    lathe('Floron', [(0, .96), (.02, .96), (.07, .985), (.07, 1.0), (0, 1.0)], mat='Metal', seg=16)

# ---------------------------------------------------------------- C) Aula 3B

def mesa_dibujo():
    a = math.radians(15)
    for s in (-1, 1):
        x = s*.44
        box('Pata delantera', (x, -.27, .395), (.05, .05, .79), bevel=.008)
        box('Pata trasera', (x, .27, .465), (.05, .05, .93), bevel=.008)
        box('Patin', (x, 0, .03), (.07, .68, .06), bevel=.012)
        beam('Larguero inclinado', (x, -.3, .778), (x, .3, .941), .045, .05)
        rod('Pomo inclinacion', (x + s*.025, .27, .8), (x + s*.06, .27, .8), .02, 'Metal', 10)
    box('Travesano bajo', (0, 0, .16), (.86, .045, .06), bevel=.008)
    box('Reposapies', (0, -.27, .25), (.86, .04, .04), bevel=.008)
    R = Euler((a, 0, 0))
    c = (0, -.31 + .35*math.cos(a), .80 + .35*math.sin(a) + .016)
    placed_box('Tablero', c, R, (0, 0, 0), (1.02, .7, .032), 'Madera', .008)
    placed_box('Regla paralela', c, R, (0, -.07, .022), (.98, .05, .01), 'LibroC', .003)
    for s in (-1, 1): placed_box('Polea', c, R, (s*.5, -.07, .022), (.03, .06, .02), 'Dorado', .004)
    placed_box('Lamina A2', c, R, (.02, .08, .017), (.594, .42, .0024), 'Papel', .0005, (0, 0, .025))
    for sx, sy in ((-1, -1), (1, -1), (-1, 1), (1, 1)):
        placed_box('Cinta', c, R, (.02 + sx*.29, .08 + sy*.2, .0185), (.045, .022, .0016), 'LibroC', 0, (0, 0, .785*sx*sy))
    for k in range(4):
        placed_box('Trazo', c, R, (-.05 + k*.04, .1, .0185), (.003, .26 - k*.03, .0012), 'Metal', 0)
    placed_box('Trazo', c, R, (.04, .21, .0185), (.32, .003, .0012), 'Metal', 0)
    placed_box('Repisa lapices', c, R, (0, -.36, .02), (1.0, .035, .055), 'Madera', .006)
    pr = R.to_matrix()
    p = Vector(c) + pr @ Vector((.25, -.37, .052))
    rod('Lapiz', tuple(p), tuple(p + Vector((.17, 0, 0))), .0045, 'LibroC', 6)
    rod('Punta', tuple(p + Vector((.17, 0, 0))), tuple(p + Vector((.185, 0, 0))), .002, 'Madera', 6)
    tri = [(.12, .02), (.32, .02), (.12, .2)]
    q = Vector(c) + pr @ Vector((0, 0, .0185))
    prism('Escuadra', tri, 0, .003, 'Vidrio', 'XY', loc=tuple(q), rot=(a, 0, 0))

def taburete():
    lathe('Asiento', [(0, 0), (.165, 0), (.175, .018), (.165, .04), (0, .04)], (0, 0, .64), 'Madera', 20)
    for i in range(4):
        ang = math.pi/4 + i*math.pi/2
        c, s = math.cos(ang), math.sin(ang)
        rod('Pata', (.1*c, .1*s, .65), (.2*c, .2*s, .012), .018, 'Madera', 8)
    tube('Aro reposapies', ring_pts((0, 0, .26), .162, 20), .011, 'Metal', 6, closed=True)

def chalk(pts, r=.0045, sides=3):
    return tube('Tiza', pts, r, 'Papel', sides)

def pizarra():
    W, H = 3.0, 1.2
    # Ojo: la pieza no puede llamarse igual que el objeto final («Pizarra»), o la unión la pierde.
    box('Tablero', (0, 0, H/2), (W - .08, .02, H - .08), 'Pizarra', 0)
    for s in (-1, 1):
        box('Marco', (s*(W/2 - .03), -.003, H/2), (.06, .045, H), bevel=.008)
        box('Marco', (0, -.004, H/2 + s*(H/2 - .03)), (W - .08, .041, .056), bevel=.008)
    box('Repisa', (0, -.055, .07), (W - .04, .08, .02), bevel=.005)
    box('Labio repisa', (0, -.092, .082), (W - .04, .012, .03), bevel=.004)
    for x, mat in ((.55, 'Papel'), (.68, 'LibroC'), (.74, 'Papel')):
        rod('Tiza suelta', (x, -.05, .085), (x + .075, -.045, .085), .0055, mat, 8)
    box('Borrador', (1.0, -.052, .094), (.15, .055, .028), 'Madera', .006)
    box('Fieltro', (1.0, -.052, .0855), (.15, .055, .012), 'Tela', .003)
    # Los trazos de tiza son tubos abiertos: la unión booleana exacta los toma como base y
    # pierde el tablero. Por ahora la pizarra va limpia (lo escrito irá como textura o calco).
    return
    y = -.01
    # «3B» en la esquina superior izquierda.
    x0, z0 = -1.25, .93
    chalk([(x0 + .05*math.cos(math.radians(a)), y, z0 + .05 + .05*math.sin(math.radians(a))) for a in range(150, -91, -30)], .006)
    chalk([(x0 - .012, y, z0)] + [(x0 + .05*math.cos(math.radians(a)), y, z0 - .05 + .05*math.sin(math.radians(a))) for a in range(60, -161, -30)], .006)
    xb = x0 + .09
    chalk([(xb, y, z0 - .1), (xb, y, z0 + .1)], .006)
    for dz, dx in ((.05, .03), (-.05, .035)):
        bump = [(xb, y, z0 + dz + .05)] + [(xb + dx + .05*math.cos(math.radians(a)), y, z0 + dz + .05*math.sin(math.radians(a))) for a in range(60, -61, -30)] + [(xb, y, z0 + dz - .05)]
        chalk(bump, .006)
    chalk([(x0 - .07, y, z0 - .14), (x0 + .2, y, z0 - .145)], .005)
    rng = random.Random(4)
    for j in range(5):
        z = .72 - j*.09
        xs = -1.3
        xe = xs + rng.uniform(.6, .9)
        pts, x = [], xs
        while x < xe:
            pts.append((x, y, z + .012*math.sin(x*90) + rng.uniform(-.004, .004)))
            x += .035
        chalk(pts)
    ax, az = .55, .28
    chalk([(ax - .32, y, az), (ax - .32, y, az + .42), (ax + .32, y, az + .42), (ax + .32, y, az)], .005)
    chalk([(ax - .45, y, az), (ax + .45, y, az)], .005)
    arch = [(ax - .13, y, az)] + [(ax + .13*math.cos(math.radians(a)), y, az + .16 + .13*math.sin(math.radians(a))) for a in range(180, -1, -20)] + [(ax + .13, y, az)]
    chalk(arch, .005)
    chalk([(ax - .32, y, az + .42), (ax, y, az + .55), (ax + .32, y, az + .42)], .005)
    for sx in (-.24, .24):
        chalk([(ax + sx - .04, y, az + .2), (ax + sx + .04, y, az + .2), (ax + sx + .04, y, az + .3), (ax + sx - .04, y, az + .3), (ax + sx - .04, y, az + .2)], .004)
    pts = [(.95 + i*.03, y, .95 + .01*math.sin(i*1.7)) for i in range(10)]
    chalk(pts)

def maqueta_arequipa():
    S = .036
    box('Tablero', (0, 0, .015), (1.0, .7, .03), 'Madera', .006)
    box('Carton', (0, 0, .0335), (.97, .67, .007), 'Cemento', 0)
    prism('Misti', [(-.06, 0), (.46, 0), (.33, .11), (.25, .27), (.23, .29), (.17, .29), (.15, .27), (.07, .11)], .298, .302, 'Cemento', 'XZ', loc=(0, 0, S - .002))
    prism('Nieve del Misti', [(.165, .29), (.235, .29), (.25, .27), (.27, .22), (.245, .235), (.22, .215), (.2, .24), (.18, .215), (.155, .235), (.13, .22), (.15, .27)], .2955, .2985, 'Papel', 'XZ', loc=(0, 0, S - .002))
    prism('Chachani', [(-.48, 0), (.05, 0), (-.02, .12), (-.08, .17), (-.15, .2), (-.2, .18), (-.28, .21), (-.34, .17), (-.42, .1)], .328, .332, 'Tela', 'XZ', loc=(0, 0, S - .002))
    prism('Nieve del Chachani', [(-.17, .195), (-.15, .2), (-.12, .186), (-.15, .18)], .3255, .3285, 'Papel', 'XZ', loc=(0, 0, S - .002))
    for x, y0 in ((.2, .302), (-.2, .332)):
        prism('Apoyo', [(y0 - .001, 0), (y0 + .03, 0), (y0 - .001, .1)], x - .004, x + .004, 'Papel', 'YZ', loc=(0, 0, S - .002))
    center = [(-.5, .25), (-.42, .16), (-.355, .04), (-.325, -.1), (-.345, -.23), (-.38, -.35)]
    left, right = [], []
    for i, (x, yy) in enumerate(center):
        a, b = center[max(i-1, 0)], center[min(i+1, len(center)-1)]
        d = Vector((b[0] - a[0], b[1] - a[1], 0)).normalized()
        n = Vector((-d.y, d.x, 0))*.026
        left.append((x + n.x, yy + n.y)); right.append((x - n.x, yy - n.y))
    left[0] = (-.485, left[0][1]); right[0] = (-.485, right[0][1])
    prism('Rio Chili', left + list(reversed(right)), S - .0015, S + .0015, 'LibroB', 'XY')
    box('Puente', (-.325, -.1, S + .008), (.11, .022, .007), 'Sillar', .001)
    for dx in (-.03, .03): box('Pila puente', (-.325 + dx, -.1, S + .003), (.012, .02, .01), 'Sillar', 0)
    rng = random.Random(17)
    cols = (-.19, -.075, .04, .155, .27, .385)
    rows = (-.27, -.155, -.04, .075, .19)
    plaza, cathedral = (.04, -.04), (.04, .075)
    for cx in cols:
        for cy in rows:
            if (cx, cy) == plaza or (cx, cy) == cathedral: continue
            if cx == -.19 and cy == .19: continue
            parts = rng.choice(((.5, .5), (.35, .65), (.6, .4), (.33, .33, .34)))
            x = cx - .045
            split_y = rng.random() < .5
            for f in parts:
                w = .09*f - .004
                h = rng.uniform(.022, .05)
                mat = 'Papel' if rng.random() < .55 else 'Sillar'
                if split_y: loc, size = (cx, x + .004/2 + w/2 - cx + cy, S - .002 + h/2), (.086, w, h)
                else: loc, size = (x + .002 + w/2, cy, S - .002 + h/2), (w, .086, h)
                box('Casa', loc, size, mat, 0)
                if rng.random() < .45:
                    r = min(size[0], size[1])*.42
                    arc = [(r*math.cos(math.pi*i/5), r*math.sin(math.pi*i/5)) for i in range(6)]
                    prism('Boveda', arc, -.039, .039, mat, 'XZ', loc=(loc[0], loc[1], S - .002 + h - .001), rot=(0, 0, math.pi/2 if split_y else 0))
                x += .09*f
    px, py = plaza
    lathe('Pileta', [(0, 0), (.018, 0), (.018, .008), (.006, .01), (.004, .022), (.009, .026), (0, .026)], (px, py, S - .001), 'Papel', 10)
    for dx in (-.03, .03):
        for dy in (-.03, .03):
            ellipsoid('Arbol', (px + dx, py + dy, S + .012), (.026, .026, .03), 'Planta', 8, 5)
    cx, cy = cathedral
    box('Catedral', (cx, cy + .015, S - .002 + .028), (.1, .045, .056), 'Papel', .001)
    arc = [(.018*math.cos(math.pi*i/5), .018*math.sin(math.pi*i/5)) for i in range(6)]
    prism('Boveda catedral', arc, -.045, .045, 'Papel', 'XZ', loc=(cx, cy + .015, S - .002 + .055), rot=(0, 0, math.pi/2))
    for dx in (-.042, .042):
        box('Torre', (cx + dx, cy - .012, S - .002 + .05), (.022, .022, .1), 'Papel', .001)
        box('Campanario', (cx + dx, cy - .012, S - .002 + .108), (.018, .018, .02), 'Papel', .001)
        lathe('Cupula', [(0, 0), (.011, 0), (.009, .008), (.004, .013), (.001, .02), (0, .02)], (cx + dx, cy - .012, S - .002 + .117), 'Papel', 8)

# ---------------------------------------------------------------- D) Módulos de casona

def zocalo_y_cornisa(spans):
    """Zócalo de cemento y cornisa de arranque; spans = tramos en X con zócalo."""
    for x0, x1 in spans:
        box('Zocalo', ((x0 + x1)/2, 0, .4), (x1 - x0, T_WALL + .04, .8), 'Cemento', 0)
        box('Remate zocalo', ((x0 + x1)/2, 0, .82), (x1 - x0, T_WALL + .06, .04), 'Cemento', 0)
    box('Cornisa baja', (0, 0, H_WALL - .14), (2, T_WALL + .05, .08), 'Sillar', 0)
    box('Cornisa alta', (0, 0, H_WALL - .05), (2, T_WALL + .09, .1), 'Sillar', 0)

def muro_sillar_alto():
    box('Sillar', (0, 0, H_WALL/2), (2, T_WALL, H_WALL), 'Sillar', 0)
    zocalo_y_cornisa([(-1, 1)])

def voussoirs(cx, cz, r_in, r_out, a0, a1, count, y0, y1):
    """Dovelas en abanico con juntas de 12 mm; la clave crece y sobresale un poco más."""
    step, gap = (a1 - a0)/count, .012
    for i in range(count):
        s, e = a0 + i*step, a0 + (i + 1)*step
        key = i == count//2
        ro = r_out + (.06 if key else 0)
        pts = []
        for r, order in ((r_in, 1), (ro, -1)):
            ds = 0 if i == 0 else gap/(2*r)
            de = 0 if i == count - 1 else gap/(2*r)
            angles = (s + ds, (s + e)/2, e - de)[::order]
            pts += [(cx + r*math.cos(a), cz + r*math.sin(a)) for a in angles]
        dy = .02 if key else 0
        prism('Dovela', pts, y0 - dy, y1 + dy, 'Sillar', 'XZ')

def muro_sillar_arco():
    r, spring = .6, 2.1
    poly = [(-1, 0), (-r, 0), (-r, spring)]
    poly += [(r*math.cos(math.pi*(1 - i/24)), spring + r*math.sin(math.pi*(1 - i/24))) for i in range(1, 24)]
    poly += [(r, spring), (r, 0), (1, 0), (1, H_WALL), (-1, H_WALL)]
    prism('Sillar', poly, -T_WALL/2, T_WALL/2, 'Sillar', 'XZ')
    zocalo_y_cornisa([(-1, -r), (r, 1)])
    voussoirs(0, spring, r + .02, r + .24, 0, math.pi, 9, -T_WALL/2 - .025, T_WALL/2 + .025)
    for s in (-1, 1):
        box('Imposta', (s*(r + .17), 0, spring - .05), (.3, T_WALL + .06, .1), 'Sillar', .01)

def muro_sillar_ventana():
    w, bottom, top = 1.0, .9, 2.3
    side = (2 - w)/2
    for s in (-1, 1): box('Sillar lateral', (s*(1 - side/2), 0, H_WALL/2), (side, T_WALL, H_WALL), 'Sillar', 0)
    box('Antepecho', (0, 0, bottom/2), (w, T_WALL, bottom), 'Sillar', 0)
    box('Dintel', (0, 0, (top + H_WALL)/2), (w, T_WALL, H_WALL - top), 'Sillar', 0)
    zocalo_y_cornisa([(-1, 1)])
    # Arco adintelado de cinco dovelas que irradian desde un punto bajo el vano.
    cx, cz = 0, top - 1.0
    a_l, a_r = math.atan2(1.0, -w/2 - .06), math.atan2(1.0, w/2 + .06)
    for i in range(5):
        s = a_l + (a_r - a_l)*i/5 + .006
        e = a_l + (a_r - a_l)*(i + 1)/5 - .006
        def at(ang, z):
            return (cx + (z - cz)*math.cos(ang)/math.sin(ang), z)
        zb, zt = top - .005, top + .2 + (.04 if i == 2 else 0)
        pts = [at(s, zb), at(e, zb), at(e, zt), at(s, zt)]
        dy = .015 if i == 2 else 0
        prism('Dovela', pts, -T_WALL/2 - .02 - dy, T_WALL/2 + .02 + dy, 'Sillar', 'XZ')

def boveda_sillar():
    ri, ro, n = .85, 1.0, 24
    outer = [(ro*math.cos(math.pi*i/n), ro*math.sin(math.pi*i/n)) for i in range(n + 1)]
    inner = [(ri*math.cos(math.pi*i/n), ri*math.sin(math.pi*i/n)) for i in range(n, -1, -1)]
    prism('Boveda de canon', outer + inner, -1, 1, 'Sillar', 'XZ')
    for y0, y1 in ((-1, -.92), (.92, 1)):
        rib = [(ri + .01)*Vector((math.cos(math.pi*i/n), math.sin(math.pi*i/n), 0)) for i in range(n + 1)]
        rib_in = [(ri - .07)*Vector((math.cos(math.pi*i/n), math.sin(math.pi*i/n), 0)) for i in range(n, -1, -1)]
        prism('Arco fajon', [(v.x, v.y) for v in rib + rib_in], y0, y1, 'Sillar', 'XZ')

def spiral(cx, cz, y, r0, r1, a0, turns, mirror, n=10):
    pts = []
    for i in range(n):
        t = i/(n - 1)
        ang = a0 + t*turns*math.tau
        r = r0 + (r1 - r0)*t
        x = r*math.cos(ang)
        pts.append((cx - x if mirror else cx + x, y, cz + r*math.sin(ang)))
    return pts

def ventana_reja():
    w, h, T = 1.0, 1.4, T_WALL
    box('Alfeizar', (0, 0, .02), (w, T, .04), 'Sillar', .004)
    box('Peana', (0, -T/2 - .01, .025), (w + .16, .1, .05), 'Sillar', .008)
    yf = .06
    for s in (-1, 1): box('Jamba', (s*(w/2 - .03), yf, .72), (.06, .07, 1.36), bevel=.006)
    box('Cabezal', (0, yf, h - .03), (w, .07, .06), bevel=.006)
    box('Peinazo', (0, yf, .07), (w, .07, .06), bevel=.006)
    box('Travesano', (0, yf, 1.07), (w - .1, .065, .045), bevel=.005)
    box('Parteluz', (0, yf - .005, .585), (.045, .06, .95), bevel=.005)
    for s in (-1, 1): box('Junquillo', (s*.235, yf - .01, .585), (.44, .035, .03), bevel=.004)
    for s in (-1, 1): box('Vidrio', (s*.235, yf + .004, .585), (.42, .006, .93), 'Vidrio', 0)
    box('Vidrio', (0, yf + .004, 1.21), (.88, .006, .23), 'Vidrio', 0)
    for s in (-1, 1):
        rod('Pomo', (s*.04, yf - .035, .6), (s*.04, yf - .06, .6), .009, 'Dorado', 8)
    yr = -.1
    for z in (.07, .3, 1.15, 1.37):
        box('Pletina', (0, yr, z), (w - .004, .022, .028), 'Metal', 0)
    xs = [-.4375 + i*.125 for i in range(8)]
    for x in xs: rod('Barrote', (x, yr, .05), (x, yr, 1.39), .011, 'Metal', 4)
    for i in range(7):
        xc = (xs[i] + xs[i + 1])/2
        if i % 2 == 0:
            for mirror in (False, True):
                tube('Voluta', spiral(xc - (.02 if not mirror else -.02), 1.305, yr, .05, .016, math.pi/2, 1.05, mirror, 9), .0055, 'Metal', 4)
        else:
            tube('Anillo', ring_pts((xc, yr, .185), .058, 10, 'XZ'), .0055, 'Metal', 4, closed=True)
    for x in (-.4375, .4375): rod('Garra', (x, yr, 1.37), (x*1.16, yr, 1.37), .01, 'Metal', 4)

# ---------------------------------------------------------------- registro

KITS = {
    'Libreria': {'Estante_Libros': estante_libros, 'Estante_Libros_Bajo': estante_libros_bajo, 'Mostrador': mostrador,
                 'Canasta_Separadores': canasta_separadores, 'Sillon': sillon, 'Pila_Libros': pila_libros,
                 'Escalera_Madera': escalera_madera, 'Calendario_Pared': calendario_pared, 'Gato_Tinta': gato_tinta},
    'Tintero': {'Barra_Cafe': barra_cafe, 'Mesa_Cafe': mesa_cafe, 'Silla_Cafe': silla_cafe, 'Taza_Cafe': taza_cafe,
                'Tablon_Corcho': tablon_corcho, 'Lampara_Colgante': lampara_colgante},
    'Aula': {'Mesa_Dibujo': mesa_dibujo, 'Taburete': taburete, 'Pizarra': pizarra, 'Maqueta_Arequipa': maqueta_arequipa},
    'Arquitectura': {'Muro_Sillar_Arco': muro_sillar_arco, 'Boveda_Sillar': boveda_sillar, 'Muro_Sillar_Alto': muro_sillar_alto,
                     'Ventana_Reja': ventana_reja, 'Muro_Sillar_Ventana': muro_sillar_ventana},
}
TRI_LIMIT = {'Estante_Libros': 6000, 'Estante_Libros_Bajo': 6000}

def source_dir(root, kit): return root/'Art/Blender/Capitulo1'/kit
def export_dir(root, kit): return root/'Game/Assets/_Project/Art/Props'/kit

def mesh_stats(objects):
    tris, non_manifold = 0, 0
    for obj in objects:
        obj.data.calc_loop_triangles()
        tris += len(obj.data.loop_triangles)
        bm = bmesh.new(); bm.from_mesh(obj.data)
        non_manifold += sum(not e.is_manifold for e in bm.edges)
        bm.free()
    return tris, non_manifold

def build(root, kit, name, builder):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.context.preferences.filepaths.save_version = 0
    bpy.context.preferences.filepaths.file_preview_type = 'NONE'
    bpy.context.scene.unit_settings.system = 'METRIC'
    bpy.context.scene.unit_settings.scale_length = 1
    az.MATS.clear()
    az.materials()
    builder()
    bpy.context.view_layer.update()
    corners = [o.matrix_world @ Vector(c) for o in bpy.context.scene.objects if o.type == 'MESH' for c in o.bound_box]
    expected = [max(v[i] for v in corners) - min(v[i] for v in corners) for i in range(3)]
    objects, dimensions = consolidate(name)
    # Si la unión booleana pierde piezas, la caja final encoge: se detecta aquí.
    assert all(abs(a - b) < .003 for a, b in zip(expected, dimensions)), (name, 'la union perdio geometria', expected, list(dimensions))
    tris, non_manifold = mesh_stats(objects)
    limit = TRI_LIMIT.get(name, 3000)
    assert 0 < tris < limit, (name, tris)
    if non_manifold:
        for obj in objects:
            bm = bmesh.new(); bm.from_mesh(obj.data)
            bad = [tuple(round(c, 3) for c in (e.verts[0].co + e.verts[1].co)/2) for e in bm.edges if not e.is_manifold]
            bm.free()
            if bad: print('NON_MANIFOLD', obj.name, bad[:8], flush=True)
    assert non_manifold == 0, (name, 'aristas no manifold', non_manifold)
    bpy.ops.object.select_all(action='DESELECT')
    for obj in objects: obj.select_set(True)
    bpy.context.view_layer.objects.active = objects[0]
    src, exp = source_dir(root, kit), export_dir(root, kit)
    src.mkdir(parents=True, exist_ok=True); exp.mkdir(parents=True, exist_ok=True)
    blend, fbx = src/('SM_' + name + '.blend'), exp/('SM_' + name + '.fbx')
    bpy.ops.wm.save_as_mainfile(filepath=str(blend))
    bpy.ops.export_scene.fbx(filepath=str(fbx), use_selection=True, object_types={'MESH'}, axis_forward='-Z', axis_up='Y',
                             apply_unit_scale=True, apply_scale_options='FBX_SCALE_ALL', bake_anim=False, use_mesh_modifiers=True,
                             mesh_smooth_type='FACE', path_mode='AUTO', add_leaf_bones=False)
    print('PROP_OK', name, tris, flush=True)
    return {'kit': kit, 'name': 'SM_' + name, 'triangles': tris, 'triangle_limit': limit, 'dimensions_m_blender_xyz': [round(v, 5) for v in dimensions],
            'non_manifold_edges': non_manifold, 'fbx_bytes': fbx.stat().st_size,
            'materials': sorted({m.name for o in objects for m in o.data.materials}),
            'blend': blend.relative_to(root).as_posix(), 'fbx': fbx.relative_to(root).as_posix()}

def verify(root, report):
    """Reimporta cada FBX: tris, dimensiones, pivote en la base centrado, escala 1 y materiales M_."""
    results = []
    for item in report:
        path = root/item['fbx']
        bpy.ops.wm.read_factory_settings(use_empty=True)
        bpy.ops.import_scene.fbx(filepath=str(path), use_anim=False)
        meshes = [o for o in bpy.context.scene.objects if o.type == 'MESH']
        tris, non_manifold = mesh_stats(meshes)
        assert tris == item['triangles'], (path, tris)
        assert non_manifold == 0, (path, non_manifold)
        assert all(m.name.startswith('M_') for o in meshes for m in o.data.materials)
        coords = [o.matrix_world @ v.co for o in meshes for v in o.data.vertices]
        low = Vector(tuple(min(v[i] for v in coords) for i in range(3)))
        high = Vector(tuple(max(v[i] for v in coords) for i in range(3)))
        assert all(abs(a - b) < .0002 for a, b in zip(high - low, item['dimensions_m_blender_xyz'])), (path, list(high - low))
        assert abs(low.z) < 1e-5 and abs(low.x + high.x) < 1e-5 and abs(low.y + high.y) < 1e-5, path
        assert all(abs(v - 1) < 1e-5 for o in meshes for v in o.scale)
        results.append({'name': item['name'], 'triangles_reimported': tris, 'non_manifold_edges': 0,
                        'scale_meters_verified': True, 'base_center_pivot_verified': True})
        print('VERIFY_OK', item['name'], tris, flush=True)
    return results

def write_inventory(root, report):
    base = root/'Art/Blender/Capitulo1'
    lines = ['# Inventario del capítulo 1', '', 'Generado por `../scripts/generate_capitulo1.py`. Dimensiones en metros (X ancho × Y fondo × Z alto, ejes de Blender).', '']
    for kit in KITS:
        items = [r for r in report if r['kit'] == kit]
        if not items: continue
        lines += [f'## {kit}', '', '| Modelo | Tris | Dimensiones | Materiales | FBX |', '|---|---:|---|---|---|']
        for r in items:
            d = ' × '.join(f'{v:.2f}' for v in r['dimensions_m_blender_xyz'])
            lines.append(f"| {r['name']} | {r['triangles']} | {d} | {', '.join(r['materials'])} | `{r['fbx']}` |")
        lines += ['', f'Hoja de contactos: [contact_sheet_{kit}.png](previews/contact_sheet_{kit}.png)', '']
    (base/'INVENTARIO.md').write_text('\n'.join(lines), encoding='utf-8')

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--output', type=Path, required=True, help='Raíz del proyecto')
    parser.add_argument('--kits', nargs='*', default=None)
    parser.add_argument('--only', nargs='*', default=None, help='Nombres SM_ a regenerar')
    parser.add_argument('--render', action='store_true')
    parser.add_argument('--verify', action='store_true')
    parser.add_argument('--skip-build', action='store_true')
    args = parser.parse_args(sys.argv[sys.argv.index('--') + 1:])
    root = args.output.resolve()
    report_path = root/'Art/Blender/Capitulo1/geometry_report.json'
    report = json.loads(report_path.read_text(encoding='utf-8')) if report_path.is_file() else []
    selected = [(kit, name, fn) for kit, items in KITS.items() for name, fn in items.items()
                if (not args.kits or kit in args.kits) and (not args.only or 'SM_' + name in args.only)]
    if not args.skip_build:
        failures = []
        for kit, name, fn in selected:
            try:
                entry = build(root, kit, name, fn)
            except AssertionError as error:
                failures.append(repr(error)); print('PROP_FAIL', error, flush=True); continue
            report = [r for r in report if r['name'] != entry['name']] + [entry]
        order = [ 'SM_' + n for items in KITS.values() for n in items]
        report.sort(key=lambda r: order.index(r['name']))
        report_path.parent.mkdir(parents=True, exist_ok=True)
        report_path.write_text(json.dumps(report, indent=2, ensure_ascii=False), encoding='utf-8')
        write_inventory(root, report)
        assert not failures, failures
    names = {'SM_' + n for _, n, _ in selected}
    if args.render:
        import render_previews
        destination = root/'Art/Blender/Capitulo1/previews'
        for kit in KITS:
            if args.kits and kit not in args.kits: continue
            render_previews.render(source_dir(root, kit), sorted(names), destination)
    if args.verify:
        results = verify(root, [r for r in report if r['name'] in names])
        vpath = root/'Art/Blender/Capitulo1/verification_report.json'
        old = json.loads(vpath.read_text(encoding='utf-8')) if vpath.is_file() else []
        merged = {r['name']: r for r in old}
        merged.update({r['name']: r for r in results})
        vpath.write_text(json.dumps([merged[k] for k in sorted(merged)], indent=2), encoding='utf-8')

if __name__ == '__main__': main()
