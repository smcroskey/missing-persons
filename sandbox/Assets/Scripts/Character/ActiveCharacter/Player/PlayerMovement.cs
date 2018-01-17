﻿using UnityEngine;

namespace Character2D
{
    public class PlayerMovement : ActiveCharacterMovement
    {
        public PlayerInteraction playerInteraction;
        private PlayerAttack playerAttack;

        public ClimbingTriggerTop climbingTriggerTop;
        public UncrouchTrigger uncrouchTrigger;

        //external input
        public bool crouchInput; //crouch input from character
        public float climbSpeedInput;
        public float climbSpeed;

        //external booleans that drive the character states
        public bool canUncrouch; //whether the character can uncrouch or not
        public bool canClimb;

        //booleans that drive the character states
        protected bool isCrouching; //whether the character is crouching or not
        public bool isOnLadder;
        public bool isClimbing;
        public bool isStrafing;

        //values that affect the character's ability to crouch/uncrouch
        protected float tCrouch; //time since the last crouch
        protected bool isInitCrouch; //initial frame of a crouch (to apply crouch logic)

        //delays after each action
        protected float crouchDelay; //the amount of time before the character can uncrouch
        protected float slideDelay; //the duration of the slide animation
        private bool slideBonus;

        //movement speed in each state
        protected float crouchSpeed; //how much crouching decreases the movement speed

        //used for initialization
        protected new void Start()
        {
            base.Start();

            playerInteraction = GetComponent<PlayerInteraction>();
            playerAttack = GetComponent<PlayerAttack>();

            crouchInput = false;

            canUncrouch = true;

            isCrouching = false;
            slideBonus = true;

            tCrouch = 0.0f;
            isInitCrouch = true;

            crouchDelay = 0.75f;
            slideDelay = 0.25f;

            crouchSpeed = 0.50f;

            climbSpeedInput = 0.0f;
            climbSpeed = 0.0f;
        }

        //called once per frame (for input)
        protected new void Update()
        {
            base.Update();
            if(playerInteraction.isInteracting)
            {
                rb.velocity = new Vector2(0.0f, rb.velocity.y);
                SendToAnimator();
            }
        }

        //called once per game tick (for physics)
        protected new void FixedUpdate()
        {
            base.FixedUpdate();
            if(playerInteraction.isInteracting)
            {
                rb.velocity = new Vector2(0.0f, rb.velocity.y);
                SendToAnimator();
            }
        }

        //handles player input
        protected override void HandleInput()
        {
            //check if the character can jump
            if (!isJumping && !isCrouching && isGrounded && Time.time - tJump > jumpDelay && !playerAttack.isWindingUp)
            {
                isJumping = jumpInput;
                SendToAnimator();
            }
            //check if the character can run (or stop running)
            if (!isJumping && !isCrouching && isGrounded && !isOnLadder)
            {
                isRunning = runInput;
                SendToAnimator();
            }
            //check if the character can crouch (or stop crouching)
            if (!isJumping && isGrounded && !playerAttack.isWindingUp && !isOnLadder)
            {
                //the character can only crouch when they are not crouching
                //the character can only uncrouch when they have room to stand up
                if(CheckIfCanUncrouch() || !isCrouching)
                {
                    isCrouching = crouchInput;
                    SendToAnimator();
                    //start the crouch timer if this is the first crouch frame
                    if(isCrouching && isInitCrouch)
                    {
                        //started crouching
                        isInitCrouch = false;
                        tCrouch = Time.time;
                    }
                    if(!isCrouching)
                    {
                        //stopped crouching
                        isInitCrouch = true;
                        slideBonus = true;
                    }
                }
            }
            if(!isCrouching && canClimb && !isOnLadder && climbingTriggerTop.currentObjects.Count != 0)
            {
                if(climbSpeedInput > 0.0f || (climbSpeedInput < 0.0f && !isGrounded))
                {
                    isOnLadder = true;
                }
            }
            if(!isJumping && isGrounded && !isCrouching && !isOnLadder)
            {
                playerAttack.canAttack = true;
            }
            else
            {
                playerAttack.canAttack = false;
            }
        }

