using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class DirectionalMovement : MonoBehaviour
{
    private string groundTag = "Ground";
    private string playerTag = "Player";
    private bool isRunning = false;
    private Coroutine moveRoutine;
    private Vector3 targetPos;
    private Vector3 tempMovePos;
    private float currentConeOffset;
    public Vector3 tempMousePos { get; private set; }

    [Header("Cone")]
    [SerializeField] private float coneAngle;
    [Space]

    [Header("Player Rotation")]
    [SerializeField] private float withinRadius;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private Transform upperBodyTrans;
    [SerializeField] private Transform lowerBodyTrans;
    [SerializeField] private Transform orientation;
    [SerializeField][Range(0, 120)] private float maxTurnDegree;
    [Space]

    [Header("Player Displacement")]
    [SerializeField] private KeyCode haltMovement;
    [SerializeField] private float moveSpeed;
    [SerializeField] private Rigidbody playerRB;
    [Space]
    [Header("Camera")]
    [SerializeField] private Camera cameraObject;

    private Vector3 gizmoTarget;

    void Start()
    {
        playerRB = GetComponent<Rigidbody>();
    }

    void Update()
    {
        tempMousePos = Input.mousePosition;
        NavigateTowardsMouse();

        if (Input.GetKeyDown(haltMovement))
        {
            StopCoroutine(moveRoutine);
            isRunning = false;
        }
    }

    private void NavigateTowardsMouse()
    {
        Ray cameraRay = cameraObject.ScreenPointToRay(tempMousePos);

        if (Physics.Raycast(cameraRay, out RaycastHit hitInfo, 400f))
        {
            targetPos = hitInfo.point;

            if (isRunning)
            {
                RotateCharacter(tempMovePos);
            }
            else if (!hitInfo.collider.CompareTag(playerTag)) 
            {
                RotateCharacter(targetPos);
            }
            
            Debug.DrawLine(transform.position, targetPos, Color.red);

            if (Input.GetMouseButtonDown(0) && hitInfo.collider.CompareTag(groundTag))
            {
                gizmoTarget = tempMovePos = targetPos;

                if (moveRoutine != null)
                {
                    StopCoroutine(moveRoutine);
                }
                moveRoutine = StartCoroutine(MoveToTarget(targetPos, moveSpeed, withinRadius));
            }
        }
    }

    private Quaternion RotateCharacter(Vector3 rotation)
    {
        Quaternion characterRot = Quaternion.LookRotation(Vector3.ProjectOnPlane(rotation - transform.position, transform.up));
        orientation.rotation = Quaternion.Slerp(orientation.rotation, characterRot, rotationSpeed * Time.deltaTime);

        if (isRunning)
        {
            upperBodyTrans.rotation = Quaternion.Slerp(upperBodyTrans.rotation, orientation.rotation, rotationSpeed * Time.deltaTime);
            lowerBodyTrans.rotation = Quaternion.Slerp(lowerBodyTrans.rotation, orientation.rotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            upperBodyTrans.rotation = Quaternion.Slerp(upperBodyTrans.rotation, ClampedRotation(orientation.rotation, lowerBodyTrans.rotation, maxTurnDegree), rotationSpeed * Time.deltaTime);
            lowerBodyTrans.rotation = Quaternion.Slerp(lowerBodyTrans.rotation, GetConeRot(), rotationSpeed * Time.deltaTime);
        }

        Vector3 coneLeft = Quaternion.Euler(0, -coneAngle / 2 + currentConeOffset, 0) * (Vector3.forward);
        Vector3 coneRight = Quaternion.Euler(0, coneAngle / 2 + currentConeOffset, 0) * (Vector3.forward);

        Debug.DrawLine(transform.position, coneLeft + transform.position, Color.green);
        Debug.DrawLine(transform.position, coneRight + transform.position, Color.green);

        Vector3 projectedVector = orientation.forward;

        Debug.DrawLine(transform.position, projectedVector + transform.position, Color.blue);

        float cone = ConeProximity(coneLeft, coneRight, projectedVector);

        if (cone != 0)
        {
            currentConeOffset += cone * coneAngle;
        }

        return characterRot;
    }

    private Quaternion ClampedRotation(Quaternion rotFrom, Quaternion rotTo, float degrees)
    {
        //LateUpdate lateUpdate = new();

        #region Dump
        //float angle = Quaternion.Angle(rotFrom, rotTo);
        //float angleDiff = Mathf.Max(angle - degrees, 0);
        //float dot = Vector3.Dot(rotFrom * Vector3.forward, Quaternion.AngleAxis(90, Vector3.up) * rotTo * Vector3.forward);
        //return Quaternion.AngleAxis(angleDiff * dot, Vector3.up) * rotFrom;
        #endregion

        float toY = rotTo.eulerAngles.y;
        return Quaternion.Euler(rotFrom.eulerAngles.x, Mathf.Clamp(rotFrom.eulerAngles.y, toY - degrees / 2, toY + degrees / 2), rotFrom.eulerAngles.z);
    }

    private Quaternion GetConeRot()
    {
        return Quaternion.Euler(0, currentConeOffset, 0);
    }

    private float ConeProximity(Vector3 left, Vector3 right, Vector3 dir)
    {
        float fromLeft = Vector3.Angle(left, dir);
        float fromRight = Vector3.Angle(right, dir);

        if (fromLeft > coneAngle || fromRight > coneAngle)
        {
            if(fromLeft > fromRight)
            {
                return 1;
            }
            else if(fromRight > fromLeft)
            {
                return -1;
            }

        }
        return 0;
    }

    private IEnumerator MoveToTarget(Vector3 targetPos, float playerSpeed, float withinRadius)
    {
        WaitForFixedUpdate wait = new();

        while (Vector3.Distance(transform.position, targetPos) > withinRadius)
        {
            isRunning = true;
            Vector3 movePlayer = Vector3.ProjectOnPlane(targetPos - transform.position, transform.up).normalized * playerSpeed;

            playerRB.velocity = new (movePlayer.x, playerRB.velocity.y, movePlayer.z);

            yield return wait;
        }
        isRunning = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(gizmoTarget, 0.2f);
    }
}