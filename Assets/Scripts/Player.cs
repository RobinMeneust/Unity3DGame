using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform cam;
    public Planet planet;

    private float gravity=-9.8f;
    private float verticalAcceleration=0;
    private bool jumpRequest=false;
    private bool destroyRequest=false;

    private int playerRange=5;

    private float mouseH;
    private float mouseV;
    private float horizontal;
    private float vertical;
    private float mouseSensibility=1;
    private Vector3 velocityTemp=Vector3.zero;
    private Vector3 velocityRelativeToFace=Vector3.zero;
    private bool isOnGround;
    private float playerWidthRadius=0.15f;
    private float playerHeight=1.8f;
    private float jumpImpulse=4f;
    private Vector3 lastGravityDirection=Vector3.down;
    //private Vector3 gravityDirection=Vector3.down;
    private int facePlanet=0;
    private int lastFacePlanet=0;

    private float walkSpeed=1.3f;
    private float sprintSpeed=2.5f;
    private bool isSprinting=false;

    private bool isLookingAtBlock=false;

    Vector3 forwardVectorPlayer=Vector3.forward;
    Vector3 backwardVectorPlayer=Vector3.back;
    Vector3 upVectorPlayer=Vector3.up;
    Vector3 downVectorPlayer=Vector3.down;
    Vector3 rightVectorPlayer=Vector3.right;
    Vector3 leftVectorPlayer=Vector3.left;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        isOnGround = true;
        facePlanet = planet.checkPlanetFacePlayer(transform.position);
    }

    private void FixedUpdate() 
    {
        Vector3 pos;
        facePlanet = planet.checkPlanetFacePlayer(transform.position);
        if(facePlanet!=lastFacePlanet){//changing face
            Debug.Log("old face=  "+lastFacePlanet+"      new face=  "+facePlanet);
            //gravityDirection = getGravityDirectionFromFace(facePlanet, gravityDirection);
            //transform.Rotate(Vector3.RotateTowards(lastGravityDirection, gravityDirection, 1.8f, 0.0f));
            
            changePlayerVectors();
            lastFacePlanet=facePlanet;
        }

        isLookingAtBlock=GetAimedBlock(out pos);
        if(isLookingAtBlock){
            planet.highLightCube.transform.position = pos + new Vector3(0.5f,0.5f,0.5f);
            planet.highLightCube.SetActive(true);
        }
        else
            planet.highLightCube.SetActive(false);

        GetVelocity();
        if(inside()){
            //transform.Translate(upVectorPlayer, Space.World);
            Debug.Log("IS INSIDE A BLOCK");
        }
        if(jumpRequest)
            Jump();

        if(destroyRequest){
            if(isLookingAtBlock){
                planet.DestroyVoxel(pos);
                planet.highLightCube.SetActive(false);
            }
            destroyRequest=false;
        }
    }

    private void changePlayerVectors()
    {
        switch(facePlanet){
            case 0: {
                forwardVectorPlayer=Vector3.forward;
                backwardVectorPlayer=Vector3.back;
                upVectorPlayer=Vector3.left;
                downVectorPlayer=Vector3.right;
                rightVectorPlayer=Vector3.up;
                leftVectorPlayer=Vector3.down;
                break;
            }
            case 1: {
                forwardVectorPlayer=Vector3.forward;
                backwardVectorPlayer=Vector3.back;
                upVectorPlayer=Vector3.right;
                downVectorPlayer=Vector3.left;
                rightVectorPlayer=Vector3.up;
                leftVectorPlayer=Vector3.down;
                break;
            }
            case 2: {
                forwardVectorPlayer=Vector3.forward;
                backwardVectorPlayer=Vector3.back;
                upVectorPlayer=Vector3.down;
                downVectorPlayer=Vector3.up;
                rightVectorPlayer=Vector3.left;
                leftVectorPlayer=Vector3.right;
                break;
            }
            case 3: {
                forwardVectorPlayer=Vector3.forward;
                backwardVectorPlayer=Vector3.back;
                upVectorPlayer=Vector3.up;
                downVectorPlayer=Vector3.down;
                rightVectorPlayer=Vector3.right;
                leftVectorPlayer=Vector3.left;
                break;
            }
            case 4: {
                forwardVectorPlayer=Vector3.up;
                backwardVectorPlayer=Vector3.down;
                upVectorPlayer=Vector3.back;
                downVectorPlayer=Vector3.forward;
                rightVectorPlayer=Vector3.right;
                leftVectorPlayer=Vector3.left;
                break;
            }
            case 5: {
                forwardVectorPlayer=Vector3.down;
                backwardVectorPlayer=Vector3.up;
                upVectorPlayer=Vector3.forward;
                downVectorPlayer=Vector3.back;
                rightVectorPlayer=Vector3.right;
                leftVectorPlayer=Vector3.left;
                break;
            }
            default : Debug.Log("ERROR : (in changePlayerVectors) Face value incorrect"); break;
        }
    }

    private void Update() {
        GetPlayerInputs();
        cam.transform.position=transform.position + upVectorPlayer*1.6f;
        transform.Rotate(Vector3.up*mouseH*mouseSensibility);
        cam.transform.Rotate(-Vector3.right*mouseV*mouseSensibility);
        transform.Translate(velocityRelativeToFace.x*rightVectorPlayer + velocityRelativeToFace.y*upVectorPlayer + velocityRelativeToFace.z*forwardVectorPlayer, Space.World);
        //Debug.Log("velocity   =  " + velocityRelativeToFace.y*upVectorPlayer + "      relative=  " + velocityRelativeToFace.y + "      upVector=  " + upVectorPlayer);

        Vector3 newUp=upVectorPlayer;
        Vector3 forward = transform.forward;
        Vector3 left = Vector3.Cross (forward,newUp);
        Vector3 newForward = Vector3.Cross (newUp,left);
        Quaternion oldRotation=transform.rotation;
        Quaternion newRotation = Quaternion.LookRotation (newForward, newUp);
        transform.rotation = Quaternion.Lerp(oldRotation, newRotation, 0.1f);

        //cam.transform.rotation = transform.rotation;

        //if(cam.transform.rotation)
    }

    private bool GetAimedBlock(out Vector3 pos)
    {
        float step=0.5f;
        float distanceToBlock=0f;

        while(distanceToBlock<playerRange){
            pos=cam.position+(cam.forward*distanceToBlock);
            if(planet.CheckVoxelMap(pos)){
                pos.x=Mathf.FloorToInt(pos.x);
                pos.y=Mathf.FloorToInt(pos.y);
                pos.z=Mathf.FloorToInt(pos.z);
                return true;
            }
            distanceToBlock+=step;
        }
        pos= new Vector3(0,0,0);
        return false;
    }

    private void GetPlayerInputs()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseH = Input.GetAxis("Mouse X");
        mouseV = Input.GetAxis("Mouse Y");
        if(isOnGround && Input.GetButtonDown("Jump"))
        {
            jumpRequest=true;
        }
        if(Input.GetButtonDown("Sprint")){
            Debug.Log("SPRINT");
            isSprinting=!isSprinting;
        }

        if(Input.GetButtonDown("Destroy")){
            Debug.Log("Destroy");
            destroyRequest=true;
        }
    }

    public void Jump(){
        Debug.Log("JUMP");
        verticalAcceleration = jumpImpulse;
        jumpRequest=false;
        isOnGround=false;
    }

    private void GetVelocity()
    {
        if(verticalAcceleration>gravity)
            verticalAcceleration+=Time.fixedDeltaTime*gravity;

        if(isSprinting)
            velocityTemp = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime*sprintSpeed;
        else
            velocityTemp = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime*walkSpeed;

        //if((Vector3.Distance(transform.position, planet.planetCoreCoord)-planet.planetRadiusInBlocks)<playerHeight){
        velocityRelativeToFace = new Vector3(Vector3.Dot(velocityTemp, rightVectorPlayer), Vector3.Dot(velocityTemp, upVectorPlayer), Vector3.Dot(velocityTemp, forwardVectorPlayer));
        //velocityRelativeToFace = Vector3.ProjectOnPlane(velocityTemp, upVectorPlayer);
        velocityRelativeToFace += Vector3.up*verticalAcceleration*Time.fixedDeltaTime;

        if(velocityRelativeToFace.y>0 && top(velocityRelativeToFace.y)){
            velocityRelativeToFace.y=0;
        }
        if(velocityRelativeToFace.y<0 && bot(velocityRelativeToFace.y)){
            velocityRelativeToFace.y=0;
        }
        if((left() && velocityRelativeToFace.x<0) || (right() && velocityRelativeToFace.x>0)){
            velocityRelativeToFace.x=0;
        }
        if((back() && velocityRelativeToFace.z<0) || (front() && velocityRelativeToFace.z>0)){
            velocityRelativeToFace.z=0;
        }
    }

    private Vector3 getGravityDirectionFromFace(int facePlanet, Vector3 gravityDirection)
    {
        switch(facePlanet){ // It's reversed because we go from the face to the center and not the opposite
            case 0: return Vector3.left;
            case 1: return Vector3.right;
            case 2: return Vector3.down;
            case 3: return Vector3.up;
            case 4: return Vector3.back;
            case 5: return Vector3.forward;
            default : Debug.Log("ERROR : (in getGravityDirectionFromFace) Incorrect face value"); return gravityDirection; //use last one
        }
    }

    private bool top(float verticalSpeed){
        if(
            planet.CheckVoxelMap(upVectorPlayer*playerHeight + transform.position + forwardVectorPlayer*playerWidthRadius + verticalSpeed*upVectorPlayer) || 
            planet.CheckVoxelMap(upVectorPlayer*playerHeight + transform.position + backwardVectorPlayer*playerWidthRadius + verticalSpeed*upVectorPlayer) ||
            planet.CheckVoxelMap(upVectorPlayer*playerHeight + transform.position + rightVectorPlayer*playerWidthRadius + verticalSpeed*upVectorPlayer) ||
            planet.CheckVoxelMap(upVectorPlayer*playerHeight + transform.position + leftVectorPlayer*playerWidthRadius + verticalSpeed*upVectorPlayer)
        )
        {
            //Debug.Log("TOP");
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool bot(float verticalSpeed){
        if(
            planet.CheckVoxelMap(downVectorPlayer*0.2f + transform.position + forwardVectorPlayer*playerWidthRadius + verticalSpeed*upVectorPlayer) || 
            planet.CheckVoxelMap(downVectorPlayer*0.2f + transform.position + backwardVectorPlayer*playerWidthRadius + verticalSpeed*upVectorPlayer) ||
            planet.CheckVoxelMap(downVectorPlayer*0.2f + transform.position + rightVectorPlayer*playerWidthRadius + verticalSpeed*upVectorPlayer) ||
            planet.CheckVoxelMap(downVectorPlayer*0.2f + transform.position + leftVectorPlayer*playerWidthRadius + verticalSpeed*upVectorPlayer)
        )
        {
            isOnGround=true;
            return true;
        }
        else
        {
            isOnGround=false;
            return false;
        }
    }

    private bool front(){
        //Debug.Log("FRONT : " + (transform.position + transform.up + transform.forward*playerWidthRadius) + "    |   pos:    " + transform.position);
        //Debug.Log("FRONT    pos_player=  " + transform.position + "     block=  " + planet.getVoxel(transform.position + forwardVectorPlayer*playerWidthRadius) + "velocityRelativeToFace=  " + velocityRelativeToFace.z);
        //Debug.Log("FRONT    velocityRelativeToFace=  " + velocityRelativeToFace.z + "         velocityTemp=  " + velocityTemp.z);
        if(
            planet.CheckVoxelMap(transform.position + forwardVectorPlayer*playerWidthRadius) || 
            planet.CheckVoxelMap(transform.position + forwardVectorPlayer*playerWidthRadius+upVectorPlayer)
        )
        {
            
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool back(){
        if(
            planet.CheckVoxelMap(transform.position + backwardVectorPlayer*playerWidthRadius) || 
            planet.CheckVoxelMap(transform.position + backwardVectorPlayer*playerWidthRadius + upVectorPlayer)
        )
        {
            //Debug.Log("BACK");
            return true;
        }
        else
        {
            return false;
        }
}

    private bool right(){
        if(
            planet.CheckVoxelMap(transform.position + rightVectorPlayer*playerWidthRadius) || 
            planet.CheckVoxelMap(transform.position + rightVectorPlayer*playerWidthRadius + upVectorPlayer)
        )
        {
            //Debug.Log("RIGHT");
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool left(){
        if(
            planet.CheckVoxelMap(transform.position + leftVectorPlayer*playerWidthRadius) || 
            planet.CheckVoxelMap(transform.position + leftVectorPlayer*playerWidthRadius + upVectorPlayer)
        )
        {
            //Debug.Log("LEFT");
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool inside(){
        if(
            planet.CheckVoxelMap(transform.position) || 
            planet.CheckVoxelMap(transform.position + upVectorPlayer)
        )
        {
            //Debug.Log("LEFT");
            return true;
        }
        else
        {
            return false;
        }
    }
}
