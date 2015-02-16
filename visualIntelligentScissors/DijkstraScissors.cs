using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Collections;
using Priority_Queue;

namespace VisualIntelligentScissors
{
    /*
     * My HeapPriorityQueue is a high speed priority queue provided by https://bitbucket.org/BlueRaja/high-speed-priority-queue-for-c/wiki/
     */
    public class DijkstraScissors : Scissors
    {
        //This is a blue colored pen named "yellowpen". You're just going to have to deal with it...
        Pen yellowpen = new Pen(Color.Blue);

        //Node class for the Priority Queue Node. Required by the HeapPriorityQueue documentation
        //point- The pixel we are at
        //weight- The weight of the pixel from the previous pixel
        //previous- The previous pixel from where it came
        public class Node : PriorityQueueNode
        {
            public Point point { get; set; }
            public int weight { get; set; }
            public Node previous { get; set; }
            public Node(Point point, int weight, Node previous)
            {
                this.point = point;
                this.weight = weight;
                this.previous = previous;
            }
        }


        public DijkstraScissors() { }
        /// <summary>
        /// constructor for intelligent scissors. 
        /// </summary>
        /// <param name="image">the image you are oging to segment.  has methods for getting gradient information</param>
        /// <param name="overlay">an overlay on which you can draw stuff by setting pixels.</param>
        public DijkstraScissors(GrayBitmap image, Bitmap overlay) : base(image, overlay) { }

        // this is the class you need to implement in CS 312

        /// <summary>
        /// this is the class you implement in CS 312. 
        /// </summary>
        /// <param name="points">these are the segmentation points from the pgm file.</param>
        /// <param name="pen">this is a pen you can use to draw on the overlay</param>
        public override void FindSegmentation(IList<Point> points, Pen pen)
        {
            if (Image == null) throw new InvalidOperationException("Set Image property first.");
            // this is the entry point for this class when the button is clicked

            //Create a Priority Queue the size of the height and width to account for every pixel
            HeapPriorityQueue<Node> priorityQueue = new HeapPriorityQueue<Node>(Overlay.Width * Overlay.Height);
            
            //Color the Starting Points that are given
            ColorStartingPoints(points);

            //For every point, find the path to that point. points[(i + 1) % points.Count] is to make sure we wrap around to the first point
            for (int i = 0; i < points.Count; i++)
            {
                PathToPoints(priorityQueue, points[i], points[(i + 1) % points.Count]);
            }
        }

        //start- the starting point
        //end- the point to end at
        private void PathToPoints(HeapPriorityQueue<Node> priorityQueue, Point start, Point end)
        {
            HashSet<Point> queue = new HashSet<Point>();

            //Queue the starting node. Its weight is 0
            //Because the priority Queue .Contains() matches based on exact nodes, and the information within the node could be different (although we could be at the same position)
            //I found it easier to add the point to a HashSet and remove it when the node is removed from the PriorityQueue. That way I can search the contents of the HashSet in O(1) time
            //.Add(), .Remove(), and .Contains() is all in O(1) time in a HashSet, meaning it doesn't add any complexity besides a constant
            priorityQueue.Enqueue(new Node(start, 0, null), 0);
            queue.Add(start);
            Boolean foundGoal = false;
            HashSet<Point> settledSet = new HashSet<Point>();
            Node finalNode = null;

            //While we haven't found the goal and the priorityQueue isn't empty
            while (priorityQueue.Count != 0 && !foundGoal)
            {
                //Take the top priority node off the queue
                Node priorityNode = priorityQueue.Dequeue();
                queue.Remove(priorityNode.point);

                //Add that node to the settled set
                settledSet.Add(priorityNode.point);

                //Get the neighbors of that pixel
                List<Node> neighbors = GetNeighborNodes(priorityNode);

                foreach (Node neighbor in neighbors)
                {
                    //If the neighbor is equal to the end point, we've found our goal and we want to exit and remember that neighbor.
                    if (neighbor.point == end)
                    {
                        finalNode = neighbor;
                        foundGoal = true;
                    }
                    else if (!settledSet.Contains(neighbor.point) && !queue.Contains(neighbor.point))
                    {
                        //If the settled set doesn't contain the neighbor and the queue doesn't contain it either, let's add it to the queue and make its priority the weight
                        priorityQueue.Enqueue(neighbor, neighbor.weight);
                        queue.Add(neighbor.point);
                    }
                }
            }

            //Clear the queue, draw the points based on the trace of previous nodes until you get to null, and start on the next point
            priorityQueue.Clear();
            DrawNodeTrace(finalNode);
        }

        //Each node can have 4 neighbors- N, S, E, W
        //Create the neighbors and add them to the list as long as they're within the picture
        //We add N, E, S, W in order in case of clockwise weight collisions
        //priorityNode- the node we are going to find neighbors on
        //returns: A list of neighbor nodes
        private List<Node> GetNeighborNodes(Node priorityNode)
        {
            List<Node> neighbors = new List<Node>();

            Node nNode = new Node(new Point(priorityNode.point.X, priorityNode.point.Y - 1), this.GetPixelWeight(new Point(priorityNode.point.X, priorityNode.point.Y - 1)) + priorityNode.weight, priorityNode);
            Node eNode = new Node(new Point(priorityNode.point.X + 1, priorityNode.point.Y), this.GetPixelWeight(new Point(priorityNode.point.X + 1, priorityNode.point.Y)) + priorityNode.weight, priorityNode);
            Node sNode = new Node(new Point(priorityNode.point.X, priorityNode.point.Y + 1), this.GetPixelWeight(new Point(priorityNode.point.X, priorityNode.point.Y + 1)) + priorityNode.weight, priorityNode);
            Node wNode = new Node(new Point(priorityNode.point.X - 1, priorityNode.point.Y), this.GetPixelWeight(new Point(priorityNode.point.X - 1, priorityNode.point.Y)) + priorityNode.weight, priorityNode);

            if (WithinPicture(nNode.point))
            {
                neighbors.Add(nNode);
            }
            if (WithinPicture(eNode.point))
            {
                neighbors.Add(eNode);
            }
            if (WithinPicture(sNode.point))
            {
                neighbors.Add(sNode);
            }
            if (WithinPicture(wNode.point))
            {
                neighbors.Add(wNode);
            }

            return neighbors;
        }

        //point- the point to check whether or not it's in the image
        //returns: a boolean based on whether or not the point is within the image
        private Boolean WithinPicture(Point point)
        {
            return (point.X < (Overlay.Width - 2) && point.Y < (Overlay.Height - 2) && point.X > 1 && point.Y > 1);
        }

        //points- The list of starting points
        //Draws a circle around the starting points for convenience
        private void ColorStartingPoints(IList<Point> points)
        {
            using (Graphics g = Graphics.FromImage(Overlay))
            {
                for (int i = 0; i < points.Count; i++)
                {
                    Point start = points[i];
                    g.DrawEllipse(yellowpen, start.X, start.Y, 5, 5);
                }
                Program.MainForm.RefreshImage();
            }
        }

        //finalNode- the last node in a trace of nodes that lead back to the original 
        //Until the previous node is null, recursively draw pixels on the current node
        private void DrawNodeTrace(Node finalNode)
        {
            Overlay.SetPixel(finalNode.point.X, finalNode.point.Y, Color.Red);
            if (finalNode.previous != null)
            {
                DrawNodeTrace(finalNode.previous);
            }
        }
    }
}
