//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Aimtec; using Aimtec.SDK.Util.Cache;
//using Aimtec.SDK;

//namespace zzzz
//{
//    using Aimtec.SDK.Extensions;
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;

//    /// <summary>
//    ///     Delegate for the event <see cref="Interrupter.OnPossibleToInterrupt" />
//    /// </summary>
//    /// <param name="unit">The unit.</param>
//    /// <param name="spell">The spell.</param>
//    public delegate void OnPossibleToInterruptH(Obj_AI_Hero unit, InterruptableSpell spell);

//    /// <summary>
//    ///     The danger level.
//    /// </summary>
//    public enum InterruptableDangerLevel
//    {
//        /// <summary>
//        ///     The low
//        /// </summary>
//        Low,

//        /// <summary>
//        ///     The medium
//        /// </summary>
//        Medium,

//        /// <summary>
//        ///     The high
//        /// </summary>
//        High,
//    }

//    /// <summary>
//    ///     Represents an interruptable spell.
//    /// </summary>
//    public struct InterruptableSpell
//    {
//        #region Fields

//        /// <summary>
//        ///     The buff name
//        /// </summary>
//        public string BuffName;

//        /// <summary>
//        ///     The champion name
//        /// </summary>
//        public string ChampionName;

//        /// <summary>
//        ///     The danger level
//        /// </summary>
//        public InterruptableDangerLevel DangerLevel;

//        /// <summary>
//        ///     The extra duration
//        /// </summary>
//        public int ExtraDuration;

//        /// <summary>
//        ///     The slot
//        /// </summary>
//        public SpellSlot Slot;

//        /// <summary>
//        ///     The spell name
//        /// </summary>
//        public string SpellName;

//        #endregion
//    }

//    /// <summary>
//    ///     This class allows you to easily interrupt interruptable spells like Katarina's ult.
//    /// </summary>
//    [Obsolete("Use Interrupter2", false)]
//    public static class Interrupter
//    {
//        #region Static Fields

//        /// <summary>
//        ///     The spells
//        /// </summary>
//        public static List<InterruptableSpell> Spells = new List<InterruptableSpell>();

//        #endregion

//        #region Constructors and Destructors

//        /// <summary>
//        ///     Initializes static members of the <see cref="Interrupter" /> class.
//        /// </summary>
//        static Interrupter()
//        {
//            #region Varus

//            Spells.Add(
//                new InterruptableSpell
//                {
//                    ChampionName = "Varus",
//                    SpellName = "VarusQ",
//                    DangerLevel = InterruptableDangerLevel.Low,
//                    Slot = SpellSlot.Q,
//                    BuffName = "VarusQ"
//                });

//            #endregion

//            #region Urgot

//            Spells.Add(
//                new InterruptableSpell
//                {
//                    ChampionName = "Urgot",
//                    SpellName = "UrgotSwap2",
//                    DangerLevel = InterruptableDangerLevel.High,
//                    Slot = SpellSlot.R,
//                    BuffName = "UrgotSwap2"
//                });

//            #endregion

//            #region Caitlyn

//            Spells.Add(
//                new InterruptableSpell
//                {
//                    ChampionName = "Caitlyn",
//                    SpellName = "CaitlynAceintheHole",
//                    DangerLevel = InterruptableDangerLevel.High,
//                    Slot = SpellSlot.R,
//                    BuffName = "CaitlynAceintheHole",
//                    ExtraDuration = 600
//                });

//            #endregion

//            #region Warwick

//            Spells.Add(
//                new InterruptableSpell
//                {
//                    ChampionName = "Warwick",
//                    SpellName = "InfiniteDuress",
//                    DangerLevel = InterruptableDangerLevel.High,
//                    Slot = SpellSlot.R,
//                    BuffName = "infiniteduresssound"
//                });

//            #endregion

//            #region Shen

//            Spells.Add(
//                new InterruptableSpell
//                {
//                    ChampionName = "Shen",
//                    SpellName = "ShenStandUnited",
//                    DangerLevel = InterruptableDangerLevel.Low,
//                    Slot = SpellSlot.R,
//                    BuffName = "shenstandunitedlock"
//                });

//            #endregion

//            #region Malzahar

