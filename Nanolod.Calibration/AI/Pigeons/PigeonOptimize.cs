using System;
using System.Collections.Generic;

namespace Nanolod.Calibration.Pigeons
{
    public class PigeonOptimize<TSolutionType> : IMetaHeuristic<TSolutionType>
    {
        public void Create(Configuration<TSolutionType> config)
        {
            throw new NotImplementedException();
        }

        public TSolutionType FullIteration()
        {
            throw new NotImplementedException();
        }

        public double GetBestFitness()
        {
            throw new NotImplementedException();
        }

        public List<double> GetIterationSequence()
        {
            throw new NotImplementedException();
        }

        public TSolutionType SingleIteration()
        {
            throw new NotImplementedException();
        }
    }
}
