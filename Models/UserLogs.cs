using System;
using System.ComponentModel.DataAnnotations;

namespace BeepApp_API.Models
{
    public enum LogAction
    {
        Create, // Yeni bir veri oluşturulduğunda
        Update, // Mevcut bir veri güncellendiğinde
        Delete  // Mevcut bir veri silindiğinde
    }
    public class UserLogs
    {
        public Guid Id { get; set; } // Log kaydının benzersiz ID'si

        public Guid DataId { get; set; } // Loglanan datanın ID'si

        public LogAction Action { get; set; } // İşlem türü (Create, Update, Delete)

        public string UserId { get; set; } // İşlemi yapan kullanıcı

        public DateTime Timestamp { get; set; } = DateTime.UtcNow; // İşlem tarihi
    }
}
