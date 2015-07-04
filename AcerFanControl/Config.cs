using System;
using System.Xml.Linq;

namespace AcerFanControl
{
    public class Config
    {
        private readonly XDocument document;
        private readonly string path;

        public Config(string name)
        {
            path = name + ".xml";
            try { document = XDocument.Load(path); }
            catch { document = new XDocument(new XElement(name)); }
        }

        public T GetValue<T>(string name, T defaultValue = default(T)) where T : IConvertible
        {
            try
            {
                XElement element = document.Root.Element(name);
                return (T)Convert.ChangeType(element.Value, typeof(T));
            }
            catch { return defaultValue; }
        }

        public void SetValue(string name, object value)
        {
            XElement element = document.Root.Element(name);
            if (element == null)
                document.Root.Add(element = new XElement(name));
            element.SetValue(value);
            document.Save(path);
        }
    }
}