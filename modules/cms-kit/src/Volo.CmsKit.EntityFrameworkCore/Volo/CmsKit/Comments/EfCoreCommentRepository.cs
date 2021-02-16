﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.CmsKit.EntityFrameworkCore;
using Volo.CmsKit.Users;

namespace Volo.CmsKit.Comments
{
    public class EfCoreCommentRepository : EfCoreRepository<ICmsKitDbContext, Comment, Guid>,
        ICommentRepository
    {
        public EfCoreCommentRepository(IDbContextProvider<ICmsKitDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<CommentWithAuthorQueryResultItem> GetWithAuthorAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var query = from comment in (await GetDbSetAsync())
                join user in (await GetDbContextAsync()).Set<CmsUser>() on comment.CreatorId equals user.Id
                where id == comment.Id
                select new CommentWithAuthorQueryResultItem
                {
                    Comment = comment,
                    Author = user
                };

            var commentWithAuthor = await query.FirstOrDefaultAsync(GetCancellationToken(cancellationToken));

            if (commentWithAuthor == null)
            {
                throw new EntityNotFoundException(typeof(Comment), id);
            }

            return commentWithAuthor;
        }

        public async Task<List<Comment>> GetListAsync(
            string filter = null, 
            string entityType = null, 
            string entityId = null, 
            Guid? repliedCommentId = null,
            Guid? creatorId = null, 
            DateTime? creationStartDate = null, 
            DateTime? creationEndDate = null, 
            string sorting = null,
            int maxResultCount = int.MaxValue, 
            int skipCount = 0, 
            CancellationToken cancellationToken = default
            )
        {
            var query = await GetListQueryAsync(
                filter, 
                entityType, 
                entityId, 
                repliedCommentId, 
                creatorId, 
                creationStartDate, 
                creationEndDate);

            return await query.OrderBy(sorting ?? "creationTime desc")
                              .PageBy(skipCount, maxResultCount)
                              .ToListAsync(GetCancellationToken(cancellationToken));
        }

        public async Task<long> GetCountAsync(
            string filter = null, 
            string entityType = null, 
            string entityId = null,
            Guid? repliedCommentId = null, 
            Guid? creatorId = null, 
            DateTime? creationStartDate = null,
            DateTime? creationEndDate = null, 
            CancellationToken cancellationToken = default
            )
        {
            var query = await GetListQueryAsync(
                filter, 
                entityType, 
                entityId, 
                repliedCommentId, 
                creatorId, 
                creationStartDate, 
                creationEndDate);

            return await query.LongCountAsync(GetCancellationToken(cancellationToken));
        }

        public async Task<List<CommentWithAuthorQueryResultItem>> GetListWithAuthorsAsync(
            string entityType,
            string entityId,
            CancellationToken cancellationToken = default)
        {
            Check.NotNullOrWhiteSpace(entityType, nameof(entityType));
            Check.NotNullOrWhiteSpace(entityId, nameof(entityId));

            var query = from comment in (await GetDbSetAsync())
                join user in (await GetDbContextAsync()).Set<CmsUser>() on comment.CreatorId equals user.Id
                where entityType == comment.EntityType && entityId == comment.EntityId
                orderby comment.CreationTime
                select new CommentWithAuthorQueryResultItem
                {
                    Comment = comment,
                    Author = user
                };

            return await query.ToListAsync(GetCancellationToken(cancellationToken));
        }

        public async Task DeleteWithRepliesAsync(
            Comment comment,
            CancellationToken cancellationToken = default)
        {
            var replies = await (await GetDbSetAsync())
                .Where(x => x.RepliedCommentId == comment.Id)
                .ToListAsync(GetCancellationToken(cancellationToken));

            foreach (var reply in replies)
            {
                await DeleteAsync(
                    reply,
                    cancellationToken: GetCancellationToken(cancellationToken)
                );
            }

            await DeleteAsync(comment, cancellationToken: GetCancellationToken(cancellationToken));
        }

        protected virtual async Task<IQueryable<Comment>> GetListQueryAsync(
            string filter = null, 
            string entityType = null, 
            string entityId = null,
            Guid? repliedCommentId = null, 
            Guid? creatorId = null, 
            DateTime? creationStartDate = null,
            DateTime? creationEndDate = null
            )
        {
            return (await GetDbSetAsync())
                .WhereIf(!filter.IsNullOrWhiteSpace(), c => c.Text.Contains(filter))
                .WhereIf(!entityType.IsNullOrWhiteSpace(), c => c.EntityType == entityType)
                .WhereIf(!entityId.IsNullOrWhiteSpace(), c => c.EntityId == entityId)
                .WhereIf(repliedCommentId.HasValue, c => c.RepliedCommentId == repliedCommentId)
                .WhereIf(creatorId.HasValue, c => c.CreatorId == creatorId)
                .WhereIf(creationStartDate.HasValue, c => c.CreationTime >= creationStartDate)
                .WhereIf(creationEndDate.HasValue, c => c.CreationTime <= creationEndDate);
        }
    }
}
