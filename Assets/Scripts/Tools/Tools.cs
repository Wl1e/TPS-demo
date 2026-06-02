using System;

static public class Tools
{
    [Serializable]
    public struct Entry<KeyType, ValueType>
    {
        public KeyType Key;
        public ValueType Value;
    }
}
