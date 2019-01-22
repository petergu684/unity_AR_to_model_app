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
using UnityEngine;

namespace Meta.Reconstruction
{
    /// <summary>
    /// Entry point of the Environment Initialization process.
    /// </summary>
    internal class EnvironmentConfiguration : MetaBehaviourInternal
    {
        [Tooltip("Prefab of the EnvironmentScanController, used to control the environment scan process.")]
        [HideInInspector]
        [SerializeField]
        private BaseEnvironmentScanController _environmentScanControllerPrefab;

        [Tooltip("Prefab of the MetaReconstruction, used to access to the 3D reconstruction functionalities.")]
        [HideInInspector]
        [SerializeField]
        private Transform _metaReconstructionPrefab;

        [Tooltip("Whether the slam relocalization is active or not.")]
        [HideInInspector]
        [SerializeField]
        private bool _slamRelocalizationActive;

        [Tooltip("Whether the surface reconstruction is active or not.")]
        [HideInInspector]
        [SerializeField]
        private bool _surfaceReconstructionActive;

        [Tooltip("The environment profile strategy.")]
        [HideInInspector]
        [SerializeField]
        private EnvironmentProfileType _environmentProfileType;

        private IEnvironmentInitializer _environmentInitializer;


        internal Transform MetaReconstructionPrefab
        {
            get { return _metaReconstructionPrefab; }
            set { _metaReconstructionPrefab = value; }
        }

        /// <summary>
        /// Whether the surface reconstruction is active or not.
        /// </summary>
        internal bool SurfaceReconstructionActive
        {
            get { return _surfaceReconstructionActive; }
        }

        /// <summary>
        /// Whether the slam relocalization is active or not.
        /// </summary>
        internal bool SlamRelocalizationActive
        {
            get { return _slamRelocalizationActive; }
        }
        
        private void Awake()
        {
            EnvironmentInitializerFactory environmentInitializerFactory = new EnvironmentInitializerFactory(metaContext, _environmentScanControllerPrefab);
            if (_surfaceReconstructionActive)
            {
                new MetaReconstructionFactory(metaContext, _metaReconstructionPrefab).Create(transform.parent);
            }
            _environmentInitializer = environmentInitializerFactory.Create(_slamRelocalizationActive, _surfaceReconstructionActive, _environmentProfileType);
        }

        private void Start()
        {
            if (_environmentInitializer == null)
            {
                throw new NullReferenceException("EnvironmentInitializer");
            }
            
            _environmentInitializer.Start();
        }
    }
}
