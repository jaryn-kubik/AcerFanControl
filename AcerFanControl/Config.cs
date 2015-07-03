using System;
using System.Xml.Linq;

namespace AcerFanControl
{
    public static class Config
    {
        private const string xmlPath = "AcerFanControl.xml";
        private static readonly XDocument document;

        static Config()
        {
            try { document = XDocument.Load(xmlPath); }
            catch { document = new XDocument(new XElement("AcerFanControl")); }
        }

        public static int Temperature
        {
            get { return getValue("Temperature", 80); }
            set { setValue("Temperature", value); }
        }

        private static T getValue<T>(string name, T defaultValue = default(T)) where T : IConvertible
        {
            try
            {
                XElement element = document.Root.Element(name);
                return (T)Convert.ChangeType(element.Value, typeof(T));
            }
            catch { return defaultValue; }
        }

        private static void setValue(string name, object value)
        {
            XElement element = document.Root.Element(name);
            if (element == null)
                document.Root.Add(element = new XElement(name));
            element.SetValue(value);
            document.Save(xmlPath);
        }
    }
}