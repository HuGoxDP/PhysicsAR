using System;

namespace _Project.Scripts.Architecture
{
    public interface IValueRestriction<T>
    {
        bool IsValid(T value);
        string GetErrorMessage(T value);
    }


    public abstract class ComparisonRestriction<T> : IValueRestriction<T>
    {
        protected readonly T _compareValue;
        protected readonly string _restrictionName;

        protected ComparisonRestriction(T compareValue)
        {
            _compareValue = compareValue;
            _restrictionName = GetType().Name;
        }

        public abstract bool IsValid(T value);
        public abstract string GetErrorMessage(T value);

        protected void ValidateComparable(T value)
        {
            if (!(value is IComparable<T>) && !(value is IComparable))
            {
                throw new InvalidOperationException(
                    $"@Error | {_restrictionName} : Type {typeof(T)} does not implement IComparable."
                );
            }
        }

        protected int Compare(T value)
        {
            if (value is IComparable<T> comparableGeneric)
            {
                return comparableGeneric.CompareTo(_compareValue);
            }

            if (value is IComparable comparable)
            {
                return comparable.CompareTo(_compareValue);
            }

            throw new InvalidOperationException(
                $"@Error | {_restrictionName} : Type {typeof(T)} does not implement IComparable."
            );
        }
    }

    public class GreaterThanRestriction<T> : ComparisonRestriction<T>
    {
        public GreaterThanRestriction(T compareValue) : base(compareValue)
        {
        }

        public override bool IsValid(T value)
        {
            return Compare(value) > 0;
        }

        public override string GetErrorMessage(T value)
        {
            return $"@Error | {_restrictionName} : Value {value} is not greater than {_compareValue}.";
        }
    }

    public class GreaterThanOrEqualRestriction<T> : ComparisonRestriction<T>
    {
        public GreaterThanOrEqualRestriction(T compareValue) : base(compareValue)
        {
        }

        public override bool IsValid(T value)
        {
            return Compare(value) >= 0;
        }

        public override string GetErrorMessage(T value)
        {
            return $"@Error | {_restrictionName} : Value {value} is not greater than or equal to {_compareValue}.";
        }
    }

    public class EqualRestriction<T> : ComparisonRestriction<T>
    {
        public EqualRestriction(T compareValue) : base(compareValue)
        {
        }

        public override bool IsValid(T value)
        {
            return Compare(value) == 0;
        }

        public override string GetErrorMessage(T value)
        {
            return $"@Error | {_restrictionName} : Value {value} is not equal to {_compareValue}.";
        }
    }

    public class NotEqualRestriction<T> : ComparisonRestriction<T>
    {
        public NotEqualRestriction(T compareValue) : base(compareValue)
        {
        }

        public override bool IsValid(T value)
        {
            return Compare(value) != 0;
        }

        public override string GetErrorMessage(T value)
        {
            return $"@Error | {_restrictionName} : Value {value} is equal to {_compareValue}.";
        }
    }

    public class LessThanRestriction<T> : ComparisonRestriction<T>
    {
        public LessThanRestriction(T compareValue) : base(compareValue)
        {
        }

        public override bool IsValid(T value)
        {
            return Compare(value) < 0;
        }

        public override string GetErrorMessage(T value)
        {
            return $"@Error | {_restrictionName} : Value {value} is not less than {_compareValue}.";
        }
    }

    public class LessThanOrEqualRestriction<T> : ComparisonRestriction<T>
    {
        public LessThanOrEqualRestriction(T compareValue) : base(compareValue)
        {
        }

        public override bool IsValid(T value)
        {
            return Compare(value) <= 0;
        }

        public override string GetErrorMessage(T value)
        {
            return $"@Error | {_restrictionName} : Value {value} is not less than or equal to {_compareValue}.";
        }
    }

    public class RangeRestriction<T> : IValueRestriction<T> where T : IComparable<T>
    {
        private readonly bool _inclusiveMax;
        private readonly bool _inclusiveMin;
        private readonly T _maxValue;
        private readonly T _minValue;
        private readonly string _restrictionName;

        public RangeRestriction(T minValue, T maxValue, bool inclusiveMin = true, bool inclusiveMax = true)
        {
            _minValue = minValue;
            _maxValue = maxValue;
            _inclusiveMin = inclusiveMin;
            _inclusiveMax = inclusiveMax;
            _restrictionName = GetType().Name;
        }

        public bool IsValid(T value)
        {
            int compareToMin = value.CompareTo(_minValue);
            int compareToMax = value.CompareTo(_maxValue);

            bool minValid = _inclusiveMin ? compareToMin >= 0 : compareToMin > 0;
            bool maxValid = _inclusiveMax ? compareToMax <= 0 : compareToMax < 0;

            return minValid && maxValid;
        }

        public string GetErrorMessage(T value)
        {
            string minBoundary = _inclusiveMin ? "[" : "(";
            string maxBoundary = _inclusiveMax ? "]" : ")";

            return
                $"@Error | {_restrictionName} : Value {value} is not within range {minBoundary}{_minValue}, {_maxValue}{maxBoundary}.";
        }
    }
}