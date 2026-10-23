using UnityEngine;

public class CloneJutsu : MonoBehaviour
{
    public CloneObject clone;
    public PlayerMovement playerMovement;

    public bool CurrentlyCloning = false;
    public float maxRecordingDuration = 6f; 

    private float cooldown;
    private float recordingTimer; 
    private PlayerMovement cloneMovement;

    private void Start()
    {
        cloneMovement = clone.GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        cooldown -= Time.deltaTime;

        if (CurrentlyCloning)
        {
            recordingTimer += Time.deltaTime;

            if (recordingTimer >= maxRecordingDuration)
            {
                EndCloning();
            }
        }
    } 

    public void OnClonePressed()
    {
        if (cooldown > 0) return;

        if (!CurrentlyCloning)
        {
            CurrentlyCloning = true;
            recordingTimer = 0f; 

            clone.transform.position = transform.position;
            clone.transform.rotation = transform.rotation;

            clone.gameObject.SetActive(true);

            playerMovement.enabled = false;
            cloneMovement.enabled = true;

            clone.StartRecording();
        }

        else
        {
            EndCloning();
        }

        cooldown = 0.2f;
    }

    private void EndCloning()
    {
        if (!CurrentlyCloning)
            return;

        CurrentlyCloning = false;

        clone.StopRecording();

        playerMovement.enabled = true;
        cloneMovement.enabled = false;

        clone.StartReplay();
    } 
}
