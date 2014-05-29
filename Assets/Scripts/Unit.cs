using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]

public class Unit : MonoBehaviour
{
	
	protected CharacterController control;
	
	protected Vector3 move = Vector3.zero;
	
	public float moveSpeed = 3f;
	public float turnSpeed = 90f;
	
	// Use this for initialization
	public virtual void Start ()
	{
		control = GetComponent<CharacterController>();
		
		if (!control)
		{
			Debug.LogError("Unit.Start() " + name + " has no CharacterController!");
			enabled = false;
		}
	}
	
	// Update is called once per frame
	public virtual void Update ()
	{
		control.SimpleMove (move * moveSpeed);
	}
}
