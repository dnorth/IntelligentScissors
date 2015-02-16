using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;

namespace VisualIntelligentScissors
{
	public class SimpleScissors : Scissors
	{
        Pen yellowpen = new Pen(Color.Blue); 

		public SimpleScissors() { }

        /// <summary>
        /// constructor for SimpleScissors. 
        /// </summary>
        /// <param name="image">the image you are going to segment including methods for getting gradients.</param>
        /// <param name="overlay">a bitmap on which you can draw stuff.</param>
		public SimpleScissors(GrayBitmap image, Bitmap overlay) : base(image, overlay) { }

        // this is a class you need to implement in CS 312. 

        /// <summary>
        ///  this is the class to implement for CS 312. 
        /// </summary>
        /// <param name="points">the list of segmentation points parsed from the pgm file</param>
        /// <param name="pen">a pen for writing on the overlay if you want to use it.</param>
		public override void FindSegmentation(IList<Point> points, Pen pen)
		{
            // this is the entry point for this class when the button is clicked for 
            // segmenting the image using the simple greedy algorithm. 
            // the points
            
			if (Image == null) throw new InvalidOperationException("Set Image property first.");

            //Color the Starting Points that are given
            ColorStartingPoints(points);

            //For every point, find the path to that point. points[(i + 1) % points.Count] is to make sure we wrap around to the first point
            for(int i = 0; i < points.Count; i++)
            {
                PathToPoints(points[i], points[(i + 1) % points.Count]);
            }
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

        //start- the starting point
        //end- the point to end at
        private void PathToPoints(Point start, Point end)
        {
            //Create a new HashSet to not visit nodes that are already visited
            HashSet<Point> visited = new HashSet<Point>();
            Point currPoint = start;

            //While the current point is not equal to the end point
            while(currPoint != end)
            {
                //Draw the current pixel and add it to the visited list. Also, set an arbitrarily large integer as a weight
                Overlay.SetPixel(currPoint.X, currPoint.Y, Color.Red);
                visited.Add(currPoint);

                int leastPointWeight = int.MaxValue;

                //Find all neighbor points and weights N, E, S, W
                //The lower Y value is more north because of the image setup
                Point nPoint = new Point(currPoint.X, currPoint.Y - 1);
                Point ePoint = new Point(currPoint.X + 1, currPoint.Y);
                Point sPoint = new Point(currPoint.X, currPoint.Y + 1);
                Point wPoint = new Point(currPoint.X - 1, currPoint.Y);

                int nWeight = this.GetPixelWeight(nPoint);
                int eWeight = this.GetPixelWeight(ePoint);
                int sWeight = this.GetPixelWeight(sPoint);
                int wWeight = this.GetPixelWeight(wPoint);

                //Get the next point based on whether or not the point is within the picture, the point has already been visited, and its weight is less than the previous point
                //Because of the way this is setup, it will go in a clockwise fashion- N, E, S, W and will only accept smaller weights once a weight is accepted
                if(WithinPicture(nPoint) && !visited.Contains(nPoint) && nWeight < leastPointWeight)
                {
                    currPoint = nPoint;
                    leastPointWeight = nWeight;
                }

                if (WithinPicture(ePoint) && !visited.Contains(ePoint) && eWeight < leastPointWeight)
                {
                    currPoint = ePoint;
                    leastPointWeight = eWeight;
                }

                if (WithinPicture(sPoint) && !visited.Contains(sPoint) && sWeight < leastPointWeight)
                {
                    currPoint = sPoint;
                    leastPointWeight = sWeight;
                }

                if (WithinPicture(wPoint) && !visited.Contains(wPoint) && wWeight < leastPointWeight)
                {
                    currPoint = wPoint;
                    leastPointWeight = wWeight;
                }

                //If our weight never changed, meaning we didn't go N, E, S, W then we've reached a dead end. Return
                if (leastPointWeight == int.MaxValue)
                {
                    break;
                }
            }
        }

        //finalNode- the last node in a trace of nodes that lead back to the original 
        //Until the previous node is null, recursively draw pixels on the current node
        private Boolean WithinPicture(Point point)
        {
            return (point.X < (Overlay.Width - 2) && point.Y < (Overlay.Height - 2) && point.X > 1 && point.Y > 1);  
        }
	}
}
