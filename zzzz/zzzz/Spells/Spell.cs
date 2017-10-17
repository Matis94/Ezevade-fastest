﻿using System;
using System.Collections.Generic;
using System.Linq;
using Aimtec;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Menu.Components;
using Aimtec.SDK.Util.Cache;

//using SharpDX;

namespace zzzz
{
    public class Spell
    {
        public Vector2 cnLeft;
        public Vector2 cnRight;
        public Vector2 cnStart;
        public Vector2 currentNegativePosition = Vector2.Zero;
        public Vector2 currentSpellPosition = Vector2.Zero;
        public int dangerlevel = 1;
        public Vector2 direction;
        public Vector2 endPos;
        public float endTime;

        public float evadeTime = float.MinValue;
        public float height;
        public int heroID;
        public SpellData info;
        public Vector2 predictedEndPos = Vector2.Zero;
        public int projectileID;

        public float radius = 0;
        public float spellHitTime = float.MinValue;
        public int spellID;
        public GameObject spellObject = null;
        public SpellType spellType;
        public Vector2 startPos;
        public float startTime;
    }

    public static class SpellExtensions
    {
        public static float GetSpellRadius(this Spell spell)
        {
            var radius =
                Evade.spellMenu[spell.info.charName + spell.info.spellName + "Settings"][
                    spell.info.spellName + "SpellRadius"].As<MenuSlider>().Value;
            var extraRadius = ObjectCache.menuCache.cache["ExtraSpellRadius"].As<MenuSlider>().Value;

            if (spell.info.hasEndExplosion && spell.spellType == SpellType.Circular)
                return spell.info.secondaryRadius + extraRadius;

            if (spell.spellType == SpellType.Arc)
            {
                var spellRange = spell.startPos.Distance(spell.endPos);
                var arcRadius = spell.info.radius * (1 + spellRange / 100) + extraRadius;

                return arcRadius;
            }

            return radius + extraRadius;
        }

        public static int GetSpellDangerLevel(this Spell spell)
        {
            var dangerStr =
                Evade.spellMenu[spell.info.charName + spell.info.spellName + "Settings"][
                    spell.info.spellName + "DangerLevel"].As<MenuList>().SelectedItem;

            var dangerlevel = 1;

            switch (dangerStr)
            {
                case "Low":
                    dangerlevel = 1;
                    break;
                case "High":
                    dangerlevel = 3;
                    break;
                case "Extreme":
                    dangerlevel = 4;
                    break;
                default:
                    dangerlevel = 2;
                    break;
            }

            return dangerlevel;
        }

        public static string GetSpellDangerString(this Spell spell)
        {
            switch (spell.GetSpellDangerLevel())
            {
                case 1:
                    return "Low";
                case 3:
                    return "High";
                case 4:
                    return "Extreme";
                default:
                    return "Normal";
            }
        }

        public static bool hasProjectile(this Spell spell)
        {
            return spell.info.projectileSpeed > 0 && spell.info.projectileSpeed != float.MaxValue;
        }

        public static Vector2 GetSpellProjection(this Spell spell, Vector2 pos, bool predictPos = false)
        {
            if (spell.spellType == SpellType.Line)
            {
                if (predictPos)
                {
                    var spellPos = spell.currentSpellPosition;
                    var spellEndPos = spell.GetSpellEndPosition();

                    return pos.ProjectOn(spellPos, spellEndPos).SegmentPoint;
                }

                return pos.ProjectOn(spell.startPos, spell.endPos).SegmentPoint;
            }

            if (spell.spellType == SpellType.Arc)
            {
                if (predictPos)
                {
                    var spellPos = spell.currentSpellPosition;
                    var spellEndPos = spell.GetSpellEndPosition();

                    return pos.ProjectOn(spellPos, spellEndPos).SegmentPoint;
                }

                return pos.ProjectOn(spell.startPos, spell.endPos).SegmentPoint;
            }

            if (spell.spellType == SpellType.Circular)
                return spell.endPos;

            if (spell.spellType == SpellType.Cone)
            {
            }

            return Vector2.Zero;
        }

        public static Obj_AI_Base CheckSpellCollision(this Spell spell, bool ignoreSelf = true)
        {
            if (spell.info.collisionObjects.Count() < 1)
                return null;

            var collisionCandidates = new List<Obj_AI_Base>();
            var spellPos = spell.currentSpellPosition;
            var distanceToHero = spellPos.Distance(ObjectCache.myHeroCache.serverPos2D);

            if (spell.info.collisionObjects.Contains(CollisionObjectType.EnemyChampions))
                foreach (var hero in GameObjects.AllyHeroes
                    .Where(h => h.IsValidTarget(distanceToHero, false, true, spellPos.To3D())))
                {
                    if (ignoreSelf && hero.IsMe)
                        continue;

                    collisionCandidates.Add(hero);
                }

            if (spell.info.collisionObjects.Contains(CollisionObjectType.EnemyMinions))
                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>()
                    .Where(h => h.Team == Evade.myHero.Team &&
                                h.IsValidTarget(distanceToHero, false, true, spellPos.To3D())))
                {
                    if (minion.UnitSkinName.ToLower() == "teemomushroom"
                        || minion.UnitSkinName.ToLower() == "shacobox")
                        continue;

                    collisionCandidates.Add(minion);
                }

