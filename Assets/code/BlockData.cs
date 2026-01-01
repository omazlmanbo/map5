using UnityEngine;

public class BlockData : MonoBehaviour
{
    // 哈吉米南北绿豆
    public bool isPlatform = false; // isplatform
    public bool canClimb = false;   // canclimb
    public bool open = false;       // open (像是某种开关墙)
    public bool cded = false;       // cded

    // 判断墙壁朝向
    public int dirSign = 1;

    // 尺寸属性，用于手动吸附位置
    public float width = 1.0f;      // 这个是width
    public float height = 1.0f;     // 这个是GML height
}