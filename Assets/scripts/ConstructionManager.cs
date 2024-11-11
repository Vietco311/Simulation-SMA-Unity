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

    private Dictionary<string, Dictionary<string, int>> houseResourceRequirements;
    private List<BuilderAgent> busyBuilders = new List<BuilderAgent>();

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
            BuilderAgent availableBuilder = FindAvailableBuilder();
            if (availableBuilder != null)
            {
                var request = requests.Dequeue();
                ProcessConstructionRequest(request, availableBuilder);
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

    public void AddBusyBuilder(BuilderAgent builder, ConstructionRequest request)
    {
        if (!busyBuilders.Contains(builder))
        {
            builder.buildingRequest = request;
            busyBuilders.Add(builder);
        }
    }

    public void RemoveBusyBuilder(BuilderAgent builder)
    {
        if (busyBuilders.Contains(builder))
        {
            busyBuilders.Remove(builder);
        }
        else
        {
            Debug.LogWarning("Le constructeur n'est pas dans la liste des constructeurs occupés.");
        }
    }

    private BuilderAgent FindAvailableBuilder()
    {
        // Trouver un constructeur disponible
        var allBuilders = FindObjectsByType<BuilderAgent>(FindObjectsSortMode.None);
        return allBuilders.FirstOrDefault(builder => !busyBuilders.Contains(builder));
    }

    private void ProcessConstructionRequest(ConstructionRequest request, BuilderAgent builder)
    {
        AddBusyBuilder(builder, request);
        request.SetToReady();
        DeliveringAgent.AddDeliverRequest(request);
    }

    public void RequestHouse(string houseType, Vector3 housePosition, List<AgentBase> coupleAgents)
    {
        // Création d'un GameObject temporaire pour ajouter le composant ConstructionRequest
        GameObject requestObject = new GameObject("ConstructionRequest");
        ConstructionRequest newRequest = requestObject.AddComponent<ConstructionRequest>();

        newRequest.HouseType = houseType;
        newRequest.ConstructionSitePosition = housePosition;
        newRequest.CoupleAgents = coupleAgents;
        newRequest.SetResourcesNeeded(houseResourceRequirements[houseType]);
        foreach (var agentCouple in coupleAgents)
        {
            agentCouple.CurrentRequest = newRequest;
        }
        requests.Enqueue(newRequest);
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
