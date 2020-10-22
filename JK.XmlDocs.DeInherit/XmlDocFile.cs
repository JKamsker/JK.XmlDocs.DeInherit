using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace XmlDocResolver
{
    public class XmlDocFile
    {
        public XmlDocFile(XmlDocument document)
        {
            Document = document;
        }

        public bool SavingEnabled { get; set; }

        public XmlDocument Document { get; }

        public string AssemblyName { get; set; }
        public Dictionary<string, XmlDocMember> Members { get; set; } = new Dictionary<string, XmlDocMember>(StringComparer.OrdinalIgnoreCase);
        public string FilePath { get; set; }

        public void ResolveInheritDoc(List<XmlDocFile> docFiles)
        {
            foreach (var member in Members.Values)
            {
                member.ResolveInheritDoc(docFiles.Where(x => x != null));
            }
        }

        public void Save()
        {
            if (SavingEnabled)
            {
                Document.Save(FilePath);
            }
        }

        public XmlDocFile EnableSaving()
        {
            SavingEnabled = true;
            return this;
        }
    }
}