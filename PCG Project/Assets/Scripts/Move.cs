using UnityEngine;
using System.Collections;

namespace SnakesAndMazes
{
    public class Move : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
            tag = "Player";
        }

        // Update is called once per frame
        void Update()
        {

            if (Input.GetKey(KeyCode.W))
                rigidbody.velocity = new Vector3(0, 0, 15);
            else if (Input.GetKey(KeyCode.S))
                rigidbody.velocity = new Vector3(0, 0, -15);
            else if (Input.GetKey(KeyCode.A))
                rigidbody.velocity = new Vector3(-15, 0, 0);
            else if (Input.GetKey(KeyCode.D))
                rigidbody.velocity = new Vector3(15, 0, 0);
            else
                rigidbody.velocity = new Vector3(0, 0, 0);
        }
    }
}