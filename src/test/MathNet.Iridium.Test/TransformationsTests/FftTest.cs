//-----------------------------------------------------------------------
// <copyright file="FftTest.cs" company="Math.NET Project">
//    Copyright (c) 2002-2009, Christoph R�egg.
//    All Right Reserved.
// </copyright>
// <author>
//    Christoph R�egg, http://christoph.ruegg.name
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

using NUnit.Framework;

namespace Iridium.Test.TransformationsTests
{
    using MathNet.Numerics.Transformations;

    [TestFixture]
    public class FftTest
    {
        private ComplexFourierTransformation _cft;
        private RealFourierTransformation _rft;

        #region Compare Helpers
        static void RealTestTimeEven(double[] samples)
        {
            int len = samples.Length;
            for(int i = 1; i < samples.Length; i++)
            {
                Assert.That(samples[len - i], NumericIs.AlmostEqualTo(samples[i]), "Real Even in Time Space");
            }
        }

        static void RealTestTimeOdd(double[] samples)
        {
            int len = samples.Length;
            for(int i = 1; i < samples.Length; i++)
            {
                Assert.That(-samples[len - i], NumericIs.AlmostEqualTo(samples[i]), "Real Odd in Time Space");
            }

            Assert.That(samples[0], NumericIs.AlmostEqualTo(0.0), "Real Odd in Time Space: Periodic Continuation");
        }

        static void ComplexTestTimeEven(double[] samples)
        {
            int len = samples.Length;
            for(int i = 2; i < samples.Length / 2; i += 2)
            {
                Assert.That(samples[len - i], NumericIs.AlmostEqualTo(samples[i]), "Complex Even in Time Space: Real Part");
                Assert.That(samples[len + 1 - i], NumericIs.AlmostEqualTo(samples[i + 1]), "Complex Even in Time Space: Imaginary Part");
            }
        }

        static void ComplexTestTimeOdd(double[] samples)
        {
            int len = samples.Length;
            for(int i = 2; i < samples.Length / 2; i += 2)
            {
                Assert.That(-samples[len - i], NumericIs.AlmostEqualTo(samples[i]), "Complex Odd in Time Space: Real Part");
                Assert.That(-samples[len + 1 - i], NumericIs.AlmostEqualTo(samples[i + 1]), "Complex Odd in Time Space: Imaginary Part");
            }

            Assert.That(samples[0], NumericIs.AlmostEqualTo(0.0), "Complex Odd in Time Space: Real Part: Periodic Continuation");
            Assert.That(samples[1], NumericIs.AlmostEqualTo(0.0), "Complex Odd in Time Space: Imaginary Part: Periodic Continuation");
        }

        static void ComplexTestFreqEven(double[] samples)
        {
            int len = samples.Length;
            for(int i = 0; i < samples.Length / 2; i += 2)
            {
                Assert.That(samples[len - 2 - i], Is.EqualTo(samples[i + 2]).Within(0.00000001), "Complex Even in Frequency Space: Real Part");
                Assert.That(samples[len - 1 - i], Is.EqualTo(samples[i + 3]).Within(0.00000001), "Complex Even in Frequency Space: Imaginary Part");
            }
        }

        static void ComplexTestFreqOdd(double[] samples)
        {
            int len = samples.Length;
            for(int i = 0; i < samples.Length / 2; i += 2)
            {
                Assert.That(-samples[len - 2 - i], Is.EqualTo(samples[i + 2]).Within(0.00000001), "Complex Odd in Frequency Space: Real Part");
                Assert.That(-samples[len - 1 - i], Is.EqualTo(samples[i + 3]).Within(0.00000001), "Complex Odd in Frequency Space: Imaginary Part");
            }

            Assert.That(samples[0], NumericIs.AlmostEqualTo(0.0), "Complex Odd in Frequency Space: Real Part: Periodic Continuation (No DC)");
            Assert.That(samples[1], NumericIs.AlmostEqualTo(0.0), "Complex Odd in Frequency Space: Imaginary Part: Periodic Continuation (No DC)");
        }

        static void ComplexTestRealZero(double[] samples)
        {
            for(int i = 0; i < samples.Length; i += 2)
            {
                Assert.That(samples[i], NumericIs.AlmostEqualTo((double) 0), "Complex: Zero Real Part");
            }
        }

        static void ComplexTestImagZero(double[] samples)
        {
            for(int i = 1; i < samples.Length; i += 2)
            {
                Assert.That(samples[i], NumericIs.AlmostEqualTo((double) 0), "Complex: Zero Imaginary Part");
            }
        }
        #endregion

        [SetUp]
        public void SetUp()
        {
            _cft = new ComplexFourierTransformation();
            _rft = new RealFourierTransformation();
        }

