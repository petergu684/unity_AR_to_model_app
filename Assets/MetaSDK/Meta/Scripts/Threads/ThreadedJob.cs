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
using System.Threading;

namespace Meta
{
    /// <summary>
    /// Runs a function in a thread.
    /// </summary>
    public abstract class ThreadedJob
    {
        private bool _isDone = true;
        private object _jobLock = new object();
        private Thread _thread = null;

        /// <summary>
        /// Whether the thread function is done or not.
        /// </summary>
        protected bool IsDone
        {
            get
            {
                bool isDone;
                lock (_jobLock)
                {
                    isDone = _isDone;
                }
                return isDone;
            }
            set
            {
                lock (_jobLock)
                {
                    _isDone = value;
                }
            }
        }
        
        /// <summary>
        /// The function that is going to run in the thread.
        /// </summary>
        protected abstract void ThreadFunction();

        private void Run()
        {
            ThreadFunction();
            IsDone = true;
        }

        /// <summary>
        /// Creates and start a new thread.
        /// </summary>
        public virtual void Start()
        {
            if (_isDone)
            {
                _isDone = false;
                _thread = new Thread(Run);
                _thread.Start();
            }
            else
            {
                throw new System.Exception("ThreadedJob.Start: A job is already running");
            }
        }

        /// <summary>
        /// Abort the current thread.
        /// </summary>
        public virtual void Abort()
        {
            if (_thread != null && _thread.ThreadState == ThreadState.Running)
            {
                _thread.Abort();
            }
            _isDone = true;
        }
    }
}
