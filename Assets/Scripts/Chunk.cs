using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public intVector3 coord;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    private Vector3 position;
    public byte[,,] voxelsMap;
    [System.NonSerialized]
    public GameObject chunkObject;
    Planet planet;
    private bool m_isActive=true;
    public bool isActive{
        set{
            m_isActive=value;
            if(chunkObject!=null)
                chunkObject.SetActive(value);
        }

        get{ return chunkObject.activeSelf; }
    }

    List<Vector3> vertices = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> triangles = new List<int>();
    Mesh mesh = new Mesh();
    
    int verticesIndex=0;

    public Chunk(intVector3 _coord, Planet _planet)
    {
        chunkObject = new GameObject();
        isActive=m_isActive;
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        planet = _planet;
        voxelsMap = new byte[Data.chunkWidth, Data.chunkHeight, Data.chunkWidth];
        meshRenderer.material = planet.material;
        coord=_coord;
        chunkObject.transform.position = new Vector3(coord.x*Data.chunkWidth, coord.y*Data.chunkHeight, coord.z*Data.chunkWidth);
        position=chunkObject.transform.position;
        chunkObject.transform.SetParent(planet.transform);
        chunkObject.name = "Chunk" + coord.x + "," + coord.y + "," + coord.z;

        

        FillChunkMap();
        CreateMeshData();
        CreateMesh();
    }


    public void EditChunk(intVector3 pos, byte newBlockID)
    {
        voxelsMap[pos.x,pos.y,pos.z]=newBlockID;
        UpdateNearVoxels();
        UpdateChunk();
    }

    private void UpdateNearVoxels()
    {
        
    }

    private void UpdateChunk()
    {

    }

    public void FillChunkMap()
    {
        for(int x=0; x<Data.chunkWidth; x++){
            for(int y=0; y<Data.chunkHeight; y++){
                for(int z=0; z<Data.chunkWidth; z++){
                    voxelsMap[x,y,z] = planet.getVoxel(new Vector3(x,y,z)+position);
                }
            }
        }
    }

    public void CreateMeshData()
    {
        for(int x=0; x<Data.chunkWidth; x++){
            for(int y=0; y<Data.chunkHeight; y++){
                for(int z=0; z<Data.chunkWidth; z++){
                    if(voxelsMap[x,y,z]!=0){ //air
                        AddVoxelToChunk(new Vector3(x,y,z));
                    }
                }
            }
        }
    }

    public void AddVoxelToChunk(Vector3 pos)
    {
        for(int i_face=0; i_face<6; i_face++){
            if(CheckVoxel(pos+Data.checkFacesArray[i_face])){
                vertices.Add(pos+Data.verticesCoord[Data.triangles[i_face, 0]]);
                vertices.Add(pos+Data.verticesCoord[Data.triangles[i_face, 1]]);
                vertices.Add(pos+Data.verticesCoord[Data.triangles[i_face, 2]]);
                vertices.Add(pos+Data.verticesCoord[Data.triangles[i_face, 3]]);

                AddTextureToVoxel(planet.blockTypes[voxelsMap[(int)pos.x, (int)pos.y, (int)pos.z]].getTextureID(i_face));
                
                triangles.Add(verticesIndex);
                triangles.Add(verticesIndex+1);
                triangles.Add(verticesIndex+2);
                triangles.Add(verticesIndex+2);
                triangles.Add(verticesIndex+1);
                triangles.Add(verticesIndex+3);
                
                verticesIndex+=4;
            }
        }        
    }

    public void AddTextureToVoxel(int textureID)
    {
        float y=textureID/Data.textureAtlasSizeInBlocks;
        float x=textureID - (y*Data.textureAtlasSizeInBlocks);

        x*=Data.normalizedAtlasSizeOfOneBlock;
        y*=Data.normalizedAtlasSizeOfOneBlock;

        y= 1f-y-Data.normalizedAtlasSizeOfOneBlock; // because we want it to start from the bottom left corner of the atlas (and the botom left of the block on the atlas, that's why we have to add the -Data.normalizedAtlasSizeOfOneBlock)

        uvs.Add(new Vector2(x,y));
        uvs.Add(new Vector2(x,y+Data.normalizedAtlasSizeOfOneBlock));
        uvs.Add(new Vector2(x+Data.normalizedAtlasSizeOfOneBlock,y));
        uvs.Add(new Vector2(x+Data.normalizedAtlasSizeOfOneBlock,y+Data.normalizedAtlasSizeOfOneBlock));
    }

    

    public bool CheckVoxel(Vector3 pos)
    {
        if(IsVoxelInChunk(pos))
            return !planet.blockTypes[voxelsMap[(int)pos.x, (int)pos.y, (int)pos.z]].isSolid;
        else
            return !planet.blockTypes[planet.getVoxel(pos+position)].isSolid;
    }

    public bool IsVoxelInChunk(Vector3 pos)
    {
        if(pos.x<0 || pos.x>=Data.chunkWidth || pos.y<0 || pos.y>=Data.chunkHeight || pos.z<0 || pos.z>=Data.chunkWidth)
            return false;
        else
            return true;
    }

    
    

    public void CreateMesh()
    {
        mesh.vertices=vertices.ToArray();
        mesh.triangles=triangles.ToArray();
        mesh.uv=uvs.ToArray();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }


}
