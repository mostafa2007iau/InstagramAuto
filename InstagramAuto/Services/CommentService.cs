using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramAuto.Client.Models;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Services
{
    /// <summary>
    /// Persian:
    ///   ????? API ???? ?????? ????????? ?? ???.
    /// English:
    ///   API service for fetching comments of a post.
    /// </summary>
    public class CommentService
    {
        private readonly IInstagramAutoClient _apiClient;
        public CommentService(IInstagramAutoClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<PaginatedComments> GetCommentsAsync(string mediaId, string cursor = null)
        {
            // This method is now handled directly in AuthService, so throw NotImplementedException
            throw new System.NotImplementedException();
        }
    }

    public class PaginatedComments
    {
        public List<CommentItem> Items { get; set; }
        public PaginationMetaDto Meta { get; set; }
    }
}
