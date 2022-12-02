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
        [SerializeField] bool testFlipper;
        [SerializeField] float testDamage;

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

        //[ReadOnly] 
        public GameController.UnitTeams currentTeam;

        UnitStates lastState;
        [ReadOnly] public UnitStates currentState;

        [SerializeField, ReadOnly] TileController lastTile;
        [ReadOnly] public TileController currentTile;

        [Title("Damage", horizontalLine: false)]
        [ReadOnly] public bool isDead;
        [ReadOnly] MeshRenderer unitMeshRenderer;
        [SerializeField, ReadOnly] Material defaultMaterial;

        [SerializeField, ReadOnly] float hurtTime = 0.5f;

        [Title("Interaction", horizontalLine: false)]
        [ReadOnly] public bool onMouse;

        [Title("Combat", horizontalLine: false)]
        [ReadOnly] public GameObject currentTarget;
        [SerializeField, ReadOnly] float distanceToCurrentTarget;
        [SerializeField, ReadOnly] int hexDistanceToTarget;

        [SerializeField, ReadOnly] float attackRangeColliderRadius;
        [SerializeField, ReadOnly] Collider[] targetCollidersInRange;
        [ReadOnly] public List<GameObject> targetsInRange;
        [SerializeField, ReadOnly] bool thisHexChecked;

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
            if (testFlipper)
            {
                //ChangeState(UnitStates.Attack);
                attackRangeColliderRadius =  attackRangeStart + ((unitAttributes.unitBehaviors.unitAttackRange + 1) * 2);
                BoardController.Instance.UpdateAllTilesList();
                currentTarget = FindClosestTarget(BoardController.Instance.allOccupyingObjects);
                testFlipper = false;
            }

            if (currentTarget != null) 
            {
                distanceToCurrentTarget = Vector3.Distance(gameObject.transform.position, currentTarget.transform.position);
                hexDistanceToTarget = FloatToHexDistance(distanceToCurrentTarget);
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
                lastState = currentState;

                switch (newState)
                {
                    case UnitStates.Idle:
                        if (lastState == UnitStates.Grabbed)
                        {
                            if (currentTile != null && !currentTile.isOccupied)
                            {
                                MoveUnitToTile();
                            }
                            else
                            {
                                MoveUnitToTile(SendToLastTile: true);
                            }
                        }

                        currentState = newState;
                        break;

                    case UnitStates.Grabbed:
                        currentState = newState;

                        transform.parent = GameController.Instance.mousePoint.transform;
                        transform.position += new Vector3(0, grabbedHeightOffset, 0);
                        break;

                    case UnitStates.Move:
                        thisHexChecked = false;

                        currentState = newState;

                        break;

                    case UnitStates.Attack:
                        //Handle attacking from starting hex
                        if (CheckForTargetInRange() != null)
                        {
                            StartCoroutine(AttackAllTargetsInRange());
                        }

                        currentState = newState;
                        break;

                    default:
                        ConsoleLogPlus("Unknown Unit State: " + currentState + ", Unit: " + gameObject.name);
                        break;
                }
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
            //find next closest target
            //calculate how far needed to move (distance between this and enemy -> hexes - attack range) 
                //integer divide distance by 5 and add one then multiply by 5 for distance in hexes away from player tile to unit tile
            //get what hex to move to (choose closest tile in line, same matching distance rules as below)
            //choose closest hex to the straight path (if same always choose hex that is closest to the enemy main objective)
            //move to hex
            //while not at destination, move to next hex
            //once at hex, switch to attck
        }

        private void UnitAttack() 
        {
            if (thisHexChecked && targetsInRange.Count == 0) 
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

        private void MoveUnitToTile(bool SendToLastTile = false) 
        {
            TileController targetTile;

            targetTile = currentTile;

            if (SendToLastTile) 
            {
                targetTile = lastTile;
            }

            transform.parent = GameController.Instance.unitHolder.transform;
            transform.position = targetTile.transform.position + new Vector3(0, tileHeightOffset, 0);

            targetTile.UpdateOccupiedUnit(gameObject);
            lastTile.UpdateOccupiedUnit(null);

            targetTile.UpdateHoverMaterial(invisible:true);

            currentTile = targetTile;
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
            UnitController tempUnitHolder = null;

            if (target.TryGetComponent<UnitController>(out tempUnitHolder)) 
            {
                while (tempUnitHolder.currentHealth > 0)
                {
                    ConsoleLogPlus("Unit " + gameObject.name + "attacked " + tempUnitHolder.name + " for " + testDamage + " damage");

                    tempUnitHolder.TakeDamage(testDamage);
                    yield return StartCoroutine(Wait(1.0f/unitAttributes.unitBehaviors.unitAttackSpeed));
                }
            }

            yield return null;
        }

        private GameObject CheckForTargetInRange() 
        {
            GameObject target = null;
            UnitController tempHolder = null;
            bool targetFound = false;

            targetCollidersInRange = Physics.OverlapSphere(gameObject.transform.position, attackRangeStart + ((unitAttributes.unitBehaviors.unitAttackRange + 1) * 2), layerMask:1<<7, queryTriggerInteraction:QueryTriggerInteraction.Collide);

            if (targetCollidersInRange.Length > 0) 
            {
                foreach (Collider potentialTarget in targetCollidersInRange)
                {
                    if (potentialTarget.TryGetComponent<UnitController>(out tempHolder))
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

            thisHexChecked = true;

            return target;
        }

        private GameObject FindClosestTarget(List<GameObject> targets) 
        {
            GameObject closestInteract = null;
            float closestDistance = 0.0f, tempDistance = 0.0f;

            foreach (GameObject potentialTarget in targets)
            {
                if (potentialTarget != gameObject) 
                {
                    if (closestInteract != null)
                    {
                        tempDistance = Mathf.Abs(Vector3.Distance(gameObject.transform.position, potentialTarget.gameObject.transform.position));

                        if (tempDistance < closestDistance)
                        {
                            closestInteract = potentialTarget;
                            closestDistance = tempDistance;
                        }
                    }
                    else
                    {
                        closestInteract = potentialTarget;
                        closestDistance = Mathf.Abs(Vector3.Distance(gameObject.transform.position, potentialTarget.gameObject.transform.position));
                    }
                }
            }

            return closestInteract;
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
