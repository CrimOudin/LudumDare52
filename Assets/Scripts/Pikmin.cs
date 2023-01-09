using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Pikmin : MonoBehaviour
{
    public PikminType PikminType;
    public Transform CarryLocation;
    public GameObject ActionHandObject;
    public GameObject ProgressBarPrefab;

    private PikminInfo info;

    private PikminState lastState = PikminState.Growing;
    public PikminState state { get; set; } = PikminState.Growing; //start a pikmin walking to formation
    public ResourceNode CurrentResourceNode { get; set; }
    public Enemy CurrentEnemy { get; set; }

    private ItemType? itemType;
    private int itemAmount;

    private int currentHealth;
    private NavMeshAgent navMeshAgent;
    private float lastTimeInteracted;
    private Collider2D collider;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private GameObject spawnedResourcePrefab;
    private float timeSpawned;
    private Slider progressBar;

    public Transform formationPositionTransform; //a transform set by the PikminFormation class to let this pikmin know exactly where to move to

    private bool goodtogo = false;

    private void Awake()
    {
        StartCoroutine(WaitThenSetInfo());
    }

    private void Start()
    {
        timeSpawned = Time.time;
        var bar = Instantiate(ProgressBarPrefab, transform);
        progressBar = bar.transform.GetChild(0).GetComponent<Slider>();
    }

    private IEnumerator WaitThenSetInfo()
    {
        while (PikminManager.Instance == null)
        {
            yield return new WaitForEndOfFrame();
        }
        info = PikminManager.Instance.GetPikminInfo(PikminType);
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateUpAxis = false;
        navMeshAgent.updatePosition = false;
        navMeshAgent.updateRotation = false;
        currentHealth = info.maxHealth;
        collider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        goodtogo = true;
        transform.rotation = Quaternion.identity; //why the fuck is this happening
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }

    void Update()
    {
        if (goodtogo)
        {
            //transform.position = new Vector2(navMeshAgent.nextPosition.x, navMeshAgent.nextPosition.y);
            GetComponent<Rigidbody2D>().MovePosition(new Vector2(navMeshAgent.nextPosition.x, navMeshAgent.nextPosition.y));

            switch (state)
            {
                case PikminState.Growing:
                    UpdateGrowthProgress();
                    break;
                case PikminState.DoneGrowing:
                    break;
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
                    if (CurrentEnemy != null)
                    {
                        PerformAttackTask(CurrentEnemy);
                    }
                    else if (!ActionHandObject.activeSelf)
                    {
                        state = PikminState.Idle;
                    }
                    break;
                case PikminState.Mining:
                    if (CurrentResourceNode != null)
                    {
                        PerformMiningTask(CurrentResourceNode);
                    }
                    else if (!ActionHandObject.activeSelf)
                    {
                        ReturnToFormation();
                    }
                    break;
                case PikminState.Dead:
                    break;
            }

            lastState = state;
        }
    }

    private void UpdateGrowthProgress()
    {
        var timeSinceSpawn = Time.time - timeSpawned;
        if (timeSinceSpawn > info.timeToBuild)
        {
            animator.SetBool("IsGrown", true);
            Destroy(progressBar.transform.parent.gameObject);
            state = PikminState.DoneGrowing;
        }
        else
        {
            float percent = timeSinceSpawn / info.timeToBuild;
            progressBar.value = percent;
        }
    }

    private void CheckForFinishedGoing()
    {
        if ((navMeshAgent.hasPath &&
            !navMeshAgent.isPathStale &&
            navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete &&
            Vector3.Distance(navMeshAgent.pathEndPosition, transform.position) <= navMeshAgent.stoppingDistance &&
            navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance) ||
            !navMeshAgent.isActiveAndEnabled)
        {
            state = PikminState.Idle;
            animator.SetBool("IsWalking", false);
        }
    }

    private bool CheckIfInRangeOfInteractable(bool defaultReturn = true)
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, info.IdleInteractRange, Vector2.zero);
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

        Vector3 closestPoint = Physics2D.ClosestPoint(transform.position, resourceNode.GetComponent<Collider2D>());
        Debug.DrawLine(transform.position, closestPoint);
        if (Vector2.Distance(transform.position, closestPoint) > info.ActionInteractionRange && navMeshAgent.isActiveAndEnabled)
        {
            // To far away we need to move closer
            navMeshAgent.SetDestination(new Vector3(closestPoint.x, closestPoint.y, 0));
            animator.SetBool("IsWalking", true);
            if (transform.position.x > closestPoint.x)
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
        else if (Time.time - lastTimeInteracted > info.ActionInteractionDelay)
        {
            animator.SetBool("IsWalking", false);
            animator.SetTrigger("Action");
            ActionHandObject.SetActive(true);
            ActionHandObject.transform.parent = null;
            ActionHandObject.transform.position = closestPoint;
            // We are close enough and can take a mining action.
            lastTimeInteracted = Time.time;
            if (itemAmount < info.maxItemCount &&
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
                if (itemAmount == info.maxItemCount)
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

    public bool PerformAttackTask(Enemy enemy)
    {
        if (state == PikminState.Dead) return false;

        if (enemy.health <= 0)
        {
            enemy.state = EnemyState.Death;
            CurrentEnemy = null;
            return false;
        }

        Vector3 closestPoint = Physics2D.ClosestPoint(transform.position, enemy.GetComponent<Collider2D>());
        if (Vector2.Distance(transform.position, closestPoint) > info.ActionInteractionRange && navMeshAgent.isActiveAndEnabled)
        {
            // To far away we need to move closer
            navMeshAgent.SetDestination(new Vector3(closestPoint.x, closestPoint.y, 0));
            animator.SetBool("IsWalking", true);
            if (transform.position.x > enemy.transform.position.x)
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
        else if (Time.time - lastTimeInteracted > info.ActionInteractionDelay)
        {
            animator.SetBool("IsWalking", false);
            animator.SetTrigger("Action");
            ActionHandObject.SetActive(true);
            ActionHandObject.transform.parent = null;
            ActionHandObject.transform.position = closestPoint;
            // We are close enough and can take a attack action.
            lastTimeInteracted = Time.time;


            if (enemy.health > 0)
            {
                enemy.health -= info.Damage;
                return true;
            }
            else
            {
                enemy.state = EnemyState.Death;
                CurrentEnemy = null;
                //ReturnToFormation();
            }
        }
        return false;
    }

    public void ReceiveCommand(Vector2 location)
    {
        if (state == PikminState.InFormation)
        {
            if (!navMeshAgent.isActiveAndEnabled)
            {
                navMeshAgent.enabled = true;
            }
            state = PikminState.Going;
            Manager.Instance.OlimarsPikminFormation.RemovePikmin(this);
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

        CurrentResourceNode = null;
        CurrentEnemy = null;

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
            if (state != PikminState.InFormation)
                Manager.Instance.OlimarsPikminFormation.PikminArrived(this);

            state = PikminState.InFormation;
        }
        else
        {
            state = PikminState.Returning;
            var position = formationPositionTransform.position;
            if (!navMeshAgent.isActiveAndEnabled)
            {
                navMeshAgent.enabled = true;
            }
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
        formationPositionTransform = Manager.Instance.OlimarsPikminFormation.GetPikminFormationPosition(this);
        var position = formationPositionTransform.position;
        if (Vector2.Distance(transform.position, position) >= 1f)
        {
            if (!navMeshAgent.isActiveAndEnabled)
            {
                animator.SetBool("IsWalking", false);
            }
            else
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
            formationPositionTransform = Manager.Instance.OlimarsPikminFormation.AddPikmin(this);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<InteractiveObject>())
        {
            navMeshAgent.enabled = false;
            StartCoroutine("ReEnableNavMesh");
        }
    }

    private IEnumerator ReEnableNavMesh()
    {
        yield return new WaitForSeconds(.05f);
        navMeshAgent.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.GetComponent<Recall>() != null) //.gameObject.TryGetComponent(out Recall recall))
        {
            if (state == PikminState.DoneGrowing)
            {
                StartCoroutine("Sprout");
            }
            else if (state != PikminState.Growing)
            {
                ReturnToFormation();
            }
        }
        if (collider.GetComponent<Damage>())
        {
            TakeDamage(collider.GetComponent<Damage>().Amount);
        }
    }

    IEnumerator Sprout()
    {
        animator.SetTrigger("Sprout");
        yield return new WaitForSeconds(1.5f);
        ReturnToFormation();
    }

    private void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        state = PikminState.Dead;
        Manager.Instance.OlimarsPikminFormation.RemovePikmin(this);
        Destroy(ActionHandObject);
        StartCoroutine("Death");
    }

    private IEnumerator Death()
    {
        yield return new WaitForSeconds(info.DeathAnimationTime);
        Destroy(gameObject);
    }
}

public enum PikminState
{
    Growing, // Still growing wait until done.
    DoneGrowing, // Waiting to be returned to formation
    Idle, //doing nothing, waiting for a command outside of your control
    Returning, //Heading to formation
    InFormation, //In the formation, will move according to formation rules
    Going, //Moving, heading away from formation
    Attacking, //Action from interacting with something that fights
    Mining, //Action from interacting with mine-able things
    Dead //If you died 
}
