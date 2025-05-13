using UnityEngine;
using UnityEngine.AI;

public class SimpleCharacterController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f; // Скорость передвижения
    public float rotationSpeed = 720f; // Скорость вращения
    public Rigidbody rb; // Ссылка на Rigidbody

    [Header("Combat Settings")]
    public float hitRadius = 1f; // Радиус атаки
    public int health = 100; // Здоровье игрока

    [Header("Interaction Settings")]
    public float interactionRadius = 2f; // Радиус взаимодействия
    public Transform interactionPoint; // Точка взаимодействия с мечом
    public GameObject currentWeapon; // Текущий меч
    public Transform weaponSlot; // Укажите WeaponSlot вручную в инспекторе

    [Header("References")]
    public Animator animator; // Ссылка на Animator
    private NavMeshAgent navMeshAgent; // Для движения к объектам

    private Vector3 movementInput;
    private bool isRunning;
    private bool isStunned = false; // Флаг, указывающий, что игрок ошеломлён (анимация получения урона)
    private bool isInteracting = false; // Флаг, идет ли взаимодействие
    private bool hasWeapon = false; // Флаг, идет ли взаимодействие


    void Start()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        // Сначала выключаем NavMeshAgent
        navMeshAgent.enabled = false;
    }

    void Update()
    {
        if (isStunned || isInteracting) return; // Блокируем действия при взаимодействии или ошеломлении

        // Движение игрока
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        movementInput = new Vector3(horizontal, 0, vertical).normalized;
        isRunning = movementInput.magnitude > 0;

        animator.SetBool("IsRunning", isRunning);

        // Проверка на взаимодействие
        if (Input.GetKeyDown(KeyCode.E))
        {
            CheckForInteraction();
        }

        // Атака (только если игрок не двигается)
        if (!isRunning)
        {
            if (Input.GetKeyDown(KeyCode.Z)) // Левая кнопка мыши
            {
                animator.SetTrigger("Attack1");
                PerformAttack(10); // Наносим урон
            }

            if (Input.GetKeyDown(KeyCode.X)) // Правая кнопка мыши
            {
                animator.SetTrigger("Attack2");
                PerformAttack(50); // Сильная атака
            }
        }
    }

    void FixedUpdate()
    {
        if (isStunned || isInteracting || !isRunning) return;

        // Движение персонажа
        Vector3 move = movementInput * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        // Поворот персонажа в направлении движения
        Quaternion targetRotation = Quaternion.LookRotation(movementInput);
        rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
    }

    private void CheckForInteraction()
    {
        Debug.Log("E нажата");
        //interactionPoint = GameObject.Find("InteractionPoint");
        // Если InteractionPoint не задан или игрок слишком далеко, ничего не делаем
        if (interactionPoint == null || Vector3.Distance(transform.position, interactionPoint.position) > interactionRadius)
        {
            Debug.Log("точка далеко, или ее нет");
            return;
        }
        else if (interactionPoint != null)
        {
            // currentWeapon = GameObject.Find("Axe_Weapon");
            StartCoroutine(InteractWithWeapon());
        }

    }


    private System.Collections.IEnumerator InteractWithWeapon()
    {
        float delayTime = 0.5f;

        isInteracting = true;

        // Включаем NavMeshAgent
        navMeshAgent.enabled = true;

        // Устанавливаем цель для движения
        navMeshAgent.SetDestination(interactionPoint.position);

        // Включаем анимацию бега
        animator.SetBool("IsRunning", true);

        // Ждем, пока игрок достигнет точки взаимодействия
        while (navMeshAgent.remainingDistance > 0.5f || navMeshAgent.velocity.magnitude > 0.1f)
        {
            yield return null;
        }
        Debug.Log("Rasstojanie: " + navMeshAgent.remainingDistance);

        // Останавливаем NavMeshAgent
        navMeshAgent.isStopped = true;

        // Отключаем анимацию бега
        animator.SetBool("IsRunning", false);

        // Разворачиваем игрока к мечу
        StartCoroutine(RotateToTarget(currentWeapon.transform));


        // Запускаем анимацию поднятия меча
        animator.SetTrigger("PickUp");

        delayTime += animator.GetCurrentAnimatorStateInfo(0).length;

        // Ждем окончания анимации
        yield return new WaitForSeconds(delayTime);
        Debug.Log("Ожидание анимации закончилось");

        // Привязываем меч к руке
        AttachWeaponToHand(currentWeapon);

        // Выключаем NavMeshAgent
        navMeshAgent.enabled = false;

        isInteracting = false;

        animator.SetBool("HasWeapon", hasWeapon);

    }







    private void AttachWeaponToHand(GameObject weapon)
    {
        Debug.Log("Запустилась функция AttachWeaponToHand");

        if (weaponSlot != null)
        {
            Debug.Log("WeaponSlot найден: " + weaponSlot.name);

            weapon.transform.SetParent(weaponSlot);
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;

            // Отключаем физику меча
            Rigidbody rb = weapon.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            Collider col = weapon.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            Debug.Log("Оружие прикреплено к WeaponSlot");
            hasWeapon = true;
        }
        else
        {
            Debug.LogError("WeaponSlot не найден!");
        }
    }


    private void PerformAttack(int damage)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, hitRadius);
        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("NPC"))
            {
                NPCController npc = collider.GetComponent<NPCController>();
                if (npc != null)
                {
                    npc.TakeDamage(damage);
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isStunned) return;

        health -= damage;

        if (health > 0)
        {
            animator.SetTrigger("Hit");
            StartCoroutine(StunPlayer());
        }
        else
        {
            animator.SetTrigger("Die");
            Debug.Log("Игрок погиб.");
        }
    }

    private System.Collections.IEnumerator StunPlayer()
    {
        isStunned = true;
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        isStunned = false;
    }

    private System.Collections.IEnumerator RotateToTarget(Transform target)
    {
        // Рассчитываем направление к цели
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0; // Игнорируем вертикальную составляющую

        if (direction.magnitude > 0)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);

            // Плавный разворот
            while (Quaternion.Angle(transform.rotation, lookRotation) > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 2f); // Уменьшили скорость
                yield return null;
            }

            // Точный финальный угол
            transform.rotation = lookRotation;
        }
    }



    /* private void OnDrawGizmosSelected()
     {
         Gizmos.color = Color.yellow;
         Gizmos.DrawWireSphere(transform.position, interactionRadius);
         Gizmos.color = Color.red;
         Gizmos.DrawWireSphere(transform.position, hitRadius);
     }*/
}