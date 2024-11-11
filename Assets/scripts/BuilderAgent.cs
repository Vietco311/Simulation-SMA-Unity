using UnityEngine;
using System.Collections;
using UnityEditor.PackageManager.Requests;

public class BuilderAgent : RoleBase
{
    public ConstructionRequest buildingRequest;
    public ConstructionRequest BuildingRequest { get => buildingRequest; set => buildingRequest = value; }
    private bool isBuilding = false;

    protected override void PerformAction()
    {
        if (buildingRequest == null)
        {
            MoveRandomly();
            return;
        }

        if (buildingRequest != null)
        {
            agentBase.MoveTo(buildingRequest.ConstructionSitePosition);
            // Vérifier si on est arrivé sur le chantier
            if (Vector3.Distance(transform.position, buildingRequest.ConstructionSitePosition) < 1.0f && !agentBase.isMoving)
            {
                Debug.Log("Arrivé sur le chantier de construction");
                if (!isBuilding)
                {
                    Debug.Log("Commencer la construction");
                    isBuilding = true;
                    agentBase.LastCoroutine = StartCoroutine(BuildHouse());
                }
            }
        }
    }

    private IEnumerator BuildHouse()
    {
        if (buildingRequest == null || buildingRequest.IsConstructed)
        {
            yield break;
        }

        currentState = AgentState.BuildingHouse;
        Coroutine buildCoroutine = null;

        while (!buildingRequest.IsConstructed)
        {
            if (!agentBase.IsResting() && !agentBase.IsSleepingOnGround())
            {
                if (buildCoroutine == null)
                {
                    buildCoroutine = StartCoroutine(buildingRequest.AdvanceConstruction(buildingRequest.ConstructedHouse, this));
                    yield return buildCoroutine;
                }
            }
            else
            {
                if (buildCoroutine != null)
                {
                    StopCoroutine(buildCoroutine);
                    buildCoroutine = null;
                }

                yield return new WaitUntil(() => !agentBase.IsResting() && !agentBase.IsSleepingOnGround());
            }
        }

        Debug.Log("Construction terminée");
        agentBase.constructionManager.RemoveBusyBuilder(this);
        buildingRequest = null;
        isBuilding = false;
        currentState = AgentState.Idle;
    }


}
