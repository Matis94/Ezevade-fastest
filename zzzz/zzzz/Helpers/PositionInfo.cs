using System;
using System.Collections.Generic;
using Aimtec;
using Aimtec.SDK.Extensions;

//using SharpDX;

namespace zzzz
{
    public class PositionInfo
    {
        public float closestDistance = float.MaxValue;
        public float distanceToMouse;
        public List<int> dodgeableSpells = new List<int>();
        public float endTime = 0;
        public bool hasComfortZone = true;
        public bool hasExtraDistance = false;
        public float intersectionTime = float.MaxValue;
        public bool isDangerousPos;
        public int posDangerCount;

        public int posDangerLevel;
        public float posDistToChamps = float.MaxValue;
        public Vector2 position;
        public bool recalculatedPath = false;
        public bool rejectPosition = false;
        public float speed = 0;
        public List<int> spellList = new List<int>();
        public Obj_AI_Base target = null;
        public float timestamp;
        public List<int> undodgeableSpells = new List<int>();

        public PositionInfo(
            Vector2 position,
            int posDangerLevel,
            int posDangerCount,
            bool isDangerousPos,
            float distanceToMouse,
            List<int> dodgeableSpells,
            List<int> undodgeableSpells)
        {
            this.position = position;
            this.posDangerLevel = posDangerLevel;
            this.posDangerCount = posDangerCount;
            this.isDangerousPos = isDangerousPos;
            this.distanceToMouse = distanceToMouse;
            this.dodgeableSpells = dodgeableSpells;
            this.undodgeableSpells = undodgeableSpells;
            timestamp = EvadeUtils.TickCount;
        }

        public PositionInfo(
            Vector2 position,
            bool isDangerousPos,
            float distanceToMouse)
        {
            this.position = position;
            this.isDangerousPos = isDangerousPos;
            this.distanceToMouse = distanceToMouse;
            timestamp = EvadeUtils.TickCount;
        }

        private static Obj_AI_Hero myHero => ObjectManager.GetLocalPlayer();

        public static PositionInfo SetAllDodgeable()
        {
            return SetAllDodgeable(myHero.Position.To2D());
        }

        public static PositionInfo SetAllDodgeable(Vector2 position)
        {
            var dodgeableSpells = new List<int>();
            var undodgeableSpells = new List<int>();

            foreach (var entry in SpellDetector.spells)
            {
                var spell = entry.Value;
                dodgeableSpells.Add(entry.Key);
            }

            return new PositionInfo(
                position,
                0,
                0,
                true,
                0,
                dodgeableSpells,
                undodgeableSpells);
        }

        public static PositionInfo SetAllUndodgeable()
        {
            var dodgeableSpells = new List<int>();
            var undodgeableSpells = new List<int>();

            var posDangerLevel = 0;
            var posDangerCount = 0;

            foreach (var entry in SpellDetector.spells)
            {
                var spell = entry.Value;
                undodgeableSpells.Add(entry.Key);

                var spellDangerLevel = spell.dangerlevel;

                posDangerLevel = Math.Max(posDangerLevel, spellDangerLevel);
                posDangerCount += spellDangerLevel;
            }

            return new PositionInfo(
                myHero.Position.To2D(),
                posDangerLevel,
                posDangerCount,
                true,
                0,
                dodgeableSpells,
                undodgeableSpells);
        }
    }

    public static class PositionInfoExtensions
    {
        public static Obj_AI_Hero myHero => ObjectManager.GetLocalPlayer();

        public static int GetHighestSpellID(this PositionInfo posInfo)
        {
            if (posInfo == null)
                return 0;

            var highest = 0;

            foreach (var spellID in posInfo.undodgeableSpells)
                highest = Math.Max(highest, spellID);

            foreach (var spellID in posInfo.dodgeableSpells)
                highest = Math.Max(highest, spellID);

            return highest;
        }

        public static bool isSamePosInfo(this PositionInfo posInfo1, PositionInfo posInfo2)
        {
            return new HashSet<int>(posInfo1.spellList).SetEquals(posInfo2.spellList);
        }

        public static bool isBetterMovePos(this PositionInfo newPosInfo)
        {
            PositionInfo posInfo = null;
            var path = myHero.Path;
            if (path.Length > 0)
            {
                var movePos = path[path.Length - 1].To2D();
                posInfo = EvadeHelper.CanHeroWalkToPos(movePos, ObjectCache.myHeroCache.moveSpeed, 0, 0, false);
            }
            else
            {
                posInfo = EvadeHelper.CanHeroWalkToPos(ObjectCache.myHeroCache.serverPos2D,
                    ObjectCache.myHeroCache.moveSpeed, 0, 0, false);
            }

            if (posInfo.posDangerCount < newPosInfo.posDangerCount)
                return false;

            return true;
        }

        public static PositionInfo CompareLastMovePos(this PositionInfo newPosInfo)
        {
            PositionInfo posInfo = null;
            var path = myHero.Path;
            if (path.Length > 0)
            {
                var movePos = path[path.Length - 1].To2D();
                posInfo = EvadeHelper.CanHeroWalkToPos(movePos, ObjectCache.myHeroCache.moveSpeed, 0, 0, false);
            }
            else
            {
                posInfo = EvadeHelper.CanHeroWalkToPos(ObjectCache.myHeroCache.serverPos2D,
                    ObjectCache.myHeroCache.moveSpeed, 0, 0, false);
            }

            if (posInfo.posDangerCount < newPosInfo.posDangerCount)
                return posInfo;

            return newPosInfo;
        }
    }
}