//            Spells.Add(
//                new InterruptableSpell
//                {
//                    ChampionName = "Malzahar",
//                    SpellName = "AlZaharNetherGrasp",
//                    DangerLevel = InterruptableDangerLevel.High,
//                    Slot = SpellSlot.R,
//                    BuffName = "alzaharnethergraspsound",
//                    ExtraDuration = 2000
//                });

//            #endregion

//            #region Nunu

//            Spells.Add(
//                new InterruptableSpell
//                {
//                    ChampionName = "Nunu",
//                    SpellName = "AbsoluteZero",
//                    DangerLevel = InterruptableDangerLevel.High,
//                    Slot = SpellSlot.R,
//                    BuffName = "AbsoluteZero",
//                });

//            #endregion

//            #region Pantheon

//            Spells.Add(
//                new InterruptableSpell
//                {
//                    ChampionName = "Pantheon",
//                    SpellName = "PantheonRJump",
//                    DangerLevel = InterruptableDangerLevel.High,
//                    Slot = SpellSlot.R,
//                    BuffName = "PantheonRJump"
//                });

//            #endregion

//            #region Karthus

//            Spells.Add(
//                new InterruptableSpell
//                {
//                    ChampionName = "Karthus",
//                    SpellName = "KarthusFallenOne",
//                    DangerLevel = InterruptableDangerLevel.High,
//                    Slot = SpellSlot.R,
//                    BuffName = "karthusfallenonecastsound"
//                });

//            #endregion

//            #region Velkoz

//            Spells.Add(
//                new InterruptableSpell
//                {
//                    ChampionName = "Velkoz",
//                    SpellName = "VelkozR",
//                    DangerLevel = InterruptableDangerLevel.High,
//                    Slot = SpellSlot.R,
//                    BuffName = "VelkozR",
//                });

//            #endregion

//            #region Galio

//            Spells.Add(
//                new InterruptableSpell
//                {
//                    ChampionName = "Galio",
//                    SpellName = "GalioIdolOfDurand",
//                    DangerLevel = InterruptableDangerLevel.High,
//                    Slot = SpellSlot.R,
//                    BuffName = "GalioIdolOfDurand",
//                    ExtraDuration = 200,
//                });

//            #endregion

//            #region MissFortune

//            Spells.Add(
//                new InterruptableSpell
//                {
//                    ChampionName = "MissFortune",
//                    SpellName = "MissFortuneBulletTime",
//                    DangerLevel = InterruptableDangerLevel.High,
//                    Slot = SpellSlot.R,
//                    BuffName = "missfortunebulletsound",
//                });

//            #endregion

//            #region Fiddlesticks

//            Spells.Add(
//                new InterruptableSpell
//                {
//                    ChampionName = "FiddleSticks",
//                    SpellName = "Drain",
//                    DangerLevel = InterruptableDangerLevel.Medium,
//                    Slot = SpellSlot.W,
//                    BuffName = "Drain",
//                });
//            //Max rank Drain had different buff name
//            Spells.Add(
//                new InterruptableSpell
//                {
//                    ChampionName = "FiddleSticks",
//                    SpellName = "Drain",
//                    DangerLevel = InterruptableDangerLevel.Medium,
//                    Slot = SpellSlot.W,
//                    BuffName = "fearmonger_marker",
//                });
//            /*  Crowstorm buffname only appears after finish casting.
//            Spells.Add(
//                new InterruptableSpell
//                {
//                    ChampionName = "FiddleSticks",
//                    SpellName = "Crowstorm",
//                    DangerLevel = InterruptableDangerLevel.High,
//                    Slot = SpellSlot.R,
//                    BuffName = "Crowstorm",
//                });*/

//            #endregion

//            #region Katarina

//            Spells.Add(
//                new InterruptableSpell
//                {
//                    ChampionName = "Katarina",
//                    SpellName = "KatarinaR",
//                    DangerLevel = InterruptableDangerLevel.High,
//                    Slot = SpellSlot.R,
//                    BuffName = "katarinarsound"
//                });

//            #endregion

//            #region MasterYi

//            Spells.Add(
//                new InterruptableSpell
//                {
//                    ChampionName = "MasterYi",
//                    SpellName = "Meditate",
//                    BuffName = "Meditate",
//                    Slot = SpellSlot.W,
//                    DangerLevel = InterruptableDangerLevel.Low,
//                });

