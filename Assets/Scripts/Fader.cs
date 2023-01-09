using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    public float totalFadeIn = 1;

    public void FadeIn(float time, Action endAction)
    {
        StartCoroutine(FI(time, endAction));
    }

    public void FadeOut(float time, Action endAction)
    {
        StartCoroutine(FO(time, endAction));
    }

    private IEnumerator FI(float time, Action endAction)
    {
        float a = 0;
        float t = 0;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Image i = GetComponent<Image>();

        if (sr != null)
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0);
        if (i != null)
            i.color = new Color(i.color.r, i.color.g, i.color.b, 0);

        while (t < time)
        {
            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
            a = t / time;
            if (sr != null)
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, a * totalFadeIn);
            if (i != null)
                i.color = new Color(i.color.r, i.color.g, i.color.b, a * totalFadeIn);
        }
        endAction?.Invoke();
        yield break;
    }

    private IEnumerator FO(float time, Action endAction)
    {
        float a = 1 * totalFadeIn;
        float t = 0;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Image i = GetComponent<Image>();

        if (sr != null)
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, totalFadeIn);
        if (i != null)
            i.color = new Color(i.color.r, i.color.g, i.color.b, totalFadeIn);

        while (t < time)
        {
            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
            a = 1 - (t / time);
            if (sr != null)
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, a * totalFadeIn);
            if (i != null)
                i.color = new Color(i.color.r, i.color.g, i.color.b, a * totalFadeIn);
        }

        endAction?.Invoke();
        yield break;
    }
}
