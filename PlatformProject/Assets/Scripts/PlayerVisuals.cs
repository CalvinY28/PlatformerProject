using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script manages updating the visuals of the character based on the values that are passed to it from the PlayerController.
/// NOTE: You shouldn't make changes to this script when attempting to implement the functionality for the W10 journal.
/// </summary>
public class PlayerVisuals : MonoBehaviour
{
    public Animator animator;
    public SpriteRenderer bodyRenderer;
    public PlayerController playerController;
    public CameraControl cameraController;

    private int idleHash, walkingHask, jumpingHash, dieHash;

    // Start is called before the first frame update
    void Start()
    {
        idleHash = Animator.StringToHash("Idle");
        walkingHask = Animator.StringToHash("Walking");
        jumpingHash = Animator.StringToHash("Jumping");
        dieHash = Animator.StringToHash("Death");
    }

    // Update is called once per frame
    void Update()
    {
        VisualsUpdate();
    }

    //It is not recommended to make changes to the functionality of this code for the W10 journal.
    private void VisualsUpdate()
    {
        if (playerController.PreviousCharacterState != playerController.currentCharacterState)
        {
            switch (playerController.currentCharacterState)
            {
                case PlayerController.CharacterState.idle:
                    if(playerController.PreviousCharacterState == PlayerController.CharacterState.jump)
                    {
                        cameraController.Shake(5f, 0.35f);
                    }
                    animator.CrossFade(idleHash, 0f);
                    break;
                case PlayerController.CharacterState.walk:
                    if (playerController.PreviousCharacterState == PlayerController.CharacterState.jump)
                    {
                        cameraController.Shake(5f, 0.35f);
                    }
                    animator.CrossFade(walkingHask, 0f);
                    break;
                case PlayerController.CharacterState.jump:
                    animator.CrossFade(jumpingHash, 0f);
                    break;
                case PlayerController.CharacterState.die:
                    animator.CrossFade(dieHash, 0f);
                    break;
            }
        }

        //animator.SetBool(isWalkingHash, playerController.IsWalking());
        //animator.SetBool(isGroundedHash, playerController.IsGrounded());

        //if (playerController.isDead())
        //{
        //    animator.SetTrigger(onDieHash);
        //}

        switch (playerController.GetFacingDirection())
        {
            case PlayerController.FacingDirection.left:
                bodyRenderer.flipX = true;
                break;
            case PlayerController.FacingDirection.right:
            default:
                bodyRenderer.flipX = false;
                break;
        }
    }
}
