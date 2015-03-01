using UnityEngine;
using System.Collections;

public class FauxWallGenerator : MonoBehaviour {

	public Vector3 gridStart;

	FauxTile[] tiles;

	GameObject[] tileObjs;

	// Use this for initialization
	void Start () {


		gridStart = new Vector3 (0, 1, 0);
		tiles = new FauxTile[14000];
		tileObjs = new GameObject[14000];
		for (int i = 0; i <370; i+=10)
		{
			for(int j = 0; j<37; j++)
			{
				tileObjs[i+j] = GameObject.CreatePrimitive (PrimitiveType.Cube);
				tileObjs[i+j].transform.position = new Vector3(gridStart.x+(2*j),gridStart.y,gridStart.z+(2*(i/10))); 
				tileObjs[i+j].transform.localScale = new Vector3(2,2,2);
				tiles[i+j] = tileObjs[i+j].AddComponent ("FauxTile") as FauxTile;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
