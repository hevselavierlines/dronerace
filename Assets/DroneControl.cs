using UnityEngine;
using System.Collections;

public class DroneControl : MonoBehaviour {
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
		if (Input.GetKeyDown(KeyCode.V)) {
			if (firstPersonCamera.gameObject.active) {
				ThirdPersonCamera ();
			} else {
				FirstPersonCamera ();	
			}
		}
		if (Input.GetKey (KeyCode.UpArrow)) {
			print ("up");
			rigid.AddForce (0.0f, 2.0f, 0.0f);
		} else if (Input.GetKey (KeyCode.DownArrow)) {
			rigid.AddForce (0.0f, -1.0f, 0.0f);
		} else if (Input.GetKey (KeyCode.W)) {
			rigid.AddForce (1.0f, 0.0f, 0.0f);
		} else if (Input.GetKey (KeyCode.S)) {
			rigid.AddForce (-1.0f, 0.0f, 0.0f);
		}

		rigid.AddForce (0.0f, 8.5f, 0.0f);

	}
}
