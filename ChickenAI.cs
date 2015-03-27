using UnityEngine;
using System.Collections;

public class ChickenAI : MonoBehaviour {

	public Transform target; //the target the chickens are to follow
	public Transform foxTarget; //the fox they will avoid

	private bool isFleeing = false;
	private bool isArriving = false;
	private bool isSeeking = false;

	private float moveSpeed = 5.0f; //movement speed of chickens
	private float rotationSpeed = 3.0f; //rotation speed of chickens

	private Vector3 currentPosition; //current position of chicken
	private Vector3 lastPosition; //last position of chicken

	private enum state {Idle, Seek, Arrive, Evade, Flee}; //different behaviour states
	private state AIstate = state.Idle;// default state

	private GameObject[] chickens; //array of all chicken objects

	private Animator anim; // animator component
	private float movement; //the amount of movement for animation purposes i.e if 0 , idle animation

	private Vector3[] chickenDirections ; //array of Vector to refer to each chickens directions
	private int chickenCounter = 0; //to keep track of chickens
	private int chickenIndex = 0; //to keep track of chicken in each array index so chicken 1 in chickenDirections[1]


	void Start()
	{
		chickens = GameObject.FindGameObjectsWithTag ("chickens"); //set the array with all the chicken followers

		//initialise chicken direction array
		chickenDirections = new Vector3[30]; //initialise the chicken directions to 0

		anim = GetComponent<Animator>();
	}

	void Idle(GameObject obj)
	{
		//do nothing
	}

	void Seek (GameObject obj, Vector3 direction, int dirIndex)
	{
		//all chickens other than the leader seek
		if(obj.gameObject.name != "Chicken L")
		{
			if(isSeeking)
			{
				//find direction of target
				direction = target.position - obj.transform.position;

				//make sure they dont move upward
				direction.y = 0.0f;
			}

			//get details for raycasting
			Vector3 ahead = obj.transform.TransformDirection(transform.forward);
			ahead = ahead.normalized;
			float rayLength = 3.0f;
			RaycastHit hit;

			//if theres something in front, turn
			if(Physics.Raycast(obj.transform.position, ahead, out hit, rayLength))
			{
				isSeeking = false;
				
				if(hit.transform != obj.transform && hit.transform.gameObject.tag != "chickens")
				{
					//turn using normal of object hit
					Debug.DrawRay(obj.transform.position, ahead, Color.red, 10);	
					direction += hit.normal * 20;	
				}
			}

			//otherwise follow chicken leader
			else isSeeking = true;

			//if the leader is more than length 1 away
			if(direction.magnitude > 1.0f)
			{
				//turn to target
				obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation,
				                   Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);

				//move towards target
				Vector3 moveVector = obj.transform.forward * moveSpeed * Time.deltaTime;
				obj.transform.position += moveVector;

				//set direction for next update
				chickenDirections[dirIndex] = direction;
			}
		}


