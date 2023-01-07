using System;
using System.Collections;
using System.Runtime.CompilerServices;
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
    private PikminState lastState = PikminState.Idle;
    public PikminState state { get; set; } = PikminState.Returning; //start a pikmin walking to formation

    private ItemType? itemType;
    private int itemAmount;

    private int currentHealth;
    private bool isDead;
    private NavMeshAgent navMeshAgent;
    private bool hasPath = false;

    public Transform formationPositionTransform; //a transform set by the PikminFormation class to let this pikmin know exactly where to move to

    private void Awake()
    {

        navMeshAgent = GetComponent<NavMeshAgent>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead) return;

        switch (state)
        {
            case PikminState.Idle:
                CheckIfInRangeOfInteractable(false);
                break;
            case PikminState.Returning:
                ReturnToFormation();
                break;
            case PikminState.InFormation:
                if(lastState == PikminState.Returning)
                {
                    //todo: return any resources you were carrying
                }
                //continue to walk in formation, rotating to your target (if not a circle)
                ReturnToFormation();
                break;
            case PikminState.Going:
                //Shouldn't be able to receive any commands here, just continuing to follow the move command
                break;
            case PikminState.Attacking:
                break;
            case PikminState.Mining:
                break;
        }
        //if (IsIdle)
        //{
        //    CheckIfInRangeOfInteractable();
        //}
        //else
        //{
        //    CheckIfStoppedMoving();
        //}

        lastState = state;
    }

    private void CheckIfStoppedMoving()
    {
        if (hasPath && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + .01f)
        {
            IsIdle = true;
            hasPath = false;
            if (Manager.Instance.OlimarsPikmanFormation.PikminReturning.Contains(this))
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

    private bool CheckIfInRangeOfInteractable(bool defaultReturn = true)
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

        if (defaultReturn)
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
        if (state == PikminState.InFormation)
        {
            state = PikminState.Going;
            Manager.Instance.OlimarsPikmanFormation.RemovePikmin(this);
            navMeshAgent.SetDestination(location);
            hasPath = true;
            //Manager.Instance.OlimarsPikmanFormation.PikminInFormation.Remove(this);
        }
    }

    private void ReturnToFormation()
    {
        state = PikminState.Returning;
        if (isDead) return;
        IsIdle = false;

        AddMeToFormation(); //does nothing if in formation already

        if (((Vector2)transform.position - (Vector2)formationPositionTransform.position).magnitude <= 20f)
            state = PikminState.InFormation;

        navMeshAgent.SetDestination(formationPositionTransform.position);// Manager.Instance.OlimarsPikmanFormation.gameObject.transform.position);
        hasPath = true;
        Manager.Instance.OlimarsPikmanFormation.PikminReturning.Add(this);
    }

    public void AddMeToFormation()
    {
        //If you're not already in the formation, tell the formation you're now in formation, and have it return where you should go to.
        //Otherwise do nothing
        if (formationPositionTransform == null)
            formationPositionTransform = Manager.Instance.OlimarsPikmanFormation.AddPikmin(this);
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
        if (collision.GetComponent<Recall>() != null) //.gameObject.TryGetComponent(out Recall recall))
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

public enum PikminState
{
    Idle, //doing nothing, waiting for a command outside of your control
    Returning, //Heading to formation
    InFormation, //In the formation, will move according to formation rules
    Going, //Moving, heading away from formation
    Attacking, //Action from interacting with something that fights
    Mining, //Action from interacting with mine-able things
}
