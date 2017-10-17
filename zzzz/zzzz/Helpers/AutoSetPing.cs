using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Aimtec;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Menu;
using Aimtec.SDK.Menu.Components;
using zzzz.Draw;
//using SharpDX;

//using SharpDX;

namespace zzzz
{
    internal class AutoSetPing
    {
        private static float sumExtraDelayTime;
        private static float avgExtraDelayTime;
        private static float numExtraDelayTime;

        private static float maxExtraDelayTime;

        private static Obj_AI_BaseIssueOrderEventArgs lastIssueOrderArgs;
        private static Vector2 lastMoveToServerPos;
        private static Vector2 lastPathEndPos;

        private static SpellBookCastSpellEventArgs lastSpellCastArgs;
        private static Vector2 lastSpellCastServerPos;
        private static Vector2 lastSpellCastEndPos;

        private static float testSkillshotDelayStart;
        private static bool testSkillshotDelayOn;

        private static bool checkPing = true;

        private static readonly List<float> pingList = new List<float>();

        public static Menu menu;

        public AutoSetPing(Menu mainMenu)
        {
            Obj_AI_Base.OnNewPath += Hero_OnNewPath;
            Obj_AI_Base.OnIssueOrder += Hero_OnIssueOrder;

            SpellBook.OnCastSpell += Game_OnCastSpell;
            GameObject.OnCreate += Game_OnCreateObj;
            Obj_AI_Base.OnProcessSpellCast += Game_ProcessSpell;

            //Game.OnUpdate += Game_OnUpdate;

            //Render.OnPresent += Game_OnDraw;

            Evade.autoSetPingMenu = new Menu("AutoSetPing", "AutoSetPingMenu");
            Evade.autoSetPingMenu.Add(new MenuBool("AutoSetPingOn", "Auto Set Ping"));
            Evade.autoSetPingMenu.Add(new MenuSlider("AutoSetPercentile", "Auto Set Percentile", 75, 0, 100));

            //autoSetPingMenu.Add(new MenuComponent("TestSkillshotDelay", "TestSkillshotDelay").SetValue<bool>(false));

            mainMenu.Add(Evade.autoSetPingMenu);

            menu = mainMenu;
        }

        public static Obj_AI_Hero myHero => ObjectManager.GetLocalPlayer();

        private void Game_ProcessSpell(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs e)
        {
            if (!sender.IsMe)
                return;

            //lastSpellCastServerPos = myHero.Position.To2D();
        }

        private void Game_OnDraw(EventArgs args)
        {
            Render.Circle(myHero.Position, 10, 5, Color.Red);
            Render.Circle(myHero.ServerPosition, 10, 5, Color.Red);
        }

        private void Game_OnCastSpell(Obj_AI_Base sender, SpellBookCastSpellEventArgs e)
        {
            checkPing = false;

            if (!sender.IsMe)
                return;

            lastSpellCastArgs = e;


            if (myHero.HasPath && myHero.Path.Count() > 0)
            {
                lastSpellCastServerPos = EvadeUtils.GetGamePosition(myHero, Game.Ping);
                lastSpellCastEndPos = myHero.Path.Last().To2D();
                checkPing = true;

                RenderObjects.Add(new RenderCircle(lastSpellCastServerPos, 1000, Color.Green, 10));
            }
        }

        private void Game_OnCreateObj(GameObject sender)
        {
            var missile = sender as MissileClient;
            if (missile != null && missile.SpellCaster.IsMe)
                if (lastSpellCastArgs.Process)
                {
                    //Draw.RenderObjects.Add(new Draw.RenderPosition(lastSpellCastServerPos, 1000, System.Drawing.Color.Red, 10));
                    RenderObjects.Add(new RenderCircle(missile.StartPosition.To2D(), 1000, Color.Red, 10));

                    var distance = lastSpellCastServerPos.Distance(missile.StartPosition.To2D());
                    var moveTime = 1000 * distance / myHero.MoveSpeed;
                    Console.WriteLine("Extra Delay: " + moveTime);
                }
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (ObjectCache.menuCache.cache["TestSkillshotDelay"].As<MenuBool>().Enabled)
            {
                testSkillshotDelayStart = EvadeUtils.TickCount;
                testSkillshotDelayOn = true;
                ObjectCache.menuCache.cache["TestSkillshotDelay"].As<MenuBool>().Value = false;
            }

            if (testSkillshotDelayOn && SpellDetector.spells.Count() > 0)
            {
                Console.WriteLine("Delay: " + (EvadeUtils.TickCount - testSkillshotDelayStart));
                testSkillshotDelayOn = false;
            }
        }

