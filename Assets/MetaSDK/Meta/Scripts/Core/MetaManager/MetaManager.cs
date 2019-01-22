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
using UnityEngine;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Meta-Editor")]
[assembly: InternalsVisibleTo("Assembly-CSharp-Editor")]

namespace Meta
{
    /// <summary>
    /// Handles setup of the Meta scene environment and provides reference to Meta context classes.
    /// </summary>
    internal class MetaManager : MonoBehaviour
    {
        [SerializeField]
        private DataAcquisitionSystem _dataAcquisitionSystem = new DataAcquisitionSystem();

        /// <summary>
        /// The scale mapping of 1 meter to the number of unity units. This affects the simulation parameters. As this value increases, 1 unity unit will become smaller in
        /// real world terms. eg To use a simulation scale of 1 = 1cm, change this variable to 100. To use 1 = 100cm, change
        /// this variable to 1.
        /// </summary>
        [SerializeField]
        internal float globalScaleMetresToUnity = 1f; // TODO: will be moved to a settings class

        /// <summary>
        /// Sensor recording playback directory.
        /// </summary>
        [SerializeField]
        private string sensorPlaybackDir = "";

        /// <summary>
        /// Playback directory
        /// </summary>
        [SerializeField]
        protected string playbackDir;

        /// <summary>
        /// MetaContext MonoBehaviour container
        /// </summary>
        [SerializeField]
        protected MetaContextBridge _contextBridge;

        private EventHandlers _eventHandlers = new EventHandlers();
        
        #region MonoBehaviour Methods
        protected virtual void Awake()
        {
            new MetaPathVariables().AddPathVariables();
            var metaFactory = new MetaFactory(_dataAcquisitionSystem, gameObject, globalScaleMetresToUnity, playbackDir, sensorPlaybackDir);
            var package = metaFactory.ConstructAll();

            // Add Context
            SetContext(package.MetaContext);

            // Initialize events
            foreach (IEventReceiver eventReceiver in package.EventReceivers)
            {
                eventReceiver.Init(_eventHandlers);
            }

            _eventHandlers.RaiseOnAwake();
        }

        private void SetContext(IMetaContextInternal context)
        {
            // Get Context Bridge
            if (_contextBridge == null)
            {
                _contextBridge = gameObject.GetComponent<MetaContextBridge>();
                // If still null, add it
                if (_contextBridge == null)
                    _contextBridge = gameObject.AddComponent<MetaContextBridge>();
            }
            
            _contextBridge.SetContext(context);
        }

        protected virtual void Start()
        {
            _eventHandlers.RaiseOnStart();
        }

        private void Update()
        {
            CheckForShortcuts();
            _eventHandlers.RaiseOnUpdate();
        }

        private void FixedUpdate()
        {
            _eventHandlers.RaiseOnFixedUpdate();
        }

        private void LateUpdate()
        {
            _eventHandlers.RaiseOnLateUpdate();
        }

        private void OnDestroy()
        {
            _eventHandlers.RaiseOnDestroy();
        }

        private void OnApplicationQuit()
        {
            _eventHandlers.RaiseOnApplicationQuit();
        }

        protected virtual void CheckForShortcuts()
        {
            
        }
        #endregion

        [System.Obsolete("metaContext is Obsolete. Please use MetaContextBridge.CurrentContext.")]
        public IMetaContext metaContext
        {
            get { return _contextBridge.CurrentContext; }
        }
    }

}
