using System.Collections.Generic;
using System.Linq;

namespace Production {

    class ForwardSolver : Solver {
        public ForwardSolver(Model model) : base(model) { }

        List<Model.Rule> FindApplicableRules(HashSet<string> facts) {
            var applicableRules = new List<Model.Rule>();
            foreach (var rule in Model.Rules) {
                if (!rule.From.IsSubsetOf(facts)) // N/A
                    continue;
                if (rule.To.IsSubsetOf(facts)) // Doesn't do anything
                    continue;
                applicableRules.Add(rule);
            }
            return applicableRules;
        }

        public override Result Solve(IEnumerable<string> _current, IEnumerable<string> _target) {
            var current = _current.ToHashSet(); var target = _target.ToHashSet();

            var result = new Result();
            var applicableRules = FindApplicableRules(current);
            var appliedRules = new List<Model.Rule>();
            while (applicableRules.Any()) {
                var rule = applicableRules.First();

                
                current.UnionWith(rule.To);
                appliedRules.Add(rule);

                // if (target.IsSubsetOf(current)) {
                //     result.Success = true;
                //     result.Rules = appliedRules;
                //     break;
                // }

                applicableRules = FindApplicableRules(current);
            }
            result.Success = true;
            result.Rules = appliedRules;
            return result;
        }
    }
}