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
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Meta.HandInput
{
    public class HandUtil
    {
     
        /// <summary>
        /// Instantiates a new HandTemplate prefab.
        /// </summary>
        /// <param name="handData">HandData of the hand to be built. </param>
        /// <returns></returns>
        public static Hand CreateNewHand(HandData handData)
        {
            var prefabName = HandName(handData.HandType);
            var handProxyObject = Object.Instantiate(Resources.Load<GameObject>("Prefabs/" + prefabName));
            var handProxy = handProxyObject.GetComponent<Hand>();

            // -- Initialize template hand features.
            handProxy.InitializeHandData(handData);
            return handProxy;
        }

        /// <summary>
        /// Util Method used to build a HandTemplate Prefab
        /// </summary>
        /// <param name="type"> HandType of the hand to be built </param>
        /// <returns></returns>
        public static Hand InitializeTemplateHand(HandType type)
        {
            var prefabName = HandName(type);

            var template = new GameObject(prefabName);
            var templateHand = template.AddComponent<Hand>();

            CreateHandFeature<CenterHandFeature>(templateHand);
            CreateHandFeature<TopHandFeature>(templateHand);
            
            return templateHand;
        }



        public static void SetupCollider(GameObject gameObject)
        {
            if (gameObject.GetComponent<Collider>() != null && !gameObject.GetComponent<Collider>().isTrigger)
            {
                Debug.LogWarning("Setting Collider associated with " + gameObject.name + " to HandFeature layer.This is required to interact with the HandFeature GameObjects in the MetaHands prefab.");
                gameObject.GetComponent<Collider>().isTrigger = true;
            }
        }


        private static T CreateHandFeature<T>(Component handProxy) where T : HandFeature
        {
            var feature = handProxy.gameObject.GetComponentInChildren<T>();
            if (feature == null)
            {
                var featureObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                featureObject.name = typeof(T).Name;
                featureObject.transform.SetParent(handProxy.transform);

                feature = featureObject.AddComponent<T>();
            }

            Switch[typeof(T)](feature.transform);

            return feature;
        }

        private static string HandName(HandType type)
        {
            return string.Format("HandTemplate ({0})", type);
        }

        private static readonly Dictionary<Type, Action<Transform>> Switch = new Dictionary<Type, Action<Transform>> {
            { typeof(CenterHandFeature), transform => { transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);} },
            { typeof(TopHandFeature), transform => { transform.localScale = new Vector3(0.0175f, 0.0175f, 0.0175f);} },
        };
    }
}
