using System.Collections.Generic;
using System.Linq;
using Aimtec;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Menu.Components;
using Aimtec.SDK.Util;

//using SharpDX;

namespace zzzz.SpecialSpells
{
    internal class AllChampions : ChampionPlugin
    {
        public static Dictionary<string, bool> pDict = new Dictionary<string, bool>();

        static AllChampions()
        {
        }

        public void LoadSpecialSpell(SpellData spellData)
        {
            if (!pDict.ContainsKey("Game_OnWndProc"))
            {
                Game.OnWndProc += Game_OnWndProc;
                pDict["Game_OnWndProc"] = true;
            }

            if (spellData.hasTrap && !pDict.ContainsKey("GameObject_OnCreate"))
            {
                GameObject.OnCreate += GameObject_OnCreate;
                pDict["GameObject_OnCreate"] = true;
            }

            if (spellData.hasTrap && !pDict.ContainsKey("GameObject_OnDelete"))
            {
                GameObject.OnDestroy += GameObject_OnDelete;
                pDict["GameObject_OnDelete"] = true;
            }

            if (spellData.hasTrap && !pDict.ContainsKey("Game_OnUpdate"))
            {
                Game.OnUpdate += Game_OnUpdate;
                pDict["Game_OnUpdate"] = true;
            }

            if (spellData.isThreeWay && !pDict.ContainsKey("ProcessSpell_ProcessThreeWay"))
            {
                SpellDetector.OnProcessSpecialSpell += ProcessSpell_ThreeWay;
                pDict["ProcessSpell_ProcessThreeWay"] = true;
            }
        }


        private void GameObject_OnCreate(GameObject sender)
        {
            //var emitter = sender as Obj_GeneralParticleEmitter;
            //if (emitter != null && emitter.CheckTeam())
            //{
            //    SpellData spellData;

            //    if (SpellDetector.onProcessTraps.TryGetValue(emitter.Name.ToLower(), out spellData))
            //    {
            //        var trapData = (SpellData) spellData.Clone();

            //        if (!trapData.spellName.Contains("_trap"))
            //             trapData.spellName = trapData.spellName + "_trap";

            //        SpellDetector.CreateSpellData(null, emitter.Position, emitter.Position, trapData, emitter, 1337f);
            //    }
            //}

            var aiBase = sender as Obj_AI_Base;
            if (aiBase != null && aiBase.CheckTeam())
            {
                SpellData spellData;

                if (SpellDetector.onProcessTraps.TryGetValue(aiBase.UnitSkinName.ToLower(), out spellData))
                {
                    var trapData = (SpellData) spellData.Clone();

                    if (!trapData.spellName.Contains("_trap"))
                        trapData.spellName = trapData.spellName + "_trap";

                    SpellDetector.CreateSpellData(aiBase, aiBase.ServerPosition, aiBase.ServerPosition, trapData,
                        aiBase, 1337f);
                }
            }
        }

        private void GameObject_OnDelete(GameObject sender)
        {
            //var emitter = sender as Obj_GeneralParticleEmitter;
            //if (emitter != null && emitter.CheckTeam())
            //{
            //    SpellData spellData;

            //    if (SpellDetector.onProcessTraps.TryGetValue(emitter.Name.ToLower(), out spellData))
            //    {
            //        foreach (var entry in SpellDetector.detectedSpells.Where(x => x.Value.info.trapTroyName.ToLower() == emitter.Name.ToLower()))
            //        {
            //            DelayAction.Add(1, () => SpellDetector.DeleteSpell(entry.Key));
            //            entry.Value.spellObject = null;
            //        }
            //    }
            //}

            var aiBase = sender as Obj_AI_Base;
            if (aiBase != null && aiBase.CheckTeam())
            {
                SpellData spellData;

                if (SpellDetector.onProcessTraps.TryGetValue(aiBase.UnitSkinName.ToLower(), out spellData))
                    foreach (var entry in SpellDetector.detectedSpells.Where(
                        x => x.Value.info.trapBaseName.ToLower() == aiBase.UnitSkinName.ToLower()))
                    {
                        DelayAction.Add(1, () => SpellDetector.DeleteSpell(entry.Key));
                        entry.Value.spellObject = null;
                    }
            }
        }

        private void Game_OnUpdate()
        {
            foreach (var entry in SpellDetector.detectedSpells.Where(x => x.Value.info.spellName.Contains("_trap")))
            {
                var spell = entry.Value;
                if (spell.spellObject == null)
                    continue;

                if (spell.spellObject.IsDead || !spell.spellObject.IsValid)
                {
                    DelayAction.Add(1, () => SpellDetector.DeleteSpell(entry.Key));
                    entry.Value.spellObject = null;
                }
            }
        }

        private void Game_OnWndProc(WndProcEventArgs e)
        {
            if (!ObjectCache.menuCache.cache["ClickRemove"].As<MenuBool>().Enabled)
                return;

            if (e.Message != (uint) WindowsMessages.WM_LBUTTONDOWN)
                return;

            foreach (var entry in SpellDetector.detectedSpells.Where(x => Game.CursorPos.To2D()
                .InSkillShot(x.Value, 50 + x.Value.info.radius, false)))
            {
                var spell = entry.Value;
                if (spell.info.range > 9000 /*global*/ || spell.info.spellName.Contains("_trap"))
                    DelayAction.Add(1, () => SpellDetector.DeleteSpell(entry.Key));
            }
        }

        private static void ProcessSpell_ThreeWay(Obj_AI_Base hero, Obj_AI_BaseMissileClientDataEventArgs args,
            SpellData spellData, SpecialSpellEventArgs specialSpellArgs)
        {
            if (spellData.isThreeWay)
            {
                var endPos2 = MathUtils.RotateVector(args.Start.To2D(), args.End.To2D(), spellData.angle).To3D();
                SpellDetector.CreateSpellData(hero, args.Start, endPos2, spellData, null, 0, false);

                var endPos3 = MathUtils.RotateVector(args.Start.To2D(), args.End.To2D(), -spellData.angle).To3D();
                SpellDetector.CreateSpellData(hero, args.Start, endPos3, spellData, null, 0, false);
            }
        }
    }
}