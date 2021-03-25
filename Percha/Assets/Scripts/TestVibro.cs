using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class TestVibro : MonoBehaviour
{
    [SerializeField] private string portName = "COM3";
    private SerialPort sp;
    // Start is called before the first frame update
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        sp = new SerialPort(portName, 115200);
        sp.Open();
        sp.ReadTimeout = 10000;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) ) 
        {
            sp.Write ("1");
            Debug.Log ("1");
        }
        
        if (Input.GetKeyDown(KeyCode.S) ) 
        {
            sp.Write ("0");
            Debug.Log ("0");
        }
        
        if (Input.GetKeyDown(KeyCode.A) ) 
        {
            sp.Write ("6");
            Debug.Log ("6");
        }
        
        if (Input.GetKeyDown(KeyCode.D) ) 
        {
            sp.Write ("5");
            Debug.Log ("5");
        }
        
        if (Input.GetKeyDown(KeyCode.T) ) 
        {
            sp.Write ("3");
            Debug.Log ("3");
        }
        
        if (Input.GetKeyDown(KeyCode.G) ) 
        {
            sp.Write ("4");
            Debug.Log ("4");
        }
        
        if (Input.GetKeyDown(KeyCode.F) ) 
        {
            sp.Write ("7");
            Debug.Log ("7");
        }
        
        if (Input.GetKeyDown(KeyCode.H) ) 
        {
            sp.Write ("8");
            Debug.Log ("8");
        }
    }
    
    private void OnDisable()
    {
        sp.Close();
        sp.Dispose();
    }
}
