//copied this shit i dont konw camera
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace VehicleBehaviour.Utils {
	public class CameraFollow : MonoBehaviour {
		[SerializeField] bool follow = false;
		public bool Follow
		{
			get => follow;
			set => follow = value;
		}

		[SerializeField] Transform target = default;

		[SerializeField] Transform[] targets = new Transform[0];

		[SerializeField] Vector3 offset = -Vector3.forward;

		[Range(0, 10)]
		[SerializeField] float lerpPositionMultiplier = 1f;
		[Range(0, 10)]		
		[SerializeField] float lerpRotationMultiplier = 1f;

		// Speedometer
		[SerializeField] Text speedometer = null;

		Rigidbody rb;
		Rigidbody targetRb;

		WheelVehicle vehicle;

		void Start () {
			rb = GetComponent<Rigidbody>();
		}

		public void SetTargetIndex(int i) {
			WheelVehicle v;

			foreach(Transform t in targets)
			{
				v = t != null ? t.GetComponent<WheelVehicle>() : null;
				if (v != null)
				{
					v.IsPlayer = false;
					v.Handbrake = true;
				}
			}

			target = targets[i % targets.Length];

			vehicle = target != null ? target.GetComponent<WheelVehicle>() : null;
			if (vehicle != null)
			{
				vehicle.IsPlayer = true;
				vehicle.Handbrake = false;
			}
		}

		void FixedUpdate() {
			// If we don't follow or target is null return
			if (!follow || target == null) return;

			// normalise velocity so it doesn't jump too far
			this.rb.velocity.Normalize();

			// Save transform localy
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

			// Keep camera above the y:0.5f to prevent camera going underground
			if (transform.position.y < 0.5f) {
				transform.position = new Vector3(transform.position.x , 0.5f, transform.position.z);
			}

			// Update speedometer
			// if (speedometer != null && vehicle != null)
			// {
			// 	StringBuilder sb = new StringBuilder();
			// 	sb.Append("Speed:");
			// 	sb.Append(((int) (vehicle.Speed)).ToString());
			// 	sb.Append(" Kph");

			// 	speedometer.text = sb.ToString();
			// }
			// else if (speedometer.text != "")
			// {
			// 	speedometer.text = "";
			// }
			
		}
	}
}
