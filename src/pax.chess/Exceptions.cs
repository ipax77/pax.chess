using System;
using System.Runtime.Serialization;
namespace pax.chess;

[Serializable]
public class MoveException : Exception
{
    public MoveException()
    {
    }

    public MoveException(string message) : base(message)
    {
    }

    public MoveException(string message, Exception innerExeption) : base(message, innerExeption)
    {
    }    

    protected MoveException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}


[Serializable]
public class MoveMapException : Exception
{
    public MoveMapException()
    {
    }

    public MoveMapException(string message) : base(message)
    {
    }

    public MoveMapException(string message, Exception innerExeption) : base(message, innerExeption)
    {
    }    

    protected MoveMapException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