        #region Complex FFT
        [Test]
        public void Complex_Symmetry_RealEven_RealEven()
        {
            /* h(t) real-valued even <=> H(f) real-valued even-with-offset */

            const int numSamples = 32;
            const int length = 2 * numSamples;

            double[] data = new double[length];

            for(int i = 0; i < length; i += 2)
            {
                double z = (double)(i - numSamples) / numSamples;
                data[i] = 1.0 / ((z * z) + 1.0);
                data[i + 1] = 0.0;
            }

            ComplexTestTimeEven(data);

            _cft.Convention = TransformationConvention.Matlab; // so we can check MATLAB consistency
            _cft.TransformForward(data);

            ComplexTestImagZero(data);
            ComplexTestFreqEven(data);

            /* Compare With MATLAB:
            samples_t = 1.0 ./ (([-16:1:15] ./ 16) .^ 2 + 1.0)
            samples_f = fft(samples_t)
            */

            Assert.That(data[0 * 2], NumericIs.AlmostEqualTo(25.128, 0.001), "MATLAB 1");
            Assert.That(data[1 * 2], NumericIs.AlmostEqualTo(-3.623, 0.001), "MATLAB 2");
            Assert.That(data[2 * 2], NumericIs.AlmostEqualTo(-0.31055, 0.0001), "MATLAB 3");

            Assert.That(data[6 * 2], NumericIs.AlmostEqualTo(-0.050611, 0.00001), "MATLAB 7");
            Assert.That(data[7 * 2], NumericIs.AlmostEqualTo(-0.03882, 0.00001), "MATLAB 8");
            Assert.That(data[8 * 2], NumericIs.AlmostEqualTo(-0.031248, 0.00001), "MATLAB 9");

            Assert.That(data[13 * 2], NumericIs.AlmostEqualTo(-0.017063, 0.0001), "MATLAB 14");
            Assert.That(data[14 * 2], NumericIs.AlmostEqualTo(-0.016243, 0.00001), "MATLAB 15");
            Assert.That(data[15 * 2], NumericIs.AlmostEqualTo(-0.015777, 0.0001), "MATLAB 16");
        }

        [Test]
        public void Complex_Symmetry_ImaginaryEven_ImaginaryEven()
        {
            /* h(t) imaginary-valued even <=> H(f) imaginary-valued even-with-offset */

            const int numSamples = 32;
            const int length = 2 * numSamples;

            double[] data = new double[length];

            for(int i = 0; i < length; i += 2)
            {
                double z = (double)(i - numSamples) / numSamples;
                data[i] = 0.0;
                data[i + 1] = 1.0 / ((z * z) + 1.0);
            }

            ComplexTestTimeEven(data);

            _cft.Convention = TransformationConvention.Matlab;
            _cft.TransformForward(data);

            ComplexTestRealZero(data);
            ComplexTestFreqEven(data);

            /* Compare With MATLAB:
            samples_t = 1.0i ./ (([-16:1:15] ./ 16) .^ 2 + 1.0)
            samples_f = fft(samples_t)
            */

            Assert.That(data[(0 * 2) + 1], NumericIs.AlmostEqualTo(25.128, 0.001), "MATLAB 1");
            Assert.That(data[(1 * 2) + 1], NumericIs.AlmostEqualTo(-3.623, 0.001), "MATLAB 2");
            Assert.That(data[(2 * 2) + 1], NumericIs.AlmostEqualTo(-0.31055, 0.0001), "MATLAB 3");

            Assert.That(data[(6 * 2) + 1], NumericIs.AlmostEqualTo(-0.050611, 0.00001), "MATLAB 7");
            Assert.That(data[(7 * 2) + 1], NumericIs.AlmostEqualTo(-0.03882, 0.00001), "MATLAB 8");
            Assert.That(data[(8 * 2) + 1], NumericIs.AlmostEqualTo(-0.031248, 0.00001), "MATLAB 9");

            Assert.That(data[(13 * 2) + 1], NumericIs.AlmostEqualTo(-0.017063, 0.0001), "MATLAB 14");
            Assert.That(data[(14 * 2) + 1], NumericIs.AlmostEqualTo(-0.016243, 0.00001), "MATLAB 15");
            Assert.That(data[(15 * 2) + 1], NumericIs.AlmostEqualTo(-0.015777, 0.0001), "MATLAB 16");
        }

