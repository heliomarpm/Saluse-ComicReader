//
// GhostscriptRectangle.cs
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

namespace Ghostscript.NET
{
    public class GhostscriptRectangle
    {

        #region Private values

        private float _llx;
        private float _lly;
        private float _urx;
        private float _ury;

        #endregion

        #region Static variables

        public static GhostscriptRectangle Empty = new GhostscriptRectangle();

        #endregion

        #region Constructor

        public GhostscriptRectangle() { }

        #endregion

        #region Constructor - llx, lly, urx, ury

        public GhostscriptRectangle(float llx, float lly, float urx, float ury)
        {
            _llx = llx;
            _lly = lly;
            _urx = urx;
            _ury = ury;
        }

        #endregion

        #region llx

        public float llx
        {
            get { return _llx; }
            set { _llx = value; }
        }

        #endregion

        #region lly

        public float lly
        {
            get { return _lly; }
            set { _lly = value; }
        }

        #endregion

        #region urx

        public float urx
        {
            get { return _urx; }
            set { _urx = value; }
        }

        #endregion

        #region ury

        public float ury
        {
            get { return _ury; }
            set { _ury = value; }
        }

        #endregion

    }
}
