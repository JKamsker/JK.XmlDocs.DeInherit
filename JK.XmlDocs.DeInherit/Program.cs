using CommandLine;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace XmlDocResolver
{
    public class Program
    {
        private static int Main(string[] args) => CommandLine.Parser.Default.ParseArguments<CommandOptions>(args).MapResult
        (
            (CommandOptions o) => Dereference(o.Paths, o.LookupPaths),
            errs => 1
        );

        private static int Dereference(IEnumerable<string> paths, IEnumerable<string> lookupPaths)
        {
            var docFiles = new List<XmlDocFile>();

            var documentationsToSave = paths.SelectMany(x => Directory.EnumerateFiles(x, "*.xml", SearchOption.AllDirectories)).Select(x => ParseDocFile(x).EnableSaving());
            var referenceDocuments = lookupPaths.SelectMany(x => Directory.EnumerateFiles(x, "*.xml", SearchOption.AllDirectories)).Select(x => ParseDocFile(x));

            docFiles.AddRange(documentationsToSave);
            docFiles.AddRange(referenceDocuments);

            foreach (var docFile in docFiles)
            {
                docFile.ResolveInheritDoc(docFiles);
            }

            foreach (var xmlDocFile in docFiles.Where(x => x.FilePath.Contains("Application.xml")))
            {
                xmlDocFile.Save();
            }

            return 0;
        }

        private static XmlDocFile ParseDocFile(string appPath)
        {
            var document = new XmlDocument();
            document.Load(appPath);

            var docFile = new XmlDocFile(document)
            {
                FilePath = appPath
            };
            Visit(docFile, document);
            return docFile;
        }

        private static void Visit(XmlDocFile docFile, XmlNode document)
        {
            foreach (var item in document.ChildNodes)
            {
                if (item is XmlElement xmlElement)
                {
                    if (string.Equals(xmlElement.Name, "assembly", StringComparison.OrdinalIgnoreCase))
                    {
                        var asmName = xmlElement.ChildNodes.OfType<XmlNode>().FirstOrDefault(x => string.Equals(x.Name, "name", StringComparison.OrdinalIgnoreCase));
                        var text = asmName?.InnerText;
                        if (!string.IsNullOrEmpty(text))
                        {
                            docFile.AssemblyName = text;
                        }
                        continue;
                    }

                    if (string.Equals(xmlElement.Name, "members", StringComparison.OrdinalIgnoreCase))
                    {
                        VisitMembers(docFile, xmlElement);
                        continue;
                    }

                    Visit(docFile, xmlElement);
                }
            }
        }

        private static void VisitMembers(XmlDocFile docFile, XmlElement xmlElement)
        {
            foreach (var childNode in xmlElement.ChildNodes.OfType<XmlElement>())
            {
                var memberName = childNode.GetAttributeValue("name");
                var summary = childNode.ChildNodes.OfType<XmlElement>().FirstOrDefault(x => string.Equals(x.Name, "summary", StringComparison.OrdinalIgnoreCase));

                if (string.IsNullOrEmpty(memberName) || summary == null)
                {
                    continue;
                }

                docFile.Members.Add(memberName, new XmlDocMember
                {
                    Name = memberName,
                    SummaryNode = summary,
                    MemberNode = childNode,
                    ParentFile = docFile
                });
            }
        }
    }
}