using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneTimeAnimationHandler : MonoBehaviour
{
    public Action endAction;
    public Action deathEndAction;

    public void OnComplete()
    {
        endAction?.Invoke();
    }

    public void OnDeathComplete()
    {
        deathEndAction?.Invoke();
    }
}
