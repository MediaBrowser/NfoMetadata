﻿using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using NfoMetadata.Configuration;
using NfoMetadata.Savers;
using System;

namespace NfoMetadata
{
    public class EntryPoint : IServerEntryPoint
    {
        private readonly IUserDataManager _userDataManager;
        private readonly ILogger _logger;
        private readonly ILibraryManager _libraryManager;
        private readonly IProviderManager _providerManager;
        private readonly IConfigurationManager _config;

        public EntryPoint(IUserDataManager userDataManager, ILibraryManager libraryManager, ILogger logger, IProviderManager providerManager, IConfigurationManager config)
        {
            _userDataManager = userDataManager;
            _libraryManager = libraryManager;
            _logger = logger;
            _providerManager = providerManager;
            _config = config;
        }

        public void Run()
        {
            _userDataManager.UserDataSaved += _userDataManager_UserDataSaved;
        }

        void _userDataManager_UserDataSaved(object sender, UserDataSaveEventArgs e)
        {
            if (e.SaveReason == UserDataSaveReason.PlaybackFinished || e.SaveReason == UserDataSaveReason.TogglePlayed || e.SaveReason == UserDataSaveReason.UpdateUserRating)
            {
                if (!string.IsNullOrWhiteSpace(_config.GetNfoConfiguration().UserId))
                {
                    SaveMetadataForItem(e.Item, ItemUpdateType.MetadataDownload);
                }
            }
        }

        public void Dispose()
        {
            _userDataManager.UserDataSaved -= _userDataManager_UserDataSaved;
        }

        private void SaveMetadataForItem(BaseItem item, ItemUpdateType updateReason)
        {
            if (!item.IsFileProtocol)
            {
                return;
            }

            if (!item.IsSaveLocalMetadataEnabled(_libraryManager.GetLibraryOptions(item)))
            {
                return;
            }

            try
            {
                _providerManager.SaveMetadata(item, updateReason, new[] { BaseNfoSaver.SaverName });
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error saving metadata for {0}", ex, item.Path ?? item.Name);
            }
        }
    }
}
