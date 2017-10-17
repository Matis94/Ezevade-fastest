using System;
using Aimtec;
using Aimtec.SDK.Extensions;

//using SharpDX;

namespace zzzz.SpecialSpells
{
    internal class Jinx : ChampionPlugin
    {
        public void LoadSpecialSpell(SpellData spellData)
        {
            /*if (spellData.spellName == "JinxWMissile")
            {
                var hero = HeroManager.Enemies.FirstOrDefault(h => h.ChampionName == "Jinx");
                if (hero != null)
                {
                    GameObject.OnCreate += (obj, args) => OnCreateObj_JinxWMissile(obj, args, hero, spellData);
                }
            }*/
        }

        private static void OnCreateObj_JinxWMissile(GameObject obj, EventArgs args, Obj_AI_Hero hero,
            SpellData spellData)
        {
            if (hero != null && !hero.IsVisible
                && obj.IsEnemy && obj.Name.Contains("Jinx") && obj.Name.Contains("W_Cas"))
            {
                var pos1 = hero.Position;
                var dir = (obj.Position - ObjectManager.GetLocalPlayer().Position).Normalized();
                var pos2 = pos1 + dir * 500;

                SpellDetector.CreateSpellData(hero, pos1, pos2, spellData, null, 0);
            }
        }
    }
}