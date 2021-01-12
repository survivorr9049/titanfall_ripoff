using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class characterController : MonoBehaviour {
    [Header("Objects")]
    public CharacterController player;
    public Text text;
    public Transform camera;
    public Transform ground;
    public Transform cameraParent;

    [Header("Basic Movement")]
    public float movementSpeed;
    public float sprintSpeed;
    public float airControl;
    public float airAccelerationMultiplier = 0.45f;
    public float drag;
    public float mouseSens;
    public float jumpForce;
    public bool secondJump; //test purpose
    public float gravity;

    [Header("WallRunning")]
    public float wallAccelerationMultiplier;
    public float wallControl;
    public float wallrunCameraAngle;
    public float cameraComes;
    public float cameraSmoothness;
    public float runTimer;
    public bool isWall;
    public bool wasWall;
    public Vector3 contact;
    public float actualGravity;
    public Vector3 lastContact;
    [Header("Debug")]
    public float normalizer = 1f;
    public bool isGrounded;
    public bool wasGrounded;
    public Vector3 velocity;
    public Vector3 input;
    public float actualSpeed;
    public float actualDrag;
    public bool diagonal;
    public float mouseX;
    public float mouseY;
    public float test;
    public float reference0;
    public float comes;
    float camZ;
    public int wallSide;
    public Quaternion rot;
    public Vector3 final;
    float timer;
    public bool jumped;

    // Start is called before the first frame update
    void Start() {
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;

        actualDrag = drag;
        actualGravity = gravity;
        //later set objects here
    }
    void Update() {
        //input

        //dev test
        if (Input.GetKey(KeyCode.C)) {
            velocity += transform.forward * 1000 * Time.deltaTime;
        }
        //dev test

        input = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        mouseX = Input.GetAxis("Mouse X") * mouseSens;
        mouseY += Input.GetAxis("Mouse Y") * mouseSens;
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded) {
            velocity.y = Mathf.Clamp(velocity.y, -gravity, 1000);
            velocity.y += jumpForce;
        }else if(Input.GetKeyDown(KeyCode.Space) && secondJump) {
            doubleJump();
        }
        velocity += transform.forward * actualSpeed * Time.deltaTime * input.z * normalizer; 
        velocity += transform.right * actualSpeed * Time.deltaTime * input.x * normalizer;
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.W)) {
            diagonal = true;
        }
        else {
            diagonal = false;
        }

        //Wallrunning logic
        if (isWall) {
            camZ = Mathf.SmoothDamp(camZ, wallrunCameraAngle * wallSide / (Mathf.Pow(runTimer, 3) / 3 + 1), ref reference0, cameraSmoothness);
            rot = Quaternion.FromToRotation(Vector3.right, contact * -wallSide);
            float reference1 = 0;
            if (wallSide != 0) transform.rotation = Quaternion.Slerp(transform.rotation, rot, cameraComes * Time.deltaTime);
            timer = 0;
            runTimer += Time.deltaTime;
        }
        else {
            camZ = Mathf.SmoothDamp(camZ, 0, ref reference0, cameraSmoothness / 1.3f);
            timer += Time.deltaTime;
            runTimer = 0;
        }
        if (timer > 1f) lastContact = Vector3.zero;
        if (wasWall && !isWall) wallExit();
        if (!wasWall && isWall) wallEnter();
        wasWall = isWall;
        camera.transform.localEulerAngles = new Vector3(0, 0, camZ);


        //rotate player and camera
        mouseY = Mathf.Clamp(mouseY, -70, 70);
        transform.eulerAngles += new Vector3(0, mouseX, 0);
        cameraParent.transform.rotation = Quaternion.Euler(-mouseY, transform.eulerAngles.y, 0f);

        //groundCheck logic
        isGrounded = groundCheck(isGrounded);
        if (isGrounded) {
            lastContact = Vector3.zero;
            secondJump = true;
            if (Input.GetKey(KeyCode.LeftShift)) actualSpeed = sprintSpeed;
            else actualSpeed = movementSpeed;

            float reference = 0f;
            actualDrag = Mathf.SmoothDamp(actualDrag, drag, ref reference, 0.01f);

            if (diagonal) normalizer = 0.707f;
            else normalizer = 1f;
        }
        else {
            if (!isWall) {
                normalizer = 1f;
                actualDrag = drag / airControl;
                actualSpeed = movementSpeed * airAccelerationMultiplier;
            }
            else {
                if (diagonal) normalizer = 0.707f;
                else normalizer = 1f;
                actualDrag = (drag / wallControl) * (runTimer / 5 + 0.6f);
                actualSpeed = movementSpeed * wallAccelerationMultiplier;
            }
        }
        isWall = false;
        player.Move(velocity * Time.deltaTime); //move player


    }

    // Update is called once per frame
    void FixedUpdate() {
        text.text = Mathf.Floor(Vector3.Magnitude(new Vector3(velocity.x, 0, velocity.z))).ToString();

        //Apply and calculate drag and gravity
        velocity.x = velocity.x * 1 / (actualDrag + 1);
        velocity.z = velocity.z * 1 / (actualDrag + 1);

        if (!isGrounded) {
            velocity.y -= actualGravity;
        }
        else {
            velocity.y = Mathf.Clamp(velocity.y, -gravity, 1000);
        }
    }


    //Check if player is grounded, called every frame
    bool groundCheck(bool isGrounded) {

        wasGrounded = isGrounded;
        isGrounded = player.isGrounded;
        if (isGrounded && !wasGrounded) {
            groundEnter();
        }
        else if (!isGrounded && wasGrounded) {
            groundExit();
        }

        return isGrounded;

    }

    //Called after player exits ground
    void groundExit() {
        print("exit");
    }

    //Called after player enters ground
    void groundEnter() {
        print("enter");
    }

    private void OnControllerColliderHit(ControllerColliderHit collision) {
        if (collision.normal.ToString("F3") != lastContact.ToString("F3")) {
            RaycastHit side;
            if (Physics.Raycast(transform.position, transform.right, out side, 4f)) {
                wallSide = 1;
            }
            else if (Physics.Raycast(transform.position, -transform.right, out side, 4f)) {
                wallSide = -1;
            }
            else {
                wallSide = 0;
            }

            if (!isGrounded && collision.normal.y < 0.1f && collision.normal.y > -0.1f) {
                isWall = true;
                contact = collision.normal;

                print(contact);
                float reference = 0f;
                actualGravity = Mathf.SmoothDamp(gravity / 10, gravity * Mathf.Pow(runTimer, 2) / (velocity.magnitude / 14 + 1), ref reference, 0.01f / velocity.magnitude);
                velocity.y = Mathf.Clamp(velocity.y, -actualGravity * 5, 20);
                velocity += contact * -15 / (velocity.magnitude / 2) * Time.deltaTime * 200;
                if (Input.GetKeyDown(KeyCode.Space)) {
                    velocity += Vector3.Scale(new Vector3(Mathf.Abs(velocity.x), Mathf.Abs(velocity.y), Mathf.Abs(velocity.z)), contact);
                    jumped = true;
                    velocity += transform.forward * input.z * 15 + transform.right * input.x * 12 + transform.up * 4 + contact * velocity.magnitude * 0.4f ;
                }
                else if (actualGravity > gravity) {
                    velocity += contact * 5;
                    jumped = true;
                }

            }
        }
    }

    //Called when player enters wall
    void wallEnter() {
        velocity += transform.up * 2;
        print("WallEnter");
        velocity.y = Mathf.Clamp(velocity.y, -gravity * 2, gravity * 2);
    }

    //Called when player exits wall
    void wallExit() {
        lastContact = contact;
        actualGravity = gravity;
        if (!jumped) {
            velocity += contact * velocity.magnitude;
        }
        secondJump = true;
        jumped = false;
    }
    void doubleJump() {
        velocity.y = Mathf.Clamp(velocity.y, -gravity, 1000);
        velocity += transform.forward * input.z * 15 + transform.right * input.x * 15 + transform.up * jumpForce;
        secondJump = false;
        print("double");
    }


}