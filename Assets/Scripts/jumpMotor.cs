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

	public Camera camera;
	private float cameraRotX = 0.0f;

	private CharacterController controller;

	private bool wallRunKey = false;
	private Vector3 wallMovement = Vector3.zero;

	private Vector3 moveDirection = Vector3.zero;
	private Vector3 lastDirection = Vector3.zero;

	private CollisionFlags collisions;

	private float wallRunTime = 2.0f;

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
		wallRunKey = Input.GetKey(KeyCode.LeftShift);
		moveKeyDown = Input.GetKey(KeyCode.W);

		if(moveKeyDown && moveDownTime <= RampUpTime) {
			moveDownTime += Time.deltaTime;
			if (moveDownTime > RampUpTime){
				moveDownTime = RampUpTime;
			}
		}

		bool wallRunning = false;
		if (wallRunKey){
			wallRunning = DoWallRunCheck();
		}

		if (!wallRunning)
			transform.Rotate (0f, (Input.GetAxis("Mouse X") * MouseSensitivity) * TurnSpeed * Time.deltaTime, 0f);
		cameraRotX -= Input.GetAxis("Mouse Y") * MouseSensitivity;

		camera.transform.forward = transform.forward;
		camera.transform.Rotate(cameraRotX, 0f, 0f);

		if (controller.isGrounded && !wallRunning){
			if (!moveKeyDown){
				if (moveDownTime > 0){
					//moveDownTime -= RunSpeedIncrease * Time.deltaTime;
					moveDownTime = 0f;
				}
				else
					moveDownTime = 0f;

			}

			//moveDirection = Input.GetAxis("Vertical") * transform.TransformDirection(Vector3.forward);
			moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
			moveDirection = transform.TransformDirection(moveDirection);
			moveDirection.Normalize();

			moveDirection *= BaseSpeed + (RunSpeedIncrease * (moveDownTime / RampUpTime));
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
				moveDirection.y = JumpSpeed;
			}
		}

		moveDirection.y -= Gravity * Time.deltaTime;
		collisions = controller.Move(moveDirection * Time.deltaTime);
		lastDirection = moveDirection;
		//Debug.Log("collisions: " + collisions);
	}

	bool DoWallRunCheck(){
		Ray ray = new Ray(transform.position, transform.TransformDirection(Vector3.right));
		Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.right), Color.red, 2f, false);
		RaycastHit hit;

		if (Physics.Raycast(ray.origin, ray.direction, out hit, 1f)){
			Debug.DrawRay(hit.point, hit.normal, Color.green, 2f, false);

			//Vector3 crossProduct = Vector3.Cross(moveDirection, hit.normal);
			Vector3 crossProduct = Vector3.Cross(Vector3.up, hit.normal);
			Debug.DrawRay(transform.position, crossProduct * BaseSpeed, Color.magenta, 2f, false);

			Debug.Log(transform.rotation);
			//transform.Rotate (0f, (180f - Vector3.Angle(crossProduct, moveDirection)) * TurnSpeed * Time.deltaTime, 0f);
			//transform.Rotate(0f, (Vector3.Angle(crossProduct, transform.TransformDirection(Vector3.forward))) * TurnSpeed * Time.deltaTime, 0f);

			Quaternion lookDirection = Quaternion.LookRotation(crossProduct);

			transform.rotation = Quaternion.Slerp(transform.rotation, lookDirection, 3.5f * Time.deltaTime);

			moveDirection = crossProduct;
			//moveDirection = transform.TransformDirection(moveDirection);
			moveDirection.Normalize();
			moveDirection *= BaseSpeed + (RunSpeedIncrease * (moveDownTime / RampUpTime));

			//moveDirection.y = JumpSpeed;

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
