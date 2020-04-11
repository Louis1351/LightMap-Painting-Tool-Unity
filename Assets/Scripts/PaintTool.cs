using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


public class PaintTool : MonoBehaviour
{
    //object have a meshcollider
    const float MAXRESTEXTURE = 1024.0f;

    [SerializeField]
    float distanceMaxToPaint = 1000.0f;

    [SerializeField]
    GameObject preview;

    [SerializeField]
    [Range(0, 10)]
    float brushSize = 1.0f;

    [SerializeField]
    [Range(0, 10)]
    float eraserSize = 1.0f;

    [SerializeField]
    [Range(128, MAXRESTEXTURE)]
    uint mappingResolution = 128;

    [SerializeField]
    Color brushColor;
    Color eraserColor;

    [SerializeField]
    Color brushPreviewColor;
    [SerializeField]
    Color eraserPreviewColor;

    [SerializeField]
    Texture2D brushTexture;

    [SerializeField]
    Texture2D eraserTexture;

    Dictionary<int, Paint> paintObjects;
    bool activeBrush;

    //dynamic size
    Texture2D tempBrush;

    float ratio;
    float oldSize;
    int halfWidth;
    int halfHeight;

    Color[] initColors;
    RaycastHit h;

    GameObject currentPreview;
    void ResetPainting()
    {
        paintObjects = new Dictionary<int, Paint>();
        brushColor = Color.red;
        eraserColor = Color.black;
        initColors = new Color[mappingResolution * mappingResolution];
        ratio = (mappingResolution / MAXRESTEXTURE) * 0.1f;
        for (int i = 0; i < initColors.Length; i++)
        {
            initColors[i] = Color.black;
        }
    }

    void Start()
    {
        activeBrush = true;

        currentPreview = Instantiate(preview);
        currentPreview.GetComponent<MeshRenderer>().material.SetColor("_TintColor", brushPreviewColor);
        currentPreview.transform.localScale = Vector3.one * (brushSize + 0.3f * brushSize);

        ResetPainting();
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.B))
        {
            activeBrush = true;
            currentPreview.GetComponent<MeshRenderer>().material.SetColor("_TintColor", brushPreviewColor);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            activeBrush = false;
            currentPreview.GetComponent<MeshRenderer>().material.SetColor("_TintColor", eraserPreviewColor);
        }

        if (activeBrush)
            currentPreview.transform.localScale = Vector3.one * (brushSize + 0.3f * brushSize);
        else
            currentPreview.transform.localScale = Vector3.one * (eraserSize + 0.3f * eraserSize);
    }

    public void LateUpdate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        ActivePreview(ray);

        if (Input.GetMouseButton(0) && currentPreview.activeSelf)
        {
            DrawOnTexture(h);
        }
    }

    void ActivePreview(Ray _Ray)
    {
        if (Physics.Raycast(_Ray, out h))
        {
            currentPreview.SetActive(true);
            currentPreview.transform.position = h.point;
        }
        else currentPreview.SetActive(false);
    }

    void DrawOnTexture(RaycastHit _hit)
    {
        int ID = 0;
        Renderer currentRend;

        ID = _hit.transform.gameObject.GetInstanceID();
        currentRend = _hit.transform.GetComponent<Renderer>();

        //Create a new Paint Texture if it doesn't exist
        if (!paintObjects.ContainsKey(ID))
        {
            Paint paint = new Paint((int)mappingResolution, (int)mappingResolution, initColors);
            paintObjects.Add(ID, paint);
        }

        if (currentRend && paintObjects[ID] != null)
        {
            float size = activeBrush ? brushSize : eraserSize;

            if (oldSize != size)
            {
                oldSize = size;

                size *= ratio;
                //Set Temporary Texture to rescale
                TextureScale.CopyTexture(activeBrush ? brushTexture : eraserTexture, ref tempBrush);
                TextureScale.Bilinear(tempBrush, (int)(brushTexture.width * size), (int)(brushTexture.height * size));

                halfWidth = (int)((brushTexture.width * 0.5f) * size);
                halfHeight = (int)((brushTexture.height * 0.5f) * size);
            }

            // local position on the lightmap texture
            int XLightMap = (int)((_hit.lightmapCoord.x) * paintObjects[ID].TmpPaint.width) - halfWidth;
            int YLightMap = (int)((_hit.lightmapCoord.y) * paintObjects[ID].TmpPaint.height) - halfHeight;

            for (int i = 0; i < tempBrush.width; i++)
            {
                for (int j = 0; j < tempBrush.height; j++)
                {
                    float alpha = tempBrush.GetPixel(i, j).a;
                    if (alpha > 0.0f)
                    {
                        int XPixel = i + XLightMap;
                        int YPixel = j + YLightMap;

                        //out of bounds texture
                        if (XPixel >= 0 && XPixel < paintObjects[ID].TmpPaint.width - 1 &&
                            YPixel >= 0 && YPixel < paintObjects[ID].TmpPaint.height - 1)
                        {
                            //paint on the mask(white or black)
                            brushColor.a = alpha;
                            eraserColor.a = alpha;
                            paintObjects[ID].TmpPaint.SetPixel(XPixel, YPixel, activeBrush ? brushColor : eraserColor);
                        }
                    }
                }
            }
            //Apply to the corresponding material
            MaterialPropertyBlock mbp = new MaterialPropertyBlock();
            paintObjects[ID].TmpPaint.Apply();

            mbp.SetTexture("_PaintTex", paintObjects[ID].TmpPaint);
            currentRend.SetPropertyBlock(mbp);
        }
    }
}
