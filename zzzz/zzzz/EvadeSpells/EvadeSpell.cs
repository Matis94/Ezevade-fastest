using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Aimtec;
using Aimtec.SDK.Events;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Menu;
using Aimtec.SDK.Menu.Components;

//using SharpDX;

namespace zzzz
{
    internal class EvadeSpell
    {
        public delegate void Callback();

        public static List<EvadeSpellData> evadeSpells = new List<EvadeSpellData>();
        public static List<EvadeSpellData> itemSpells = new List<EvadeSpellData>();

        public static EvadeCommand lastSpellEvadeCommand =
            new EvadeCommand {isProcessed = true, timestamp = EvadeUtils.TickCount};

        public static Menu menu;

        public EvadeSpell(Menu mainMenu)
        {
            menu = mainMenu;

            //Game.OnUpdate += Game_OnGameUpdate;

            Evade.evadeSpellMenu = new Menu("EvadeSpells", "Evade Spells");
            menu.Add(Evade.evadeSpellMenu);

            LoadEvadeSpellList();
            DelayAction.Add(100, () => CheckForItems());
        }

        private static Obj_AI_Hero myHero => ObjectManager.GetLocalPlayer();

        private void Game_OnGameUpdate()
        {
            //CheckDashing();
        }

        public static void CheckDashing()
        {
            if (EvadeUtils.TickCount - lastSpellEvadeCommand.timestamp < 250 && myHero.IsDashing()
                && lastSpellEvadeCommand.evadeSpellData.evadeType == EvadeType.Dash)
            {
                var dashInfo = myHero.GetDashInfo();

                //Console.WriteLine("" + dashInfo.EndPos.Distance(lastSpellEvadeCommand.targetPosition));
                lastSpellEvadeCommand.targetPosition = dashInfo.EndPos;
            }
        }

        private static void CheckForItems()
        {
            foreach (var spell in itemSpells)
            {
                var hasItem = myHero.HasItem(spell.itemID);

                if (hasItem && !evadeSpells.Exists(s => s.spellName == spell.spellName))
                {
                    evadeSpells.Add(spell);

                    var newSpellMenu = CreateEvadeSpellMenu(spell);
                    Evade.menu.Add(newSpellMenu);
                    //ObjectCache.menuCache.AddMenuToCache(newSpellMenu);
                }
            }

            DelayAction.Add(5000, () => CheckForItems());
        }

        private static Menu CreateEvadeSpellMenu(EvadeSpellData spell)
        {
            var menuName = spell.name + " (" + spell.spellKey + ") Settings";

            if (spell.isItem)
                menuName = spell.name + " Settings";

            var newSpellMenu = new Menu(spell.charName + spell.name + "EvadeSpellSettings", menuName);
            newSpellMenu.Add(new MenuBool(spell.name + "UseEvadeSpell", "Use Spell"));

            newSpellMenu.Add(new MenuList(spell.name + "EvadeSpellDangerLevel", "Danger Level",
                new[] {"Low", "Normal", "High", "Extreme"}, spell.dangerlevel - 1, false));
            //newSpellMenu.Add(new MenuComponent(spell.name + "SpellActivationTime", "Spell Activation Time").SetValue(new MenuSlider(0, 0, 1000)));

            //Menu newSpellMiscMenu = new Menu("Misc Settings", spell.charName + spell.name + "EvadeSpellMiscSettings");
            //newSpellMenu.Add(newSpellMiscMenu);

            newSpellMenu.Add(new MenuList(spell.name + "EvadeSpellMode", "Spell Mode",
                new[] {"Undodgeable", "Activation Time", "Always"}, GetDefaultSpellMode(spell)));

            Evade.evadeSpellMenu.Add(newSpellMenu);
            ObjectCache.menuCache.AddMenuToCache(newSpellMenu);

            return newSpellMenu;
        }

        public static int GetDefaultSpellMode(EvadeSpellData spell)
        {
            if (spell.dangerlevel > 3)
                return 0;

            return 1;
        }

        public static bool PreferEvadeSpell()
        {
            if (!Situation.ShouldUseEvadeSpell())
                return false;

            foreach (var entry in SpellDetector.spells)
            {
                var spell = entry.Value;

                if (!ObjectCache.myHeroCache.serverPos2D.InSkillShot(spell, ObjectCache.myHeroCache.boundingRadius))
                    continue;

                if (ActivateEvadeSpell(spell, true))
                    return true;
            }

            return false;
        }

