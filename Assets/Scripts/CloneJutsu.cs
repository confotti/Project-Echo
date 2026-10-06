using UnityEngine;

public class CloneJutsu : MonoBehaviour
{
    public CloneObject clone;
    public PlayerMovement playerMovement;

    public bool CurrentlyCloning = false;

    private float cooldown;
    private PlayerMovement cloneMovement;

    private void Start()
    {
        cloneMovement = clone.GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        cooldown -= Time.deltaTime;
    }

    public void OnClonePressed()
    {
        if (cooldown > 0) return;

        if (!CurrentlyCloning)
        {
            CurrentlyCloning = true;

            clone.transform.position = transform.position;
            clone.transform.rotation = transform.rotation;

            clone.gameObject.SetActive(true);

            playerMovement.enabled = false;
            cloneMovement.enabled = true;

            clone.StartRecording();
        }

        else
        {
            CurrentlyCloning = false;

            clone.StopRecording();

            playerMovement.enabled = true;
            cloneMovement.enabled = false;

            clone.StartReplay();
        }

        cooldown = 0.2f;
    }
}
