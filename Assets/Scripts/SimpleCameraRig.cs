using UnityEngine;

public class SimpleCameraRig : MonoBehaviour
{
    [Header("Camera Distance Tuning")]
    public Transform gridRoot;           // Assign your HexGrid here
    public float distanceMultiplier = 1.5f; // Scales with grid size
    public float heightFactor = 0.6f;        // Controls vertical angle
    public KeyCode resetKey = KeyCode.R;

    public float zoomSpeed = 10f;
    public float panSpeed = 0.5f;
    public float rotationSpeed = 5f;

    private Vector3 defaultPosition;
    private Quaternion defaultRotation;

    void Start()
    {
        Vector3 offset = new Vector3(0, 15f, -15f); // fallback

        if (gridRoot != null)
        {
            Bounds bounds = CalculateBounds(gridRoot);
            float size = Mathf.Max(bounds.size.x, bounds.size.z);
            float dist = size * distanceMultiplier;

            offset = new Vector3(0, dist * heightFactor, -dist);
        }

        transform.position = offset;
        transform.LookAt(Vector3.zero);

        defaultPosition = transform.position;
        defaultRotation = transform.rotation;
    }

    void Update()
    {
        if (Input.GetKeyDown(resetKey))
        {
            transform.position = defaultPosition;
            transform.rotation = defaultRotation;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            transform.position += transform.forward * scroll * zoomSpeed;
        }

        if (Input.GetMouseButton(2))
        {
            float h = -Input.GetAxis("Mouse X") * panSpeed;
            float v = -Input.GetAxis("Mouse Y") * panSpeed;
            transform.Translate(h, v, 0);
        }

        if (Input.GetMouseButton(1))
        {
            float h = Input.GetAxis("Mouse X") * rotationSpeed;
            float v = -Input.GetAxis("Mouse Y") * rotationSpeed;
            transform.RotateAround(transform.position, Vector3.up, h);
            transform.RotateAround(transform.position, transform.right, v);
        }
    }

    Bounds CalculateBounds(Transform root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return new Bounds(root.position, Vector3.one);

        Bounds bounds = renderers[0].bounds;
        foreach (var r in renderers)
            bounds.Encapsulate(r.bounds);
        return bounds;
    }
}
