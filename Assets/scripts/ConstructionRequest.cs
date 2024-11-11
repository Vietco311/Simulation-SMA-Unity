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
    public string HouseType { get; set; }
    public GameObject ConstructedHouse { get; set; }
    public Vector3 ConstructionSitePosition { get; set; }

    public List<AgentBase> CoupleAgents { get; set; }

    public Dictionary<string, int> ResourceNeededForHouse => resourceNeededForHouse;

    public bool isReadyForConstruction => isReady;

    private void Start()
    {
        MoveAgentsToConstruction(ConstructionSitePosition);
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

        // Vérifiez si toutes les ressources nécessaires ont été livrées  
        if (AreResourcesFulfilled())
        {
            DeliveringAgent.RemoveDeliverRequest(this);
            Debug.Log("Toutes les ressources nécessaires ont été livrées.");
        }
    }


    public IEnumerator AdvanceConstruction(GameObject housePrefab, BuilderAgent builderAgent)
    {
        Debug.Log($"IsConstructed {IsConstructed} isReady {isReady} AreResourcesFulfilled {AreResourcesFulfilled()}");
        if (IsConstructed || !isReady || !AreResourcesFulfilled()) yield break;

        while (constructionProgress < 100)
        {
            if (builderAgent.agentBase.IsResting() || builderAgent.agentBase.IsSleepingOnGround())
                yield return new WaitUntil(() => !builderAgent.agentBase.IsResting() && !builderAgent.agentBase.IsSleepingOnGround());
            yield return new WaitForSeconds(1);
            constructionProgress += constructionSpeed;
            Debug.Log($"Construction à {constructionProgress}%");

            if (constructionProgress >= 100)
            {
                constructionProgress = 100;
                ConstructHouse(housePrefab);
                Debug.Log("Construction de la maison terminée !");
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
    private void ConstructHouse(GameObject housePrefab)
    {
        ConstructedHouse = Instantiate(housePrefab, ConstructionSitePosition, Quaternion.identity);
        foreach (var agent in CoupleAgents)
        {
            agent.CurrentRequest = null;
            agent.assignedHouse = ConstructedHouse;
            Debug.Log($"{agent.name} a reçu une maison.");
        }      
        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(2);
        Destroy(gameObject);
    }

    public void MoveAgentsToConstruction(Vector3 housePosition)
    {
        foreach (AgentBase cAgent in CoupleAgents)
        {
            if (!cAgent.isMoving)
            {

                cAgent.MoveTo(housePosition);
            }

        }
    }
}
