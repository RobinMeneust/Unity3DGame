using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    [System.NonSerialized]
    public int planetGroundRadiusInChunks = 4;
    [System.NonSerialized]
    public int planetMaxHeightRadiusInBlocks = 4;
    [System.NonSerialized]
    public int planetTotalPlanetRadiusInChunks;
    [System.NonSerialized]
    public int planetTotalPlanetRadiusInBlocks;
    [System.NonSerialized]
    public int planetTotalPlanetDiameterInChunks;
    [System.NonSerialized]
    public int planetTotalPlanetDiameterInBlocks;
    [System.NonSerialized]
    public int planetGroundDiameterInChunks;
    [System.NonSerialized]
    public int planetGroundDiameterInBlocks;
    [System.NonSerialized]
    public int planetGroundRadiusInBlocks;
    [System.NonSerialized]
    public Vector3 planetCoreCoord;
    
    private Chunk[,,] chunks;
    public Material material;
    public Player player;
    public GameObject highLightCube;
    private Vector3 spawn;
    public int seed;
    private List<intVector3> activeChunks = new List<intVector3>();
    private List<intVector3> chunksToCreate = new List<intVector3>();
    private int viewDistance=2;
    public BlockTypes[] blockTypes;
    private intVector3 lastPlayerChunkPos;
    private intVector3 playerChunkPos;
    private bool isCreatingChunks=false;

    // Start is called before the first frame update

    void Awake() 
    {
        planetGroundRadiusInBlocks=planetGroundRadiusInChunks*Data.chunkWidth;
        planetTotalPlanetRadiusInChunks=planetGroundRadiusInChunks+(planetMaxHeightRadiusInBlocks/Data.chunkWidth)+1;
        planetGroundDiameterInChunks=planetGroundRadiusInChunks*2;
        planetTotalPlanetDiameterInChunks=planetTotalPlanetRadiusInChunks*2;
        planetGroundDiameterInBlocks=planetGroundRadiusInBlocks*2;
        planetTotalPlanetRadiusInBlocks=planetTotalPlanetRadiusInChunks*Data.chunkWidth;
        planetTotalPlanetDiameterInBlocks=planetTotalPlanetRadiusInBlocks*2;
        planetCoreCoord = new Vector3(planetTotalPlanetRadiusInBlocks, planetTotalPlanetRadiusInBlocks, planetTotalPlanetRadiusInBlocks);
        chunks = new Chunk[planetTotalPlanetDiameterInChunks, planetTotalPlanetDiameterInChunks, planetTotalPlanetDiameterInChunks];

        FillBlockTypesArray();
    }

    void Start()
    {
        //highLightCube.SetActive(false);
        Random.InitState(seed);
        GeneratePlanet();
        spawn=new Vector3(planetTotalPlanetRadiusInBlocks, planetTotalPlanetDiameterInBlocks-1, planetTotalPlanetRadiusInBlocks);
        player.transform.position=spawn;
        lastPlayerChunkPos=GetintVector3FromPos(spawn);
    }

    // Update is called once per frame
    void Update()
    {
        playerChunkPos = GetintVector3FromPos(player.transform.position);
        //Player is moving
        if(lastPlayerChunkPos!=playerChunkPos) 
        {
            UpdateChunks();
            lastPlayerChunkPos=playerChunkPos;
        }

        if (chunksToCreate.Count>0 && !isCreatingChunks)
            StartCoroutine(CreateChunks());
    }

    public intVector3 GetintVector3FromPos(Vector3 pos)
    {
        return new intVector3(Mathf.FloorToInt(pos.x/Data.chunkWidth), Mathf.FloorToInt(pos.y/Data.chunkHeight), Mathf.FloorToInt(pos.z/Data.chunkWidth));
    }

    public intVector3 GetVoxelCoordRelativeToChunk(Vector3 pos, intVector3 posChunk)
    {
        return new intVector3(Mathf.FloorToInt(pos.x-Data.chunkWidth*posChunk.x), Mathf.FloorToInt(pos.y-Data.chunkHeight*posChunk.y), Mathf.FloorToInt(pos.z-Data.chunkWidth*posChunk.z));
    }

    IEnumerator CreateChunks()
    {
        isCreatingChunks=true;
        while(chunksToCreate.Count>0){
            chunks[chunksToCreate[0].x,chunksToCreate[0].y,chunksToCreate[0].z] = new Chunk(chunksToCreate[0], this);
            chunksToCreate.RemoveAt(0);
            yield return null;
        }
        isCreatingChunks=false;
    }

    private void UpdateChunks()
    {
        //Update the active chunks list and create new chunks
        //Disable inactive chunks

        int chunkX=Mathf.FloorToInt(player.transform.position.x/Data.chunkWidth);
        int chunkY=Mathf.FloorToInt(player.transform.position.y/Data.chunkHeight);
        int chunkZ=Mathf.FloorToInt(player.transform.position.z/Data.chunkWidth);
        //Debug.Log("Player chunk coord : "+chunkX+", "+chunkY+", "+chunkZ+")");
        
        List<intVector3> inactiveChunks = new List<intVector3>(activeChunks); //Chunks that have to be set inactive

        for(int x=chunkX-viewDistance; x<=chunkX+viewDistance; x++)
        {
            for(int y=chunkY-viewDistance; y<=chunkY+viewDistance; y++)
            {
                for(int z=chunkZ-viewDistance; z<=chunkZ+viewDistance; z++)
                {
                    intVector3 intVector3 = new intVector3(x, y, z);
                    //Debug.Log("coord ("+x+", "+y+", "+z+")"+test);
                    if(IsChunkInWorld(x,y,z)){
                        if(chunks[x,y,z]==null){ //Has not be created
                            //Debug.Log("not yet created but will be : "+x+", "+y+", "+z+")"+test);
                            chunksToCreate.Add(intVector3);
                            //chunks[x,y,z] = new Chunk(intVector3, this);
                            activeChunks.Add(intVector3);
                        }
                        else if(!chunks[x,y,z].isActive){ //It's not currently active but has be set active
                            //Debug.Log("not yet active but will be : "+x+", "+y+", "+z+")"+test);
                            activeChunks.Add(intVector3);
                            chunks[x,y,z].isActive=true;
                        }
                        
                        for(int i=0; i<inactiveChunks.Count; i++){
                            if(inactiveChunks[i].x==intVector3.x && inactiveChunks[i].y==intVector3.y && inactiveChunks[i].z==intVector3.z){
                                //Debug.Log("removed from inactive : "+inactiveChunks[i].x+", "+inactiveChunks[i].y+", "+inactiveChunks[i].z+")"+test);
                                inactiveChunks.RemoveAt(i);
                            }
                        }
                    }
                }
            }
        }

        foreach(intVector3 c in inactiveChunks){
            for(int i=0; i<activeChunks.Count; i++){
                if(activeChunks[i]==c)
                    activeChunks.RemoveAt(i);
            }
            
            chunks[c.x, c.y, c.z].isActive=false;
            //Debug.Log("set to inactive : "+c.x+", "+c.y+", "+c.z+")");
        }

    }

    public bool IsChunkInWorld(int xChunk, int yChunk, int zChunk)
    {
        if(xChunk<0 || xChunk>=planetTotalPlanetDiameterInChunks || yChunk<0 || yChunk>=planetTotalPlanetDiameterInChunks || zChunk<0 || zChunk>=planetTotalPlanetDiameterInChunks){
            //Debug.Log("OUT WORLD ("+xChunk+", "+yChunk+", "+zChunk+")");
            return false;
        }
        else{
            //Debug.Log("IN WORLD ("+xChunk+", "+yChunk+", "+zChunk+")");
            return true;
        }
    }

    public void DestroyVoxel(Vector3 pos)
    {
        intVector3 posChunk=GetintVector3FromPos(pos);
        intVector3 posVoxelInChunk=GetVoxelCoordRelativeToChunk(pos, posChunk);
        
        chunks[posChunk.x,posChunk.y,posChunk.z].EditChunk(posVoxelInChunk, 0);//air

        /*
        if(chunks[posChunk.x,posChunk.y,posChunk.z]!=null){
            Destroy(chunks[posChunk.x,posChunk.y,posChunk.z].chunkObject);
        }

        chunksToCreate.Add(posChunk);
        activeChunks.Add(posChunk);
        */
    }

    private void GeneratePlanet()
    {
        int boundMin = planetTotalPlanetRadiusInChunks-viewDistance;
        int boundMax = planetTotalPlanetRadiusInChunks+viewDistance;
        int boundMinY = planetTotalPlanetDiameterInChunks-1-viewDistance;
        for(int x=boundMin; x<=boundMax; x++)
        {
            for(int y=boundMinY; y<planetTotalPlanetDiameterInChunks; y++)
            {
                for(int z=boundMin; z<=boundMax; z++)
                {
                    if(IsChunkInWorld(x,y,z)){
                        chunks[x,y,z] = new Chunk(new intVector3(x,y,z), this);
                        activeChunks.Add(new intVector3(x,y,z));
                    }
                }
            }
        }
    }

    public int CheckVoxelTypeIndex(Vector3 pos)
    {
        if(IsVoxelInPlanet(pos))
        {

            int x=Mathf.FloorToInt(pos.x);
            int y=Mathf.FloorToInt(pos.y);
            int z=Mathf.FloorToInt(pos.z);
            

            int xChunk = x/Data.chunkWidth;
            int yChunk = y/Data.chunkHeight;
            int zChunk = z/Data.chunkWidth;
            //Debug.Log("BEFORE   :   x, y, z=  "+x+", "+y+", "+z+"      xC, yC, zC =  "+xChunk+", "+yChunk+", "+zChunk);

            x-=xChunk*Data.chunkWidth;
            y-=yChunk*Data.chunkHeight;
            z-=zChunk*Data.chunkWidth;
            //Debug.Log("x, y, z=  "+x+", "+y+", "+z+"      xC, yC, zC =  "+xChunk+", "+yChunk+", "+zChunk+"      planetTotalPlanetDiameterInChunks=  "+planetTotalPlanetDiameterInChunks);
            //Debug.Log("CHUNK VM =  "+chunks[xChunk, yChunk, zChunk].voxelsMap[x,y,z]);
            return chunks[xChunk, yChunk, zChunk].voxelsMap[x,y,z];
        }
        else
            return 0; // It's air since it's out of the planet
    }

    

    public bool IsVoxelInPlanet(Vector3 pos)
    {
        if(pos.x<0 || pos.x>=planetTotalPlanetDiameterInBlocks || pos.y<0 || pos.y>=planetTotalPlanetDiameterInBlocks || pos.z<0 || pos.z>=planetTotalPlanetDiameterInBlocks)
            return false;
        else
            return true;
    }

    public bool CheckVoxelMap(Vector3 pos)
    {
        return blockTypes[CheckVoxelTypeIndex(pos)].isSolid;
        /*
        if(IsVoxelInPlanet(pos))
        {

            int x=Mathf.FloorToInt(pos.x);
            int y=Mathf.FloorToInt(pos.y);
            int z=Mathf.FloorToInt(pos.z);
            

            int xChunk = x/Data.chunkWidth;
            int yChunk = y/Data.chunkHeight;
            int zChunk = z/Data.chunkWidth;
            //Debug.Log("BEFORE   :   x, y, z=  "+x+", "+y+", "+z+"      xC, yC, zC =  "+xChunk+", "+yChunk+", "+zChunk);

            x-=xChunk*Data.chunkWidth;
            y-=yChunk*Data.chunkHeight;
            z-=zChunk*Data.chunkWidth;
            //Debug.Log("x, y, z=  "+x+", "+y+", "+z+"      xC, yC, zC =  "+xChunk+", "+yChunk+", "+zChunk+"      planetTotalPlanetDiameterInChunks=  "+planetTotalPlanetDiameterInChunks);
            //Debug.Log("CHUNK VM =  "+chunks[xChunk, yChunk, zChunk].voxelsMap[x,y,z]);
            return blockTypes[chunks[xChunk, yChunk, zChunk].voxelsMap[x,y,z]].isSolid;
        }
        else
            return false;
        */
    }

    public byte getVoxel(Vector3 pos)
    {
        Vector3 distance = pos - planetCoreCoord;
        Vector3 distanceAbs = new Vector3(Mathf.Abs(pos.x-planetCoreCoord.x), Mathf.Abs(pos.y-planetCoreCoord.y), Mathf.Abs(pos.z-planetCoreCoord.z));
        if(!IsVoxelInPlanet(pos)){
            return 0; //air or void
        }

       // Debug.Log("TH   :   " + planetMaxHeightRadiusInBlocks);

        //CORE
        if(distanceAbs.x<5 && distanceAbs.y<5 && distanceAbs.z<5)
            return 4; //bedrock

        //SURFACE
        if(
            (distanceAbs.x>planetGroundRadiusInBlocks) ||
            (distanceAbs.y>planetGroundRadiusInBlocks) ||
            (distanceAbs.z>planetGroundRadiusInBlocks)
        )
        {
            int sharedEdgeFace=-1;
            int facePlanet = checkPlanetFace(distance, out sharedEdgeFace);

            if(facePlanet==0){
                int terrainRadiusInBlocks = Mathf.FloorToInt(planetMaxHeightRadiusInBlocks*Noise.Get2DPerlinNoise(new Vector2(pos.y, pos.z), 0, 1f))+planetGroundRadiusInBlocks;
                if(distance.x<-terrainRadiusInBlocks)
                    return 0; //air
                else{
                    if(distance.x<-terrainRadiusInBlocks*0.9)
                        return 3;//grass
                    else if(
                        (distance.x<-terrainRadiusInBlocks*0.7)
                    )
                        return 2; //dirt
                    else if(
                        (distance.x<-terrainRadiusInBlocks*0.1)
                    )
                        return 1; //stone
                    else
                        return 2; //default case
                } 
            }
            else if(facePlanet==1)
            {
                int terrainRadiusInBlocks = Mathf.FloorToInt(planetMaxHeightRadiusInBlocks*Noise.Get2DPerlinNoise(new Vector2(pos.y, pos.z), 0, 1f))+planetGroundRadiusInBlocks;
                if(distance.x>terrainRadiusInBlocks)
                    return 0; //air
                else{
                    if(distance.x>terrainRadiusInBlocks*0.9)
                        return 3;//grass
                    else if(
                        (distance.x>terrainRadiusInBlocks*0.7)
                    )
                        return 2; //dirt
                    else if(
                        (distance.x>terrainRadiusInBlocks*0.1)
                    )
                        return 1; //stone
                    else
                        return 2; //default case
                } 
            }
            else if(facePlanet==2){
                int terrainRadiusInBlocks = Mathf.FloorToInt(planetMaxHeightRadiusInBlocks*Noise.Get2DPerlinNoise(new Vector2(pos.x, pos.z), 0, 1f))+planetGroundRadiusInBlocks;
                if(distance.y<-terrainRadiusInBlocks)
                    return 0; //air
                else{
                    if(distance.y<-terrainRadiusInBlocks*0.9)
                        return 3;//grass
                    else if(
                        (distance.y<-terrainRadiusInBlocks*0.7)
                    )
                        return 2; //dirt
                    else if(
                        (distanceAbs.y<-terrainRadiusInBlocks*0.1)
                    )
                        return 1; //stone
                    else
                        return 2; //default case
                }
            }
            else if(facePlanet==3){
                int terrainRadiusInBlocks = Mathf.FloorToInt(planetMaxHeightRadiusInBlocks*Noise.Get2DPerlinNoise(new Vector2(pos.x, pos.z), 0, 1f))+planetGroundRadiusInBlocks;
                if(distance.y>terrainRadiusInBlocks)
                    return 0; //air
                else{
                    if(distance.y>terrainRadiusInBlocks*0.9)
                        return 3;//grass
                    else if(
                        (distance.y>terrainRadiusInBlocks*0.7)
                    )
                        return 2; //dirt
                    else if(
                        (distance.y>terrainRadiusInBlocks*0.1)
                    )
                        return 1; //stone
                    else
                        return 2; //default case
                }
            }
            else if(facePlanet==4){
                int terrainRadiusInBlocks = Mathf.FloorToInt(planetMaxHeightRadiusInBlocks*Noise.Get2DPerlinNoise(new Vector2(pos.x, pos.y), 0, 1f))+planetGroundRadiusInBlocks;
                if(distance.z<-terrainRadiusInBlocks)
                    return 0; //air
                else{
                    if(distance.z<-terrainRadiusInBlocks*0.9)
                        return 3;//grass
                    else if(
                        (distance.z<-terrainRadiusInBlocks*0.7)
                    )
                        return 2; //dirt
                    else if(
                        (distance.z<-terrainRadiusInBlocks*0.1)
                    )
                        return 1; //stone
                    else
                        return 2; //default case
                }
            }
            else if(facePlanet==5){
                int terrainRadiusInBlocks = Mathf.FloorToInt(planetMaxHeightRadiusInBlocks*Noise.Get2DPerlinNoise(new Vector2(pos.x, pos.y), 0, 1f))+planetGroundRadiusInBlocks;
                if(distance.z>terrainRadiusInBlocks)
                    return 0; //air
                else{
                    if(distance.z>terrainRadiusInBlocks*0.9)
                        return 3;//grass
                    else if(
                        (distance.z>terrainRadiusInBlocks*0.7)
                    )
                        return 2; //dirt
                    else if(
                        (distance.z>terrainRadiusInBlocks*0.1)
                    )
                        return 1; //stone
                    else
                        return 2; //default case
                }
            }
            else {
                //Debug.Log("TRANSITION FACE");
                int terrainRadiusInBlocks;
                switch(sharedEdgeFace){
                    case 0: terrainRadiusInBlocks= Mathf.FloorToInt(planetMaxHeightRadiusInBlocks*Noise.Get2DPerlinNoise(new Vector2(pos.y, pos.z), 0, 1f))+planetGroundRadiusInBlocks; break;
                    case 1: terrainRadiusInBlocks = Mathf.FloorToInt(planetMaxHeightRadiusInBlocks*Noise.Get2DPerlinNoise(new Vector2(pos.y, pos.z), 0, 1f))+planetGroundRadiusInBlocks; break;
                    case 2: terrainRadiusInBlocks = Mathf.FloorToInt(planetMaxHeightRadiusInBlocks*Noise.Get2DPerlinNoise(new Vector2(pos.x, pos.z), 0, 1f))+planetGroundRadiusInBlocks; break;
                    case 3: terrainRadiusInBlocks = Mathf.FloorToInt(planetMaxHeightRadiusInBlocks*Noise.Get2DPerlinNoise(new Vector2(pos.x, pos.z), 0, 1f))+planetGroundRadiusInBlocks; break;
                    case 4: terrainRadiusInBlocks = Mathf.FloorToInt(planetMaxHeightRadiusInBlocks*Noise.Get2DPerlinNoise(new Vector2(pos.x, pos.y), 0, 1f))+planetGroundRadiusInBlocks; break;
                    case 5: terrainRadiusInBlocks = Mathf.FloorToInt(planetMaxHeightRadiusInBlocks*Noise.Get2DPerlinNoise(new Vector2(pos.x, pos.y), 0, 1f))+planetGroundRadiusInBlocks; break;
                    default : terrainRadiusInBlocks = Mathf.FloorToInt(planetMaxHeightRadiusInBlocks*Noise.Get2DPerlinNoise(new Vector2(pos.y, pos.z), 0, 1f))+planetGroundRadiusInBlocks; break;
                }
                if(distanceAbs.x<=terrainRadiusInBlocks && distanceAbs.y<=terrainRadiusInBlocks && distanceAbs.z<=terrainRadiusInBlocks)
                    return 3;
                else
                    return 0;
            }
        }   
        else //UNDERGROUND
            return 1; //stone
    }

    int checkPlanetFace(Vector3 distance, out int sharedEdgeFace)
    {
        int returnedFace=0;
        sharedEdgeFace=0;

        if(distance.x<=-planetGroundRadiusInBlocks)//0
            returnedFace+=1;
        if(distance.x>=planetGroundRadiusInBlocks)//1
            returnedFace+=2;
        if(distance.y<=-planetGroundRadiusInBlocks)//2
            returnedFace+=4;
        if(distance.y>=planetGroundRadiusInBlocks)//3
            returnedFace+=8;
        if(distance.z<=-planetGroundRadiusInBlocks)//4
            returnedFace+=16;
        if(distance.z>=planetGroundRadiusInBlocks)//5
            returnedFace+=32;

        switch(returnedFace){
            //On one face
            case 1: return 0;
            case 2: return 1;
            case 4: return 2;
            case 8: return 3;
            case 16: return 4;
            case 32: return 5;
            //On 2 faces (edges)
            case 33: sharedEdgeFace=0; return 6; //between 0 and 5
            case 9: sharedEdgeFace=0; return 6; //between 0 and 3
            case 17: sharedEdgeFace=0; return 6; //between 0 and 4
            case 5: sharedEdgeFace=0; return 6; //between 0 and 2

            case 34: sharedEdgeFace=1; return 6; //between 1 and 5
            case 10: sharedEdgeFace=1; return 6; //between 1 and 3
            case 18: sharedEdgeFace=1; return 6; //between 1 and 4
            case 6: sharedEdgeFace=1; return 6; //between 1 and 2
            
            case 40: sharedEdgeFace=3; return 6; //between 3 and 5
            case 24: sharedEdgeFace=3; return 6; //between 3 and 4
            case 36: sharedEdgeFace=2; return 6; //between 2 and 5
            case 20: sharedEdgeFace=2; return 6; //between 2 and 4

            //On 3 faces (vertices)
            case 41: sharedEdgeFace=0; return 6; //between 0, 5 and 3
            case 37: sharedEdgeFace=0; return 6; //between 0, 5 and 2
            case 25: sharedEdgeFace=0; return 6; //between 0, 4 and 3
            case 21: sharedEdgeFace=0; return 6; //between 0, 4 and 2
            case 42: sharedEdgeFace=1; return 6; //between 1, 5 and 3
            case 38: sharedEdgeFace=1; return 6; //between 1, 5 and 2
            case 26: sharedEdgeFace=1; return 6; //between 1, 4 and 3
            case 22: sharedEdgeFace=1; return 6; //between 1, 4 and 2


            default: sharedEdgeFace=0;  Debug.Log("ERROR : (in checkPlanetFace) Face not found  faceReturned=      " + returnedFace); return 6;
        }
    }

    /* int checkPlanetFace(Vector3 distance)
    {
        int returnedFace=0;

        if(distance.x<=-planetGroundRadiusInBlocks)//0
            returnedFace+=1;
        if(distance.x>=planetGroundRadiusInBlocks)//1
            returnedFace+=2;
        if(distance.y<=-planetGroundRadiusInBlocks)//2
            returnedFace+=4;
        if(distance.y>=planetGroundRadiusInBlocks)//3
            returnedFace+=8;
        if(distance.z<=-planetGroundRadiusInBlocks)//4
            returnedFace+=16;
        if(distance.z>=planetGroundRadiusInBlocks)//5
            returnedFace+=32;

        switch(returnedFace){
            //On one face
            case 1: return 0;
            case 2: return 1;
            case 4: return 2;
            case 8: return 3;
            case 16: return 4;
            case 32: return 5;
            //On 2 faces (edges)
            case 33: return 0; //between 0 and 5
            case 9: return 0; //between 0 and 3
            case 17: return 0; //between 0 and 4
            case 5: return 0; //between 0 and 2

            case 34: return 1; //between 1 and 5
            case 10: return 1; //between 1 and 3
            case 18: return 1; //between 1 and 4
            case 6: return 1; //between 1 and 2
            
            case 40: return 3; //between 3 and 5
            case 24: return 3; //between 3 and 4
            case 36: return 2; //between 2 and 5
            case 20: return 2; //between 2 and 4

            //On 3 faces (vertices)
            case 41: return 0; //between 0, 5 and 3
            case 37: return 0; //between 0, 5 and 2
            case 25: return 0; //between 0, 4 and 3
            case 21: return 0; //between 0, 4 and 2
            case 42: return 1; //between 1, 5 and 3
            case 38: return 1; //between 1, 5 and 2
            case 26: return 1; //between 1, 4 and 3
            case 22: return 1; //between 1, 4 and 2


            default: Debug.Log("ERROR : (in checkPlanetFace) Face not found  faceReturned=      " + returnedFace); return 0;
        }
    }
*/

    public int checkPlanetFacePlayer(Vector3 pos)
    {
        Vector3 distance = pos - planetCoreCoord;
        Vector3 distanceAbs = new Vector3(Mathf.Abs(pos.x-planetCoreCoord.x), Mathf.Abs(pos.y-planetCoreCoord.y), Mathf.Abs(pos.z-planetCoreCoord.z));
        
        if(distanceAbs.x>distanceAbs.y){
            if(distanceAbs.x>distanceAbs.z){ //x>0 ou <0
                if(distance.x>0)
                    return 1;
                else
                    return 0;
            }
            else{ //z>0 ou <0
                if(distance.z>0)
                    return 5;
                else
                    return 4;
            }
        }
        else{
            if(distanceAbs.y>distanceAbs.z){ //y>0 ou <0
                if(distance.y>0)
                    return 3;
                else
                    return 2;
            }
            else{ //z>0 ou <0
                if(distance.z>0)
                    return 5;
                else
                    return 1;
            }
        }
    }

