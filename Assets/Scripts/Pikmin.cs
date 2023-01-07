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

    public PikminFormation PikminFormation { get; set; }

    public bool IsIdle { get; set; } = true;

    private ItemType itemType;
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
            CheckIfMoving();
        }

    }

    private void CheckIfMoving()
    {
        hasPath |= navMeshAgent.hasPath;
        if (hasPath && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + .01f)
        {
            IsIdle = true;
            hasPath = false;
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
        }
        else
        {
            navMeshAgent.SetDestination(transform.position);
            if(itemAmount < maxItemCount)
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
        Manager.Instance.OlimarsPikmanFormation.PikminInFormation.Remove(this);
    }

    private void ReturnToFormation()
    {
        if(isDead) return;
        IsIdle = false;
        //TODO: add to pikimin returning list.
        Manager.Instance.OlimarsPikmanFormation.PikminInFormation.Add(this);
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
