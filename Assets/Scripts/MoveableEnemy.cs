using UnityEngine;
using System.Collections;

public class MoveableEnemy : MonoBehaviour
{
    [Header("Targets")]
    public Transform player;
    public Transform clone;
    public Transform cloneTarget;

    [Header("Movement")]
    public float detectionRange = 10f;
    public float moveSpeed = 3f;
    public bool canMove; 

    [Header("Combat")]
    public float health;
    public float damagePerHit;
    private float currentHealth;

    [Header("Shield")]
    public CrystalShieldReset crystalShieldReset;

    public Material hitMaterial;
    public GameObject enemy;

    private Material originalMaterial;
    public ParticleSystem deathvfx;

    private void Start()
    {
        if (enemy != null)
            originalMaterial = enemy.GetComponent<MeshRenderer>().material;

        currentHealth = health;
    }

    private void Update()
    {
        if (player == null)
            return;

        CloneObject cloneObject = clone != null ? clone.GetComponent<CloneObject>(): null;

        //stannna vid record
        if (cloneObject != null && cloneObject.IsRecording)
            return;

        //jaga klonen
        Transform target = player;

        if (cloneObject != null && cloneObject.IsReplaying && clone != null)
        {
            target = clone;
        }

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // detect range
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget <= detectionRange && direction.sqrMagnitude > 0.001f && canMove)
        {
            transform.position += direction.normalized * moveSpeed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Sword"))
            return;

        // odödlig pga krystall vid liv
        if (crystalShieldReset != null && crystalShieldReset.AreCrystalsRemaining && canMove)
        {
            return;
        }

        if (enemy != null)
        {
            MeshRenderer meshRenderer = enemy.GetComponent<MeshRenderer>();

            if (meshRenderer != null)
            {
                meshRenderer.material = hitMaterial;
                StartCoroutine(HitDuration());
            }
        }
        TakeDamage();
    }

    public void RemoveShieldMaterial()
    {
        MeshRenderer meshRenderer = enemy.GetComponent<MeshRenderer>();
        Material[] materials = meshRenderer.materials;

        if (materials.Length > 1)
        {
            Material[] newMaterials = new Material[materials.Length - 1];

            newMaterials[0] = materials[0];

            for (int i = 2; i < materials.Length; i++)
            {
                newMaterials[i - 1] = materials[i];
            }
            meshRenderer.materials = newMaterials;
        }
    }

    IEnumerator HitDuration()
    {
        yield return new WaitForSeconds(0.1f);

        if (enemy != null)
        {
            MeshRenderer meshRenderer = enemy.GetComponent<MeshRenderer>();

            if (meshRenderer != null)
                meshRenderer.material = originalMaterial;
        }
    }

    private void TakeDamage()
    {
        currentHealth -= damagePerHit;

        if (currentHealth <= 0)
        {
            if (deathvfx != null)
                deathvfx.Play();

            if (enemy != null)
                Destroy(enemy);
        }
    }
} 