using UnityEngine;

public abstract class InteractiveObject : MonoBehaviour
{
    private void OnMouseDown()
    {
        // Left click only
        if (Input.GetMouseButtonDown(0))
        {
            OnPikminInteract(Manager.Instance.GetNextAvailablePikmin());
        }
    }

    public abstract void OnPikminInteract(Pikmin pikmin);
}