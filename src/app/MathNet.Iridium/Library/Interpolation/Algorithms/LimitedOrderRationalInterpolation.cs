//-----------------------------------------------------------------------
// <copyright file="LimitedOrderRationalInterpolation.cs" company="Math.NET Project">
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
//    Numerical Recipes in C++, Second Edition [2003]
//    Handbook of Mathematical Functions [1965]
// </contribution>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace MathNet.Numerics.Interpolation.Algorithms
{
    /// <summary>
    /// Limited Order Rational Interpolation (with poles) using Bulirsch &amp; Stoer's Algorithm.
    /// </summary>
    public class LimitedOrderRationalInterpolation :
        IInterpolationMethod
    {
        SampleList _samples;
        int _maximumOrder;
        int _effectiveOrder;

        /// <summary>
        /// Initializes a new instance of the LimitedOrderRationalInterpolation class.
        /// with full order.
        /// </summary>
        public
        LimitedOrderRationalInterpolation()
        {
            _maximumOrder = int.MaxValue;
            _effectiveOrder = -1;
        }

        /// <summary>
        /// Initializes a new instance of the LimitedOrderRationalInterpolation class.
        /// with the given order.
        /// </summary>
        public
        LimitedOrderRationalInterpolation(int maximumOrder)
        {
            _maximumOrder = maximumOrder;
            _effectiveOrder = -1;
        }

        /// <summary>
        /// True if the algorithm supports differentiation.
        /// </summary>
        /// <seealso cref="Differentiate"/>
        public bool SupportsDifferentiation
        {
            get { return false; }
        }

        /// <summary>
        /// True if the algorithm supports integration.
        /// </summary>
        /// <seealso cref="Integrate"/>
        public bool SupportsIntegration
        {
            get { return false; }
        }

        /// <summary>
        /// The maximum interpolation order.
        /// </summary>
        /// <seealso cref="EffectiveOrder"/>
        public int MaximumOrder
        {
            get
            {
                return _maximumOrder;
            }

            set
            {
                if(value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                if(_maximumOrder == value)
                {
                    return;
                }

                if(null == _samples)
                {
                    _maximumOrder = value;
                    _effectiveOrder = -1;
                    return;
                }

                _maximumOrder = value;
                _effectiveOrder = Math.Min(value, _samples.Count);
            }
        }

        /// <summary>
        /// The interpolation order that is effectively used.
        /// </summary>
        /// <seealso cref="MaximumOrder"/>
        public int EffectiveOrder
        {
            get { return _effectiveOrder; }
        }

        /// <summary>
        /// Precompute/optimize the algorithm for the given sample set.
        /// </summary>
        /// <param name="t">Points t</param>
        /// <param name="x">Values x(t)</param>
        public
        void
        Init(
            IList<double> t,
            IList<double> x)
        {
            Init(new SampleList(t, x));
        }

        /// <summary>
        /// Precompute/optimize the algorithm for the given sample set.
        /// </summary>
        /// <param name="samples">Sample points t and values x(t).</param>
        public
        void
        Init(SampleList samples)
        {
            if(null == samples)
            {
                throw new ArgumentNullException("samples");
            }

            _samples = samples;
            _effectiveOrder = Math.Min(_maximumOrder, samples.Count);
        }

        /// <summary>
        /// Interpolate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated value x(t).</returns>
        public
        double
        Interpolate(double t)
        {
            if(null == _samples)
            {
                throw new InvalidOperationException(Properties.LocalStrings.InvalidOperationNoSamplesProvided);
            }

            const double Tiny = 1.0e-15;
            int closestIndex;
            int offset = SuggestOffset(t, out closestIndex);
            double[] c = new double[_effectiveOrder];
            double[] d = new double[_effectiveOrder];
            
            int ns = closestIndex - offset;

            if(Number.AlmostEqual(_samples.GetT(closestIndex), t))
            {
                return _samples.GetX(closestIndex);
            }

            for(int i = 0; i < _effectiveOrder; i++)
            {
                c[i] = _samples.GetX(offset + i);
                d[i] = c[i] + Tiny; // prevent rare zero-over-zero condition
            }

            double x = _samples.GetX(offset + ns--);
            for(int level = 1; level < _effectiveOrder; level++)
            {
                for(int i = 0; i < _effectiveOrder - level; i++)
                {
                    double hp = _samples.GetT(offset + i + level) - t;
                    double ho = (_samples.GetT(offset + i) - t) * d[i] / hp;
                    
                    double den = ho - c[i + 1];
                    if(Number.AlmostZero(den))
                    {
                        // BUGBUG: check - positive or negative infinity?
                        return double.PositiveInfinity;
                    }

                    den = (c[i + 1] - d[i]) / den;
                    d[i] = c[i + 1] * den;
                    c[i] = ho * den;
                }

                x += (2 * (ns + 1) < (_effectiveOrder - level) ? c[ns + 1] : d[ns--]);
            }

            return x;
        }

        int
        SuggestOffset(
            double t,
            out int closestIndex)
        {
            closestIndex = Math.Max(_samples.Locate(t), 0);
            int ret = Math.Min(
                Math.Max(
                    closestIndex - ((_effectiveOrder - 1) / 2),
                    0),
                _samples.Count - _effectiveOrder);

            if(closestIndex < (_samples.Count - 1))
            {
                double dist1 = Math.Abs(t - _samples.GetT(closestIndex));
                double dist2 = Math.Abs(t - _samples.GetT(closestIndex + 1));

                if(dist1 > dist2)
                {
                    closestIndex++;
                }
            }

            return ret;
        }

        /// <summary>
        /// Differentiate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <param name="first">Interpolated first derivative at point t.</param>
        /// <param name="second">Interpolated second derivative at point t.</param>
        /// <returns>Interpolated value x(t).</returns>
        public
        double
        Differentiate(
            double t,
            out double first,
            out double second)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Definite Integrate up to point t.
        /// </summary>
        /// <param name="t">Right bound of the integration interval [a,t].</param>
        /// <returns>Interpolated definite integral over the interval [a,t].</returns>
        /// <seealso cref="SupportsIntegration"/>
        public
        double
        Integrate(double t)
        {
            throw new NotSupportedException();
        }
    }
}
