using System;
using System.Collections.Generic;
using System.Linq;

namespace Baballonia.Services.Inference.Filters;

public class OneEuroFilter : IFilter
{
    private float[] _minCutoff;
    private float[] _beta;
    private float[] _dCutoff;
    private float[] _xPrev;
    private float[] _dxPrev;
    private DateTime _tPrev;
    public OneEuroFilter(float[] x0, float minCutoff = 1.0f, float beta = 0.0f)
    {
        var length = x0.Length;
        _minCutoff = CreateFilledArray(length, minCutoff);
        _beta = CreateFilledArray(length, beta);
        _dCutoff = CreateFilledArray(length, 1);
        // Previous values.
        _xPrev = (float[])x0.Clone();
        _dxPrev = CreateFilledArray(length, 0);
        _tPrev = DateTime.UtcNow;
    }

    public float[] Filter(float[] x)
    {
        if (x.Length != _xPrev.Length)
            throw new ArgumentException($"Input shape does not match initial shape. Expected: {_xPrev.Length}, got: {x.Length}");

        var now = DateTime.UtcNow;
        var elapsedTime = (float)(now - _tPrev).TotalSeconds;

        if (elapsedTime == 0.0f)
        {
            _xPrev = (float[])x.Clone();
            return x;
        }

        var t_e = CreateFilledArray(x.Length, elapsedTime);

        // Derivative
        var dx = new float[x.Length];
        for (var i = 0; i < x.Length; i++) dx[i] = (x[i] - _xPrev[i]) / t_e[i];

        var a_d = SmoothingFactor(t_e, _dCutoff);
        var dxHat = ExponentialSmoothing(a_d, dx, _dxPrev);

        // Adjusted cutoff
        var cutoff = new float[x.Length];
        for (var i = 0; i < x.Length; i++) cutoff[i] = _minCutoff[i] + _beta[i] * Math.Abs(dxHat[i]);

        var a = SmoothingFactor(t_e, cutoff);
        var xHat = ExponentialSmoothing(a, x, _xPrev);

        // Store previous values
        _xPrev = xHat;
        _dxPrev = dxHat;
        _tPrev = now;

        return xHat;
    }

    private static float[] CreateFilledArray(int length, float value) => Enumerable.Repeat(value, length).ToArray();

    private static float[] SmoothingFactor(float[] t_e, float[] cutoff)
    {
        var length = t_e.Length;
        var result = new float[length];
        for (var i = 0; i < length; i++)
        {
            var r = 2 * (float)Math.PI * cutoff[i] * t_e[i];
            result[i] = r / (r + 1);
        }
        return result;
    }

    private static float[] ExponentialSmoothing(float[] a, float[] x, float[] xPrev)
    {
        var length = a.Length;
        var result = new float[length];
        for (var i = 0; i < length; i++) result[i] = a[i] * x[i] + (1 - a[i]) * xPrev[i];
        return result;
    }

}