//            #endregion

//            #region Xerath

//            Spells.Add(
//                new InterruptableSpell
//                {
//                    ChampionName = "Xerath",
//                    SpellName = "XerathLocusOfPower2",
//                    BuffName = "XerathLocusOfPower2",
//                    Slot = SpellSlot.R,
//                    DangerLevel = InterruptableDangerLevel.Low,
//                });

//            #endregion

//            #region Janna

//            Spells.Add(
//                new InterruptableSpell
//                {
//                    ChampionName = "Janna",
//                    SpellName = "ReapTheWhirlwind",
//                    BuffName = "ReapTheWhirlwind",
//                    Slot = SpellSlot.R,
//                    DangerLevel = InterruptableDangerLevel.Low,
//                });

//            #endregion

//            #region Lucian

//            Spells.Add(
//                new InterruptableSpell
//                {
//                    ChampionName = "Lucian",
//                    SpellName = "LucianR",
//                    DangerLevel = InterruptableDangerLevel.High,
//                    Slot = SpellSlot.R,
//                    BuffName = "LucianR"
//                });

//            #endregion

//            #region TwistedFate

//            Spells.Add(
//                new InterruptableSpell
//                {
//                    ChampionName = "TwistedFate",
//                    SpellName = "Destiny",
//                    DangerLevel = InterruptableDangerLevel.Medium,
//                    Slot = SpellSlot.R,
//                    BuffName = "Destiny"
//                });

//            #endregion

//            Initialize();
//        }

//        #endregion

//        #region Public Events

//        [Obsolete("Use Interrupter2.OnInterruptableTarget", false)]
//        public static event OnPossibleToInterruptH OnPossibleToInterrupt;

//        #endregion

//        #region Public Methods and Operators

//        public static void Initialize()
//        {
//            Game.OnUpdate += Game_OnGameUpdate;
//        }

//        /// <summary>
//        ///     Determines whether the unit is channeling an important spell.
//        /// </summary>
//        /// <param name="unit">The unit.</param>
//        /// <returns></returns>
//        public static bool IsChannelingImportantSpell(this Obj_AI_Hero unit)
//        {
//            return
//                Spells.Any(
//                    spell =>
//                    spell.ChampionName == unit.ChampionName
//                    && ((unit.spell != null
//                         && String.Equals(
//                             unit.LastCastedspell().Name,
//                             spell.SpellName,
//                             StringComparison.CurrentCultureIgnoreCase)
//                         && EvadeUtils.TickCount - unit.LastCastedSpellT() < 350 + spell.ExtraDuration)
//                        || (spell.BuffName != null && unit.HasBuff(spell.BuffName))
//                        || (unit.IsMe && LastCastedSpell.LastCastPacketSent != null
//                            && LastCastedSpell.LastCastPacketSent.Slot == spell.Slot
//                            && EvadeUtils.TickCount - LastCastedSpell.LastCastPacketSent.Tick < 150 + Game.Ping)));
//        }

//        public static void Shutdown()
//        {
//            Game.OnUpdate -= Game_OnGameUpdate;
//        }

//        #endregion

//        #region Methods

//        /// <summary>
//        ///     Fires the on interruptable event.
//        /// </summary>
//        /// <param name="unit">The unit.</param>
//        /// <param name="spell">The spell.</param>
//        private static void FireOnInterruptable(Obj_AI_Hero unit, InterruptableSpell spell)
//        {
//            if (OnPossibleToInterrupt != null)
//            {
//                OnPossibleToInterrupt(unit, spell);
//            }
//        }

//        /// <summary>
//        ///     Fired when the game updates.
//        /// </summary>
//        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
//        private static void Game_OnGameUpdate()
//        {
//            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(e => e.IsValidTarget()))
//            {
//                foreach (var spell in
//                    Spells.Where(
//                        spell =>
//                        (enemy.LastCastedspell() != null
//                         && String.Equals(
//                             enemy.LastCastedspell().Name,
//                             spell.SpellName,
//                             StringComparison.CurrentCultureIgnoreCase)
//                         && Utils.TickCount - enemy.LastCastedSpellT() < 350 + spell.ExtraDuration)
//                        || (!string.IsNullOrEmpty(spell.BuffName) && enemy.HasBuff(spell.BuffName))))
//                {
//                    FireOnInterruptable(enemy, spell);
//                }
//            }
//        }

