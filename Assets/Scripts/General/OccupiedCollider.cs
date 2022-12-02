using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

namespace ExoDev
{
    public class OccupiedCollider : MonoBehaviour
    {
        #region Variables

        public enum OccupiedModes { Single, Multiple }

        [Title("Options")]
        [Title("General", horizontalLine:false)]
        public OccupiedModes currentMode;

        [Title("Single", horizontalLine: false)]
        public GameObject touchingObject;

        [Title("Multiple", horizontalLine: false)]
        public List<GameObject> touchingObjects;

        bool isAssigned;

        #endregion
        #region Unity Methods

        private void OnTriggerEnter(Collider other)
        {
            switch (currentMode)
            {
                case OccupiedModes.Single:
                    touchingObject = other.gameObject;
                    break;

                case OccupiedModes.Multiple:
                    if (!touchingObjects.Contains(other.gameObject))
                    {
                        touchingObjects.Add(other.gameObject);
                    }
                    break;

                default:
                    break;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            switch(currentMode)
            {
                case OccupiedModes.Single:
                    touchingObject = null;
                    break;

                case OccupiedModes.Multiple:
                    if (touchingObjects.Contains(other.gameObject)) 
                    {
                        touchingObjects.Remove(other.gameObject);
                    }
                    break;

                default:
                    break;
            }
        }

        private void OnTriggerStay(Collider other)
        {
            switch (currentMode)
            {
                case OccupiedModes.Single:
                    break;

                case OccupiedModes.Multiple:
                    if (!touchingObjects.Contains(other.gameObject))
                    {
                        touchingObjects.Add(other.gameObject);
                    }
                    break;

                default:
                    break;
            }
        }

        #endregion
    }
}
