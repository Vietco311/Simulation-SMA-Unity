using UnityEngine;

public class JoblessAgent : RoleBase
{


    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void PerformAction()
    {
        // D�placement al�atoire pour explorer la carte
        MoveRandomly();
    }

}

