using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class PlayerController : MonoBehaviour
{
    /*
        Variables, initializations etc.
        Character sprites have pivot anchored x: 0.5 and y: 0.1 (normalized)
        because of topdown angle
    */
    private float movementSpeed = 3f;
    private bool dash = false;

    [SerializeField] private LayerMask dashLayerMask; //layers for colision detection of dash


    private float treshold = 0.5f; //movement treshold (for handling mouse controls)
    private Vector3 moveDir;
    private Vector3 lookingDir;
    private Vector3 targetPos; //clicked position

    private Rigidbody2D rigidbody2d;
    private CharacterAnimationController characterAnimationController;
    private CharacterMovementController characterMovementController;


    /*
        Methods
    */
    
    // Start is called before the first frame update
    private void Start()
    {
        this.characterAnimationController = GetComponent<CharacterAnimationController>();
        this.characterMovementController = GetComponent<CharacterMovementController>();
        this.rigidbody2d = GetComponent<Rigidbody2D>();
        this.rigidbody2d.freezeRotation = true;
        gameObject.tag = "Player";
    }

    //every tick
    private void Update(){
        this.MoveMOUSE();
        this.characterMovementController.characterPathExpand();
        this.characterMovementController.characterMovementDetection();
    }

    // Fixed Update -> work with rigidbody here
    private void FixedUpdate()
    {
        rigidbody2d.velocity = moveDir * movementSpeed;
        //dash
        if(dash){
            Dash();
        }
    }

    /*
        Follow Mouse on hold.
        Gets mouse position and converts it into camera pixel points.

        *note:
        Pivot point is anchored on y: 0.1 and x: 0.5. So for better mouse
        control, considering middle of the model is anchor would be more
        advisible.
    */
    private void MoveMOUSE(){
        /* look direction*/
        var v = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1);
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(v);
        Vector3 playerPos = this.transform.position;
        playerPos.y += 0.4f; //pivot is normalized to y: 0.1 but for controls purpose consider as middle y: 0.5

        float x = mousePos.x - playerPos.x;
        float y = mousePos.y - playerPos.y;
        
        this.lookingDir = new Vector3(x,y).normalized;
        /*movement direction on hold*/
        if (Input.GetMouseButton(0)){
            targetPos = mousePos;
            targetPos.z = 0; //for some reason z is always set to -9

            x = mousePos.x - playerPos.x;
            y = mousePos.y - playerPos.y;
            
            //walk towards held mouse
            if (Vector3.Distance(playerPos, targetPos) >=treshold){
                this.moveDir = new Vector3(x,y).normalized;
                //animation
                characterAnimationController.CharacterMovement(moveDir);
                treshold = 0.5f;
            }else{
                this.moveDir = Vector3.zero;
                //animation
                characterAnimationController.CharacterDirection(lookingDir);
                treshold = 1f;
            }
            

        }else{ 
            /* move to last known held/clicked position */
            if (Vector3.Distance(playerPos, targetPos) >=treshold && characterMovementController.IsMoving){
                this.moveDir = new Vector3(targetPos.x-playerPos.x, targetPos.y - playerPos.y, 0f).normalized;
                characterAnimationController.CharacterMovement(moveDir);
            }else{
                this.moveDir = Vector3.zero;
                //idle animation
                characterAnimationController.CharacterDirection(lookingDir);
            }
        } 

        //dash roll
        if(Input.GetKeyDown(KeyCode.Space)){
            dash = true;
        } 
    }
    /*
        Dash function. Teleports RB in the mouse direction when. This function
        workis with physics so must be used within FixedUpdate(). Cast Raycast before actual
        dashing, to prevent going through the walls. If wall is detected, move character to 
        collided raycast point instead.
    */
    private void Dash(){
        float dashAmount = 5f;
        Vector3 dashDir = Vector3.zero;
        //player is not moving, so use lookingDir vector instead of moveDir
        if(Vector3.Distance(moveDir, Vector3.zero) == 0){
            dashDir = lookingDir;
        }else{
            dashDir = moveDir;
        }
        // dashPosition is position where player should land
        Vector3 dashPosition = transform.position + dashDir * dashAmount;

        RaycastHit2D raycast = Physics2D.Raycast(transform.position, dashDir, dashAmount, dashLayerMask);
        if (raycast.collider != null){
            dashPosition = raycast.point;
        }
        //Spawn visual effect here
        

        rigidbody2d.MovePosition(dashPosition);
        this.transform.position = dashPosition; //otherwise character will walk back to its last "pressed" position
        dash = false;
    }
}