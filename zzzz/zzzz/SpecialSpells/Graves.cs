using Aimtec;
using Aimtec.SDK.Extensions;

//using SharpDX;

namespace zzzz.SpecialSpells
{
    internal class Graves : ChampionPlugin
    {
        public void LoadSpecialSpell(SpellData spellData)
        {
            if (spellData.spellName == "GravesQLineSpell")
                SpellDetector.OnProcessSpecialSpell += SpellDetector_OnProcessSpecialSpell;
        }

        private void SpellDetector_OnProcessSpecialSpell(Obj_AI_Base hero, Obj_AI_BaseMissileClientDataEventArgs args,
            SpellData spellData, SpecialSpellEventArgs specialSpellArgs)
        {
            if (spellData.spellName == "GravesQLineSpell")
            {
                var newData = (SpellData) spellData.Clone();
                newData.isPerpendicular = true;
                newData.secondaryRadius = 255f;
                newData.updatePosition = false;
                newData.extraEndTime = 1300;

                var end = args.End;
                var start = args.Start;

                if (end.Distance(start) > newData.range)
                    end = args.Start + (args.End - args.Start).Normalized() * newData.range;

                if (end.Distance(start) < newData.range)
                    end = args.Start + (args.End - args.Start).Normalized() * newData.range;

                var w = EvadeHelper.GetNearWallPoint(start, end);
                if (w != default(Vector3))
                    end = w;

                //SpellDetector.CreateSpellData(hero, hero.ServerPosition, end, spellData);
                SpellDetector.CreateSpellData(hero, hero.ServerPosition, end, newData);
            }
        }
    }
}