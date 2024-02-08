using System;
using System.Xml;

#nullable disable
namespace Telegram_Remote_Control.ViewModel
{
    internal class ConfigSettings
    {
        XmlDocument xmlDocument;
        XmlElement xRoot;
        string pathToXml;

        public ConfigSettings(string path)
        {
            xmlDocument = new XmlDocument();
            xmlDocument.Load(path);

            this.pathToXml = path;

            if(xmlDocument != null)
            {
                xRoot = xmlDocument.DocumentElement;
            }
        }

        internal string GetValueByKey(string key)
        {
            string value = String.Empty;

            if(xRoot != null)
            {
                foreach(XmlNode node in xRoot.ChildNodes)
                {
                    if(node.Attributes.GetNamedItem("key").Value == key)
                    {
                        value = node.Attributes.GetNamedItem("value").Value;
                    }
                }
            }

            return value;
        }

        internal void SetValueByKey(string key, string value)
        {
            if (xRoot != null)
            {
                foreach (XmlNode node in xRoot.ChildNodes)
                {
                    if (node.Attributes.GetNamedItem("key").Value == key)
                    {
                        XmlNode attr = node.Attributes.GetNamedItem("value");

                        attr.Value = value;

                        node.Attributes.SetNamedItem(attr);

                        xmlDocument.Save(pathToXml);
                    }
                }
            }
        }
    }
}
