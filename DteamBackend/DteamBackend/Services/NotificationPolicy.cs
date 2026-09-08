using DteamBackend.Models;

namespace DteamBackend.Services
{
    public static class NotificationPolicy
    {
        public static bool IsMandatory(string type) => type switch
        {
            NotificationTypes.WalletDeposit => true,
            NotificationTypes.SecurityAlert => true,
            NotificationTypes.System => true,
            _ => false
        };

        public static bool IsEnabled(UserNotificationPreferences? prefs, string type)
        {
            if (IsMandatory(type))
            {
                return true;
            }

            if (prefs == null)
            {
                return true;
            }

            return type switch
            {
                NotificationTypes.FriendRequest => prefs.NotifyFriendRequests,
                NotificationTypes.FriendAccepted => prefs.NotifyFriendRequestAccepted,
                NotificationTypes.ChatMessage => prefs.ChatNotificationsEnabled,
                _ => true
            };
        }
    }
}
