using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class InitialScript : MonoBehaviour
{

	// Defining all public variables
	
	// Thumb (bolshoi)
	public Transform thumb_start;
	public Transform thumb_mid;
	public Transform thumb_final;
	// Index (ukazatelniy)
	public Transform index_start;
	public Transform index_mid;
	public Transform index_final;
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
	
	// Use this for initialization
	void Start ()
	{
		sp = new SerialPort("COM7", 115200);
		sp.Open();
		sp.ReadTimeout = 10000;
		
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
	void Update () {		
		
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
		}

		// Flexing fingers
		thumb_angle = -((thumb_flex_start - thumb_flex) / (thumb_flex_start - thumb_flex_end) * 30);
		if (thumb_angle < -90) thumb_angle = -90;
		if (thumb_angle > 0) thumb_angle = 0;
		thumb_start.localRotation = Quaternion.Euler(thumb_start.localRotation.eulerAngles.x, thumb_start.localRotation.eulerAngles.y, start_position_thumb + thumb_angle);
		thumb_mid.localRotation = Quaternion.Euler(0, 0,thumb_angle);
		thumb_final.localRotation = Quaternion.Euler(0, 0,thumb_angle);
		
		// index_angle =  - ((index_flex_start - index_flex) / (index_flex_start - index_flex_end) * 90);
		// if (index_angle < -90) index_angle = -90;
		// if (index_angle > 0) index_angle = 0;
		// index_start.localRotation = Quaternion.Euler(index_start.localRotation.eulerAngles.x, index_start.localRotation.eulerAngles.y, start_position_index + index_angle);
		// index_mid.localRotation = Quaternion.Euler(0, 0,index_angle);
		// index_final.localRotation = Quaternion.Euler(0, 0,index_angle);
		
		index_angle =  - ((index_flex_start - index_flex) / (index_flex_start - index_flex_end) * 90);
		if (index_angle < -90) index_angle = -90;
		if (index_angle > 0) index_angle = 0;
		index_start.localRotation = Quaternion.Euler(index_start.localRotation.eulerAngles.x, index_start.localRotation.eulerAngles.y, start_position_index + index_angle);
		index_mid.localRotation = Quaternion.Euler(0, 0,index_angle);
		index_final.localRotation = Quaternion.Euler(0, 0,index_angle);
		
		middle_angle =  - ((middle_flex_start - middle_flex) / (middle_flex_start - middle_flex_end) * 90);
		if (middle_angle < -90) middle_angle = -90;
		if (middle_angle > 0) middle_angle = 0;
		middle_start.localRotation = Quaternion.Euler(middle_start.localRotation.eulerAngles.x, middle_start.localRotation.eulerAngles.y, start_position_middle + middle_angle);
		middle_mid.localRotation = Quaternion.Euler(0, 0,middle_angle);
		middle_final.localRotation = Quaternion.Euler(0, 0,middle_angle);
		
		ring_angle =  - ((ring_flex_start - ring_flex) / (ring_flex_start - ring_flex_end) * 90);
		if (ring_angle < -90) ring_angle = -90;
		if (ring_angle > 0) ring_angle = 0;
		ring_start.localRotation = Quaternion.Euler(ring_start.localRotation.eulerAngles.x, ring_start.localRotation.eulerAngles.y, start_position_ring + ring_angle);
		ring_mid.localRotation = Quaternion.Euler(0, 0,ring_angle);
		ring_final.localRotation = Quaternion.Euler(0, 0,ring_angle);
		
		pinky_angle =  - ((pinky_flex_start - pinky_flex) / (pinky_flex_start - pinky_flex_end) * 90);
		if (pinky_angle < -90) pinky_angle = -90;
		if (pinky_angle > 0) pinky_angle = 0;
		pinky_start.localRotation = Quaternion.Euler(pinky_start.localRotation.eulerAngles.x, pinky_start.localRotation.eulerAngles.y, start_position_pinky + pinky_angle);
		pinky_mid.localRotation = Quaternion.Euler(0, 0,pinky_angle);
		pinky_final.localRotation = Quaternion.Euler(0, 0,pinky_angle);

		// Visualisation of hand rotation
		Quaternion palm_rotation = new Quaternion(q1,q0,q2,q3);
		transform.rotation = palm_rotation;
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
	}
}