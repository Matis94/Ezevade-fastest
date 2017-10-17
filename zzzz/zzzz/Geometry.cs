using System.Collections.Generic;
using System.Drawing;
using Aimtec;
using Aimtec.SDK.Extensions;

namespace zzzz
{
    public class Geometry
    {
        private const int CircleLineSegmentN = 22;

        public class Polygon
        {
            public List<Vector3> Points = new List<Vector3>();

            public void Add(Vector3 point)
            {
                Points.Add(point);
            }

            public void Draw(Color color, int width = 1)
            {
                for (var i = 0; i <= Points.Count - 1; i++)
                {
                    var nextIndex = Points.Count - 1 == i ? 0 : i + 1;
                    Render.Line(Points[i].ToScreenPosition(), Points[nextIndex].ToScreenPosition(), width, true, color);
                }
            }
        }

        public class Rectangle
        {
            public Vector3 Direction;
            public Vector3 Perpendicular;
            public Vector3 REnd;
            public Vector3 RStart;
            public float Width;

            public Rectangle(Vector3 start, Vector3 end, float width)
            {
                RStart = start;
                REnd = end;
                Width = width;
                Direction = (end - start).Normalized();
                Perpendicular = Direction.Perpendicular();
            }

            public Polygon ToPolygon(int offset = 0, float overrideWidth = -1)
            {
                var result = new Polygon();

                result.Add(
                    RStart + (overrideWidth > 0 ? overrideWidth : Width + offset) * Perpendicular - offset * Direction);
                result.Add(
                    RStart - (overrideWidth > 0 ? overrideWidth : Width + offset) * Perpendicular - offset * Direction);
                result.Add(
                    REnd - (overrideWidth > 0 ? overrideWidth : Width + offset) * Perpendicular + offset * Direction);
                result.Add(
                    REnd + (overrideWidth > 0 ? overrideWidth : Width + offset) * Perpendicular + offset * Direction);

                return result;
            }
        }
    }
}