using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

using Sirenix.OdinInspector;

namespace ExoDev.AutoBattler
{
    public class UnitController : MonoBehaviour
    {
        #region Variables

        [Title("Test Variables")]
        [SerializeField] bool findTarget;
        [SerializeField] bool findPath;
        [SerializeField] bool moveToHex;
        [SerializeField] bool startAttackCycle;
        [SerializeField] float testDamage;
        [SerializeField] TileController moveToThisHex;
        [SerializeField] float moveTime;

        public enum UnitCategories { Test, Tank, DPS, Support }

        public enum UnitStates { Idle, Grabbed, Move, Attack }

        [Title("Assigned Variables")]

        [Title("Base", horizontalLine: false)]
        [SerializeField] UnitAttributes_SO unitAttributes;

        [Title("Materials", horizontalLine: false)]
        [SerializeField] Material deadMaterial;
        [SerializeField] Material hurtMaterial;

        [Title("Position", horizontalLine: false)]
        [SerializeField] float tileHeightOffset;
        [SerializeField] float grabbedHeightOffset;
        [SerializeField] float hexDiameter;

        [Title("Collision Detection", horizontalLine: false)]
        [SerializeField] CapsuleCollider attackRange;
        [SerializeField, ReadOnly] float attackRangeStart = 1.5f;

        [Title("Current Variables")]

        [Title("Base", horizontalLine:false)]
        [ReadOnly] public float currentHealth = 0.0f;
        [ReadOnly] public float currentMana = 0.0f;

        [ReadOnly] public GameController.UnitTeams currentTeam;

        UnitStates lastState;
        [ReadOnly] public UnitStates currentState;

        [SerializeField, ReadOnly] TileController lastTile;
        [ReadOnly] public TileController currentTile;

        [Title("Movement", horizontalLine: false)]
        [ReadOnly] public TileController targetHex;
        [ReadOnly] public int hexesAlongPath;
        [ReadOnly] public bool movingToHex;
        [ReadOnly] public bool atTargetHex;

        [Title("Damage", horizontalLine: false)]
        [ReadOnly] public bool isDead;
        [ReadOnly] MeshRenderer unitMeshRenderer;
        [SerializeField, ReadOnly] Material defaultMaterial;

        [SerializeField, ReadOnly] float hurtTime = 0.5f;

        [Title("Interaction", horizontalLine: false)]
        [ReadOnly] public bool onMouse;

        [Title("Combat", horizontalLine: false)]
        [SerializeField, ReadOnly] GameObject targetCheck;
        [ReadOnly] public GameObject currentTarget;
        
        [SerializeField, ReadOnly] bool checkedStartingHex;
        [SerializeField, ReadOnly] TileController currentTargetTile;
        [SerializeField, ReadOnly] float distanceToCurrentTarget;
        [SerializeField, ReadOnly] int hexDistanceToTarget;
        [SerializeField, ReadOnly] List<TileController> pathToCurrentTarget;

        [SerializeField, ReadOnly] bool currentTargetKilled;

        [SerializeField, ReadOnly] float attackRangeColliderRadius;
        [SerializeField, ReadOnly] Collider[] targetCollidersInRange;
        [ReadOnly] public List<GameObject> targetsInRange;

        [SerializeField, ReadOnly] Collider[] allTargetColliders;
        [ReadOnly] public List<GameObject> allTargets;

        [SerializeField, ReadOnly] float floatToHexErrorRange;
        [SerializeField, ReadOnly] float floatToHexErrorRangeHigh;
        [SerializeField, ReadOnly] float floatToHexErrorRangeLow;

        #endregion
        #region Unity Methods

        private void Awake()
        {
            unitMeshRenderer = GetComponent<MeshRenderer>();
            defaultMaterial = unitMeshRenderer.material;
        }

        private void Start()
        {
            currentState = UnitStates.Idle;
            currentHealth += unitAttributes.unitBaseHealth;
            currentMana += unitAttributes.unitBaseMana;
        }

