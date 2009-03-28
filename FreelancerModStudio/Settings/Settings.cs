using System.Collections.Generic;

namespace FreelancerModStudio.Settings
{
	public class Settings
	{
		public SettingsData Data = new SettingsData();

		public void Load(System.IO.Stream stream)
		{
			this.Data = (SettingsData)Serializer.Load(stream, this.Data.GetType());
		}

		public void Load(string path)
		{
			this.Data = (SettingsData)Serializer.Load(path, this.Data.GetType());
		}

		public void Save(System.IO.Stream stream)
		{
			Serializer.Save(stream, this.Data);
		}

		public void Save(string path)
		{
			Serializer.Save(path, this.Data);
		}

		[System.Xml.Serialization.XmlRootAttribute("FreelancerModStudio-Settings-1.0")]
		public class SettingsData
		{
			public General General = new General();
			public Forms Forms = new Forms();
		}

		public class Main
		{
			[System.Xml.Serialization.XmlArrayItem("RecentFile")]
			public List<string> RecentFiles;

			public ushort RecentFilesCount = 4;

			public System.Drawing.Point Location;
			public System.Drawing.Size Size;

			public bool Maximized;
			public bool FullScreen;
		}

		public class General
		{
			public string Language;

			public AutoUpdate AutoUpdate = new AutoUpdate();
		}

		public class AutoUpdate
		{
			public bool Enabled = true;
			public bool SilentDownload = true;
			public string NewestVersionFile = "http://freelancermodstudio.googlecode.com/files/NewestVersion.txt";

			public System.DateTime LastCheck;
			
			public Update Update = new Update();
			public Proxy Proxy = new Proxy();
		}

		public class Update
        {
            public string FileName;
            public bool Downloaded = false;
            public bool Installed = false;
			public bool SilentInstall;
		}

		public class Proxy
		{
			public bool Enabled = false;
			public string Uri;
		}

		public class Forms
		{
			public Main Main = new Main();
			public NewMod NewMod = new NewMod();
		}

		public class NewMod
		{
			public string ModSaveLocation = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "Mods");

			public System.Drawing.Size Size;
		}
	}
}