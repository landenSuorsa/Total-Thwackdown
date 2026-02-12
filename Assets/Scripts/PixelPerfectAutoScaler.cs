using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(PixelPerfectCamera))]
public class PixelPerfectAutoScaler : MonoBehaviour
{
    [Header("Sprite/Grid Settings")]
    public int spriteSize = 48;   // your sprite size in pixels
    public int gridWidth = 16;    // how many sprites wide your game world is
    public int gridHeight = 12;   // how many sprites tall your game world is

    private PixelPerfectCamera ppc;

    void Awake()
    {
        ppc = GetComponent<PixelPerfectCamera>();

        // Base reference resolution
        int referenceWidth = spriteSize * gridWidth;
        int referenceHeight = spriteSize * gridHeight;

        ppc.assetsPPU = spriteSize;
        ppc.refResolutionX = referenceWidth;
        ppc.refResolutionY = referenceHeight;
        ppc.pixelSnapping = true;
        ppc.cropFrameX = true; // ensures letterboxing/pillarboxing
        ppc.cropFrameY = true;
        ppc.stretchFill = false;

        ApplyIntegerScaling(referenceWidth, referenceHeight);
    }

    void ApplyIntegerScaling(int refWidth, int refHeight)
    {
        // Screen resolution
        int screenWidth = Screen.width;
        int screenHeight = Screen.height;

        // Calculate maximum integer scale factor
        int scaleX = screenWidth / refWidth;
        int scaleY = screenHeight / refHeight;
        int scaleFactor = Mathf.Max(1, Mathf.Min(scaleX, scaleY));

        // Apply the scale factor
        ppc.refResolutionX = refWidth;
        ppc.refResolutionY = refHeight;
        ppc.cropFrameX = true; // ensures letterboxing/pillarboxing
        ppc.cropFrameY = true;

        // Optionally log for debugging
        Debug.Log($"PixelPerfect Scale Factor: {scaleFactor}");
    }
}
