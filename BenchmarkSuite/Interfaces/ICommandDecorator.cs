﻿// ***********************************************************************
// Copyright (c) 2011 Charlie Poole
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
// ***********************************************************************

using System;
using System.Collections.Generic;
using BenchmarkSuite.Framework.Internal.Commands;

namespace BenchmarkSuite.Framework.Interfaces
{
    /// <summary>
    /// ICommandDecorator is implemented by attributes and other
    /// objects able to decorate a TestCommand, usually by wrapping
    /// it with an outer command.
    /// </summary>
    public interface ICommandDecorator
    {
        /// <summary>
        /// Decorate a command, usually by wrapping it with another
        /// command, and return the decorated command.
        /// </summary>
        /// <param name="command">The command to be decorated</param>
        /// <returns>The decorated command</returns>
        TestCommand Decorate(TestCommand command);
    }
}
