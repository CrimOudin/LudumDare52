using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public static Manager Instance { get; set; }

    private Dictionary<ItemType, int> totalItems { get; set; } = new Dictionary<ItemType, int>();


    public GameObject RedPikminPrefab;
    public GameObject YellowPikminPrefab;
    public GameObject BluePikminPrefab;
    public PikminFormation OlimarsPikmanFormation;
    public TMP_Text FoodResourceUI;
    public TMP_Text MetalResourceUI;

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

    private void Start()
    {
        InitializeResources();
    }

    private void InitializeResources()
    {
        totalItems.Add(ItemType.Food, 50);
        totalItems.Add(ItemType.Metal, 0);

        FoodResourceUI.text = totalItems[ItemType.Food].ToString();
        MetalResourceUI.text = totalItems[ItemType.Metal].ToString();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, 1000f);
            if (hits.Any())
            {
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.gameObject.TryGetComponent<InteractiveObject>(out var interactiveObject))
                    {
                        // not sure why the raycast wont detect the object.
                        Debug.Log("InteraciveObject Clicked");
                        var nextAvailablePikmin = GetNextAvailablePikmin(interactiveObject.pikminTypesAllowed);
                        if (nextAvailablePikmin != null)
                        {
                            interactiveObject.OnPikminInteract(nextAvailablePikmin);
                        }
                    }
                    else if (hits.Length == 1 && hit.collider.CompareTag("Terrain"))
                    {
                        Pikmin nextPikmin = GetNextAvailablePikmin();
                        if (nextPikmin != null)
                        {
                            nextPikmin.ReceiveCommand(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                        }
                    }
                }
            }
        }
    }

    internal Pikmin GetNextAvailablePikmin(List<PikminType> pikminTypesAllowed = null)
    {
        Pikmin first = selectedPikminTypes.Count == 0 ?
                           OlimarsPikmanFormation.PikminInFormation.FirstOrDefault() :
                           OlimarsPikmanFormation.PikminInFormation
                           .Where(x => selectedPikminTypes.Contains(x.PikminType))
                           .Where(x => pikminTypesAllowed == null || pikminTypesAllowed.Contains(x.PikminType))
                           .FirstOrDefault();
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

        GameObject go = null;
        switch (type)
        {
            case PikminType.Red:
                go = Instantiate(RedPikminPrefab, OlimarsPikmanFormation.transform.position, Quaternion.identity);
                break;
            case PikminType.Yellow:
                go = Instantiate(YellowPikminPrefab, OlimarsPikmanFormation.transform.position, Quaternion.identity);
                break;
            case PikminType.Blue:
                go = Instantiate(BluePikminPrefab, OlimarsPikmanFormation.transform.position, Quaternion.identity);
                break;
            default:
                Debug.LogError("No Type Setup for this type");
                break;
        }
        Pikmin p = go.GetComponent<Pikmin>();
        p.PikminType = type;
        //p.AddMeToFormation();
    }

    public void TogglePikminTypeSelected(PikminType type, bool adding)
    {
        //added the "adding" bool just in case the UI and the manager get out of sync somehow
        if (!adding && selectedPikminTypes.Contains(type))
            selectedPikminTypes.Remove(type);
        else if (adding && !selectedPikminTypes.Contains(type))
            selectedPikminTypes.Add(type);
    }

    public void AddResource(ItemType type, int amount)
    {
        totalItems[type] += amount;

        switch (type)
        {
            case ItemType.Food:
                FoodResourceUI.text = totalItems[type].ToString();
                break;
            case ItemType.Metal:
                MetalResourceUI.text = totalItems[type].ToString();
                break;
            default:
                break;
        }
    }

    public bool SubtractResource(ItemType type, int amount)
    {
        //return true or false based on if you could afford it
        if (totalItems[type] >= amount)
        {
            totalItems[type] -= amount;

            switch (type)
            {
                case ItemType.Food:
                    FoodResourceUI.text = totalItems[type].ToString();
                    break;
                case ItemType.Metal:
                    MetalResourceUI.text = totalItems[type].ToString();
                    break;
                default:
                    break;
            }

            return true;
        }
        else
            return false;
    }

    public int GetResourceAmount(ItemType type)
    {
        return totalItems[type];
    }
}
