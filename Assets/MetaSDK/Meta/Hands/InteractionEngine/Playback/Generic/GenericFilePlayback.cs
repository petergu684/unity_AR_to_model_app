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
using System.IO;
using System.Collections.Generic;

namespace Meta.Internal.Playback
{
    /// <summary>
    /// Handles generic playback functionality for frames in separate files. 
    /// Must be extended for format-specific playback.
    /// </summary>
    /// <typeparam name="T">The type of object to be queued for playback.</typeparam>
    internal class GenericFilePlayback<T> : IPlaybackSource<T>
    {
        protected string _filepath;
        protected List<T> _frames;
        protected T _currFrame;
        protected IFileParser<T> _parser;
        protected int _totalFrames = 0;
        protected int _framesSeen = -1;
        private bool _hasValidSource = false;

        public T CurrentFrame
        {
            get { return _currFrame; }
        }

        public GenericFilePlayback()
        {
            _frames = new List<T>();
        }

        /// <summary>
        /// Creates a new playback object which reads from the given source.
        /// </summary>
        /// <param name="filepath">The path to the playback file.</param>
        public GenericFilePlayback(string filepath) 
        {
            _filepath = filepath;
            if (!ValidRecordedFile(_filepath))
            {
                throw new FileNotFoundException("The playback filepath was not found");
            }
            _hasValidSource = true;
            _frames = new List<T>();
        }

        /// <summary>
        /// Creates a new playback object which reads from the given source.
        /// </summary>
        /// <param name="filepath">The path to the playback directory.</param>
        /// <param name="parser">A predefined parser to be used for file reading.</param>
        public GenericFilePlayback(string filepath, IFileParser<T> parser) : this(filepath)
        {
            _parser = parser;
        }

        #region Playback storage

        /// <summary>
        /// Finds files of the specified format in the playback directory and processes them for playback.
        /// </summary>
        /// <param name="ext"></param>
        public virtual void LoadFrameFiles()
        {
            FileInfo file = new FileInfo(_filepath);
            ProcessFile(file, 0);
            UnityEngine.Debug.Log("Loaded " + _totalFrames + " frames from file.");
        }

        /// <summary>
        /// Reads the file and queues data for this frame using the connected parser.
        /// </summary>
        /// <param name="f">The file to be parsed.</param>
        /// <param name="id">The id for this playback data item.</param>
        protected virtual void ProcessFile(FileInfo f, int id)
        {
            try
            {
                _parser.ParseFileIntoList(f, id, ref _frames);
                _totalFrames = _frames.Count;
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.Log(e.Message);
            }
        }

        /// <summary>
        /// Checks if the path to the frame data is a valid file.
        /// </summary>
        /// <param name="sourcePath">The path of the file to be loaded.</param>
        /// <returns></returns>
        protected bool ValidRecordedFile(string sourcePath)
        {
            return File.Exists(sourcePath);
        }

        #endregion

        #region Playback Controls

        public bool HasValidSource()
        {
            return _hasValidSource;
        }

        public int GetTotalFrameCount()
        {
            return _totalFrames;
        }

        public int GetCurrentFrameIndex()
        {
            return _framesSeen;
        }

        public virtual bool AreFramesLoaded()
        {
            return (_frames.Count != 0) && (_frames.Count == _totalFrames);
        }   

        public virtual bool IsFinished()
        {
            return (_framesSeen + 1 == _totalFrames);
        }

        public virtual bool HasNextFrame()
        {
            return (_framesSeen >= -1 && _framesSeen + 1 < _frames.Count);
        }

        public virtual bool HasPrevFrame()
        {
            return (_framesSeen > 0 && _framesSeen <= _frames.Count);
        }

        public virtual T NextFrame()
        {
            _framesSeen++;
            _currFrame = _frames[_framesSeen];
            return _currFrame;
        }

        public virtual T PreviousFrame()
        {
            _framesSeen--;
            _currFrame = _frames[_framesSeen];
            return _currFrame;
        }

        public string GetPlaybackSourcePath()
        {
            return _filepath;
        }

        public void Reset()
        {
            if (_frames.Count > 0)
            {
                _framesSeen = -1;
                _currFrame = _frames[0];
            }
        }

        public void Clear()
        {
            _frames.Clear();
            _framesSeen = -1;
            _totalFrames = 0;
            _hasValidSource = false;
            _filepath = "";
        }

        public void UseNewPlaybackSourcePath(string filepath, string extension)
        {
            _filepath = filepath;
            if (!ValidRecordedFile(_filepath))
            {
                throw new FileNotFoundException("The playback filepath was not found");
            }
            _frames.Clear();
            _currFrame = default(T);
            _framesSeen = -1;
            LoadFrameFiles();
            _hasValidSource = true;
        }

        #endregion
    }
}
