using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FreelancerModStudio.Settings
{
	public class News
	{
		public NewsData Data = new NewsData();

		public void Load(System.IO.Stream stream)
		{
			this.Data = (NewsData)Serializer.Load(stream, this.Data.GetType());
		}

		public void Load(string path)
		{
			this.Data = (NewsData)Serializer.Load(path, this.Data.GetType());
		}

		public void Save(System.IO.Stream stream)
		{
			Serializer.Save(stream, this.Data);
		}

		public void Save(string path)
		{
			Serializer.Save(path, this.Data);
		}

		[XmlRootAttribute("FreelancerModStudio-News-1.0")]
		public class NewsData
		{
			[XmlArrayItem("Headline")]
			public List<Headline> Headlines;
		}

		public class Headline
		{
			public DateTime Date;
			public string Text;
			public string Link;
		}
	}
}