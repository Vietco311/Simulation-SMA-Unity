using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.UIElements;
using static RoleBase;
using System.Data;

public class AgentBase : MonoBehaviour
{
    public float moveSpeed = 1f;
    public string agentRole;
    protected SpriteRenderer spriteRenderer;
    public Dictionary<string, int> inventory = new Dictionary<string, int>();
    private int weightCapacity = 20;
    public GameObject assignedHouse;
    public ConstructionManager constructionManager;
    public bool isMoving = false;
    protected AgentBase half;
    public Queue<Action> actionQueue = new Queue<Action>();
    private Queue<Action> tempActionQueue = new Queue<Action>();

    public ConstructionRequest currentRequest;
    public ConstructionRequest CurrentRequest { get => currentRequest; set => currentRequest = value; }
    public int WeightCapacity { get => weightCapacity; set => weightCapacity = value; }
    public string AgentRole { get; set; }

    public float energy = 100f;
    private float energyDecreaseRate = 1f; 
    private float restEnergyIncreaseRate = 5f;
    private Coroutine lastCoroutine;
    public Coroutine LastCoroutine { get => lastCoroutine; set => lastCoroutine = value; }
    private bool isResting = false;

    private void Awake()
    {
    }

    protected virtual void Start()
    {
        InitializeAgent();
        constructionManager = FindAnyObjectByType<ConstructionManager>();
    }

    protected virtual void Update()
    {
        if (isResting) return;
        DecreaseEnergy();

        // Prioriser le repos si l'énergie est faible
        if (energy <= 20f && assignedHouse != null && !IsResting() && !IsSleepingOnGround())
        {
            Rest();
        }
        else if (energy == 0f && !IsSleepingOnGround())
        {
            Rest();
        }
        else if (actionQueue.Count > 0 && !isMoving)
        {
            Debug.Log("Action en cours d'exécution.");
            Action currentAction = actionQueue.Dequeue();
            currentAction.Invoke();
        }

        if (half != null && assignedHouse == null && currentRequest == null)
        {
            QueueAction(() => RequestHouseConstruction(), true);
        }
        AgentBase potentialPartner = FindPotentialPartner();
        if (potentialPartner != null)
        {
            PairWith(potentialPartner);
        }
    }

    public bool IsResting()
    {
        RoleBase role = gameObject.GetComponent<RoleBase>();
        return role.currentState == AgentState.Resting;
    }

    public bool IsSleepingOnGround()
    {
        RoleBase role = gameObject.GetComponent<RoleBase>();
        return role.currentState == AgentState.SleepingOnGround;
    }

    private void DecreaseEnergy()
    {
        energy -= energyDecreaseRate * Time.deltaTime;
        if (energy < 0f)
        {
            energy = 0f;
        }
    }

    private void Rest()
    {
        if (isResting) return;

        isResting = true;
        InterruptCurrentAction();

        while (actionQueue.Count > 0)
        {
            tempActionQueue.Enqueue(actionQueue.Dequeue());
        }

        if (energy == 0)
        {
            StartCoroutine(RestCoroutine(AgentState.SleepingOnGround, restEnergyIncreaseRate / 2));
        }
        else
        {
            StartCoroutine(RestCoroutine(AgentState.Resting, restEnergyIncreaseRate));
        }
    }

    private IEnumerator RestCoroutine(AgentState restingState, float energyIncreaseRate)
    {
        RoleBase role = gameObject.GetComponent<RoleBase>();
        role.currentState = restingState;

        while (energy < 100f)
        {
            energy += energyIncreaseRate * Time.deltaTime;
            yield return null;
        }

        role.currentState = AgentState.Idle;
        while (tempActionQueue.Count > 0)
        {
            actionQueue.Enqueue(tempActionQueue.Dequeue());
        }
        isResting = false; // Fin du repos
    }

    public void QueueAction(Action newAction, bool isPriority = false)
    {
        if (!isPriority && actionQueue.Count >= 4)
        {
            Debug.LogWarning("La file d'attente des actions non prioritaires est pleine. Impossible d'ajouter une nouvelle action.");
            return;
        }

        if (isPriority)
        {
            if (actionQueue.Count == 0)
            {
                actionQueue.Enqueue(newAction);
            }
            else
            {
                Queue<Action> tempQueue = new Queue<Action>();
                tempQueue.Enqueue(newAction);

                while (actionQueue.Count > 0)
                {
                    tempQueue.Enqueue(actionQueue.Dequeue());
                }

                actionQueue = tempQueue;
            }
        }
        else
        {
            actionQueue.Enqueue(newAction);
        }
    }

