using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.DTO.Search
{
    public abstract class SearchableBase
    {
        public string ObjectId { get; set; }

        public SearchableBase(Guid cardId)
        {
            ObjectId = cardId.ToString();
        }
    }
}
