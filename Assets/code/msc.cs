using UnityEngine;
using System.Collections;

public class MonsterController : MonoBehaviour
{
    [Header("基础属性")]
    public float moveSpeed = 3.5f;        // 移动速度
    public float detectionRange = 10f;    // 索敌范围
    public int damageToPlayer = 10;       // 对玩家造成的伤害

    [Header("死亡动画参数")]
    public float hitStopDuration = 0.2f;  // 被击中后的停滞时间（顿帧感）
    public float shakeDuration = 0.5f;    // 抖动持续时间
    public float shakeIntensity = 0.1f;   // 抖动幅度
    public float retreatDistance = 2.0f;  // 后撤距离
    public float retreatSpeed = 5.0f;     // 后撤速度

    private Transform playerTarget;
    private bool isDead = false;          // 是否已死亡
    private Rigidbody rb;

    void Start()
    {
        // 自动寻找场景中标签为 "Player" 的物体
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
        }

        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 如果死了或者没找到玩家，就不执行AI逻辑
        if (isDead || playerTarget == null) return;

        float distance = Vector3.Distance(transform.position, playerTarget.position);

        // 在范围内则追逐玩家
        if (distance <= detectionRange)
        {
            ChasePlayer();
        }
    }

    // 简单的追逐逻辑
    void ChasePlayer()
    {
        // 面向玩家
        transform.LookAt(new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z));

        // 向前移动
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }

    // 碰撞检测：碰到玩家造成伤害
    void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        // 检查撞到的是不是玩家
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageToPlayer);

                // 简单的撞击反弹效果（可选）
                if (rb != null) rb.AddForce(-transform.forward * 5f, ForceMode.Impulse);
            }
        }
    }

    // --- 被玩家攻击的逻辑 (对应你原来脚本里的 monster.TakeHit()) ---
    public void TakeHit()
    {
        if (isDead) return; // 防止鞭尸

        // 开启死亡协程
        StartCoroutine(DeathSequence());
    }

    // 核心：死亡演出的协程
    IEnumerator DeathSequence()
    {
        isDead = true;

        // 关闭物理碰撞，防止尸体挡路或继续造成伤害
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        if (rb != null) rb.isKinematic = true; // 停止物理运算

        // 1. 停滞 (Hit Stop) - 模拟打击感，瞬间不动
        yield return new WaitForSeconds(hitStopDuration);

        // 2. 抖动 (Shake)
        float elapsed = 0f;
        Vector3 originalPos = transform.position;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float z = Random.Range(-1f, 1f) * shakeIntensity;

            // 在原位置基础上随机偏移
            transform.position = originalPos + new Vector3(x, 0, z);

            elapsed += Time.deltaTime;
            yield return null; // 等待下一帧
        }

        // 恢复位置准备后撤
        transform.position = originalPos;

        // 3. 后撤 (Retreat) - 快速向后滑动并变透明（如果材质支持）或直接消失
        elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = transform.position - transform.forward * retreatDistance; // 向后方移动

        while (elapsed < 0.5f) // 后撤过程持续0.5秒
        {
            // 平滑插值移动
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / 0.5f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 4. 消失 (Destroy)
        Destroy(gameObject);
    }
}