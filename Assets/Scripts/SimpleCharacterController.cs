using UnityEngine;
using UnityEngine.AI;

public class SimpleCharacterController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f; // �������� ������������
    public float rotationSpeed = 720f; // �������� ��������
    public Rigidbody rb; // ������ �� Rigidbody

    [Header("Combat Settings")]
    public float hitRadius = 1f; // ������ �����
    public int health = 100; // �������� ������

    [Header("Interaction Settings")]
    public float interactionRadius = 2f; // ������ ��������������
    public Transform interactionPoint; // ����� �������������� � �����
    public GameObject currentWeapon; // ������� ���
    public Transform weaponSlot; // ������� WeaponSlot ������� � ����������

    [Header("References")]
    public Animator animator; // ������ �� Animator
    private NavMeshAgent navMeshAgent; // ��� �������� � ��������

    private Vector3 movementInput;
    private bool isRunning;
    private bool isStunned = false; // ����, �����������, ��� ����� �������� (�������� ��������� �����)
    private bool isInteracting = false; // ����, ���� �� ��������������
    private bool hasWeapon = false; // ����, ���� �� ��������������


    void Start()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        // ������� ��������� NavMeshAgent
        navMeshAgent.enabled = false;
    }

    void Update()
    {
        if (isStunned || isInteracting) return; // ��������� �������� ��� �������������� ��� �����������

        // �������� ������
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        movementInput = new Vector3(horizontal, 0, vertical).normalized;
        isRunning = movementInput.magnitude > 0;

        animator.SetBool("IsRunning", isRunning);

        // �������� �� ��������������
        if (Input.GetKeyDown(KeyCode.E))
        {
            CheckForInteraction();
        }

        // ����� (������ ���� ����� �� ���������)
        if (!isRunning)
        {
            if (Input.GetKeyDown(KeyCode.Z)) // ����� ������ ����
            {
                animator.SetTrigger("Attack1");
                PerformAttack(10); // ������� ����
            }

            if (Input.GetKeyDown(KeyCode.X)) // ������ ������ ����
            {
                animator.SetTrigger("Attack2");
                PerformAttack(50); // ������� �����
            }
        }
    }

    void FixedUpdate()
    {
        if (isStunned || isInteracting || !isRunning) return;

        // �������� ���������
        Vector3 move = movementInput * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        // ������� ��������� � ����������� ��������
        Quaternion targetRotation = Quaternion.LookRotation(movementInput);
        rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
    }

    private void CheckForInteraction()
    {
        Debug.Log("E ������");
        //interactionPoint = GameObject.Find("InteractionPoint");
        // ���� InteractionPoint �� ����� ��� ����� ������� ������, ������ �� ������
        if (interactionPoint == null || Vector3.Distance(transform.position, interactionPoint.position) > interactionRadius)
        {
            Debug.Log("����� ������, ��� �� ���");
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

        // �������� NavMeshAgent
        navMeshAgent.enabled = true;

        // ������������� ���� ��� ��������
        navMeshAgent.SetDestination(interactionPoint.position);

        // �������� �������� ����
        animator.SetBool("IsRunning", true);

        // ����, ���� ����� ��������� ����� ��������������
        while (navMeshAgent.remainingDistance > 0.5f || navMeshAgent.velocity.magnitude > 0.1f)
        {
            yield return null;
        }
        Debug.Log("Rasstojanie: " + navMeshAgent.remainingDistance);

        // ������������� NavMeshAgent
        navMeshAgent.isStopped = true;

        // ��������� �������� ����
        animator.SetBool("IsRunning", false);

        // ������������� ������ � ����
        StartCoroutine(RotateToTarget(currentWeapon.transform));


        // ��������� �������� �������� ����
        animator.SetTrigger("PickUp");

        delayTime += animator.GetCurrentAnimatorStateInfo(0).length;

        // ���� ��������� ��������
        yield return new WaitForSeconds(delayTime);
        Debug.Log("�������� �������� �����������");

        // ����������� ��� � ����
        AttachWeaponToHand(currentWeapon);

        // ��������� NavMeshAgent
        navMeshAgent.enabled = false;

        isInteracting = false;

        animator.SetBool("HasWeapon", hasWeapon);

    }







    private void AttachWeaponToHand(GameObject weapon)
    {
        Debug.Log("����������� ������� AttachWeaponToHand");

        if (weaponSlot != null)
        {
            Debug.Log("WeaponSlot ������: " + weaponSlot.name);

            weapon.transform.SetParent(weaponSlot);
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;

            // ��������� ������ ����
            Rigidbody rb = weapon.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            Collider col = weapon.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            Debug.Log("������ ����������� � WeaponSlot");
            hasWeapon = true;
        }
        else
        {
            Debug.LogError("WeaponSlot �� ������!");
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
            Debug.Log("����� �����.");
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
        // ������������ ����������� � ����
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0; // ���������� ������������ ������������

        if (direction.magnitude > 0)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);

            // ������� ��������
            while (Quaternion.Angle(transform.rotation, lookRotation) > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 2f); // ��������� ��������
                yield return null;
            }

            // ������ ��������� ����
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