using UnityEngine;
using System.Collections;


namespace SMARTS_SDK.Ultrasound
{
	[ExecuteInEditMode]
	[AddComponentMenu("Image Effects/Blur")]
	public class BlurEffect : MonoBehaviour
	{
		/// Blur iterations - larger number means more blur.
		[SerializeField]
		private int iterations = 4;
		public int Iterations
		{
			get
			{
				return iterations;
			}
			set
			{
				iterations = value;
			}
		}

		/// Blur spread for each iteration. Lower values
		/// give better looking blur, but require more iterations to
		/// get large blurs. Value is usually between 0.5 and 1.0.
		[SerializeField]
		private float blurSpread = -0.17f;
		public float BlurSpread
		{
			get
			{
				return blurSpread;
			}
			set
			{
				blurSpread = value;
			}
		}

		// The blur iteration shader.
		// Basically it just takes 4 texture samples and averages them.
		// By applying it repeatedly and spreading out sample locations
		// we get a Gaussian blur approximation. 
		[SerializeField]
		private Shader blurShader;

		static Material m_Material = null;
		protected Material material
		{
			get
			{
				if (m_Material == null)
				{
					m_Material = new Material(blurShader);
					m_Material.hideFlags = HideFlags.DontSave;
				}
				return m_Material;
			}
		}

		protected void OnDisable()
		{
			if (m_Material)
			{
				DestroyImmediate(m_Material);
			}
		}

		// --------------------------------------------------------

		protected void Start()
		{
			if (blurShader == null)
			{
				try
				{
					blurShader = Shader.Find("BlurEffectConeTaps");
				}
				catch
				{
					Debug.LogError("Could not find \"BlurEffectConeTaps\" shader");
				}
			}
			// Disable if we don't support image effects
			if (!SystemInfo.supportsImageEffects)
			{
				enabled = false;
				return;
			}
			// Disable if the shader can't run on the users graphics card
			if (!blurShader || !material.shader.isSupported)
			{
				enabled = false;
				return;
			}
		}

		// Performs one blur iteration.
		public void FourTapCone(RenderTexture source, RenderTexture dest, int iteration)
		{
			float off = 0.5f + iteration * blurSpread;
			Graphics.BlitMultiTap(source, dest, material,
				new Vector2(-off, -off),
				new Vector2(-off, off),
				new Vector2(off, off),
				new Vector2(off, -off)
			);
		}

		// Downsamples the texture to a quarter resolution.
		private void DownSample4x(RenderTexture source, RenderTexture dest)
		{
			float off = 1.0f;
			Graphics.BlitMultiTap(source, dest, material,
				new Vector2(-off, -off),
				new Vector2(-off, off),
				new Vector2(off, off),
				new Vector2(off, -off)
			);
		}

		// Called by the camera to apply the image effect
		void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			RenderTexture buffer = RenderTexture.GetTemporary(source.width / 4, source.height / 4, 0);
			RenderTexture buffer2 = RenderTexture.GetTemporary(source.width / 4, source.height / 4, 0);

			// Copy source to the 4x4 smaller texture.
			DownSample4x(source, buffer);

			// Blur the small texture
			bool oddEven = true;
			for (int i = 0; i < iterations; i++)
			{
				if (oddEven)
					FourTapCone(buffer, buffer2, i);
				else
					FourTapCone(buffer2, buffer, i);
				oddEven = !oddEven;
			}
			if (oddEven)
				Graphics.Blit(buffer, destination);
			else
				Graphics.Blit(buffer2, destination);

			RenderTexture.ReleaseTemporary(buffer);
			RenderTexture.ReleaseTemporary(buffer2);
		}
	}
}