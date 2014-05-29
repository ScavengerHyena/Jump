using UnityEngine;
using System.Collections;

public class UnitPlayer : Unit
{
	float cameraRotX = 0f;
	
	public float cameraPitchMax = 45f;
	
	// Use this for initialization
	public override void Start ()
	{
		base.Start ();
	}
	
	// Update is called once per frame
	public override void Update ()
	{
		// rotation
		
		transform.Rotate (0f, Input.GetAxis ("Mouse X") * turnSpeed * Time.deltaTime, 0f);
		
		cameraRotX -= Input.GetAxis ("Mouse Y");
		
		cameraRotX = Mathf.Clamp (cameraRotX, -cameraPitchMax, cameraPitchMax);
		
		Camera.main.transform.forward = transform.forward;
		Camera.main.transform.Rotate (cameraRotX, 0f, 0f);
		
		// movement
		
		move = new Vector3(Input.GetAxis ("Horizontal"), 0f, Input.GetAxis ("Vertical"));
		
		move.Normalize();
		
		move = transform.TransformDirection (move);
		
		base.Update ();
	}
}
