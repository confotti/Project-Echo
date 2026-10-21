using UnityEngine;

public class Spear : MonoBehaviour
{
    [Header("Player Respawn")]
    [SerializeField] private Transform playerRestartPoint;

    private bool playerHit;

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();

        if (player != null && !playerHit)
        {
            playerHit = true;

            if (playerRestartPoint != null)
            {
                player.RespawnAt(playerRestartPoint.position,playerRestartPoint.rotation);
            }
            else
            {
                Debug.LogWarning(
                    "SpearTrap: Assign the player's restart point.", this
                );
            }
            return;
        }

        CloneObject clone = other.GetComponentInParent<CloneObject>();

        if (clone != null)
        {
            clone.KillFromWeapon();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovement>() != null)
        {
            playerHit = false;
        }
    }
}