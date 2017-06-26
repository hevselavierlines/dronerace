/// <summary>
/// Manuel Baumgartner (c) 2017
/// Using code from the helicopter script 1 of: Saul Aceves Montes 29/12/2015
/// https://github.com/54UL/ArcadeHelicopterPhysics
/// </summary>

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/**
 * Controller for the drone rigidbody and the stabilisation.
 **/
public class PlayerController : MonoBehaviour {

	//Definition of the helicopter parts.
	[System.Serializable]
	public class DroneParts 
	{
		//leftRotor
		public Vector3 RotationAxisRotors;
		public GameObject LeftRotor;
		//rightRotor
		public GameObject rightRotor;

		public GameObject leftWing;
		public GameObject rightWing;
		//Chasis and others
		//public float ChasisDamage;
		public GameObject Chasis;
	}
	[System.Serializable]
	public class DroneSettings
	{
		//Motor settings
		public float MotorForce;
		public Vector3 MainForceDir;//Direccion configurable, esto se debe a que no en todos los modelos tienen la misma horientacion   
		//Control force settings
		public float ThrottleSpeed;//Speed multiplier (throttle)
		public float PitchForce;
		public float RollForce;
		public float YawForce;
		//RigidBody Settings
		public Transform CenterOfMass;
		public float maxAngle;
		public int stabilisationWait;
	}
	//Controller enum. Smartphone control will be available later
	public enum CT { Keyboard,SmartPhone };
	//Helicopter Control
	[System.Serializable]
	public class DroneControl 
	{
		public CT ControlType=CT.Keyboard;
		// Control options
		public float Throttle;// Force Multiplier
		// Movement
		public float Roll;
		public float Pitch;
		public float Yaw;
		[HideInInspector]
		public float ReturnSpeed=1;
		[HideInInspector]
		public bool ClampValues = true;
	}

	//Audio
	[System.Serializable]
	public class AudioSettings
	{
		public float PitchMultiplier;
		public float VolumeMultiplier;

	};

	[System.Serializable]
	public class Stabilisation 
	{
		public int pitch;
		public int yaw;
		public bool rotate;
		public bool throttle;
		public Vector3 lastPos;
	}

	//Variables y instancias de clases
	public bool engineOn;// Control Enabled For the helicopter ??
	public DroneParts Parts;
	public DroneSettings Settings;
	public DroneControl Control;
	private Stabilisation stabilisation;
	private Text debugText;
	private bool landingMode;
	private bool startingMode;
	private Rigidbody rigidBody;
	public AudioSettings audioSettings;
	private AudioSource audioSource;
	private float height;

	float getDistanceToGround() {
		RaycastHit hit = new RaycastHit();
		float distance = 0.0f;
		if (Physics.Raycast (transform.position, -Vector3.up, out hit)) {
			distance = hit.distance;
		}
		return distance;
	}

