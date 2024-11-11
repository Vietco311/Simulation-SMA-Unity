using UnityEngine;
using UnityEngine.UIElements;

public class ExplorerAgent : RoleBase
{

    protected override void PerformAction()
    {
        // Déplacement aléatoire pour explorer la carte
        MoveRandomly();
    }
}
