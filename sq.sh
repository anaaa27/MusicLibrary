#!/usr/bin/env sh
# SharpQuest helper for Linux and macOS. Usage: sh sq.sh test | sh sq.sh update | sh sq.sh upload | sh sq.sh help
# MANAGED FILE: `sq update` and CI replace it with the official copy.
#
# Finds dotnet even when it isn't on the PATH: on some lab PCs it only works inside Rider's terminal.

HERE=$(dirname "$0")

# sq is built from tools/sq/sq.csproj. A repository made in session 1 never received that file
# (the sq it started with didn't know about it), and a fresh clone or ZIP lacks it if it was never
# committed. Put it back instead of failing: it is the same for everyone.
if [ ! -f "$HERE/tools/sq/sq.csproj" ]; then
  mkdir -p "$HERE/tools/sq"
  cat > "$HERE/tools/sq/sq.csproj" <<'EOF'
<Project Sdk="Microsoft.NET.Sdk">
  <!--
    MANAGED FILE: `sq update` and CI replace it with the official copy.
    Builds the `sq` helper from ../../sq.cs. sq.cmd and sq.sh run this project, which works with a
    .NET 9 or a .NET 10 SDK. Running sq.cs as a single file would need .NET 10.
  -->
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <AssemblyName>sq</AssemblyName>
    <RootNamespace>SharpQuest.Helper</RootNamespace>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="../../sq.cs" />
  </ItemGroup>
</Project>
EOF
  echo "sq: tools/sq/sq.csproj was missing, so sq put it back." >&2
  echo "    Save it with your work: git add tools (then commit and push), or sh sq.sh upload." >&2
fi

DOTNET=$(command -v dotnet 2>/dev/null)
if [ -z "$DOTNET" ]; then
  for dir in "$DOTNET_ROOT" "$HOME/.dotnet" /usr/share/dotnet /usr/lib/dotnet /usr/lib64/dotnet /opt/dotnet /usr/local/share/dotnet; do
    if [ -n "$dir" ] && [ -x "$dir/dotnet" ]; then DOTNET="$dir/dotnet"; break; fi
  done
fi
if [ -z "$DOTNET" ]; then
  echo "sq: can't find dotnet. On a lab PC, run this in Rider's terminal (View > Tool Windows > Terminal)." >&2
  exit 127
fi

# The programs dotnet starts (sq itself, then dotnet test) need to find it too.
REAL=$(readlink -f "$DOTNET" 2>/dev/null || echo "$DOTNET")
if [ -z "$DOTNET_ROOT" ] && [ -d "$(dirname "$REAL")/sdk" ]; then
  DOTNET_ROOT=$(dirname "$REAL"); export DOTNET_ROOT
fi
PATH="$(dirname "$REAL"):$PATH"; export PATH

exec "$DOTNET" run --project "$HERE/tools/sq" -- "$@"
