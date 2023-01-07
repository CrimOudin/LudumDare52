using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public static Manager Instance { get; set; }

    public Dictionary<ItemType, int> totalItems { get; set; } = new Dictionary<ItemType, int>();

    public PikminFormation OlimarsPikmanFormation;

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    internal Pikmin GetNextAvailablePikmin(List<PikminType> pikminTypesAllowed = null)
    {
        foreach (Pikmin pikmin in OlimarsPikmanFormation.PikminInFormation)
        {
            if(pikminTypesAllowed == null || pikminTypesAllowed.Contains(pikmin.PikminType))
            {
                return pikmin;
            }
        }
        return null;
    }
}
