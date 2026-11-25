
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class PlatformDrop : MonoBehaviour
{
    [Header("Double Tap Settings")]
    public float doubleTapTime = 0.3f;
    public Camera gameCamera;

    [Header("Drop Settings")]
    public float minimumCrushHeight = 2f;
    public float platformWeight = 2f;

    private Rigidbody rb;
    private float lastTapTime = -10f;
    private float dropStartHeight;
    private float downwardSpeed;

    private bool isConfirmed = false;
    private bool hasDropped = false;

    private HashSet<EnemyHealth> damagedEnemies =
        new HashSet<EnemyHealth>();

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = true;

        rb.constraints =
            RigidbodyConstraints.FreezePositionZ |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY;

        if (gameCamera == null)
            gameCamera = Camera.main;
    }

    public void EnableDropping(Camera placementCamera)
    {
        if (placementCamera != null)
            gameCamera = placementCamera;

        isConfirmed = true;
        lastTapTime = -10f;
    }

    void Update()
    {
        if (!isConfirmed || hasDropped)
            return;

        if (Time.timeScale == 0f)
            return;

        if (gameCamera == null)
            return;

        // Android touchscreen input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (EventSystem.current != null &&
                    EventSystem.current.IsPointerOverGameObject(
                        touch.fingerId))
                    return;

                CheckTap(touch.position);
            }

            return;
        }

        // Mouse input for Unity Editor
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
                return;

            CheckTap(Input.mousePosition);
        }
    }

    void CheckTap(Vector2 screenPosition)
    {
        Ray ray = gameCamera.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            PlatformDrop selectedPlatform =
                hit.collider.GetComponentInParent<PlatformDrop>();

            if (selectedPlatform == this)
            {
                float currentTime = Time.unscaledTime;

                if (currentTime - lastTapTime <= doubleTapTime)
                {
                    DropPlatform();
                    return;
                }

                lastTapTime = currentTime;
            }
            else
            {
                lastTapTime = -10f;
            }
        }
        else
        {
            lastTapTime = -10f;
        }
    }

    void DropPlatform()
    {
        if (hasDropped)
            return;

        hasDropped = true;

        dropStartHeight = transform.position.y;

        rb.isKinematic = false;
        rb.useGravity = true;

        rb.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;

        rb.interpolation =
            RigidbodyInterpolation.Interpolate;

        Debug.Log("Platform dropped!");
    }

    void FixedUpdate()
    {
        if (!hasDropped)
            return;

        downwardSpeed = -rb.linearVelocity.y;

        // Update starting height if the platform
        // gets pushed upward again
        if (transform.position.y > dropStartHeight)
        {
            dropStartHeight = transform.position.y;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!hasDropped)
            return;

        EnemyHealth enemy =
            collision.collider.GetComponentInParent<EnemyHealth>();

        if (enemy == null)
            return;

        if (damagedEnemies.Contains(enemy))
            return;

        // Calculate how far the platform has fallen
        float fallDistance =
            dropStartHeight - transform.position.y;

        if (fallDistance < minimumCrushHeight)
        {
            Debug.Log("Not high enough to squish!");
            return;
        }

        // Make sure the platform was falling downward
        if (downwardSpeed <= 0.1f)
            return;

        // Make sure the enemy is underneath the platform
        bool enemyBelow = false;

        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                enemyBelow = true;
                break;
            }
        }

        if (!enemyBelow)
            return;

        // Prevent repeated damage from the same platform
        damagedEnemies.Add(enemy);

        // Deal exactly one point of damage
        enemy.Squish(rb, platformWeight);

        Debug.Log("PANCAKE! Fall distance: " + fallDistance);
    }
}
