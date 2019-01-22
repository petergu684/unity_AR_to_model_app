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
using System.Collections;
using UnityEngine;

namespace Meta
{
    /// <summary>
    /// Runs a function in a thread, and waits for it. Then call a finish event in the unity thread.
    /// </summary>
    public class UnityThreadedJob : ThreadedJob, IDisposable
    {
        private bool _disposed;
        private Action _currentThreadFunction;
        private MonoBehaviourThreadedJob _threadedJobObject;

        private MonoBehaviourThreadedJob ThreadedJobObject
        {
            get
            {
                if (_threadedJobObject == null)
                {
                    _threadedJobObject = new GameObject("ThreadedJob").AddComponent<MonoBehaviourThreadedJob>();
                    _threadedJobObject.Disabled.AddListener(Abort);
                    #if UNITY_EDITOR
                    {
                        _threadedJobObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                    }
                    #endif
                }
                return _threadedJobObject;
            }
        }
        
        ~UnityThreadedJob()
        {
            Clean();
        }
        
        /// <summary>
        /// Cleans resourses.
        /// </summary>
        public void Dispose()
        {
            Clean();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Runs a function in the thread from. doneAction will be called when the thread is ready.
        /// </summary>
        /// <param name="threadAction">The function to be run in a thread.</param>
        /// <param name="doneAction">Event that occurs when the threadAction is done.</param>
        public void RunFunction(System.Action threadAction, System.Action doneAction)
        {
            if (threadAction == null)
            {
                throw new ArgumentNullException("threadAction");
            }

            ThreadedJobObject.StartCoroutine(RunFunctionFromGameObject(threadAction, () =>
            {
                if (doneAction != null)
                {
                    doneAction();
                }
            }));
        }

        /// <summary>
        /// Runs a function in the thread from a gameObject. doneAction will be called when the thread is ready.
        /// </summary>
        /// <param name="threadAction">The function to be run in a thread.</param>
        /// <param name="doneAction">Event that occurs when the threadAction is done.</param>
        public IEnumerator RunFunctionFromGameObject(System.Action action, System.Action doneAction)
        {
            while (!IsDone)
            {
                yield return 0;
            }

            _currentThreadFunction = action;

            Start();
            while (!IsDone)
            {
                yield return 0;
            }

            if (doneAction != null)
            {
                doneAction();
            }
        }

        /// <summary>
        /// Thread function that will run in the thread.
        /// </summary>
        protected override void ThreadFunction()
        {
            if (_currentThreadFunction != null)
            {
                _currentThreadFunction();
            }
        }

        private void Clean()
        {
            if (_disposed)
            {
                return;
            }

            if (_threadedJobObject != null)
            {
                _threadedJobObject.MarkToDestroy();
                _threadedJobObject = null;
            }

            _disposed = true;
        }
    }
}
