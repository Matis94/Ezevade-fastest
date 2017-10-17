using System.Drawing;
using Aimtec;
using Aimtec.SDK.Extensions;

//using SharpDX;

namespace zzzz.Draw
{
    internal class RenderText : RenderObject
    {
        public Color color = Color.White;
        public Vector2 renderPosition = new Vector2(0, 0);
        public string text = "";

        public RenderText(string text, Vector2 renderPosition, float renderTime)
        {
            startTime = EvadeUtils.TickCount;
            endTime = startTime + renderTime;
            this.renderPosition = renderPosition;

            this.text = text;
        }

        public RenderText(string text, Vector2 renderPosition, float renderTime,
            Color color)
        {
            startTime = EvadeUtils.TickCount;
            endTime = startTime + renderTime;
            this.renderPosition = renderPosition;

            this.color = color;

            this.text = text;
        }

        public override void Draw()
        {
            if (!renderPosition.IsZero)
            {
                var textDimension = 10; // LUL Drawing.GetTextExtent
                Vector2 wardScreenPos;
                Render.WorldToScreen(renderPosition.To3D(), out wardScreenPos);

                Render.Text(wardScreenPos.X - textDimension / 2, wardScreenPos.Y, color, text);
            }
        }
    }
}