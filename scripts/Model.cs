using System.Collections.Generic;
using System.Linq;
#nullable enable

namespace Production
{
    public class Model
    {
        public Dictionary<string, Fact> Facts { get; } = new Dictionary<string, Fact>();
        public List<Rule> Rules { get; } = new List<Rule>();

        public Model() { }

        public Model(Dictionary<string, Fact> facts, List<Rule> rules)
        {
            Facts = facts; Rules = rules;
        }

        public void AddFact(string name, string description)
        {
            Facts.Add(name, new Fact(name, description));
        }

        public void AddRule(IEnumerable<string> from, IEnumerable<string> to)
        {
            Rules.Add(new Rule(from, to));
        }

        public string? GetFactDescription(string name)
        {
            return Facts[name].Description;
        }

        public string? GetFactName(string description)
        {
            return Facts.FirstOrDefault(x => x.Value.Description == description).Key;
        }

        public class Fact
        {
            public string Name { get; }
            public string? Description { get; }

            public Fact(string name, string? description)
            {
                Name = name; Description = description;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        public class Rule
        {
            public HashSet<string> From { get; set; } = new HashSet<string>();
            public HashSet<string> To { get; set; } = new HashSet<string>();

            public Rule(IEnumerable<string> from, IEnumerable<string> to)
            {
                From = from.ToHashSet();
                To = to.ToHashSet();
            }

            public Rule(Rule other)
            {
                From = new HashSet<string>(other.From);
                To = new HashSet<string>(other.To);
            }

            public bool IsApplicable(HashSet<string> current) => From.IsSubsetOf(current);

            public static string StringifySet(IEnumerable<string> set)
            {
                string s = "[]";

                if (set.Any())
                {
                    s = "[";
                    s += set.First().ToString();
                    bool skip = true;
                    foreach (var item in set) {
                        if (skip) { skip = false; continue; }
                        s += ", " + item.ToString();
                    }
                    s += "]";
                }

                return s;
            }

            public string ToStringWithDescriptions(Model model)
            {
                var fromDescs = From.Select(name => model.GetFactDescription(name) ?? name);
                var toDescs = To.Select(name => model.GetFactDescription(name) ?? name);
                return StringifySet(fromDescs) + " => " + StringifySet(toDescs);
            }

            public IEnumerable<string> ToStringWithDescriptionsFrom(Model model)
            {
                var fromDescs = From.Select(name => model.GetFactDescription(name) ?? name);
                return fromDescs;
            }

            public IEnumerable<string> ToStringWithDescriptionsTo(Model model)
            {
                var toDescs = To.Select(name => model.GetFactDescription(name) ?? name);
                return toDescs;
            }

            public override string ToString()
            {
                return StringifySet(From) + " => " + StringifySet(To);
            }
        }
    }
}