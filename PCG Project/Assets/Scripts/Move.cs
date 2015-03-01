using UnityEngine;
using System.Collections;

namespace SnakesAndMazes
{
    public class Move : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

            if (Input.GetKey(KeyCode.W))
                transform.Translate(0, 0, .2f);
            if (Input.GetKey(KeyCode.S))
                transform.Translate(0, 0, -.2f);
            if (Input.GetKey(KeyCode.A))
                transform.Translate(-.2f, 0, 0);
            if (Input.GetKey(KeyCode.D))
                transform.Translate(.2f, 0, 0);
        }
    }
}