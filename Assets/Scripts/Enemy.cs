using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    public abstract int health { get; }
    public EnemyState state;
    public EnemyAttackInfo attackInfo;
    public EnemyPatrolInfo patrolInfo;


    public abstract void Attack();

    public abstract void AttackAnimationComplete();

    public abstract void MoveTo(Vector2 location);

    public abstract void Patrol();

    public abstract void Return();
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


    public float patrolDuration; //base duration
    public float patrolVariance; //allowed variance 
    public float patrolRadius; //how far you can move around your start location
}
