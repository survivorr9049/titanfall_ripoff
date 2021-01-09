﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class characterController : MonoBehaviour
{   
    [Header("Objects")]
    public CharacterController player;
    public Text text;
    public Transform camera;
    public Transform ground;

    [Header("Basic Movement")]
    public float movementSpeed;
    public float sprintSpeed;
    public float airControl;
    public float airAccelerationMultiplier = 0.45f;
    public float drag;
    public float mouseSens;
    public float jumpForce;
    public float jumpAmount; //test purpose
    public float gravity;

    [Header("WallRunning")]
    public bool isWall;
    public bool wasWall;
    public Vector3 contact;
    public float actualGravity;

    [Header("Debug")]
    public float normalizer = 1f;
    public bool isGrounded;
    public bool wasGrounded;
    public Vector3 velocity;
    public float actualSpeed;
    public float actualDrag;
    public bool diagonal;
    float mouseX;
    float mouseY;


    // Start is called before the first frame update
    void Start() {
        actualDrag = drag;
        actualGravity = gravity;
        //later set objects here
    }
    void Update( ){
        if (wasWall && !isWall) wallExit();
        if (!wasWall && isWall) print("Wall Enter");
        wasWall = isWall;
        if (isWall) isWall = false;

        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
        transform.Rotate(new Vector3(0, mouseX * mouseSens));
        camera.Rotate(new Vector3(-mouseY * mouseSens, 0f));

        isGrounded = groundCheck(isGrounded);
        if (Input.GetKeyDown(KeyCode.Space) && jumpAmount > 0) {
            velocity.y = Mathf.Clamp(velocity.y, -gravity, 1000);
            velocity.y += jumpForce;
            jumpAmount -= 1;
        }
        if (Input.GetKey(KeyCode.W)) velocity += transform.forward * actualSpeed * normalizer * Time.deltaTime;
        if (Input.GetKey(KeyCode.D)) velocity += transform.right * actualSpeed * normalizer * Time.deltaTime;
        if (Input.GetKey(KeyCode.A)) velocity += transform.right * -actualSpeed * normalizer * Time.deltaTime;
        if (Input.GetKey(KeyCode.S)) velocity += transform.forward * -actualSpeed * normalizer * Time.deltaTime;


        if (isGrounded) {
            jumpAmount = 2;
            if (Input.GetKey(KeyCode.LeftShift)) actualSpeed = sprintSpeed;
            else actualSpeed = movementSpeed;

            float reference = 0f;
            actualDrag = Mathf.SmoothDamp(actualDrag, drag, ref reference, 0.01f);

            if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.W)) {
                diagonal = true;
            }
            else {
                diagonal = false;
            }

            if (diagonal) normalizer = 0.707f;
            else normalizer = 1f;
        }
        else {
            actualDrag = drag / airControl;
            normalizer = 1f;
            actualSpeed = movementSpeed * airAccelerationMultiplier;
        }

        player.Move(velocity * Time.deltaTime);

    }

    // Update is called once per frame
    void FixedUpdate() {
        text.text = Vector3.Magnitude(new Vector3(velocity.x, 0, velocity.z)).ToString();

        velocity.x = velocity.x * 1 / (actualDrag + 1);
        velocity.z = velocity.z * 1 / (actualDrag + 1);

        if (!isGrounded) {
            velocity.y -= actualGravity;
        }
        else {
            velocity.y = Mathf.Clamp(velocity.y, -gravity, 1000);
        }
    }
    bool groundCheck(bool isGrounded) {
   
        wasGrounded = isGrounded;
        float radius = 0.4f;
        LayerMask mask = LayerMask.GetMask("Ground");
        isGrounded = Physics.CheckSphere(ground.position, radius, mask);
        if(isGrounded && !wasGrounded)
        {
            groundEnter();
        }
        else if(!isGrounded && wasGrounded)
        {
            groundExit();
        }
  
        return isGrounded;
       
    }
    void groundExit() {
        print("exit");
    }
    void groundEnter() {
        print("enter");
    }

    private void OnControllerColliderHit(ControllerColliderHit collision) {
        if (!isGrounded && collision.normal.y < 0.1f) {
            isWall = true;
            contact = collision.normal;
            //velocity += contact * 40;
            //velocity.y += 20;
            float reference = 0f;
            actualGravity = Mathf.SmoothDamp(gravity / 10, gravity, ref reference, 0.005f);
            velocity.y = Mathf.Clamp(velocity.y, -actualGravity, actualGravity);
            velocity += contact * -10/velocity.magnitude;
            //velocity += transform.forward;
            if (Input.GetKeyDown(KeyCode.Space)) {
                velocity += Vector3.Scale(new Vector3(Mathf.Abs(velocity.x), Mathf.Abs(velocity.y), Mathf.Abs(velocity.z)), contact); 
                velocity += contact * 15 + transform.up * 4;

            }

        }

    }
    void wallEnter() {

    }
    void wallExit() {
        actualGravity = gravity;
        velocity += Vector3.Scale(new Vector3(Mathf.Abs(velocity.x), Mathf.Abs(velocity.y), Mathf.Abs(velocity.z)) * 1.3f, contact * 1.2f);
        print(velocity.x);

    }
}