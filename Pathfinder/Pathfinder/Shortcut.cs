using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using Pathfinder.Analysis;
using Pathfinder.GraphTheory;
using Pathfinder.Reading;


namespace Pathfinder
{
    public class Shortcut
    {
        #region Methods

        private static void ClearIntermediateVertices(List<List<Vertex>> outlines)
        {
            foreach (List<Vertex> outline in outlines)
            {
                ClearIntermediateVertices(outline);
            }
        }

        private static void ClearIntermediateVertices(List<Vertex> outline)
        {
            for (int i = 0; i < outline.Count && outline.Count > 2; i++)
            {
                int j = (i + 1)%outline.Count;
                int k = (i + 2)%outline.Count;

                Vertex a = outline[i];
                Vertex b = outline[j];
                Vertex c = outline[k];

                bool remove = false;
                if (a.X == b.X && a.X == c.X)
                {
                    if (a.Y < c.Y)
                    {
                        if (b.Y > a.Y && b.Y < c.Y)
                        {
                            remove = true;
                        }
                    }
                    else
                    {
                        if (b.Y > c.Y && b.Y < a.Y)
                        {
                            remove = true;
                        }
                    }
                }
                else if (a.Y == b.Y && a.Y == c.Y)
                {
                    if (a.X < c.X)
                    {
                        if (b.X > a.X && b.X < c.X)
                        {
                            remove = true;
                        }
                    }
                    else
                    {
                        if (b.X > c.X && b.X < a.X)
                        {
                            remove = true;
                        }
                    }
                }

                if (remove)
                {
                    outline.RemoveAt(j);
                    i--;
                }
            }
        }

