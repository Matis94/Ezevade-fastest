using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Aimtec;
using Aimtec.SDK.Events;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Menu;
using Aimtec.SDK.Menu.Components;
using Aimtec.SDK.Util;
using Aimtec.SDK.Util.Cache;
using zzzz.Draw;

//using SharpDX;

namespace zzzz
{
    internal class EvadeTester
    {
        public static Menu menu;
        public static Menu testMenu;

        private static Vector2 circleRenderPos;

        private static Vector2 startWalkPos;
        private static float startWalkTime;

        private static Vector2 testCollisionPos;
        private static bool testingCollision = false;

        private static float lastStopingTime;

        private static IOrderedEnumerable<PositionInfo> sortedBestPos;

        private static float lastGameTimerStart;
        private static float lastTickCountTimerStart;
        private static float lastWatchTimerStart;

        private static float lastGameTimerTick;
        private static float lastTickCountTimerTick;
        private static float lastWatchTimerTick;

        public static float lastProcessPacketTime = 0;

        private static float lastTimerCheck;
        private static bool lastRandomMoveCoeff = false;

        private static float lastRightMouseClickTime;

        private static EvadeCommand lastTestMoveToCommand;

        private static float lastSpellCastTimeEx;
        private static float lastSpellCastTime;
        private static float lastHeroSpellCastTime;

        private static MissileClient testMissile;
        private static float testMissileStartTime;
        private static float testMissileStartSpeed = 0;

        public EvadeTester(Menu mainMenu)
        {
            lastGameTimerStart = getGameTimer;
            lastTickCountTimerStart = getTickCountTimer;
            lastWatchTimerStart = getWatchTimer;

            lastTimerCheck = getTickCountTimer;

            Render.OnPresent += Render_OnPresent;
            Obj_AI_Base.OnIssueOrder += Game_OnIssueOrder;
            Game.OnUpdate += Game_OnGameUpdate;
            // Game.OnInput += Game_OnGameInput;


            //Game.OnSendPacket += Game_onSendPacket;


            //Game.OnProcessPacket += Game_onRecvPacket;

            GameObject.OnDestroy += Game_OnDelete;

            GameObject.OnCreate += SpellMissile_OnCreate;

            Obj_AI_Base.OnProcessSpellCast += Game_ProcessSpell;
            SpellBook.OnCastSpell += Game_OnCastSpell;
            //GameObject.OnFloatPropertyChange += GameObject_OnFloatPropertyChange;

            AttackableUnit.OnDamage += Game_OnDamage;
            //GameObject.OnIntegerPropertyChange += GameObject_OnIntegerPropertyChange;
            //Game.OnGameNotifyEvent += Game_OnGameNotifyEvent;

            Game.OnWndProc += Game_OnWndProc;

            Obj_AI_Base.OnPerformCast += Game_OnDoCast;

            Obj_AI_Base.OnNewPath += ObjAiHeroOnOnNewPath;

            SpellDetector.OnProcessDetectedSpells += SpellDetector_OnProcessDetectedSpells;

            //menu = mainMenu;

            testMenu = new Menu("Test", "Test");
            testMenu.Add(new MenuBool("(TestWall)", "TestWall"));
            testMenu.Add(new MenuBool("(TestPath)", "TestPath"));
            testMenu.Add(new MenuBool("(TestTracker)", "TestTracker"));
            testMenu.Add(new MenuBool("(TestHeroPos)", "TestHeroPos"));
            testMenu.Add(new MenuBool("(DrawHeroPos)", "DrawHeroPos"));
            testMenu.Add(new MenuBool("(TestSpellEndTime)", "TestSpellEndTime"));
            testMenu.Add(new MenuBool("(ShowBuffs)", "ShowBuffs"));
            testMenu.Add(new MenuBool("(ShowDashInfo)", "ShowDashInfo"));
            testMenu.Add(new MenuBool("(ShowProcessSpell)", "ShowProcessSpell"));
            testMenu.Add(new MenuBool("(ShowDoCastInfo)", "ShowDoCastInfo"));
            testMenu.Add(new MenuBool("(ShowMissileInfo)", "ShowMissileInfo"));
            testMenu.Add(new MenuBool("(ShowWindupTime)", "ShowWindupTime"));
            testMenu.Add(new MenuKeyBind("(TestMoveTo)", "TestMoveTo", KeyCode.L, KeybindType.Toggle, false));
            testMenu.Add(new MenuBool("(EvadeTesterPing)", "EvadeTesterPing"));
            Evade.menu.Add(testMenu);

            Game_OnGameLoad();
        }

