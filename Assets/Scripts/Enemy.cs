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
    bool returnAfterAttack = false;

    public abstract void Attack();

    public abstract void AttackAnimationComplete();

    public abstract void Patrol();

    public virtual void CheckForReturnDone()
    {
        if ((patrolInfo.startLoc - (Vector2)transform.position).magnitude < 50)
        {
            state = EnemyState.Patrolling;
        }
    }

    public virtual bool UpdateMove()
    {
        if (currentAggroTarget == null || currentAggroTarget.transform == null)
        {
            Deaggro();
            return false;
        }

        if (state == EnemyState.Aggro) //if you're aggro'd, wait until you've reached melee range, or the end of your patrol
        {
            if (((Vector2)transform.position - patrolInfo.startLoc).magnitude >= patrolInfo.patrolRadius)
            {
                Deaggro();
                return false;
            }

            float distAway = ((Vector2)currentAggroTarget.transform.position - (Vector2)transform.position).magnitude;
            if (distAway < 230)
            {
                GetComponent<NavMeshAgent>().velocity = Vector3.zero;
                return true; //proceed with trying to attack
            }
            else if (distAway < 250)
            {
                MoveTo(currentAggroTarget.transform.position);
                return true;
            }
            else
            {
                if (tunnelVision)
                    MoveTo(currentAggroTarget.transform.position);
                else
                {
                    GameObject go = attackableThings.OrderBy(x => Mathf.Abs((x.transform.position - transform.position).magnitude)).FirstOrDefault();
                    if (currentAggroTarget == go)
                    {
                        //a catch all... if you're too far away, but in patrol range, and you also can't move, just fucking attack fuck it
                        if (!MoveTo(currentAggroTarget.transform.position))
                        {
                            state = EnemyState.Attacking;
                        }
                    }
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

    public void Deaggro()
    {
        if (state != EnemyState.Attacking)
            state = EnemyState.Returning;
        else
            returnAfterAttack = true;

        MoveTo(patrolInfo.startLoc);//patrolInfo.currentPatrolPosition); //?
        currentAggroTarget = null;
    }


    public virtual bool MoveTo(Vector2 loc)
    {
        if (canGiveMoveCommand)
        {
            //StartCoroutine(CommandWasIssued());
            Vector2 test = loc - (Vector2)transform.position;
            if (test.magnitude < 20)
                test = test * 20 / test.magnitude;

            float percent = 1;
            while (percent > 0 && !GetComponent<NavMeshAgent>().SetDestination((Vector2)transform.position + (test * percent)))//new Vector3(loc.x, loc.y, transform.position.z)))
            {
                percent -= 0.2f;
                Vector2 a = (Vector2)currentAggroTarget.transform.position;
                Vector2 b = (Vector2)transform.position + (test * percent);
            }

            TurnTo(loc);

            if (percent > 0)
                return true;
        }
        return false;
    }

    public void TurnTo(Vector2 loc)
    {
        GetComponent<SpriteRenderer>().flipX = loc.x > transform.position.x;
        attackInfo.attackGO.GetComponent<SpriteRenderer>().flipX = loc.x > transform.position.x;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (state != EnemyState.Returning)
        {
            Pikmin p = collision.GetComponent<Pikmin>();
            Olimar o = collision.GetComponent<Olimar>();
            //Add to attackable things, if this is the first one, aggro to it.
            if (p != null)
                AddToAttackable(p.gameObject);
            else if (o != null)
                AddToAttackable(o.gameObject);
        }
    }

    private void AddToAttackable(GameObject go)
    {
        attackableThings.Add(go);
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
                    Deaggro();
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
                    Deaggro();
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