        [Test]
        public void Complex_Symmetry_RealOdd_ImaginaryEven()
        {
            /* h(t) real-valued odd <=> H(f) imaginary-valued odd-with-offset */

            const int numSamples = 32;
            const int length = 2 * numSamples;

            double[] data = new double[length];

            for(int i = 0; i < length; i += 2)
            {
                double z = (double)(i - numSamples) / numSamples;
                data[i] = z / ((z * z) + 1.0);
                data[i + 1] = 0.0;
            }

            data[0] = 0.0; // periodic continuation; force odd

            ComplexTestTimeOdd(data);

            _cft.Convention = TransformationConvention.Matlab;
            _cft.TransformForward(data);

            ComplexTestRealZero(data);
            ComplexTestFreqOdd(data);

            /* Compare With MATLAB:
            samples_t = ([-16:1:15] ./ 16) ./ (([-16:1:15] ./ 16) .^ 2 + 1.0)
            samples_t(1) = 0
            samples_f = fft(samples_t)
            */

            Assert.That(data[(0 * 2) + 1], NumericIs.AlmostEqualTo((double) 0, 0.001), "MATLAB 1");
            Assert.That(data[(1 * 2) + 1], NumericIs.AlmostEqualTo(7.4953, 0.0001), "MATLAB 2");
            Assert.That(data[(2 * 2) + 1], NumericIs.AlmostEqualTo(2.4733, 0.0001), "MATLAB 3");

            Assert.That(data[(6 * 2) + 1], NumericIs.AlmostEqualTo(0.75063, 0.00001), "MATLAB 7");
            Assert.That(data[(7 * 2) + 1], NumericIs.AlmostEqualTo(0.61071, 0.00001), "MATLAB 8");
            Assert.That(data[(8 * 2) + 1], NumericIs.AlmostEqualTo(0.50097, 0.00001), "MATLAB 9");

            Assert.That(data[(13 * 2) + 1], NumericIs.AlmostEqualTo(0.15183, 0.0001), "MATLAB 14");
            Assert.That(data[(14 * 2) + 1], NumericIs.AlmostEqualTo(0.099557, 0.00001), "MATLAB 15");
            Assert.That(data[(15 * 2) + 1], NumericIs.AlmostEqualTo(0.049294, 0.00001), "MATLAB 16");
        }

        [Test]
        public void Complex_Symmetry_ImaginaryOdd_RealEven()
        {
            /* h(t) imaginary-valued odd <=> H(f) real-valued odd-with-offset */

            const int numSamples = 32;
            const int length = 2 * numSamples;

            double[] data = new double[length];

            for(int i = 0; i < length; i += 2)
            {
                double z = (double)(i - numSamples) / numSamples;
                data[i] = 0.0;
                data[i + 1] = z / ((z * z) + 1.0);
            }

            data[1] = 0.0; // periodic continuation; force odd

            ComplexTestTimeOdd(data);

            _cft.Convention = TransformationConvention.Matlab;
            _cft.TransformForward(data);

            ComplexTestImagZero(data);
            ComplexTestFreqOdd(data);

            /* Compare With MATLAB:
            samples_t = 1.0i * ([-16:1:15] ./ 16) ./ (([-16:1:15] ./ 16) .^ 2 + 1.0)
            samples_t(1) = 0
            samples_f = fft(samples_t)
            */

            Assert.That(data[0 * 2], NumericIs.AlmostEqualTo((double) 0, 0.001), "MATLAB 1");
            Assert.That(data[1 * 2], NumericIs.AlmostEqualTo(-7.4953, 0.0001), "MATLAB 2");
            Assert.That(data[2 * 2], NumericIs.AlmostEqualTo(-2.4733, 0.0001), "MATLAB 3");

            Assert.That(data[6 * 2], NumericIs.AlmostEqualTo(-0.75063, 0.00001), "MATLAB 7");
            Assert.That(data[7 * 2], NumericIs.AlmostEqualTo(-0.61071, 0.00001), "MATLAB 8");
            Assert.That(data[8 * 2], NumericIs.AlmostEqualTo(-0.50097, 0.00001), "MATLAB 9");

            Assert.That(data[13 * 2], NumericIs.AlmostEqualTo(-0.15183, 0.0001), "MATLAB 14");
            Assert.That(data[14 * 2], NumericIs.AlmostEqualTo(-0.099557, 0.00001), "MATLAB 15");
            Assert.That(data[15 * 2], NumericIs.AlmostEqualTo(-0.049294, 0.00001), "MATLAB 16");
        }

