using UnityEngine;

public class ResourceBase : MonoBehaviour
{

    protected int amount;
    protected int weightPerUnit;


    public int Collect()
    {
        int collected = amount;
        amount = 0; // Une fois collecté, la ressource est épuisée
        Destroy(gameObject); // Détruire l'objet une fois collecté
        return collected;
    }

    public int WeightPerUnit { get; set; }
}

