using UnityEngine;
using System.Collections;

public class FauxTile : MonoBehaviour {
	

    // Use this for initialization
    void Start()
    {
		renderer.enabled = false;
		collider.isTrigger = true;
		renderer.material.color = Color.red;
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnMouseDown()
    {
        // this object was clicked - do something


        if (renderer.enabled == true)
        {
            renderer.enabled = false;
			collider.isTrigger = true;
        }
        else if (renderer.enabled == false)
        {
            renderer.enabled = true;
			collider.isTrigger = false;
        }
    }
	
}
