//-----------------------------------------------------------------------------
// SpriteBatch.cs
//
// Spooker Open Source Game Framework
// Copyright (C) Indie Armory. All rights reserved.
// Website: http://indiearmory.com
// Other Contributors: krzat @ https://bitbucket.org/krzat
// License: MIT
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using SFML.Window;

namespace Spooker.Graphics
{
	/// <summary>
	/// Provides optimized drawing of sprites
	/// </summary>
	public class SpriteBatch : IDisposable
	{
		#region Private fields

		private struct BatchedTexture
		{
			public uint Count;
			public SFML.Graphics.Texture Texture;
		}

		private readonly List<SpriteBatch> _batchers = new List<SpriteBatch>();
		private readonly List<BatchedTexture> _textures = new List<BatchedTexture>();
		private readonly SFML.Graphics.RenderTarget _graphicsDevice;
		private readonly SFML.Graphics.Text _str;
		private SFML.Graphics.RenderStates _states;
        private SFML.Graphics.Vertex[] _vertices = new SFML.Graphics.Vertex[100 * 4];
		private SFML.Graphics.Texture _activeTexture;
		private SpriteSortMode _sortMode;
		private bool _active;
		private uint _queueCount;

		#endregion

		#region Public fields
		
		/// <summary>
		/// Returns or sets maximal possible number of verticles at
		/// once in this vertex batch. Max capacity is always divided
		/// by 4 (becouse of 4 verticle corners).
		/// </summary>
		public int Max;

		#endregion

		#region Properties

		/// <summary>
		/// Returns count of all vertices in this vertex batch.
		/// </summary>
		/// <value>The count.</value>
		public int Count { get; private set; }

		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="Spooker.Graphics.SpriteBatch"/> class.
		/// </summary>
		/// <param name="graphicsDevice">Graphics device.</param>
		public SpriteBatch(SFML.Graphics.RenderTarget graphicsDevice)
			: this(graphicsDevice, 40000)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Spooker.Graphics.SpriteBatch"/> class.
		/// </summary>
		/// <param name="graphicsDevice">Graphics device.</param>
		/// <param name="capacity">Maximal number of vertices in this vertex batch.</param>
		public SpriteBatch(SFML.Graphics.RenderTarget graphicsDevice, int capacity)
		{
			_str = new SFML.Graphics.Text ();
			_graphicsDevice = graphicsDevice;
			_states = SFML.Graphics.RenderStates.Default;
			Max = capacity;
		}
		#endregion

		#region Public methods

		/// <summary>
		/// Begins this sprite batch, so we can draw sprites after.
		/// </summary>
		public void Begin()
		{
			Begin (SpriteBlendMode.Alpha);
		}

		/// <summary>
		/// Begins this sprite batch, so we can draw sprites after.
		/// </summary>
		/// <param name="blendMode">Blend mode.</param>
		public void Begin(SpriteBlendMode blendMode)
		{
			Begin (blendMode, SpriteSortMode.FrontToBack);
		}

		/// <summary>
		/// Begins this sprite batch, so we can draw sprites after.
		/// </summary>
		/// <param name="blendMode">Blend mode.</param>
		/// <param name="sortMode">Sort mode.</param>
		public void Begin(SpriteBlendMode blendMode, SpriteSortMode sortMode)
		{
			Begin (blendMode, sortMode, Matrix.Identity);
		}

		/// <summary>
		/// Begins this sprite batch, so we can draw sprites after.
		/// </summary>
		/// <param name="blendMode">Blend mode.</param>
		/// <param name="sortMode">Sort mode.</param>
		/// <param name="transMatrix">Transformation matrix.</param>
		public void Begin(SpriteBlendMode blendMode, SpriteSortMode sortMode, Matrix transMatrix)
		{
			if (_active) 
			{
				var batcher = new SpriteBatch (_graphicsDevice);
				batcher.Begin (blendMode, sortMode);
				_batchers.Add (batcher);
				return;
			}
		
			Count = 0;
			_textures.Clear();
			_sortMode = SpriteSortMode.FrontToBack;

			switch (blendMode)
			{
			case SpriteBlendMode.None:
				_states.BlendMode = SFML.Graphics.BlendMode.None;
				break;
			case SpriteBlendMode.Alpha:
				_states.BlendMode = SFML.Graphics.BlendMode.Alpha;
				break;
			case SpriteBlendMode.Additive:
				_states.BlendMode = SFML.Graphics.BlendMode.Add;
				break;
			case SpriteBlendMode.Multiply:
				_states.BlendMode = SFML.Graphics.BlendMode.Multiply;
				break;
			}
			_activeTexture = null;
			_states.Transform = transMatrix.ToSfml ();
			_active = true;
		}

