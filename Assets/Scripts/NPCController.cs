using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    public Animator animator; // Ссылка на Animator
    public Transform player; // Ссылка на игрока
    public float detectionRange = 10f; // Радиус обнаружения игрока
    public float attackRange = 2f; // Радиус атаки
    public float fieldOfView = 60f; // Угол обзора NPC (в градусах)
    public float attackAngle = 30f; // Угол попадания атакой (в градусах)
    public float attackCooldown = 1.5f; // Время между атаками
    public int damage = 10; // Урон от NPC
    public int health = 100;

    private NavMeshAgent agent; // Ссылка на NavMeshAgent
    private float lastAttackTime; // Время последней атаки
    private bool isStunned = false; // Флаг для блокировки действий при получении урона

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (player == null || isStunned) return; // Если игрока нет или NPC ошеломлён, ничего не делаем

        // Расстояние до игрока
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange && CanSeePlayer())
        {
            // Если игрок в зоне видимости и не за стеной, следуем за ним
            if (distanceToPlayer > attackRange)
            {
                agent.SetDestination(player.position);
                animator.SetBool("IsRunning", true);
            }
            else
            {
                // Останавливаемся перед атакой
                agent.SetDestination(transform.position);
                animator.SetBool("IsRunning", false);

                // Атака игрока только если прошло достаточно времени
                if (Time.time - lastAttackTime >= attackCooldown && IsWithinAttackAngle(player.position))
                {
                    AttackPlayer();
                    lastAttackTime = Time.time;
                }
            }
        }
        else
        {
            // Если игрок вне зоны видимости, NPC останавливается
            agent.SetDestination(transform.position);
            animator.SetBool("IsRunning", false);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isStunned) return; // Если NPC уже ошеломлён, игнорируем

        health -= damage;

        if (health > 0)
        {
            Debug.Log("Npc получил урон: " + health);
            // Анимация получения урона
            animator.SetTrigger("TakeDamage");
            StartCoroutine(StunNPC()); // Запускаем временную блокировку действий
        }
        else
        {
            // Анимация смерти
            animator.SetTrigger("Die");
            Destroy(gameObject, 2f); // Уничтожаем объект через 2 секунды
        }
    }

    private bool CanSeePlayer()
    {
        // Направление к игроку
        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        // Проверяем угол между направлением NPC и направлением к игроку
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        // Если угол меньше половины угла обзора, проверяем наличие препятствий
        if (angleToPlayer <= fieldOfView / 2f)
        {
            Ray ray = new Ray(transform.position + Vector3.up, directionToPlayer); // Луч от NPC к игроку (немного выше земли)
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, detectionRange))
            {
                if (hit.transform == player)
                {
                    // Игрок виден
                    return true;
                }
            }
        }

        // Игрок вне поля зрения или за препятствием
        return false;
    }

    private bool IsWithinAttackAngle(Vector3 position)
    {
        // Направление к проверяемой позиции
        Vector3 directionToPosition = (position - transform.position).normalized;

        // Проверяем угол между направлением NPC и направлением к позиции
        float angleToPosition = Vector3.Angle(transform.forward, directionToPosition);

        // Возвращаем true, если позиция находится в угле атаки
        return angleToPosition <= attackAngle / 2f;
    }

    private void AttackPlayer()
    {
        // Анимация атаки
        animator.SetTrigger("Attack");

        // Наносим урон игроку (если он в угле атаки)
        SimpleCharacterController playerHealth = player.GetComponent<SimpleCharacterController>();
        if (playerHealth != null && IsWithinAttackAngle(player.position))
        {
            playerHealth.TakeDamage(damage);
        }
    }

    private System.Collections.IEnumerator StunNPC()
    {
        isStunned = true; // Блокируем действия NPC
        agent.isStopped = true; // Останавливаем движение NPC
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length); // Ждём окончания анимации получения урона
        isStunned = false; // Разблокируем действия
        agent.isStopped = false; // Возобновляем движение
    }

    private System.Collections.IEnumerator DelayedDamage()
    {
        // Ждём определённое время (например, 0.5 секунды)
        yield return new WaitForSeconds(0.5f);

        // Проверяем, находится ли игрок в угле атаки
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
