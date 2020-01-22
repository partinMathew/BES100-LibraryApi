using System;

namespace LibraryApi2.Services
{
    public interface IGenerateEnrollmentIds
    {
        Guid GetNewId();
    }
}