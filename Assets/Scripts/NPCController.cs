using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    public Animator animator; // ������ �� Animator
    public Transform player; // ������ �� ������
    public float detectionRange = 10f; // ������ ����������� ������
    public float attackRange = 2f; // ������ �����
    public float fieldOfView = 60f; // ���� ������ NPC (� ��������)
    public float attackAngle = 30f; // ���� ��������� ������ (� ��������)
    public float attackCooldown = 1.5f; // ����� ����� �������
    public int damage = 10; // ���� �� NPC
    public int health = 100;

    private NavMeshAgent agent; // ������ �� NavMeshAgent
    private float lastAttackTime; // ����� ��������� �����
    private bool isStunned = false; // ���� ��� ���������� �������� ��� ��������� �����

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (player == null || isStunned) return; // ���� ������ ��� ��� NPC ��������, ������ �� ������

        // ���������� �� ������
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange && CanSeePlayer())
        {
            // ���� ����� � ���� ��������� � �� �� ������, ������� �� ���
            if (distanceToPlayer > attackRange)
            {
                agent.SetDestination(player.position);
                animator.SetBool("IsRunning", true);
            }
            else
            {
                // ��������������� ����� ������
                agent.SetDestination(transform.position);
                animator.SetBool("IsRunning", false);

                // ����� ������ ������ ���� ������ ���������� �������
                if (Time.time - lastAttackTime >= attackCooldown && IsWithinAttackAngle(player.position))
                {
                    AttackPlayer();
                    lastAttackTime = Time.time;
                }
            }
        }
        else
        {
            // ���� ����� ��� ���� ���������, NPC ���������������
            agent.SetDestination(transform.position);
            animator.SetBool("IsRunning", false);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isStunned) return; // ���� NPC ��� ��������, ����������

        health -= damage;

        if (health > 0)
        {
            Debug.Log("Npc ������� ����: " + health);
            // �������� ��������� �����
            animator.SetTrigger("TakeDamage");
            StartCoroutine(StunNPC()); // ��������� ��������� ���������� ��������
        }
        else
        {
            // �������� ������
            animator.SetTrigger("Die");
            Destroy(gameObject, 2f); // ���������� ������ ����� 2 �������
        }
    }

    private bool CanSeePlayer()
    {
        // ����������� � ������
        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        // ��������� ���� ����� ������������ NPC � ������������ � ������
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        // ���� ���� ������ �������� ���� ������, ��������� ������� �����������
        if (angleToPlayer <= fieldOfView / 2f)
        {
            Ray ray = new Ray(transform.position + Vector3.up, directionToPlayer); // ��� �� NPC � ������ (������� ���� �����)
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, detectionRange))
            {
                if (hit.transform == player)
                {
                    // ����� �����
                    return true;
                }
            }
        }

        // ����� ��� ���� ������ ��� �� ������������
        return false;
    }

    private bool IsWithinAttackAngle(Vector3 position)
    {
        // ����������� � ����������� �������
        Vector3 directionToPosition = (position - transform.position).normalized;

        // ��������� ���� ����� ������������ NPC � ������������ � �������
        float angleToPosition = Vector3.Angle(transform.forward, directionToPosition);

        // ���������� true, ���� ������� ��������� � ���� �����
        return angleToPosition <= attackAngle / 2f;
    }

    private void AttackPlayer()
    {
        // �������� �����
        animator.SetTrigger("Attack");

        // ������� ���� ������ (���� �� � ���� �����)
        SimpleCharacterController playerHealth = player.GetComponent<SimpleCharacterController>();
        if (playerHealth != null && IsWithinAttackAngle(player.position))
        {
            playerHealth.TakeDamage(damage);
        }
    }

    private System.Collections.IEnumerator StunNPC()
    {
        isStunned = true; // ��������� �������� NPC
        agent.isStopped = true; // ������������� �������� NPC
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length); // ��� ��������� �������� ��������� �����
        isStunned = false; // ������������ ��������
        agent.isStopped = false; // ������������ ��������
    }

    private System.Collections.IEnumerator DelayedDamage()
    {
        // ��� ����������� ����� (��������, 0.5 �������)
        yield return new WaitForSeconds(0.5f);

        // ���������, ��������� �� ����� � ���� �����
        if (player != null && IsWithinAttackAngle(player.position))
        {
            SimpleCharacterController playerHealth = player.GetComponent<SimpleCharacterController>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }
}
