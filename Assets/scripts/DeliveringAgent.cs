using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeliveringAgent : RoleBase
{
    public static Queue<ConstructionRequest> deliverRequests = new Queue<ConstructionRequest>();
    private bool hasResourcesForDelivery = false;

    public static void AddDeliverRequest(ConstructionRequest request)
    {
        if (!deliverRequests.Contains(request))
        {
            Debug.Log($"Nouvelle requête de livraison ajoutée à {request.ConstructionSitePosition}");
            deliverRequests.Enqueue(request);
        }
    }

    public static void RemoveDeliverRequest(ConstructionRequest request)
    {
        if (deliverRequests.Contains(request))
        {
            Debug.Log($"Requête de livraison supprimée de {request.ConstructionSitePosition}");
            // Pour retirer un élément spécifique d'une queue, il faut recréer la queue sans cet élément
            var tempQueue = new Queue<ConstructionRequest>(deliverRequests.Where(r => r != request));
            deliverRequests = tempQueue;
        }
    }

    protected override void PerformAction()
    {
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
                GatherResourcesFromDepot(closestRequest);
            }
            else
            {
                currentState = AgentState.DeliveringResources;
                DeliverResourcesToConstruction(closestRequest);
            }
        }
    }

    private void GatherResourcesFromDepot(ConstructionRequest request)
    {
        Dictionary<string, int> resourceForHouse = request.ResourceNeededForHouse;
        StorageBase nearestDepot = FindNearestDepot(resourceForHouse);
        if (nearestDepot != null)
        {
            agentBase.MoveTo(nearestDepot.transform.position);

            if (Vector3.Distance(transform.position, nearestDepot.transform.position) < 1.0f)
            {
                CollectResourcesFromDepot(nearestDepot, resourceForHouse);
                hasResourcesForDelivery = agentBase.inventory.GetValueOrDefault("Wood", 0) >= resourceForHouse.GetValueOrDefault("Wood", 0) &&
                                          agentBase.inventory.GetValueOrDefault("Stone", 0) >= resourceForHouse.GetValueOrDefault("Stone", 0);
            }
        }
        else
        {
            Debug.Log("Pas de dépôt disponible avec des ressources.");
        }
    }

    private void CollectResourcesFromDepot(StorageBase depot, Dictionary<string, int> resourceForHouse)
    {
        switch (depot)
        {
            case WoodStorage woodStorage:
                int woodNeeded = resourceForHouse.GetValueOrDefault("Wood", 0) - agentBase.inventory.GetValueOrDefault("Wood", 0);
                if (woodNeeded > 0)
                {
                    int woodTaken = woodStorage.GiveResource(woodNeeded);
                    agentBase.inventory["Wood"] += woodTaken;
                }
                break;

            case StoneStorage stoneStorage:
                int stoneNeeded = resourceForHouse.GetValueOrDefault("Stone", 0) - agentBase.inventory.GetValueOrDefault("Stone", 0);
                if (stoneNeeded > 0)
                {
                    int stoneTaken = stoneStorage.GiveResource(stoneNeeded);
                    agentBase.inventory["Stone"] += stoneTaken;
                }
                break;

            default:
                Debug.Log("Type de dépôt inconnu.");
                break;
        }
    }

    private void DeliverResourcesToConstruction(ConstructionRequest request)
    {
        agentBase.MoveTo(request.ConstructionSitePosition);

        if (Vector3.Distance(transform.position, request.ConstructionSitePosition) < 1.0f)
        {
            request.DeliverResources(agentBase.inventory);
            hasResourcesForDelivery = false;

            Debug.Log("Ressources livrées au chantier.");
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

