using System;
using System.Collections.Generic;
using Aimtec;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Menu.Components;
using Aimtec.SDK.Util.Cache;

//using SharpDX;

namespace zzzz
{
    public static class Position
    {
        private static Obj_AI_Hero myHero => ObjectManager.GetLocalPlayer();

        public static int CheckPosDangerLevel(this Vector2 pos, float extraBuffer)
        {
            var dangerlevel = 0;
            foreach (var entry in SpellDetector.spells)
            {
                var spell = entry.Value;

                if (pos.InSkillShot(spell, ObjectCache.myHeroCache.boundingRadius + extraBuffer))
                    dangerlevel += spell.dangerlevel;
            }
            return dangerlevel;
        }

        public static bool InSkillShot(this Vector2 position, Spell spell, float radius, bool predictCollision = true)
        {
            if (spell.spellType == SpellType.Line)
            {
                var spellPos = spell.currentSpellPosition;
                var spellEndPos = predictCollision ? spell.GetSpellEndPosition() : spell.endPos;

                var projection = position.ProjectOn(spellPos, spellEndPos);
                return projection.IsOnSegment && projection.SegmentPoint.Distance(position) <= spell.radius + radius;
            }

            if (spell.spellType == SpellType.Circular)
            {
                if (spell.info.spellName == "VeigarEventHorizon")
                    return position.Distance(spell.endPos) <=
                           spell.radius + radius - ObjectCache.myHeroCache.boundingRadius
                           && position.Distance(spell.endPos) >= spell.radius + radius -
                           ObjectCache.myHeroCache.boundingRadius - 125;
                if (spell.info.spellName == "DariusCleave")
                    return position.Distance(spell.endPos) <=
                           spell.radius + radius - ObjectCache.myHeroCache.boundingRadius
                           && position.Distance(spell.endPos) >= spell.radius + radius -
                           ObjectCache.myHeroCache.boundingRadius - 220;

                return position.Distance(spell.endPos) <=
                       spell.radius + radius - ObjectCache.myHeroCache.boundingRadius;
            }

            if (spell.spellType == SpellType.Arc)
            {
                if (position.isLeftOfLineSegment(spell.startPos, spell.endPos))
                    return false;

                var spellRange = spell.startPos.Distance(spell.endPos);
                var midPoint = spell.startPos + spell.direction * (spellRange / 2);

                return position.Distance(midPoint) <= spell.radius + radius - ObjectCache.myHeroCache.boundingRadius;
            }

            if (spell.spellType == SpellType.Cone)
                return !position.isLeftOfLineSegment(spell.cnStart, spell.cnLeft) &&
                       !position.isLeftOfLineSegment(spell.cnLeft, spell.cnRight) &&
                       !position.isLeftOfLineSegment(spell.cnRight, spell.cnStart);

            return false;
        }

        public static bool isLeftOfLineSegment(this Vector2 pos, Vector2 start, Vector2 end)
        {
            return (end.X - start.X) * (pos.Y - start.Y) - (end.Y - start.Y) * (pos.X - start.X) > 0;
        }

        public static float GetDistanceToTurrets(this Vector2 pos)
        {
            var minDist = float.MaxValue;

            foreach (var entry in ObjectCache.turrets)
            {
                var turret = entry.Value;
                if (turret == null || !turret.IsValid || turret.IsDead)
                {
                    DelayAction.Add(1, () => ObjectCache.turrets.Remove(entry.Key));
                    continue;
                }

                if (turret.IsAlly)
                    continue;

                var distToTurret = pos.Distance(turret.Position.To2D());

                minDist = Math.Min(minDist, distToTurret);
            }

            return minDist;
        }

        public static float GetDistanceToChampions(this Vector2 pos)
        {
            var minDist = float.MaxValue;

            foreach (var hero in GameObjects.EnemyHeroes)
                if (hero != null && hero.IsValid && !hero.IsDead && hero.IsVisible)
                {
                    var heroPos = hero.ServerPosition.To2D();
                    var dist = heroPos.Distance(pos);

                    minDist = Math.Min(minDist, dist);
                }

            return minDist;
        }

        public static bool HasExtraAvoidDistance(this Vector2 pos, float extraBuffer)
        {
            foreach (var entry in SpellDetector.spells)
            {
                var spell = entry.Value;

                if (spell.spellType == SpellType.Line)
                    if (pos.InSkillShot(spell, ObjectCache.myHeroCache.boundingRadius + extraBuffer))
                        return true;
            }
            return false;
        }

        public static float GetEnemyPositionValue(this Vector2 pos)
        {
            float posValue = 0;

            if (ObjectCache.menuCache.cache["PreventDodgingNearEnemy"].As<MenuBool>().Enabled)
            {
                var minComfortDistance = ObjectCache.menuCache.cache["MinComfortZone"].As<MenuSlider>().Value;

                foreach (var hero in GameObjects.EnemyHeroes)
                    if (hero != null && hero.IsValid && !hero.IsDead && hero.IsVisible)
                    {
                        var heroPos = hero.ServerPosition.To2D();
                        var dist = heroPos.Distance(pos);

                        if (minComfortDistance > dist)
                            posValue += 2 * (minComfortDistance - dist);
                    }
            }

            return posValue;
        }

        public static float GetPositionValue(this Vector2 pos)
        {
            var posValue = pos.Distance(Game.CursorPos.To2D());

            if (ObjectCache.menuCache.cache["PreventDodgingUnderTower"].As<MenuBool>().Enabled)
            {
                var turretRange = 875 + ObjectCache.myHeroCache.boundingRadius;
                var distanceToTurrets = pos.GetDistanceToTurrets();

                if (turretRange > distanceToTurrets)
                    posValue += 5 * (turretRange - distanceToTurrets);
            }

            return posValue;
        }

        public static bool CheckDangerousPos(this Vector2 pos, float extraBuffer, bool checkOnlyDangerous = false)
        {
            foreach (var entry in SpellDetector.spells)
            {
                var spell = entry.Value;

                if (checkOnlyDangerous && spell.dangerlevel < 3)
                    continue;

                if (pos.InSkillShot(spell, ObjectCache.myHeroCache.boundingRadius + extraBuffer))
                    return true;
            }
            return false;
        }

        public static List<Vector2> GetSurroundingPositions(int maxPosToCheck = 150, int posRadius = 25)
        {
            var positions = new List<Vector2>();

            var posChecked = 0;
            var radiusIndex = 0;

            var heroPoint = ObjectCache.myHeroCache.serverPos2D;
            var lastMovePos = Game.CursorPos.To2D();

            var posTable = new List<PositionInfo>();

            while (posChecked < maxPosToCheck)
            {
                radiusIndex++;

                var curRadius = radiusIndex * 2 * posRadius;
                var curCircleChecks = (int) Math.Ceiling(2 * Math.PI * curRadius / (2 * (double) posRadius));

                for (var i = 1; i < curCircleChecks; i++)
                {
                    posChecked++;
                    var cRadians = 2 * Math.PI / (curCircleChecks - 1) * i; //check decimals
                    var pos = new Vector2((float) Math.Floor(heroPoint.X + curRadius * Math.Cos(cRadians)),
                        (float) Math.Floor(heroPoint.Y + curRadius * Math.Sin(cRadians)));

                    positions.Add(pos);
                }
            }

            return positions;
        }
    }
}