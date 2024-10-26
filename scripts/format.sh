#! /usr/bin/env bash
set -euo pipefail
source "$( dirname "${BASH_SOURCE[0]}" )/common_functions.sh"

require_command dotnet

(
cd "$REPO"

# NOTE 2024-10-26:
# Setting severity of warning or greater for IDE0130 (Namespace does not match
# folder structure) breaks `dotnet format` on MSBuild workspaces. This is a
# known issue in `dotnet format` (https://github.com/dotnet/format/issues/1623)
# The exclude for IDE0130 can be dropped once the issue is fixed upstream.
log "Applying formatting rules"
log ".. applying workaround: ignore IDE0130 (https://github.com/dotnet/format/issues/1623)"
dotnet format Jakojaannos.WisperingWoods.sln --verbosity detailed --exclude-diagnostics IDE0130

echo "All done."
)
