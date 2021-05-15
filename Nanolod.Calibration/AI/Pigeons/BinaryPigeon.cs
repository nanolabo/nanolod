using Nanolod.Calibration.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nanolod.Calibration.Pigeons
{
    /// <summary>
    /// A class for representing pigeons in Pigeon Inspired Optimization algorithm that deals with binary 1/0 problems
    /// </summary>
    public class BinaryPigeon<TData>
    {
        public double[] Velocity { get; set; }

        private TData[] _current;
        private readonly TData[] _bestSolution;
        private double _bestFitness;
        private readonly Configuration<TData[]> _config;

        public BinaryPigeon(Configuration<TData[]> config)
        {
            this._config = config;
            this._current = config.initializeSolutionFunction();
            this.Velocity = Array.CreateInstance(typeof(double), _current.Length).OfType<double>().ToArray();
            this._bestSolution = this._current;
            this._bestFitness = config.objectiveFunction(this._current);
        }

        public TData[] GetSolution()
        {
            return this._config.cloneFunction(_bestSolution);
        }

        public void SetSolution(TData[] sol)
        {
            this._current = sol;
            this._bestFitness = _config.objectiveFunction(this._current);
        }

        public double GetFitness()
        {
            return this._bestFitness;
        }

        public static double[] UpdateVelocity(double[] globalBestSolution, double[] current, double[] velocity, double mapFactor, int noOfIterations)
        {
            List<double> ret = new List<double>();
            double w = Math.Pow(Math.E, -(mapFactor * noOfIterations));
            List<double> fx1 = velocity.Select(v => v * w).ToList();
            List<double> fx2 = new List<double>();
            for (int i = 0; i < Math.Min(globalBestSolution.Length, current.Length); i++)
            {
                int numX = Convert.ToInt32(globalBestSolution[i]) - Convert.ToInt32(current[i]);
                double num = Number.Rnd() * numX;
                fx2.Add(num);
            }
            for (int i = 0; i < Math.Min(fx1.Count, fx2.Count); i++)
            {
                ret.Add(fx1[i] + fx2[i]);
            }
            return ret.ToArray();
        }

        public static double[] UpdateLocation(double[] sol, double[] velocity)
        {
            List<double> ret = new List<double>();
            for (int i = 0; i < Math.Min(sol.Length, velocity.Count()); i++)
            {
                ret.Add(Math.Abs(sol[i] + velocity[i]));
            }
            return ret.ToArray();
        }
    }
}