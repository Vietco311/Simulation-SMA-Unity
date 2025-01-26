using UnityEngine;

public class ResourceBase : MonoBehaviour
{
    protected int amount;
    protected int weightPerUnit;
    private AgentBase reservedBy = null;

    public int Collect()
    {
        int collected = amount;
        amount = 0; 
        Destroy(gameObject); 
        return collected;
    }

    public int WeightPerUnit { get => weightPerUnit; set => weightPerUnit = value; }

    public bool IsReserved()
    {
        return reservedBy != null;
    }

    public void Reserve(AgentBase collector)
    {
        reservedBy = collector;
    }

    public void Unreserve()
    {
        reservedBy = null;
    }

    public bool IsReservedBy(AgentBase collector)
    {
        return reservedBy == collector;
    }
}

