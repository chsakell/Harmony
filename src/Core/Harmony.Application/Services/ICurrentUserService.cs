using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Services
{
    public interface ICurrentUserService 
    {
        string UserId { get; }
    }
}
