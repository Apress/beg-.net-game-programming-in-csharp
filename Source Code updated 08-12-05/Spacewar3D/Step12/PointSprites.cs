
using System;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;


    
	/// <summary>
	/// The Vapor Trail class
	/// </summary>
public class VaporTrail
{
	public struct PointVertex
	{
		public Vector3 v;
		public int color;
		public static readonly VertexFormats Format =   VertexFormats.Position | VertexFormats.Diffuse;
	};

	/// <summary>
	/// Global data for the particles
	/// </summary>
	public struct Particle
	{
		public bool isSpark;     // Sparks are less energetic particles that
		// are generated where/when the main particles
		// hit the ground

		public Vector3 positionVector;       // Current position
		public Vector3 velocityVector;       // Current velocity

		public Vector3 initialPosition;      // Initial position
		public Vector3 initialVelocity;      // Initial velocity
		public float creationTime;     // Time of creation

		public System.Drawing.Color diffuseColor; // Initial diffuse color
		public System.Drawing.Color fadeColor;    // Faded diffuse color
		public float fadeProgression;      // Fade progression
	};

	private float radius = 0.0f;

	private float time = 0.0f;
	private int baseParticle = 0;
	private int flush = 0;
	private int discard = 0;

	private int particles = 0;
	private int particlesLimit = 0;
	private Vector3 m_loc;
	private Vector3 offset;
	private System.Collections.ArrayList particlesList = new System.Collections.ArrayList();
	private System.Collections.ArrayList freeParticles = new System.Collections.ArrayList();

	private System.Random rand = new System.Random();

	public Vector3 EmitterLocation {get { return m_loc; } }
	public Vector3 EmitterOffset { get { return offset; } set { offset = value; } }


	// Geometry
	private VertexBuffer vertexBuffer = null;

	private Texture particleTexture;
	public Texture ParticleTexture { set { particleTexture = value; } }

	private Device device = null;






	/// <summary>
	/// VaporTrail constructor
	/// </summary>
	public VaporTrail(Device device, int flush, int discard, float radius)
	{
		this.device = device;
		if (device != null)
		{
			device.DeviceLost += new System.EventHandler(this.InvalidateDeviceObjects);
			device.Disposing += new System.EventHandler(this.InvalidateDeviceObjects);
			device.DeviceReset += new System.EventHandler(this.RestoreDeviceObjects);
		}
		this.radius        = radius;

		this.baseParticle = discard;
		this.flush        = flush;
		this.discard      = discard;

		particles    = 0;
		particlesLimit = 2048;
		RestoreDeviceObjects(device, null);
	}


        
	public void InvalidateDeviceObjects(object sender, EventArgs e)
	{
		if (vertexBuffer != null)
			vertexBuffer.Dispose();
		vertexBuffer = null;
	}
        
	/// <summary>
	/// Restores the device objects
	/// </summary>
	public void RestoreDeviceObjects(object sender, EventArgs e)
	{

		Device device = (Device)sender;

		// Create a vertex buffer for the particle system.  The size of this buffer
		// does not relate to the number of particles that exist.  Rather, the
		// buffer is used as a communication channel with the device.. we fill in 
		// a bit, and tell the device to draw.  While the device is drawing, we
		// fill in the next bit using NOOVERWRITE.  We continue doing this until 
		// we run out of vertex buffer space, and are forced to DISCARD the buffer
		// and start over at the beginning.

		vertexBuffer = new VertexBuffer(typeof(PointVertex), discard, device,  Usage.Dynamic | Usage.WriteOnly | Usage.Points, PointVertex.Format, Pool.Default);
	}

