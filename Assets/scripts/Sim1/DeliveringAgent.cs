using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeliveringAgent : RoleBase
{
    public static Queue<ConstructionRequest> deliverRequests = new Queue<ConstructionRequest>();
    private bool hasResourcesForDelivery = false;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    public static void HandleNewDeliverRequest(ConstructionRequest request)
    {
        if (!deliverRequests.Contains(request))
        {
            deliverRequests.Enqueue(request);
            Debug.Log($"{request.name} a reçu une nouvelle requête de livraison.");
        }
    }

    protected override void PerformAction()
    {
        Debug.Log($"{gameObject.name} - PerformAction: Current State: {currentState}, Deliver Requests Count: {deliverRequests.Count}");

        if (deliverRequests.Count == 0)
        {
            MoveRandomly();
            return;
        }

        ConstructionRequest closestRequest = FindClosestDeliveryRequest();

        if (closestRequest != null)
        {
            if (!hasResourcesForDelivery)
            {
                currentState = AgentState.GatheringResources;
                StartCoroutine(GatherResourcesFromDepot(closestRequest));
            }
            else
            {
                currentState = AgentState.DeliveringResources;
                DeliverResourcesToConstruction(closestRequest);
            }
        }
    }

    private IEnumerator GatherResourcesFromDepot(ConstructionRequest request)
    {
        Debug.Log($"{gameObject.name} - Gathering resources for request at {request.ConstructionSitePosition}");
        Dictionary<string, int> resourceForHouse = request.resourceNeededForHouse;
        StorageBase nearestDepot = FindNearestDepot(resourceForHouse);

        while (nearestDepot == null || !HasRequiredResources(nearestDepot, resourceForHouse))
        {
            Debug.Log($"{gameObject.name} - Waiting for resources to be available in depot");
            yield return new WaitForSeconds(1f);
            nearestDepot = FindNearestDepot(resourceForHouse);
        }

        MoveTo(nearestDepot.transform.position);

        while (Vector3.Distance(transform.position, nearestDepot.transform.position) > 1.0f)
        {
            yield return null;
        }

        CollectResourcesFromDepot(nearestDepot, resourceForHouse);
        hasResourcesForDelivery = inventory.GetValueOrDefault("Wood", 0) >= resourceForHouse.GetValueOrDefault("Wood", 0) &&
                                  inventory.GetValueOrDefault("Stone", 0) >= resourceForHouse.GetValueOrDefault("Stone", 0);
        Debug.Log($"{gameObject.name} - Resources gathered: Wood: {inventory.GetValueOrDefault("Wood", 0)}, Stone: {inventory.GetValueOrDefault("Stone", 0)}");
    }

    private void CollectResourcesFromDepot(StorageBase depot, Dictionary<string, int> resourceForHouse)
    {
        Debug.Log($"{gameObject.name} - Collecting resources from depot at {depot.transform.position}");
        switch (depot)
        {
            case WoodStorage woodStorage:
                int woodNeeded = resourceForHouse.GetValueOrDefault("Wood", 0) - inventory.GetValueOrDefault("Wood", 0);
                if (woodNeeded > 0)
                {
                    int woodTaken = woodStorage.GiveResource(woodNeeded);
                    inventory["Wood"] += woodTaken;
                    Debug.Log($"{gameObject.name} - Collected {woodTaken} wood from depot");
                }
                break;

            case StoneStorage stoneStorage:
                int stoneNeeded = resourceForHouse.GetValueOrDefault("Stone", 0) - inventory.GetValueOrDefault("Stone", 0);
                if (stoneNeeded > 0)
                {
                    int stoneTaken = stoneStorage.GiveResource(stoneNeeded);
                    inventory["Stone"] += stoneTaken;
                    Debug.Log($"{gameObject.name} - Collected {stoneTaken} stone from depot");
                }
                break;

            default:
                Debug.Log("Type de dépôt inconnu.");
                break;
        }
    }

    private void DeliverResourcesToConstruction(ConstructionRequest request)
    {
        Debug.Log($"{gameObject.name} - Delivering resources to construction site at {request.ConstructionSitePosition}");
        MoveTo(request.ConstructionSitePosition);

        if (Vector3.Distance(transform.position, request.ConstructionSitePosition) < 1.0f)
        {
            request.DeliverResources(inventory);
            hasResourcesForDelivery = false;

            Debug.Log("Ressources livrées au chantier.");
            deliverRequests.Dequeue(); // Retirer la requête de la file d'attente après livraison
        }
    }

    private StorageBase FindNearestDepot(Dictionary<string, int> requiredResources)
    {
        StorageBase[] depots = FindObjectsByType<StorageBase>(FindObjectsSortMode.None);
        StorageBase nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (var depot in depots)
        {
            if (HasRequiredResources(depot, requiredResources))
            {
                float distance = Vector3.Distance(transform.position, depot.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = depot;
                }
            }
        }
        return nearest;
    }

    private bool HasRequiredResources(StorageBase depot, Dictionary<string, int> requiredResources)
    {
        foreach (var resource in requiredResources)
        {
            if (resource.Value > 0)
            {
                if (depot is WoodStorage woodStorage && resource.Key == "Wood")
                {
                    if (woodStorage.Amount < resource.Value)
                    {
                        return false;
                    }
                }
                else if (depot is StoneStorage stoneStorage && resource.Key == "Stone")
                {
                    if (stoneStorage.Amount < resource.Value)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }
        return true;
    }

    private ConstructionRequest FindClosestDeliveryRequest()
    {
        ConstructionRequest closestRequest = null;
        float minDistance = Mathf.Infinity;

        foreach (var request in deliverRequests)
        {
            float distance = Vector3.Distance(transform.position, request.ConstructionSitePosition);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestRequest = request;
            }
        }

        return closestRequest;
    }
}

