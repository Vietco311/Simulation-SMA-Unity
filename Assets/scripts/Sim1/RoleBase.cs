using UnityEngine;

public abstract class RoleBase : AgentBase
{
    protected override void Start()
    {
        base.Start();
        QueueAction(() => PerformAction(), true); // Assurez-vous que l'action PerformAction est mise en file d'attente dès le début
    }

    protected override void Update()
    {
        base.Update();
        if (actionQueue.Count == 0 && currentState == AgentState.Idle)
        {
            QueueAction(() => PerformAction(), true); // Assurez-vous que l'action PerformAction est mise en file d'attente si l'agent est inactif
        }
    }

    public void AssignRoleToAgent(string role)
    {
        Debug.Log($"Début du changement de rôle en : {role}");

        switch (role)
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
                Debug.LogWarning($"Rôle non pris en charge : {role}");
                break;
        }

        Debug.Log($"Fin du changement de rôle en : {role}");
    }

    public void MoveRandomly()
    {
        Vector3 randomDirection = Random.insideUnitSphere * 5.0f;
        randomDirection += transform.position;
        randomDirection.y = transform.position.z;
        MoveTo(randomDirection);
    }

    protected abstract void PerformAction();
}
