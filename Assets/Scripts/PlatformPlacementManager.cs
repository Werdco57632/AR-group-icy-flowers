
using UnityEngine;
using UnityEngine.EventSystems;

public class PlatformPlacementManager : MonoBehaviour
{
    [Header("Hand Gesture")]
    public bool isOpenHand = false;
    private bool wasOpenHand = false;

    [Header("Platform Settings")]
    public GameObject platformPrefab;
    public int platformCount = 3;

    [Header("Dragging")]
    public Camera gameCamera;

    [Header("Rotation")]
    public float rotationAmount = 15f;
    public float rotationSpeed = 90f;

    private bool rotatingLeft = false;
    private bool rotatingRight = false;

    [Header("Rotation UI")]
    public RectTransform rotationUI;
    public Vector3 rotationUIOffset = Vector3.zero;

    [Header("Placement Buttons")]
    public GameObject confirmButton;
    public GameObject cancelButton;

    private GameObject previewPlatform;
    private bool isPlacingPlatform = false;
    private bool isDragging = false;

    private Vector3 dragOffset;
    private float previousTimeScale = 1f;

    void Start()
    {
        HidePlacementUI();
    }

    void Update()
    {
        // Detect when the hand first opens
        if (isOpenHand && !wasOpenHand &&
            platformCount > 0 && !isPlacingPlatform)
        {
            StartPlacement();
        }

        wasOpenHand = isOpenHand;

        if (isPlacingPlatform && previewPlatform != null)
        {
            HandleDragging();
            HandleRotation();
            UpdateRotationUI();
        }
    }

    void StartPlacement()
    {
        if (platformPrefab == null)
        {
            Debug.LogWarning("Platform prefab is missing!");
            return;
        }

        isPlacingPlatform = true;
        isDragging = false;
        rotatingLeft = false;
        rotatingRight = false;

        // Pause gameplay
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        // Spawn temporary platform
        previewPlatform = Instantiate(
            platformPrefab,
            new Vector3(0f, 0f, 0f),
            Quaternion.identity
        );

        // Disable 3D collider during placement
        Collider platformCollider =
            previewPlatform.GetComponent<Collider>();

        if (platformCollider != null)
            platformCollider.enabled = false;

        // Disable 2D collider during placement
        Collider2D platformCollider2D =
            previewPlatform.GetComponent<Collider2D>();

        if (platformCollider2D != null)
            platformCollider2D.enabled = false;

        ShowPlacementUI();

        Debug.Log("Platform placement started!");
    }

    void HandleDragging()
    {
        if (gameCamera == null)
            return;

        // Android touch controls
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (!IsTouchOverUI(touch.fingerId))
                    BeginDrag(touch.position);
            }

            if (touch.phase == TouchPhase.Moved)
                Drag(touch.position);

            if (touch.phase == TouchPhase.Ended ||
                touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }

