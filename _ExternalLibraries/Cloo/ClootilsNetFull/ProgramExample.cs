﻿#region License

/*

Copyright (c) 2009 - 2013 Fatjon Sakiqi

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.

*/

#endregion

using Cloo;
using Cloo.Bindings;
using System;
using System.IO;

namespace Clootils
{
    class ProgramExample : IExample
    {
        TextWriter log;
        ComputeProgram program;
        string clSource = @"kernel void Test(int argument) { }";

        public string Name
        {
            get { return "Program building"; }
        }

        public string Description
        {
            get { return "Demonstrates how to use a callback function when building a program and retrieve its binary when finished."; }
        }

        public void Run(ComputeContext context, TextWriter log)
        {
            this.log = log;

            try
            {
                program = new ComputeProgram(context, clSource);
                program.Build(null, null, notify, IntPtr.Zero);
            }
            catch (Exception e)
            {
                log.WriteLine(e.ToString());
            }

            // cleanup program
            program.Dispose();
        }

        private void notify(CLProgramHandle programHandle, IntPtr userDataPtr)
        {
            log.WriteLine("Program build notification.");
            byte[] bytes = program.Binaries[0];
            log.WriteLine("Beginning of program binary (compiled for the 1st selected device):");
            log.WriteLine(BitConverter.ToString(bytes, 0, 24) + "...");
        }
    }
}