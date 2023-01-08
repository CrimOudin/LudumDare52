using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceNode : InteractiveObject
{
    public ItemType ResourceType;
    public int ResourceTotalAmount;
    public float ResourceDepletedAnimationTime;
    public GameObject ResourcePrefab;

    private bool isDepleted = false;

    public override void OnPikminInteract(Pikmin pikmin)
    {
        if (isDepleted) return;
        Manager.Instance.OlimarsPikmanFormation.RemovePikmin(pikmin);

        pikmin.state = PikminState.Mining;
        pikmin.CurrentResourceNode = this;
        pikmin.PerformMiningTask(this);        
    }
    internal void HandleDepleted()
    {
        isDepleted = true;
        StartCoroutine("Depleted");
    }

    private IEnumerator Depleted()
    {
        yield return new WaitForSeconds(ResourceDepletedAnimationTime);
        Destroy(gameObject);
    }
}
