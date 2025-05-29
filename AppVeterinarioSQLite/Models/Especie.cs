using SQLite;

namespace AppVeterinarioSQLite.Models
{
    [Table("tblespecies")]
    public class Especie
    {
        [PrimaryKey, AutoIncrement]
        [Column("espid")]
        public int Id { get; set; }

        [Column("espnome")]
        [MaxLength(50)]
        public string Nome { get; set; }

        public override string ToString()
        {
            return Nome;
        }
    }
}
