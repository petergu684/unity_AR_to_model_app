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
using UnityEngine.Events;

namespace Meta.Reconstruction
{
    /// <summary>
    /// Triggers the environment reconstruction scanning process, using the keyboard.
    /// </summary>
    public class EnvironmentScanController : BaseEnvironmentScanController
    {
        private enum ScanState
        {
            Waiting,
            Scanning,
            Finished
        }

        [Tooltip("Keys used to start the scan process.")]
        [SerializeField]
        private KeySet _startScanningKey;

        [Tooltip("Keys used to stop the scan process.")]
        [SerializeField]
        private KeySet _stopScanningKey;

        [Tooltip("Time to wait before finish the scan selector.")]
        [SerializeField]
        private float _finishWaitDuration = 0f;

        [Tooltip("Occurs when scan controller is ready to start scanning.")]
        [SerializeField]
        private UnityEvent _scanControllerStarted = new UnityEvent();

        [Tooltip("Occurs when scan process is finished.")]
        [SerializeField]
        private UnityEvent _scanFinished = new UnityEvent();

        [Tooltip("Occurs when scan process is stopped.")]
        [SerializeField]
        private UnityEvent _scanStopped = new UnityEvent();

        private ScanState _scanState = ScanState.Waiting;
        
        private void Update()
        {
            if (_startScanningKey.GetDown())
            {
                StartScanning();
            }

            if (_stopScanningKey.GetDown())
            {
                FinishScanning();
            }
        }

        /// <summary>
        /// Stops environment reconstruction scanning process.
        /// </summary>
        public override void StopScanning()
        {
            StopAllCoroutines();
            if (_scanState == ScanState.Waiting || _scanState == ScanState.Finished)
            {
                return;
            }
            StopReconstruction();
            _scanStopped.Invoke();
        }

        private void StopReconstruction()
        {
            Validate();
            _metaReconstruction.StopReconstruction();
        }

        private void StartScanning()
        {
            Validate();
            if (_scanState == ScanState.Finished || _scanState == ScanState.Scanning)
            {
                return;
            }
            if (_scanState == ScanState.Waiting)
            {
                _scanState = ScanState.Scanning;
                _metaReconstruction.InitReconstruction();
            }
        }

        private void FinishScanning()
        {
            if (_scanState == ScanState.Waiting || _scanState == ScanState.Finished)
            {
                return;
            }
            StopReconstruction();
            StartCoroutine(FinishScan());
        }

        /// <summary>
        /// Initializes the scan controller.
        /// </summary>
        protected override void Initialize()
        {
            _scanControllerStarted.Invoke();
        }

        private IEnumerator FinishScan()
        {
            _scanState = ScanState.Finished;
            _scanFinished.Invoke();
            if (_finishWaitDuration > 0)
            {
                yield return new WaitForSeconds(_finishWaitDuration);
            }
            Finish();
        }

        private void Validate()
        {
            if (_metaReconstruction == null)
            {
                throw new Exception("Please set a MetaReconstruction in order to control the scan process.");
            }
        }
    }
}
