using UnityEngine;
using System.Collections;

public class MonsterController : MonoBehaviour
{
    [Header("��������")]
    public float moveSpeed = 3.5f;        // �ƶ��ٶ�
    public float detectionRange = 10f;    // ���з�Χ
    public int damageToPlayer = 10;       // �������ɵ��˺�

    [Header("������������")]
    public float hitStopDuration = 0.2f;  // �����к��ͣ��ʱ�䣨��֡�У�
    public float shakeDuration = 0.5f;    // ��������ʱ��
    public float shakeIntensity = 0.1f;   // ��������
    public float retreatDistance = 2.0f;  // �󳷾���
    public float retreatSpeed = 5.0f;     // ���ٶ�

    private Transform playerTarget;
    private bool isDead = false;          // �Ƿ�������
    private Rigidbody rb;

    void Start()
    {
        // �Զ�Ѱ�ҳ����б�ǩΪ "Player" ������
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
        }

        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // ������˻���û�ҵ���ң��Ͳ�ִ��AI�߼�
        if (isDead || playerTarget == null) return;

        float distance = Vector3.Distance(transform.position, playerTarget.position);

        // �ڷ�Χ����׷�����
        if (distance <= detectionRange)
        {
            // ChasePlayer();
        }
    }

    // �򵥵�׷���߼�
    void ChasePlayer()
    {
        // �������
        transform.LookAt(new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z));

        // ��ǰ�ƶ�
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }

    // ��ײ��⣺�����������˺�
    void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        // ���ײ�����ǲ������
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageToPlayer);

                // �򵥵�ײ������Ч������ѡ��
                if (rb != null) rb.AddForce(-transform.forward * 5f, ForceMode.Impulse);
            }
        }
    }

    // --- ����ҹ������߼� (��Ӧ��ԭ���ű���� monster.TakeHit()) ---
    public void TakeHit()
    {
        if (isDead) return; // ��ֹ��ʬ

        // ��������Э��
        StartCoroutine(DeathSequence());
    }

    // ���ģ������ݳ���Э��
    IEnumerator DeathSequence()
    {
        isDead = true;

        // �ر�������ײ����ֹʬ�嵲·���������˺�
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        if (rb != null) rb.isKinematic = true; // ֹͣ��������

        // 1. ͣ�� (Hit Stop) - ģ�����У�˲�䲻��
        yield return new WaitForSeconds(hitStopDuration);

        // 2. ���� (Shake)
        float elapsed = 0f;
        Vector3 originalPos = transform.position;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float z = Random.Range(-1f, 1f) * shakeIntensity;

            // ��ԭλ�û��������ƫ��
            transform.position = originalPos + new Vector3(x, 0, z);

            elapsed += Time.deltaTime;
            yield return null; // �ȴ���һ֡
        }

        // �ָ�λ��׼����
        transform.position = originalPos;

        // 3. �� (Retreat) - ������󻬶�����͸�����������֧�֣���ֱ����ʧ
        elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = transform.position - transform.forward * retreatDistance; // ����ƶ�

        while (elapsed < 0.5f) // �󳷹��̳���0.5��
        {
            // ƽ����ֵ�ƶ�
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / 0.5f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 4. ��ʧ (Destroy)
        Destroy(gameObject);
    }
}