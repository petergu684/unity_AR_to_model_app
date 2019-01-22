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
using UnityEditor;
using System.Reflection;
using System;
using System.Runtime.InteropServices;

// https://www.reddit.com/r/oculus/comments/2r3mc5/full_screen_play_mode_a_unity_extension/
// http://stackoverflow.com/questions/1565734/is-it-possible-to-set-private-property-via-reflection
// http://stackoverflow.com/questions/2402739/how-to-retrieve-the-screen-resolution-from-a-c-sharp-winform-app
public class SetupSingleGameWindow : EditorWindow
{
    [MenuItem("Meta 2/Create Meta 2 Game View...")]
    static void OpenPopup()
    {
        EditorWindow window = GetWindow(typeof(SetupSingleGameWindow));
        window.titleContent = new GUIContent("Create Game View");
        window.minSize = window.maxSize = new Vector2(180, 180); // When minSize and maxSize are the same, no OS border is applied to the window.
        window.ShowPopup();
    }

    private const int TAB_HEIGHT = 22; // The size of the toolbar above the game view, excluding the OS border.
#if UNITY_STANDALONE_WIN
    private int BORDER_WIDTH = 5;
#elif UNITY_STANDALONE_OSX
    private int BORDER_WIDTH = 0;
#else
    private int BORDER_WIDTH = 5;
#endif

    private class WindowData
    {
        public string Title = string.Empty;
        public Vector2 DefaultResolution = new Vector2(960, 540);
        public Vector2 DefaultOffset = Vector2.zero;
        public int DisplayNumber = 0;
        public Vector2 MonitorResolution = new Vector2(1920, 1080);
        public Vector2 MonitorOffset = Vector2.zero;
        public bool IsFullScreen = false;
        public EditorWindow EditorWindow = null;
    }
    private static readonly WindowData Meta2WindowData = new WindowData
    {
        Title = "Meta 2",
        DefaultResolution = new Vector2(960, 540),
        DefaultOffset = new Vector2(300, 300),
        DisplayNumber = 1,
        MonitorResolution = new Vector2(2560, 1440),
        MonitorOffset = new Vector2(0, 0),
        IsFullScreen = false,
        EditorWindow = null,
    };

    [DllImport("User32.dll")]
    static extern IntPtr GetDC(IntPtr hwnd);

