using System.Collections.Generic;
using UnityEngine;

public abstract class InteractiveObject : MonoBehaviour
{
    public List<PikminType> pikminTypesAllowed;

    private void OnMouseDown()
    {
        //Debug.Log("InteraciveObject Clicked");
        //var nextAvailablePikmin = Manager.Instance.GetNextAvailablePikmin(pikminTypesAllowed);
        //if(nextAvailablePikmin != null)
        //{
        //    OnPikminInteract(nextAvailablePikmin);
        //}
    }

    public abstract void OnPikminInteract(Pikmin pikmin);
}