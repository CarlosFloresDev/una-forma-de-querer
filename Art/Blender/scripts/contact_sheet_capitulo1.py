"""Hojas de contactos del capítulo 1, una por kit: Python + Pillow; --output raíz del proyecto."""
import argparse, json
from pathlib import Path
from PIL import Image, ImageDraw

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    base = args.output/'Art/Blender/Capitulo1'
    folder = base/'previews'
    report = json.loads((base/'geometry_report.json').read_text(encoding='utf-8'))
    kits = {}
    for item in report: kits.setdefault(item['kit'], []).append(item)
    for kit, items in kits.items():
        cols = min(4, len(items))
        sheet = Image.new('RGB', (256*cols, 280*((len(items) + cols - 1)//cols)), (236, 231, 222))
        draw = ImageDraw.Draw(sheet)
        for i, item in enumerate(items):
            x, y = (i % cols)*256, (i//cols)*280
            with Image.open(folder/(item['name'] + '.png')) as image:
                sheet.paste(image.convert('RGB').resize((256, 256)), (x, y))
            draw.text((x + 10, y + 261), f"{item['name']}  {item['triangles']} tris", fill=(45, 43, 42))
        sheet.save(folder/f'contact_sheet_{kit}.png')
        print('SHEET_OK', kit, len(items))

if __name__ == '__main__': main()
