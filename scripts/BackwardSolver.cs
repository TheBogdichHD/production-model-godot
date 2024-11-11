using System.Collections.Generic;
using System.Linq;

namespace Production
{
    class BackwardSolver : Solver
    {
        public BackwardSolver(Model model) : base(model) { }

        public override Result Solve(IEnumerable<string> _current, IEnumerable<string> _target)
        {
            var current = _current.ToHashSet(); var target = _target.ToHashSet();
            var rules = Model.Rules;

            var result = new Result() { Success = false };

            var stack = new Stack<string>(target);
            var factProducingRules = new Dictionary<string, Model.Rule>();
            var appliedRules = new HashSet<Model.Rule>();
            var backPropagatedRules = new HashSet<Model.Rule>();

            while (stack.Any() && !current.IsSupersetOf(target))
            {
                var next = stack.Peek();
                if (current.Contains(next))
                {
                    stack.Pop();
                    continue;
                }

                var usefulRules = rules.Where((rule) => rule.To.Contains(next));
                if (!usefulRules.Any())
                {
                    stack.Pop();
                    continue;
                }

                var applicableRules = usefulRules.Where((rule) => rule.IsApplicable(current));
                if (applicableRules.Any())
                {
                    var rule = applicableRules.First();

                    current.UnionWith(rule.To);
                    foreach (var fact in rule.To)
                        factProducingRules[fact] = rule;

                    appliedRules.Add(rule);
                    continue;
                }

                var backPropagationRules = usefulRules.Where((rule) => !backPropagatedRules.Contains(rule));
                if (!backPropagationRules.Any())
                {
                    stack.Pop();
                    continue;
                }

                foreach (var rule in backPropagationRules)
                {
                    backPropagatedRules.Add(rule);
                    
                    var missingFacts = rule.From.Where((fact) => !current.Contains(fact));
                    foreach (var fact in missingFacts)
                        stack.Push(fact);
                }
            }

            if (!target.IsSubsetOf(current))
                return result;
            result.Success = true;

            stack = new Stack<string>(target);
            while (stack.Any())
            {
                var fact = stack.Pop();
                if (!factProducingRules.ContainsKey(fact))
                    continue;
                var rule = factProducingRules[fact];

                result.Rules.Add(rule);
                foreach (var fromFact in rule.From)
                    stack.Push(fromFact);
            }

            var seenRules = new HashSet<Model.Rule>();
            for (var i = result.Rules.Count - 1; i >= 0; i--)
            {
                var rule = result.Rules[i];
                if (seenRules.Contains(rule))
                {
                    result.Rules.RemoveAt(i);
                    continue;
                }
                seenRules.Add(rule);
            }

            result.Rules.Reverse();

            return result;
        }
    }
}
