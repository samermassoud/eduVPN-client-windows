﻿/*
    eduVPN - VPN for education and research

    Copyright: 2017-2023 The Commons Conservancy
    SPDX-License-Identifier: GPL-3.0+
*/

using eduVPN.Models;
using eduVPN.ViewModels.Pages;
using Prism.Commands;
using Prism.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace eduVPN.ViewModels.Windows
{
    /// <summary>
    /// Connect wizard
    /// </summary>
    public class ConnectWizard : Window, IDisposable
    {
        #region Fields

        /// <summary>
        /// Stack of displayed popup pages
        /// </summary>
        private readonly List<ConnectWizardPopupPage> PopupPages = new List<ConnectWizardPopupPage>();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly byte[] Entropy =
        {
            0x55, 0x03, 0xde, 0x6b, 0x8a, 0x6a, 0xf9, 0x28, 0xd5, 0xdd, 0xaf, 0xf8, 0x3a, 0x89, 0xf9, 0xf3,
            0x5e, 0x1c, 0xe6, 0x7d, 0x8b, 0x70, 0x2c, 0xb8, 0x64, 0xf4, 0x1e, 0xf0, 0x3f, 0x33, 0x52, 0x91,
            0x3d, 0x9e, 0x63, 0x4e, 0x7e, 0xd5, 0x07, 0x7b, 0x24, 0x91, 0x2a, 0x3d, 0x0c, 0x6a, 0x43, 0x47,
            0xe6, 0xdb, 0xc7, 0xad, 0x1a, 0x94, 0x93, 0x04, 0x3b, 0xf0, 0xa7, 0x48, 0xe2, 0x55, 0x28, 0x3a,
        };

        /// <summary>
        /// eduvpn-common cookie of operation in progress
        /// </summary>
        public Engine.Cookie OperationInProgress;

        #endregion

        #region Properties

        /// <summary>
        /// Copyright notice
        /// </summary>
        public string Copyright => (Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyCopyrightAttribute)) as AssemblyCopyrightAttribute)?.Copyright;

        /// <summary>
        /// Executing assembly version
        /// </summary>
        public Version Version
        {
            get
            {
                var ver = Assembly.GetExecutingAssembly()?.GetName()?.Version;
                return
                    ver.Revision != 0 ? new Version(ver.Major, ver.Minor, ver.Build, ver.Revision) :
                    ver.Build != 0 ? new Version(ver.Major, ver.Minor, ver.Build) :
                        new Version(ver.Major, ver.Minor);
            }
        }

        /// <summary>
        /// Build timestamp
        /// </summary>
        public DateTimeOffset Build =>
                // The Builtin class is implemented in Builtin target in Default.targets.
                new DateTimeOffset(Builtin.CompileTime, TimeSpan.Zero);

        /// <summary>
        /// Navigate to a pop-up page command
        /// </summary>
        public DelegateCommand<ConnectWizardPopupPage> NavigateTo
        {
            get
            {
                if (_NavigateTo == null)
                    _NavigateTo = new DelegateCommand<ConnectWizardPopupPage>(
                        page =>
                        {
                            var displayPagePrev = DisplayPage;
                            var removed = PopupPages.Remove(page);
                            PopupPages.Add(page);
                            if (!removed) page.OnActivate();
                            if (displayPagePrev != DisplayPage)
                                RaisePropertyChanged(nameof(DisplayPage));
                        });
                return _NavigateTo;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private DelegateCommand<ConnectWizardPopupPage> _NavigateTo;

        /// <summary>
        /// Navigate back from a pop-up page command
        /// </summary>
        public DelegateCommand<ConnectWizardPopupPage> NavigateBack
        {
            get
            {
                if (_NavigateBack == null)
                    _NavigateBack = new DelegateCommand<ConnectWizardPopupPage>(
                        page =>
                        {
                            var displayPagePrev = DisplayPage;
                            PopupPages.Remove(page);
                            if (displayPagePrev != DisplayPage)
                                RaisePropertyChanged(nameof(DisplayPage));
                        });
                return _NavigateBack;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private DelegateCommand<ConnectWizardPopupPage> _NavigateBack;

        /// <summary>
        /// Occurs when auto-reconnection failed.
        /// </summary>
        /// <remarks>Sender is the connection wizard <see cref="ConnectWizard"/>.</remarks>
        public event EventHandler<AutoReconnectFailedEventArgs> AutoReconnectFailed;

        /// <summary>
        /// Occurs when application should quit.
        /// </summary>
        /// <remarks>Sender is the connection wizard <see cref="ConnectWizard"/>.</remarks>
        public event EventHandler QuitApplication;

        #region Pages

        /// <summary>
        /// The page the wizard is currently displaying
        /// </summary>
        public ConnectWizardPage DisplayPage => PopupPages.Count > 0 ? (ConnectWizardPage)PopupPages.Last() : _CurrentPage;

        /// <summary>
        /// The page the wizard should be displaying (if no pop-up page)
        /// </summary>
        public ConnectWizardStandardPage CurrentPage
        {
            get => _CurrentPage;
            set
            {
                if (SetProperty(ref _CurrentPage, value))
                {
                    _CurrentPage.OnActivate();
                    if (PopupPages.Count <= 0)
                        RaisePropertyChanged(nameof(DisplayPage));
                }
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ConnectWizardStandardPage _CurrentPage;

        /// <summary>
        /// Page to add another server
        /// </summary>
        public ConnectWizardStandardPage AddAnotherPage => Properties.Settings.Default.Discovery ? (ConnectWizardStandardPage)SearchPage : SelectOwnServerPage;

        /// <summary>
        /// The first page of the wizard
        /// </summary>
        public ConnectWizardStandardPage StartingPage
        {
            get
            {
                if (HomePage.InstituteAccessServers.Count != 0 ||
                    HomePage.SecureInternetServers.Count != 0 ||
                    HomePage.OwnServers.Count != 0)
                    return HomePage;

                return AddAnotherPage;
            }
        }

        /// <summary>
        /// Authorization wizard page
        /// </summary>
        public AuthorizationPage AuthorizationPage
        {
            get
            {
                if (_AuthorizationPage == null)
                    _AuthorizationPage = new AuthorizationPage(this);
                return _AuthorizationPage;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private AuthorizationPage _AuthorizationPage;

        /// <summary>
        /// Search wizard page
        /// </summary>
        public SearchPage SearchPage
        {
            get
            {
                if (_SearchPage == null)
                    _SearchPage = new SearchPage(this);
                return _SearchPage;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private SearchPage _SearchPage;

        /// <summary>
        /// Select own server page
        /// </summary>
        public SelectOwnServerPage SelectOwnServerPage
        {
            get
            {
                if (_SelectOwnServerPage == null)
                    _SelectOwnServerPage = new SelectOwnServerPage(this);
                return _SelectOwnServerPage;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private SelectOwnServerPage _SelectOwnServerPage;

        /// <summary>
        /// Home wizard page
        /// </summary>
        public HomePage HomePage
        {
            get
            {
                if (_HomePage == null)
                    _HomePage = new HomePage(this);
                return _HomePage;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private HomePage _HomePage;

        /// <summary>
        /// Select secure internet connecting server wizard page
        /// </summary>
        public SelectSecureInternetCountryPage SelectSecureInternetCountryPage
        {
            get
            {
                if (_SelectSecureInternetCountryPage == null)
                    _SelectSecureInternetCountryPage = new SelectSecureInternetCountryPage(this);
                return _SelectSecureInternetCountryPage;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private SelectSecureInternetCountryPage _SelectSecureInternetCountryPage;

        /// <summary>
        /// Status wizard page
        /// </summary>
        public ConnectionPage ConnectionPage
        {
            get
            {
                if (_ConnectionPage == null)
                    _ConnectionPage = new ConnectionPage(this);
                return _ConnectionPage;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ConnectionPage _ConnectionPage;

        /// <summary>
        /// Settings wizard page
        /// </summary>
        public SettingsPage SettingsPage
        {
            get
            {
                if (_SettingsPage == null)
                    _SettingsPage = new SettingsPage(this);
                return _SettingsPage;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private SettingsPage _SettingsPage;

        /// <summary>
        /// About wizard page
        /// </summary>
        public AboutPage AboutPage
        {
            get
            {
                if (_AboutPage == null)
                    _AboutPage = new AboutPage(this);
                return _AboutPage;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private AboutPage _AboutPage;

        /// <summary>
        /// Self-update prompt wizard page
        /// </summary>
        public SelfUpdatePromptPage SelfUpdatePromptPage
        {
            get
            {
                if (_SelfUpdatePromptPage == null)
                    _SelfUpdatePromptPage = new SelfUpdatePromptPage(this);
                return _SelfUpdatePromptPage;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private SelfUpdatePromptPage _SelfUpdatePromptPage;

        /// <summary>
        /// Self-update progress wizard page
        /// </summary>
        public SelfUpdateProgressPage SelfUpdateProgressPage
        {
            get
            {
                if (_SelfUpdateProgressPage == null)
                    _SelfUpdateProgressPage = new SelfUpdateProgressPage(this);
                return _SelfUpdateProgressPage;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private SelfUpdateProgressPage _SelfUpdateProgressPage;

        /// <summary>
        /// Please wait wizard page
        /// </summary>
        public PleaseWaitPage PleaseWaitPage
        {
            get
            {
                if (_PleaseWaitPage == null)
                    _PleaseWaitPage = new PleaseWaitPage(this);
                return _PleaseWaitPage;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private PleaseWaitPage _PleaseWaitPage;

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs the wizard
        /// </summary>
        public ConnectWizard()
        {
            Engine.Callback += Engine_Callback;
            Engine.SetToken += Engine_SetToken;
            Engine.GetToken += Engine_GetToken;
            Engine.Register();

            var actions = new List<KeyValuePair<Action, int>>
            {
                new KeyValuePair<Action, int>(() =>
                    {
                        try { VPN.OpenVPNSession.PurgeOldLogs(); } catch { }
                        try { VPN.WireGuardSession.PurgeOldLogs(); } catch { }
                    },
                    24 * 60 * 60 * 1000), // Repeat every 24 hours

                new KeyValuePair<Action, int>(() =>
                    {
                        var ts = CGo.GetLastUpdateTimestamp(Abort.Token);
                        if (DateTimeOffset.UtcNow - ts > TimeSpan.FromDays(60))
                            throw new Exception(Resources.Strings.WarningWindowsUpdatesStalled);
                    },
                    24 * 60 * 60 * 1000) // Repeat every 24 hours
            };

            bool migrate = (Properties.Settings.Default.SettingsVersion & 0x2) == 0;

            ICollection<Uri> iaSrvList = new Xml.UriList();
            string siOrgId = null;

            if (Properties.Settings.Default.Discovery)
            {
                iaSrvList = Properties.SettingsEx.Default.InstituteAccessServers;
                if (iaSrvList != null)
                    Trace.TraceInformation("Adding preconfigured Institute Access servers {0}", string.Join(", ", iaSrvList));
                else
                    iaSrvList = new Xml.UriList();
                if (migrate && Properties.Settings.Default.GetPreviousVersion("InstituteAccessServers") is Xml.UriList instituteAccessServers)
                {
                    Trace.TraceInformation("Migrating Institute Access servers {0}", string.Join(", ", instituteAccessServers));
                    foreach (var srv in instituteAccessServers)
                        iaSrvList.Add(srv);
                }
                // Skip servers that are already on our Institute Access list. Maybe server discovery won't be required at all.
                foreach (var u in iaSrvList.Where(uri => HomePage.InstituteAccessServers.FirstOrDefault(srv => srv.Id == uri.AbsoluteUri) != null).ToList())
                    iaSrvList.Remove(u);

                siOrgId = Properties.SettingsEx.Default.SecureInternetOrganization;
                if (siOrgId != null)
                    Trace.TraceInformation("Using preconfigured Secure Internet organization {0}", siOrgId);
                else if (migrate && Properties.Settings.Default.GetPreviousVersion("SecureInternetOrganization") is string secureInternetOrganization &&
                    !string.IsNullOrEmpty(secureInternetOrganization))
                {
                    Trace.TraceInformation("Migrating Secure Internet organization {0}", secureInternetOrganization);
                    siOrgId = secureInternetOrganization;
                }
                // Skip organization that is already set for Secure Internet. Maybe organization discovery won't be required at all.
                if (HomePage.SecureInternetServers.FirstOrDefault(srv => srv.Id == siOrgId) != null ||
                    siOrgId == "" && HomePage.SecureInternetServers.Count == 0)
                    siOrgId = null;
            }

            var ownSrvList = new Xml.UriList();
            if (migrate && Properties.Settings.Default.GetPreviousVersion("OwnServers") is Xml.UriList ownServers)
            {
                Trace.TraceInformation("Migrating own servers {0}", string.Join(", ", ownServers));
                foreach (var srv in ownServers)
                    ownSrvList.Add(srv);
            }
            foreach (var u in ownSrvList.Where(uri => HomePage.OwnServers.FirstOrDefault(srv => srv.Id == uri.AbsoluteUri) != null).ToList())
                ownSrvList.Remove(u);

            if (iaSrvList.Count > 0 ||
                siOrgId != null ||
                ownSrvList.Count > 0)
            {
                CurrentPage = PleaseWaitPage;
                actions.Add(new KeyValuePair<Action, int>(() =>
                    {
                        using (var cookie = new Engine.CancellationTokenCookie(Abort.Token))
                        {
                            Task<List<object>> serverDiscovery = null;
                            if (iaSrvList.Count > 0 || siOrgId != "" && siOrgId != null)
                                serverDiscovery = Task.Run(() => eduJSON.Parser.GetValue<List<object>>(
                                    eduJSON.Parser.Parse(
                                        Engine.DiscoServers(cookie),
                                        Abort.Token) as Dictionary<string, object>,
                                    "server_list"));
                            Task<List<object>> orgDiscovery = null;
                            if (siOrgId != "" && siOrgId != null)
                                orgDiscovery = Task.Run(() => eduJSON.Parser.GetValue<List<object>>(
                                    eduJSON.Parser.Parse(
                                        Engine.DiscoOrganizations(cookie),
                                        Abort.Token) as Dictionary<string, object>,
                                    "organization_list"));

                            //Abort.Token.WaitHandle.WaitOne(5000); // Mock slow settings import

                            List<object> serverList = null;
                            if (serverDiscovery != null)
                            {
                                serverDiscovery.Wait(Abort.Token);
                                serverList = serverDiscovery.Result;
                            }
                            List<object> orgList = null;
                            if (orgDiscovery != null)
                            {
                                orgDiscovery.Wait(Abort.Token);
                                orgList = orgDiscovery.Result;
                            }

                            if (iaSrvList.Count > 0)
                                foreach (var srv in iaSrvList)
                                {
                                    Abort.Token.ThrowIfCancellationRequested();
                                    try { Engine.AddServer(cookie, ServerType.InstituteAccess, srv.AbsoluteUri, true); }
                                    catch (OperationCanceledException) { throw; }
                                    catch { }
                                }

                            if (siOrgId == "")
                            {
                                if (eduJSON.Parser.Parse(Engine.ServerList(), Abort.Token) is Dictionary<string, object> obj &&
                                    obj.TryGetValue("secure_internet_server", out Dictionary<string, object> srvObj))
                                {
                                    var srv = new SecureInternetServer(srvObj);
                                    try { Engine.RemoveServer(ServerType.SecureInternet, srv.Id); }
                                    catch (OperationCanceledException) { throw; }
                                    catch { }
                                }
                            }
                            else if (siOrgId != null)
                            {
                                try
                                {
                                    Engine.AddServer(cookie, ServerType.SecureInternet, siOrgId, true);
                                    if (Properties.Settings.Default.GetPreviousVersion("SecureInternetConnectingServer") is Uri uri)
                                    {
                                        if (serverList.FirstOrDefault(obj =>
                                                obj is Dictionary<string, object> obj2 && obj2.TryGetValue("base_url", out string base_url) && new Uri(base_url).Equals(uri)) is Dictionary<string, object> obj3 &&
                                            obj3.TryGetValue("country_code", out string country_code))
                                        {
                                            Engine.SetSecureInternetLocation(cookie, country_code);
                                            if (Properties.Settings.Default.LastSelectedServer == uri.AbsoluteUri)
                                                Properties.Settings.Default.LastSelectedServer = siOrgId;
                                        }
                                    }

                                    // Rekey all OAuth tokens to use organization ID instead of authenticating server base URI as the key.
                                    lock (Properties.Settings.Default.AccessTokenCache2)
                                    {
                                        foreach (var obj in orgList.Select(item => item as Dictionary<string, object>))
                                        {
                                            Abort.Token.ThrowIfCancellationRequested();
                                            if (eduJSON.Parser.GetValue(obj, "org_id", out string org_id) && Uri.TryCreate(org_id, UriKind.Absolute, out var orgId) && orgId.AbsoluteUri == siOrgId &&
                                                eduJSON.Parser.GetValue(obj, "secure_internet_home", out string secure_internet_home) && Uri.TryCreate(secure_internet_home, UriKind.Absolute, out var serverId) &&
                                                Properties.Settings.Default.AccessTokenCache2.TryGetValue(serverId.AbsoluteUri, out var value))
                                            {
                                                Properties.Settings.Default.AccessTokenCache2[orgId.AbsoluteUri] = value;
                                                Properties.Settings.Default.AccessTokenCache2.Remove(serverId.AbsoluteUri);
                                            }
                                        }
                                    }
                                }
                                catch (OperationCanceledException) { throw; }
                                catch { }
                            }

                            foreach (var srv in ownSrvList)
                            {
                                Abort.Token.ThrowIfCancellationRequested();
                                try { Engine.AddServer(cookie, ServerType.Own, srv.AbsoluteUri, true); }
                                catch (OperationCanceledException) { throw; }
                                catch { }
                            }
                        }

                        // Don't set SettingsVersion flag if user cancelled and we missed it somehow.
                        Abort.Token.ThrowIfCancellationRequested();
                        Properties.Settings.Default.SettingsVersion |= 0x2;

                        // eduvpn-common does not do callback after servers are added. Do the bookkeeping manually.
                        Engine_Callback(this, new Engine.CallbackEventArgs(Engine.State.Deregistered, Engine.State.NoServer, null));
                    }, 0)); // No repeat
            }

            if (Properties.SettingsEx.Default.SelfUpdateDiscovery?.Uri != null)
                actions.Add(new KeyValuePair<Action, int>(
                    SelfUpdatePromptPage.DiscoverVersions,
                    24 * 60 * 60 * 1000)); // Repeat every 24 hours

            foreach (var action in actions)
            {
                new Thread(() =>
                {
                    var random = new Random();
                    do
                    {
                        TryInvoke((Action)(() => TaskCount++));
                        try { action.Key(); }
                        catch (OperationCanceledException) { break; }
                        catch (Exception ex) { TryInvoke((Action)(() => Error = ex)); }
                        finally { TryInvoke((Action)(() => TaskCount--)); }
                    }
                    // Sleep for given time±10%, then retry.
                    while (action.Value != 0 && !Abort.Token.WaitHandle.WaitOne(random.Next(action.Value * 9 / 10, action.Value * 11 / 10)));
                }).Start();
            }

            //Abort.Token.WaitHandle.WaitOne(5000); // Mock slow wizard initialization
        }

        #endregion

        #region Methods

        private void Engine_Callback(object sender, Engine.CallbackEventArgs e)
        {
            Trace.TraceInformation("eduvpn-common state {0}", e.NewState);
            switch (e.NewState)
            {
                case Engine.State.NoServer:
                    {
                        var obj = eduJSON.Parser.Parse(Engine.ServerList(), Abort.Token) as Dictionary<string, object>;
                        TryInvoke((Action)(() =>
                        {
                            HomePage.LoadServers(obj);
                            CurrentPage = StartingPage;
                            if (ConnectionPage.Server == null)
                            {
                                var id = Properties.Settings.Default.LastSelectedServer;
                                Server srv =
                                    HomePage.InstituteAccessServers.FirstOrDefault(s => !s.Delisted && s.Id == id) ??
                                    HomePage.SecureInternetServers.FirstOrDefault(s => !s.Delisted && s.Id == id) ??
                                    HomePage.OwnServers.FirstOrDefault(s => s.Id == id);
                                if (srv != null)
                                    Connect(srv, true);
                            }
                        }));
                        e.Handled = true;
                    }
                    break;

                case Engine.State.AskLocation:
                    {
                        if (eduJSON.Parser.Parse(e.Data, Abort.Token) is Dictionary<string, object> obj)
                        {
                            var data = new AskLocationTransition(obj);
                            TryInvoke((Action)(() =>
                            {
                                SelectSecureInternetCountryPage.SetSecureInternetCountries(data.Countries);
                                CurrentPage = SelectSecureInternetCountryPage;
                            }));
                            e.Handled = true;
                        }
                    }
                    break;

                case Engine.State.OAuthStarted:
                    Process.Start(e.Data);
                    TryInvoke((Action)(() => CurrentPage = AuthorizationPage));
                    e.Handled = true;
                    break;

                case Engine.State.AskProfile:
                    {
                        if (eduJSON.Parser.Parse(e.Data, Abort.Token) is Dictionary<string, object> obj)
                        {
                            var data = new AskProfileTransition(obj);
                            TryInvoke((Action)(() =>
                            {
                                ConnectionPage.SetProfiles(data.Profiles);
                                CurrentPage = ConnectionPage;
                            }));
                            e.Handled = true;
                        }
                    }
                    break;

                case Engine.State.Disconnected:
                    TryInvoke((Action)(() => CurrentPage = ConnectionPage));
                    e.Handled = true;
                    break;

                case Engine.State.ChosenLocation:
                case Engine.State.LoadingServer:
                case Engine.State.ChosenServer:
                case Engine.State.RequestConfig:
                case Engine.State.ChosenProfile:
                    TryInvoke((Action)(() => CurrentPage = PleaseWaitPage));
                    e.Handled = true;
                    break;

                default:
                    // Silence "WARNING - transition not completed..." in the log.
                    e.Handled = true;
                    break;
            }
        }

        static private void Engine_GetToken(object sender, Engine.GetTokenEventArgs e)
        {
            lock (Properties.Settings.Default.AccessTokenCache2)
                if (Properties.Settings.Default.AccessTokenCache2.TryGetValue(e.Id, out var value))
                    e.Token = Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(value), Entropy, DataProtectionScope.CurrentUser));
        }

        static public void Engine_SetToken(object sender, Engine.SetTokenEventArgs e)
        {
            lock (Properties.Settings.Default.AccessTokenCache2)
                if (e.Token != null)
                    Properties.Settings.Default.AccessTokenCache2[e.Id] =
                        Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(e.Token), Entropy, DataProtectionScope.CurrentUser));
                else
                    Properties.Settings.Default.AccessTokenCache2.Remove(e.Id);
        }

        /// <summary>
        /// Connect to given server
        /// </summary>
        /// <param name="server">Server</param>
        /// <param name="startup">indicates that the client is auto-starting connection (unattended)</param>
        public async void Connect(Server server, bool startup = false)
        {
            // We have to set this to display server name on the connection page when/if user is presented with
            // a list of profiles to select one. It will be replaced by true server info provided by
            // Engine.CurrentServer() later.
            ConnectionPage.Server = server;

            Configuration config;
            Expiration expiration;
            try
            {
                using (OperationInProgress = new Engine.CancellationTokenCookie(Abort.Token))
                    (server, config, expiration) = await Task.Run(() =>
                    {
                        var cfg = new Configuration(eduJSON.Parser.Parse(
                            Engine.GetConfig(OperationInProgress, server.ServerType, server.Id, Properties.Settings.Default.OpenVPNPreferTCP, startup),
                            Abort.Token) as Dictionary<string, object>);

                        var srv = Server.Load(eduJSON.Parser.Parse(
                            Engine.CurrentServer(),
                            Abort.Token) as Dictionary<string, object>);

                        var exp = new Expiration(eduJSON.Parser.Parse(
                            Engine.ExpiryTimes(),
                            Abort.Token) as Dictionary<string, object>);

                        return (srv, cfg, exp);
                    });
            }
            catch
            {
                if (Properties.Settings.Default.LastSelectedServer == server.Id)
                {
                    AutoReconnectFailed?.Invoke(this, new AutoReconnectFailedEventArgs(server, server));
                    Properties.Settings.Default.LastSelectedServer = null;
                    ConnectionPage.Server = null;
                    return;
                }
                throw;
            }
            finally { OperationInProgress = null; }
            ConnectionPage.Server = server;
            ConnectionPage.ActivateSession(config, expiration);
            CurrentPage = ConnectionPage;
        }

        /// <summary>
        /// Add and connect to given server
        /// </summary>
        /// <param name="server">Server</param>
        public async void AddAndConnect(Server server)
        {
            // We have to set this to display server name on the connection page when/if user is presented with
            // a list of profiles to select one. It will be replaced by true server info provided by
            // Engine.CurrentServer() later.
            ConnectionPage.Server = server;

            Configuration config;
            Expiration expiration;
            try
            {
                using (OperationInProgress = new Engine.CancellationTokenCookie(Abort.Token))
                    (server, config, expiration) = await Task.Run(() =>
                    {
                        Engine.AddServer(OperationInProgress, server.ServerType, server.Id, false);

                        var cfg = new Configuration(eduJSON.Parser.Parse(
                            Engine.GetConfig(OperationInProgress, server.ServerType, server.Id, Properties.Settings.Default.OpenVPNPreferTCP, false),
                            Abort.Token) as Dictionary<string, object>);

                        var srv = Server.Load(eduJSON.Parser.Parse(
                            Engine.CurrentServer(),
                            Abort.Token) as Dictionary<string, object>);

                        var exp = new Expiration(eduJSON.Parser.Parse(
                            Engine.ExpiryTimes(),
                            Abort.Token) as Dictionary<string, object>);

                        return (srv, cfg, exp);
                    });
            }
            finally { OperationInProgress = null; }
            ConnectionPage.Server = server;
            ConnectionPage.ActivateSession(config, expiration);
            CurrentPage = ConnectionPage;
        }

        /// <summary>
        /// Add and connect to Secure Internet home server of organization
        /// </summary>
        /// <param name="org">Organization</param>
        public void AddAndConnect(Organization org)
        {
            AddAndConnect(new SecureInternetServer(org));
        }

        /// <summary>
        /// Renew and connect to given server
        /// </summary>
        /// <param name="server">Server</param>
        public async void RenewAndConnect(Server server)
        {
            // We have to set this to display server name on the connection page when/if user is presented with
            // a list of profiles to select one. It will be replaced by true server info provided by
            // Engine.CurrentServer() later.
            ConnectionPage.Server = server;

            // Not auto-reconnecting. Just reconnecting.
            Properties.Settings.Default.LastSelectedServer = null;

            Configuration config;
            Expiration expiration;
            try
            {
                using (OperationInProgress = new Engine.CancellationTokenCookie(Abort.Token))
                    (server, config, expiration) = await Task.Run(() =>
                    {
                        Engine.RenewSession(OperationInProgress);

                        var cfg = new Configuration(eduJSON.Parser.Parse(
                            Engine.GetConfig(OperationInProgress, server.ServerType, server.Id, Properties.Settings.Default.OpenVPNPreferTCP, false),
                            Abort.Token) as Dictionary<string, object>);

                        var srv = Server.Load(eduJSON.Parser.Parse(
                            Engine.CurrentServer(),
                            Abort.Token) as Dictionary<string, object>);

                        var exp = new Expiration(eduJSON.Parser.Parse(
                            Engine.ExpiryTimes(),
                            Abort.Token) as Dictionary<string, object>);

                        return (srv, cfg, exp);
                    });
            }
            finally { OperationInProgress = null; }
            ConnectionPage.Server = server;
            ConnectionPage.ActivateSession(config, expiration);
            CurrentPage = ConnectionPage;
        }

        /// <summary>
        /// Ask view to quit.
        /// </summary>
        /// <param name="sender">Event sender</param>
        public void OnQuitApplication(object sender)
        {
            Trace.TraceInformation("Quitting client");
            QuitApplication?.Invoke(sender, EventArgs.Empty);
        }

        /// <summary>
        /// Invoke method on GUI thread if it's not terminating.
        /// </summary>
        /// <param name="method">Method to execute</param>
        /// <returns>The return value from the delegate being invoked or <c>null</c> if the delegate has no return value or dispatcher is shutting down.</returns>
        public object TryInvoke(Delegate method)
        {
            if (Dispatcher.HasShutdownStarted)
                return null;
            return Dispatcher.Invoke(DispatcherPriority.Normal, method);
        }

        #endregion

        #region IDisposable Support
        /// <summary>
        /// Flag to detect redundant <see cref="Dispose(bool)"/> calls.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool disposedValue = false;

        /// <summary>
        /// Called to dispose the object.
        /// </summary>
        /// <param name="disposing">Dispose managed objects</param>
        /// <remarks>
        /// To release resources for inherited classes, override this method.
        /// Call <c>base.Dispose(disposing)</c> within it to release parent class resources, and release child class resources if <paramref name="disposing"/> parameter is <c>true</c>.
        /// This method can get called multiple times for the same object instance. When the child specific resources should be released only once, introduce a flag to detect redundant calls.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Engine.Deregister();
                    Engine.Callback -= Engine_Callback;
                    Engine.SetToken -= Engine_SetToken;
                    Engine.GetToken -= Engine_GetToken;
                }
                disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting resources.
        /// </summary>
        /// <remarks>
        /// This method calls <see cref="Dispose(bool)"/> with <c>disposing</c> parameter set to <c>true</c>.
        /// To implement resource releasing override the <see cref="Dispose(bool)"/> method.
        /// </remarks>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
