using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Aimtec;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Menu;
using Aimtec.SDK.Menu.Components;
using Aimtec.SDK.Util;
using zzzz.Draw;
//using SharpDX;

namespace zzzz
{
    internal class SpellTester
    {
        private static Obj_AI_Base test;

        public static Menu menu;
        public static Menu selectSpellMenu;

        private static readonly Dictionary<string, Dictionary<string, SpellData>> spellCache
            = new Dictionary<string, Dictionary<string, SpellData>>();

        public static Vector3 spellStartPosition = myHero.ServerPosition;

        public static Vector3 spellEndPostion = myHero.ServerPosition
                                                + (myHero.Orientation.To2D().Perpendicular() * 500).To3D();

        public static float lastSpellFireTime;

        private bool added = false;

        public SpellTester()
        {
            menu = new Menu("DummySpellTester", "Spell Tester", true);

            selectSpellMenu = new Menu("SelectSpellMenu", "Select Spell");
            menu.Add(selectSpellMenu);

            var setSpellPositionMenu = new Menu("SetPositionMenu", "Set Spell Position");
            setSpellPositionMenu.Add(new MenuBool("SetDummySpellStartPosition", "Set Start Position"));
            setSpellPositionMenu.Add(new MenuBool("SetDummySpellEndPosition", "Set End Position"));
            setSpellPositionMenu["SetDummySpellStartPosition"].OnValueChanged += OnSpellStartChange;
            setSpellPositionMenu["SetDummySpellEndPosition"].OnValueChanged += OnSpellEndChange;

            menu.Add(setSpellPositionMenu);

            var fireDummySpellMenu = new Menu("FireDummySpellMenu", "Fire Dummy Spell");
            fireDummySpellMenu.Add(new MenuKeyBind("FireDummySpell", "Fire Dummy Spell Key", KeyCode.O,
                KeybindType.Press));

            fireDummySpellMenu.Add(new MenuSlider("SpellInterval", "Spell Interval", 2500, 0, 5000));

            menu.Add(fireDummySpellMenu);
            ObjectCache.menuCache.AddMenuToCache(menu);
            menu.Attach();

            LoadSpellDictionary();

            Game.OnUpdate += Game_OnGameUpdate;
            Render.OnPresent += Render_OnPresent;
        }

        private static Obj_AI_Hero myHero => ObjectManager.GetLocalPlayer();

        private void Render_OnPresent()
        {
            foreach (var spell in SpellDetector.drawSpells.Values)
            {
                var spellPos = spell.currentSpellPosition;

                if (spell.heroID == myHero.NetworkId)
                    if (spell.spellType == SpellType.Line)
                    {
                        if (Vector2.Distance(spellPos, myHero.ServerPosition.To2D()) <=
                            myHero.BoundingRadius + spell.radius
                            && EvadeUtils.TickCount - spell.startTime > spell.info.spellDelay
                            && Vector2.Distance(spell.startPos, myHero.ServerPosition.To2D()) < spell.info.range)
                        {
                            RenderObjects.Add(new RenderCircle(spellPos, 1000, Color.Red,
                                (int) spell.radius, 10));
                            DelayAction.Add(1, () => SpellDetector.DeleteSpell(spell.spellID));
                        }
                        else
                        {
                            Render.Circle(new Vector3(spellPos.X, spellPos.Y, myHero.Position.Z), (int) spell.radius,
                                50, Color.White);
                        }
                    }
                    else if (spell.spellType == SpellType.Circular)
                    {
                        if (EvadeUtils.TickCount - spell.startTime >= spell.endTime - spell.startTime)
                            if (myHero.ServerPosition.To2D().InSkillShot(spell, myHero.BoundingRadius))
                            {
                                RenderObjects.Add(new RenderCircle(spellPos, 1000, Color.Red, (int) spell.radius, 5));
                                DelayAction.Add(1, () => SpellDetector.DeleteSpell(spell.spellID));
                            }
                    }
                    else if (spell.spellType == SpellType.Cone)
                    {
                        // SPELL TESTER
                        if (EvadeUtils.TickCount - spell.startTime >= spell.endTime - spell.startTime)
                            if (myHero.ServerPosition.To2D().InSkillShot(spell, myHero.BoundingRadius))
                                DelayAction.Add(1, () => SpellDetector.DeleteSpell(spell.spellID));
                    }
            }
        }

