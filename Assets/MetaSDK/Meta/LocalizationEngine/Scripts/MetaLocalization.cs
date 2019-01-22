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
using UnityEngine.Events;
using System;

namespace Meta
{
    /// <summary>
    /// Handles setup and execution of localization for the MetaWorld prefab.
    /// </summary>
    internal class MetaLocalization : IEventReceiver
    {
        private UnityEvent _localizationWillReset = new UnityEvent();
        private UnityEvent _localizationReset = new UnityEvent();
        private GameObject _targetGO;
        private ILocalizer _currentLocalizer;
        private KeyCode _resetShortcut = KeyCode.F4;

        /// <summary>
        /// Occurs before the localization reset.
        /// </summary>
        public UnityEvent LocalizationWillReset
        {
            get { return _localizationWillReset; }
        }

        /// <summary>
        /// Occurs after the localization reset.
        /// </summary>
        public UnityEvent LocalizationReset
        {
            get { return _localizationReset; }
        }

        /// <summary>
        /// Constructor for the localization module.
        /// </summary>
        /// <param name="targetGO">The object to be updated with values from the localizer.</param>
        /// <param name="currentLocalizer">The localization method to be used.</param>
        internal MetaLocalization(GameObject targetGO)
        {
            _targetGO = targetGO;
        }

        /// <summary>
        /// Initializes the events for the module.
        /// </summary>
        /// <param name="eventHandlers"></param>
        public void Init(IEventHandlers eventHandlers)
        {
            eventHandlers.SubscribeOnUpdate(Update);
        }

        /// <summary>
        /// Sets the localizer to be used for position and rotation tracking.
        /// </summary>
        /// <param name="localizerType">The type of localizer to be used.</param>
        public void SetLocalizer(Type localizerType)
        {
            if (_currentLocalizer != null && localizerType == _currentLocalizer.GetType())
            {
                return; //The same type of localizer is already assigned
            }

            //Get the components that are ILocalizers from the target object
            var oldComponents = _targetGO.GetComponents<ILocalizer>();
            if (oldComponents != null && oldComponents.Length > 0 && oldComponents[0].GetType() == localizerType)
            {
                SetLocalizer(oldComponents[0]);
                return; //Avoid reassignment and loss of set script values in editor
            }
            if (!(typeof(ILocalizer).IsAssignableFrom(localizerType)))
            {
                return; // type was not a ILocalizer
            }

            if (oldComponents != null)
            {
                foreach (var component in oldComponents)
                {
                    if (component != null)
                    {
                        GameObject.DestroyImmediate((UnityEngine.Object) component);
                    }
                }
            }
            SetLocalizer(_targetGO.AddComponent(localizerType) as ILocalizer);
        }

        /// <summary>
        /// Gets the current localizer.
        /// </summary>
        /// <returns></returns>
        public ILocalizer GetLocalizer()
        {
            return _currentLocalizer;
        }

        /// <summary>
        /// Resets the currently enabled localizer.
        /// </summary>
        public void ResetLocalization()
        {
            if (_currentLocalizer != null)
            {
                _localizationWillReset.Invoke();
                _currentLocalizer.ResetLocalizer();
                _localizationReset.Invoke();
            }
        }

        /// <summary>
        /// Calls the update loop to get new values from the localizer.
        /// </summary>
        private void Update()
        {
            if (_currentLocalizer != null)
            {
                if (Input.GetKeyDown(_resetShortcut))
                {
                    ResetLocalization();
                }
                _currentLocalizer.UpdateLocalizer();
            }
        }

        private void SetLocalizer(ILocalizer localizer)
        {
            _currentLocalizer = localizer;
            _currentLocalizer.SetTargetGameObject(_targetGO);
        }
    }
}
