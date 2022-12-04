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
        public GameObject testEnemyUnitPrefab;

        public enum GameStates { Idle, PreBattle, Battle, BattleOver }

        public enum UnitTeams { User, Enemy }

        [Title("Assigned Variables")]

        public GameObject unitHolder;

        public BoardController currentBoard;
        public BenchController UserBench;
        public BenchController EnemyBench;

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

        }

        private void GameBattle() { }

        private void GameBattleOver() { }

        #endregion
        #region Test Methods

        public void SpawnTesters() 
        {
            SpawnUnitPrefab(testUnitPrefab);
            SpawnUnitPrefab(testEnemyUnitPrefab);
            SpawnUnitPrefab(testEnemyUnitPrefab);
        }

        private void SpawnUnitPrefab(GameObject prefab)
        {
            GameObject tempGO;
            int tempRand;

            if (prefab.GetComponentInChildren<UnitController>().currentTeam == UnitTeams.User)
            {
                tempRand = Random.Range(0, UserBench.benchTiles.Count);

                tempGO = Instantiate<GameObject>(prefab, unitHolder.transform);
                tempGO.transform.position += UserBench.benchTiles[tempRand].transform.position;
            }
            else 
            {
                tempRand = Random.Range(0, EnemyBench.benchTiles.Count);

                tempGO = Instantiate<GameObject>(prefab, unitHolder.transform);
                tempGO.transform.position += EnemyBench.benchTiles[tempRand].transform.position;
            }
        }

        #endregion
    }
}
