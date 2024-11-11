using UnityEngine;

public class ResourceBase : MonoBehaviour
{

    protected int amount;
    protected int weightPerUnit;


    public int Collect()
    {
        int collected = amount;
        amount = 0; // Une fois collect�, la ressource est �puis�e
        Destroy(gameObject); // D�truire l'objet une fois collect�
        return collected;
    }

    public int WeightPerUnit { get; set; }
}

