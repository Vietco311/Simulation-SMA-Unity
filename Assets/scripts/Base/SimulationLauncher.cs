using UnityEngine;

public class SimulationLauncher : MonoBehaviour
{
    public GridManager grid;
    void Start()
    {
        // Choisissez la simulation � lancer
        string simulationType = "original"; // simple ou original
        grid.StartSimulation(simulationType);
    }
}
