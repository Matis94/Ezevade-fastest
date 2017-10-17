using Aimtec;
using Aimtec.SDK.Extensions;

//using SharpDX;

namespace zzzz.SpecialSpells
{
    internal class Lucian : ChampionPlugin
    {
        public void LoadSpecialSpell(SpellData spellData)
        {
            if (spellData.spellName == "LucianQ")
                SpellDetector.OnProcessSpecialSpell += ProcessSpell_LucianQ;
        }

        private static void ProcessSpell_LucianQ(Obj_AI_Base hero, Obj_AI_BaseMissileClientDataEventArgs args,
            SpellData spellData, SpecialSpellEventArgs specialSpellArgs)
        {
            if (spellData.spellName == "LucianQ")
                if (args.Target.IsValid && args.Target.Type == GameObjectType.obj_AI_Base)
                {
                    var target = args.Target as Obj_AI_Base;

                    var spellDelay = (350 - ObjectCache.gamePing) / 1000;
                    var heroWalkDir = (target.ServerPosition - target.Position).Normalized();
                    var predictedHeroPos = target.Position + heroWalkDir * target.MoveSpeed * spellDelay;


                    SpellDetector.CreateSpellData(hero, args.Start, predictedHeroPos, spellData, null, 0);

                    specialSpellArgs.noProcess = true;
                }
        }
    }
}