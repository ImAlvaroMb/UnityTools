using UnityEngine;

public class CarController : MonoBehaviour
{
    public Wheel[] wheels; //front wheels first then back ones

    [SerializeField] private float maxMotorTorque;//adjust for acceleration
    [SerializeField] private float maxBrakeTorque;//adjust for braking force
    [SerializeField] private float maxSteerAngle;//adjust for steering snsitivity
    [SerializeField] private float steeringSmoothness;//adjust for steering delay

    [SerializeField] private SteeringMode steeringMode = SteeringMode.FrontWheel;

    private float targetSteerAngle;
    private float currentSteerAngle;

    private Rigidbody carRb;

    private void Start()
    {
        carRb = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        HandleInput();
        ApplySteering();
    }

    private void FixedUpdate()
    {
        ApplySteeringResiatance();
        UpdateWheelsVisuals();
    }

    private void HandleInput()
    {

        //acceleration
        float acceleration = Input.GetAxis("Vertical") * maxMotorTorque;
        foreach (var wheel in wheels)
        {
            wheel.ApplyMotorTorque(acceleration);
        }

        //braking
        if (Input.GetKey(KeyCode.Space))
        {
            foreach (var wheel in wheels)
            {
                wheel.ApplyBrakeTorque(maxBrakeTorque);
            }
        }
        else
        {
            foreach (var wheel in wheels)
            {
                wheel.ApplyBrakeTorque(0f);
            }
        }

        //steering
        targetSteerAngle = maxSteerAngle * Input.GetAxis("Horizontal");
    }

    private void ApplySteering()
    {
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteerAngle, Time.deltaTime * steeringSmoothness);//smooth interpolate to ad a delay effect (turns are not instant)
        switch (steeringMode)
        {
            case SteeringMode.FrontWheel:
                //front wheels only
                wheels[0].ApplySteering(currentSteerAngle);
                wheels[1].ApplySteering(currentSteerAngle); 
                break;

            case SteeringMode.RearWheel:
                //rear wheels only
                float rearSteerAngle = currentSteerAngle;
                if (carRb.velocity.magnitude < 10f) 
                {
                    rearSteerAngle = -currentSteerAngle; //opositre direction when on lowSpeed
                }
                wheels[2].ApplySteering(rearSteerAngle); 
                wheels[3].ApplySteering(rearSteerAngle); 
                break;

            case SteeringMode.AllWheel:
                foreach (var wheel in wheels)
                {
                    wheel.ApplySteering(currentSteerAngle);
                }
                break;
        }
    }

    private void UpdateWheelsVisuals()
    {
        foreach (var wheel in wheels)
        {
            wheel.UpdateWheelVisuals();
        }
    }

    private void ApplySteeringResiatance()
    {
        foreach (var wheel in wheels)
        {
            wheel.ApplySteeringResistance(carRb);
        }
    }

}
