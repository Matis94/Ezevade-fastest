using System.Drawing;
using Aimtec;
using Aimtec.SDK.Extensions;

//using SharpDX;

namespace zzzz.Draw
{
    internal class RenderLine : RenderObject
    {
        public Color color = Color.White;
        public Vector2 end = new Vector2(0, 0);
        public Vector2 start = new Vector2(0, 0);

        public int width = 3;

        public RenderLine(Vector2 start, Vector2 end, float renderTime,
            int radius = 65, int width = 3)
        {
            startTime = EvadeUtils.TickCount;
            endTime = startTime + renderTime;
            this.start = start;
            this.end = end;

            this.width = width;
        }

        public RenderLine(Vector2 start, Vector2 end, float renderTime,
            Color color, int radius = 65, int width = 3)
        {
            startTime = EvadeUtils.TickCount;
            endTime = startTime + renderTime;
            this.start = start;
            this.end = end;

            this.color = color;

            this.width = width;
        }

        public override void Draw()
        {
            if (start.IsOnScreen() || end.IsOnScreen())
            {
                Vector2 realStart;
                Render.WorldToScreen(start.To3D(), out realStart);
                Vector2 realEnd;
                Render.WorldToScreen(end.To3D(), out realEnd);

                Render.Line(realStart, realEnd, color);
            }
        }
    }
}