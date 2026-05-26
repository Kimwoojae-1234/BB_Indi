using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsColorData
{
	public int Idx { get; set; }
	public int UiType { get; set; }
	public int StatsMin { get; set; }
	public int StatsMax { get; set; }
	public string ColorCode01 { get; set; }
	public string ColorCode02 { get; set; }
}

public class StatsColorDataRecord : BaseDataRecord
{
    public StatsColorData[] StatsColorData;

    public override bool Initialize()
    {
        return base.Initialize();
    }

    public override bool Uninitialize()
    {
        return base.Uninitialize();
    }
}

public class StatsColors
{
    public enum Types
    {
        Running = 1,
        Batter = 2,
        Pitcher = 3,
        Throwing = 4,
        OutGameSlider = 5,
    }
    public class ColorData
    {
        public int Min
        {
            get;
            private set;
        }
        public int Max
        {
            get;
            private set;
        }
        public string SpriteName
        {
            get;
            private set;
        }
        public Color MinColor
        {
            get;
            private set;
        }
        public Color MaxColor
        {
            get;
            private set;
        }
        public ColorData(int min, int max, string color1, string color2)
        {
            Min = min;
            Max = max;
            //MinColor = GameConfig.GetColor(color1);
            //MaxColor = GameConfig.GetColor(color2);
        }
        public ColorData(int min, int max, string spriteName)
        {
            Min = min;
            Max = max;
            SpriteName = spriteName;
        }
        public Color GetColor(int value)
        {
            //float t = (float)(value - Min) / (float)(Max - Min);
            //return Color.Lerp(MinColor, MaxColor, t);
            return MaxColor;
        }
        public Sprite GetSprite()
        {
            return Resources.Load<Sprite>(string.Format("MainGame/texture/{0}", SpriteName));
        }
    }
    public Color Color
    {
        get;
        private set;
    }
    public Sprite Sprite
    {
        get;
        private set;
    }
    Types type;
    ColorData colorData;
    List<ColorData> colorDatas = new List<ColorData>();

    public StatsColors(Types type)
    {
        /*StatsColorData[] datas = MainManager.Database.LoadStatsColorDatas(type);
        if (datas != null)
        {
            for (int i = 0; i < datas.Length; i++)
            {
                if (type == Types.Pitcher || type == Types.OutGameSlider)
                {
                    colorDatas.Add(new ColorData(datas[i].StatsMin, datas[i].StatsMax, datas[i].ColorCode01));
                }
                else
                {
                    colorDatas.Add(new ColorData(datas[i].StatsMin, datas[i].StatsMax, datas[i].ColorCode01, datas[i].ColorCode02));
                }
            }
        }*/
        this.type = type;
    }
    public Color GetColor(float ratio)
    {
        if (colorData != null)
        {
            //return Color.Lerp(Color, colorData.MaxColor, ratio);
            return colorData.MaxColor;
        }
        return Color.white;
    }
    public void InitColor()
    {
        if (colorDatas.Count > 0)
        {
            colorData = colorDatas[0];
        }
        SetColorData(0);
    }
    public bool SetColor(int power)
    {
        bool changed = false;
        for (int i = 0; i < colorDatas.Count; i++)
        {
            if ((power >= colorDatas[i].Min) && (power <= colorDatas[i].Max))
            {
                changed = (colorData != null) && (colorData != colorDatas[i]);
                colorData = colorDatas[i];
                break;
            }
        }
        SetColorData(power);
        return changed;
    }
    void SetColorData(int power)
    {
        if (type == Types.Pitcher || type == Types.OutGameSlider)
        {
            Sprite = (colorData != null) ? colorData.GetSprite() : null;
        }
        else
        {
            Color = (colorData != null) ? colorData.GetColor(power) : Color.white;
        }
    }
}