/*if((distanceAbs.x==distanceAbs.y && distanceAbs.x>=distanceAbs.z) || (distanceAbs.x==distanceAbs.z && distanceAbs.x>=distanceAbs.y))
            return 0;
        else if(distanceAbs.x>distanceAbs.y){
            if(distanceAbs.x>distanceAbs.z){ //x>0 ou <0
                if(distance.x>0)
                    return 0;
                else
                    return 1;
            }
            else{ //z>0 ou <0
                if(distance.z>0)
                    return 4;
                else
                    return 5;
            }
        }
        else{
            if(distanceAbs.y>distanceAbs.z){ //y>0 ou <0
                if(distance.y>0)
                    return 2;
                else
                    return 3;
            }
            else{ //z>0 ou <0
                if(distance.z>0)
                    return 4;
                else
                    return 5;
            }
        }
    }*/

    public void FillBlockTypesArray()
    {
        // front 0  back 1  top 2   bot 3   right 4  left 5

        blockTypes = new BlockTypes[10];
        
        blockTypes[0] = new BlockTypes("air", false, new int[6]{0,0,0,0,0,0});
        blockTypes[1] = new BlockTypes("stone", true, new int[6]{0,0,0,0,0,0});
        blockTypes[2] = new BlockTypes("dirt", true, new int[6]{1,1,1,1,1,1});
        blockTypes[3] = new BlockTypes("grass", true, new int[6]{7,7,7,7,7,7});
        blockTypes[4] = new BlockTypes("bedrock", true, new int[6]{9,9,9,9,9,9});
    }

}



