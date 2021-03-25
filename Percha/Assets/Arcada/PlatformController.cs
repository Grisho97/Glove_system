using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : MonoBehaviour
{
    public Transform endPoint;
    // Start is called before the first frame update
    void Start()
    {
        WorldController.instance.PlatformMovement += TryToAddPlatform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void TryToAddPlatform()
    {
        if (transform.position.z < WorldController.instance.zMin)
        {
            WorldController.instance.worldBuilder.CreatePlatform();
            Destroy(gameObject);
        }
        
    }

    private void OnDestroy()
    {
        WorldController.instance.PlatformMovement -= TryToAddPlatform;
    }
    
}
