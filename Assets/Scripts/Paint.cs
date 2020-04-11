using UnityEngine;

public class Paint 
{
    Texture2D tmpPaint;
    public int objectId;

    public Texture2D TmpPaint
    {
        get
        {
            return tmpPaint;
        }

        set
        {
            tmpPaint = value;
        }
    }

    public Paint(int _width, int _height, Color[] _colors)
    {
        TmpPaint = new Texture2D(_width, _height, TextureFormat.ARGB32, false);
        TmpPaint.wrapMode = TextureWrapMode.Clamp;
        tmpPaint.filterMode = FilterMode.Bilinear;
        TmpPaint.SetPixels(_colors);
        TmpPaint.Apply();
    }
}
