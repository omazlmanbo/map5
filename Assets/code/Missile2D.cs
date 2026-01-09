using UnityEngine;

public class Missile2D : MonoBehaviour
{
    public float speed = 10f;           // 飞行速度
    public float lifeTime = 4f;         // 自动销毁时间
    private Vector2 fixedDirection;     // 锁定的弹道方向
    private bool isLaunched = false;

    public void Launch(Vector2 direction)
    {
        fixedDirection = direction;
        isLaunched = true;

        // 让导弹的“头”指向飞行方向（2D 旋转逻辑）
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Start()
    {
        Destroy(gameObject, lifeTime); // 防止飞出地图的物体堆积
    }

    void Update()
    {
        if (isLaunched)
        {
            // 沿固定弹道直线移动，不受玩家后续位移影响
            transform.Translate(Vector2.right * speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 在此处理玩家伤害逻辑
            Debug.Log("命中玩家！");
            Destroy(gameObject);
        }
        // 如果撞到墙壁也可以销毁
        else if (collision.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}