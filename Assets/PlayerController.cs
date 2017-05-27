/// <summary>
/// Saul Aceves Montes 29/12/2015
/// Helicopter Script 1.0 
/// 
/// -LICENCIA (ZLIB)
/// 
/// Derechos de autor (c) <2015> <Saul Aceves>
/// Este software se proporciona "tal cual", sin ninguna garantía expresa o implícita. En ningún caso los autores ser declarados responsables de los daños y perjuicios derivados de la utilización de este software.
/// Se concede permiso para que cualquiera pueda utilizar este software para cualquier propósito, incluyendo aplicaciones comerciales, y para alterarlo y redistribuirlo libremente, sujeto a las siguientes restricciones:
/// 1. El origen de este software no debe ser tergiversado; no hay que decir que usted escribió el software original. Si utiliza este software en un producto, un reconocimiento en la documentación del producto sería apreciada pero no se requiere.
//  2. Las versiones alteradas de la fuente deben estar claramente identificados como tales, y no deben ser tergiversados ​​como el software original.
//  3. Este aviso no puede ser suprimida o alterada de cualquier distribución de código fuente.
/// 
/// -Contacto
/// 54ulxd@gmail.com 
/// </summary>

using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class PlayerController : MonoBehaviour {

	//Definiciones de clases 
	[System.Serializable]
	public class HelicopterParts 
	{
		//Helices
		//Main
		public Vector3 RotationAxisMainRotor;
		public GameObject MainRotor;
		//Tail
		public Vector3 RotationAxisTailRotor;
		public GameObject TailRotor;

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
	public enum CT { Keyboard,JoyStick,KeyBoardAndMouse};
	//Helicopter Control
	[System.Serializable]
	public class HC  
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
	public HC Control;
	private Stabilisation stabilisation;
	private Text debugText;

	//obtenemos todo el input necesairo
	void InputControl(HC c) 
	{
		switch (c.ControlType) {
		//KeyBoard Control
		case CT.Keyboard:
			// W AND S FOR THROTTLE, A & D FOR YAW , ARROWS FOR PITCH(up, down) and ROLL(right,left) 
			//throttle 
			Vector3 eulerAngles = transform.rotation.eulerAngles;
			debugText.text = eulerAngles.ToString ();
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



			Vector3 driftingDirection = transform.position - stabilisation.lastPos;
			Vector3 forwardDrifting = driftingDirection - transform.forward;
			Vector3 sideDrifting = driftingDirection - transform.right;

			if (stabilisation.throttle) {
				float difference = (transform.position.y - stabilisation.lastPos.y);
				if (difference < 0) {
					c.Throttle += Settings.ThrotleSpeed * Time.deltaTime;
				} else if (difference > 0) {
					c.Throttle -= Settings.ThrotleSpeed * Time.deltaTime;
				}
			}
			if (stabilisation.pitch) {
				Vector3 forward = transform.forward;
				float difference = forwardDrifting.y;
				if (difference < 0) {
					c.Pitch += Mathf.Abs(difference) * 10.0f * Mathf.Abs(difference) * Time.deltaTime;
				} else if (difference > 0) {
					c.Pitch -= Mathf.Abs(difference) * 10.0f * Mathf.Abs(difference) * Time.deltaTime;
				}
			}

			if (stabilisation.yaw) {
				float difference = sideDrifting.y;
				if (difference < 0) {
					c.Yaw -= Mathf.Abs (difference) * 10.0f * Mathf.Abs(difference) * Time.deltaTime;
				} else if (difference > 0) {
					c.Yaw += Mathf.Abs (difference) * 10.0f * Mathf.Abs(difference) * Time.deltaTime;	
				}
			}
			stabilisation.lastPos = transform.position;
			break;
		}
		//Generals
		c.Throttle = Mathf.Clamp(c.Throttle, 3, 10);

	}
	//Control del motor y de mecanica extra c: 
	void Hmotor() 
	{
		// MEJORAR LA MECANICA DEL MOTOR Y ANADIR COSAS NUEVAS C:

		//"Set the horse power", realmente esto no son caballos de fuerza... pero no encontre otro nombre cual ponerle
		float HP = (Settings.MotorForce *Control.Throttle);    // es la potencia actual HP, no son horse power exactos si no es mi propia unidad de medida llamada horse power :v
		float RotationPower = HP / (Settings.MotorForce * 10); // este es el porcentaje normalizado de la potencia del motor
		// Debug.Log(HP);


		//-----------------------------------------------------------Main rotor
		//aplicamos la fuerza sobre el chasis y verificamos los danos del rotor 
		//if (Parts.MainRotor.GetComponent<RotorDamage>().CurrentDamage < 100)
		{
			//la parte del pitch, roll y el trhottle lo llevara el rotor principal
			//Simulacion de la rotacion
			Parts.MainRotor.transform.Rotate(Parts.RotationAxisMainRotor,Mathf.Lerp(0,HP,0.36f*Time.deltaTime));//Rotation
			Parts.TailRotor.transform.Rotate(Parts.RotationAxisMainRotor,Mathf.Lerp(0,HP,0.36f*Time.deltaTime));
			//damos la fuerza principal del helicoptero 
			//Parts.Chasis.GetComponent<Rigidbody>().AddRelativeForce(Settings.MainForceDir* HP);
			Parts.Chasis.GetComponent<Rigidbody> ().AddRelativeForce (Settings.MainForceDir * HP);

			//Main Sound
			/*if (!HelicopterSound.AudioPoint.isPlaying) 
				HelicopterSound.AudioPoint.PlayOneShot(HelicopterSound.IdleMotorSound);

			HelicopterSound.AudioPoint.pitch = RotationPower*  HelicopterSound.PitchMultiplier + (Parts.Chasis.GetComponent<Rigidbody>().velocity.magnitude*0.014f);
			HelicopterSound.AudioPoint.volume = RotationPower * HelicopterSound.VolumeMultiplier;
			*/

			//Asignamos la funcionalidad del rotor primario solamente si la potencia es del 20%
			if (RotationPower > 0.2f) {
				Parts.Chasis.transform.Rotate (new Vector3 ((Control.Pitch * RotationPower) * Settings.PitchForce, (Control.Roll * RotationPower) * Settings.RollForce, 0));
			}
		}
		//else //decuple part 
		//{
		//	Parts.MainRotor.transform.parent = null;
		//	Parts.MainRotor.GetComponent<Rigidbody>().isKinematic = false;
		//	Parts.Chasis.GetComponent<Rigidbody>().AddTorque(0, HP / .10f * Time.deltaTime, 0);
		//}

		//-------------------------------------------------------Tail Rotor
		//if (Parts.TailRotor.GetComponent<RotorDamage>().CurrentDamage < 100)
		{
			// el rotor secundario es el YAW,
			//Simulacion de la rotacion

			//Parts.TailRotor.transform.Rotate(Parts.RotationAxisTailRotor, Mathf.Lerp(0, (HP / 0.5f),0.36f*Time.deltaTime));//Rotation
			//Asignamos la funcionalidad del rotor secundario solamente si la potencia es del 10 % (puede variar este porcentaje pero no debe ser mayor al del principal)
			if(RotationPower > 0.1f)
				Parts.Chasis.transform.Rotate(0, 0, (Control.Yaw*RotationPower)*Settings.YawForce);
		}
		/*else //decuple part 
		{
			Parts.TailRotor.transform.parent = null;
			Parts.TailRotor.GetComponent<Rigidbody>().isKinematic = false;
			Parts.Chasis.GetComponent<Rigidbody>().AddTorque(0, HP / .30f * Time.deltaTime, 0);
		}*/
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
		debugText.text = "JONAS";
	}
	void FixedUpdate() 
	{
		if (engineOn) {
			InputControl (Control);
		}
		Hmotor();
	}

}

