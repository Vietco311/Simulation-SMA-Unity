using System;
using UnityEngine;

public class SpawnerOriginal : MonoBehaviour
{
    public GameObject builderPrefab;
    public GameObject explorerPrefab;
    public GameObject deliveringPrefab;
    public GameObject collectorPrefab;
    public GameObject joblessPrefab;
    public int width = 30;
    public int height = 30;
    protected void Start()
    {
        SpawnPopulation(10); // Générer 10 agents pour la simulation originale
    }
    protected void SpawnPopulation(int count)
    {
        GameObject[] prefabs = { builderPrefab, explorerPrefab, deliveringPrefab, collectorPrefab, joblessPrefab };
        int prefabCount = prefabs.Length;
        int fullSets = count / prefabCount;
        int remainder = count % prefabCount;

        for (int i = 0; i < fullSets; i++)
        {
            foreach (GameObject prefab in prefabs)
            {
                Vector3 spawnPosition = new Vector3(UnityEngine.Random.Range(-width / 2, width / 2), UnityEngine.Random.Range(-height / 2, height / 2), 0);
                Instantiate(prefab, spawnPosition, Quaternion.identity);
            }
        }

        for (int i = 0; i < remainder; i++)
        {
            Vector3 spawnPosition = new Vector3(UnityEngine.Random.Range(-width / 2, width / 2), UnityEngine.Random.Range(-height / 2, height / 2), 0);
            Instantiate(prefabs[i], spawnPosition, Quaternion.identity);
        }
    }
}
