using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;


public class EventManager : MonoBehaviour
{
    //Эта строчка должна быть в управляющем скрипте
    [SerializeField] public OutlineObjects outlineObjects;
    
    
    [SerializeField] private GameObject[] ObjectsToHighlight;
    private GameObject[] a;
    private GameObject[] b;
    private GameObject[] c;

    private int previousGestID;
    private int gestID;
    private int numberOfLine = 100;
    [SerializeField]private int numberOfElement = 100;
    private bool lineSelected = false;
    private bool elementSelected = false;
    [SerializeField] private GameObject[] marker;

    private void Start()
    {
        //Эта строчка должна быть в управляющем скрипте
        outlineObjects = FindObjectOfType<OutlineObjects>();
        Broker.GestRec += RecognizingGestures;
        
        a = new GameObject[3];
        b = new GameObject[3];
        c = new GameObject[3];

        for (int i = 0; i < 3; i++)
        {
            a[i] = ObjectsToHighlight[i];
            b[i] = ObjectsToHighlight[i+3];
            c[i] = ObjectsToHighlight[i+6];
        }
    }


    // Update is called once per frame
    void Update()
    {
       
    }
    
    private void RecognizingGestures(int gestNumber)
    {
        switch (gestNumber)
        {
            case 0:
                gestID = 1;//ok
                if (lineSelected == false && numberOfLine != 100)
                {
                    lineSelected = true;
                    marker[numberOfLine].SetActive(true);
                }
                else if (elementSelected == false && numberOfElement != 100 )
                {
                    //elementSelected = true;
                    if (numberOfLine == 1 && numberOfElement == 0)
                    {
                        SceneManager.LoadScene(1); 
                    }
                }
                break;
            case 1:
                gestID = 2;//index
                if (previousGestID != gestID)
                {
                    if (lineSelected == false)
                    {
                        numberOfLine = 0;
                        SetLine(0);
                        //lineSelected = true;
                    }
                    else if (elementSelected == false)
                    {
                        numberOfElement = 0;
                        SetElement(numberOfLine, numberOfElement);
                       // elementSelected = true;
                    }
                }

                break;
            case 2:
                gestID = 3;//2
                if (previousGestID != gestID)
                {
                    if (lineSelected == false)
                    {
                        numberOfLine = 1;
                        SetLine(1);
                        //lineSelected = true;
                    }
                    else if (elementSelected == false)
                    {
                        numberOfElement = 1;
                        SetElement(numberOfLine, numberOfElement);
                       // elementSelected = true;
                    }
                }

                break;
            case 3:
                gestID = 4;//3
                if (previousGestID != gestID)
                {
                    if (lineSelected == false)
                    {
                        numberOfLine = 2;
                        SetLine(2);
                       // lineSelected = true;
                    }
                    else if (elementSelected == false)
                    {
                        numberOfElement = 2;
                        SetElement(numberOfLine, numberOfElement);
                        //elementSelected = true;
                    }
                }
                break;
            case 4:
                gestID = 5;//fist
                if (lineSelected == true)
                {
                    TurnOff();
                    lineSelected = false;
                    elementSelected = false;
                    numberOfElement = 100;
                    numberOfLine = 100;
                    marker[numberOfLine].SetActive(false);
                }
                break;
            default:
                gestID = 0;
                break;
        }

        previousGestID = gestID;
    }

    private void SetLine(int abc)
    {
        switch (abc)
        {
            case 0:
                outlineObjects.ChangeHighlight(a, true);
                break;
            case 1:
                outlineObjects.ChangeHighlight(b, true);
                break;
            case 2:
                outlineObjects.ChangeHighlight(c, true);
                break;
        }
    }
    
    private void SetElement(int abc, int num)
    {
        switch (abc)
        {
            case 0:
                GameObject[] i1 = new GameObject[]{a[num]};
                outlineObjects.ChangeHighlight(i1, true);
                break;
            case 1:
                GameObject[] i2 = new GameObject[]{b[num]};
                outlineObjects.ChangeHighlight(i2, true);
                break;
            case 2:
                GameObject[] i3 = new GameObject[]{c[num]};
                outlineObjects.ChangeHighlight(i3, true);
                break;
        }
    }

    private void TurnOff()
    {
        outlineObjects.ChangeHighlight(false);
    }
}
