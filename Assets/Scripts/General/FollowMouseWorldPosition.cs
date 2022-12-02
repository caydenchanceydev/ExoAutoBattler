using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExoDev
{
    public class FollowMouseWorldPosition : MonoBehaviour
    {
        public enum MouseFollowTypes { Plane }

        [Header("Assigned Variables")]
        public MouseFollowTypes followType;

        [Tooltip("IF PLANE: Height of mouse plane from the origin.")]
        public float planeOffset;

        [Header("Current Variables")]
        public Vector3 mouseScreenPosition;
        public Vector3 mouseWorldPosition;

        Plane mousePlane;

        private void Awake()
        {
            switch (followType)
            {
                case MouseFollowTypes.Plane:
                    mousePlane = new Plane(Vector3.down, planeOffset);
                    break;

                default:
                    break;
            }
        }

        private void Update()
        {
            switch (followType) 
            {
                case MouseFollowTypes.Plane:
                    mouseScreenPosition = Input.mousePosition;

                    Ray ray = Camera.main.ScreenPointToRay(mouseScreenPosition);

                    if (mousePlane.Raycast(ray, out float distance))
                    {
                        mouseWorldPosition = ray.GetPoint(distance);
                    }

                    transform.position = mouseWorldPosition;
                    break;

                default:
                    break;
            }
        }
    }
}
