using System.Collections;
using System.Collections.Specialized;
using System.Security.Cryptography;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.UIElements;
using System.Threading;

public class DirectionalMovement : MonoBehaviour
{
    private string groundTag = "Ground";
    private string playerTag = "Player";
    private bool isRunning = false;
    private Coroutine moveRoutine;
    private Vector3 targetPos;
    private Vector3 tempMovePos;

    public Vector3 tempMousePos { get; private set; }

    [SerializeField] private float withinRadius;
    [SerializeField] private KeyCode haltMovement;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private Camera cameraObject;
    [SerializeField] private Rigidbody playerRB;
    [SerializeField] private Transform upperBodyTrans;
    [SerializeField] private float coneWidth;
    [SerializeField] private float coneLength;

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
        transform.rotation = Quaternion.Slerp(transform.rotation, characterRot, rotationSpeed * Time.deltaTime);

        Vector3 line1End = transform.position + Quaternion.Euler(0, -coneWidth, 0) * (Vector3.forward * coneLength);
        Vector3 line2End = transform.position + Quaternion.Euler(0, coneWidth, 0) * (Vector3.forward * coneLength);

        Debug.DrawLine(transform.position, line1End, Color.green);
        Debug.DrawLine(transform.position, line2End, Color.green);

        Vector3 triangleNormal = Vector3.Cross(line2End - transform.position, line1End - transform.position).normalized;
        Vector3 projectedVector = Vector3.ProjectOnPlane(rotation, triangleNormal);
        float dotProduct = Vector3.Dot(projectedVector - line2End, projectedVector - line1End);
        Debug.DrawLine(transform.position, projectedVector, Color.blue);
        Debug.DrawLine(rotation, projectedVector, Color.yellow);
        Debug.DrawLine(transform.position, triangleNormal, Color.yellow);

        if (dotProduct < 0)
        {
            Debug.Log("The cursor exits on the left side");
        }
        else
        {
            Debug.Log("The cursor exits on the right side");
        }

        //if (isRunning)
        //{
        //    transform.rotation = Quaternion.Slerp(transform.rotation, characterRot, rotationSpeed * Time.deltaTime);
        //    upperBodyTrans.rotation = Quaternion.Slerp(transform.rotation, characterRot, rotationSpeed * Time.deltaTime);
        //}
        //else
        //{
        //    upperBodyTrans.rotation = Quaternion.Slerp(transform.rotation, characterRot, rotationSpeed * Time.deltaTime);
        //}
        return characterRot;
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

    private void ProceduralRotation()
    {

    }
}