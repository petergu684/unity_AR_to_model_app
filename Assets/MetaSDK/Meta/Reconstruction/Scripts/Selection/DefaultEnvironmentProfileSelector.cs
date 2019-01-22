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
    /// Auto selects the default environment profile.
    /// </summary>
    public class DefaultEnvironmentProfileSelector : IEnvironmentProfileSelector
    {
        private readonly IEnvironmentProfileRepository _environmentProfileRepository;
        private readonly ISlamChecker _slamChecker;
        private readonly EnvironmentSelectionResultTypeEvent _environmentSelected = new EnvironmentSelectionResultTypeEvent();
        private IEnvironmentProfile _defaultEnvironmentProfile;

        /// <summary>
        /// Occurs when an environment profile is selected.
        /// </summary>
        public EnvironmentSelectionResultTypeEvent EnvironmentSelected
        {
            get { return _environmentSelected; }
        }

        /// <summary>
        /// Creates an instance of <see cref="DefaultEnvironmentProfileSelector"/> class.
        /// </summary>
        /// <param name="environmentProfileRepository">Repository to access to the environment profiles.</param>
        /// <param name="slamChecker">Object to check if an slam map can be localized.</param>
        public DefaultEnvironmentProfileSelector(IEnvironmentProfileRepository environmentProfileRepository, ISlamChecker slamChecker)
        {
            if (environmentProfileRepository == null)
            {
                throw new ArgumentNullException("environmentProfileRepository");
            }

            if (slamChecker == null)
            {
                throw new ArgumentNullException("slamChecker");
            }

            _environmentProfileRepository = environmentProfileRepository;
            _slamChecker = slamChecker;
        }

        /// <summary>
        /// Selects an environment profile.
        /// </summary>
        public void Select()
        {
            Read();
            FinishReading();
        }

        /// <summary>
        /// Resets the environment profile selection.
        /// </summary>
        public void Reset()
        {
            if (_slamChecker != null)
            {
                _slamChecker.Stop();
            }

            _environmentSelected.Invoke(EnvironmentSelectionResultTypeEvent.EnvironmentSelectionResultType.NewEnvironment);
        }
        
        private void Read()
        {
            _environmentProfileRepository.Read();
            _defaultEnvironmentProfile = _environmentProfileRepository.GetDefault();
        }
        
        private void FinishReading()
        {
            //If there is no default environment profile, or if it is not valid, just skip automatically and return to start creating one.
            if (_defaultEnvironmentProfile == null || !_environmentProfileRepository.Verify(_defaultEnvironmentProfile.Id))
            {
                if (_environmentSelected != null)
                {
                    _environmentSelected.Invoke(EnvironmentSelectionResultTypeEvent.EnvironmentSelectionResultType.NewEnvironment);
                }
            }
            else
            {
                //Otherwise, lets select the first one.
                SelectEnvironment(_defaultEnvironmentProfile);
            }
        }

        private void SelectEnvironment(IEnvironmentProfile environmentProfile)
        {
            _slamChecker.TryLocalizeMap(environmentProfile.MapName, (ok) =>
            {
                if (ok)
                {
                    _environmentProfileRepository.Select(environmentProfile.Id);
                }
                _environmentSelected.Invoke(ok ? EnvironmentSelectionResultTypeEvent.EnvironmentSelectionResultType.SelectedEnvironment : EnvironmentSelectionResultTypeEvent.EnvironmentSelectionResultType.NewEnvironment);
            });
        }
    }
}