        private void Game_OnGameUpdate()
        {
            if (menu["FireDummySpellMenu"]["FireDummySpell"].As<MenuKeyBind>().Enabled)
            {
                float interval = menu["SpellInterval"].As<MenuSlider>().Value;

                if (EvadeUtils.TickCount - lastSpellFireTime > interval)
                {
                    var charName = selectSpellMenu["DummySpellHero"].As<MenuList>().SelectedItem;
                    var spellName = selectSpellMenu["DummySpellList"].As<MenuList>().SelectedItem;
                    var spellData = spellCache[charName][spellName];

                    //if (Evade.spellMenu[charName + spellName + "Settings"][spellName + "DodgeSpell"].Enabled)
                    //{
                    //if (Evade.spellMenu[charName + spellName + "Settings"][spellName + "DodgeSpell"].As<MenuBool>() == null)
                    //{

                    //if (!added)
                    //{
                    //    SpellDetector.LoadDummySpell(spellData);
                    //    SpellDetector.CreateSpellData(myHero, spellStartPosition, spellEndPostion, spellData);

                    //    added = true;
                    //}
                    //}

                    //lastSpellFireTime = EvadeUtils.TickCount;

                    if (!ObjectCache.menuCache.cache.ContainsKey(spellName + "DodgeSpell"))
                        SpellDetector.LoadDummySpell(spellData);

                    SpellDetector.CreateSpellData(myHero, spellStartPosition, spellEndPostion, spellData);
                    lastSpellFireTime = EvadeUtils.TickCount;
                }
            }
        }

        private void OnSpellEndChange(MenuComponent sender, ValueChangedArgs e)
        {
            // e.Process = false;

            spellEndPostion = myHero.ServerPosition;
            RenderObjects.Add(new RenderCircle(spellEndPostion.To2D(), 1000, Color.Red, 100, 20));
        }

        private void OnSpellStartChange(MenuComponent sender, ValueChangedArgs e)
        {
            //e.Process = false;

            spellStartPosition = myHero.ServerPosition;
            RenderObjects.Add(new RenderCircle(spellStartPosition.To2D(), 1000, Color.Red, 100, 20));
        }

        private void LoadSpellDictionary()
        {
            foreach (var spell in SpellDatabase.Spells)
                if (spellCache.ContainsKey(spell.charName))
                {
                    var spellList = spellCache[spell.charName];
                    if (spellList != null && !spellList.ContainsKey(spell.spellName))
                        spellList.Add(spell.spellName, spell);
                }
                else
                {
                    spellCache.Add(spell.charName, new Dictionary<string, SpellData>());
                    var spellList = spellCache[spell.charName];
                    if (spellList != null && !spellList.ContainsKey(spell.spellName))
                        spellList.Add(spell.spellName, spell);
                }

            selectSpellMenu.Add(new MenuBool("DummySpellDescription", "    -- Select A Dummy Spell To Fire --    "));

            var heroList = spellCache.Keys.ToArray();
            selectSpellMenu.Add(new MenuList("DummySpellHero", "Hero", heroList, 0));

            var selectedHeroStr = selectSpellMenu["DummySpellHero"].As<MenuList>().SelectedItem;
            var selectedHero = spellCache[selectedHeroStr];
            var selectedHeroList = selectedHero.Keys.ToArray();

            selectSpellMenu.Add(new MenuList("DummySpellList", "Spell", selectedHeroList, 0));

            selectSpellMenu["DummySpellHero"].OnValueChanged += OnSpellHeroChange;
        }

        private void OnSpellHeroChange(MenuComponent sender, ValueChangedArgs args)
        {
            //var previousHeroStr = e.GetOldValue<MenuList>().SelectedValue;
            var selectedHeroStr = args.GetNewValue<MenuList>().SelectedItem;
            var selectedHero = spellCache[selectedHeroStr];
            var selectedHeroList = selectedHero.Keys.ToArray();

            selectSpellMenu["DummySpellList"].As<MenuList>().Items = selectedHeroList;
        }
    }
}