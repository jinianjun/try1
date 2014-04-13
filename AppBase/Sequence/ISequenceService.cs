using System;
using System.Collections.Generic;
using System.Text;
using AppBase.Domain;
namespace AppBase.Sequence
{
    public interface ISequenceService : IBaseService<Domain_SEQUENCE>
    {
        int GetNextId(string tablename);
    }
}
