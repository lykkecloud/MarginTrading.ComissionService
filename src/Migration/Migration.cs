using System;

namespace Migration
{
    public class Migration
    {
        public string Id { get; set; }
        
        public DateTime Time { get; set; }
        
        public MigrationState State { get; set; }
    }
}