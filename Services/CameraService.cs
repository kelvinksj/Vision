using DirectShowLib;
using OpenCvSharp;

namespace CameraWebApp.Services
{
    public class CameraService
    {
        public List<string> GetConnectedCameras()
        {
            var cameras = new List<string>();
            DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            foreach (var dev in devices)
                cameras.Add(dev.Name);
            return cameras;
        }

        public VideoCapture? OpenCamera(int index)
        {
            var capture = new VideoCapture(index);
            if (!capture.IsOpened())
                return null;
            return capture;
        }
    }
}
