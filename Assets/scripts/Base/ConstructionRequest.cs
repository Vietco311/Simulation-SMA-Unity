using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public class ConstructionRequest : MonoBehaviour
{
    public bool IsConstructed => constructionProgress >= 100;
    private bool isReady = false;
    public int constructionProgress = 0;
    public int constructionSpeed = 1;
    public Dictionary<string, int> resourceNeededForHouse;
    public delegate void ResourcesNeededHandler(ConstructionRequest request);
    public event ResourcesNeededHandler OnResourcesNeeded;
    public event Action OnResourcesDelivered; 
    public GameObject ConstructedHouse { get; set; }
    public Vector3 ConstructionSitePosition { get; set; }
    public List<AgentBase> CoupleAgents;

    public bool isReadyForConstruction => isReady;

    private void Start()
    {
    }

    public void SetResourcesNeeded(Dictionary<string, int> resources)
    {
        resourceNeededForHouse = new Dictionary<string, int>(resources);
    }

    public void SetToReady()
    {
        isReady = true;
    }

    public void DeliverResources(Dictionary<string, int> inventory)
    {
        foreach (var resource in inventory.Keys.ToList())
        {
            if (resourceNeededForHouse.ContainsKey(resource))
            {
                int deliveredAmount = Mathf.Min(inventory[resource], resourceNeededForHouse[resource]);
                resourceNeededForHouse[resource] -= deliveredAmount;
                inventory[resource] -= deliveredAmount;
            }
        }

        if (AreResourcesFulfilled())
        {
            Debug.Log("Toutes les ressources nécessaires ont été livrées.");
            OnResourcesDelivered?.Invoke(); // Notifier que les ressources sont livrées
        }
        else
        {
            Debug.Log("Ressources nécessaires non complètes, déclenchement de OnResourcesNeeded.");
            OnResourcesNeeded?.Invoke(this); 
        }
    }

    public IEnumerator AdvanceConstruction(AgentBase agent)
    {
        if (IsConstructed || !isReady || !AreResourcesFulfilled()) yield break;

        while (constructionProgress < 100)
        {
            if (agent.IsResting() || agent.IsSleepingOnGround())
                yield return new WaitUntil(() => !agent.IsResting() && !agent.IsSleepingOnGround());
            yield return new WaitForSeconds(1);
            constructionProgress += constructionSpeed;

            if (constructionProgress >= 100)
            {
                constructionProgress = 100;
                ConstructHouse();
                yield break;
            }
        }
    }

    public bool AreResourcesFulfilled()
    {
        foreach (var resource in resourceNeededForHouse.Values)
        {
            if (resource > 0) return false;
        }
        return true;
    }

    private void ConstructHouse()
    {
        var clone = Instantiate(ConstructedHouse, ConstructionSitePosition, Quaternion.identity);
        foreach (var agent in CoupleAgents)
        {
            agent.CurrentRequest = null;
            agent.assignedHouse = clone;
        }
        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(2);
        Destroy(gameObject);
    }
}
