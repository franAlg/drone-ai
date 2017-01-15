﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class BasicControl : MonoBehaviour {

	public GameObject funcion;

    [Header("Limites")]
    public float speedLimit;

    [Header("Control")]
	public Controller Controller;
	public float ThrottleIncrease;
	
	[Header("Motors")]
	public Motor[] Motors;
	public float ThrottleValue;

    [Header("Internal")]
    public ComputerModule Computer;

	//angulo que debe girar el drone
	private float anguloGiro;

	//velocidad de rotacion
	public float turningRate = 30f; 
	// Rotation we should blend towards.
	private Quaternion _targetRotation;
	// Call this when you want to turn the object smoothly.
	public void SetBlendedEulerAngles(Vector3 angles)
	{
		_targetRotation = Quaternion.Euler(angles);
	}


	void FixedUpdate() {
		anguloGiro = funcion.GetComponent<clienteFuzzy> ().Evaluar (funcion.GetComponent<Sensing> ().getMinDistObj(), 
																	funcion.GetComponent<Sensing> ().getAnguloObj(), 
																	funcion.GetComponent<Sensing> ().getAnguloDest());
		Debug.Log ("++ Recibido  angDrone: " + anguloGiro);

		_targetRotation = Quaternion.Euler(0.0f, anguloGiro, 0.0f);
		transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation, turningRate * Time.deltaTime);
		
        Computer.UpdateComputer(Controller.Pitch, Controller.Roll, Controller.Throttle * ThrottleIncrease);
        ThrottleValue = Computer.HeightCorrection;
        ComputeMotors();
        if (Computer != null)
            Computer.UpdateGyro();
        ComputeMotorSpeeds();
    }

    private void ComputeMotors()
    {
        float yaw = 0.0f;
        Rigidbody rb = GetComponent<Rigidbody>();
        int i = 0;
        foreach (Motor motor in Motors)
        {
            motor.UpdateForceValues();
            yaw += motor.SideForce;
            i++;
            Transform t = motor.GetComponent<Transform>();
            rb.AddForceAtPosition(transform.up * motor.UpForce, t.position, ForceMode.Impulse);
        }
        rb.AddTorque(Vector3.up * yaw, ForceMode.Force);
    }

    private void ComputeMotorSpeeds()
    {
        foreach (Motor motor in Motors)
        {
            if (Computer.Gyro.Altitude < 0.1)
                motor.UpdatePropeller(0.0f);
            else
                motor.UpdatePropeller(1200.0f); 
        }
    }
}