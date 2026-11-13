using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rakan_eservices
{
    class XMLParser2
    {
        public string FormatXml(string xml)
        {
            try
            {
                System.Xml.Linq.XDocument doc = System.Xml.Linq.XDocument.Parse(xml);
                return doc.ToString();
            }
            catch (Exception)
            {
                // Handle and throw if fatal exception here; don't just ignore them
                return xml;
            }
        }
        public String getEleValue(String xmlStr, String ele)
        {
            int check = 0;
            String newStr = "";
            char c = ' ';
            int checkStr = 0;
            int i = 0;
            String comStr = "";

            while (i < xmlStr.Length)
            {

                c = xmlStr[i];

                if (c == '<' || check == 0)
                {

                    newStr = newStr + c;

                    if (c == '>')
                    {

                        if (newStr == ("<" + ele + ">") && xmlStr[i + 1] == '<')
                        {
                            comStr = "&nbsp";
                            newStr = "";
                            break;
                        }
                        if (newStr != ("<" + ele + ">"))
                        {
                            i++;
                            comStr = "";
                            newStr = "";

                        }
                        if (newStr == ("<" + ele + ">"))
                        {
                            comStr = "";
                            while (true)
                            {
                                i++;
                                comStr = comStr + xmlStr[i];
                                if (xmlStr[i + 1] == '<')
                                {
                                    break;
                                }
                            }
                            break;
                        }

                    }
                    else
                    {
                        i++;
                    }

                }
                else
                {
                    i++;
                }

            }


            return comStr;
        }
    }
}
