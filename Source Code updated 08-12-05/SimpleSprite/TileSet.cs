using System;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
	/// <summary>
	/// Summary description for TileSet.
	/// </summary>

namespace SimpleSprite 
{
	public class TileSet 
	{
		private Texture texture;

		public Texture Texture 
		{
			get 
			{
				return texture;
			}
		}
		private int xOrigin;

		public int XOrigin 
		{
			get 
			{
				return xOrigin;
			}
		}
		private int yOrigin;

		public int YOrigin 
		{
			get 
			{
				return yOrigin;
			}
		} 
		private int numberFrameRows;

		public int NumberFrameRows 
		{
			get 
			{
				return numberFrameRows;
			}
		}
		private int numberFrameColumns;

		public int NumberFrameColumns 
		{
			get 
			{
				return numberFrameColumns;
			}
		}
		private int extentX;
		public int ExtentX 
		{
			get 
			{
				return extentX;
			}
		}

		private int extentY;
		public int ExtentY 
		{
			get 
			{
				return extentY;
			}
		}

		public TileSet(Texture tex, int StartX, int StartY, int RowCount, int ColumnCount, int ExtentX, int ExtentY) 
		{
			xOrigin = StartX;
			yOrigin = StartY;
			extentX = ExtentX;
			extentY = ExtentY;
			numberFrameRows = RowCount;
			numberFrameColumns = ColumnCount;
			texture = tex;
		}
	}
}