        [Test]
        public void Complex_Inverse_Mix()
        {
            const int numSamples = 32;
            const int length = 2 * numSamples;

            double[] data = new double[length];

            for(int i = 0; i < length; i += 2)
            {
                double z = (double)(i - numSamples) / numSamples;
                data[i] = 1.0 / ((z * z) + 1.0);
                data[i + 1] = z / ((z * z) + 1.0);
            }

            data[1] = 0.0; // periodic continuation; force odd

            _cft.Convention = TransformationConvention.Matlab;
            _cft.TransformForward(data);

            ComplexTestImagZero(data);

            /* Compare With MATLAB:
            samples_t = 1.0i * ([-16:1:15] ./ 16) ./ (([-16:1:15] ./ 16) .^ 2 + 1.0)
                                         + 1.0 ./ (([-16:1:15] ./ 16) .^ 2 + 1.0)
            samples_t(1) = real(samples_t(1))
            samples_f = fft(samples_t)
            */

            Assert.That(data[0 * 2], NumericIs.AlmostEqualTo(25.128, 0.001), "MATLAB 1");
            Assert.That(data[1 * 2], NumericIs.AlmostEqualTo(-11.118, 0.001), "MATLAB 2");
            Assert.That(data[2 * 2], NumericIs.AlmostEqualTo(-2.7838, 0.0001), "MATLAB 3");

            Assert.That(data[6 * 2], NumericIs.AlmostEqualTo(-0.80124, 0.00001), "MATLAB 7");
            Assert.That(data[7 * 2], NumericIs.AlmostEqualTo(-0.64953, 0.00001), "MATLAB 8");
            Assert.That(data[8 * 2], NumericIs.AlmostEqualTo(-0.53221, 0.00001), "MATLAB 9");

            Assert.That(data[13 * 2], NumericIs.AlmostEqualTo(-0.1689, 0.0001), "MATLAB 14");
            Assert.That(data[14 * 2], NumericIs.AlmostEqualTo(-0.1158, 0.0001), "MATLAB 15");
            Assert.That(data[15 * 2], NumericIs.AlmostEqualTo(-0.065071, 0.00001), "MATLAB 16");

            Assert.That(data[20 * 2], NumericIs.AlmostEqualTo(0.18904, 0.0001), "MATLAB 21");
            Assert.That(data[21 * 2], NumericIs.AlmostEqualTo(0.2475, 0.0001), "MATLAB 22");
            Assert.That(data[22 * 2], NumericIs.AlmostEqualTo(0.31196, 0.00001), "MATLAB 23");

            Assert.That(data[29 * 2], NumericIs.AlmostEqualTo(1.4812, 0.0001), "MATLAB 30");
            Assert.That(data[30 * 2], NumericIs.AlmostEqualTo(2.1627, 0.0001), "MATLAB 31");
            Assert.That(data[31 * 2], NumericIs.AlmostEqualTo(3.8723, 0.0001), "MATLAB 32");

            _cft.TransformBackward(data);

            // Compare with original samples
            for(int i = 0; i < length; i += 2)
            {
                double z = (double)(i - numSamples) / numSamples;
                Assert.That(data[i], NumericIs.AlmostEqualTo(1.0 / ((z * z) + 1.0), 0.00001), "Inv: Real: " + i);
                Assert.That(data[i + 1], NumericIs.AlmostEqualTo(i == 0 ? 0.0 : z / ((z * z) + 1.0), 0.00001), "Inv: Imag: " + i);
            }
        }
        #endregion

        #region Real FFT
        [Test]
        public void Real_TwoReal_EvenOdd()
        {
            const int numSamples = 32;
            const int half = numSamples >> 1;

            double[] dataEven = new double[numSamples];
            double[] dataOdd = new double[numSamples];

            for(int i = 0; i < numSamples; i++)
            {
                double z = (double)(i - half) / half;
                dataEven[i] = 1.0 / ((z * z) + 1.0);
                dataOdd[i] = z / ((z * z) + 1.0);
            }

            dataOdd[0] = 0.0; // periodic continuation; force odd

            RealTestTimeEven(dataEven);
            RealTestTimeOdd(dataOdd);

            double[] evenReal, evenImag, oddReal, oddImag;

            _rft.Convention = TransformationConvention.Matlab;
            _rft.TransformForward(dataEven, dataOdd, out evenReal, out evenImag, out oddReal, out oddImag);

            /* Compare EVEN With MATLAB:
            samples_t = 1.0 ./ (([-16:1:15] ./ 16) .^ 2 + 1.0)
            samples_f = fft(samples_t)
            */

            Assert.That(evenReal[0], NumericIs.AlmostEqualTo(25.128, 0.001), "MATLAB 1 (even)");
            Assert.That(evenReal[1], NumericIs.AlmostEqualTo(-3.623, 0.001), "MATLAB 2 (even)");
            Assert.That(evenReal[2], NumericIs.AlmostEqualTo(-0.31055, 0.0001), "MATLAB 3 (even)");

            Assert.That(evenReal[6], NumericIs.AlmostEqualTo(-0.050611, 0.00001), "MATLAB 7 (even)");
            Assert.That(evenReal[7], NumericIs.AlmostEqualTo(-0.03882, 0.00001), "MATLAB 8 (even)");
            Assert.That(evenReal[8], NumericIs.AlmostEqualTo(-0.031248, 0.00001), "MATLAB 9 (even)");

            Assert.That(evenReal[13], NumericIs.AlmostEqualTo(-0.017063, 0.0001), "MATLAB 14 (even)");
            Assert.That(evenReal[14], NumericIs.AlmostEqualTo(-0.016243, 0.00001), "MATLAB 15 (even)");
            Assert.That(evenReal[15], NumericIs.AlmostEqualTo(-0.015777, 0.0001), "MATLAB 16 (even)");

            Assert.That(evenImag[1], NumericIs.AlmostEqualTo((double) 0, 0.001), "MATLAB 2i (even)");
            Assert.That(evenImag[7], NumericIs.AlmostEqualTo((double) 0, 0.001), "MATLAB 8i (even)");
            Assert.That(evenImag[14], NumericIs.AlmostEqualTo((double) 0, 0.001), "MATLAB 15i (even)");

            /* Compare ODD With MATLAB:
            samples_t = ([-16:1:15] ./ 16) ./ (([-16:1:15] ./ 16) .^ 2 + 1.0)
            samples_t(1) = 0
            samples_f = fft(samples_t)
            */

            Assert.That(oddImag[0], NumericIs.AlmostEqualTo((double) 0, 0.001), "MATLAB 1 (odd)");
            Assert.That(oddImag[1], NumericIs.AlmostEqualTo(7.4953, 0.0001), "MATLAB 2 (odd)");
            Assert.That(oddImag[2], NumericIs.AlmostEqualTo(2.4733, 0.0001), "MATLAB 3 (odd)");

            Assert.That(oddImag[6], NumericIs.AlmostEqualTo(0.75063, 0.00001), "MATLAB 7 (odd)");
            Assert.That(oddImag[7], NumericIs.AlmostEqualTo(0.61071, 0.00001), "MATLAB 8 (odd)");
            Assert.That(oddImag[8], NumericIs.AlmostEqualTo(0.50097, 0.00001), "MATLAB 9 (odd)");

            Assert.That(oddImag[13], NumericIs.AlmostEqualTo(0.15183, 0.0001), "MATLAB 14 (odd)");
            Assert.That(oddImag[14], NumericIs.AlmostEqualTo(0.099557, 0.00001), "MATLAB 15 (odd)");
            Assert.That(oddImag[15], NumericIs.AlmostEqualTo(0.049294, 0.00001), "MATLAB 16 (odd)");

            Assert.That(oddReal[1], NumericIs.AlmostEqualTo((double) 0, 0.001), "MATLAB 2r (odd)");
            Assert.That(oddReal[7], NumericIs.AlmostEqualTo((double) 0, 0.001), "MATLAB 8r (odd)");
            Assert.That(oddReal[14], NumericIs.AlmostEqualTo((double) 0, 0.001), "MATLAB 15r (odd)");
        }

