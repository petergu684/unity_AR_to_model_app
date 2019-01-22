// Copyright Â© 2018, Meta Company.  All rights reserved.
// 
// Redistribution and use of this software (the "Software") in binary form, without modification, is 
// permitted provided that the following conditions are met:
// 
// 1.      Redistributions of the unmodified Software in binary form must reproduce the above 
//         copyright notice, this list of conditions and the following disclaimer in the 
//         documentation and/or other materials provided with the distribution.
// 2.      The name of Meta Company (â€œMetaâ€) may not be used to endorse or promote products derived 
//         from this Software without specific prior written permission from Meta.
// 3.      LIMITATION TO META PLATFORM: Use of the Software is limited to use on or in connection 
//         with Meta-branded devices or Meta-branded software development kits.  For example, a bona 
//         fide recipient of the Software may incorporate an unmodified binary version of the 
//         Software into an application limited to use on or in connection with a Meta-branded 
//         device, while he or she may not incorporate an unmodified binary version of the Software 
//         into an application designed or offered for use on a non-Meta-branded device.
// 
// For the sake of clarity, the Software may not be redistributed under any circumstances in source 
// code form, or in the form of modified binary code â€“ and nothing in this License shall be construed 
// to permit such redistribution.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDER "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, 
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
// PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL META COMPANY BE LIABLE FOR ANY DIRECT, 
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, 
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS 
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using System;
using System.Runtime.InteropServices;
using Meta;
using Meta.Internal;

/// <summary>   A class to encapsulate all the function calls to the hand kernel. /// </summary>
public static class HandKernelInterop
{
    //todo: Change from Event + GetData format to Event HAndler passing the new Data (should be less cofusing that way)

    /// <summary>   Handler, called when the new data. </summary>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void NewDataHandler();

    /// <summary>   Destroys the hand consumer. </summary>
    [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "DestroyHandConsumer")]
    public static extern void DestroyHandConsumer();

    /// <summary>   Builds hand consumer. </summary>
    /// <param name="handConsumerType"> Type of the hand consumer. </param>
    /// <returns>   true if it succeeds, false if it fails. </returns>
    [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "BuildHandConsumer")]
    internal static extern bool BuildHandConsumer(string handConsumerType);

    /// <summary>   Sets depth data cleaner options. </summary>
    /// <param name="depthDataCleanerOptions">  Options for controlling the depth data cleaner. </param>
    /// <returns>   true if it succeeds, false if it fails. </returns>
    [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "SetDepthDataCleanerOptions")]
    internal static extern bool SetDepthDataCleanerOptions(ref DepthDataCleanerOptions depthDataCleanerOptions);

    /// <summary>   Sets point cloud generator options./ </summary>
    /// <param name="cloudGeneratorOptions">    Options for controlling the cloud generator. </param>
    /// <returns>   true if it succeeds, false if it fails. </returns>
    [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "SetPointCloudGeneratorOptions")]
    internal static extern bool SetPointCloudGeneratorOptions(ref CloudGeneratorOptions cloudGeneratorOptions);

    /// <summary>   Sets hand processor options. </summary>
    /// <param name="handProcessorOptions"> Options for controlling the hand processor. </param>
    /// <returns>   true if it succeeds, false if it fails. </returns>
    [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "SetHandProcessorOptions")]
    internal static extern bool SetHandProcessorOptions(ref HandProcessorOptions handProcessorOptions);

    /// <summary>   Gets point cloud meta data. </summary>
    /// <param name="pointCloudMetaData">   Information describing the point cloud meta. </param>
    /// <returns>   true if it succeeds, false if it fails. </returns>
    [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "GetPointCloudMetaData")]
    internal static extern bool GetPointCloudMetaData(ref PointCloudInteropMetaData pointCloudMetaData);

    /// <summary>   Gets point cloud data. </summary>
    /// <param name="pointCloudInteropData">    Information describing the point cloud interop. </param>
    /// <returns>   true if it succeeds, false if it fails. </returns>
    [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "GetPointCloudData")]
    internal static extern bool GetPointCloudData(ref PointCloudInteropData pointCloudInteropData);

    /// <summary>
    ///     Registers the new point cloud data event handler described by newPointCloudDataCallback.
    /// </summary>
    /// <param name="newPointCloudDataCallback">    The new point cloud data callback. </param>
    /// <returns>   true if it succeeds, false if it fails. </returns>
    [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "RegisterNewPointCloudDataEventHandler")]
    internal static extern bool RegisterNewPointCloudDataEventHandler(NewDataHandler newPointCloudDataCallback);

    /// <summary>
    ///     Registers the new hand data event handler described by newHanddataCallback./
    /// </summary>
    /// <param name="newHanddataCallback">  The new handdata callback. </param>
    /// <returns>   true if it succeeds, false if it fails. </returns>
    [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "RegisterNewHandDataEventHandler")]
    internal static extern bool RegisterNewHandDataEventHandler(NewDataHandler newHanddataCallback);

    /// <summary>   Gets sensor meta data. </summary>
    /// <param name="sensorMetaData">   Information describing the sensor meta. </param>
    /// <returns>   true if it succeeds, false if it fails. </returns>
    [DllImport(DllReferences.MetaVisionDLLName, EntryPoint = "GetSensorMetaData")]
    internal static extern bool GetSensorMetaData(ref SensorMetaData sensorMetaData);

    /// <summary>   Sets time from unity. </summary>
    /// <param name="t">    The float to process. </param>
    [DllImport("MetaUnityDepthVisualizer", EntryPoint = "SetTimeFromUnity")]
    internal static extern void SetTimeFromUnity(float t);

    /// <summary>   Gets render event function. </summary>
    /// <returns>   The render event function. </returns>
    [DllImport("MetaUnityDepthVisualizer", EntryPoint = "GetRenderEventFunc")]
    internal static extern IntPtr GetRenderEventFunc();

    // We'll also pass native pointer to a texture in Unity.
    // The plugin will fill texture data from native code.s
    [DllImport("MetaUnityDepthVisualizer", EntryPoint = "SetTextureFromUnity")]
    internal static extern void SetTextureFromUnity(IntPtr texture, int height, int width);
}
