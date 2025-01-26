using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimpleAgent : AgentBase
{
    private ResourceBase currentTargetResource;

    protected override void Start()
    {
        base.Start();
        StartCoroutine(AgentBehavior());
    }

    protected override void Update()
    {
        base.Update();
    }

    private IEnumerator AgentBehavior()
    {
        while (true)
        {
            if (energy <= 30f && !(currentState == AgentState.Resting || currentState == AgentState.SleepingOnGround))
            {
                MoveToHouseAndRest();
                yield return new WaitUntil(() => currentState == AgentState.Idle);
            }
            else if (assignedHouse == null)
            {
                if (half == null)
                {
                    FindPartner();
                }
                else
                {
                    if (currentRequest == null)
                    {
                        RequestHouseConstruction();
                    }
                    else
                    {
                        if (!currentRequest.AreResourcesFulfilled())
                        {
                            GatherResources();
                            yield return new WaitUntil(() => currentState == AgentState.Idle);
                        }
                        else if (!currentRequest.IsConstructed)
                        {
                            BuildHouse();
                            yield return new WaitUntil(() => currentState == AgentState.Idle);
                        }
                    }
                }
            }
            else if (CanProcreate())
            {
                yield return StartCoroutine(ProcreateCoroutine());
            }
            else if (energy > 30f)
            {
                MoveRandomlyAroundHouse();
                yield return new WaitUntil(() => currentState == AgentState.Idle);
            }

            yield return new WaitForSeconds(1);
        }
    }

    private void FindPartner()
    {
        SimpleAgent[] allAgents = FindObjectsOfType<SimpleAgent>();
        foreach (var agent in allAgents)
        {
            if (agent != this && agent.half == null)
            {
                half = agent;
                agent.half = this;
                Debug.Log($"{gameObject.name} est maintenant en couple avec {agent.gameObject.name}.");
                break;
            }
        }
    }

    private void GatherResources()
    {
        if (inventory["Wood"] < currentRequest.resourceNeededForHouse["Wood"])
        {
            MoveToResource("Wood");
        }
        else if (inventory["Stone"] < currentRequest.resourceNeededForHouse["Stone"])
        {
            MoveToResource("Stone");
        }
        else
        {
            DeliverResourcesToConstruction();
        }
    }

    private void MoveToResource(string resourceType)
    {
        ResourceBase resource = FindClosestResource(resourceType);
        if (resource != null && !resource.IsReserved())
        {
            ReserveResource(resource);
            MoveTo(resource.transform.position);
            StartCoroutine(CheckProximityAndCollect(resource, resourceType));
        }
    }

    private IEnumerator CheckProximityAndCollect(ResourceBase resource, string resourceType)
    {
        while (Vector3.Distance(transform.position, resource.transform.position) > 1.0f)
        {
            yield return null;
        }

        if (resourceType == "Wood")
        {
            inventory["Wood"] += resource.Collect();
        }
        else if (resourceType == "Stone")
        {
            inventory["Stone"] += resource.Collect();
        }
        UnreserveResource();
    }

    private ResourceBase FindClosestResource(string resourceType)
    {
        GameObject[] resources = GameObject.FindGameObjectsWithTag(resourceType);
        ResourceBase closestResource = null;
        float minDistance = Mathf.Infinity;

        foreach (var resource in resources)
        {
            ResourceBase resourceBase = resource.GetComponent<ResourceBase>();
            float distance = Vector3.Distance(transform.position, resource.transform.position);
            if (distance < minDistance && !resourceBase.IsReserved())
            {
                minDistance = distance;
                closestResource = resourceBase;
            }
        }

        return closestResource;
    }

    private void DeliverResourcesToConstruction()
    {
        MoveTo(currentRequest.ConstructionSitePosition);

        if (Vector3.Distance(transform.position, currentRequest.ConstructionSitePosition) < 1.0f)
        {
            currentRequest.DeliverResources(inventory);
        }
    }

    private void BuildHouse()
    {
        if (currentRequest != null && currentRequest.AreResourcesFulfilled() && !currentRequest.IsConstructed)
        {
            MoveTo(currentRequest.ConstructionSitePosition);

            if (Vector3.Distance(transform.position, currentRequest.ConstructionSitePosition) < 1.0f)
            {
                StartCoroutine(currentRequest.AdvanceConstruction(this));
            }
        }
    }

    private void MoveRandomlyAroundHouse()
    {
        if (assignedHouse != null)
        {
            Vector3 randomDirection = Random.insideUnitSphere * 5.0f;
            randomDirection += assignedHouse.transform.position;
            randomDirection.z = 0;
            MoveTo(randomDirection);
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

    protected void ReserveResource(ResourceBase resource)
    {
        if (resource != null && !resource.IsReserved())
        {
            resource.Reserve(this);
            currentTargetResource = resource;
        }
    }

    protected void UnreserveResource()
    {
        if (currentTargetResource != null)
        {
            currentTargetResource.Unreserve();
            currentTargetResource = null;
        }
    }
}

