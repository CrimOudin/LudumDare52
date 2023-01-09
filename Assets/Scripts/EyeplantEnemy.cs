using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EyeplantEnemy : Enemy
{
    bool alive = true;

    private void Awake()
    {
        attackInfo.attackGO.GetComponent<OneTimeAnimationHandler>().endAction = AttackAnimationComplete;
        attackInfo.attackGO.GetComponent<OneTimeAnimationHandler>().deathEndAction = DeathAnimationComplete;
        patrolInfo.startLoc = transform.position;

        GetComponent<NavMeshAgent>().updateUpAxis = false;
        GetComponent<NavMeshAgent>().updatePosition = false;
        GetComponent<NavMeshAgent>().updateRotation = false;

        state = EnemyState.Patrolling;
    }

    private void Update()
    {
        if (alive)
        {
            transform.position = new Vector2(GetComponent<NavMeshAgent>().nextPosition.x, GetComponent<NavMeshAgent>().nextPosition.y);

            if (state == EnemyState.Patrolling)
            {
                Patrol();
            }
            else if (state == EnemyState.Aggro || state == EnemyState.Attacking)
            {
                Attack();
            }
            else if (state == EnemyState.Returning)
            {
                CheckForReturnDone();
            }
        }
    }

    public override void Attack()
    {
        if (state == EnemyState.Attacking)
        {
            if (attackInfo.state == AttackState.Windup)
            {
                attackInfo.currentCooldown += Time.deltaTime;
                if (attackInfo.currentCooldown >= attackInfo.attackWindupTime)
                {
                    attackInfo.state = AttackState.Attacking;
                    attackInfo.attackGO.GetComponent<Animator>().SetBool("Attack", true);
                    attackInfo.currentCooldown = 0;
                }
            }
            //After windup, attack
            else if (attackInfo.state == AttackState.Attacking)
            {
                attackInfo.currentCooldown += Time.deltaTime;
                if(attackInfo.currentCooldown >= attackInfo.attackWindupTime && attackInfo.attackGO.GetComponent<BoxCollider2D>().isActiveAndEnabled)
                    attackInfo.attackGO.GetComponent<BoxCollider2D>().enabled = true;
                else if (attackInfo.currentCooldown >= attackInfo.attackDuration && attackInfo.attackGO.GetComponent<BoxCollider2D>().isActiveAndEnabled)
                    attackInfo.attackGO.GetComponent<BoxCollider2D>().enabled = false;
            }
        }
        else
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
                    attackInfo.state = AttackState.Windup;

                    attackInfo.attackGO.transform.position = currentAggroTarget.transform.position;
                    GetComponent<Animator>().SetBool("Attacking", true);
                    attackInfo.attackGO.GetComponent<SpriteRenderer>().enabled = true;
                    attackInfo.attackGO.GetComponent<Animator>().SetBool("Attack", false); //just in case
                    attackInfo.currentCooldown = 0;
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

        if (returnAfterAttack)
        {
            state = EnemyState.Returning;
            returnAfterAttack = false;
        }
        else
            state = EnemyState.Aggro;
    }

    public void DeathAnimationComplete()
    {
        alive = false;
        attackInfo.attackGO.GetComponent<Animator>().SetBool("Attack", false);
    }

    public override void Patrol()
    {
        //do nothing you're a plant
    }

    public override void CheckForReturnDone()
    {
        state = EnemyState.Patrolling;
    }

    public override void OnPikminInteract(Pikmin pikmin)
    {
        pikmin.CurrentEnemy = this;
        pikmin.state = PikminState.Attacking;
        pikmin.PerformAttackTask(this);
    }
}
