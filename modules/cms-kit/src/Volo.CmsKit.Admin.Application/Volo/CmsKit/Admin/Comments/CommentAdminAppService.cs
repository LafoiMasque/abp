﻿using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.CmsKit.Comments;
using Volo.CmsKit.Users;

namespace Volo.CmsKit.Admin.Comments
{
    public class CommentAdminAppService : CmsKitAdminAppServiceBase, ICommentAdminAppService
    {
        protected readonly ICommentRepository CommentRepository;

        public CommentAdminAppService(ICommentRepository commentRepository)
        {
            CommentRepository = commentRepository;
        }

        public virtual async Task<PagedResultDto<CommentDto>> GetListAsync(CommentGetListInput input)
        {
            var totalCount = await CommentRepository.GetCountAsync(
                input.Filter,
                input.EntityType,
                input.EntityId,
                input.RepliedCommentId,
                input.CreatorId,
                input.CreationStartDate,
                input.CreationEndDate);

            var comments = await CommentRepository.GetListAsync(
                input.Filter,
                input.EntityType,
                input.EntityId,
                input.RepliedCommentId,
                input.CreatorId,
                input.CreationStartDate,
                input.CreationEndDate,
                input.Sorting,
                input.MaxResultCount,
                input.SkipCount
            );

            var dtos = ObjectMapper.Map<List<Comment>, List<CommentDto>>(comments);

            return new PagedResultDto<CommentDto>(totalCount, dtos);
        }

        public virtual async Task<CommentWithAuthorDto> GetAsync(Guid id)
        {
            var comment = await CommentRepository.GetWithAuthorAsync(id);

            var dto = ObjectMapper.Map<Comment, CommentWithAuthorDto>(comment.Comment);
            dto.Author = ObjectMapper.Map<CmsUser, CmsUserDto>(comment.Author);

            return dto;
        }

        public virtual async Task DeleteAsync(Guid id)
        {
            var comment = await CommentRepository.GetAsync(id);

            await CommentRepository.DeleteWithRepliesAsync(comment);
        }
    }
}