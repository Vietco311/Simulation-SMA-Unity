using System.Collections.Generic;
using UnityEngine;

public class CollectorAgent : RoleBase
{


    protected override void PerformAction()
    {
        WoodResource nearestResource = FindNearestWoodResource();

        if (nearestResource != null && agentBase.GetTotalWeight() < agentBase.WeightCapacity)
        {
            agentBase.MoveTo(nearestResource.transform.position);
            if (Vector3.Distance(transform.position, nearestResource.transform.position) < 0.1f)
            {
                CollectResource(nearestResource);
            }
        }
        else
        {
            DeliverResourceToStorage(); // Si inventaire plein, va livrer les ressources
        }
    }

    public WoodResource FindNearestWoodResource()
    {
        WoodResource[] resources = FindObjectsByType<WoodResource>(FindObjectsSortMode.None);
        WoodResource nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (WoodResource resource in resources)
        {
            float distance = Vector3.Distance(transform.position, resource.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = resource;
            }
        }
        return nearest;
    }

    public void CollectResource(ResourceBase resource)
    {
        currentState = AgentState.CollectingResources;
        int collectedResource = resource.Collect();
        int resourceWeight = resource.WeightPerUnit;

        if (agentBase.GetTotalWeight() + (collectedResource * resourceWeight) <= agentBase.WeightCapacity)
        {
            // Utilisation de `switch` pour trier chaque ressource par type
            switch (resource)
            {
                case WoodResource:
                    if (!agentBase.inventory.ContainsKey("Wood"))
                        agentBase.inventory["Wood"] = 0;
                    agentBase.inventory["Wood"] += collectedResource;
                    break;

                case StoneResource:
                    if (!agentBase.inventory.ContainsKey("Stone"))
                        agentBase.inventory["Stone"] = 0;
                    agentBase.inventory["Stone"] += collectedResource;
                    break;

                default:
                    Debug.LogWarning($"Type de ressource non pris en charge : {resource.GetType().Name}");
                    break;
            }

            Debug.Log($"Ajout de {collectedResource} unités de {resource.GetType().Name} à l'inventaire.");
        }
        else
        {
            Debug.Log("Inventaire plein, retour au dépôt.");
        }
    }

    public void DeliverResourceToStorage()
    {
        currentState = AgentState.DeliveringResources;
        StorageBase woodStorage = FindWoodStorage();
        StorageBase stoneStorage = FindStoneStorage();

        if (woodStorage == null && stoneStorage == null) return; // Aucun stockage trouvé

        Vector3 targetPosition = woodStorage?.transform.position ?? stoneStorage.transform.position;
        agentBase.MoveTo(targetPosition);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            if (agentBase.inventory["Wood"] > 0)
            {
                woodStorage.ReceiveResource(agentBase.inventory["Wood"]);
                agentBase.inventory["Wood"] = 0; 
            }

            if (agentBase.inventory["Stone"] > 0 && stoneStorage != null)
            {
                stoneStorage.ReceiveResource(agentBase.inventory["Stone"]);
                agentBase.inventory["Stone"] = 0; 
            }
        }
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
