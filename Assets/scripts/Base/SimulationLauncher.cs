using UnityEngine;

public class SimulationLauncher : MonoBehaviour
{
    public GridManager grid;
    void Start()
    {
        // Choisissez la simulation à lancer
        string simulationType = "original"; // simple ou original
        grid.StartSimulation(simulationType);
    }
}
