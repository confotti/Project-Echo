
using System.Collections.Generic;
using UnityEngine;

public class CrystalShieldReset : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<GameObject> crystals = new List<GameObject>();
    [SerializeField] private BossHit bossButton;

    [Header("Shield")]
    [SerializeField] private int shieldMaterialIndex = 1;
    [SerializeField] private Material shieldMaterial;

    [SerializeField] private List<MoveableEnemy> enemies = new List<MoveableEnemy>();

    [Header("Completion")]
    [SerializeField] private int crystalsRequired = 3;
    [SerializeField] private ParticleSystem shurikenEffect;

    private readonly HashSet<GameObject> destroyedCrystals = new HashSet<GameObject>();
    private bool puzzleCompleted;

    private void Update()
    {
        if (puzzleCompleted)
            return;

        foreach (GameObject crystal in crystals)
        {
            if (crystal == null || destroyedCrystals.Contains(crystal))
                continue;

            CrystalHit crystalHit = crystal.GetComponent<CrystalHit>();

            if (crystalHit != null && crystalHit.IsDestroyed)
            {
                destroyedCrystals.Add(crystal);
                CheckCrystalProgress();
                break;
            }
        }
    }

    private void CheckCrystalProgress()
    {
        if (destroyedCrystals.Count >= crystalsRequired)
        {
            CompletePuzzle();
        }
        else
        {
            RestoreShield();
        }
    }

    public bool AreCrystalsRemaining
    {
        get
        {
            foreach (GameObject crystal in crystals)
            {
                if (crystal != null && crystal.activeInHierarchy)
                    return true;
            }

            return false;
        }
    } 

    private void RestoreShield()
    {
        foreach (GameObject crystal in crystals)
        {
            if (crystal == null || destroyedCrystals.Contains(crystal))
                continue;

            Renderer crystalRenderer = crystal.GetComponent<Renderer>();

            if (crystalRenderer == null)
                continue;

            Material[] materials = crystalRenderer.materials;

            if (materials.Length <= shieldMaterialIndex)
            {
                Material[] updatedMaterials = new Material[materials.Length + 1];

                for (int i = 0; i < materials.Length; i++)
                    updatedMaterials[i] = materials[i];

                updatedMaterials[shieldMaterialIndex] = shieldMaterial;
                crystalRenderer.materials = updatedMaterials;
            }
        }

        if (bossButton != null)
            bossButton.ResetButton();

        Debug.Log("Shield restored - press the button again");
    }

    private void RemoveEnemyShields()
    {
        foreach (MoveableEnemy moveableEnemy in enemies)
        {
            if (moveableEnemy == null)
                continue;

            moveableEnemy.RemoveShieldMaterial();
        }
    }

    private void CompletePuzzle()
    {
        puzzleCompleted = true;
        RemoveEnemyShields();

        foreach (GameObject crystal in crystals)
        {
            if (crystal != null)
                crystal.SetActive(false);
        }

        if (shurikenEffect != null)
        {
            shurikenEffect.gameObject.SetActive(true);
            shurikenEffect.Play();
        }
    } 
}