using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuilderUI : MonoBehaviour
{
    public GameObject PikminUI;

    private void Awake()
    {
        SetInitial(); //todo: delete once the manager? calls this
    }

    public void SetInitial()
    {
        GameObject go = Instantiate(PikminUI);
        go.GetComponent<PikminUI>().SetType(PikminType.Red);
        go.transform.SetParent(transform);
        go.transform.localScale = new Vector3(1, 1, 1);

        GameObject go2 = Instantiate(PikminUI);
        go2.GetComponent<PikminUI>().SetType(PikminType.Yellow);
        go2.transform.SetParent(transform);
        go2.transform.localScale = new Vector3(1, 1, 1);

        GameObject go3 = Instantiate(PikminUI);
        go3.GetComponent<PikminUI>().SetType(PikminType.Blue);
        go3.transform.SetParent(transform);
        go3.transform.localScale = new Vector3(1, 1, 1);
    }

}
