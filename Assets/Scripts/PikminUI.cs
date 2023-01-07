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
    public RectTransform ProgressBaseTransform; //the background for the build progress bar
    public RectTransform ProgressTransform; //a transform that is left centered to show the progressImage where to be

    public Text BuildText;

    public void SetType(PikminType pt)
    {
        type = pt;
        //todo: use type to update the image
    }

    public void MakePikmin()
    {
        //todo: check resources and take them if you can afford
        bool canAfford = true;
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
                //todo: tell manager that pikmin build is done
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
}

public enum PikminType
{
    Red,
    Yellow,
    Blue
}
