using System;
using Unity.VisualScripting;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject stoneResourcePrefab;
    public GameObject woodResourcePrefab;
    public int width = 30;
    public int height = 30;
    public GameObject tilePrefab;
    public GameObject builderPrefab;
    public GameObject explorerPrefab;
    public GameObject deliveringPrefab;
    public GameObject collectorPrefab;
    public GameObject joblessPrefab;
    public GameObject villagerPrefab;
    public GameObject woodStoragePrefab;
    public GameObject stoneStoragePrefab;
    private Vector3 offset;

    public void StartSimulation(string simulationType)
    {
        if (simulationType == "original")
        {
            SpawnerOriginal spawner = this.AddComponent<SpawnerOriginal>();
            spawner.builderPrefab = this.builderPrefab;
            spawner.explorerPrefab = this.explorerPrefab;
            spawner.deliveringPrefab = this.deliveringPrefab;
            spawner.collectorPrefab = this.collectorPrefab;
            spawner.joblessPrefab = this.joblessPrefab;
        }
        else if (simulationType == "simple")
        {
            SpawnerSimple spawner = this.AddComponent<SpawnerSimple>();
            spawner.agentPrefab = this.villagerPrefab;
        }
        else
        {
            Debug.LogError("Type de simulation inconnu.");
        }
    }

    protected virtual void Start()
    {
        offset = new Vector3(-width / 2, -height / 2, 0);
        GenerateGrid();
        CenterCamera();
        SpawnResources();
        Vector3 storageWPosition = new Vector3(-1, 0, 0);
        Instantiate(woodStoragePrefab, storageWPosition, Quaternion.identity);
        Vector3 storageSPosition = new Vector3(1, 0, 0);
        Instantiate(stoneStoragePrefab, storageSPosition, Quaternion.identity);
    }

    private void Update()
    {

    }

    void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(x, y, 0) + offset;
                Instantiate(tilePrefab, position, Quaternion.identity, transform);
            }
        }
    }

    protected virtual void SpawnPopulation(int count) { }

    void SpawnResources()
    {
        int resourceCount = 20;
        for (int i = 0; i < resourceCount; i++)
        {
            Vector3 woodPosition = new Vector3(UnityEngine.Random.Range(-width / 2, width / 2), UnityEngine.Random.Range(-height / 2, height / 2), 0);
            Vector3 stonePosition = new Vector3(UnityEngine.Random.Range(-width / 2, width / 2), UnityEngine.Random.Range(-height / 2, height / 2), 0);
            Instantiate(woodResourcePrefab, woodPosition, Quaternion.identity);
            Instantiate(stoneResourcePrefab, stonePosition, Quaternion.identity);
        }
    }

    void CenterCamera()
    {
        Camera.main.transform.position = new Vector3(0, 0, -10);
    }
}
