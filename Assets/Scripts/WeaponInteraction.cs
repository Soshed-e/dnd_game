// Weapon interaction script
using UnityEngine;
using UnityEngine.AI;

public class WeaponInteractionController : MonoBehaviour
{
    public float interactionRadius = 2f;
    public Transform interactionPoint;
    public GameObject currentWeapon;
    public Transform weaponSlot;

    private NavMeshAgent navMeshAgent;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.enabled = false;
    }

    public void CheckForInteraction()
    {
        if (interactionPoint == null || Vector3.Distance(transform.position, interactionPoint.position) > interactionRadius)
            return;

        StartCoroutine(InteractWithWeapon());
    }

    private System.Collections.IEnumerator InteractWithWeapon()
    {
        navMeshAgent.enabled = true;
        navMeshAgent.SetDestination(interactionPoint.position);

        while (navMeshAgent.remainingDistance > 0.5f)
        {
            yield return null;
        }

        navMeshAgent.isStopped = true;
        AttachWeaponToHand(currentWeapon);
        navMeshAgent.enabled = false;
    }

    private void AttachWeaponToHand(GameObject weapon)
    {
        if (weaponSlot != null)
        {
            weapon.transform.SetParent(weaponSlot);
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;

            Rigidbody rb = weapon.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            Collider col = weapon.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }
    }
}