        private static Obj_AI_Hero myHero => ObjectManager.GetLocalPlayer();

        private static float getGameTimer => Game.ClockTime * 1000;
        private static float getTickCountTimer => Environment.TickCount & int.MaxValue;
        private static float getWatchTimer => EvadeUtils.TickCount;

        private void Game_OnDoCast(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs args)
        {
            if (!testMenu["(ShowDoCastInfo)"].As<MenuBool>().Enabled)
                return;

            ConsolePrinter.Print("DoCast " + sender.Name + ": " + args.SpellData.Name);
        }

        private void Game_OnWndProc(WndProcEventArgs args)
        {
            if (args.Message == (uint) WindowsMessages.WM_RBUTTONDOWN)
                lastRightMouseClickTime = EvadeUtils.TickCount;
        }

        //private void Game_onRecvPacket(GamePacketEventArgs args)
        //{
        //    if (args.GetPacketId() == 178)
        //    {
        //        /*
        //        //ConsolePrinter.Print(args.GetPacketId());

        //        foreach (var data in args.PacketData)
        //        {
        //            Console.Write(" " + data);
        //        }
        //        ConsolePrinter.Print("");*/

        //        lastProcessPacketTime = EvadeUtils.TickCount;
        //    }
        //}

        //private void Game_onSendPacket(GamePacketEventArgs args)
        //{
        //    if (args.GetPacketId() == 160)
        //    {
        //        if (testMenu["(EvadeTesterPing)"].As<MenuBool>().Enabled)
        //        {
        //            ConsolePrinter.Print("Send Path ClickTime: " + (EvadeUtils.TickCount - lastRightMouseClickTime));
        //        }
        //    }
        //}

        private void Game_OnGameLoad()
        {
            ConsolePrinter.Print("EvadeTester loaded");
            //menu.Add(new Menu("Test", "Test"));

            //ConsolePrinter.Print("Ping:" + ObjectCache.gamePing);
            if (testMenu["(ShowBuffs)"].As<MenuBool>().Enabled)
            {
                //ConsolePrinter.Print(myHero);
            }
        }

        //private void Game_OnGameInput(GameInputEventArgs args)
        //{
        //    ConsolePrinter.Print("" + args.Input);

        //}

        private static void ObjAiHeroOnOnNewPath(Obj_AI_Base unit, Obj_AI_BaseNewPathEventArgs args)
        {
            if (unit.Type == GameObjectType.obj_AI_Hero)
            {
                if (testMenu["(TestSpellEndTime)"].As<MenuBool>().Enabled)
                {
                    //ConsolePrinter.Print("Dash windup: " + (EvadeUtils.TickCount - EvadeSpell.lastSpellEvadeCommand.timestamp));
                }

                if (args.IsDash && testMenu["(ShowDashInfo)"].As<MenuBool>().Enabled)
                {
                    var dist = args.Path.First().Distance(args.Path.Last());
                    ConsolePrinter.Print("Dash Speed: " + args.Speed + " Dash dist: " + dist);
                }

                if (unit.IsMe && testMenu["(EvadeTesterPing)"].As<MenuBool>().Enabled
                    && args.Path.Count() > 1)
                {
                    //ConsolePrinter.Print("Received Path ClickTime: " + (EvadeUtils.TickCount - lastRightMouseClickTime));
                }

                if (unit.IsMe)
                {
                    //Draw.RenderObjects.Add(new Draw.RenderCircle(args.Path.Last().To2D(), 500));
                    //Draw.RenderObjects.Add(new Draw.RenderCircle(args.Path.First().To2D(), 500));
                }
            }
        }

        private void Game_OnCastSpell(Obj_AI_Base sender, SpellBookCastSpellEventArgs args)
        {
            if (!sender.IsMe)
                return;

            if (testMenu["TestPath"].As<MenuBool>().Enabled)
                RenderObjects.Add(new RenderCircle(args.End.To2D(), 500));

            lastSpellCastTimeEx = EvadeUtils.TickCount;
        }

        private void SpellDetector_OnProcessDetectedSpells()
        {
            //var pos1 = newSpell.startPos;//SpellDetector.GetCurrentSpellPosition(newSpell);
            //DelayAction.Add(250, () => CompareSpellLocation2(newSpell));

            sortedBestPos = EvadeHelper.GetBestPositionTest();
            circleRenderPos = Evade.lastPosInfo.position;

            lastSpellCastTime = EvadeUtils.TickCount;
        }

