using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExoDev.AutoBattler
{
    public class Buff_TileController : TileController
    {
        public enum BuffType { None, MoveSpeed }

        public BuffType buffBehavior;

        public bool isSpawned;
    }
}
