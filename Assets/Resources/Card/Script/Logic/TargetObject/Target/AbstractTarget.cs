using Assets.GameComponent.Card.Logic.Effect;
using SerializeReferenceEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.GameComponent.Card.Logic.TargetObject.Target.AbstractTarget.AbstractTargetDataType.AbstractTargetDataTypeValue.ValueNumber;
using static Unity.VisualScripting.Member;

namespace Assets.GameComponent.Card.Logic.TargetObject.Target
{
    [Serializable]
    public abstract class AbstractTarget
    {
        /*
         * Data type
         * * Value: that it contant number or a digit
         * * Attribute: that effect attribute
         */
        [Serializable]
        public abstract class AbstractTargetDataType
        {
            [Serializable]
            public abstract class AbstractTargetDataTypeValue : AbstractTargetDataType
            {
                public abstract void Execute<T, U>(List<U> source, Func<T, int> GetValue) where T : U;

                [SRName("Value/Number")]
                public class ValueNumber : AbstractTargetDataTypeValue
                {
                    public enum compareType
                    {
                        equal,
                        more,
                        moreEqual,
                        less,
                        lessEqual
                    }

                    public compareType comepare;
                    public int number;



                    public override void Execute<T, U>(List<U> source, Func<T, int> GetValue)
                    {
                        for (int i = source.Count - 1; i >= 0; i--)
                        {
                            var target = source[i];
                            int input = GetValue((T)target);

                            bool flag = false;

                            switch (comepare)
                            {
                                case compareType.equal:
                                    flag = input == number;
                                    break;
                                case compareType.more:
                                    flag = input > number;
                                    break;

                                case compareType.moreEqual:
                                    flag = input >= number;
                                    break;

                                case compareType.less:
                                    flag = input < number;
                                    break;

                                case compareType.lessEqual:
                                    flag = input <= number;
                                    break;
                                default:
                                    Debug.Log("Not found compare type");
                                    break;
                            }
                            if (!flag)
                            {
                                source.RemoveAt(i);
                            }
                        }
                    }

                    [SRName("Value/Highest")]

                    public class ValueHighest : AbstractTargetDataTypeValue
                    {
                        //public override bool Check<T>(T source, Func<T, int> GetValue)
                        //{
                        //    throw new NotImplementedException();
                        //}

                        public override void Execute<T, U>(List<U> source, Func<T, int> GetValue)
                        {
                            // Check if the source list is empty
                            if (source == null || source.Count == 0)
                            {
                                // Throw an exception or return
                                throw new ArgumentException("Source list cannot be empty");
                            }

                            // Initialize the highest value and a list of items with the highest value
                            int highestValue = int.MinValue;
                            List<T> highestItems = new List<T>();

                            // Loop through the source list
                            foreach (T item in source)
                            {
                                // Get the value of the current item using the GetValue function
                                int value = GetValue(item);

                                // Compare the value with the highest value so far
                                if (value > highestValue)
                                {
                                    // Update the highest value and clear the list of items with the highest value
                                    highestValue = value;
                                    highestItems.Clear();
                                }

                                // If the value is equal to the highest value, add the item to the list of items with the highest value
                                if (value == highestValue)
                                {
                                    highestItems.Add(item);
                                }
                            }

                            // Remove all items from the source list that are not in the list of items with the highest value
                            source.RemoveAll(item => !highestItems.Contains((T)item));

                            // Do something with the modified source list and the highest value
                            // For example, print them to the console
                            Console.WriteLine($"Highest value: {highestValue}");
                            Console.WriteLine($"Modified source list: {string.Join(", ", source)}");
                        }
                    }
                    [SRName("Value/Lowest")]
                    public class ValueLowest : AbstractTargetDataTypeValue
                    {
                        //public override bool Check<T>(T source, Func<T, int> GetValue)
                        //{
                        //    throw new NotImplementedException();
                        //}

                        public override void Execute<T, U>(List<U> source, Func<T, int> GetValue)
                        {
                            // Check if the source list is empty
                            if (source == null || source.Count == 0)
                            {
                                // Throw an exception or return
                                throw new ArgumentException("Source list cannot be empty");
                            }

                            // Initialize the lowest value and a list of items with the lowest value
                            int lowestValue = int.MaxValue;
                            List<T> lowestItems = new List<T>();

                            // Loop through the source list
                            foreach (T item in source)
                            {
                                // Get the value of the current item using the GetValue function
                                int value = GetValue(item);

                                // Compare the value with the lowest value so far
                                if (value < lowestValue)
                                {
                                    // Update the lowest value and clear the list of items with the lowest value
                                    lowestValue = value;
                                    lowestItems.Clear();
                                }

                                // If the value is equal to the lowest value, add the item to the list of items with the lowest value
                                if (value == lowestValue)
                                {
                                    lowestItems.Add(item);
                                }
                            }

                            // Remove all items from the source list that are not in the list of items with the lowest value
                            source.RemoveAll(item => !lowestItems.Contains((T)item));

                            // Do something with the modified source list and the lowest value
                            // For example, print them to the console
                            Console.WriteLine($"Lowest value: {lowestValue}");
                            Console.WriteLine($"Modified source list: {string.Join(", ", source)}");
                        }
                    }
                }
                [Serializable]
                public class AbstractTargetDataTypeAttribute : AbstractTargetDataType, IEffectAttributes
                {
                    public bool _IsCharming;
                    public bool _IsTreating;
                    public bool _IsDominating;
                    public bool _IsBlockAttack;
                    public bool _IsBlockDefend;


                    public bool IsCharming { get => _IsCharming; set => _IsCharming = value; }
                    public bool IsTreating { get => _IsTreating; set => _IsTreating = value; }
                    public bool IsDominating { get => _IsDominating; set => _IsDominating = value; }
                    public bool IsBlockAttack { get => _IsBlockAttack; set => _IsBlockAttack = value; }
                    public bool IsBlockDefend { get => _IsBlockDefend; set => _IsBlockDefend = value; }

                    public void Execute<T, U>(List<U> source) where T : IEffectAttributes, U
                    {
                        for (int i = source.Count - 1; i >= 0; i--)
                        {
                            var target = (IEffectAttributes)source[i];



                            bool flag = target.isEqualsAttibutes(this);

                            if (!flag)
                            {
                                source.RemoveAt(i);
                            }
                        }
                    }
                }
            }
        }
    }
}
