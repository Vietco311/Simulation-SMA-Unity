using UnityEngine;
using UnityEngine.UIElements;

public class ExplorerAgent : RoleBase
{

    protected override void PerformAction()
    {
        // D�placement al�atoire pour explorer la carte
        MoveRandomly();
    }
}
