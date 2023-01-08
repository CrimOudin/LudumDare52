using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneTimeAnimationHandler : MonoBehaviour
{
    public Action endAction;
    public void OnComplete()
    {
        endAction?.Invoke();
    }
}
