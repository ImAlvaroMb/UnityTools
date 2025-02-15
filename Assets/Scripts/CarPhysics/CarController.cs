using UnityEngine;

public class CarController : MonoBehaviour
{
    public WheelCollider[] wheelColliders; //front wheels first then back ones
    public bool[] isTractionWheel;//traction wheels, same order than in the wheelColliderArray
    public Transform[] wheelMeshes;//child objects from wheels

    [SerializeField] private float motorTorque;//adjust for acceleration
    [SerializeField] private float brakeTorque;//adjust for braking force
    [SerializeField] private float maxSteerAngle;//adjust for steering snsitivity
    [SerializeField] private float steeringSmoothness;//adjust for steering delay

    private float targetSteerAngle;
    private float currentSteerAngle;

    private void Update()
    {
       
    }

    private void FixedUpdate()
    {
        HandleInput();
        ApplySteering();
        //UpdateWheelVisuals();
    }

    private void HandleInput()
    {
        //acceleratio9n
        float acceleration = Input.GetAxis("Vertical");
        for (int i = 0; i < wheelColliders.Length; i++)
        {
            if (isTractionWheel[i])
            {
                wheelColliders[i].motorTorque = acceleration * motorTorque;
            }
        }

        //braking
        if(Input.GetKey(KeyCode.Space))
        {
            foreach(var wheel in wheelColliders)
            {
                wheel.brakeTorque = brakeTorque;
            }
        } else
        {
            foreach(var wheel in wheelColliders)
            {
                wheel.brakeTorque = 0f;
            }
        }

        //steering
        targetSteerAngle = maxSteerAngle * Input.GetAxis("Horizontal");
    }

    private void ApplySteering()
    {
        //smoothly interpolate the steering angle for a delayed effect
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteerAngle, Time.deltaTime * steeringSmoothness);

        //aapply steering to front wheels (assuming wheels 0 and 1 are the front wheels)
        wheelColliders[0].steerAngle = currentSteerAngle;
        wheelColliders[1].steerAngle = currentSteerAngle;
    }

    private void UpdateWheelVisuals()
    {
        for (int i = 0; i < wheelColliders.Length; i++)
        {
            Vector3 pos;
            Quaternion rot;
            wheelColliders[i].GetWorldPose(out pos, out rot); // Get the Wheel Collider's position and rotation
            wheelMeshes[i].position = pos; // Update the visual mesh's position
            wheelMeshes[i].rotation = rot; // Update the visual mesh's rotation
        }
    }

}
