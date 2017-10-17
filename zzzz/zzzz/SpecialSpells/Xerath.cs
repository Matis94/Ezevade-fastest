using System;
using Aimtec;
using Aimtec.SDK.Extensions;

//using SharpDX;

namespace zzzz.SpecialSpells
{
    internal class Xerath : ChampionPlugin
    {
        public void LoadSpecialSpell(SpellData spellData)
        {
            if (spellData.spellName == "xeratharcanopulse2")
            {
                //SpellDetector.OnProcessSpecialSpell += ProcessSpell_XerathArcanopulse2;
            }
        }

        private static void ProcessSpell_XerathArcanopulse2(Obj_AI_Base hero,
            Obj_AI_BaseMissileClientDataEventArgs args, SpellData spellData, SpecialSpellEventArgs specialSpellArgs)
        {
            if (args.SpellData.Name == "XerathArcanopulseChargeUp") // || spellData.spellName == "xeratharcanopulse2")
            {
                // Not sure with CastEndTime
                var castTime = -1 * (hero.SpellBook.CastEndTime - Game.ClockTime) * 1000;

                if (castTime > 0)
                {
                    var dir = (args.End.To2D() - args.Start.To2D()).Normalized();
                    var endPos = args.Start.To2D() + dir * Math.Min(spellData.range, 750 + castTime / 2);
                    SpellDetector.CreateSpellData(hero, args.Start, endPos.To3D(), spellData);
                }

                specialSpellArgs.noProcess = true;
            }
        }
    }
}