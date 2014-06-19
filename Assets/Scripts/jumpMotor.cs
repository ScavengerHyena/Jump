using UnityEngine;
using System.Collections;

// <author> Paige Hicks </author>
// <summary>Class controlling movement for Jump game</summary>

[RequireComponent(typeof(CharacterController))]

public class jumpMotor : MonoBehaviour {

	private MotorStates motorState = MotorStates.Default;

	public float BaseSpeed = 4.0f;
	public float RunSpeedIncrease = 4.0f;

	public float RampUpTime = 0.75f;
	private bool moveKeyDown = false;
	private float moveDownTime = 0f;
	private float friction = 15f;

	public float TurnSpeed = 90.0f;
	public float JumpSpeed = 8.0f;
	public float Gravity = 20.0f;
	public float MouseSensitivity = 2.5f;

	//private bool jumpKeyDown = false;

	public Camera camera;
	private float cameraRotX = 0.0f;

	private CharacterController controller;

	private bool canWallRun = true;

	private Vector3 moveDirection = Vector3.zero;
	private Vector3 lastDirection = Vector3.zero;

	private float wallRunMaxTime = 1.5f;
	private float wallRunTime = 0.0f;
	private RaycastHit wallHit;

	private bool canGrabLedge = true;

	float climbTime = 0.0f;
	bool canClimb = true;


	// Use this for initialization
	void Start () {
		camera = Camera.main;
		controller = GetComponent<CharacterController>();
		controller.detectCollisions = true;
	}
	
	// Update is called once per frame
	void Update () {
		// Get inputs?

		switch(motorState) {

		case(MotorStates.Climbing):
			UpdateWallClimb();
			break;
		case(MotorStates.Jumping):
			UpdateJump();
			break;
		case(MotorStates.Ledgegrabbing):
			UpdateLedgeGrab();
			break;
		case(MotorStates.MusclingUp):
			MuscleUp();
			break;
		case(MotorStates.Wallrunning):
			UpdateWallRun();
			break;
		default:
			UpdateDefault();
			break;
		}

		controller.Move(moveDirection * Time.deltaTime);
		lastDirection = moveDirection;
	}

	// Update camera and rotation based on mouse movent
	void StandardCameraUpdate(){
		transform.Rotate (0f, (Input.GetAxis("Mouse X") * MouseSensitivity) * TurnSpeed * Time.deltaTime, 0f);
		cameraRotX -= Input.GetAxis("Mouse Y") * MouseSensitivity;

		camera.transform.forward = transform.forward;

		camera.transform.Rotate(cameraRotX, 0f, 0f);
	}

	// Default movement update for when someone's just on the ground, running and such.
	void UpdateDefault() {

		// Update camera and general house keeping.
		StandardCameraUpdate();

		// Update momentum amount based on continuous run time.
		moveKeyDown = Input.GetKey(KeyCode.W);
		if(moveKeyDown && moveDownTime <= RampUpTime) {
			moveDownTime += Time.deltaTime;
			if (moveDownTime > RampUpTime){
				moveDownTime = RampUpTime;
			}
		}

		if (controller.isGrounded){

			// Stop  momentum only if grounded. Can't slow down while airborne.
			if (!moveKeyDown){
				moveDownTime = 0f;
			}

			// Make sure some things get reset now that we've touched down and can recover.
			canClimb = true;
			canWallRun = true;
			canGrabLedge = true;

			// Update move direction with standard forward, back, and strafe controls.
			moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
			moveDirection = transform.TransformDirection(moveDirection);
			moveDirection.Normalize();
			
			moveDirection *= BaseSpeed + (RunSpeedIncrease * (moveDownTime / RampUpTime));

			// slow to a stop if no input
			if ((moveDirection == Vector3.zero && lastDirection != Vector3.zero)) {
				if (lastDirection.x != 0){
					moveDirection.x = DoSlowDown(lastDirection.x);
				}
				
				if (lastDirection.z != 0){
					moveDirection.z = DoSlowDown(lastDirection.z);
				}
			}

			// Debating using a controller function or class to handle this, but for now...
			if (Input.GetButton("Jump")){
				motorState = MotorStates.Jumping;
				moveDirection.y = JumpSpeed;
			}
		}

		// Keeping this here right now in case I don't feel the need for a falling function.
		moveDirection.y -= Gravity * Time.deltaTime;
	}

