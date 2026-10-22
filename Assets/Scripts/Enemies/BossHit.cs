using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHit : MonoBehaviour
{
    [Header("Shield Crystals")]
    [SerializeField] private List<GameObject> crystals = new List<GameObject>();
    [SerializeField] private int materialIndexToRemove = 1;
    [SerializeField] private float shieldDownDuration = 1f;

    public bool isHit;

    private readonly Dictionary<GameObject, Material[]> originalMaterials =
        new Dictionary<GameObject, Material[]>();

    private void Start()
    {
        // Save materials
        foreach (GameObject crystal in crystals)
        {
            if (crystal == null)
                continue;

            Renderer crystalRenderer = crystal.GetComponent<Renderer>();

            if (crystalRenderer != null)
                originalMaterials[crystal] = crystalRenderer.materials;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Sword") || isHit)
            return;

        isHit = true;
        StartCoroutine(TemporarilyRemoveShield());

        Debug.Log("Button hit - shield lowered");
    }

    private IEnumerator TemporarilyRemoveShield()
    {
        foreach (GameObject crystal in crystals)
        {
            if (crystal == null || !crystal.activeSelf)
                continue;

            Renderer crystalRenderer = crystal.GetComponent<Renderer>();

            if (crystalRenderer == null)
                continue;

            Material[] materials = crystalRenderer.materials;

            if (materials.Length <= materialIndexToRemove)
                continue;

            Material[] updatedMaterials = new Material[materials.Length - 1];
            int newIndex = 0;

            for (int i = 0; i < materials.Length; i++)
            {
                if (i == materialIndexToRemove)
                    continue;

                updatedMaterials[newIndex++] = materials[i];
            }

            crystalRenderer.materials = updatedMaterials;
        }

        yield return new WaitForSeconds(shieldDownDuration);

        // Restore the material 
        foreach (GameObject crystal in crystals)
        {
            if (crystal == null || !crystal.activeSelf)
                continue;

            if (!originalMaterials.TryGetValue(crystal, out Material[] materials))
                continue;

            Renderer crystalRenderer = crystal.GetComponent<Renderer>();

            if (crystalRenderer != null)
                crystalRenderer.materials = materials;
        }

        isHit = false;

        Debug.Log("Shield restored");
    }

    public void ResetButton()
    {
        isHit = false;
    }
} 