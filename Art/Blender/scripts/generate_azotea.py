"""Kit de Mateo: Blender -- --output <raíz del proyecto>."""
import argparse, json, math, sys
from pathlib import Path
import bpy, bmesh
from mathutils import Vector

PALETTE = {'Sillar':'EDE6DA','Madera':'8A5A3C','Tela':'6F7C8C','Lana':'B5523B','Metal':'3B3A3F','Papel':'F3EAD3','Dorado':'D9A441','Vidrio':'A7C8CD'}
MATS = {}
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
        if name=='Vidrio':
            node.inputs['Transmission Weight'].default_value=.65
            node.inputs['Roughness'].default_value=.22
        MATS[name]=mat

def finish(obj,name,mat):
    obj.name=name
    obj.data.materials.append(MATS[mat])
    bpy.context.view_layer.objects.active=obj
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    return obj

def box(name,loc,size,mat='Madera',bevel=.012):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc)
    obj=bpy.context.object
    obj.dimensions=size
    finish(obj,name,mat)
    if bevel:
        mod=obj.modifiers.new('Bisel suave','BEVEL')
        mod.width=min(bevel,min(size)*.22)
        mod.segments=1
        bpy.ops.object.modifier_apply(modifier=mod.name)
    return obj

def ellipsoid(name,loc,size,mat='Tela',segments=16,rings=8):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments,ring_count=rings,radius=1,location=loc)
    obj=bpy.context.object
    obj.scale=tuple(v/2 for v in size)
    finish(obj,name,mat)
    for p in obj.data.polygons:p.use_smooth=True
    return obj

def rod(name,a,b,radius=.018,mat='Metal',vertices=10):
    vec=Vector(b)-Vector(a)
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=radius,depth=vec.length,location=(Vector(a)+Vector(b))/2)
    obj=bpy.context.object
    obj.rotation_euler=vec.to_track_quat('Z','Y').to_euler()
    return finish(obj,name,mat)

def mesh_object(name,verts,faces,mat):
    mesh=bpy.data.meshes.new(name)
    mesh.from_pydata(verts,[],faces)
    mesh.update()
    obj=bpy.data.objects.new(name,mesh)
    bpy.context.collection.objects.link(obj)
    return finish(obj,name,mat)

def bed():
    for x in (-.46,.46):
        for y in (-.97,.97):rod('Pata',(x,y,0),(x,y,.68 if y>0 else .48),.028)
        box('Larguero',(x,0,.30),(.045,1.98,.08),'Metal')
    for y in (-.97,.97):
        rod('Travesaño',(-.46,y,.34),(.46,y,.34),.025)
        rod('Baranda',(-.46,y,.64 if y>0 else .45),(.46,y,.64 if y>0 else .45),.023)
    for x in (-.23,0,.23):rod('Cabecera',(x,.97,.35),(x,.97,.64),.015)
    box('Somier',(0,0,.32),(.9,1.87,.07))
    box('Colchon',(0,0,.435),(.91,1.89,.18),'Sillar',.07)
    obj=box('Almohada',(0,.62,.56),(.64,.38,.13),'Papel',.07)
    obj.rotation_euler.z=-.05
    box('Frazada doblada',(0,-.58,.56),(.94,.51,.095),'Lana',.035)
    for y in (-.77,-.42):box('Banda tejida',(0,y,.607),(.89,.025,.008),'Dorado',.003)
    for x in (-.37,-.23,-.09,.05,.19,.33):rod('Fleco',(x,-.829,.558),(x,-.87,.53),.007,'Lana',6)

def desk():
    box('Tablero',(0,0,.75),(1.18,.60,.065),bevel=.024)
    for x in (-.49,.49):
        for y in (-.215,.215):box('Pata',(x,y,.36),(.067,.067,.72))
        box('Faldon lateral',(x,0,.65),(.045,.47,.15))
    box('Faldon trasero',(0,.235,.65),(1.03,.045,.15))
    box('Cajon',(.21,-.012,.65),(.48,.46,.13))
    box('Frente cajon',(.21,-.251,.65),(.5,.035,.14),bevel=.008)
    rod('Tirador',(.135,-.291,.65),(.285,-.291,.65),.012,'Dorado')
    for x in (.135,.285):rod('Soporte tirador',(x,-.29,.65),(x,-.26,.65),.01,'Dorado')

