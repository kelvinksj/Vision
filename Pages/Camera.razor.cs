using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace CameraWebApp.Pages
{
    public partial class Camera : IDisposable
    {
        // -------------------------------------------------------
        //  C# → JS Callback Helper (merged logic)
        // -------------------------------------------------------
        public class DetectionCallbackHelper
        {
            public Camera Parent { get; set; } = default!;
        }

        // -------------------------------------------------------
        // Private fields
        // -------------------------------------------------------
        private DotNetObjectReference<DetectionCallbackHelper>? _detCbRef;

        // -------------------------------------------------------
        // Start Detection Pipeline
        // -------------------------------------------------------
        private async Task StartDetectionPipeline()
        {
            if (_detCbRef == null)
            {
                var helper = new DetectionCallbackHelper { Parent = this };
                _detCbRef = DotNetObjectReference.Create(helper);
            }

            Console.WriteLine("🚀 Starting JS Detection Pipeline...");

            await JS.InvokeVoidAsync("DetectionModules.lockCustomCropForDetection", _detCbRef);

        }

        private async Task TestStart()
        {
            await StartDetectionPipeline();
        }

        public void Dispose()
        {
            _detCbRef?.Dispose();
        }

        [JSInvokable]
        public void ReceiveDetections(object data)
        {
            Console.WriteLine("[Pipeline→C#] " + data);
        }

        [JSInvokable("OnTargetSelected")]
        public void OnTargetSelected(string id)
        {
            Console.WriteLine($"🎯 [C#] Target selected = {id}");
        }

    }
}
