using UnityEngine;
using System.Collections;

// <author> Paige Hicks </author>
// <summary>Class controlling movement for Jump game</summary>

[RequireComponent(typeof(CharacterController))]

public class jumpMotor : MonoBehaviour {

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

	private bool isJumping = false;
	private bool jumpKeyDown = false;

	public Camera camera;
	private float cameraRotX = 0.0f;

	private CharacterController controller;

	private bool canWallRun = true;
	private bool wallRunning = false;

	private Vector3 moveDirection = Vector3.zero;
	private Vector3 lastDirection = Vector3.zero;

	private CollisionFlags collisions;

	private float wallRunMaxTime = 1.5f;
	private float wallRunTime = 0.0f;
	private RaycastHit wallHit;

	// Use this for initialization
	void Start () {
		camera = Camera.main;
		controller = GetComponent<CharacterController>();
		controller.detectCollisions = true;
	}
	
	// Update is called once per frame
	void Update () {
		UpdateFunction();
	}

	void UpdateFunction() {
		moveKeyDown = Input.GetKey(KeyCode.W);

		if(moveKeyDown && moveDownTime <= RampUpTime) {
			moveDownTime += Time.deltaTime;
			if (moveDownTime > RampUpTime){
				moveDownTime = RampUpTime;
			}
		}

		transform.Rotate (0f, (Input.GetAxis("Mouse X") * MouseSensitivity) * TurnSpeed * Time.deltaTime, 0f);
		cameraRotX -= Input.GetAxis("Mouse Y") * MouseSensitivity;

		camera.transform.forward = transform.forward;
		camera.transform.Rotate(cameraRotX, 0f, 0f);

		if (controller.isGrounded && !wallRunning){
			if (!moveKeyDown){
				moveDownTime = 0f;
			}

			canClimb = true;
			canWallRun = true;
			isJumping = false;

			//moveDirection = Input.GetAxis("Vertical") * transform.TransformDirection(Vector3.forward);
			moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
			moveDirection = transform.TransformDirection(moveDirection);
			moveDirection.Normalize();

			moveDirection *= BaseSpeed + (RunSpeedIncrease * (moveDownTime / RampUpTime));

			// slow to a stop
			if ((moveDirection == Vector3.zero && lastDirection != Vector3.zero)) {
				if (lastDirection.x != 0){
					if (lastDirection.x > 0){
						lastDirection.x -= friction * Time.deltaTime;
						if (lastDirection.x < 0)
							lastDirection.x = 0;
					}
					else{
						lastDirection.x += friction * Time.deltaTime;
						if (lastDirection.x > 0)
							lastDirection.x = 0;
					}
					moveDirection.x = lastDirection.x;
				}

				if (lastDirection.z != 0){
					if (lastDirection.z > 0){
						lastDirection.z -= friction * Time.deltaTime;
						if (lastDirection.z < 0)
							lastDirection.z = 0;
					}
					else{
						lastDirection.z += friction * Time.deltaTime;
						if (lastDirection.z > 0)
							lastDirection.z = 0;
					}
					moveDirection.z = lastDirection.z;
				}
			}

			if (Input.GetButton("Jump")){
				isJumping = true;
				moveDirection.y = JumpSpeed;
			}
		}
		else if ((!controller.isGrounded && isJumping) || wallRunning) {

			UpdateWallRun();
			WallClimb();
		}

		if (!wallRunning)
			moveDirection.y -= Gravity * Time.deltaTime;

		collisions = controller.Move(moveDirection * Time.deltaTime);
		lastDirection = moveDirection;
		//Debug.Log("collisions: " + collisions);
	}

