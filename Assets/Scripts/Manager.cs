using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    public static Manager Instance { get; set; }

    private Dictionary<ItemType, int> totalItems { get; set; } = new Dictionary<ItemType, int>();


    public GameObject RedPikminPrefab;
    public GameObject YellowPikminPrefab;
    public GameObject BluePikminPrefab;
    public PikminFormation OlimarsPikminFormation;
    public TMP_Text FoodResourceUI;
    public TMP_Text MetalResourceUI;

    public int startFood = 50;

    public Olimar player;
    public Fader fade;
    public DialogDisplay dd;
    public DialogManager dm;
    public BuilderUI ui;
    public GameObject RocketPrefab;


    [HideInInspector]
    public List<PikminType> encountered = new List<PikminType>();

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
            if (fade != null)
            {
                player.canAct = false;
                fade.FadeOut(1.5f, () =>
                {
                    dd.SetText(false, 2, dm.openingDialog, () => { player.canAct = true; });
                });
            }
        }
    }

    private void Start()
    {
        InitializeResources();
    }

    private void InitializeResources()
    {
        totalItems.Add(ItemType.Food, startFood);
        totalItems.Add(ItemType.Metal, 0);

        FoodResourceUI.text = totalItems[ItemType.Food].ToString();
        MetalResourceUI.text = totalItems[ItemType.Metal].ToString();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && player.canAct)
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
        else if(Input.GetKeyDown(KeyCode.R))
        {
            fade.FadeIn(1f, () => { SceneManager.LoadScene(0); });
        }
    }

    internal Pikmin GetNextAvailablePikmin(List<PikminType> pikminTypesAllowed = null)
    {
        Pikmin first = selectedPikminTypes.Count == 0 ?
                           OlimarsPikminFormation.PikminInFormation.FirstOrDefault() :
                           OlimarsPikminFormation.PikminInFormation
                           .Where(x => selectedPikminTypes.Contains(x.PikminType))
                           .Where(x => (pikminTypesAllowed == null || pikminTypesAllowed.Contains(x.PikminType)) && x.state == PikminState.InFormation)
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
                go = Instantiate(RedPikminPrefab, OlimarsPikminFormation.transform.position, Quaternion.identity);
                break;
            case PikminType.Yellow:
                go = Instantiate(YellowPikminPrefab, OlimarsPikminFormation.transform.position, Quaternion.identity);
                break;
            case PikminType.Blue:
                go = Instantiate(BluePikminPrefab, OlimarsPikminFormation.transform.position, Quaternion.identity);
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


    public void PikminIntro()
    {
        player.canAct = false; 
        dd.SetText(false, 2, dm.pikminIntro, () => { player.canAct = true; });
    }

    public void EnemyIntro()
    {
        player.canAct = false;
        dd.SetText(false, 2, dm.enemyIntro, () => { player.canAct = true; });
    }

    public void NewPikminEncountered(PikminType type)
    {
        if (!encountered.Contains(type))
        {
            if (encountered.Count == 0)
            {
                //pikmin intro 2.  Show UI finally
                player.canAct = false;
                dd.SetText(false, 1, dm.pikminIntroduction, () => { player.canAct = true; ui.gameObject.SetActive(true); ui.Set(type); ui.Set(PikminType.Rocket); });
            }
            else
            {
                //add pikmin type to UI.  Summary text for that pikmin type?
                string a = type == PikminType.Blue ? dm.bluePikminText :
                          (type == PikminType.Red ? dm.redPikminText : dm.yellowPikminText);
                dd.SetText(true, 1, a, () => { ui.Set(type); });
            }
            encountered.Add(type);
        }
    }


    public void Victory()
    {
        player.canAct = false;
        GameObject rocket = Instantiate(RocketPrefab);
        rocket.transform.position = player.transform.position + new Vector3(200, 0, 0);
        dd.SetText(false, 1, dm.rocketDialog, () => { StartCoroutine(WinAnimation(rocket)); });

    }

    private IEnumerator WinAnimation(GameObject rocket)
    {
        Destroy(player.gameObject);
        yield return new WaitForSeconds(1f);
        rocket.GetComponent<Animator>().SetBool("blastoff", true);
        yield return new WaitForSeconds(1f);

        fade.FadeIn(2f, null);
        while(rocket.transform.position.y < Camera.main.transform.position.y + 540)
        {
            rocket.transform.position += new Vector3(0, Time.deltaTime * 200);
            yield return new WaitForEndOfFrame();
        }

        yield break;
    }
}
