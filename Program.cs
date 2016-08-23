using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json; // Using this for the Json file writing and reading
using System.IO;
using System.Xml; // Using this for reading the information from the website and comparisons between the old and new file.


namespace BBC_RSS_Reader
{
    class Program
    {
        /* Struct containing all information for the website*/
        struct BBCInformation
        {
            public string title, link, description;
            public List<Information> items;
        }

        /* Struct containing just individual news story information for the website*/
        struct Information
        {
            public string title, description, link, date;
        }

        /*-----------------------------------Method for storing the news elements within the struct--------------------------------------------*/
        /*  XmlNodeList xnListBBC: This holds the nodes for the whole BBC struct. 
            BBCInformation BBCinfo: This is the object holding the information for the whole site.
            Information info: This object holds the information for the individual news stories.
            XmlNodeList nodeList2: This node list has the list of all nodes containing information relating to the individual stories
            List<Information> news_list: The purpose of this object is to hold the final list of news stories obtained from the website.*/
        /*-------------------------------------------------------------------------------------------------------------------------------------*/
        static void StoreValues(XmlNodeList xnListBBC, ref BBCInformation BBCinfo, Information info, XmlNodeList nodeList2, List<Information> news_list)
        {
            // For each node within the xnListBBC 
            foreach (XmlNode xn in xnListBBC) // Set the title, description and link to the corresponding struct element.
            {

                BBCinfo.title = xn["title"].InnerText;
                BBCinfo.description = xn["description"].InnerText;
                BBCinfo.link = xn["link"].InnerText;
            }

            // For each node within node list 2 
            foreach (XmlNode xNn in nodeList2)// Set the title, description, link and publication date to the corresponding struct element.
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

        /*-----------------------------------Method for storing the news elements within the struct--------------------------------------------*/
        /*  XmlReader reader: Object used for reading the information from the website. 
            XmlDocument xmlDoc: Information from the website is then saved to the xmlDoc. */
        /*-------------------------------------------------------------------------------------------------------------------------------------*/
        static void ReadNewsFromWebsite(XmlReader reader, XmlDocument xmlDoc)
        {
            if (reader != null) // aslong as the reader is not empty
            {
                StringBuilder sb = new StringBuilder(); // create a new string builder object

                while (reader.Read()) // while there is still text to read
                    sb.AppendLine(reader.ReadOuterXml()); // add the line to the string

                xmlDoc.LoadXml(sb.ToString()); // save the string to the document
            }
        }

        /*---------------------------------------------------------------------Method for Checking for the previous document-------------------------------------------------------------------------*/
        /*  string currentDate: String that holds the current date (assigned in main).
            ref BBCInformation BBCinfo: This is the object holding the information for the whole site. (NOTE: REFERENCE AS WANTED TO LOOK AT LOCATION OF DATA, NOT ACTUAL VALUE - DUE TO NESTED CALL).
            XmlDocument xmlDoc: Information from the website is then saved to the xmlDoc.
            XmlNodeList xnListBBC: Node list used for holding the whole website information. 
            Information info: Information object used to assign values to struct
            XmlNodeList nodeList2: This node list has the list of all nodes containing information relating to the individual stories
            List<Information> news_list: The purpose of this object is to hold the final list of news stories obtained from the website.*/
        /*------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        static void CheckForPrevious(string currentDate, ref BBCInformation BBCinfo,
            XmlReader reader, XmlDocument xmlDoc, XmlNodeList xnListBBC, Information info, XmlNodeList nodeList2, List<Information> news_list)
        {
            DateTime current = DateTime.Now; // obtaining current date and time
            string oldDate = current.AddHours(-1).ToString("yyyy-MM-dd-HH"); // converting it to the right format

            if (!Directory.Exists("feed"))
            {
                System.IO.Directory.CreateDirectory("feed");
            }
            if (!File.Exists("feed/" + oldDate + ".json"))
            {
                string newfile = "feed/" + currentDate + ".json"; // make a filename to load that file

                if (File.Exists(newfile))
                {
                    Console.WriteLine(" Already have the latest news for this hour!\n\n Press any key to exit.");
                    Console.Read();
                    Environment.Exit(1); // exit
                }
                else
                {
                    Console.WriteLine(" No file found.\n Saving latest news...");
                    ReadNewsFromWebsite(reader, xmlDoc); // Read news from website and save to XML doc
                    StoreValues(xnListBBC, ref BBCinfo, info, nodeList2, news_list); // Store values in list
                    WriteToFile(BBCinfo, newfile); // write new list to file
                    Console.WriteLine(" Saved!\n\n Press any key to exit.");
                    Console.Read(); // Read input to temporarily pause program
                    Environment.Exit(1); // exit
                }
            }

        }

        /*--------------------Method for Comparing the previous and newest document and then converting them to JSON--------------------------*/
        /*  string oldDate: String that holds the current date (assigned in main).
            XmlDocument xmlDoc: Information from the website is then saved to the xmlDoc.
            BBCInformation BBCinfo: This is the object holding the information for the whole site.
            Information info: Information object used to assign values to struct 
            List<Information> news_list: The purpose of this object is to hold the final list of news stories obtained from the website.
            XmlReader reader: Object used for reading the information from the website.*/
        /*-------------------------------------------------------------------------------------------------------------------------------------*/

        static void CompareANDConvert(string oldDate, XmlDocument xmlDoc, BBCInformation BBCinfo, Information info, List<Information> news_list, XmlReader reader)
        {
            DateTime currentDate = DateTime.Now; // obtaining the current Date and time
            oldDate = currentDate.AddHours(-1).ToString("yyyy-MM-dd-HH"); // using the current date to obtain the correct date and time from an hour previous

            string oldFileName = "feed/" + oldDate + ".json"; // make a filename to load the old file
            string newDate = DateTime.Now.ToString("yyyy-MM-dd-HH"); // get the current date and time in string format
            string afterComparisonFile = "feed/" + newDate + ".json"; // create a new file for the most up to date website data
            string oldFileData; // string that holds the data from the old file

            // Using a streamReader to load the old file and read its contents to a string
            using (StreamReader r = new StreamReader(oldFileName))
            {
                oldFileData = r.ReadToEnd(); // assigning contents to the string
            }

            XmlDocument XMLDocument1 = (XmlDocument)JsonConvert.DeserializeXmlNode(oldFileData, "title"); // converting to XML format for comparison
            XmlDocument XMLDocument2 = xmlDoc; // second (up to date document) being assigned for comparison

            //Node list for BBC Main page
            XmlNodeList xnListBBC = xmlDoc.SelectNodes("/rss/channel");
            // Get a list of all news title nodes
            XmlNodeList nodeList1 = XMLDocument1.SelectNodes("title/items");
            XmlNodeList nodeList2 = XMLDocument2.SelectNodes("/rss/channel/item");

            // The list of Root elements within the documents
            XmlElement rootList1 = XMLDocument1.DocumentElement;
            XmlElement rootList2 = XMLDocument2.DocumentElement;

            // Get a list of all news titles
            XmlNodeList title_List1 = rootList1.GetElementsByTagName("title");
            XmlNodeList title_List2 = rootList2.GetElementsByTagName("title");

            //for each of the nodes within node list 1 (old doc)
            foreach (XmlNode xOn in nodeList1) //Xml Old Node
            {
                foreach (XmlNode xNn in nodeList2)//for each of the nodes within node list 1 (new doc)
                {
                    if (xNn["title"] != null) // providing it is not a null title
                    {
                        if (xOn["title"].InnerText == xNn["title"].InnerText) // if there is a match
                        {
                            xNn.RemoveAll(); // delete ( set to null
                        }

                    }
                }
            }
            StoreValues(xnListBBC, ref BBCinfo, info, nodeList2, news_list); // store the new information (new file)
            if (File.Exists(afterComparisonFile)) // Check to ensure that the multiple file is not being written to more than once if ran again.
            {
                Console.WriteLine(" Already compared news for this hour!\n\n Press any key to exit.");
                Console.Read();
                Environment.Exit(1); // exit
            }
            WriteToFile(BBCinfo, afterComparisonFile); // writing information to file
        }

        /*-----------------------------------------Method for Writing information to file------------------------------------------------------*/
        /*  BBCInformation BBCinfo: This is the object holding the information for the whole site.
            string filename: String to hold the file name to which the information will be written to
            bool append: Optional argument to ensure that the file is only opened and closed once.*/
        /*-------------------------------------------------------------------------------------------------------------------------------------*/
        static void WriteToFile(BBCInformation BBCinfo, string filename, bool append = true)
        {

            StreamWriter sw = new StreamWriter(filename, append); // new stream writer object created with the filename and append variable (true)
            string json = JsonConvert.SerializeObject(BBCinfo, Newtonsoft.Json.Formatting.Indented); // making a string to hold the serialized information
            JsonWriter writer = new JsonTextWriter(sw); //New Json writer object created that uses the stream writer information
            writer.Formatting = Newtonsoft.Json.Formatting.Indented; // ensuring proper formatting is used
            sw.Write(json); // writing the information

            sw.Close(); // clossing the file when the program is done with it.
        }

        static void Main(string[] args)
        {
            List<Information> news_list = new List<Information>(); // News list object to store information of specific news stories
            BBCInformation BBCinfo = new BBCInformation(); // BBCInfo holds the information for the BBC title page, description and link
            Information info = new Information(); // Info object holds the specific information for an individual story
            XmlDocument xmlDoc = new XmlDocument(); // Document contatining current xml version

            string url = "http://feeds.bbci.co.uk/news/uk/rss.xml"; // URL to obtain information from
            XmlReader reader = XmlReader.Create(url); // Reader for obtaining information from URL

            string date = DateTime.Now.ToString("yyyy-MM-dd-HH"); // Current Date and Time in proper format
            string filename = "feed/" + date + ".json"; // Filename for current version

            XmlNodeList xnListBBC = xmlDoc.SelectNodes("/rss/channel"); // Where to look for BBC information in XML download from URL
            XmlNodeList xnListNews = xmlDoc.SelectNodes("/rss/channel/item"); // Where to look for BBC information in XML download from URL


            CheckForPrevious(date, ref BBCinfo, reader, xmlDoc, xnListBBC, info, xnListNews, news_list); // Nested call to Read from website if previous file was found.
            ReadNewsFromWebsite(reader, xmlDoc); // If previous file was found, the read the news from website for latest news then to make comparison.

            CompareANDConvert(date, xmlDoc, BBCinfo, info, news_list, reader); // Making comparison then converting back to JSON and saving. Nested call to StoreValues and WriteToFile made within this function.
        }
    }
}
