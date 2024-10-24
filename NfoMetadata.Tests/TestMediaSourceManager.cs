using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Dlna;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.MediaInfo;

namespace NfoMetadata.Tests
{
    public class TestMediaSourceManager : IMediaSourceManager
    {
        public MediaProtocol GetPathProtocol(ReadOnlySpan<char> path)
        {
            return MediaBrowser.Model.MediaInfo.MediaProtocol.File;
        }

        #region Not Implemented

        public Task AddMediaInfoWithProbe(MediaSourceInfo mediaSource, bool isAudio, bool addProbeDelay, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task AddMediaInfoWithProbeSafe(MediaSourceInfo mediaSource, bool isAudio, bool addProbeDelay, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void AddParts(IEnumerable<IMediaSourceProvider> providers)
        {
            throw new NotImplementedException();
        }

        public Task CloseLiveStream(string id)
        {
            throw new NotImplementedException();
        }

        public ILiveStream GetLiveStreamInfo(string id)
        {
            throw new NotImplementedException();
        }

        public ILiveStream GetLiveStreamInfoByUniqueId(string uniqueId)
        {
            throw new NotImplementedException();
        }

        public MediaSourceInfo GetLiveStreamMediaSource(string id)
        {
            throw new NotImplementedException();
        }

        public Task<MediaSourceInfo> GetMediaSource(BaseItem item, string mediaSourceId, string liveStreamId, bool enablePathSubstitution, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public List<MediaStream> GetMediaStreams(BaseItem item)
        {
            throw new NotImplementedException();
        }

        public List<MediaStream> GetMediaStreams(long itemId)
        {
            throw new NotImplementedException();
        }

        public List<MediaStream> GetMediaStreams(MediaStreamQuery query)
        {
            throw new NotImplementedException();
        }

        public Task<List<MediaSourceInfo>> GetPlayackMediaSources(BaseItem item, User user, bool allowMediaProbe, bool enablePathSubstitution, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<MediaSourceInfo>> GetPlayackMediaSources(BaseItem item, User user, bool allowMediaProbe, string probeMediaSourceId, bool enablePathSubstitution, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<MediaSourceInfo>> GetPlayackMediaSources(BaseItem item, User user, bool allowMediaProbe, string probeMediaSourceId, bool enablePathSubstitution, DeviceProfile deviceProfile, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public List<MediaSourceInfo> GetStaticMediaSources(BaseItem item, bool enablePathSubstitution, DeviceProfile deviceProfile, User user = null)
        {
            throw new NotImplementedException();
        }

        public List<MediaSourceInfo> GetStaticMediaSources(BaseItem item, bool enableAlternateMediaSources, bool enablePathSubstitution, LibraryOptions libraryOptions, DeviceProfile deviceProfile, User user = null)
        {
            throw new NotImplementedException();
        }

        public void NormalizeMediaStreams(List<MediaStream> streams)
        {
            throw new NotImplementedException();
        }

        public Task<LiveStreamResponse> OpenLiveStream(LiveStreamRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<LiveStreamResponse, ILiveStream>> OpenLiveStreamInternal(LiveStreamRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void SetDefaultAudioAndSubtitleStreamIndexes(BaseItem item, MediaSourceInfo[] sources, User user)
        {
            throw new NotImplementedException();
        }

        public bool SupportsDirectStream(ReadOnlySpan<char> path, MediaProtocol protocol)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
