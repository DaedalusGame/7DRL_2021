using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    class MapGenerator
    {
        enum RoomType
        {
            None,
            Start,
            End,
            Corridor,
            Chasm,
            Filled,
        }

        class Room
        {
            MapGenerator Generator;
            public RoomType Type;
            public Rectangle Exterior => new Rectangle(Interior.X - 1, Interior.Y - 1, Interior.Width + 2, Interior.Height + 2);
            public Rectangle Interior;
            public Zone Zone;

            public IEnumerable<Room> Neighbors => Generator.Neighbors.Where(x => x.RoomA == this || x.RoomB == this).Select(x => x.GetOther(this));
            public int Size => Interior.Width * Interior.Height;
            public float SizeRatio => Math.Min((float)Interior.Width / Interior.Height, (float)Interior.Height / Interior.Width);


            public Room(MapGenerator generator, RoomType type, Rectangle interior)
            {
                Generator = generator;
                Type = type;
                Interior = interior;
                Zone = new Zone();
            }

            public bool CanDivideHorizontal => Interior.Width >= 1 + Generator.LimitMinRoomSize * 2;
            public bool CanDivideVertical => Interior.Height >= 1 + Generator.LimitMinRoomSize * 2;

            public bool IsEdgeRoom()
            {
                return Exterior.Left == 0 || Exterior.Right == Generator.Width || Exterior.Top == 0 || Exterior.Bottom == Generator.Height;
            }

            public Point GetEdgeDirection()
            {
                Point direction = Point.Zero;
                if (Exterior.Left == 0)
                    direction.X -= 1;
                if (Exterior.Right == Generator.Width)
                    direction.X += 1;
                if (Exterior.Top == 0)
                    direction.Y -= 1;
                if (Exterior.Bottom == Generator.Height)
                    direction.Y += 1;
                return direction;
            }

            public IEnumerable<Room> DivideHorizontal(Random random)
            {
                var division = random.Next(Interior.Left + Generator.LimitMinRoomSize, Interior.Right - Generator.LimitMinRoomSize);
                yield return new Room(Generator, Type, new Rectangle(Interior.Left, Interior.Top, division - Interior.Left, Interior.Height));
                yield return new Room(Generator, Type, new Rectangle(division + 1, Interior.Top, Interior.Right - division - 1, Interior.Height));
            }

            public IEnumerable<Room> DivideVertical(Random random)
            {
                var division = random.Next(Interior.Top + Generator.LimitMinRoomSize, Interior.Bottom - Generator.LimitMinRoomSize);
                yield return new Room(Generator, Type, new Rectangle(Interior.Left, Interior.Top, Interior.Width, division - Interior.Top));
                yield return new Room(Generator, Type, new Rectangle(Interior.Left, division + 1, Interior.Width, Interior.Bottom - division - 1));
            }

            public IList<DivideDelegate> GetDivisions()
            {
                var divisions = new List<DivideDelegate>();
                if (CanDivideHorizontal)
                    divisions.Add(DivideHorizontal);
                if (CanDivideVertical)
                    divisions.Add(DivideVertical);
                return divisions;
            }

            public IEnumerable<Point> GetCoveredTiles()
            {
                List<Point> points = new List<Point>();
                for(int x = Exterior.Left; x < Exterior.Right; x++)
                {
                    for (int y = Exterior.Top; y < Exterior.Bottom; y++)
                    {
                        points.Add(new Point(x,y));
                    }
                }
                return points;
            }
        }

        class Neighborhood
        {
            public Room RoomA;
            public Room RoomB;
            public bool Connected;

            public Neighborhood(Room roomA, Room roomB)
            {
                RoomA = roomA;
                RoomB = roomB;
            }

            //Returns if the two rooms are neighbored in a way that we can create a door
            public bool IsProperConnection()
            {
                var rectHorA = RoomA.Interior;
                var rectHorB = RoomB.Interior;
                var rectVerA = RoomA.Interior;
                var rectVerB = RoomB.Interior;
                rectHorA.Inflate(1, 0);
                rectHorB.Inflate(1, 0);
                rectVerA.Inflate(0, 1);
                rectVerB.Inflate(0, 1);
                var deltaHor = Rectangle.Intersect(rectHorA, rectHorB);
                var deltaVer = Rectangle.Intersect(rectVerA, rectVerB);

                return deltaHor.Height > 2 || deltaVer.Width > 2;
            }

            public Room GetOther(Room room)
            {
                if (room == RoomA)
                    return RoomB;
                if (room == RoomB)
                    return RoomA;
                throw new Exception();
            }

            public bool IsBetween(Room roomA, Room roomB)
            {
                if (roomA == RoomA && roomB == RoomB)
                    return true;
                if (roomA == RoomB && roomB == RoomA)
                    return true;
                return false;
            }
        }
        
        class Zone
        {
        }

        delegate IEnumerable<Room> DivideDelegate(Random random);

        Random Random = new Random();
        int Width = 100;
        int Height = 100;
        int LimitMinRoomSize = 3;
        int LimitMinRooms = 20;
        int LimitMinChasms = 1;
        int LimitMinFilled = 6;
        float LimitRoomSizeAverage = 60;
        float LimitRoomSizeMax = 80;
        float LimitChasmRate = 0.3f;
        List<Room> Rooms = new List<Room>();
        List<Neighborhood> Neighbors = new List<Neighborhood>();

        float RoomSizeMin => Rooms.Min(x => x.Size);
        float RoomSizeMax => Rooms.Max(x => x.Size);
        float RoomSizeAverage => (float)Rooms.Average(x => x.Size);

        public void Generate()
        {
            Random random = new Random();
            Rooms.Add(new Room(this, RoomType.None, new Rectangle(1, 1, Width-2, Height-2)));
            while (Rooms.Count < LimitMinRooms || RoomSizeAverage > LimitRoomSizeAverage || RoomSizeMax > LimitRoomSizeMax)
            {
                Divide();
            }
            var interiors = Rooms.Where(x => !x.IsEdgeRoom()).ToList();
            for(int i = 0; i < LimitMinChasms; i++)
            {
                var chasm = interiors.PickAndRemove(Random);
                chasm.Type = RoomType.Chasm;
            }
            for (int i = 0; i < LimitMinFilled; i++)
            {
                var filled = interiors.PickAndRemove(Random);
                filled.Type = RoomType.Filled;
            }
            SpreadChasms();
            MakeCorridors();
            Kruskal();
            PlaceExits();
        }

        public void SpreadChasms()
        {
            var chasms = Rooms.Where(x => x.Type == RoomType.Chasm).ToList();
            while((float)chasms.Sum(x => x.Size) / Rooms.Sum(x => x.Size) < LimitChasmRate)
            {
                var expand = chasms.Pick(Random);
                //var allNeighbors = chasms.SelectMany(x => x.Neighbors).Where(x => x.Type == RoomType.None).ToList();
                //var picked = allNeighbors.Pick(Random);
                foreach (var neighbor in expand.Neighbors)
                {
                    neighbor.Type = RoomType.Chasm;
                    chasms.Add(neighbor);
                }
            }
        }

        public void MakeCorridors()
        {
            foreach(var room in Rooms.Where(x => x.Type == RoomType.Chasm))
            {
                foreach (var neighbor in room.Neighbors.Where(x => x.Type != RoomType.Chasm))
                    neighbor.Type = RoomType.Corridor;
            }
        }

        public void Divide()
        {
            var dividables = Rooms.Where(room => room.CanDivideHorizontal || room.CanDivideVertical).ToList();
            if (dividables.Any()) {
                var picked = dividables.PickBest(x => x.Size, Random);
                Remove(picked);
                var division = picked.GetDivisions().Pick(Random);
                foreach(var split in division(Random))
                {
                    Rooms.Add(split);
                    CalculateNeighbors(split);
                }
            }
        }

        void Remove(Room room)
        {
            Neighbors.RemoveAll(x => x.RoomA == room || x.RoomB == room);
            Rooms.Remove(room);
        }

        void CalculateNeighbors(Room room)
        {
            var neighbors = Rooms.Except(new[] { room }).Where(x => x.Exterior.Intersects(room.Exterior));
            foreach(var neighbor in neighbors)
            {
                Neighbors.Add(new Neighborhood(room, neighbor));
            }
        }

        IEnumerable<Room> GetZoneRooms(Zone zone)
        {
            return Rooms.Where(x => x.Zone == zone);
        }

        void Connect(Zone major, Zone minor)
        {
            foreach (var room in GetZoneRooms(minor))
                room.Zone = major;
        }

        void Kruskal()
        {
            var normalRooms = new[] { RoomType.None, RoomType.Start, RoomType.End, RoomType.Corridor };
            var edges = new Queue<Neighborhood>(Neighbors.Shuffle(Random));
            while(!edges.Empty())
            {
                var edge = edges.Dequeue();
                if (!normalRooms.Contains(edge.RoomA.Type) || !normalRooms.Contains(edge.RoomB.Type))
                    continue;
                if ((edge.RoomA.Zone != edge.RoomB.Zone || Random.NextDouble() < 0.1) && edge.IsProperConnection())
                {
                    edge.Connected = true;
                    Connect(edge.RoomA.Zone, edge.RoomB.Zone);
                    edge.RoomB.Zone = edge.RoomA.Zone;
                }
            }
        }

        void PlaceExits()
        {
            var normalRooms = new[] { RoomType.None, RoomType.Start, RoomType.End, RoomType.Corridor };
            var zones = Rooms.Where(x => normalRooms.Contains(x.Type)).GroupBy(x => x.Zone).ToList();
            var pickedZone = zones.WithMax(x => x.Count());
            var edgeRooms = pickedZone.Where(x => x.IsEdgeRoom()).ToList();
            var start = edgeRooms.PickAndRemove(Random);
            var end = edgeRooms.PickAndRemoveBest(x => GetManhattenDistance(start, x), Random);

            start.Type = RoomType.Start;
            end.Type = RoomType.End;
        }

        int GetManhattenDistance(Room a, Room b)
        {
            Point centerA = a.Interior.Center;
            Point centerB = b.Interior.Center;

            return Math.Max(Math.Abs(centerA.X - centerB.X), Math.Abs(centerA.Y - centerB.Y));
        }

        private bool IsEdgeTile(Point point)
        {
            return point.X <= 0 || point.Y <= 0 || point.X >= Width - 1 || point.Y >= Height - 1;
        }

        private bool IsConnected(Room a, Room b)
        {
            return Neighbors.Any(x => x.Connected && x.IsBetween(a, b));
        }

        public void Print(Map map)
        {
            SimpleNoise spikeNoise = new SimpleNoise(Random.Next());
            if (map.Width != Width || map.Height != Height)
                throw new Exception();
            var tiles = Rooms.SelectMany(x => x.GetCoveredTiles().Select(y => new Tuple<Room, Point>(x, y))).GroupBy(pair => pair.Item2, pair => pair.Item1);
            foreach(var rooms in tiles)
            {
                var pos = rooms.Key;
                var tile = map.GetTile(pos.X, pos.Y);
                var count = rooms.Count();
                var singleRoom = count == 1 ? rooms.Single() : null;
                RoomType[] normalRooms = new[] { RoomType.None, RoomType.Start, RoomType.End, RoomType.Corridor };
                RoomType[] startEnd = new[] { RoomType.Start, RoomType.End };
                Template[] floors = new[] { null, Template.Corridor };

                Template template = null;

                if (singleRoom != null && singleRoom.Type == RoomType.Filled)
                    template = Template.Wall;
                if (singleRoom != null && singleRoom.Type == RoomType.Chasm)
                    template = Template.Chasm;
                if (count > 1)
                    template = Template.Wall;
                if (rooms.Any(x => x.Type == RoomType.Chasm))
                    template = Template.Corridor;
                if (rooms.All(x => x.Type == RoomType.Chasm))
                    template = Template.Chasm;
                if (rooms.All(x => x.Type == RoomType.Corridor))
                    template = Template.Corridor;
                if (singleRoom?.Type == RoomType.Start || singleRoom?.Type == RoomType.End)
                    template = Template.Floor;
                if (count == 2 && IsConnected(rooms.First(), rooms.Last()) && !floors.Contains(template))
                    template = null;
                if (IsEdgeTile(pos) && floors.Contains(template) && !rooms.All(x => startEnd.Contains(x.Type)))
                    template = Template.Wall;

                int spikeValue = spikeNoise.GetValue(pos.X / 3, pos.Y / 3);

                if (template == Template.Wall && (spikeValue % 10 == 0 || spikeNoise.GetValue(pos.X, pos.Y) % 30 == 0))
                    template = Template.SpikeWall;
                if (template == Template.Wall && Random.NextDouble() < 0.1)
                    template = Template.WraithWall;

                if (template == null)
                    template = Template.Floor;

                if (template != null)
                    tile.ApplyTemplate(template);
                if (rooms.Any(x => x.Type == RoomType.Chasm) && template != Template.Chasm)
                    Behavior.Apply(new BehaviorChasmSeam(tile, template == Template.Corridor ? new Color(155, 99, 74) : new Color(124, 88, 114)));
                if (singleRoom?.Type == RoomType.Start)
                    Behavior.Apply(new BehaviorLevelStart(tile, singleRoom.GetEdgeDirection()));
                if (singleRoom?.Type == RoomType.End)
                {
                    Behavior.Apply(new BehaviorLevelEnd(tile, singleRoom.GetEdgeDirection()));
                    if (singleRoom.Interior.Center == pos)
                    {
                        var pointer = new Curio(Template.Pointer);
                        pointer.MoveTo(tile);
                        Behavior.Apply(new BehaviorEscapeTarget(pointer, singleRoom.Interior));
                    }
                }
            }
        }
    }
}
