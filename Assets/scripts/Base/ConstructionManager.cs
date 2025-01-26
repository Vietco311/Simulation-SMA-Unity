using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConstructionManager : MonoBehaviour
{
    private static ConstructionManager _instance;
    public static ConstructionManager Instance => _instance;
    public Queue<ConstructionRequest> requests = new Queue<ConstructionRequest>();
    public GameObject woodHousePrefab;
    public GameObject stoneHousePrefab;
    public delegate void ConstructionRequestAddedHandler(ConstructionRequest request);
    public event ConstructionRequestAddedHandler OnConstructionRequestAdded;
    private Dictionary<string, Dictionary<string, int>> houseResourceRequirements;
    private List<AgentBase> busyAgents = new List<AgentBase>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }

        InitializeHouseResourceRequirements();
    }

    private void InitializeHouseResourceRequirements()
    {
        houseResourceRequirements = new Dictionary<string, Dictionary<string, int>>
                                {
                                    { "Wood", new Dictionary<string, int> { { "Wood", 30 }, { "Stone", 0 } } },
                                    { "Stone", new Dictionary<string, int> { { "Wood", 15 }, { "Stone", 30 } } }
                                };
    }

    private void Update()
    {
        CheckPendingRequests();
    }

    private void CheckPendingRequests()
    {
        if (requests.Count > 0)
        {
            AgentBase availableAgent = FindAvailableAgent();
            if (availableAgent != null)
            {
                var request = requests.Dequeue();
                ProcessConstructionRequest(request, availableAgent);
            }
        }
    }

    public bool HasEnoughResourcesFor(string houseType)
    {
        switch (houseType)
        {
            case "Wood":
                return FindFirstObjectByType<WoodStorage>().Amount >= 30;
            case "Stone":
                return FindFirstObjectByType<WoodStorage>().Amount >= 15 && FindFirstObjectByType<StoneStorage>().Amount >= 30;
            default:
                return false;
        }
    }

    public GameObject GetPrefab(string houseType)
    {
        switch (houseType)
        {
            case "Wood":
                return woodHousePrefab;
            case "Stone":
                return stoneHousePrefab;
            default:
                Debug.LogError($"Prefab pour le type de maison '{houseType}' non trouvé !");
                return null; // ou gérer une erreur selon votre logique
        }
    }

    public void AddBusyAgent(AgentBase agent, ConstructionRequest request)
    {
        if (!busyAgents.Contains(agent))
        {
            agent.currentRequest = request;
            busyAgents.Add(agent);
        }
    }

    public void RemoveBusyAgent(AgentBase agent)
    {
        if (busyAgents.Contains(agent))
        {
            busyAgents.Remove(agent);
        }
        else
        {
            Debug.LogWarning("L'agent n'est pas dans la liste des agents occupés.");
        }
    }

    private AgentBase FindAvailableAgent()
    {
        // Trouver un agent disponible
        var allAgents = FindObjectsByType<AgentBase>(FindObjectsSortMode.None);
        return allAgents.FirstOrDefault(agent => !busyAgents.Contains(agent));
    }

    private void ProcessConstructionRequest(ConstructionRequest request, AgentBase agent)
    {
        AddBusyAgent(agent, request);
        request.SetToReady();
    }

    public void RequestHouse(string houseType, Vector3 housePosition, List<AgentBase> coupleAgents)
    {
        GameObject requestObject = new GameObject("ConstructionRequest");
        ConstructionRequest newRequest = requestObject.AddComponent<ConstructionRequest>();

        if (houseType == "Wood")
        {
            newRequest.ConstructedHouse = woodHousePrefab;
        }
        else if (houseType == "Stone")
        {
            newRequest.ConstructedHouse = stoneHousePrefab;
        }
        newRequest.ConstructionSitePosition = housePosition;
        newRequest.CoupleAgents = coupleAgents;
        newRequest.SetResourcesNeeded(houseResourceRequirements[houseType]);
        foreach (var agentCouple in coupleAgents)
        {
            agentCouple.CurrentRequest = newRequest;
        }
        requests.Enqueue(newRequest);

        OnConstructionRequestAdded?.Invoke(newRequest);

        // Abonner l'événement OnResourcesNeeded à la méthode HandleNewDeliverRequest
        newRequest.OnResourcesNeeded += DeliveringAgent.HandleNewDeliverRequest;
    }

    public Dictionary<string, int> GetAvailableResources()
    {
        Dictionary<string, int> resourceAvailableInTown = new Dictionary<string, int>();

        var woodStorage = FindAnyObjectByType<WoodStorage>();
        var stoneStorage = FindAnyObjectByType<StoneStorage>();

        resourceAvailableInTown["Wood"] = woodStorage != null ? woodStorage.Amount : 0;
        resourceAvailableInTown["Stone"] = stoneStorage != null ? stoneStorage.Amount : 0;

        return resourceAvailableInTown;
    }

    public bool AreTownResourcesSufficient(Dictionary<string, int> resourceRequirements)
    {
        var availableResources = GetAvailableResources();

        foreach (var requirement in resourceRequirements)
        {
            if (!availableResources.ContainsKey(requirement.Key) || availableResources[requirement.Key] < requirement.Value)
                return false;
        }
        return true;
    }
}
