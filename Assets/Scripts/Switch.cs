using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Button;

public class Switch : MonoBehaviour
{
    public ButtonClickedEvent WalkOverEvent;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<Olimar>() != null)
        {
            WalkOverEvent?.Invoke();
            gameObject.SetActive(false);
        }
    }
}
