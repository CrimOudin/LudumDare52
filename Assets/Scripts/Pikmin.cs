using System;
using System.Collections;
using UnityEngine;

public class Pikmin : MonoBehaviour
{
    public float IdleInteractRange = 3f;
    public float ActionInteractionRange = 1f;
    public int maxHealth;
    public PikminType PikminType;
    public float DeathAnimationTime;

    public PikminFormation PikminFormation { get; set; }

    public bool IsIdle { get; set; }

    Vector2 TargetLocation;
    ItemType ItemType;
    int itemAmount;

    private int currentHealth;
    private bool isDead;
    private PikminActionState pikminActionState;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if(isDead) return;

        if (IsIdle)
        {
            CheckIfInRangeOfInteractable();
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
        return false;
    }

    private void PerformMiningTask(Vector2 minePosition)
    {
        if (Vector2.Distance(transform.position, minePosition) > ActionInteractionRange)
        {
            // Move Towards
        }
        else
        {
            // Mine 
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Damage"))
        {
            TakeDamage(collision.gameObject.GetComponent<Damage>().Amount);
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
        // TODO: handle game reset/player respawn.
        isDead = true;
        StartCoroutine("Death");
    }

    IEnumerable Death()
    {
        yield return new WaitForSeconds(DeathAnimationTime);
        Destroy(gameObject);
    }
}

public enum PikminActionState
{
    NotThere,
    There,

}