	float DoSlowDown(float lastVelocity){
		if (lastVelocity > 0){
			lastVelocity -= friction * Time.deltaTime;
			if (lastVelocity < 0)
				lastVelocity = 0;
		}
		else{
			lastVelocity += friction * Time.deltaTime;
			if (lastVelocity > 0)
				lastVelocity = 0;
		}
		return lastVelocity;
	}

	// UpdateJump updates the gravity, but more importantly it checks if the player is able to do specific,
	// vertical movement related actions such as Wall Running, or Wall Climbing.
	void UpdateJump() {
		StandardCameraUpdate();

		// Do a wall run check and change state if successful.
		wallHit = DoWallRunCheck();
		if (wallHit.collider != null) {
			motorState = MotorStates.Wallrunning;
			return;
		}

		// Do a wall climb check and I need to clean up these hits.
		RaycastHit hit = DoWallClimbCheck(new Ray(transform.position, 
		                                          transform.TransformDirection(Vector3.forward).normalized * 0.1f));
		if (hit.collider != null) {
			motorState = MotorStates.Climbing;
			return;
		}

		// Now, if we've actually gotten this far...Sheesh.
		// Update gravity, since there's no other movement direction to worry about
		moveDirection.y -= Gravity * Time.deltaTime;

		if (controller.isGrounded){
			motorState = MotorStates.Default;
		}
	}

	void UpdateWallRun (){

		if (!controller.isGrounded && canWallRun && wallRunTime < wallRunMaxTime){

			// Always update the wallhit, because we run past the edge of a wall. This keeps us 
			// from floating off in to the ether.
			wallHit = DoWallRunCheck();
			if (wallHit.collider == null){
				StopWallRun();
				return;
			}

			motorState = MotorStates.Wallrunning;
			float previousJumpHeight = moveDirection.y;

			Vector3 crossProduct = Vector3.Cross(Vector3.up, wallHit.normal);

			Quaternion lookDirection = Quaternion.LookRotation(crossProduct);
			transform.rotation = Quaternion.Slerp(transform.rotation, lookDirection, 3.5f * Time.deltaTime);

			//camera.transform.Rotate(new Vector3(0f,0f,20f * Time.deltaTime));

			moveDirection = crossProduct;
			moveDirection.Normalize();
			moveDirection *= BaseSpeed + (RunSpeedIncrease * (moveDownTime / RampUpTime));

			if (wallRunTime == 0.0f){
				// increase vertical movement.
				moveDirection.y = JumpSpeed / 4;
			}
			else {
				moveDirection.y = previousJumpHeight;
				moveDirection.y -= (Gravity / 4) * Time.deltaTime;
			}

			wallRunTime += Time.deltaTime;
			//Debug.Log("Wall run time: " + wallRunTime);

			if (wallRunTime > wallRunMaxTime){
				canWallRun = false;
				Debug.Log ("Max wall run time hit.");
			}

		}
		else {
			StopWallRun();
		}
	}

	void StopWallRun(){
		if (motorState == MotorStates.Wallrunning)
			canWallRun = false;

		wallRunTime = 0.0f;
		motorState = MotorStates.Default;
	}

	// Does a raycast to check if a wall was hit on either side, then checks if the angle between
	// the forward vector and the wall's normal is appropriate for a wall run (ie: don't wall run 
	// if facing away), then returns the closest, properly angled impact (if there are two somehow)
	RaycastHit DoWallRunCheck(){
		Ray rayRight = new Ray(transform.position, transform.TransformDirection(Vector3.right));
		Ray rayLeft = new Ray(transform.position, transform.TransformDirection(Vector3.left));

		RaycastHit wallImpactRight;
		RaycastHit wallImpactLeft;

		bool rightImpact = Physics.Raycast(rayRight.origin, rayRight.direction, out wallImpactRight, 1f);
		bool leftImpact = Physics.Raycast(rayLeft.origin, rayLeft.direction, out wallImpactLeft, 1f);

		if (rightImpact && Vector3.Angle(transform.TransformDirection(Vector3.forward), wallImpactRight.normal) > 90) {
			return wallImpactRight;
		}
		else if (leftImpact && Vector3.Angle(transform.TransformDirection(Vector3.forward), wallImpactLeft.normal) > 90) {
			wallImpactLeft.normal *= -1;
			return wallImpactLeft;
		}
		else {
			// Just return something empty, because nothing is good for a wall run
			return new RaycastHit();
		}
	} 

