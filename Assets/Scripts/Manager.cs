using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public static Manager Instance { get; set; }

    public Dictionary<ItemType, int> totalItems { get; set; } = new Dictionary<ItemType, int>();


    public GameObject PikminPrefab;
    public PikminFormation OlimarsPikmanFormation;

    private List<PikminType> selectedPikminTypes = new List<PikminType>();

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

    internal Pikmin GetNextAvailablePikmin()//List<PikminType> pikminTypesAllowed = null)
    {
        Pikmin first = selectedPikminTypes.Count == 0 ? 
                           OlimarsPikmanFormation.PikminInFormation.FirstOrDefault() : 
                           OlimarsPikmanFormation.PikminInFormation.Where(x => selectedPikminTypes.Contains(x.PikminType)).FirstOrDefault();
        return first;

        //foreach (Pikmin pikmin in OlimarsPikmanFormation.PikminInFormation)
        //{
        //    if(pikminTypesAllowed == null || pikminTypesAllowed.Contains(pikmin.PikminType))
        //    {
        //        return pikmin;
        //    }
        //}
        //return null;
    }

    public void MakeNewPikmin(PikminType type)
    {
        GameObject go = Instantiate(PikminPrefab);
        Pikmin p = go.GetComponent<Pikmin>();
        //todo: code
        //p.SetType(type);
        p.AddMeToFormation();
    }

    public void TogglePikminTypeSelected(PikminType type, bool adding)
    {
        //added the "adding" bool just in case the UI and the manager get out of sync somehow
        if (!adding && selectedPikminTypes.Contains(type))
            selectedPikminTypes.Remove(type);
        else if(adding && !selectedPikminTypes.Contains(type))
            selectedPikminTypes.Add(type);
    }
}
