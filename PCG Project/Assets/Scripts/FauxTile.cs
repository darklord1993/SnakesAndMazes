using UnityEngine;
using System.Collections;

namespace SnakesAndMazes
{

    public class FauxTile : MonoBehaviour
    {


        // Use this for initialization
        void Start()
        {
			tag = "Untagged";
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
				tag = "Untagged";
            }
            else if (renderer.enabled == false)
            {
                renderer.enabled = true;
                collider.isTrigger = false;
				tag = "GameController";
            }
        }

    }
}