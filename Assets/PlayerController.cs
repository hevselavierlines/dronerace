/// <summary>
/// Manuel Baumgartner (c) 2017
/// Using code from the helicopter script 1 of: Saul Aceves Montes 29/12/2015
/// https://github.com/54UL/ArcadeHelicopterPhysics
/// </summary>

using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class PlayerController : MonoBehaviour {

	//Definition of the helicopter parts.
	[System.Serializable]
	public class HelicopterParts 
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
	public class HelicopterSettings
	{
		//Motor settings
		public float MotorForce;
		public Vector3 MainForceDir;//Direccion configurable, esto se debe a que no en todos los modelos tienen la misma horientacion   
		//Control force settings
		public float ThrotleSpeed;//Speed multiplier (throttle)
		public float PitchForce;
		public float RollForce;
		public float YawForce;
		//RigidBody Settings
		public Transform CenterOfMass;
		public float maxAngle;
	}
	//Enum :v
	public enum CT { Keyboard,SmartPhone };
	//Helicopter Control
	[System.Serializable]
	public class HelicopterControl 
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
	public class AudioH
	{
		public AudioSource AudioPoint;
		public AudioClip IdleMotorSound;
		public float PitchMultiplier;
		public float VolumeMultiplier;

	};

	[System.Serializable]
	public class Stabilisation 
	{
		public bool pitch;
		public bool yaw;
		public bool rotate;
		public bool throttle;
		public Vector3 lastPos;
	}

	//Variables y instancias de clases
	public bool engineOn;// Control Enabled For the helicopter ??
	public HelicopterParts Parts;
	public AudioH HelicopterSound;
	public HelicopterSettings Settings;
	public HelicopterControl Control;
	private Stabilisation stabilisation;
	private Text debugText;
	private bool landingMode;
	private bool startingMode;

	float getDistanceToGround() {
		RaycastHit hit = new RaycastHit();
		float distance = 0.0f;
		if (Physics.Raycast (transform.position, -Vector3.up, out hit)) {
			distance = hit.distance;
		}
		return distance;
	}

	//get the input for the control
	void InputControl(HelicopterControl c) 
	{
		// Up AND Down FOR THROTTLE, A & D FOR YAW , W AND S for PITCH right,left ROLL
		//throttle 
		Vector3 eulerAngles = transform.rotation.eulerAngles;

		float height = getDistanceToGround ();
		debugText.text = eulerAngles.ToString () + ", "+ height +" m";
		if (Input.GetKey (KeyCode.E) && height >= 1) {
			landingMode = true;
		}
		if (landingMode) {
			debugText.text += " LM active"; 
		} else if (startingMode) {
			debugText.text += " SM active";
		}

		if (Input.GetKey (KeyCode.W)) {
			if (eulerAngles.x < Settings.maxAngle || eulerAngles.x > 180) { 
				c.Pitch += 1 * Time.deltaTime;
			} else {
				float difference = Mathf.Abs (eulerAngles.x) - Mathf.Abs (Settings.maxAngle);
				if (difference > 5.0f) {
					c.Pitch -= Mathf.Clamp (difference, 0, 1) * Time.deltaTime;
				}
			}
			stabilisation.pitch = false;
		} else if (Input.GetKey (KeyCode.S)) {
			if (eulerAngles.x > (360 - Settings.maxAngle) || eulerAngles.x < 180) {
				c.Pitch -= 1 * Time.deltaTime;
			} else {
				float difference = Mathf.Abs (360 - Settings.maxAngle) - Mathf.Abs (eulerAngles.x);
				if (difference > 5.0f) {
					c.Pitch += Mathf.Clamp (difference, 0, 1) * Time.deltaTime;
				}
			}
			stabilisation.pitch = false;
		} else {
			stabilisation.pitch = true;
		}
		//pitch roll yaw control
		//YAW
		if (Input.GetKey (KeyCode.D)) {
			if (eulerAngles.z > (360 - Settings.maxAngle) || eulerAngles.z < 180) {
				c.Yaw -= 1 * Time.deltaTime;
			} else {
				float difference = Mathf.Abs (360 - Settings.maxAngle) - Mathf.Abs (eulerAngles.z);
				if (difference > 5.0f) {
					c.Yaw += Mathf.Clamp (difference, 0, 0.5f) * Time.deltaTime;
				}
			}
			stabilisation.yaw = false;
		} else if (Input.GetKey (KeyCode.A)) {
			if (eulerAngles.z < Settings.maxAngle || eulerAngles.z > 180) {
				c.Yaw += 1 * Time.deltaTime;
			} else {
				float difference = Mathf.Abs (eulerAngles.z) - Mathf.Abs (Settings.maxAngle);
				if (difference > 5.0f) {
					c.Yaw -= Mathf.Clamp (difference, 0, 0.5f) * Time.deltaTime;
				}
			}
			stabilisation.yaw = false;
		} else {
			c.Yaw = Mathf.Lerp (c.Yaw, 0, c.ReturnSpeed * Time.deltaTime);
			stabilisation.yaw = true;
		}
		//Pitch
		if (Input.GetKey (KeyCode.UpArrow)) {
			c.Throttle += Settings.ThrotleSpeed * Time.deltaTime;
			stabilisation.throttle = false;
		} else if (Input.GetKey (KeyCode.DownArrow)) {
			c.Throttle -= Settings.ThrotleSpeed * Time.deltaTime;
			stabilisation.throttle = false;
		} else { 
			c.Pitch = Mathf.Lerp (c.Pitch, 0, c.ReturnSpeed * Time.deltaTime);
			stabilisation.throttle = true;
		}
		//Roll
		if (Input.GetKey (KeyCode.RightArrow)) {
			c.Roll += 1 * Time.deltaTime; 
		} else if (Input.GetKey (KeyCode.LeftArrow)) {
			c.Roll -= 1 * Time.deltaTime;
		} else {
			c.Roll = Mathf.Lerp (c.Roll, 0, c.ReturnSpeed * Time.deltaTime);
		}

		if (c.ClampValues) {
			c.Roll = Mathf.Clamp (c.Roll, -1, 1);
			c.Pitch = Mathf.Clamp (c.Pitch, -1, 1);
			c.Yaw = Mathf.Clamp (c.Yaw, -1, 1);
		}
		if (landingMode) {
			c.Throttle -= Settings.ThrotleSpeed * Time.deltaTime;

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
		if (startingMode) {
			c.Throttle += Settings.ThrotleSpeed * Time.deltaTime;
			stabilisation.throttle = false;
			if (height >= 2.0f) {
				startingMode = false;
			}
		}

		Vector3 driftingDirection = transform.position - stabilisation.lastPos;
		Vector3 forwardDrifting = driftingDirection - transform.forward;
		Vector3 sideDrifting = driftingDirection - transform.right;
		//throttle (height) stabilisation
		if (stabilisation.throttle) {
			float difference = (transform.position.y - stabilisation.lastPos.y);
			if (difference < 0) {
				c.Throttle += Settings.ThrotleSpeed * Time.deltaTime;
			} else if (difference > 0) {
				c.Throttle -= Settings.ThrotleSpeed * Time.deltaTime;
			}
		}
		//Pitch (forward) stabilisation
		if (stabilisation.pitch) {
			Vector3 forward = transform.forward;
			float difference = forwardDrifting.y;
			if (difference < 0) {
				c.Pitch += Mathf.Abs(difference) * 10.0f * Mathf.Abs(difference) * Time.deltaTime;
			} else if (difference > 0) {
				c.Pitch -= Mathf.Abs(difference) * 10.0f * Mathf.Abs(difference) * Time.deltaTime;
			}
		}
		//Yaw (sideward) stabilisation
		if (stabilisation.yaw) {
			float difference = sideDrifting.y;
			if (difference < 0) {
				c.Yaw -= Mathf.Abs (difference) * 10.0f * Mathf.Abs(difference) * Time.deltaTime;
			} else if (difference > 0) {
				c.Yaw += Mathf.Abs (difference) * 10.0f * Mathf.Abs(difference) * Time.deltaTime;	
			}
		}
		stabilisation.lastPos = transform.position;

		//Generals
		if(!engineOn || startingMode || landingMode) {
			c.Throttle = Mathf.Clamp (c.Throttle, 0, 10);
		} else {
			c.Throttle = Mathf.Clamp (c.Throttle, 3, 10);
		}
	}

	void EnginesControl() 
	{
		float currentPower = (Settings.MotorForce * Control.Throttle);    // es la potencia actual currentPower, no son horse power exactos si no es mi propia unidad de medida llamada horse power :v
		float RotationPower = currentPower / (Settings.MotorForce * 10); // este es el porcentaje normalizado de la potencia del motor

		//Rotation simulation
		Parts.LeftRotor.transform.Rotate(Parts.RotationAxisRotors,Mathf.Lerp(0,currentPower,0.36f*Time.deltaTime));//Rotation
		Parts.rightRotor.transform.Rotate(Parts.RotationAxisRotors,Mathf.Lerp(0,currentPower,0.36f*Time.deltaTime));

		Parts.Chasis.GetComponent<Rigidbody> ().AddRelativeForce (Settings.MainForceDir * currentPower);

		//TODO Main Sound of the helicopter
		/*if (!HelicopterSound.AudioPoint.isPlaying) 
			HelicopterSound.AudioPoint.PlayOneShot(HelicopterSound.IdleMotorSound);

		HelicopterSound.AudioPoint.pitch = RotationPower*  HelicopterSound.PitchMultiplier + (Parts.Chasis.GetComponent<Rigidbody>().velocity.magnitude*0.014f);
		HelicopterSound.AudioPoint.volume = RotationPower * HelicopterSound.VolumeMultiplier;
		*/
		//ENDTODO

		//The transformation in the direction only works if the current power is above 20 %.
		if (RotationPower > 0.2f) {
			Parts.Chasis.transform.Rotate (new Vector3 ((Control.Pitch * RotationPower) * Settings.PitchForce, (Control.Roll * RotationPower) * Settings.RollForce, 0));
		}

		if (RotationPower > 0.2f) {
			Parts.Chasis.transform.Rotate (0, 0, (Control.Yaw * RotationPower) * Settings.YawForce);
		}
		//Debug.Log (Parts.Chasis.GetComponent<Rigidbody> ().velocity.magnitude);
		Parts.leftWing.transform.localEulerAngles = new Vector3 (Control.Pitch * 10.0f, 0, Control.Yaw * 10.0f);
		Parts.rightWing.transform.localEulerAngles = new Vector3 (Control.Pitch * 10.0f, 0, Control.Yaw * 10.0f);
	}
	//Outpouts methods 
	void Start() 
	{
		//Set the center of mass
		Parts.Chasis.GetComponent<Rigidbody>().centerOfMass = Settings.CenterOfMass.localPosition;
		stabilisation = new Stabilisation ();
		//HelicopterSound.AudioPoint.clip = HelicopterSound.IdleMotorSound;
		debugText = GameObject.Find("Canvas/Debug").GetComponent<Text>();
		debugText.text = "";
		landingMode = false;
	}
	void FixedUpdate() 
	{
		if (engineOn) {
			InputControl (Control);
		} else {
			if (Input.GetKey (KeyCode.E)) {
				engineOn = true;
				landingMode = false;
				startingMode = true;
			}
		}
		EnginesControl();
	}
}
	