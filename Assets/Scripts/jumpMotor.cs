using UnityEngine;
using System.Collections;

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
			//if (canWallRun){
			//	wallRunning = DoWallRunCheck();
			//}

			// If wallRunning
			//if (wallRunning && canWallRun){
				UpdateWallRun();
			//}

		}

		if (!wallRunning)
			moveDirection.y -= Gravity * Time.deltaTime;

		collisions = controller.Move(moveDirection * Time.deltaTime);
		lastDirection = moveDirection;
		//Debug.Log("collisions: " + collisions);
	}

	void UpdateWallRun (){

		Ray ray = new Ray(transform.position, transform.TransformDirection(Vector3.right));
		
		if (!controller.isGrounded && canWallRun && Physics.Raycast(ray.origin, ray.direction, out wallHit, 1f) && wallRunTime < wallRunMaxTime){
			//if (wallRunTime < wallRunMaxTime) {
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
			//}
			if (wallRunTime > wallRunMaxTime){
				canWallRun = false;
				Debug.Log ("Max wall run time hit.");
			}
		}
		else {
			wallRunning = false;
			wallRunTime = 0.0f;
		}
	}

	bool DoWallRunCheck(){
		Ray ray = new Ray(transform.position, transform.TransformDirection(Vector3.right));

		if (Physics.Raycast(ray.origin, ray.direction, out wallHit, 1f)){
			return true;
		}
		else {
			return false;
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
