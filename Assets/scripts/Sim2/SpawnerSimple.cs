using System;
using UnityEngine;

public class SpawnerSimple : MonoBehaviour
{
    public GameObject agentPrefab;
    public int width = 30;
    public int height = 30;
    protected void Start()
    {
        SpawnPopulation(2); 
    }

    protected void SpawnPopulation(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = new Vector3(UnityEngine.Random.Range(-width / 2, width / 2), UnityEngine.Random.Range(-height / 2, height / 2), 0);
            GameObject agent = Instantiate(agentPrefab, spawnPosition, Quaternion.identity);
        }
    }
}