def chair():
    box('Asiento',(0,0,.445),(.44,.43,.065),bevel=.026)
    for x in (-.165,.165):
        for y in (-.16,.16):box('Pata',(x,y,.215),(.052,.052,.43))
        box('Montante respaldo',(x,.16,.65),(.05,.055,.47))
        rod('Travesaño',(x,-.16,.18),(x,.16,.18),.018,'Madera',8)
    for z in (.68,.82):box('Respaldo',(0,.16,z),(.41,.055,.085),bevel=.018)

def notebook():
    box('Hojas A4',(0,0,.015),(.203,.290,.021),'Papel',.003)
    for z in (.004,.029):box('Tapa',(0,0,z),(.218,.304,.006),'Tela',.003)
    box('Lomo',(-.106,0,.016),(.012,.300,.028),'Tela',.004)
    box('Etiqueta',(.014,.045,.032),(.117,.08,.002),'Papel',.0003)
    box('Trazo etiqueta',(.014,.059,.033),(.074,.003,.001),'Dorado',0)
    box('Elastico',(.078,0,.032),(.009,.297,.004),'Lana',.001)
    box('Marcapaginas',(-.043,-.15,.015),(.016,.035,.002),'Dorado',.001)

def rounded_prism(name,loc,size,radius,mat):
    # Radio XY independiente del espesor: silueta reconocible del celular.
    w,h,d=size
    ring=[]
    for cx,cy,start in ((w/2-radius,h/2-radius,0),(-w/2+radius,h/2-radius,90),(-w/2+radius,-h/2+radius,180),(w/2-radius,-h/2+radius,270)):
        for step in range(5):
            angle=math.radians(start+step*22.5)
            ring.append((cx+radius*math.cos(angle),cy+radius*math.sin(angle)))
    verts=[(x+loc[0],y+loc[1],z+loc[2]) for z in (-d/2,d/2) for x,y in ring]
    n=len(ring)
    faces=[tuple(reversed(range(n))),tuple(range(n,2*n))]
    faces += [(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    return mesh_object(name,verts,faces,mat)

def phone():
    rounded_prism('Carcasa',(0,0,.006),(.074,.149,.012),.009,'Metal')
    rounded_prism('Pantalla',(0,-.001,.01275),(.064,.129,.0015),.007,'Vidrio')
    box('Auricular',(0,.066,.012),(.017,.0025,.001),'Metal',.001)
    box('Boton lateral',(.037,.023,.007),(.002,.018,.004),'Dorado',.001)

def heater():
    for x in (-.145,.145):box('Pie',(x,0,.025),(.06,.24,.05),'Metal')
    box('Cuerpo',(0,0,.22),(.40,.16,.36),'Sillar',.04)
    box('Panel',(0,-.079,.225),(.33,.015,.25),'Metal',.018)
    for z in (.16,.23,.30):rod('Resistencia',(-.13,-.093,z),(.13,-.093,z),.01,'Lana',8)
    for x in (-.14,-.07,0,.07,.14):rod('Rejilla',(x,-.103,.115),(x,-.103,.345),.004,'Metal',6)
    rod('Control',(.13,-.074,.367),(.13,-.10,.367),.019,'Dorado',10)
    box('Asa superior',(0,0,.401),(.16,.035,.026),'Metal')

def lamp():
    ellipsoid('Base',(0,0,.023),(.23,.19,.046),'Metal',16,6)
    rod('Brazo inferior',(0,0,.025),(.015,.07,.31),.016)
    rod('Brazo superior',(.015,.07,.31),(.015,-.07,.49),.014)
    ellipsoid('Articulacion',(.015,.07,.31),(.047,.047,.047),'Dorado',10,6)
    verts=[]
    for z,r in ((.40,.105),(.50,.04),(.40,.098),(.494,.034)):
        verts.extend([(.015+r*math.cos(i*math.tau/16),-.09+r*math.sin(i*math.tau/16),z) for i in range(16)])
    faces=[]
    for i in range(16):
        j=(i+1)%16
        faces.extend([(i,j,16+j,16+i),(32+j,32+i,48+i,48+j),(j,i,32+i,32+j),(16+i,16+j,48+j,48+i)])
    mesh_object('Pantalla hueca',verts,faces,'Metal')
    ellipsoid('Bombilla',(.015,-.09,.438),(.045,.045,.065),'Papel',10,6)

def frame():
    box('Respaldo',(0,.009,.14),(.21,.018,.28),bevel=.005)
    box('Papel foto',(0,-.001,.14),(.174,.005,.244),'Papel',.001)
    for x in (-.101,.101):box('Marco vertical',(x,-.012,.14),(.018,.035,.28),bevel=.004)
    for z in (.009,.271):box('Marco horizontal',(0,-.012,z),(.19,.035,.018),bevel=.004)
    ellipsoid('Sol foto',(.044,-.005,.20),(.035,.006,.035),'Dorado',12,6)
    box('Paisaje',(0,-.005,.068),(.165,.006,.07),'Tela',.001)
    for x in (-.035,.027):
        ellipsoid('Retrato',(x,-.009,.10),(.038,.012,.065),'Lana',10,6)
        ellipsoid('Cabeza retrato',(x,-.009,.145),(.027,.012,.028),'Madera',10,6)
    leg=box('Apoyo',(0,.07,.09),(.04,.025,.19),bevel=.004)
    leg.rotation_euler.x=-.62

def window():
    for x in (-.57,.57):box('Jamba',(x,0,.60),(.06,.14,1.20))
    for z in (.03,1.17):box('Travesaño',(0,0,z),(1.08,.14,.06))
    box('Parteluz',(0,-.015,.60),(.035,.065,1.08),bevel=.006)
    box('Peana',(0,-.04,.018),(1.28,.23,.036),bevel=.008)
    for x in (-.279,.279):box('Vidrio',(x,.015,.60),(.523,.008,1.08),'Vidrio',0)
    box('Pestillo',(.055,-.053,.58),(.08,.025,.025),'Dorado',.004)

def door():
    for x in (-.48,.48):box('Jamba',(x,0,1.05),(.06,.18,2.10))
    box('Dintel',(0,0,2.07),(.924,.18,.06))
    box('Hoja',(0,-.006,1.019),(.882,.055,2.025),bevel=.01)
    for x in (-.29,0,.29):
        for z,h in ((.49,.70),(1.48,.86)):box('Panel',(x,-.04,z),(.247,.025,h),bevel=.014)
    box('Placa manija',(.325,-.056,1.01),(.04,.014,.14),'Metal',.005)
    rod('Cuello manija',(.325,-.06,1.04),(.325,-.10,1.04),.012,'Dorado')
    rod('Manija',(.325,-.10,1.04),(.235,-.10,1.04),.012,'Dorado')
    for z in (.26,1.78):rod('Bisagra',(-.438,-.028,z-.04),(-.438,-.028,z+.04),.026)

def whale():
    ellipsoid('Cuerpo',(0,-.015,.118),(.29,.40,.235),'Tela',16,10)
    ellipsoid('Panza',(0,-.046,.056),(.235,.30,.092),'Sillar',16,8)
    for x in (-1,1):
        obj=ellipsoid('Aleta',(x*.146,-.025,.075),(.14,.18,.045),'Tela',12,6)
        obj.rotation_euler.z=x*.48
        obj=ellipsoid('Cola',(x*.066,.231,.133),(.17,.135,.042),'Tela',12,6)
        obj.rotation_euler.z=x*-.5
        ellipsoid('Ojo bordado',(x*.125,-.116,.148),(.014,.018,.018),'Metal',10,6)
    ellipsoid('Pedunculo',(0,.17,.115),(.095,.18,.09),'Tela',12,6)
    for x in (-.025,0,.025):rod('Costura',(x,-.185,.072),(x,-.194,.083),.0025,'Dorado',6)

def suitcase():
    box('Maleta',(0,0,.225),(.64,.23,.45),bevel=.055)
    for x in (-.20,.20):
        for y in (-.114,.114):box('Correa',(x,y,.225),(.035,.018,.42),'Lana',.004)
        box('Correa superior',(x,0,.448),(.035,.23,.012),'Lana',.003)
        box('Cierre',(x,-.124,.35),(.054,.02,.053),'Dorado',.006)
    for x in (-.275,.275):
        for z in (.048,.402):box('Esquinera',(x,-.112,z),(.065,.025,.052),'Metal',.01)
    for x in (-.085,.085):rod('Asa soporte',(x,0,.445),(x,0,.505),.014)
    rod('Asa',(-.085,0,.505),(.085,0,.505),.019)
    box('Etiqueta viajera',(.052,-.116,.26),(.11,.012,.073),'Papel',.004)

def wall(opening=None):
    if not opening:box('Sillar continuo',(0,0,1.3),(2,.18,2.6),'Sillar',0)
    else:
        width,bottom,top=(1.20,1.,2.20) if opening=='window' else (1.02,0,2.10)
        side=(2-width)/2
        for x in (-1+side/2,1-side/2):box('Sillar lateral',(x,0,1.3),(side,.18,2.6),'Sillar',0)
        box('Sillar dintel',(0,0,(top+2.6)/2),(width,.18,2.6-top),'Sillar',0)
        if bottom:box('Antepecho',(0,0,bottom/2),(width,.18,bottom),'Sillar',0)

def floor():box('Losa',(0,0,.06),(2,2,.12),'Sillar',0)

def roof():
    steps=160
    verts=[]
    for layer in (0,1):
        for y in (-1,1):
            for i in range(steps+1):
                x=-1+2*i/steps
                z=.018*(1-math.cos(i/steps*20*math.tau))+.1*(y+1)+layer*.006
                verts.append((x,y,z))
    n=steps+1
    faces=[]
    for i in range(steps):faces += [(i,i+1,n+i+1,n+i),(2*n+i,3*n+i,3*n+i+1,2*n+i+1),(i,2*n+i,2*n+i+1,i+1),(n+i,n+i+1,3*n+i+1,3*n+i)]
    faces += [(0,n,3*n,2*n),(n-1,3*n-1,4*n-1,2*n-1)]
    mesh_object('Calamina espesor 6mm',verts,faces,'Metal')

BUILDERS={'Cama':bed,'Escritorio':desk,'Silla':chair,'Cuaderno':notebook,'Celular':phone,'Estufa':heater,'Lampara':lamp,'MarcoFoto':frame,'Ventana':window,'Puerta':door,'Ballena':whale,'Maleta':suitcase,'Muro_Liso':lambda:wall(),'Muro_Ventana':lambda:wall('window'),'Muro_Puerta':lambda:wall('door'),'Piso':floor,'Techo_Calamina':roof}

def consolidate(name):
    solids=[o for o in bpy.context.scene.objects if o.type=='MESH' and o.data.materials[0].name!='M_Vidrio']
    glass=[o for o in bpy.context.scene.objects if o.type=='MESH' and o.data.materials[0].name=='M_Vidrio']
    base=solids[0]
    bpy.context.view_layer.objects.active=base
    # Unión exacta de sólidos: elimina superficies interiores de las piezas ensambladas.
    for obj in solids[1:]:
        for mat in obj.data.materials:
            if mat.name not in base.data.materials:base.data.materials.append(mat)
        mod=base.modifiers.new('Eliminar caras internas','BOOLEAN')
        mod.operation='UNION'
        mod.solver='EXACT'
        mod.object=obj
        bpy.ops.object.modifier_apply(modifier=mod.name)
        bpy.data.objects.remove(obj,do_unlink=True)
    base.name='SM_'+name
    objects=[base]+glass
    for i,obj in enumerate(glass):obj.name='SM_'+name+'_Vidrio_'+str(i+1)
    for obj in objects:
        bpy.ops.object.select_all(action='DESELECT')
        obj.select_set(True)
        bpy.context.view_layer.objects.active=obj
        bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.mesh.select_all(action='SELECT')
        bpy.ops.mesh.remove_doubles(threshold=.0000001)
        bpy.ops.mesh.dissolve_limited(angle_limit=.001)
        bpy.ops.mesh.normals_make_consistent(inside=False)
        bpy.ops.object.mode_set(mode='OBJECT')
    for obj in objects:
        bpy.context.view_layer.objects.active=obj
        mod=obj.modifiers.new('Triangulacion estable para Unity','TRIANGULATE')
        bpy.ops.object.modifier_apply(modifier=mod.name)
    coords=[v.co.copy() for obj in objects for v in obj.data.vertices]
    low=Vector(tuple(min(v[i] for v in coords) for i in range(3)))
    high=Vector(tuple(max(v[i] for v in coords) for i in range(3)))
    offset=Vector(((low.x+high.x)/2,(low.y+high.y)/2,low.z))
    for obj in objects:
        for v in obj.data.vertices:v.co-=offset
    return objects,list(high-low)

def write_inventory(report, source):
    lines = ['# Inventario del kit Azotea', '', '| Modelo | Tris | Fuente | FBX | Miniatura |', '|---|---:|---|---|---|']
    for item in report:
        name = item['name']
        lines.append(f"| {name} | {item['triangles']} | [{name}.blend]({name}.blend) | [{name}.fbx](../../../Game/Assets/_Project/Art/Props/Azotea/{name}.fbx) | [{name}.png](previews/{name}.png) |")
    lines += ['', '17 modelos. Todos por debajo de 3.000 tris. Miniaturas Eevee de 512 × 512.', '', 'Scripts: `generate_azotea.py`, `render_previews.py`, `verify_exports.py`, `contact_sheet.py`, `regenerate.py` en `../scripts/`.', '', 'Verificación geométrica: `geometry_report.json` y `verification_report.json`. Revisión conjunta: [contact_sheet.png](previews/contact_sheet.png).', '']
    (source/'INVENTARIO.md').write_text('\n'.join(lines), encoding='utf-8')

def main():
    parser=argparse.ArgumentParser()
    parser.add_argument('--output',type=Path,required=True,help='Raíz del proyecto')
    args=parser.parse_args(sys.argv[sys.argv.index('--')+1:])
    root=args.output.resolve()
    source=root/'Art/Blender/Azotea'
    export=root/'Game/Assets/_Project/Art/Props/Azotea'
    source.mkdir(parents=True,exist_ok=True)
    export.mkdir(parents=True,exist_ok=True)
    report=[]
    for name,builder in BUILDERS.items():
        bpy.ops.wm.read_factory_settings(use_empty=True)
        bpy.context.preferences.filepaths.save_version=0
        bpy.context.preferences.filepaths.file_preview_type='NONE'
        bpy.context.scene.unit_settings.system='METRIC'
        bpy.context.scene.unit_settings.scale_length=1
        materials()
        builder()
        objects,dimensions=consolidate(name)
        tris,non_manifold=0,0
        for obj in objects:
            obj.data.calc_loop_triangles()
            tris+=len(obj.data.loop_triangles)
            bm=bmesh.new()
            bm.from_mesh(obj.data)
            non_manifold+=sum(not e.is_manifold for e in bm.edges)
            bm.free()
        assert 0<tris<3000,(name,tris)
        assert non_manifold==0,(name,'aristas no manifold',non_manifold)
        bpy.ops.object.select_all(action='DESELECT')
        for obj in objects:obj.select_set(True)
        bpy.context.view_layer.objects.active=objects[0]
        blend=source/('SM_'+name+'.blend')
        fbx=export/('SM_'+name+'.fbx')
        bpy.ops.wm.save_as_mainfile(filepath=str(blend))
        bpy.ops.export_scene.fbx(filepath=str(fbx),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL',bake_anim=False,use_mesh_modifiers=True,mesh_smooth_type='FACE',path_mode='AUTO',add_leaf_bones=False)
        report.append({'name':'SM_'+name,'triangles':tris,'dimensions_m_blender_xyz':dimensions,'non_manifold_edges':non_manifold,'fbx_bytes':fbx.stat().st_size,'materials':sorted({m.name for o in objects for m in o.data.materials}),'blend':str(blend.relative_to(root)),'fbx':str(fbx.relative_to(root))})
        print('PROP_OK',name,tris,flush=True)
    (source/'geometry_report.json').write_text(json.dumps(report,indent=2,ensure_ascii=False),encoding='utf-8')
    write_inventory(report,source)

if __name__=='__main__':main()



