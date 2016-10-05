using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectSound;
using DI = Microsoft.DirectX.DirectInput;
using D3D = Microsoft.DirectX.Direct3D;
using DS = Microsoft.DirectX.DirectSound;

namespace SimpleSprite {
	/// <summary>
	/// Summary description for Step6.
	/// </summary>
	public class Step6 : System.Windows.Forms.Form {
		private const float frameRate = 1f/30f; //30 times a second
		private const float spinRate = 0.1f;
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


		private D3D.Device device;
		private DI.Device kbd;
		private DS.Device snd;
		private DS.SecondaryBuffer bounce;

		private System.ComponentModel.Container components = null;
		public Step6() {
			InitializeComponent();

			this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);
			this.Text = "Simple Sprite: Step 6";
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
			// Step6
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(640, 480);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "Step6";
			this.Text = "Step6";

		}
		#endregion

		[STAThread]
		public static void Main() {
			using (Step6 frm = new Step6()) {
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
				int adapterOrdinal = D3D.Manager.Adapters.Default.Adapter;
				CreateFlags flags = CreateFlags.SoftwareVertexProcessing;

				// Check to see if we can use a pure hardware device
				D3D.Caps caps = D3D.Manager.GetDeviceCaps(adapterOrdinal, D3D.DeviceType.Hardware);

				// Do we support hardware vertex processing?
				if (caps.DeviceCaps.SupportsHardwareTransformAndLight)
					// Replace the software vertex processing
					flags = CreateFlags.HardwareVertexProcessing;
            
				// Do we support a pure device?
				if (caps.DeviceCaps.SupportsPureDevice)
					flags |= CreateFlags.PureDevice;

				device = new D3D.Device(0, D3D.DeviceType.Hardware, this, flags, presentParams);
				device.DeviceReset += new System.EventHandler(this.OnResetDevice);
				OnResetDevice(device, null);

				tileSheet = TextureLoader.FromFile(device, MediaUtilities.FindFile("donuts.bmp"), 1024, 1024, 
					1, 0,Format.A8R8G8B8, Pool.Managed, Filter.Point, Filter.Point, (unchecked((int)0xff000000)));

				tileSet = new TileSet(tileSheet, 0, 0, 6, 5, 32, 32);
				tilePosition = new Rectangle(tileSet.XOrigin, tileSet.YOrigin,tileSet.ExtentX*2, tileSet.ExtentY*2);

				//set up DirectInput keyboard device...
				kbd = new DI.Device(SystemGuid.Keyboard);
				kbd.SetCooperativeLevel(this, 
					DI.CooperativeLevelFlags.Background | DI.CooperativeLevelFlags.NonExclusive );
				kbd.Acquire();

				//Set up DirectSound device and buffers
				snd = new DS.Device();
				snd.SetCooperativeLevel(this, DS.CooperativeLevel.Normal);
				bounce = new DS.SecondaryBuffer(MediaUtilities.FindFile("bounce.wav"), snd);
				
				hrt.Start();
			}
			catch (DirectXException) {
				// Catch any errors and return a failure
			}
		}


		public void OnResetDevice(object sender, EventArgs e) {
			D3D.Device device = (D3D.Device)sender;
		}

		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) {
			deltaTime = hrt.ElapsedTime;
			ProcessInputState();
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
			tilePosition.X = tileSet.XOrigin + 
				( (int)frame % tileSet.NumberFrameColumns ) * tileSet.ExtentX*2;
			tilePosition.Y = tileSet.YOrigin + 
				( (int)frame / tileSet.NumberFrameColumns) * tileSet.ExtentY*2;

			//update sprite position
			spritePosition.X += spriteVelocity.X * DeltaTime;
			spritePosition.Y += spriteVelocity.Y * DeltaTime;

			//bounce sprite if it tries to go outside window
			if (spritePosition.X > (this.Width-(tileSet.ExtentX*2)) || spritePosition.X < 0) {
				spriteVelocity.X *= -1;
				bounce.Play(0, DS.BufferPlayFlags.Default);
			}
			if (spritePosition.Y > (this.Height-(tileSet.ExtentY*2)) || spritePosition.Y < 0) {
				spriteVelocity.Y *= -1;
				bounce.Play(0, DS.BufferPlayFlags.Default);
			}
		}

		protected void ProcessInputState() {
			foreach (Key k in kbd.GetPressedKeys()) {
				if (k == Key.Left) {
					//Turn counterclockwise
					angle -= spinRate;
				}
				if (k == Key.Right) {
					//turn clockwise
					angle += spinRate;
				}
				if (k == Key.Escape) {
					kbd.Unacquire(); //release the keyboard device
					kbd.Dispose();
					Application.Exit();
				}
			}
		}
	}
}
