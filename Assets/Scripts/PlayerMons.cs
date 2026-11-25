using UnityEngine;

public class MonCollectible : MonoBehaviour
{
    [Header("Collectible Settings")]
    public int value = 10;

    private bool collected = false;

    void OnTriggerEnter(Collider other)
    {
        if (collected)
            return;

        PlayerMoney playerMoney =
            other.GetComponentInParent<PlayerMoney>();

        PlayerHealth playerHealth =
            other.GetComponentInParent<PlayerHealth>();

        if (playerMoney == null || playerHealth == null)
            return;

        if (playerHealth.isDead ||
            playerMoney.rewardsCollected)
            return;

        collected = true;

        playerMoney.AddMons(value);

        Destroy(gameObject);
    }
}