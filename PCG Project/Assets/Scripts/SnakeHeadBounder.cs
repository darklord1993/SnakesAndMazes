using UnityEngine;
using System.Collections;

public class SnakeHeadBounder : MonoBehaviour {

	public bool canMove;

	public int numWall;

	public Collider[] hitColliders;

	// Use this for initialization
	void Start () {
		canMove = true;
	}
	
	// Update is called once per frame
	void Update () 
	{
		numWall = 0;
		hitColliders = Physics.OverlapSphere(transform.position, 1);
		foreach (Collider hc in hitColliders) 
		{
			if(hc.gameObject.tag  == "GameController")
				numWall++;
		}

		if (numWall > 0)
			canMove = false;
		else
			canMove = true;
	}
	
}