        private void Game_OnDelete(GameObject sender)
        {
            if (testMenu["(ShowMissileInfo)"].As<MenuBool>().Enabled)
                if (testMissile != null && testMissile.NetworkId == sender.NetworkId)
                {
                    var range = sender.Position.To2D().Distance(testMissile.StartPosition.To2D());
                    ConsolePrinter.Print("[" + testMissile.SpellData.Name + "]: Est.Missile range: " + range);
                    ConsolePrinter.Print("[" + testMissile.SpellData.Name + "]: Est.Missile speed: " +
                                         1000 * (range / (EvadeUtils.TickCount - testMissileStartTime)));
                }
        }

        private void SpellMissile_OnCreate(GameObject obj)
        {
            /*if (sender.Name.ToLower().Contains("minion")
                || sender.Name.ToLower().Contains("turret")
                || sender.Type == GameObjectType.obj_GeneralParticleEmitter)
            {
                return;
            }

            if (sender.IsValid<MissileClient>())
            {
                var tMissile = sender as MissileClient;
                if (tMissile.SpellCaster.Type != GameObjectType.obj_AI_Hero)
                {
                    return;
                }
            }

            ConsolePrinter.Print(sender.Type + " : " + sender.Name);*/

            if (obj.IsValid && obj.Type == GameObjectType.MissileClient)
            {
                var mis = (MissileClient) obj;

                if (mis.SpellCaster is Obj_AI_Hero && mis.SpellData.ConsideredAsAutoAttack)
                {
                    ConsolePrinter.Print("[" + mis.SpellData.Name + "]: Missile Speed " + mis.SpellData.MissileSpeed);
                    ConsolePrinter.Print("[" + mis.SpellData.Name + "]: LineWidth " + mis.SpellData.LineWidth);
                    ConsolePrinter.Print("[" + mis.SpellData.Name + "]: Range " + mis.SpellData.CastRange);
                    ConsolePrinter.Print("[" + mis.SpellData.Name + "]: Accel " + mis.SpellData.MissileAccel);
                }
            }


            //ConsolePrinter.Print(obj.Name + ": " + obj.Type);

            if (!obj.IsValid || obj.Type != GameObjectType.MissileClient)
                return;

            if (testMenu["ShowMissileInfo"].As<MenuBool>().Value == false)
                return;


            var missile = (MissileClient) obj;

            if (!(missile.SpellCaster is Obj_AI_Hero))
                return;

            var testMissileSpeedStartTime = EvadeUtils.TickCount;
            var testMissileSpeedStartPos = missile.Position.To2D();

            DelayAction.Add(250, () =>
            {
                if (missile != null && missile.IsValid && !missile.IsDead)
                {
                    testMissileSpeedStartTime = EvadeUtils.TickCount;
                    testMissileSpeedStartPos = missile.Position.To2D();
                }
            });

            testMissile = missile;
            testMissileStartTime = EvadeUtils.TickCount;

            ConsolePrinter.Print("[" + missile.SpellData.Name + "]: Est.CastTime: " +
                                 (EvadeUtils.TickCount - lastHeroSpellCastTime));
            ConsolePrinter.Print("[" + missile.SpellData.Name + "]: Missile Name " + missile.SpellData.Name);
            ConsolePrinter.Print("[" + missile.SpellData.Name + "]: Missile Speed " + missile.SpellData.MissileSpeed);
            ConsolePrinter.Print("[" + missile.SpellData.Name + "]: Accel " + missile.SpellData.MissileAccel);
            ConsolePrinter.Print("[" + missile.SpellData.Name + "]: Max Speed " + missile.SpellData.MissileMaxSpeed);
            ConsolePrinter.Print("[" + missile.SpellData.Name + "]: LineWidth " + missile.SpellData.LineWidth);
            ConsolePrinter.Print("[" + missile.SpellData.Name + "]: Range " + missile.SpellData.CastRange);
            //ConsolePrinter.Print("Angle " + missile.SpellData.CastConeAngle);
            /*ConsolePrinter.Print("Offset: " + missile.SpellData.ParticleStartOffset);
            ConsolePrinter.Print("Missile Speed " + missile.SpellData.MissileSpeed);
            ConsolePrinter.Print("LineWidth " + missile.SpellData.LineWidth);
            circleRenderPos = missile.SpellData.ParticleStartOffset.To2D();*/

            //ConsolePrinter.Print("Acquired: " + (EvadeUtils.TickCount - lastSpellCastTime));

            RenderObjects.Add(
                new RenderCircle(missile.StartPosition.To2D(), 500));
            RenderObjects.Add(
                new RenderCircle(missile.EndPosition.To2D(), 500));

            DelayAction.Add(750, () =>
            {
                if (missile != null && missile.IsValid && !missile.IsDead)
                {
                    var dist = missile.Position.To2D().Distance(testMissileSpeedStartPos);
                    ConsolePrinter.Print("[" + missile.SpellData.Name + "]: Est.Missile speed: " +
                                         1000 * (dist / (EvadeUtils.TickCount - testMissileSpeedStartTime)));
                }
            });

            SpellData spellData;

            if (missile.SpellCaster != null && missile.SpellCaster.Team != myHero.Team &&
                missile.SpellData.Name != null &&
                SpellDetector.onMissileSpells.TryGetValue(missile.SpellData.Name, out spellData)
                && missile.StartPosition != null && missile.EndPosition != null)
                if (missile.StartPosition.Distance(myHero.Position) < spellData.range + 1000)
                {
                    var hero = missile.SpellCaster;

                    if (hero.IsVisible)
                        foreach (var entry in SpellDetector.spells)
                        {
                            var spell = entry.Value;

                            if (spell.info.missileName == missile.SpellData.Name
                                && spell.heroID == missile.SpellCaster.NetworkId)
                                if (spell.info.isThreeWay == false && spell.info.isSpecial == false)
                                    ConsolePrinter.Print("Acquired: " + (EvadeUtils.TickCount - spell.startTime));
                        }
                }
        }

