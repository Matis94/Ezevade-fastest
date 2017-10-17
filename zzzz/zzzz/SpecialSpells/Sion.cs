using System.Linq;
using Aimtec;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Util.Cache;

//using SharpDX;

namespace zzzz.SpecialSpells
{
    internal class Sion : ChampionPlugin
    {
        public void LoadSpecialSpell(SpellData spellData)
        {
            if (spellData.spellName == "SionR")
            {
                var hero = GameObjects.Heroes.FirstOrDefault(x => x.ChampionName == "Sion");
                if (hero != null && hero.CheckTeam())
                {
                    Game.OnUpdate += () => Game_OnUpdate(hero);
                    SpellDetector.OnProcessSpecialSpell += SpellDetector_OnProcessSpecialSpell;
                }
            }
        }

        private void SpellDetector_OnProcessSpecialSpell(Obj_AI_Base hero, Obj_AI_BaseMissileClientDataEventArgs args,
            SpellData spellData, SpecialSpellEventArgs specialSpellArgs)
        {
            if (spellData.spellName == "SionR")
            {
                spellData.projectileSpeed = hero.MoveSpeed;
                specialSpellArgs.spellData = spellData;
            }
        }

        private void Game_OnUpdate(Obj_AI_Hero hero)
        {
            foreach (var spell in SpellDetector.detectedSpells.Where(
                x => x.Value.heroID == hero.NetworkId && x.Value.info.spellName == "SionR"))
            {
                var facingPos = hero.ServerPosition.To2D() + hero.Orientation.To2D().Perpendicular();
                var endPos = hero.ServerPosition.To2D() + (facingPos - hero.ServerPosition.To2D()).Normalized() * 450;

                spell.Value.startPos = hero.ServerPosition.To2D();
                spell.Value.endPos = endPos;

                if (EvadeUtils.TickCount - spell.Value.startTime >= 1000)
                {
                    SpellDetector.CreateSpellData(hero, hero.ServerPosition, endPos.To3D(), spell.Value.info, null, 0,
                        false, SpellType.Line, false);
                    spell.Value.startTime = EvadeUtils.TickCount;
                    break;
                }
            }
        }
    }
}