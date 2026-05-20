using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ClockController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public Transform hourHand;
    public Transform minuteHand;
    public Transform secondHand;

    private float hourOffset = -270f;
    private float minuteOffset = 0f;
    private float secondOffset = 0f;
/*
    void Update()
    {
        DateTime time = DateTime.Now;

        float seconds = time.Second + time.Millisecond / 1000f;
        float minutes = time.Minute + seconds / 60f;
        float hours = (time.Hour % 12) + minutes / 60f;

        secondHand.localRotation = Quaternion.Euler( seconds * 6f, 0, 0);
        minuteHand.localRotation = Quaternion.Euler( minutes * 6f, 0, 0);
        hourHand.localRotation = Quaternion.Euler( hours * 30f, 0, 0);
    }*/


    
    void Update()
    {
        //DateTime time = DateTime.Now;
        DateTime time = DateTime.Now.ToLocalTime();

        float seconds = time.Second + time.Millisecond / 1000f;
        float minutes = time.Minute + seconds / 60f;
        float hours = (time.Hour % 12) + minutes / 60f;

        

        // Rotaciones (en negativo porque en Unity rota al revés del reloj)
        secondHand.localRotation = Quaternion.Euler( seconds * 6f + secondOffset, 0, 0);
        minuteHand.localRotation = Quaternion.Euler( minutes * 6f + minuteOffset, 0, 0);
        hourHand.localRotation = Quaternion.Euler( hours * 30f + hourOffset, 0, 0);
    }
}
