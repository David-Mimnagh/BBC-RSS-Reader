using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.Xml;
using XmlDiffLib;

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


        static void ReadFileFromWebsite(string url, XmlReader reader, XmlDocument xmlDoc, string filename)
        {
            if (reader != null)
            {
                StringBuilder sb = new StringBuilder();

                while (reader.Read())
                    sb.AppendLine(reader.ReadOuterXml());

                xmlDoc.LoadXml(sb.ToString());
                xmlDoc.Save(filename);
            }
        }

        static void CompareANDConvert(string olddate, string url, string CurrentFile, XmlDocument xmlDoc, BBCInformation BBCinfo, Information info, List<Information> news_list, XmlReader reader)
        {
            DateTime currentDate = DateTime.Now;
           
            string oldDate = currentDate.AddHours(-1).ToString("yyyy-MM-dd-HH");
            
            XmlTextReader oldFile = new XmlTextReader("F:/Repositories/BBC RSS Reader/feed/" + oldDate + ".xml"); // make a filename to load that file
            if (oldFile == null)
            {
                Console.WriteLine(" No file found. Saving latest news, press any key to exit.");
                Console.Read();
                Environment.Exit(1);
            }
            string newDate = DateTime.Now.ToString("yyyy-MM-dd-HH"); // get the current date and time
            string afterComparisonFilename = "F:/Repositories/BBC RSS Reader/feed/" + newDate + ".json"; // create a new file with the website data
            
            // Load the documents
            XmlDocument XMLDocument1 = new XmlDocument();
            XMLDocument1.Load(oldFile);
            XmlDocument XMLDocument2 = new XmlDocument();
            XMLDocument2.Load(CurrentFile);

            //Node list for BBC Main page
            XmlNodeList xnListBBC = xmlDoc.SelectNodes("/rss/channel");
            // Get a list of all news title nodes
            XmlNodeList nodeList1 = XMLDocument1.SelectNodes("/rss/channel/item");
            XmlNodeList nodeList2 = XMLDocument2.SelectNodes("/rss/channel/item");

            // Get the root Xml element
            XmlElement rootList1 = XMLDocument1.DocumentElement;
            XmlElement rootList2 = XMLDocument2.DocumentElement;

            // Get a list of all news titles
            XmlNodeList title_List1 = rootList1.GetElementsByTagName("title");
            XmlNodeList title_List2 = rootList2.GetElementsByTagName("title");


           foreach (XmlNode xOn in nodeList1) //Xml Old Node
            {
                foreach (XmlNode xNn in nodeList2)//Xml New Node
                {
                    if (xNn["title"] != null)
                    {
                        if (xOn["title"].InnerText == xNn["title"].InnerText)
                        {
                            xNn.RemoveAll();
                        }

                    }
                }
            }
                foreach (XmlNode xn in xnListBBC)
                {

                    BBCinfo.title = xn["title"].InnerText;
                    BBCinfo.description = xn["description"].InnerText;
                    BBCinfo.link = xn["link"].InnerText;
                }
                BBCinfo.items = news_list;

                foreach (XmlNode xNn in nodeList2)//Xml New Node
                {
                    if (xNn["title"] != null)
                    {
                        info.title = xNn["title"].InnerText;
                        info.description = xNn["description"].InnerText;
                        info.link = xNn["link"].InnerText;
                        info.date = xNn["pubDate"].InnerText;
                        news_list.Add(info);
                    }
                }
                WriteToFile(BBCinfo, afterComparisonFilename);
        }


        static void WriteToFile(BBCInformation BBCinfo, string filename, bool append = true)
        {

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

            string date = DateTime.Now.ToString("yyyy-MM-dd-HH");
            string filename = "F:/Repositories/BBC RSS Reader/feed/" + date + ".xml";

            XmlNodeList xnListBBC = xmlDoc.SelectNodes("/rss/channel");
            XmlNodeList xnListNews = xmlDoc.SelectNodes("/rss/channel/item");

            ReadFileFromWebsite(url, reader, xmlDoc, filename);
            
            CompareANDConvert(date, url, filename, xmlDoc, BBCinfo, info, news_list, reader);
        }
    }
}
