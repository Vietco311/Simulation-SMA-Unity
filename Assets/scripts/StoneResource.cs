using UnityEngine;

public class StoneResource : ResourceBase
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        amount = Random.Range(2, 10);
        weightPerUnit = 2;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
