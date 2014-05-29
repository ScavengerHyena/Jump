using UnityEngine;
using System.Collections;

public class Landed : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider other){
		renderer.material.color = Color.green;
	}

	void OnTriggerExit() {
		renderer.material.color = Color.red;
	}
}
