//-----------------------------------------------------------------------
// <copyright file="MyStopWatch.cs" company="Math.NET Project">
//    Copyright (c) 2002-2009, Christoph R�egg, Kevin Whitefoot.
//    All Right Reserved.
// </copyright>
// <author>
//    Christoph R�egg, http://christoph.ruegg.name
//    Kevin Whitefoot, kwhitefoot@hotmail.com
// </author>
// <product>
//    Math.NET Iridium, part of the Math.NET Project.
//    https://iridium.mathdotnet.com
// </product>
// <license type="opensource" name="LGPL" version="2 or later">
//    This program is free software; you can redistribute it and/or modify
//    it under the terms of the GNU Lesser General Public License as published 
//    by the Free Software Foundation; either version 2 of the License, or
//    any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public 
//    License along with this program; if not, write to the Free Software
//    Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
// </license>
//-----------------------------------------------------------------------

using System;
using System.Diagnostics;

namespace Iridium.Test
{
    public class MyStopwatch : IDisposable
    {
        public delegate void MethodToTime();

        public delegate object FunctionToTime();

        private readonly MethodToTime methodToTime;
        private int numberOfTimesToInvokeMethod;

        public MyStopwatch(MethodToTime methodToTime)
            : this(methodToTime, 1)
        {
        }

        public MyStopwatch(MethodToTime methodToTime, int numberOfTimesToInvokeMethod)
        {
            this.methodToTime = methodToTime;
            this.numberOfTimesToInvokeMethod = numberOfTimesToInvokeMethod;
        }

        public void Dispose()
        {
            TimeMethodInvocation();
        }

        private void TimeMethodInvocation()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for(int i = 0; i < numberOfTimesToInvokeMethod; i++)
            {
                methodToTime();
            }

            stopwatch.Stop();
            Console.Out.WriteLine(stopwatch.ElapsedMilliseconds);
        }

        public static void Time(MethodToTime methodToTime)
        {
            Time(methodToTime, 1);
        }

        public static void Time(MethodToTime methodToTime, int numberOfTimesToInvokeMethod)
        {
            new MyStopwatch(methodToTime, numberOfTimesToInvokeMethod).Dispose();
        }

        public static object Time(FunctionToTime functionToTime)
        {
            object result = null;
            MethodToTime m = delegate
            {
                result = functionToTime;
            };
            Time(m);
            return result;
        }
    }
}