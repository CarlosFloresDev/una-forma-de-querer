"""Hoja de revisión opcional: Python + Pillow; --output raíz del proyecto."""
import argparse
from pathlib import Path
from PIL import Image, ImageDraw

def main():
    parser=argparse.ArgumentParser()
    parser.add_argument('--output',type=Path,required=True)
    args=parser.parse_args()
    folder=args.output/'Art/Blender/Azotea/previews'
    paths=sorted(folder.glob('SM_*.png'))
    sheet=Image.new('RGB',(1024,280*((len(paths)+3)//4)),(236,231,222))
    draw=ImageDraw.Draw(sheet)
    for i,path in enumerate(paths):
        x,y=(i%4)*256,(i//4)*280
        with Image.open(path) as image:sheet.paste(image.convert('RGB').resize((256,256)),(x,y))
        draw.text((x+10,y+261),path.stem,fill=(45,43,42))
    sheet.save(folder/'contact_sheet.png')

if __name__=='__main__':main()
