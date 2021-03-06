namespace _7DRL_2021.Pathfinding
{
    using Priority_Queue;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// calculates paths given an IWeightedGraph and start/goal positions
    /// </summary>
    public static class WeightedPathfinder
    {
        public static bool Search<T>( IWeightedGraph<T> graph, T start, T goal, out Dictionary<T, T> cameFrom)
        {
            var foundPath = false;
            cameFrom = new Dictionary<T, T>();
            cameFrom.Add(start, start);

            var costSoFar = new Dictionary<T, float>();
            var frontier = new SimplePriorityQueue<T>();

            costSoFar[start] = 0;

            frontier.Enqueue(start, 0);

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                if (current.Equals(goal))
                {
                    foundPath = true;
                    break;
                }

                foreach (var next in graph.GetNeighbors(current))
                {
                    var newCost = costSoFar[current] + graph.Cost(current, next);
                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        frontier.Enqueue(next, newCost);
                        cameFrom[next] = current;
                    }
                }
            }

            return foundPath;
        }

        /*public static bool Search<T>( IWeightedGraph<T> graph, T start, T goal, out Dictionary<T,T> cameFrom )
        {
            var foundPath = false;
            cameFrom = new Dictionary<T,T>();
            cameFrom.Add( start, start );

            var costSoFar = new Dictionary<T, int>();
            var frontier = new List<Tuple<int, T>> { new Tuple<int, T>(0, start) };

            costSoFar[start] = 0;

            while( frontier.Count > 0 )
            {
                var current = frontier[0];
                frontier.RemoveAt(0);

                if ( current.Item2.Equals( goal ) )
                {
                    foundPath = true;
                    break;
                }

                foreach( var next in graph.GetNeighbors( current.Item2) )
                {
                    var newCost = costSoFar[current.Item2] + graph.Cost( current.Item2, next );
                    if( !costSoFar.ContainsKey( next ) || newCost < costSoFar[next] )
                    {
                        costSoFar[next] = newCost;
                        var priority = newCost;
                        frontier.Add( new Tuple<int, T>(priority, next));
                        cameFrom[next] = current.Item2;
                    }
                }

                frontier.Sort(new TupleComparer<T>());
            }

            return foundPath;
        }*/

        /// <summary>
        /// gets a path from start to goal if possible. If no path is found null is returned.
        /// </summary>
        /// <param name="graph">Graph.</param>
        /// <param name="start">Start.</param>
        /// <param name="goal">Goal.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static List<T> Search<T>( IWeightedGraph<T> graph, T start, T goal )
        {
            var foundPath = Search( graph, start, goal, out var cameFrom );
            return foundPath ? PathConstructor.RecontructPath( cameFrom, start, goal ) : null;
        }
    }
}