        private void Update()
        {
            if (findTarget)
            {
                attackRangeColliderRadius =  attackRangeStart + ((unitAttributes.unitBehaviors.unitAttackRange + 1) * 2);
                BoardController.Instance.UpdateAllTilesList();
                currentTarget = FindClosestTarget(BoardController.Instance.allOccupyingObjects);
                findTarget = false;
            }

            if (currentTarget != null)
            {
                distanceToCurrentTarget = Vector3.Distance(gameObject.transform.position, currentTarget.transform.position);
                hexDistanceToTarget = FloatToHexDistance(distanceToCurrentTarget);
                
                if (currentTarget.GetComponent<UnitController>().currentState == UnitStates.Grabbed) 
                {
                    BoardController.Instance.ResetAllTilesHighlight();
                }
            }

            if (findPath) 
            {
                BoardController.Instance.ResetAllTilesHighlight();

                if (currentTarget.GetComponent<UnitController>().currentTile != null)
                {
                    currentTargetTile = currentTarget.GetComponent<UnitController>().currentTile;
                    pathToCurrentTarget = FindPathToHex(currentTargetTile);

                    foreach (TileController tile in pathToCurrentTarget) 
                    {
                        tile.UpdateHoverMaterial();
                    }
                }

                findPath = false;
            }

            if (moveToHex) 
            {
                StartCoroutine(UnitMoveToHex(moveToThisHex, moveTime));
                moveToHex = false;
            }

            if (startAttackCycle) 
            {
                BoardController.Instance.UpdateAllTilesList();
                ChangeState(UnitStates.Attack);
                startAttackCycle = false;
            }

            if (!isDead)
            {
                switch (currentState)
                {
                    case UnitStates.Idle:
                        UnitIdle();
                        break;

                    case UnitStates.Grabbed:
                        UnitGrabbed();
                        break;

                    case UnitStates.Move:
                        UnitMove();
                        break;

                    case UnitStates.Attack:
                        UnitAttack();
                        break;

                    default:
                        ConsoleLogPlus("Unknown Unit State: " + currentState + ", Unit: " + gameObject.name);
                        break;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            //ConsoleLogPlus("Trigger Entered, This: " + gameObject.name + ", Other: " + other.gameObject.name);

            if (other.gameObject.TryGetComponent(out TileController tc))
            {
                currentTile = tc;

                if (currentState == UnitStates.Grabbed)
                {
                    currentTile.UpdateHoverMaterial();
                }
            }

            if (other.gameObject == GameController.Instance.mousePoint)
            {
                onMouse = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            //ConsoleLogPlus("Trigger Exited, This: " + gameObject.name + ", Other: " + other.gameObject.name);

            if (other.gameObject.TryGetComponent(out TileController tc))
            {
                lastTile = tc;
                currentTile = null;

                if (currentState == UnitStates.Grabbed)
                {
                    lastTile.UpdateHoverMaterial(invisible: true);
                }
            }

            if (other.gameObject == GameController.Instance.mousePoint)
            {
                onMouse = false;
            }
        }

        #endregion
        #region Public Methods

        public void ChangeState(UnitStates newState)
        {
            if (!isDead)
            {
                //if (newState == UnitStates.Grabbed && GameController.Instance.currentState != GameController.GameStates.Battle)
                //{
                    lastState = currentState;

                    switch (newState)
                    {
                        case UnitStates.Idle:
                            if (lastState == UnitStates.Grabbed)
                            {
                                if (currentTile != null && !currentTile.isOccupied)
                                {
                                    MoveUnitToTileFromGrabbed();
                                }
                                else
                                {
                                    MoveUnitToTileFromGrabbed(SendToLastTile: true);
                                }
                            }

                            currentState = newState;
                            break;

                        case UnitStates.Grabbed:

                            transform.parent = GameController.Instance.mousePoint.transform;
                            transform.position += new Vector3(0, grabbedHeightOffset, 0);

                            currentState = newState;
                            break;

                        case UnitStates.Move:

                            targetCheck = FindClosestTarget(BoardController.Instance.allOccupyingObjects);
                            hexesAlongPath = 0;

                            if (targetCheck != null && pathToCurrentTarget.Count > 0)
                            {
                                movingToHex = true;
                                StartCoroutine(UnitMoveToHex(pathToCurrentTarget[hexesAlongPath], 0.5f));
                            }
                            else
                            {
                                ChangeState(UnitStates.Idle);
                                break;
                            }

                            currentState = newState;
                            break;

                        case UnitStates.Attack:

                            if (!checkedStartingHex)
                            {
                                currentTargetKilled = true;
                                targetCheck = FindClosestTarget(BoardController.Instance.allOccupyingObjects);
                                checkedStartingHex = true;
                            }

                            if (FloatToHexDistance(Vector3.Distance(gameObject.transform.position, targetCheck.transform.position)) <= unitAttributes.unitBehaviors.unitAttackRange)
                            {
                                if (currentTargetKilled)
                                {
                                    currentTarget = targetCheck;
                                    currentTargetKilled = false;
                                }

                                StartCoroutine(AttackTarget(currentTarget));
                            }
                            else
                            {
                                ChangeState(UnitStates.Move);
                                break;
                            }

                            currentState = newState;
                            break;

                        default:
                            ConsoleLogPlus("Unknown Unit State: " + currentState + ", Unit: " + gameObject.name);
                            break;
                    }
                //}
                //else 
                //{
                //    ConsoleLogPlus("Cannot grab unit during battle. (Current game state: " + GameController.Instance.currentState + ")");
                //}
            }
            else 
            {
                ConsoleLogPlus("Cannot change state to " + newState + ", " + gameObject.name + " is dead. (isDead = " + isDead + ")");
            }
        }

        public void TakeDamage(float rawDamage) 
        {
            currentHealth -= rawDamage;

            if (currentHealth <= 0)
            {
                ChangeState(UnitStates.Idle);
                UnitDie();
            }
            else
            {
                StartCoroutine(TakeHit());
            }
        }

        public int FloatToHexDistance(float distance) 
        {
            float tempFloat;
            float tempMod;
            int tempInt;

            tempFloat = distance / hexDiameter;

            tempMod = tempFloat % 1;

            if (tempFloat % 1 < floatToHexErrorRange)
            {
                if (Mathf.Abs(floatToHexErrorRange - tempMod) > floatToHexErrorRangeHigh || Mathf.Abs(floatToHexErrorRange - tempMod) < floatToHexErrorRangeLow)
                {
                    tempInt = (int)tempFloat;
                }
                else 
                {
                    tempInt = ((int)tempFloat) + 1;
                }
            }
            else if (tempFloat % 1 > 1.0f - floatToHexErrorRange) 
            {
                tempInt = ((int)tempFloat) + 1;
            }
            else
            {
                tempInt = ((int)tempFloat) - 1;
            }

            return tempInt;
        }

        #endregion
        #region Private Methods

        private void UnitIdle() 
        {
            if (onMouse && Input.GetKeyDown(KeyCode.Mouse0))
            {
                ChangeState(UnitStates.Grabbed);
            }
        }

        private void UnitGrabbed() 
        {
            if (Input.GetKeyUp(KeyCode.Mouse0)) 
            {
                onMouse = false;
                ChangeState(UnitStates.Idle);
            }
        }

        private void UnitMove() 
        {
            if (!movingToHex)
            {
                hexesAlongPath++;

                if (FloatToHexDistance(Vector3.Distance(gameObject.transform.position, targetCheck.transform.position)) <= unitAttributes.unitBehaviors.unitAttackRange)
                {
                    if (!targetCheck.GetComponent<UnitController>().isDead)
                    {
                        //go back to attack
                        ChangeState(UnitStates.Attack);
                    }
                    else 
                    {
                        ChangeState(UnitStates.Idle);
                    }
                }
                else 
                {
                    //move to next tile
                    movingToHex = true;
                    StartCoroutine(UnitMoveToHex(pathToCurrentTarget[hexesAlongPath], 0.5f));
                }
            }
        }

        private void UnitAttack() 
        {
            if (currentTargetKilled) 
            {
                ChangeState(UnitStates.Move);
            }
        }

        private void UnitDie() 
        {
            unitMeshRenderer.material = deadMaterial;
            isDead = true;
        }

        private IEnumerator TakeHit()
        {
            unitMeshRenderer.material = hurtMaterial;
            yield return Wait(hurtTime);
            unitMeshRenderer.material = defaultMaterial;
        }

        private void MoveUnitToTileFromGrabbed(bool SendToLastTile = false) 
        {
            TileController targetTile;

            targetTile = currentTile;

            if (SendToLastTile) 
            {
                targetTile = lastTile;
            }

            transform.parent = GameController.Instance.unitHolder.transform;
            transform.position = targetTile.transform.position + new Vector3(0, tileHeightOffset, 0);

            lastTile.UpdateOccupiedUnit(null);
            targetTile.UpdateOccupiedUnit(gameObject);

            targetTile.UpdateHoverMaterial(invisible:true);

            currentTile = targetTile;
        }

        private IEnumerator UnitMoveToHex(TileController endTile, float timeToMove) 
        {
            movingToHex = true;
            float time = 0.0f;

            Vector3 startPos = transform.position;

            while (time < timeToMove) 
            {
                transform.position = Vector3.Lerp(startPos, endTile.parentObject.transform.position + new Vector3(0, tileHeightOffset, 0), time / timeToMove);
                time += Time.deltaTime;
                yield return null;
            }

            transform.position = endTile.parentObject.transform.position + new Vector3(0, tileHeightOffset, 0);

            lastTile = currentTile;
            lastTile.isOccupied = false;
            lastTile.occupyingUnit = gameObject;

            currentTile = endTile;
            currentTile.isOccupied = true;
            currentTile.occupyingUnit = gameObject;

            movingToHex = false;
        }

        private IEnumerator AttackAllTargetsInRange() 
        {
            CheckForTargetInRange();

            while (targetsInRange.Count > 0) 
            {
                yield return AttackTarget(currentTarget);
            }
        }

        private IEnumerator AttackTarget(GameObject target) 
        {
            //need to handle target moving out of attack range during attack, for now attack them no matter where they go
            //movingToHex = false;

            if (target.TryGetComponent<UnitController>(out UnitController tempUnitHolder))
            {
                while (tempUnitHolder.currentHealth > 0)
                {
                    //if (FloatToHexDistance(Vector3.Distance(gameObject.transform.position, targetCheck.transform.position)) <= unitAttributes.unitBehaviors.unitAttackRange)
                    //{
                        ConsoleLogPlus("Unit " + gameObject.name + "attacked " + tempUnitHolder.name + " for " + testDamage + " damage");

                        tempUnitHolder.TakeDamage(testDamage);
                        yield return StartCoroutine(Wait(1.0f / unitAttributes.unitBehaviors.unitAttackSpeed));
                    //}

                    /*else
                    {
                        //follow target

                        //ChangeState(UnitStates.Move);
                        //yield return null;
                        
                        while (FloatToHexDistance(Vector3.Distance(gameObject.transform.position, targetCheck.transform.position)) <= unitAttributes.unitBehaviors.unitAttackRange)
                        {
                            while (!movingToHex)
                            {
                                StartCoroutine(UnitMoveToHex(FindClosestNeighborToPath(currentTile, tempUnitHolder.currentTile), 0.5f));
                            }
                        }
                    }*/
                }

                currentTargetKilled = true;
            }

            yield return null;
        }

        private GameObject CheckForTargetInRange() 
        {
            GameObject target;
            bool targetFound = false;

            targetCollidersInRange = Physics.OverlapSphere(gameObject.transform.position, attackRangeStart + ((unitAttributes.unitBehaviors.unitAttackRange + 1) * 2), layerMask:1<<7, queryTriggerInteraction:QueryTriggerInteraction.Collide);

            if (targetCollidersInRange.Length > 0) 
            {
                foreach (Collider potentialTarget in targetCollidersInRange)
                {
                    if (potentialTarget.TryGetComponent<UnitController>(out UnitController tempHolder))
                    {
                        if (tempHolder.currentTeam != currentTeam) 
                        {
                            targetsInRange.Add(potentialTarget.gameObject);
                            targetFound = true;
                        }
                    }
                }
            }

            if (targetFound)
            {
                target = FindClosestTarget(targetsInRange);
            }
            else 
            {
                target = null;
            }

            currentTarget = target;

            return target;
        }

        private GameObject FindClosestTarget(List<GameObject> targets) 
        {
            GameObject closestInteract = null;
            UnitController tempUnit;
            float closestDistance = 0.0f;

            foreach (GameObject potentialTarget in targets)
            {
                if (potentialTarget != gameObject) 
                {
                    if (closestInteract != null)
                    {
                        if (potentialTarget.TryGetComponent<UnitController>(out tempUnit))
                        {
                            if (!tempUnit.isDead && tempUnit.currentTeam != currentTeam)
                            {
                                float tempDistance = Mathf.Abs(Vector3.Distance(gameObject.transform.position, potentialTarget.gameObject.transform.position));

                                if (tempDistance < closestDistance)
                                {
                                    closestInteract = potentialTarget;
                                    closestDistance = tempDistance;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (potentialTarget.TryGetComponent<UnitController>(out tempUnit))
                        {
                            if (!tempUnit.isDead && tempUnit.currentTeam != currentTeam)
                            {
                                closestInteract = potentialTarget;
                                closestDistance = Mathf.Abs(Vector3.Distance(gameObject.transform.position, potentialTarget.gameObject.transform.position));
                            }
                        }
                    }
                }
            }

            if (closestInteract != null) 
            {
                targetHex = closestInteract.GetComponent<UnitController>().currentTile;
                pathToCurrentTarget = FindPathToHex(targetHex);
            }

            return closestInteract;
        }

        private List<TileController> FindPathToHex(TileController pathEnd) 
        {
            List<TileController> tempList = new();
            TileController thisTile, nextTile;

            thisTile = currentTile;
            nextTile = FindClosestNeighborToPath(thisTile, pathEnd);

            if (nextTile != pathEnd)
            {
                tempList.Add(nextTile);
            }

            distanceToCurrentTarget = Vector3.Distance(gameObject.transform.position, pathEnd.occupyingUnit.transform.position);
            hexDistanceToTarget = FloatToHexDistance(distanceToCurrentTarget);

            for (int i = 0; i < hexDistanceToTarget - 1; i++) 
            {
                thisTile = nextTile;
                nextTile = FindClosestNeighborToPath(thisTile, pathEnd);

                if (nextTile != pathEnd)
                {
                    tempList.Add(nextTile);
                }
            }

            return tempList;
        }

        private TileController FindClosestNeighborToPath(TileController tileToSearch, TileController pathEnd)
        {
            TileController tempTile = null;
            float tempDistance = 10000.0f;
            float shortestDistance = 0.0f;

            if (tileToSearch.neighborControllers.Count > 0) 
            {
                foreach (TileController neighbor in tileToSearch.neighborControllers) 
                {
                    shortestDistance = Mathf.Abs(Vector3.Distance(pathEnd.parentObject.transform.position, neighbor.parentObject.transform.position));
                    if (shortestDistance < tempDistance) 
                    {
                        tempDistance = shortestDistance;
                        tempTile = neighbor;
                    }
                }
            }

            return tempTile;
        }

        #endregion
        #region Other

        public IEnumerator Wait(float sec) 
        {
            yield return new WaitForSeconds(sec);
        }

        private void ConsoleLogPlus(string output, [CallerLineNumber] int l = 0, [CallerMemberName] string m = "")
        {
            Debug.Log("Line " + l + " in method: " + m + "\n" + output);
        }

        #endregion
    }
}
