using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraMovement : MonoBehaviour
{
    private float offsetVal;
    private Vector3 strongObjFollow;
    private Quaternion desiredRotation;

    private int tempIndex;
    private Vector3[] viewAngleRot = new Vector3[] {new Vector3 (25f, 45f,  0f),
                                                    new Vector3 (25f, 135f, 0f),
                                                    new Vector3 (25f, 225f, 0f),
                                                    new Vector3 (25f, 315f, 0f)};

    [SerializeField] private Transform transformTarget;
    [SerializeField] private float smoothVal;
    [SerializeField] private KeyCode rotateLeftKey;
    [SerializeField] private KeyCode rotateRightKey;

    void Start()
    {
        strongObjFollow = transformTarget.position;
        offsetVal = Vector3.Distance(transform.position, transformTarget.position);
    }

    private void Update()
    {
        if (Input.GetKeyDown(rotateRightKey))
        {
            tempIndex--;
            if (tempIndex < 0)
            {
                tempIndex = viewAngleRot.Length - 1;
            }
            desiredRotation = Quaternion.Euler(viewAngleRot[tempIndex]);
        }
        if (Input.GetKeyDown(rotateLeftKey))
        {
            tempIndex++;
            if (tempIndex > viewAngleRot.Length - 1)
            {
                tempIndex = 0;
            }
            desiredRotation = Quaternion.Euler(viewAngleRot[tempIndex]);
        }
    }

    void FixedUpdate()
    {
        RotateCamera();

        strongObjFollow = Vector3.Lerp(strongObjFollow, transformTarget.position, smoothVal * Time.deltaTime);
        Vector3 cameraPos = strongObjFollow + transform.rotation * -Vector3.forward * offsetVal;

        transform.position = cameraPos;
    }

    void RotateCamera()
    {
        transform.rotation = Quaternion.Lerp (transform.rotation, desiredRotation, smoothVal * Time.deltaTime);
    }
}
