using UnityEngine;

public class SimpleSpriteColorChanger : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Texture2D originalTexture;
    private Texture2D modifiedTexture;
    
    [Header("Color Settings")]
    public Color targetColor = Color.red;
    public Color newColor = Color.green;
    [Range(0f, 1f)]
    public float tolerance = 0.1f;
    
    [Header("Presets")]
    public ColorPreset[] colorPresets;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            SetupTextures();
        }
    }
    
    void SetupTextures()
    {
        Sprite originalSprite = spriteRenderer.sprite;
        originalTexture = originalSprite.texture;
        
        Texture2D readableTexture = MakeTextureReadable(originalTexture);
        
        modifiedTexture = new Texture2D(readableTexture.width, readableTexture.height, TextureFormat.RGBA32, false);
        modifiedTexture.filterMode = FilterMode.Point;
        
        CopyOriginalTexture(readableTexture);
        ApplyColorChange();
        
        DestroyImmediate(readableTexture);
    }
    
    Texture2D MakeTextureReadable(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        Graphics.Blit(source, renderTex);
        
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        
        Texture2D readableTexture = new Texture2D(source.width, source.height);
        readableTexture.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableTexture.Apply();
        
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        
        return readableTexture;
    }
    
    void CopyOriginalTexture(Texture2D sourceTexture)
    {
        Color[] pixels = sourceTexture.GetPixels();
        modifiedTexture.SetPixels(pixels);
    }
    
    void ApplyColorChange()
    {
        Color[] pixels = modifiedTexture.GetPixels();
        
        for (int i = 0; i < pixels.Length; i++)
        {
            if (ColorsMatch(pixels[i], targetColor))
            {
                pixels[i] = newColor;
            }
        }
        
        modifiedTexture.SetPixels(pixels);
        modifiedTexture.Apply();
        
        Rect spriteRect = spriteRenderer.sprite.rect;
        Vector2 pivot = spriteRenderer.sprite.pivot;
        
        Sprite newSprite = Sprite.Create(modifiedTexture, spriteRect, pivot / spriteRect.size);
        spriteRenderer.sprite = newSprite;
    }
    
    bool ColorsMatch(Color color1, Color color2)
    {
        float distance = Vector3.Distance(
            new Vector3(color1.r, color1.g, color1.b),
            new Vector3(color2.r, color2.g, color2.b)
        );
        
        return distance <= tolerance;
    }
    
    public void ChangeColor(Color target, Color replacement)
    {
        targetColor = target;
        newColor = replacement;
        
        if (modifiedTexture != null)
        {
            Texture2D readableTexture = MakeTextureReadable(originalTexture);
            CopyOriginalTexture(readableTexture);
            ApplyColorChange();
            DestroyImmediate(readableTexture);
        }
    }
    
    public void ApplyPreset(int presetIndex)
    {
        if (presetIndex >= 0 && presetIndex < colorPresets.Length)
        {
            ColorPreset preset = colorPresets[presetIndex];
            ChangeColor(preset.targetColor, preset.newColor);
            tolerance = preset.tolerance;
        }
    }
    
    public void RandomizeColor()
    {
        Color randomColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
        ChangeColor(targetColor, randomColor);
    }
    
    void OnDestroy()
    {
        if (modifiedTexture != null)
        {
            DestroyImmediate(modifiedTexture);
        }
    }
}

[System.Serializable]
public class ColorPreset
{
    public string name;
    public Color targetColor;
    public Color newColor;
    [Range(0f, 1f)]
    public float tolerance = 0.1f;
}