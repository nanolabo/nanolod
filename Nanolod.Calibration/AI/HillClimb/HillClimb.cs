using Nanolod.Calibration.Common;
using System;
using System.Collections.Generic;

namespace Nanolod.Calibration.HillClimb
{
    public class HillClimb<SolutionType> : IMetaHeuristic<SolutionType>
    {
        public Configuration<SolutionType> Config { get; set; }
        private SolutionType _bestIndividual { get; set; }
        private double _bestFitness { get; set; }
        private int _iterationCount = 0;
        private readonly List<double> _iterationFitnessSequence = new List<double>();

        public HillClimb()
        {

        }

        public void Create(Configuration<SolutionType> config)
        {
            this.Config = config;
            if (Config.movement == Search.Direction.Optimization)
            {
                _bestFitness = double.MaxValue;
            }
            else if (Config.movement == Search.Direction.Divergence)
            {
                _bestFitness = double.MinValue;
            }
            this._bestIndividual = Config.initializeSolutionFunction();
            this._bestFitness = Config.objectiveFunction(this._bestIndividual);
        }

        public SolutionType FullIteration()
        {
            for (int count = 1; count <= Config.noOfIterations; count++)
            {
                _iterationCount = count;
                _bestIndividual = SingleIteration();
                _iterationFitnessSequence.Add(_bestFitness);
            }
            if (Config.writeToConsole) Console.WriteLine("End of Iterations");
            return _bestIndividual;
        }

        public List<double> GetIterationSequence()
        {
            return _iterationFitnessSequence;
        }

        public SolutionType SingleIteration()
        {
            SolutionType newSol = Config.mutationFunction(Config.cloneFunction(_bestIndividual));
            double newFit = Config.objectiveFunction(newSol);

            if ((Config.hardObjectiveFunction != null &&
                    ((Config.enforceHardObjective && Config.hardObjectiveFunction(newSol)) || (!Config.enforceHardObjective))) ||
                    Config.hardObjectiveFunction == null)
            {
                if ((Config.movement == Search.Direction.Optimization && newFit < _bestFitness) || (Config.movement == Search.Direction.Divergence && newFit > _bestFitness))
                {
                    _bestIndividual = Config.cloneFunction(newSol);
                    _bestFitness = newFit;
                }
            }

            if (Config.writeToConsole && ((_iterationCount % Config.consoleWriteInterval) == 0) || (_iterationCount - 1 == 0))
            {
                if (Config.consoleWriteFunction == null)
                    Console.WriteLine(_iterationCount + "\t" + _bestIndividual + " = " + _bestIndividual);
                else
                    Config.consoleWriteFunction(_bestIndividual, _bestFitness, _iterationCount);
            }

            return _bestIndividual;
        }

        public double GetBestFitness()
        {
            return this._bestFitness;
        }
    }
}
