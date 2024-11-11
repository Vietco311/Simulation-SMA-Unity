using UnityEngine;

public abstract class StorageBase : MonoBehaviour
{
    private int amount = 0;

    public int Amount {  get { return amount; } }
    public void ReceiveResource(int resourceAmount)
    {
        amount += resourceAmount;
    }

    public int GiveResource(int resourceAmount)
    {
        amount -= resourceAmount;
        return resourceAmount;
    }


}
