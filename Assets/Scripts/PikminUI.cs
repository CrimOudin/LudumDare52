using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PikminUI : MonoBehaviour
{
    private float progress = 0;
    private int queued = 0;
    private PikminType type;

    public int TIMETOBUILDPIKMIN = 10;
    public Transform ProgressTransform;
    public Image ProgressImage;

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
            if (queued == 1)
                StartCoroutine(AnimateProgress());
        }
    }

    public IEnumerator AnimateProgress()
    {
        while(queued > 0)
        {
            yield return new WaitForSeconds(0.05f);
            progress += 0.05f;
            if(progress > TIMETOBUILDPIKMIN)
            {
                //todo: tell manager that pikmin build is done
                progress = 0;
                queued--;
            }
        }
    }
}

public enum PikminType
{
    Red
}
