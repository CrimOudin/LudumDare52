using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PikminFormation : MonoBehaviour
{
    public List<Pikmin> PikminInFormation { get; set; } = new List<Pikmin>();
    public List<Pikmin> PikminReturning { get; set; } = new List<Pikmin>();
    public GameObject Follow;
    private float previousAngle = 0;


    private float diff = 0;
    private Vector3 lastLoc = new Vector2(0, 0);
    private float lastAngle;

    private void Awake()
    {
        if(Follow != null)
        {
            diff = (transform as RectTransform).sizeDelta.y * 0.5f + (Follow.transform as RectTransform).sizeDelta.y * 0.5f + 10; 
            transform.position = Follow.transform.position + new Vector3(0, -diff, 0);
            lastLoc = Follow.transform.position;
            lastAngle = (float)Math.Atan2(-1, 0) * 180 / (float)Math.PI;
        }
    }


    void Update()
    {
        if(Follow != null && lastLoc != Follow.transform.position)
        {
            Vector2 d = new Vector2(Follow.transform.position.x - transform.position.x, Follow.transform.position.y - transform.position.y);
            float angle = (float)Math.Atan2(-d.y, d.x) * 180 / (float)Math.PI;
            transform.position = Follow.transform.position + (Vector3)(-d.normalized * diff);
            transform.Rotate(new Vector3(0, 0, lastAngle - angle));
            lastAngle = angle;
        }
    }

    public void AddPikmin()
    {

    }
}
