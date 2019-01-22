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
    /// Broadcast the button events from the headset via GameObject.FindObjectsOfType.
    /// This class is not performant but is the easiest way to interact with buttons.
    /// For more performant implementations please see MetaButtonIndividualEventBroadcaster, MetaButtonGeneralEventBroadcaster or MetaButtonEventBroadcaster.
    /// </summary>
    public class MetaButtonGameObjectEventBroadcaster : BaseMetaButtonEventBroadcaster
    {
        [SerializeField]
        private ButtonBroadcastType _broadcastType;

        /// <summary>
        /// Process the button events
        /// </summary>
        /// <param name="button">Button event</param>
        protected override void ProcessButtonEvents(IMetaButton button)
        {
            switch (_broadcastType)
            {
                case ButtonBroadcastType.None:
                    break;
                case ButtonBroadcastType.Scene:
                    BroadcastToAll(button);
                    break;
                case ButtonBroadcastType.Children:
                    BroadcastChildren(button);
                    break;
                case ButtonBroadcastType.Parents:
                    BroadcastParent(button);
                    break;
            }
        }

        /// <summary>
        /// Broadcast to all GameObjects in the scene
        /// </summary>
        /// <param name="button">Button event</param>
        private void BroadcastToAll(IMetaButton button)
        {
            var objects = GameObject.FindObjectsOfType<BaseMetaButtonInteractionObject>();
            for (int i = 0; i < objects.Length; ++i)
            {
                objects[i].OnMetaButtonEvent(button);
            }
        }

        /// <summary>
        /// Broadcast to the children of the current GameObject
        /// </summary>
        /// <param name="button">Button event</param>
        private void BroadcastChildren(IMetaButton button)
        {
            var objects = GetComponentsInChildren<IOnMetaButtonEvent>();
            for (int i = 0; i < objects.Length; ++i)
            {
                objects[i].OnMetaButtonEvent(button);
            }
        }

        /// <summary>
        /// Broadcast to the parents of the current GameObject
        /// </summary>
        /// <param name="button">Button event</param>
        private void BroadcastParent(IMetaButton button)
        {
            var objects = GetComponentsInParent<IOnMetaButtonEvent>();
            for (int i = 0; i < objects.Length; ++i)
            {
                objects[i].OnMetaButtonEvent(button);
            }
        }

        /// <summary>
        /// Gets or sets the Broadcast type of this broadcaster
        /// </summary>
        public ButtonBroadcastType BroadcastType
        {
            get { return _broadcastType; }
            set { _broadcastType = value; }
        }
    }
}
