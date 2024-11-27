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
    public LayerMask groundLayer; // need this

    public float apexHeight = 6f;
    public float apexTime = 5f;
    public float jumpVelocity;
    public float gravity;
    public float terminalSpeed = -10f;

    public float coyoteTime = 0.1f;
    public float coyoteTimeCounter;

    public int health = 10;

    public enum CharacterState
    {
        idle, walk, jump, die
    }

    public CharacterState currentCharacterState = CharacterState.idle;
    public CharacterState PreviousCharacterState = CharacterState.idle;


    public enum FacingDirection
    {
        left, right
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        gravity = (2 * apexHeight) / Mathf.Pow(apexTime, 2); // can not get this to work
        jumpVelocity = (2 * apexHeight) / apexTime;
        rb.gravityScale = gravity;
    }

    void Update()
    {
        PreviousCharacterState = currentCharacterState;

        //The input from the player needs to be determined and then passed in the to the MovementUpdate which should
        //manage the actual movement of the character.
        //Vector2 playerInput = new Vector2();

        float xInput = Input.GetAxisRaw("Horizontal"); // Get axisraw for no automatic smoothing
        Vector2 playerInput = new Vector2(xInput, 0);
        MovementUpdate(playerInput);
        isGrounded = IsGrounded();

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime; // Reset counter if grounded
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime; // Lower counter if in the air
        }

        switch (currentCharacterState)
        {
            case CharacterState.die:
                

                break;
            case CharacterState.jump:
                if (isGrounded)
                {
                    if (IsWalking())
                    {
                        currentCharacterState = CharacterState.walk;
                    }
                    else
                    {
                        currentCharacterState = CharacterState.idle;
                    }
                }

                break;
            case CharacterState.walk:
                if (!IsWalking())
                {
                    currentCharacterState = CharacterState.idle;
                }
                if (!isGrounded)
                {
                    currentCharacterState = CharacterState.jump;
                }

                break;
            case CharacterState.idle:
                if (IsWalking())
                {
                    currentCharacterState = CharacterState.walk;
                }
                if (!isGrounded)
                {
                    currentCharacterState = CharacterState.jump;
                }

                break;
        }
        if (isDead())
        {
            currentCharacterState = CharacterState.die;
        }
    }

    private void FixedUpdate()
    {
        //MovementUpdate(playerInput); should be in fixed update || might not run every frame
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        if (playerInput.x != 0) // If input then add to currentSpeed until maxSpeed is reached
        {
            currentSpeed += playerInput.x * acceleration * Time.deltaTime;
            currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);

            if (playerInput.x > 0) // When moving right fase the right when moving left face the left
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

        if(Input.GetButtonDown("Jump") && (isGrounded || coyoteTimeCounter > 0))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
            isGrounded = false;
        }

        if (rb.velocity.y < terminalSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, terminalSpeed);
        }

    }

    public bool IsWalking()
    {
        if (currentSpeed > 0.01f || currentSpeed < -0.01f)
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
        Debug.DrawRay(transform.position, Vector2.down * 0.7f, Color.red);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.7f, groundLayer);
        if (hit.collider != null && hit.collider.CompareTag("Ground")) // got spammed with error without != null
        {
            return true;
        }
        return false;
    }

    public FacingDirection GetFacingDirection()
    {
        return facingDirection;
    }

    private FacingDirection facingDirection = FacingDirection.left; // Created private variable to store information from the movement method

    public bool isDead()
    {
        return health <= 0;
    }

    public void OnDeathAnimationComplete()
    {
        gameObject.SetActive(false);
    }

}