        // Mouse controls for Unity testing
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!IsMouseOverUI())
                    BeginDrag(Input.mousePosition);
            }

            if (Input.GetMouseButton(0) && isDragging)
                Drag(Input.mousePosition);

            if (Input.GetMouseButtonUp(0))
                isDragging = false;
        }
    }

    void BeginDrag(Vector2 screenPosition)
    {
        Vector3 worldPosition = ScreenToWorld(screenPosition);

        Vector2 platformPosition = new Vector2(
            previewPlatform.transform.position.x,
            previewPlatform.transform.position.y
        );

        Vector2 pointerPosition = new Vector2(
            worldPosition.x,
            worldPosition.y
        );

        // Only drag when touching near the platform
        if (Vector2.Distance(
            platformPosition, pointerPosition) < 2f)
        {
            isDragging = true;

            dragOffset =
                previewPlatform.transform.position -
                worldPosition;
        }
    }

    void Drag(Vector2 screenPosition)
    {
        if (!isDragging)
            return;

        Vector3 worldPosition = ScreenToWorld(screenPosition);

        Vector3 newPosition = worldPosition + dragOffset;

        // Keep the platform at its original depth
        newPosition.z = previewPlatform.transform.position.z;

        previewPlatform.transform.position = newPosition;
    }

    Vector3 ScreenToWorld(Vector2 screenPosition)
    {
        float distance = Mathf.Abs(
            previewPlatform.transform.position.z -
            gameCamera.transform.position.z
        );

        Vector3 screenPoint = new Vector3(
            screenPosition.x,
            screenPosition.y,
            distance
        );

        Vector3 worldPosition =
            gameCamera.ScreenToWorldPoint(screenPoint);

        worldPosition.z = previewPlatform.transform.position.z;

        return worldPosition;
    }

    bool IsMouseOverUI()
    {
        return EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject();
    }

    bool IsTouchOverUI(int fingerId)
    {
        return EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject(
                fingerId);
    }

    // Make rotation arrows follow the platform
    void UpdateRotationUI()
    {
        if (rotationUI == null || gameCamera == null)
            return;

        Vector3 screenPosition =
            gameCamera.WorldToScreenPoint(
                previewPlatform.transform.position
            );

        rotationUI.position =
            screenPosition + rotationUIOffset;
    }

    // Single-click rotation
    public void RotateLeft()
    {
        if (!isPlacingPlatform || previewPlatform == null)
            return;

        previewPlatform.transform.Rotate(
            0f, 0f, rotationAmount
        );
    }

    public void RotateRight()
    {
        if (!isPlacingPlatform || previewPlatform == null)
            return;

        previewPlatform.transform.Rotate(
            0f, 0f, -rotationAmount
        );
    }

    // Press-and-hold rotation
    public void StartRotateLeft()
    {
        if (!isPlacingPlatform || previewPlatform == null)
            return;

        rotatingLeft = true;
        rotatingRight = false;
    }

    public void StopRotateLeft()
    {
        rotatingLeft = false;
    }

    public void StartRotateRight()
    {
        if (!isPlacingPlatform || previewPlatform == null)
            return;

        rotatingRight = true;
        rotatingLeft = false;
    }

    public void StopRotateRight()
    {
        rotatingRight = false;
    }

    void HandleRotation()
    {
        if (!isPlacingPlatform || previewPlatform == null)
            return;

        if (rotatingLeft)
        {
            previewPlatform.transform.Rotate(
                0f, 0f,
                rotationSpeed * Time.unscaledDeltaTime
            );
        }

        if (rotatingRight)
        {
            previewPlatform.transform.Rotate(
                0f, 0f,
                -rotationSpeed * Time.unscaledDeltaTime
            );
        }
    }

    // Show all placement controls
    void ShowPlacementUI()
    {
        if (rotationUI != null)
            rotationUI.gameObject.SetActive(true);

        if (confirmButton != null)
            confirmButton.SetActive(true);

        if (cancelButton != null)
            cancelButton.SetActive(true);
    }

    // Hide all placement controls
    void HidePlacementUI()
    {
        if (rotationUI != null)
            rotationUI.gameObject.SetActive(false);

        if (confirmButton != null)
            confirmButton.SetActive(false);

        if (cancelButton != null)
            cancelButton.SetActive(false);
    }

    public void ConfirmPlacement()
    {
        if (!isPlacingPlatform || previewPlatform == null)
            return;

        // Enable 3D collision
        Collider platformCollider =
            previewPlatform.GetComponent<Collider>();

        if (platformCollider != null)
            platformCollider.enabled = true;

        // Enable 2D collision
        Collider2D platformCollider2D =
            previewPlatform.GetComponent<Collider2D>();

        if (platformCollider2D != null)
            platformCollider2D.enabled = true;

        // NEW: Enable double-tap dropping
        PlatformDrop platformDrop =
            previewPlatform.GetComponent<PlatformDrop>();

        if (platformDrop != null)
        {
            platformDrop.EnableDropping(gameCamera);
        }

        previewPlatform = null;
        platformCount--;

        isPlacingPlatform = false;
        isDragging = false;
        rotatingLeft = false;
        rotatingRight = false;

        HidePlacementUI();

        // Resume gameplay
        Time.timeScale = previousTimeScale;

        Debug.Log("Platform placed!");
    }

    public void CancelPlacement()
    {
        if (!isPlacingPlatform)
            return;

        Destroy(previewPlatform);
        previewPlatform = null;

        isPlacingPlatform = false;
        isDragging = false;
        rotatingLeft = false;
        rotatingRight = false;

        HidePlacementUI();

        // Resume gameplay without spending a platform
        Time.timeScale = previousTimeScale;

        Debug.Log("Placement cancelled!");
    }

    void OnDisable()
    {
        rotatingLeft = false;
        rotatingRight = false;

        if (isPlacingPlatform)
        {
            Time.timeScale = previousTimeScale;
        }
    }
}