//        #endregion
//    }
//}

//class Extensions
//{


//}

using System.Collections.Generic;
using Aimtec;

namespace zzzz
{
    internal static class Extensions
    {
        public static bool IsOnScreen(this Vector3 vector)
        {
            Vector2 screen;
            Render.WorldToScreen(vector, out screen);

            if (screen.X < 0 || screen.X > Render.Width || screen.Y < 0 || screen.Y > Render.Height)
                return false;
            return true;
        }

        public static bool IsOnScreen(this Vector2 vector)
        {
            var screen = vector;
            if (screen.X < 0 || screen.X > Render.Width || screen.Y < 0 || screen.Y > Render.Height)
                return false;
            return true;
        }

        //public static SharpDX.Vector3 SetZ(this SharpDX.Vector3 vector, float value)
        //{
        //    vector.Z = value;
        //    return vector;
        //}

        public static Vector3 SetZ(this Vector3 vector, float value)
        {
            vector.Z = value;
            return vector;
        }

        //public static SharpDX.Vector2 To2D(this SharpDX.Vector3 v)
        //{
        //    return (SharpDX.Vector2)v;
        //}

        public static Vector3 Perpendicular(this Vector3 v)
        {
            return new Vector3(-v.Z, v.Y, v.X);
        }
        //    //var spellDir = spell.direction;
        //    var myBoundingRadius = ObjectManager.GetLocalPlayer().BoundingRadius;
        //{

        //public static bool LineIntersectsLine(this Vector3 vector, Vector3 dir, Vector2 a, Vector2 b, out Vector2 intersection) //edited
        //    var direction = dir;
        //    var pSpellDir = direction.Perpendicular();
        //    //var spellRadius = spell.radius;
        //    var spellPos = spell.currentSpellPosition - direction * myBoundingRadius; //leave some space at back of spell
        //    var endPos = spell.GetSpellEndPosition() + direction * myBoundingRadius; //leave some space at the front of spell

        //    var startRightPos = spellPos + pSpellDir * (spellRadius + myBoundingRadius);
        //    var startLeftPos = spellPos - pSpellDir * (spellRadius + myBoundingRadius);
        //    var endRightPos = endPos + pSpellDir * (spellRadius + myBoundingRadius);
        //    var endLeftPos = endPos - pSpellDir * (spellRadius + myBoundingRadius);

        //    List<Vector2Extensions.IntersectionResult> intersects = new List<Vector2Extensions.IntersectionResult>();
        //    Vector2 heroPos = ObjectManager.GetLocalPlayer().ServerPosition.To2D();

        //    intersects.Add(a.Intersection(b, startRightPos, startLeftPos));
        //    intersects.Add(a.Intersection(b, endRightPos, endLeftPos));
        //    intersects.Add(a.Intersection(b, startRightPos, endRightPos));
        //    intersects.Add(a.Intersection(b, startLeftPos, endLeftPos));

        //    var sortedIntersects = intersects.Where(i => i.Intersects).OrderBy(i => i.Point.Distance(heroPos)); //Get first intersection

        //    if (sortedIntersects.Count() > 0)
        //    {
        //        intersection = sortedIntersects.First().Point;
        //        return true;
        //    }

        //    intersection = Vector2.Zero;
        //    return false;
        //}

        //public static Size GetTextExtent(string text)
        //{
        //    Graphics g;
        //    Font font = new Font(FontFamily.GenericSansSerif, 10);
        //    g.MeasureString(text, font);
        //}
    }


    /// <summary>
    ///     Represents a last casted spell.
    /// </summary>
    public class LastCastedSpellEntry
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="LastCastedSpellEntry" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="tick">The tick.</param>
        /// <param name="target">The target.</param>
        public LastCastedSpellEntry(string name, int tick, Obj_AI_Base target)
        {
            Name = name;
            Tick = tick;
            Target = target;
        }

        #endregion

        #region Fields

        /// <summary>
        ///     The name
        /// </summary>
        public string Name;

        /// <summary>
        ///     The target
        /// </summary>
        public Obj_AI_Base Target;

        /// <summary>
        ///     The tick
        /// </summary>
        public int Tick;

