using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	public Camera firstPersonCamera;
	public Camera thirdPersonCamera;
	private Rigidbody rigid;
	// Use this for initialization
	void Start () {
		ThirdPersonCamera ();

		rigid = GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void Update () {
		ReadKeyboardButtons ();

		//rigid.AddForce (new Vector3 (0.0f, 40.0f, 0.0f));
	}

	void FirstPersonCamera() {
		firstPersonCamera.gameObject.SetActive (true);
		thirdPersonCamera.gameObject.SetActive (false);
	}

	void ThirdPersonCamera() {
		firstPersonCamera.gameObject.SetActive (false);
		thirdPersonCamera.gameObject.SetActive (true);
	}

	void ReadKeyboardButtons() {
		if (Input.GetKeyDown(KeyCode.C)) {
			if (firstPersonCamera.gameObject.active) {
				ThirdPersonCamera ();
			} else {
				FirstPersonCamera ();	
			}
		}
	}
}