	void UpdateWallRun (){

		if (!controller.isGrounded && canWallRun && wallRunTime < wallRunMaxTime){

			wallHit = DoWallRunCheck();
			if (wallHit.collider == null){
				//Debug.Log("MISS! leaving wall run.");
				StopWallRun();
				return;
			}

			wallRunning = true;
			float previousJumpHeight = moveDirection.y;

			Vector3 crossProduct = Vector3.Cross(Vector3.up, wallHit.normal);

			Quaternion lookDirection = Quaternion.LookRotation(crossProduct);

			transform.rotation = Quaternion.Slerp(transform.rotation, lookDirection, 3.5f * Time.deltaTime);

			moveDirection = crossProduct;
			moveDirection.Normalize();
			moveDirection *= BaseSpeed + (RunSpeedIncrease * (moveDownTime / RampUpTime));

			if (wallRunTime == 0.0f){
				// increase vertical movement.
				Debug.Log ("Wall run starting, increasing jump.");
				moveDirection.y = JumpSpeed / 4;
			}
			else {
				moveDirection.y = previousJumpHeight;
				moveDirection.y -= (Gravity / 4) * Time.deltaTime;
			}

			wallRunTime += Time.deltaTime;
			Debug.Log("Wall run time: " + wallRunTime);

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
		if (wallRunning)
			canWallRun = false;

		wallRunning = false;
		wallRunTime = 0.0f;
	}

	RaycastHit DoWallRunCheck(){
		Ray rayRight = new Ray(transform.position, transform.TransformDirection(Vector3.right));
		Ray rayLeft = new Ray(transform.position, transform.TransformDirection(Vector3.left));

		RaycastHit wallImpactRight;
		RaycastHit wallImpactLeft;

		bool rightImpact = Physics.Raycast(rayRight.origin, rayRight.direction, out wallImpactRight, 1f);
		bool leftImpact = Physics.Raycast(rayLeft.origin, rayLeft.direction, out wallImpactLeft, 1f);

		if (rightImpact && leftImpact) {
			// Return closest impact as wall run
			return wallImpactLeft.distance < wallImpactRight.distance ? wallImpactLeft : wallImpactRight;
		}
		else if (rightImpact) {
			//Debug.Log("Angle of incidence: " + Vector3.Angle(rayRight.direction, wallImpactRight.normal));
			return wallImpactRight;
		}
		else if (leftImpact) {
			wallImpactLeft.normal *= -1;
			return wallImpactLeft;
		}
		else {
			return wallImpactLeft;
		}
	} 

	// Temporarily here while experimenting with WallClimb.
	float climbTime = 0.0f;
	bool climbing = false;
	bool canClimb = true;
	void WallClimb() {
		if (!moveKeyDown) {
			climbTime = 0.0f;
			if (climbing)
				canClimb = false;
			return;
		}

		Ray forwardRay = new Ray(transform.position, transform.TransformDirection(Vector3.forward).normalized);
		forwardRay.direction /= 4;

		RaycastHit hit;
		if (canClimb && Physics.Raycast(forwardRay.origin, forwardRay.direction, out hit, 1f) && 
		    climbTime < 0.5f && Vector3.Angle(forwardRay.direction, hit.normal) > 165){

			climbTime += Time.deltaTime;

			Quaternion lookDirection = Quaternion.LookRotation(hit.normal * -1);
			
			transform.rotation = Quaternion.Slerp(transform.rotation, lookDirection, 3.5f * Time.deltaTime);
			camera.transform.Rotate(-85f * (climbTime / 0.5f), 0f, 0f);

			moveDirection = transform.TransformDirection(Vector3.up);
			moveDirection.Normalize();
			moveDirection *= BaseSpeed;
			climbing = true;
		}
		else {
			if (climbing)
				canClimb = false;
			climbTime = 0f;
			climbing = false;
		}
	}

	void OnControllerColliderHit(ControllerColliderHit hit){
		if ((collisions & CollisionFlags.Sides) != 0) {
			//Debug.DrawRay(hit.point, hit.normal, Color.red, 10f, false);
			//Debug.DrawRay(transform.position, moveDirection, Color.magenta, 10f, false);
			//wallMovement = Vector3.Cross(moveDirection, hit.normal);
			//wallMovement = Vector3.Cross(wallMovement, hit.normal);
			//Debug.DrawRay(transform.position, wallMovement * BaseSpeed, Color.red, 4f, false);
			//wallMovement.y = JumpSpeed;
		}
		/*else {
			wallMovement = Vector3.zero;
		}*/
	}

}
