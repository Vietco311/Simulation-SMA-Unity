using UnityEngine;

public class JoblessAgent : RoleBase
{


    private void Update()
    {
    }

    protected override void PerformAction()
    {
        // D�placement al�atoire pour explorer la carte
        MoveRandomly();
    }

}

