using System.Collections;
using UnityEngine;

public class HitObject : MonoBehaviour
{
    public Material hitMaterial;
    public GameObject button;

    public bool isHit = false;
    private Material originalMaterial;

    private void Start()
    {
        originalMaterial = button.GetComponent<MeshRenderer>().material;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sword"))
        {
            isHit = true;
            button.GetComponent<MeshRenderer>().material = hitMaterial;
            StartCoroutine(HitDuration());
            Debug.Log("Player hit");
        }
    }

    IEnumerator HitDuration()
    {
        yield return new WaitForSeconds(3f);

        isHit = false;
        button.GetComponent<MeshRenderer>().material = originalMaterial;
    }
} 