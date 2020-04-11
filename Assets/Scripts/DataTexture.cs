using UnityEngine;

public class DataTexture 
{
    Texture2D texture;
    int width;
    int height;

    public Texture2D Texture { get => texture; set => texture = value; }
    public int Width { get => width; set => width = value; }
    public int Height { get => height; set => height = value; }
}