        [Test]
        public void Real_TwoReal_Inverse()
        {
            const int numSamples = 32;
            const int half = numSamples >> 1;

            double[] dataEven = new double[numSamples];
            double[] dataOdd = new double[numSamples];

            for(int i = 0; i < numSamples; i++)
            {
                double z = (double)(i - half) / half;
                dataEven[i] = 1.0 / ((z * z) + 1.0);
                dataOdd[i] = z / ((z * z) + 1.0);
            }

            dataOdd[0] = 0.0; // periodic continuation; force odd

            RealTestTimeEven(dataEven);
            RealTestTimeOdd(dataOdd);

            double[] evenReal, evenImag, oddReal, oddImag;

            // Forward Transform
            _rft.Convention = TransformationConvention.Default;
            _rft.TransformForward(dataEven, dataOdd, out evenReal, out evenImag, out oddReal, out oddImag);

            // Backward Transform
            double[] dataEven2, dataOdd2;
            _rft.TransformBackward(evenReal, evenImag, oddReal, oddImag, out dataEven2, out dataOdd2);

            // Compare with original samples
            for(int i = 0; i < numSamples; i += 2)
            {
                Assert.That(dataEven[i], Is.EqualTo(dataEven2[i]).Within(0.00001), "Inv: Even: " + i);
                Assert.That(dataOdd[i], Is.EqualTo(dataOdd2[i]).Within(0.00001), "Inv: Odd: " + i);
            }
        }