    [DllImport("User32.dll")]
    static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);

    [DllImport("gdi32.dll")]
    static extern int GetDeviceCaps(IntPtr hdc, int nIndex);


    private float GetScalingFactor()
    {
        IntPtr primary = GetDC(IntPtr.Zero);
        int VERTRES = 10; //observed vertical resolution
        int DESKTOPVERTRES = 117; //actual vertical resolution
        int pixelsY = GetDeviceCaps(primary, VERTRES);
        int actualPixelsY = GetDeviceCaps(primary, DESKTOPVERTRES);
        ReleaseDC(IntPtr.Zero, primary);
        return actualPixelsY / ((float)pixelsY); // 1.25 = 125%
    }

    void OnEnable()
    {
        ReadPlayerPrefs();
    }

    void OnDestroy()
    {
        WritePlayerPrefs();
    }

    void OnGUI()
    {
        EditorGUILayout.Space(); EditorGUILayout.Space();
        ShowWindowData(Meta2WindowData);
        EditorGUILayout.Space(); EditorGUILayout.Space();

        GUI.enabled = !WindowsHaveBeenCreated();
        if (GUILayout.Button("Open Game View"))
        {
            OpenWindow(Meta2WindowData);
        }

        GUI.enabled = !GUI.enabled;
        if (GUILayout.Button("Toggle Full Screen"))
        {
            ToggleWindowState();
        }
        if (GUILayout.Button("Close Game View"))
        {
            CloseWindow(Meta2WindowData);
        }
    }

    private void ShowWindowData(WindowData windowData)
    {
        EditorGUILayout.LabelField((windowData.Title + " ").PadRight(36, '-'));
        windowData.MonitorResolution = EditorGUILayout.Vector2Field("Resolution", new Vector2(windowData.MonitorResolution.x, windowData.MonitorResolution.y));
        windowData.MonitorOffset = EditorGUILayout.Vector2Field("Offset", new Vector2(windowData.MonitorOffset.x, windowData.MonitorOffset.y));
    }

    private void ToggleWindowState()
    {
        if (WindowsAreFullScreen())
        {
            MakeWindowed(Meta2WindowData);
        }
        else
        {
            MakeFullScreen(Meta2WindowData);
        }
    }

    private bool WindowsHaveBeenCreated()
    {
        return
            (Meta2WindowData.EditorWindow != null);
    }

    private bool WindowsAreFullScreen()
    {
        return
            (Meta2WindowData.IsFullScreen);
    }

    private void OpenWindow(WindowData windowData)
    {
        if (windowData.EditorWindow != null) return;

        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        windowData.EditorWindow = (EditorWindow)CreateInstance(T.Name);
        windowData.EditorWindow.titleContent = new GUIContent(windowData.Title);
        windowData.EditorWindow.Show();
        windowData.EditorWindow.minSize = windowData.EditorWindow.maxSize = new Vector2(
            windowData.DefaultResolution.x,
            windowData.DefaultResolution.y + TAB_HEIGHT - BORDER_WIDTH);
        windowData.EditorWindow.position = new Rect(
            windowData.DefaultOffset.x,
            windowData.DefaultOffset.y - TAB_HEIGHT,
            windowData.DefaultResolution.x,
            windowData.DefaultResolution.y + TAB_HEIGHT - BORDER_WIDTH);

        windowData.DisplayNumber = 0; //This is 'display 1'. This fixes an issue with the display going to 2 by default.
        GetPrivateField(windowData.EditorWindow, "m_TargetDisplay").SetValue(windowData.EditorWindow, windowData.DisplayNumber);
    }

    private void MakeWindowed(WindowData windowData)
    {
        if (windowData.EditorWindow == null || !windowData.IsFullScreen) return;

        windowData.IsFullScreen = false;
        windowData.EditorWindow.minSize = windowData.EditorWindow.maxSize = new Vector2(
            windowData.DefaultResolution.x,
            windowData.DefaultResolution.y + TAB_HEIGHT - BORDER_WIDTH);
        windowData.EditorWindow.position = new Rect(
            windowData.DefaultOffset.x,
            windowData.DefaultOffset.y - TAB_HEIGHT,
            windowData.DefaultResolution.x,
            windowData.DefaultResolution.y + TAB_HEIGHT - BORDER_WIDTH);
    }

    private void MakeFullScreen(WindowData windowData)
    {
        if (windowData.EditorWindow == null || windowData.IsFullScreen) return;

        float scalingFactor = GetScalingFactor();
        Vector2 dims = windowData.MonitorResolution / scalingFactor;
        Vector2 offset = windowData.MonitorOffset / scalingFactor;
        int tabHeight = (int)(TAB_HEIGHT / scalingFactor);
        int borderWidth = (int)(BORDER_WIDTH / scalingFactor);


        Rect windowRect = new Rect(
            offset.x,
            offset.y - tabHeight,
            dims.x,
            dims.y + tabHeight - borderWidth);



        windowData.IsFullScreen = true;
        windowData.EditorWindow.minSize = windowData.EditorWindow.maxSize = new Vector2(
            dims.x,
            dims.y + tabHeight - borderWidth);
        windowData.EditorWindow.position = windowRect;


    }

    private void CloseWindow(WindowData windowData)
    {
        if (windowData.EditorWindow == null) return;

        windowData.EditorWindow.Close();
        windowData.EditorWindow = null;
    }

    private void ReadPlayerPrefs()
    {
        // EditorPrefs.DeleteAll();

        Meta2WindowData.MonitorResolution.x = EditorPrefs.GetInt("Meta2WindowData.MonitorResolution.x", (int)Meta2WindowData.MonitorResolution.x);
        Meta2WindowData.MonitorResolution.y = EditorPrefs.GetInt("Meta2WindowData.MonitorResolution.y", (int)Meta2WindowData.MonitorResolution.y);
        Meta2WindowData.MonitorOffset.x = EditorPrefs.GetInt("Meta2WindowData.MonitorOffset.x", (int)Meta2WindowData.MonitorOffset.x);
        Meta2WindowData.MonitorOffset.y = EditorPrefs.GetInt("Meta2WindowData.MonitorOffset.y", (int)Meta2WindowData.MonitorOffset.y);
    }

    private void WritePlayerPrefs()
    {
        EditorPrefs.SetInt("Meta2WindowData.MonitorResolution.x", (int)Meta2WindowData.MonitorResolution.x);
        EditorPrefs.SetInt("Meta2WindowData.MonitorResolution.y", (int)Meta2WindowData.MonitorResolution.y);
        EditorPrefs.SetInt("Meta2WindowData.MonitorOffset.x", (int)Meta2WindowData.MonitorOffset.x);
        EditorPrefs.SetInt("Meta2WindowData.MonitorOffset.y", (int)Meta2WindowData.MonitorOffset.y);
    }

    /// <summary>
    /// Returns a private Property from a given Object. Uses Reflection.
    /// Throws a ArgumentOutOfRangeException if the Property is not found.
    /// </summary>
    /// <param name="obj">Object from where the Property Value is returned</param>
    /// <param name="propName">Propertyname as string.</param>
    /// <returns>FieldInfo</returns>
    private static FieldInfo GetPrivateField(object obj, string propName)
    {
        if (obj == null) throw new ArgumentNullException("obj");
        Type t = obj.GetType();
        FieldInfo fi = null;
        while (fi == null && t != null)
        {
            fi = t.GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            t = t.BaseType;
        }
        if (fi == null) throw new ArgumentOutOfRangeException("propName", string.Format("Field {0} was not found in Type {1}", propName, obj.GetType().FullName));
        return fi;
    }
}
