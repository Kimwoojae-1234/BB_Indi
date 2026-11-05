from PIL import Image, ImageDraw, ImageFont
from pathlib import Path
import uuid,re
root=Path(__file__).resolve().parents[1]
out=root/'Assets/Resources/Localization/QuickSim'
meta=(root/'Assets/Resources/Prototype/LeagueRankUp.png.meta').read_text(encoding='utf-8-sig')
for lang in ('en','ko','ja'):
 folder=out/lang;folder.mkdir(parents=True,exist_ok=True)
 fontpath=root/('Assets/Resources/Font/FredokaOne-Regular.ttf' if lang=='en' else 'Assets/Resources/Font/Localized/NotoSansCJKkr-Medium.otf')
 words={'starter': {'en':'SP','ko':'선발','ja':'先発'}[lang], 'relief':{'en':'RP','ko':'중계','ja':'中継ぎ'}[lang], 'closer':{'en':'CP','ko':'마무리','ja':'抑え'}[lang]}
 for i in range(1,10):words['order_'+str(i)]=f'No.{i}' if lang=='en' else f'{i}번' if lang=='ko' else f'{i}番'
 for i in range(1,13):
  for top in (True,False):words[f'inning_{i}_{"top" if top else "bottom"}']=(f'{"TOP" if top else "BOT"} {i}' if lang=='en' else f'{i}회{"초" if top else "말"}' if lang=='ko' else f'{i}回{"表" if top else "裏"}')
 for key,txt in words.items():
  image=Image.new('RGBA',(320,100));d=ImageDraw.Draw(image)
  size=72
  while True:
   font=ImageFont.truetype(str(fontpath),size);box=d.textbbox((0,0),txt,font=font,stroke_width=3)
   if box[2]-box[0]<=304 and box[3]-box[1]<=88:break
   size-=1
  x=(320-(box[2]-box[0]))/2-box[0];y=(100-(box[3]-box[1]))/2-box[1]
  d.text((x,y+2),txt,font=font,fill='white',stroke_width=3,stroke_fill=(23,52,82,255))
  if lang!='en':d.text((x,y+2),txt,font=font,fill='white',stroke_width=1,stroke_fill='white')
  p=folder/(key+'.png');image.save(p)
  mp=Path(str(p)+'.meta')
  if not mp.exists():mp.write_text(re.sub(r'guid: [a-f0-9]+','guid: '+uuid.uuid4().hex,meta,count=1),encoding='utf-8')
print('Generated',len(list(out.rglob('*.png'))),'localized NGUI label images')
preview=Image.new('RGB',(640,300),(23,75,103))
for row,lang in enumerate(('en','ko','ja')):
 for col,key in enumerate(('order_2','inning_1_top')):
  im=Image.open(out/lang/(key+'.png'));preview.paste(im,(col*320,row*100),im)
preview.save(root/'Temp/quicksim-label-preview.png')