        private void Game_ProcessSpell(Obj_AI_Base hero, Obj_AI_BaseMissileClientDataEventArgs args)
        {
            if (!(hero is Obj_AI_Hero))
                return;


            if (testMenu["(ShowProcessSpell)"].As<MenuBool>().Enabled)
            {
                ConsolePrinter.Print(
                    args.SpellData.Name + " CastTime: " + (hero.SpellBook.CastEndTime - Game.ClockTime));

                ConsolePrinter.Print("CastRadius: " + args.SpellData.CastRadius);

                /*foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(args.SData))
                {
                    string name = descriptor.Name;
                    object value = descriptor.As(args.SData);
                    ConsolePrinter.Print("{0}={1}", name, value);
                }*/
            }

            if (args.SpellData.Name == "YasuoQW")
            {
                RenderObjects.Add(
                    new RenderCircle(args.Start.To2D(), 500));
                RenderObjects.Add(
                    new RenderCircle(args.End.To2D(), 500));
            }

            //ConsolePrinter.Print(EvadeUtils.TickCount - lastProcessPacketTime);
            //circleRenderPos = args.SpellData.ParticleStartOffset.To2D();

            /*Draw.RenderObjects.Add(
                new Draw.RenderPosition(args.Start.To2D(), Evade.GetTickCount + 500));
            Draw.RenderObjects.Add(
                new Draw.RenderPosition(args.End.To2D(), Evade.GetTickCount + 500));*/

            /*float testTime;
            
            
            testTime = Evade.GetTickCount;
            for (int i = 0; i < 100000; i++)
            {
                var testVar = ObjectCache.myHeroCache.boundingRadius;
            }
            ConsolePrinter.Print("Test time1: " + (Evade.GetTickCount - testTime));

            testTime = Evade.GetTickCount;
            var cacheVar = ObjectCache.myHeroCache.boundingRadius;
            for (int i = 0; i < 100000; i++)
            {
                var testVar = cacheVar;
            }
            ConsolePrinter.Print("Test time1: " + (Evade.GetTickCount - testTime));*/

            // ConsolePrinter.Print("NetworkID: " + args.MissileNetworkId);

            lastHeroSpellCastTime = EvadeUtils.TickCount;

            foreach (var entry in SpellDetector.spells)
            {
                var spell = entry.Value;

                if (spell.info.spellName == args.SpellData.Name
                    && spell.heroID == hero.NetworkId)
                    if (spell.info.isThreeWay == false && spell.info.isSpecial == false)
                        ConsolePrinter.Print("Time diff: " + (EvadeUtils.TickCount - spell.startTime));
            }

            if (hero.IsMe)
                lastSpellCastTime = EvadeUtils.TickCount;
        }

        private void CompareSpellLocation(Spell spell, Vector2 pos, float time)
        {
            var pos2 = spell.currentSpellPosition;
            if (spell.spellObject != null)
                ConsolePrinter.Print("Compare: " + pos2.Distance(pos) / (EvadeUtils.TickCount - time));
        }

