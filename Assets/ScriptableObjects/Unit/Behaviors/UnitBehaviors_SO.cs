using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

namespace ExoDev.AutoBattler
{
    [CreateAssetMenu(fileName = "UnitBehaviors", menuName = "ScriptableObjects/UnitBehaviors", order = 2)]
    public class UnitBehaviors_SO : ScriptableObject
    {
        #region Variables

        [Title("Movement")]
        [Tooltip("Time to move between tiles.")]
        public float unitMoveSpeed;

        [Title("Combat")]
        public bool isMelee;

        [Tooltip("Hexes of attack range. (Melee = 1)")]
        public int unitAttackRange;

        [Tooltip("Number of attacks per second.")]
        public float unitAttackSpeed;

        //[Header("Abilities")]
        //List<AbilityStats_SO> unitAbilites;

        #endregion
        #region Public Methods



        #endregion
        #region Private Methods



        #endregion
    }
}
