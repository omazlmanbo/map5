using UnityEngine;
using UnityEngine.AI; // 关键：引入AI命名空间

[RequireComponent(typeof(NavMeshAgent))] // 强制要求物体必须有 NavMeshAgent 组件
public class MonsterNavMesh : MonoBehaviour
{
    [Header("设置 (Settings)")]
    public Transform player;
    public float detectionRange = 10f; // 侦测/仇恨范围
    public int damage = 10;
    public float attackCooldown = 1.0f;

    private NavMeshAgent agent;       // AI 代理组件引用
    private float lastAttackTime;

    void Start()
    {
        // 获取自身的 NavMeshAgent 组件
        agent = GetComponent<NavMeshAgent>();

        // 自动查找玩家
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        // 1. 计算距离
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 2. 如果在侦测范围内，设置寻路目标为玩家位置
        if (distanceToPlayer <= detectionRange)
        {
            agent.isStopped = false; // 确保代理没有停下
            agent.SetDestination(player.position); // 核心：告诉AI去哪里
        }
        else
        {
            // 如果超出了范围，让怪物停在原地 (或者你可以写巡逻逻辑)
            agent.isStopped = true;
            // agent.ResetPath(); // 或者清除路径
        }
    }

    // 3. 触碰攻击逻辑 (和之前一样，依靠物理碰撞)
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Attack(collision.gameObject);
                lastAttackTime = Time.time;
            }
        }
    }

    void Attack(GameObject target)
    {
        Debug.Log("NavMesh 怪物攻击了玩家！伤害: " + damage);
        // 在这里添加扣血逻辑...
    }

    // 调试辅助线
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}