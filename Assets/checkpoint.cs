using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkpoint : MonoBehaviour {

	// Use this for initialization
	void OnTriggerEnter(Collider coll) {
		print ("enter trigger");
	}

	void OnTriggerExit(Collider coll) {
		print ("leaving trigger");
	}
}
