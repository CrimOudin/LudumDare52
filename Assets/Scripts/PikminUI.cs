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
    public Image CostTypeSprite;

    public RectTransform ProgressBaseTransform; //the background for the build progress bar
    public RectTransform ProgressTransform; //a transform that is left centered to show the progressImage where to be

    public RectTransform Highlight;
    public bool selected = false;

    public Text BuildText;

    public Sprite foodSprite;
    public Sprite metalSprite;

    private bool canBeSelected = true;

    public void SetType(PikminType pt)
    {
        info = PikminManager.Instance.GetPikminInfo(pt);
        PortraitBackground.color = info.backgroundColor;
        Portrait.sprite = info.uiPortraitSprite;
        FoodCostText.text = info.foodCost.ToString();
        if (pt == PikminType.Rocket)
        {
            CostTypeSprite.sprite = metalSprite;
            canBeSelected = false;
        }
    }

    public void MakePikmin()
    {
        if (CostTypeSprite.sprite == foodSprite)
        {
            if (Manager.Instance.SubtractResource(ItemType.Food, info.foodCost))
            {
                queued++;
                //BuildText.text = queued.ToString();

                Manager.Instance.MakeNewPikmin(info.type);
                //if (queued == 1)
                //    StartCoroutine(AnimateProgress());
            }
        }
        else
        {
            //try to end the game
            if (Manager.Instance.SubtractResource(ItemType.Metal, 150))
            {
                Manager.Instance.Victory();
            }
        }
    }

    public IEnumerator AnimateProgress()
    {
        while (queued > 0)
        {
            yield return new WaitForEndOfFrame();
            progress += Time.deltaTime;
            if (progress > info.timeToBuild)
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
        if (canBeSelected)
        {
            selected = !selected;
            Highlight.gameObject.SetActive(selected);

            Manager.Instance.TogglePikminTypeSelected(info.type, selected);
        }
    }
}

public enum PikminType
{
    Red,
    Yellow,
    Blue,
    Rocket
}
