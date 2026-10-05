using UnityEngine;

public class CloneJutsu : MonoBehaviour
{
    public CloneObject clone;
    public PlayerMovement playerMovement;

    public bool CurrentlyCloning = false;

    private float cooldown;

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

            clone.StartRecording();
        }

        else
        {
            CurrentlyCloning = false;

            clone.StopRecording();

            playerMovement.enabled = true;

            clone.StartReplay();
        }

        cooldown = 0.2f;
    }
}
