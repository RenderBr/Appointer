using PetaPoco;
namespace Appointer.Models
{
    [TableName("UserPlaytime")]
    [PrimaryKey("AccountName")]
    public class UserPlaytime
    {
        [Column("AccountName")]
        public string AccountName { get; set; }

        [Column("Playtime")]
        public int _playtime { get; set; }

        [Ignore]
        public int Playtime
        {
            get => _playtime;

            set
            {
                _playtime = value;
                Appointer.DB.Update(this);
            }
        }
    }
}