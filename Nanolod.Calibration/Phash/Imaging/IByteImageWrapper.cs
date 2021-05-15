namespace Nanolod.Calibration.Phash.Imaging
{
    internal interface IByteImageWrapper : IByteImage
    {
        IByteImageOperations GetOperations();
    }
}