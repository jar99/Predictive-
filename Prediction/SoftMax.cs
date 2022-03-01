namespace Document;

public struct SoftMax
{
    public SoftMax(uint[] values, double smRMin = 0, double smRMax = 10)
    {
        var (min, max, sum) = MinMaxSum(values);
        var d = values.Select(x => Range(x, min, max, smRMin, smRMax)).ToArray();

        var (expvalues, smsum) = BuildExp(d);
        _values = d;

        _max = max;
        _min = min;
        _sum = sum;
        _expvalues = expvalues;
        _smsum = smsum;
        _smRMin = smRMin;
        _smRMax = smRMax;
    }

    public SoftMax(double[] values)
    {
        var (min, max, sum) = MinMaxSum(values);

        var (expvalues, smsum) = BuildExp(values);
        _values = values;
        _max = max;
        _min = min;
        _sum = sum;
        _expvalues = expvalues;
        _smsum = smsum;
    }


    private static (uint min, uint max, uint sum) MinMaxSum(uint[] values)
    {
        var max = uint.MinValue;
        var min = uint.MaxValue;
        var sum = (uint)0;

        var length = values.Length;
        for (int i = 0; i < length; i++)
        {
            var count = values[i];
            if (count > max) max = count;
            if (count < min) min = count;
            sum += count;
        }

        return (min, max, sum);
    }

    private static (double min, double max, double sum) MinMaxSum(double[] values)
    {
        var max = double.MinValue;
        var min = double.MaxValue;
        var sum = 0.0;

        var length = values.Length;
        for (int i = 0; i < length; i++)
        {
            var count = values[i];
            if (count > max) max = count;
            if (count < min) min = count;
            sum += count;
        }

        return (min, max, sum);
    }

    private static (double[] expvalues, double smsum) BuildExp(double[] values)
    {
        var length = values.Length;
        var expvalues = new double[length];
        var smsum = 0.0;

        for (int i = 0; i < length; i++)
        {
            var count = values[i];
            var exp = Math.Exp(count);
            expvalues[i] = exp;
            smsum += exp;
        }

        return (expvalues, smsum);
    }


    public static double Range(double value, double inMin, double inMax, double outMin = 0, double outMax = 10)
    {
        var slope = (outMax - outMin) / (inMax - inMin);
        return outMin + slope * (value - inMin);
    }


    private double _smsum;
    private double[] _expvalues;

    private double[] _values;

    private double _smRMin = 0;
    private double _smRMax = 10;

    private double _max;
    private double _min;
    private double _sum;


    public double IndexValue(int id)
    {
        return Value(_values[id]);
    }

    public double Value(double value)
    {
        var e = Math.Exp(value) / _smsum;
        return e;
    }

    public double Range(uint value)
    {
        return Range(value, _min, _max, _smRMin, _smRMax);
    }

    public static Dictionary<TKey, double> Dictionary<TKey>(IDictionary<TKey, uint> dictionary, int min = 0,
        int max = 10) where TKey : notnull
    {
        var array = dictionary.ToArray();
        var length = array.Length;
        var dic = new Dictionary<TKey, double>();

        var values = array.Select(x => x.Value).ToArray();
        var d = new SoftMax(values, min, max);
        for (int i = 0; i < length; i++)
        {
            dic[array[i].Key] = d.IndexValue(i);
        }

        return dic;
    }

    public static Dictionary<TKey, double> WordsPerMillion<TKey>(Dictionary<TKey, uint> dictionary, int factor = 1_000_000) where TKey : notnull
    {
        var array = dictionary.ToArray();
        var length = array.Length;
        var dic = new Dictionary<TKey, double>();
        
        
        
        var values = array.Select(x => x.Value).ToArray();
        var (min, max, sum) = MinMaxSum(values);
        for (int i = 0; i < length; i++)
        {
            dic[array[i].Key] = (double)values[i] / sum * factor;
        }

        return dic;
    }
}