        private void CompareSpellLocation2(Spell spell)
        {
            var pos1 = spell.currentSpellPosition;
            var timeNow = EvadeUtils.TickCount;

            if (spell.spellObject != null)
                ConsolePrinter.Print("start distance: " + spell.startPos.Distance(pos1));

            DelayAction.Add(250, () => CompareSpellLocation(spell, pos1, timeNow));
        }

        private void Game_OnGameUpdate()
        {
            if (startWalkTime > 0)
                if (EvadeUtils.TickCount - startWalkTime > 500 && myHero.HasPath == false)
                    startWalkTime = 0;

            if (testMenu["(ShowWindupTime)"].As<MenuBool>().Enabled)
                if (myHero.HasPath && lastStopingTime > 0)
                {
                    ConsolePrinter.Print("WindupTime: " + (EvadeUtils.TickCount - lastStopingTime));
                    lastStopingTime = 0;
                }
                else if (!myHero.HasPath && lastStopingTime == 0)
                {
                    lastStopingTime = EvadeUtils.TickCount;
                }

            if (testMenu["(ShowDashInfo)"].As<MenuBool>().Enabled)
                if (myHero.IsDashing())
                {
                    var dashInfo = myHero.GetDashInfo();
                    ConsolePrinter.Print("Dash Speed: " + dashInfo.Speed + " Dash dist: " +
                                         dashInfo.EndPos.Distance(dashInfo.StartPos));
                }
        }

        //private void Game_OnGameNotifyEvent(GameNotifyEventArgs args)
        //{
        //    //ConsolePrinter.Print("" + args.EventId);
        //}

        //private void GameObject_OnFloatPropertyChange(GameObject obj, GameObjectFloatPropertyChangeEventArgs args)
        //{
        //    //ConsolePrinter.Print(obj.Name);

        //    /*foreach (var sth in ObjectManager.Get<Obj_AI_Base>())
        //    {
        //        ConsolePrinter.Print(sth.Name);

        //    }*/

        //    if (testMenu["(TestSpellEndTime)"].As<MenuBool>().Value == false)
        //    {
        //        return;
        //    }

        //    if (obj.Name == "RobotBuddy")
        //    {
        //        //Draw.RenderObjects.Add(new Draw.RenderPosition(obj.Position.To2D(), EvadeUtils.TickCount + 10));
        //    }

        //    //ConsolePrinter.Print(obj.Name);


        //    if (args.Property == "mHP" && args.OldValue > args.NewValue)
        //    {
        //        //ConsolePrinter.Print("Damage taken time: " + (EvadeUtils.TickCount - lastSpellCastTime));
        //    }

        //    if (!obj.IsMe)
        //    {
        //        return;
        //    }


        //    if (args.Property != "mExp" && args.Property != "mGold" && args.Property != "mGoldTotal"
        //        && args.Property != "mMP" && args.Property != "mPARRegenRate")
        //    {
        //        //ConsolePrinter.Print(args.Property + ": " + args.NewValue);
        //    }
        //}

        private void Game_OnDamage(AttackableUnit sender, AttackableUnitDamageEventArgs args)
        {
            if (testMenu["(TestSpellEndTime)"].As<MenuBool>().Value == false)
                return;

            if (!sender.IsMe)
                return;

            ConsolePrinter.Print("Damage taken time: " + (EvadeUtils.TickCount - lastSpellCastTime));
        }

        //private void GameObject_OnIntegerPropertyChange(GameObject obj, GameObjectIntegerPropertyChangeEventArgs args)
        //{
        //    if (obj.IsMe)
        //    {
        //        if (args.Property != "mExp" && args.Property != "mGold" && args.Property != "mGoldTotal")
        //        {
        //            ConsolePrinter.Print("Int" + args.Property + ": " + args.NewValue);
        //        }

        //    }
        //}

