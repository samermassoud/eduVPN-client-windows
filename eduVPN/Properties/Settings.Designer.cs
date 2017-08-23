﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace eduVPN.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.1.0.0")]
    public sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("https://static.eduvpn.nl/disco/institute_access.json")]
        public string InstituteAccessDirectory {
            get {
                return ((string)(this["InstituteAccessDirectory"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("E5On0JTtyUVZmcWd+I/FXRm32nSq8R2ioyW7dcu/U88=")]
        public string InstituteAccessDirectoryPubKey {
            get {
                return ((string)(this["InstituteAccessDirectoryPubKey"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::eduVPN.JSON.Response InstituteAccessDirectoryCache {
            get {
                return ((global::eduVPN.JSON.Response)(this["InstituteAccessDirectoryCache"]));
            }
            set {
                this["InstituteAccessDirectoryCache"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("https://static.eduvpn.nl/disco/secure_internet.json")]
        public string SecureInternetDirectory {
            get {
                return ((string)(this["SecureInternetDirectory"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("E5On0JTtyUVZmcWd+I/FXRm32nSq8R2ioyW7dcu/U88=")]
        public string SecureInternetDirectoryPubKey {
            get {
                return ((string)(this["SecureInternetDirectoryPubKey"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::eduVPN.JSON.Response SecureInternetDirectoryCache {
            get {
                return ((global::eduVPN.JSON.Response)(this["SecureInternetDirectoryCache"]));
            }
            set {
                this["SecureInternetDirectoryCache"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string OpenVPNInterface {
            get {
                return ((string)(this["OpenVPNInterface"]));
            }
            set {
                this["OpenVPNInterface"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int SettingsVersion {
            get {
                return ((int)(this["SettingsVersion"]));
            }
            set {
                this["SettingsVersion"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<SerializableStringDictionary/>")]
        public global::eduVPN.SerializableStringDictionary AccessTokens {
            get {
                return ((global::eduVPN.SerializableStringDictionary)(this["AccessTokens"]));
            }
            set {
                this["AccessTokens"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<InstanceSettingsDictionary/>")]
        public global::eduVPN.Models.InstanceSettingsDictionary InstanceSettings {
            get {
                return ((global::eduVPN.Models.InstanceSettingsDictionary)(this["InstanceSettings"]));
            }
            set {
                this["InstanceSettings"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<VPNConfigurationSettingsList/>")]
        public global::eduVPN.Models.VPNConfigurationSettingsList InstituteAccessDirectoryConfigHistory {
            get {
                return ((global::eduVPN.Models.VPNConfigurationSettingsList)(this["InstituteAccessDirectoryConfigHistory"]));
            }
            set {
                this["InstituteAccessDirectoryConfigHistory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<VPNConfigurationSettingsList/>")]
        public global::eduVPN.Models.VPNConfigurationSettingsList SecureInternetDirectoryConfigHistory {
            get {
                return ((global::eduVPN.Models.VPNConfigurationSettingsList)(this["SecureInternetDirectoryConfigHistory"]));
            }
            set {
                this["SecureInternetDirectoryConfigHistory"] = value;
            }
        }
    }
}
