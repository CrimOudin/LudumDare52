using System.Collections.Generic;
using UnityEngine;

public abstract class InteractiveObject : MonoBehaviour
{
    public List<PikminType> pikminTypesAllowed;

    private void OnMouseDown()
    {
        OnPikminInteract(Manager.Instance.GetNextAvailablePikmin(pikminTypesAllowed));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }

    public abstract void OnPikminInteract(Pikmin pikmin);
}