        [Test]
        public void Real_SingleReal_EvenOdd()
        {
            const int numSamples = 32;
            const int half = numSamples >> 1;

            double[] dataEven = new double[numSamples];
            double[] dataOdd = new double[numSamples];

            for(int i = 0; i < numSamples; i++)
            {
                double z = (double)(i - half) / half;
                dataEven[i] = 1.0 / ((z * z) + 1.0);
                dataOdd[i] = z / ((z * z) + 1.0);
            }

            dataOdd[0] = 0.0; // periodic continuation; force odd

            RealTestTimeEven(dataEven);
            RealTestTimeOdd(dataOdd);

            double[] evenReal, evenImag, oddReal, oddImag;

            _rft.Convention = TransformationConvention.Matlab;
            _rft.TransformForward(dataEven, out evenReal, out evenImag);
            _rft.TransformForward(dataOdd, out oddReal, out oddImag);

            /* Compare EVEN With MATLAB:
            samples_t = 1.0 ./ (([-16:1:15] ./ 16) .^ 2 + 1.0)
            samples_f = fft(samples_t)
            */

            Assert.That(evenReal[0], NumericIs.AlmostEqualTo(25.128, 0.001), "MATLAB 1 (even)");
            Assert.That(evenReal[1], NumericIs.AlmostEqualTo(-3.623, 0.001), "MATLAB 2 (even)");
            Assert.That(evenReal[2], NumericIs.AlmostEqualTo(-0.31055, 0.0001), "MATLAB 3 (even)");

            Assert.That(evenReal[6], NumericIs.AlmostEqualTo(-0.050611, 0.00001), "MATLAB 7 (even)");
            Assert.That(evenReal[7], NumericIs.AlmostEqualTo(-0.03882, 0.00001), "MATLAB 8 (even)");
            Assert.That(evenReal[8], NumericIs.AlmostEqualTo(-0.031248, 0.00001), "MATLAB 9 (even)");

            Assert.That(evenReal[13], NumericIs.AlmostEqualTo(-0.017063, 0.0001), "MATLAB 14 (even)");
            Assert.That(evenReal[14], NumericIs.AlmostEqualTo(-0.016243, 0.00001), "MATLAB 15 (even)");
            Assert.That(evenReal[15], NumericIs.AlmostEqualTo(-0.015777, 0.0001), "MATLAB 16 (even)");

            Assert.That(evenImag[1], NumericIs.AlmostEqualTo((double) 0, 0.001), "MATLAB 2i (even)");
            Assert.That(evenImag[7], NumericIs.AlmostEqualTo((double) 0, 0.001), "MATLAB 8i (even)");
            Assert.That(evenImag[14], NumericIs.AlmostEqualTo((double) 0, 0.001), "MATLAB 15i (even)");

            /* Compare ODD With MATLAB:
            samples_t = ([-16:1:15] ./ 16) ./ (([-16:1:15] ./ 16) .^ 2 + 1.0)
            samples_t(1) = 0
            samples_f = fft(samples_t)
            */

            Assert.That(oddImag[0], NumericIs.AlmostEqualTo((double) 0, 0.001), "MATLAB 1 (odd)");
            Assert.That(oddImag[1], NumericIs.AlmostEqualTo(7.4953, 0.0001), "MATLAB 2 (odd)");
            Assert.That(oddImag[2], NumericIs.AlmostEqualTo(2.4733, 0.0001), "MATLAB 3 (odd)");

            Assert.That(oddImag[6], NumericIs.AlmostEqualTo(0.75063, 0.00001), "MATLAB 7 (odd)");
            Assert.That(oddImag[7], NumericIs.AlmostEqualTo(0.61071, 0.00001), "MATLAB 8 (odd)");
            Assert.That(oddImag[8], NumericIs.AlmostEqualTo(0.50097, 0.00001), "MATLAB 9 (odd)");

            Assert.That(oddImag[13], NumericIs.AlmostEqualTo(0.15183, 0.0001), "MATLAB 14 (odd)");
            Assert.That(oddImag[14], NumericIs.AlmostEqualTo(0.099557, 0.00001), "MATLAB 15 (odd)");
            Assert.That(oddImag[15], NumericIs.AlmostEqualTo(0.049294, 0.00001), "MATLAB 16 (odd)");

            Assert.That(oddReal[1], NumericIs.AlmostEqualTo((double) 0, 0.001), "MATLAB 2r (odd)");
            Assert.That(oddReal[7], NumericIs.AlmostEqualTo((double) 0, 0.001), "MATLAB 8r (odd)");
            Assert.That(oddReal[14], NumericIs.AlmostEqualTo((double) 0, 0.001), "MATLAB 15r (odd)");
        }

        [Test]
        public void Real_SingleReal_Inverse()
        {
            const int numSamples = 32;
            const int half = numSamples >> 1;

            double[] dataEven = new double[numSamples];
            double[] dataOdd = new double[numSamples];

            for(int i = 0; i < numSamples; i++)
            {
                double z = (double)(i - half) / half;
                dataEven[i] = 1.0 / ((z * z) + 1.0);
                dataOdd[i] = z / ((z * z) + 1.0);
            }

            dataOdd[0] = 0.0; // periodic continuation; force odd

            RealTestTimeEven(dataEven);
            RealTestTimeOdd(dataOdd);

            double[] evenReal, evenImag, oddReal, oddImag;

            // Forward Transform
            _rft.Convention = TransformationConvention.Default;
            _rft.TransformForward(dataEven, out evenReal, out evenImag);
            _rft.Convention = TransformationConvention.NumericalRecipes; // to also check this one once...
            _rft.TransformForward(dataOdd, out oddReal, out oddImag);

            // Backward Transform
            double[] dataEven2, dataOdd2;
            _rft.Convention = TransformationConvention.Default;
            _rft.TransformBackward(evenReal, evenImag, out dataEven2);
            _rft.Convention = TransformationConvention.NumericalRecipes;
            _rft.TransformBackward(oddReal, oddImag, out dataOdd2);

            // Compare with original samples
            for(int i = 0; i < numSamples; i += 2)
            {
                Assert.That(dataEven[i], Is.EqualTo(dataEven2[i]).Within(0.00001), "Inv: Even: " + i);

                // Note: Numerical Recipes applies no scaling, 
                // so we have to compensate this here by scaling with N/2
                Assert.That(dataOdd[i] * half, Is.EqualTo(dataOdd2[i]).Within(0.00001), "Inv: Odd: " + i);
            }
        }
        #endregion

        #region Complex Multi Dimensional FFT

