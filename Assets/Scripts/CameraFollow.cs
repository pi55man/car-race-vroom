using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace VehicleBehaviour.Utils {
    public class CameraFollow : MonoBehaviour {
        [SerializeField] bool follow = false;
        public bool Follow {
            get => follow;
            set => follow = value;
        }

        [SerializeField] Transform target = default;
        [SerializeField] Transform[] targets = new Transform[0]; // Array of possible targets
        [SerializeField] Vector3 offset = -Vector3.forward;

        [Range(0, 10)]
        [SerializeField] float lerpPositionMultiplier = 1f;
        [Range(0, 10)]
        [SerializeField] float lerpRotationMultiplier = 1f;

        // Speedometer
        [SerializeField] Text speedometer = null;

        Rigidbody rb;
        WheelVehicle vehicle;
        int currentTargetIndex = 0; // Track the current index of the target

        void Start() {
            rb = GetComponent<Rigidbody>();
            // Set initial target
            SetTargetIndex(currentTargetIndex);
        }

        public void SetTargetIndex(int i) {
            if (targets.Length == 0) return; // Return if no targets are set

            // Modulus operator ensures we loop back to the start of the list if index exceeds length
            currentTargetIndex = i % targets.Length;

            // Disable player control on all vehicles
            foreach (Transform t in targets) {
                var v = t != null ? t.GetComponent<WheelVehicle>() : null;
                if (v != null) {
                    v.IsPlayer = false;
                    v.Handbrake = true;
                }
            }

            // Set the new target
            target = targets[currentTargetIndex];

            // Enable player control on the current target vehicle
            vehicle = target != null ? target.GetComponent<WheelVehicle>() : null;
            if (vehicle != null) {
                vehicle.IsPlayer = true;
                vehicle.Handbrake = false;
            }
        }

        void FixedUpdate() {
            // Check if "K" is pressed and change the target
            if (Input.GetKeyDown(KeyCode.K)) {
                // Increment the index and update the target
                SetTargetIndex(currentTargetIndex + 1);
            }

            // If we don't follow or target is null, return
            if (!follow || target == null) return;

            // Normalize velocity so it doesn't jump too far
            this.rb.velocity.Normalize();

            // Save transform locally
            Quaternion curRot = transform.rotation;
            Vector3 tPos = target.position + target.TransformDirection(offset);

            // Look at the target
            transform.LookAt(target);

            // Keep the camera above the target y position
            if (tPos.y < target.position.y) {
                tPos.y = target.position.y;
            }

            // Set transform with lerp
            transform.position = Vector3.Lerp(transform.position, tPos, Time.fixedDeltaTime * lerpPositionMultiplier);
            transform.rotation = Quaternion.Lerp(curRot, transform.rotation, Time.fixedDeltaTime * lerpRotationMultiplier);

            // Prevent camera from going underground
            if (transform.position.y < 0.5f) {
                transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
            }

            // Speedometer update
            if (speedometer != null && vehicle != null) {
                StringBuilder sb = new StringBuilder();
                sb.Append("Speed: ");
                sb.Append(((int)vehicle.Speed).ToString()); // Convert speed to int and append it
                speedometer.text = sb.ToString(); // Set the speedometer text
            }
            else if (speedometer != null && speedometer.text != "") {
                // Clear speedometer if no vehicle is being followed
                speedometer.text = "";
            }
        }
    }
}