	//get the input for the control
	void InputControl(DroneControl c) 
	{
		// Up AND Down FOR THROTTLE, A & D FOR YAW , W AND S for PITCH right,left ROLL
		//throttle 
		Vector3 eulerAngles = transform.rotation.eulerAngles;

		height = getDistanceToGround ();
		if ((Input.GetKey (KeyCode.E) || Input.GetKey (KeyCode.L)) && height >= 1) {
			landingMode = true;
		}
		//PITCH
			//W sets the pitch forward.
		if (Input.GetKey (KeyCode.W)) {
			if (eulerAngles.x < Settings.maxAngle || eulerAngles.x > 180) { 
				c.Pitch += 1 * Time.deltaTime;
			} else {
				float difference = Mathf.Abs (eulerAngles.x) - Mathf.Abs (Settings.maxAngle);
				if (difference > 5.0f) {
					c.Pitch -= Mathf.Clamp (difference, 0, 1) * Time.deltaTime;
				}
			}
			stabilisation.pitch = 0;
			//S sets the pitch backward.
		} else if (Input.GetKey (KeyCode.S)) {
			if (eulerAngles.x > (360 - Settings.maxAngle) || eulerAngles.x < 180) {
				c.Pitch -= 1 * Time.deltaTime;
			} else {
				float difference = Mathf.Abs (360 - Settings.maxAngle) - Mathf.Abs (eulerAngles.x);
				if (difference > 5.0f) {
					c.Pitch += Mathf.Clamp (difference, 0, 1) * Time.deltaTime;
				}
			}
			stabilisation.pitch = 0;
		} else {
			//no button press means that stabilisation will be activated for pitch.
			stabilisation.pitch += 1;
		}
		//YAW
			//D sets the yaw to the right
		if (Input.GetKey (KeyCode.D)) {
			if (eulerAngles.z > (360 - Settings.maxAngle) || eulerAngles.z < 180) {
				c.Yaw -= 1 * Time.deltaTime;
			} else {
				float difference = Mathf.Abs (360 - Settings.maxAngle) - Mathf.Abs (eulerAngles.z);
				if (difference > 5.0f) {
					c.Yaw += Mathf.Clamp (difference, 0, 0.5f) * Time.deltaTime;
				}
			}
			stabilisation.yaw = 0;
			//A sets the yaw to the left
		} else if (Input.GetKey (KeyCode.A)) {
			if (eulerAngles.z < Settings.maxAngle || eulerAngles.z > 180) {
				c.Yaw += 1 * Time.deltaTime;
			} else {
				float difference = Mathf.Abs (eulerAngles.z) - Mathf.Abs (Settings.maxAngle);
				if (difference > 5.0f) {
					c.Yaw -= Mathf.Clamp (difference, 0, 0.5f) * Time.deltaTime;
				}
			}
			stabilisation.yaw = 0;
		} else {
			//No A or D press activates the yaw stabilisation.
			c.Yaw = Mathf.Lerp (c.Yaw, 0, c.ReturnSpeed * Time.deltaTime);
			stabilisation.yaw += 1;
		}
		//HEIGHT
			//With increasing the throttle speed the drone is flying up.
		if (Input.GetKey (KeyCode.UpArrow)) {
			c.Throttle += Settings.ThrottleSpeed * Time.deltaTime;
			stabilisation.throttle = false;
			//With decreasing the throttle speed the drone is flying down.
		} else if (Input.GetKey (KeyCode.DownArrow)) {
			c.Throttle -= Settings.ThrottleSpeed * Time.deltaTime;
			stabilisation.throttle = false;
		} else { 
			c.Pitch = Mathf.Lerp (c.Pitch, 0, c.ReturnSpeed * Time.deltaTime);
			stabilisation.throttle = true;
		}
		//ROLL
			//Rotating to right with right arrow
		if (Input.GetKey (KeyCode.RightArrow)) {
			c.Roll += 1 * Time.deltaTime; 
			//Rotating to left with left arrow
		} else if (Input.GetKey (KeyCode.LeftArrow)) {
			c.Roll -= 1 * Time.deltaTime;
		} else {
			c.Roll = Mathf.Lerp (c.Roll, 0, c.ReturnSpeed * Time.deltaTime);
		}
		//All values must be between -1 and 1 for the control.
		if (c.ClampValues) {
			c.Roll = Mathf.Clamp (c.Roll, -1, 1);
			c.Pitch = Mathf.Clamp (c.Pitch, -1, 1);
			c.Yaw = Mathf.Clamp (c.Yaw, -1, 1);
		}
		//Landing mode reduces engine power (minimum 2.5).
		if (landingMode) {
			c.Throttle -= Settings.ThrottleSpeed * Time.deltaTime;

			stabilisation.throttle = false;
			if (height < 0.6f) {
				if (c.Throttle <= 0.0f) {
					c.Throttle = 0.0f;
					landingMode = false;
					engineOn = false;
				}
			} else {
				if (c.Throttle <= 2.5f) {
					c.Throttle = 2.5f;
				}
			}
		}
		//starting mode increases the engine power.
		if (startingMode) {
			c.Throttle += Settings.ThrottleSpeed * Time.deltaTime;
			stabilisation.throttle = false;
			if (height >= 2.0f) {
				startingMode = false;
			}
		}
		//Emergency landing (switch off engine immediately)
		if (Input.GetKey (KeyCode.N)) {
			engineOn = false;
			c.Throttle = 0f;
		}

		Vector3 driftingDirection = transform.position - stabilisation.lastPos;
		Vector3 forwardDrifting = driftingDirection - transform.forward;
		Vector3 sideDrifting = driftingDirection - transform.right;
		//throttle (height) stabilisation
		if (stabilisation.throttle) {
			float difference = (transform.position.y - stabilisation.lastPos.y);
			if (difference < 0) {
				c.Throttle += Settings.ThrottleSpeed * Time.deltaTime;
			} else if (difference > 0) {
				c.Throttle -= Settings.ThrottleSpeed * Time.deltaTime;
			}
		}
		//Pitch (forward) stabilisation
		if (stabilisation.pitch > Settings.stabilisationWait) {
			Vector3 forward = transform.forward;
			float difference = forwardDrifting.y;
			if (difference < 0) {
				c.Pitch += Mathf.Abs(difference) * 10.0f * Mathf.Abs(difference) * Time.deltaTime;
			} else if (difference > 0) {
				c.Pitch -= Mathf.Abs(difference) * 10.0f * Mathf.Abs(difference) * Time.deltaTime;
			}
		}
		//Yaw (sideward) stabilisation
		if (stabilisation.yaw > Settings.stabilisationWait) {
			float difference = sideDrifting.y;
			if (difference < 0) {
				c.Yaw -= Mathf.Abs (difference) * 10.0f * Mathf.Abs(difference) * Time.deltaTime;
			} else if (difference > 0) {
				c.Yaw += Mathf.Abs (difference) * 10.0f * Mathf.Abs(difference) * Time.deltaTime;	
			}
		}
		stabilisation.lastPos = transform.position;

		/*Setting the throttle between these 3 and 10 except for the 
		/*landing, starting and when the engine is switched off.*/
		if(!engineOn || startingMode || landingMode) {
			c.Throttle = Mathf.Clamp (c.Throttle, 0, 10);
		} else {
			c.Throttle = Mathf.Clamp (c.Throttle, 3, 10);
		}
	}

