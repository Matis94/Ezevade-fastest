using System;
using System.Linq;
using Aimtec;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Menu;
using Aimtec.SDK.Menu.Components;
using Aimtec.SDK.Orbwalking;
using Aimtec.SDK.Util;

//using SharpDX;

namespace zzzz
{
    internal class Evade
    {
        public static SpellDetector spellDetector;
        private static SpellDrawer spellDrawer;
        private static EvadeTester evadeTester;
        private static PingTester pingTester;
        private static EvadeSpell evadeSpell;
        private static SpellTester spellTester;
        private static AutoSetPing autoSetPing;

        public static SpellSlot lastSpellCast;
        public static float lastSpellCastTime;

        public static float lastWindupTime;

        public static float lastTickCount;
        public static float lastStopEvadeTime;

        public static Vector3 lastMovementBlockPos = Vector3.Zero;
        public static float lastMovementBlockTime;

        public static float lastEvadeOrderTime;
        public static float lastIssueOrderGameTime;
        public static float lastIssueOrderTime;
        public static Obj_AI_BaseIssueOrderEventArgs lastIssueOrderArgs;

        public static Vector2 lastMoveToPosition = Vector2.Zero;
        public static Vector2 lastMoveToServerPos = Vector2.Zero;
        public static Vector2 lastStopPosition = Vector2.Zero;

        public static DateTime assemblyLoadTime = DateTime.Now;

        public static bool isDodging;
        public static bool dodgeOnlyDangerous;

        public static bool devModeOn = false;
        public static bool hasGameEnded;
        public static bool isChanneling;
        public static Vector2 channelPosition = Vector2.Zero;

        public static PositionInfo lastPosInfo;

        public static EvadeCommand lastEvadeCommand =
            new EvadeCommand {isProcessed = true, timestamp = EvadeUtils.TickCount};

        public static EvadeCommand lastBlockedUserMoveTo =
            new EvadeCommand {isProcessed = true, timestamp = EvadeUtils.TickCount};

        public static float lastDodgingEndTime;

        public static Menu menu,
            miscMenu,
            keyMenu,
            mainMenu,
            limiterMenu,
            bufferMenu,
            fastEvadeMenu,
            spellMenu,
            drawMenu,
            autoSetPingMenu,
            evadeSpellMenu;

        public static float sumCalculationTime = 0;
        public static float numCalculationTime = 0;
        public static float avgCalculationTime = 0;

        public Evade()
        {
            LoadAssembly();
        }

        public static Obj_AI_Hero myHero => ObjectManager.GetLocalPlayer();

        private void LoadAssembly()
        {
            DelayAction.Add(0, () =>
            {
                if (Game.Mode == GameMode.Running)
                    Game_OnGameLoad();
                else
                    Game.OnStart += Game_OnGameLoad;
            });
        }

