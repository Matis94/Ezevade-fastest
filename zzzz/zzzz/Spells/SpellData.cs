using System;
using Aimtec;

//using SharpDX;

namespace zzzz
{
    public enum SpellType
    {
        Line,
        Circular,
        Cone,
        Arc,
        None
    }

    public enum CollisionObjectType
    {
        EnemyChampions,
        EnemyMinions,
        YasuoWall
    }

    public class SpellData : ICloneable
    {
        public float angle;
        public string charName;
        public CollisionObjectType[] collisionObjects = { };
        public int dangerlevel = 1;
        public bool defaultOff = false;
        public float extraDelay = 0;
        public float extraDistance = 0;
        public float extraDrawHeight = 0;
        public float extraEndTime = 0;
        public string[] extraMissileNames = { };
        public string[] extraSpellNames = { };
        public bool fixedRange = false;
        public bool hasEndExplosion = false;
        public bool hasTrap = false;
        public bool invert = false;
        public bool isPerpendicular = false;
        public bool isSpecial = false;
        public bool isThreeWay = false;
        public bool isWall = false;
        public string missileName = "";
        public string name;
        public bool noProcess = false;
        public float projectileSpeed = float.MaxValue;
        public float radius;
        public float range;
        public float secondaryRadius;
        public float sideRadius;
        public float spellDelay = 250;
        public SpellSlot spellKey = SpellSlot.Q;
        public string spellName;
        public SpellType spellType;
        public string trapBaseName = "";
        public string trapTroyName = "";
        public bool updatePosition = true;

        public bool useEndPosition = false;

        //public int splits; no idea when this was added xd
        public bool usePackets = false;

        public SpellData()
        {
        }

        public SpellData(
            string charName,
            string spellName,
            string name,
            int range,
            int radius,
            int dangerlevel,
            SpellType spellType
        )
        {
            this.charName = charName;
            this.spellName = spellName;
            this.name = name;
            this.range = range;
            this.radius = radius;
            this.dangerlevel = dangerlevel;
            this.spellType = spellType;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}