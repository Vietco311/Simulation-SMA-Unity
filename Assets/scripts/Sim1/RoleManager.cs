using UnityEngine;

public class RoleManager : MonoBehaviour
{
    // Start is called une fois avant la première exécution de Update après la création du MonoBehaviour
    void Start()
    {
    }

    // Update est appelé une fois par frame
    void Update()
    {
        AssignAllRolesOnUpdate();
    }

    private void AssignRoleOnUpdate(ref int currentCount, int neededCount, string role)
    {
        foreach (var agent in FindObjectsByType<RoleBase>(FindObjectsSortMode.None))
        {
            if (currentCount >= neededCount)
                break;

            if (agent != null && string.IsNullOrEmpty(agent.AgentRole) && agent.GetComponent<RoleBase>() == null)
            {
                agent.AssignRoleToAgent(role);
                currentCount++;
            }
        }
    }

    public void AssignAllRolesOnUpdate()
    {
        CountRoles(out int joblessCount, out int collectorCount, out int builderCount, out int explorerCount, out int deliveryCount);

        int totalPeople = collectorCount + builderCount + explorerCount + deliveryCount + joblessCount;
        int neededCollectors = Mathf.RoundToInt(totalPeople / 5);
        int neededBuilders = Mathf.RoundToInt(totalPeople / 10);
        int neededExplorers = Mathf.RoundToInt(totalPeople / 10);
        int neededDeliveries = Mathf.RoundToInt(totalPeople / 5);

        AssignRoleOnUpdate(ref collectorCount, neededCollectors, "Collector");
        AssignRoleOnUpdate(ref builderCount, neededBuilders, "Builder");
        AssignRoleOnUpdate(ref explorerCount, neededExplorers, "Explorer");
        AssignRoleOnUpdate(ref deliveryCount, neededDeliveries, "Delivery");
        AssignRoleOnUpdate(ref joblessCount, totalPeople - (collectorCount + builderCount + explorerCount + deliveryCount), "Jobless");
    }

    private void CountRoles(out int joblessCount, out int collectorCount, out int builderCount, out int explorerCount, out int deliveryCount)
    {
        joblessCount = 0;
        collectorCount = 0;
        builderCount = 0;
        explorerCount = 0;
        deliveryCount = 0;

        // Compte les agents par rôle
        foreach (var agent in FindObjectsByType<AgentBase>(FindObjectsSortMode.None))
        {
            switch (agent.AgentRole)
            {
                case "Collector":
                    collectorCount++;
                    break;
                case "Builder":
                    builderCount++;
                    break;
                case "Explorer":
                    explorerCount++;
                    break;
                case "Delivery":
                    deliveryCount++;
                    break;
                default:
                    joblessCount++;
                    break;
            }
        }
    }
}
