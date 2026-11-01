
using UnityEngine;

public class deathzone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("IM FALLING");

            playerPerish perish = other.GetComponent<playerPerish>();

            if (perish != null)
            {
                perish.Die();
            }
        }
    }
}
