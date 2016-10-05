using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace SimpleSprite
{
	/// <summary>
	/// Summary description for Step2.
	/// </summary>
	public class Step2 : System.Windows.Forms.Form
	{
		private TileSet tileSet;
		private Texture tileSheet;
		private Rectangle tilePosition;
		private Vector3 spritePosition;
		private Vector3 spriteCenter;

		private Device device;

		private System.ComponentModel.Container components = null;
		public Step2() 
		{
			InitializeComponent();

			this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);
			this.Text = "Simple Sprite: Step 2";
		}

		protected override void Dispose( bool disposing ) 
		{
			if( disposing ) 
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() 
		{
			// 
			// Step2
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(640, 480);
			this.Name = "Step2";
			this.Text = "Step2";
		}
		#endregion

		[STAThread]
		public static void Main() 
		{
			using (Step2 frm = new Step2()) 
			{
				frm.Show();
				frm.InitializeGraphics();
				Application.Run(frm); //triggers OnPaint event, which is main loop
			}
			Application.Exit();
		}

		private void InitializeGraphics() 
		{
			try 
			{
				PresentParameters presentParams = new PresentParameters();
				presentParams.Windowed = true;
				presentParams.SwapEffect = SwapEffect.Discard;
				presentParams.BackBufferFormat = Format.Unknown;
				presentParams.AutoDepthStencilFormat = DepthFormat.D16;
				presentParams.EnableAutoDepthStencil = true;

				// Store the default adapter
				int adapterOrdinal = Manager.Adapters.Default.Adapter;
				CreateFlags flags = CreateFlags.SoftwareVertexProcessing;

				// Check to see if we can use a pure hardware device
				Caps caps = Manager.GetDeviceCaps(adapterOrdinal, DeviceType.Hardware);

				// Do we support hardware vertex processing?
				if (caps.DeviceCaps.SupportsHardwareTransformAndLight)
					// Replace the software vertex processing
					flags = CreateFlags.HardwareVertexProcessing;
            
				// Do we support a pure device?
				if (caps.DeviceCaps.SupportsPureDevice)
					flags |= CreateFlags.PureDevice;

				device = new Device(0, DeviceType.Hardware, this, flags, presentParams);
				device.DeviceReset += new System.EventHandler(this.OnResetDevice);
				OnResetDevice(device, null);

				tileSheet = TextureLoader.FromFile(device, MediaUtilities.FindFile("donuts.bmp"), 1024, 1024, 
					1, 0,Format.A8R8G8B8, Pool.Managed, Filter.Point, Filter.Point, (unchecked((int)0xff000000)));
				//Uncomment these lines to see the spite border areas
				//				donutTexture = TextureLoader.FromFile(device, MediaUtilities.FindFile("donuts.bmp"), 1024, 1024, 
				//					1, 0,Format.A8R8G8B8, Pool.Managed, Filter.Point, Filter.Point, 0);

				tileSet = new TileSet(tileSheet, 0, 0, 6, 5, 32, 32);
				tilePosition = new Rectangle(tileSet.XOrigin, tileSet.YOrigin,tileSet.ExtentX*2, tileSet.ExtentY*2);

			}
			catch (DirectXException) 
			{
				// Catch any errors and return a failure
			}
		}


		public void OnResetDevice(object sender, EventArgs e) 
		{
			Device device = (Device)sender;
		}

		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) 
		{
			spritePosition = new Vector3(200f, 200f, 0f);
			device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Blue, 1.0f, 0);
			device.BeginScene();
			using (Sprite sprite = new Sprite(device)) 
			{
				sprite.Begin(SpriteFlags.AlphaBlend);
				sprite.Draw(tileSet.Texture, tilePosition, spriteCenter, spritePosition,
					Color.FromArgb(255,255,255,255));
				sprite.End();
			}
			device.EndScene();
			device.Present();
			this.Invalidate();
		}
	}
}
