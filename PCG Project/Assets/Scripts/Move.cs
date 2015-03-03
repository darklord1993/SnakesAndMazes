using UnityEngine;
using System.Collections;

namespace SnakesAndMazes
{
    public class Move : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
			renderer.material.color = Color.yellow;
        }

        // Update is called once per frame
        void Update()
        {

            if (Input.GetKey (KeyCode.W))
				rigidbody.velocity = new Vector3 (0, 0, 15);
            else if (Input.GetKey(KeyCode.S))
				rigidbody.velocity = new Vector3 (0, 0, -15);
            else if (Input.GetKey(KeyCode.A))
				rigidbody.velocity = new Vector3 (-15, 0, 0);
            else if (Input.GetKey(KeyCode.D))
				rigidbody.velocity = new Vector3 (15, 0, 0);
			else
				rigidbody.velocity = new Vector3 (0, 0, 0);
        }

		void OnTriggerEnter(Collider col)
		{
			if (col.gameObject.tag == "Finish") {
				transform.position=new Vector3(38.9f,132.2f,4.1f);
				rigidbody.useGravity=false;
				Debug.Log("Player Eat'ed-ed");

			}
		}
    }


}