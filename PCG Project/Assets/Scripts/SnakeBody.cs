using UnityEngine;
using System.Collections;

public class SnakeBody : MonoBehaviour {

	public Snakehead head;

	public SnakeBody next;

	public bool isNeck;

	public Vector3 prevMove, transformer;

	public int delayTimer, delayMax;

	// Use this for initialization
	void Start () {
		renderer.material.color = Color.blue;
		prevMove = Vector3.zero;
		delayMax = 50;
		
		delayTimer = 0;
	}
	
	// Update is called once per frame
	void Update () {


		if (delayTimer >= delayMax) {
			delayTimer = 0;
			transform.position += prevMove;

		} else
			delayTimer++;


		if (isNeck) 
		{
			transformer = head.transform.position - transform.position;
		}
		else 
		{
			transformer = next.transform.position - transform.position;
		}

		prevMove = transformer;
	
	}
}
