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
namespace Meta.Internal.Playback
{
    /// <summary>
    /// Interface for data playback classes.
    /// Provides controls for playback functionality.
    /// </summary>
    /// <typeparam name="T">The type of frame data.</typeparam>
    internal interface IPlaybackSource<T>
    {

        /// <summary>
        /// Whether playback of the loaded recorded has been completed.
        /// </summary>
        /// <returns></returns>
        bool IsFinished();

        /// <summary>
        /// Whether the source for this playback source is valid.
        /// </summary>
        /// <returns></returns>
        bool HasValidSource();

        /// <summary>
        /// Loads valid frame files from the directory into memory for playback.
        /// </summary>
        /// 
        void LoadFrameFiles();

        /// <summary>
        /// Gets the total number of loaded frames.
        /// </summary>
        /// <returns>The number of loaded frames.</returns>
        int GetTotalFrameCount();

        /// <summary>
        /// Gets the index of the current frame in the playback queue.
        /// </summary>
        /// <returns>The index of the current frame in the playback playlist.</returns>
        int GetCurrentFrameIndex();

        /// <summary>
        /// Indicates if the directory has been fully loaded and is ready for playback.
        /// </summary>
        /// <returns>True, if all files are parsed and loaded into memory. Else, false.</returns>
        bool AreFramesLoaded();

        /// <summary>
        /// Retrieves the current frame being played back.
        /// </summary>
        /// <returns></returns>
        T CurrentFrame { get; }

        /// <summary>
        /// Retrieves the next frame for playback.
        /// </summary>
        /// <returns>The frame object following the current frame index.</returns>
        T NextFrame();

        /// <summary>
        /// Retrieves the previous frame for playback.
        /// </summary>
        /// <returns>The frame object type preceding the current frame index.</returns>
        T PreviousFrame();

        /// <summary>
        /// Checks if there is another frame to be played after the current frame.
        /// </summary>
        /// <returns>True, if another frame exists after the index of the current frame. Else, false.</returns>
        bool HasNextFrame();

        /// <summary>
        /// Checks if there is another frame before the current frame.
        /// </summary>
        /// <returns>True, if another frame exists before the index of the current frame. Else, false.</returns>
        bool HasPrevFrame();

        /// <summary>
        /// Returns the path of the playback directory.
        /// </summary>
        /// <returns>String of the path being used for playback.</returns>
        string GetPlaybackSourcePath();

        /// <summary>
        /// Resets the playback to the first frame of the loaded playback session.
        /// </summary>
        void Reset();

        /// <summary>
        /// Clears any data currently stored about a loaded recording.
        /// </summary>
        void Clear();

        /// <summary>
        /// Changes the source for this playback source.
        /// </summary>
        void UseNewPlaybackSourcePath(string path, string extension);
    }
}
