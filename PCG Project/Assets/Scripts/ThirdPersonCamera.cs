using UnityEngine;
using System.Collections;

public class ThirdPersonCamera : MonoBehaviour {

	public GameObject player;
	public int ScaleHeight, ScaleDistance, ScaleAngle;

	// Use this for initialization
	void Start () 
	{
		ScaleHeight = 20;
		ScaleDistance = -15;
		ScaleAngle = 60;

		transform.position = player.transform.position;
		transform.Translate (0, ScaleHeight, -ScaleDistance);
		transform.Rotate (ScaleAngle, 0, 0);
	}
	
	// Update is called once per frameas
	void Update () 
	{
		resetCamera (ScaleHeight, ScaleDistance, ScaleAngle);
	}

	public void resetCamera(int height, int dist, int angle)
	{
		transform.Rotate (-angle, 0, 0);
		transform.position = player.transform.position;
		transform.Translate (0, height, dist);
		if (Input.GetKey (KeyCode.Q))
			transform.Rotate (0,1, 0);
		if (Input.GetKey (KeyCode.E))
			transform.Rotate (0,-1, 0);
		transform.Rotate (angle, 0, 0);
	}
}
