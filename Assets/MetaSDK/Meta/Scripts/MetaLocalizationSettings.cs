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
using System.Collections;
using Meta;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace Meta
{
    /// <summary>
    /// To be attached to any gameObject in a Scene containing the meta2 gameObject.
    /// This script allows the user to select from a drop-down menu the localizer to be used.
    /// </summary>
    [Serializable]
    public class MetaLocalizationSettings : MetaBehaviour
    {
        [SerializeField]
        private string _selectedLocalizerName;

        public void Start()
        {
            //Debug.Log("at start: " + m_listIdx); //manual test
            AssignLocalizationType(Type.GetType(_selectedLocalizerName, false));
            ILocalizer localizerMember = GetComponent<ILocalizer>();
            if (localizerMember != null)
            {
                metaContext.Get<MetaLocalization>().SetLocalizer(localizerMember.GetType());
            }
        }

        /// <summary>
        /// Gets a list of all class types that are descendants of the interface 'ILocalizer'
        /// </summary>
        /// <returns>List of Type, which contains all the types descending from ILocalizer </returns>
        public List<Type> GetLocalizationTypes()
        {
            Type baseType = typeof(ILocalizer);
            var assembly = Assembly.GetExecutingAssembly();
            var types = assembly.GetTypes().Where(baseType.IsAssignableFrom).Where(t => baseType != t).ToList();
            return types;
        }

        /// <summary>
        /// Get the localizer assigned as a component
        /// </summary>
        /// <returns></returns>
        public ILocalizer GetAssignedLocalizer()
        {
            if (Application.isPlaying)
            {
                return metaContext.Get<MetaLocalization>().GetLocalizer();
            }

            var oldComponents = GetComponents<ILocalizer>();
            if (oldComponents != null && oldComponents.Length > 0)
            {
                return oldComponents[0];
            }

            return null;
        }

        /// <summary>
        /// Assigns the localization type as a component member of the GameObject
        /// </summary>
        /// <param name="localizationType"></param>
        public void AssignLocalizationType(Type localizationType)
        {
            if (localizationType == null)
            {
                Debug.LogError("localizationType cannot be null.");
                return;
            }

            if (Application.isPlaying)
            {
                metaContext.Get<MetaLocalization>().SetLocalizer(localizationType);
                //Change the localizer in case that this was not invoked by the editor.
                _selectedLocalizerName = localizationType.ToString();
            }
            else
            {
                //Record a list of existing localizers assigned
                var oldComponents = GetComponents<ILocalizer>();
                if (oldComponents != null && oldComponents.Length > 0 && oldComponents[0].GetType() == localizationType)
                {
                    return; //Avoid reassignment and loss of set script values
                }

                //Assign the new localizer
                gameObject.AddComponent(localizationType);
                
                //Support for hotswapping the localizer method
                if (oldComponents != null)
                {
                    foreach (var component in oldComponents)
                    {
                        if (component != null)
                        {
                            DestroyImmediate((UnityEngine.Object)component);
                        }
                    }
                }
            }
        }
    }
}
