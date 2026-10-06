using UnityEngine;

public class RespawnPlayer : MonoBehaviour
{
    public GameObject player;
    public Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            player.transform.position = respawnPoint.position;
            player.transform.rotation = respawnPoint.rotation;
            Debug.Log("Player found");
        }
    }
}
