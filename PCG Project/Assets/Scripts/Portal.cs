using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SnakesAndMazes
{
    public class Portal : MonoBehaviour
    {
        public Portal linkedPortal;
        public bool activated;

        void Start()
        {
            SphereCollider sc = (SphereCollider)this.gameObject.AddComponent(typeof(SphereCollider));
            sc.center = Vector3.zero;
            collider.isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player" && activated && linkedPortal.activated)
            {
                linkedPortal.activated = false;
                other.gameObject.transform.position = linkedPortal.transform.position;
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (linkedPortal != null)
            {
                activated = true;
            }
        }

    }
}