		/// <summary>
		/// Ends this vertex batch, so we can not draw any more
		/// sprites.
		/// </summary>
		public void End()
		{
			if (!_active) throw new Exception("Call Begin first.");

			if (_batchers.Count > 0)
			{
				_batchers [_batchers.Count - 1].End ();
				_batchers.RemoveAt (_batchers.Count - 1);
				return;
			}

			Enqueue();
			_active = false;
			Draw (_graphicsDevice, _states);
		}

		/// <summary>
		/// Draw the specified drawable
		/// </summary>
		/// <param name="drawable">Drawable.</param>
		public void Draw(IDrawable drawable)
		{
			Draw(drawable, SpriteEffects.None);
		}

		/// <summary>
		/// Draw the specified drawable using specified effects.
		/// </summary>
		/// <param name="drawable">Drawable.</param>
		/// <param name="effects">Effects.</param>
		public void Draw(IDrawable drawable, SpriteEffects effects)
		{
			drawable.Draw (this, effects);
		}

		/// <summary>
		/// Draw the specified texture with specified position, sourceRect, color, scale, origin, rotation and effects.
		/// </summary>
		/// <param name="texture">Texture.</param>
		/// <param name="position">Position.</param>
		/// <param name="sourceRect">Source rect.</param>
		/// <param name="color">Color.</param>
		/// <param name="scale">Scale.</param>
		/// <param name="origin">Origin.</param>
		/// <param name="rotation">Rotation.</param>
		/// <param name="effects">Effects.</param>
		public void Draw(Texture texture, Vector2 position, Rectangle sourceRect, Color color, Vector2 scale, Vector2 origin, float rotation, SpriteEffects effects = SpriteEffects.None)
		{
			if (!_active) throw new Exception("Call Begin first.");

			if (effects != SpriteEffects.None)
				scale *= ScaleEffectMultiplier.Get (effects);

			var batcher = this;

			if (_batchers.Count > 0)
				batcher = _batchers [_batchers.Count - 1];

			batcher.WriteQuad (texture.ToSfml (), position.ToSfml (), sourceRect.ToSfml (), color.ToSfml (), scale.ToSfml (), origin.ToSfml (), rotation);
		}

