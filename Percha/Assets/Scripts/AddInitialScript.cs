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


public class AddInitialScript : MonoBehaviour
{
	#region Defining varibles
	
	// Defining all public variables
	public GameObject cube;
	public float velocity;
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
	private float start_position_thumb;
	private float start_position_index;
	private float start_position_middle;
	private float start_position_ring;
	private float start_position_pinky;
		
	// Data from sensors
	[SerializeField] private float thumb_flex = 0;
	[SerializeField] private float index_flex = 0;
	[SerializeField] private float middle_flex = 0;
	[SerializeField] private float ring_flex = 0;
	[SerializeField] private float pinky_flex = 0;
	
	//relative data
	 private float i_thumb = 0;
	 private float i_index = 0;
	 private float i_middle = 0;
	 private float i_ring = 0;
	 private float i_pinky = 0;
	
	[SerializeField] private float[] relativeData = new float[5];
	private float[ , ] filteredRelativeData = new float[5,4];
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
	private Thread dataReceivingThread;
	
	string fileName = @"C:\Users\Gregory\Desktop\gestures\gesture.bin";
	private string directory = @"C:\Users\Gregory\Desktop\Percha\experiment1.txt";
	private BinaryWriter binaryFile;
	private bool startWriting = false;
	private float q3Default;
	private float q1Default;
	private float q2Default;
	private bool ifq3active;
	

	#endregion
	
	// Use this for initialization
	void Start ()
	{
		
		QualitySettings.vSyncCount = 0;
		item = new Perceptron(9);
		item.loadFromFile();
		srene.SetActive(false);
		sp = new SerialPort("COM7", 115200);
		sp.Open();
		sp.ReadTimeout = 10000;
		
			binaryFile = new BinaryWriter(new FileStream(fileName, FileMode.Create));
		
		dataReceivingThread = new Thread(new ThreadStart(data_receiving_thread));
		dataReceivingThread.Start();

		Broker.IsTouching += DeviceReaction;
		
		// Starting positions to start model with calibration
		start_position_thumb = thumb_start.localRotation.eulerAngles.z;
		start_position_index = index_start.localRotation.eulerAngles.z;
		start_position_middle = middle_start.localRotation.eulerAngles.z;
		start_position_ring = ring_start.localRotation.eulerAngles.z;
		start_position_pinky = pinky_start.localRotation.eulerAngles.z;

	}
	
	void Update () {

		#region Calibration positions of the fingers (s & d keys)
		
		// For calibration we use buttons "s" and "d" for start and final positions of fingers
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

		#endregion

		#region Bending of the fingers
		
		//Record relative data 
		relativeData[0] = (thumb_flex_start - thumb_flex) / (thumb_flex_start - thumb_flex_end);
		relativeData[1] = (index_flex_start - index_flex) / (index_flex_start - index_flex_end);
		relativeData[2] = (middle_flex_start - middle_flex) / (middle_flex_start - middle_flex_end);
		relativeData[3]= (ring_flex_start - ring_flex) / (ring_flex_start - ring_flex_end);
		relativeData[4] = (pinky_flex_start - pinky_flex) / (pinky_flex_start - pinky_flex_end);

		// Bending fingers
		//thumb_angle = -((thumb_flex_start - thumb_flex) / (thumb_flex_start - thumb_flex_end) * 30);
		thumb_angle = - Filter(0, relativeData[0]) * 30;
		if (thumb_angle < -30) thumb_angle = -30;
		if (thumb_angle > 0) thumb_angle = 0;
		thumb_start.localRotation = Quaternion.Euler(thumb_start.localRotation.eulerAngles.x, thumb_start.localRotation.eulerAngles.y, start_position_thumb + thumb_angle);
		thumb_mid.localRotation = Quaternion.Euler(0, 0,thumb_angle);
		thumb_final.localRotation = Quaternion.Euler(0, 0,thumb_angle);
		
		//index_angle =  - ((index_flex_start - index_flex) / (index_flex_start - index_flex_end) * 90);
		index_angle = - Filter(1, relativeData[1]) * 90;
		if (index_angle < -90) index_angle = -90;
		if (index_angle > 0) index_angle = 0;
		index_start.localRotation = Quaternion.Euler(index_start.localRotation.eulerAngles.x, index_start.localRotation.eulerAngles.y, start_position_index + index_angle);
		index_mid.localRotation = Quaternion.Euler(0, 0,index_angle);
		index_final.localRotation = Quaternion.Euler(0, 0,index_angle);
		
		//middle_angle =  - ((middle_flex_start - middle_flex) / (middle_flex_start - middle_flex_end) * 90);
		middle_angle = - Filter(2, relativeData[2]) * 90;
		if (middle_angle < -90) middle_angle = -90;
		if (middle_angle > 0) middle_angle = 0;
		middle_start.localRotation = Quaternion.Euler(middle_start.localRotation.eulerAngles.x, middle_start.localRotation.eulerAngles.y, start_position_middle + middle_angle);
		middle_mid.localRotation = Quaternion.Euler(0, 0,middle_angle);
		middle_final.localRotation = Quaternion.Euler(0, 0,middle_angle);
		
		//ring_angle =  - ((ring_flex_start - ring_flex) / (ring_flex_start - ring_flex_end) * 90);
		ring_angle = - Filter(3, relativeData[3]) * 90;
		if (ring_angle < -90) ring_angle = -90;
		if (ring_angle > 0) ring_angle = 0;
		ring_start.localRotation = Quaternion.Euler(ring_start.localRotation.eulerAngles.x, ring_start.localRotation.eulerAngles.y, start_position_ring + ring_angle);
		ring_mid.localRotation = Quaternion.Euler(0, 0,ring_angle);
		ring_final.localRotation = Quaternion.Euler(0, 0,ring_angle);
		
		//pinky_angle =  - ((pinky_flex_start - pinky_flex) / (pinky_flex_start - pinky_flex_end) * 90);
		pinky_angle = - Filter(4, relativeData[4]) * 90;
		if (pinky_angle < -90) pinky_angle = -90;
		if (pinky_angle > 0) pinky_angle = 0;
		pinky_start.localRotation = Quaternion.Euler(pinky_start.localRotation.eulerAngles.x, pinky_start.localRotation.eulerAngles.y, start_position_pinky + pinky_angle);
		pinky_mid.localRotation = Quaternion.Euler(0, 0,pinky_angle);
		pinky_final.localRotation = Quaternion.Euler(0, 0,pinky_angle);
		
		#endregion

		#region Data for training gestures (w key)

		//Writing data for training
		if (Input.GetKeyDown(KeyCode.W))
		{
			startWriting = true;
		}
		
		if (startWriting == true)
		{
			WriteLearningPerchaFile();
		}
		
		#endregion

		// Visualisation of hand rotation
		Quaternion palm_rotation = new Quaternion(-q2,-q3,q1,q0);
		transform.rotation = palm_rotation;

		#region Recognizing gestures
		
		//Recognizing gestures
		int gestNumber = item.recognize(relativeData);
		Debug.Log(gestNumber);

		RecognizingGestures(gestNumber);
		//Broker.CallGestRec(gestNumber);

		#endregion

		#region ExperimentData

		if (Input.GetKeyDown(KeyCode.P))
		{
			WriteExcel(relativeData);
		}

		#endregion
	}

