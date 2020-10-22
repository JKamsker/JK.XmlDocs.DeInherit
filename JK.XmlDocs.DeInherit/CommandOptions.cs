using System.Collections.Generic;
using CommandLine;

namespace XmlDocResolver
{
    public class CommandOptions
    {
        [Option('p', "paths", Required = true, HelpText = "Paths to the xmldocs to be dereferenced")]
        public IEnumerable<string> Paths { get; set; }

        [Option('l', "lookup", Required = false, HelpText = "Paths to additional read-only xmldocs for references")]
        public IEnumerable<string> LookupPaths { get; set; }
    }
}