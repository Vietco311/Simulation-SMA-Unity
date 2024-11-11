using UnityEngine;

public class WoodResource : ResourceBase
{
    private void Start()
    {
        amount = Random.Range(5, 20);
        weightPerUnit = 1;
    }
}
