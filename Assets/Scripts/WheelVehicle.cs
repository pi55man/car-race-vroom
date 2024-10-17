using System.Runtime.CompilerServices;
using UnityEngine;



[assembly: InternalsVisibleTo("VehicleBehaviour.Dots")]
namespace VehicleBehaviour {
    [RequireComponent(typeof(Rigidbody))]
    public class WheelVehicle : MonoBehaviour {
        
        [Header("Inputs")]
   
        [SerializeField] bool isPlayer = true;
        public bool IsPlayer { get => isPlayer;
            set => isPlayer = value;
        } 

        [SerializeField] internal VehicleInputs m_Inputs;
        string throttleInput => m_Inputs.ThrottleInput;
        string brakeInput => m_Inputs.BrakeInput;
        string turnInput => m_Inputs.TurnInput;
        string jumpInput => m_Inputs.JumpInput;
        string driftInput => m_Inputs.DriftInput;
	    string boostInput => m_Inputs.BoostInput;
        // inputs in manager  to be read 
        
        
        [SerializeField] AnimationCurve turnInputCurve = AnimationCurve.Linear(-1.0f, -1.0f, 1.0f, 1.0f);

        [Header("Wheels")]
        [SerializeField] WheelCollider[] driveWheel = new WheelCollider[0];
        public WheelCollider[] DriveWheel => driveWheel;
        [SerializeField] WheelCollider[] turnWheel = new WheelCollider[0];

        public WheelCollider[] TurnWheel => turnWheel;

        bool isGrounded = false;
        int lastGroundCheck = 0;
        public bool IsGrounded { get {
            if (lastGroundCheck == Time.frameCount)
                return isGrounded;

            lastGroundCheck = Time.frameCount;
            isGrounded = true;
            foreach (WheelCollider wheel in wheels)
            {
                if (!wheel.gameObject.activeSelf || !wheel.isGrounded)
                    isGrounded = false;
            }
            return isGrounded;
        }}

        [Header("Behaviour")]
        /*
         *  Motor torque represent the torque sent to the wheels by the motor with x: speed in km/h and y: torque
         *  The curve should start at x=0 and y>0 and should end with x>topspeed and y<0
         *  The higher the torque the faster it accelerate
         *  the longer the curve the faster it gets
         */
        [SerializeField] AnimationCurve motorTorque = new AnimationCurve(new Keyframe(0, 200), new Keyframe(50, 300), new Keyframe(200, 0));

        [Range(2, 16)]
        [SerializeField] float diffGearing = 4.0f;
        public float DiffGearing { get => diffGearing;
            set => diffGearing = value;
        }

        //  how hard it brakes
        [SerializeField] float brakeForce = 1500.0f;
        public float BrakeForce { get => brakeForce;
            set => brakeForce = value;
        }

    
        [Range(0f, 50.0f)]
        [SerializeField] float steerAngle = 30.0f;
        public float SteerAngle { get => steerAngle;
            set => steerAngle = Mathf.Clamp(value, 0.0f, 50.0f);
        }

        [Range(0.001f, 1.0f)]
        [SerializeField] float steerSpeed = 0.2f;
        public float SteerSpeed { get => steerSpeed;
            set => steerSpeed = Mathf.Clamp(value, 0.001f, 1.0f);
        }

        

        [Range(0.0f, 2f)]
        [SerializeField] float driftIntensity = 1f;
        public float DriftIntensity { get => driftIntensity;
            set => driftIntensity = Mathf.Clamp(value, 0.0f, 2.0f);
        }

        Vector3 spawnPosition;
        Quaternion spawnRotation;
        //what the fuck is a quaternion shlawg i had to look everything up i wanna kill myself

        
        [SerializeField] Transform centerOfMass = null;

        [Range(0.5f, 10f)]
        [SerializeField] float downforce = 1.0f;

        public float Downforce
        {
            get => downforce;
            set => downforce = Mathf.Clamp(value, 0, 5);
        }     

        float steering;
        public float Steering { get => steering;
            set => steering = Mathf.Clamp(value, -1f, 1f);
        } 

        float throttle;
        public float Throttle { get => throttle;
            set => throttle = Mathf.Clamp(value, -1f, 1f);
        } 

        [SerializeField] bool handbrake;
        public bool Handbrake { get => handbrake;
            set => handbrake = value;
        } 
        
        [HideInInspector] public bool allowDrift = true;
        bool drift;
        public bool Drift { get => drift;
            set => drift = value;
        }         

        //speedometer
        [SerializeField] float speed = 0.0f;
        public float Speed => speed;

        [Header("Particles")]
        // Exhaust fumes
        [SerializeField] ParticleSystem[] gasParticles = new ParticleSystem[0];

        [Header("Boost")]
        // Disable boost
        [HideInInspector] public bool allowBoost = true;

        [SerializeField] float maxBoost = 10f;
        public float MaxBoost { get => maxBoost;
            set => maxBoost = value;
        }

        [SerializeField] float boost = 10f;
        public float Boost { get => boost;
            set => boost = Mathf.Clamp(value, 0f, maxBoost);
        }

        // Regen boostRegen per second until it's back to maxBoost
        [Range(0f, 1f)]
        [SerializeField] float boostRegen = 0.2f;
        public float BoostRegen { get => boostRegen;
            set => boostRegen = Mathf.Clamp01(value);
        }

        [SerializeField] float boostForce = 5000;
        public float BoostForce { get => boostForce;
            set => boostForce = value;
        }

