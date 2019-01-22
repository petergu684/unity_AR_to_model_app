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

namespace Meta.Buttons
{
    /// <summary>
    /// Handles setup and execution of localization for the MetaWorld prefab.
    /// </summary>
    internal class MetaButtonEventProvider : IEventReceiver, IMetaButtonEventProvider
    {
        private IMetaButtonEventInterop _interop;
        private event Action<IMetaButton> _mainEvent;
        private ButtonType _lastType;
        private ButtonState _lastState;

        public MetaButtonEventProvider()
        {
#if UNITY_EDITOR
            _interop = new EditorMetaButtonEventInterop();
#else
            _interop = new MetaButtonEventInterop();
#endif
        }

        /// <summary>
        /// Initalises the events for the module.
        /// </summary>
        /// <param name="eventHandlers"></param>
        public void Init(IEventHandlers eventHandlers)
        {
            eventHandlers.SubscribeOnUpdate(Update);
        }

        /// <summary>
        /// Calls the update loop to get new values from the localizer.
        /// </summary>
        private void Update()
        {
            IMetaButton buttonEvent = null;

            // Pull buttons until button queue is exhausted for this update.
            while ((buttonEvent = _interop.GetButtonEvent()) != null)
            {
                // Make sure we send a Release event
                if (buttonEvent.Type != _lastType && _lastState != ButtonState.ButtonRelease)
                {
                    var forceRelease = new MetaButton(_lastType, ButtonState.ButtonRelease, buttonEvent.Timestamp);
                    SendEvent(forceRelease);
                }
                SendEvent(buttonEvent);

                _lastType = buttonEvent.Type;
                _lastState = buttonEvent.State;
            }
        }

        /// <summary>
        /// Send the event
        /// </summary>
        /// <param name="button">Button event</param>
        private void SendEvent(IMetaButton button)
        {
            if (_mainEvent != null)
            {
                _mainEvent.Invoke(button);
            }
        }

        /// <summary>
        /// Subscribe to the button events
        /// </summary>
        /// <param name="action">Action to register</param>
        public void Subscribe(Action<IMetaButton> action)
        {
            _mainEvent += action;
        }

        /// <summary>
        /// Unsubscribe to the button events
        /// </summary>
        /// <param name="action">Action to unregister</param>
        public void Unsubscribe(Action<IMetaButton> action)
        {
            _mainEvent -= action;
        }
    }
}
