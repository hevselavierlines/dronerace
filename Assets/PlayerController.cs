using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
	public GameObject rightBlade;
	public GameObject leftBlade;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		rightBlade.transform.Rotate (new Vector3 (0, 10, 0));
		leftBlade.transform.Rotate (new Vector3 (0, 10, 0));
	}
}
