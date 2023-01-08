using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PikminAnimationActionEventListener : MonoBehaviour
{
    // Animation event calls this when finished.
    public void ActionFinished()
    {
        gameObject.SetActive(false);
    }
}
