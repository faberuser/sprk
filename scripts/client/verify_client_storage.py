"""Windows-only isolated Unity proof for storage identity and one-time migration.
Creates only GUID-scoped probe preferences, never reads/writes the official hive.
"""
import argparse
import json
import os
from pathlib import Path
import shutil
import subprocess
import uuid
import winreg
from verify_graphics_options import prepare, REPO, DATA


def hashed(key):
    value=5381
    for char in key:value=((value*33)^ord(char))&0xffffffff
    return key+"_h"+str(value)


def main():
    parser=argparse.ArgumentParser();parser.add_argument("--client",type=Path,default=REPO.parent/"sprk-client");args=parser.parse_args()
    identity=uuid.uuid4().hex;root=REPO/"target/client-storage"/("native-"+identity)
    prepare(args.client.resolve(),root,installed=True)
    staged=REPO/"target/client-storage/staged"/DATA
    for relative in ["globalgamemanagers","Managed/SprkAutomation.dll","Managed/Assembly-CSharp.dll"]:shutil.copy2(staged/relative,root/DATA/relative)
    old=root/"old";old.mkdir();(old/"sprk-accounts.dat").write_text("probe accounts")
    source="Software\\SPRK\\StorageProbe\\"+identity;target="Software\\sprk\\King's Raid"
    key="Sprk.Graphics.StorageProbe."+identity;value_name=hashed(key);receipt="Sprk.Storage.ProbeReceipt."+identity
    with winreg.CreateKey(winreg.HKEY_CURRENT_USER,source) as hive:winreg.SetValueEx(hive,value_name,0,winreg.REG_DWORD,123)
    def launch(tag,expected):
        env=dict(os.environ,SPRK_AUTOMATION_DIR=str(root/"automation"),SPRK_AUTOMATION_PROFILE="storage-"+identity,SPRK_STORAGE_PROBE_DIR=str(root),SPRK_STORAGE_PROBE_ID=identity)
        for name in list(env):
            if name.startswith("SPRK_GRAPHICS_"):env.pop(name)
        startup=subprocess.STARTUPINFO();startup.dwFlags|=subprocess.STARTF_USESHOWWINDOW;startup.wShowWindow=0
        executable=next(root.glob("SprkTest-graphics-*.exe"))
        proc=subprocess.Popen([str(executable),"-screen-fullscreen","0","-screen-width","640","-screen-height","480","-logFile",str(root/(tag+".log"))],cwd=root,env=env,startupinfo=startup)
        try:code=proc.wait(timeout=120)
        except subprocess.TimeoutExpired:
            proc.terminate();proc.wait(timeout=15);raise RuntimeError("Storage probe timed out: "+str(root))
        if code:raise RuntimeError("Storage probe failed: "+str(root))
        result=json.loads((root/"native-result.json").read_text());assert result["company"]=="sprk",result
        assert result["path"].replace("\\","/").lower().endswith("/sprk/king's raid"),result
        assert result["value"]==expected,result
        assert (root/"new/sprk-accounts.dat").read_text()=="probe accounts"
        shutil.copy2(root/"native-result.json",root/(tag+".json"))
    try:
        launch("first",123)
        with winreg.CreateKey(winreg.HKEY_CURRENT_USER,source) as hive:winreg.SetValueEx(hive,value_name,0,winreg.REG_DWORD,987)
        with winreg.CreateKey(winreg.HKEY_CURRENT_USER,target) as hive:winreg.SetValueEx(hive,value_name,0,winreg.REG_DWORD,456)
        (old/"sprk-accounts.dat").write_text("later official data")
        launch("restart",456)
        print("PASS: native company sprk, new persistent data path, migrated native PlayerPrefs, saved account bytes, restart preservation and no future legacy reimport.")
        print("Artifacts:",root)
    finally:
        for hive_path,names in [(target,[value_name,receipt]),(source,[value_name])]:
            try:
                with winreg.OpenKey(winreg.HKEY_CURRENT_USER,hive_path,0,winreg.KEY_SET_VALUE) as hive:
                    for name in names:
                        try:winreg.DeleteValue(hive,name)
                        except FileNotFoundError:pass
            except FileNotFoundError:pass
        try:winreg.DeleteKey(winreg.HKEY_CURRENT_USER,source)
        except FileNotFoundError:pass


if __name__=="__main__":main()
