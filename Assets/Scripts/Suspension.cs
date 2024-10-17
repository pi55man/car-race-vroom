using UnityEngine;
using UnityEngine.Serialization;

namespace VehicleBehaviour {
    [RequireComponent(typeof(WheelCollider))]


    public class Suspension : MonoBehaviour {
        // Don't follow steer angle (used by tanks)
        public bool cancelSteerAngle = false;
        [FormerlySerializedAs("_wheelModel")]
        public GameObject wheelModel;
        private WheelCollider wheelCollider;

        public Vector3 localRotOffset;

        private float lastUpdate;

        void Start()
        {
            lastUpdate = Time.realtimeSinceStartup;

            wheelCollider = GetComponent<WheelCollider>();
        }
        
        void FixedUpdate()
        {
            // We don't really need to do this update every time, keep it at a maximum of 60FPS
            if (Time.realtimeSinceStartup - lastUpdate < 1f/60f)
            {
                return;
            }
            lastUpdate = Time.realtimeSinceStartup;

            if (wheelModel && wheelCollider)
            {
                Vector3 pos = new Vector3(0, 0, 0);
                Quaternion quat = new Quaternion();
                wheelCollider.GetWorldPose(out pos, out quat);

                wheelModel.transform.rotation = quat;
                if (cancelSteerAngle)
                    wheelModel.transform.rotation = transform.parent.rotation;

                wheelModel.transform.localRotation *= Quaternion.Euler(localRotOffset);
                wheelModel.transform.position = pos;

                WheelHit wheelHit;
                wheelCollider.GetGroundHit(out wheelHit);
            }
        }
    }
}
