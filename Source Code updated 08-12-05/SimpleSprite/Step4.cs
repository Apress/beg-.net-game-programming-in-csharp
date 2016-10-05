using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace SimpleSprite {
	/// <summary>
	/// Summary description for Step4.
	/// </summary>
	public class Step4 : System.Windows.Forms.Form {
		private const float frameRate = 1f/1f; //30 times a second
		private TileSet tileSet;
		private Texture tileSheet;
		private Rectangle tilePosition;

		//Sprite state data
		private Vector3 spritePosition = new Vector3(200f, 200f, 0f);
		private Vector3 spriteCenter;
		private Vector3 spriteVelocity = new Vector3(100.0f, 100.0f, 0.0f);
		private float angle = 0.0f; //directional angle of the sprite
		private int frame;
		private float frameTrigger; //accumulates elapsed time

		private HighResolutionTimer hrt = new HighResolutionTimer();
		private float deltaTime;

		private int PrevXOffset = -1;
		private int PrevYOffset = -1;

		private Device device;

		private System.ComponentModel.Container components = null;
		public Step4() {
			InitializeComponent();

			this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);
			this.Text = "Simple Sprite: Step 4";
		}

		protected override void Dispose( bool disposing ) {
			if( disposing ) {
				if (components != null) {
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
		private void InitializeComponent() {
			// 
			// Step4
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(640, 480);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "Step4";
			this.Text = "Step4";

		}
		#endregion

		[STAThread]
		public static void Main() {
			using (Step4 frm = new Step4()) {
				frm.Show();
				frm.InitializeGraphics();
				Application.Run(frm); //triggers OnPaint event, which is main loop
			}
			Application.Exit();
		}

		private void InitializeGraphics() {
			try {
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

				hrt.Start();

			}
			catch (DirectXException) {
				// Catch any errors and return a failure
			}
		}


		public void OnResetDevice(object sender, EventArgs e) {
			Device device = (Device)sender;
		}

		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) {
			deltaTime = hrt.ElapsedTime;
			UpdateSprite(deltaTime);
			device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Blue, 1.0f, 0);
			device.BeginScene();
			using (Sprite sprite = new Sprite(device)) {
				sprite.Begin(SpriteFlags.None);
				//Set rotation center for sprite
				spriteCenter.X = spritePosition.X + tileSet.ExtentX;
				spriteCenter.Y = spritePosition.Y + tileSet.ExtentY;

				//Spin, Shift, Stretch :-)
				sprite.Transform = Matrix.RotationZ(angle) * Matrix.Translation(spriteCenter);
				sprite.Draw(tileSet.Texture, tilePosition, spriteCenter, spritePosition,
					Color.FromArgb(255,255,255,255));
				sprite.End();
			}
			device.EndScene();
			device.Present();
			this.Invalidate();
		}

		public virtual void UpdateSprite(float DeltaTime) {
			frameTrigger += DeltaTime;
			//Do we move to the next frame?
			if (frameTrigger >= frameRate) {
				frameTrigger = 0f;
				frame++;
				if (frame == tileSet.NumberFrameColumns * tileSet.NumberFrameRows)
					frame = 0; //loop to beginning
			}
			//Now change the location of the image
            int Xoffset = ((int)frame % tileSet.NumberFrameColumns ) * tileSet.ExtentX*2;
            int Yoffset = ((int)frame / tileSet.NumberFrameColumns) * tileSet.ExtentY*2;

            tilePosition.X = tileSet.XOrigin + Xoffset;
            tilePosition.Y = tileSet.YOrigin + Yoffset;

            if ((PrevXOffset != Xoffset) || (PrevYOffset != Yoffset)) {
                Console.Write("Tileposition.X (" + tilePosition.X + ") = " + tileSet.XOrigin + " + " + Xoffset);
				Console.WriteLine(",  Tileposition.Y (" + tilePosition.Y + ") = " + tileSet.YOrigin + " + " + Yoffset);
			}

            PrevXOffset = Xoffset;
            PrevYOffset = Yoffset;

			//update sprite position
			spritePosition.X += spriteVelocity.X * DeltaTime;
			spritePosition.Y += spriteVelocity.Y * DeltaTime;

			angle += DeltaTime;

			//bounce sprite if it tries to go outside window
			if (spritePosition.X > (this.Width-(tileSet.ExtentX*2)) || spritePosition.X < 0) {
				spriteVelocity.X *= -1;
			}
			if (spritePosition.Y > (this.Height-(tileSet.ExtentY*2)) || spritePosition.Y < 0) {
				spriteVelocity.Y *= -1;
			}

		}
	}
}
