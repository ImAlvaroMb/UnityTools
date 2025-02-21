using System.Threading;
using TMPro;
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

    [Header("Handbrake")]
    [SerializeField] private BrakeMode brakeMode = BrakeMode.AllWheels;

    [Header("Dashing")]
    [SerializeField] private float dashSpeed;
    public bool isDashing { get; set; } = false;

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

    [Header("Testing")]
    public TextMeshProUGUI speedText;

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
        if(!isDashing)
        {
            ApplySteering();
        }
        UpdateSpeedText();

        if(canCheckRaycast)
        {
            DetectCarState();
        } else
        {
            
        }

    }

    private void FixedUpdate()
    {
        //ApplySteeringResiatance();
        ApplyRollPrevention();
        UpdateWheelsVisuals();
    }

    private void HandleInput()
    {

        float accelerationInput = Input.GetAxis("Vertical");
        float acceleration = accelerationInput * maxMotorTorque;

        foreach (var wheel in wheels)
        {
            if (accelerationInput != 0)
            {
                if(!isDashing)
                    wheel.ApplyMotorTorque(acceleration);
            }
            else
            {
                wheel.ApplyMotorTorque(0);
            }
        }

        //braking
        if (Input.GetKey(KeyCode.Space))
        {
            if(!isDashing)
                ApplyBrake();
            /*foreach (var wheel in wheels)
            {
                wheel.ApplyBrakeTorque(maxBrakeTorque);
                Debug.Log("NBraeking");
            }*/
        } else
        {
            foreach (var wheel in wheels)
            {
                wheel.ApplyBrakeTorque(0f);
            }
        }

        if(Input.GetKey(KeyCode.LeftShift))
        {
            HandleDash();
        }
        
       

        //steering
        targetSteerAngle = maxSteerAngle * Input.GetAxis("Horizontal");
    }

    private void ApplyBrake()
    {
        //to do add logic for all brake Types
        switch (brakeMode)
        {
            case BrakeMode.AllWheels:
                foreach (var wheel in wheels)
                {
                    wheel.ApplyBrakeTorque(maxBrakeTorque);
                }
                break;

            case BrakeMode.FrontWheelsStronger:
                wheels[0].ApplyBrakeTorque(maxBrakeTorque * 0.8f);
                wheels[1].ApplyBrakeTorque(maxBrakeTorque * 0.8f);
                wheels[2].ApplyBrakeTorque(maxBrakeTorque * 0.2f);
                wheels[3].ApplyBrakeTorque(maxBrakeTorque * 0.2f);
                break;
        }
    }

    private void HandleDash()
    {
        if(!isDashing) 
        {
            isDashing = true;
            Vector3 dashDirection = transform.forward;
            carRb.isKinematic = true;
            TimersManager.Instance.StartTimer(0.7f, () =>
            {
                isDashing = false;
                carRb.isKinematic = false;
            }, (progress) =>
            {
                transform.position += dashDirection * dashSpeed * Time.deltaTime;
            }, "dash", false, false);
        }
        
    }

    /*private void ApplyBrakeEffect()
    {
        if(!isBraking)
        {
            //evaluate over t to reduce speed in brake time
            isBraking = true;
            float initialSpeed = carRb.velocity.magnitude; 
            TimersManager.Instance.StartTimer(brakeTime,
            () =>
            {
                isBraking = false;
                carRb.velocity = Vector3.zero; //ensure the car is stopped
            },
            (progress) =>
            {
                float reversedProgress = 1f - progress;
                float newSpeed = Mathf.Lerp(initialSpeed, 0, reversedProgress); //reduce speed progressevely
                //carRb.velocity = carRb.velocity.normalized * reversedProgress; //maintain direction
            }, "brake", false, true);

        }

    }*/

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
           
        }
        //do raycast logficx
    }

    private void UpdateSpeedText()
    {
        float speedZ = Mathf.Abs(carRb.velocity.z);
        float speedKmh = speedZ * 3.6f;
        speedText.text = "Speed: " + speedKmh.ToString("F1") + " km/h";
    }

}
