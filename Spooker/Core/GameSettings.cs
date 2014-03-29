﻿//-----------------------------------------------------------------------------
// GameSettings.cs
//
// Spooker Open Source Game Framework
// Copyright (C) Indie Armory. All rights reserved.
// Website: http://indiearmory.com
// Other Contributors: None
// License: MIT
//-----------------------------------------------------------------------------

using System;
using System.IO;
using System.Xml.Serialization;
using SFML.Window;
using Spooker.Time;

namespace Spooker.Core
{
	////////////////////////////////////////////////////////////
	/// <summary>
	/// Class used for storing all game settings (is used when
	/// creating instance of game window).
	/// </summary>
	////////////////////////////////////////////////////////////
	[Serializable]
    public class GameSettings
    {
		#region Variables
		internal VideoMode VideoMode = new VideoMode (800, 600);
		internal ContextSettings ContextSettings = new ContextSettings (32, 0, 4);
		#endregion

		#region Properties
		/// <summary>Specifies width of game window(in pixels)</summary>
		public uint Width
		{
			get { return VideoMode.Width; }
			set { VideoMode.Width = value; }
		}

		/// <summary>Specifies height of game window(in pixels)</summary>
		public uint Height
		{
			get { return VideoMode.Height; }
			set { VideoMode.Height = value; }
		}

		/// <summary>Specifies antialiasing level of game window</summary>
		public uint AntialiasingLevel
		{
			get { return ContextSettings.AntialiasingLevel; }
			set { ContextSettings.AntialiasingLevel = value; }
		}

		/// <summary>Specifies depth bits of game window</summary>
		public uint DepthBits
		{
			get { return ContextSettings.DepthBits; }
			set { ContextSettings.DepthBits = value; }
		}

		/// <summary>Specifies stencil bits of game window</summary>
		public uint StencilBits
		{
			get { return ContextSettings.StencilBits; }
			set { ContextSettings.StencilBits = value; }
		}

		/// <summary>Specifies bits per pixel of game window</summary>
		public uint BitsPerPixel
		{
			get { return VideoMode.BitsPerPixel; }
			set { VideoMode.BitsPerPixel = value; }
		}

        /// <summary>Relative path to icon file what will be used as icon for game window.</summary>
        public string Icon = "none";

		/// <summary>Root directory of contet manager</summary>
		public string ContentDirectory = "Content";

		/// <summary>Directory in root directory of content manager,from what will audio cache sounds</summary>
		public string SoundDirectory = "Sounds";

		/// <summary>Extension of sounds to cache</summary>
		public string SoundExtension = "ogg";

		/// <summary>Determines how fast will be update rate of game.</summary>
		public GameTime UpdaterateLimit = GameTime.FromMilliseconds (1);

		/// <summary>Sets default background color of empty rendering window</summary>
		public Graphics.Color ClearColor = Graphics.Color.Black;

		/// <summary>Title of game window</summary>
		public string Title = "Spooker Game";

		/// <summary>Style of game window borders.</summary>
		public Styles Style = Styles.Default;

		/// <summary>Sets, if game will wait for vertical sync or not.</summary>
		public bool VerticalSync = false;

		/// <summary>Determines how fast will be frame rate of game.</summary>
		public uint FramerateLimit = 32;

		/// <summary>Sets major version of game.</summary>
		public uint MajorVersion;

		/// <summary>Sets minor version of game.</summary>
		public uint MinorVersion;
		#endregion

		////////////////////////////////////////////////////////////
		/// <summary>
		/// Creates new instance of GameSettings class using default
		/// parameters
		/// </summary>
		////////////////////////////////////////////////////////////
		public GameSettings()
		{
		}

		////////////////////////////////////////////////////////////
		/// <summary>
		/// Creates new instance of GameSettings using parameters
		/// loaded from settings file.
		/// </summary>
		////////////////////////////////////////////////////////////
		public GameSettings(string filename)
		{
			var reader = new StreamReader (filename);
			var ser = new XmlSerializer (GetType ());
			var temp = (GameSettings)ser.Deserialize (reader);
			reader.Close ();

			Width = temp.Width;
			Height = temp.Height;
			AntialiasingLevel = temp.AntialiasingLevel;
			DepthBits = temp.DepthBits;
			StencilBits = temp.StencilBits;
			BitsPerPixel = temp.BitsPerPixel;
			UpdaterateLimit = temp.UpdaterateLimit;
			ClearColor = temp.ClearColor;
			Title = temp.Title;
			Style = temp.Style;
			VerticalSync = temp.VerticalSync;
			FramerateLimit = temp.FramerateLimit;
			MajorVersion = temp.MajorVersion;
			MinorVersion = temp.MinorVersion;
			ContentDirectory = temp.ContentDirectory;
			SoundDirectory = temp.SoundDirectory;
			SoundExtension = temp.SoundExtension;
		    Icon = temp.Icon;
		}
    }
}
