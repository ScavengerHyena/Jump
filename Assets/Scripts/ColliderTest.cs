using UnityEngine;
using System.Collections;

public class ColliderTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		BoxCollider trigger1;
		trigger1 = gameObject.AddComponent<BoxCollider>();
		trigger1.isTrigger = true;

		trigger1.size = new Vector3(1.5f, 0.1f, 1.5f);
		trigger1.center += new Vector3(0f, 0.45f, 0f);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider collider){
		Debug.Log("Collision for ledge grab.");
		collider.gameObject.SendMessage("LedgeGrab");
	}
}
