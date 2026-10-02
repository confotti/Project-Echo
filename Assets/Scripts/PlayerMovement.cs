using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public float speed;
    private Vector2 move;
    public Rigidbody rb;

    private bool dashing = false;
    private float dashSpeed = 20f;
    private float dashDecaySpeed = 64f;

    private bool attacking = false;
    public GameObject sword; 

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.started && !dashing)
        {
            dashing = true;

            rb.linearVelocity = transform.forward.normalized * dashSpeed;
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started && !dashing)
        {
            attacking = true;
        }
    }

    private void FixedUpdate()
    {
        movePlayer();
        dashPlayer();
    }

    public void movePlayer()
    {
        Vector3 movement = new Vector3(move.x, 0f, move.y);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement), 0.15f);

        transform.Translate(movement * speed * Time.deltaTime, Space.World);
    }

    public void dashPlayer()
    {
        if (dashing)
        {
            rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, Vector3.zero, dashDecaySpeed * Time.fixedDeltaTime);

            if (rb.linearVelocity.sqrMagnitude < 1f)
            {
                rb.linearVelocity = Vector3.zero;
                dashing = false;
            }
        }
    }
} 