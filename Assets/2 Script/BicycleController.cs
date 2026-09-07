using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SBPScripts
{
    [System.Serializable]
    public class CycleGeometry
    {
        public GameObject handles, lowerFork, fWheelVisual, RWheel, crank, lPedal, rPedal, fGear, rGear;
    }
     
    [System.Serializable]
    public class PedalAdjustments
    {
        public float crankRadius;
        public Vector3 lPedalOffset, rPedalOffset;
        public float pedalingSpeed;
    }
    // Wheel Friction Settings
    [System.Serializable]
    public class WheelFrictionSettings
    {
        public PhysicsMaterial fPhysicMaterial, rPhysicMaterial;
        public Vector2 fFriction, rFriction;
    }
    // Way Point System Class - Replay Ghosting system
    [System.Serializable]
    public class WayPointSystem
    {
        public enum RecordingState { DoNothing, Record, Playback };
        public RecordingState recordingState = RecordingState.DoNothing;
        [Range(1, 10)]
        public int frameIncrement;
        [HideInInspector]
        public List<Vector3> bicyclePositionTransform;
        [HideInInspector]
        public List<Quaternion> bicycleRotationTransform;
        [HideInInspector]
        public List<Vector2Int> movementInstructionSet;
        [HideInInspector]
        public List<bool> sprintInstructionSet;
        [HideInInspector]
        public List<int> bHopInstructionSet;
    }
    [System.Serializable]
    public class AirTimeSettings
    {
        public bool freestyle;
        public float airTimeRotationSensitivity;
        [Range(0.5f, 10)]
        public float heightThreshold;
        public float groundSnapSensitivity;
    }
    public class BicycleController : MonoBehaviour
    {
        public CycleGeometry cycleGeometry;
        public GameObject fPhysicsWheel, rPhysicsWheel;
        public WheelFrictionSettings wheelFrictionSettings;
        public AnimationCurve accelerationCurve;
        [Tooltip("Steer Angle over Speed")]
        public AnimationCurve steerAngle;
        public float axisAngle;

        [Header("Steering Tuning")]
        [Tooltip("Ceklis jika ingin menekan A/D langsung memutar stang 100% instan tanpa jeda ramp-up hold")]
        public bool instantSteering = false;
        [Tooltip("Sensitivitas ramp-up belokan keyboard (makin tinggi makin lincah/responsif)")]
        public float steerSensitivity = 15f;
        [Tooltip("Kecepatan stang kembali lurus saat tombol dilepas")]
        public float steerReturnSpeed = 15f;
        [Tooltip("Penguat sudut belok stang")]
        public float steerAngleMultiplier = 1.3f;
        [Tooltip("Bantuan rotasi belokan fisik agar sepeda tidak seret saat belok")]
        public float steerTorqueAssistance = 3.0f;

        public AnimationCurve leanCurve;
        public float torque, topSpeed;

        // BOOST SYSTEM
        private bool isBoosting = false;
        private float originalTopSpeed;

        [Range(0.1f, 0.9f)]
        [Tooltip("Ratio of Relaxed mode to Top Speed")]
        public float relaxedSpeed;
        public float reversingSpeed;
        public Vector3 centerOfMassOffset;
        [HideInInspector]
        public bool isReversing, isAirborne, stuntMode;

        [Range(0, 8)]
        public float oscillationAmount;
       

        [Range(0, 1)]
        public float oscillationAffectSteerRatio;
        float oscillationSteerEffect;
        [HideInInspector]

        public float cycleOscillation;

        // Air stabilizer settings
        [Header("Air Stabilizer Settings")]
        public bool enableStabilizer = true;
        public float maxAirTimeBeforeStabilize = 2f;
        public float stabilizeRotationSpeed = 5f;

        private float airTimeCounter = 0f;

        // END
        [Header("AI Settings")]
        public bool isAIControlled = false; // Diatur otomatis dari tag
        public float steerInput;            // Diatur dari AI
        public float pedalInput;           // Diatur dari AI


        [HideInInspector]
        public Rigidbody rb, fWheelRb, rWheelRb;
        float turnAngle;
        float xQuat, zQuat;
        [HideInInspector]
        public float crankSpeed, crankCurrentQuat, crankLastQuat, restingCrank;
        public PedalAdjustments pedalAdjustments;
        [HideInInspector]
        public float turnLeanAmount;
        RaycastHit hit;
        [HideInInspector]
        public float customSteerAxis, customLeanAxis, customAccelerationAxis, rawCustomAccelerationAxis;
        bool isRaw, sprint;
        [HideInInspector]
        public bool wheelieInput;
        [HideInInspector]
        public float wheeliePower;
        public bool wheelieToggle;
        [HideInInspector]
        public int bunnyHopInputState;
        [HideInInspector]
        public float currentTopSpeed, pickUpSpeed;
        Quaternion initialLowerForkLocalRotaion, initialHandlesRotation;
        ConfigurableJoint fPhysicsWheelConfigJoint, rPhysicsWheelConfigJoint;
     
        public bool groundConformity;
        RaycastHit hitGround;
        Vector3 theRay;
        float groundZ;
        JointDrive fDrive, rYDrive, rZDrive;
        // Attempts to Reduce/eliminate bouncing of the bicycle after a fall impact 
        public bool inelasticCollision;
        [HideInInspector]
        public Vector3 lastVelocity, deceleration, lastDeceleration;
        int impactFrames;
        bool isBunnyHopping;
        [HideInInspector]
        public float bunnyHopAmount;
        // The upward force the rider can bunny hop with. 
        public float bunnyHopStrength;

        [Header("Double Jump Settings")]
        [Tooltip("Enable or disable double jump feature in the air")]
        public bool enableDoubleJump = false; // Checklist di Inspector
        public float doubleJumpForce = 8f;   // Gaya dorong double jump ke atas
        private bool canDoubleJump = false;   // Status tracker apakah double jump bisa digunakan

        public WayPointSystem wayPointSystem;
        public AirTimeSettings airTimeSettings;

        void Awake()
        {
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }

        void Start()
        {
            if (CompareTag("NPCRider"))
            {
                isAIControlled = true;
                Debug.Log($"{name} = NPC Detected (Tag: NPCRider)");
            }
            else
            {
                isAIControlled = false;
                Debug.Log($"{name} = Player Detected (Tag: {tag})");
            }

            rb = GetComponent<Rigidbody>();
            rb.maxAngularVelocity = Mathf.Infinity;
            // rb.maxAngularVelocity = 5f;


            fWheelRb = fPhysicsWheel.GetComponent<Rigidbody>();
            fWheelRb.maxAngularVelocity = Mathf.Infinity;

            rWheelRb = rPhysicsWheel.GetComponent<Rigidbody>();
            rWheelRb.maxAngularVelocity = Mathf.Infinity;

            currentTopSpeed = topSpeed;

            initialHandlesRotation = cycleGeometry.handles.transform.localRotation;
            initialLowerForkLocalRotaion = cycleGeometry.lowerFork.transform.localRotation;

            fPhysicsWheelConfigJoint = fPhysicsWheel.GetComponent<ConfigurableJoint>();
            rPhysicsWheelConfigJoint = rPhysicsWheel.GetComponent<ConfigurableJoint>();

            //Recording is set to 0 to remove the recording previous data if not set to playback
            if (wayPointSystem.recordingState == WayPointSystem.RecordingState.Record || wayPointSystem.recordingState == WayPointSystem.RecordingState.DoNothing)
            {
                wayPointSystem.bicyclePositionTransform.Clear();
                wayPointSystem.bicycleRotationTransform.Clear();
                wayPointSystem.movementInstructionSet.Clear();
                wayPointSystem.sprintInstructionSet.Clear();
                wayPointSystem.bHopInstructionSet.Clear();
            }
        }

        void FixedUpdate()
        {
            float currentSpeed = rb.linearVelocity.magnitude;
            float effectiveSteerAngle = steerAngle.Evaluate(currentSpeed) * steerAngleMultiplier;

            // Bantuan rotasi belokan fisik agar sepeda lincah dan tidak seret saat belok
            if (!isAirborne && Mathf.Abs(customSteerAxis) > 0.05f && currentSpeed > 0.3f)
            {
                float turnTorque = customSteerAxis * (currentSpeed * 0.15f + 3f) * steerTorqueAssistance;
                rb.AddTorque(Vector3.up * turnTorque, ForceMode.Acceleration);
            }

            // 1. Update fork normal dari steer
            cycleGeometry.lowerFork.transform.localRotation = Quaternion.Euler(
                0,
                customSteerAxis * effectiveSteerAngle + oscillationSteerEffect * 5,
                customSteerAxis * -axisAngle
            ) * initialLowerForkLocalRotaion;

            // 2. Jika airborne / kena impact, reset secara smooth
            if (isAirborne || impactFrames > 0)
            {
                cycleGeometry.lowerFork.transform.localRotation = Quaternion.Lerp(
                    cycleGeometry.lowerFork.transform.localRotation,
                    initialLowerForkLocalRotaion,
                    Time.fixedDeltaTime * 3f
                );
            }

            // 3. Clamp rotasi akhir supaya gak ngawur
            Vector3 forkEuler = cycleGeometry.lowerFork.transform.localEulerAngles;
            forkEuler.x = ClampAngle(forkEuler.x, -20f, 20f);
            forkEuler.z = ClampAngle(forkEuler.z, -30f, 30f);
            cycleGeometry.lowerFork.transform.localEulerAngles = forkEuler;


            //Physics based Steering Control.
            fPhysicsWheel.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + customSteerAxis * effectiveSteerAngle + oscillationSteerEffect, 0);
            fPhysicsWheelConfigJoint.axis = new Vector3(5, 0, 0);

            //Power Control. Wheel Torque + Acceleration curves

            //cache rb velocity
            // float currentSpeed = rb.linearVelocity.magnitude;

            if (isBoosting)
            {
                currentTopSpeed = topSpeed; // langsung pakai nilai boosted
            }
            else
            {
                if (!sprint)
                    currentTopSpeed = Mathf.Lerp(currentTopSpeed, topSpeed * relaxedSpeed, Time.fixedDeltaTime);
                else
                   
                    currentTopSpeed = Mathf.Lerp(currentTopSpeed, topSpeed, Time.fixedDeltaTime);
            }
            if (airTimeCounter > maxAirTimeBeforeStabilize)
            {
                Vector3 euler = transform.eulerAngles;
                Quaternion targetRotation = Quaternion.Euler(euler.x, euler.y, 0f);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * stabilizeRotationSpeed));
            }

            // AIR STABILIZER
            if (hit.distance > 2f || impactFrames > 0)
            {
                isAirborne = true;

                // 🔧 Hitung durasi di udara
                if (enableStabilizer)
                {
                    airTimeCounter += Time.fixedDeltaTime;

                    if (airTimeCounter > maxAirTimeBeforeStabilize)
                    {
                        // Stabilkan rotasi Z agar tetap lurus
                        Vector3 euler = transform.eulerAngles;
                        euler.z = Mathf.LerpAngle(euler.z, 0f, Time.fixedDeltaTime * stabilizeRotationSpeed);
                        transform.rotation = Quaternion.Euler(euler);
                    }
                }
            }
            else
            {
                isAirborne = false;
                airTimeCounter = 0f; // ✅ Reset saat kembali menyentuh tanah
            }
            // AIR STABILIZER end
            if (impactFrames > 0)
            {
                rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, Time.fixedDeltaTime * 5f);
            }





            if (currentSpeed < currentTopSpeed && rawCustomAccelerationAxis > 0)
                rWheelRb.AddTorque(transform.right * torque * customAccelerationAxis);

            if (currentSpeed < currentTopSpeed && rawCustomAccelerationAxis > 0 && !isAirborne && !isBunnyHopping)
                rb.AddForce(transform.forward * accelerationCurve.Evaluate(customAccelerationAxis));

            if (currentSpeed < reversingSpeed && rawCustomAccelerationAxis < 0 && !isAirborne && !isBunnyHopping)
                rb.AddForce(-transform.forward * accelerationCurve.Evaluate(customAccelerationAxis) * 0.5f);

            if (transform.InverseTransformDirection(rb.linearVelocity).z < 0)
                isReversing = true;
            else
                isReversing = false;

            if (rawCustomAccelerationAxis < 0 && isReversing == false && !isAirborne && !isBunnyHopping)
                rb.AddForce(-transform.forward * accelerationCurve.Evaluate(customAccelerationAxis) * 2);

            // Center of Mass handling
            if (stuntMode)
                rb.centerOfMass = GetComponent<BoxCollider>().center;
            else
                rb.centerOfMass = Vector3.zero + centerOfMassOffset;

            //Handles
            cycleGeometry.handles.transform.localRotation = Quaternion.Euler(0, customSteerAxis * effectiveSteerAngle + oscillationSteerEffect * 5, 0) * initialHandlesRotation;

            //LowerFork
            cycleGeometry.lowerFork.transform.localRotation = Quaternion.Euler(0, customSteerAxis * effectiveSteerAngle + oscillationSteerEffect * 5, customSteerAxis * -axisAngle) * initialLowerForkLocalRotaion;

            //FWheelVisual
            xQuat = Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y));
            zQuat = Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y));
            cycleGeometry.fWheelVisual.transform.rotation = Quaternion.Euler(xQuat * (customSteerAxis * -axisAngle), customSteerAxis * effectiveSteerAngle + oscillationSteerEffect * 5, zQuat * (customSteerAxis * -axisAngle));
            cycleGeometry.fWheelVisual.transform.GetChild(0).transform.localRotation = cycleGeometry.RWheel.transform.rotation;

            //Crank
            crankCurrentQuat = cycleGeometry.RWheel.transform.rotation.eulerAngles.x;
            if (customAccelerationAxis > 0 && !isAirborne && !isBunnyHopping)
            {
                crankSpeed += Mathf.Sqrt(customAccelerationAxis * Mathf.Abs(Mathf.DeltaAngle(crankCurrentQuat, crankLastQuat) * pedalAdjustments.pedalingSpeed));
                crankSpeed %= 360;
            }
            else if (Mathf.Floor(crankSpeed) > restingCrank)
                crankSpeed += -6;
            else if (Mathf.Floor(crankSpeed) < restingCrank)
                crankSpeed = Mathf.Lerp(crankSpeed, restingCrank, Time.fixedDeltaTime * 5);

            crankLastQuat = crankCurrentQuat;
            cycleGeometry.crank.transform.localRotation = Quaternion.Euler(crankSpeed, 0, 0);

            //Pedals
            cycleGeometry.lPedal.transform.localPosition = pedalAdjustments.lPedalOffset + new Vector3(0, Mathf.Cos(Mathf.Deg2Rad * (crankSpeed + 180)) * pedalAdjustments.crankRadius, Mathf.Sin(Mathf.Deg2Rad * (crankSpeed + 180)) * pedalAdjustments.crankRadius);
            cycleGeometry.rPedal.transform.localPosition = pedalAdjustments.rPedalOffset + new Vector3(0, Mathf.Cos(Mathf.Deg2Rad * (crankSpeed)) * pedalAdjustments.crankRadius, Mathf.Sin(Mathf.Deg2Rad * (crankSpeed)) * pedalAdjustments.crankRadius);

            //FGear
            if (cycleGeometry.fGear != null)
                cycleGeometry.fGear.transform.rotation = cycleGeometry.crank.transform.rotation;
            //RGear
            if (cycleGeometry.rGear != null)
                cycleGeometry.rGear.transform.rotation = rPhysicsWheel.transform.rotation;

            //CycleOscillation
            if ((sprint && currentSpeed > 5 && isReversing == false) || isAirborne || isBunnyHopping)
                pickUpSpeed += Time.fixedDeltaTime * 2;
            else
                pickUpSpeed -= Time.fixedDeltaTime * 2;

            pickUpSpeed = Mathf.Clamp(pickUpSpeed, 0.1f, 1);

            cycleOscillation = -Mathf.Sin(Mathf.Deg2Rad * (crankSpeed + 90)) * (oscillationAmount * (Mathf.Clamp(currentTopSpeed / currentSpeed, 1f, 1.5f))) * pickUpSpeed;
            turnLeanAmount = -leanCurve.Evaluate(customLeanAxis) * Mathf.Clamp(currentSpeed * 0.1f, 0, 1);
            oscillationSteerEffect = cycleOscillation * Mathf.Clamp01(customAccelerationAxis) * (oscillationAffectSteerRatio * (Mathf.Clamp(topSpeed / currentSpeed, 1f, 1.5f)));

            //Friction & Bounce Settings
            if (wheelFrictionSettings.fPhysicMaterial != null)
            {
                wheelFrictionSettings.fPhysicMaterial.staticFriction = wheelFrictionSettings.fFriction.x;
                wheelFrictionSettings.fPhysicMaterial.dynamicFriction = wheelFrictionSettings.fFriction.y;
                wheelFrictionSettings.fPhysicMaterial.bounciness = 0f;
                wheelFrictionSettings.fPhysicMaterial.bounceCombine = PhysicsMaterialCombine.Minimum;
            }
            if (wheelFrictionSettings.rPhysicMaterial != null)
            {
                wheelFrictionSettings.rPhysicMaterial.staticFriction = wheelFrictionSettings.rFriction.x;
                wheelFrictionSettings.rPhysicMaterial.dynamicFriction = wheelFrictionSettings.rFriction.y;
                wheelFrictionSettings.rPhysicMaterial.bounciness = 0f;
                wheelFrictionSettings.rPhysicMaterial.bounceCombine = PhysicsMaterialCombine.Minimum;
            }

            if (Physics.Raycast(fPhysicsWheel.transform.position, Vector3.down, out hit, Mathf.Infinity))
                if (hit.distance < 0.5f)
                {
                    Vector3 velf = fPhysicsWheel.transform.InverseTransformDirection(fWheelRb.linearVelocity);
                    velf.x *= Mathf.Clamp01(1 / (wheelFrictionSettings.fFriction.x + wheelFrictionSettings.fFriction.y));
                    fWheelRb.linearVelocity = fPhysicsWheel.transform.TransformDirection(velf);
                }
            if (Physics.Raycast(rPhysicsWheel.transform.position, Vector3.down, out hit, Mathf.Infinity))
                if (hit.distance < 0.5f)
                {
                    Vector3 velr = rPhysicsWheel.transform.InverseTransformDirection(rWheelRb.linearVelocity);
                    velr.x *= Mathf.Clamp01(1 / (wheelFrictionSettings.rFriction.x + wheelFrictionSettings.rFriction.y));
                    rWheelRb.linearVelocity = rPhysicsWheel.transform.TransformDirection(velr);
                }

            //Impact sensing
            deceleration = (fWheelRb.linearVelocity - lastVelocity) / Time.fixedDeltaTime;
            lastVelocity = fWheelRb.linearVelocity;
            impactFrames--;
            impactFrames = Mathf.Clamp(impactFrames, 0, 15);
            // Hanya aktifkan impactFrames dari pendaratan vertikal (bukan dari tabrakan tembok horizontal)
            if (deceleration.y > 200 && lastDeceleration.y < -1 && lastVelocity.y < -2f)
                impactFrames = 30;

            lastDeceleration = deceleration;

            if (impactFrames > 0 && inelasticCollision)
            {
                // Redam pantulan kecepatan vertikal positif saat mendarat agar tidak melontar ke atas atau menembus tanah
                if (fWheelRb.linearVelocity.y > 0)
                    fWheelRb.linearVelocity = new Vector3(fWheelRb.linearVelocity.x, fWheelRb.linearVelocity.y * 0.1f, fWheelRb.linearVelocity.z);
                if (rWheelRb.linearVelocity.y > 0)
                    rWheelRb.linearVelocity = new Vector3(rWheelRb.linearVelocity.x, rWheelRb.linearVelocity.y * 0.1f, rWheelRb.linearVelocity.z);
                if (rb.linearVelocity.y > 0)
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y * 0.1f, rb.linearVelocity.z);
            }

            // Safety Clamp: Cegah fisika ConfigurableJoint meledak / melontarkan sepeda ke langit saat menabrak rintangan
            float maxAllowedUpwardVelocity = 15f;
            if (rb.linearVelocity.y > maxAllowedUpwardVelocity)
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, maxAllowedUpwardVelocity, rb.linearVelocity.z);
            if (fWheelRb != null && fWheelRb.linearVelocity.y > maxAllowedUpwardVelocity)
                fWheelRb.linearVelocity = new Vector3(fWheelRb.linearVelocity.x, maxAllowedUpwardVelocity, fWheelRb.linearVelocity.z);
            if (rWheelRb != null && rWheelRb.linearVelocity.y > maxAllowedUpwardVelocity)
                rWheelRb.linearVelocity = new Vector3(rWheelRb.linearVelocity.x, maxAllowedUpwardVelocity, rWheelRb.linearVelocity.z);

            //AirControl
            if (Physics.Raycast(transform.position + new Vector3(0, 1f, 0), Vector3.down, out hit, Mathf.Infinity))
            {
                if (hit.distance > 2f || impactFrames > 0)
                {
                    isAirborne = true;
                    restingCrank = 100;
                }
                else if (isBunnyHopping)
                {
                    restingCrank = 100;
                }
                else
                {
                    isAirborne = false;
                    restingCrank = 10;
                }
                // For stunts
                // 5f is the snap to ground distance
                if (hit.distance > airTimeSettings.heightThreshold && airTimeSettings.freestyle)
                {
                    stuntMode = true;
                    // Stunt + flips controls (Not available for Waypoint system as of yet)
                    // You may use Numpad Inputs as well.
                    rb.AddTorque(Vector3.up * customSteerAxis * 4 * airTimeSettings.airTimeRotationSensitivity, ForceMode.Impulse);
                    rb.AddTorque(transform.right * rawCustomAccelerationAxis * -3 * airTimeSettings.airTimeRotationSensitivity, ForceMode.Impulse);
                }
                else
                    stuntMode = false;
            }

            // Setting the Main Rotational movements of the bicycle
            if (airTimeSettings.freestyle)
            {
                if (!stuntMode && isAirborne)
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, transform.rotation.eulerAngles.y, turnLeanAmount + cycleOscillation + GroundConformity(groundConformity)), Time.fixedDeltaTime * airTimeSettings.groundSnapSensitivity);
                else if (!stuntMode && !isAirborne)
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, turnLeanAmount + cycleOscillation + GroundConformity(groundConformity)), Time.fixedDeltaTime * 10 * airTimeSettings.groundSnapSensitivity);
            }
            else
            {
                //Pre-version 1.5
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, turnLeanAmount + cycleOscillation + GroundConformity(groundConformity));
            }
            //Wheelie
            if(!isAirborne && wheelieInput && rawCustomAccelerationAxis>0)
            {
                rb.angularDamping = 15;
                wheeliePower = customAccelerationAxis*150*System.Convert.ToInt32(wheelieToggle);
                var rot = Quaternion.FromToRotation(transform.forward, new Vector3(transform.forward.x,0.75f,transform.forward.z));
                rb.AddTorque(new Vector3(rot.x, rot.y, rot.z) * wheeliePower, ForceMode.Acceleration);
            }
            else
            {
                rb.angularDamping = 1;
            }


        }
        void Update()
        {
            ApplyCustomInput();

            // Reset Double Jump saat mendarat di tanah
            if (!isAirborne)
            {
                canDoubleJump = true;
            }

            // Logika Double Jump saat di udara (aktif jika enableDoubleJump di-ceklis di Inspector)
            if (enableDoubleJump && isAirborne && canDoubleJump && Input.GetKeyDown(KeyCode.Space))
            {
                PerformDoubleJump();
            }

            //GetKeyUp/Down requires an Update Cycle
            //BunnyHopping
            if (bunnyHopInputState == 1)
            {
                isBunnyHopping = true;
                bunnyHopAmount += Time.deltaTime * 8f;
            }
            if (bunnyHopInputState == -1)
                StartCoroutine(DelayBunnyHop());

            if (bunnyHopInputState == -1 && !isAirborne)
                rb.AddForce(transform.up * bunnyHopAmount * bunnyHopStrength, ForceMode.VelocityChange);
            else
                bunnyHopAmount = Mathf.Lerp(bunnyHopAmount, 0, Time.deltaTime * 8f);

            bunnyHopAmount = Mathf.Clamp01(bunnyHopAmount);

        }

        private void PerformDoubleJump()
        {
            canDoubleJump = false;
            // Dorong velocity Y ke atas untuk efek lompatan kedua di udara
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, doubleJumpForce, rb.linearVelocity.z);
            if (fWheelRb != null) fWheelRb.linearVelocity = new Vector3(fWheelRb.linearVelocity.x, doubleJumpForce, fWheelRb.linearVelocity.z);
            if (rWheelRb != null) rWheelRb.linearVelocity = new Vector3(rWheelRb.linearVelocity.x, doubleJumpForce, rWheelRb.linearVelocity.z);
            Debug.Log("<color=yellow>Double Jump Executed!</color>");
        }

        private void OnCollisionEnter(Collision collision)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                // Deteksi jika menabrak tembok/rintangan (normal bidang tegak lurus/miring > 50 derajat)
                float wallAngle = Vector3.Angle(contact.normal, Vector3.up);
                if (wallAngle > 50f)
                {
                    // Redam lonjakan impulsif ke atas saat menabrak rintangan agar tidak terlempar ke langit
                    if (rb != null && rb.linearVelocity.y > 3f)
                        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 3f, rb.linearVelocity.z);
                    if (fWheelRb != null && fWheelRb.linearVelocity.y > 3f)
                        fWheelRb.linearVelocity = new Vector3(fWheelRb.linearVelocity.x, 3f, fWheelRb.linearVelocity.z);
                    if (rWheelRb != null && rWheelRb.linearVelocity.y > 3f)
                        rWheelRb.linearVelocity = new Vector3(rWheelRb.linearVelocity.x, rWheelRb.linearVelocity.y * 0.2f, rWheelRb.linearVelocity.z);
                    break;
                }
            }
        }

        public void ResetPhysicsState()
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (fWheelRb != null)
            {
                fWheelRb.linearVelocity = Vector3.zero;
                fWheelRb.angularVelocity = Vector3.zero;
            }

            if (rWheelRb != null)
            {
                rWheelRb.linearVelocity = Vector3.zero;
                rWheelRb.angularVelocity = Vector3.zero;
            }

            Rigidbody[] childRbs = GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody r in childRbs)
            {
                r.linearVelocity = Vector3.zero;
                r.angularVelocity = Vector3.zero;
            }

            customSteerAxis = 0f;
            customLeanAxis = 0f;
            customAccelerationAxis = 0f;
            rawCustomAccelerationAxis = 0f;
            steerInput = 0f;
            pedalInput = 0f;
            crankSpeed = restingCrank;
            impactFrames = 0;
            isAirborne = false;
            isBunnyHopping = false;
            isReversing = false;

            Debug.Log("BicycleController physics and wheel state completely reset on respawn.");
        }
        float GroundConformity(bool toggle)
        {
            if (toggle)
            {
                groundZ = transform.rotation.eulerAngles.z;
            }
            return groundZ;

        }
        float ClampAngle(float angle, float min, float max)
        {
            angle = Mathf.Repeat(angle + 180f, 360f) - 180f;
            return Mathf.Clamp(angle, min, max);
        }

        // Boost
        public void ApplySpeedBoost(float multiplier, float duration)
        {
            if (!isBoosting)
            {
                StartCoroutine(SpeedBoostRoutine(multiplier, duration));
            }
        }

        private IEnumerator SpeedBoostRoutine(float multiplier, float duration)
        {
            isBoosting = true;
            
            // Simpan nilai topSpeed awal
            float originalTopSpeed = topSpeed;
            float targetTopSpeed = topSpeed * multiplier;
            currentTopSpeed = targetTopSpeed;

            // Terus perbarui kecepatan Rigidbody untuk durasi boost
            while (duration > 0)
            {
                // Memastikan kecepatan tetap tinggi
                rb.linearVelocity = rb.linearVelocity.normalized * targetTopSpeed;

                // Kurangi durasi boost
                duration -= Time.deltaTime;

                // Menunggu hingga frame berikutnya
                yield return null;
            }

            // Reset kecepatan ke nilai semula setelah boost selesai
            currentTopSpeed = originalTopSpeed;
            rb.linearVelocity = rb.linearVelocity.normalized * originalTopSpeed;
            isBoosting = false;
        }


        // void ApplyCustomInput()
        // {
        //     if (wayPointSystem.recordingState == WayPointSystem.RecordingState.DoNothing || wayPointSystem.recordingState == WayPointSystem.RecordingState.Record)
        //     {
        //         CustomInput("Horizontal", ref customSteerAxis, 5, 5, false);
        //         CustomInput("Vertical", ref customAccelerationAxis, 1, 1, false);
        //         CustomInput("Horizontal", ref customLeanAxis, 1, 1, false);
        //         CustomInput("Vertical", ref rawCustomAccelerationAxis, 1, 1, true);

        //         sprint = Input.GetKey(KeyCode.LeftShift);

        //         wheelieInput = Input.GetKey(KeyCode.LeftControl);

        //         //Stateful Input - bunny hopping
        //         if (Input.GetKey(KeyCode.Space))
        //             bunnyHopInputState = 1;
        //         else if (Input.GetKeyUp(KeyCode.Space))
        //             bunnyHopInputState = -1;
        //         else
        //             bunnyHopInputState = 0;

        //         //Record
        //         if (wayPointSystem.recordingState == WayPointSystem.RecordingState.Record)
        //         {
        //             if (Time.frameCount % wayPointSystem.frameIncrement == 0)
        //             {
        //                 wayPointSystem.bicyclePositionTransform.Add(new Vector3(Mathf.Round(transform.position.x * 100f) * 0.01f, Mathf.Round(transform.position.y * 100f) * 0.01f, Mathf.Round(transform.position.z * 100f) * 0.01f));
        //                 wayPointSystem.bicycleRotationTransform.Add(transform.rotation);
        //                 wayPointSystem.movementInstructionSet.Add(new Vector2Int((int)Input.GetAxisRaw("Horizontal"), (int)Input.GetAxisRaw("Vertical")));
        //                 wayPointSystem.sprintInstructionSet.Add(sprint);
        //                 wayPointSystem.bHopInstructionSet.Add(bunnyHopInputState);
        //             }
        //         }
        //     }

        //     else
        //     {
        //         if (wayPointSystem.recordingState == WayPointSystem.RecordingState.Playback)
        //         {
        //             if (wayPointSystem.movementInstructionSet.Count - 1 > Time.frameCount / wayPointSystem.frameIncrement)
        //             {
        //                 transform.position = Vector3.Lerp(transform.position, wayPointSystem.bicyclePositionTransform[Time.frameCount / wayPointSystem.frameIncrement], Time.deltaTime * wayPointSystem.frameIncrement);
        //                 transform.rotation = Quaternion.Lerp(transform.rotation, wayPointSystem.bicycleRotationTransform[Time.frameCount / wayPointSystem.frameIncrement], Time.deltaTime * wayPointSystem.frameIncrement);
        //                 WayPointInput(wayPointSystem.movementInstructionSet[Time.frameCount / wayPointSystem.frameIncrement].x, ref customSteerAxis, 5, 5, false);
        //                 WayPointInput(wayPointSystem.movementInstructionSet[Time.frameCount / wayPointSystem.frameIncrement].y, ref customAccelerationAxis, 1, 1, false);
        //                 WayPointInput(wayPointSystem.movementInstructionSet[Time.frameCount / wayPointSystem.frameIncrement].x, ref customLeanAxis, 1, 1, false);
        //                 WayPointInput(wayPointSystem.movementInstructionSet[Time.frameCount / wayPointSystem.frameIncrement].y, ref rawCustomAccelerationAxis, 1, 1, true);
        //                 sprint = wayPointSystem.sprintInstructionSet[Time.frameCount / wayPointSystem.frameIncrement];
        //                 bunnyHopInputState = wayPointSystem.bHopInstructionSet[Time.frameCount / wayPointSystem.frameIncrement];
        //             }
        //         }
        //     }
        // }
        void ApplyCustomInput()
        {
            if (wayPointSystem.recordingState == WayPointSystem.RecordingState.DoNothing || wayPointSystem.recordingState == WayPointSystem.RecordingState.Record)
            {
                if (!isAIControlled)
                {
                    steerInput = Input.GetAxis("Horizontal");
                    pedalInput = Input.GetAxis("Vertical");

                    CustomInput("Horizontal", ref customSteerAxis, steerSensitivity, steerReturnSpeed, instantSteering);
                    CustomInput("Vertical", ref customAccelerationAxis, 1, 1, false);
                    CustomInput("Horizontal", ref customLeanAxis, steerSensitivity, steerReturnSpeed, instantSteering);
                    CustomInput("Vertical", ref rawCustomAccelerationAxis, 1, 1, true);
                }
                else
                {
                    customSteerAxis = steerInput;
                    customAccelerationAxis = pedalInput;
                    rawCustomAccelerationAxis = pedalInput;
                    customLeanAxis = steerInput;
                }

                // Input hanya untuk Player
                if (!isAIControlled)
                {
                    sprint = Input.GetKey(KeyCode.LeftShift);
                    wheelieInput = Input.GetKey(KeyCode.LeftControl);

                    if (Input.GetKey(KeyCode.Space))
                        bunnyHopInputState = 1;
                    else if (Input.GetKeyUp(KeyCode.Space))
                        bunnyHopInputState = -1;
                    else
                        bunnyHopInputState = 0;
                }

                // Ghost recording
                if (wayPointSystem.recordingState == WayPointSystem.RecordingState.Record && !isAIControlled)
                {
                    if (Time.frameCount % wayPointSystem.frameIncrement == 0)
                    {
                        wayPointSystem.bicyclePositionTransform.Add(new Vector3(
                            Mathf.Round(transform.position.x * 100f) * 0.01f,
                            Mathf.Round(transform.position.y * 100f) * 0.01f,
                            Mathf.Round(transform.position.z * 100f) * 0.01f));
                        wayPointSystem.bicycleRotationTransform.Add(transform.rotation);
                        wayPointSystem.movementInstructionSet.Add(new Vector2Int(
                            (int)Input.GetAxisRaw("Horizontal"),
                            (int)Input.GetAxisRaw("Vertical")));
                        wayPointSystem.sprintInstructionSet.Add(sprint);
                        wayPointSystem.bHopInstructionSet.Add(bunnyHopInputState);
                    }
                }
            }
            else
            {
                // Ghost playback
                int frameIndex = Time.frameCount / wayPointSystem.frameIncrement;

                if (wayPointSystem.movementInstructionSet.Count - 1 > frameIndex)
                {
                    transform.position = Vector3.Lerp(transform.position,
                        wayPointSystem.bicyclePositionTransform[frameIndex],
                        Time.deltaTime * wayPointSystem.frameIncrement);

                    transform.rotation = Quaternion.Lerp(transform.rotation,
                        wayPointSystem.bicycleRotationTransform[frameIndex],
                        Time.deltaTime * wayPointSystem.frameIncrement);

                    WayPointInput(wayPointSystem.movementInstructionSet[frameIndex].x, ref customSteerAxis, 5, 5, false);
                    WayPointInput(wayPointSystem.movementInstructionSet[frameIndex].y, ref customAccelerationAxis, 1, 1, false);
                    WayPointInput(wayPointSystem.movementInstructionSet[frameIndex].x, ref customLeanAxis, 1, 1, false);
                    WayPointInput(wayPointSystem.movementInstructionSet[frameIndex].y, ref rawCustomAccelerationAxis, 1, 1, true);

                    sprint = wayPointSystem.sprintInstructionSet[frameIndex];
                    bunnyHopInputState = wayPointSystem.bHopInstructionSet[frameIndex];
                }
            }
        }



        //Input Manager Controls
        float CustomInput(string name, ref float axis, float sensitivity, float gravity, bool isRaw)
        {
            var r = Input.GetAxisRaw(name);
            var s = sensitivity;
            var g = gravity;
            var t = Time.unscaledDeltaTime;

            if (isRaw)
                axis = r;
            else
            {
                if (r != 0)
                    axis = Mathf.Clamp(axis + r * s * t, -1f, 1f);
                else
                    axis = Mathf.Clamp01(Mathf.Abs(axis) - g * t) * Mathf.Sign(axis);
            }

            return axis;
        }

        float WayPointInput(float instruction, ref float axis, float sensitivity, float gravity, bool isRaw)
        {
            var r = instruction;
            var s = sensitivity;
            var g = gravity;
            var t = Time.unscaledDeltaTime;

            if (isRaw)
                axis = r;
            else
            {
                if (r != 0)
                    axis = Mathf.Clamp(axis + r * s * t, -1f, 1f);
                else
                    axis = Mathf.Clamp01(Mathf.Abs(axis) - g * t) * Mathf.Sign(axis);
            }

            return axis;
        }

        IEnumerator DelayBunnyHop()
        {
            yield return new WaitForSeconds(0.5f);
            isBunnyHopping = false;
            yield return null;
        }

    }
}