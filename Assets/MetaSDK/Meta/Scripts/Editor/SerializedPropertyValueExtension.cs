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
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;


/// <summary>
/// A series of extensions for SerializedProperty, to make common tasks easier.
/// Based largely on: https://gist.github.com/capnslipp/8516384
/// </summary>
public static class SerializedPropertyValueExtension
{
	/// <summary>
	/// Get a SerializedProperty value of type T.
	/// </summary>
	/// <typeparam name="T">Type of value to get.</typeparam>
	/// <returns>The stored value of type T.</returns>
    public static T Value<T>(this SerializedProperty thisSP)
	{
		Type valueType = typeof(T);
		
		// First, do special Type checks
		if (valueType.IsEnum)
			return (T)Enum.ToObject(valueType, thisSP.enumValueIndex);
		
		// Next, check for literal UnityEngine struct-types
		if (typeof(Color).IsAssignableFrom(valueType))
			return (T)(object)thisSP.colorValue;
		if (typeof(LayerMask).IsAssignableFrom(valueType))
			return (T)(object)thisSP.intValue;
		if (typeof(Vector2).IsAssignableFrom(valueType))
			return (T)(object)thisSP.vector2Value;
		if (typeof(Vector3).IsAssignableFrom(valueType))
			return (T)(object)thisSP.vector3Value;
		if (typeof(Rect).IsAssignableFrom(valueType))
			return (T)(object)thisSP.rectValue;
		if (typeof(AnimationCurve).IsAssignableFrom(valueType))
			return (T)(object)thisSP.animationCurveValue;
		if (typeof(Bounds).IsAssignableFrom(valueType))
			return (T)(object)thisSP.boundsValue;
		if (typeof(Gradient).IsAssignableFrom(valueType))
			return (T)(object)SafeGradientValue(thisSP);
		if (typeof(Quaternion).IsAssignableFrom(valueType))
			return (T)(object)thisSP.quaternionValue;
		
		// Next, check if derived from UnityEngine.Object base class
		if (typeof(UnityEngine.Object).IsAssignableFrom(valueType))
			return (T)(object)thisSP.objectReferenceValue;
		
		// Finally, check for native type-families
		if (typeof(int).IsAssignableFrom(valueType))
			return (T)(object)thisSP.intValue;
		if (typeof(bool).IsAssignableFrom(valueType))
			return (T)(object)thisSP.boolValue;
		if (typeof(float).IsAssignableFrom(valueType))
			return (T)(object)thisSP.floatValue;
		if (typeof(string).IsAssignableFrom(valueType))
			return (T)(object)thisSP.stringValue;
		if (typeof(char).IsAssignableFrom(valueType))
			return (T)(object)thisSP.intValue;
		
		// And if all fails, throw an exception.
		throw new NotImplementedException("Unimplemented propertyType "+thisSP.propertyType+".");
	}

    /// <summary>
    /// Sets a value of type T.
    /// </summary>
    /// <typeparam name="T">Type of value to save.</typeparam>
    /// <param name="value">Value to save.</param>
    public static void SetValue<T>(this SerializedProperty thisSP, T value) {
        switch (thisSP.propertyType) {
            case SerializedPropertyType.Integer:
                thisSP.intValue = (int) Convert.ChangeType(value, typeof(int));
                break;
            case SerializedPropertyType.Boolean:
                thisSP.boolValue = (bool)Convert.ChangeType(value, typeof(bool));
                break;
            case SerializedPropertyType.Float:
                thisSP.floatValue = (float)Convert.ChangeType(value, typeof(float));
                break;
            case SerializedPropertyType.String:
                thisSP.stringValue = (string)Convert.ChangeType(value, typeof(string));
                break;
            case SerializedPropertyType.Color:
                thisSP.colorValue = (Color)Convert.ChangeType(value, typeof(Color));
                break;
            case SerializedPropertyType.LayerMask:
                thisSP.intValue = (int)Convert.ChangeType(value, typeof(int));
                break;
            case SerializedPropertyType.Enum:
                thisSP.enumValueIndex = (int)Convert.ChangeType(value, typeof(int));
                break;
            case SerializedPropertyType.Vector2:
                thisSP.vector2Value = (Vector2)Convert.ChangeType(value, typeof(Vector2));
                break;
            case SerializedPropertyType.Vector3:
                thisSP.vector3Value = (Vector3)Convert.ChangeType(value, typeof(Vector3));
                break;
            case SerializedPropertyType.Rect:
                thisSP.rectValue = (Rect)Convert.ChangeType(value, typeof(Rect));
                break;
            case SerializedPropertyType.ArraySize:
                thisSP.intValue = (int)Convert.ChangeType(value, typeof(int));
                break;
            case SerializedPropertyType.Character:
                thisSP.intValue = (char)(int)Convert.ChangeType(value, typeof(int));
                break;
            case SerializedPropertyType.AnimationCurve:
                thisSP.animationCurveValue = (AnimationCurve)Convert.ChangeType(value, typeof(AnimationCurve));
                break;
            case SerializedPropertyType.Bounds:
                thisSP.boundsValue = (Bounds)Convert.ChangeType(value, typeof(Bounds));
                break;
            case SerializedPropertyType.Gradient:
                throw new NotImplementedException("gradientValue cannot be set.");
            case SerializedPropertyType.Quaternion:
                thisSP.quaternionValue = (Quaternion)Convert.ChangeType(value, typeof(Quaternion));
                break;

            default:
                throw new NotImplementedException("Unimplemented propertyType " + thisSP.propertyType + ".");
        }
    }

