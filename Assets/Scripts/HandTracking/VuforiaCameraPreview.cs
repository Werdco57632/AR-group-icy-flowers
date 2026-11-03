using UnityEngine;
using UnityEngine.UI;

public class VuforiaCameraPreview : MonoBehaviour
{
    public VuforiaCameraBridge cameraBridge;
    public RawImage preview;

    void Update()
    {
        if (cameraBridge == null || preview == null)
            return;

        if (cameraBridge.CameraTexture != null)
        {
            preview.texture = cameraBridge.CameraTexture;
        }
    }
}
