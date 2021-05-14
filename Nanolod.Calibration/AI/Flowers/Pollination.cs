using Nanolod.Calibration.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nanolod.Calibration.Flowers
{
    public class Pollination<TPollenType> : IMetaHeuristic<TPollenType>
    {
        private Flower<TPollenType>[] _flowers = null;
        private Configuration<List<TPollenType>> _config = null;
        private Flower<TPollenType> _gBest = null;
        /// <summary>
        /// aka pValue
        /// </summary>
        private double _switchProbability = 0.4;
        private int _iterationCount = 0;
        private readonly List<double> _iterationFitnessSequence = new List<double>();
        public Pollination()
        {

        }

        private Flower<TPollenType>[] generateFlowers(int count)
        {
            List<Flower<TPollenType>> flowers = new List<Flower<TPollenType>>();
            for (int i = 0; i < count; i++)
            {
                flowers.Add(new Flower<TPollenType>(_config));
            }
            return flowers.ToArray();
        }

        public void create(Configuration<List<TPollenType>> config)
        {
            this._config = config;
            this._flowers = this.generateFlowers(config.populationSize);
        }

        public void create(Configuration<List<TPollenType>> config, double _switchProbability)
        {
            this.create(config);
            this._switchProbability = _switchProbability;
        }

        public List<TPollenType> fullIteration()
        {
            while (_iterationCount < _config.noOfIterations)
            {
                List<TPollenType> _bestSolution = this.singleIteration();
                double _bestFitness = this.getBestFlower().getFitness();
                this._iterationFitnessSequence.Add(_bestFitness);
                if (this._config.writeToConsole && ((_iterationCount % this._config.consoleWriteInterval) == 0) || (_iterationCount - 1 == 0))
                {
                    if (this._config.consoleWriteFunction == null)
                        Console.WriteLine(_iterationCount + "\t" + _bestSolution + " = " + _bestFitness);
                    else
                        this._config.consoleWriteFunction(_bestSolution, _bestFitness, _iterationCount);
                }
                this._iterationCount++;
            }
            return this.getBestFlower().getSolution();
        }

        public double getLocalBestFitness()
        {
            double localBestFitness = _config.movement == Common.Search.Direction.Divergence ? this._flowers.Max((flower) => flower.getFitness()) : this._flowers.Min((flower) => flower.getFitness());
            return localBestFitness;
        }

        public Flower<TPollenType> getLocalBestFlower()
        {
            Flower<TPollenType> localBest = this._flowers.Where(flower => flower.getFitness() == getLocalBestFitness()).FirstOrDefault();
            return localBest;
        }

        public double GetBestFitness()
        {
            double localBestFitness = getLocalBestFitness();
            return _gBest == null || _config.newFitnessIsBetter(_gBest.getFitness(), localBestFitness) ? localBestFitness : _gBest.getFitness();
        }

        public List<double> GetIterationSequence()
        {
            return this._iterationFitnessSequence;
        }

        public Flower<TPollenType> getBestFlower()
        {
            Flower<TPollenType> localBest = getLocalBestFlower();
            if (_gBest == null || _config.newFitnessIsBetter(_gBest.getFitness(), localBest.getFitness()))
            {
                this._gBest = localBest.clone();
            }
            return _gBest;
        }

        public List<TPollenType> singleIteration()
        {
            if (this._gBest == null) this._gBest = this.getBestFlower();
            for (int i = 0; i < this._flowers.Length; i++)
            {
                Flower<TPollenType> flower = this._flowers[i];
                Flower<TPollenType> flowerClone = null;
                if (Number.Rnd() < this._switchProbability)
                {
                    //perform global pollination
                    flowerClone = flower.doGlobalPollination(_gBest);
                }
                else
                {
                    //perform local pollination
                    int a = 0, b = 0;
                line1:
                    a = Convert.ToInt32(Math.Floor(Number.Rnd(this._flowers.Length)));
                    b = Convert.ToInt32(Math.Floor(Number.Rnd(this._flowers.Length)));
                    if (a == b) goto line1;
                    flowerClone = flower.doLocalPollination(this._flowers[a], this._flowers[b]);
                }
                if (_config.newFitnessIsBetter(this._flowers[i].getFitness(), flowerClone.getFitness()) && ((_config.enforceHardObjective && _config.hardObjectiveFunction != null && _config.hardObjectiveFunction.Invoke(flowerClone.getSolution())) || (!_config.enforceHardObjective || _config.hardObjectiveFunction == null)))
                {
                    this._flowers[i] = flowerClone;
                }
            }
            this._gBest = this.getBestFlower();
            return this._gBest.getSolution();
        }

        public void Create(Configuration<TPollenType> config)
        {
            throw new NotImplementedException();
        }

        TPollenType IMetaHeuristic<TPollenType>.SingleIteration()
        {
            throw new NotImplementedException();
        }

        TPollenType IMetaHeuristic<TPollenType>.FullIteration()
        {
            throw new NotImplementedException();
        }
    }
}
