using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerPhysicsAndMovement : MonoBehaviour
{
    CharacterController controller;
    public Transform groundCheck;

    public LayerMask groundMask;
    public LayerMask wallMask;

    Vector3 move;
    Vector3 input;
    Vector3 Yvelocity;
    Vector3 forwardDirection;

    int jumpCharges = 1;

    bool isGrounded;
    bool isSprinting;
    bool isCrouching;
    bool isSliding;


    public float slideSpeedIncrease;
    public float slideSpeedDecrease;
    public float wallRunSpeedIncrease;
    public float wallRunSpeedDecrease;
    float speed;
    public float runSpeed;
    public float sprintSpeed;
    public float crouchSpeed;
    public float airSpeed;

    float gravity;
    public float normalGravity;

    public float jumpHeight;

    public float wallRunGravity;
    float startHeight;
    float crouchHeight = 0.5f;
    Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    Vector3 standingCenter = new Vector3(0, 0, 0);
    float slideTimer;
    public float maxSlideTimer;

    bool isWallRunning;
    bool hasWallRun = false;
    bool onLeftWall;
    bool onRightWall;
    RaycastHit leftWallHit;
    RaycastHit rightWallHit;
    Vector3 wallNormal;
    Vector3 lastWallNormal;

    public Camera playerCamera;
    float normalFov;
    public float specialFov;
    public float cameraChangeTime;
    public float wallRunTilt;
    public float tilt;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        startHeight = transform.localScale.y;
        normalFov = playerCamera.fieldOfView;
    }

    void IncreaseSpeed(float speedIncrease)
    {
        speed += speedIncrease;
    }

    void DecreaseSpeed(float speedDecrease)
    {
        speed -= speedDecrease * Time.deltaTime;
    }
    void CameraEffects()
    {
        float fov = isWallRunning ? specialFov : isSliding ? specialFov : normalFov;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, fov, cameraChangeTime * Time.deltaTime);

        if (isWallRunning)
        {
            if (onRightWall)
            {
                tilt = Mathf.Lerp(tilt, wallRunTilt, cameraChangeTime * Time.deltaTime);
            }
            if (onLeftWall)
            {
                tilt = Mathf.Lerp(tilt, -wallRunTilt, cameraChangeTime * Time.deltaTime);
            }
        }
        if (!isWallRunning)
        {
            tilt = Mathf.Lerp(tilt, 0f, cameraChangeTime * Time.deltaTime);
        }
    }
    void Update()
    {
        CheckWallRun();
        HandleInput();
        if (isGrounded && !isSliding)
        {
            GroundedMovement();
        }
        else if (!isGrounded && !isWallRunning)
        {
            Airmovement();
        }
        else if (isSliding)
        {
            SlideMovement();
            DecreaseSpeed(slideSpeedDecrease);
            slideTimer -= 1f * Time.deltaTime;
            if (slideTimer < 0)
            {
                isSliding = false;
            }
        }
        else if (isWallRunning)
        {
            WallRunMovement();
            DecreaseSpeed(wallRunSpeedDecrease);
        }
        GroundedMovement();
        checkGround();
        controller.Move(move * Time.deltaTime);
        ApplyGravity();
        CameraEffects();
    }
    void HandleInput()
    {
        input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

        input = transform.TransformDirection( input );
        input = Vector3.ClampMagnitude( input, 1f );

        if (Input.GetKeyUp(KeyCode.Space) && jumpCharges > 0)
        {
            jump();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            Crouch();
        }
        if (Input.GetKeyUp(KeyCode.C))
        {
            ExitCrouch();
        }
        if (Input.GetKeyDown(KeyCode.LeftShift ) && isGrounded)
        {
            isSprinting = true;
        }
        if (Input.GetKeyDown(KeyCode.LeftShift ))
        {
            isSprinting = false;
        }
    }   
    // Update is called once per frame

    void GroundedMovement()
    {
        speed = isSprinting ? sprintSpeed : isCrouching ? crouchSpeed : runSpeed;
        if (input.x != 0)
        {
            move.x += input.x * speed;
        }
        else
        {
            move.x = 0;
        }
        if (input.z != 0)
        {
            move.z += input.z * speed;
        }
        else
        {
            move.z = 0;
        }
        move = Vector3.ClampMagnitude(move, speed);
    }
    void checkGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.2f, groundMask);
        if (isGrounded)
        {
            jumpCharges = 1;
            hasWallRun = false;
        }
    }
    void ApplyGravity()
    {
        gravity = isWallRunning ? wallRunGravity : normalGravity;
        Yvelocity.y += gravity * Time.deltaTime;
        controller.Move( Yvelocity * Time.deltaTime );
    }
    void jump()
    {
        if (!isGrounded && !isWallRunning)
        {
            jumpCharges -= 1;
        }
        else if (isWallRunning)
        {
            ExitWallRun();
            IncreaseSpeed(wallRunSpeedIncrease);
        }
        Yvelocity.y = Mathf.Sqrt( jumpHeight * -2f * normalGravity );
    }
    void Airmovement()
    {
        move.x += input.x * airSpeed;
        move.z += input.z * airSpeed;

        move = Vector3.ClampMagnitude(move, airSpeed);
    }

    void SlideMovement()
    {
        move += forwardDirection;
        move = Vector3.ClampMagnitude(move, speed);
    }
    void Crouch()
    {
        controller.height = crouchHeight;
        controller.center = crouchingCenter;
        transform.localScale = new Vector3(transform.localScale.x, crouchHeight, transform.localScale.z);
        isCrouching = true;
        if (speed >= runSpeed)
        {
            isSliding = true;
            forwardDirection = transform.forward;
            if (isGrounded)
            {
                IncreaseSpeed(slideSpeedIncrease);
            }
            slideTimer = maxSlideTimer;
        }
    }
    
    void ExitCrouch()
    {
        controller.height = (startHeight * 2);
        controller.center = standingCenter;
        transform.localScale = new Vector3(transform.localScale.x, startHeight, transform.localScale.z);
        isCrouching = false;
        isSliding = false;
    }
    void CheckWallRun()
    {
        onLeftWall = Physics.Raycast(transform.position, -transform.right, out leftWallHit, 0.7f, wallMask);
        onRightWall = Physics.Raycast(transform.position, transform.right, out rightWallHit, 0.7f, wallMask);

        if ((onRightWall || onLeftWall) && !isWallRunning)
        {
            TestWallRun();
        }
        if ((!onRightWall && !onLeftWall) && isWallRunning)
        {
            ExitWallRun();
        }
    }
    void WallRun()
    {
        isWallRunning = true;
        jumpCharges =1;
        IncreaseSpeed(wallRunSpeedIncrease);
        Yvelocity = new Vector3(0f, 0f, 0f);

        forwardDirection = Vector3.Cross(wallNormal, Vector3.up);

        if (Vector3.Dot(forwardDirection, transform.forward) < 0)
        {
            forwardDirection = -forwardDirection;
        }
    }

    void ExitWallRun()
    {
        isWallRunning = false;
        lastWallNormal = wallNormal;
    }
    void WallRunMovement()
    {
        if(input.z > (forwardDirection.z - 10f) && input.z < (forwardDirection.z + 10f))
        {
            move.z += forwardDirection.z * speed;
        }
        else if (input.z < (forwardDirection.z - 10f) && input.z > (forwardDirection.z +10f))
        {
            move.x = 0f;
            move.z = 0f;
            ExitWallRun();
        }
        move.x += input.x * airSpeed;

        move = Vector3.ClampMagnitude(move, speed);
    }

    void TestWallRun()
    {
        wallNormal = onLeftWall ? leftWallHit.normal : rightWallHit.normal;
        if (hasWallRun)
        {
            float wallAngle = Vector3.Angle(wallNormal, lastWallNormal);
            if (wallAngle > 15)
            {
                WallRun();
            }
        }
        else
        {
            WallRun();
            hasWallRun = true;
        }
    }
}
