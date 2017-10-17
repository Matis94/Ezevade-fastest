using System;
using Aimtec;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Menu;
using Aimtec.SDK.Menu.Components;

//using SharpDX;

namespace zzzz
{
    internal class PingTester
    {
        public static Menu testMenu;

        private static float lastTimerCheck = 0;
        private static bool lastRandomMoveCoeff;

        private static float sumPingTime;
        private static float averagePingTime = ObjectCache.gamePing;
        private static int testCount;
        private static int autoTestCount;
        private static float maxPingTime = ObjectCache.gamePing;

        private static bool autoTestPing;

        private static EvadeCommand lastTestMoveToCommand;

        public PingTester()
        {
            Game.OnUpdate += Game_OnGameUpdate;

            testMenu = new Menu("PingTest", "Ping Tester", true);
            testMenu.Add(new MenuBool("AutoSetPing", "Auto Set Ping"));
            testMenu.Add(new MenuBool("TestMoveTime", "Test Ping"));
            testMenu.Add(new MenuBool("SetMaxPing", "Set Max Ping"));
            testMenu.Add(new MenuBool("SetAvgPing", "Set Avg Ping"));
            testMenu.Add(new MenuBool("Test20MoveTime", "Test Ping x20"));
            testMenu.Add(new MenuBool("PrintResults", "Print Results"));
            testMenu.Attach();
        }

        private static Obj_AI_Hero myHero => ObjectManager.GetLocalPlayer();

        private void IssueTestMove(int recursionCount)
        {
            var movePos = ObjectCache.myHeroCache.serverPos2D;

            var rand = new Random();

            lastRandomMoveCoeff = !lastRandomMoveCoeff;
            if (lastRandomMoveCoeff)
                movePos.X += 65 + rand.Next(0, 20);
            else
                movePos.X -= 65 + rand.Next(0, 20);

            lastTestMoveToCommand = new EvadeCommand
            {
                order = EvadeOrderCommand.MoveTo,
                targetPosition = movePos,
                timestamp = EvadeUtils.TickCount,
                isProcessed = false
            };
            myHero.IssueOrder(OrderType.MoveTo, movePos.To3D());

            if (recursionCount > 1)
                DelayAction.Add(500, () => IssueTestMove(recursionCount - 1));
        }

        private void SetPing(int ping)
        {
            ObjectCache.menuCache.cache["ExtraPingBuffer"].As<MenuSlider>().Value = ping;
        }

        private void Game_OnGameUpdate()
        {
            if (testMenu["AutoSetPing"].As<MenuBool>().Enabled)
            {
                Console.WriteLine("Testing Ping...Please wait 10 seconds");

                var testAmount = 20;

                testMenu["AutoSetPing"].As<MenuBool>().Value = false;
                IssueTestMove(testAmount);
                autoTestCount = testCount + testAmount;
                autoTestPing = true;
            }

            if (testMenu["PrintResults"].As<MenuBool>().Enabled)
            {
                testMenu["PrintResults"].As<MenuBool>().Value = false;

                Console.WriteLine("Average Extra Delay: " + averagePingTime);
                Console.WriteLine("Max Extra Delay: " + maxPingTime);
            }

            if (autoTestPing && testCount >= autoTestCount)
            {
                Console.WriteLine("Auto Set Ping Complete");

                Console.WriteLine("Average Extra Delay: " + averagePingTime);
                Console.WriteLine("Max Extra Delay: " + maxPingTime);

                SetPing((int) (averagePingTime + 10));
                Console.WriteLine("Set Average extra ping + 10: " + (averagePingTime + 10));

                autoTestPing = false;
            }

            if (testMenu["TestMoveTime"].As<MenuBool>().Enabled)
            {
                testMenu["TestMoveTime"].As<MenuBool>().Value = false;
                IssueTestMove(1);
            }


            if (testMenu["Test20MoveTime"].As<MenuBool>().Enabled)
            {
                testMenu["Test20MoveTime"].As<MenuBool>().Value = false;
                IssueTestMove(20);
            }

            if (testMenu["SetMaxPing"].As<MenuBool>().Enabled)
            {
                testMenu["SetMaxPing"].As<MenuBool>().Value = false;

                if (testCount < 10)
                {
                    Console.WriteLine("Please test 10 times before setting ping");
                }
                else
                {
                    Console.WriteLine("Set Max extra ping: " + maxPingTime);
                    SetPing((int) maxPingTime);
                }
            }

            if (testMenu["SetAvgPing"].As<MenuBool>().Enabled)
            {
                testMenu["SetAvgPing"].As<MenuBool>().Value = false;

                if (testCount < 10)
                {
                    Console.WriteLine("Please test 10 times before setting ping");
                }
                else
                {
                    Console.WriteLine("Set Average extra ping: " + averagePingTime);
                    SetPing((int) averagePingTime);
                }
            }

            if (myHero.HasPath)
                if (lastTestMoveToCommand != null && lastTestMoveToCommand.isProcessed == false &&
                    lastTestMoveToCommand.order == EvadeOrderCommand.MoveTo)
                {
                    var path = myHero.Path;

                    if (path.Length > 0)
                    {
                        var movePos = path[path.Length - 1].To2D();

                        if (movePos.Distance(lastTestMoveToCommand.targetPosition) < 10)
                        {
                            var moveTime = EvadeUtils.TickCount - lastTestMoveToCommand.timestamp -
                                           ObjectCache.gamePing;
                            Console.WriteLine("Extra Delay: " + moveTime);
                            lastTestMoveToCommand.isProcessed = true;

                            sumPingTime += moveTime;
                            testCount += 1;
                            averagePingTime = sumPingTime / testCount;
                            maxPingTime = Math.Max(maxPingTime, moveTime);
                        }
                    }
                }
        }
    }
}