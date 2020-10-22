using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace XmlDocResolver
{
    public class XmlDocMember
    {
        public string Name { get; set; }
        public XmlElement SummaryNode { get; set; }

        public XmlDocFile ParentFile { get; set; }
        public XmlElement MemberNode { get; set; }

        public bool ResolveInheritDoc(IEnumerable<XmlDocFile> docFiles)
        {
            if (!ContainsInheritDoc())
            {
                return false;
            }

            var newNode = ParentFile.Document.CreateNode(SummaryNode.NodeType, SummaryNode.Name, SummaryNode.NamespaceURI);

            foreach (var elem in SummaryNode.ChildNodes.OfType<XmlNode>())
            {
                if (!string.Equals(elem.Name, "inheritdoc", StringComparison.OrdinalIgnoreCase))
                {
                    var clone = SummaryNode.OwnerDocument.ImportNode(elem, true);

                    newNode.AppendChild(clone);
                    continue;
                }

                if (TryDeepDRef(docFiles, newNode, elem))
                {
                    continue;
                }

                newNode.AppendChild(elem);
            }

            MemberNode.ReplaceChild(newNode, SummaryNode);
            SummaryNode = (XmlElement)newNode;

            //if (ContainsInheritDoc(newNode.ChildNodes.OfType<XmlElement>()))
            //{
            //    ResolveInheritDoc(docFiles);
            //}

            return true;
        }

        private bool TryDeepDRef(IEnumerable<XmlDocFile> docFiles, XmlNode newNode, XmlNode crefNode)
        {
            var crefValue = crefNode.GetAttributeValue("cref");
            if (TryGetCref(crefValue, docFiles, out var refMember))
            {
                foreach (var elem in refMember.SummaryNode.OfType<XmlNode>())
                {
                    if (!string.Equals(elem.Name, "inheritdoc", StringComparison.OrdinalIgnoreCase))
                    {
                        var clone = SummaryNode.OwnerDocument.ImportNode(elem, true);
                        newNode.AppendChild(clone);
                        continue;
                    }

                    if (TryDeepDRef(docFiles, newNode, elem))
                    {
                        continue;
                    }

                    var clone1 = SummaryNode.OwnerDocument.ImportNode(elem, true);
                    newNode.AppendChild(clone1);
                }
                return true;
            }

            //We haven't found any reference...continue
            return false;
        }

        private bool TryGetCref(string crefValue, IEnumerable<XmlDocFile> docFiles, out XmlDocMember member)
        {
            if (TryGetCrefImpl(crefValue, ParentFile, out member) && member.SummaryNode.HasChildNodes)
            {
                return true;
            }

            foreach (var docFile in docFiles)
            {
                if (TryGetCrefImpl(crefValue, docFile, out member) && member.SummaryNode.HasChildNodes)
                {
                    return true;
                }
            }

            return false;

            static bool TryGetCrefImpl(string crefValue, XmlDocFile docFile, out XmlDocMember member)
            {
                return docFile.Members.TryGetValue(crefValue, out member);
            }
        }

        public bool ContainsInheritDoc(IEnumerable<XmlElement> elements = null)
        {
            return GetInheritDocs(elements).Any();
        }

        private IEnumerable<XmlElement> GetInheritDocs(IEnumerable<XmlElement> elements = null)
        {
            elements ??= SummaryNode.ChildNodes.OfType<XmlElement>();
            return elements.Where(x => string.Equals(x.Name, "inheritdoc", StringComparison.OrdinalIgnoreCase));
        }
    }
}