using System.Drawing;
using Aimtec;
using Aimtec.SDK.Extensions;

//using SharpDX;

namespace zzzz.Draw
{
    internal class RenderCircle : RenderObject
    {
        public Color color = Color.White;

        public int radius = 65;
        public Vector2 renderPosition = new Vector2(0, 0);
        public /*SharpDX.*/ Vector2 renderPositionDX = new /*SharpDX.*/Vector2(0, 0);
        public int width = 5;

        public RenderCircle(Vector2 renderPosition, float renderTime,
            int radius = 65, int width = 5)
        {
            startTime = EvadeUtils.TickCount;
            endTime = startTime + renderTime;
            this.renderPosition = renderPosition;

            this.radius = radius;
            this.width = width;
        }

        public RenderCircle(Vector2 renderPosition, float renderTime,
            Color color, int radius = 65, int width = 5)
        {
            startTime = EvadeUtils.TickCount;
            endTime = startTime + renderTime;
            this.renderPosition = renderPosition;

            this.color = color;

            this.radius = radius;
            this.width = width;
        }

        //public RenderCircle(/*SharpDX.*/Vector2 renderPosition, float renderTime,
        //    Color color, int radius = 65, int width = 5)
        //{
        //    this.startTime = EvadeUtils.TickCount;
        //    this.endTime = this.startTime + renderTime;
        //    this.renderPositionDX = renderPosition;

        //    this.color = color;

        //    this.radius = radius;
        //    this.width = width;
        //}

        public override void Draw()
        {
            if (renderPosition.IsOnScreen())
                Render.Circle(renderPosition.To3D(), radius, 50, color);
        }
    }
}