		//leader flees around aimlessly
		else Flee (obj, direction, dirIndex);
	}


	void Arrive(GameObject obj, Vector3 direction, int dirIndex)
	{
		//all chickens other than the leader arrive
		if(obj.gameObject.name != "Chicken L")
		{
			if(isArriving)
			{
				//find direction of target
				direction = target.position - obj.transform.position;
				direction.y = 0.0f;
			}

			//calculate distance and set brake factor in accordance
			float distance = direction.magnitude;
			float brakeFactor = distance / 5;
			float speed = 0;

			//if the chickens are within visibility, they will follow
			if(distance < 10.0f)
			{
				//there speed will depend on distance between target and follower
				if(distance > 3.0f)
				{
					//multiply by brake factor as come into proximity
					speed = moveSpeed * brakeFactor;
				}

				else 
				{
					speed = 0;	
				}

			}

			//get details for ray casting
			Vector3 ahead = obj.transform.TransformDirection(transform.forward);
			ahead = ahead.normalized;
			float rayLength = 1.0f;
			RaycastHit hit;
		
			//if theres something in front, turn
			if(Physics.Raycast(obj.transform.position, ahead, out hit, rayLength))
			{
				isArriving = false;

				//if theres something in front, turn
				if(hit.transform != obj.transform && hit.transform.gameObject.tag != "chickens")
				{
					Debug.DrawRay(obj.transform.position, ahead, Color.red, 10);	
					direction += hit.normal * 20;	
				}
			}

			//otherwise turn back to what it's following
			else isArriving = true; 

			//rotate to face the target position
			obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);

			//move towards target
			Vector3 moveVector = obj.transform.forward * speed * Time.deltaTime;
			obj.transform.position += moveVector;

			chickenDirections [dirIndex] = direction;
		}
		//leader flees around aimlessly
		else Flee (obj, direction, dirIndex);
	}

	
	
	void Evade(GameObject obj)
	{	 
		//find direction that is opposite of target
		Vector3 direction = obj.transform.position - foxTarget.transform.position;
		direction.y = 0.0f;
			
		//find distance of target
		float distance = direction.magnitude;
	
		if(distance < 3.0f)
		{
			direction +=  obj.transform.right * 2;

			//rotate away from it
			obj.transform.rotation = Quaternion.Slerp (obj.transform.rotation, Quaternion.LookRotation (direction), rotationSpeed * Time.deltaTime);


			//move away from it when it comes close
			Vector3 moveVector = obj.transform.forward * moveSpeed * Time.deltaTime;
			obj.transform.position += moveVector;
		}
	}



	void Flee(GameObject obj, Vector3 direction, int dirIndex)
	{
		//if in fleeing state, first turn away from the fox
		if(isFleeing)
		{
			int nextSteps = 2; //prediction of next n update steps so 2 steps
			
			//get the rigidbody of fox for velocity
			Rigidbody fox = foxTarget.GetComponent<Rigidbody> ();

			//calculate where it will be in the next 2 update steps using it's velocity
			direction = obj.transform.position - (foxTarget.position + fox.velocity * nextSteps) ; 

			isFleeing = false;
		}

		direction.y = 0.0f;

		//then keep moving forward and if theres any obstacle, turn away and keep moving forward
		Vector3 ahead = obj.transform.forward;
		float rayLength = 3.0f;
		RaycastHit hit;
		ahead = ahead.normalized;
		
		if(Physics.Raycast(obj.transform.position, ahead, out hit, rayLength))
		{
			if(hit.transform != obj.transform && hit.transform.gameObject.tag != "chickens")
			{
				Debug.DrawRay(obj.transform.position, ahead, Color.red, 10);	
				direction += hit.normal * 45;

			}
		}

		//rotate towards the direction
		obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);

		//move forward
		Vector3 moveVector = obj.transform.forward * moveSpeed * Time.deltaTime;
		obj.transform.position += moveVector;

		//set the direction for next iteration
		chickenDirections [dirIndex] = direction;
	}

	void ChangeBehaviour(GameObject gob, Vector3 chickenDir, int chickenInd)
	{
		//for the gameobject specified - gob , changes it's behaviour dependant on button clicked on gui
		switch(AIstate)
		{
		case state.Idle:
			Idle(gob);
			break;
		case state.Seek: 
			Seek(gob, chickenDir, chickenInd);
			break;
		case state.Arrive:
			Arrive(gob, chickenDir, chickenInd);
			break;
		case state.Evade:
			Evade (gob);
			break;
		case state.Flee:
			Flee (gob, chickenDir, chickenInd);
			break;
		}
	}

	void Update () 
	{
		chickenCounter = 0;
		chickenIndex = 0;

		//for every chicken follower, change their behaviour
		foreach(GameObject gob in chickens)
		{
			if(gob != null)
			{
				ChangeBehaviour (gob, chickenDirections[chickenCounter], chickenIndex);
				chickenCounter++;
				chickenIndex++;
			}
		}

		//if the current position of the chicken is the same as it's last position then theres no movement
		currentPosition = transform.position;
		if (currentPosition == lastPosition) 
		{
			movement = 0;
		}
	
		else movement = Vector3.Distance(foxTarget.position, transform.position) * moveSpeed;
	
		lastPosition = currentPosition;

		//if theres no movement, the chicken will display idle animation
		anim.SetFloat ("Speed", Mathf.Abs (movement));	
	}

	private void OnGUI () 
	{
		// Make a background box
		GUI.Box(new Rect(10,10,160, 190), "Select Chicken Behaviour");

		// Make the idle button.
		if(GUI.RepeatButton(new Rect(50,40,80,20), "Idle"))
		{
			AIstate = state.Idle;
		}

		// Make the seek button.
		if(GUI.RepeatButton(new Rect(50,70,80,20), "Seek"))
		{
			isSeeking = true;
			AIstate = state.Seek;
		}
		
		// Make the arrive button.
		if(GUI.RepeatButton(new Rect(50,100,80,20), "Arrive")) 
		{
			isArriving = true;
			AIstate = state.Arrive;
		}
		
		// Make the evade button.
		if(GUI.RepeatButton(new Rect(50,130,80,20), "Evade")) 
		{
			AIstate = state.Evade;
		}

		// Make the flee button.
		if(GUI.RepeatButton(new Rect(50,160,80,20), "Flee")) 
		{
			isFleeing = true;
			AIstate = state.Flee;
		}
	}
}