public class intVector3
{
    public int x;
    public int y;
    public int z;

    public intVector3(int _x, int _y, int _z)
    {
        x=_x;
        y=_y;
        z=_z;
    }
}

public class BlockTypes
{
    public BlockTypes(string name, bool _isSolid, int[] textureFaces)
    {
        blockName=name;
        isSolid=_isSolid;
        frontFaceTexture=textureFaces[0];
        backFaceTexture=textureFaces[1];
        topFaceTexture=textureFaces[2];
        botFaceTexture=textureFaces[3];
        rightFaceTexture=textureFaces[4];
        leftFaceTexture=textureFaces[5];
    }

    public string blockName;
    public bool isSolid;

    public int frontFaceTexture;
    public int backFaceTexture;
    public int topFaceTexture;
    public int botFaceTexture;
    public int rightFaceTexture;
    public int leftFaceTexture;

    public int getTextureID(int face)
    {
        switch(face){
            case 0 : return frontFaceTexture;
            case 1 : return backFaceTexture;
            case 2 : return topFaceTexture;
            case 3 : return backFaceTexture;
            case 4 : return rightFaceTexture;
            case 5 : return leftFaceTexture;
            default : Debug.Log("ERROR : (in getTextureID) index \"face\" incorrect"); return 0;
        }
    }
}