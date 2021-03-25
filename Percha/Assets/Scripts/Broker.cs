using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Broker 
{
    public static event Action<byte> IsTouching;
    
    public static void CallIsTouching(byte a)
    {
        if (IsTouching != null)
        {
            IsTouching(a);
        }
    }
    
    public static event Action<int> GestRec;
    
    public static void CallGestRec(int gestNumber)
    {
        if ( GestRec != null)
        {
            GestRec(gestNumber);
        }
    }
}
