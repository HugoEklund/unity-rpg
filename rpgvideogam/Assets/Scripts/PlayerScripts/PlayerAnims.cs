using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnims : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Rigidbody playerRB;

    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        playerRB = GetComponent<Rigidbody>();

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerAnimator.SetBool("isInBrush", true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerAnimator.SetBool("isInBrush", false);
        }
    }

    private void FixedUpdate()
    {
        playerAnimator.SetFloat("playerSpeed", Mathf.Abs(playerRB.velocity.sqrMagnitude));
    }
}