        // Use this to boost when IsPlayer is set to false
        public bool boosting = false;
        // Use this to jump when IsPlayer is set to false
        public bool jumping = false;

        // Boost particles and sound
        [SerializeField] ParticleSystem[] boostParticles = new ParticleSystem[0];
        [SerializeField] AudioClip boostClip = default;
        [SerializeField] AudioSource boostSource = default;
        
        // Private variables set at the start
        Rigidbody rb = default;
        internal WheelCollider[] wheels = new WheelCollider[0];

        // Init rigidbody, center of mass, wheels and more
        void Start() {
#if MULTIOSCONTROLS
            Debug.Log("[ACP] Using MultiOSControls");
#endif
            if (boostClip != null) {
                boostSource.clip = boostClip;
            }

		    boost = maxBoost;

            rb = GetComponent<Rigidbody>();
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;

            if (rb != null && centerOfMass != null)
            {
                rb.centerOfMass = centerOfMass.localPosition;
            }

            wheels = GetComponentsInChildren<WheelCollider>();

            // Set the motor torque to a non null value because 0 means the wheels won't turn no matter what
            foreach (WheelCollider wheel in wheels)
            {
                wheel.motorTorque = 0.0001f;
            }
        }

        // Visual feedbacks and boost regen
        void Update()
        {
            foreach (ParticleSystem gasParticle in gasParticles)
            {
                gasParticle.Play();
                ParticleSystem.EmissionModule em = gasParticle.emission;
                em.rateOverTime = handbrake ? 0 : Mathf.Lerp(em.rateOverTime.constant, Mathf.Clamp(150.0f * throttle, 30.0f, 100.0f), 0.1f);
            }

            if (isPlayer && allowBoost) {
                boost += Time.deltaTime * boostRegen;
                if (boost > maxBoost) { boost = maxBoost; }
            }
        }
        
        // Update everything
        void FixedUpdate () {
            // Mesure current speed
            speed = transform.InverseTransformDirection(rb.velocity).z * 3.6f;

            // Get all the inputs!
            if (isPlayer) {
                // Accelerate & brake
                if (throttleInput != "" && throttleInput != null)
                {
                    throttle = GetInput(throttleInput) - GetInput(brakeInput);
                }
                // Boost
                boosting = (GetInput(boostInput) > 0.5f);
                // Turn
                steering = turnInputCurve.Evaluate(GetInput(turnInput)) * steerAngle;
                // Dirft
                drift = GetInput(driftInput) > 0 && rb.velocity.sqrMagnitude > 100;
                // // Jump
                // jumping = GetInput(jumpInput) != 0;
            }

            // Direction
            foreach (WheelCollider wheel in turnWheel)
            {
                wheel.steerAngle = Mathf.Lerp(wheel.steerAngle, steering, steerSpeed);
            }

            foreach (WheelCollider wheel in wheels)
            {
                wheel.motorTorque = 0.0001f;
                wheel.brakeTorque = 0;
            }

            // Handbrake
            if (handbrake)
            {
                foreach (WheelCollider wheel in wheels)
                {
                    // Don't zero out this value or the wheel completly lock up
                    wheel.motorTorque = 0.0001f;
                    wheel.brakeTorque = brakeForce;
                }
            }
            else if (throttle != 0 && (Mathf.Abs(speed) < 4 || Mathf.Sign(speed) == Mathf.Sign(throttle)))
            {
                foreach (WheelCollider wheel in driveWheel)
                {
                    wheel.motorTorque = throttle * motorTorque.Evaluate(speed) * diffGearing / driveWheel.Length;
                }
            }
            else if (throttle != 0)
            {
                foreach (WheelCollider wheel in wheels)
                {
                    wheel.brakeTorque = Mathf.Abs(throttle) * brakeForce;
                }
            }

            // Boost
            if (boosting && allowBoost && boost > 0.1f) {
                rb.AddForce(transform.forward * boostForce);

                boost -= Time.fixedDeltaTime;
                if (boost < 0f) { boost = 0f; }

                if (boostParticles.Length > 0 && !boostParticles[0].isPlaying) {
                    foreach (ParticleSystem boostParticle in boostParticles) {
                        boostParticle.Play();
                    }
                }

                if (boostSource != null && !boostSource.isPlaying) {
                    boostSource.Play();
                }
            } else {
                if (boostParticles.Length > 0 && boostParticles[0].isPlaying) {
                    foreach (ParticleSystem boostParticle in boostParticles) {
                        boostParticle.Stop();
                    }
                }

                if (boostSource != null && boostSource.isPlaying) {
                    boostSource.Stop();
                }
            }

            // Drift sOMEONE FIX THIS PLEAAAAAAAAAAAAASE
            if (drift && allowDrift) {
                Vector3 driftForce = -transform.right;
                driftForce.y = 0.0f;
                driftForce.Normalize();

                if (steering != 0)
                    driftForce *= rb.mass * speed/7f * throttle * steering/steerAngle * 0.1f;
                Vector3 driftTorque = transform.up * 0.1f * steering/steerAngle;


                rb.AddForce(driftForce * driftIntensity, ForceMode.Force);
                rb.AddTorque(driftTorque * driftIntensity, ForceMode.VelocityChange);             
            }
            
            // Downforce
            rb.AddForce(-transform.up * speed * downforce);
        }

        // Reposition the car to the start position
        public void ResetPos() {
            transform.position = spawnPosition;
            transform.rotation = spawnRotation;

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        public void ToogleHandbrake(bool h)
        {
            handbrake = h;
        }

        private float GetInput(string input) {

        return Input.GetAxis(input);

        }
    }
}
