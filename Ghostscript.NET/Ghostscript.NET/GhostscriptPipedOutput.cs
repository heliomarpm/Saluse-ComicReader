﻿//
// GhostscriptPipedOutput.cs
// This file is part of Ghostscript.NET library
//
// Author: Josip Habjan (habjan@gmail.com, http://www.linkedin.com/in/habjan) 
// Copyright (c) 2013-2014 by Josip Habjan. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace Ghostscript.NET
{
    public class GhostscriptPipedOutput : IDisposable
    {
        #region Private variables

        private bool _disposed = false;
        private AnonymousPipeServerStream _pipe;
        private Thread _thread = null;
        private MemoryStream _data = new MemoryStream();

        #endregion

        #region Constructor

        public GhostscriptPipedOutput()
        {
            _pipe = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
            _thread = new Thread(new System.Threading.ParameterizedThreadStart(ReadGhostscriptPipeOutput));
            _thread.Start();
        }

        #endregion

        #region Destructor

        ~GhostscriptPipedOutput()
        {
            Dispose(false);
        }

        #endregion

        #region Dispose

        #region Dispose

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Dispose - disposing

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_pipe != null)
                    {
                        if (_pipe.IsConnected)
                        {
                            _pipe.DisposeLocalCopyOfClientHandle();
                        }

                        _pipe.Dispose(); _pipe = null;
                    }

                    if (_thread != null)
                    {
                        if (_thread.ThreadState == ThreadState.Running)
                        {
                            _thread.Abort();
                        }

                        _thread = null;
                    }
                }

                _disposed = true;
            }
        }

        #endregion

        #endregion

        #region ClientHandle

        public string ClientHandle
        {
            get { return _pipe.GetClientHandleAsString(); }
        }

        #endregion

        #region ReadGhostscriptPipeOutput

        public void ReadGhostscriptPipeOutput(object obj)
        {
            BinaryReader reader = new BinaryReader(_pipe);

            int readCount = 0;

            byte[] buffer = new byte[_pipe.InBufferSize];

            while ((readCount = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                _data.Write(buffer, 0, readCount);
            }
        }

        #endregion

        #region Data

        public byte[] Data
        {
            get
            {
                _thread.Join();
                return _data.ToArray();
            }
        }

        #endregion
    }

}