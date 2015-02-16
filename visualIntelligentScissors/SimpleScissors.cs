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

            ColorStartingPoints(points);

            for(int i = 0; i < points.Count; i++)
            {
                PathToPoints(points[i], points[(i + 1) % points.Count]);
            }
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

        private void PathToPoints(Point start, Point end)
        {
            List<Point> visited = new List<Point>();
            Point currPoint = start;

            while(currPoint != end)
            {
                Overlay.SetPixel(currPoint.X, currPoint.Y, Color.Red);
                visited.Add(currPoint);

                int leastPointWeight = int.MaxValue;

                //The lower Y value is more north
                Point nPoint = new Point(currPoint.X, currPoint.Y - 1);
                Point ePoint = new Point(currPoint.X + 1, currPoint.Y);
                Point sPoint = new Point(currPoint.X, currPoint.Y + 1);
                Point wPoint = new Point(currPoint.X - 1, currPoint.Y);

                int nWeight = this.GetPixelWeight(nPoint);
                int eWeight = this.GetPixelWeight(ePoint);
                int sWeight = this.GetPixelWeight(sPoint);
                int wWeight = this.GetPixelWeight(wPoint);

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

                if (leastPointWeight == int.MaxValue)
                {
                    break;
                }
            }
        }

        private Boolean WithinPicture(Point point)
        {
            return (point.X < (Overlay.Width - 2) && point.Y < (Overlay.Height - 2) && point.X > 1 && point.Y > 1);  
        }
	}
}
