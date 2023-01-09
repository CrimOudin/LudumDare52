using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PikminManager : MonoBehaviour
{
    public int test;
    public List<PikminInfo> PikminInfos;

    public static PikminManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public PikminInfo GetPikminInfo(PikminType type)
    {
        return PikminInfos.Where(x => x.type == type).FirstOrDefault();
    }
}

[Serializable]
public class PikminInfo
{
    public PikminType type;
    public Sprite uiPortraitSprite;
    public GameObject prefab;
    public Color backgroundColor;
    public int foodCost;
    public int timeToBuild;

    public float IdleInteractRange = 3f;
    public float ActionInteractionRange = 1f;
    public float ActionInteractionDelay = 5f;
    public int maxHealth = 10;
    public float DeathAnimationTime = 1f;
    public int maxItemCount = 2;
    public int Damage = 1;
}