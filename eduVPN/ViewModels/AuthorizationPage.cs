﻿/*
    eduVPN - End-user friendly VPN

    Copyright: 2017, The Commons Conservancy eduVPN Programme
    SPDX-License-Identifier: GPL-3.0+
*/

using eduOAuth;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Web;
using System.Windows.Input;

namespace eduVPN.ViewModels
{
    /// <summary>
    /// Authorization wizard page
    /// </summary>
    public class AuthorizationPage : ConnectWizardPage
    {
        #region Fields

        /// <summary>
        /// OAuth pending authorization grant
        /// </summary>
        private AuthorizationGrant _authorization_grant;

        /// <summary>
        /// Registered client redirect callback URI (endpoint)
        /// </summary>
        private const string _redirect_endpoint = "org.eduvpn.app:/api/callback";

        #endregion

        #region Properties

        /// <summary>
        /// Retry authorization command
        /// </summary>
        public ICommand Retry
        {
            get
            {
                if (_retry == null)
                    _retry = new DelegateCommand(TriggerAuthorization);
                return _retry;
            }
        }
        private ICommand _retry;

        public ICommand Authorize
        {
            get
            {
                if (_authorize == null)
                    _authorize = new DelegateCommand<string>(
                        // execute
                        async param =>
                        {
                            Error = null;
                            TaskCount++;
                            try
                            {
                                var api = Parent.AuthenticatingInstance.GetEndpointsAsync(ConnectWizard.Abort.Token);

                                // Process response and get access token.
                                Parent.AccessToken = await _authorization_grant.ProcessResponseAsync(
                                    HttpUtility.ParseQueryString(new Uri(param).Query),
                                    (await api).TokenEndpoint,
                                    null,
                                    ConnectWizard.Abort.Token);

                                // Save the access token.
                                Properties.Settings.Default.AccessTokens[(await api).AuthorizationEndpoint.AbsoluteUri] = Parent.AccessToken.ToBase64String();

                                // Go to profile selection page.
                                if (Parent.ConnectingInstance == null)
                                    Parent.CurrentPage = Parent.InstanceAndProfileSelectPage;
                                else
                                    Parent.CurrentPage = Parent.ProfileSelectPage;
                            }
                            catch (Exception ex) { Error = ex; }
                            finally { TaskCount--; }
                        },

                        // canExecute
                        param =>
                        {
                            Uri uri;

                            // URI must be:
                            // - non-NULL
                            if (param == null) return false;
                            // - Valid URI (parsable)
                            try { uri = new Uri(param); }
                            catch (Exception) { return false; }
                            // - Must match the redirect endpoint provided in request.
                            if (uri.Scheme + ":" + uri.AbsolutePath != _redirect_endpoint) return false;

                            return true;
                        });
                return _authorize;
            }
        }
        private ICommand _authorize;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs an authorization wizard page
        /// </summary>
        /// <param name="parent"></param>
        public AuthorizationPage(ConnectWizard parent) :
            base(parent)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invokes client authorization process in the browser.
        /// </summary>
        private void TriggerAuthorization()
        {
            Error = null;
            TaskCount++;
            try
            {
                // Open authorization request in the browser.
                _authorization_grant = new AuthorizationGrant()
                {
                    AuthorizationEndpoint = Parent.AuthenticatingInstance.GetEndpoints(ConnectWizard.Abort.Token).AuthorizationEndpoint,
                    RedirectEndpoint = new Uri(_redirect_endpoint),
                    ClientID = "org.eduvpn.app",
                    Scope = new List<string>() { "config" },
                    CodeChallengeAlgorithm = AuthorizationGrant.CodeChallengeAlgorithmType.S256
                };
                System.Diagnostics.Process.Start(_authorization_grant.AuthorizationURI.ToString());
            }
            catch (Exception ex) { Error = ex; }
            finally { TaskCount--; }
        }

        public override void OnActivate()
        {
            base.OnActivate();

            TriggerAuthorization();
        }

        protected override void DoNavigateBack()
        {
            if (Parent.InstanceList is Models.InstanceInfoFederatedList)
                Parent.CurrentPage = Parent.AccessTypePage;
            else if (Parent.AuthenticatingInstance.IsCustom)
                Parent.CurrentPage = Parent.CustomInstancePage;
            else
                switch (Parent.AccessType)
                {
                    case AccessType.SecureInternet: Parent.CurrentPage = Parent.SecureInternetSelectPage; break;
                    case AccessType.InstituteAccess: Parent.CurrentPage = Parent.InstituteAccessSelectPage; break;
                }
        }

        protected override bool CanNavigateBack()
        {
            if (Parent.InstanceList is Models.InstanceInfoFederatedList)
                return true;
            else if (Parent.AuthenticatingInstance.IsCustom)
                return true;
            else
                switch (Parent.AccessType)
                {
                    case AccessType.SecureInternet: return true;
                    case AccessType.InstituteAccess: return true;
                    default: return false;
                }
        }

        #endregion
    }
}
