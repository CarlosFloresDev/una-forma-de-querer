"""Verificación real de cada FBX mediante reimportación en Blender."""
import argparse,json,sys
from pathlib import Path
import bpy,bmesh
from mathutils import Vector

def verify(root):
    source=root/'Art/Blender/Azotea'
    report=json.loads((source/'geometry_report.json').read_text(encoding='utf-8'))
    results=[]
    assert len(report)==17
    for item in report:
        path=root/item['fbx']
        assert path.is_file() and path.stat().st_size>1000,path
        assert (root/item['blend']).is_file()
        bpy.ops.wm.read_factory_settings(use_empty=True)
        bpy.ops.import_scene.fbx(filepath=str(path),use_anim=False)
        meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
        triangles=0
        non_manifold=0
        for obj in meshes:
            obj.data.calc_loop_triangles()
            triangles+=len(obj.data.loop_triangles)
            bm=bmesh.new();bm.from_mesh(obj.data)
            non_manifold+=sum(not e.is_manifold for e in bm.edges)
            bm.free()
            assert all(m.name.startswith('M_') for m in obj.data.materials)
        assert triangles==item['triangles'],(path,triangles,item['triangles'])
        assert non_manifold==0,(path,non_manifold)
        assert triangles<3000
        coords=[o.matrix_world@v.co for o in meshes for v in o.data.vertices]
        low=Vector(tuple(min(v[i] for v in coords) for i in range(3)))
        high=Vector(tuple(max(v[i] for v in coords) for i in range(3)))
        dimensions=high-low
        assert all(abs(a-b)<.0001 for a,b in zip(dimensions,item['dimensions_m_blender_xyz'])),(path,list(dimensions))
        assert abs(low.z)<.00001 and abs(low.x+high.x)<.00001 and abs(low.y+high.y)<.00001
        assert all(o.location.length<.00001 for o in meshes)
        assert all(abs(v-1)<.00001 for o in meshes for v in o.scale)
        image=bpy.data.images.load(str(source/'previews'/(item['name']+'.png')))
        assert tuple(image.size)==(512,512)
        bpy.data.images.remove(image)
        results.append({'name':item['name'],'fbx_exists':True,'triangles_reimported':triangles,'non_manifold_edges':non_manifold,'scale_meters_verified':True,'base_center_pivot_verified':True,'preview_512px':True})
        print('VERIFY_OK',item['name'],triangles,flush=True)
    (source/'verification_report.json').write_text(json.dumps(results,indent=2),encoding='utf-8')
    return results

def main():
    parser=argparse.ArgumentParser()
    parser.add_argument('--output',type=Path,required=True)
    args=parser.parse_args(sys.argv[sys.argv.index('--')+1:])
    verify(args.output.resolve())

if __name__=='__main__':main()
