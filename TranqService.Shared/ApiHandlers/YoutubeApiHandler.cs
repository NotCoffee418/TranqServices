﻿using Google.Apis.YouTube.v3.Data;

namespace TranqService.Shared.ApiHandlers
{
    public class YoutubeApiHandler : IYoutubeApiHandler
    {
        IConfig _config;

        private YouTubeService? _youTubeService;

        public YoutubeApiHandler(IConfig config)
        {
            _config = config;

            Authenticate();
        }

        private void Authenticate()
        {
            _youTubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApplicationName = this.GetType().ToString(),
                ApiKey = _config.YoutubeApiKey
            });
        }

        /// <summary>
        /// Gets all items in a youtube playlist as YoutubeVideoModel
        /// </summary>
        /// <returns></returns>
        public async Task<List<YoutubeVideoModel>> GetAllPlaylistItemsAsync(string playlistId)
        {
            // Prepare request
            Repeatable<string> part = new Repeatable<string>(new string[] { "id", "snippet" });
            var request = _youTubeService.PlaylistItems.List(part);
            request.PlaylistId = playlistId;
            request.MaxResults = 50;

            List<YoutubeVideoModel> result = new();

            // Get all items in playlist
            PlaylistItemListResponse? response = null;
            do
            {
                // Define page token and make request
                if (response != null)
                    request.PageToken = response.NextPageToken;
                response = await request.ExecuteAsync();

                // Add to result
                result.AddRange(response.Items
                    .Select(x => new YoutubeVideoModel()
                    {
                        VideoGuid = x.Id,
                        Name = x.Snippet.Title,
                        Uploader = x.Snippet.ChannelTitle,
                        PlaylistGuid = playlistId
                    }));
            } while (response.NextPageToken != null);

            return result;
        }
    }
}
