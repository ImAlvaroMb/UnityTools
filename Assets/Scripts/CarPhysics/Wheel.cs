using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    public WheelCollider wheelCollider;
    public Transform wheelMesh;
    [SerializeField] private bool isTractionWheel;
    [SerializeField] private float motorTorque; 
    [SerializeField] private float brakeTorque; 
    [SerializeField] private float steerAngle; 
    [SerializeField] private float steeringResistance;// resistance to prevent overturning (generate a force to counter the cardrigid bodys centrifugal force) 


    public void ApplyMotorTorque(float torque)
    {
        if(isTractionWheel)
        {
            wheelCollider.motorTorque = torque; 
        }
    }

    public void ApplyBrakeTorque(float torque)
    {
        wheelCollider.brakeTorque = torque;
    }

    public void ApplySteering(float angle)
    {
        wheelCollider.steerAngle = angle;
    }

    public void ApplySteeringResistance(Rigidbody carRb)
    {
        Vector3 steeringDirection = transform.right;//directionm of the steering force
        Vector3 tireWorldVelocity = carRb.GetPointVelocity(wheelCollider.transform.position);//wheel velocity

        float steeringVelocity = Vector3.Dot(steeringDirection, tireWorldVelocity);//velocity in the steering direction
        float desiredVelocityChange = -steeringVelocity * steeringResistance;// calculate resistance
        float desiredAcceleration = desiredVelocityChange / Time.fixedDeltaTime;//acceleration needed to apply

        Vector3 resistanceForce = steeringDirection * (wheelCollider.mass * desiredAcceleration); // f = m * a + direction
        carRb.AddForceAtPosition(resistanceForce, wheelCollider.transform.position);
    }

    public void UpdateWheelVisuals()
    {
        Vector3 position;
        Quaternion rotation;

        wheelCollider.GetWorldPose(out position, out rotation);
        if(wheelMesh != null)
        {
            wheelMesh.position = position;
            wheelMesh.rotation = rotation;
        }
    }
}
