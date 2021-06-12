using System;
using System.Runtime.Serialization;
using Mono.CecilX;

namespace Mirror.Weaver
{
    [Serializable]
    public abstract class WeaverException : Exception
    {
        protected WeaverException(string message, MemberReference member) : base(message)
        {
            MemberReference = member;
        }

        protected WeaverException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(
            serializationInfo, streamingContext)
        {
        }

        public MemberReference MemberReference { get; }
    }

    [Serializable]
    public class GenerateWriterException : WeaverException
    {
        public GenerateWriterException(string message, MemberReference member) : base(message, member)
        {
        }

        protected GenerateWriterException(SerializationInfo serializationInfo, StreamingContext streamingContext) :
            base(serializationInfo, streamingContext)
        {
        }
    }
}