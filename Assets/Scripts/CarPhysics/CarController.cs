using UnityEngine;

public class CarController : MonoBehaviour
{
    //REALLY IMPORTANT TO ADJUST THE CENTER OF MASS ON THE CAR RIGID BODY, IF THE CENTYER OF MASS IS HIGH (CONSIDERING THE CAR OBJECT) INSTEAD OF LOW THE CAR WILL ROLL EASDILY
    public Wheel[] wheels; //front wheels first then back ones

    [Header("Motor Settings")]
    [SerializeField] private float maxMotorTorque;//adjust for acceleration
    [SerializeField] private float maxBrakeTorque;//adjust for braking force
    [SerializeField] private float maxSteerAngle;//adjust for steering snsitivity
    [SerializeField] private float steeringSmoothness;//adjust for steering delay
    [SerializeField] private SteeringMode steeringMode = SteeringMode.FrontWheel;

    [Header("Speed Limiters")]
    [SerializeField] private float maxRbSpeed;

    [Header("Raycats parameters")]
    public Transform buttomRayPoint;
    public Transform rightRayPoint;
    public Transform leftRayPoint;
    [SerializeField] private float raycastCooldown;
    [SerializeField] private string raycastName;


    private bool canCheckRaycast;
    private bool hasInvokedRaycastTimer = false;
    private bool hasInvokedRaycastDuration = false;

    [Header("Roll preveention")]
    [SerializeField] private float baseDownwardForce; //strength of the force to keep the car grounded
    [SerializeField] private float turnDownwardForceMultiplier; //multiplier when turning
    [SerializeField] private float speedDownwardForceMultiplier; //force based on speed

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
        ApplyRollPrevention();

        if(canCheckRaycast)
        {
            DetectCarState();
        } else
        {
            if(!hasInvokedRaycastTimer)
            {
                TimersManager.Instance.StartTimer(raycastCooldown, () =>
                {
                    canCheckRaycast = true;
                }, raycastName, false, false);
                hasInvokedRaycastTimer = true;
            }
            
        }

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

    private void ApplyRollPrevention()
    {
        //calculate the downward force based on steering input and speed
        float steeringInput = Mathf.Abs(Input.GetAxis("Horizontal"));
        float speedFactor = carRb.velocity.magnitude * speedDownwardForceMultiplier; //speed-based factor
        float downwardForce = baseDownwardForce + (steeringInput * turnDownwardForceMultiplier * speedFactor);

        //apply the downward force to the car's Rigidbody
        Vector3 forceDirection = -transform.up; 
        carRb.AddForce(forceDirection * downwardForce, ForceMode.Force);
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

    private void DetectCarState()//detect whether the car is currently driving or has rolled over
    {
        if(!hasInvokedRaycastDuration)
        {
            TimersManager.Instance.StartTimer(raycastCooldown, () =>
            {
                canCheckRaycast = false;
            }, raycastName + "1", false, false);
            hasInvokedRaycastDuration = true;
        }
        //do raycast logficx
    }

}
