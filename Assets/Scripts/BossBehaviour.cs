using UnityEngine;
using System.Collections;

public class BossBehaviour : MonoBehaviour
{
    public Transform player;
    public float health;
    public float damagePerHit;
    private float currentHealth; 

    public Material hitMaterial;
    public GameObject enemy;

    private Material originalMaterial;
    public ParticleSystem deathvfx; 

    private void Start()
    {
        originalMaterial = enemy.GetComponent<MeshRenderer>().material;
        currentHealth = health; 
    }

    void Update()
    {
        if (player != null)
        {
            transform.LookAt(player);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sword"))
        {
            enemy.GetComponent<MeshRenderer>().material = hitMaterial;
            TakeDamage(); 
            StartCoroutine(HitDuration());
            Debug.Log("Player hit");
        }
    }

    IEnumerator HitDuration()
    {
        yield return new WaitForSeconds(.1f);

        enemy.GetComponent<MeshRenderer>().material = originalMaterial;
    }

    private void TakeDamage()
    {
        currentHealth -= damagePerHit;
        if (currentHealth <= 0)
        {
            deathvfx.Play(); 
            Destroy(enemy); 
        }
    }
}
