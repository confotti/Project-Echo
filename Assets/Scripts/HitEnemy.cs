using System.Collections;
using UnityEngine;

public class HitEnemy : MonoBehaviour
{
    public Material hitMaterial;
    public GameObject enemy;

    private Material originalMaterial;

    private void Start()
    {
        originalMaterial = enemy.GetComponent<MeshRenderer>().material;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sword"))
        {
            enemy.GetComponent<MeshRenderer>().material = hitMaterial;
            StartCoroutine(HitDuration());
            Debug.Log("Player hit");
        }
    }

    IEnumerator HitDuration()
    {
        yield return new WaitForSeconds(.1f);

        enemy.GetComponent<MeshRenderer>().material = originalMaterial;
    }
} 