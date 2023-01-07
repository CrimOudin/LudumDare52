using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public static Manager Instance { get; set; }

    PikminFormation olimarsPikmanFormation;

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
        }
    }

    internal Pikmin GetNextAvailablePikmin()
    {
        throw new NotImplementedException();
    }

    private void OnMouseDown()
    {
        // Only right click
        if (Input.GetMouseButtonDown(1)) 
        {
        
        }
    }

    private void OnMouseUp()
    {
        // Only right click
        if (Input.GetMouseButtonUp(1))
        {

        }
    }
}
