using Harmony.Application.DTO.Search;
using Harmony.Application.Features.Labels.Commands.CreateCardLabel;
using Harmony.Shared.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Features.Search.Commands.AdvancedSearch
{
    internal class AdvancedSearchCommandHandler : IRequestHandler<AdvancedSearchCommand, IResult<List<SearchableCard>>>
    {
        public AdvancedSearchCommandHandler()
        {
            
        }

        public async Task<IResult<List<SearchableCard>>> Handle(AdvancedSearchCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