            var sortedCandidates = collisionCandidates.OrderBy(h => h.Distance(spellPos));

            foreach (var candidate in sortedCandidates)
                if (candidate.ServerPosition.To2D().InSkillShot(spell, candidate.BoundingRadius, false))
                    return candidate;

            return null;
        }

        public static float GetSpellHitTime(this Spell spell, Vector2 pos)
        {
            switch (spell.spellType)
            {
                case SpellType.Line:
                    if (spell.info.projectileSpeed == float.MaxValue)
                        return Math.Max(0, spell.endTime - EvadeUtils.TickCount - ObjectCache.gamePing);

                    var spellPos = spell.GetCurrentSpellPosition(true, ObjectCache.gamePing);
                    return 1000 * spellPos.Distance(pos) / spell.info.projectileSpeed;
                case SpellType.Cone:
                case SpellType.Circular:
                    return Math.Max(0, spell.endTime - EvadeUtils.TickCount - ObjectCache.gamePing);
            }

            return float.MaxValue;
        }

        public static bool CanHeroEvade(this Spell spell, Obj_AI_Base hero, out float rEvadeTime,
            out float rSpellHitTime)
        {
            var heroPos = hero.ServerPosition.To2D();
            float evadeTime = 0;
            float spellHitTime = 0;
            var speed = hero.MoveSpeed;
            float delay = 0;

            var moveBuff = EvadeSpell.evadeSpells.OrderBy(s => s.dangerlevel)
                .FirstOrDefault(s => s.evadeType == EvadeType.MovementSpeedBuff);
            if (moveBuff != null && EvadeSpell.ShouldUseMovementBuff(spell))
            {
                speed += speed * moveBuff.speedArray[
                             ObjectManager.GetLocalPlayer().GetSpell(moveBuff.spellKey).Level - 1] / 100;
                delay += (moveBuff.spellDelay > 50 ? moveBuff.spellDelay : 0) + ObjectCache.gamePing;
            }

            if (spell.spellType == SpellType.Line)
            {
                var projection = heroPos.ProjectOn(spell.startPos, spell.endPos).SegmentPoint;
                evadeTime = 1000 * (spell.radius - heroPos.Distance(projection) + hero.BoundingRadius) / speed;
                spellHitTime = spell.GetSpellHitTime(projection);
            }
            else if (spell.spellType == SpellType.Circular)
            {
                evadeTime = 1000 * (spell.radius - heroPos.Distance(spell.endPos)) / speed;
                spellHitTime = spell.GetSpellHitTime(heroPos);
            }
            else if (spell.spellType == SpellType.Cone)
            {
                var sides = new[]
                {
                    heroPos.ProjectOn(spell.cnStart, spell.cnLeft).SegmentPoint,
                    heroPos.ProjectOn(spell.cnLeft, spell.cnRight).SegmentPoint,
                    heroPos.ProjectOn(spell.cnRight, spell.cnStart).SegmentPoint
                };

                var p = sides.OrderBy(x => x.Distance(x)).First();
                evadeTime = 1000 * (spell.info.range / 2 - heroPos.Distance(p) + hero.BoundingRadius) / speed;
                spellHitTime = spell.GetSpellHitTime(heroPos);
            }

            rEvadeTime = evadeTime;
            rSpellHitTime = spellHitTime;

            return spellHitTime - delay > evadeTime;
        }

        public static BoundingBox GetLinearSpellBoundingBox(this Spell spell)
        {
            var myBoundingRadius = ObjectCache.myHeroCache.boundingRadius;
            var spellDir = spell.direction;
            var pSpellDir = spell.direction.Perpendicular();
            var spellRadius = spell.radius;
            var spellPos = spell.currentSpellPosition - spellDir * myBoundingRadius; //leave some space at back of spell
            var endPos =
                spell.GetSpellEndPosition() + spellDir * myBoundingRadius; //leave some space at the front of spell

            var startRightPos = spellPos + pSpellDir * (spellRadius + myBoundingRadius);
            var endLeftPos = endPos - pSpellDir * (spellRadius + myBoundingRadius);


            return new BoundingBox(new Vector3(endLeftPos.X, endLeftPos.Y, -1),
                new Vector3(startRightPos.X, startRightPos.Y, 1));
        }

        public static Vector2 GetSpellEndPosition(this Spell spell)
        {
            return spell.predictedEndPos == Vector2.Zero ? spell.endPos : spell.predictedEndPos;
        }

        public static void UpdateSpellInfo(this Spell spell)
        {
            spell.currentSpellPosition = spell.GetCurrentSpellPosition();
            spell.currentNegativePosition = spell.GetCurrentSpellPosition(true, 0);
            spell.dangerlevel = spell.GetSpellDangerLevel();
        }

