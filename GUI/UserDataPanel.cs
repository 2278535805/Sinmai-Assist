using System;
using System.IO;
using System.Linq;
using MAI2.Util;
using Manager;
using MelonLoader;
using SinmaiAssist.Utils;
using UnityEngine;

namespace SinmaiAssist.GUI;

public class UserDataPanel
{
    private static UserData _player1 = null;
    private static UserData _player2 = null;
    private static bool _isNewItem = false;
    
    private enum CollectionType
    {
        Icon = UserData.Collection.Icon,
        Plate = UserData.Collection.Plate,
        Title = UserData.Collection.Title,
        Partner = UserData.Collection.Partner,
        Frame = UserData.Collection.Frame
    }
    
    private static string[] _userInputId = ["", "", "", "", "", "", ""];
    private static string _removeMusicId = "";
    private static string _removeDifficulty = "";
    
    public static void OnGUI()
    {
        GUILayout.Label($"User Info", MainGUI.Style.Title);
        try
        {
            _player1 = Singleton<UserDataManager>.Instance.GetUserData(0);
            _player2 = Singleton<UserDataManager>.Instance.GetUserData(1);
        }
        catch (Exception e)
        {
            // ignore
        }
        GUILayout.Label($"1P: {_player1.Detail.UserName} ({_player1.Detail.UserID})", MainGUI.Style.Text);
        GUILayout.Label($"2P: {_player2.Detail.UserName} ({_player2.Detail.UserID})", MainGUI.Style.Text);
        
        GUILayout.Label("Add Collections (Space-Separated)", MainGUI.Style.Title);
        foreach (CollectionType type in Enum.GetValues(typeof(CollectionType)))
        {
            GUILayout.BeginHorizontal();
            int typeId = (int)type;
            GUILayout.Label(type.ToString(), new GUIStyle(MainGUI.Style.Text){fixedWidth = 50});
            _userInputId[typeId] = GUILayout.TextField(_userInputId[typeId]);
            if (string.IsNullOrEmpty(_userInputId[typeId]))
            {
                if (GUILayout.Button("All", new GUIStyle(MainGUI.Style.Button){ fixedWidth = 50}))
                {
                    AddAllCollections(0, type);
                    AddAllCollections(1, type);
                }
            }
            else
            {
                if (GUILayout.Button("Add", new GUIStyle(MainGUI.Style.Button){ fixedWidth = 50}))
                {
                    AddCollections(0, type, _userInputId[typeId]);
                    AddCollections(1, type, _userInputId[typeId]);
                }
            }
            GUILayout.EndHorizontal();
        }
        _isNewItem = GUILayout.Toggle(_isNewItem, "Is New Item");
        
        GUILayout.Label("Unlock Music (Space-Separated)", MainGUI.Style.Title);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Music", new GUIStyle(MainGUI.Style.Text){fixedWidth = 50});
        _userInputId[0] = GUILayout.TextField(_userInputId[0]);
        if (string.IsNullOrEmpty(_userInputId[0]))
        {
            if (GUILayout.Button("All", new GUIStyle(MainGUI.Style.Button){ fixedWidth = 50}))
            {
                UnlockAllMusic(0);
                UnlockAllMusic(1);
            }
        }
        else
        {
            if (GUILayout.Button("Add", new GUIStyle(MainGUI.Style.Button){ fixedWidth = 50}))
            {
                UnlockMusic(0, _userInputId[0]);
                UnlockMusic(1, _userInputId[0]);
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Space(60);
        if (string.IsNullOrEmpty(_userInputId[0]))
        {
            if (GUILayout.Button("All Base", MainGUI.Style.Button))
            {
                UnlockAllBaseOnly(0);
                UnlockAllBaseOnly(1);
            }
            if (GUILayout.Button("All Master", MainGUI.Style.Button))
            {
                UnlockAllMasterOnly(0);
                UnlockAllMasterOnly(1);
            }
            if (GUILayout.Button("All ReMaster", MainGUI.Style.Button))
            {
                UnlockAllReMasterOnly(0);
                UnlockAllReMasterOnly(1);
            }
        }
        else
        {
            if (GUILayout.Button("Only Base", new GUIStyle(MainGUI.Style.Button)))
            {
                UnlockBaseOnly(0, _userInputId[0]);
                UnlockBaseOnly(1, _userInputId[0]);
            }
            if (GUILayout.Button("Only Master", new GUIStyle(MainGUI.Style.Button)))
            {
                UnlockMasterOnly(0, _userInputId[0]);
                UnlockMasterOnly(1, _userInputId[0]);
            }
            if (GUILayout.Button("Only ReMaster", new GUIStyle(MainGUI.Style.Button)))
            {
                UnlockReMasterOnly(0, _userInputId[0]);
                UnlockReMasterOnly(1, _userInputId[0]);
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Score", new GUIStyle(MainGUI.Style.Text){fixedWidth = 50});
        _removeMusicId = GUILayout.TextField(_removeMusicId);
        GUILayout.Label("Diff", new GUIStyle(MainGUI.Style.Text){fixedWidth = 30});
        _removeDifficulty = GUILayout.TextField(_removeDifficulty, new GUIStyle(UnityEngine.GUI.skin.textField){fixedWidth = 25});
        if (GUILayout.Button("Remove", new GUIStyle(MainGUI.Style.Button){ fixedWidth = 60}))
        {
            RemoveMusicScore(0, _removeMusicId, _removeDifficulty);
            RemoveMusicScore(1, _removeMusicId, _removeDifficulty);
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Label("MaiMile", MainGUI.Style.Title);
        GUILayout.BeginHorizontal();
        GUILayout.Label("MaiMile", new GUIStyle(MainGUI.Style.Text){fixedWidth = 50});
        _userInputId[6] = GUILayout.TextField(_userInputId[6]);
        if (GUILayout.Button("Add", new GUIStyle(MainGUI.Style.Button){ fixedWidth = 50}))
        {
            AddMaiMile(0, _userInputId[6]);
            AddMaiMile(1, _userInputId[6]);
        }
        if (GUILayout.Button("Sub", new GUIStyle(MainGUI.Style.Button){ fixedWidth = 50}))
        {
            SubtractMaiMile(0, _userInputId[6]);
            SubtractMaiMile(1, _userInputId[6]);
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Label("User Data Backup", MainGUI.Style.Title);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("1P", MainGUI.Style.Button)) User.ExportBackupData(0);
        if (GUILayout.Button("2P", MainGUI.Style.Button)) User.ExportBackupData(1);
        GUILayout.EndHorizontal();
        
    }

    private static void AddCollections(long index, CollectionType type, string input)
    {
        UserData userData = Singleton<UserDataManager>.Instance.GetUserData(index);
        if (userData.IsGuest())
        {
            GameMessageManager.SendMessage((int)index,$"Guest Account\nUnable to add collections");
            return;
        }
        try
        {
            var ids = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (ids.Length == 0)
            {
                GameMessageManager.SendMessage((int)index,$"Invalid ID\n {input}");
                return;
            }
            foreach (var idStr in ids)
            {
                if (int.TryParse(idStr, out int id))
                {
                    if (userData.AddCollections((UserData.Collection)type, id, _isNewItem))
                    {
                        GameMessageManager.SendMessage((int)index,$"Add Collections \n{type} {id}" + (_isNewItem ? " (New Item)" : "") );
                    }
                    else
                    {
                        GameMessageManager.SendMessage((int)index,$"Failed to add Collections or already added\n{type} {id}");
                    }
                }
                else
                {
                    GameMessageManager.SendMessage((int)index,$"Invalid ID\n {idStr}");
                }
            }
        }
        catch (Exception e)
        {
            GameMessageManager.SendMessage((int)index,$"Unknown error");
            MelonLogger.Error(e);
        }
    }

    private static void AddAllCollections(long index, CollectionType type)
    {
        UserData userData = Singleton<UserDataManager>.Instance.GetUserData(index);
        if (userData.IsGuest())
        {
            GameMessageManager.SendMessage((int)index, $"Guest Account\nUnable to add collections");
            return;
        }
        try
        {
            int count = 0;
            var userCollectionType = (UserData.Collection)type;
            switch (type)
            {
                case CollectionType.Frame:
                    foreach (var kvp in Singleton<DataManager>.Instance.GetFrames())
                        if (userData.AddCollections(userCollectionType, kvp.Value.GetID(), _isNewItem)) count++;
                    break;
                case CollectionType.Icon:
                    foreach (var kvp in Singleton<DataManager>.Instance.GetIcons())
                        if (userData.AddCollections(userCollectionType, kvp.Value.GetID(), _isNewItem)) count++;
                    break;
                case CollectionType.Plate:
                    foreach (var kvp in Singleton<DataManager>.Instance.GetPlates())
                        if (userData.AddCollections(userCollectionType, kvp.Value.GetID(), _isNewItem)) count++;
                    break;
                case CollectionType.Partner:
                    foreach (var kvp in Singleton<DataManager>.Instance.GetPartners())
                        if (userData.AddCollections(userCollectionType, kvp.Value.GetID(), _isNewItem)) count++;
                    break;
                case CollectionType.Title:
                    foreach (var kvp in Singleton<DataManager>.Instance.GetTitles())
                        if (userData.AddCollections(userCollectionType, kvp.Value.GetID(), _isNewItem)) count++;
                    break;
            }
            GameMessageManager.SendMessage((int)index, $"Add All {type}\n{count} items");
        }
        catch (Exception e)
        {
            GameMessageManager.SendMessage((int)index, $"Unknown error");
            MelonLogger.Error(e);
        }
    }

    private static void UnlockMusic(long index, string input)
    {
        UserData userData = Singleton<UserDataManager>.Instance.GetUserData(index);
        if (userData.IsGuest())
        {
            GameMessageManager.SendMessage((int)index,$"Guest Account\nUnable to unlock music");
            return;
        }
        try
        {
            var ids = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (ids.Length == 0)
            {
                GameMessageManager.SendMessage((int)index,$"Invalid ID\n {input}");
                return;
            }
            foreach (var idStr in ids)
            {
                if (int.TryParse(idStr, out int id))
                {
                    var messages = new System.Collections.Generic.List<string>();
                    if (!userData.IsUnlockMusic(UserData.MusicUnlock.Base, id))
                    {
                        if (userData.AddUnlockMusic(UserData.MusicUnlock.Base, id))
                            messages.Add($"Unlock Music {id}");
                        else
                            messages.Add($"Failed to unlock music {id}");
                    }
                    if (!userData.IsUnlockMusic(UserData.MusicUnlock.Master, id))
                    {
                        userData.AddUnlockMusic(UserData.MusicUnlock.Master, id);
                        messages.Add($"Unlock Master {id}");
                    }
                    if (!userData.IsUnlockMusic(UserData.MusicUnlock.ReMaster, id))
                    {
                        userData.AddUnlockMusic(UserData.MusicUnlock.ReMaster, id);
                        messages.Add($"Unlock ReMaster {id}");
                    }
                    if (messages.Count == 0)
                        messages.Add($"Already unlocked {id}");
                    GameMessageManager.SendMessage((int)index, string.Join("\n", messages));
                }
                else
                {
                    GameMessageManager.SendMessage((int)index,$"Invalid ID\n {idStr}");
                }
            }
        }
        catch (Exception e)
        {
            GameMessageManager.SendMessage((int)index,$"Unknown error");
            MelonLogger.Error(e);
        }
    }

    private static void UnlockAllMusic(long index)
    {
        UserData userData = Singleton<UserDataManager>.Instance.GetUserData(index);
        if (userData.IsGuest())
        {
            GameMessageManager.SendMessage((int)index,$"Guest Account\nUnable to unlock music");
            return;
        }
        try
        {
            var musicDict = Singleton<DataManager>.Instance.GetMusics();
            int baseCount = 0;
            int masterCount = 0;
            int reMasterCount = 0;
            foreach (var kvp in musicDict)
            {
                int id = kvp.Key;
                if (!userData.IsUnlockMusic(UserData.MusicUnlock.Base, id))
                {
                    if (userData.AddUnlockMusic(UserData.MusicUnlock.Base, id))
                        baseCount++;
                }
                if (!userData.IsUnlockMusic(UserData.MusicUnlock.Master, id))
                {
                    userData.AddUnlockMusic(UserData.MusicUnlock.Master, id);
                    masterCount++;
                }
                if (!userData.IsUnlockMusic(UserData.MusicUnlock.ReMaster, id))
                {
                    userData.AddUnlockMusic(UserData.MusicUnlock.ReMaster, id);
                    reMasterCount++;
                }
            }
            GameMessageManager.SendMessage((int)index,$"Unlock All Complete\nBase: {baseCount}, Master: {masterCount}, ReMaster: {reMasterCount}");
        }
        catch (Exception e)
        {
            GameMessageManager.SendMessage((int)index,$"Unknown error");
            MelonLogger.Error(e);
        }
    }

    private static void UnlockAllBaseOnly(long index)
    {
        UserData userData = Singleton<UserDataManager>.Instance.GetUserData(index);
        if (userData.IsGuest())
        {
            GameMessageManager.SendMessage((int)index, "Guest Account\nUnable to unlock music");
            return;
        }
        try
        {
            var musicDict = Singleton<DataManager>.Instance.GetMusics();
            int count = 0;
            foreach (var kvp in musicDict)
            {
                int id = kvp.Key;
                if (!userData.IsUnlockMusic(UserData.MusicUnlock.Base, id))
                {
                    if (userData.AddUnlockMusic(UserData.MusicUnlock.Base, id))
                        count++;
                }
            }
            GameMessageManager.SendMessage((int)index, $"Unlock All Base\n{count} songs");
        }
        catch (Exception e)
        {
            GameMessageManager.SendMessage((int)index, "Unknown error");
            MelonLogger.Error(e);
        }
    }

    private static void UnlockAllMasterOnly(long index)
    {
        UserData userData = Singleton<UserDataManager>.Instance.GetUserData(index);
        if (userData.IsGuest())
        {
            GameMessageManager.SendMessage((int)index, "Guest Account\nUnable to unlock music");
            return;
        }
        try
        {
            var musicDict = Singleton<DataManager>.Instance.GetMusics();
            int count = 0;
            foreach (var kvp in musicDict)
            {
                int id = kvp.Key;
                if (userData.IsUnlockMusic(UserData.MusicUnlock.Base, id)
                    && !userData.IsUnlockMusic(UserData.MusicUnlock.Master, id))
                {
                    userData.AddUnlockMusic(UserData.MusicUnlock.Master, id);
                    count++;
                }
            }
            GameMessageManager.SendMessage((int)index, $"Unlock Only Master\n{count} songs");
        }
        catch (Exception e)
        {
            GameMessageManager.SendMessage((int)index, "Unknown error");
            MelonLogger.Error(e);
        }
    }

    private static void UnlockAllReMasterOnly(long index)
    {
        UserData userData = Singleton<UserDataManager>.Instance.GetUserData(index);
        if (userData.IsGuest())
        {
            GameMessageManager.SendMessage((int)index, "Guest Account\nUnable to unlock music");
            return;
        }
        try
        {
            var musicDict = Singleton<DataManager>.Instance.GetMusics();
            int count = 0;
            foreach (var kvp in musicDict)
            {
                int id = kvp.Key;
                if (userData.IsUnlockMusic(UserData.MusicUnlock.Base, id)
                    && !userData.IsUnlockMusic(UserData.MusicUnlock.ReMaster, id))
                {
                    userData.AddUnlockMusic(UserData.MusicUnlock.ReMaster, id);
                    count++;
                }
            }
            GameMessageManager.SendMessage((int)index, $"Unlock Only ReMaster\n{count} songs");
        }
        catch (Exception e)
        {
            GameMessageManager.SendMessage((int)index, "Unknown error");
            MelonLogger.Error(e);
        }
    }

    private static void UnlockBaseOnly(long index, string input)
    {
        UserData userData = Singleton<UserDataManager>.Instance.GetUserData(index);
        if (userData.IsGuest())
        {
            GameMessageManager.SendMessage((int)index, "Guest Account\nUnable to unlock music");
            return;
        }
        try
        {
            var ids = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var messages = new System.Collections.Generic.List<string>();
            foreach (var idStr in ids)
            {
                if (!int.TryParse(idStr, out int id)) continue;
                if (!userData.IsUnlockMusic(UserData.MusicUnlock.Base, id))
                {
                    if (userData.AddUnlockMusic(UserData.MusicUnlock.Base, id))
                        messages.Add($"Unlock Base {id}");
                    else
                        messages.Add($"Failed to unlock {id}");
                }
                else
                {
                    messages.Add($"Already unlocked {id}");
                }
            }
            if (messages.Count > 0)
                GameMessageManager.SendMessage((int)index, string.Join("\n", messages));
        }
        catch (Exception e)
        {
            GameMessageManager.SendMessage((int)index, "Unknown error");
            MelonLogger.Error(e);
        }
    }

    private static void UnlockMasterOnly(long index, string input)
    {
        UserData userData = Singleton<UserDataManager>.Instance.GetUserData(index);
        if (userData.IsGuest())
        {
            GameMessageManager.SendMessage((int)index, "Guest Account\nUnable to unlock music");
            return;
        }
        try
        {
            var ids = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var messages = new System.Collections.Generic.List<string>();
            foreach (var idStr in ids)
            {
                if (!int.TryParse(idStr, out int id)) continue;
                if (!userData.IsUnlockMusic(UserData.MusicUnlock.Base, id))
                {
                    messages.Add($"Not unlocked Base {id}");
                    continue;
                }
                if (!userData.IsUnlockMusic(UserData.MusicUnlock.Master, id))
                {
                    userData.AddUnlockMusic(UserData.MusicUnlock.Master, id);
                    messages.Add($"Unlock Master {id}");
                }
                else
                {
                    messages.Add($"Already unlocked {id}");
                }
            }
            if (messages.Count > 0)
                GameMessageManager.SendMessage((int)index, string.Join("\n", messages));
        }
        catch (Exception e)
        {
            GameMessageManager.SendMessage((int)index, "Unknown error");
            MelonLogger.Error(e);
        }
    }

    private static void UnlockReMasterOnly(long index, string input)
    {
        UserData userData = Singleton<UserDataManager>.Instance.GetUserData(index);
        if (userData.IsGuest())
        {
            GameMessageManager.SendMessage((int)index, "Guest Account\nUnable to unlock music");
            return;
        }
        try
        {
            var ids = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var messages = new System.Collections.Generic.List<string>();
            foreach (var idStr in ids)
            {
                if (!int.TryParse(idStr, out int id)) continue;
                if (!userData.IsUnlockMusic(UserData.MusicUnlock.Base, id))
                {
                    messages.Add($"Not unlocked Base {id}");
                    continue;
                }
                if (!userData.IsUnlockMusic(UserData.MusicUnlock.ReMaster, id))
                {
                    userData.AddUnlockMusic(UserData.MusicUnlock.ReMaster, id);
                    messages.Add($"Unlock ReMaster {id}");
                }
                else
                {
                    messages.Add($"Already unlocked {id}");
                }
            }
            if (messages.Count > 0)
                GameMessageManager.SendMessage((int)index, string.Join("\n", messages));
        }
        catch (Exception e)
        {
            GameMessageManager.SendMessage((int)index, "Unknown error");
            MelonLogger.Error(e);
        }
    }

    private static void RemoveMusicScore(long index, string input, string difficulty)
    {
        UserData userData = Singleton<UserDataManager>.Instance.GetUserData(index);
        if (userData.IsGuest())
        {
            GameMessageManager.SendMessage((int)index, $"Guest Account\nUnable to remove score");
            return;
        }
        try
        {
            var ids = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (ids.Length == 0)
            {
                GameMessageManager.SendMessage((int)index, $"Invalid ID\n {input}");
                return;
            }
            bool hasDiff = int.TryParse(difficulty, out int targetDiff);
            int removedCount = 0;
            foreach (var idStr in ids)
            {
                if (!int.TryParse(idStr, out int musicId)) continue;

                bool removed = false;
                if (hasDiff)
                {
                    try
                    {
                        var diffScores = userData.ScoreDic[targetDiff];
                        if (diffScores != null && diffScores.Remove(musicId))
                            removed = true;
                    }
                    catch { }
                }
                else
                {
                    for (int diff = 0; diff <= 5; diff++)
                    {
                        try
                        {
                            var diffScores = userData.ScoreDic[diff];
                            if (diffScores != null && diffScores.Remove(musicId))
                                removed = true;
                        }
                        catch { break; }
                    }
                }
                if (removed) removedCount++;
            }
            GameMessageManager.SendMessage((int)index, $"Removed Score\n{removedCount} song(s)");
        }
        catch (Exception e)
        {
            GameMessageManager.SendMessage((int)index, $"Unknown error");
            MelonLogger.Error(e);
        }
    }

    private static void AddMaiMile(long index, string input)
    {
        UserData userData = Singleton<UserDataManager>.Instance.GetUserData(index);
        if (SinmaiAssist.GameVersion < 25000)
        {
            GameMessageManager.SendMessage((int)index,$"MaiMile is not supported in this version");
            return;
        }
        if (userData.IsGuest())
        {
            GameMessageManager.SendMessage((int)index,$"Guest Account\nUnable to add MaiMile");
            return;
        }
        try
        {
            if (int.TryParse(input , out int addMile))
            {
                var haveMile = userData.Detail.Point;
                if (haveMile + addMile >= 99999)
                    addMile = 99999 - haveMile;
                var addMileBefore = haveMile + addMile;
                
                userData.AddPresentMile(addMile);
                GameMessageManager.SendMessage((int)index,$"Add {addMile} MaiMile\n ({haveMile} -> {addMileBefore})");
            }
            else
            {
                GameMessageManager.SendMessage((int)index,$"Invalid MaiMile\n {input}");
            }
        }
        catch (Exception e)
        {
            GameMessageManager.SendMessage((int)index,$"Unknown error");
            MelonLogger.Error(e);
        }
    }

    private static void SubtractMaiMile(long index, string input)
    {
        UserData userData = Singleton<UserDataManager>.Instance.GetUserData(index);
        if (SinmaiAssist.GameVersion < 25000)
        {
            GameMessageManager.SendMessage((int)index,$"MaiMile is not supported in this version");
            return;
        }
        if (userData.IsGuest())
        {
            GameMessageManager.SendMessage((int)index,$"Guest Account\nUnable to subtract MaiMile");
            return;
        }
        try
        {
            if (int.TryParse(input, out int subMile))
            {
                var haveMile = userData.Detail.Point;
                if (subMile > haveMile)
                    subMile = haveMile;
                var subMileAfter = haveMile - subMile;

                userData.AddPresentMile(-subMile);
                GameMessageManager.SendMessage((int)index,$"Sub {subMile} MaiMile\n ({haveMile} -> {subMileAfter})");
            }
            else
            {
                GameMessageManager.SendMessage((int)index,$"Invalid MaiMile\n {input}");
            }
        }
        catch (Exception e)
        {
            GameMessageManager.SendMessage((int)index,$"Unknown error");
            MelonLogger.Error(e);
        }
    }
}