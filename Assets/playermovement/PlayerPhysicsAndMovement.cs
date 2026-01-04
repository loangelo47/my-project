using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerPhysicsAndMovement : MonoBehaviour
{
    CharacterController controller;
    public Transform groundCheck;

    public LayerMask groundMask;

    Vector3 move;
    Vector3 input;
    Vector3 Yvelocity;

    int jumpCharges;

    bool isGrounded;
    bool isSprinting;
    bool isCrouching;
    float speed;
    public float runSpeed;
    public float sprintSpeed;
    public float crouchSpeed;
    public float airSpeed;

    float gravity;
    public float normalGravity;

    public float jumpHeight;

    float startHeight;
    float crouchHeight = 0.5f;
    Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    Vector3 standingCenter = new Vector3(0, 0, 0);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        startHeight = transform.localScale.y;
    }
    void Update()
    {
        HandleInput();
        if (isGrounded)
        {
            GroundedMovement();
        }
        else
        {
            Airmovement();
        }
        GroundedMovement();
        checkGround();
        controller.Move(move * Time.deltaTime);
        ApplyGravity();
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
        }
    }
    void ApplyGravity()
    {
        gravity = normalGravity;
        Yvelocity.y += gravity * Time.deltaTime;
        controller.Move( Yvelocity * Time.deltaTime );
    }
    void jump()
    {
        Yvelocity.y = Mathf.Sqrt( jumpHeight * -2f * normalGravity );
    }
    void Airmovement()
    {
        move.x += input.x * airSpeed;
        move.z += input.z * airSpeed;

        move = Vector3.ClampMagnitude(move, airSpeed);
    }
    void Crouch()
    {
        controller.height = crouchHeight;
        controller.center = crouchingCenter;
        transform.localScale = new Vector3(transform.localScale.x, crouchHeight, transform.localScale.z);
        isCrouching = true;
    }
    void ExitCrouch()
    {
        controller.height = (startHeight * 2);
        controller.center = standingCenter;
        transform.localScale = new Vector3(transform.localScale.x, startHeight, transform.localScale.z);
        isCrouching = false;
    }
}