        #endregion
    }

    /// <summary>
    ///     Represents the last cast packet sent.
    /// </summary>
    public class LastCastPacketSentEntry
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="LastCastPacketSentEntry" /> class.
        /// </summary>
        /// <param name="slot">The slot.</param>
        /// <param name="tick">The tick.</param>
        /// <param name="targetNetworkId">The target network identifier.</param>
        public LastCastPacketSentEntry(SpellSlot slot, int tick, int targetNetworkId)
        {
            Slot = slot;
            Tick = tick;
            TargetNetworkId = targetNetworkId;
        }

        #endregion

        #region Fields

        /// <summary>
        ///     The slot
        /// </summary>
        public SpellSlot Slot;

        /// <summary>
        ///     The target network identifier
        /// </summary>
        public int TargetNetworkId;

        /// <summary>
        ///     The tick
        /// </summary>
        public int Tick;

        #endregion
    }

    /// <summary>
    ///     Gets the last casted spell of the unit.
    /// </summary>
    public static class LastCastedSpell
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes static members of the <see cref="LastCastedSpell" /> class.
        /// </summary>
        static LastCastedSpell()
        {
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            SpellBook.OnCastSpell += OnCastSpell;
        }

        #endregion

        #region Static Fields

        /// <summary>
        ///     The last cast packet sent
        /// </summary>
        public static LastCastPacketSentEntry LastCastPacketSent;

        /// <summary>
        ///     The casted spells
        /// </summary>
        internal static readonly Dictionary<int, LastCastedSpellEntry> CastedSpells =
            new Dictionary<int, LastCastedSpellEntry>();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Gets the last casted spell.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <returns></returns>
        public static LastCastedSpellEntry LastCastedspell(this Obj_AI_Hero unit)
        {
            return CastedSpells.ContainsKey(unit.NetworkId) ? CastedSpells[unit.NetworkId] : null;
        }

        /// <summary>
        ///     Gets the last casted spell name.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <returns></returns>
        public static string LastCastedSpellName(this Obj_AI_Hero unit)
        {
            return CastedSpells.ContainsKey(unit.NetworkId) ? CastedSpells[unit.NetworkId].Name : string.Empty;
        }

        /// <summary>
        ///     Gets the last casted spell tick.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <returns></returns>
        public static int LastCastedSpellT(this Obj_AI_Hero unit)
        {
            return CastedSpells.ContainsKey(unit.NetworkId)
                ? CastedSpells[unit.NetworkId].Tick
                : (Game.TickCount > 0 ? 0 : int.MinValue);
        }

        /// <summary>
        ///     Gets the last casted spell's target.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <returns></returns>
        public static Obj_AI_Base LastCastedSpellTarget(this Obj_AI_Hero unit)
        {
            return CastedSpells.ContainsKey(unit.NetworkId) ? CastedSpells[unit.NetworkId].Target : null;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Fired when the game processes the spell cast.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="Obj_AI_BaseMissileClientDataEventArgs" /> instance containing the event data.</param>
        private static void OnProcessSpellCast(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs args)
        {
            if (sender is Obj_AI_Hero)
            {
                var entry = new LastCastedSpellEntry(args.SpellData.Name, Game.TickCount,
                    ObjectManager.GetLocalPlayer());
                if (CastedSpells.ContainsKey(sender.NetworkId))
                    CastedSpells[sender.NetworkId] = entry;
                else
                    CastedSpells.Add(sender.NetworkId, entry);
            }
        }

        /// <summary>
        ///     Fired then a spell is casted.
        /// </summary>
        /// <param name="spellbook">The spellbook.</param>
        /// <param name="args">The <see cref="SpellbookCastSpellEventArgs" /> instance containing the event data.</param>
        private static void OnCastSpell(Obj_AI_Base sender, SpellBookCastSpellEventArgs spellBookCastSpellEventArgs)
        {
            if (sender.IsMe)
                LastCastPacketSent = new LastCastPacketSentEntry(
                    spellBookCastSpellEventArgs.Slot,
                    Game.TickCount,
                    spellBookCastSpellEventArgs.Target is Obj_AI_Base
                        ? spellBookCastSpellEventArgs.Target.NetworkId
                        : 0);
        }

        #endregion
    }
}