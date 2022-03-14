using System.Collections.Generic;
using System.IO;
using System.Text;

using SystemdManager.UnitParser;

namespace SystemdManager.Objects;

public class Service
{

    public string Name { get; set; }
    public string Raw { get; set; }
    public string FullName { get; set; }
    public List<SystemdSection> Sections { get; set; }
    public ServiceStatus Status { get; set; }

    // TODO: Serialize comments
    public string Serialize()
    {
        var sb = new StringBuilder();
        using var writer = new StringWriter(sb)
        {
            NewLine = "\n"
        };

        var idx = 0;
        foreach (var section in Sections)
        {
            idx++;
            writer.WriteLine($"[{section.Name}]");
            foreach (var property in section.Properties)
            {
                writer.WriteLine($"{property.Name}={property.Value}");
            }

            if (idx < Sections.Count)
            {
                writer.WriteLine();
            }
        }

        return writer.ToString();
    }

}
