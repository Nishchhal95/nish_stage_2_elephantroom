using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DraggableItem : MonoBehaviour
{
    [SerializeField] private float dragYLevel;
    [SerializeField] private Vector3 dragOffset;
    [SerializeField] private Collider myCollider;
    [SerializeField] private float boxPadding = 0.01f;
    private Plane dragPlane;
    private Camera cam;
    private LayerMask wallLayer;
    private Vector3 targetPosition;
    private bool isRotating;
    
    public void Init(Camera cam, LayerMask wallLayer)
    {
        this.cam = cam;
        this.wallLayer = wallLayer;
        myCollider = GetComponent<Collider>();
    }
    
    public void StartDrag(Vector3 hitPoint)
    {
        dragYLevel = hitPoint.y;
        dragPlane = new Plane(Vector3.up, new Vector3(0, dragYLevel, 0));
        dragOffset = transform.position - hitPoint;
    }
    
    public void Drag()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float point))
        {
            Vector3 worldMouse = ray.GetPoint(point);
            Vector3 targetPos = worldMouse + dragOffset;
            targetPosition = new Vector3(targetPos.x, transform.position.y, targetPos.z);

            transform.position = GetValidMovement(transform.position, targetPosition);;
        }
    }

    public void EndDrag()
    {
        dragYLevel = 0;
        dragPlane = new Plane();
        dragOffset = Vector3.zero;
    }

    private Vector3 GetValidMovement(Vector3 currentPos, Vector3 desiredPos)
    {
        BoxCollider box = (BoxCollider)myCollider;
        Vector3 halfExtents = Vector3.Scale(box.size * 0.5f, transform.lossyScale);
        Quaternion rotation = transform.rotation;

        Vector3 finalPos = currentPos;
        Vector3 totalMove = desiredPos - currentPos;

        // Check X movement
        Vector3 xMove = new Vector3(totalMove.x, 0f, 0f);
        if (!Physics.BoxCast(currentPos, halfExtents, xMove.normalized, out _, rotation, Mathf.Abs(xMove.x), wallLayer))
        {
            finalPos += xMove;
        }

        // Check Z movement
        Vector3 zMove = new Vector3(0f, 0f, totalMove.z);
        if (!Physics.BoxCast(finalPos, halfExtents, zMove.normalized, out _, rotation, Mathf.Abs(zMove.z), wallLayer))
        {
            finalPos += zMove;
        }

        return finalPos;
    }
    
    private void AlignObjectToWall()
    {
        BoxCollider box = myCollider as BoxCollider;
        if (box == null)
        {
            return;
        }

        Vector3 halfExtents = Vector3.Scale(box.size * 0.5f, transform.lossyScale);
        Vector3 contactNormal = Vector3.zero;

        Collider[] walls = Physics.OverlapBox(targetPosition, halfExtents, transform.rotation, wallLayer);
        if (walls.Length == 0)
        {
            Debug.Log("No walls detected near target position.");
            return;
        }

        Vector3 directionToWall = (walls[0].transform.position - transform.position).normalized;
        if (Physics.Raycast(transform.position, directionToWall, out RaycastHit hit, 10f, wallLayer))
        {
            contactNormal = hit.normal;
        }
        else
        {
            Debug.LogWarning("Raycast to wall failed. Cannot align.");
            return;
        }

        // Convert collision normal to object's local space
        Vector3 localNormal = transform.InverseTransformDirection(contactNormal);
        Debug.Log($"Contact Normal (world): {contactNormal}");
        Debug.Log($"Local Normal (object space): {localNormal}");

        // Which axis of the object hit the wall
        bool frontBackHit = Mathf.Abs(localNormal.z) > Mathf.Abs(localNormal.x);

        string hitSide = "";
        if (frontBackHit)
        {
            if (localNormal.z < 0)
                hitSide = "Front";
            else
                hitSide = "Back";
        }
        else
        {
            if (localNormal.x > 0)
                hitSide = "Right";
            else
                hitSide = "Left";
        }

        Debug.Log($"Sofa hit side: {hitSide}");

        // Calculate tangent direction along the wall surface which is perpendicular to normal and up
        Vector3 wallTangent = Vector3.Cross(Vector3.up, contactNormal).normalized;
        Debug.DrawRay(transform.position, wallTangent * 2f, Color.cyan, 2f);

        Quaternion targetRotation = Quaternion.LookRotation(wallTangent, Vector3.up);

        if (frontBackHit)
        {
            if (localNormal.z < 0)
            {
                targetRotation *= Quaternion.Euler(0, 0, 0);
                Debug.Log($"Target Rotation Euler Angles: {targetRotation.eulerAngles}");
            }
            else
            {
                targetRotation *= Quaternion.Euler(0, -180, 0);
                Debug.Log($"Target Rotation Euler Angles: {targetRotation.eulerAngles}");
            }
        }
        else
        {
            if (localNormal.x < 0)
            {
                targetRotation *= Quaternion.Euler(0, 0, 0);
                Debug.Log($"Target Rotation Euler Angles: {targetRotation.eulerAngles}");
            }
            else if (localNormal.x > 0)
            {
                targetRotation *= Quaternion.Euler(0, -180, 0);
                Debug.Log($"Target Rotation Euler Angles: {targetRotation.eulerAngles}");
            }
        }
        
        StartCoroutine(RotateRoutine(targetRotation));
    }

    private IEnumerator RotateRoutine(Quaternion targetRotation)
    {
        isRotating = true;
        Quaternion currentRotation = transform.rotation;
        float timeElapsed = 0;
        float timeDuration = 0.5f;

        while (timeElapsed < timeDuration)
        {
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, timeElapsed / timeDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.rotation = targetRotation;
        isRotating = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (myCollider == null)
        {
            return;
        }
        
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Vector3 halfExtents = Vector3.Scale(((BoxCollider)myCollider).size * 0.5f, transform.lossyScale);
        Gizmos.matrix = transform.localToWorldMatrix;
        Vector3 localTargetPos = transform.InverseTransformPoint(targetPosition);
        Gizmos.DrawWireCube(localTargetPos, halfExtents * 2);
        Gizmos.matrix = oldMatrix;
    }
}
