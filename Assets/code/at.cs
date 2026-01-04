using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("����")]
    public int damageAmount = 20;      // �˺���ֵ
    public float attackRange = 2.0f;   // ��������
    public LayerMask enemyLayer;       // �������ڵ�ͼ��(Layer)

    [Header("�������")]
    public Animator animator;          // ����������

    void Update()
    {
        // 0�������1���Ҽ���2���м�
        if (Input.GetMouseButtonDown(0))
        {
            PerformAttack();
        }
    }

    void PerformAttack()
    {
        // 1. ���Ź������� (��Ҫ��Animator������һ����Ϊ "Attack" �� Trigger)
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // 2. �������߼��ǰ����λ
        // origin: ���ߵ���� (λ����΢̧��һ�㣬��ֹ�򵽵���)
        Vector2 rayOrigin = transform.position;
        // direction: ���ߵķ��� (��ɫ�泯�ķ���)
        Vector2 rayDirection = new Vector2(1, 0);

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, attackRange, enemyLayer);
        // ��������
        if (hit != null && hit.collider != null)
        {
            // ��鱻���е������Ƿ�������ֵ�ű�
            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();

            if (enemy != null)
            {
                enemy.TakeDamage(damageAmount);

                // ��ѡ�����������ӻ�����Ч����Ч
                Debug.Log("������ " + hit.collider.name);
            }
        }else{
            rayOrigin = transform.position;
            // direction: ���ߵķ��� (��ɫ�泯�ķ���)
            rayDirection = new Vector2(-1, 0);

            hit = Physics2D.Raycast(rayOrigin, rayDirection, attackRange, enemyLayer);
        // ��������
           if (hit != null && hit.collider != null)
            {
                // ��鱻���е������Ƿ�������ֵ�ű�
                EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();

                if (enemy != null)
                {
                    enemy.TakeDamage(damageAmount);

                    // ��ѡ�����������ӻ�����Ч����Ч
                    Debug.Log("������ " + hit.collider.name);
                }
            }
        }
    }

    // �ڱ༭���л��ƹ�����Χ������ (��Scene���ڿɼ�)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 rayOrigin = transform.position + Vector3.up * 1.0f;
        Gizmos.DrawRay(rayOrigin, transform.forward * attackRange);
    }
}