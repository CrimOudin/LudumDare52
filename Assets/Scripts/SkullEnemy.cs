using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SkullEnemy : Enemy
{
    public override int health => 420;


    private void Awake()
    {
        attackInfo.attackGO.GetComponent<OneTimeAnimationHandler>().endAction = AttackAnimationComplete;
        patrolInfo.startLoc = transform.position;

        GetComponent<NavMeshAgent>().updateUpAxis = false;
        GetComponent<NavMeshAgent>().updatePosition = false;
        GetComponent<NavMeshAgent>().updateRotation = false;

        state = EnemyState.Patrolling;
    }

    private void Update()
    {
        transform.position = new Vector2(GetComponent<NavMeshAgent>().nextPosition.x, GetComponent<NavMeshAgent>().nextPosition.y);

        if (state == EnemyState.Patrolling)
        {
            Patrol();
        }
        else if (state == EnemyState.Aggro)
        {
            Attack();
        }
        else if (state == EnemyState.Returning)
        {
            CheckForReturnDone();
        }
    }

    public override void Attack()
    {
        //todo: actually target pikmin.  If no pikmin in range, check if pikmin in range.
        //                              If none, Return().  Else Move to new location to attack new target

        //MoveTo(currentAggroTarget.transform.position);
        if (UpdateMove()) //if UpdateMove says you're good to attack (returns true)
        {
            //If you're just starting off, check if you're off attack cooldown
            if (attackInfo.state == AttackState.None)
            {
                if (attackInfo.currentCooldown < attackInfo.attackCooldown)
                {
                    attackInfo.currentCooldown += Time.deltaTime;
                }
                else
                {
                    state = EnemyState.Attacking;
                    attackInfo.attackGO.transform.position = transform.position + new Vector3(150, 0, 0); //todo: replace with target pikmin's position (not z)
                    GetComponent<Animator>().SetBool("Attacking", true);
                    attackInfo.attackGO.GetComponent<SpriteRenderer>().enabled = true;
                    attackInfo.attackGO.GetComponent<Animator>().SetBool("Attack", false); //just in case
                    attackInfo.state = AttackState.Windup;
                    attackInfo.currentCooldown = 0;
                }
            }
            //Once you're done with cooldown, perform an attack windup
            else if (attackInfo.state == AttackState.Windup)
            {
                attackInfo.currentCooldown += Time.deltaTime;
                if (attackInfo.currentCooldown >= attackInfo.attackWindupTime)
                {
                    attackInfo.state = AttackState.Attacking;
                    attackInfo.attackGO.GetComponent<Animator>().SetBool("Attack", true);
                    attackInfo.currentCooldown = 0;
                    attackInfo.attackGO.GetComponent<BoxCollider2D>().enabled = true;
                }
            }
            //After windup, attack
            else if (attackInfo.state == AttackState.Attacking)
            {
                attackInfo.currentCooldown += Time.deltaTime;
                if (attackInfo.attackGO.GetComponent<BoxCollider2D>().isActiveAndEnabled && attackInfo.currentCooldown >= attackInfo.attackDuration)
                {
                    attackInfo.attackGO.GetComponent<BoxCollider2D>().enabled = false;
                }
            }
        }
    }

    public override void AttackAnimationComplete()
    {
        attackInfo.state = AttackState.None;
        attackInfo.attackGO.GetComponent<Animator>().SetBool("Attack", false);
        attackInfo.currentCooldown = 0;
        GetComponent<Animator>().SetBool("Attacking", false);
        attackInfo.attackGO.GetComponent<SpriteRenderer>().enabled = false;
    }

    public override void Patrol()
    {
        //Patrol will get interrupted by Enemy.OnTriggerEnter2D, setting the state of this enemy to Aggro
        if (patrolInfo.activePatrolTime <= 0)
        {
            patrolInfo.activePatrolTime = patrolInfo.patrolDuration + UnityEngine.Random.Range(-patrolInfo.patrolVariance, patrolInfo.patrolVariance);
            patrolInfo.current = 0;
        }

        if (patrolInfo.current < patrolInfo.activePatrolTime)
        {
            patrolInfo.current += Time.deltaTime;
        }
        else
        {
            patrolInfo.activePatrolTime = -1;
            patrolInfo.current = 0;
            float angleInRad = UnityEngine.Random.Range(0, 2 * Mathf.PI);
            float distance = UnityEngine.Random.Range(0, patrolInfo.patrolRadius);
            Vector2 delta = new Vector2(Mathf.Cos(angleInRad), Mathf.Sin(angleInRad));
            MoveTo(patrolInfo.startLoc + distance * delta);
        }
    }
}
