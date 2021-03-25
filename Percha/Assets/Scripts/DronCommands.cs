using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using File = UnityEngine.Windows.File;
using Random = System.Random;

public class DronCommands: MonoBehaviour
{ 
	public GameObject camera;
	public GameObject[] lopasti;
	public float rotationSpeed;
	public float velocity;
    public float propellersSpeed;
    public GameObject srene;
	public Text gesture;
	public static int gestID;

	// Thumb (bolshoi)
	public Transform thumb_start;
	public Transform thumb_mid;
	public Transform thumb_final;
	// Index (ukazatelniy)
	public Transform index_start;
	public Transform index_mid;
	public Transform index_final;
	public Transform index_ignore;
	public GameObject index_line;
	// Middle (sredniy)
	public Transform middle_start;
	public Transform middle_mid;
	public Transform middle_final;
	// Ring (bezimyanniy)
	public Transform ring_start;
	public Transform ring_mid;
	public Transform ring_final;
	// Pinky (mizinets)
	public Transform pinky_start;
	public Transform pinky_mid;
	public Transform pinky_final;
	
	// Starting positions of fingers (for calibration)
	public float start_position_thumb;
	public float start_position_index;
	public float start_position_middle;
	public float start_position_ring;
	public float start_position_pinky;
		
	// Data from sensors
	public float thumb_flex = 0;
	public float index_flex = 0;
	public float middle_flex = 0;
	public float ring_flex = 0;
	public float pinky_flex = 0;
	
	//relative data
	
	public float i_thumb = 0;
	public float i_index= 0;
	public float i_middle = 0;
	public float i_ring = 0;
	public float i_pinky = 0;
	public float[] relativeData = new float[5];
	private Perceptron item;
	
	// Palm rotation in Quaternions
	private float q0 = 0;
	private float q1 = 0;
	private float q2 = 0;
	private float q3 = 0;
	
	// min and max values from sensors
	private float thumb_flex_start = 388;
	private float index_flex_start = 360;
	private float middle_flex_start = 412;
	private float ring_flex_start = 373;
	private float pinky_flex_start = 393;
	
	private float thumb_flex_end = 560;
	private float index_flex_end = 620;
	private float middle_flex_end = 655;
	private float ring_flex_end = 615;
	private float pinky_flex_end = 592;
	
	// For Debugging
	public float thumb_angle = 0;
	public float index_angle = 0;
	public float middle_angle = 0;
	public float ring_angle = 0;
	public float pinky_angle = 0;

	private SerialPort sp;
	
	string fileName = @"C:\Users\Gregory\Desktop\gestures\gesture.bin"; 
	private BinaryWriter binaryFile;
	private bool startWriting = false;
	private float q3Default;
	private float q1Default;
	private float q2Default;
	private bool ifq3active;
	private Thread dataReceivingThread;

	// Use this for initialization
	void Start ()
	{
		item = new Perceptron(5);
		item.loadFromFile();
		srene.SetActive(false);
		sp = new SerialPort("COM7", 115200);
		sp.Open();
		sp.ReadTimeout = 10000;
		
			binaryFile = new BinaryWriter(new FileStream(fileName, FileMode.Create));
		
		dataReceivingThread = new Thread(new ThreadStart(data_receiving_thread));
		dataReceivingThread.Start();
		
		// Starting positions to start model with calibration
		start_position_thumb = thumb_start.localRotation.eulerAngles.z;
		start_position_index = index_start.localRotation.eulerAngles.z;
		start_position_middle = middle_start.localRotation.eulerAngles.z;
		start_position_ring = ring_start.localRotation.eulerAngles.z;
		start_position_pinky = pinky_start.localRotation.eulerAngles.z;
		

	}
	
	// Update is called once per frame
	void Update ()
	{
        //just for beauty
		foreach (var propeller in lopasti)
		{
			propeller.transform.localRotation *= Quaternion.Euler(0,0,propellersSpeed*Time.deltaTime); 
		}
		
		// For calibration we use buttons "s" and "f" for start and final positions of fingers
		if (Input.GetKey("s"))
		{
			thumb_flex_start = thumb_flex;
			index_flex_start = index_flex;
			middle_flex_start = middle_flex;
			ring_flex_start = ring_flex;
			pinky_flex_start = pinky_flex;
		}
		
		if (Input.GetKey("d"))
		{
			thumb_flex_end = thumb_flex;
			index_flex_end = index_flex;
			middle_flex_end = middle_flex;
			ring_flex_end = ring_flex;
			pinky_flex_end = pinky_flex;
			srene.SetActive(false);
		}

		GetDataFromPercha();

		GestRecognision();
		
	}

	private void GetDataFromPercha()
	{
		// Flexing fingers
		i_thumb = (thumb_flex_start - thumb_flex) / (thumb_flex_start - thumb_flex_end);
		i_index = (index_flex_start - index_flex) / (index_flex_start - index_flex_end);
		i_middle = (middle_flex_start - middle_flex) / (middle_flex_start - middle_flex_end);
		i_ring = (ring_flex_start - ring_flex) / (ring_flex_start - ring_flex_end);
		i_pinky = (pinky_flex_start - pinky_flex) / (pinky_flex_start - pinky_flex_end);
		
		relativeData[0] = i_thumb;
		relativeData[1] = i_index;
		relativeData[2] = i_middle;
		relativeData[3] = i_ring;
		relativeData[4] = i_pinky;

		
		// Visualisation of hand rotation
		Quaternion palm_rotation = new Quaternion(q2,0,-q1,q0);
		transform.localRotation = palm_rotation;
	}

	private void GestRecognision()
	{
		// gesture recognision 
		int gestNumber = item.recognize(relativeData);

		switch (gestNumber)
		{
			case 0:
				srene.SetActive(true);
				gesture.text = "OK!";
				gestID = 1;
				break;
			case 1:
				srene.SetActive(true);
				gesture.text = "Index";
				gestID = 2;
				break;
			case 2:
				srene.SetActive(true);
				gesture.text = "Thumb";
				gestID = 3;
				break;
			case 3:
				srene.SetActive(true);
				gesture.text = "Hand";
				gestID = 4;
				CameraPosition();
				ifq3active = false;
				break;
			default:
				srene.SetActive(false);
				index_line.SetActive(false);
				gestID = 0;
				ifq3active = true;
				break;
		}
	}

	//method for drone control
	private void CameraPosition()
	{
		if (ifq3active == true)
		{
			//values for keeping current position when method activates
			q3Default = q3; 
			q1Default = q1;
			q2Default = q2;
			ifq3active = false;
		}
		
		if (q2>(q2Default+0.1f))
		{
			camera.transform.position += camera.transform.forward* Time.deltaTime * velocity;
		}
		if (q2<q2Default-0.1f)
		{
			camera.transform.position -= camera.transform.forward * Time.deltaTime * velocity;
		}
		if (q1<(q1Default-0.1f))
		{
			camera.transform.position -= camera.transform.right * Time.deltaTime * velocity;
		}
		if (q1>q1Default+0.1f)
		{
			camera.transform.position += camera.transform.right * Time.deltaTime * velocity;
		}
		if (q3<(q3Default-0.1f))
		{
			camera.transform.rotation *= new Quaternion(0, rotationSpeed*Time.deltaTime,0, 1);
		}
		if (q3>q3Default+0.1f)
		{
			camera.transform.rotation *= new Quaternion(0, -rotationSpeed*Time.deltaTime,0, 1);
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

	
	private void OnApplicationQuit()
	{
		dataReceivingThread.Abort();
		sp.Close();
		binaryFile.Close();
	}
	
}

