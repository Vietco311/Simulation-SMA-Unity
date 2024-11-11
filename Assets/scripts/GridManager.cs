using System;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject stoneResourcePrefab;
    public GameObject woodResourcePrefab;
    public int width = 30;
    public int height = 30; 
    public GameObject tilePrefab;
    public GameObject agentPrefab;
    public GameObject woodStoragePrefab;
    public GameObject stoneStoragePrefab;
    private int totalPeople;
    private int totalCollector;
    private int totalBuilder;
    private int totalDelivery;
    private int totalExplorer;
    private int totalJobless;


    private Vector3 offset; // Déclare un offset global pour l'alignement

    void Start()
    {
        offset = new Vector3(-width / 2, -height / 2, 0); // Calcule l'offset une seule fois
        GenerateGrid();
        CenterCamera();
        SpawnResources();
        Vector3 storageWPosition = new Vector3(-1, 0, 0);
        Instantiate(woodStoragePrefab, storageWPosition, Quaternion.identity);
        Vector3 storageSPosition = new Vector3(1, 0, 0);
        Instantiate(stoneStoragePrefab, storageSPosition, Quaternion.identity);    
        SpawnPopulation(10);
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

    void SpawnPopulation(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = new Vector3(UnityEngine.Random.Range(-width / 2, width / 2), UnityEngine.Random.Range(-height / 2, height / 2), 0);
            GameObject agent = Instantiate(agentPrefab, spawnPosition, Quaternion.identity);
        }
        
    }


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
