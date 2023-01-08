using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.UI;

public class PikminUI : MonoBehaviour
{
    private float progress = 0;
    private int queued = 0;
    //private PikminType type;
    private PikminInfo info;

    public SpriteRenderer PortraitBackground;
    public SpriteRenderer Portrait;
    public Text FoodCostText;

    public RectTransform ProgressBaseTransform; //the background for the build progress bar
    public RectTransform ProgressTransform; //a transform that is left centered to show the progressImage where to be

    public RectTransform Highlight;
    public bool selected = false;

    public Text BuildText;

    public void SetType(PikminType pt)
    {
        info = PikminManager.Instance.GetPikminInfo(pt);
        PortraitBackground.color = info.backgroundColor;
        Portrait.sprite = info.uiPortraitSprite;
        FoodCostText.text = info.foodCost.ToString();
    }

    public void MakePikmin()
    {
        if(Manager.Instance.SubtractResource(ItemType.Food, info.foodCost))
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
            if(progress > info.timeToBuild)
            {
                Manager.Instance.MakeNewPikmin(info.type);

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
                float percent = progress / info.timeToBuild;
                ProgressTransform.sizeDelta = new Vector2(ProgressBaseTransform.sizeDelta.x * percent, ProgressTransform.sizeDelta.y);
            }
        }

        yield break;
    }


    public void ToggleHighlight()
    {
        selected = !selected;
        Highlight.gameObject.SetActive(selected);

        Manager.Instance.TogglePikminTypeSelected(info.type, selected);
    }
}

public enum PikminType
{
    Red,
    Yellow,
    Blue
}
