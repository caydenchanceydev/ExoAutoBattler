using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

namespace ExoDev.AutoBattler
{
    [CreateAssetMenu(fileName = "UnitAttributes", menuName = "ScriptableObjects/UnitAttributes", order = 1)]
    public class UnitAttributes_SO : ScriptableObject
    {
        #region Variables

        [Title("Base")]
        public string unitName;

        [Range(1,5)]
        public int unitCost;

        public UnitController.UnitCategories unitCategory;

        [TextArea]
        public string unitDescription;

        [Title("Stats")]
        public float unitBaseHealth;
        public float unitBaseMana;

        [Header("AI")]
        public UnitBehaviors_SO unitBehaviors;

        #endregion
        #region Public Methods



        #endregion
        #region Private Methods



        #endregion
    }
}
