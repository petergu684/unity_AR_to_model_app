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
using meta.types;
using HandData = Meta.HandInput.HandData;

namespace Meta
{
    internal class HandsModule : IEventReceiver
    {
        /// <summary> Specified weather the module is initialized. /// </summary>
        internal bool Initialized { private get; set; }
        /// <summary> Datastructure containing all data for right hand. /// </summary>
        public HandData RightHand { get; private set; }
        /// <summary> Datastructure containing all data for left hand. /// </summary>
        public HandData LeftHand { get; private set; }

        public System.Action<HandData> OnHandEnterFrame;
        public System.Action<HandData> OnHandExitFrame;

        private FrameHands _frame;
        private bool _recievedFirstFrame = false;
        private const int kBuffMaxSize = 4000;
        private byte[] _buffer = new byte[kBuffMaxSize];

        /// <summary>
        /// Container for all hands related data for current camera frame.
        /// </summary>
        internal FrameHands? Frame
        {
            get { return _recievedFirstFrame ? _frame : (FrameHands?)null; }
        }

        public HandsModule(Transform handsOrigin)
        {
            // Initialize Hands Datastructures.

            RightHand = new HandData(handsOrigin);
            LeftHand = new HandData(handsOrigin);

            // Subscribe to hand events.

            RightHand.OnEnterFrame += () =>
            {
                if (OnHandEnterFrame != null)
                {
                    OnHandEnterFrame.Invoke(RightHand);
                }
            };
            LeftHand.OnEnterFrame += () =>
            {
                if (OnHandEnterFrame != null)
                {
                    OnHandEnterFrame.Invoke(LeftHand);
                }
            };

            RightHand.OnExitFrame += () =>
            {
                if (OnHandExitFrame != null)
                {
                    OnHandExitFrame.Invoke(RightHand);
                }
            };
            LeftHand.OnExitFrame += () =>
            {
                if (OnHandExitFrame != null)
                {
                    OnHandExitFrame.Invoke(LeftHand);
                }
            };
        }

        private void Update()
        {
            if (!Initialized) return;

            if (MetaCocoInterop.GetFrameHandsFlatbufferObject(ref _buffer, out _frame))
            {
                _recievedFirstFrame = true;

                meta.types.HandData? incomingRight = null;
                meta.types.HandData? incomingLeft = null;
                for (int i = 0; i < _frame.HandsLength; i++)
                {
                    switch (_frame.Hands(i).Value.HandType)
                    {
                        case meta.types.HandType.RIGHT:
                            incomingRight = _frame.Hands(i);
                            break;
                        default:
                            incomingLeft = _frame.Hands(i);
                            break;
                    }
                }

                RightHand.UpdateHand(incomingRight);
                LeftHand.UpdateHand(incomingLeft);

                RightHand.UpdateEvents();
                LeftHand.UpdateEvents();
            }
        }

        public void Init(IEventHandlers eventHandlers)
        {
            eventHandlers.SubscribeOnUpdate(Update);
        }
    }
}
