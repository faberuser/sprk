"""Pinned client release that supplies everything the build does not compile.

Written by pin_client_baseline.py. Game assets, tables, Unity data files and the
runtime DLLs come from this signed release; client/overrides/files is applied on
top, and Assembly-CSharp.dll plus every Sprk*.dll is rebuilt from client/managed.
Change it only to adopt a newer published release as the starting point.
"""

BASELINE = {
    "repository": "faberuser/sprk",
    "tag": "client-ci-2795d44b755a-43152391931-1",
    "asset": "client-update.tar.gz",
    "sha256": "52976d9ed51bfc019b885099fc95187a2476418688cf3cbfb3da85486c24feb6",
}
