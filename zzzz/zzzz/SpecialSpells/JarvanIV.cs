﻿using System.Collections.Generic;
using System.Linq;
using Aimtec;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Util.Cache;

//using SharpDX;

namespace zzzz.SpecialSpells
{
    internal class JarvanIV : ChampionPlugin
    {
        private static readonly Dictionary<float, Vector3> _eSpots = new Dictionary<float, Vector3>();

        static JarvanIV()
        {
        }

        public void LoadSpecialSpell(SpellData spellData)
        {
            if (spellData.spellName == "JarvanIVDragonStrike")
            {
                var jarvaniv = GameObjects.Heroes.FirstOrDefault(h => h.ChampionName == "JarvanIV");
                if (jarvaniv != null && jarvaniv.CheckTeam())
                {
                    Game.OnUpdate += Game_OnUpdate;
                    Obj_AI_Base.OnProcessSpellCast += ProcessSpell_JarvanIVDemacianStandard;
                    SpellDetector.OnProcessSpecialSpell += ProcessSpell_JarvanIVDragonStrike;
                    GameObject.OnCreate += OnCreateObj_JarvanIVDragonStrike;
                    GameObject.OnDestroy += OnDeleteObj_JarvanIVDragonStrike;
                }
            }
        }

        private void Game_OnUpdate()
        {
            foreach (var spot in _eSpots.ToArray())
            {
                var flag = spot.Key;
                if (Game.ClockTime - flag >= 1.2f * 0.6f)
                    _eSpots.Remove(flag);
            }
        }

        private static void ProcessSpell_JarvanIVDemacianStandard(Obj_AI_Base hero,
            Obj_AI_BaseMissileClientDataEventArgs args)
        {
            if (hero.IsEnemy && args.SpellData.Name == "JarvanIVDemacianStandard")
                ObjectTracker.AddObjTrackerPosition("Beacon", args.End, 1000);
        }

        private static void OnDeleteObj_JarvanIVDragonStrike(GameObject obj)
        {
            if (obj.Name == "Beacon")
                ObjectTracker.objTracker.Remove(obj.NetworkId);
        }

        private static void OnCreateObj_JarvanIVDragonStrike(GameObject obj)
        {
            if (obj.Name == "Beacon")
                ObjectTracker.objTracker.Add(obj.NetworkId, new ObjectTrackerInfo(obj));
        }

        private static void ProcessSpell_JarvanIVDragonStrike(Obj_AI_Base hero,
            Obj_AI_BaseMissileClientDataEventArgs args, SpellData spellData, SpecialSpellEventArgs specialSpellArgs)
        {
            if (spellData.spellName == "JarvanIVDemacianStandard")
            {
                var end = args.End;
                if (args.Start.Distance(end) > spellData.range)
                    end = args.Start + (args.End - args.Start).Normalized() * spellData.range;

                _eSpots.Add(Game.ClockTime, end);
            }

            if (spellData.spellName == "JarvanIVDragonStrike")
                if (SpellDetector.onProcessSpells.TryGetValue("jarvanivdragonstrike2", out spellData))
                {
                    foreach (var entry in _eSpots)
                    {
                        var flagPosition = entry.Value;

                        if (args.End.To2D().Distance(flagPosition) < 300)
                        {
                            var dir = (flagPosition.To2D() - args.Start.To2D()).Normalized();
                            var endPosition = flagPosition.To2D() + dir * 110;

                            SpellDetector.CreateSpellData(hero, args.Start, endPosition.To3D(), spellData);
                            specialSpellArgs.noProcess = true;
                            return;
                        }
                    }

                    foreach (var entry in ObjectTracker.objTracker)
                    {
                        var info = entry.Value;

                        if (info.Name == "Beacon" || info.obj.Name == "Beacon")
                        {
                            if (info.usePosition == false && (info.obj == null || !info.obj.IsValid || info.obj.IsDead))
                            {
                                DelayAction.Add(1, () => ObjectTracker.objTracker.Remove(info.obj.NetworkId));
                                continue;
                            }

                            var objPosition = info.usePosition ? info.position.To2D() : info.obj.Position.To2D();

                            if (args.End.To2D().Distance(objPosition) < 300)
                            {
                                var dir = (objPosition - args.Start.To2D()).Normalized();
                                var endPosition = objPosition + dir * 110;

                                SpellDetector.CreateSpellData(hero, args.Start, endPosition.To3D(), spellData);
                                specialSpellArgs.noProcess = true;
                                return;
                            }
                        }
                    }
                }
        }
    }
}