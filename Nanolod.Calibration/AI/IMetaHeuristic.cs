using System.Collections.Generic;

namespace Nanolod.Calibration
{
    public interface IMetaHeuristic<SolutionType>
    {
        void Create(Configuration<SolutionType> config);

        SolutionType SingleIteration();

        SolutionType FullIteration();

        double GetBestFitness();

        List<double> GetIterationSequence();
    }
}
