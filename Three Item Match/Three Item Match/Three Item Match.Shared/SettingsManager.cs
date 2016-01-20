using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage;

namespace Three_Item_Match
{
    public static class SettingsManager
    {
        public static T GetSetting<T>(string settingName, bool roaming, T def)
        {
            ApplicationDataContainer curContainer = roaming ? ApplicationData.Current.RoamingSettings : ApplicationData.Current.LocalSettings;
            object result = curContainer.Values[settingName];
            if (result == null) return def;
            return (T)result;
        }

        public static T GetSetting<T>(string settingName, bool roaming) => GetSetting<T>(settingName, roaming, default(T));

        public static void SetSetting<T>(string settingName, bool roaming, T value)
        {
            ApplicationDataContainer curContainer = roaming ? ApplicationData.Current.RoamingSettings : ApplicationData.Current.LocalSettings;
            if (!curContainer.Values.ContainsKey(settingName))
                curContainer.Values.Add(settingName, value);
            else
                curContainer.Values[settingName] = value;
        }
    }
}

//IncorrectSetBehavior IncorrectBehavior = IncorrectSetBehavior.Nothing
//bool AutoDeal = false
//bool EnsureSets = false
//bool PenaltyOnDealWithSets = false
//bool TrainingMode = false
//bool DrawThree = true
//bool instantDeal = false