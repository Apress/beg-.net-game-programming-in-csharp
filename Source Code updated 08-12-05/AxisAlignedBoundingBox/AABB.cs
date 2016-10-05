using System;
using System.Drawing;

namespace AxisAlignedBoundingBox
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public struct AABB 
	{
		private float topLeftX, topLeftY; //Coordinate top left of the box
		private float bottomRightX, bottomRightY; //Coordinate lower right of the box
		private float boxMinX, boxMaxX, boxMinY, boxMaxY;

		//Constructor
		public AABB (float tlX, float tlY, float brX, float brY) {
			topLeftX = tlX;
			topLeftY = tlY;
			bottomRightX = brX;
			bottomRightY = brY;
			if (topLeftX < bottomRightX) {
				boxMinX = topLeftX;
				boxMaxX = bottomRightX;
			} else {
				boxMaxX = topLeftX;
				boxMinX = bottomRightX;
			}
			if (topLeftY < bottomRightY) {
				boxMinY = topLeftY;
				boxMaxY = bottomRightY;
			} else {
				boxMaxY = topLeftY;
				boxMinY = bottomRightY;
			}
		}

		public float MaxX { get { return boxMaxX; } }
		public float MinX { get { return boxMinX; } }
		public float MaxY { get { return boxMaxY; } }
		public float MinY { get { return boxMinY; } }

		public bool CircleIntersect (float CircleCenterX, float CircleCenterY, float Radius) {
			double dist = 0;
			//Check x axis. If Circle is outside box limits, add to distance.
			if (CircleCenterX < this.MinX) 
				dist += Math.Pow(CircleCenterX - this.MinX, 2.0);
			else if (CircleCenterX > this.MaxX) 
					 dist += Math.Pow(CircleCenterX - this.MaxX, 2.0);
			//Check y axis. If Circle is outside box limits, add to distance.
			if (CircleCenterY < this.MinY) 
				dist += Math.Pow(CircleCenterY - this.MinY, 2.0);
			else if (CircleCenterY > this.MaxY) 
					 dist += Math.Pow(CircleCenterY - this.MaxY, 2.0);
			//Now that distances along x and y axis are added, check if the square
			//of the Circle's radius is longer and return the boolean result.
			return dist < (Radius*Radius);
		}
	}
}