        public static void UseEvadeSpell()
        {
            if (!Situation.ShouldUseEvadeSpell())
                return;

            //int posDangerlevel = EvadeHelper.CheckPosDangerLevel(ObjectCache.myHeroCache.serverPos2D, 0);

            if (EvadeUtils.TickCount - lastSpellEvadeCommand.timestamp < 1000)
                return;

            foreach (var entry in SpellDetector.spells)
            {
                var spell = entry.Value;

                if (ShouldActivateEvadeSpell(spell))
                    if (ActivateEvadeSpell(spell))
                    {
                        Evade.SetAllUndodgeable();
                        return;
                    }
            }
        }

        public static bool ActivateEvadeSpell(Spell spell, bool checkSpell = false)
        {
            if (spell.info.spellName.Contains("_trap"))
                return false;

            var sortedEvadeSpells = evadeSpells.OrderBy(s => s.dangerlevel);

            var extraDelayBuffer = ObjectCache.menuCache.cache["ExtraPingBuffer"].As<MenuSlider>().Value;
            var spellActivationTime = ObjectCache.menuCache.cache["SpellActivationTime"].As<MenuSlider>().Value +
                                      ObjectCache.gamePing + extraDelayBuffer;

            if (ObjectCache.menuCache.cache["CalculateWindupDelay"].As<MenuBool>().Enabled)
            {
                var extraWindupDelay = Evade.lastWindupTime - EvadeUtils.TickCount;
                if (extraWindupDelay > 0)
                    return false;
            }

            foreach (var evadeSpell in sortedEvadeSpells)
            {
                var processSpell = true;

                if (Evade.evadeSpellMenu[evadeSpell.charName + evadeSpell.name + "EvadeSpellSettings"][
                        evadeSpell.name + "UseEvadeSpell"].As<MenuBool>().Value == false
                    || GetSpellDangerLevel(evadeSpell) > spell.GetSpellDangerLevel() ||
                    !myHero.SpellBook.CanUseSpell(evadeSpell.spellKey) || evadeSpell.checkSpellName &&
                    myHero.SpellBook.GetSpell(evadeSpell.spellKey).Name != evadeSpell.spellName)
                    continue; //can't use spell right now               

                float evadeTime, spellHitTime;
                spell.CanHeroEvade(myHero, out evadeTime, out spellHitTime);

                var finalEvadeTime = spellHitTime - evadeTime;

                if (checkSpell)
                {
                    var mode =
                        Evade.evadeSpellMenu[evadeSpell.charName + evadeSpell.name + "EvadeSpellSettings"][
                                evadeSpell.name + "EvadeSpellMode"]
                            .As<MenuList>().Value;

                    if (mode == 0)
                        continue;
                    if (mode == 1)
                        if (spellActivationTime < finalEvadeTime)
                            continue;
                }
                else
                {
                    //if (Evade.menu[evadeSpell.name + "LastResort"].As<MenuBool>().Enabled)
                    if (evadeSpell.spellDelay <= 50 && evadeSpell.evadeType != EvadeType.Dash)
                    {
                        var path = myHero.Path;
                        if (path.Length > 0)
                        {
                            var movePos = path[path.Length - 1].To2D();
                            var posInfo =
                                EvadeHelper.CanHeroWalkToPos(movePos, ObjectCache.myHeroCache.moveSpeed, 0, 0);

                            if (GetSpellDangerLevel(evadeSpell) > posInfo.posDangerLevel)
                                continue;
                        }
                    }
                }

                if (evadeSpell.evadeType != EvadeType.Dash && spellHitTime > evadeSpell.spellDelay + 100 + Game.Ping +
                    ObjectCache.menuCache.cache["ExtraPingBuffer"].As<MenuSlider>().Value)
                {
                    processSpell = false;

                    if (checkSpell == false)
                        continue;
                }

                if (evadeSpell.isSpecial)
                {
                    if (evadeSpell.useSpellFunc != null)
                        if (evadeSpell.useSpellFunc(evadeSpell, processSpell))
                            return true;
                }
                else if (evadeSpell.evadeType == EvadeType.Blink)
                {
                    if (evadeSpell.castType == CastType.Position)
                    {
                        var posInfo = EvadeHelper.GetBestPositionBlink();
                        if (posInfo != null)
                        {
                            if (processSpell)
                                myHero.SpellBook.CastSpell(evadeSpell.spellKey, posInfo.position.To3D());
                            //CastEvadeSpell(() => EvadeCommand.CastSpell(evadeSpell, posInfo.position), processSpell);
                            //DelayAction.Add(50, () => myHero.IssueOrder(OrderType.MoveTo, posInfo.position.To3D()));
                            return true;
                        }
                    }
                    else if (evadeSpell.castType == CastType.Target)
                    {
                        var posInfo = EvadeHelper.GetBestPositionTargetedDash(evadeSpell);
                        if (posInfo != null && posInfo.target != null && posInfo.posDangerLevel == 0)
                        {
                            if (processSpell)
                                myHero.SpellBook.CastSpell(evadeSpell.spellKey, posInfo.target);
                            //CastEvadeSpell(() => EvadeCommand.CastSpell(evadeSpell, posInfo.target), processSpell);
                            //DelayAction.Add(50, () => myHero.IssueOrder(OrderType.MoveTo, posInfo.position.To3D()));
                            return true;
                        }
                    }
                }
                else if (evadeSpell.evadeType == EvadeType.Dash)
                {
                    if (evadeSpell.castType == CastType.Position)
                    {
                        var posInfo = EvadeHelper.GetBestPositionDash(evadeSpell);
                        if (posInfo != null && CompareEvadeOption(posInfo, checkSpell))
                        {
                            if (evadeSpell.isReversed)
                            {
                                var dir = (posInfo.position - ObjectCache.myHeroCache.serverPos2D).Normalized();
                                var range = ObjectCache.myHeroCache.serverPos2D.Distance(posInfo.position);
                                var pos = ObjectCache.myHeroCache.serverPos2D - dir * range;

                                posInfo.position = pos;
                            }

                            if (processSpell)
                                myHero.SpellBook.CastSpell(evadeSpell.spellKey, posInfo.position.To3D());
                            //DelayAction.Add(50, () => myHero.IssueOrder(OrderType.MoveTo, posInfo.position.To3D()));
                            return true;
                        }
                    }
                    else if (evadeSpell.castType == CastType.Target)
                    {
                        var posInfo = EvadeHelper.GetBestPositionTargetedDash(evadeSpell);
                        if (posInfo != null && posInfo.target != null && posInfo.posDangerLevel == 0)
                        {
                            if (processSpell)
                                myHero.SpellBook.CastSpell(evadeSpell.spellKey, posInfo.target);
                            //CastEvadeSpell(() => EvadeCommand.CastSpell(evadeSpell, posInfo.target), processSpell);
                            //DelayAction.Add(50, () => myHero.IssueOrder(OrderType.MoveTo, posInfo.position.To3D()));
                            return true;
                        }
                    }
                }
                else if (evadeSpell.evadeType == EvadeType.WindWall)
                {
                    if (spell.hasProjectile() || evadeSpell.spellName == "FioraW") //temp fix, don't have fiora :'(
                    {
                        var dir = (spell.startPos - ObjectCache.myHeroCache.serverPos2D).Normalized();
                        var pos = ObjectCache.myHeroCache.serverPos2D + dir * 100;

                        if (processSpell)
                            myHero.SpellBook.CastSpell(evadeSpell.spellKey, pos.To3D());
                        //CastEvadeSpell(() => EvadeCommand.CastSpell(evadeSpell, pos), processSpell);
                        return true;
                    }
                }
                else if (evadeSpell.evadeType == EvadeType.SpellShield)
                {
                    if (evadeSpell.isItem)
                    {
                        if (processSpell)
                            myHero.SpellBook.CastSpell(evadeSpell.spellKey);
                        //CastEvadeSpell(() => myHero.SpellBook.CastSpell(evadeSpell.spellKey), processSpell);
                        return true;
                    }

                    if (evadeSpell.castType == CastType.Target)
                    {
                        if (processSpell)
                            myHero.SpellBook.CastSpell(evadeSpell.spellKey, myHero);
                        // CastEvadeSpell(() => EvadeCommand.CastSpell(evadeSpell, myHero), processSpell);
                        return true;
                    }

                    if (evadeSpell.castType == CastType.Self)
                    {
                        if (processSpell)
                            myHero.SpellBook.CastSpell(evadeSpell.spellKey);
                        //CastEvadeSpell(() => EvadeCommand.CastSpell(evadeSpell), processSpell);
                        return true;
                    }
                }
                else if (evadeSpell.evadeType == EvadeType.MovementSpeedBuff)
                {
                    if (evadeSpell.isItem)
                    {
                        var posInfo = EvadeHelper.GetBestPosition();
                        if (posInfo != null)
                        {
                            if (processSpell)
                                myHero.SpellBook.CastSpell(evadeSpell.spellKey);
                            //CastEvadeSpell(() => myHero.SpellBook.CastSpell(evadeSpell.spellKey), processSpell);
                            DelayAction.Add(5, () => EvadeCommand.MoveTo(posInfo.position));
                            return true;
                        }
                    }
                    else
                    {
                        if (evadeSpell.castType == CastType.Self)
                        {
                            var posInfo = EvadeHelper.GetBestPosition();
                            if (posInfo != null)
                            {
                                if (processSpell)
                                    myHero.SpellBook.CastSpell(evadeSpell.spellKey);
                                //CastEvadeSpell(() => EvadeCommand.CastSpell(evadeSpell), processSpell);
                                DelayAction.Add(5, () => EvadeCommand.MoveTo(posInfo.position));
                                return true;
                            }
                        }

                        else if (evadeSpell.castType == CastType.Position)
                        {
                            var posInfo = EvadeHelper.GetBestPosition();
                            if (posInfo != null)
                            {
                                if (processSpell)
                                    myHero.SpellBook.CastSpell(evadeSpell.spellKey, posInfo.position.To3D());
                                //CastEvadeSpell(() => EvadeCommand.CastSpell(evadeSpell, posInfo.position), processSpell);
                                DelayAction.Add(5, () => EvadeCommand.MoveTo(posInfo.position));
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public static void CastEvadeSpell(Callback func, bool process = true)
        {
            if (process)
                func();
        }

        public static bool CompareEvadeOption(PositionInfo posInfo, bool checkSpell = false)
        {
            if (checkSpell)
                if (posInfo.posDangerLevel == 0)
                    return true;

            return posInfo.isBetterMovePos();
        }

        private static bool ShouldActivateEvadeSpell(Spell spell)
        {
            if (Evade.lastPosInfo == null)
                return false;


            if (ObjectCache.menuCache.cache["DodgeSkillShots"].As<MenuKeyBind>().Enabled)
            {
                if (Evade.lastPosInfo.undodgeableSpells.Contains(spell.spellID)
                    && ObjectCache.myHeroCache.serverPos2D.InSkillShot(spell, ObjectCache.myHeroCache.boundingRadius))
                    return true;
            }
            else
            {
                if (ObjectCache.myHeroCache.serverPos2D.InSkillShot(spell, ObjectCache.myHeroCache.boundingRadius))
                    return true;
            }


            /*float activationTime = Evade.menu.SubMenu("MiscSettings").SubMenu("EvadeSpellMisc").Item("EvadeSpellActivationTime")
                .As<MenuSlider>().Value + ObjectCache.gamePing;

            if (spell.spellHitTime != float.MinValue && activationTime > spell.spellHitTime - spell.evadeTime)
            {
                return true;
            }*/

            return false;
        }

        public static bool ShouldUseMovementBuff(Spell spell)
        {
            var sortedEvadeSpells = evadeSpells.Where(s => s.evadeType == EvadeType.MovementSpeedBuff)
                .OrderBy(s => s.dangerlevel);

            foreach (var evadeSpell in sortedEvadeSpells)
                if (Evade.evadeSpellMenu[evadeSpell.charName + evadeSpell.name + "EvadeSpellSettings"][
                        evadeSpell.name + "UseEvadeSpell"].As<MenuBool>().Value == false
                    || GetSpellDangerLevel(evadeSpell) > spell.GetSpellDangerLevel()
                    || evadeSpell.isItem == false && myHero.SpellBook.CanUseSpell(evadeSpell.spellKey) ||
                    evadeSpell.isItem && !myHero.SpellBook.CanUseSpell(evadeSpell.spellKey) ||
                    evadeSpell.checkSpellName && myHero.SpellBook.GetSpell(evadeSpell.spellKey).Name !=
                    evadeSpell.spellName)
                    return false;

            return true;
        }

        public static int GetSpellDangerLevel(EvadeSpellData spell)
        {
            var dangerStr =
                Evade.evadeSpellMenu[spell.charName + spell.name + "EvadeSpellSettings"][
                    spell.name + "EvadeSpellDangerLevel"].As<MenuList>().SelectedItem;

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

        private SpellSlot GetSummonerSlot(string spellName)
        {
            if (myHero.SpellBook.GetSpell(SpellSlot.Summoner1).Name == spellName)
            {
                return SpellSlot.Summoner1;
            }
            if (myHero.SpellBook.GetSpell(SpellSlot.Summoner2).SpellData.Name == spellName)
                return SpellSlot.Summoner2;

            return SpellSlot.Unknown;
        }

        private void LoadEvadeSpellList()
        {
            foreach (var spell in EvadeSpellDatabase.Spells.Where(
                s => s.charName == myHero.ChampionName || s.charName == "AllChampions"))
            {
                if (spell.isSummonerSpell)
                {
                    var spellKey = GetSummonerSlot(spell.spellName);
                    if (spellKey == SpellSlot.Unknown)
                        continue;

                    spell.spellKey = spellKey;
                }

                if (spell.isItem)
                {
                    itemSpells.Add(spell);
                    continue;
                }

                if (spell.isSpecial)
                    SpecialEvadeSpell.LoadSpecialSpell(spell);

                evadeSpells.Add(spell);

                var newSpellMenu = CreateEvadeSpellMenu(spell);
            }

            evadeSpells.Sort((a, b) => a.dangerlevel.CompareTo(b.dangerlevel));
        }
    }
}