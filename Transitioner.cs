using UnityEngine;
using System.Collections;

public class Transitioner : MonoBehaviour {

	//this class is for animation transitioning purposes, it gets the animator and sets the "Speed" value

	public Transform target; //target to follow

	private float moveSpeed = 5.0f;   
	private Animator anim;
	private Vector3 currentPosition;
	private Vector3 lastPosition;


	void Start () 
	{
		anim = GetComponent<Animator> ();	
	}
	
	// Update is called once per frame
	void Update () 
	{
		float movement;

		//if the current position of the chicken is the same as it's last position then theres no movement
		currentPosition = transform.position;
		if (currentPosition == lastPosition) 
		{
			movement = 0;
		}
		else movement = Vector3.Distance(target.position, transform.position) * moveSpeed;
		lastPosition = currentPosition;

		//if theres no movement, the chicken will display idle animation
		anim.SetFloat ("Speed", Mathf.Abs (movement));

	}
}
