using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PikminUI : MonoBehaviour
{
    private float progress = 0;
    private int queued = 0;
    private PikminType type;

    public int TIMETOBUILDPIKMIN;
    public int RedPikminFoodCost;
    public int YellowPikminFoodCost;
    public int BluePikminFoodCost;
    public RectTransform ProgressBaseTransform; //the background for the build progress bar
    public RectTransform ProgressTransform; //a transform that is left centered to show the progressImage where to be

    public RectTransform Highlight;
    public bool selected = false;

    public Text BuildText;

    public void SetType(PikminType pt)
    {
        type = pt;
        //todo: use type to update the image
    }

    public void MakePikmin()
    {
        bool canAfford = false;
        var amountOfFood = Manager.Instance.GetResourceAmount(ItemType.Food);
        switch (type)
        {
            case PikminType.Red:
                if (amountOfFood >= RedPikminFoodCost)
                {
                    Manager.Instance.SubtractResource(ItemType.Food, RedPikminFoodCost);
                    canAfford = true;
                }
                break;
            case PikminType.Yellow:
                if (amountOfFood >= YellowPikminFoodCost)
                {
                    Manager.Instance.SubtractResource(ItemType.Food, YellowPikminFoodCost);
                    canAfford = true;
                }
                break;
            case PikminType.Blue:
                if (amountOfFood >= BluePikminFoodCost)
                {
                    Manager.Instance.SubtractResource(ItemType.Food, BluePikminFoodCost);
                    canAfford = true;
                }
                break;
            default:
                break;
        }

        if(canAfford)
        {
            queued++;
            BuildText.text = queued.ToString();
            if (queued == 1)
                StartCoroutine(AnimateProgress());
        }
    }

    public IEnumerator AnimateProgress()
    {
        while(queued > 0)
        {
            yield return new WaitForEndOfFrame();
            progress += Time.deltaTime;
            if(progress > TIMETOBUILDPIKMIN)
            {
                Manager.Instance.MakeNewPikmin(type);

                progress = 0;
                queued--;
                if (queued > 0)
                    BuildText.text = queued.ToString();
                else
                    BuildText.text = "Build";

                ProgressTransform.sizeDelta = new Vector2(0, ProgressTransform.sizeDelta.y);
            }
            else
            {
                float percent = progress / TIMETOBUILDPIKMIN;
                ProgressTransform.sizeDelta = new Vector2(ProgressBaseTransform.sizeDelta.x * percent, ProgressTransform.sizeDelta.y);
            }
        }

        yield break;
    }


    public void ToggleHighlight()
    {
        selected = !selected;
        Highlight.gameObject.SetActive(selected);

        Manager.Instance.TogglePikminTypeSelected(type, selected);
    }
}

public enum PikminType
{
    Red,
    Yellow,
    Blue
}