		/// <summary>
		/// Draw the specified font with specified text, characterSize, position, color, scale, origin, rotation, style and effects.
		/// </summary>
		/// <param name="font">Font.</param>
		/// <param name="text">Text.</param>
		/// <param name="characterSize">Character size.</param>
		/// <param name="position">Position.</param>
		/// <param name="color">Color.</param>
		/// <param name="scale">Scale.</param>
		/// <param name="origin">Origin.</param>
		/// <param name="rotation">Rotation.</param>
		/// <param name="style">Style.</param>
		/// <param name="effects">Effects.</param>
		public void Draw(Font font, string text, int characterSize, Vector2 position, Color color, Vector2 scale, Vector2 origin, float rotation, Text.Styles style, SpriteEffects effects = SpriteEffects.None)
		{
			if (!_active) throw new Exception ("Call Begin first.");

			if (effects != SpriteEffects.None)
				scale *= ScaleEffectMultiplier.Get (effects);

			var batcher = this;

			if (_batchers.Count > 0)
				batcher = _batchers [_batchers.Count - 1];

			batcher.WriteText (font.ToSfml (), text, (uint)characterSize, position.ToSfml (), color.ToSfml (), scale.ToSfml (), origin.ToSfml (), rotation, (SFML.Graphics.Text.Styles)style);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="Spooker.Graphics.SpriteBatch"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="Spooker.Graphics.SpriteBatch"/> in an unusable state. After
		/// calling <see cref="Dispose"/>, you must release all references to the <see cref="Spooker.Graphics.SpriteBatch"/>
		/// so the garbage collector can reclaim the memory that the <see cref="Spooker.Graphics.SpriteBatch"/> was occupying.</remarks>
		public void Dispose()
		{
			_active = false;
		}

		#endregion

		#region Private methods

		private void Draw(SFML.Graphics.RenderTarget target, SFML.Graphics.RenderStates states)
		{
			if (_active) throw new Exception("Call End first.");

			uint index = 0;
			foreach (var item in _textures)
			{
				states.Texture = item.Texture;

				target.Draw(_vertices, index, item.Count, SFML.Graphics.PrimitiveType.Quads, states);
				index += item.Count;
			}
		}

		private void Enqueue()
		{
			if (_queueCount > 0)
			{
			    if (_sortMode == SpriteSortMode.BackToFront) _textures.Reverse ();
				_textures.Add (new BatchedTexture {
						Texture = _activeTexture,
					Count = _queueCount
				});
			}
			_queueCount = 0;
		}

		private int Create(SFML.Graphics.Texture texture)
		{
			if (!_active) throw new Exception("Call Begin first.");

			if (texture != _activeTexture)
			{
				Enqueue();
				_activeTexture = texture;
			}

			if (Count >= (_vertices.Length / 4))
			{
				if (_vertices.Length < Max)
					Array.Resize(ref _vertices, Math.Min(_vertices.Length * 2, Max));
				else throw new Exception("Too many items");
			}

			_queueCount += 4;
			return 4 * Count++;
		}

		private unsafe void WriteQuad(SFML.Graphics.Texture texture, Vector2f position, SFML.Graphics.IntRect rec, SFML.Graphics.Color color, Vector2f scale,
			Vector2f origin, float rotation = 0)
		{
			var index = Create(texture);
			var pX = -origin.X * scale.X;
			var pY = -origin.Y * scale.Y;
			var sin = (float)Math.Sin(rotation);
			var cos = (float)Math.Cos(rotation);

			scale.X *= rec.Width;
			scale.Y *= rec.Height;

			fixed (SFML.Graphics.Vertex* fptr = _vertices)
			{
				var ptr = fptr + index;

				ptr->Position.X = pX * cos - pY * sin + position.X;
				ptr->Position.Y = pX * sin + pY * cos + position.Y;
				ptr->TexCoords.X = rec.Left;
				ptr->TexCoords.Y = rec.Top;
				ptr->Color = color;
				ptr++;

				pX += scale.X;
				ptr->Position.X = pX * cos - pY * sin + position.X;
				ptr->Position.Y = pX * sin + pY * cos + position.Y;
				ptr->TexCoords.X = rec.Left + rec.Width;
				ptr->TexCoords.Y = rec.Top;
				ptr->Color = color;
				ptr++;

				pY += scale.Y;
				ptr->Position.X = pX * cos - pY * sin + position.X;
				ptr->Position.Y = pX * sin + pY * cos + position.Y;
				ptr->TexCoords.X = rec.Left + rec.Width;
				ptr->TexCoords.Y = rec.Top + rec.Height;
				ptr->Color = color;
				ptr++;

				pX -= scale.X;
				ptr->Position.X = pX * cos - pY * sin + position.X;
				ptr->Position.Y = pX * sin + pY * cos + position.Y;
				ptr->TexCoords.X = rec.Left;
				ptr->TexCoords.Y = rec.Top + rec.Height;
				ptr->Color = color;
			}
		}

		private void WriteText(SFML.Graphics.Font font, string text, uint characterSize, Vector2f position, SFML.Graphics.Color color, Vector2f scale, Vector2f origin, float rotation, SFML.Graphics.Text.Styles style)
		{
			_str.Font = font;
			_str.DisplayedString = text;
			_str.Position = position;
			_str.Color = color;
			_str.Rotation = rotation;
			_str.Origin = origin;
			_str.Scale = scale;
			_str.Style = style;
			_str.CharacterSize = characterSize;

			_graphicsDevice.Draw(_str, _states);
		}

		#endregion
	}
}