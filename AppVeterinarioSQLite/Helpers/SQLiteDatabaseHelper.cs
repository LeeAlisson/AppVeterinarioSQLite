using AppVeterinarioSQLite.Models;
using SQLite;

namespace AppVeterinarioSQLite.Helpers
{
    public class SQLiteDatabaseHelpers
    {
        readonly SQLiteAsyncConnection _conn;

        public SQLiteDatabaseHelpers(string path)
        {
            _conn = new SQLiteAsyncConnection(path);

            _conn.CreateTableAsync<Especie>().Wait();
            _conn.CreateTableAsync<Animal>().Wait();
            _conn.CreateTableAsync<Cliente>().Wait();
            _conn.CreateTableAsync<AnimalCliente>().Wait();
        }

        #region Especie CRUD
        public Task<int> Insert(Especie especie)
        {
            return _conn.InsertAsync(especie);
        }

        public Task<int> Update(Especie especie)
        {
            return _conn.UpdateAsync(especie);
        }

        public Task<int> Delete(int especieId)
        {
            return _conn.Table<Especie>().DeleteAsync(e => e.Id == especieId);
        }

        public Task<List<Especie>> GetAll()
        {
            return _conn.Table<Especie>().ToListAsync();
        }

        public Task<Especie> GetEspecieById(int id)
        {
            return _conn.Table<Especie>().Where(e => e.Id == id).FirstOrDefaultAsync();
        }

        public Task<List<Especie>> Search(string nome)
        {
            return _conn.Table<Especie>().Where(e => e.Nome.Contains(nome)).ToListAsync();
        }

        public async Task<bool> PodeExcluirEspecie(int especieId)
        {
            var animais = await _conn.Table<Animal>().Where(a => a.EspecieId == especieId).CountAsync();
            return animais == 0;
        }
        #endregion

        #region Animal CRUD
        public Task<int> InsertAnimal(Animal animal)
        {
            return _conn.InsertAsync(animal);
        }

        public Task<int> UpdateAnimal(Animal animal)
        {
            return _conn.UpdateAsync(animal);
        }

        public Task<int> DeleteAnimal(int animalId)
        {
            return _conn.Table<Animal>().DeleteAsync(a => a.Id == animalId);
        }

        public async Task<List<Animal>> GetAllAnimais()
        {
            var animais = await _conn.Table<Animal>().ToListAsync();

            foreach (var animal in animais)
            {
                var especie = await _conn.Table<Especie>().Where(e => e.Id == animal.EspecieId).FirstOrDefaultAsync();
                if (especie != null)
                {
                    animal.EspecieNome = especie.Nome;
                }
                else
                {
                    animal.EspecieNome = "Não definida";
                }
            }

            return animais;
        }

        public async Task<Animal> GetAnimalById(int id)
        {
            var animal = await _conn.Table<Animal>().Where(a => a.Id == id).FirstOrDefaultAsync();

            if (animal != null)
            {
                var especie = await _conn.Table<Especie>().Where(e => e.Id == animal.EspecieId).FirstOrDefaultAsync();
                if (especie != null)
                {
                    animal.EspecieNome = especie.Nome;
                }
                else
                {
                    animal.EspecieNome = "Não definida";
                }
            }

            return animal;
        }

        public async Task<List<Animal>> SearchAnimais(string nome)
        {
            var animais = await _conn.Table<Animal>().Where(a => a.Nome.Contains(nome)).ToListAsync();

            foreach (var animal in animais)
            {
                var especie = await _conn.Table<Especie>().Where(e => e.Id == animal.EspecieId).FirstOrDefaultAsync();
                if (especie != null)
                {
                    animal.EspecieNome = especie.Nome;
                }
                else
                {
                    animal.EspecieNome = "Não definida";
                }
            }

            return animais;
        }

        public async Task<List<Animal>> GetAnimaisByEspecie(int especieId)
        {
            var animais = await _conn.Table<Animal>().Where(a => a.EspecieId == especieId).ToListAsync();

            var especie = await _conn.Table<Especie>().Where(e => e.Id == especieId).FirstOrDefaultAsync();
            if (especie != null)
            {
                foreach (var animal in animais)
                {
                    animal.EspecieNome = especie.Nome;
                }
            }

            return animais;
        }

        public async Task<bool> PodeExcluirAnimal(int animalId)
        {
            var vinculos = await _conn.Table<AnimalCliente>().Where(ac => ac.AnimalId == animalId).CountAsync();
            return vinculos == 0;
        }

