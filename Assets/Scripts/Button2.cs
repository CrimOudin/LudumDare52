using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Button;

public class Button2 : MonoBehaviour
{
    public ButtonClickedEvent clickEvent;

    private void OnMouseDown()
    {
        clickEvent.Invoke();
    }
}