        [Test]
        public void Complex_MultiDim_1D_Inverse_Mix()
        {
            const int numSamples = 32;
            const int length = 2 * numSamples;

            int[] dims = new int[] { numSamples };
            double[] data = new double[length];

            for(int i = 0; i < length; i += 2)
            {
                double z = (double)(i - numSamples) / numSamples;
                data[i] = 1.0 / ((z * z) + 1.0);
                data[i + 1] = z / ((z * z) + 1.0);
            }

            data[1] = 0.0; // periodic continuation; force odd

            _cft.Convention = TransformationConvention.Matlab;
            _cft.TransformForward(data, dims);

            ComplexTestImagZero(data);

            /* Compare With MATLAB:
            samples_t = 1.0i * ([-16:1:15] ./ 16) ./ (([-16:1:15] ./ 16) .^ 2 + 1.0)
                                         + 1.0 ./ (([-16:1:15] ./ 16) .^ 2 + 1.0)
            samples_t(1) = real(samples_t(1))
            samples_f = fftn(samples_t)
            */

            Assert.That(data[0 * 2], NumericIs.AlmostEqualTo(25.128, 0.001), "MATLAB 1");
            Assert.That(data[1 * 2], NumericIs.AlmostEqualTo(-11.118, 0.001), "MATLAB 2");
            Assert.That(data[2 * 2], NumericIs.AlmostEqualTo(-2.7838, 0.0001), "MATLAB 3");

            Assert.That(data[6 * 2], NumericIs.AlmostEqualTo(-0.80124, 0.00001), "MATLAB 7");
            Assert.That(data[7 * 2], NumericIs.AlmostEqualTo(-0.64953, 0.00001), "MATLAB 8");
            Assert.That(data[8 * 2], NumericIs.AlmostEqualTo(-0.53221, 0.00001), "MATLAB 9");

            Assert.That(data[13 * 2], NumericIs.AlmostEqualTo(-0.1689, 0.0001), "MATLAB 14");
            Assert.That(data[14 * 2], NumericIs.AlmostEqualTo(-0.1158, 0.0001), "MATLAB 15");
            Assert.That(data[15 * 2], NumericIs.AlmostEqualTo(-0.065071, 0.00001), "MATLAB 16");

            Assert.That(data[20 * 2], NumericIs.AlmostEqualTo(0.18904, 0.0001), "MATLAB 21");
            Assert.That(data[21 * 2], NumericIs.AlmostEqualTo(0.2475, 0.0001), "MATLAB 22");
            Assert.That(data[22 * 2], NumericIs.AlmostEqualTo(0.31196, 0.00001), "MATLAB 23");

            Assert.That(data[29 * 2], NumericIs.AlmostEqualTo(1.4812, 0.0001), "MATLAB 30");
            Assert.That(data[30 * 2], NumericIs.AlmostEqualTo(2.1627, 0.0001), "MATLAB 31");
            Assert.That(data[31 * 2], NumericIs.AlmostEqualTo(3.8723, 0.0001), "MATLAB 32");

            _cft.TransformBackward(data, dims);

            // Compare with original samples
            for(int i = 0; i < length; i += 2)
            {
                double z = (double)(i - numSamples) / numSamples;
                Assert.That(data[i], Is.EqualTo(1.0 / ((z * z) + 1.0)).Within(0.00001), "Inv: Real: " + i);
                Assert.That(data[i + 1], Is.EqualTo(i == 0 ? 0.0 : z / ((z * z) + 1.0)).Within(0.00001), "Inv: Imag: " + i);
            }
        }

        [Test]
        public void Complex_MultiDim_2D_Inverse_Mix()
        {
            const int numSamples = 4;
            const int length = 2 * numSamples;

            int[] dims = new int[] { 2, 2 };
            double[] data = new double[length];

            for(int i = 0; i < length; i += 2)
            {
                data[i] = i;
                data[i + 1] = numSamples - i;
            }

            data[1] = 0.0; // periodic continuation; force odd

            _cft.Convention = TransformationConvention.Matlab;
            _cft.TransformForward(data, dims);

            /* Compare With MATLAB:
            samples_t = [0, 2+2i;4,6-2i]
            samples_f = fftn(samples_t)
            */

            Assert.That(data[0 * 2], Is.EqualTo(12), "MATLAB 1");
            Assert.That(data[(0 * 2) + 1], Is.EqualTo(0), "MATLAB 1b");
            Assert.That(data[1 * 2], Is.EqualTo(-4), "MATLAB 2");
            Assert.That(data[(1 * 2) + 1], Is.EqualTo(0), "MATLAB 2b");
            Assert.That(data[2 * 2], Is.EqualTo(-8), "MATLAB 3");
            Assert.That(data[(2 * 2) + 1], Is.EqualTo(4), "MATLAB 3b");
            Assert.That(data[3 * 2], Is.EqualTo(0), "MATLAB 4");
            Assert.That(data[(3 * 2) + 1], Is.EqualTo(-4), "MATLAB 4b");

            _cft.TransformBackward(data, dims);

            // Compare with original samples
            for(int i = 0; i < length; i += 2)
            {
                Assert.That(data[i], Is.EqualTo(i), "Inv: Real: " + i);
                Assert.That(data[i + 1], Is.EqualTo(i == 0 ? 0.0 : numSamples - i), "Inv: Imag: " + i);
            }
        }

