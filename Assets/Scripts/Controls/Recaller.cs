using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recaller : MonoBehaviour
{
    public GameObject Recall;
    private GameObject activeRecall;

    private bool mouseDown = false;


    // Update is called once per frame
    void Update()
    {
        //while right click is held, a Recall object should exist and follow the mouse position.
        //When lifted, the object should be destroyed.
        if(Input.GetKeyDown(KeyCode.Mouse1) && !mouseDown)
        {
            mouseDown = true;
            activeRecall = Instantiate(Recall);
        }
        else if(Input.GetKeyUp(KeyCode.Mouse1) && mouseDown)
        {
            activeRecall.GetComponent<Recall>().Die();
            activeRecall = null;
            mouseDown = false;
        }

        if(mouseDown)
        {
            activeRecall.transform.position = Input.mousePosition - new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        }
    }
}
