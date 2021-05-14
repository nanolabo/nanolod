namespace Nanolod.Calibration.Common
{
    public class KeyValue<KeyType, ValueType>
    {
        public KeyType key { get; set; }
        public ValueType value { get; set; }
        public KeyValue()
        {
            this.key = default(KeyType);
            this.value = default(ValueType);
        }

        public KeyValue(KeyType key, ValueType value)
        {
            if (key != null) this.key = key;
            if (value != null) this.value = value;
        }
    }
}
