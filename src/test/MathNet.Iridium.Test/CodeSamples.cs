//-----------------------------------------------------------------------
// <copyright file="CodeSamples.cs" company="Math.NET Project">
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

namespace Iridium.Test
{
    using MathNet.Numerics;
    using MathNet.Numerics.LinearAlgebra;

    [TestFixture]
    public class CodeSamples
    {
        [Test]
        public void CodeSample_Combinatorics_Permutation()
        {
            int[] numbers = new int[] { 1, 2, 3, 4, 5 };
            int count = numbers.Length;

            Assert.That(Combinatorics.Permutations(count), NumericIs.AlmostEqualTo(120.0), "perm(5)");

            int[] permutation = new int[count];
            Combinatorics.RandomShuffle(numbers, permutation);
        }

        [Test]
        public void CodeSample_LinearAlgebra_Eigen()
        {
            Matrix m = new Matrix(new double[][] {
                new double[] { 10.0, -18.0 },
                new double[] { 6.0, -11.0 }
                });

            ComplexVector eigenValues = m.EigenValues;
            Assert.That(eigenValues[0].Real, NumericIs.AlmostEqualTo(1.0), "Re{eigenvalueA}");
            Assert.That(eigenValues[0].Imag, NumericIs.AlmostEqualTo(0.0), "Im{eigenvalueA}");
            Assert.That(eigenValues[1].Real, NumericIs.AlmostEqualTo(-2.0), "Re{eigenvalueB}");
            Assert.That(eigenValues[1].Imag, NumericIs.AlmostEqualTo(0.0), "Im{eigenvalueB}");

            Matrix eigenVectors = m.EigenVectors;
            Assert.That(eigenVectors[0, 0], NumericIs.AlmostEqualTo(.8944271910, 1e-9), "eigenvectorA[0]");
            Assert.That(eigenVectors[1, 0], NumericIs.AlmostEqualTo(.4472135955, 1e-9), "eigenvectorA[1]");
            Assert.That(eigenVectors[0, 1], NumericIs.AlmostEqualTo(6.708203936, 1e-9), "eigenvectorB[0]");
            Assert.That(eigenVectors[1, 1], NumericIs.AlmostEqualTo(4.472135956, 1e-9), "eigenvectorB[1]");
        }

        [Test]
        public void CodeSample_PolynomialRegression()
        {
            double[] x = new double[] { 1000, 2000, 3000, 4000, 5000, 6000, 7000 };
            double[] y = new double[] { -30, -60, -88, -123, -197, -209, -266 };
            int polynomialOrder = 3;

            // Build the matrix for the least-squares fitting
            double[][] m = Matrix.CreateMatrixData(x.Length, polynomialOrder + 1);
            for(int i = 0; i < x.Length; i++)
            {
                double xi = x[i];
                double[] xrow = m[i];
                xrow[0] = 1d;
                for(int j = 1; j < xrow.Length; j++)
                {
                    xrow[j] = xrow[j - 1] * xi;
                }
            }

            // Find the least-squares solution
            Matrix matrix = new Matrix(m);
            Vector solution = matrix.Solve(y);

            // Extract the values (in our case into a polynomial for fast evaluation)
            Polynomial polynomial = new Polynomial(solution);

            // Verify that the polynomial fits with less than 10% error for all given value pairs.
            for(int i = 0; i < x.Length; i++)
            {
                Assert.That(polynomial.Evaluate(x[i]), NumericIs.AlmostEqualTo(y[i], 0.1), i.ToString());
            }
        }
    }
}
