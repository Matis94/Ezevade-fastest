using System.Collections.Generic;

//using SharpDX;

namespace zzzz.Draw
{
    internal abstract class RenderObject
    {
        public float endTime = 0;
        public float startTime = 0;

        public abstract void Draw();
    }

    internal class RenderObjects
    {
        private static readonly List<RenderObject> objects = new List<RenderObject>();

        static RenderObjects()
        {
            Aimtec.Render.OnPresent += Render_OnPresent; //Render.OnPresent += Render_OnPresent;
        }

        private static void Render_OnPresent()
        {
            Render();
        }

        private static void Render()
        {
            foreach (var obj in objects)
                if (obj.endTime - EvadeUtils.TickCount > 0)
                    obj.Draw(); //weird after draw
                else
                    DelayAction.Add(1, () => objects.Remove(obj));
        }

        public static void Add(RenderObject obj)
        {
            objects.Add(obj);
        }
    }
}