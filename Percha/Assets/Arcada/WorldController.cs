using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    public float speed = 0f;
    public WorldBuilder worldBuilder;
    public float zMin = -10;
    //public delegate void AddRemovePlatform();
    //public event AddRemovePlatform PlatformMovement;
    public event Action PlatformMovement;
    
    public static WorldController instance;


    public void Awake()
    {
        if (WorldController.instance != null)
        {
            Destroy(gameObject);
            return;
        }

        WorldController.instance = this;
    }

    private void Start()
    {
        StartCoroutine(OnPlatformMovementCorutine());
    }

    private void OnDestroy()
    {
        WorldController.instance = null;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position -= Vector3.forward * speed * Time.deltaTime;
    }
    
    IEnumerator OnPlatformMovementCorutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (PlatformMovement != null)
                PlatformMovement();
        }
    }
}
