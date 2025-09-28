using System.Collections.Generic;

namespace InstagramAuto.Client.Models
{
    public class PaginatedMediasDto
    {
        public List<MediaItemDto> Items { get; set; }
        public PaginationMetaDto Meta { get; set; }
    }
}