	private void RecognizingGestures(int gestNumber)
	{
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
				//IndexLineSetActive();
				break;
			case 2:
				srene.SetActive(true);
				gesture.text = "Thumb";
				gestID = 3;
				CameraPosition();
				ifq3active = false;
				break;
			case 3:
				srene.SetActive(true);
				gesture.text = "Hand";
				gestID = 4;
				break;
			case 4:
				srene.SetActive(true);
				gesture.text = "Koza";
				gestID = 5;
				break;
			case 5:
				srene.SetActive(true);
				gesture.text = "Phone";
				gestID = 6;
				break;
			case 6:
				srene.SetActive(true);
				gesture.text = "Gun";
				gestID = 7;
				break;
			case 7:
				srene.SetActive(true);
				gesture.text = "Fist";
				gestID = 8;
				break;
			case 8:
				srene.SetActive(true);
				gesture.text = "Fuck you";
				gestID = 9;
				break;
			default:
				srene.SetActive(false);
				index_line.SetActive(false);
				gesture.text = "-";
				gestID = 0;
				ifq3active = true;
				break;
		}
	}

	private float Filter(int fingerNumber, float new_i)
	{
		for (int i = 3; i > 0; i--)
		{
			if (filteredRelativeData[fingerNumber, i] == 0)
			{
				filteredRelativeData[fingerNumber, i] = new_i;
			}
			else
			{
				filteredRelativeData[fingerNumber, i] = filteredRelativeData[fingerNumber, i - 1];
			}
		}
		filteredRelativeData[fingerNumber, 0] = new_i;
		float avarage = 0.3f * filteredRelativeData[fingerNumber, 0] + 0.3f * filteredRelativeData[fingerNumber, 1] +
		                0.2f * filteredRelativeData[fingerNumber, 2] + 0.2f * filteredRelativeData[fingerNumber, 3];
		return avarage;
	}

	private void IndexLineSetActive()
	{
		index_line.SetActive(true);
		float f = RayCastManager.lineDistance.magnitude;
		index_line.transform.localPosition = new Vector3(f/2,0, 0);
		index_line.transform.localScale = new Vector3(0.005f,f/2,0.005f);
	}

	private void CameraPosition()
	{
		if (ifq3active == true)
		{
			q3Default = q3; 
			q1Default = q1;
			q2Default = q2;
			ifq3active = false;
		}
		
		if (q3<(q3Default-0.1f))
		{
			cube.transform.position += new Vector3(0,0,1) * Time.deltaTime * velocity;
		}
		if (q3>q3Default+0.1f)
		{
			cube.transform.position -= new Vector3(0,0,1) * Time.deltaTime * velocity;
		}
		if (q1<(q1Default-0.1f))
		{
			cube.transform.position -= new Vector3(1,0,0) * Time.deltaTime * velocity;
		}
		if (q1>q1Default+0.1f)
		{
			cube.transform.position += new Vector3(1,0,0) * Time.deltaTime * velocity;
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
				thumb_flex = float.Parse(dataString[8]);
				index_flex = float.Parse(dataString[7]);
				middle_flex = float.Parse(dataString[6]);
				ring_flex = float.Parse(dataString[5]);
				pinky_flex = float.Parse(dataString[4]);
				
				
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

	private void DeviceReaction(byte a)
	{
		sp.Write(a.ToString());
		Debug.Log(a.ToString());
	}

	private void WriteExcel(float[] relativeDataVar)
	{
		using (StreamWriter sw = new StreamWriter(directory, true, System.Text.Encoding.Default))
		{
			for (int i = 0; i < 5; i++)
			{
				float round = (float)System.Math.Round(relativeDataVar[i], 3);
				sw.Write(round + " ");
			}
			sw.WriteLine(gesture.text);
		}
	}

	private void WriteLearningPerchaFile()
	{
		for (int i = 0; i < 5; i++)
		{
			binaryFile.Write(relativeData[i]);
		}
	}

	private void OnApplicationQuit()
	{
		dataReceivingThread.Abort();
		sp.Close();
		binaryFile.Close();
	}
	
}


