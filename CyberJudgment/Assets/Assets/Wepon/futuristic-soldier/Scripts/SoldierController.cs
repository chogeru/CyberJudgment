using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierController : MonoBehaviour
{

    [HideInInspector]
    public bool alive = true;
    Animator anim;
    CharacterController characterController;
    public Camera camera;
    
    public float speed = 300f;
    float vertical = 0f;
    float horizontal = 0f;

    float gravity = 0.5f;

    [HideInInspector]
    public bool rolling = false;
    [HideInInspector]
    public bool reloading = false;


    void Start(){
        anim = GetComponentInChildren<Animator>();
        characterController = GetComponent<CharacterController>();
        speed *= Scale();
    }

    public float Scale(){
        return transform.localScale.y;
    }

    void Update(){
        if(alive){
            vertical = Input.GetAxis("Vertical");
            horizontal = Input.GetAxis("Horizontal");
            
            float x = DirectionX(horizontal);
            float z = DirectionZ(vertical);

            anim.SetFloat("z", z);    
            anim.SetFloat("x", x);

            AdjustAim();
            Roll();
            Reload();
            
            Vector3 movement = new Vector3(horizontal, 0, vertical);
            movement = CalculateMovement(movement, x , z);
            characterController.Move(movement);
            RotateCharacter(movement);
        }else{
            anim.Play("death",0);
            anim.Play("death",1);
        }
    }

    void Reload(){
        if(Input.GetKeyDown("r")){
            reloading = true; 
        }
        if(reloading){
            anim.Play("reload",1);
        }else{
            anim.Play("movement",1);
        }
    }

    void Roll(){
        if(Input.GetButtonDown("Jump")){
            rolling = true;
            AdjustRollDirection();
        }
        if(rolling){
            anim.Play("roll");
        }else{
            anim.Play("movement");
        }
    }

    float DirectionZ(float input){
        float z = 0;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)){
            z = Math.Abs(input);
        }else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)){
            z = -1.0f * Math.Abs(input);
        }else{
            z = input;
        }
        return z;
    }

    float DirectionX(float input){
        float x = 0;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)){
            x = -1.0f * Math.Abs(input);
        }else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)){
            x = Math.Abs(input);
        }else{
            x = input;
            
        }
        return x;
    }

    void RotateCharacter(Vector3 movement){
        if(!rolling){
            if(!(movement.x == 0 && movement.z == 0 && !Input.GetMouseButton(0))){
                transform.rotation = Quaternion.Euler(0,Camera.main.transform.eulerAngles.y,0);
            }
        }

    }

    void AdjustAim(){
        float angle =camera.transform.eulerAngles.x;
        if(camera.transform.eulerAngles.x >280){
            angle = camera.transform.eulerAngles.x - 360;
        }
        anim.SetFloat("angle", angle);
    }

    Vector3 CalculateMovement(Vector3 movement, float x, float z){
        movement = Quaternion.Euler( 0, camera.transform.eulerAngles.y, 0) * movement;
        float movementSpeed = speed;
        if(rolling){
            movementSpeed = speed * 1.5f;
            movement = transform.forward * Time.deltaTime * movementSpeed;
        }else{
            if(z == 1 && x ==0){
                movementSpeed = speed;
            }else{
                movementSpeed = (speed/3);
            }
            movement = movement * Time.deltaTime * movementSpeed;
        }
        movement.y -= gravity;
        return movement;
    }

    void AdjustRollDirection(){
        if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)){ 
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, camera.transform.rotation.eulerAngles.y-90, transform.rotation.eulerAngles.z);
        }else if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)){ 
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, camera.transform.rotation.eulerAngles.y+90, transform.rotation.eulerAngles.z);
        }else if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)){ 
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, camera.transform.rotation.eulerAngles.y+180, transform.rotation.eulerAngles.z);
        }else{
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, camera.transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        }
    }

    public void Die(){
        alive=false;
    }

}
