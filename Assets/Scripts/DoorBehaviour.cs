using UnityEngine;
using System.Collections.Generic;

public class DoorBehaviour : MonoBehaviour
{
    public List<GameObject> buttons;
    public Animator animator;

    private bool doorOpened = false;

    private void Update()
    {
        if (doorOpened)
            return;

        bool allHit = true;

        foreach (GameObject button in buttons)
        {
            HitObject hitObject = button.GetComponent<HitObject>();

            if (!hitObject.isHit)
            {
                allHit = false;
                break;
            }
        }

        if (allHit)
        {
            doorOpened = true;
            animator.SetTrigger("OpenDoor");
        }
    }
} 