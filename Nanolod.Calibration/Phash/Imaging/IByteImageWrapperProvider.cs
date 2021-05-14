namespace Nanolod.Calibration.Phash.Imaging
{
    internal interface IByteImageWrapperProvider : IByteImage
    {
        IByteImageWrapper GetWrapper();
    }
}