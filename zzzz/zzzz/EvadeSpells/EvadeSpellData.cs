using Aimtec;

//using SharpDX;

namespace zzzz
{
    public delegate bool UseSpellFunc(EvadeSpellData evadeSpell, bool process = true);

    public enum CastType
    {
        Position,
        Target,
        Self
    }

    public enum SpellTargets
    {
        AllyMinions,
        EnemyMinions,

        AllyChampions,
        EnemyChampions,

        Targetables
    }

    public enum EvadeType
    {
        Blink,
        Dash,
        Invulnerability,
        MovementSpeedBuff,
        Shield,
        SpellShield,
        WindWall
    }

    public class EvadeSpellData
    {
        public bool behindTarget = false;
        public CastType castType = CastType.Position;
        public string charName;
        public bool checkSpellName = false;
        public int dangerlevel = 1;
        public EvadeType evadeType;
        public bool fixedRange = false;
        public bool infrontTarget = false;
        public bool isItem = false;
        public bool isReversed = false;
        public bool isSpecial = false;
        public bool isSummonerSpell = false;
        public uint itemID;
        public string name;
        public float range;
        public float speed = 0;
        public float[] speedArray = {0f, 0f, 0f, 0f, 0f};
        public float spellDelay = 250;
        public SpellSlot spellKey = SpellSlot.Q;
        public string spellName;
        public SpellTargets[] spellTargets = { };
        public bool untargetable = false;
        public UseSpellFunc useSpellFunc = null;

        public EvadeSpellData()
        {
        }

        public EvadeSpellData(
            string charName,
            string name,
            SpellSlot spellKey,
            EvadeType evadeType,
            int dangerlevel
        )
        {
            this.charName = charName;
            this.name = name;
            this.spellKey = spellKey;
            this.evadeType = evadeType;
            this.dangerlevel = dangerlevel;
        }
    }
}