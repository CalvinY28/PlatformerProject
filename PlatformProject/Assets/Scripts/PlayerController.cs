using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float maxSpeed = 5f;
    public float acceleration = 5f;
    public float friction = 5f;
    public float currentSpeed = 0f;

    private Rigidbody2D rb;
    public bool isGrounded = false;

    public enum FacingDirection
    {
        left, right
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        //The input from the player needs to be determined and then passed in the to the MovementUpdate which should
        //manage the actual movement of the character.
        //Vector2 playerInput = new Vector2();

        float xInput = Input.GetAxisRaw("Horizontal"); // Get axisraw for no automatic smoothing
        Vector2 playerInput = new Vector2(xInput, 0);
        MovementUpdate(playerInput);
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        if (playerInput.x != 0) // If input then add to currentSpeed until maxSpeed is reached
        {
            currentSpeed += playerInput.x * acceleration * Time.deltaTime;
            currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);

            if (playerInput.x > 0) // When moving right fast the right when moving left face the left
            {
                facingDirection = FacingDirection.right;
            }
            else
            {
                facingDirection = FacingDirection.left;
            }
        }

        if (playerInput.x == 0) // If no input then subtract from current speed until zero ---------- unless im facing the other way then add to current speed until zero
        {
            if (currentSpeed > 0)
            {
                currentSpeed -= friction * Time.deltaTime;
                currentSpeed = Mathf.Max(currentSpeed, 0);
            }
            if (currentSpeed < 0) // Need to do both ways
            {
                currentSpeed += friction * Time.deltaTime;
                currentSpeed = Mathf.Min(currentSpeed, 0);
            }
        }

        rb.velocity = new Vector2(currentSpeed, rb.velocity.y);

    }

    public bool IsWalking()
    {
        if (currentSpeed > 0.1f || currentSpeed < -0.1f)
        {
            return true;
        }
        else
        {
            return false;
        }

    }
    public bool IsGrounded()
    {
        return isGrounded;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    public FacingDirection GetFacingDirection()
    {
        return facingDirection;
    }

    private FacingDirection facingDirection = FacingDirection.left; // Created private variable to store information from the movement method
}
