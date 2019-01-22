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
using Meta.Internal;
using UnityEngine;

namespace Meta
{

    /// <summary>   A class for logging the data from interaction engine . </summary>
    ///
    /// <seealso cref="T:Meta.MetaBehaviour"/>

    internal class InteractionEngineLogging : MetaBehaviour
    {
        /// <summary>   The point cloud data logging class. </summary>
        private PointCloudDataLogging _pointCloudDataLogging;

        /// <summary>   The sensor data recorder. </summary>
        private RecordSensorData _sensorDataRecorder;

        /// <summary>   true to log in record sesnor data folder. </summary>
        public bool m_logInRecordSesnorDataFolder = false;

        /// <summary>   Pathname of the logging folder. </summary>
        public string m_loggingFolder;

        /// <summary>   Information describing the point cloud. </summary>
        private PointCloudData<PointXYZConfidence> _pointCloudData;

        /// <summary>   Information describing the point cloud meta. </summary>
        private PointCloudMetaData _pointCloudMetaData;

        /// <summary>   The interaction engine. </summary>
        private InteractionEngine _interactionEngine;

        public void Start()
        {

            if (m_logInRecordSesnorDataFolder)
            {
                _sensorDataRecorder = GameObject.Find("MetaCameraRig").GetComponent<RecordSensorData>();
                if (_sensorDataRecorder != null)
                {
                    UnityEngine.Debug.LogError("cant find RecordsensorData");
                }
                m_loggingFolder = _sensorDataRecorder.GetRecordingPath();
            }
            if (m_loggingFolder == null)
            {
                UnityEngine.Debug.LogError("Logging Folder is null");
                return;
            }
            _pointCloudMetaData = new PointCloudMetaData();
            _pointCloudDataLogging = new PointCloudDataLogging(m_loggingFolder);
            _interactionEngine = metaContext.Get<InteractionEngine>();

        }

        public void Update()
        {
            if (_pointCloudData == null)
            {

                if (_interactionEngine.GetCloudMetaData(ref _pointCloudMetaData))
                {
                    _pointCloudData = new PointCloudData<PointXYZConfidence>(_pointCloudMetaData.maxSize);
                }
                else
                {
                    return;
                }
            }
            _interactionEngine.GetCloudData(ref _pointCloudData);
            _pointCloudDataLogging.Update(_pointCloudData);
        }
    }
}
