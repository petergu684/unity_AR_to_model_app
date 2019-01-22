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
using UnityEngine.EventSystems;

namespace Meta
{
    /// <summary>
    /// The Gaze class allows MetaBodies to receive OnGazeStart and OnGazeEnd events
    /// </summary>
    internal class Gaze : IEventReceiver
    {

        /// <summary>
        /// The GameObject that is currently being gazed at
        /// </summary>
        private GameObject _currentlyGazedObject;
        /// <summary>
        /// The GameObject that is currently being gazed at
        /// </summary>
        public GameObject currentlyGazedObject
        {
            get { return _currentlyGazedObject; }
        }
        /// <summary>
        /// Whether the currently gazed object implements an interface
        /// </summary>
        /// <remarks>
        /// These objects have received the OnGazeStart event and therefore should also receive the OnGazeEnd event.
        /// This saves us from having to call the ObjectImplemenetsGazeInterface() method twice.
        /// </remarks>
        private bool _objectImplementsInterface;

        /// <summary>
        /// Adds the IEventReceiver functions to the delegates in order to be called from MetaManager
        /// </summary>
        public void Init(IEventHandlers eventHandlers)
        {
            eventHandlers.SubscribeOnUpdate(Update);
        }

        /// <summary>
        /// Runs the update loop
        /// </summary>
        private void Update()
        {
            UpdateGazeCast(Camera.main.transform.position, Camera.main.transform.forward);
        }

        /// <summary>
        /// Updates the gazed GameObject and sends OnGazeStart event to newly gazed GameObject
        /// </summary>
        private void UpdateGazeCast(Vector3 origin, Vector3 direction)
        {
            GameObject gazedObject = null;
            RaycastHit hit;
            if (UnityEngine.Physics.Raycast(origin, direction, out hit, Mathf.Infinity))
            {
                gazedObject = hit.collider.gameObject;
                if (gazedObject != _currentlyGazedObject)
                {
                    EndGaze();
                }
                StartGaze(gazedObject);
                _currentlyGazedObject = gazedObject;
            }
            else
            {
                EndGaze();
            }
            _currentlyGazedObject = gazedObject;
        }

        /// <summary>
        /// Sends the OnGazeStart event to an object that is just being gazed at
        /// </summary>
        private void StartGaze(GameObject gazedObject)
        {
            
            if (gazedObject != _currentlyGazedObject && ObjectImplemenetsGazeInterface(gazedObject))
            {
                ExecuteEvents.Execute<IGazeStartEvent>(gazedObject, null, (x, y) => x.OnGazeStart());
                _objectImplementsInterface = true;
            }
        }

        /// <summary>
        /// Sends the OnGazeEnd event to an object that is no longer being gazed at
        /// </summary>
        private void EndGaze()
        {
            
            if (_currentlyGazedObject != null && _objectImplementsInterface)
            {
                ExecuteEvents.Execute<IGazeEndEvent>(_currentlyGazedObject, null, (x, y) => x.OnGazeEnd());
                _objectImplementsInterface = false;
            }
        }

        /// <summary>
        /// Checks if a gameobject's monobehaviours implement the IGazeStart or IGazeEnd interfaces
        /// </summary>
        private bool ObjectImplemenetsGazeInterface(GameObject objectToSearch)
        {
            MonoBehaviour[] list = objectToSearch.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour mb in list)
            {
                if (mb is IGazeStartEvent || mb is IGazeEndEvent)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
