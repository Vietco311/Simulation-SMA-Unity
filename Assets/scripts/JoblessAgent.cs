using UnityEngine;

public class JoblessAgent : RoleBase
{


    private void Update()
    {
    }

    protected override void PerformAction()
    {
        // Déplacement aléatoire pour explorer la carte
        MoveRandomly();
    }

}

