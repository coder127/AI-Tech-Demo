using UnityEngine;
using System.Collections;

public class CameraSelector : MonoBehaviour {

	public Transform target;

	Camera camera1;
	Camera camera2;
	Camera camera3;
	Camera camera4;

	// Use this for initialization
	void Start () {
	

		foreach (Camera cam in Camera.allCameras) {
		
			if (cam.gameObject.name == "Camera 1") {
					camera1 = cam;
			}
			if (cam.gameObject.name == "Camera 2") {
					camera2 = cam;
			}
			if (cam.gameObject.name == "Camera 3") {
					camera3 = cam;
					
			}
			if (cam.gameObject.name == "Camera 4") {
					camera4 = cam;
			}
		}
	}

	void change_camera_status(Camera cam){

		camera1.enabled = false;
		camera2.enabled = false;
		camera3.enabled = false;
		camera4.enabled = false;

		cam.enabled = true;

	}

	// Update is called once per frame
	void Update () {

		Vector3 position = target.position;

		if (position.z < -7) {

			change_camera_status(camera1);

		}

		else if (position.z > 7) {

			change_camera_status(camera3);
		}

		if (position.x < -7) {
			
			change_camera_status(camera4);

		}
		
		else if (position.x > 7) {
			
			change_camera_status(camera2);
		}





	}
}
