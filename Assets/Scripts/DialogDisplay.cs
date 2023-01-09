using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class DialogDisplay : MonoBehaviour
{
    public Image Portrait;
    public Text Text;

    private Coroutine running = null;
    private bool skip = false;

    public void SetText(bool quickText, float waitTime, string text, Action endAction)
    {
        GetComponent<Image>().enabled = true;
        Portrait.enabled = true;
        Text.enabled = true;

        GetComponent<Fader>().totalFadeIn = 0.5f;
        float fadeInTime = quickText ? 0.5f : 1.5f;
        GetComponent<Fader>().FadeIn(fadeInTime, () => { running = StartCoroutine(DisplayText(quickText, waitTime, text, endAction)); });
        Portrait.GetComponent<Fader>().FadeIn(fadeInTime, null);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && running != null)
            skip = true;
        else if (Input.GetKeyDown(KeyCode.Mouse0) && running == null) //to prevent autoskips
            skip = false;
    }

    private IEnumerator DisplayText(bool quickText, float waitTime, string text, Action endAction)
    {
        bool wasSkipped = false;
        if (!quickText)
        {
            int index = 0;
            while (index <= text.Length)
            {
                if(skip)
                {
                    index = text.Length - 1;
                    wasSkipped = true;
                    skip = false;
                }

                Text.text = text.Substring(0, index);
                yield return new WaitForSeconds(0.03f + UnityEngine.Random.value * 0.01f);
                index++;
            }
            float a = 0;
            //If you skip with a mouse click, don't auto-fade out the text box, require another click
            if (wasSkipped)
            {
                while(!skip)
                {
                    yield return new WaitForSeconds(0.05f);
                }
                skip = false;
            }
            else //if not skipped, you can still start the fade out early by clicking, or wait
            {
                while (a < waitTime)
                {
                    yield return new WaitForEndOfFrame();
                    if (skip)
                    {
                        skip = false;
                        a = 1000;
                    }
                    a += Time.deltaTime;
                }
            }
        }
        else
        {
            Text.text = text;
            yield return new WaitForSeconds(3);
        }
        Text.text = "";
        GetComponent<Fader>().FadeOut(.5f, endAction);
        Portrait.GetComponent<Fader>().FadeOut(.5f, null);
        running = null;
        skip = false;
        yield break;
    }
}
