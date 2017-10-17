using Aimtec;

//using SharpDX;

namespace zzzz.SpecialSpells
{
    internal class Twitch : ChampionPlugin
    {
        public void LoadSpecialSpell(SpellData spellData)
        {
            if (spellData.spellName == "TwitchSprayandPrayAttack")
                SpellDetector.OnProcessSpecialSpell += ProcessSpell_TwitchSprayandPrayAttack;
        }

        private void ProcessSpell_TwitchSprayandPrayAttack(Obj_AI_Base hero, Obj_AI_BaseMissileClientDataEventArgs args,
            SpellData spellData, SpecialSpellEventArgs specialSpellArgs)
        {
            if (spellData.spellName == "TwitchSprayandPrayAttack")
                if (args.Target != null)
                {
                    var start = hero.ServerPosition;
                    var end = hero.ServerPosition + (args.Target.Position - hero.ServerPosition) * spellData.range;

                    var data = (SpellData) spellData.Clone();
                    data.spellDelay = hero.AttackCastDelay * 1000;

                    SpellDetector.CreateSpellData(hero, start, end, data);
                }
        }
    }
}