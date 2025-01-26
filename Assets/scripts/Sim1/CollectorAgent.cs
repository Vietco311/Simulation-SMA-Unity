using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectorAgent : RoleBase
{
    private ResourceBase currentTargetResource = null;

    protected override void Start()
    {
        base.Start();
        QueueAction(() => PerformAction(), true); 
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void PerformAction()
    {
        if (GetTotalWeight() < WeightCapacity)
        {
            WoodResource nearestResource = FindNearestWoodResource();
            if (nearestResource != null)
            {
                MoveTo(nearestResource.transform.position);
                StartCoroutine(CheckAndCollectResource(nearestResource));
            }
            else
            {
                DeliverResourceToStorage();
            }
        }
        else
        {
            DeliverResourceToStorage();
        }
    }

    private IEnumerator CheckAndCollectResource(WoodResource resource)
    {
        while (Vector3.Distance(transform.position, resource.transform.position) > 0.1f)
        {
            yield return null;
        }

        if (resource != null)
        {
            CollectResource(resource);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} - Resource was destroyed before collection");
            PerformAction();
        }
    }

    public WoodResource FindNearestWoodResource()
    {
        WoodResource[] resources = FindObjectsByType<WoodResource>(FindObjectsSortMode.None);
        WoodResource nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (WoodResource resource in resources)
        {
            if (!resource.IsReserved() || resource.IsReservedBy(this))
            {
                float distance = Vector3.Distance(transform.position, resource.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = resource;
                }
            }
        }

        if (nearest != null)
        {
            nearest.Reserve(this);
            currentTargetResource = nearest;
        }

        return nearest;
    }

    public void CollectResource(ResourceBase resource)
    {
        currentState = AgentState.CollectingResources;
        int collectedResource = resource.Collect();
        int resourceWeight = resource.WeightPerUnit;

        if (GetTotalWeight() + (collectedResource * resourceWeight) <= WeightCapacity)
        {
            switch (resource)
            {
                case WoodResource:
                    if (!inventory.ContainsKey("Wood"))
                        inventory["Wood"] = 0;
                    inventory["Wood"] += collectedResource;
                    break;

                case StoneResource:
                    if (!inventory.ContainsKey("Stone"))
                        inventory["Stone"] = 0;
                    inventory["Stone"] += collectedResource;
                    break;

                default:
                    Debug.LogWarning($"Type de ressource non pris en charge : {resource.GetType().Name}");
                    break;
            }
        }
        else
        {
            Debug.Log($"{gameObject.name} - Inventory full, returning to depot.");
        }

        resource.Unreserve();
        currentTargetResource = null;
    }

    public void DeliverResourceToStorage()
    {
        currentState = AgentState.DeliveringResources;
        StorageBase woodStorage = FindWoodStorage();
        StorageBase stoneStorage = FindStoneStorage();

        if (woodStorage == null && stoneStorage == null)
        {
            Debug.LogWarning($"{gameObject.name} - No storage found");
            return;
        }

        Vector3 targetPosition = woodStorage?.transform.position ?? stoneStorage.transform.position;
        MoveTo(targetPosition);

        StartCoroutine(DeliverResourcesCoroutine(targetPosition, woodStorage, stoneStorage));
    }

    private IEnumerator DeliverResourcesCoroutine(Vector3 targetPosition, StorageBase woodStorage, StorageBase stoneStorage)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            yield return null;
        }

        if (inventory["Wood"] > 0)
        {
            woodStorage.ReceiveResource(inventory["Wood"]);
            inventory["Wood"] = 0;
            Debug.Log($"{gameObject.name} - Delivered wood to storage");
        }

        if (inventory["Stone"] > 0 && stoneStorage != null)
        {
            stoneStorage.ReceiveResource(inventory["Stone"]);
            inventory["Stone"] = 0;
            Debug.Log($"{gameObject.name} - Delivered stone to storage");
        }

        currentState = AgentState.Idle;
        QueueAction(() => PerformAction(), true); // Requeue the PerformAction to continue collecting resources
    }

    public StorageBase FindWoodStorage()
    {
        return FindFirstObjectByType<WoodStorage>();
    }

    public StorageBase FindStoneStorage()
    {
        return FindFirstObjectByType<StoneStorage>();
    }
}
