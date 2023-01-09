using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour
{
    public abstract int health { get; }
    public EnemyState state;
    public EnemyAttackInfo attackInfo;
    public EnemyPatrolInfo patrolInfo;
    public GameObject currentAggroTarget;
    public List<GameObject> attackableThings = new List<GameObject>();
    public bool tunnelVision = false;
    public bool canGiveMoveCommand = true;


    public abstract void Attack();

    public abstract void AttackAnimationComplete();

    public abstract void Patrol();

    public virtual void CheckForReturnDone()
    {
        if ((patrolInfo.startLoc - (Vector2)transform.position).magnitude < 10)
        {
            state = EnemyState.Patrolling;
        }
    }

    public virtual bool UpdateMove()
    {
        if (state == EnemyState.Aggro) //if you're aggro'd, wait until you've reached melee range, or the end of your patrol
        {
            if (((Vector2)transform.position - patrolInfo.startLoc).magnitude >= patrolInfo.patrolRadius)
            {
                state = EnemyState.Returning;
                MoveTo(patrolInfo.startLoc); //patrolInfo.currentPatrolPosition); //?
            }
            if (((Vector2)currentAggroTarget.transform.position - (Vector2)transform.position).magnitude < 120)
            {
                return true; //proceed with trying to attack
            }
            else
            {
                if (tunnelVision)
                    MoveTo(currentAggroTarget.transform.position);
                else
                {
                    GameObject go = attackableThings.OrderBy(x => Mathf.Abs((x.transform.position - transform.position).magnitude)).FirstOrDefault();
                    if (currentAggroTarget == go)
                        MoveTo(currentAggroTarget.transform.position);
                    else
                    {
                        //see if someone else is closer first, then call this function again to see if you're close enough or need to move
                        currentAggroTarget = go;
                        TurnTo(currentAggroTarget.transform.position);
                        return UpdateMove();
                    }
                }
                return false;
            }
        }
        else
        {
            return true;
        }
    }


    public virtual void MoveTo(Vector2 loc)
    {
        if (canGiveMoveCommand)
        {
            StartCoroutine(CommandWasIssued());
            GetComponent<NavMeshAgent>().SetDestination(new Vector3(loc.x, loc.y, transform.position.z));
            TurnTo(loc);
        }
    }

    public void TurnTo(Vector2 loc)
    {
        GetComponent<SpriteRenderer>().flipX = loc.x > transform.position.x;
        attackInfo.attackGO.GetComponent<SpriteRenderer>().flipX = loc.x > transform.position.x;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Pikmin p = collision.GetComponent<Pikmin>();
        Olimar o = collision.GetComponent<Olimar>();
        //Add to attackable things, if this is the first one, aggro to it.
        if (p != null)
        {
            attackableThings.Add(p.gameObject);
        }
        else if (o != null)
        {
            attackableThings.Add(o.gameObject);
        }
        if (attackableThings.Count == 1)
        {
            state = EnemyState.Aggro;
            currentAggroTarget = attackableThings[0];
            MoveTo((Vector2)currentAggroTarget.transform.position);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Pikmin p = collision.GetComponent<Pikmin>();
        Olimar o = collision.GetComponent<Olimar>();
        //remove if in the list of attackable things.  If this was your current target, try finding another one, else set to null and set state to Returning
        if (p != null)
        {
            attackableThings.Remove(p.gameObject);
            if (currentAggroTarget == p.gameObject)
            {
                if (attackableThings.Count > 0)
                    currentAggroTarget = attackableThings.OrderBy(x => Mathf.Abs((x.transform.position - transform.position).magnitude)).FirstOrDefault();
                else
                {
                    state = EnemyState.Returning;
                    MoveTo(patrolInfo.startLoc);
                    currentAggroTarget = null;
                }
            }
        }
        else if (o != null)
        {
            attackableThings.Remove(o.gameObject);
            if (currentAggroTarget == o.gameObject)
            {
                if (attackableThings.Count > 0)
                    currentAggroTarget = attackableThings.OrderBy(x => Mathf.Abs((x.transform.position - transform.position).magnitude)).FirstOrDefault();
                else
                {
                    state = EnemyState.Returning;
                    MoveTo(patrolInfo.startLoc);
                    currentAggroTarget = null;
                }
            }
        }

    }

    private IEnumerator CommandWasIssued()
    {
        canGiveMoveCommand = false;
        yield return new WaitForSeconds(1f);
        canGiveMoveCommand = true;
        yield break;
    }
}

public enum EnemyState
{
    Patrolling,
    Aggro,
    Attacking,
    Returning
}

[Serializable]
public class EnemyAttackInfo
{
    public float attackRange; //how far away you can attack
    public float attackCooldown;

    public AttackState state; //to know what your currentCooldown is tracking (winduptime, attackCooldown, etc)
    public float currentCooldown; //if you're on cooldown, how long you've been waiting 

    public float attackWindupTime; //time the attack is telegraphed before it actually attacks (to give you time to react)
    public float attackDuration;

    public GameObject attackGO; //the thing that says where the attack will be, and has the collider for the damage, and animator
}

public enum AttackState
{
    None,
    Windup,
    Attacking
}

[Serializable]
public class EnemyPatrolInfo
{
    [HideInInspector]
    public float current; //to track how long you've been in this patrol
    [HideInInspector]
    public float activePatrolTime; //duration + variance chosen for this patrol
    [HideInInspector]
    public Vector2 startLoc; //where the gameobject started
    [HideInInspector]
    public Vector2 currentPatrolPosition;


    public float patrolDuration; //base duration
    public float patrolVariance; //allowed variance 
    public float patrolRadius; //how far you can move around your start location
}
