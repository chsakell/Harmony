using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Domain.Contracts
{
    /// <summary>
    /// The base class for entities
    /// </summary>
    /// <typeparam name="TId"></typeparam>
    public interface IEntity<TId> : IEntity
    {
        public TId Id { get; set; }
    }

    public interface IEntity
    {
    }
}
