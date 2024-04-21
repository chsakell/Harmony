using AutoMapper;
using Harmony.Application.SourceControl.DTO;
using Harmony.Domain.SourceControl;

namespace Harmony.Application.SourceControl.Mappings
{
    public class SourceControlProfile : Profile
    {
        public SourceControlProfile()
        {
            CreateMap<Branch, BranchDto>();
            CreateMap<RepositoryUser, RepositoryUserDto>();
            CreateMap<Commit, CommitDto>();
            CreateMap<Author, AuthorDto>();
            CreateMap<PullRequest, PullRequestDto>();
        }
    }
}
