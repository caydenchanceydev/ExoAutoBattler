using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

namespace ExoDev.AutoBattler
{
    public class GameController : MonoBehaviour
    {
        #region Variables

        [Title("Test Variables")]

        public GameObject testUnitPrefab;
        [SerializeField] bool testFlipper;

        public enum GameStates { Idle, PreBattle, Battle, BattleOver }

        public enum UnitTeams { User, Enemy }

        [Title("Assigned Variables")]

        public GameObject unitHolder;

        public BoardController currentBoard;

        public GameObject mousePoint;

        [Title("Current Variables")]

        [ShowInInspector, ReadOnly] static GameController _instance;
        public static GameController Instance { get { return _instance; } }

        [ReadOnly] public GameStates currentState;

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
            ChangeState(GameStates.Idle);
        }

        private void Update()
        {
            switch (currentState)
            {
                case GameStates.Idle:
                    GameIdle();
                    break;
                case GameStates.PreBattle:
                    GamePreBattle();
                    break;
                case GameStates.Battle:
                    GameBattle();
                    break;
                case GameStates.BattleOver:
                    GameBattleOver();
                    break;
                default:
                    break;
            }
        }

        #endregion
        #region Public Methods

        public void ChangeState(GameStates newState) 
        {
            switch (newState) 
            {
                case GameStates.Idle:
                    currentState = newState;
                    ChangeState(GameStates.PreBattle);
                    break;
                case GameStates.PreBattle:
                    currentState = newState;
                    break;
                case GameStates.Battle:
                    currentState = newState;
                    break;
                case GameStates.BattleOver:
                    currentState = newState;
                    break;
                default:
                    break;
            }
        }

        #endregion
        #region Private Methods

        private void GameIdle() { }

        private void GamePreBattle() 
        {
            if (testFlipper)
            {
                SpawnTesters();
                testFlipper = false;
            }
        }

        private void GameBattle() { }

        private void GameBattleOver() { }

        #endregion
        #region Test Methods

        private void SpawnTesters()
        {
            GameObject tempGO;
            int tempRand;

            for (int i = 0; i < 2; i++) 
            {
                tempRand = Random.Range(0, BoardController.Instance.AllTiles.Count);

                tempGO = Instantiate<GameObject>(testUnitPrefab, unitHolder.transform);
                tempGO.transform.position += BoardController.Instance.AllTiles[tempRand].transform.position;
            }
        }

        #endregion
    }
}