        public Task<int> DeleteVinculosDoAnimal(int animalId)
        {
            return _conn.Table<AnimalCliente>().DeleteAsync(ac => ac.AnimalId == animalId);
        }
        #endregion

        #region Cliente CRUD
        public Task<int> InsertCliente(Cliente cliente)
        {
            return _conn.InsertAsync(cliente);
        }

        public Task<int> UpdateCliente(Cliente cliente)
        {
            return _conn.UpdateAsync(cliente);
        }

        public Task<int> DeleteCliente(int clienteId)
        {
            return _conn.Table<Cliente>().DeleteAsync(c => c.Id == clienteId);
        }

        public Task<List<Cliente>> GetAllClientes()
        {
            return _conn.Table<Cliente>().ToListAsync();
        }

        public Task<Cliente> GetClienteById(int id)
        {
            return _conn.Table<Cliente>().Where(c => c.Id == id).FirstOrDefaultAsync();
        }

        public Task<List<Cliente>> SearchClientes(string nome)
        {
            return _conn.Table<Cliente>().Where(c => c.Nome.Contains(nome)).ToListAsync();
        }

        public async Task<bool> PodeExcluirCliente(int clienteId)
        {
            var vinculos = await _conn.Table<AnimalCliente>().Where(ac => ac.ClienteId == clienteId).CountAsync();
            return vinculos == 0;
        }

        public Task<int> DeleteVinculosDoCliente(int clienteId)
        {
            return _conn.Table<AnimalCliente>().DeleteAsync(ac => ac.ClienteId == clienteId);
        }
        #endregion

        #region AnimalCliente CRUD
        public Task<int> InsertAnimalCliente(AnimalCliente animalCliente)
        {
            return _conn.InsertAsync(animalCliente);
        }

        public Task<int> DeleteAnimalCliente(int clienteId, int animalId)
        {
            return _conn.Table<AnimalCliente>()
                .DeleteAsync(ac => ac.ClienteId == clienteId && ac.AnimalId == animalId);
        }

        public async Task<List<AnimalCliente>> GetAnimaisByCliente(int clienteId)
        {
            var vinculos = await _conn.Table<AnimalCliente>().Where(ac => ac.ClienteId == clienteId).ToListAsync();

            foreach (var vinculo in vinculos)
            {
                var cliente = await _conn.Table<Cliente>().Where(c => c.Id == vinculo.ClienteId).FirstOrDefaultAsync();
                var animal = await _conn.Table<Animal>().Where(a => a.Id == vinculo.AnimalId).FirstOrDefaultAsync();

                if (cliente != null)
                {
                    vinculo.ClienteNome = cliente.Nome;
                }

                if (animal != null)
                {
                    vinculo.AnimalNome = animal.Nome;
                }
            }

            return vinculos;
        }

        public async Task<List<AnimalCliente>> GetClientesByAnimal(int animalId)
        {
            var vinculos = await _conn.Table<AnimalCliente>().Where(ac => ac.AnimalId == animalId).ToListAsync();

            foreach (var vinculo in vinculos)
            {
                var cliente = await _conn.Table<Cliente>().Where(c => c.Id == vinculo.ClienteId).FirstOrDefaultAsync();
                var animal = await _conn.Table<Animal>().Where(a => a.Id == vinculo.AnimalId).FirstOrDefaultAsync();

                if (cliente != null)
                {
                    vinculo.ClienteNome = cliente.Nome;
                }

                if (animal != null)
                {
                    vinculo.AnimalNome = animal.Nome;
                }
            }

            return vinculos;
        }

        public async Task<List<AnimalCliente>> GetAllVinculos()
        {
            var vinculos = await _conn.Table<AnimalCliente>().ToListAsync();

            foreach (var vinculo in vinculos)
            {
                var cliente = await _conn.Table<Cliente>().Where(c => c.Id == vinculo.ClienteId).FirstOrDefaultAsync();
                var animal = await _conn.Table<Animal>().Where(a => a.Id == vinculo.AnimalId).FirstOrDefaultAsync();

                if (cliente != null)
                {
                    vinculo.ClienteNome = cliente.Nome;
                }

                if (animal != null)
                {
                    vinculo.AnimalNome = animal.Nome;
                }
            }

            return vinculos;
        }
        #endregion
    }
}
