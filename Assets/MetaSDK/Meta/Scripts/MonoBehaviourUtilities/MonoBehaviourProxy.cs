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
using UnityEngine;

namespace Meta
{
    /// <summary>
    /// Handles the access to a prefab, in order to create and destroy one instance of it.
    /// </summary>
    /// <typeparam name="T">Prefab type.</typeparam>
    public class MonoBehaviourProxy<T> : IMonoBehaviourProxy<T> where T : UnityEngine.Object
    {
        private readonly T _prefab;
        private PrefabInstantiator _instantiator;

        private PrefabInstantiator Instantiator
        {
            get
            {
                if (_instantiator == null)
                {
                    _instantiator = new GameObject("instantiator").AddComponent<PrefabInstantiator>();
                    #if UNITY_EDITOR
                    {
                        _instantiator.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                    }
                    #endif
                }
                return _instantiator;
            }
        }

        /// <summary>
        /// Gets the current instantiated object.
        /// </summary>
        public T CurrentObject { get; private set; }

        /// <summary>
        /// Creates an instance of <see cref="CurrentObject"/> class.
        /// </summary>
        public MonoBehaviourProxy(T prefab)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException("prefab");
            }
            _prefab = prefab;
        }

        /// <summary>
        /// Creates a new instance of the prefab.
        /// </summary>
        public T Create()
        {
            if (CurrentObject != null)
            {
                throw new Exception("You can only instantiate one object");
            }

            CurrentObject = Instantiator.InstantiateObject(_prefab);
            return CurrentObject;
        }

        /// <summary>
        /// Destroys the current instantiated object.
        /// </summary>
        public void Destroy()
        {
            if (_instantiator != null)
            {
                Component currentComponent = CurrentObject as Component;
                if (currentComponent != null)
                {
                    _instantiator.DestroyGameObject(currentComponent.gameObject);
                }
                else
                {
                    _instantiator.DestroyGameObject(CurrentObject);
                }

                _instantiator.Destroy();

                CurrentObject = null;
                _instantiator = null;
            }
            else
            {
                throw new Exception("You can only Destroy if you created the object before.");
            }
        }
    }
}
