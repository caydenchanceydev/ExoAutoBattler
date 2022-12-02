using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

namespace ExoDev.AutoBattler
{
    public class BoardController : MonoBehaviour
    {
        #region Variables

        [Title("Assigned Variables")]

        public List<Objective_TileController> ObjectiveTiles;
        public List<Buff_TileController> BuffTiles;

        [Title("Current Variables")]

        [ShowInInspector, ReadOnly] static BoardController _instance;
        public static BoardController Instance { get { return _instance; } }

        [ReadOnly] public Collider[] tileFinder;

        [ReadOnly] public List<TileController> AllTiles;
        [ReadOnly] public List<GameObject> allOccupyingObjects;

        #endregion
        #region Unity Methods

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        private void Start()
        {
            TileController tempTileController;

            tileFinder = Physics.OverlapSphere(gameObject.transform.position, 100f, layerMask:1<<6, queryTriggerInteraction:QueryTriggerInteraction.Collide);

            if (tileFinder.Length > 0)
            {
                foreach (Collider tile in tileFinder)
                {
                    if (tile.TryGetComponent<TileController>(out tempTileController))
                    {
                        AllTiles.Add(tempTileController);

                        if (tempTileController.occupyingUnit != null)
                        {
                            allOccupyingObjects.Add(tempTileController.occupyingUnit);
                        }
                    }
                }
            }
        }

        #endregion
        #region Public Methods

        public void UpdateAllTilesList() 
        {
            UpdateAllUnitsList();
        }

        #endregion
        #region Private Methods

        private void UpdateAllUnitsList() 
        {
            foreach (TileController tile in AllTiles) 
            {
                if (tile.occupyingUnit != null && !allOccupyingObjects.Contains(tile.occupyingUnit))
                {
                    allOccupyingObjects.Add(tile.occupyingUnit);
                }
            }
        }

        #endregion
    }
}
