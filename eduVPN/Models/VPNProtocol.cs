﻿/*
    eduVPN - VPN for education and research

    Copyright: 2017-2023 The Commons Conservancy
    SPDX-License-Identifier: GPL-3.0+
*/

namespace eduVPN.Models
{
    /// <summary>
    /// Known VPN protocols
    /// </summary>
    /// <see cref="eduvpn-common/types/protocol/protocol.go"/>
    public enum VPNProtocol
    {
        Unknown,
        OpenVPN,
        WireGuard
    }
}
