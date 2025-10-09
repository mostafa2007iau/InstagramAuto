using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramAuto.Client.Models;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Services
{
    /// <summary>
    /// Persian: ????? API ???? ?????? ????????? ?? ???
    /// English: API service for fetching comments of a post
    /// </summary>
    public class CommentService
    {
        private readonly InstagramAuto.Client.InstagramAutoClient _apiClient;
        
        public CommentService(InstagramAuto.Client.InstagramAutoClient apiClient)
        {
            _apiClient = apiClient;
        }

        /// <summary>
        /// Persian: ?????? ????????? ?? ??? ?? ???????? ?? ?????????
        /// English: Get paginated comments for a post
        /// </summary>
        public async Task<InstagramAuto.Client.PaginatedComments> GetCommentsAsync(string mediaId, string cursor = null)
        {
            var response = await _apiClient.GetCommentsAsync(mediaId, cursor: cursor);
            return response;
        }
    }
}
