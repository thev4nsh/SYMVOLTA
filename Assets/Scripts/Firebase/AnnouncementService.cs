using System;
using SYMVOLTA.Core;
using SYMVOLTA.Utilities;

namespace SYMVOLTA.FirebaseNS
{
    [Serializable]
    public class Announcement
    {
        public string id;
        public string title;
        public string body;
        public bool active;
    }

    public class AnnouncementService : Singleton<AnnouncementService>
    {
        public Announcement Current { get; private set; }
        public event Action<Announcement> OnAnnouncementReady;

        public void InitializeFromConfig(GameConfig config)
        {
            Current = new Announcement
            {
                id = string.IsNullOrWhiteSpace(config.announcementId) ? $"{config.announcementTitle}:{config.announcementBody}".GetHashCode().ToString() : config.announcementId,
                title = config.announcementTitle,
                body = config.announcementBody,
                active = config.announcementActive && !string.IsNullOrWhiteSpace(config.announcementTitle)
            };

            if (ShouldShow(Current))
            {
                OnAnnouncementReady?.Invoke(Current);
            }
        }

        public bool ShouldShow(Announcement announcement)
        {
            if (announcement == null || !announcement.active) return false;
            string last = SaveSystem.LoadString(Constants.SaveKeys.LAST_ANNOUNCEMENT, "");
            return !string.Equals(last, announcement.id, StringComparison.Ordinal);
        }

        public void MarkSeen(Announcement announcement)
        {
            if (announcement == null) return;
            SaveSystem.SaveString(Constants.SaveKeys.LAST_ANNOUNCEMENT, announcement.id);
        }
    }
}
