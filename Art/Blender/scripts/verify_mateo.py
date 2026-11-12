"""Reimporta CH_Mateo.fbx y comprueba tris, huesos, clips, pesos, escala y pivote: Blender -b -P verify_mateo.py -- --output <raíz>."""
import argparse, json, sys
from pathlib import Path
import bpy
from mathutils import Vector

HUMANOID = ['Hips','Spine','Chest','UpperChest','Neck','Head'] + [s+b for s in ('Left','Right') for b in ('Shoulder','UpperArm','LowerArm','Hand','UpperLeg','LowerLeg','Foot','Toes')]
CLIPS = {'Idle':2.0, 'Walk':1.0, 'LookPhone':2.0}

def verify(root):
    source = root/'Art/Blender/Characters/Mateo'
    report = json.loads((source/'character_report.json').read_text(encoding='utf-8'))
    fbx = root/report['fbx']
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=str(fbx), use_anim=True)
    fps = bpy.context.scene.render.fps / bpy.context.scene.render.fps_base
    meshes = [o for o in bpy.context.scene.objects if o.type=='MESH']
    rigs = [o for o in bpy.context.scene.objects if o.type=='ARMATURE']
    assert len(meshes)==1 and len(rigs)==1, (meshes, rigs)
    mesh, rig = meshes[0], rigs[0]
    mesh.data.calc_loop_triangles()
    tris = len(mesh.data.loop_triangles)
    assert tris == report['triangles'] and tris < 12000, tris
    bones = [b.name for b in rig.data.bones]
    assert sorted(bones) == sorted(HUMANOID), bones
    assert all(m.name.startswith('M_') for m in mesh.data.materials)
    unweighted = sum(1 for v in mesh.data.vertices if not any(g.weight > 0 for g in v.groups))
    max_inf = max(sum(1 for g in v.groups if g.weight > 0) for v in mesh.data.vertices)
    assert unweighted == 0 and max_inf <= 4, (unweighted, max_inf)
    coords = [mesh.matrix_world @ v.co for v in mesh.data.vertices]
    low = Vector(tuple(min(v[i] for v in coords) for i in range(3)))
    high = Vector(tuple(max(v[i] for v in coords) for i in range(3)))
    assert abs(low.z) < 1e-3 and abs(high.z-1.72) < .01, (low, high)
    feet = [rig.data.bones[s+"Foot"].head_local for s in ("Left","Right")]
    assert abs(feet[0].x+feet[1].x) < 1e-4 and abs(low.x+high.x) < .01, (low, high)
    actions = []
    for act in bpy.data.actions:
        start, end = act.frame_range
        actions.append({'name':act.name, 'frames':[start,end], 'seconds':round((end-start)/fps,4)})
    for clip, seconds in CLIPS.items():
        match = [a for a in actions if a['name'].split('|')[-1]==clip]
        assert match and abs(match[0]['seconds']-seconds) < 1e-3, (clip, actions)
    result = {'fbx':report['fbx'], 'triangles_reimported':tris, 'bones':bones, 'actions':actions, 'fps':fps, 'unweighted_vertices':unweighted, 'max_influences':max_inf, 'height_m':high.z-low.z, 'width_m':high.x-low.x, 'pivot_feet_centered':True, 'materials':[m.name for m in mesh.data.materials]}
    (source/'verification_report.json').write_text(json.dumps(result, indent=2, ensure_ascii=False), encoding='utf-8')
    print('VERIFY_OK', json.dumps(result, ensure_ascii=False), flush=True)

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index('--')+1:])
    verify(args.output.resolve())

if __name__ == '__main__': main()
