using UnityEngine;
using System.Collections;
using UnityEditor.PackageManager.Requests;

public class BuilderAgent : RoleBase
{
    public ConstructionRequest buildingRequest;
    public ConstructionRequest BuildingRequest { get => buildingRequest; set => buildingRequest = value; }
    private bool isBuilding = false;
    private Coroutine buildCoroutine = null;

    protected override void Start()
    {
        base.Start();
        constructionManager.OnConstructionRequestAdded += HandleNewConstructionRequest;
    }

    protected override void Update()
    {
        base.Update();
    }

    private void HandleNewConstructionRequest(ConstructionRequest request)
    {
        if (buildingRequest == null)
        {
            buildingRequest = request;
            buildingRequest.OnResourcesDelivered += OnResourcesDelivered; // S'abonner à l'événement
            Debug.Log($"{gameObject.name} a reçu une nouvelle requête de construction.");
        }
    }

    private void OnResourcesDelivered()
    {
        if (buildingRequest != null && !isBuilding)
        {
            Debug.Log("Ressources livrées, prêt à commencer la construction.");
            currentState = AgentState.BuildingHouse;
            isBuilding = true;
            LastCoroutine = StartCoroutine(BuildHouse());
        }
    }

    protected override void PerformAction()
    {
        if (buildingRequest == null)
        {
            MoveRandomly();
            return;
        }

        if (buildingRequest != null)
        {
            MoveTo(buildingRequest.ConstructionSitePosition);
            if (Vector3.Distance(transform.position, buildingRequest.ConstructionSitePosition) < 1.0f && !isMoving)
            {
                if (!isBuilding && buildingRequest.AreResourcesFulfilled())
                {
                    Debug.Log("Commencer la construction");
                    currentState = AgentState.BuildingHouse;
                    isBuilding = true;
                    LastCoroutine = StartCoroutine(BuildHouse());
                }
                else if (currentState == AgentState.Idle)
                {
                    Debug.Log("Reprendre la construction après le repos");
                    currentState = AgentState.BuildingHouse;
                    if (buildCoroutine == null)
                    {
                        buildCoroutine = StartCoroutine(BuildHouse());
                    }
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

        while (!buildingRequest.IsConstructed)
        {
            if (!IsResting() && !IsSleepingOnGround())
            {
                if (buildCoroutine == null)
                {
                    buildCoroutine = StartCoroutine(buildingRequest.AdvanceConstruction(this));
                    yield return buildCoroutine;
                    buildCoroutine = null;
                }
            }
            else
            {
                if (buildCoroutine != null)
                {
                    StopCoroutine(buildCoroutine);
                    buildCoroutine = null;
                }

                yield return new WaitUntil(() => !IsResting() && !IsSleepingOnGround());
            }
        }

        Debug.Log("Construction terminée");
        constructionManager.RemoveBusyAgent(this);
        constructionManager.OnConstructionRequestAdded -= HandleNewConstructionRequest;
        buildingRequest = null;
        isBuilding = false;
        currentState = AgentState.Idle;
    }
}
