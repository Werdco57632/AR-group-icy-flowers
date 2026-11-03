
using UnityEngine;
using Vuforia;

public class VuforiaCameraBridge : MonoBehaviour
{
    private const PixelFormat Format = PixelFormat.RGB888;

    public Texture2D CameraTexture { get; private set; }

    private bool registered;

    void Start()
    {
        VuforiaApplication.Instance.OnVuforiaStarted += OnVuforiaStarted;
        VuforiaBehaviour.Instance.World.OnStateUpdated += OnVuforiaUpdated;

        if (VuforiaApplication.Instance.IsRunning)
            OnVuforiaStarted();
    }

    void OnVuforiaStarted()
    {
        if (registered) return;

        registered = VuforiaBehaviour.Instance.CameraDevice
            .SetFrameFormat(Format, true);

        Debug.Log("Vuforia camera format registered: " + registered);
    }

    void OnVuforiaUpdated()
    {
        if (!registered) return;

        var image = VuforiaBehaviour.Instance.CameraDevice
            .GetCameraImage(Format);

        if (image == null || image.Width <= 0 || image.Height <= 0)
            return;

        if (CameraTexture == null)
        {
            CameraTexture = new Texture2D(
                image.Width, image.Height,
                TextureFormat.RGB24, false);
        }

        image.CopyToTexture(CameraTexture, true);
    }

    void OnDestroy()
    {
        VuforiaApplication.Instance.OnVuforiaStarted -= OnVuforiaStarted;

        if (VuforiaBehaviour.Instance != null)
        {
            VuforiaBehaviour.Instance.World.OnStateUpdated
                -= OnVuforiaUpdated;
        }

        if (registered && VuforiaApplication.Instance.IsRunning)
        {
            VuforiaBehaviour.Instance.CameraDevice
                .SetFrameFormat(Format, false);
        }

        if (CameraTexture != null)
            Destroy(CameraTexture);
    }
}
