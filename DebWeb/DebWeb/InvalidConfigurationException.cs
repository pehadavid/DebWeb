namespace DebWeb
{

    [System.Serializable]
    public class InvalidConfigurationException : System.Exception
    {
        public InvalidConfigurationException() { }
        public InvalidConfigurationException(string message) : base(message) { }
        public InvalidConfigurationException(string message, System.Exception inner) : base(message, inner) { }
        protected InvalidConfigurationException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    
}