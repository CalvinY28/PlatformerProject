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

    public float doubleTapTime = 0.2f;
    public float lastTapTime = 0;
    public float lastHorizontalInput = 0f;
    public float dashCooldownTime = 0.5f;
    public bool canDash = true;

    public bool isTouchingWall;
    public float wallJumpXVelocity = 5f;
    public float wallJumpYVelocity = 13f;
    public bool isWallJumping;

    public float jetpackAcceleration = 5f;
    public float maxJetpackSpeed = 10f;
    public bool isJetpacking = false;
    public bool hasJumped = false;
    public float maxFuel = 3f;
    public float fuelRegenRate = 1f;
    public float currentFuel;

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

        currentFuel = maxFuel;
    }

    void Update()
    {
        DoubleTapToDash();
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
        int isCollidingHorizontallyLeftandRight = CheckHorizontalCollision();

        isTouchingWall = isCollidingHorizontallyLeftandRight != 0 && !isGrounded;

        if (isTouchingWall == true)
        {
            //rb.gravityScale = gravity / 200f;
            //rb.velocity = new Vector2(rb.velocity.x, -2f);
            //rb.AddForce(Vector2.down / 200f, ForceMode2D.Impulse);
            if (rb.velocity.y < -1f)
            {
                rb.AddForce(new Vector2(0, -1f), ForceMode2D.Force); // sliding down wall dosent work how intended
            }
        }
        else
        {
            //rb.gravityScale = gravity;
        }

        if (playerInput.x != 0) // If input then add to currentSpeed until maxSpeed is reached
        {
            if (playerInput.x > 0 && isCollidingHorizontallyLeftandRight != 1 || // should add cyote time to wall jumping
                playerInput.x < 0 && isCollidingHorizontallyLeftandRight != -1) // If is colliding left or right speed = 0
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
            else
            {
                currentSpeed = 0f;
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
            hasJumped = true;
        }

        if (Input.GetButtonDown("Jump") && isTouchingWall && !isWallJumping)
        {
            isWallJumping = true;
            rb.velocity = new Vector2(-wallJumpXVelocity, wallJumpYVelocity);
            hasJumped = true;
        }

        if(isGrounded)
        {
            isWallJumping = false;
            //hasJumped = false; // this is broken keeps being false but its ok i dont really need it anyway
            currentFuel = Mathf.Min(currentFuel + fuelRegenRate * Time.deltaTime, maxFuel); // Regen fuel when on ground
        }

        if (rb.velocity.y < terminalSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, terminalSpeed);
        }

        if (Input.GetButton("Jump") && !isGrounded && !isTouchingWall && hasJumped && currentFuel > 0)
        {
            isJetpacking = true;
            currentFuel -= Time.deltaTime; // Consume feul when true
        }
        else
        {
            isJetpacking = false;
        }

        if (isJetpacking)
        {
            if (rb.velocity.y < maxJetpackSpeed)
            {
                rb.velocity += new Vector2(0, jetpackAcceleration * Time.deltaTime);
            }
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
        //Debug.DrawRay(transform.position, Vector2.down * 0.7f, Color.red);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.7f, groundLayer);
        if (hit.collider != null && hit.collider.CompareTag("Ground")) // got spammed with error without != null
        {
            return true;
        }
        return false;
    }

    private int CheckHorizontalCollision() // Change to int so i can use numbers
    {
        float checkDistance = 0.7f;
        Vector2 leftCheck = transform.position - new Vector3(0.5f, -0.2f, 0);
        Vector2 rightCheck = transform.position + new Vector3(0.4f, 0.2f, 0);

        //Debug.DrawRay(rightCheck, Vector2.down * checkDistance, Color.red); // debug to place the raycast in the right place
        //Debug.DrawRay(leftCheck, Vector2.down * checkDistance, Color.red);

        RaycastHit2D hitLeft = Physics2D.Raycast(leftCheck, Vector2.down, checkDistance, groundLayer); // Cast to the left
        RaycastHit2D hitRight = Physics2D.Raycast(rightCheck, Vector2.down, checkDistance, groundLayer); // Cast to the right

        if (hitLeft.collider != null) return -1; // Colliding to the left
        if (hitRight.collider != null) return 1; // Colliding to the right
        return 0;
    }

    public void DoubleTapToDash()
    {
        float xInput = Input.GetAxisRaw("Horizontal");

        if (!canDash) return; // Exit if cant dash

        if (xInput < 0 && lastHorizontalInput == 0 && !isGrounded) // Detect left double-tap
        {
            if (Time.time - lastTapTime <= doubleTapTime) // If time is less than or equal to doubleTapTime, double-tap.
            {
                Debug.Log("left");
                Dash(Vector2.left);
            }

            lastTapTime = Time.time;
        }

        if (xInput > 0 && lastHorizontalInput == 0 && !isGrounded) // Detect right double-tap
        {
            if (Time.time - lastTapTime <= doubleTapTime) // I should use time here instead of delta time
            {
                Debug.Log("right");
                Dash(Vector2.right);
            }

            lastTapTime = Time.time;
        }

        lastHorizontalInput = xInput; // Reset
    }

    public void Dash(Vector2 direction)
    {
        //rb.velocity = new Vector2(direction.x * 20f, rb.velocity.y);
        //rb.AddForce(direction * 20f, ForceMode2D.Force);

        //Vector2 dashDistance = direction.normalized * 2f; // dosent work for now just leave it as teleporting
        //transform.position += dashDistance;

        //rb.AddForce(direction);
        //rb.velocity = direction.normalized * 20f;
        //rb.AddForce(direction.normalized * 20f, ForceMode2D.Impulse);

        //transform.Translate(direction.normalized * 5f, Space.World);

        StartCoroutine(PerformDash(direction, 6f, 0.2f)); // realized i need to use a corutine/timer to make it push
        rb.gravityScale = -1f;


        Debug.Log("gfrewuihwe");
        StartCoroutine(DashCooldown());
    }

    public IEnumerator DashCooldown()
    {
        canDash = false;
        yield return new WaitForSeconds(dashCooldownTime);
        canDash = true;
    }

    private IEnumerator PerformDash(Vector2 direction, float distance, float duration)
    {
        Vector2 startVelocity = rb.velocity;
        //Vector2 dashVelocity = direction.normalized * distance;
        Vector2 dashVelocity = direction.normalized * (distance / duration); // Velocity needed to cover the distance in the time frame

        float elapsedTime = 0f;
        while (elapsedTime < duration) // Need this to make the player move over time and hit walls during time of the dash
        {
            int isCollidingHorizontallyLeftandRight = CheckHorizontalCollision();
            if (isCollidingHorizontallyLeftandRight != 0)
            {
                Debug.Log("wall wall wall");
                break;
            }

            rb.velocity = new Vector2(direction.x * distance * 2, 0f); // I can lerp this or sdomthing to make it smooth i think?

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rb.velocity = dashVelocity;
        rb.gravityScale = gravity;

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
