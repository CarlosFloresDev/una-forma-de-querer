"""Miniaturas Eevee del kit; --output raíz, --generate regenera antes de renderizar."""
import argparse, math, sys
from pathlib import Path
import bpy
from mathutils import Vector

def point_at(obj,target):obj.rotation_euler=(Vector(target)-obj.location).to_track_quat('-Z','Y').to_euler()

def render(source, only=None):
    destination=source/'previews'
    destination.mkdir(parents=True,exist_ok=True)
    for path in sorted(source.glob('SM_*.blend')):
        if only and path.stem not in only: continue
        bpy.ops.wm.open_mainfile(filepath=str(path))
        scene=bpy.context.scene
        scene.render.engine='BLENDER_EEVEE'
        scene.render.resolution_x=512
        scene.render.resolution_y=512
        scene.render.resolution_percentage=100
        scene.render.image_settings.file_format='PNG'
        scene.render.film_transparent=False
        scene.render.filepath=str(destination/(path.stem+'.png'))
        scene.render.image_settings.color_mode='RGBA'
        scene.view_settings.view_transform='AgX'
        scene.world=bpy.data.worlds.new('Fondo neutro')
        scene.world.use_nodes=True
        scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.22,.23,.25,1)
        scene.world.node_tree.nodes['Background'].inputs[1].default_value=.65
        meshes=[o for o in scene.objects if o.type=='MESH']
        scene.eevee.taa_render_samples=256 if any(m.name=='M_Vidrio' for o in meshes for m in o.data.materials) else 64
        coords=[o.matrix_world@Vector(v) for o in meshes for v in o.bound_box]
        low=Vector(tuple(min(v[i] for v in coords) for i in range(3)))
        high=Vector(tuple(max(v[i] for v in coords) for i in range(3)))
        center=(high+low)/2
        span=max(high-low)
        bpy.ops.object.camera_add(location=center+Vector((1.35,-1.9,1.25))*span)
        camera=bpy.context.object
        camera.name='PREVIEW_Camera'
        point_at(camera,center)
        camera.data.type='ORTHO'
        scene.camera=camera
        # Ajustar cámara a las ocho esquinas proyectadas: ningún prop queda recortado.
        inverse=camera.rotation_euler.to_matrix().transposed()
        projected=[inverse@(v-center) for v in coords]
        camera.data.ortho_scale=max(max(v.x for v in projected)-min(v.x for v in projected),max(v.y for v in projected)-min(v.y for v in projected))*1.22
        camera.data.clip_start=.0001
        camera.data.clip_end=1000
        bpy.ops.mesh.primitive_plane_add(size=200*span,location=(0,0,-.002*span))
        ground=bpy.context.object
        ground.name='PREVIEW_Suelo'
        mat=bpy.data.materials.new('PREVIEW_Neutro')
        mat.diffuse_color=(.23,.24,.26,1)
        ground.data.materials.append(mat)
        for label,position,power,size in [('Principal',(-3,-4,6),700,4),('Relleno',(4,-1,3),350,3),('Borde',(0,4,5),500,3)]:
            bpy.ops.object.light_add(type='AREA',location=center+Vector(position)*span)
            light=bpy.context.object
            light.name='PREVIEW_'+label
            light.data.energy=power*span*span
            light.data.shape='DISK'
            light.data.size=size*span
            point_at(light,center)
        bpy.ops.render.render(write_still=True)
        print('PREVIEW_OK',path.stem,flush=True)

def main():
    parser=argparse.ArgumentParser()
    parser.add_argument('--output',type=Path,required=True)
    parser.add_argument('--generate',action='store_true')
    parser.add_argument('--only',nargs='*',help='Nombres SM_ para revisi?n selectiva')
    args=parser.parse_args(sys.argv[sys.argv.index('--')+1:])
    if args.generate:
        import importlib.util
        spec=importlib.util.spec_from_file_location('azotea_generator',Path(__file__).with_name('generate_azotea.py'))
        module=importlib.util.module_from_spec(spec)
        spec.loader.exec_module(module)
        old=sys.argv[:]
        sys.argv=['blender','--','--output',str(args.output)]
        module.main()
        sys.argv=old
    render(args.output.resolve()/'Art/Blender/Azotea',args.only)

if __name__=='__main__':main()

