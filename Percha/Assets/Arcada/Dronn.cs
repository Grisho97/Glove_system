using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Dronn : MonoBehaviour
{
    [SerializeField] private int triggerLeft;
    [SerializeField] private int triggerCenter;
    [SerializeField] private int triggerRight;
    
    [SerializeField] private string portName = "COM3";
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject[] lopasti;
    [SerializeField] private float propellersSpeed;
    [SerializeField] private float velocity = 7f;

    // Data from sensors
    public float thumb_flex = 0;
    public float index_flex = 0;
    public float middle_flex = 0;
    public float ring_flex = 0;
    public float pinky_flex = 0;
    
    //relative data
    public float[] relativeData = new float[5];
    private Perceptron item;
	
    // Palm rotation in Quaternions
    private float q0 = 0;
    private float q1 = 0;
    private float q2 = 0;
    private float q3 = 0;
	
    // min and max values from sensors
    private float thumb_flex_start = 3700;
    private float index_flex_start = 2570;
    private float middle_flex_start = 2500;
    private float ring_flex_start = 2510;
    private float pinky_flex_start = 3650;
	
    private float thumb_flex_end = 3100;
    private float index_flex_end = 1450;
    private float middle_flex_end = 1400;
    private float ring_flex_end = 1380;
    private float pinky_flex_end = 2600;

    private bool needColibratoin = false;
    private float rotationSpeed;
    private Rigidbody rb;

    private SerialPort sp;
    private Vector3 direction;
    private int gestID;
    private Thread dataReceivingThread;
    
    private float q3Default;
    private float q1Default;
    private float q2Default;

    // Start is called before the first frame update
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        item = new Perceptron(3);
        item.loadFromFile();
        sp = new SerialPort(portName, 115200);
        sp.Open();
        sp.ReadTimeout = 10000;
            //binaryFile = new BinaryWriter(new FileStream(fileName, FileMode.Create));
        dataReceivingThread = new Thread(new ThreadStart(data_receiving_thread));
        dataReceivingThread.Start();
        sp.Write("1");
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var propeller in lopasti)
        {
            propeller.transform.localRotation *= Quaternion.Euler(0, 0, propellersSpeed * Time.deltaTime);
        }

        #region Calibration positions of the fingers (s & d keys)
		
        // For calibration we use buttons "s" and "d" for start and final positions of fingers
        if (Input.GetKeyDown("s"))
        {
            if (needColibratoin==true)
            {
                thumb_flex_start = thumb_flex;
                index_flex_start = index_flex;
                middle_flex_start = middle_flex;
                ring_flex_start = ring_flex;
                pinky_flex_start = pinky_flex;
            }
            
            q1Default = q1;
        }
		
        if (Input.GetKeyDown("d"))
        {
            if (needColibratoin==true)
            {
                thumb_flex_end = thumb_flex;
                index_flex_end = index_flex;
                middle_flex_end = middle_flex;
                ring_flex_end = ring_flex;
                pinky_flex_end = pinky_flex;
            }
           
            WorldController.instance.speed = 2.5f;
            sp.Write("0");
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            velocity = velocity * 2;
            WorldController.instance.speed = 5;
        }

        #endregion
        
        GetDataFromPercha();
        GestRecognision();
        DronMovement();
        //ManualControl();
    }
    
    private void GetDataFromPercha()
    {
        relativeData[0] = (thumb_flex_start - thumb_flex) / (thumb_flex_start - thumb_flex_end);
        relativeData[1] = (index_flex_start - index_flex) / (index_flex_start - index_flex_end);
        relativeData[2] = (middle_flex_start - middle_flex) / (middle_flex_start - middle_flex_end);
        relativeData[3]= (ring_flex_start - ring_flex) / (ring_flex_start - ring_flex_end);
        relativeData[4] = (pinky_flex_start - pinky_flex) / (pinky_flex_start - pinky_flex_end);
    }
    
    private void GestRecognision()
    {
        // gesture recognision 
        int gestNumber = item.recognize(relativeData);

        switch (gestNumber)
        {
            case 0:
                gestID = 1;
                break;
            case 1:
                gestID = 2;
                break;
            case 2:
                gestID = 3;
                break;
//            case 3:
//                gestID = 4;
//                break;
            default:
                gestID = 0;
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "AreaRight")
        {
           sp.Write("6");
           triggerRight++;
        }
        
        if (other.tag == "AreaLeft")
        {
            sp.Write("5");
            triggerLeft++;
        }
        
        if (other.tag == "AreaCenter")
        {
            sp.Write("1");
            triggerCenter++;
        }

        if (other.tag == "Building")
        {
            GetComponent<AudioSource>().Play();
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "AreaCenter" || other.tag =="AreaLeft" || other.tag =="AreaRight")
        {
            sp.Write("0");
        }
    }

    private void DronMovement()
    {
        //x = q2, y = q3, z = -q1, w = q0
        Quaternion palm_rotation = new Quaternion(0,0,q1-q1Default,q0);
        transform.localRotation = palm_rotation;
        
//        if (q2>(q2Default+0.1f))
//        {
//            transform.position += transform.forward* Time.deltaTime * velocity;
//        }
//        if (q2<q2Default-0.1f)
//        {
//            transform.position -= transform.forward * Time.deltaTime * velocity;
//        }
        if (gestID == 2)
        {
            if (q1<(q1Default-0.08f))
            {
                float moveLeft = 1 * Time.deltaTime * velocity;
                moveLeft += player.transform.position.x;
                float limitHorizontal =
                    Mathf.Clamp(moveLeft, -5, 5);
                player.transform.position = new Vector3(limitHorizontal, player.transform.position.y, player.transform.position.z);
                //player.transform.position -= player.transform.right * Time.deltaTime * velocity;
            }
            if (q1>q1Default+0.08f)
            {
                float moveRight = -1 * Time.deltaTime * velocity;
                moveRight += player.transform.position.x;
                float limitHorizontal =
                    Mathf.Clamp(moveRight, -5, 5);
                player.transform.position = new Vector3(limitHorizontal, player.transform.position.y, player.transform.position.z);
                //player.transform.position += player.transform.right * Time.deltaTime * velocity;
            }
        }
        
        if (gestID == 3)
        {
            float moveUp = 1 * Time.deltaTime * velocity;
            moveUp += player.transform.position.y;
            float limitVertical =
                Mathf.Clamp(moveUp, 0f, 1.7f);
            player.transform.position = new Vector3(player.transform.position.x, limitVertical, player.transform.position.z);
        }
        else
        {
            float moveUp = -0.1f * Time.deltaTime * velocity;
            moveUp += player.transform.position.y;
            float limitVertical =
                Mathf.Clamp(moveUp, 0f, 1.7f);
            player.transform.position = new Vector3(player.transform.position.x, limitVertical, player.transform.position.z);
        }
       
    }
    
    private void data_receiving_thread()
    {
        System.Threading.Thread.Sleep(500);
        while (true)
        {
            // Cutting String received from Controller by symbol ','
            string[] dataString = sp.ReadLine().Split(',');
			
            try
            {
                thumb_flex = float.Parse(dataString[4]);
                index_flex = float.Parse(dataString[5]);
                middle_flex = float.Parse(dataString[6]);
                ring_flex = float.Parse(dataString[7]);
                pinky_flex = float.Parse(dataString[8]);
				
				
                q0 = float.Parse(dataString[0], CultureInfo.InvariantCulture.NumberFormat);
                q1 = float.Parse(dataString[1], CultureInfo.InvariantCulture.NumberFormat);
                q2 = float.Parse(dataString[2], CultureInfo.InvariantCulture.NumberFormat);
                q3 = float.Parse(dataString[3], CultureInfo.InvariantCulture.NumberFormat);
            }
            catch (FormatException e)
            {
                Debug.Log(e.ToString());
            }
            catch (IndexOutOfRangeException e)
            {
                Debug.Log(e.ToString());
            }
        }
    }

    //if needs
    private void ManualControl()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        //float moveVertical = Input.GetAxis("Vertical");
        if(Mathf.Abs(moveHorizontal) > Mathf.Epsilon)
        {
            moveHorizontal = moveHorizontal * Time.deltaTime * velocity;
            moveHorizontal += transform.position.x;

            float limitHorizontal =
                Mathf.Clamp(moveHorizontal, -5, 5);

            transform.position = new Vector3(limitHorizontal, transform.position.y, transform.position.z);
        }
        
        if (Input.GetKey(KeyCode.W))
        {
            float moveVertical = 0.3f * Time.deltaTime * velocity;
            moveVertical += transform.position.y;
            
            float limitVertical = Mathf.Clamp(moveVertical, 1.5f, 3);
            transform.position = new Vector3(transform.position.x, limitVertical, transform.position.z);
        }
        else
        {
            float moveVertical = -0.03f * Time.deltaTime * velocity;
            moveVertical += transform.position.y;
            
            float limitVertical = Mathf.Clamp(moveVertical, 1.5f, 3);
            transform.position = new Vector3(transform.position.x, limitVertical, transform.position.z);
        }
    }
    
    private void OnDisable()
    {
        sp.Close();
        sp.Dispose();
    }
}
