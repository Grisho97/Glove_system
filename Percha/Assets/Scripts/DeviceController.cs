using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
//        if (Input.GetKeyDown("1"))
//        {
//            Broker.CallIsTouching(1);
//            Debug.Log("IsTouching");
//        }
//        else if (Input.GetKeyDown("0"))
//        {
//            Broker.CallIsTouching(0);
//            Debug.Log("NotTouching");
//        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Broker.CallIsTouching(1);
        Debug.Log("IsTouching");
    }
    
    private void OnTriggerExit(Collider other)
    {
        Broker.CallIsTouching(0);
        Debug.Log("NotTouching");
    }
}
