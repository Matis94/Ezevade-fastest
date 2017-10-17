using System.Linq;
using Aimtec;
using Aimtec.SDK.Util.Cache;

//using SharpDX;

namespace zzzz.SpecialSpells
{
    internal class Yasuo : ChampionPlugin
    {
        public void LoadSpecialSpell(SpellData spellData)
        {
            if (spellData.spellName == "YasuoQW" || spellData.spellName == "YasuoQ3W")
            {
                var hero = GameObjects.Heroes.FirstOrDefault(h => h.ChampionName == "Yasuo");
                if (hero != null && hero.CheckTeam())
                    Obj_AI_Base.OnProcessSpellCast += (sender, args) => ProcessSpell_YasuoQW(sender, args, spellData);
            }
        }

        private static void ProcessSpell_YasuoQW(Obj_AI_Base hero, Obj_AI_BaseMissileClientDataEventArgs args,
            SpellData spellData)
        {
            if (hero.IsEnemy && args.SpellData.Name == "YasuoQ")
            {
                // Not sure with castendtime
                var castTime = (hero.SpellBook.CastEndTime - Game.ClockTime) * 1000;

                if (castTime > 0)
                    spellData.spellDelay = castTime;
            }
        }
    }
}