	public void Explode(int NumParticlesToEmit, Vector3 vPosition)
	{
		// Emit new particles
		int particlesEmit = particles + NumParticlesToEmit;
		while(particles < particlesLimit && particles < particlesEmit)
		{
			Particle particle;

			if (freeParticles.Count > 0)
			{
				particle = (Particle)freeParticles[0];
				freeParticles.RemoveAt(0);
			}
			else
			{
				particle = new Particle();
			}

			// Emit new particle
			float fRand1 = ((float)rand.Next(int.MaxValue)/(float)int.MaxValue) * (float)Math.PI * 2.0f;
			float fRand2 = ((float)rand.Next(int.MaxValue)/(float)int.MaxValue) * (float)Math.PI * 2.0f;

			particle.isSpark = false;

			particle.initialPosition = vPosition + new Vector3(0.0f, radius, 0.0f);

			particle.initialVelocity.X  = (float)Math.Cos(fRand1) * (float)Math.Sin(fRand2) * 100f;
			particle.initialVelocity.Z  = (float)Math.Sin(fRand1) * (float)Math.Sin(fRand2) * 100f;
			particle.initialVelocity.Y  = (float)Math.Cos(fRand2) * 100f;
				

			particle.positionVector = particle.initialPosition;
			particle.velocityVector = particle.initialVelocity;

			particle.diffuseColor = Color.Violet;
			particle.fadeColor    = Color.Black;
			particle.fadeProgression      = 1.0f;
			particle.creationTime     = time;

			particlesList.Add(particle);
			particles++;
		}
	}
  
