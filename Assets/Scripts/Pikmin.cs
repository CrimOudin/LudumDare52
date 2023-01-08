using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class Pikmin : MonoBehaviour
{
    public float IdleInteractRange = 3f;
    public float ActionInteractionRange = 1f;
    public float ActionInteractionDelay = 5f;
    public int maxHealth;
    public PikminType PikminType;
    public float DeathAnimationTime;
    public int maxItemCount;
    public Transform CarryLocation;
    public GameObject ActionHandObject;

    private PikminState lastState = PikminState.Idle;
    public PikminState state { get; set; } = PikminState.Returning; //start a pikmin walking to formation
    public ResourceNode CurrentResourceNode { get; set; }

    private ItemType? itemType;
    private int itemAmount;

    private int currentHealth;
    private NavMeshAgent navMeshAgent;
    private float lastTimeInteracted;
    private Collider2D collider;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private GameObject spawnedResourcePrefab;

    public Transform formationPositionTransform; //a transform set by the PikminFormation class to let this pikmin know exactly where to move to

    private void Awake()
    {

        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateUpAxis = false;
        navMeshAgent.updatePosition = false;
        navMeshAgent.updateRotation = false;
        currentHealth = maxHealth;
        collider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        transform.position = new Vector2(navMeshAgent.nextPosition.x, navMeshAgent.nextPosition.y);
        if (lastState == PikminState.Dead) return;

        switch (state)
        {
            case PikminState.Idle:
                CheckIfInRangeOfInteractable(false);
                break;
            case PikminState.Returning:
                ReturnToFormation();
                break;
            case PikminState.InFormation:
                StayInFormation();
                break;
            case PikminState.Going:
                //Shouldn't be able to receive any commands here, just continuing to follow the move command
                CheckForFinishedGoing();
                break;
            case PikminState.Attacking:
                break;
            case PikminState.Mining:
                if (CurrentResourceNode != null)
                {
                    PerformMiningTask(CurrentResourceNode);
                }
                else if(!ActionHandObject.activeSelf)
                {
                    ReturnToFormation();
                }
                break;
            case PikminState.Dead:
                break;
        }

        lastState = state;
    }

    private void CheckForFinishedGoing()
    {
        if (navMeshAgent.hasPath &&
            !navMeshAgent.isPathStale &&
            navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete &&
            Vector3.Distance(navMeshAgent.pathEndPosition, transform.position) <= navMeshAgent.stoppingDistance &&
            navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            state = PikminState.Idle;
            animator.SetBool("IsWalking", false);
        }
    }

    private bool CheckIfInRangeOfInteractable(bool defaultReturn = true)
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, IdleInteractRange, Vector2.zero);
        hits = hits.OrderBy(x => Vector2.Distance(x.collider.gameObject.transform.position, transform.position)).ToArray();
        foreach (var possibleInteractiveObject in hits)
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

    /// Returns true if a resource was mined
    public bool PerformMiningTask(ResourceNode resourceNode)
    {
        if (state == PikminState.Dead) return false;

        Vector3 closestPoint = Physics2D.ClosestPoint(collider.bounds.center, resourceNode.GetComponent<Collider2D>());
        if (Vector2.Distance(transform.position, closestPoint) > ActionInteractionRange)
        {
            // To far away we need to move closer
            navMeshAgent.SetDestination(new Vector3(closestPoint.x, closestPoint.y, 0));
            animator.SetBool("IsWalking", true);
            if(transform.position.x > resourceNode.transform.position.x)
            {
                //moving left
                spriteRenderer.flipX = true;
            }
            else 
            {
                //moving right
                spriteRenderer.flipX = false;
            }
        }
        else if (Time.time - lastTimeInteracted > ActionInteractionDelay)
        {
            animator.SetBool("IsWalking", false);
            animator.SetTrigger("Action");
            ActionHandObject.SetActive(true);
            ActionHandObject.transform.parent = null;
            ActionHandObject.transform.position = closestPoint;
            // We are close enough and can take a mining action.
            lastTimeInteracted = Time.time;
            if (itemAmount < maxItemCount &&
                resourceNode.ResourceTotalAmount > 0)
            {
                itemType = resourceNode.ResourceType;
                itemAmount++;
                if (spawnedResourcePrefab == null)
                {
                    animator.SetBool("IsCarrying", true);
                    spawnedResourcePrefab = Instantiate(resourceNode.ResourcePrefab, CarryLocation);
                }
                resourceNode.ResourceTotalAmount -= 1;
                if (itemAmount == maxItemCount)
                {
                    CurrentResourceNode = null;
                    //ReturnToFormation();
                }

                if (resourceNode.ResourceTotalAmount <= 0)
                {
                    resourceNode.HandleDepleted();
                }

                return true;
            }
            else
            {
                CurrentResourceNode = null;
                //ReturnToFormation();
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
            navMeshAgent.SetDestination(new Vector3(location.x, location.y, 0));
            animator.SetBool("IsWalking", true);
            if (transform.position.x > location.x)
            {
                //moving left
                spriteRenderer.flipX = true;
            }
            else
            {
                //moving right
                spriteRenderer.flipX = false;
            }
        }
    }

    private void ReturnToFormation()
    {
        if (state == PikminState.Dead) return;


        AddMeToFormation(); //does nothing if in formation already

        if (Vector2.Distance(transform.position, formationPositionTransform.position) <= 20f)
        {
            formationPositionTransform = null;
            if (itemType.HasValue)
            {
                Manager.Instance.AddResource(itemType.Value, itemAmount);
                itemType = null;
                itemAmount = 0;
                animator.SetBool("IsCarrying", false);
                Destroy(spawnedResourcePrefab);
            }
            state = PikminState.InFormation;
        }
        else
        {
            state = PikminState.Returning;
            var position = formationPositionTransform.position;
            navMeshAgent.SetDestination(new Vector3(position.x, position.y, 0));// Manager.Instance.OlimarsPikmanFormation.gameObject.transform.position);
            animator.SetBool("IsWalking", true);
            if (transform.position.x > position.x)
            {
                //moving left
                spriteRenderer.flipX = true;
            }
            else
            {
                //moving right
                spriteRenderer.flipX = false;
            }
        }
    }

    private void StayInFormation()
    {
        formationPositionTransform = Manager.Instance.OlimarsPikmanFormation.GetPikminFormationPosition(this);
        var position = formationPositionTransform.position;
        if (Vector2.Distance(transform.position, position) >= 1f)
        {
            navMeshAgent.SetDestination(new Vector3(position.x, position.y, 0));
            animator.SetBool("IsWalking", true);
            if (transform.position.x > position.x)
            {
                //moving left
                spriteRenderer.flipX = true;
            }
            else
            {
                //moving right
                spriteRenderer.flipX = false;
            }
        }
        else
        {
            animator.SetBool("IsWalking", false);
        }
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
        StartCoroutine("Death");
    }

    private IEnumerator Death()
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
    Dead //If you died 
}
