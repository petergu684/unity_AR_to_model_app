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
    /// Loads the selected environment reconstruction.
    /// </summary>
    public class EnvironmentReconstructionLoaderStep : EnvironmentInitializationStep
    {
        private readonly IMetaReconstruction _metaReconstruction;
        private readonly IEnvironmentProfileRepository _environmentProfileRepository;

        /// <summary>
        /// Creates an instance of <see cref="EnvironmentReconstructionLoaderStep"/> class.
        /// </summary>
        /// <param name="metaMetaReconstruction">Object that manages the environment reconstruction.</param>
        /// <param name="environmentProfileRepository">Repository to access to the environment profiles.</param>
        public EnvironmentReconstructionLoaderStep(IMetaReconstruction metaMetaReconstruction, IEnvironmentProfileRepository environmentProfileRepository)
        {
            if (metaMetaReconstruction == null)
            {
                throw new ArgumentNullException("metaMetaReconstruction");
            }
            if (environmentProfileRepository == null)
            {
                throw new ArgumentNullException("environmentProfileRepository");
            }

            _environmentProfileRepository = environmentProfileRepository;
            _metaReconstruction = metaMetaReconstruction;
        }

        /// <summary>
        /// Stops the environment initialization process.
        /// </summary>
        public override void Stop()
        {
            base.Stop();
            _metaReconstruction.ReconstructionLoaded.RemoveListener(FinishReconstruction);
        }

        protected override void Initialize()
        {
            IEnvironmentProfile currentEnvironment = _environmentProfileRepository.SelectedEnvironment;

            if (currentEnvironment != null)
            {
                _metaReconstruction.ReconstructionLoaded.AddListener(FinishReconstruction);
                _metaReconstruction.LoadReconstruction(currentEnvironment.Name);
            }
            else
            {
                Finish();
            }
        }

        private void FinishReconstruction(GameObject gameObject)
        {
            _metaReconstruction.ReconstructionLoaded.RemoveListener(FinishReconstruction);
            Finish();
        }
    }
}