        private static void CorrectCorridorsAndGaps(Vertex start,
                                                    Vertex end,
                                                    List<List<Vertex>> outlines,
                                                    List<Corridor> horizontalCorridors,
                                                    List<Corridor> verticalCorridors)
        {
            int xmin, xmax;
            if (start.X < end.X)
            {
                xmin = start.X;
                xmax = end.X;
            }
            else
            {
                xmin = end.X;
                xmax = start.X;
            }

            int ymin, ymax;
            if (start.Y < end.Y)
            {
                ymin = start.Y;
                ymax = end.Y;
            }
            else
            {
                ymin = end.Y;
                ymax = start.Y;
            }

            foreach (List<Vertex> outline in outlines)
            {
                foreach (Vertex vertex in outline)
                {
                    int x = vertex.X;
                    int y = vertex.Y;

                    if (x < xmin)
                    {
                        xmin = x;
                    }
                    else if (x > xmax)
                    {
                        xmax = x;
                    }

                    if (y < ymin)
                    {
                        ymin = y;
                    }
                    else if (y > ymax)
                    {
                        ymax = y;
                    }
                }
            }

            for (int i = 0; i < horizontalCorridors.Count; i++)
            {
                Corridor corridor = horizontalCorridors[i];

                int y1 = corridor.Ymin;
                int y2 = corridor.Ymax;

                int x1 = corridor.Xmin;
                int x2 = corridor.Xmax;

                if (y1 > ymin && y1 < ymax || y2 > ymin && y2 < ymax)
                {
                    byte b1;
                    if (x1 < xmin)
                    {
                        b1 = 1;
                    }
                    else if (x1 > xmax)
                    {
                        b1 = 2;
                    }
                    else
                    {
                        b1 = 0;
                    }

                    byte b2;
                    if (x2 < xmin)
                    {
                        b2 = 1;
                    }
                    else if (x2 > xmax)
                    {
                        b2 = 2;
                    }
                    else
                    {
                        b2 = 0;
                    }

                    if ((b1 & b2) == 0)
                    {
                        if (b1 == 1)
                        {
                            corridor.Xmin = xmin;
                        }

                        if (b2 == 2)
                        {
                            corridor.Xmax = xmax;
                        }
                    }
                    else
                    {
                        horizontalCorridors.RemoveAt(i);
                        i--;
                    }
                }
                else
                {
                    horizontalCorridors.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < verticalCorridors.Count; i++)
            {
                Corridor corridor = verticalCorridors[i];

                int x1 = corridor.Xmin;
                int x2 = corridor.Xmax;

                int y1 = corridor.Ymin;
                int y2 = corridor.Ymax;

                if (x1 > xmin && x1 < xmax || x2 > xmin && x2 < xmax)
                {
                    byte b1;
                    if (y1 < ymin)
                    {
                        b1 = 1;
                    }
                    else if (y1 > ymax)
                    {
                        b1 = 2;
                    }
                    else
                    {
                        b1 = 0;
                    }

                    byte b2;
                    if (y2 < ymin)
                    {
                        b2 = 1;
                    }
                    else if (y2 > ymax)
                    {
                        b2 = 2;
                    }
                    else
                    {
                        b2 = 0;
                    }

                    if ((b1 & b2) == 0)
                    {
                        if (b1 == 1)
                        {
                            corridor.Ymin = ymin;
                        }

                        if (b2 == 2)
                        {
                            corridor.Ymax = ymax;
                        }
                    }
                    else
                    {
                        verticalCorridors.RemoveAt(i);
                        i--;
                    }
                }
                else
                {
                    verticalCorridors.RemoveAt(i);
                    i--;
                }
            }
        }

        private static List<List<Vertex>> CorrectOutlines(List<List<Vertex>> outlines, Vertex start, Vertex end)
        {
            int xmin, xmax;
            if (start.X < end.X)
            {
                xmin = start.X;
                xmax = end.X;
            }
            else
            {
                xmin = end.X;
                xmax = start.X;
            }

            int ymin, ymax;
            if (start.Y < end.Y)
            {
                ymin = start.Y;
                ymax = end.Y;
            }
            else
            {
                ymin = end.Y;
                ymax = start.Y;
            }

            List<List<Vertex>> selectedOutlines = SelectOutlines(outlines, xmin, ymin, xmax, ymax);

            foreach (List<Vertex> outline in selectedOutlines)
            {
                foreach (Vertex vertex in outline)
                {
                    int x = vertex.X;
                    if (x < xmin)
                    {
                        xmin = x;
                    }
                    else if (x > xmax)
                    {
                        xmax = x;
                    }

                    int y = vertex.Y;
                    if (y < ymin)
                    {
                        ymin = y;
                    }
                    else if (y > ymax)
                    {
                        ymax = y;
                    }
                }
            }

            return SelectOutlines(outlines, xmin, ymin, xmax, ymax);
        }

        private static void DivideSegments(List<Segment> segments)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                Segment a = segments[i];

                if (a.Min == a.Max)
                {
                    segments.RemoveAt(i);
                    i--;
                }
                else
                {
                    for (int j = i + 1; j < segments.Count; j++)
                    {
                        Segment b = segments[j];

                        if (a.Level == b.Level)
                        {
                            if (b.Min <= a.Max && b.Max >= a.Min)
                            {
                                if (b.Min == a.Min)
                                {
                                    if (b.Max == a.Min)
                                    {
                                        segments.RemoveAt(j);
                                        j--;
                                    }
                                    if (b.Max == a.Max)
                                    {
                                        segments.RemoveAt(j);
                                        j--;
                                    }
                                    else if (b.Max > a.Max)
                                    {
                                        b.Min = a.Max;
                                    }
                                    else
                                    {
                                        a.Min = b.Max;
                                    }
                                }
                                else if (b.Min == a.Max)
                                {
                                    if (b.Max == a.Max)
                                    {
                                        segments.RemoveAt(j);
                                        j--;
                                    }
                                }
                                else if (b.Min < a.Min)
                                {
                                    if (b.Max == a.Min)
                                    {
                                    }
                                    else if (b.Max == a.Max)
                                    {
                                        b.Max = a.Min;
                                    }
                                    else if (b.Max > a.Max)
                                    {
                                        segments.Add(new Segment(a.Level, a.Max, b.Max));
                                        b.Max = a.Min;
                                    }
                                    else if (b.Max < a.Max)
                                    {
                                        segments.Add(new Segment(a.Level, b.Max, a.Max));
                                        a.Max = b.Max;
                                        b.Max = a.Min;
                                    }
                                }
                                else if (b.Min > a.Min)
                                {
                                    if (b.Max == a.Max)
                                    {
                                        a.Max = b.Min;
                                    }
                                    else if (b.Max > a.Max)
                                    {
                                        segments.Add(new Segment(a.Level, a.Max, b.Max));
                                        b.Max = a.Max;
                                        a.Max = b.Min;
                                    }
                                    else
                                    {
                                        segments.Add(new Segment(a.Level, b.Max, a.Max));
                                        a.Max = b.Min;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void DoubleToInt(List<Area> areas,
                                        List<Section> horizontalSections,
                                        List<Section> verticalSections,
                                        int offset,
                                        out List<Rectangle> rectangles,
                                        out List<Corridor> horizontalCorridors,
                                        out List<Corridor> verticalCorridors)
        {
            rectangles = new List<Rectangle>(areas.Count);
            foreach (Area area in areas)
            {
                rectangles.Add(new Rectangle(area, offset));
            }

            horizontalCorridors = new List<Corridor>(horizontalSections.Count);
            foreach (Section section in horizontalSections)
            {
                horizontalCorridors.Add(Corridor.CreateHorizontalCorridor(section, offset));
            }

            verticalCorridors = new List<Corridor>(verticalSections.Count);
            foreach (Section section in verticalSections)
            {
                verticalCorridors.Add(Corridor.CreateVerticalCorridor(section, offset));
            }
        }

        private static void GetCoordinates(Vertex start,
                                           Vertex end,
                                           List<List<Vertex>> outlines,
                                           List<Corridor> horizontalCorridors,
                                           List<Corridor> verticalCorridors,
                                           out HashSet<int> xs,
                                           out HashSet<int> ys)
        {
            xs = new HashSet<int> {start.X, end.X};
            ys = new HashSet<int> {start.Y, end.Y};

            foreach (List<Vertex> outline in outlines)
            {
                foreach (Vertex vertex in outline)
                {
                    xs.Add(vertex.X);
                    ys.Add(vertex.Y);
                }
            }

            foreach (Corridor corridor in horizontalCorridors)
            {
                xs.Add(corridor.Xmin);
                xs.Add(corridor.Xmax);

                ys.Add(corridor.Ymin);
                ys.Add(corridor.Ymax);
            }

            foreach (Corridor corridor in verticalCorridors)
            {
                xs.Add(corridor.Xmin);
                xs.Add(corridor.Xmax);

                ys.Add(corridor.Ymin);
                ys.Add(corridor.Ymax);
            }
        }

        private static List<List<Vertex>> GetOutlines(List<Rectangle> rectangles)
        {
            var horizontalSegments = new List<Segment>();
            var verticalSegments = new List<Segment>();

            foreach (Rectangle rectangle in rectangles)
            {
                int x1 = rectangle.Xmin;
                int y1 = rectangle.Ymin;

                int x2 = rectangle.Xmax;
                int y2 = rectangle.Ymax;

                horizontalSegments.Add(new Segment(y1, x1, x2));
                horizontalSegments.Add(new Segment(y2, x1, x2));

                verticalSegments.Add(new Segment(x1, y1, y2));
                verticalSegments.Add(new Segment(x2, y1, y2));
            }

            DivideSegments(horizontalSegments);
            DivideSegments(verticalSegments);

            var verticals = new Dictionary<int, List<Segment>>();
            foreach (Segment segment in verticalSegments)
            {
                int key = segment.Level;
                if (verticals.ContainsKey(key))
                {
                    verticals[key].Add(segment);
                }
                else
                {
                    verticals[key] = new List<Segment> {segment};
                }
            }

            for (int i = 0; i < horizontalSegments.Count; i++)
            {
                Segment horizontal = horizontalSegments[i];

                foreach (int x in new[] {horizontal.Min, horizontal.Max})
                {
                    if (verticals.ContainsKey(x))
                    {
                        List<Segment> segments = verticals[x];
                        for (int j = 0; j < segments.Count; j++)
                        {
                            Segment vertical = segments[j];

                            if (horizontal.Level > vertical.Min && horizontal.Level < vertical.Max)
                            {
                                segments.Add(new Segment(x, horizontal.Level, vertical.Max));
                                vertical.Max = horizontal.Level;
                            }
                        }
                    }
                }

                for (int x = horizontal.Min + 1; x < horizontal.Max; x++)
                {
                    if (verticals.ContainsKey(x))
                    {
                        List<Segment> segments = verticals[x];
                        for (int j = 0; j < segments.Count; j++)
                        {
                            Segment vertical = segments[j];

                            if (horizontal.Level == vertical.Min || horizontal.Level == vertical.Max)
                            {
                                horizontalSegments.Add(new Segment(horizontal.Level, x, horizontal.Max));
                                horizontal.Max = x;
                                break;
                            }
                            if (horizontal.Level > vertical.Min && horizontal.Level < vertical.Max)
                            {
                                segments.Add(new Segment(x, horizontal.Level, vertical.Max));
                                vertical.Max = horizontal.Level;

                                horizontalSegments.Add(new Segment(horizontal.Level, x, horizontal.Max));
                                horizontal.Max = x;
                                break;
                            }
                        }
                    }
                }
            }

            var horizontals = new Dictionary<int, List<Segment>>();
            foreach (Segment segment in horizontalSegments)
            {
                int key = segment.Level;
                if (horizontals.ContainsKey(key))
                {
                    horizontals[key].Add(segment);
                }
                else
                {
                    horizontals[key] = new List<Segment> {segment};
                }
            }

            List<int> horizontalKeys = horizontals.Keys.ToList();
            horizontalKeys.Sort();

            List<int> verticalKeys = verticals.Keys.ToList();
            verticalKeys.Sort();

            var vertices = new Dictionary<int, Dictionary<int, Vertex>>();
            var rays = new Dictionary<Vertex, Vertex[]>();

            foreach (int y in horizontalKeys)
            {
                List<Segment> segments = horizontals[y];
                segments.Sort((a, b) => a.Min.CompareTo(b.Min));

                var xs = new Dictionary<int, Vertex>();

                int previous;
                Vertex previousVertex;
                Vertex[] previousRay;
                {
                    Segment segment = segments[0];

                    var minVertex = new Vertex(segment.Min, y);
                    xs.Add(segment.Min, minVertex);

                    var maxVertex = new Vertex(segment.Max, y);
                    xs.Add(segment.Max, maxVertex);

                    rays[minVertex] = new[] {maxVertex, null, null, null};

                    var maxRay = new[] {null, null, minVertex, null};
                    rays[maxVertex] = maxRay;

                    previous = segment.Max;
                    previousVertex = maxVertex;
                    previousRay = maxRay;
                }

                for (int i = 1; i < segments.Count; i++)
                {
                    Segment segment = segments[i];

                    var maxVertex = new Vertex(segment.Max, y);

                    Vertex minVertex;
                    if (segment.Min == previous)
                    {
                        minVertex = previousVertex;
                        previousRay[0] = maxVertex;
                    }
                    else
                    {
                        minVertex = new Vertex(segment.Min, y);
                        xs.Add(segment.Min, minVertex);

                        rays[minVertex] = new[] {maxVertex, null, null, null};
                    }

                    xs.Add(segment.Max, maxVertex);

                    var maxRay = new[] {null, null, minVertex, null};
                    rays[maxVertex] = maxRay;

                    previous = segment.Max;
                    previousVertex = maxVertex;
                    previousRay = maxRay;
                }

                vertices.Add(y, xs);
            }

            foreach (int x in verticalKeys)
            {
                List<Segment> segments = verticals[x];
                segments.Sort((a, b) => a.Min.CompareTo(b.Min));

                foreach (Segment segment in segments)
                {
                    Vertex minVertex;
                    if (vertices.ContainsKey(segment.Min))
                    {
                        Dictionary<int, Vertex> xs = vertices[segment.Min];
                        if (xs.ContainsKey(x))
                        {
                            minVertex = vertices[segment.Min][x];
                        }
                        else
                        {
                            minVertex = new Vertex(x, segment.Min);
                            xs[x] = minVertex;
                        }
                    }
                    else
                    {
                        minVertex = new Vertex(x, segment.Min);
                        vertices[segment.Min] = new Dictionary<int, Vertex> {{x, minVertex}};
                    }

                    Vertex maxVertex;
                    if (vertices.ContainsKey(segment.Max))
                    {
                        Dictionary<int, Vertex> xs = vertices[segment.Max];
                        if (xs.ContainsKey(x))
                        {
                            maxVertex = vertices[segment.Max][x];
                        }
                        else
                        {
                            maxVertex = new Vertex(x, segment.Max);
                            xs[x] = maxVertex;
                        }
                    }
                    else
                    {
                        maxVertex = new Vertex(x, segment.Max);
                        vertices[segment.Max] = new Dictionary<int, Vertex> {{x, maxVertex}};
                    }

                    if (rays.ContainsKey(minVertex))
                    {
                        rays[minVertex][1] = maxVertex;
                    }
                    else
                    {
                        rays[minVertex] = new[] {null, maxVertex, null, null};
                    }

                    if (rays.ContainsKey(maxVertex))
                    {
                        rays[maxVertex][3] = minVertex;
                    }
                    else
                    {
                        rays[maxVertex] = new[] {null, null, null, minVertex};
                    }
                }
            }

            var visitedVertices = new HashSet<Vertex>();
            var outlines = new List<List<Vertex>>();

            foreach (KeyValuePair<Vertex, Vertex[]> pair in rays)
            {
                Vertex current = pair.Key;
                if (visitedVertices.Contains(current))
                {
                    continue;
                }

                int shift = 0;
                var outline = new List<Vertex> {current};

                Vertex next = null;
                do
                {
                    Vertex[] ends = rays[current];

                    for (int i = 0; i < 4; i++)
                    {
                        int index = (i + shift)%4;
                        next = ends[index];

                        if (next != null)
                        {
                            if (outline.Count > 1)
                            {
                                if (outline[0] == current && outline[1] == next)
                                {
                                    next = null;
                                    break;
                                }
                            }

                            outline.Add(next);

                            current = next;
                            shift = (index + 3)%4;
                            break;
                        }
                    }
                } while (next != null);

                foreach (Vertex vertex in outline)
                {
                    MarkVertexAsVisited(vertex, rays, visitedVertices);
                }

                outlines.Add(outline);
            }

            for (int i = 0; i < outlines.Count; i++)
            {
                List<Vertex> outline = outlines[i];
                Vertex point = outline[0];

                for (int k = 0; k < outlines.Count; k++)
                {
                    if (k == i)
                    {
                        continue;
                    }

                    if (IsPointInPolygon(point, outlines[k]))
                    {
                        outlines.RemoveAt(i);
                        i--;
                        break;
                    }
                }
            }

            ClearIntermediateVertices(outlines);

            return outlines;
        }

        public static List<Point> GetPath(List<Area> areas,
                                          List<Section> horizontalSections,
                                          List<Section> verticalSections,
                                          int offset,
                                          int intersectionWeight,
                                          Vertex start,
                                          Vertex end,
                                          int orientation)
        {
            if (start.X == end.X && start.Y == end.Y || offset <= 0)
            {
                return new List<Point>();
            }

            DoubleToInt(areas,
                        horizontalSections,
                        verticalSections,
                        offset,
                        out List<Rectangle> rectangles,
                        out List<Corridor> horizontalCorridors,
                        out List<Corridor> verticalCorridors);

            List<List<Vertex>> outlines = GetOutlines(rectangles);

            //outlines = CorrectOutlines(outlines, start, end);

            CorrectCorridorsAndGaps(start,
                                    end,
                                    outlines,
                                    horizontalCorridors,
                                    verticalCorridors);

            GetCoordinates(start,
                           end,
                           outlines,
                           horizontalCorridors,
                           verticalCorridors,
                           out HashSet<int> unsortedXs,
                           out HashSet<int> unsortedYs);


            List<int> xs = unsortedXs.ToList();
            xs.Sort();

            List<int> ys = unsortedYs.ToList();
            ys.Sort();

            GetSides(outlines,
                     out Dictionary<int, List<Side>> horizontalSides,
                     out Dictionary<int, List<Side>> verticalSides);

            var graph = new Graph(start,
                                  end,
                                  horizontalSides,
                                  verticalSides,
                                  horizontalCorridors,
                                  verticalCorridors,
                                  xs,
                                  ys,
                                  intersectionWeight,
                                  orientation);

            Node startNode;
            {
                int a = graph.AbscissaRedirects[start.X];
                int o = graph.OrdinateRedirects[start.Y];

                if (a >= 0 && o >= 0)
                {
                    startNode = graph[a, o];
                }
                else
                {
                    startNode = null;
                }
            }

            Node endNode;
            {
                int a = graph.AbscissaRedirects[end.X];
                int o = graph.OrdinateRedirects[end.Y];

                if (a >= 0 && o >= 0)
                {
                    endNode = graph[a, o];
                }
                else
                {
                    endNode = null;
                }
            }

            {
                var points = new List<Point>();
                if (startNode != null && endNode != null)
                {
                    Dijkstra.FindShortPath(graph,
                                           startNode,
                                           endNode,
                                           out List<Node> nodePath,
                                           out List<Edge> edgePath);

                    SmoothPath(nodePath, edgePath, out List<Node> resultNodePath);

                    var vertices = new List<Vertex>();
                    if (resultNodePath != null && resultNodePath.Count > 0)
                    {
                        foreach (Node node in resultNodePath)
                        {
                            int x = xs[node.A];
                            int y = ys[node.O];

                            var vertex = new Vertex(x, y);
                            vertices.Add(vertex);
                        }

                        ClearIntermediateVertices(vertices);

                        foreach (Vertex vertex in vertices)
                        {
                            points.Add(new Point(vertex.X, vertex.Y));
                        }
                    }
                }
                return points;
            }
        }

        private static void GetSides(List<List<Vertex>> outlines,
                                     out Dictionary<int, List<Side>> horizontalSides,
                                     out Dictionary<int, List<Side>> verticalSides)
        {
            horizontalSides = new Dictionary<int, List<Side>>();
            verticalSides = new Dictionary<int, List<Side>>();

            foreach (List<Vertex> outline in outlines)
            {
                if (outline.Count < 2)
                {
                    continue;
                }

                int upper = outline.Count - 1;
                for (int i = 0; i < upper; i++)
                {
                    int j = i + 1;

                    Vertex p1 = outline[i];
                    Vertex p2 = outline[j];

                    if (p1.X == p2.X)
                    {
                        Side side;
                        if (p1.Y < p2.Y)
                        {
                            side = new Side(p1.Y, p2.Y, 1);
                        }
                        else
                        {
                            side = new Side(p2.Y, p1.Y, -1);
                        }

                        int x = p1.X;
                        if (verticalSides.ContainsKey(x))
                        {
                            verticalSides[x].Add(side);
                        }
                        else
                        {
                            verticalSides[x] = new List<Side> {side};
                        }
                    }
                    else
                    {
                        Side side;
                        if (p1.X < p2.X)
                        {
                            side = new Side(p1.X, p2.X, 1);
                        }
                        else
                        {
                            side = new Side(p2.X, p1.X, -1);
                        }

                        int y = p1.Y;
                        if (horizontalSides.ContainsKey(y))
                        {
                            horizontalSides[y].Add(side);
                        }
                        else
                        {
                            horizontalSides[y] = new List<Side> {side};
                        }
                    }
                }
            }

            foreach (List<Side> sides in horizontalSides.Values)
            {
                sides.Sort((a, b) => a.Cmin.CompareTo(b.Cmin));
            }

            foreach (List<Side> sides in verticalSides.Values)
            {
                sides.Sort((a, b) => a.Cmin.CompareTo(b.Cmin));
            }
        }

        private static bool IsPointInPolygon(Vertex point, List<Vertex> polygon)
        {
            Vertex p0 = polygon[0];

            double xmin = p0.X;
            double xmax = p0.X;

            double ymin = p0.Y;
            double ymax = p0.Y;

            for (int i = 1; i < polygon.Count; i++)
            {
                Vertex p = polygon[i];

                if (p.X < xmin)
                {
                    xmin = p.X;
                }
                else if (p.X > xmax)
                {
                    xmax = p.X;
                }

                if (p.Y < ymin)
                {
                    ymin = p.Y;
                }
                else if (p.Y > ymax)
                {
                    ymax = p.Y;
                }
            }

            if (point.X < xmin || point.X > xmax || point.Y < ymin || point.Y > ymax)
            {
                return false;
            }

            bool inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if (polygon[i].Y > point.Y != polygon[j].Y > point.Y &&
                    point.X < (polygon[j].X - polygon[i].X)*(point.Y - polygon[i].Y)/(polygon[j].Y - polygon[i].Y) +
                    polygon[i].X)
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        private static void MarkVertexAsVisited(Vertex vertex,
                                                Dictionary<Vertex, Vertex[]> rays,
                                                HashSet<Vertex> visitedVertices)
        {
            if (!visitedVertices.Contains(vertex))
            {
                visitedVertices.Add(vertex);

                foreach (Vertex end in rays[vertex])
                {
                    if (end != null)
                    {
                        MarkVertexAsVisited(end, rays, visitedVertices);
                    }
                }
            }
        }

        private static List<List<Vertex>> SelectOutlines(List<List<Vertex>> outlines,
                                                         int xmin,
                                                         int ymin,
                                                         int xmax,
                                                         int ymax)
        {
            var selectedOutlines = new List<List<Vertex>>();

            foreach (List<Vertex> outline in outlines)
            {
                var bs = new byte[outline.Count];

                for (int j = 0; j < outline.Count; j++)
                {
                    Vertex vertex = outline[j];
                    byte b;

                    double x = vertex.X;
                    if (x < xmin)
                    {
                        b = 1;
                    }
                    else if (x > xmax)
                    {
                        b = 2;
                    }
                    else
                    {
                        b = 0;
                    }

                    double y = vertex.Y;
                    if (y < ymin)
                    {
                        b |= 4;
                    }
                    else if (y > ymax)
                    {
                        b |= 8;
                    }

                    bs[j] = b;
                }

                int r = bs[0];
                for (int j = 1; j < bs.Length; j++)
                {
                    int b = bs[j];
                    r &= b;
                }

                if (r == 0)
                {
                    selectedOutlines.Add(outline);
                }
            }
            return selectedOutlines;
        }

        private static void SmoothPath(List<Node> nodePath, List<Edge> edgePath, out List<Node> resultNodePath)
        {
            resultNodePath = new List<Node>();

            if (nodePath == null || nodePath.Count <= 0 ||
                edgePath == null || edgePath.Count <= 0 ||
                nodePath.Count != edgePath.Count + 1)
            {
                return;
            }

            var nodesList = new List<List<Node>>();
            var edgesList = new List<List<Edge>>();
            var orientations = new List<Orientation>();

            for (int i = 0; i < edgePath.Count; i++)
            {
                Edge currentEdge = edgePath[i];

                var nodes = new List<Node> {nodePath[i]};
                nodesList.Add(nodes);

                var edges = new List<Edge> {currentEdge};
                edgesList.Add(edges);

                orientations.Add(currentEdge.Orientation);

                for (i = i + 1; i < edgePath.Count; i++)
                {
                    Edge nextEdge = edgePath[i];
                    if (nextEdge.Orientation == currentEdge.Orientation)
                    {
                        nodes.Add(nodePath[i]);
                        edges.Add(nextEdge);
                    }
                    else
                    {
                        nodes.Add(nodePath[i]);
                        i--;
                        break;
                    }
                }
            }

            nodesList.Last().Add(nodePath.Last());

            for (int i = 0; i < nodesList.Count - 3; i++)
            {
                int j = i + 3;

                Node a1, b1;
                {
                    List<Node> nodes = nodesList[i];

                    a1 = nodes.First();
                    b1 = nodes.Last();
                }

                Node a2, b2;
                {
                    List<Node> nodes = nodesList[j];

                    a2 = nodes.First();
                    b2 = nodes.Last();
                }

                int xa, ya;

                Orientation o1 = orientations[i];
                Orientation o2 = orientations[j];

                if (o1 == o2)
                {
                    continue;
                }

                switch (o1)
                {
                    case Orientation.Horizontal:
                        if (a1.A < b1.A)
                        {
                            xa = a2.A - b1.A;
                            if (xa < 0)
                            {
                                continue;
                            }

                            ya = a2.O - b1.O;
                            int yb = b2.O - b1.O;

                            if (ya*yb < 0)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            xa = a2.A - b1.A;
                            if (xa > 0)
                            {
                                continue;
                            }

                            ya = a2.O - b1.O;
                            int yb = b2.O - b1.O;

                            if (ya*yb < 0)
                            {
                                continue;
                            }
                        }
                        break;

                    case Orientation.Vertical:
                        if (a1.O < b1.O)
                        {
                            ya = a2.O - b1.O;
                            if (ya < 0)
                            {
                                continue;
                            }

                            xa = a2.A - b1.A;
                            int xb = b2.A - b1.A;

                            if (xa*xb < 0)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            ya = a2.O - b1.O;
                            if (ya > 0)
                            {
                                continue;
                            }

                            xa = a2.A - b1.A;
                            int xb = b2.A - b1.A;

                            if (xa*xb < 0)
                            {
                                continue;
                            }
                        }
                        break;

                    default:
                        continue;
                }

                if (xa == 0 || ya == 0)
                {
                    continue;
                }

                int upper1, upper2;
                if (o1 == Orientation.Horizontal)
                {
                    upper1 = Math.Abs(xa);
                    upper2 = Math.Abs(ya);
                }
                else
                {
                    upper1 = Math.Abs(ya);
                    upper2 = Math.Abs(xa);
                }

                var nodes1 = new List<Node>();
                var edges1 = new List<Edge>();

                {
                    Edge previousEdge = edgesList[i].Last();
                    Node currentNode = b1;

                    for (int k = 0; k < upper1; k++)
                    {
                        bool exit = true;
                        foreach (Edge edge in currentNode.IncidentEdges)
                        {
                            if (edge != previousEdge && edge.Orientation == previousEdge.Orientation)
                            {
                                previousEdge = edge;
                                currentNode = edge.OtherNode(currentNode);

                                edges1.Add(edge);
                                nodes1.Add(currentNode);

                                exit = false;
                                break;
                            }
                        }

                        if (exit)
                        {
                            break;
                        }
                    }
                }

                if (nodes1.Count == upper1)
                {
                    var nodes2 = new List<Node>();
                    var edges2 = new List<Edge>();

                    Edge previousEdge = edgesList[j].First();
                    Node currentNode = a2;

                    for (int k = 0; k < upper2; k++)
                    {
                        bool exit = true;
                        foreach (Edge edge in currentNode.IncidentEdges)
                        {
                            if (edge != previousEdge && edge.Orientation == previousEdge.Orientation)
                            {
                                previousEdge = edge;
                                currentNode = edge.OtherNode(currentNode);

                                edges2.Add(edge);
                                nodes2.Add(currentNode);

                                exit = false;
                                break;
                            }
                        }

                        if (exit)
                        {
                            break;
                        }
                    }

                    if (nodes2.Count == upper2 && nodes1.Last() == nodes2.Last())
                    {
                        int oldWeight = 0;
                        foreach (Edge edge in edgesList[i + 1])
                        {
                            oldWeight += edge.Weight;
                        }
                        foreach (Edge edge in edgesList[i + 2])
                        {
                            oldWeight += edge.Weight;
                        }

                        int newWeight = 0;
                        foreach (Edge edge in edges1)
                        {
                            newWeight += edge.Weight;
                        }
                        foreach (Edge edge in edges2)
                        {
                            newWeight += edge.Weight;
                        }

                        if (newWeight <= oldWeight)
                        {
                            nodesList[i].AddRange(nodes1);

                            nodes2.Reverse();
                            nodes2.AddRange(nodesList[j]);
                            nodesList[j] = nodes2;

                            int removedIndex = i + 1;

                            nodesList.RemoveAt(removedIndex);
                            nodesList.RemoveAt(removedIndex);

                            edgesList[i].AddRange(edges1);

                            edges2.Reverse();
                            edges2.AddRange(edgesList[j]);
                            edgesList[j] = edges2;

                            edgesList.RemoveAt(removedIndex);
                            edgesList.RemoveAt(removedIndex);

                            i--;
                        }
                    }
                }
            }

            resultNodePath.Add(nodesList[0][0]);
            foreach (List<Node> nodes in nodesList)
            {
                for (int i = 1; i < nodes.Count; i++)
                {
                    resultNodePath.Add(nodes[i]);
                }
            }
        }

        #endregion
    }
}