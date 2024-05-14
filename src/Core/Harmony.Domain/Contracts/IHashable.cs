using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Domain.Contracts
{
    public interface IHashable
    {
        Dictionary<string, string> ConvertToDictionary();
    }
}
