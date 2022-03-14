using System.Text.RegularExpressions;

namespace SystemdManager.UnitParser;

public class UnitFileParser
{

    private static readonly Regex PropertyRegex = new(@"(.*)=(.*)", RegexOptions.Compiled, TimeSpan.FromSeconds(2));

    private readonly string[] _content;

    public UnitFileParser(string[] content)
    {
        _content = content;
    }

    // TODO: Make this better
    // TODO: Parse comments
    public SystemdService Parse()
    {
        var map = new Dictionary<SystemdSection, List<SystemdProperty>>();
        SystemdSection currentSection = null;
        foreach (var line in _content)
        {
            if (line.StartsWith("#"))
            {
                continue;
            }

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                currentSection = new SystemdSection { Name = line[1..^1] };
                map[currentSection] = new List<SystemdProperty>();
                continue;
            }

            var propertyRegexResult = PropertyRegex.Match(line);
            if (propertyRegexResult.Groups.Count == 3)
            {
                var propName = propertyRegexResult.Groups[1];
                var propValue = propertyRegexResult.Groups[2];
                var property = new SystemdProperty
                {
                    Name = propName.Value,
                    Value = propValue.Value
                };

                map[currentSection!].Add(property);
            }
        }

        return new SystemdService
        {
            Sections = map.Select(x =>
            {
                var (key, value) = x;
                key.Properties = value;
                return key;
            }).ToList()
        };
    }

}
