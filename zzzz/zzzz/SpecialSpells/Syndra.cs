﻿using System.Collections.Generic;
using System.Linq;
using Aimtec;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Util.Cache;
//using SharpDX;

namespace zzzz.SpecialSpells
{
    internal class Syndra : ChampionPlugin
    {
        private const string _sphereName = "syndrasphere";
        private static readonly List<Obj_AI_Minion> _spheres = new List<Obj_AI_Minion>();
        private static readonly Dictionary<float, Vector3> _qSpots = new Dictionary<float, Vector3>();

        static Syndra()
        {
        }

        public void LoadSpecialSpell(SpellData spellData)
        {
            if (spellData.spellName.ToLower() == "syndrae")
            {
                var hero = GameObjects.Heroes.FirstOrDefault(x => x.ChampionName == "Syndra");
                if (hero != null && hero.CheckTeam())
                {
                    Game.OnUpdate += Game_OnUpdate;
                    Obj_AI_Base.OnPlayAnimation += Obj_AI_Base_OnPlayAnimation;
                    GameObject.OnCreate += GameObject_OnCreate;
                    GameObject.OnDestroy += GameObject_OnDelete;
                    SpellDetector.OnProcessSpecialSpell += SpellDetector_OnProcessSpecialSpell;
                }
            }
        }

        private static void Game_OnUpdate()
        {
            _spheres.RemoveAll(i => !i.IsValid || i.IsDead);

            foreach (var spot in _qSpots.ToArray())
            {
                var timestamp = spot.Key;
                if (Game.ClockTime - timestamp >= 1.2f * 0.6f)
                    _qSpots.Remove(timestamp);
            }
        }

        private static void Obj_AI_Base_OnPlayAnimation(Obj_AI_Base sender, Obj_AI_BasePlayAnimationEventArgs e)
        {
            var sphere = sender as Obj_AI_Minion;
            if (sphere != null && sphere.UnitSkinName == _sphereName && sphere.CheckTeam())
                if (e.Animation == "Death")
                    _spheres.RemoveAll(i => i.NetworkId == sphere.NetworkId);
        }

        private static void GameObject_OnCreate(GameObject sender)
        {
            var sphere = sender as Obj_AI_Minion;
            if (sphere != null && sphere.UnitSkinName == _sphereName && sphere.CheckTeam())
                if (!_spheres.Contains(sphere))
                {
                    RemovePairsNear(sphere.Position);
                    _spheres.Add(sphere);
                }
        }

        private static void GameObject_OnDelete(GameObject sender)
        {
            var sphere = sender as Obj_AI_Minion;
            if (sphere != null && sphere.UnitSkinName == _sphereName && sphere.CheckTeam())
                _spheres.RemoveAll(i => i.NetworkId == sphere.NetworkId);
        }

        private void SpellDetector_OnProcessSpecialSpell(Obj_AI_Base hero, Obj_AI_BaseMissileClientDataEventArgs args,
            SpellData spellData, SpecialSpellEventArgs specialSpellArgs)
        {
            if (spellData.spellName.ToLower() == "syndrae")
            {
                var estart = args.Start;
                var eend = args.Start + (args.End - args.Start).Normalized() * 800;

                foreach (var sphere in _spheres.Where(s => s.IsValid && !s.IsDead))
                {
                    // check if e whill hit the sphere
                    var proj = sphere.Position.To2D().ProjectOn(estart.To2D(), eend.To2D());
                    if (proj.IsOnSegment && sphere.Position.To2D().Distance(proj.SegmentPoint) <=
                        sphere.BoundingRadius + 155)
                    {
                        var start = sphere.Position;
                        var end = hero.ServerPosition + (sphere.Position - hero.ServerPosition).Normalized() *
                                  spellData.range;
                        var data = (SpellData) spellData.Clone();
                        data.spellDelay = sphere.Distance(hero.ServerPosition) / spellData.projectileSpeed * 1000;
                        SpellDetector.CreateSpellData(hero, start, end, data, sphere);
                    }
                }

                foreach (var entry in _qSpots)
                {
                    var spherePosition = entry.Value;

                    // check if e whill hit the sphere
                    var proj = spherePosition.To2D().ProjectOn(estart.To2D(), eend.To2D());
                    if (proj.IsOnSegment && spherePosition.To2D().Distance(proj.SegmentPoint) <= 155)
                    {
                        var start = spherePosition;
                        var end = hero.ServerPosition + (spherePosition - hero.ServerPosition).Normalized() *
                                  spellData.range;
                        var data = (SpellData) spellData.Clone();
                        data.spellDelay = spherePosition.Distance(hero.ServerPosition) / spellData.projectileSpeed *
                                          1000;
                        SpellDetector.CreateSpellData(hero, start, end, data, null);
                    }
                }

                specialSpellArgs.noProcess = true;
            }

            if (spellData.spellName.ToLower() == "syndraq")
            {
                var end = args.End;
                if (args.Start.Distance(end) > spellData.range)
                    end = args.Start + (args.End - args.Start).Normalized() * spellData.range;

                _qSpots[Game.ClockTime] = end;
            }

            if (spellData.spellName.ToLower() == "syndrawcast")
            {
                var end = args.End;
                if (args.Start.Distance(end) > spellData.range)
                    end = args.Start + (args.End - args.Start).Normalized() * spellData.range;

                _qSpots[Game.ClockTime] = end;
            }
        }

        private static void RemovePairsNear(Vector3 pos)
        {
            foreach (var pair in _qSpots.ToArray().Where(o => o.Value.Distance(pos) <= 30))
                _qSpots.Remove(pair.Key);
        }
    }
}