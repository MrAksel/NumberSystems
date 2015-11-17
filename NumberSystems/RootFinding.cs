using System;
using Q = Numerics.BigRational;
using Z = System.Numerics.BigInteger;

namespace NumberSystems
{
    public delegate Q FunctionOfOneVariable(Q x);

    class RootFinding
    {
        const int maxIterations = 50;

        public static Q Bisect(FunctionOfOneVariable f, Q left, Q right)
        {
            // extra info that callers may not always want
            int iterationsUsed;
            Q errorEstimate;

            return Bisect(f, left, right, new Q(1, 100000000), Q.Zero, out iterationsUsed, out errorEstimate);
        }

        public static Q Bisect(FunctionOfOneVariable f, Q left, Q right, Q tolerance, Q target, out int iterationsUsed, out Q errorEstimate)
        {
            if (tolerance <= Q.Zero)
            {
                string msg = string.Format("Tolerance must be positive. Recieved {0}.", tolerance);
                throw new ArgumentOutOfRangeException(msg);
            }

            iterationsUsed = 0;
            errorEstimate = tolerance * 2;

            // Standardize the problem.  To solve f(x) = target,
            // solve g(x) = 0 where g(x) = f(x) - target.
            FunctionOfOneVariable g = delegate (Q x) { return f(x) - target; };


            Q g_left = g(left);  // evaluation of f at left end of interval
            Q g_right = g(right);
            Q mid;
            Q g_mid;
            if (g_left * g_right >= Q.Zero)
            {
                string str = "Invalid starting bracket. Function must be above target on one end and below target on other end.";
                string msg = string.Format("{0} Target: {1}. f(left) = {2}. f(right) = {3}", str, g_left + target, g_right + target);
                throw new ArgumentException(msg);
            }

            Q intervalWidth = right - left;

            for (iterationsUsed = 0; iterationsUsed < maxIterations && intervalWidth > tolerance; iterationsUsed++)
            {
                intervalWidth *= new Q(1, 2);
                mid = left + intervalWidth;

                if ((g_mid = g(mid)) == Q.Zero)
                {
                    errorEstimate = Q.Zero;
                    return mid;
                }
                if (g_left * g_mid < Q.Zero)           // g changes sign in (left, mid)    
                    g_right = g(right = mid);
                else                            // g changes sign in (mid, right)
                    g_left = g(left = mid);
            }
            errorEstimate = right - left;
            return left;
        }

        public static Q Brent(FunctionOfOneVariable f, Q left, Q right)
        {
            // extra info that callers may not always want
            int iterationsUsed;
            Q errorEstimate;

            return Brent(f, left, right, new Q(1, 100000000), Q.Zero, out iterationsUsed, out errorEstimate);
        }

        public static Q Brent(FunctionOfOneVariable g, Q left, Q right, Q tolerance, Q target, out int iterationsUsed, out Q errorEstimate)
        {
            if (tolerance <= Q.Zero)
            {
                string msg = string.Format("Tolerance must be positive. Recieved {0}.", tolerance);
                throw new ArgumentOutOfRangeException(msg);
            }

            errorEstimate = tolerance * 2;

            // Standardize the problem.  To solve g(x) = target,
            // solve f(x) = 0 where f(x) = g(x) - target.
            FunctionOfOneVariable f = delegate (Q x) { return g(x) - target; };

            // Implementation and notation based on Chapter 4 in
            // "Algorithms for Minimization without Derivatives"
            // by Richard Brent.

            Q c, d, e, fa, fb, fc, tol, m, p, q, r, s;

            // set up aliases to match Brent's notation
            Q a = left; Q b = right; Q t = tolerance;
            iterationsUsed = 0;

            fa = f(a);
            fb = f(b);

            if (fa * fb > Q.Zero)
            {
                string str = "Invalid starting bracket. Function must be above target on one end and below target on other end.";
                string msg = string.Format("{0} Target: {1}. f(left) = {2}. f(right) = {3}", str, target, fa + target, fb + target);
                throw new ArgumentException(msg);
            }

            label_int:
            c = a; fc = fa; d = e = b - a;
            label_ext:
            if (Q.Abs(fc) < Q.Abs(fb))
            {
                a = b; b = c; c = a;
                fa = fb; fb = fc; fc = fa;
            }

            iterationsUsed++;

            tol = 2.0 * t * Q.Abs(b) + t;
            errorEstimate = m = 0.5 * (c - b);
            if (Q.Abs(m) > tol && fb != Q.Zero) // exact comparison with 0 is OK here
            {
                // See if bisection is forced
                if (Q.Abs(e) < tol || Q.Abs(fa) <= Q.Abs(fb))
                {
                    d = e = m;
                }
                else
                {
                    s = fb / fa;
                    if (a == c)
                    {
                        // linear interpolation
                        p = 2.0 * m * s; q = 1.0 - s;
                    }
                    else
                    {
                        // Inverse quadratic interpolation
                        q = fa / fc; r = fb / fc;
                        p = s * (2.0 * m * q * (q - r) - (b - a) * (r - 1.0));
                        q = (q - 1.0) * (r - 1.0) * (s - 1.0);
                    }
                    if (p > 0.0)
                        q = -q;
                    else
                        p = -p;
                    s = e; e = d;
                    if (new Q(2, 1) * p < new Q(3, 1) * m * q - Q.Abs(tol * q) && p < Q.Abs(new Q(1, 2) * s * q))
                        d = p / q;
                    else
                        d = e = m;
                }
                a = b; fa = fb;
                if (Q.Abs(d) > tol)
                    b += d;
                else if (m > Q.Zero)
                    b += tol;
                else
                    b -= tol;
                if (iterationsUsed == maxIterations)
                    return b;

                fb = f(b);
                if ((fb > Q.Zero && fc > Q.Zero) || (fb <= Q.Zero && fc <= Q.Zero))
                    goto label_int;
                else
                    goto label_ext;
            }
            else
                return b;
        }

        public static Q Newton(FunctionOfOneVariable f, FunctionOfOneVariable fprime, Q guess)
        {
            // extra info that callers may not always want
            int iterationsUsed;
            Q errorEstimate;

            return Newton(f, fprime, guess, new Q(1, 100000000), Q.Zero, out iterationsUsed, out errorEstimate);
        }

        public static Q Newton(FunctionOfOneVariable f, FunctionOfOneVariable fprime, Q guess, Q tolerance, Q target, out int iterationsUsed, out Q errorEstimate)
        {
            if (tolerance <= Q.Zero)
            {
                string msg = string.Format("Tolerance must be positive. Recieved {0}.", tolerance);
                throw new ArgumentOutOfRangeException(msg);
            }

            iterationsUsed = 0;
            errorEstimate = tolerance * 2;

            // Standardize the problem.  To solve f(x) = target,
            // solve g(x) = 0 where g(x) = f(x) - target.
            // Note that f(x) and g(x) have the same derivative.
            FunctionOfOneVariable g = delegate (Q x) { return f(x) - target; };

            Q oldX, newX = guess;

            for (iterationsUsed = 0; iterationsUsed < maxIterations && errorEstimate > tolerance; iterationsUsed++)
            {
                oldX = newX;
                Q gx = g(oldX);
                Q gprimex = fprime(oldX);
                Q absgprimex = Q.Abs(gprimex);

                newX = oldX - gx / gprimex;
                errorEstimate = Q.Abs(newX - oldX);
            }

            return newX;
        }
    }
}