        protected override void Move()
        {
            if (isJumping)
            {
                if (isRunning)
                {
                    speedMultiplier = jumpSpeed * runSpeed;
                }
                else
                {
                    speedMultiplier = jumpSpeed;
                }

                //if this is the first jump frame, add the jump force
                if (isInitJump)
                {
                    rb.AddForce(new Vector2(0f, jumpForce));
                    isInitJump = false;
                    isOnLadder = false;
                }
            }
            else if (isCrouching)
            {
                if(isRunning && Time.time - tCrouch < slideDelay && slideBonus)
                {
                    speedMultiplier = runSpeed * crouchSpeed;
                }
                else
                {
                    slideBonus = false;
                    speedMultiplier = crouchSpeed;
                }
                if(playerInteraction.isInteracting)
                {
                    tCrouch = Time.time;
                }
            }
            else if (isRunning)
            {
                speedMultiplier = runSpeed;
            }
            else
            {
                speedMultiplier = 1;
            }

            if(isOnLadder)
            {
                isClimbing = false;
                isStrafing = false;
                rb.gravityScale = 0.0f;
                rb.velocity = Vector2.zero;
                if(climbSpeedInput > 0.0f || (climbSpeedInput < 0.0f && !isGrounded))
                {
                    climbSpeed = climbSpeedInput;
                    isClimbing = true;
                    transform.Translate(Vector3.up * climbSpeedInput / 50f);
                }
                else if(Mathf.Abs(mvmtSpeed) > 0.0f && !isGrounded)
                {
                    climbSpeed = 0.0f;
                    isStrafing = true;
                    transform.Translate(Vector3.right * Mathf.Abs(mvmtSpeed) / 75f);
                }
                else if(isGrounded && climbingTriggerTop.currentObjects.Count != 0)
                {
                    climbSpeed = 0.0f;
                    DoneClimbing();
                }
                else if(isGrounded && climbingTriggerTop.currentObjects.Count == 0)
                {
                    climbSpeed = 1.0f;
                    isClimbing = true;
                    transform.Translate(Vector3.up / 50f);
                }
                else
                {
                    climbSpeed = 0.0f;
                }
            }
            else
            {
                SmoothMove(mvmtSpeed * speedMultiplier * maxSpeed * Time.deltaTime, rb.velocity.y, true);
            }  
        }

        protected override void SetMovementLogic()
        {
            //TODO: change to 2d movement check
            //boolean expression that sets whether or not the player has moved this tick
            isMoving = Mathf.Abs(lastPosition - rb.transform.position.x) > 0.001f ? true : false;

            //boolean expression that sets whether or not the character is falling on this tick
            isFalling = rb.velocity.y < 0 && !isGrounded ? true : false;

            lastPosition = rb.transform.position.x; //set the last position for the next tick

            //change the player's direction if they are moving in another direction
            if (mvmtSpeed < 0 && isFacingRight)
            {
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                isFacingRight = false;
            }
            else if (mvmtSpeed > 0 && !isFacingRight)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                isFacingRight = true;
            }
        }

        //sends boolean values describing character state to the animator
        protected override void SendToAnimator()
        {
            anim.SetBool("isJumping", isJumping);
            anim.SetBool("isGrounded", isGrounded);
            anim.SetBool("isRunning", isRunning);
            anim.SetBool("isMoving", isMoving);
            anim.SetBool("isFalling", isFalling);
            anim.SetFloat("mvmtSpeed", Mathf.Abs(mvmtSpeed));
            anim.SetBool("isCrouching", isCrouching);
            anim.SetBool("isClimbing", isClimbing);
            anim.SetBool("isStrafing", isStrafing);
            anim.SetFloat("climbSpeed", Mathf.Abs(climbSpeed));
            anim.SetBool("isOnLadder", isOnLadder);
            anim.SetBool("isInteracting", playerInteraction.isInteracting);
        } 

        //checks if the character has enough room to uncrouch
        protected bool CheckIfCanUncrouch()
        {
            if (canUncrouch && !crouchInput && Time.time - tCrouch > crouchSpeed)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void DoneClimbing()
        {
            isOnLadder = false;
            isStrafing = false;
            isClimbing = false;
            rb.gravityScale = 2.0f;
            climbSpeed = 0.0f;
        }

        public void RollBonus()
        {
            if(isFacingRight)
            {
                rb.AddForce(new Vector2(1500f, 0f));
            }
            else
            {
                rb.AddForce(new Vector2(-1500f, 0f));
            }
        }
    }
}