        private void Game_OnIssueOrder(Obj_AI_Base hero, Obj_AI_BaseIssueOrderEventArgs args)
        {
            if (!hero.IsMe)
                return;

            if (args.OrderType == OrderType.HoldPosition)
            {
                var path = myHero.Path;
                var heroPoint = ObjectCache.myHeroCache.serverPos2D;


                if (path.Length > 0)
                {
                    var movePos = path[path.Length - 1].To2D();
                    var walkDir = (movePos - heroPoint).Normalized();

                    //circleRenderPos = EvadeHelper.GetRealHeroPos();
                    //heroPoint;// +walkDir * ObjectCache.myHeroCache.moveSpeed * (((float)ObjectCache.gamePing) / 1000);
                }
            }

            if (testMenu["(TestPath)"].As<MenuBool>().Enabled)
            {
                var tPath = myHero.GetPath(args.Target.Position);
                var lastPoint = Vector2.Zero;

                foreach (var point in tPath)
                {
                    var point2D = point.To2D();
                    RenderObjects.Add(new RenderCircle(point2D, 500));
                    //Render.Circle.DrawCircle(new Vector3(point.X, point.Y, point.Z), ObjectCache.myHeroCache.boundingRadius, Color.Violet, 3);
                }
            }

            /*
            if (args.OrderType == OrderType.MoveTo)
            {         
                if (testingCollision)
                {
                    if (args.TargetPosition.To2D().Distance(testCollisionPos) < 3)
                    {
                        //var path = myHero.GetPath();
                        //circleRenderPos

                        args.Process = false;
                    }
                }
            }*/

            if (args.OrderType == OrderType.MoveTo)
            {
                if (testMenu["(EvadeTesterPing)"].As<MenuBool>().Enabled)
                    ConsolePrinter.Print("Sending Path ClickTime: " + (EvadeUtils.TickCount - lastRightMouseClickTime));

                var heroPos = ObjectCache.myHeroCache.serverPos2D;
                var pos = args.Target.Position.To2D();
                var speed = ObjectCache.myHeroCache.moveSpeed;

                startWalkPos = heroPos;
                startWalkTime = EvadeUtils.TickCount;

                foreach (var entry in SpellDetector.spells)
                {
                    var spell = entry.Value;
                    var spellPos = spell.currentSpellPosition;
                    var walkDir = (pos - heroPos).Normalized();


                    var spellTime = EvadeUtils.TickCount - spell.startTime - spell.info.spellDelay;
                    spellPos = spell.startPos + spell.direction * spell.info.projectileSpeed * (spellTime / 1000);
                    //ConsolePrinter.Print("aaaa" + spellTime);


                    var isCollision = false;
                    var movingCollisionTime = MathUtils.GetCollisionTime(heroPos, spellPos, walkDir * (speed - 25),
                        spell.direction * (spell.info.projectileSpeed - 200), ObjectCache.myHeroCache.boundingRadius,
                        spell.radius, out isCollision);
                    if (isCollision)
                        if (true) //spellPos.Distance(spell.endPos) / spell.info.projectileSpeed > movingCollisionTime)
                            ConsolePrinter.Print("movingCollisionTime: " + movingCollisionTime);
                }
            }
        }

        private void GetPath(Vector2 movePos)
        {
        }

        private void PrintTimers()
        {
            Render.Text(10, 10, Color.White, "Timer1 Freq: " + (getGameTimer - lastGameTimerTick));
            Render.Text(10, 30, Color.White, "Timer2 Freq: " + (getTickCountTimer - lastTickCountTimerTick));
            Render.Text(10, 50, Color.White,
                "Timer3 Freq: " + (getWatchTimer - lastWatchTimerTick)); //(getWatchTimer - lastWatchTimerTick));

            if (getTickCountTimer - lastTimerCheck > 1000)
            {
                ConsolePrinter.Print("" + (getGameTimer - lastGameTimerStart -
                                           (getTickCountTimer - lastTickCountTimerStart)));
                lastTimerCheck = getTickCountTimer;
            }


            Render.Text(10, 70, Color.White, "Timer1 Freq: " + (getGameTimer - lastGameTimerStart));
            Render.Text(10, 90, Color.White, "Timer2 Freq: " + (getTickCountTimer - lastTickCountTimerStart));
            Render.Text(10, 110, Color.White, "Timer3 Freq: " + (getWatchTimer - lastWatchTimerStart));

            /*Render.Text(10, 70, Color.White, "Timer1 Freq: " + (getGameTimer));
            Render.Text(10, 90, Color.White, "Timer2 Freq: " + (getTickCountTimer));
            Render.Text(10, 100, Color.White, "Timer3 Freq: " + (getWatchTimer));*/


            lastGameTimerTick = getGameTimer;
            lastTickCountTimerTick = getTickCountTimer;
            lastWatchTimerTick = getWatchTimer;
        }

        private void TestUnderTurret()
        {
            if (Game.CursorPos.To2D().IsUnderTurret())
                Render.Circle(Game.CursorPos, 50, 50, Color.Red);
            else
                Render.Circle(Game.CursorPos, 50, 50, Color.White);
        }

