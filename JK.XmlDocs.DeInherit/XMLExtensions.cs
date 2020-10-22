using System;
using System.Linq;
using System.Xml;

namespace XmlDocResolver
{
    public static class XMLExtensions
    {
        public static string GetAttributeValue(this XmlNode element, string attributeName)
        {
            return element.Attributes?.OfType<XmlAttribute>().FirstOrDefault(x => string.Equals(x.Name, attributeName, StringComparison.OrdinalIgnoreCase))?.Value;

            //return element.ChildNodes.OfType<XmlNode>().FirstOrDefault(x => string.Equals(x.Name, "name", StringComparison.OrdinalIgnoreCase));
        }
    }
}