	// Does a raycast forward to check for a wall. If found, it looks up and climbs it.
	void UpdateWallClimb() {
		if (!moveKeyDown) {
			climbTime = 0.0f;
			if (motorState == MotorStates.Climbing)
				canClimb = false;
			motorState = MotorStates.Default;
			return;
		}

		Ray forwardRay = new Ray(transform.position, transform.TransformDirection(Vector3.forward).normalized);
		forwardRay.direction *= 0.1f;

		RaycastHit hit = DoWallClimbCheck(forwardRay);
		if (canClimb && hit.collider != null && 
		    climbTime < 0.5f && Vector3.Angle(forwardRay.direction, hit.normal) > 165){

			climbTime += Time.deltaTime;

			// Look up. Disabled for now.
			Quaternion lookDirection = Quaternion.LookRotation(hit.normal * -1);
			transform.rotation = Quaternion.Slerp(transform.rotation, lookDirection, 3.5f * Time.deltaTime);
			//camera.transform.Rotate(-85f * (climbTime / 0.5f), 0f, 0f); //            ^ Magic number for tweaking look time

			// Move up.
			moveDirection += transform.TransformDirection(Vector3.up);
			moveDirection.Normalize();
			moveDirection *= BaseSpeed;

			motorState = MotorStates.Climbing;
		}
		else {
			if (motorState == MotorStates.Climbing)
				canClimb = false;
			climbTime = 0f;
			motorState = MotorStates.Default;
		}
	}

	RaycastHit DoWallClimbCheck(Ray forwardRay){
		RaycastHit hit;

		Physics.Raycast(forwardRay.origin, forwardRay.direction, out hit, 1f);

		return hit;
	}

	// Activates ledge grab
	// TODO: Requires user to be jumping or climbing, should allow for falling as well.
	void LedgeGrab(){
		if (canGrabLedge && 
		    (motorState == MotorStates.Jumping || motorState == MotorStates.Climbing) && 
		    Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward).normalized, 1f)) {
			motorState = MotorStates.Ledgegrabbing;
		}
	}

	void UpdateLedgeGrab(){
		// Need to make a non-standard update to limit how people can look around while hanging.
		StandardCameraUpdate();

		if (moveDirection.y != 0){
			moveDirection.y -= friction * Time.deltaTime;
			moveDirection.y = Mathf.Clamp(moveDirection.y, 0, 100);
		}

		if (Input.GetKey(KeyCode.LeftShift) ||  Input.GetKey(KeyCode.RightShift)){
			canGrabLedge = false;
			motorState = MotorStates.Default;
			climbTime = 0f;
		}

		if (Input.GetButton("Jump")){
			// Muscle up
			motorState = MotorStates.MusclingUp;
			climbTime = 0f;
		}
	}

	void MuscleUp(){

		Ray ray = new Ray(transform.position, transform.TransformDirection(Vector3.forward));
		ray.direction.Normalize();
		ray.origin = ray.origin - new Vector3(0f, 1f, 0f);

		if (Physics.Raycast(ray.origin, ray.direction, 1f)){
			moveDirection = transform.TransformDirection(Vector3.up + Vector3.forward);
			moveDirection.Normalize();
			moveDirection *= BaseSpeed;
		}
		else {
			motorState = MotorStates.Default;
		}
	}

}

public enum MotorStates {
	Climbing,
	Default,
	Falling,
	Jumping,
	Ledgegrabbing,
	MusclingUp,
	Wallrunning
}