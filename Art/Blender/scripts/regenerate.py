"""Regenerar todo con un comando; Python con Pillow y Blender 5.2.3."""
import argparse,os,subprocess,sys
from pathlib import Path

def main():
    parser=argparse.ArgumentParser()
    parser.add_argument('--blender',default='blender')
    parser.add_argument('--output',type=Path,default=Path(__file__).resolve().parents[3])
    parser.add_argument('--temp-root',type=Path,required=True)
    args=parser.parse_args()
    env=os.environ.copy()
    env.update(TEMP=str(args.temp_root),TMP=str(args.temp_root),PYTHONDONTWRITEBYTECODE='1',PYTHONWARNINGS='ignore::DeprecationWarning')
    scripts=Path(__file__).resolve().parent
    for script,extra in [('render_previews.py',['--generate']),('verify_exports.py',[])]:
        subprocess.run([args.blender,'-b','--factory-startup','--python-exit-code','1','-P',str(scripts/script),'--','--output',str(args.output.resolve()),*extra],env=env,check=True)
    subprocess.run([sys.executable,str(scripts/'contact_sheet.py'),'--output',str(args.output.resolve())],env=env,check=True)
    print('Kit generado, renderizado y verificado: 17 props.')

if __name__=='__main__':main()
