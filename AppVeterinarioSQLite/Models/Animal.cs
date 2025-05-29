using SQLite;

namespace AppVeterinarioSQLite.Models
{
    [Table("tblanimais")]
    public class Animal
    {
        [PrimaryKey, AutoIncrement]
        [Column("anid")]
        public int Id { get; set; }

        [Column("aninome")]
        [MaxLength(50)]
        public string Nome { get; set; }

        [Column("aniapetido")]
        [MaxLength(25)]
        public string Apelido { get; set; }

        [Column("anidatanasc")]
        public DateTime DataNascimento { get; set; }

        [Column("aniobservacoes")]
        [MaxLength(600)]
        public string Observacoes { get; set; }

        [Column("espid")]
        public int EspecieId { get; set; }

        [Ignore]
        public string EspecieNome { get; set; }

        public override string ToString()
        {
            return $"{Nome} ({Apelido})";
        }
    }
}
