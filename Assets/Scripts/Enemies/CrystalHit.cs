using UnityEngine;

public class CrystalHit : MonoBehaviour
{
    [SerializeField] private int shieldMaterialIndex = 1;

    public bool IsDestroyed { get; private set; }

    private Renderer crystalRenderer;

    private void Awake()
    {
        crystalRenderer = GetComponent<Renderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Sword") || IsDestroyed)
            return;

        // The shield material must be removed before this crystal can be hit.
        if (crystalRenderer == null ||
            crystalRenderer.materials.Length > shieldMaterialIndex)
            return;

        IsDestroyed = true;
        gameObject.SetActive(false);

        Debug.Log("Crystal destroyed");
    }
}