        private void Render_OnPresent()
        {
            //PrintTimers();

            //EvadeHelper.CheckMovePath(Game.CursorPos.To2D());            

            //TestUnderTurret();


            /*if (EvadeHelper.CheckPathCollision(myHero, Game.CursorPos.To2D()))
            {                
                var paths = myHero.GetPath(ObjectCache.myHeroCache.serverPos2DExtra.To3D(), Game.CursorPos);
                foreach (var path in paths)
                {
                    Render.Circle.DrawCircle(path, ObjectCache.myHeroCache.boundingRadius, Color.Red, 3);
                }
            }
            else
            {
                Render.Circle.DrawCircle(Game.CursorPos, ObjectCache.myHeroCache.boundingRadius, Color.White, 3);
            }*/

            foreach (var entry in SpellDetector.drawSpells)
            {
                var spell = entry.Value;

                if (spell.spellType == SpellType.Line)
                {
                    var spellPos = spell.currentSpellPosition;

                    Render.Circle(new Vector3(spellPos.X, spellPos.Y, myHero.Position.Z), spell.info.radius, 50,
                        Color.White);

                    /*spellPos = spellPos + spell.Orientation * spell.info.projectileSpeed * (60 / 1000); //move the spellPos by 50 miliseconds forwards
                    spellPos = spellPos + spell.Orientation * 200; //move the spellPos by 50 units forwards        

                    Render.Circle.DrawCircle(new Vector3(spellPos.X, spellPos.Y, myHero.Position.Z), spell.info.radius, Color.White, 3);*/
                }
            }

            if (testMenu["(TestHeroPos)"].As<MenuBool>().Enabled)
            {
                var path = myHero.Path;
                if (path.Length > 0)
                {
                    var heroPos2 =
                        EvadeHelper.GetRealHeroPos(ObjectCache.gamePing + 50); // path[path.Length - 1].To2D();
                    var heroPos1 = ObjectCache.myHeroCache.serverPos2D;

                    Render.Circle(new Vector3(heroPos2.X, heroPos2.Y, myHero.ServerPosition.Z),
                        ObjectCache.myHeroCache.boundingRadius, 50, Color.Red);
                    Render.Circle(
                        new Vector3(myHero.ServerPosition.X, myHero.ServerPosition.Y, myHero.ServerPosition.Z),
                        ObjectCache.myHeroCache.boundingRadius, 50, Color.White);

                    Vector2 heroPos;
                    Render.WorldToScreen(ObjectManager.GetLocalPlayer().Position, out heroPos);
                    //var dimension = Render.GetTextExtent("Evade: ON");
                    Render.Text(heroPos.X - 10, heroPos.Y, Color.Red, "" + (int) heroPos2.Distance(heroPos1));

                    Render.Circle(new Vector3(circleRenderPos.X, circleRenderPos.Y, myHero.ServerPosition.Z), 10, 50,
                        Color.Red);
                }
            }

            if (testMenu["(DrawHeroPos)"].As<MenuBool>().Enabled)
                Render.Circle(new Vector3(myHero.ServerPosition.X, myHero.ServerPosition.Y, myHero.ServerPosition.Z),
                    ObjectCache.myHeroCache.boundingRadius, 50, Color.White);

            if (testMenu["(TestMoveTo)"].As<MenuKeyBind>().Enabled)
            {
                // fix ? wtf
                var MenuKeyBind = testMenu["(TestMoveTo)"].As<MenuKeyBind>();
                testMenu["(TestMoveTo)"].As<MenuKeyBind>().Value = false;

                /*lastRightMouseClickTime = EvadeUtils.TickCount;
                myHero.IssueOrder(OrderType.MoveTo, Game.CursorPos,false);*/

                myHero.IssueOrder(OrderType.MoveTo, Game.CursorPos);

                var dir = (Game.CursorPos - myHero.Position).Normalized();
                //var pos2 = myHero.Position - dir * Game.CursorPos.Distance(myHero.Position);

                //var pos2 = myHero.Position.To2D() - dir.To2D() * 75;
                var pos2 = Game.CursorPos.To2D() - dir.To2D() * 75;

                //Console.WriteLine(myHero.BBox.Maximum.Distance(myHero.Position));

                DelayAction.Add(20, () => myHero.IssueOrder(OrderType.MoveTo, pos2.To3D()));
                //myHero.IssueOrder(OrderType.MoveTo, pos2, false);
            }

            if (testMenu["(TestPath)"].As<MenuBool>().Enabled)
            {
                var tPath = myHero.GetPath(Game.CursorPos);
                var lastPoint = Vector2.Zero;

                foreach (var point in tPath)
                {
                    var point2D = point.To2D();
                    Render.Circle(new Vector3(point.X, point.Y, point.Z), ObjectCache.myHeroCache.boundingRadius, 50,
                        Color.Violet);

                    lastPoint = point2D;
                }
            }

            if (testMenu["(TestPath)"].As<MenuBool>().Enabled)
            {
                var tPath = myHero.GetPath(Game.CursorPos);
                var lastPoint = Vector2.Zero;

                foreach (var point in tPath)
                {
                    var point2D = point.To2D();
                    //Render.Circle.DrawCircle(new Vector3(point.X, point.Y, point.Z), ObjectCache.myHeroCache.boundingRadius, Color.Violet, 3);

                    lastPoint = point2D;
                }

                foreach (var entry in SpellDetector.spells)
                {
                    var spell = entry.Value;

                    var to = Game.CursorPos.To2D();
                    var dir = (to - myHero.Position.To2D()).Normalized();
                    Vector2 cPos1, cPos2;

                    var cpa = MathUtilsCPA.CPAPointsEx(myHero.Position.To2D(), dir * ObjectCache.myHeroCache.moveSpeed,
                        spell.endPos, spell.direction * spell.info.projectileSpeed, to, spell.endPos);
                    var cpaTime = MathUtilsCPA.CPATime(myHero.Position.To2D(), dir * ObjectCache.myHeroCache.moveSpeed,
                        spell.endPos, spell.direction * spell.info.projectileSpeed);

                    //ConsolePrinter.Print("" + cpaTime);
                    //Render.Circle.DrawCircle(cPos1.To3D(), ObjectCache.myHeroCache.boundingRadius, Color.Red, 3);

                    if (cpa < ObjectCache.myHeroCache.boundingRadius + spell.radius)
                    {
                    }
                }
            }

            if (testMenu["(ShowBuffs)"].As<MenuBool>().Enabled)
            {
                var target = myHero;

                foreach (var hero in GameObjects.EnemyHeroes)
                    target = hero;

                var buffs = target.Buffs;

                //ConsolePrinter.Print(myHero.ChampionName);

                //if(myHero.IsDead)
                //    ConsolePrinter.Print("dead");

                if (!target.IsTargetable)
                    ConsolePrinter.Print("invul" + EvadeUtils.TickCount);

                var height = 20;

                foreach (var buff in buffs)
                    if (buff.IsValid)
                    {
                        Render.Text(10, height, Color.White, buff.Name);
                        height += 20;

                        ConsolePrinter.Print(buff.Name);
                    }
            }

            if (testMenu["(TestTracker)"].As<MenuBool>().Enabled)
                foreach (var entry in ObjectTracker.objTracker)
                {
                    var info = entry.Value;

                    Vector3 endPos2;
                    if (info.usePosition == false)
                        endPos2 = info.obj.Position;
                    else
                        endPos2 = info.position;

                    Render.Circle(new Vector3(endPos2.X, endPos2.Y, myHero.Position.Z), 50, 50, Color.Green);
                }

            if (testMenu["(ShowMissileInfo)"].As<MenuBool>().Enabled)
                if (testMissile != null)
                {
                    //Render.Circle.DrawCircle(testMissile.Position, testMissile.BoundingRadius, Color.White, 3);
                }

            if (testMenu["(TestWall)"].As<MenuBool>().Enabled)
            {
                /*foreach (var posInfo in sortedBestPos)
                {
                    var posOnScreen = Drawing.WorldToScreen(posInfo.position.To3D());
                    //Render.Text(posOnScreen.X, posOnScreen.Y, Color.Aqua, "" + (int)posInfo.closestDistance);

                    
                    if (!posInfo.rejectPosition)
                    {
                        Render.Text(posOnScreen.X, posOnScreen.Y, Color.Aqua, "" + (int)posInfo.closestDistance);
                    }

                    Render.Text(posOnScreen.X, posOnScreen.Y, Color.Aqua, "" + (int)posInfo.closestDistance);

                    if (posInfo.posDangerCount <= 0)
                    {
                        var pos = posInfo.position;
                        Render.Circle.DrawCircle(new Vector3(pos.X, pos.Y, myHero.Position.Z), (float)25, Color.White, 3);
                    }                                      
                }*/

                var posChecked = 0;
                var maxPosToCheck = 50;
                var posRadius = 50;
                var radiusIndex = 0;

                var heroPoint = ObjectCache.myHeroCache.serverPos2D;
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

                        if (!EvadeHelper.CheckPathCollision(myHero, pos))
                            Render.Circle(new Vector3(pos.X, pos.Y, myHero.Position.Z), 25, 50, Color.White);
                    }
                }
            }
        }
    }
}