        public static Vector2 GetCurrentSpellPosition(this Spell spell, bool allowNegative = false, float delay = 0,
            float extraDistance = 0)
        {
            var spellPos = spell.startPos;

            if (spell.info.updatePosition == false)
                return spellPos;

            if (spell.spellType == SpellType.Line || spell.spellType == SpellType.Arc)
            {
                var spellTime = EvadeUtils.TickCount - spell.startTime -
                                spell.info.spellDelay - Math.Max(0, spell.info.extraEndTime);

                if (spell.info.projectileSpeed == float.MaxValue)
                    return spell.startPos;

                if (spellTime >= 0 || allowNegative)
                    spellPos = spell.startPos + spell.direction * spell.info.projectileSpeed * (spellTime / 1000);
            }
            else if (spell.spellType == SpellType.Circular || spell.spellType == SpellType.Cone)
            {
                spellPos = spell.endPos;
            }

            if (spell.spellObject != null && spell.spellObject.IsValid && spell.spellObject.IsVisible &&
                spell.spellObject.Position.To2D().Distance(ObjectCache.myHeroCache.serverPos2D) <
                spell.info.range + 1000)
                spellPos = spell.spellObject.Position.To2D();

            if (delay > 0 && spell.info.projectileSpeed != float.MaxValue
                && spell.spellType == SpellType.Line)
                spellPos = spellPos + spell.direction * spell.info.projectileSpeed * (delay / 1000);

            if (extraDistance > 0 && spell.info.projectileSpeed != float.MaxValue
                && spell.spellType == SpellType.Line)
                spellPos = spellPos + spell.direction * extraDistance;

            return spellPos;
        }

        public static bool LineIntersectLinearSpell(this Spell spell, Vector2 a, Vector2 b)
        {
            var myBoundingRadius = ObjectManager.GetLocalPlayer().BoundingRadius;
            var spellDir = spell.direction;
            var pSpellDir = spell.direction.Perpendicular();
            var spellRadius = spell.radius;
            var spellPos =
                spell.currentSpellPosition; // -spellDir * myBoundingRadius; //leave some space at back of spell
            var endPos =
                spell.GetSpellEndPosition(); // +spellDir * myBoundingRadius; //leave some space at the front of spell

            var startRightPos = spellPos + pSpellDir * (spellRadius + myBoundingRadius);
            var startLeftPos = spellPos - pSpellDir * (spellRadius + myBoundingRadius);
            var endRightPos = endPos + pSpellDir * (spellRadius + myBoundingRadius);
            var endLeftPos = endPos - pSpellDir * (spellRadius + myBoundingRadius);

            var int1 = MathUtils.CheckLineIntersection(a, b, startRightPos, startLeftPos);
            var int2 = MathUtils.CheckLineIntersection(a, b, endRightPos, endLeftPos);
            var int3 = MathUtils.CheckLineIntersection(a, b, startRightPos, endRightPos);
            var int4 = MathUtils.CheckLineIntersection(a, b, startLeftPos, endLeftPos);

            if (int1 || int2 || int3 || int4)
                return true;

            return false;
        }

        public static bool LineIntersectLinearSpellEx(this Spell spell, Vector2 a, Vector2 b,
            out Vector2 intersection) //edited
        {
            var myBoundingRadius = ObjectManager.GetLocalPlayer().BoundingRadius;
            var spellDir = spell.direction;
            var pSpellDir = spell.direction.Perpendicular();
            var spellRadius = spell.radius;
            var spellPos = spell.currentSpellPosition - spellDir * myBoundingRadius; //leave some space at back of spell
            var endPos =
                spell.GetSpellEndPosition() + spellDir * myBoundingRadius; //leave some space at the front of spell

            var startRightPos = spellPos + pSpellDir * (spellRadius + myBoundingRadius);
            var startLeftPos = spellPos - pSpellDir * (spellRadius + myBoundingRadius);
            var endRightPos = endPos + pSpellDir * (spellRadius + myBoundingRadius);
            var endLeftPos = endPos - pSpellDir * (spellRadius + myBoundingRadius);

            var intersects = new List<Vector2Extensions.IntersectionResult>();
            var heroPos = ObjectManager.GetLocalPlayer().ServerPosition.To2D();

            intersects.Add(a.Intersection(b, startRightPos, startLeftPos));
            intersects.Add(a.Intersection(b, endRightPos, endLeftPos));
            intersects.Add(a.Intersection(b, startRightPos, endRightPos));
            intersects.Add(a.Intersection(b, startLeftPos, endLeftPos));

            var sortedIntersects = intersects.Where(i => i.Intersects)
                .OrderBy(i => i.Point.Distance(heroPos)); //Get first intersection

            if (sortedIntersects.Count() > 0)
            {
                intersection = sortedIntersects.First().Point;
                return true;
            }

            intersection = Vector2.Zero;
            return false;
        }
    }
}