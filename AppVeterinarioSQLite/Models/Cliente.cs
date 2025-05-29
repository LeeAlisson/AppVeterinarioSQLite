using SQLite;

namespace AppVeterinarioSQLite.Models
{
    [Table("tblclientes")]
    public class Cliente
    {
        [PrimaryKey, AutoIncrement]
        [Column("cliid")]
        public int Id { get; set; }

        [Column("clinome")]
        [MaxLength(50)]
        public string Nome { get; set; }

        [Column("clicpf")]
        public decimal CPF { get; set; }

        [Column("cliemail")]
        [MaxLength(100)]
        public string Email { get; set; }

        [Column("clidatacadastro")]
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        public override string ToString()
        {
            return Nome;
        }
    }
}
