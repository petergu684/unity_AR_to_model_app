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

namespace Meta.Buttons
{
    /// <summary>
    /// Broadcast each individual button events from the headset via events per button type
    /// </summary>
    public class MetaButtonIndividualEventBroadcaster : BaseMetaButtonEventBroadcaster
    {
        [SerializeField]
        private bool _enableCameraEvents = true;
        [SerializeField]
        private MetaButtonUnityEvent _cameraEvent;
        [SerializeField]
        private bool _enableVolumeUpEvents = true;
        [SerializeField]
        private MetaButtonUnityEvent _volumeUpEvent;
        [SerializeField]
        private bool _enableVolumeDownEvents = true;
        [SerializeField]
        private MetaButtonUnityEvent _volumeDownEvent;

        /// <summary>
        /// Process the button events
        /// </summary>
        /// <param name="button">Button event</param>
        protected override void ProcessButtonEvents(IMetaButton button)
        {
            switch (button.Type)
            {
                case ButtonType.ButtonCamera:
                    RaiseCameraEvent(button);
                    break;
                case ButtonType.ButtonVolumeDown:
                    RaiseVolumeDownEvent(button);
                    break;
                case ButtonType.ButtonVolumeUp:
                    RaiseVolumeUpEvent(button);
                    break;
            }
        }

        /// <summary>
        /// Raise the Camera Button Event
        /// </summary>
        /// <param name="button">Button message</param>
        private void RaiseCameraEvent(IMetaButton button)
        {
            if (!_enableCameraEvents)
            {
                return;
            }
            if (_cameraEvent == null)
            {
                return;
            }
            _cameraEvent.Invoke(button);
        }

        /// <summary>
        /// Raise the Volume Up Button Event
        /// </summary>
        /// <param name="button">Button message</param>
        private void RaiseVolumeUpEvent(IMetaButton button)
        {
            if (!_enableVolumeUpEvents)
            {
                return;
            }
            if (_volumeUpEvent == null)
            {
                return;
            }
            _volumeUpEvent.Invoke(button);
        }

        /// <summary>
        /// Raise the Volume Down Event
        /// </summary>
        /// <param name="button">Button message</param>
        private void RaiseVolumeDownEvent(IMetaButton button)
        {
            if (!_enableVolumeDownEvents)
            {
                return;
            }
            if (_volumeDownEvent == null)
            {
                return;
            }
            _volumeDownEvent.Invoke(button);
        }

        /// <summary>
        /// Enable or Disable the Camera Button Events
        /// </summary>
        public bool EnableCameraEvents
        {
            get { return _enableCameraEvents; }
            set { _enableCameraEvents = value; }
        }

        /// <summary>
        /// Enable or Disable the Volume Up Button Events
        /// </summary>
        public bool EnableVolumeUpEvents
        {
            get { return _enableVolumeUpEvents; }
            set { _enableVolumeUpEvents = value; }
        }

        /// <summary>
        /// Enable or Disable the Volume Down Button Events
        /// </summary>
        public bool EnableVolumeDownEvents
        {
            get { return _enableVolumeDownEvents; }
            set { _enableVolumeDownEvents = value; }
        }
    }
}
