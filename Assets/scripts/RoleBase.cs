using UnityEngine;

public abstract class RoleBase : MonoBehaviour
{
    public AgentState currentState = AgentState.Idle;
    public AgentBase agentBase;

    private void Start()
    {
        agentBase = gameObject.GetComponent<AgentBase>();
    }

    private void Update()
    {
        if (agentBase.actionQueue.Count == 0 && !agentBase.isMoving)
        {
            agentBase.QueueAction(() => PerformAction());
        }

        // Réduire l'énergie en fonction de l'état actuel
        ReduceEnergyBasedOnState();
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
            case AgentState.Resting:
                energyReductionRate = 0f;
                break;
            case AgentState.CollectingResources:
                energyReductionRate = 1.5f;
                break;
            case AgentState.SleepingOnGround:
                energyReductionRate = 0f;
                break;
        }

        agentBase.energy -= energyReductionRate * Time.deltaTime;
        if (agentBase.energy < 0f)
        {
            agentBase.energy = 0f;
        }
    }

    public void AssignRoleToAgent(string role)
    {
        if (agentBase == null)
        {
            agentBase = gameObject.GetComponent<AgentBase>();
            if (agentBase == null)
            {
                Debug.LogError("AgentBase component is missing on the GameObject.");
                return;
            }
        }
        Destroy(this);
        agentBase.ChangeRole(role);
    }

    public void MoveRandomly()
    {
        Vector3 randomDirection = Random.insideUnitSphere * 5.0f;
        randomDirection += transform.position;
        randomDirection.y = transform.position.z;
        agentBase.MoveTo(randomDirection);
    }

    protected abstract void PerformAction();

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
}
