define(['baseView', 'loading', 'globalize', 'emby-input', 'emby-button', 'emby-select', 'emby-checkbox', 'emby-scroller'], function (BaseView, loading, globalize) {
    'use strict';

    function loadPage(page, config) {

        ApiClient.getUsers().then(function (users) {
            var html = '<option value="" selected="selected">' + globalize.translate('None') + '</option>';

            html += users.map(function (user) {
                return '<option value="' + user.Id + '">' + user.Name + '</option>';
            }).join('');

            var selectUser = page.querySelector('.selectUser');
            selectUser.innerHTML = html;
            selectUser.value = config.UserIdForUserData || '';

            page.querySelector('.selectReleaseDateFormat').value = config.ReleaseDateFormat;

            page.querySelector('.chkSaveImagePaths').checked = config.SaveImagePathsInNfoFiles;
            page.querySelector('.chkEnableExtraThumbs').checked = config.EnableExtraThumbsDuplication;
            page.querySelector('.chkPreferMovieNfo').checked = config.PreferMovieNfo;

            loading.hide();
        });
    }

    function onSubmit(e) {

        e.preventDefault();

        loading.show();

        var form = this;

        ApiClient.getNamedConfiguration("xbmcmetadata").then(function (config) {

            config.UserIdForUserData = form.querySelector('.selectUser').value || null;
            config.ReleaseDateFormat = form.querySelector('.selectReleaseDateFormat').value;

            config.SaveImagePathsInNfoFiles = form.querySelector('.chkSaveImagePaths').checked;
            config.EnableExtraThumbsDuplication = form.querySelector('.chkEnableExtraThumbs').checked;
            config.PreferMovieNfo = form.querySelector('.chkPreferMovieNfo').checked;

            ApiClient.updateNamedConfiguration("xbmcmetadata", config).then(Dashboard.processServerConfigurationUpdateResult);
        });

        // Disable default form submission
        return false;
    }

    function getConfig() {

        return ApiClient.getNamedConfiguration("xbmcmetadata");
    }

    function View(view, params) {
        BaseView.apply(this, arguments);

        view.querySelector('form').addEventListener('submit', onSubmit);
    }

    Object.assign(View.prototype, BaseView.prototype);

    View.prototype.onResume = function (options) {

        BaseView.prototype.onResume.apply(this, arguments);

        loading.show();

        var page = this.view;

        getConfig().then(function (response) {

            loadPage(page, response);
        });
    };

    return View;
});
