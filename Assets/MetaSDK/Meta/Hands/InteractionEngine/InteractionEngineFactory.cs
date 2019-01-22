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
using Meta.Internal.Playback;
using UnityEngine;


namespace Meta
{
    /// <summary>   The interaction engine factory. </summary>
    internal static class InteractionEngineFactory
    {
        /// <summary>   Creates the sources. </summary>
        /// <typeparam name="TPoint">   Type of the point. </typeparam>
        /// <param name="pointCloudSource">         The point cloud source. </param>
        /// <param name="handConsumerType">         Type of the hand consumer. </param>
        /// <param name="interactionDataSource">    The interaction data source. </param>
        /// <param name="playbackFolder">           Pathname of the playback folder. </param>
        internal static void CreateSources<TPoint>( out IPointCloudSource<TPoint> pointCloudSource,
                                                    out IPlaybackSource<PointCloudData<TPoint>> pcdPlaybackSource,
                                                    string handConsumerType,
                                                    string interactionDataSource,
                                                    string playbackFolder = "") where TPoint : PointXYZ, new()
        {
            switch (interactionDataSource)
            {
                case "Playback":
                    CreatePlaybackSources(out pointCloudSource, out pcdPlaybackSource, playbackFolder);
                    break;
                default:
                    CreateRealdataSources(out pointCloudSource);
                    pcdPlaybackSource = null;
                    break;
            }
        }

        /// <summary>   Creates playback sources. </summary>
        /// <typeparam name="TPoint">   Type of the point. </typeparam>
        /// <param name="pointCloudSource"> The point cloud source. </param>
        /// <param name="pcdPlaybackSource">   The playback source. </param>
        /// <param name="playbackFolder">   Pathname of the playback folder. </param>
        internal static void CreatePlaybackSources<TPoint>( out IPointCloudSource<TPoint> pointCloudSource,
                                                            out IPlaybackSource<PointCloudData<TPoint>> pcdPlaybackSource,
                                                            string playbackFolder) where TPoint : PointXYZ, new()
        {
            if (System.IO.Directory.Exists(playbackFolder))
            {
                ThreadedPlaybackPointCloudSource src;
                try
                {
                    src = new ThreadedPlaybackPointCloudSource(playbackFolder);
                }
                catch (System.IO.FileNotFoundException)
                {
                    src = new ThreadedPlaybackPointCloudSource();
                }
                pointCloudSource = src as IPointCloudSource<TPoint>;
                pcdPlaybackSource = src as IPlaybackSource<PointCloudData<TPoint>>;
            }
            else
            {
                UnityEngine.Debug.Log("Playback folder did not exist. Creating blank sources.");
                ThreadedPlaybackPointCloudSource pcdSrc = new ThreadedPlaybackPointCloudSource();
                pointCloudSource = pcdSrc as IPointCloudSource<TPoint>;
                pcdPlaybackSource = pcdSrc as IPlaybackSource<PointCloudData<TPoint>>;
            }
        }

        /// <summary>   Creates realdata sources. </summary>
        /// <typeparam name="TPoint">   Type of the point. </typeparam>
        /// <param name="pointCloudSource"> The point cloud source. </param>
        public static void CreateRealdataSources<TPoint>(out IPointCloudSource<TPoint> pointCloudSource) where TPoint : PointXYZ, new()
        {
            pointCloudSource = new PointCloudInterop<PointXYZConfidence>() as IPointCloudSource<TPoint>;
        }

        /// <summary>   Constructs the interaction Engine. </summary>
        /// <param name="interactionEngine">        The interaction engine. </param>
        /// <param name="handConsumerType">         Type of the hand consumer. </param>
        /// <param name="interactionDataSource">    The interaction data source. </param>
        /// <param name="playbackFolder">           Pathname of the playback folder. </param>
        public static void Construct(out InteractionEngine interactionEngine, string handConsumerType, string interactionDataSource, Transform depthOcclusionTransform, string playbackFolder = "")
        {
            IPointCloudSource<PointXYZConfidence> pointCloudSource; //todo: maybe support other stuff?
            IPlaybackSource<PointCloudData<PointXYZConfidence>> pcdPlaybackSource;
            CreateSources(out pointCloudSource, out pcdPlaybackSource, handConsumerType, interactionDataSource, playbackFolder);
            interactionEngine = new InteractionEngine(pointCloudSource, handConsumerType, depthOcclusionTransform, pcdPlaybackSource);
        }
    }
}