        [Test]
        public void Complex_MultiDim_3D_Inverse_Mix()
        {
            int[] dims = new int[] { 2, 4, 8 };
            const int ntot = 2 * 4 * 8;
            const int len = 2 * ntot;

            double[] data = new double[len];
            for(int i = 0; i < len; i += 2)
            {
                data[i] = i;
                data[i + 1] = 0.0;
            }

            _cft.Convention = TransformationConvention.Matlab;
            _cft.TransformForward(data, dims);

            /* Compare With MATLAB:
            M = zeros(2,4,8);
            for x = 0:1
                for y = 0:3
                    for z = 0:7
                        M(x+1,y+1,z+1) = 2*(4*8*x+8*y+z);
                    end
                end
            end
            H = fftn(M);

            M1 = reshape(M(1,:,:),[4,8])
            M2 = reshape(M(2,:,:),[4,8])
            H1 = reshape(H(1,:,:),[4,8])
            H2 = reshape(H(2,:,:),[4,8])
            */

            Assert.That(data[0 * 2], NumericIs.AlmostEqualTo((double) 4032), "MATLAB 1");
            Assert.That(data[(0 * 2) + 1], NumericIs.AlmostEqualTo((double) 0), "MATLAB 1b");

            Assert.That(data[1 * 2], NumericIs.AlmostEqualTo((double) (-64)), "MATLAB 2");
            Assert.That(data[(1 * 2) + 1], NumericIs.AlmostEqualTo(154.51, 1e-5), "MATLAB 2b");
            Assert.That(data[2 * 2], NumericIs.AlmostEqualTo((double) (-64)), "MATLAB 3");
            Assert.That(data[(2 * 2) + 1], NumericIs.AlmostEqualTo((double) 64), "MATLAB 3b");
            Assert.That(data[6 * 2], NumericIs.AlmostEqualTo((double) (-64)), "MATLAB 7");
            Assert.That(data[(6 * 2) + 1], NumericIs.AlmostEqualTo((double) (-64)), "MATLAB 7b");
            Assert.That(data[7 * 2], NumericIs.AlmostEqualTo((double) (-64)), "MATLAB 8");
            Assert.That(data[(7 * 2) + 1], NumericIs.AlmostEqualTo(-154.51, 1e-5), "MATLAB 8b");

            Assert.That(data[8 * 2], NumericIs.AlmostEqualTo((double) (-512)), "MATLAB 9");
            Assert.That(data[(8 * 2) + 1], NumericIs.AlmostEqualTo((double) 512), "MATLAB 9b");

            Assert.That(data[9 * 2], NumericIs.AlmostEqualTo((double) 0), "MATLAB 10");
            Assert.That(data[(9 * 2) + 1], NumericIs.AlmostEqualTo((double) 0), "MATLAB 10b");
            Assert.That(data[15 * 2], NumericIs.AlmostEqualTo((double) 0), "MATLAB 16");
            Assert.That(data[(15 * 2) + 1], NumericIs.AlmostEqualTo((double) 0), "MATLAB 16b");

            Assert.That(data[16 * 2], NumericIs.AlmostEqualTo((double) (-512)), "MATLAB 17");
            Assert.That(data[(16 * 2) + 1], NumericIs.AlmostEqualTo((double) 0), "MATLAB 17b");

            Assert.That(data[24 * 2], NumericIs.AlmostEqualTo((double) (-512)), "MATLAB 25");
            Assert.That(data[(24 * 2) + 1], NumericIs.AlmostEqualTo((double) (-512)), "MATLAB 25b");

            Assert.That(data[32 * 2], NumericIs.AlmostEqualTo((double) (-2048)), "MATLAB 33");
            Assert.That(data[(32 * 2) + 1], NumericIs.AlmostEqualTo((double) 0), "MATLAB 33b");

            Assert.That(data[33 * 2], NumericIs.AlmostEqualTo((double) 0), "MATLAB 34");
            Assert.That(data[(33 * 2) + 1], NumericIs.AlmostEqualTo((double) 0), "MATLAB 34b");
            Assert.That(data[39 * 2], NumericIs.AlmostEqualTo((double) 0), "MATLAB 40");
            Assert.That(data[(39 * 2) + 1], NumericIs.AlmostEqualTo((double) 0), "MATLAB 40b");

            Assert.That(data[56 * 2], NumericIs.AlmostEqualTo((double) 0), "MATLAB 57");
            Assert.That(data[(56 * 2) + 1], NumericIs.AlmostEqualTo((double) 0), "MATLAB 57b");

            _cft.TransformBackward(data, dims);

            // Compare with original samples
            for(int i = 0; i < len; i += 2)
            {
                Assert.That(data[i], NumericIs.AlmostEqualTo((double)i), "Inv: Real: " + i);
                Assert.That(data[i + 1], NumericIs.AlmostEqualTo(0d), "Inv: Imag: " + i);
            }
        }
        #endregion
    }
}