    /// <summary>
    /// Special SetValue overload for UnityEngine.Object values, to avoid the messing casting done in the other function.
    /// </summary>
    /// <param name="value">Value to set.</param>
    public static void SetValue(this SerializedProperty thisSP, Object value) {
        thisSP.objectReferenceValue = value;
    }
	
    
	/// <summary>
    /// Access to SerializedProperty's internal gradientValue property getter, in a manner 
    /// that'll only soft break (returning null) if the property changes or disappears in future Unity revs.
	/// </summary>
	/// <param name="sp">SerializedProperty whose Gradient value should be fetched.</param>
	/// <returns>Gradient value stored in SerializedProperty.</returns>
    static Gradient SafeGradientValue(SerializedProperty sp)
	{
		BindingFlags instanceAnyPrivacyBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
		PropertyInfo propertyInfo = typeof(SerializedProperty).GetProperty(
			"gradientValue",
			instanceAnyPrivacyBindingFlags,
			null,
			typeof(Gradient),
			new Type[0],
			null
		);
		if (propertyInfo == null)
			return null;
		
		Gradient gradientValue = propertyInfo.GetValue(sp, null) as Gradient;
		return gradientValue;
	}

    /// <summary>
    /// Gets the array stored under a SerializedProperty.
    /// </summary>
    /// <typeparam name="T">The Type of the values in an array.</typeparam>
    /// <returns>An array of values of Type T.</returns>
    public static T[] ArrayValue<T>(this SerializedProperty thisSP) {
        if (!thisSP.isArray) {
            throw new Exception("Serialized Property was not an array.");
        }
        T[] array = new T[thisSP.arraySize];
        for (int i = 0; i < thisSP.arraySize; i++) {
            array[i] = thisSP.GetArrayElementAtIndex(i).Value<T>();
        }
        return array;
    }

    /// <summary>
    /// Gets the list stored under a SerializedProperty.
    /// </summary>
    /// <typeparam name="T">The Type of the values in a list.</typeparam>
    /// <returns>An list of values of Type T.</returns>
    public static List<T> ListValue<T>(this SerializedProperty thisSP)
    {
        if (!thisSP.isArray)
        {
            throw new Exception("Serialized Property was not an array.");
        }
        List<T> list = new List<T>();
        for (int i = 0; i < thisSP.arraySize; i++)
        {
            list.Add(thisSP.GetArrayElementAtIndex(i).Value<T>());
        }
        return list;
    }

    /// <summary>
    /// Saves an array to a SerializedProperty.
    /// </summary>
    /// <typeparam name="T">The Type of the values in the array.</typeparam>
    /// <param name="array">Array to save.</param>
    public static void SaveArray<T>(this SerializedProperty thisSP, T[] array)
    {
        if (!thisSP.isArray)
        {
            throw new Exception("Serialized Property was not an array.");
        }
        thisSP.ClearArray();
        thisSP.arraySize = array.Length;
        for (int i = 0; i < array.Length; i++) {
            thisSP.FindPropertyRelative("Array.data[" + i + "]").SetValue(array[i]);
        }
    }

    /// <summary>
    /// Saves a list to a SerializedProperty.
    /// </summary>
    /// <typeparam name="T">The Type of the values in the list.</typeparam>
    /// <param name="list">List to save.</param>
    public static void SaveList<T>(this SerializedProperty thisSP, List<T> list)
    {
        if (!thisSP.isArray)
        {
            throw new Exception("Serialized Property was not an array.");
        }
        thisSP.ClearArray();
        thisSP.arraySize = list.Count;
        for (int i = 0; i < list.Count; i++)
        {
            thisSP.FindPropertyRelative("Array.data[" + i + "]").SetValue(list[i]);
        }
    }

    /// <summary>
    /// Saves an array of UnityEngine.Objects to a SerializedProperty.
    /// </summary>
    /// <param name="array">Array of Objects to save.</param>
    public static void SaveObjectArray(this SerializedProperty thisSP, Object[] array)
    {
        if (!thisSP.isArray)
        {
            throw new Exception("Serialized Property was not an array.");
        }
        thisSP.ClearArray();
        thisSP.arraySize = array.Length;
        for (int i = 0; i < array.Length; i++)
        {
            thisSP.FindPropertyRelative("Array.data[" + i + "]").objectReferenceValue = array[i];
        }
    }

    /// <summary>
    /// Saves a list of UnityEngine.Objects to a SerializedProperty.
    /// </summary>
    /// <param name="list">List of Objects to save.</param>
    public static void SaveObjectList(this SerializedProperty thisSP, List<Object> list)
    {
        Debug.LogWarning("Deprecated. Please use SaveObjectArray using LINQ ToArray method.");
    }
}