	/// <summary>
	/// Updates the scene
	/// </summary>
	public void Update(float fSecsPerFrame, int NumParticlesToEmit,
		System.Drawing.Color clrEmitColor,System.Drawing.Color clrFadeColor, float fEmitVel, Vector3 vPosition)
	{
			
		time += fSecsPerFrame;
		//Console.WriteLine(time.ToString());
		//if (time < .1)
		//	return;
		m_loc = vPosition;
		for (int ii = particlesList.Count-1; ii >= 0; ii--)
		{
			Particle p = (Particle)particlesList[ii];
			// Calculate new position
			//float fT = time - p.creationTime;
			float fT = time - p.creationTime;


			p.fadeProgression -= fSecsPerFrame * 0.60f;
				

				
			p.positionVector    = p.initialVelocity * fT + p.initialPosition;
				
			p.velocityVector.Z = 0;


			if (p.fadeProgression < 0.0f)
				p.fadeProgression = 0.0f;

			// Kill old particles
				
			if (p.fadeProgression <= 0.0f)
			{
					

				
				// Kill particle
				freeParticles.Add(p);
				particlesList.RemoveAt(ii);

				if (!p.isSpark)
					particles--;
			}
			else
				particlesList[ii] = p;
		}

		// Emit new particles
		int particlesEmit = particles + NumParticlesToEmit;
		while(particles < particlesLimit && particles < particlesEmit)
		{
			Particle particle;

			if (freeParticles.Count > 0)
			{
				particle = (Particle)freeParticles[0];
				freeParticles.RemoveAt(0);
			}
			else
			{
				particle = new Particle();
			}

			// Emit new particle
			float fRand1 = ((float)rand.Next(int.MaxValue)/(float)int.MaxValue) * (float)Math.PI * 1.0f;
			float fRand2 = ((float)rand.Next(int.MaxValue)/(float)int.MaxValue) * (float)Math.PI * 0.25f;

			particle.isSpark = false;

			particle.initialPosition = vPosition + offset;

			particle.initialVelocity.X  = (float)Math.Cos(fRand1) * (float)Math.Sin(fRand2) * .5f;
			particle.initialVelocity.Y  = (float)Math.Sin(fRand1) * (float)Math.Sin(fRand2) * .5f;
			particle.initialVelocity.Z  = (float)Math.Cos(fRand2);


			particle.positionVector = particle.initialPosition;
			particle.velocityVector = particle.initialVelocity;

			particle.diffuseColor = clrEmitColor;
			particle.fadeColor    = clrFadeColor;
			particle.fadeProgression      = 1.0f;
			particle.creationTime     = time;

			particlesList.Add(particle);
			particles++;
		}
	}

        
        
        
	/// <summary>
	/// Renders the scene
	/// </summary>
	public void Render()
	{
		// Set the render states for using point sprites
		device.RenderState.ZBufferWriteEnable = false;
		device.RenderState.AlphaBlendEnable = true;
		device.RenderState.SourceBlend = Blend.One;
		device.RenderState.DestinationBlend = Blend.One;
		bool lightEnabled = device.RenderState.Lighting;
		device.RenderState.Lighting = false;
		device.SetTexture(0, particleTexture);
		device.Transform.World = Matrix.Identity;

		device.RenderState.PointSpriteEnable = true;
		device.RenderState.PointScaleEnable = true;
		device.RenderState.PointSize = 1.0f;
		device.RenderState.PointScaleA = 0f;
		device.RenderState.PointScaleB = 1.0f;
		device.RenderState.PointScaleC = 1.0f;

		// Set up the vertex buffer to be rendered
		device.SetStreamSource(0, vertexBuffer, 0);
		device.VertexFormat = PointVertex.Format;

		PointVertex[] vertices = null;
		int numParticlesToRender = 0;



		// Lock the vertex buffer.  We fill the vertex buffer in small
		// chunks, using LockFlags.NoOverWrite.  When we are done filling
		// each chunk, we call DrawPrim, and lock the next chunk.  When
		// we run out of space in the vertex buffer, we start over at
		// the beginning, using LockFlags.Discard.

		baseParticle += flush;

		if (baseParticle >= discard)
			baseParticle = 0;

		int count = 0;
		vertices = (PointVertex[])vertexBuffer.Lock(baseParticle * DXHelp.GetTypeSize(typeof(PointVertex)), typeof(PointVertex), (baseParticle != 0) ? LockFlags.NoOverwrite : LockFlags.Discard, flush);
		foreach(Particle p in particlesList)
		{
			Vector3 vPos = p.positionVector;
			Vector3 vVel = p.velocityVector;
			float fLengthSq = vVel.LengthSq();
			uint steps;

			if (fLengthSq < 1.0f)        steps = 2;
			else if (fLengthSq <  4.00f) steps = 3;
			else if (fLengthSq <  9.00f) steps = 4;
			else if (fLengthSq < 12.25f) steps = 5;
			else if (fLengthSq < 16.00f) steps = 6;
			else if (fLengthSq < 20.25f) steps = 7;
			else                          steps = 8;

			vVel *= -0.01f / (float)steps;
			System.Drawing.Color diffuse = ColorOperator.Lerp(p.fadeColor, p.diffuseColor, p.fadeProgression);


			// Render each particle a bunch of times to get a blurring effect
			for (int i = 0; i < steps; i++)
			{
				vertices[count].v     = vPos;
				vertices[count].color = diffuse.ToArgb();
				count++;

				if (++numParticlesToRender == flush)
				{
					// Done filling this chunk of the vertex buffer.  Lets unlock and
					// draw this portion so we can begin filling the next chunk.

					vertexBuffer.Unlock();

					device.DrawPrimitives(PrimitiveType.PointList, baseParticle, numParticlesToRender);

					// Lock the next chunk of the vertex buffer.  If we are at the 
					// end of the vertex buffer, LockFlags.Discard the vertex buffer and start
					// at the beginning.  Otherwise, specify LockFlags.NoOverWrite, so we can
					// continue filling the VB while the previous chunk is drawing.
					baseParticle += flush;

					if (baseParticle >= discard)
						baseParticle = 0;

					vertices = (PointVertex[])vertexBuffer.Lock(baseParticle * DXHelp.GetTypeSize(typeof(PointVertex)), typeof(PointVertex), (baseParticle != 0) ? LockFlags.NoOverwrite : LockFlags.Discard, flush);
					count = 0;

					numParticlesToRender = 0;
				}

				vPos += vVel;
			}
		}

		// Unlock the vertex buffer
		vertexBuffer.Unlock();
		// Render any remaining particles
		if (numParticlesToRender > 0)
			device.DrawPrimitives(PrimitiveType.PointList, baseParticle, numParticlesToRender);

		// Reset render states
		device.RenderState.PointSpriteEnable = false;
		device.RenderState.PointScaleEnable = false;


		device.RenderState.Lighting = lightEnabled;
		device.RenderState.ZBufferWriteEnable = true;
		device.RenderState.AlphaBlendEnable = false;

	}

	public void Dispose()
	{
		if (vertexBuffer != null)
			vertexBuffer.Dispose();
	}
}

