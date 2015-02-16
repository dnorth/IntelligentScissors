using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Collections; 

namespace VisualIntelligentScissors
{
	public class DijkstraScissors : Scissors
	{
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
            Program.MainForm.RefreshImage();
            GetPixelWeight(points[1]); 

			throw new NotImplementedException();
		}
	}
}
