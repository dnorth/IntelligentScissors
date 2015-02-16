using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Collections;
using Priority_Queue;

namespace VisualIntelligentScissors
{
	public class DijkstraScissors : Scissors
	{
        Pen yellowpen = new Pen(Color.Blue);

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
            // to do the image segmentation with intelligent scissors.

            ColorStartingPoints(points);

            for (int i = 0; i < points.Count; i++)
            {
                PathToPoints(points[i], points[(i + 1) % points.Count]);
            }
		}

        private void PathToPoints(Point start, Point end)
        {
            HeapPriorityQueue<Node> priorityQueue = new HeapPriorityQueue<Node>(Overlay.Width * Overlay.Height);
            List<Point> queue = new List<Point>();

            //Queue the starting node. Its weight is 0
            priorityQueue.Enqueue(new Node(start, 0, null), 0);
            Boolean foundGoal = false;
            List<Point> settledSet = new List<Point>();
            Node finalNode = null;

            while (priorityQueue.Count != 0 && !foundGoal)
            {
                Node priorityNode = priorityQueue.Dequeue();
                settledSet.Add(priorityNode.point);

                List<Node> neighbors = GetNeighborNodes(priorityNode);

                foreach(Node neighbor in neighbors)
                {
                    //Console.WriteLine("Does the settledSet have me: {0} Does the priorityQueue have me: {1}", settledSet.Contains(neighbor.point), pQueueContains(priorityQueue, neighbor));
                    if(neighbor.point == end)
                    {
                        finalNode = neighbor;
                        foundGoal = true;
                    }
                    else if (!settledSet.Contains(neighbor.point) && !pQueueContains(priorityQueue, neighbor))
                    {
                        priorityQueue.Enqueue(neighbor, neighbor.weight);
                        //Console.WriteLine("Adding- X: {0} Y: {1} Weight: {2}", neighbor.point.X, neighbor.point.Y, neighbor.weight);
                    }
                }
            }

            PrintNodeTrace(finalNode);
        }

        private bool pQueueContains(HeapPriorityQueue<Node> priorityQueue, Node compared)
        {
            int count = 0;
            foreach(Node node in priorityQueue)
            {
                if(node.point == compared.point)
                {
                    return true;
                }
                count++;
            }
            return false;
        }

        private List<Node> GetNeighborNodes(Node priorityNode)
        {
            List<Node> neighbors = new List<Node>();

            Node nNode = new Node(new Point(priorityNode.point.X, priorityNode.point.Y - 1), this.GetPixelWeight(new Point(priorityNode.point.X, priorityNode.point.Y - 1)) + priorityNode.weight, priorityNode);
            Node eNode = new Node(new Point(priorityNode.point.X + 1, priorityNode.point.Y), this.GetPixelWeight(new Point(priorityNode.point.X + 1, priorityNode.point.Y)) + priorityNode.weight, priorityNode);
            Node sNode = new Node(new Point(priorityNode.point.X, priorityNode.point.Y + 1), this.GetPixelWeight(new Point(priorityNode.point.X, priorityNode.point.Y + 1)) + priorityNode.weight, priorityNode);
            Node wNode = new Node(new Point(priorityNode.point.X - 1, priorityNode.point.Y), this.GetPixelWeight(new Point(priorityNode.point.X - 1, priorityNode.point.Y)) + priorityNode.weight, priorityNode);

            if(WithinPicture(nNode.point))
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

        private Boolean WithinPicture(Point point)
        {
            return (point.X < (Overlay.Width - 2) && point.Y < (Overlay.Height - 2) && point.X > 1 && point.Y > 1);
        }

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

        private void PrintNodeTrace(Node finalNode)
        {
            Overlay.SetPixel(finalNode.point.X, finalNode.point.Y, Color.Red);
            if(finalNode.previous != null)
            {
                PrintNodeTrace(finalNode.previous);
            }
        }
	}
}
