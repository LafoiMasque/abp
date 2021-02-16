﻿using System;
using Volo.Abp.Application.Dtos;

namespace Volo.CmsKit.Admin.Comments
{
    public class CommentGetListInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
        
        public string EntityType { get; set; }

        public string EntityId { get; set; }

        public string Text { get; set; }

        public Guid? RepliedCommentId { get; set; }

        public Guid CreatorId { get; set; }

        public DateTime CreationStartDate { get; set; }
        
        public DateTime CreationEndDate { get; set; }
    }
}