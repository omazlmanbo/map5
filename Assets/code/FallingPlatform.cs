using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))] // 注意：这里改为 Rigidbody2D
public class FallingPlatform2D : MonoBehaviour
{
    [Header("设置")]
    [Tooltip("玩家离开后多久开始掉落（秒）")]
    public float fallDelay = 0.5f;

    [Tooltip("掉落后多久销毁物体（秒）")]
    public float destroyDelay = 3.0f;

    private Rigidbody2D rb;
    private bool isFalling = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 2D 物理设置：
        // 将刚体类型设为 Kinematic（运动学）。
        // 这种状态下物体不受重力影响，会定在空中，直到我们要它掉落。
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    // 注意：这里使用的是 OnCollisionExit2D (2D专用)
    void OnCollisionExit2D(Collision2D collision)
    {
        // 检查离开的是不是玩家，并且平台还没掉落
        if (collision.gameObject.CompareTag("Player") && !isFalling)
        {
            // 只有当玩家是从上方接触时才触发（可选优化，防止玩家碰到侧面也掉落）
            // 如果不需要这个判断，可以去掉下面这行 if 的前半部分
            if (collision.transform.position.y > transform.position.y)
            {
                StartCoroutine(Fall());
            }
            else
            {
                // 如果不想判断方向，直接开启掉落：
                StartCoroutine(Fall());
            }
        }
    }

    IEnumerator Fall()
    {
        isFalling = true;

        // 等待延迟时间
        yield return new WaitForSeconds(fallDelay);

        // 核心逻辑：将刚体类型改为 Dynamic（动力学）
        // 这样它就会立刻受到重力影响掉下去
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f; // 确保重力倍数为1（正常下落）

        // 几秒后销毁物体
        Destroy(gameObject, destroyDelay);
    }
}