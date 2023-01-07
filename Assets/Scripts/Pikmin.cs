using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Pikmin : MonoBehaviour
{
    public float IdleInteractRange = 3f;
    public float ActionInteractionRange = 1f;
    public int maxHealth;
    public PikminType PikminType;
    public float DeathAnimationTime;
    public int maxItemCount;

    public bool IsIdle { get; set; } = true;

    private ItemType? itemType;
    private int itemAmount;

    private int currentHealth;
    private bool isDead;
    private NavMeshAgent navMeshAgent;
    private bool hasPath = false;

    private void Awake()
    {

        navMeshAgent = GetComponent<NavMeshAgent>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        if(isDead) return;

        if (IsIdle)
        {
            CheckIfInRangeOfInteractable();
        }
        else
        {
            CheckIfStoppedMoving();
        }

    }

    private void CheckIfStoppedMoving()
    {
        if (hasPath && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + .01f)
        {
            IsIdle = true;
            hasPath = false;
            if(Manager.Instance.OlimarsPikmanFormation.PikminReturning.Contains(this))
            {
                Manager.Instance.OlimarsPikmanFormation.PikminReturning.Remove(this);
                Manager.Instance.OlimarsPikmanFormation.PikminInFormation.Add(this);

                // Drop any items off to the player that this Pikmin had.
                if (itemType.HasValue)
                {
                    Manager.Instance.totalItems[itemType.Value] += itemAmount;
                    itemType = null;
                    itemAmount = 0;
                }
            }
        }
    }

    private bool CheckIfInRangeOfInteractable()
    {
        RaycastHit2D[] hit = Physics2D.CircleCastAll(transform.position, IdleInteractRange, Vector2.zero);
        foreach (var possibleInteractiveObject in hit)
        {
            if (possibleInteractiveObject.collider.gameObject.TryGetComponent(out InteractiveObject interactiveObject))
            {
                if (interactiveObject.pikminTypesAllowed.Contains(PikminType))
                {
                    interactiveObject.OnPikminInteract(this);
                    return true;
                }
            }
        }
        ReturnToFormation();
        return false;
    }

    /// Returns true if the mine was mined
    public bool PerformMiningTask(Vector2 minePosition, ItemType resourceType)
    {
        if (isDead) return false;
        if (Vector2.Distance(transform.position, minePosition) > ActionInteractionRange)
        {
            navMeshAgent.SetDestination(minePosition);
            hasPath = true;
        }
        else
        {
            navMeshAgent.SetDestination(transform.position);
            hasPath = true;
            if (itemAmount < maxItemCount)
            {
                itemType = resourceType;
                itemAmount++;
                return true;
            }
        }
        return false;
    }

    public void ReceiveCommand(Vector2 location)
    {
        navMeshAgent.SetDestination(location);
        hasPath = true;
        Manager.Instance.OlimarsPikmanFormation.PikminInFormation.Remove(this);
    }

    private void ReturnToFormation()
    {
        if(isDead) return;
        IsIdle = false;
        navMeshAgent.SetDestination(Manager.Instance.OlimarsPikmanFormation.gameObject.transform.position);
        hasPath = true;
        Manager.Instance.OlimarsPikmanFormation.PikminReturning.Add(this);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Damage"))
        {
            TakeDamage(collision.gameObject.GetComponent<Damage>().Amount);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.TryGetComponent(out Recall recall))
        {
            ReturnToFormation();
        }
    }

    private void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0)
        {
            currentHealth = 0;
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        Manager.Instance.OlimarsPikmanFormation.PikminInFormation.Remove(this);
        Manager.Instance.OlimarsPikmanFormation.PikminReturning.Remove(this);
        isDead = true;
        StartCoroutine("Death");
    }

    IEnumerable Death()
    {
        yield return new WaitForSeconds(DeathAnimationTime);
        Destroy(gameObject);
    }
}
