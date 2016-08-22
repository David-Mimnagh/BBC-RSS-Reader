using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.Xml;

namespace BBC_RSS_Reader
{
    class Program
    {

        struct BBCInformation
        {
            public string title, link, description;
            public List<Information> items;
        }
        struct Information
        {
            public string title, description, link, date;
        }

        static void WriteToFile(BBCInformation BBCinfo, bool append = true)
        {

            string date = DateTime.Now.ToString("yyyy-MM-dd-HH");
            string filename = "F:/Repositories/NewsApplication/feed/" + date + ".json";

            StreamWriter sw = new StreamWriter(filename, append);
            sw.WriteLine(BBCinfo);


            string json = JsonConvert.SerializeObject(BBCinfo, Newtonsoft.Json.Formatting.Indented);
            JsonWriter writer = new JsonTextWriter(sw);
            writer.Formatting = Newtonsoft.Json.Formatting.Indented;
            sw.Write(json);

            sw.Close();
        }
        static void Main(string[] args)
        {

            List<Information> news_list = new List<Information>();
            BBCInformation BBCinfo = new BBCInformation();
            Information info = new Information();

            XmlDocument xmlDoc = new XmlDocument();

            string url = "http://feeds.bbci.co.uk/news/uk/rss.xml";
            XmlReader reader = XmlReader.Create(url);

            XmlNodeList xnListBBC = xmlDoc.SelectNodes("/rss/channel");
            XmlNodeList xnListNews = xmlDoc.SelectNodes("/rss/channel/item");

            if (reader != null)
            {
                StringBuilder sb = new StringBuilder();

                while (reader.Read())
                    sb.AppendLine(reader.ReadOuterXml());

                xmlDoc.LoadXml(sb.ToString());
            }

            foreach (XmlNode xn in xnListBBC)
            {

                BBCinfo.title = xn["title"].InnerText;
                BBCinfo.description = xn["description"].InnerText;
                BBCinfo.link = xn["link"].InnerText;
            }
            BBCinfo.items = news_list;

            foreach (XmlNode xn in xnListNews)
            {
                info.title = xn["title"].InnerText;
                info.description = xn["description"].InnerText;
                info.link = xn["link"].InnerText;
                info.date = xn["pubDate"].InnerText;
                news_list.Add(info);
            }

            WriteToFile(BBCinfo);
        }
    }
}