        private void Hero_OnIssueOrder(Obj_AI_Base hero, Obj_AI_BaseIssueOrderEventArgs args)
        {
            checkPing = false;

            var distance = myHero.Position.To2D().Distance(myHero.ServerPosition.To2D());
            var moveTime = 1000 * distance / myHero.MoveSpeed;
            //Console.WriteLine("Extra Delay: " + moveTime);

            if (ObjectCache.menuCache.cache["AutoSetPingOn"].As<MenuBool>().Value == false)
                return;

            if (!hero.IsMe)
                return;

            lastIssueOrderArgs = args;

            if (args.OrderType == OrderType.MoveTo)
                if (myHero.HasPath && myHero.Path.Count() > 0)
                {
                    lastMoveToServerPos = myHero.ServerPosition.To2D();
                    lastPathEndPos = myHero.Path.Last().To2D();
                    checkPing = true;
                }
        }

        private void Hero_OnNewPath(Obj_AI_Base hero, Obj_AI_BaseNewPathEventArgs args)
        {
            if (ObjectCache.menuCache.cache["AutoSetPingOn"].As<MenuBool>().Value == false)
                return;

            if (!hero.IsMe)
                return;

            var path = args.Path;

            if (path.Length > 1 && !args.IsDash)
            {
                var movePos = path.Last().To2D();

                if (checkPing
                    && lastIssueOrderArgs.ProcessEvent
                    && lastIssueOrderArgs.OrderType == OrderType.MoveTo
                    && lastIssueOrderArgs.Target.Position.To2D().Distance(movePos) < 3
                    && myHero.Path.Count() == 1
                    && args.Path.Count() == 2
                    && myHero.HasPath)
                {
                    //Draw.RenderObjects.Add(new Draw.RenderPosition(myHero.Path.Last().To2D(), 1000));

                    RenderObjects.Add(new RenderLine(args.Path.First().To2D(), args.Path.Last().To2D(), 1000));
                    RenderObjects.Add(new RenderLine(myHero.Position.To2D(), myHero.Path.Last().To2D(), 1000));

                    //Draw.RenderObjects.Add(new Draw.RenderCircle(lastMoveToServerPos, 1000, System.Drawing.Color.Red, 10));

                    var distanceTillEnd = myHero.Path.Last().To2D().Distance(myHero.Position.To2D());
                    var moveTimeTillEnd = 1000 * distanceTillEnd / myHero.MoveSpeed;

                    if (moveTimeTillEnd < 500)
                        return;

                    /*SharpDX.*/
                    var myHeroPosition = new /*SharpDX.*/
                        Vector3(myHero.Position.X, myHero.Position.Y, myHero.Position.Z);

                    var dir1 = (myHero.Path.Last().To2D() - myHero.Position.To2D()).Normalized();

                    //SharpDX.Vector2 dir1sharpdx = new SharpDX.Vector2(myHero.Position.X, myHero.Position.Y);

                    //var ray1 = new Ray(myHeroPosition.SetZ(0), new SharpDX.Vector3(dir1.X, dir1.Y, 0));
                    var ray1startpos = new Vector3(myHeroPosition.X, myHeroPosition.Y, 0);
                    var ray1dir = new Vector3(dir1.X, dir1.Y, 0);
                    //Vector3 ray1 = ray1startpos.ExtendDir(ray1dir, args.Path.Length);

                    var dir2 = (args.Path.First().To2D() - args.Path.Last().To2D()).Normalized();
                    //var pos2 = new Vector3(args.Path.First().X, args.Path.First().Y, 0);

                    /*SharpDX.*/
                    var argsPathFirst = new /*SharpDX.*/
                        Vector3(args.Path.First().X, args.Path.First().Y, args.Path.First().Z);

                    //var ray2 = new Ray(argsPathFirst.SetZ(0), new SharpDX.Vector3(dir2.X, dir2.Y, 0));
                    var ray2startpos = new Vector3(argsPathFirst.X, argsPathFirst.Y, 0);
                    var ray2dir = new Vector3(dir2.X, dir2.Y, 0);

                    //Vector3 ray2 = ray2startpos.ExtendDir(ray2dir, args.Path.Length);

                    /*SharpDX.*/ //Vector3 intersection3;

                    var intersection = ray2startpos.To2D()
                        .Intersection(ray2startpos.To2D().ExtendDir(ray2dir.To2D(), args.Path.Length),
                            ray1startpos.To2D(), ray1startpos.To2D().ExtendDir(ray1dir.To2D(), args.Path.Length));


                    if (intersection.Intersects)
                    {
                        var intersection3 = intersection.Point.To3D();

                        var x = intersection3.To2D().X;
                        var y = intersection3.To2D().Y;

                        var intersectionAT = new Vector2(x, y);

                        var projection = intersectionAT.ProjectOn(myHero.Path.Last().To2D(), myHero.Position.To2D());

                        if (projection.IsOnSegment && dir1.AngleBetween(dir2) > 20 && dir1.AngleBetween(dir2) < 160)
                        {
                            RenderObjects.Add(new RenderCircle(intersectionAT, 1000, Color.Red, 10, 5));

                            var distance = //args.Path.First().To2D().Distance(intersection);
                                lastMoveToServerPos.Distance(intersectionAT);
                            var moveTime = 1000 * distance / myHero.MoveSpeed;

                            //Console.WriteLine("waa: " + distance);

                            if (moveTime < 1000)
                            {
                                if (numExtraDelayTime > 0)
                                {
                                    sumExtraDelayTime += moveTime;
                                    avgExtraDelayTime = sumExtraDelayTime / numExtraDelayTime;

                                    pingList.Add(moveTime);
                                }
                                numExtraDelayTime += 1;

                                if (maxExtraDelayTime == 0)
                                    maxExtraDelayTime = ObjectCache.menuCache.cache["ExtraPingBuffer"].As<MenuSlider>()
                                        .Value;

                                if (numExtraDelayTime % 100 == 0)
                                {
                                    pingList.Sort();

                                    var percentile = ObjectCache.menuCache.cache["AutoSetPercentile"].As<MenuSlider>()
                                        .Value;
                                    var percentIndex = (int) Math.Floor(pingList.Count() * (percentile / 100f)) - 1;
                                    maxExtraDelayTime = Math.Max(pingList.ElementAt(percentIndex) - Game.Ping, 0);
                                    ObjectCache.menuCache.cache["ExtraPingBuffer"].As<MenuSlider>().Value =
                                        (int) maxExtraDelayTime; //(new MenuSlider((int)maxExtraDelayTime, 0, 200));

                                    pingList.Clear();

                                    Console.WriteLine("Max Extra Delay: " + maxExtraDelayTime);
                                }

                                Console.WriteLine("Extra Delay: " + Math.Max(moveTime - Game.Ping, 0));
                            }
                        }
                    }

                    //if (ray2.Intersects(ref ray1, out intersection3))
                    //{
                    //    //var intersection = intersection3.To2D();

                    //    float x = intersection3.To2D().X;
                    //    float y = intersection3.To2D().Y;

                    //    Vector2 intersectionAT = new Vector2(x, y);

                    //    var projection = intersectionAT.ProjectOn(myHero.Path.Last().To2D(), myHero.Position.To2D());

                    //    if (projection.IsOnSegment && dir1.AngleBetween(dir2) > 20 && dir1.AngleBetween(dir2) < 160)
                    //    {
                    //        Draw.RenderObjects.Add(new Draw.RenderCircle(intersectionAT, 1000, System.Drawing.Color.Red, 10, 5));

                    //        var distance = //args.Path.First().To2D().Distance(intersection);
                    //            lastMoveToServerPos.Distance(intersectionAT);
                    //        float moveTime = 1000 * distance / myHero.MoveSpeed;

                    //        //Console.WriteLine("waa: " + distance);

                    //        if (moveTime < 1000)
                    //        {
                    //            if (numExtraDelayTime > 0)
                    //            {
                    //                sumExtraDelayTime += moveTime;
                    //                avgExtraDelayTime = sumExtraDelayTime / numExtraDelayTime;

                    //                pingList.Add(moveTime);
                    //            }
                    //            numExtraDelayTime += 1;

                    //            if (maxExtraDelayTime == 0)
                    //            {
                    //                maxExtraDelayTime = ObjectCache.menuCache.cache["ExtraPingBuffer"].As<MenuSlider>().Value;
                    //            }

                    //            if (numExtraDelayTime % 100 == 0)
                    //            {
                    //                pingList.Sort();

                    //                var percentile = ObjectCache.menuCache.cache["AutoSetPercentile"].As<MenuSlider>().Value;
                    //                int percentIndex = (int)Math.Floor(pingList.Count() * (percentile / 100f)) - 1;
                    //                maxExtraDelayTime = Math.Max(pingList.ElementAt(percentIndex) - Game.Ping, 0);
                    //                ObjectCache.menuCache.cache["ExtraPingBuffer"].As<MenuSlider>().Value =
                    //                    (int)maxExtraDelayTime; //(new MenuSlider((int)maxExtraDelayTime, 0, 200));

                    //                pingList.Clear();

                    //                Console.WriteLine("Max Extra Delay: " + maxExtraDelayTime);
                    //            }

                    //            Console.WriteLine("Extra Delay: " + Math.Max(moveTime - Game.Ping, 0));
                    //        }
                    //    }
                    //}
                }

                checkPing = false;
            }
        }
    }
}