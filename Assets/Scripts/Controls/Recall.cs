using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recall : MonoBehaviour
{
    private float aliveTime = 0f;
    private float dyingTime = -1f;
    private void Awake()
    {
        transform.localScale = new Vector3(0.25f, 0.25f, 1);
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (dyingTime < 0)
        {
            if (aliveTime < .75f)
            {
                aliveTime += Time.deltaTime;
                float scale = 0.25f + (0.75f * aliveTime * 1.333334f);
                transform.localScale = new Vector3(scale, scale, 1);
                float huh = aliveTime * 1.333334f;
                GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f + (0.5f * aliveTime * 1.3333334f));
            }
        }
        else
        {
            dyingTime += Time.deltaTime;
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1 - (dyingTime * 5));
            if (dyingTime * 5 >= 1)
                Destroy(gameObject);
        }
        transform.Rotate(new Vector3(0, 0, 0.75f));
    }

    public void Die()
    {
        if (dyingTime == -1)
            dyingTime = 0;
    }
}
