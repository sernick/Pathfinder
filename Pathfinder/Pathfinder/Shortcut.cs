using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using Pathfinder.GraphTheory;


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

        private static List<List<Vertex>> CorrectOutlines(List<List<Vertex>> outlines, Vertex start, Vertex end)
        {
            int xMin, xMax;
            if (start.X < end.X)
            {
                xMin = start.X;
                xMax = end.X;
            }
            else
            {
                xMin = end.X;
                xMax = start.X;
            }

            int yMin, yMax;
            if (start.Y < end.Y)
            {
                yMin = start.Y;
                yMax = end.Y;
            }
            else
            {
                yMin = end.Y;
                yMax = start.Y;
            }

            List<List<Vertex>> selectedOutlines = SelectOutlines(outlines, xMin, yMin, xMax, yMax);

            foreach (List<Vertex> outline in selectedOutlines)
            {
                foreach (Vertex vertex in outline)
                {
                    int x = vertex.X;
                    if (x < xMin)
                    {
                        xMin = x;
                    }
                    else if (x > xMax)
                    {
                        xMax = x;
                    }

                    int y = vertex.Y;
                    if (y < yMin)
                    {
                        yMin = y;
                    }
                    else if (y > yMax)
                    {
                        yMax = y;
                    }
                }
            }

            return SelectOutlines(outlines, xMin, yMin, xMax, yMax);
        }

        private static void DivideSegments(List<int[]> segments)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                int[] a = segments[i];
                int ya = a[0];

                int amin = a[1];
                int amax = a[2];

                if (amin == amax)
                {
                    segments.RemoveAt(i);
                    i--;
                    continue;
                }

                for (int j = i + 1; j < segments.Count; j++)
                {
                    int[] b = segments[j];
                    int yb = b[0];

                    if (ya == yb)
                    {
                        int bmin = b[1];
                        int bmax = b[2];

                        if (bmin <= amax && bmax >= amin)
                        {
                            if (bmin == amin)
                            {
                                if (bmax == amin)
                                {
                                    segments.RemoveAt(j);
                                    j--;
                                }
                                if (bmax == amax)
                                {
                                    segments.RemoveAt(j);
                                    j--;
                                }
                                else if (bmax > amax)
                                {
                                    b[1] = amax;
                                }
                                else
                                {
                                    amin = a[1] = bmax;
                                }
                            }
                            else if (bmin == amax)
                            {
                                if (bmax == amax)
                                {
                                    segments.RemoveAt(j);
                                    j--;
                                }
                            }
                            else if (bmin < amin)
                            {
                                if (bmax == amin)
                                {
                                }
                                else if (bmax == amax)
                                {
                                    b[2] = amin;
                                }
                                else if (bmax > amax)
                                {
                                    segments.Add(new[] {ya, amax, bmax});
                                    b[2] = amin;
                                }
                                else if (bmax < amax)
                                {
                                    segments.Add(new[] {ya, bmax, amax});
                                    b[2] = amin;
                                    amax = a[2] = bmax;
                                }
                            }
                            else if (bmin > amin)
                            {
                                if (bmax == amax)
                                {
                                    amax = a[2] = bmin;
                                }
                                else if (bmax > amax)
                                {
                                    segments.Add(new[] {ya, amax, bmax});
                                    b[2] = amax;
                                    amax = a[2] = bmin;
                                }
                                else
                                {
                                    segments.Add(new[] {ya, bmax, amax});
                                    amax = a[2] = bmin;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void GetCoordinatesAndCorrectCorridors(Vertex start,
                                                              Vertex end,
                                                              List<List<Vertex>> outlines,
                                                              List<int[]> horizontalCorridors,
                                                              List<int[]> verticalCorridors,
                                                              out HashSet<int> xs,
                                                              out HashSet<int> ys)
        {
            int xMin, xMax;
            if (start.X < end.X)
            {
                xMin = start.X;
                xMax = end.X;
            }
            else
            {
                xMin = end.X;
                xMax = start.X;
            }

            int yMin, yMax;
            if (start.Y < end.Y)
            {
                yMin = start.Y;
                yMax = end.Y;
            }
            else
            {
                yMin = end.Y;
                yMax = start.Y;
            }

            xs = new HashSet<int>(new[] {start.X, end.X});
            ys = new HashSet<int>(new[] {start.Y, end.Y});

            foreach (List<Vertex> outline in outlines)
            {
                foreach (Vertex vertex in outline)
                {
                    int x = vertex.X;
                    int y = vertex.Y;

                    xs.Add(x);
                    ys.Add(y);

                    if (x < xMin)
                    {
                        xMin = x;
                    }
                    else if (x > xMax)
                    {
                        xMax = x;
                    }

                    if (y < yMin)
                    {
                        yMin = y;
                    }
                    else if (y > yMax)
                    {
                        yMax = y;
                    }
                }
            }

            for (int i = 0; i < horizontalCorridors.Count; i++)
            {
                int[] corridor = horizontalCorridors[i];

                int y1 = corridor[0];
                int y2 = corridor[1];

                int x1 = corridor[2];
                int x2 = corridor[3];

                if (y1 > yMin && y1 < yMax || y2 > yMin && y2 < yMax)
                {
                    byte b1;
                    if (x1 < xMin)
                    {
                        b1 = 1;
                    }
                    else if (x1 > xMax)
                    {
                        b1 = 2;
                    }
                    else
                    {
                        b1 = 0;
                    }

                    byte b2;
                    if (x2 < xMin)
                    {
                        b2 = 1;
                    }
                    else if (x2 > xMax)
                    {
                        b2 = 2;
                    }
                    else
                    {
                        b2 = 0;
                    }

                    if ((b1 & b2) == 0)
                    {
                        ys.Add(y1);
                        ys.Add(y2);

                        if (b1 == 1)
                        {
                            corridor[2] = xMin;
                        }
                        else
                        {
                            xs.Add(x1);
                        }

                        if (b2 == 2)
                        {
                            corridor[3] = xMax;
                        }
                        else
                        {
                            xs.Add(x2);
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
                int[] corridor = verticalCorridors[i];

                int x1 = corridor[0];
                int x2 = corridor[1];

                int y1 = corridor[2];
                int y2 = corridor[3];

                if (x1 > xMin && x1 < xMax || x2 > xMin && x2 < xMax)
                {
                    byte b1;
                    if (y1 < yMin)
                    {
                        b1 = 1;
                    }
                    else if (y1 > yMax)
                    {
                        b1 = 2;
                    }
                    else
                    {
                        b1 = 0;
                    }

                    byte b2;
                    if (y2 < yMin)
                    {
                        b2 = 1;
                    }
                    else if (y2 > yMax)
                    {
                        b2 = 2;
                    }
                    else
                    {
                        b2 = 0;
                    }

                    if ((b1 & b2) == 0)
                    {
                        xs.Add(x1);
                        xs.Add(x2);

                        if (b1 == 1)
                        {
                            corridor[2] = yMin;
                        }
                        else
                        {
                            ys.Add(y1);
                        }

                        if (b2 == 2)
                        {
                            corridor[3] = yMax;
                        }
                        else
                        {
                            ys.Add(y2);
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

        private static void GetCorridors(List<double[]> horizontalSections,
                                         List<double[]> verticalSections,
                                         int offset,
                                         out List<int[]> horizontalCorridors,
                                         out List<int[]> verticalCorridors)
        {
            horizontalCorridors = new List<int[]>();
            verticalCorridors = new List<int[]>();

            foreach (double[] section in horizontalSections)
            {
                double y = section[0];
                double xMin = section[1];
                double xMax = section[2];

                var corridor = new[]
                               {
                                   (int) Math.Floor(y) - offset,
                                   (int) Math.Ceiling(y) + offset,
                                   (int) Math.Floor(xMin) - offset,
                                   (int) Math.Ceiling(xMax) + offset
                               };
                horizontalCorridors.Add(corridor);
            }

            foreach (double[] section in verticalSections)
            {
                double x = section[0];
                double yMin = section[1];
                double yMax = section[2];

                var corridor = new[]
                               {
                                   (int) Math.Floor(x) - offset,
                                   (int) Math.Ceiling(x) + offset,
                                   (int) Math.Floor(yMin) - offset,
                                   (int) Math.Ceiling(yMax) + offset
                               };
                verticalCorridors.Add(corridor);
            }
        }

        private static List<List<Vertex>> GetOutlines(List<int[]> rectangles)
        {
            var hs = new List<int[]>();
            var vs = new List<int[]>();

            foreach (int[] rectangle in rectangles)
            {
                int x1 = rectangle[0];
                int y1 = rectangle[1];
                int x2 = rectangle[2];
                int y2 = rectangle[3];

                hs.Add(new[] {y1, x1, x2});
                hs.Add(new[] {y2, x1, x2});

                vs.Add(new[] {x1, y1, y2});
                vs.Add(new[] {x2, y1, y2});
            }

            DivideSegments(hs);
            DivideSegments(vs);

            var verticals = new Dictionary<int, List<int[]>>();
            foreach (int[] segment in vs)
            {
                int key = segment[0];
                if (verticals.ContainsKey(key))
                {
                    verticals[key].Add(segment);
                }
                else
                {
                    verticals[key] = new List<int[]> {segment};
                }
            }

            for (int i = 0; i < hs.Count; i++)
            {
                int[] horizontal = hs[i];

                int y = horizontal[0];
                int xMin = horizontal[1];
                int xMax = horizontal[2];

                foreach (int x in new[] {xMin, xMax})
                {
                    if (verticals.ContainsKey(x))
                    {
                        List<int[]> segments = verticals[x];
                        for (int j = 0; j < segments.Count; j++)
                        {
                            int[] vertical = segments[j];

                            int yMin = vertical[1];
                            int yMax = vertical[2];

                            if (y > yMin && y < yMax)
                            {
                                segments.Add(new[] {x, y, yMax});
                                vertical[2] = y;
                            }
                        }
                    }
                }

                for (int x = xMin + 1; x < xMax; x++)
                {
                    if (verticals.ContainsKey(x))
                    {
                        List<int[]> segments = verticals[x];
                        for (int j = 0; j < segments.Count; j++)
                        {
                            int[] vertical = segments[j];

                            int yMin = vertical[1];
                            int yMax = vertical[2];

                            if (y == yMin || y == yMax)
                            {
                                hs.Add(new[] {y, x, xMax});
                                xMax = horizontal[2] = x;
                                break;
                            }
                            if (y > yMin && y < yMax)
                            {
                                segments.Add(new[] {x, y, yMax});
                                vertical[2] = y;

                                hs.Add(new[] {y, x, xMax});
                                xMax = horizontal[2] = x;
                                break;
                            }
                        }
                    }
                }
            }

            var horizontals = new Dictionary<int, List<int[]>>();
            foreach (int[] segment in hs)
            {
                int key = segment[0];
                if (horizontals.ContainsKey(key))
                {
                    horizontals[key].Add(segment);
                }
                else
                {
                    horizontals[key] = new List<int[]> {segment};
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
                List<int[]> segments = horizontals[y];
                segments.Sort((a, b) => a[1].CompareTo(b[1]));

                var xs = new Dictionary<int, Vertex>();

                int previous;
                Vertex previousVertex;
                Vertex[] previousRay;
                {
                    int[] segment = segments[0];

                    int min = segment[1];
                    int max = segment[2];

                    var minVertex = new Vertex(min, y);
                    xs.Add(min, minVertex);

                    var maxVertex = new Vertex(max, y);
                    xs.Add(max, maxVertex);

                    rays[minVertex] = new[] {maxVertex, null, null, null};

                    var maxRay = new[] {null, null, minVertex, null};
                    rays[maxVertex] = maxRay;

                    previous = max;
                    previousVertex = maxVertex;
                    previousRay = maxRay;
                }

                for (int i = 1; i < segments.Count; i++)
                {
                    int[] segment = segments[i];

                    int min = segment[1];
                    int max = segment[2];

                    var maxVertex = new Vertex(max, y);

                    Vertex minVertex;
                    if (min == previous)
                    {
                        minVertex = previousVertex;
                        previousRay[0] = maxVertex;
                    }
                    else
                    {
                        minVertex = new Vertex(min, y);
                        xs.Add(min, minVertex);

                        rays[minVertex] = new[] {maxVertex, null, null, null};
                    }

                    xs.Add(max, maxVertex);

                    var maxRay = new[] {null, null, minVertex, null};
                    rays[maxVertex] = maxRay;

                    previous = max;
                    previousVertex = maxVertex;
                    previousRay = maxRay;
                }

                vertices.Add(y, xs);
            }

            foreach (int x in verticalKeys)
            {
                List<int[]> segments = verticals[x];
                segments.Sort((a, b) => a[1].CompareTo(b[1]));

                foreach (int[] segment in segments)
                {
                    int yMin = segment[1];
                    int yMax = segment[2];

                    Vertex minVertex;
                    if (vertices.ContainsKey(yMin))
                    {
                        Dictionary<int, Vertex> xs = vertices[yMin];
                        if (xs.ContainsKey(x))
                        {
                            minVertex = vertices[yMin][x];
                        }
                        else
                        {
                            minVertex = new Vertex(x, yMin);
                            xs[x] = minVertex;
                        }
                    }
                    else
                    {
                        minVertex = new Vertex(x, yMin);
                        vertices[yMin] = new Dictionary<int, Vertex> {{x, minVertex}};
                    }

                    Vertex maxVertex;
                    if (vertices.ContainsKey(yMax))
                    {
                        Dictionary<int, Vertex> xs = vertices[yMax];
                        if (xs.ContainsKey(x))
                        {
                            maxVertex = vertices[yMax][x];
                        }
                        else
                        {
                            maxVertex = new Vertex(x, yMax);
                            xs[x] = maxVertex;
                        }
                    }
                    else
                    {
                        maxVertex = new Vertex(x, yMax);
                        vertices[yMax] = new Dictionary<int, Vertex> {{x, maxVertex}};
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

        public static List<Point> GetPath(List<double[]> areas,
                                          List<double[]> horizontalSections,
                                          List<double[]> verticalSections,
                                          int offset,
                                          int intersectionWeight,
                                          int xStart,
                                          int yStart,
                                          int xEnd,
                                          int yEnd,
                                          bool optimize = false)
        {
            if (xStart == xEnd && yStart == yEnd || offset <= 0)
            {
                return new List<Point>();
            }

            var start = new Vertex(xStart, yStart);
            var end = new Vertex(xEnd, yEnd);

            var rectangles = new List<int[]>(areas.Count);
            foreach (double[] area in areas)
            {
                var rectangle = new[]
                                {
                                    (int) Math.Floor(area[0]) - offset,
                                    (int) Math.Floor(area[1]) - offset,
                                    (int) Math.Ceiling(area[2]) + offset,
                                    (int) Math.Ceiling(area[3]) + offset
                                };
                rectangles.Add(rectangle);
            }

            List<List<Vertex>> outlines = GetOutlines(rectangles);
            if (optimize)
            {
                outlines = CorrectOutlines(outlines, start, end);
            }

            GetCorridors(horizontalSections,
                         verticalSections,
                         offset,
                         out List<int[]> horizontalCorridors,
                         out List<int[]> verticalCorridors);

            GetCoordinatesAndCorrectCorridors(start,
                                              end,
                                              outlines,
                                              horizontalCorridors,
                                              verticalCorridors,
                                              out HashSet<int> xs,
                                              out HashSet<int> ys);

            var horizontalSides = new Dictionary<int, List<int[]>>();
            var verticalSides = new Dictionary<int, List<int[]>>();

            foreach (List<Vertex> outline in outlines)
            {
                if (outline.Count < 2)
                {
                    continue;
                }

                for (int i = 0; i < outline.Count - 1; i++)
                {
                    int j = i + 1;

                    Vertex p1 = outline[i];
                    Vertex p2 = outline[j];

                    if (p1.X == p2.X)
                    {
                        int[] side;
                        if (p1.Y < p2.Y)
                        {
                            side = new[] {p1.Y, p2.Y, 1};
                        }
                        else
                        {
                            side = new[] {p2.Y, p1.Y, -1};
                        }

                        int x = p1.X;
                        if (verticalSides.ContainsKey(x))
                        {
                            verticalSides[x].Add(side);
                        }
                        else
                        {
                            verticalSides[x] = new List<int[]> {side};
                        }
                    }
                    else
                    {
                        int[] side;
                        if (p1.X < p2.X)
                        {
                            side = new[] {p1.X, p2.X, 1};
                        }
                        else
                        {
                            side = new[] {p2.X, p1.X, -1};
                        }

                        int y = p1.Y;
                        if (horizontalSides.ContainsKey(y))
                        {
                            horizontalSides[y].Add(side);
                        }
                        else
                        {
                            horizontalSides[y] = new List<int[]> {side};
                        }
                    }
                }
            }

            foreach (List<int[]> sides in horizontalSides.Values)
            {
                sides.Sort((a, b) => a[0].CompareTo(b[0]));
            }

            foreach (List<int[]> sides in verticalSides.Values)
            {
                sides.Sort((a, b) => a[0].CompareTo(b[0]));
            }

            List<int> abscissas = xs.ToList();
            abscissas.Sort();

            List<int> ordinates = ys.ToList();
            ordinates.Sort();

            var horizontalEdges = new byte[ordinates.Count, abscissas.Count - 1];
            for (int i = 0; i < ordinates.Count; i++)
            {
                int y = ordinates[i];

                int crossing = 0;
                int previousDirection = 0;

                for (int j = 0; j < abscissas.Count - 1; j++)
                {
                    int x = abscissas[j];

                    if (verticalSides.ContainsKey(x))
                    {
                        foreach (int[] side in verticalSides[x])
                        {
                            int yMin = side[0];
                            int yMax = side[1];

                            if (y >= yMin && y < yMax || y > yMin && y <= yMax)
                            {
                                int direction = side[2];
                                if (direction != previousDirection)
                                {
                                    previousDirection = direction;
                                    crossing += direction;
                                }
                            }
                        }
                    }

                    if (crossing == 0)
                    {
                        horizontalEdges[i, j] = 1;
                    }
                }

                if (horizontalSides.ContainsKey(y))
                {
                    foreach (int[] side in horizontalSides[y])
                    {
                        int min = abscissas.IndexOf(side[0]);
                        int max = abscissas.IndexOf(side[1]);

                        for (int j = min; j < max; j++)
                        {
                            horizontalEdges[i, j] = 1;
                        }
                    }
                }
            }

            var verticalEdges = new byte[abscissas.Count, ordinates.Count - 1];
            for (int i = 0; i < abscissas.Count; i++)
            {
                int x = abscissas[i];

                int crossing = 0;
                int previousDirection = 0;

                for (int j = 0; j < ordinates.Count - 1; j++)
                {
                    int y = ordinates[j];

                    if (horizontalSides.ContainsKey(y))
                    {
                        foreach (int[] side in horizontalSides[y])
                        {
                            int xMin = side[0];
                            int xMax = side[1];

                            if (x >= xMin && x <= xMax || x > xMin && x <= xMax)
                            {
                                int direction = side[2];
                                if (direction != previousDirection)
                                {
                                    previousDirection = direction;
                                    crossing += direction;
                                }
                            }
                        }
                    }

                    if (crossing == 0)
                    {
                        verticalEdges[i, j] = 1;
                    }
                }

                if (verticalSides.ContainsKey(x))
                {
                    foreach (int[] side in verticalSides[x])
                    {
                        int min = ordinates.IndexOf(side[0]);
                        int max = ordinates.IndexOf(side[1]);

                        for (int j = min; j < max; j++)
                        {
                            verticalEdges[i, j] = 1;
                        }
                    }
                }
            }

            foreach (int[] corridor in horizontalCorridors)
            {
                int yMin = ordinates.IndexOf(corridor[0]);
                int yMax = ordinates.IndexOf(corridor[1]);

                int xMin = abscissas.IndexOf(corridor[2]);
                int xMax = abscissas.IndexOf(corridor[3]);

                for (int i = yMin + 1; i < yMax; i++)
                {
                    for (int j = xMin; j < xMax; j++)
                    {
                        horizontalEdges[i, j] = 0;
                    }
                }

                for (int i = xMin + 1; i < xMax; i++)
                {
                    if (verticalEdges[i, yMin] == 1)
                    {
                        verticalEdges[i, yMin] = 2;
                    }
                }
            }

            foreach (int[] corridor in verticalCorridors)
            {
                int xMin = abscissas.IndexOf(corridor[0]);
                int xMax = abscissas.IndexOf(corridor[1]);

                int yMin = ordinates.IndexOf(corridor[2]);
                int yMax = ordinates.IndexOf(corridor[3]);

                for (int i = xMin + 1; i < xMax; i++)
                {
                    for (int j = yMin; j < yMax; j++)
                    {
                        verticalEdges[i, j] = 0;
                    }
                }

                for (int i = yMin + 1; i < yMax; i++)
                {
                    if (horizontalEdges[i, xMin] == 1)
                    {
                        horizontalEdges[i, xMin] = 2;
                    }
                }
            }

            var nodes = new bool[abscissas.Count, ordinates.Count];

            {
                int j = ordinates.Count - 1;
                for (int i = 0; i < abscissas.Count; i++)
                {
                    nodes[i, 0] = true;
                    nodes[i, j] = true;
                }
            }

            {
                int i = abscissas.Count - 1;
                for (int j = 0; j < ordinates.Count; j++)
                {
                    nodes[0, j] = true;
                    nodes[i, j] = true;
                }
            }

            {
                int iUpper = horizontalEdges.GetLength(0) - 1;
                int jUpper = horizontalEdges.GetLength(1) - 1;

                for (int i = 0; i < iUpper; i++)
                {
                    for (int j = 0; j < jUpper; j++)
                    {
                        int k = j + 1;

                        byte current = horizontalEdges[i, j];
                        byte next = horizontalEdges[i, k];

                        if (current > 0 || next > 0)
                        {
                            nodes[k, i] = true;
                        }
                    }
                }
            }

            {
                int iUpper = verticalEdges.GetLength(0) - 1;
                int jUpper = verticalEdges.GetLength(1) - 1;

                for (int i = 0; i < iUpper; i++)
                {
                    for (int j = 0; j < jUpper; j++)
                    {
                        int k = j + 1;

                        if (!nodes[i, k])
                        {
                            byte current = verticalEdges[i, j];
                            byte next = verticalEdges[i, k];

                            if (current > 0 || next > 0)
                            {
                                nodes[i, k] = true;
                            }
                        }
                    }
                }
            }

            foreach (KeyValuePair<int, List<int[]>> pair in horizontalSides)
            {
                int j = ordinates.IndexOf(pair.Key);
                foreach (int[] side in pair.Value)
                {
                    int min = abscissas.IndexOf(side[0]);
                    int max = abscissas.IndexOf(side[1]);

                    for (int i = min; i <= max; i++)
                    {
                        nodes[i, j] = true;
                    }
                }
            }

            foreach (KeyValuePair<int, List<int[]>> pair in verticalSides)
            {
                int i = abscissas.IndexOf(pair.Key);
                foreach (int[] side in pair.Value)
                {
                    int min = ordinates.IndexOf(side[0]);
                    int max = ordinates.IndexOf(side[1]);

                    for (int j = min; j <= max; j++)
                    {
                        nodes[i, j] = true;
                    }
                }
            }

            var horizontalWeights = new int[abscissas.Count - 1];
            for (int i = 0; i < abscissas.Count - 1; i++)
            {
                horizontalWeights[i] = abscissas[i + 1] - abscissas[i];
            }

            var verticalWeights = new int[ordinates.Count - 1];
            for (int i = 0; i < ordinates.Count - 1; i++)
            {
                verticalWeights[i] = ordinates[i + 1] - ordinates[i];
            }

            int iLength = nodes.GetLength(0);
            int jLength = nodes.GetLength(1);

            var graph = new Graph(iLength, jLength);

            graph.AddNode(0, 0);
            graph.AddNode(0, jLength - 1);

            for (int i = 1; i < iLength; i++)
            {
                int iPrevious = i - 1;
                int weight = horizontalWeights[iPrevious];

                {
                    const int jFirst = 0;

                    Node currentNode = graph.AddNode(i, jFirst);
                    Node previousNode = graph[iPrevious, jFirst];

                    Node.Connect(previousNode, currentNode, weight, Orientation.Horizontal, graph);
                }

                {
                    int jLast = jLength - 1;

                    Node currentNode = graph.AddNode(i, jLast);
                    Node previousNode = graph[iPrevious, jLast];

                    Node.Connect(previousNode, currentNode, weight, Orientation.Horizontal, graph);
                }
            }

            int iPenult = iLength - 2;
            int jPenult = jLength - 2;

            {
                for (int j = 1; j <= jPenult; j++)
                {
                    int jPrevious = j - 1;
                    int weight = verticalWeights[jPrevious];

                    {
                        const int i = 0;

                        Node currentNode = graph.AddNode(i, j);
                        Node previousNode = graph[i, jPrevious];

                        Node.Connect(previousNode, currentNode, weight, Orientation.Vertical, graph);

                        if (j == jPenult)
                        {
                            int jNext = j + 1;
                            Node nextNode = graph[i, jNext];

                            Node.Connect(currentNode, nextNode, verticalWeights[j], Orientation.Vertical, graph);
                        }
                    }

                    {
                        int i = iLength - 1;

                        Node currentNode = graph.AddNode(i, j);
                        Node previousNode = graph[i, jPrevious];

                        Node.Connect(previousNode, currentNode, weight, Orientation.Vertical, graph);

                        if (j == jPenult)
                        {
                            int jNext = j + 1;
                            Node nextNode = graph[i, jNext];

                            Node.Connect(currentNode, nextNode, verticalWeights[j], Orientation.Vertical, graph);
                        }
                    }
                }
            }

            for (int j = 1; j <= jPenult; j++)
            {
                for (int i = 1; i <= iPenult; i++)
                {
                    bool node = nodes[i, j];
                    if (node)
                    {
                        Node currentNode = graph.AddNode(i, j);
                        int iPrevious = i - 1;

                        {
                            byte edge = horizontalEdges[j, iPrevious];
                            if (edge > 0)
                            {
                                Node previousNode = graph[iPrevious, j];

                                int weight = horizontalWeights[iPrevious];
                                if (edge == 2)
                                {
                                    weight += intersectionWeight;
                                }

                                Node.Connect(previousNode, currentNode, weight, Orientation.Horizontal, graph);
                            }
                        }

                        if (i == iPenult)
                        {
                            byte edge = horizontalEdges[j, i];
                            if (edge > 0)
                            {
                                int iNext = i + 1;
                                Node nextNode = graph[iNext, j];

                                int weight = horizontalWeights[i];
                                if (edge == 2)
                                {
                                    weight += intersectionWeight;
                                }

                                Node.Connect(currentNode, nextNode, weight, Orientation.Horizontal, graph);
                            }
                        }
                    }
                }
            }

            for (int i = 1; i <= iPenult; i++)
            {
                for (int j = 1; j <= jPenult; j++)
                {
                    Node currentNode = graph[i, j];
                    if (currentNode != null)
                    {
                        int jPrevious = j - 1;

                        {
                            byte edge = verticalEdges[i, jPrevious];
                            if (edge > 0)
                            {
                                Node previousNode = graph[i, jPrevious];

                                int weight = verticalWeights[jPrevious];
                                if (edge == 2)
                                {
                                    weight += intersectionWeight;
                                }

                                Node.Connect(previousNode, currentNode, weight, Orientation.Vertical, graph);
                            }
                        }

                        if (j == jPenult)
                        {
                            byte edge = verticalEdges[i, j];
                            if (edge > 0)
                            {
                                int jNext = j + 1;
                                Node nextNode = graph[i, jNext];

                                int weight = verticalWeights[j];
                                if (edge == 2)
                                {
                                    weight += intersectionWeight;
                                }

                                Node.Connect(currentNode, nextNode, weight, Orientation.Vertical, graph);
                            }
                        }
                    }
                }
            }

            Node startNode;
            {
                int i = abscissas.IndexOf(start.X);
                int j = ordinates.IndexOf(start.Y);

                if (i >= 0 && j >= 0)
                {
                    startNode = graph[i, j];
                }
                else
                {
                    startNode = null;
                }
            }

            Node endNode;
            {
                int i = abscissas.IndexOf(end.X);
                int j = ordinates.IndexOf(end.Y);

                if (i >= 0 && j >= 0)
                {
                    endNode = graph[i, j];
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
                            int x = abscissas[node.X];
                            int y = ordinates[node.Y];

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

        private static bool IsPointInPolygon(Vertex point, List<Vertex> polygon)
        {
            Vertex p0 = polygon[0];

            double xMin = p0.X;
            double xMax = p0.X;
            double yMin = p0.Y;
            double yMax = p0.Y;

            for (int i = 1; i < polygon.Count; i++)
            {
                Vertex p = polygon[i];

                if (p.X < xMin)
                {
                    xMin = p.X;
                }
                else if (p.X > xMax)
                {
                    xMax = p.X;
                }

                if (p.Y < yMin)
                {
                    yMin = p.Y;
                }
                else if (p.Y > yMax)
                {
                    yMax = p.Y;
                }
            }

            if (point.X < xMin || point.X > xMax || point.Y < yMin || point.Y > yMax)
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
                                                         int xMin,
                                                         int yMin,
                                                         int xMax,
                                                         int yMax)
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
                    if (x < xMin)
                    {
                        b = 1;
                    }
                    else if (x > xMax)
                    {
                        b = 2;
                    }
                    else
                    {
                        b = 0;
                    }

                    double y = vertex.Y;
                    if (y < yMin)
                    {
                        b |= 4;
                    }
                    else if (y > yMax)
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
                        if (a1.X < b1.X)
                        {
                            xa = a2.X - b1.X;
                            if (xa < 0)
                            {
                                continue;
                            }

                            ya = a2.Y - b1.Y;
                            int yb = b2.Y - b1.Y;

                            if (ya*yb < 0)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            xa = a2.X - b1.X;
                            if (xa > 0)
                            {
                                continue;
                            }

                            ya = a2.Y - b1.Y;
                            int yb = b2.Y - b1.Y;

                            if (ya*yb < 0)
                            {
                                continue;
                            }
                        }
                        break;

                    case Orientation.Vertical:
                        if (a1.Y < b1.Y)
                        {
                            ya = a2.Y - b1.Y;
                            if (ya < 0)
                            {
                                continue;
                            }

                            xa = a2.X - b1.X;
                            int xb = b2.X - b1.X;

                            if (xa*xb < 0)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            ya = a2.Y - b1.Y;
                            if (ya > 0)
                            {
                                continue;
                            }

                            xa = a2.X - b1.X;
                            int xb = b2.X - b1.X;

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