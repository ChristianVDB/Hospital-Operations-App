using System;

namespace HospitalOperationsSystem
{
    //thrown when an operation is attempted that is invalid given the current state of the system
    public class InvalidSystemStateException : Exception
    {
        public InvalidSystemStateException(string message) : base(message) { }

    }

    //thrown when a resource limit is exceeded, such as maximum number of patients or beds
    public class ResourceLimitExceededException : Exception
    {
        public ResourceLimitExceededException(string message) : base(message) { }
    }


}