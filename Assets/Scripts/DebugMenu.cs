using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugMenu : MonoBehaviour
{
    private Text text;
    public Planet planet;
    private int fps;
    private float timer=0;
    // Start is called before the first frame update
    void Start()
    {
        text=GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if(timer>1f){
            Vector3 pos=planet.player.transform.position;
            intVector3 chunkPos = planet.GetChunkCoordFromPos(pos);
            pos.x=Mathf.FloorToInt(pos.x); pos.y=Mathf.FloorToInt(pos.y); pos.z=Mathf.FloorToInt(pos.z);
            fps=Mathf.FloorToInt(1f/Time.unscaledDeltaTime);
            text.text="FPS: "+fps+"\n\n";
            text.text+="Position: "+pos.x+", "+pos.y+", "+pos.z+"\n";
            text.text+="Current chunk:"+chunkPos.x+", "+chunkPos.y+", "+chunkPos.z+"\n\n";

            timer=0;
        }
        else
            timer+=Time.deltaTime;
    }
}