    private void InterruptCurrentAction()
    {
        if (lastCoroutine != null)
        {
            StopCoroutine(lastCoroutine);
        }
        isMoving = false;
        RoleBase role = gameObject.GetComponent<RoleBase>();
        role.currentState = AgentState.Idle;
    }



    private AgentBase FindPotentialPartner()
    {
        AgentBase[] allAgents = FindObjectsByType<AgentBase>(FindObjectsSortMode.None);
        foreach (var agent in allAgents)
        {
            if (agent != this && agent.half == null)
            {
                Debug.Log($"{gameObject.name} a trouvé un partenaire potentiel : {agent.gameObject.name}");
                return agent;
            }
        }
        return null;
    }

    public void PairWith(AgentBase otherAgent)
    {
        if (otherAgent == null)
        {
            Debug.LogError("L'autre agent est null.");
            return;
        }

        if (this.half != null || otherAgent.half != null)
        {
            Debug.LogWarning("Un des agents est déjà en couple.");
            return;
        }

        this.half = otherAgent;
        otherAgent.half = this;

        Debug.Log($"{this.gameObject.name} est maintenant en couple avec {otherAgent.gameObject.name}.");
    }

    public void RequestHouseConstruction()
    {
        if (currentRequest == null)
        {
            Vector3 housePosition = DetermineHousePosition();
            string houseType = DetermineHouseType();
            constructionManager.RequestHouse(houseType, housePosition, new List<AgentBase> { this, half });
            Debug.Log($"{gameObject.name} a demandé une maison.");
        }

    }

    public Vector3 DetermineHousePosition()
    {
        Vector3 villageMin = new Vector3(-15, -15, 0);
        Vector3 villageMax = new Vector3(14, 14, 0);

        float xPosition = UnityEngine.Random.Range(villageMin.x, villageMax.x);
        float yPosition = UnityEngine.Random.Range(villageMin.y, villageMax.y);

        Vector3 housePosition = new Vector3(xPosition, yPosition, 0);
        Debug.Log($"Position déterminée pour la maison : {housePosition}");
        return housePosition;
    }

    private string DetermineHouseType()
    {
        if (constructionManager.HasEnoughResourcesFor("StoneHouse"))
        {
            return "Stone";
        }
        else
        {
            return "Wood";
        }
    }

    public void ChangeRole(string newRole)
    {
        Debug.Log($"Début du changement de rôle en : {newRole}");

        switch (newRole)
        {
            case "Collector":
                gameObject.AddComponent<CollectorAgent>();
                SetColor(Color.green);
                agentRole = "Collector";
                Debug.Log("Nouveau rôle assigné : CollectorAgent");
                break;
            case "Builder":
                gameObject.AddComponent<BuilderAgent>();
                SetColor(Color.blue);
                agentRole = "Builder";
                Debug.Log("Nouveau rôle assigné : BuilderAgent");
                break;
            case "Explorer":
                gameObject.AddComponent<ExplorerAgent>();
                SetColor(Color.yellow);
                agentRole = "Explorer";
                Debug.Log("Nouveau rôle assigné : ExplorerAgent");
                break;
            case "Delivery":
                gameObject.AddComponent<DeliveringAgent>();
                SetColor(Color.red);
                agentRole = "Delivery";
                Debug.Log("Nouveau rôle assigné : DeliveringAgent");
                break;
            case "Jobless":
                gameObject.AddComponent<JoblessAgent>();
                SetColor(Color.gray);
                agentRole = "Jobless";
                Debug.Log("Nouveau rôle assigné : JoblessAgent");
                break;
            default:
                Debug.LogWarning($"Rôle non pris en charge : {newRole}");
                break;
        }

        Debug.Log($"Fin du changement de rôle en : {newRole}");
    }

    protected void SetColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }

    public void MoveTo(Vector3 targetPosition)
    {
        if (this != null && gameObject != null)
        {
            lastCoroutine = StartCoroutine(MoveTowardsPosition(targetPosition));
        }
    }

    private IEnumerator MoveTowardsPosition(Vector3 targetPosition)
    {
        RoleBase role = gameObject.GetComponent<RoleBase>();
        isMoving = true;
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            role.currentState = AgentState.Moving;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
        isMoving = false;
        role.currentState = AgentState.Idle;
    }

    private void InitializeAgent()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("No SpriteRenderer found on the agent!");
        }
        inventory.Add("Wood", 0);
        inventory.Add("Stone", 0);
    }

    public int GetTotalWeight()
    {
        int woodWeight = inventory["Wood"];
        int stoneWeight = inventory["Stone"] * 2;

        return woodWeight + stoneWeight;
    }
}
