using SQLite;

namespace AppVeterinarioSQLite.Models
{
    [Table("tblanimaisclientes")]
    public class AnimalCliente
    {
        [Column("cliid")]
        public int ClienteId { get; set; }

        [Column("anid")]
        public int AnimalId { get; set; }

        [Ignore]
        public string ClienteNome { get; set; }

        [Ignore]
        public string AnimalNome { get; set; }
    }
}
