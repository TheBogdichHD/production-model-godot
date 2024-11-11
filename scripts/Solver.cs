using System.Collections.Generic;

namespace Production
{
    public abstract class Solver
    {
        public Model Model { get; set; }
        public Solver(Model model)
        {
            Model = model;
        }
        
        public abstract Result Solve(IEnumerable<string> current, IEnumerable<string> target);

        public class Result
        {
            public bool Success { get; set; } = false;

            public List<Model.Rule> Rules { get; set; } = new List<Model.Rule>();
        }
    }
}