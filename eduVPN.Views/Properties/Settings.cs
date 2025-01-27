﻿/*
    eduVPN - VPN for education and research

    Copyright: 2017-2023 The Commons Conservancy
    SPDX-License-Identifier: GPL-3.0+
*/

using System.Configuration;

namespace eduVPN.Views.Properties
{
    /// <summary>
    /// eduVPN settings
    /// </summary>
    public sealed partial class Settings : ApplicationSettingsBase
    {
        #region Methods

        /// <summary>
        /// Initialize settings
        /// </summary>
        public static void Initialize()
        {
            if ((Default.SettingsVersion & 0x1) == 0)
            {
                // Migrate settings from previous version.
                Default.Upgrade();
                Default.SettingsVersion |= 0x1;

                // TODO: Migrate legacy settings here.
            }
        }

        #endregion
    }
}
