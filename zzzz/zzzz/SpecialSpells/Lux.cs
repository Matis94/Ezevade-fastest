﻿using System.Linq;
using Aimtec;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Util.Cache;

//using SharpDX;

namespace zzzz.SpecialSpells
{
    internal class Lux : ChampionPlugin
    {
        public void LoadSpecialSpell(SpellData spellData)
        {
            if (spellData.spellName == "LuxMaliceCannon")
            {
                var hero = GameObjects.Heroes.FirstOrDefault(h => h.ChampionName == "Lux");
                if (hero != null && hero.CheckTeam())
                {
                    ObjectTracker.HuiTrackerForceLoad();
                    GameObject.OnCreate += obj => OnCreateObj_LuxMaliceCannon(obj, hero, spellData);
                }
            }
        }

        private static void OnCreateObj_LuxMaliceCannon(GameObject obj, Obj_AI_Hero hero, SpellData spellData)
        {
            if (obj.Name.Contains("Lux") && obj.Name.Contains("R_mis_beam_middle"))
            {
                if (hero.IsVisible) return;

                var objList = ObjectTracker.objTracker.Values.Where(o => o.Name == "hiu");
                if (objList.Count() >= 2)
                {
                    var dir = ObjectTracker.GetLastHiuOrientation();
                    var pos1 = obj.Position.To2D() - dir * 1750;
                    var pos2 = obj.Position.To2D() + dir * 1750;

                    SpellDetector.CreateSpellData(hero, pos1.To3D(), pos2.To3D(), spellData, null, 0);

                    foreach (var gameObj in objList)
                        DelayAction.Add(1, () => ObjectTracker.objTracker.Remove(gameObj.obj.NetworkId));
                }
            }
        }
    }
}