
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ScissorsController : MonoBehaviour
{
    public Camera gameCamera;
    public Transform cuttingPlane;
    public bool scissorsActive = true;
    public float lineWidth = 0.03f;

    private LineRenderer line;
    private Vector3 startPoint;
    private Vector3 endPoint;
    private bool isCutting;

    void Start()
    {
        line = GetComponent<LineRenderer>();

        if (gameCamera == null)
            gameCamera = Camera.main;

        line.useWorldSpace = true;
        line.positionCount = 0;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.startColor = Color.red;
        line.endColor = Color.red;
    }

    void Update()
    {
        if (!scissorsActive)
        {
            ClearCut();
            return;
        }

        // Android touchscreen input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    StartCut(touch.position);
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    UpdateCut(touch.position);
                    break;

                case TouchPhase.Ended:
                    EndCut(touch.position);
                    break;

                case TouchPhase.Canceled:
                    ClearCut();
                    break;
            }

            return;
        }

        // Mouse input for Unity Editor
        if (Input.GetMouseButtonDown(0))
            StartCut(Input.mousePosition);

        if (Input.GetMouseButton(0))
            UpdateCut(Input.mousePosition);

        if (Input.GetMouseButtonUp(0))
            EndCut(Input.mousePosition);
    }

    bool GetWorldPoint(Vector2 screenPosition, out Vector3 point)
    {
        Vector3 normal = cuttingPlane != null
            ? cuttingPlane.forward : Vector3.forward;

        Vector3 origin = cuttingPlane != null
            ? cuttingPlane.position : Vector3.zero;

        Plane plane = new Plane(normal, origin);
        Ray ray = gameCamera.ScreenPointToRay(screenPosition);

        if (plane.Raycast(ray, out float distance))
        {
            point = ray.GetPoint(distance);
            return true;
        }

        point = Vector3.zero;
        return false;
    }

    void StartCut(Vector2 screenPosition)
    {
        if (!GetWorldPoint(screenPosition, out startPoint))
            return;

        isCutting = true;
        endPoint = startPoint;

        line.positionCount = 2;
        line.SetPosition(0, startPoint);
        line.SetPosition(1, endPoint);
    }

    void UpdateCut(Vector2 screenPosition)
    {
        if (!isCutting)
            return;

        if (GetWorldPoint(screenPosition, out Vector3 point))
        {
            endPoint = point;
            line.SetPosition(1, endPoint);
        }
    }
    
    void DetectBreakableObjects()
    {
        RaycastHit2D[] hits = Physics2D.LinecastAll(startPoint, endPoint);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
            {
                continue;
            }
            
            if (hit.collider.CompareTag("breakable"))
            {
                Collider2D col = hit.collider;
                //Requires the swipe to begin and end
                //outside the obstacle.
                if (col.OverlapPoint(startPoint) || col.OverlapPoint(endPoint))
                {
                    continue;    
                }
                
                SpriteSlicer.Slice(col.gameObject, startPoint, endPoint);
            }
        }
    }


  

    void EndCut(Vector2 screenPosition)
    {
        if (!isCutting)
        {
            return;
            
        }
        UpdateCut(screenPosition);
        DetectBreakableObjects();
        ClearCut();
    }

    void ClearCut()
    {
        isCutting = false;
        line.positionCount = 0;
    }
}
