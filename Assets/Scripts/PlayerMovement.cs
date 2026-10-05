using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public Camera mainCamera;
    public LayerMask groundLayer; 

    public float speed;
    private Vector2 move;
    public Rigidbody rb;
    public Animator animator;

    private bool dashing = false;
    private float dashSpeed = 40f;
    private float dashDecaySpeed = 64f;
    public ParticleSystem dashEffect;

    private bool attacking = false;
    private float attackMoveSpeed = 2f; 

    private void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (!enabled) return;

        if (context.started && !dashing && move.sqrMagnitude > 0.01f)
        {
            dashing = true;

            Vector3 dashDirection = new Vector3(move.x, 0f, move.y).normalized;

            rb.linearVelocity = dashDirection * dashSpeed;
            dashEffect.Play();
        }
    } 

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!enabled) return;

        if (context.started)
        {
            animator.SetTrigger("Attacking");
            rb.linearVelocity = transform.forward * attackMoveSpeed;
        }
    } 

    private void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()); 

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3 direction = hit.point - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        movePlayer();
        dashPlayer();
    }

    public void movePlayer()
    {
        if (attacking)
            return; 

        Vector3 movement = new Vector3(move.x, 0f, move.y);
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