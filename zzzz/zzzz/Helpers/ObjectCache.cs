using System;
using System.Collections.Generic;
using Aimtec;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Menu;
using Aimtec.SDK.Menu.Components;

//using SharpDX;

namespace zzzz
{
    public class HeroInfo
    {
        public float boundingRadius;
        public Vector2 currentPosition;
        public bool HasPath;
        public Obj_AI_Hero hero;
        public float moveSpeed;
        public Vector2 serverPos2D;
        public Vector2 serverPos2DExtra;
        public Vector2 serverPos2DPing;

        public HeroInfo(Obj_AI_Hero hero)
        {
            this.hero = hero;
            Game.OnUpdate += Game_OnGameUpdate;
        }

        private void Game_OnGameUpdate()
        {
            UpdateInfo();
        }

        public void UpdateInfo()
        {
            try
            {
                // fix
                var extraDelayBuffer = ObjectCache.menuCache.cache["ExtraPingBuffer"].As<MenuSlider>().Value;
                serverPos2D = hero.ServerPosition.To2D(); //CalculatedPosition.GetPosition(hero, Game.Ping);
                serverPos2DExtra = EvadeUtils.GetGamePosition(hero, Game.Ping + extraDelayBuffer);
                serverPos2DPing = EvadeUtils.GetGamePosition(hero, Game.Ping);
                //CalculatedPosition.GetPosition(hero, Game.Ping + extraDelayBuffer);            
                currentPosition = hero.Position.To2D(); //CalculatedPosition.GetPosition(hero, 0); 
                boundingRadius = hero.BoundingRadius;
                moveSpeed = hero.MoveSpeed;
                HasPath = hero.HasPath;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    public class MenuCache
    {
        public Dictionary<string, MenuComponent> cache = new Dictionary<string, MenuComponent>();
        public Menu menu;

        public MenuCache(Menu menu)
        {
            this.menu = menu;

            AddMenuToCache(menu);
        }

        public void AddMenuToCache(Menu newMenu)
        {
            foreach (var item in ReturnAllItems(newMenu))
                AddMenuComponentToCache(item);
        }

        public void AddMenuComponentToCache(MenuComponent item)
        {
            if (item != null && !cache.ContainsKey(item.InternalName))
                cache.Add(item.InternalName, item);
        }

        //public static List<MenuItem> ReturnAllItems(Menu menu)
        //{
        //    List<MenuItem> menuList = new List<MenuItem>();

        //    menuList.AddRange(menu.Items);

        //    foreach (var submenu in menu.Children)
        //    {
        //        menuList.AddRange(ReturnAllItems(submenu));
        //    }

        //    return menuList;
        //}

        public static List<MenuComponent> ReturnAllItems(Menu menu)
        {
            var menuList = new List<MenuComponent>();

            foreach (var item in menu.Children.Values)
            {
                if (item != null)
                {
                    Console.WriteLine(item.InternalName);
                    menuList.Add(item);
                }

                var asmenu = item as Menu;

                if (asmenu == null)
                    continue;

                foreach (var item2 in asmenu.Children.Values)
                {
                    if (item2 == item)
                        continue;

                    if (item2 != null)
                    {
                        Console.WriteLine(item2.InternalName);
                        menuList.Add(item2);
                    }
                }
            }
            // menuList.AddRange(menu.OfType<MenuComponent>());


            return menuList;
        }
    }

    public static class ObjectCache
    {
        public static Dictionary<int, Obj_AI_Turret> turrets = new Dictionary<int, Obj_AI_Turret>();

        public static HeroInfo myHeroCache = new HeroInfo(myHero);
        public static MenuCache menuCache = new MenuCache(Evade.menu);

        public static float gamePing;

        static ObjectCache()
        {
            InitializeCache();
            Game.OnUpdate += Game_OnGameUpdate;
        }

        private static Obj_AI_Hero myHero => ObjectManager.GetLocalPlayer();

        private static void Game_OnGameUpdate()
        {
            gamePing = Game.Ping;
        }

        private static void InitializeCache()
        {
            foreach (var obj in ObjectManager.Get<Obj_AI_Turret>())
                if (!turrets.ContainsKey(obj.NetworkId))
                    turrets.Add(obj.NetworkId, obj);
        }
    }
}