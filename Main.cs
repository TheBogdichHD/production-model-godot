using Godot;
using System.Collections.Generic;
using System.Linq;


public abstract class TreeNode
{
    public Fact Fact { get; set; }
    public List<TreeNode> Children { get; set; } = new List<TreeNode>();
    public abstract bool Evaluate(List<Fact> knownFacts); // Оценка узла
}

public class AndNode : TreeNode
{
    public override bool Evaluate(List<Fact> knownFacts)
    {
        foreach (var child in Children)
        {
            if (!child.Evaluate(knownFacts))
                return false;
        }
        return true;
    }
}

public class OrNode : TreeNode
{
    public override bool Evaluate(List<Fact> knownFacts)
    {
        foreach (var child in Children)
        {
            if (child.Evaluate(knownFacts))
                return true;
        }
        return false;
    }
}

public class Fact
{
    public int Id { get; }
    public string Description { get; }

    public Fact(int id, string description)
    {
        Id = id;
        Description = description;
    }

    public override bool Equals(object obj)
    {
        return obj is Fact fact && Id == fact.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}


public class Rule
{
    public int Id { get; }
    public string Description { get; }
    public List<Fact> Conditions { get; }
    public Fact Conclusion { get; }
    public bool IsApplied { get; set; } = false;

    public Rule(int id, string description, List<Fact> conditions, Fact conclusion)
    {
        Id = id;
        Description = description;
        Conditions = conditions;
        Conclusion = conclusion;
    }

    public bool CanApply(List<Fact> facts) => Conditions.All(f => facts.Contains(f));

    public List<Fact> Apply() => new List<Fact> { Conclusion };
}

public class AndOrTreeBuilder
{
    public List<Fact> knownFacts;
    private List<Rule> rules;

    public AndOrTreeBuilder(List<Fact> facts, List<Rule> rules)
    {
        knownFacts = facts;
        this.rules = rules;
    }

    public TreeNode BuildAndOrTree(Fact targetFact)
    {
        if (knownFacts.Contains(targetFact))
        {
            return new OrNode { Fact = targetFact };
        }

        var applicableRules = rules.Where(r => r.Conclusion.Equals(targetFact)).ToList();

        if (applicableRules.Count == 0)
            return null;

        OrNode orNode = new OrNode { Fact = targetFact };

        foreach (var rule in applicableRules)
        {
            AndNode andNode = new AndNode();
            foreach (var condition in rule.Conditions)
            {
                var childNode = BuildAndOrTree(condition);
                if (childNode != null)
                    andNode.Children.Add(childNode);
            }
            orNode.Children.Add(andNode);
        }
        return orNode;
    }
}

public class ProductionSystem
{
    private List<Fact> facts;
    private List<Rule> rules;
    private List<Fact> inferredFacts;

    public ProductionSystem(List<Fact> initialFacts, List<Rule> rules)
    {
        facts = initialFacts;
        this.rules = rules;
        inferredFacts = new List<Fact>();
    }

    public void ForwardChaining()
    {
        bool appliedRule;
        do
        {
            appliedRule = false;
            foreach (var rule in rules)
            {
                if (rule.CanApply(facts) && !rule.IsApplied)
                {
                    var newFacts = rule.Apply();
                    facts.AddRange(newFacts);
                    inferredFacts.AddRange(newFacts);
                    rule.IsApplied = true;
                    appliedRule = true;
                    GD.Print($"Применено правило {rule.Id}: {rule.Description}");
                    foreach (var fact in newFacts)
                    {
                        GD.Print($"Новый факт: {fact.Description}");
                    }
                }
            }
        } while (appliedRule);
    }
}


public class BackwardChainingSystem
{
    private AndOrTreeBuilder treeBuilder;

    public BackwardChainingSystem(List<Fact> knownFacts, List<Rule> rules)
    {
        treeBuilder = new AndOrTreeBuilder(knownFacts, rules);
    }

    public bool BackwardChaining(Fact target)
    {
        TreeNode tree = treeBuilder.BuildAndOrTree(target);
        if (tree == null)
        {
            GD.Print($"Не найдено правил для вывода факта: {target.Description}");
            return false;
        }

        bool result = tree.Evaluate(treeBuilder.knownFacts);
        GD.Print($"Факт {target.Description} достижим: {result}");
        return result;
    }
}

public partial class Main : Control
{
	public override void _Ready()
	{
		List<Fact> knownFacts = new List<Fact>
		{
    		new Fact(1, "Факт A"),
    		new Fact(2, "Факт B"),

		};

		List<Rule> rules = new List<Rule>
		{
			new Rule(1, "Правило 1", new List<Fact> { new Fact(1, "Факт A"), new Fact(2, "Факт B") }, new Fact(3, "Факт C")),
			new Rule(2, "Правило 2", new List<Fact> { new Fact(3, "Факт C") }, new Fact(4, "Факт D"))
		};

		ProductionSystem forwardSystem = new ProductionSystem(knownFacts, rules);
		forwardSystem.ForwardChaining();
		
		BackwardChainingSystem backwardSystem = new BackwardChainingSystem(knownFacts, rules);
		Fact targetFact = new Fact(4, "Факт D");
		backwardSystem.BackwardChaining(targetFact);
	}

	public override void _Process(double delta)
	{
	}
}
