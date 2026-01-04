using UnityEngine;

public class CollectibleItem2D : MonoBehaviour
{

    void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Player"))
        {
            Debug.Log(" ∞»°¡À∞Î‘¬∞Â£°");


            Destroy(gameObject);
        }
    }
}