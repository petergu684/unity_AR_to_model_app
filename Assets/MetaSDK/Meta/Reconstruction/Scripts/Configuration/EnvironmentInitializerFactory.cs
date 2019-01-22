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
    /// Creates an <see cref="IEnvironmentInitializer"/> to triggers the environment initialization.
    /// </summary>
    internal class EnvironmentInitializerFactory
    {
        private IMetaContextInternal _metaContext;
        private BaseEnvironmentScanController _environmentScanControllerPrefab;
        private MetaLocalization _metaLocalization;
        private IEnvironmentProfileRepository _environmentProfileRepository;
        private ISlamLocalizer _slamLocalizer;

        /// <summary>
        /// Creates an instance of <see cref="EnvironmentInitializerFactory"/> class.
        /// </summary>
        /// <param name="metaContext">The Context to acces to the required services.</param>
        /// <param name="environmentScanControllerPrefab">The scan selector prefab used to control the environment reconstruction.</param>
        internal EnvironmentInitializerFactory(IMetaContextInternal metaContext, BaseEnvironmentScanController environmentScanControllerPrefab)
        {
            if (metaContext == null)
            {
                throw new ArgumentNullException("metaContext");
            }

            _metaContext = metaContext;
            _environmentScanControllerPrefab = environmentScanControllerPrefab;
            _metaLocalization = _metaContext.Get<MetaLocalization>();
            _environmentProfileRepository = _metaContext.Get<IEnvironmentProfileRepository>();
            _slamLocalizer = GameObject.FindObjectOfType<SlamLocalizer>();
        }

        /// <summary>
        /// Creates an <see cref="IEnvironmentInitializer"/> for the given environment selection result.
        /// </summary>
        /// <param name="slamRelocalizationActive">Whether the slam relocalization is active or not.</param>
        /// <param name="surfaceReconstructionActive">Whether the surface reconstruction is active or not.</param>
        /// <param name="environmentProfileType">The environment profile type.</param>
        /// <returns>The environment initializer for the given environment initializer type.</returns>
        internal IEnvironmentInitializer Create(bool slamRelocalizationActive, bool surfaceReconstructionActive, EnvironmentProfileType environmentProfileType)
        {
            if (_slamLocalizer == null)
            {
                return CreateDefaultInitializer();
            }

            if (slamRelocalizationActive)
            {
                return CreateRelocalizationActiveInitializer(surfaceReconstructionActive, environmentProfileType);
            }
            return CreateRelocalizationInactiveInitializer(surfaceReconstructionActive);
        }

        private IEnvironmentInitializer CreateRelocalizationInactiveInitializer(bool surfaceReconstructionActive)
        {
            if (!surfaceReconstructionActive)
            {
                return CreateDefaultInitializer();
            }

            IMetaReconstruction metaReconstruction = _metaContext.Get<IMetaReconstruction>();
            IEnvironmentInitialization initialization = new ReconstructionOnlyEnvironmentInitialization(_slamLocalizer, metaReconstruction, _environmentScanControllerPrefab);
            IEnvironmentReset environmentReset = new ReconstructionOnlyEnvironmentReset(metaReconstruction);
            
            return new EnvironmentInitializer(initialization, environmentReset, _metaLocalization);
        }

        private IEnvironmentInitializer CreateRelocalizationActiveInitializer(bool surfaceReconstructionActive, EnvironmentProfileType environmentProfileType)
        {
            switch (environmentProfileType)
            {
                case EnvironmentProfileType.DefaultProfile:
                    return CreateDefaultProfileInitializer(surfaceReconstructionActive);
                default:
                    throw new Exception(string.Format("EnvironmentProfileType {0} is not supported.", environmentProfileType));
            }
        }

        private IEnvironmentInitializer CreateDefaultProfileInitializer(bool surfaceReconstructionActive)
        {
            ISlamChecker slamChecker = new SlamChecker(_slamLocalizer);
            IEnvironmentProfileSelector environmentProfileSelector = new DefaultEnvironmentProfileSelector(_environmentProfileRepository, slamChecker);
            IEnvironmentInitializationFactory environmentInitializationFactory;
            IEnvironmentReset environmentReset;

            if (surfaceReconstructionActive)
            {
                IMetaReconstruction metaReconstruction = _metaContext.Get<IMetaReconstruction>();
                environmentInitializationFactory = new DefaultEnvironmentWithReconstructionInitializationFactory(_environmentProfileRepository, _slamLocalizer, metaReconstruction, _environmentScanControllerPrefab);
                environmentReset = new DefaultEnvironmentReset(_environmentProfileRepository, metaReconstruction);
            }
            else
            {
                environmentInitializationFactory = new DefaultEnvironmentInitializationFactory(_environmentProfileRepository, _slamLocalizer);
                environmentReset = new DefaultEnvironmentReset(_environmentProfileRepository);
            }
            return new EnvironmentProfileInitializer(environmentProfileSelector, environmentInitializationFactory, environmentReset, _metaLocalization);
        }

        private IEnvironmentInitializer CreateDefaultInitializer()
        {
            return new EnvironmentDefaultInitializer();
        }
    }
}
