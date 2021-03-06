//-----------------------------------------------------------------------
// <copyright file="DiscreteUniformDistribution.cs" company="Math.NET Project">
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
// <contribution>
//    Troschuetz.Random Class Library, Stefan Trosch�tz (stefan@troschuetz.de)
// </contribution>
//-----------------------------------------------------------------------

using System;

namespace MathNet.Numerics.Distributions
{
    using RandomSources;

    /// <summary>
    /// Provides generation of discrete uniformly distributed random numbers.
    /// </summary>
    /// <remarks>
    /// The discrete uniform distribution generates only discrete numbers.<br />
    /// The implementation bases upon information presented on
    ///   <a href="http://en.wikipedia.org/wiki/Uniform_distribution_%28discrete%29">
    ///   Wikipedia - Uniform distribution (discrete)</a>.
    /// </remarks>
    public sealed class DiscreteUniformDistribution : DiscreteDistribution
    {
        int _a;
        int _b;
        int _n;

        #region Construction
        /// <summary>
        /// Initializes a new instance of the DiscreteUniformDistribution class,
        /// using a <see cref="SystemRandomSource"/> as underlying random number generator.
        /// </summary>
        public
        DiscreteUniformDistribution()
        {
            SetDistributionParameters(0, 1);
        }

        /// <summary>
        /// Initializes a new instance of the DiscreteUniformDistribution class,
        /// using the specified <see cref="RandomSource"/> as underlying random number generator.
        /// </summary>
        /// <param name="random">A <see cref="RandomSource"/> object.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="random"/> is NULL (<see langword="Nothing"/> in Visual Basic).
        /// </exception>
        public
        DiscreteUniformDistribution(RandomSource random)
            : base(random)
        {
            SetDistributionParameters(0, 1);
        }

        /// <summary>
        /// Initializes a new instance of the DiscreteUniformDistribution class,
        /// using a <see cref="SystemRandomSource"/> as underlying random number generator.
        /// </summary>
        public
        DiscreteUniformDistribution(
            int lowerLimit,
            int upperLimit)
        {
            SetDistributionParameters(lowerLimit, upperLimit);
        }
        #endregion

        #region Distribution Parameters
        /// <summary>
        /// Gets or sets the lower limit parameter.
        /// To set all parameters at once consider using
        /// <see cref="SetDistributionParameters"/> instead.
        /// </summary>
        public int LowerLimit
        {
            get { return _a; }
            set { SetDistributionParameters(value, _b); }
        }

        /// <summary>
        /// Gets or sets the upper limit parameter.
        /// To set all parameters at once consider using
        /// <see cref="SetDistributionParameters"/> instead.
        /// </summary>
        public int UpperLimit
        {
            get { return _b; }
            set { SetDistributionParameters(_a, value); }
        }

        /// <summary>
        /// Configure all distribution parameters.
        /// </summary>
        public
        void
        SetDistributionParameters(
            int lowerLimit,
            int upperLimit)
        {
            if(!IsValidParameterSet(lowerLimit, upperLimit))
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentParameterSetInvalid);
            }

            _a = lowerLimit;
            _b = upperLimit;
            _n = _b - _a + 1;
        }

        /// <summary>
        /// Determines whether the specified parameters is valid.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if <paramref name="lowerLimit"/> &lt;= <paramref name="upperLimit"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public static
        bool
        IsValidParameterSet(
            int lowerLimit,
            int upperLimit)
        {
            return lowerLimit <= upperLimit;
        }
        #endregion

        #region Distribution Properties
        /// <summary>
        /// Gets the minimum possible value of generated random numbers.
        /// </summary>
        public override int Minimum
        {
            get { return _a; }
        }

        /// <summary>
        /// Gets the maximum possible value of generated random numbers.
        /// </summary>
        public override int Maximum
        {
            get { return _b; }
        }

        /// <summary>
        /// Gets the mean value of generated random numbers.
        /// </summary>
        public override double Mean
        {
            get { return (0.5 * _a) + (0.5 * _b); }
        }

        /// <summary>
        /// Gets the median of generated random numbers.
        /// </summary>
        public override int Median
        {
            get { return (_a + _b) / 2; }
        }

        /// <summary>
        /// Gets the variance of generated random numbers.
        /// </summary>
        public override double Variance
        {
            get { return (((double)_n * _n) - 1.0) / 12.0; }
        }

        /// <summary>
        /// Gets the skewness of generated random numbers.
        /// </summary>
        public override double Skewness
        {
            get { return 0.0; }
        }

        /// <summary>
        /// Discrete probability mass function (pmf) of this probability distribution.
        /// </summary>
        public override
        double
        ProbabilityMass(int x)
        {
            if(_a <= x && x <= _b)
            {
                return 1.0 / _n;
            }

            return 0.0;
        }

        /// <summary>
        /// Continuous cumulative distribution function (cdf) of this probability distribution.
        /// </summary>
        public override
        double
        CumulativeDistribution(double x)
        {
            if(x < _a)
            {
                return 0.0;
            }

            if(x <= _b)
            {
                return (x - _a + 1.0) / _n;
            }

            return 1.0;
        }
        #endregion

        #region Generator
        /// <summary>
        /// Returns a uniformly distributed floating point random number.
        /// </summary>
        /// <returns>A uniformly distributed double-precision floating point number.</returns>
        public override
        int
        NextInt32()
        {
            return RandomSource.Next(_a, _b + 1);
        }
        #endregion
    }
}
