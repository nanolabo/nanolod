using Nanolod.Calibration.ABC;
using Nanolod.Calibration.Common;
using Nanolod.Calibration.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nanolod.Calibration.Pigeons
{
    public class BinaryPIO<TData> : IMetaHeuristic<TData>
    {
        public BinaryPigeon<TData> Current { get; set; }

        private List<BinaryPigeon<TData>> _pigeons = new List<BinaryPigeon<TData>>();
        private double _mapFactor = 0.05;
        private TData[] _globalBestSolution;
        private TData[] _localBestSolution;
        private double _globalBestFitness;
        private double _localBestFitness;
        private LAHC _lateAcceptance;
        private Configuration<TData[]> _config;
        private readonly List<double> _iterationFitnessSequence = new List<double>();
        private int _iterationCount;
        public double _switchProbability = 0.2;

        private TData[] getBestIndividual()
        {
            if (_localBestFitness > _globalBestFitness) return _localBestSolution;
            else return _globalBestSolution;
        }

        public void create(Configuration<TData[]> config)
        {
            this._config = config;
            for (int i = 0; i < config.populationSize; i++) this._pigeons.Add(new BinaryPigeon<TData>(config)); //generate pigeons in swarm
            if (this._config.movement == Search.Direction.Optimization)
            {
                _globalBestFitness = double.MaxValue;
                _localBestFitness = double.MaxValue;
            }
            else if (this._config.movement == Search.Direction.Divergence)
            {
                _globalBestFitness = double.MinValue;
                _localBestFitness = double.MinValue;
            }
            this._lateAcceptance = new LAHC(config.tableSize, _localBestFitness, this._config.movement);
            this._globalBestSolution = this._config.initializeSolutionFunction();
            this._globalBestFitness = this._config.objectiveFunction(this._globalBestSolution);
            this._localBestSolution = this._config.cloneFunction(_globalBestSolution);
            this._localBestFitness = this._globalBestFitness + 0;
        }

        public TData[] fullIteration()
        {
            for (int count = 1; count <= this._config.noOfIterations; count++)
            {
                this._iterationCount = count;
                this.singleIteration();
                _iterationFitnessSequence.Add(this.GetBestFitness());
            }
            if (this._config.writeToConsole) Console.WriteLine("End of Iterations");
            return getBestIndividual();
        }

        public double GetBestFitness()
        {
            return this._config.movement == Search.Direction.Optimization ?
                                Math.Min(this._globalBestFitness, this._localBestFitness) :
                                Math.Max(this._globalBestFitness, this._localBestFitness);
        }

        public List<double> GetIterationSequence()
        {
            return this._iterationFitnessSequence;
        }

        public TData[] singleIteration()
        {
            Action<TData[]> updateBestFn = (TData[] sol) =>
            {
                double fitness = _config.objectiveFunction(sol);
                if ((_config.hardObjectiveFunction != null &&
                    ((_config.enforceHardObjective && _config.hardObjectiveFunction(sol)) || (!_config.enforceHardObjective))) ||
                    _config.hardObjectiveFunction == null)
                {
                    if ((_config.newFitnessIsBetter(_localBestFitness, fitness) || _lateAcceptance.Update(fitness)))
                    {
                        _localBestFitness = fitness;
                        _localBestSolution = this._config.cloneFunction(sol);
                    }
                    if (_config.newFitnessIsBetter(_globalBestFitness, fitness))
                    {
                        _globalBestFitness = fitness;
                        _globalBestSolution = this._config.cloneFunction(sol);
                    }
                }
            };
            if (Number.Rnd() < _switchProbability)
            {
                int a = Convert.ToInt32(Math.Floor(Number.Rnd() * _pigeons.Count));
                int b = Convert.ToInt32(Math.Floor(Number.Rnd() * _pigeons.Count));
                IEnumerable<TData>[] newSol = GA.CrossOver.AutoTwoPoint(_pigeons[a].GetSolution(), _pigeons[b].GetSolution());
                updateBestFn(newSol.ToArray().First().ToArray());
                updateBestFn(newSol.ToArray().Last().ToArray());
                _pigeons[a].SetSolution(newSol.ToArray().First().ToArray());
                _pigeons[b].SetSolution(newSol.ToArray().Last().ToArray());
            }
            else
            {
                for (int i = 0; i < _pigeons.Count; i++)
                {
                    this.Current = _pigeons[i];
                    TData[] newSol = this._config.mutationFunction(_pigeons[i].GetSolution());
                    updateBestFn(newSol);
                    _pigeons[i].SetSolution(newSol);
                }
            }

            if (_config.writeToConsole && ((_iterationCount % _config.consoleWriteInterval) == 0) || (_iterationCount - 1 == 0))
            {
                if (_config.consoleWriteFunction == null)
                    Console.WriteLine(_iterationCount + "\t" + this.getBestIndividual() + " = " + _localBestFitness + "\t" + _globalBestFitness);
                else
                    _config.consoleWriteFunction(this.getBestIndividual(), _globalBestFitness, _iterationCount);
            }

            return this.getBestIndividual();
        }

        public void Create(Configuration<TData> config)
        {
            throw new NotImplementedException();
        }

        TData IMetaHeuristic<TData>.SingleIteration()
        {
            throw new NotImplementedException();
        }

        TData IMetaHeuristic<TData>.FullIteration()
        {
            throw new NotImplementedException();
        }

        public TData[] getLocalBestSolution() => this._localBestSolution;

        public TData[] getGlobalBestSolution() => this._globalBestSolution;
    }
}
