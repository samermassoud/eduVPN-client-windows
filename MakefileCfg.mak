#
#   eduVPN - VPN for education and research
#
#   Copyright: 2017-2022 The Commons Conservancy
#   SPDX-License-Identifier: GPL-3.0+
#

!IF "$(CFG)" == "Debug"
CFG_TARGET=D
CFG_VCPKG=debug\\
!ELSE
CFG_TARGET=
CFG_VCPKG=
!ENDIF

# WiX parameters
WIX_CANDLE_FLAGS_CFG=$(WIX_CANDLE_FLAGS) \
	-dCfgTarget="$(CFG_TARGET)"


######################################################################
# Building
######################################################################

"bin\$(CFG)" :
	if not exist $@ md $@

!IF "$(CFG)" == "$(SETUP_CFG)"
SetupPDB :: \
	"bin\Setup\PDB_$(VERSION)$(CFG_TARGET).zip"

"bin\Setup\PDB_$(VERSION)$(CFG_TARGET).zip" :
	-if exist "$(@:"=).tmp" del /f /q "$(@:"=).tmp"
	zip.exe -9 "$(@:"=).tmp" $**
	move /y "$(@:"=).tmp" $@ > NUL

Clean ::
	-if exist "bin\Setup\PDB_$(VERSION)$(CFG_TARGET).zip" del /f /q "bin\Setup\PDB_$(VERSION)$(CFG_TARGET).zip"
!ENDIF


######################################################################
# Platform specific rules
######################################################################

PLAT=x86
!INCLUDE "MakefileCfgPlat.mak"

PLAT=x64
!INCLUDE "MakefileCfgPlat.mak"

PLAT=ARM64
!INCLUDE "MakefileCfgPlat.mak"


######################################################################
# Client-specific rules
######################################################################

!INCLUDE "eduVPN.mak"
!INCLUDE "MakefileCfgClient.mak"

!INCLUDE "LetsConnect.mak"
!INCLUDE "MakefileCfgClient.mak"


######################################################################
# Signing
######################################################################

!IF "$(CFG)" == "$(SETUP_CFG)"
Setup ::
	echo Signing setup files
	minisign.exe -Sm \
		"bin\Setup\eduVPNClient_$(VERSION)$(CFG_TARGET).exe" \
		"bin\Setup\LetsConnectClient_$(VERSION)$(CFG_TARGET).exe" \
		"bin\Setup\eduVPNClient_$(VERSION)$(CFG_TARGET)_ARM64.msi" \
		"bin\Setup\eduVPNClient_$(VERSION)$(CFG_TARGET)_x64.msi" \
		"bin\Setup\eduVPNClient_$(VERSION)$(CFG_TARGET)_x86.msi" \
		"bin\Setup\LetsConnectClient_$(VERSION)$(CFG_TARGET)_ARM64.msi" \
		"bin\Setup\LetsConnectClient_$(VERSION)$(CFG_TARGET)_x64.msi" \
		"bin\Setup\LetsConnectClient_$(VERSION)$(CFG_TARGET)_x86.msi"

Clean ::
	-if exist "bin\Setup\*Client_*.exe.minisig" del /f /q "bin\Setup\*Client_*.exe.minisig"
	-if exist "bin\Setup\*Client_*.msi.minisig" del /f /q "bin\Setup\*Client_*.msi.minisig"
!ENDIF

!IF "$(CFG)" == "Release"
Publish ::
	echo Signing self-update discovery files
	minisign.exe -Sm \
		"bin\Setup\eduVPN.windows.json" \
		"bin\Setup\LetsConnect.windows.json"
!ENDIF