	void EnginesControl() 
	{
		float currentPower = (Settings.MotorForce * Control.Throttle);    // Calculating the current power for the rigidbody relative force
		float RotationPower = currentPower / (Settings.MotorForce * 10); // Rotation amount for the rotors.

		//Rotation simulation
		Parts.LeftRotor.transform.Rotate(Parts.RotationAxisRotors,Mathf.Lerp(0,currentPower,0.36f*Time.deltaTime));//Rotation for left rotor
		Parts.rightRotor.transform.Rotate(Parts.RotationAxisRotors,Mathf.Lerp(0,currentPower,0.36f*Time.deltaTime));//Rotation for right rotor

		rigidBody.AddRelativeForce (Settings.MainForceDir * currentPower); //Adding the relative force to the rigid body.

		//TODO Main Sound of the helicopter
		//if (!HelicopterSound.AudioPoint.isPlaying) 
		//	HelicopterSound.AudioPoint.PlayOneShot(HelicopterSound.IdleMotorSound);
		audioSource.pitch = RotationPower * audioSettings.PitchMultiplier + (Parts.Chasis.GetComponent<Rigidbody>().velocity.magnitude*0.02f);
		audioSource.volume = RotationPower * audioSettings.VolumeMultiplier;

		//ENDTODO

		//The transformation in the direction only works if the current power is above 20 %.
		if (RotationPower > 0.2f) {
			Parts.Chasis.transform.Rotate (new Vector3 ((Control.Pitch * RotationPower) * Settings.PitchForce, (Control.Roll * RotationPower) * Settings.RollForce, 0));
		}

		if (RotationPower > 0.2f) {
			Parts.Chasis.transform.Rotate (0, 0, (Control.Yaw * RotationPower) * Settings.YawForce);
		}
		//Debug.Log (Parts.Chasis.GetComponent<Rigidbody> ().velocity.magnitude);
		//Local rotation for the wings (holder of the rotors).
		Parts.leftWing.transform.localEulerAngles = new Vector3 (Control.Pitch * 10.0f, 0, Control.Yaw * 10.0f);
		Parts.rightWing.transform.localEulerAngles = new Vector3 (Control.Pitch * 10.0f, 0, Control.Yaw * 10.0f);
	}
	//Outpouts methods 
	void Start() 
	{
		rigidBody = Parts.Chasis.GetComponent<Rigidbody> ();
		//Set the center of mass
		rigidBody.centerOfMass = Settings.CenterOfMass.localPosition;
		stabilisation = new Stabilisation ();
		//HelicopterSound.AudioPoint.clip = HelicopterSound.IdleMotorSound;
		debugText = GameObject.Find("Canvas/DebugText").GetComponent<Text>();
		if (debugText != null) {
			debugText.text = "";
		}
		landingMode = false;
		audioSource = GetComponent<AudioSource> ();
	}
	void FixedUpdate() 
	{
		if (engineOn) {
			InputControl (Control);
		} else {
			//E button or up arrow starts the engine
			if (Input.GetKey (KeyCode.E) || (!engineOn && Input.GetKey(KeyCode.UpArrow))) {
				engineOn = true;
				landingMode = false;
				startingMode = true;
			}
		}
		//the engine control does the rigid body control.
		EnginesControl();
		if (Input.GetKey (KeyCode.Escape)) {
			Application.Quit ();
		}

		PrintText ();
	}

	void PrintText() {
		if (debugText != null) {
			debugText.text = height.ToString("F3") + " m\t";
			debugText.text += (rigidBody.velocity.magnitude * 3.6f).ToString("F1") + " km/h";
			if (landingMode) {
				debugText.text += "\nLanding"; 
			} else if (startingMode) {
				debugText.text += "\nStarting";
			}
		}
	}
}
	