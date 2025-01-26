using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.UIElements;
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
    protected bool canProcreate = true;

    public ConstructionRequest currentRequest;
    public ConstructionRequest CurrentRequest { get => currentRequest; set => currentRequest = value; }
    public int WeightCapacity { get => weightCapacity; set => weightCapacity = value; }
    public string AgentRole { get; set; }

    public float energy = 100f;
    protected float energyDecreaseRate = 1f;
    protected float restEnergyIncreaseRate = 5f;
    protected Coroutine lastCoroutine;
    public AgentState currentState = AgentState.Idle;
    public Coroutine LastCoroutine { get => lastCoroutine; set => lastCoroutine = value; }

    public enum AgentState
    {
        Idle,
        Moving,
        GatheringResources,
        DeliveringResources,
        BuildingHouse,
        Resting,
        CollectingResources,
        SleepingOnGround
    }

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
        if (currentState == AgentState.Resting) return;
        ReduceEnergyBasedOnState();

        // Prioriser le repos si l'énergie est faible
        if (energy <= 20f && assignedHouse != null && !IsResting() && !IsSleepingOnGround())
        {
            MoveToHouseAndRest();
        }
        else if (energy == 0f && !IsSleepingOnGround())
        {
            Rest();
        }
        else if (actionQueue.Count > 0 && !isMoving)
        {
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

    private void ReduceEnergyBasedOnState()
    {
        float energyReductionRate = 0f;

        switch (currentState)
        {
            case AgentState.Idle:
                energyReductionRate = 0.5f;
                break;
            case AgentState.Moving:
                energyReductionRate = 1.5f;
                break;
            case AgentState.GatheringResources:
                energyReductionRate = 2.0f;
                break;
            case AgentState.DeliveringResources:
                energyReductionRate = 1.8f;
                break;
            case AgentState.BuildingHouse:
                energyReductionRate = 2.5f;
                break;
            case AgentState.CollectingResources:
                energyReductionRate = 1.5f;
                break;
            case AgentState.SleepingOnGround:
                energyReductionRate = 0f;
                break;
        }

        energy -= energyReductionRate * Time.deltaTime;
        if (energy < 0f)
        {
            energy = 0f;
        }
    }

    public bool IsResting()
    {
        return currentState == AgentState.Resting;
    }

    public bool IsSleepingOnGround()
    {
        return currentState == AgentState.SleepingOnGround;
    }

    public void Rest()
    {
        if (currentState == AgentState.Resting) return;

        currentState = AgentState.Resting;
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
        currentState = restingState;

        while (energy < 100f)
        {
            energy += energyIncreaseRate * Time.deltaTime;
            yield return null;
        }

        currentState = AgentState.Idle;
        while (tempActionQueue.Count > 0)
        {
            actionQueue.Enqueue(tempActionQueue.Dequeue());
        }
    }

    public void QueueAction(Action newAction, bool isPriority = false)
    {
        if (!isPriority && actionQueue.Count >= 4)
        {
            Debug.LogWarning("La file d'attente des actions non prioritaires est pleine. Impossible d'ajouter une nouvelle action.");
            return;
        }

        // Vérifier si une action similaire est déjà présente dans la file d'attente
        if (actionQueue.Contains(newAction))
        {
            Debug.LogWarning("Action similaire déjà présente dans la file d'attente. Ignorée.");
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
        isMoving = true;
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            currentState = AgentState.Moving;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
        isMoving = false;
        currentState = AgentState.Idle;
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
        int woodWeight = inventory.ContainsKey("Wood") ? inventory["Wood"] : 0;
        int stoneWeight = inventory.ContainsKey("Stone") ? inventory["Stone"] * 2 : 0;

        return woodWeight + stoneWeight;
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

    protected void SetColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }

    private void MoveToHouseAndRest()
    {
        if (assignedHouse != null)
        {
            MoveTo(assignedHouse.transform.position);
            if (Vector3.Distance(transform.position, assignedHouse.transform.position) < 1.0f)
            {
                Rest();
            }
        }
    }

    protected bool CanProcreate()
    {
        return assignedHouse != null && half != null && half.assignedHouse == assignedHouse && energy >= 60f && half.energy >= 60f && canProcreate && half.canProcreate;
    }

    protected IEnumerator ProcreateCoroutine()
    {
        Debug.Log($"{gameObject.name} et {half.gameObject.name} commencent à procréer !");
        currentState = AgentState.Idle;
        half.currentState = AgentState.Idle;

        yield return new WaitForSeconds(10);

        Debug.Log($"{gameObject.name} et {half.gameObject.name} ont procréé !");
        Vector3 spawnPosition = assignedHouse.transform.position + new Vector3(1, 0, 0);
        GameObject newAgent = Instantiate(gameObject, spawnPosition, Quaternion.identity);
        SimpleAgent newSimpleAgent = newAgent.GetComponent<SimpleAgent>();
        newSimpleAgent.half = null;
        newSimpleAgent.assignedHouse = null;
        newSimpleAgent.energy = 100f;


        if (UnityEngine.Random.value < 0.1f) 
        {
            GameObject twinAgent = Instantiate(gameObject, spawnPosition + new Vector3(1, 0, 0), Quaternion.identity);
            SimpleAgent newTwinAgent = twinAgent.GetComponent<SimpleAgent>();
            newTwinAgent.half = null;
            newTwinAgent.assignedHouse = null;
            newTwinAgent.energy = 100f;
            Debug.Log($"{gameObject.name} et {half.gameObject.name} ont eu des jumeaux !");
        }

        energy -= 20f;
        half.energy -= 20f;

        canProcreate = false;
        half.canProcreate = false;
        yield return new WaitForSeconds(60);
        canProcreate = true;
        half.canProcreate = true;
    }
}
