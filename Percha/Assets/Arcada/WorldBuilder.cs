using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBuilder : MonoBehaviour
{
    public GameObject[] freePlatform;
    public GameObject[] obstaclePlatform;
    public Transform platformContainer;
    private bool isObstacle;

    private Transform lastPlatform = null;
    private int lastIndex = -1;
    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CreatePlatform();
        }
    }

    public void CreatePlatform()
    {
        if (isObstacle)
            CreateFreePlatform();
        else
            CreateObstaclePlatform();
    }

    private void CreateFreePlatform()
    {
        Vector3 pos = (lastPlatform == null) ? platformContainer.position :
            lastPlatform.GetComponent<PlatformController>().endPoint.position;

        int index = Random.Range(0, freePlatform.Length);
        GameObject res = Instantiate(freePlatform[index], pos, Quaternion.identity, platformContainer);
        lastPlatform = res.transform;
        isObstacle = false;
    }
    
    private void CreateObstaclePlatform()
    {
        Vector3 pos = (lastPlatform == null) ? platformContainer.position :
            lastPlatform.GetComponent<PlatformController>().endPoint.position;

        int index = 0;
        while (true)
        {
            index = Random.Range(0, obstaclePlatform.Length);
            if (index != lastIndex)
            {
                break;
            }
        }
        lastIndex = index;
        GameObject res = Instantiate(obstaclePlatform[index], pos, Quaternion.identity, platformContainer);
        lastPlatform = res.transform;
        isObstacle = true;
    }

    public void Init()
    {
        CreateFreePlatform();
        CreateFreePlatform();
        for (int i = 0; i < 5; i++)
        {
            CreatePlatform();
        }
    }
}
