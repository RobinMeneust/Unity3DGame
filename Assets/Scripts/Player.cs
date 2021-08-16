using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform cam;
    public Planet planet;
    public GameObject debugMenu;

    private float gravity=-9.8f;
    private bool jumpRequest=false;
    private bool destroyRequest=false;

    private int playerRange=5;

    private float mouseH;
    private float mouseV;
    private float horizontal;
    private float vertical;
    private float mouseSensibility=350;
    private Vector3 velocityTemp=Vector3.zero;
    private Vector3 velocityRelativeToFace=Vector3.zero;
    private Vector3 accelerationVectorRelativeToFace=Vector3.zero;
    private bool isOnGround;
    private float playerWidthRadius=0.15f;
    private float playerHeight=1.8f;
    private float playerHeigthWhenLyingDown=0.8f;
    private float currentPlayerHeight=1.8f;
    private float jumpImpulse=5f;
    private float maxFreeFallSpeed=100f;
    private Vector3 gravityVector;
    private int facePlanet=0;
    private int lastFacePlanet=0;
    private bool isLyingDown=false;

    private float walkSpeed=4f;
    private float sprintSpeed=8f;
    private bool isSprinting=false;
    private Vector3 aimedBlockPos;

    private bool isLookingAtBlock=false;

    private Vector3 forwardVectorPlayer=Vector3.forward;
    private Vector3 backwardVectorPlayer=Vector3.back;
    private Vector3 upVectorPlayer=Vector3.up;
    private Vector3 downVectorPlayer=Vector3.down;
    private Vector3 rightVectorPlayer=Vector3.right;
    private Vector3 leftVectorPlayer=Vector3.left;

    private Vector3 forwardVectorPlayerLocal=Vector3.forward;
    private Vector3 backwardVectorPlayerLocal=Vector3.back;
    private Vector3 upVectorPlayerLocal=Vector3.up;
    private Vector3 downVectorPlayerLocal=Vector3.down;
    private Vector3 rightVectorPlayerLocal=Vector3.right;
    private Vector3 leftVectorPlayerLocal=Vector3.left;

    private InventorySlots[] inventory = new InventorySlots[20];

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        isOnGround = true;
        facePlanet = planet.checkPlanetFacePlayer(transform.localPosition);
        cam.position=transform.position + upVectorPlayer*currentPlayerHeight;
    }

    private void FixedUpdate() 
    {
        facePlanet = planet.checkPlanetFacePlayer(transform.localPosition);
        if(facePlanet!=lastFacePlanet){//changing face
            //Debug.Log("old face=  "+lastFacePlanet+"      new face=  "+facePlanet);
            //gravityDirection = getGravityDirectionFromFace(facePlanet, gravityDirection);
            //transform.Rotate(Vector3.RotateTowards(lastGravityDirection, gravityDirection, 1.8f, 0.0f));
            
            
            lastFacePlanet=facePlanet;
        }
        changePlayerVectorsLocal();
        changePlayerVectors();
        gravityVector = Vector3.Normalize(transform.position-planet.planetCoreCoord)*gravity;
        GetVelocity();

        cam.position=transform.position + upVectorPlayer*currentPlayerHeight;
        Vector3 newUp=upVectorPlayer;
        Vector3 forward = transform.forward;
        Vector3 left = Vector3.Cross (forward,newUp);
        Vector3 newForward = Vector3.Cross (newUp,left);
        Quaternion oldRotation=transform.rotation;
        Quaternion newRotation = Quaternion.LookRotation (newForward, newUp);
        transform.rotation = Quaternion.Lerp(oldRotation, newRotation, 0.1f);

        
        transform.Translate(velocityRelativeToFace.x*rightVectorPlayer + velocityRelativeToFace.y*upVectorPlayer + velocityRelativeToFace.z*forwardVectorPlayer, Space.World);
    }

    private void Update() {
        GetPlayerInputs();
        
        isLookingAtBlock=GetAimedBlock(out aimedBlockPos);
        if(isLookingAtBlock){
            planet.highLightCube.transform.localPosition = aimedBlockPos + new Vector3(0.5f,0.5f,0.5f);
            planet.highLightCube.SetActive(true);
        }
        else
            planet.highLightCube.SetActive(false);

        if(inside()){
            //transform.Translate(upVectorPlayer, Space.World);
            //Debug.LogError("IS INSIDE A BLOCK");
        }

        if(destroyRequest){
            if(isLookingAtBlock){
                planet.DestroyVoxel(aimedBlockPos);
                planet.highLightCube.SetActive(false);
            }
            destroyRequest=false;
        }

        transform.Rotate(Vector3.up*mouseH*mouseSensibility*Time.deltaTime);
        cam.Rotate(-Vector3.right*mouseV*mouseSensibility*Time.deltaTime);
        /*
        else if(camAngle>90)
            cam.Rotate(Vector3.right*mouseSensibility*Time.deltaTime);
        else
            cam.Rotate(-Vector3.right*mouseSensibility*Time.deltaTime);
        */
        //Debug.Log("Angle=  "+camAngle);
        //Debug.Log("cam right=  "+cam.right+"     player right=  "+transform.right);
        /*
        if(camAngle>90){
            cam.forward=upVectorPlayer;
            cam.right=transform.right;
        }
        else if(camAngle<-90){
            cam.forward=downVectorPlayer;
            cam.right=transform.right;
        }
        */
    }

    


    private bool GetAimedBlock(out Vector3 pos)
    {
        float step=0.5f;
        float distanceToBlock=0f;
        Vector3 camForwardRelativeToPlanet;

        while(distanceToBlock<playerRange){
            camForwardRelativeToPlanet = new Vector3(Vector3.Dot(cam.forward, planet.transform.right), Vector3.Dot(cam.forward, planet.transform.up), Vector3.Dot(cam.forward, planet.transform.forward));
            pos=transform.localPosition+currentPlayerHeight*upVectorPlayerLocal+(camForwardRelativeToPlanet*distanceToBlock);
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

        float camAngle;
        camAngle=Vector3.SignedAngle(transform.forward,cam.forward, -transform.right);
        //Debug.Log("Angle=  "+camAngle);
        //Debug.Log("V=  "+mouseV);

        if(camAngle>=90 && mouseV>0)
            mouseV=0;
        else if(camAngle<=-90 && mouseV<0)
            mouseV=0;
        
        if(!isLyingDown && isOnGround && Input.GetButtonDown("Jump"))
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
        if(canStand() && isOnGround && Input.GetButtonDown("Prone"))
        {
            isLyingDown=!isLyingDown;
            if(isLyingDown)
                currentPlayerHeight=playerHeigthWhenLyingDown;
            else
                currentPlayerHeight=playerHeight;
        }

        if(Input.GetButtonDown("Debug Menu"))
        {
            debugMenu.SetActive(!debugMenu.activeSelf);
        }
    }

    public void Jump(){
        Debug.Log("JUMP");
        jumpRequest=false;
        isOnGround=false;
        accelerationVectorRelativeToFace.y += jumpImpulse;
    }

    private void GetVelocity()
    {
        if(jumpRequest)
            Jump();

        if(isSprinting)
            velocityTemp = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime*sprintSpeed;
        else
            velocityTemp = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime*walkSpeed;

        //if((Vector3.Distance(transform.localPosition, planet.planetCoreCoord)-planet.planetRadiusInBlocks)<playerHeight){
        if(velocityRelativeToFace.y<maxFreeFallSpeed){
            if(isOnGround)
                accelerationVectorRelativeToFace+= new Vector3(0, Vector3.Dot(Time.fixedDeltaTime*gravityVector, upVectorPlayer), 0);
            else
                accelerationVectorRelativeToFace+= new Vector3(Vector3.Dot(Time.fixedDeltaTime*gravityVector, rightVectorPlayer), Vector3.Dot(Time.fixedDeltaTime*gravityVector, upVectorPlayer), Vector3.Dot(Time.fixedDeltaTime*gravityVector, forwardVectorPlayer));;
        }
            

        velocityRelativeToFace = new Vector3(Vector3.Dot(velocityTemp, rightVectorPlayer), Vector3.Dot(velocityTemp, upVectorPlayer), Vector3.Dot(velocityTemp, forwardVectorPlayer));
        velocityRelativeToFace+=accelerationVectorRelativeToFace*Time.fixedDeltaTime;

        
        //velocityRelativeToFace = Vector3.ProjectOnPlane(velocityTemp, upVectorPlayer);

        if(velocityRelativeToFace.y>0 && top(velocityRelativeToFace.y)){
            velocityRelativeToFace.y=0;
            accelerationVectorRelativeToFace=Vector3.zero;
        }
        if(velocityRelativeToFace.y<0 && bot(velocityRelativeToFace.y)){
            velocityRelativeToFace.y=0;
            accelerationVectorRelativeToFace=Vector3.zero;
        }
        if((left() && velocityRelativeToFace.x<0) || (right() && velocityRelativeToFace.x>0)){
            velocityRelativeToFace.x=0;
        }
        if((back() && velocityRelativeToFace.z<0) || (front() && velocityRelativeToFace.z>0)){
            velocityRelativeToFace.z=0;
        }
    }
/*
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
*/
    private bool top(float verticalSpeed){
        float heightCheck=currentPlayerHeight+0.2f;

        if(
            planet.CheckVoxelMap(upVectorPlayerLocal*heightCheck + transform.localPosition + (forwardVectorPlayerLocal+leftVectorPlayerLocal)*playerWidthRadius) || 
            planet.CheckVoxelMap(upVectorPlayerLocal*heightCheck + transform.localPosition + (forwardVectorPlayerLocal+rightVectorPlayerLocal)*playerWidthRadius) ||
            planet.CheckVoxelMap(upVectorPlayerLocal*heightCheck + transform.localPosition + (backwardVectorPlayerLocal+leftVectorPlayerLocal)*playerWidthRadius) ||
            planet.CheckVoxelMap(upVectorPlayerLocal*heightCheck + transform.localPosition + (backwardVectorPlayerLocal+rightVectorPlayerLocal)*playerWidthRadius)
        )
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool bot(float verticalSpeed){
        if(
            planet.CheckVoxelMap(transform.localPosition + (forwardVectorPlayerLocal+leftVectorPlayerLocal)*playerWidthRadius + verticalSpeed*upVectorPlayerLocal) || 
            planet.CheckVoxelMap(transform.localPosition + (forwardVectorPlayerLocal+rightVectorPlayerLocal)*playerWidthRadius + verticalSpeed*upVectorPlayerLocal) ||
            planet.CheckVoxelMap(transform.localPosition + (backwardVectorPlayerLocal+leftVectorPlayerLocal)*playerWidthRadius + verticalSpeed*upVectorPlayerLocal) ||
            planet.CheckVoxelMap(transform.localPosition + (backwardVectorPlayerLocal+rightVectorPlayerLocal)*playerWidthRadius + verticalSpeed*upVectorPlayerLocal)
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
        bool condition=true;
        if(isLyingDown)
            condition=planet.CheckVoxelMap(transform.localPosition + forwardVectorPlayerLocal*playerWidthRadius);
        else
            condition=planet.CheckVoxelMap(transform.localPosition + forwardVectorPlayerLocal*playerWidthRadius) || planet.CheckVoxelMap(transform.localPosition + forwardVectorPlayerLocal*playerWidthRadius+upVectorPlayerLocal);

        if(condition)
        {
            //Debug.Log("front");
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool back(){
        bool condition=true;
        if(isLyingDown)
            condition=planet.CheckVoxelMap(transform.localPosition + backwardVectorPlayerLocal*playerWidthRadius);
        else
            condition=planet.CheckVoxelMap(transform.localPosition + backwardVectorPlayerLocal*playerWidthRadius) || planet.CheckVoxelMap(transform.localPosition + backwardVectorPlayerLocal*playerWidthRadius+upVectorPlayerLocal);

        if(condition)
        {
            //Debug.Log("back");
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool right(){
        bool condition=true;
        if(isLyingDown)
            condition=planet.CheckVoxelMap(transform.localPosition + rightVectorPlayerLocal*playerWidthRadius);
        else
            condition=planet.CheckVoxelMap(transform.localPosition + rightVectorPlayerLocal*playerWidthRadius) || planet.CheckVoxelMap(transform.localPosition + rightVectorPlayerLocal*playerWidthRadius+upVectorPlayerLocal);

        if(condition)
        {
            //Debug.Log("right | 0-bool: "+planet.CheckVoxelMap(transform.localPosition + rightVectorPlayerLocal*playerWidthRadius)+"    0-type:  "+planet.CheckVoxelTypeIndex(transform.localPosition + rightVectorPlayerLocal*playerWidthRadius));
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool left(){
        bool condition=true;
        if(isLyingDown)
            condition=planet.CheckVoxelMap(transform.localPosition + leftVectorPlayerLocal*playerWidthRadius);
        else
            condition=planet.CheckVoxelMap(transform.localPosition + leftVectorPlayerLocal*playerWidthRadius) || planet.CheckVoxelMap(transform.localPosition + leftVectorPlayerLocal*playerWidthRadius+upVectorPlayerLocal);

        if(condition)
        {
            //Debug.Log("left");
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool inside(){
        bool condition=true;
        if(isLyingDown)
            condition=planet.CheckVoxelMap(transform.localPosition);
        else
            condition=planet.CheckVoxelMap(transform.localPosition) || planet.CheckVoxelMap(transform.localPosition + upVectorPlayerLocal);

        if(condition)
        {
            //Debug.Log("inside");
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool canStand()
    {
        if(planet.CheckVoxelMap(transform.localPosition + upVectorPlayerLocal))
        {
            isLyingDown=true;
            currentPlayerHeight=playerHeigthWhenLyingDown;
            return false;
        }
        else
        {
            return true;
        }
    }

    private void changePlayerVectors()
    {
        switch(facePlanet){
            case 0: {
                forwardVectorPlayer=planet.transform.forward;
                backwardVectorPlayer=-planet.transform.forward;
                upVectorPlayer=-planet.transform.right;
                downVectorPlayer=planet.transform.right;
                rightVectorPlayer=planet.transform.up;
                leftVectorPlayer=-planet.transform.up;
                break;
            }
            case 1: {
                forwardVectorPlayer=planet.transform.forward;
                backwardVectorPlayer=-planet.transform.forward;
                upVectorPlayer=planet.transform.right;
                downVectorPlayer=-planet.transform.right;
                rightVectorPlayer=planet.transform.up;
                leftVectorPlayer=-planet.transform.up;
                break;
            }
            case 2: {
                forwardVectorPlayer=planet.transform.forward;
                backwardVectorPlayer=-planet.transform.forward;
                upVectorPlayer=-planet.transform.up;
                downVectorPlayer=planet.transform.up;
                rightVectorPlayer=-planet.transform.right;
                leftVectorPlayer=planet.transform.right;
                break;
            }
            case 3: {
                forwardVectorPlayer=planet.transform.forward;
                backwardVectorPlayer=-planet.transform.forward;
                upVectorPlayer=planet.transform.up;
                downVectorPlayer=-planet.transform.up;
                rightVectorPlayer=planet.transform.right;
                leftVectorPlayer=-planet.transform.right;
                break;
            }
            case 4: {
                forwardVectorPlayer=planet.transform.up;
                backwardVectorPlayer=-planet.transform.up;
                upVectorPlayer=-planet.transform.forward;
                downVectorPlayer=planet.transform.forward;
                rightVectorPlayer=planet.transform.right;
                leftVectorPlayer=-planet.transform.right;
                break;
            }
            case 5: {
                forwardVectorPlayer=-planet.transform.up;
                backwardVectorPlayer=planet.transform.up;
                upVectorPlayer=planet.transform.forward;
                downVectorPlayer=-planet.transform.forward;
                rightVectorPlayer=planet.transform.right;
                leftVectorPlayer=-planet.transform.right;
                break;
            }
            default : Debug.Log("ERROR : (in changePlayerVectors) Face value incorrect"); break;
        }
    }

    private void changePlayerVectorsLocal()
    {
        switch(facePlanet){
            case 0: {
                forwardVectorPlayerLocal=Vector3.forward;
                backwardVectorPlayerLocal=Vector3.back;
                upVectorPlayerLocal=Vector3.left;
                downVectorPlayerLocal=Vector3.right;
                rightVectorPlayerLocal=Vector3.up;
                leftVectorPlayerLocal=Vector3.down;
                break;
            }
            case 1: {
                forwardVectorPlayerLocal=Vector3.forward;
                backwardVectorPlayerLocal=Vector3.back;
                upVectorPlayerLocal=Vector3.right;
                downVectorPlayerLocal=Vector3.left;
                rightVectorPlayerLocal=Vector3.up;
                leftVectorPlayerLocal=Vector3.down;
                break;
            }
            case 2: {
                forwardVectorPlayerLocal=Vector3.forward;
                backwardVectorPlayerLocal=Vector3.back;
                upVectorPlayerLocal=Vector3.down;
                downVectorPlayerLocal=Vector3.up;
                rightVectorPlayerLocal=Vector3.left;
                leftVectorPlayerLocal=Vector3.right;
                break;
            }
            case 3: {
                forwardVectorPlayerLocal=Vector3.forward;
                backwardVectorPlayerLocal=Vector3.back;
                upVectorPlayerLocal=Vector3.up;
                downVectorPlayerLocal=Vector3.down;
                rightVectorPlayerLocal=Vector3.right;
                leftVectorPlayerLocal=Vector3.left;
                break;
            }
            case 4: {
                forwardVectorPlayerLocal=Vector3.up;
                backwardVectorPlayerLocal=Vector3.down;
                upVectorPlayerLocal=Vector3.back;
                downVectorPlayerLocal=Vector3.forward;
                rightVectorPlayerLocal=Vector3.right;
                leftVectorPlayerLocal=Vector3.left;
                break;
            }
            case 5: {
                forwardVectorPlayerLocal=Vector3.down;
                backwardVectorPlayerLocal=Vector3.up;
                upVectorPlayerLocal=Vector3.forward;
                downVectorPlayerLocal=Vector3.back;
                rightVectorPlayerLocal=Vector3.right;
                leftVectorPlayerLocal=Vector3.left;
                break;
            }
            default : Debug.Log("ERROR : (in changePlayerVectorsLocal) Face value incorrect"); break;
        }
    }
}

public class InventorySlots
{
    private int m_itemID;
    private int m_itemAmount;
    private string m_itemName;
    private Sprite m_icon;

    public InventorySlots(int itemID=0, int itemAmount=0)
    {
        m_itemID=itemID;
        m_itemAmount=itemAmount;
        m_itemName="Name";
    }
}