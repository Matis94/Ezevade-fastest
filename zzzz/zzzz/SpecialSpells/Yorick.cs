using Aimtec;
using Aimtec.SDK.Extensions;

namespace zzzz.SpecialSpells
{
    internal class Yorick : ChampionPlugin
    {
        public void LoadSpecialSpell(SpellData spellData)
        {
            if (spellData.spellName == "YorickE")
                SpellDetector.OnProcessSpecialSpell += SpellDetector_OnProcessSpecialSpell;
        }

        private void SpellDetector_OnProcessSpecialSpell(Obj_AI_Base hero, Obj_AI_BaseMissileClientDataEventArgs args,
            SpellData spellData, SpecialSpellEventArgs specialSpellArgs)
        {
            if (spellData.spellName == "YorickE")
            {
                var end = args.End;
                var start = args.Start;
                var direction = (end - start).Normalized();

                if (start.Distance(end) > spellData.range)
                    end = start + (end - start).Normalized() * spellData.range;

                var spellStart = end.Extend(hero.ServerPosition, 100);
                var spellEnd = spellStart + direction * 1;

                SpellDetector.CreateSpellData(hero, spellStart, spellEnd, spellData);
                specialSpellArgs.noProcess = true;
            }
        }
    }
}