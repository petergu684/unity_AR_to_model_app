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
using System.Linq;

namespace Meta.HandInput
{
    /// <summary>
    /// Represents sdk HandData in the game world.
    /// </summary>
    public class Hand : MonoBehaviour
    {
        /// <summary>
        /// Hand which this feature belongs to.
        /// </summary>
        private HandData _handHand;

        /// <summary>
        /// List of all hand features.
        /// </summary>
        private HandFeature[] _allHandFeatures;

        /// <summary>
        /// The type.
        /// </summary>
        public HandType HandType
        {
            get { return _handHand.HandType; }
        }

        /// <summary>
        /// Interop hand datastructure.
        /// </summary>
        public HandData Data
        {
            get { return _handHand; }
        }

        /// <summary>
        /// The center position of the hand.
        /// </summary>
        public CenterHandFeature Palm
        {
            get { return _allHandFeatures.First(feature => feature is CenterHandFeature) as CenterHandFeature; }
        }

        /// <summary>
        /// The topmost position of the hand.
        /// </summary>
        public TopHandFeature Top
        {
            get { return _allHandFeatures.First(feature => feature is TopHandFeature) as TopHandFeature; }
        }

        /// <summary>
        /// Returns weather the hand is currently grabbing.
        /// </summary>
        public bool IsGrabbing
        {
            get { return _handHand.IsGrabbing; }
        }

        /// <summary>
        /// Unique Id associated with the hand.
        /// </summary>
        public int HandId
        {
            get { return _handHand.HandId; }
        }
        
        /// <summary>
        /// Initialize Hand and each of the HandFeatures.
        /// </summary>
        public void InitializeHandData(HandData handData)
        {
            _handHand = handData;

            _allHandFeatures = GetComponentsInChildren<HandFeature>();

            foreach (var handFeature in _allHandFeatures)
            {
                handFeature.Initialize(handData);
            }
        }

        /// <summary>
        /// Notifies each of the HandFeatures that the hand has become invalid.
        /// </summary>
        public void MarkInvalid()
        {
            foreach (var motionHandFeature in _allHandFeatures)
            {
                motionHandFeature.OnInvalid();
            }
        }

        /// <summary>
        /// Returns hand feature of specified type.
        /// </summary>
        /// <typeparam name="THandFeature">Requested HandFeature Type.</typeparam>
        public HandFeature GetChildHandFeature<THandFeature>() where THandFeature : HandFeature
        {
            return _allHandFeatures.First(h => h is THandFeature);
        }
    }
}
