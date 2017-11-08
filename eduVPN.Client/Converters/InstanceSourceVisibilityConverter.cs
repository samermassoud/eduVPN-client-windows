﻿/*
    eduVPN - End-user friendly VPN

    Copyright: 2017, The Commons Conservancy eduVPN Programme
    SPDX-License-Identifier: GPL-3.0+
*/

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace eduVPN.Client.Converters
{
    /// <summary>
    /// Returns <c>Visibility.Visible</c> if instance source contains any connecting instances; or <c>Visibility.Collapsed</c> otherwise.
    /// </summary>
    class InstanceSourceVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return
                value is ViewModels.ConnectWizard parent &&
                parameter is Models.InstanceSourceType instance_source_type &&
                parent.InstanceSources[(int)instance_source_type].ConnectingInstance != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
