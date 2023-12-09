using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.DTO;
using Harmony.Domain.Entities;
using Harmony.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Infrastructure.Services.Management
{
    public class CommentService : ICommentService
    {
        private readonly UserManager<HarmonyUser> _userManager;
        private readonly ICommentRepository _commentRepository;

        public CommentService(UserManager<HarmonyUser> userManager,
            ICommentRepository commentRepository)
        {
            _userManager = userManager;
            _commentRepository = commentRepository;
        }

        public async Task<List<CommentDto>> GetCommentsForCard(Guid cardId, string userId)
        {
            return await (from comment in _commentRepository.Entities
                          join user in _userManager.Users on comment.UserId equals user.Id
                          where comment.CardId == cardId
                          orderby comment.DateCreated ascending
                          select new CommentDto
                          {
                              Id = comment.Id,
                              DateCreated = comment.DateCreated,
                              Text = comment.Text,
                              User = new UserPublicInfo()
                              {
                                  FirstName = user.FirstName,
                                  LastName = user.LastName,
                                  UserName = user.UserName,
                                  ProfilePicture = user.ProfilePicture
                              },
                              Editable = user.Id == userId
                          }).ToListAsync();
        }
    }
}
