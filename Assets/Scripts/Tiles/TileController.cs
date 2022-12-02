using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

namespace ExoDev.AutoBattler
{
    public class TileController : MonoBehaviour
    {
        #region Variables

        [Title("Assigned Variables")]

        [Title("Interaction", horizontalLine: false)]
        public BoxCollider occupiedCollider;

        [Title("Visuals", horizontalLine: false)]
        public MeshRenderer hoverMeshRenderer;

        [Title("Current Variables")]

        [Title("Interaction", horizontalLine: false)]
        [ReadOnly] public Collider[] neighborFinder;
        [ReadOnly] public List<TileController> neighborControllers = new List<TileController>();

        [ReadOnly] public bool isOccupied;
        [ReadOnly] public GameObject occupyingUnit;

        [Title("Materials", horizontalLine: false)]
        [SerializeField, ReadOnly] Material invisibleHoverMaterial;
        [SerializeField, ReadOnly] Material validHoverMaterial;
        [SerializeField, ReadOnly] Material invalidHoverMaterial;

        #endregion
        #region Unity Methods

        private void Start()
        {
            neighborFinder = Physics.OverlapSphere(gameObject.transform.position, 7.5f, layerMask:1<<6, queryTriggerInteraction:QueryTriggerInteraction.Collide);
            foreach (Collider neighbor in neighborFinder) 
            {
                if (neighbor.gameObject != gameObject) 
                {
                    neighborControllers.Add(neighbor.GetComponent<TileController>());
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            //ReliableOnTriggerExit.NotifyTriggerEnter(other, gameObject, OnTriggerExit);

            if (other.gameObject.layer == 1<<7) 
            {
                /*if (other.GetComponent<UnitController>().currentState == UnitController.UnitStates.Grabbed)
                {
                    UpdateHoverMaterial();
                }*/

                isOccupied = true;
                occupyingUnit = other.gameObject;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            //ReliableOnTriggerExit.NotifyTriggerExit(other, gameObject);

            if (other.gameObject.layer == 1<<7)
            {
                isOccupied = false;
                occupyingUnit = null;

                /*if (other.GetComponent<UnitController>().currentState == UnitController.UnitStates.Grabbed) 
                {
                    UpdateHoverMaterial(invisible: true);
                }*/
            }
        }

        #endregion
        #region Public Methods

        public void UpdateOccupiedUnit(GameObject occupying)
        {
            bool tempBool = false;

            if (occupying != null)
            {
                tempBool = true;
            }

            isOccupied = tempBool;
            occupyingUnit = occupying;
        }

        public void UpdateHoverMaterial(bool invisible = false)
        {
            if (!invisible)
            {
                if (isOccupied)
                {
                    hoverMeshRenderer.material = invalidHoverMaterial;
                }
                else
                {
                    hoverMeshRenderer.material = validHoverMaterial;
                }
            }
            else 
            {
                hoverMeshRenderer.material = invisibleHoverMaterial;
            }
        }

        #endregion
    }
}
