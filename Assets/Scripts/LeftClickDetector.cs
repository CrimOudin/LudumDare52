using UnityEngine;

public class LeftClickDetector : MonoBehaviour
{
    private void OnMouseDown()
    {
        Pikmin nextPikmin = Manager.Instance.GetNextAvailablePikmin();
        if (nextPikmin != null)
        {
            nextPikmin.ReceiveCommand(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
    }
}
