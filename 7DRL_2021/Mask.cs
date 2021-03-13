using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    class Mask : IEnumerable<Point>
    {
        public HashSet<Point> PointLookup = new HashSet<Point>();
        List<Point> PointList = new List<Point>();

        public Mask()
        {
        }

        public Mask(IEnumerable<Point> points)
        {
            foreach (var point in points)
            {
                Add(point);
            }
        }

        public void Add(Point point)
        {
            PointLookup.Add(point);
            PointList.Add(point);
        }

        public void Remove(Point point)
        {
            PointLookup.Remove(point);
            PointList.Remove(point);
        }

        public bool Contains(Point point)
        {
            return PointLookup.Contains(point);
        }

        public Rectangle GetRectangle()
        {
            return GetRectangle(0, 0);
        }

        public Rectangle GetRectangle(int xOff, int yOff)
        {
            int xMin = 0;
            int xMax = 0;
            int yMin = 0;
            int yMax = 0;
            foreach (Point point in PointList)
            {
                if (point.X < xMin)
                    xMin = point.X;
                if (point.Y < yMin)
                    yMin = point.Y;
                if (point.X > xMax)
                    xMax = point.X;
                if (point.Y > yMax)
                    yMax = point.Y;
            }
            return new Rectangle(xOff + xMin, yOff + yMin, xMax - xMin + 1, yMax - yMin + 1);
        }

        public IEnumerable<Point> GetFrontier()
        {
            HashSet<Point> frontier = new HashSet<Point>();

            foreach (Point point in PointList)
            {
                frontier.Add(new Point(point.X + 1, point.Y));
                frontier.Add(new Point(point.X - 1, point.Y));
                frontier.Add(new Point(point.X, point.Y + 1));
                frontier.Add(new Point(point.X, point.Y - 1));
            }

            return frontier.Except(PointLookup);
        }

        public IEnumerable<Point> GetFullFrontier()
        {
            HashSet<Point> frontier = new HashSet<Point>();

            foreach (Point point in PointList)
            {
                frontier.Add(new Point(point.X + 1, point.Y));
                frontier.Add(new Point(point.X - 1, point.Y));
                frontier.Add(new Point(point.X, point.Y + 1));
                frontier.Add(new Point(point.X, point.Y - 1));
                frontier.Add(new Point(point.X + 1, point.Y + 1));
                frontier.Add(new Point(point.X - 1, point.Y - 1));
                frontier.Add(new Point(point.X - 1, point.Y + 1));
                frontier.Add(new Point(point.X + 1, point.Y - 1));
            }

            return frontier.Except(PointLookup);
        }

        public IEnumerable<Point> GetFrontier(int dx, int dy)
        {
            HashSet<Point> frontier = new HashSet<Point>();

            foreach (Point point in PointList)
            {
                frontier.Add(new Point(point.X + dx, point.Y + dy));
            }

            return frontier.Except(PointLookup);
        }

        public IEnumerator<Point> GetEnumerator()
        {
            return PointList.GetEnumerator();
        }

        public Vector2 GetRandomPixel(Random random)
        {
            Point point = PointList.Pick(random);
            float x = (point.X + random.NextFloat()) * 16;
            float y = (point.Y + random.NextFloat()) * 16;
            return new Vector2(x, y);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Mask Copy()
        {
            Mask newMask = new Mask();
            foreach (var point in PointList)
            {
                newMask.Add(point);
            }
            return newMask;
        }
    }
}
