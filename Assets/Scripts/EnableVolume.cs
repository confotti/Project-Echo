using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EnableVolume : MonoBehaviour
{
    public Volume volume;

    public GameObject object1;
    public GameObject object2;

    private SplitToning splitToning;

    private void Start()
    {
        volume.profile.TryGet(out splitToning);

        if (splitToning != null)
        {
            splitToning.active = false;
        }
    }

    private void Update()
    {
        bool object1Dead = object1 == null || !object1.activeInHierarchy;
        bool object2Dead = object2 == null || !object2.activeInHierarchy;

        if (object1Dead && object2Dead)
        {
            if (splitToning != null)
            {
                splitToning.active = true;
            }

            enabled = false;
        }
    }
} 