using Assets.GameComponent.Card.LogicCard;
using JetBrains.Annotations;
using SerializeReferenceEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Assets.GameComponent.Card.Logic.Effect.Store
{
    [SRName("Logic/Effect/Temp Store")]
    public class TempStore : AbstractEffect, IDictionary<int, object>
    {
        [SerializeReference]
        [SRLogicCard(typeof(AbstractAction))]
        public List<AbstractAction> Actions;

        /// <summary>
        /// key: PhotonViewID
        /// value: list of object stored
        /// </summary>
        private static Dictionary<int, object> aaa = new Dictionary<int, object>();
        // A function that takes an int key and an object value as parameters
        public static void AddToStore(int key, object value)
        {
            // Check if the dictionary contains the key
            if (aaa.ContainsKey(key))
            {
                // If yes, add the value to the existing list
                aaa[key] = value;
            }
            else
            {
                // If no, create a new list with the value and add it to the dictionary
                aaa.Add(key, value);
            }
        }
        public static object GetFromStore(int key)
        {
            object value;
            // Try to get the value from the dictionary
            if (aaa.TryGetValue(key, out value))
            {
                // If the key was found, return the value
                return value;
            }
            else
            {
                // If the key was not found, return an empty list
                return null;
            }
        }

        public static bool isContaint(int key)
        {
            return aaa.ContainsKey(key);
        }
        // Add an element with the provided key and value to the dictionary
        public void Add(int key, object value)
        {
            aaa.Add(key, value);
        }

        // Remove all elements from the dictionary
        public void Clear()
        {
            aaa.Clear();
        }

        // Determine whether the dictionary contains an element with the specified key
        public bool ContainsKey(int key)
        {
            return aaa.ContainsKey(key);
        }

        // Determine whether the dictionary contains an element with the specified value
        public bool ContainsValue(object value)
        {
            return aaa.ContainsValue(value);
        }

        // Remove the element with the specified key from the dictionary
        public bool Remove(int key)
        {
            return aaa.Remove(key);
        }

        // Get the value associated with the specified key, or return false if the key does not exist
        public bool TryGetValue(int key, out object value)
        {
            return aaa.TryGetValue(key, out value);
        }
        // Add a key-value pair to the dictionary
        public void Add(KeyValuePair<int, object> item)
        {
            aaa.Add(item.Key, item.Value);
        }

        // Determine whether the dictionary contains a specific key-value pair
        public bool Contains(KeyValuePair<int, object> item)
        {
            return aaa.ContainsKey(item.Key) && aaa[item.Key].Equals(item.Value);
        }

        // Copy the elements of the dictionary to an array, starting at a particular array index
        public void CopyTo(KeyValuePair<int, object>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            if (arrayIndex < 0 || arrayIndex > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }
            if (array.Length - arrayIndex < aaa.Count)
            {
                throw new ArgumentException("The number of elements in the source dictionary is greater than the available space from arrayIndex to the end of the destination array.");
            }
            foreach (var item in aaa)
            {
                array[arrayIndex++] = item;
            }
        }

        // Remove a specific key-value pair from the dictionary
        public bool Remove(KeyValuePair<int, object> item)
        {
            return aaa.Remove(item.Key) && item.Value.Equals(aaa[item.Key]);
        }

        // Return an enumerator that iterates through the dictionary
        public IEnumerator<KeyValuePair<int, object>> GetEnumerator()
        {
            return aaa.GetEnumerator();
        }

        // Return an enumerator that iterates through the dictionary
        IEnumerator IEnumerable.GetEnumerator()
        {
            return aaa.GetEnumerator();
        }

        public override void RevokeEffect(object register, MatchManager match)
        {
        }

        public override bool GainEffect(object register, EffectManager match)
        {
            return true;
        }

        // Get or set the element with the specified key
        public object this[int key]
        {
            get { return aaa[key]; }
            set { aaa[key] = value; }
        }

        // Get a collection containing the keys in the dictionary
        public ICollection<int> Keys
        {
            get { return aaa.Keys; }
        }

        // Get a collection containing the values in the dictionary
        public ICollection<object> Values
        {
            get { return aaa.Values; }
        }

        // Get the number of elements in the dictionary
        public int Count
        {
            get { return aaa.Count; }
        }

        // Get a value indicating whether the dictionary is read-only
        public bool IsReadOnly
        {
            get { return false; }
        }
    }
}
