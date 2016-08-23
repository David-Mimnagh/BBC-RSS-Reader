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

        static void StoreValues(XmlNodeList xnListBBC, ref BBCInformation BBCinfo, Information info, XmlNodeList nodeList2, List<Information> news_list)
        {
            foreach (XmlNode xn in xnListBBC)
            {

                BBCinfo.title = xn["title"].InnerText;
                BBCinfo.description = xn["description"].InnerText;
                BBCinfo.link = xn["link"].InnerText;
            }


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
            BBCinfo.items = news_list;
        }
        static void ReadNewsFromWebsite(string url, XmlReader reader, XmlDocument xmlDoc, string filename)
        {
            if (reader != null)
            {
                StringBuilder sb = new StringBuilder();

                while (reader.Read())
                    sb.AppendLine(reader.ReadOuterXml());

                xmlDoc.LoadXml(sb.ToString());
            }
        }


        static void CheckForPrevious(XmlTextReader oldFile, string currentDate, ref BBCInformation BBCinfo, string url, XmlReader reader, XmlDocument xmlDoc, XmlNodeList xnListBBC, Information info, XmlNodeList nodeList2, List<Information> news_list)
        {

            DateTime current = DateTime.Now;
            string oldDate = current.AddHours(-1).ToString("yyyy-MM-dd-HH");

            oldFile = new XmlTextReader("feed/" + oldDate + ".json"); // make a file to load the old file

            if (!Directory.Exists("feed"))
            {
                System.IO.Directory.CreateDirectory("feed");
                if (!File.Exists("feed/" + oldDate + ".json"))
                {
                    string newfile = "feed/" + currentDate + ".json"; // make a filename to load that file

                    Console.WriteLine(" No file found.\n Saving latest news...");
                    ReadNewsFromWebsite(url, reader, xmlDoc, newfile); // Read news from website and save to XML doc
                    StoreValues(xnListBBC, ref BBCinfo, info, nodeList2, news_list); // Store values in list
                    WriteToFile(BBCinfo, newfile); // write new list to file
                    Console.WriteLine(" Saved!\n\n Press any key to exit.");
                    Console.Read();
                    Environment.Exit(1); // exit
                }
            }
        }


        static void CompareANDConvert(string olddate, string url, string CurrentFile, XmlDocument xmlDoc, BBCInformation BBCinfo, Information info, List<Information> news_list, XmlReader reader)
        {
            DateTime currentDate = DateTime.Now;

            string oldDate = currentDate.AddHours(-1).ToString("yyyy-MM-dd-HH");

            XmlTextReader oldFile = new XmlTextReader("feed/" + oldDate + ".json"); // make a filename to load that file

            string oldFileName = "feed/" + oldDate + ".json"; // make a filename to load that file

            string newDate = DateTime.Now.ToString("yyyy-MM-dd-HH"); // get the current date and time
            string afterComparisonFilename = "feed/" + newDate + ".json"; // create a new file with the website data
            string oldFileData;

            // Load the documents
            using (StreamReader r = new StreamReader(oldFileName))
            {
                oldFileData = r.ReadToEnd();
            }

            XmlDocument XMLDocument1 = (XmlDocument)JsonConvert.DeserializeXmlNode(oldFileData, "title");
            XmlDocument XMLDocument2 = xmlDoc;

            //Node list for BBC Main page
            XmlNodeList xnListBBC = xmlDoc.SelectNodes("/rss/channel");
            // Get a list of all news title nodes
            XmlNodeList nodeList1 = XMLDocument1.SelectNodes("title/items");
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
            StoreValues(xnListBBC, ref BBCinfo, info, nodeList2, news_list);
            WriteToFile(BBCinfo, afterComparisonFilename);
        }


        static void WriteToFile(BBCInformation BBCinfo, string filename, bool append = true)
        {

            StreamWriter sw = new StreamWriter(filename, append);
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
            XmlTextReader oldFile = null;
            string date = DateTime.Now.ToString("yyyy-MM-dd-HH");
            string filename = "feed/" + date + ".json";

            XmlNodeList xnListBBC = xmlDoc.SelectNodes("/rss/channel");
            XmlNodeList xnListNews = xmlDoc.SelectNodes("/rss/channel/item");


            CheckForPrevious(oldFile, date, ref BBCinfo, url, reader, xmlDoc, xnListBBC, info, xnListNews, news_list); // Nested call to Read from website if previous file was found.

            ReadNewsFromWebsite(url, reader, xmlDoc, filename);

            CompareANDConvert(date, url, filename, xmlDoc, BBCinfo, info, news_list, reader);
        }
    }
}
