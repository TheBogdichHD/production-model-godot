using Godot;
using System.IO;
using Production;
using System.Collections.Generic;
using System.Linq;

public partial class Main : Control
{
    [Export]
	string FactsPath = "games-knowledge-base/facts.md";
	[Export]
    string RulesPath = "games-knowledge-base/rules.md";

    private PackedScene nodeScene = GD.Load<PackedScene>("graph_node.tscn");
    private GraphEdit graphEdit;
    private ItemList itemListInitial;
    private ItemList itemListTarget;
    private Model model = new Model();
    private Solver solver;

    private void ReadFiles()
    {
        foreach (var line in File.ReadAllLines(FactsPath).Where(x => !x.StartsWith('#') && x.Any())) {
            var split = line.Split(": ");
            model.AddFact(split[0], split[1]);

            itemListInitial.AddItem(split[1]);
            itemListTarget.AddItem(split[1]);
        }

        foreach (var line in File.ReadAllLines(RulesPath).Where(x => !x.StartsWith('#') && x.Any())) {
            var split = line.Split("->");
            model.AddRule(split[0].Split(";"), split[1].Split(";"));
        }
    }

    public void OnButtonPressed()
    {
        foreach (var child in graphEdit.GetChildren())
        {
            if (child is GraphNode)
                graphEdit.RemoveChild(child);
        }
        
        Solver.Result result;
        var current = new List<string>();
        var target = new List<string>();

        foreach (var item in itemListInitial.GetSelectedItems())
        {
            current.Add(model.GetFactName(itemListInitial.GetItemText(item)));
            GD.Print(model.GetFactName(itemListInitial.GetItemText(item)));
        }
        
        foreach (var item in itemListTarget.GetSelectedItems())
        {
            target.Add(model.GetFactName(itemListTarget.GetItemText(item)));
            GD.Print(model.GetFactName(itemListTarget.GetItemText(item)));
        }

        result = solver.Solve(current, target);
        PrintResult(result);

        graphEdit.ArrangeNodes();
    }

    public void OnOptionButtonItemSelected(int index)
    {
        if (index == 0)
        {
            solver = new ForwardSolver(model);
        }
        else
        {
            solver = new BackwardSolver(model);
        }
    }

    public void PrintResult(Solver.Result result)
    {
        GD.Print("> Success: " + result.Success);
        if (result.Success) {
            GD.Print("> Productions:");
            foreach (var rule in result.Rules) {
                IEnumerable<string> fromNodeTitles = rule.ToStringWithDescriptionsFrom(model);
                string toNodeTitle = rule.ToStringWithDescriptionsTo(model);

                if (!graphEdit.HasNode(toNodeTitle))
                {
                    GraphNode graphNodeTo = nodeScene.Instantiate<GraphNode>();
                    graphNodeTo.Name = toNodeTitle;
                    graphNodeTo.Title = toNodeTitle;
                    
                    graphEdit.AddChild(graphNodeTo);
                }

                foreach (var fromNodeTitle in fromNodeTitles)
                {
                    if (!graphEdit.HasNode(fromNodeTitle))
                    {
                        GraphNode graphNodeFrom = nodeScene.Instantiate<GraphNode>();
                        graphNodeFrom.Name = fromNodeTitle;
                        graphNodeFrom.Title = fromNodeTitle;
                        
                        graphEdit.AddChild(graphNodeFrom);
                    }

                    
                    graphEdit.ConnectNode(fromNodeTitle, 0, toNodeTitle, 0);
                }
                
                GD.Print("\t" + rule.ToStringWithDescriptions(model));
            }
        }
    }
	public override void _Ready()
	{
        itemListInitial = GetNode<ItemList>("VBoxContainer/HSplitContainer/HSplitContainer/ItemListInitial");
        itemListTarget = GetNode<ItemList>("VBoxContainer/HSplitContainer/HSplitContainer/ItemListTarget");
        graphEdit = GetNode<GraphEdit>("VBoxContainer/HSplitContainer/GraphEdit");
        solver = new ForwardSolver(model);

        ReadFiles();
	}
}
