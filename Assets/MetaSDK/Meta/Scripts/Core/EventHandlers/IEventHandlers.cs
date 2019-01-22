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

namespace Meta
{
    /// <summary>
    /// Subscribe or unsubscribe to specific unity events
    /// </summary>
    internal interface IEventHandlers
    {
        /// <summary>
        /// Subsribe to Unity Awake event
        /// </summary>
        /// <param name="action">Action to be triggered on Awake</param>
        void SubscribeOnAwake(Action action);

        /// <summary>
        /// Unsubscribe to Unity Awake event
        /// </summary>
        /// <param name="action">Action to be unsubscribed from Awake</param>
        void UnSubscribeOnAwake(Action action);

        /// <summary>
        /// Subsribe to Unity Start event
        /// </summary>
        /// <param name="action">Action to be triggered on Start</param>
        void SubscribeOnStart(Action action);

        /// <summary>
        /// Unsubscribe to Unity Start event
        /// </summary>
        /// <param name="action">Action to be unsubscribed from Start</param>
        void UnSubscribeOnStart(Action action);

        /// <summary>
        /// Subsribe to Unity Update event
        /// </summary>
        /// <param name="action">Action to be triggered on Update</param>
        void SubscribeOnUpdate(Action action);

        /// <summary>
        /// Unsubscribe to Unity Update event
        /// </summary>
        /// <param name="action">Action to be unsubscribed from Update</param>
        void UnSubscribeOnUpdate(Action action);

        /// <summary>
        /// Subsribe to Unity FixedUpdate event
        /// </summary>
        /// <param name="action">Action to be triggered on FixedUpdate</param>
        void SubscribeOnFixedUpdate(Action action);

        /// <summary>
        /// Unsubscribe to Unity FixedUpdate event
        /// </summary>
        /// <param name="action">Action to be unsubscribed from FixedUpdate</param>
        void UnSubscribeOnFixedUpdate(Action action);

        /// <summary>
        /// Subsribe to Unity LateUpdate event
        /// </summary>
        /// <param name="action">Action to be triggered on LateUpdate</param>
        void SubscribeOnLateUpdate(Action action);

        /// <summary>
        /// Unsubscribe to Unity LateUpdate event
        /// </summary>
        /// <param name="action">Action to be unsubscribed from LateUpdate</param>
        void UnSubscribeOnLateUpdate(Action action);

        /// <summary>
        /// Subsribe to Unity Destroy event
        /// </summary>
        /// <param name="action">Action to be triggered on Destroy</param>
        void SubscribeOnDestroy(Action action);

        /// <summary>
        /// Unsubscribe to Unity Destroy event
        /// </summary>
        /// <param name="action">Action to be unsubscribed from Destroy</param>
        void UnSubscribeOnDestroy(Action action);

        /// <summary>
        /// Subsribe to Unity ApplicationQuit event
        /// </summary>
        /// <param name="action">Action to be triggered on ApplicationQuit</param>
        void SubscribeOnApplicationQuit(Action action);

        /// <summary>
        /// Unsubscribe to Unity ApplicationQuit event
        /// </summary>
        /// <param name="action">Action to be unsubscribed from ApplicationQuit</param>
        void UnSubscribeOnApplicationQuit(Action action);
    }
}