        private void Game_OnGameLoad()
        {
            try
            {
                // devModeOn = true;

                Obj_AI_Base.OnIssueOrder += Game_OnIssueOrder;
                SpellBook.OnCastSpell += Game_OnCastSpell;
                Game.OnUpdate += Game_OnGameUpdate;

                Obj_AI_Base.OnProcessSpellCast += Game_OnProcessSpell;

                Game.OnEnd += Game_OnGameEnd;
                SpellDetector.OnProcessDetectedSpells += SpellDetector_OnProcessDetectedSpells;
                var OrbwalkerInst = Orbwalker.OrbwalkerInstances.FirstOrDefault();
                if (OrbwalkerInst != null)
                    OrbwalkerInst.PreAttack += Orbwalker_PreAttack;


                menu = new Menu("ezevadeeeeeeeee", "ezEvade Ported by ya homeboy Sean", true);

                mainMenu = new Menu("MainMenu", "Main Menu");
                mainMenu.Add(new MenuKeyBind("DodgeSkillShots", "Dodge SkillShots", KeyCode.K, KeybindType.Toggle));
                mainMenu.Add(new MenuBool("DodgeDangerous", "Dodge Only Dangerous", false));
                mainMenu.Add(new MenuBool("DodgeCircularSpells", "Dodge Circular Spells"));
                mainMenu.Add(new MenuKeyBind("ActivateEvadeSpells", "Activate Evade Spells", KeyCode.K,
                    KeybindType.Toggle));
                mainMenu.Add(new MenuBool("DodgeFOWSpells", "Dodge FOW Spells"));
                menu.Add(mainMenu);

                keyMenu = new Menu("KeyMenu", "Key Menu");
                keyMenu.Add(new MenuBool("DodgeOnlyOnComboKeyEnabled", "Dodge Only On Combo Key Enabled", false));
                keyMenu.Add(new MenuKeyBind("DodgeComboKey", "Dodge Combo Key", KeyCode.Space, KeybindType.Press));
                keyMenu.Add(new MenuBool("DodgeDangerousKeyEnabled", "Enable Dodge Only Dangerous Keys", false));
                keyMenu.Add(new MenuKeyBind("DodgeDangerousKey", "Dodge Only Dangerous Key", KeyCode.Space,
                    KeybindType.Press));
                keyMenu.Add(new MenuKeyBind("DodgeDangerousKey2", "Dodge Only Dangerous Key 2", KeyCode.V,
                    KeybindType.Press));
                keyMenu.Add(new MenuBool("DontDodgeKeyEnabled", "Dont Dodge Key Enabled", false));
                keyMenu.Add(new MenuKeyBind("DontDodgeKey", "Dodge Combo Key", KeyCode.Z, KeybindType.Press));
                menu.Add(keyMenu);

                var loadTestMenu = new Menu("LoadTests", "Tests")
                {
                    new MenuBool("LoadPingTester", "Load Ping Tester", false),
                    new MenuBool("LoadSpellTester", "Load Spell Tester", false)
                };

                loadTestMenu["LoadPingTester"].OnValueChanged += OnLoadPingTesterChange;
                loadTestMenu["LoadSpellTester"].OnValueChanged += OnLoadSpellTesterChange;

                miscMenu = new Menu("MiscMenu", "Misc Menu");
                miscMenu.Add(new MenuBool("HigherPrecision", "Higher Precision"));
                miscMenu.Add(new MenuBool("RecalculatePosition", "Recalculate Position"));
                miscMenu.Add(new MenuBool("ContinueMovement", "Continue Previous Movement"));
                miscMenu.Add(new MenuBool("ClickRemove", "Click Remove"));
                miscMenu.Add(new MenuBool("CalculateWindupDelay", "Calculate Windup Delay"));
                miscMenu.Add(new MenuBool("AdvancedSpellDetection", "Advanced Spell Detection", true));
                miscMenu.Add(new MenuBool("CheckSpellCollision", "Check Spell Collision"));
                miscMenu.Add(new MenuList("EvadeMode", "Evade Profile",
                    new[] {"Smooth", "Very Smooth", "Fastest", "Hawk", "Kurisu", "GuessWho"}, 0));
                miscMenu.Add(new MenuBool("PreventDodgingUnderTower", "Prevent Dodging Under Tower"));
                miscMenu.Add(new MenuBool("PreventDodgingNearEnemy", "Prevent Dodging Near Enemy"));
                //miscMenu.Add(new MenuBool("DrawEvadePosition", "Draw Evade Position", false));
                miscMenu.Add(loadTestMenu);
                menu.Add(miscMenu);

                miscMenu["EvadeMode"].OnValueChanged += OnEvadeModeChange;

                bufferMenu = new Menu("BufferMenu", "Buffer Menu");
                bufferMenu.Add(new MenuSlider("ExtraSpellRadius", "Extra Spell Radius", 0, 0, 100));
                bufferMenu.Add(new MenuSlider("ExtraPingBuffer", "Extra Ping Buffer", 65, 0, 200));
                bufferMenu.Add(new MenuSlider("ExtraAvoidDistance", "Extra Avoid Distance", 50, 0, 300));
                bufferMenu.Add(new MenuSlider("ExtraEvadeDistance", "Extra Evade Distance", 100, 0, 300));
                bufferMenu.Add(new MenuSlider("ExtraCPADistance", "Extra Collision Distance", 10, 0, 150));
                bufferMenu.Add(new MenuSlider("MinComfortZone", "Min Distance to Champion", 550, 0, 1000));

                menu.Add(bufferMenu);

                limiterMenu = new Menu("LimiterMenu", "Humanizer Menu");
                limiterMenu.Add(new MenuSlider("SpellDetectionTime", "Spell Detection Time", 0, 0, 1000));
                limiterMenu.Add(new MenuSlider("ReactionTime", "Reaction Time", 0, 0, 500));
                limiterMenu.Add(new MenuSlider("DodgeInterval", "Dodge Interval Time", 0, 0, 2000));
                limiterMenu.Add(new MenuSlider("TickLimiter", "Tick Limiter", 100, 0, 500));
                limiterMenu.Add(new MenuBool("EnableEvadeDistance", "Extended Evade"));
                limiterMenu.Add(new MenuBool("ClickOnlyOnce", "Only Click Once"));
                menu.Add(limiterMenu);

                fastEvadeMenu = new Menu("FastEvade", "Fast Evade Menu");
                fastEvadeMenu.Add(new MenuBool("FastMovementBlock", "Fast Movement Block"));
                fastEvadeMenu.Add(new MenuSlider("FastEvadeActivationTime", "FastEvade Activation Time", 65, 0, 500));
                fastEvadeMenu.Add(new MenuSlider("SpellActivationTime", "Spell Activation Time", 400, 0, 1000));
                fastEvadeMenu.Add(new MenuSlider("RejectMinDistance", "Collision Distance Buffer", 10, 0, 100));
                menu.Add(fastEvadeMenu);

                //keyMenu = new Menu("KeySettings", "Key Settings")
                //{
                //    new MenuBool("DodgeDangerousKeyEnabled", "Enable Dodge Only Dangerous Keys"),
                //    new MenuKeyBind("DodgeDangerousKey", "Dodge Only Dangerous Key", KeyCode.Space, KeybindType.Press),
                //    new MenuKeyBind("DodgeDangerousKey2", "Dodge Only Dangerous Key 2", KeyCode.V, KeybindType.Press),
                //    new MenuBool("DodgeOnlyOnComboKeyEnabled", "Enable Dodge Only On Combo Key"),
                //    new MenuKeyBind("DodgeComboKey", "Dodge Only Combo Key", KeyCode.Space, KeybindType.Press),
                //    new MenuBool("DontDodgeKeyEnabled", "Enable Don't Dodge Key"),
                //    new MenuKeyBind("DontDodgeKey", "Don't Dodge Key", KeyCode.Z, KeybindType.Press)
                //};
                //menu.Add(keyMenu);

                //miscMenu = new Menu("MiscSettings", "Misc Settings")
                //{
                //    new MenuBool("HigherPrecision", "Enhanced Dodge Precision"),
                //    new MenuBool("RecalculatePosition", "Recalculate Path"),
                //    new MenuBool("ContinueMovement", "Continue Last Movement"),
                //    new MenuBool("CalculateWindupDelay", "Calculate Windup Delay"),
                //    new MenuBool("CheckSpellCollision", "Check Spell Collision"),
                //    new MenuBool("PreventDodgingUnderTower", "Prevent Dodging Under Tower"),
                //    new MenuBool("PreventDodgingNearEnemy", "Prevent Dodging Near Enemies"),
                //    new MenuBool("AdvancedSpellDetection", "Advanced Spell Detection"),
                //    new MenuBool("ClickRemove", "Allow Left Click Removal"),
                //    new MenuList("EvadeMode", "Evade Profile",
                //        new[] {"Smooth", "Very Smooth", "Fastest", "Hawk", "Kurisu", "GuessWho"}, 0),
                //    new MenuBool("ResetConfig", "Reset Evade Config")
                //};
                //menu.Add(miscMenu);

                //bufferMenu = new Menu("ExtraBuffers", "Extra Buffers")
                //{
                //    new MenuSlider("ExtraPingBuffer", "Extra Ping Buffer", 65, 0, 200),
                //    new MenuSlider("ExtraCPADistance", "Extra Collision Distance", 10, 0, 150),
                //    new MenuSlider("ExtraSpellRadius", "Extra Spell Radius", 0, 0, 100),
                //    new MenuSlider("ExtraEvadeDistance", "Extra Evade Distance", 100, 0, 300),
                //    new MenuSlider("ExtraAvoidDistance", "Extra Avoid Distance", 50, 0, 300),
                //    new MenuSlider("MinComfortZone", "Min Distance to Champion", 550, 0, 1000)
                //};
                //miscMenu.Add(bufferMenu);

                //mainMenu = new Menu("Main", "Main")
                //{
                //    new MenuKeyBind("DodgeSkillShots", "Dodge SkillShots", KeyCode.K, KeybindType.Toggle, true),
                //    new MenuKeyBind("ActivateEvadeSpells", "Use Evade Spells", KeyCode.K, KeybindType.Toggle, true),
                //    new MenuBool("DodgeDangerous", "Dodge Only Dangerous"),
                //    new MenuBool("DodgeFOWSpells", "Dodge FOW SkillShots"),
                //    new MenuBool("DodgeCircularSpells", "Dodge Circular SkillShots")
                //};
                //menu.Add(mainMenu);


                spellDetector = new SpellDetector(menu);
                evadeSpell = new EvadeSpell(menu);

                //miscMenu["EvadeMode"].OnValueChanged += OnEvadeModeChange;

                //limiterMenu = new Menu("Limiter", "Humanizer")
                //{
                //    new MenuBool("ClickOnlyOnce", "Click Only Once"),
                //    new MenuBool("EnableEvadeDistance", "Extended Evade"),
                //    new MenuSlider("TickLimiter", "Tick Limiter", 100, 0, 500),
                //    new MenuSlider("SpellDetectionTime", "Spell Detection Time", 0, 0, 1000),
                //    new MenuSlider("ReactionTime", "Reaction Time", 0, 0, 500),
                //    new MenuSlider("DodgeInterval", "Dodge Interval", 0, 0, 2000)
                //};

                //miscMenu.Add(limiterMenu);

                //fastEvadeMenu = new Menu("FastEvade", "Fast Evade")
                //{
                //    new MenuBool("FastMovementBlock", "Fast Movement Block"),
                //    new MenuSlider("FastEvadeActivationTime", "FastEvade Activation Time", 65, 0, 500),
                //    new MenuSlider("SpellActivationTime", "Spell Activation Time", 400, 0, 1000),
                //    new MenuSlider("RejectMinDistance", "Collision Distance Buffer", 10, 0, 100)
                //};

                //miscMenu.Add(fastEvadeMenu);               

                //Menu loadTestMenu = new Menu("LoadTests", "Tests")
                //{
                //    new MenuBool("LoadPingTester", "Load Ping Tester"),
                //    new MenuBool("LoadSpellTester", "Load Spell Tester")
                //};

                //loadTestMenu["LoadPingTester"].OnValueChanged += OnLoadPingTesterChange;
                //loadTestMenu["LoadSpellTester"].OnValueChanged += OnLoadSpellTesterChange;

                //miscMenu.Add(loadTestMenu);

                menu.Attach();


                spellDrawer = new SpellDrawer(menu);

                //autoSetPing = new AutoSetPing(menu);

                var initCache = ObjectCache.myHeroCache;

                if (devModeOn)
                {
                    var evadeTester = new Menu("ezevade: Test", "ezEvadeTest", true);
                    var o = new EvadeTester(evadeTester);
                    evadeTester.Attach();
                    //Utility.DelayAction.Add(100, () => loadTestMenu.Item("LoadSpellTester"));
                }

                Console.WriteLine("ezevade Loaded");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void ResetConfig(bool kappa = true)
        {
            //menu["(DodgeSkillShots)"].SetValue(new MenuKeyBind('K', KeybindType.Toggle, true));
            //menu["(ActivateEvadeSpells)"].SetValue(new MenuKeyBind('K', KeybindType.Toggle, true));
            //menu.Item("DodgeDangerous"));
            //menu["(DodgeFOWSpells)"].SetValue(true);
            //menu["(DodgeCircularSpells)"].SetValue(true);

            //menu["HigherPrecision"]. = true;
            //menu["(RecalculatePosition)"].SetValue(true);
            //menu["(ContinueMovement)"].SetValue(true);
            //menu["(CalculateWindupDelay)"].SetValue(true);
            //menu.Item("CheckSpellCollision"));
            //menu.Item("PreventDodgingUnderTower"));
            //menu["(PreventDodgingNearEnemy)"].SetValue(true);
            //menu.Item("AdvancedSpellDetection"));
            //menu["(LoadPingTester)"].SetValue(true);

            //menu["(ClickOnlyOnce)"].SetValue(true);
            //menu.Item("EnableEvadeDistance"));
            //menu["(TickLimiter)"].SetValue(new MenuSlider(100, 0, 500));
            //menu["(SpellDetectionTime)"].SetValue(new MenuSlider(0, 0, 1000));
            //menu["(ReactionTime)"].SetValue(new MenuSlider(0, 0, 500));
            //menu["(DodgeInterval)"].SetValue(new MenuSlider(0, 0, 2000));

            //menu.Item("FastMovementBlock"));
            //menu["(FastEvadeActivationTime)"].SetValue(new MenuSlider(65, 0, 500));
            //menu["(SpellActivationTime)"].SetValue(new MenuSlider(400, 0, 1000));
            //menu["(RejectMinDistance)"].SetValue(new MenuSlider(10, 0, 100));

            //menu["(ExtraPingBuffer)"].SetValue(new MenuSlider(65, 0, 200));
            //menu["(ExtraCPADistance)"].SetValue(new MenuSlider(10, 0, 150));
            //menu["(ExtraSpellRadius)"].SetValue(new MenuSlider(0, 0, 100));
            //menu["(ExtraEvadeDistance)"].SetValue(new MenuSlider(200, 0, 300));
            //menu["(ExtraAvoidDistance)"].SetValue(new MenuSlider(50, 0, 300));
            //menu["(MinComfortZone)"].SetValue(new MenuSlider(550, 0, 1000));

            //// drawings
            //menu["(DrawSkillShots)"].SetValue(true);
            //menu["(ShowStatus)"].SetValue(true);
            //menu.Item("DrawSpellPos"));
            //menu.Item("DrawEvadePosition"));

            //if (kappa)
            //{
            //    // profiles
            //    menu.Item("EvadeMode")
            //        .SetValue(new MenuList(new[] { "Smooth", "Very Smooth", "Fastest", "Hawk", "Kurisu", "GuessWho" }, 0));

            //    // keys
            //    menu.Item("DodgeDangerousKeyEnabled"));
            //    menu["(DodgeDangerousKey)"].SetValue(new MenuKeyBind(32, KeybindType.Press));
            //    menu["(DodgeDangerousKey2)"].SetValue(new MenuKeyBind('V', KeybindType.Press));
            //    menu.Item("DodgeOnlyOnComboKeyEnabled"));
            //    menu["(DodgeComboKey)"].SetValue(new MenuKeyBind(32, KeybindType.Press));
            //    menu.Item("DontDodgeKeyEnabled"));
            //    menu["(DontDodgeKey)"].SetValue(new MenuKeyBind('Z', KeybindType.Press));
            //}
        }

        private void OnEvadeModeChange(MenuComponent sender, ValueChangedArgs e)
        {
            var mode = e.GetNewValue<MenuList>().SelectedItem;

            if (mode == "Fastest")
            {
                ResetConfig(false);
                menu["FastMovementBlock"].As<MenuBool>().Value = true;
                menu["FastEvadeActivationTime"].As<MenuSlider>().Value = 120;
                menu["RejectMinDistance"].As<MenuSlider>().Value = 25;
                menu["ExtraCPADistance"].As<MenuSlider>().Value = 25;
                menu["ExtraPingBuffer"].As<MenuSlider>().Value = 80;
                menu["TickLimiter"].As<MenuSlider>().Value = 100;
                menu["SpellDetectionTime"].As<MenuSlider>().Value = 0;
                menu["ReactionTime"].As<MenuSlider>().Value = 0;
                menu["DodgeInterval"].As<MenuSlider>().Value = 0;
            }
            else if (mode == "Very Smooth")
            {
                ResetConfig(false);
                menu["FastEvadeActivationTime"].As<MenuSlider>().Value = 0;
                menu["RejectMinDistance"].As<MenuSlider>().Value = 0;
                menu["ExtraCPADistance"].As<MenuSlider>().Value = 0;
                menu["ExtraPingBuffer"].As<MenuSlider>().Value = 40;
            }
            else if (mode == "Smooth")
            {
                ResetConfig(false);
                menu["FastMovementBlock"].As<MenuBool>().Value = true;
                menu["FastEvadeActivationTime"].As<MenuSlider>().Value = 65;
                menu["RejectMinDistance"].As<MenuSlider>().Value = 10;
                menu["ExtraCPADistance"].As<MenuSlider>().Value = 10;
                menu["ExtraPingBuffer"].As<MenuSlider>().Value = 65;
            }

            else if (mode == "Hawk")
            {
                ResetConfig(false);
                menu["DodgeDangerous"].As<MenuBool>().Value = false;
                menu["DodgeFOWSpells"].As<MenuBool>().Value = false;
                menu["DodgeCircularSpells"].As<MenuBool>().Value = false;
                menu["DodgeDangerousKeyEnabled"].As<MenuBool>().Value = true;
                menu["HigherPrecision"].As<MenuBool>().Value = true;
                menu["RecalculatePosition"].As<MenuBool>().Value = true;
                menu["ContinueMovement"].As<MenuBool>().Value = true;
                menu["CalculateWindupDelay"].As<MenuBool>().Value = true;
                menu["CheckSpellCollision"].As<MenuBool>().Value = true;
                menu["PreventDodgingUnderTower"].As<MenuBool>().Value = false;
                menu["PreventDodgingNearEnemy"].As<MenuBool>().Value = true;
                menu["AdvancedSpellDetection"].As<MenuBool>().Value = true;
                menu["ClickOnlyOnce"].As<MenuBool>().Value = true;
                menu["EnableEvadeDistance"].As<MenuBool>().Value = true;
                menu["TickLimiter"].As<MenuSlider>().Value = 200;
                menu["SpellDetectionTime"].As<MenuSlider>().Value = 375;
                menu["ReactionTime"].As<MenuSlider>().Value = 285;
                menu["DodgeInterval"].As<MenuSlider>().Value = 235;
                menu["FastEvadeActivationTime"].As<MenuSlider>().Value = 0;
                menu["SpellActivationTime"].As<MenuSlider>().Value = 200;
                menu["RejectMinDistance"].As<MenuSlider>().Value = 0;
                menu["ExtraPingBuffer"].As<MenuSlider>().Value = 65;
                menu["ExtraCPADistance"].As<MenuSlider>().Value = 0;
                menu["ExtraSpellRadius"].As<MenuSlider>().Value = 0;
                menu["ExtraEvadeDistance"].As<MenuSlider>().Value = 200;
                menu["ExtraAvoidDistance"].As<MenuSlider>().Value = 200;
                menu["MinComfortZone"].As<MenuSlider>().Value = 550;
            }

            else if (mode == "Kurisu")
            {
                ResetConfig(false);
                menu["DodgeFOWSpells"].As<MenuBool>().Value = false;
                menu["DodgeDangerousKeyEnabled"].As<MenuBool>().Value = true;
                menu["RecalculatePosition"].As<MenuBool>().Value = true;
                menu["ContinueMovement"].As<MenuBool>().Value = true;
                menu["CalculateWindupDelay"].As<MenuBool>().Value = true;
                menu["PreventDodgingUnderTower"].As<MenuBool>().Value = true;
                menu["PreventDodgingNearEnemy"].As<MenuBool>().Value = true;
                menu["MinComfortZone"].As<MenuSlider>().Value = 850;
            }

            else if (mode == "GuessWho")
            {
                ResetConfig(false);
                menu["DodgeDangerousKeyEnabled"].As<MenuBool>().Value = true;
                //menu["DodgeDangerousKey2"].As<MenuBool>().Value = 109;
                menu["HigherPrecision"].As<MenuBool>().Value = true;
                menu["CheckSpellCollision"].As<MenuBool>().Value = true;
                menu["PreventDodgingUnderTower"].As<MenuBool>().Value = true;
                menu["ShowStatus"].As<MenuBool>().Value = false;
                menu["DrawSpellPos"].As<MenuBool>().Value = true;
            }
        }

        private void OnLoadPingTesterChange(MenuComponent sender, ValueChangedArgs e)
        {
            if (pingTester == null)
                pingTester = new PingTester();
        }

        private void OnLoadSpellTesterChange(MenuComponent sender, ValueChangedArgs e)
        {
            if (spellTester == null)
                spellTester = new SpellTester();
        }

        private void Game_OnGameEnd(GameObjectTeam team)
        {
            hasGameEnded = true;
        }

        private void Game_OnCastSpell(Obj_AI_Base sender, SpellBookCastSpellEventArgs args)
        {
            if (!sender.IsMe)
                return;

            var sData = sender.SpellBook.GetSpell(args.Slot);
            string name;

            if (SpellDetector.channeledSpells.TryGetValue(sData.Name, out name))
                lastStopEvadeTime = EvadeUtils.TickCount + ObjectCache.gamePing + 100;

            //block spell commmands if evade spell just used
            if (EvadeSpell.lastSpellEvadeCommand != null &&
                EvadeSpell.lastSpellEvadeCommand.timestamp + ObjectCache.gamePing + 150 > EvadeUtils.TickCount)
                args.Process = false;

            lastSpellCast = args.Slot;
            lastSpellCastTime = EvadeUtils.TickCount;

            //moved from processPacket

            /*if (args.Slot == SpellSlot.Recall)
            {
                lastStopPosition = myHero.ServerPosition.To2D();
            }*/

            // fix : uncomment all 
            if (Situation.ShouldDodge())
                if (isDodging && SpellDetector.spells.Any())
                    foreach (var entry in SpellDetector.windupSpells)
                    {
                        var spellData = entry.Value;

                        if (spellData.spellKey == args.Slot) //check if it's a spell that we should block
                        {
                            args.Process = false;
                            return;
                        }
                    }

            foreach (var evadeSpell in EvadeSpell.evadeSpells)
                if (evadeSpell.isItem == false && evadeSpell.spellKey == args.Slot &&
                    evadeSpell.untargetable == false)
                {
                    if (evadeSpell.evadeType == EvadeType.Blink)
                    {
                        var blinkPos = args.Start.To2D();

                        var posInfo = EvadeHelper.CanHeroWalkToPos(blinkPos, evadeSpell.speed, ObjectCache.gamePing, 0);
                        if (posInfo != null && posInfo.posDangerLevel == 0)
                        {
                            //Console.WriteLine("Evade spell");
                            EvadeCommand.MoveTo(posInfo.position);
                            lastStopEvadeTime = EvadeUtils.TickCount + ObjectCache.gamePing + evadeSpell.spellDelay;
                        }
                    }

                    if (evadeSpell.evadeType == EvadeType.Dash)
                    {
                        var dashPos = args.Start.To2D();
                        // fix : uncommont .target
                        if (args.Target != null)
                            dashPos = args.Target.Position.To2D();

                        if (evadeSpell.fixedRange || dashPos.Distance(myHero.ServerPosition.To2D()) > evadeSpell.range)
                        {
                            var dir = (dashPos - myHero.ServerPosition.To2D()).Normalized();
                            dashPos = myHero.ServerPosition.To2D() + dir * evadeSpell.range;
                        }

                        var posInfo = EvadeHelper.CanHeroWalkToPos(dashPos, evadeSpell.speed, ObjectCache.gamePing, 0);
                        if (posInfo != null && posInfo.posDangerLevel > 0)
                        {
                            args.Process = false;
                            return;
                        }

                        if (isDodging || EvadeUtils.TickCount < lastDodgingEndTime + 500)
                        {
                            //Console.WriteLine("Evade Spell 2");
                            EvadeCommand.MoveTo(Game.CursorPos.To2D());
                            lastStopEvadeTime = EvadeUtils.TickCount + ObjectCache.gamePing + 100;
                        }
                    }
                    return;
                }
        }

        private void Game_OnIssueOrder(Obj_AI_Base hero, Obj_AI_BaseIssueOrderEventArgs args)
        {
            if (!hero.IsMe)
                return;

            if (!Situation.ShouldDodge())
                return;

            var limitDelay = ObjectCache.menuCache.cache["TickLimiter"].As<MenuSlider>(); //Tick limiter                
            if (EvadeUtils.TickCount - lastTickCount > limitDelay.Value)
            {
                args.ProcessEvent = false;
                return;
            }


            //if (args.OrderType == OrderType.MoveTo)
            //{
            //    var end = args.Position;

            //    var path = myHero.Path;

            //    Console.WriteLine("got path");
            //}


            if (args.OrderType == OrderType.MoveTo)
            {
                if (isDodging && SpellDetector.spells.Any())
                {
                    CheckHeroInDanger();

                    lastBlockedUserMoveTo = new EvadeCommand
                    {
                        // fix all the args.Target.Position / args.Position
                        order = EvadeOrderCommand.MoveTo,
                        targetPosition = args /*.Target*/.Position.To2D(), // NOT SURE IF POSITION OR TARGET.POSITION
                        timestamp = EvadeUtils.TickCount,
                        isProcessed = false
                    };
                    //args.ProcessEvent = true;

                    args.ProcessEvent = false;
                }
                else
                {
                    var movePos = args.Position.To2D();
                    var extraDelay = ObjectCache.menuCache.cache["ExtraPingBuffer"].As<MenuSlider>().Value;

                    if (EvadeHelper.CheckMovePath(movePos, ObjectCache.gamePing + extraDelay))
                    {
                        /*if (ObjectCache.menuCache.cache["AllowCrossing"].As<MenuBool>().Enabled)
                        {
                            var extraDelayBuffer = ObjectCache.menuCache.cache["ExtraPingBuffer"]
                                .As<MenuSlider>().Value + 30;
                            var extraDist = ObjectCache.menuCache.cache["ExtraCPADistance"]
                                .As<MenuSlider>().Value + 10;

                            var tPosInfo = EvadeHelper.CanHeroWalkToPos(movePos, ObjectCache.myHeroCache.moveSpeed, extraDelayBuffer + ObjectCache.gamePing, extraDist);

                            if (tPosInfo.posDangerLevel == 0)
                            {
                                lastPosInfo = tPosInfo;
                                return;
                            }
                        }*/

                        lastBlockedUserMoveTo = new EvadeCommand
                        {
                            order = EvadeOrderCommand.MoveTo,
                            targetPosition = args /*.Target*/.Position.To2D(),
                            timestamp = EvadeUtils.TickCount,
                            isProcessed = false
                        };

                        args.ProcessEvent = false; //Block the command
                        // args.ProcessEvent = true;

                        if (EvadeUtils.TickCount - lastMovementBlockTime < 500 &&
                            lastMovementBlockPos.Distance(args. /*Target.*/Position) < 100)
                            return;

                        lastMovementBlockPos = args. /*Target.*/Position;
                        lastMovementBlockTime = EvadeUtils.TickCount;

                        var posInfo = EvadeHelper.GetBestPositionMovementBlock(movePos);
                        if (posInfo != null)
                            EvadeCommand.MoveTo(posInfo.position);
                        return;
                    }
                    lastBlockedUserMoveTo.isProcessed = true;
                }
            }
            else //need more logic
            {
                if (isDodging)
                {
                    args.ProcessEvent = false; //Block the command
                }
                else
                {
                    if (args.OrderType == OrderType.AttackUnit)
                    {
                        var target = args.Target;
                        if (target != null && target.IsValid)
                        {
                            // fix
                            var baseTarget = target as Obj_AI_Base;
                            if (baseTarget != null &&
                                ObjectCache.myHeroCache.serverPos2D.Distance(baseTarget.ServerPosition.To2D()) >
                                myHero.AttackRange + ObjectCache.myHeroCache.boundingRadius + baseTarget.BoundingRadius)
                            {
                                var movePos = args /*.Target*/.Position.To2D();
                                var extraDelay = ObjectCache.menuCache.cache["ExtraPingBuffer"].As<MenuSlider>().Value;
                                if (EvadeHelper.CheckMovePath(movePos, ObjectCache.gamePing + extraDelay))
                                {
                                    args.ProcessEvent = false; //Block the command
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            if (args.ProcessEvent)
            {
                lastIssueOrderGameTime = Game.ClockTime * 1000;
                lastIssueOrderTime = EvadeUtils.TickCount;
                lastIssueOrderArgs = args;

                if (args.OrderType == OrderType.MoveTo)
                {
                    lastMoveToPosition = args /*.Target*/.Position.To2D();
                    lastMoveToServerPos = myHero.ServerPosition.To2D();
                }

                if (args.OrderType == OrderType.Stop)
                    lastStopPosition = myHero.ServerPosition.To2D();
            }
        }

        private void Orbwalker_PreAttack(object sender, PreAttackEventArgs e)
        {
            if (isDodging)
                e.Cancel = true; //Block orbwalking
        }

        private void Game_OnProcessSpell(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs args)
        {
            if (!sender.IsMe)
                return;

            string name;
            if (SpellDetector.channeledSpells.TryGetValue(args.SpellData.Name.ToLower(), out name))
            {
                isChanneling = true;
                channelPosition = myHero.ServerPosition.To2D();
            }

            if (ObjectCache.menuCache.cache["CalculateWindupDelay"].As<MenuBool>().Enabled)
            {
                var castTime = (sender.SpellBook.CastEndTime - Game.ClockTime) * 1000;

                if (castTime > 0 && args.SpellData.ConsideredAsAutoAttack
                    && Math.Abs(castTime - myHero.AttackCastDelay * 1000) > 1)
                {
                    lastWindupTime = EvadeUtils.TickCount + castTime - Game.Ping / 2;

                    if (isDodging)
                        SpellDetector_OnProcessDetectedSpells(); //reprocess
                }
            }
        }

        private void Game_OnGameUpdate()
        {
            try
            {
                ObjectCache.myHeroCache.UpdateInfo();
                CheckHeroInDanger();

                if (isChanneling && channelPosition.Distance(ObjectCache.myHeroCache.serverPos2D) > 50
                    && !myHero.SpellBook.IsChanneling)
                    isChanneling = false;

                // fix
                //if (ObjectCache.menuCache.cache["ResetConfig"].As<MenuBool>().Enabled)
                //{
                //    //ResetConfig();
                //    //menu.Item("ResetConfig"));
                //}

                var limitDelay =
                    ObjectCache.menuCache.cache["TickLimiter"].As<MenuSlider>(); //Tick limiter                
                if (EvadeHelper.fastEvadeMode || EvadeUtils.TickCount - lastTickCount > limitDelay.Value)
                {
                    if (EvadeUtils.TickCount > lastStopEvadeTime)
                    {
                        DodgeSkillShots(); //walking           
                        ContinueLastBlockedCommand();
                    }

                    lastTickCount = EvadeUtils.TickCount;
                }

                EvadeSpell.UseEvadeSpell(); //using spells
                CheckDodgeOnlyDangerous();
                RecalculatePath();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void RecalculatePath()
        {
            if (ObjectCache.menuCache.cache["RecalculatePosition"].As<MenuBool>().Enabled && isDodging) //recheck path
                if (lastPosInfo != null && !lastPosInfo.recalculatedPath)
                {
                    var path = myHero.Path;
                    if (path.Length > 0)
                    {
                        var movePos = path.Last().To2D();

                        if (movePos.Distance(lastPosInfo.position) < 5) //more strict checking
                        {
                            var posInfo =
                                EvadeHelper.CanHeroWalkToPos(movePos, ObjectCache.myHeroCache.moveSpeed, 0, 0, false);
                            if (posInfo.posDangerCount > lastPosInfo.posDangerCount)
                            {
                                lastPosInfo.recalculatedPath = true;

                                if (EvadeSpell.PreferEvadeSpell())
                                {
                                    lastPosInfo = PositionInfo.SetAllUndodgeable();
                                }
                                else
                                {
                                    var newPosInfo = EvadeHelper.GetBestPosition();
                                    if (newPosInfo.posDangerCount < posInfo.posDangerCount)
                                    {
                                        lastPosInfo = newPosInfo;
                                        CheckHeroInDanger();
                                        DodgeSkillShots();
                                    }
                                }
                            }
                        }
                    }
                }
        }

        private void ContinueLastBlockedCommand()
        {
            if (ObjectCache.menuCache.cache["ContinueMovement"].As<MenuBool>().Enabled
                && Situation.ShouldDodge())
            {
                var movePos = lastBlockedUserMoveTo.targetPosition;
                var extraDelay = ObjectCache.menuCache.cache["ExtraPingBuffer"].As<MenuSlider>().Value;

                if (isDodging == false && lastBlockedUserMoveTo.isProcessed == false
                    && EvadeUtils.TickCount - lastEvadeCommand.timestamp > ObjectCache.gamePing + extraDelay
                    && EvadeUtils.TickCount - lastBlockedUserMoveTo.timestamp < 1500)
                {
                    movePos = movePos + (movePos - ObjectCache.myHeroCache.serverPos2D).Normalized()
                              * EvadeUtils.random.Next(1, 65);

                    if (!EvadeHelper.CheckMovePath(movePos, ObjectCache.gamePing + extraDelay))
                    {
                        //Console.WriteLine("Continue Movement");
                        //myHero.IssueOrder(OrderType.MoveTo, movePos.To3D());
                        EvadeCommand.MoveTo(movePos);
                        lastBlockedUserMoveTo.isProcessed = true;
                    }
                }
            }
        }

        private void CheckHeroInDanger()
        {
            var playerInDanger = false;

            foreach (var entry in SpellDetector.spells)
            {
                var spell = entry.Value;

                if (lastPosInfo != null && lastPosInfo.dodgeableSpells.Contains(spell.spellID))
                {
                    if (myHero.ServerPosition.To2D().InSkillShot(spell, ObjectCache.myHeroCache.boundingRadius))
                    {
                        playerInDanger = true;
                        break;
                    }

                    if (ObjectCache.menuCache.cache["EnableEvadeDistance"].As<MenuBool>().Enabled &&
                        EvadeUtils.TickCount < lastPosInfo.endTime)
                    {
                        playerInDanger = true;
                        break;
                    }
                }
            }

            if (isDodging && !playerInDanger)
                lastDodgingEndTime = EvadeUtils.TickCount;

            if (isDodging == false && !Situation.ShouldDodge())
                return;

            isDodging = playerInDanger;
        }

        private void DodgeSkillShots()
        {
            if (!Situation.ShouldDodge())
            {
                isDodging = false;
                return;
            }
            /*
            if (isDodging && playerInDanger == false) //serverpos test
            {
                myHero.IssueOrder(OrderType.HoldPosition, myHero, false);
            }*/

            if (isDodging)
            {
                if (lastPosInfo != null)
                {
                    /*foreach (KeyValuePair<int, Spell> entry in SpellDetector.spells)
                    {
                        Spell spell = entry.Value;

                        Console.WriteLine("" + (int)(TickCount-spell.startTime));
                    }*/


                    var lastBestPosition = lastPosInfo.position;

                    if (ObjectCache.menuCache.cache["ClickOnlyOnce"].As<MenuBool>().Value == false
                        || !(myHero.Path.Count() > 0 && lastPosInfo.position.Distance(myHero.Path.Last().To2D()) < 5))
                        //|| lastPosInfo.timestamp > lastEvadeOrderTime)
                    {
                        // Console.WriteLine("DodgeSkillshots");
                        EvadeCommand.MoveTo(lastBestPosition);
                        lastEvadeOrderTime = EvadeUtils.TickCount;
                    }
                }
            }
            else //if not dodging
            {
                //Check if hero will walk into a skillshot
                var path = myHero.Path;
                //if (path == null)
                //    return;

                if (path.Length > 0)
                {
                    var movePos = path[path.Length - 1].To2D();

                    if (EvadeHelper.CheckMovePath(movePos))
                    {
                        /*if (ObjectCache.menuCache.cache["AllowCrossing"].As<MenuBool>().Enabled)
                        {
                            var extraDelayBuffer = ObjectCache.menuCache.cache["ExtraPingBuffer"]
                                .As<MenuSlider>().Value + 30;
                            var extraDist = ObjectCache.menuCache.cache["ExtraCPADistance"]
                                .As<MenuSlider>().Value + 10;

                            var tPosInfo = EvadeHelper.CanHeroWalkToPos(movePos, ObjectCache.myHeroCache.moveSpeed, extraDelayBuffer + ObjectCache.gamePing, extraDist);

                            if (tPosInfo.posDangerLevel == 0)
                            {
                                lastPosInfo = tPosInfo;
                                return;
                            }
                        }*/

                        var posInfo = EvadeHelper.GetBestPositionMovementBlock(movePos);
                        if (posInfo != null)
                            EvadeCommand.MoveTo(posInfo.position);
                    }
                }
            }
        }

        public void CheckLastMoveTo()
        {
            if (EvadeHelper.fastEvadeMode || ObjectCache.menuCache.cache["FastMovementBlock"].As<MenuBool>().Enabled)
                if (isDodging == false)
                    if (lastIssueOrderArgs != null && lastIssueOrderArgs.OrderType == OrderType.MoveTo)
                        if (Game.ClockTime * 1000 - lastIssueOrderGameTime < 500)
                        {
                            Game_OnIssueOrder(myHero, lastIssueOrderArgs);
                            lastIssueOrderArgs = null;
                        }
        }

        public static bool isDodgeDangerousEnabled()
        {
            // fix
            //return true;
            if (ObjectCache.menuCache.cache["DodgeDangerous"].As<MenuBool>().Value)
                return true;

            if (ObjectCache.menuCache.cache["DodgeDangerousKeyEnabled"].As<MenuBool>().Value)
                if (ObjectCache.menuCache.cache["DodgeDangerousKey"].As<MenuKeyBind>().Enabled
                    || ObjectCache.menuCache.cache["DodgeDangerousKey2"].As<MenuKeyBind>().Enabled)
                    return true;

            return false;
        }

        public static void CheckDodgeOnlyDangerous() //Dodge only dangerous event
        {
            var bDodgeOnlyDangerous = isDodgeDangerousEnabled();

            if (dodgeOnlyDangerous == false && bDodgeOnlyDangerous)
            {
                // fix
                spellDetector.RemoveNonDangerousSpells();
                dodgeOnlyDangerous = true;
            }
            else
            {
                dodgeOnlyDangerous = bDodgeOnlyDangerous;
            }
        }

        public static void SetAllUndodgeable()
        {
            lastPosInfo = PositionInfo.SetAllUndodgeable();
        }

        private void SpellDetector_OnProcessDetectedSpells()
        {
            ObjectCache.myHeroCache.UpdateInfo();

            if (ObjectCache.menuCache.cache["DodgeSkillShots"].As<MenuKeyBind>().Enabled == false)
            {
                lastPosInfo = PositionInfo.SetAllUndodgeable();
                EvadeSpell.UseEvadeSpell();
                return;
            }

            if (ObjectCache.myHeroCache.serverPos2D.CheckDangerousPos(0)
                || ObjectCache.myHeroCache.serverPos2DExtra.CheckDangerousPos(0))
            {
                if (EvadeSpell.PreferEvadeSpell())
                {
                    lastPosInfo = PositionInfo.SetAllUndodgeable();
                }
                else
                {
                    var posInfo = EvadeHelper.GetBestPosition();
                    var calculationTimer = EvadeUtils.TickCount;
                    var caculationTime = EvadeUtils.TickCount - calculationTimer;
                    //computing time
                    /*if (numCalculationTime > 0)
                    {
                        sumCalculationTime += caculationTime;
                        avgCalculationTime = sumCalculationTime / numCalculationTime;
                    }
                    numCalculationTime += 1;*/

                    //Console.WriteLine("CalculationTime: " + caculationTime);

                    /*if (EvadeHelper.GetHighestDetectedSpellID() > EvadeHelper.GetHighestSpellID(posInfo))
                    {
                        return;
                    }*/
                    if (posInfo != null)
                    {
                        lastPosInfo = posInfo.CompareLastMovePos();

                        var travelTime = ObjectCache.myHeroCache.serverPos2DPing.Distance(lastPosInfo.position) /
                                         myHero.MoveSpeed;

                        lastPosInfo.endTime = EvadeUtils.TickCount + travelTime * 1000 - 100;
                    }

                    CheckHeroInDanger();

                    if (EvadeUtils.TickCount > lastStopEvadeTime)
                        DodgeSkillShots(); //walking           

                    CheckLastMoveTo();
                    EvadeSpell.UseEvadeSpell(); //using spells
                }
            }
            else
            {
                lastPosInfo = PositionInfo.SetAllDodgeable();
                CheckLastMoveTo();
            }


            //Console.WriteLine("SkillsDodged: " + lastPosInfo.dodgeableSpells.Count + " DangerLevel: " + lastPosInfo.undodgeableSpells.Count);            
        }
    }
}
