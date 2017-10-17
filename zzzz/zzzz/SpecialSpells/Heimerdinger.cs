using Aimtec;

//using SharpDX;

namespace zzzz.SpecialSpells
{
    internal class Heimerdinger : ChampionPlugin
    {
        public void LoadSpecialSpell(SpellData spellData)
        {
            if (spellData.spellName == "HeimerdingerTurretEnergyBlast"
                || spellData.spellName == "HeimerdingerTurretBigEnergyBlast")
                SpellDetector.OnProcessSpecialSpell += ProcessSpell_HeimerdingerTurretEnergyBlast;

            if (spellData.spellName == "HeimerdingerW")
            {
                //SpellDetector.OnProcessSpecialSpell += ProcessSpell_HeimerdingerW;
            }
        }

        private void ProcessSpell_HeimerdingerW(Obj_AI_Base hero, Obj_AI_BaseMissileClientDataEventArgs args,
            SpellData spellData, SpecialSpellEventArgs specialSpellArgs)
        {
            if (spellData.spellName == "HeimerdingerW")
                specialSpellArgs.noProcess = true;
        }

        private static void ProcessSpell_HeimerdingerTurretEnergyBlast(Obj_AI_Base hero,
            Obj_AI_BaseMissileClientDataEventArgs args, SpellData spellData, SpecialSpellEventArgs specialSpellArgs)
        {
            if (spellData.spellName == "HeimerdingerTurretEnergyBlast"
                || spellData.spellName == "HeimerdingerTurretBigEnergyBlast")
            {
                SpellDetector.CreateSpellData(hero, args.Start, args.End, spellData);

                specialSpellArgs.noProcess = true;
            }
        }
    }
}