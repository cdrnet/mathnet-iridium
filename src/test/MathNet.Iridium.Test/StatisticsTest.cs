//-----------------------------------------------------------------------
// <copyright file="StatisticsTest.cs" company="Math.NET Project">
//    Copyright (c) 2002-2009, Christoph R�egg.
//    All Right Reserved.
// </copyright>
// <author>
//    Christoph R�egg, http://christoph.ruegg.name
// </author>
// <product>
//    Math.NET Iridium, part of the Math.NET Project.
//    http://mathnet.opensourcedotnet.info
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
using System.Collections.Generic;
using System.Text;

using MathNet.Numerics.Statistics;
using MathNet.Numerics.Distributions;

using NUnit.Framework;

namespace Iridium.Test
{
    [TestFixture]
    public class StatisticsTest
    {
        [Test]
        public void TestAccumulatorNumericStability()
        {
             /* NOTE: Statistically it is possible that this test fails even
              * if everything works as expected. However, it's very unlikely to happen,
              * and even more unlikely to happen in a series. */

            Accumulator accumulator = new Accumulator();
            NormalDistribution gaussian = new NormalDistribution();

            // Test around 0, no stability issues expected
            gaussian.SetDistributionParameters(0, 1);
            for(int i = 0; i < 10000; i++)
            {
                accumulator.Add(gaussian.NextDouble());
            }

            NumericAssert.AreAlmostEqual(0, accumulator.Mean, 0.2, "Mean of (0,1)");
            NumericAssert.AreAlmostEqual(1, accumulator.Variance, 0.5, "Variance of (0,1)");

            // Test around 10^9, potential stability issues
            accumulator.Clear();
            gaussian.SetDistributionParameters(1e+9, 1);
            for(int i = 0; i < 10000; i++)
            {
                accumulator.Add(gaussian.NextDouble());
            }

            NumericAssert.AreAlmostEqual(1e+9, accumulator.Mean, 0.2, "Mean of (1e+9,1)");
            NumericAssert.AreAlmostEqual(1, accumulator.Variance, 0.5, "Variance of (1e+9,1)");
        }

        [Test]
        public void TestAccumulatorAddRemove()
        {
            Accumulator accumulator = new Accumulator();

            for(int i = 0; i <= 10; i++)
            {
                accumulator.Add(i);
            }

            NumericAssert.AreAlmostEqual(5, accumulator.Mean, "A Mean");
            NumericAssert.AreAlmostEqual(11, accumulator.Variance, "A Variance");
            NumericAssert.AreAlmostEqual(55, accumulator.Sum, "A Sum");

            accumulator.Remove(9);
            accumulator.Remove(4);

            NumericAssert.AreAlmostEqual(14d / 3, accumulator.Mean, "B Mean");
            NumericAssert.AreAlmostEqual(23d / 2, accumulator.Variance, "B Variance");
            NumericAssert.AreAlmostEqual(42, accumulator.Sum, "B Sum");

            accumulator.Add(9);
            accumulator.Add(4);

            NumericAssert.AreAlmostEqual(5, accumulator.Mean, "C Mean");
            NumericAssert.AreAlmostEqual(11, accumulator.Variance, "C Variance");
            NumericAssert.AreAlmostEqual(55, accumulator.Sum, "C Sum");
        }

        [Test]
        public void TestDescriptiveStatisticsMinMax()
        {
            double[] samples = new double[] { -1, 5, 0, -3, 10, -0.5, 4 };
            Assert.That(DescriptiveStatistics.Min(samples), Is.EqualTo(-3), "Min");
            Assert.That(DescriptiveStatistics.Max(samples), Is.EqualTo(10), "Max");
        }

        [Test]
        public void TestDescriptiveStatisticsMeanVariance()
        {
            // Test around 10^9, potential stability issues
            NormalDistribution gaussian = new NormalDistribution(1e+9, 2);

            NumericAssert.AreAlmostEqual(1e+9, DescriptiveStatistics.Mean(gaussian.EnumerateDoubles(10000)), 0.2, "Mean of (1e+9,2)");
            NumericAssert.AreAlmostEqual(4, DescriptiveStatistics.Variance(gaussian.EnumerateDoubles(10000)), 0.5, "Variance of (1e+9,2)");
            NumericAssert.AreAlmostEqual(2, DescriptiveStatistics.StandardDeviation(gaussian.EnumerateDoubles(10000)), 0.5, "StdDev of (1e+9,2)");
        }

        [Test]
        public void TestDescriptiveStatisticsOrderMedian()
        {
            // -3 -1 -0.5 0  1  4 5 6 10
            double[] samples = new double[] { -1, 5, 0, -3, 10, -0.5, 4, 1, 6 };
            Assert.That(DescriptiveStatistics.Median(samples), Is.EqualTo(1), "Median");
            Assert.That(DescriptiveStatistics.OrderStatistic(samples, 1), Is.EqualTo(-3), "Order-1");
            Assert.That(DescriptiveStatistics.OrderStatistic(samples, 3), Is.EqualTo(-0.5), "Order-3");
            Assert.That(DescriptiveStatistics.OrderStatistic(samples, 7), Is.EqualTo(5), "Order-7");
            Assert.That(DescriptiveStatistics.OrderStatistic(samples, 9), Is.EqualTo(10), "Order-9");
        }
    }
}
