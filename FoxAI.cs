using UnityEngine;
using System.Collections;

public class FoxAI : MonoBehaviour {
	
	public AudioClip chickenDeath;
	private AudioSource source; //sounds

	private Transform mainTarget; //the current target the fox is chasing
	
	private float moveSpeed = 3.0f; //movement speed of fox
	private float rotationSpeed = 3.0f; //rotation speed of fox
	
	private Vector3 currentPosition; //current position of fox
	private Vector3 lastPosition; //last position
	private Vector3 direction; //the direction the fox is facing

	private Vector3[] chickenPositions;// postions of chickens

	private bool isPursuiting = false; //is it pursuiting
	private bool isSeeking = false; //is it seeking

	private float distance = 0; //keeping track of distance to chickens
	private float closestDistance = 50.0f; //which chicken is closest? set to 50 initially

	private int chickenAmount = 30; //how many chickens are there
	private int chickenCounter = 0; //keeps track of number chickens
	private int chickensEaten = 0; //how many chickens have been eaten

	private float movement; //keeps track of movement for animation transitioning i.e. movement > 0 is walk animation otherwise idle
	private int counter; //chicken counter

	private enum state {Idle, Seek, Pursuit}; //different behaviour states
	
	private state AIstate = state.Idle; //default state

	private GameObject[] chickens; //array of all chicken objects
	
	private Animator anim;

	void Awake()
	{

		source = GetComponent<AudioSource> ();
	}

	void Start()
	{
		chickens = GameObject.FindGameObjectsWithTag ("chickens"); //set the array with all the chicken followers
		anim = GetComponent<Animator>();
	
		//initialise chicken direction array
		chickenPositions = new Vector3[chickenAmount];
	}

	void Idle()
	{
		//set self as target and do nothing
		mainTarget = transform;
	}

	void Seek ()
	{
		FindChicken (); //find the closest chicken

		if(isSeeking)
		{
			//find direction of target
			direction = mainTarget.position - transform.position;
			direction.y = 0.0f;
		}

		//check for upcoming collisions
		collisionChecker ();

		//turn to target
		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);

		//move towards target if theres a distance greater than 1
		if(direction.magnitude > 1)
		{
			//turn to target
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);

			Vector3 moveVector = (direction.normalized * moveSpeed * Time.deltaTime);
			transform.position += moveVector;
		}
	}

	void Pursuit()
	{
		//go through all chickens and find closest
		FindChicken ();

		//get the rigidbody of chicken for velocity
		Rigidbody chicken = mainTarget.GetComponent<Rigidbody> ();

		if(isPursuiting)
		{
			//calculate where it will be in the next step using it's velocity
			direction = (mainTarget.position + chicken.velocity) - transform.position; 
			direction.y = 0.0f;
		}

		//check for upcoming collisions
		collisionChecker ();

		//move towards target if there is a distance greater than 3
		if(direction.magnitude > 1)
		{
			direction.y = 0.0f;

			//turn to target
			transform.rotation = Quaternion.Slerp(transform.rotation, 
			                       Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);

			Vector3 moveVector = transform.forward * moveSpeed * Time.deltaTime;
			transform.position += moveVector;
		}
	}
	
	void FindChicken()
	{
		for(int i = 0; i < chickenPositions.Length; i++)
		{
			//get the position and distance to each chicken
			Vector3 checkDir = chickenPositions[i] - transform.position;
			distance = checkDir.magnitude;
			
			//if the distance of the current chicken is less than the closest distance
			if(distance < closestDistance)
			{
				if(chickens[i] != null)
				{
					//set route to this chicken
					mainTarget = chickens[i].transform;
					closestDistance = distance;
				}
			}
		}

		//reset the fox's closest distance
		closestDistance = 50.0f;
	}

	void collisionChecker()
	{
		//variables for raycasting
		Vector3 ahead = transform.TransformDirection(transform.forward);
		ahead = ahead.normalized;
		float rayLength = 2.0f;
		RaycastHit hit;
		
		//if theres something in front, turn
		if(Physics.Raycast(transform.position, ahead, out hit, rayLength))
		{
			isSeeking = false;
			isPursuiting = false;
			
			if(hit.transform != transform && hit.transform.gameObject.tag != "chickens")
			{
				Debug.DrawRay(transform.position, ahead, Color.blue, 10);	
				direction += hit.normal * 20;	
			}
		}	

		//otherwise turn back to what it's following
		else 
		{
			isPursuiting = true;
			isSeeking = true;
		}
	}


	
	void ChangeBehaviour()
	{
		//for the gameobject specified - gob , changes it's behaviour dependant on button clicked on gui
		switch(AIstate)
		{
		case state.Idle:
			Idle();
			break;
		case state.Seek: 
			Seek();
			break;
		case state.Pursuit:
			Pursuit();
			break;
		}
	}
	
	void Update () 
	{

		chickenCounter = 0;

		//set all chicken locations into array
		foreach(GameObject gob in chickens)
		{
			if(gob != null)
			{
				chickenPositions[chickenCounter] = gob.transform.position;
				chickenCounter++;
			}
		}

		ChangeBehaviour ();

		//if the current position of the fox is the same as it's last position then theres no movement
		currentPosition = transform.position;
		if (currentPosition == lastPosition) 
		{
			movement = 0;
		}
		else movement = Vector3.Distance(mainTarget.position, transform.position) * moveSpeed;
		lastPosition = currentPosition;

		//if theres no movement, the fox will display idle animation
		anim.SetFloat ("Speed", Mathf.Abs (movement));

	}

	private void OnGUI () 
	{

		GUI.Label(new Rect(600, 10,160, 190),"Chickens Eaten: " + chickensEaten);


		// Make a background box
		GUI.Box(new Rect(200, 10, 160 , 190), "Select Fox Behaviour");

		// Make the Idle button.
		if(GUI.RepeatButton(new Rect(240,40,80,20), "Idle"))
		{
			AIstate = state.Idle;
		}

		// Make the Seek button.
		if(GUI.RepeatButton(new Rect(240,70,80,20), "Seek"))
		{
			AIstate = state.Seek;
		}
		
		// Make the Pursuit button.
		if(GUI.RepeatButton(new Rect(240,100,80,20), "Pursuit")) 
		{
			isPursuiting = true;
			AIstate = state.Pursuit;
		}

		//Make the Quit button
		if(GUI.RepeatButton(new Rect(400, 10, 80, 20), "Quit Demo"))
		{
			Application.Quit();
		}

	}

	void OnCollisionEnter(Collision collision)
	{	
		//keeps track of all chickens the fox has eaten
		if(collision.gameObject.tag == "chickens" && collision.gameObject.name != "Chicken L")
		{
			//play chicken death sound
			source.PlayOneShot(chickenDeath);

			//delete the gameobject
			Destroy(collision.gameObject);
			chickenAmount--;
			chickensEaten++;

			//remove it from array
			chickens = GameObject.FindGameObjectsWithTag ("chickens"); //set the array with all the chicken followers
			chickenPositions = new Vector3[chickenAmount];
		}

	}
}
