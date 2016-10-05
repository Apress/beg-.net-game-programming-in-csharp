using System;
using NUnit.Framework;
using AxisAlignedBoundingBox;
namespace AABBTest {

	[TestFixture] 
	public class SimpleTest {
		protected AABB testBox1;
		
		[SetUp] public void Init() {
			testBox1 = new AABB(2,2,6,6);
		}

		[Test] public void OutsideCircle() {
			bool result = testBox1.CircleIntersect(8,8,1);
			// forced false result
			Assert.IsFalse(result, "Expected False.");
		}

		[Test] public void InsideCircle() {
			bool result = testBox1.CircleIntersect(3,3,1);
			Assert.IsTrue(result, "Expected True.");
		}
		[Test] public void OnCircle() {
			bool result = testBox1.CircleIntersect(2,2,1);
			Assert.IsTrue(result, "Expected True.");
		}
		[Test] public void OverlapsCircle() {
			bool result = testBox1.CircleIntersect(7,7,1.5f);
			Assert.IsTrue(result, "Expected True.");
		}
	}
}