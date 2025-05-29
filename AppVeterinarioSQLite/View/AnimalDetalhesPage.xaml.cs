using AppVeterinarioSQLite.Models;

namespace AppVeterinarioSQLite.Views;

[QueryProperty(nameof(AnimalId), "id")]
[QueryProperty(nameof(AnimalObj), "animal")]
public partial class AnimalDetalhesPage : ContentPage
{
    private int _animalId;
    private Animal _animal;
    private bool _isEdicao = false;
    private List<Especie> _especies;

    public string AnimalId
    {
        set
        {
            _animalId = int.Parse(value);
            CarregarAnimal(_animalId);
        }
    }

    public Animal AnimalObj
    {
        set
        {
            _animal = value;
            if (_animal != null)
            {
                CarregarAnimalFromObject(_animal);
            }
        }
    }

    public AnimalDetalhesPage()
    {
        InitializeComponent();
        datePickerNascimento.Date = DateTime.Now.AddYears(-1);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarEspecies();

        if (_animal != null && _especies != null)
        {
            var especie = _especies.FirstOrDefault(e => e.Id == _animal.EspecieId);
            if (especie != null)
            {
                int index = _especies.IndexOf(especie);
                if (index >= 0)
                {
                    pickerEspecie.SelectedIndex = index;
                }
            }
        }
    }

    private async Task CarregarEspecies()
    {
        try
        {
            _especies = await App.Db.GetAll();

            pickerEspecie.Items.Clear();
            foreach (var especie in _especies)
            {
                pickerEspecie.Items.Add(especie.Nome);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Não foi possível carregar as espécies: {ex.Message}", "OK");
        }
    }

    private async void CarregarAnimal(int id)
    {
        try
        {
            _animal = await App.Db.GetAnimalById(id);
            if (_animal != null)
            {
                CarregarAnimalFromObject(_animal);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Não foi possível carregar o animal: {ex.Message}", "OK");
        }
    }

    private void CarregarAnimalFromObject(Animal animal)
    {
        _isEdicao = true;

        lblTitulo.Text = "Editar Animal";
        containerID.IsVisible = true;
        btnExcluir.IsVisible = true;
        entryID.Text = animal.Id.ToString();
        entryNome.Text = animal.Nome;
        entryApelido.Text = animal.Apelido;
        datePickerNascimento.Date = animal.DataNascimento;
        editorObservacoes.Text = animal.Observacoes;

    }

    private async void OnSalvarClicked(object sender, EventArgs e)
    {
        bool isValid = true;

        if (string.IsNullOrWhiteSpace(entryNome.Text))
        {
            lblErroNome.IsVisible = true;
            isValid = false;
        }
        else
        {
            lblErroNome.IsVisible = false;
        }

        if (pickerEspecie.SelectedIndex < 0)
        {
            lblErroEspecie.IsVisible = true;
            isValid = false;
        }
        else
        {
            lblErroEspecie.IsVisible = false;
        }

        if (!isValid) return;

        try
        {
            var especieSelecionada = _especies[pickerEspecie.SelectedIndex];

            if (_isEdicao)
            {
                _animal.Nome = entryNome.Text;
                _animal.Apelido = entryApelido.Text ?? string.Empty;
                _animal.DataNascimento = datePickerNascimento.Date;
                _animal.EspecieId = especieSelecionada.Id;
                _animal.EspecieNome = especieSelecionada.Nome;
                _animal.Observacoes = editorObservacoes.Text ?? string.Empty;

                await App.Db.UpdateAnimal(_animal);
                await DisplayAlert("Sucesso", "Animal atualizado com sucesso!", "OK");
            }
            else
            {
                var novoAnimal = new Animal
                {
                    Nome = entryNome.Text,
                    Apelido = entryApelido.Text ?? string.Empty,
                    DataNascimento = datePickerNascimento.Date,
                    EspecieId = especieSelecionada.Id,
                    EspecieNome = especieSelecionada.Nome,
                    Observacoes = editorObservacoes.Text ?? string.Empty
                };

                await App.Db.InsertAnimal(novoAnimal);
                await DisplayAlert("Sucesso", "Animal cadastrado com sucesso!", "OK");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Não foi possível salvar o animal: {ex.Message}", "OK");
        }
    }

    private async void OnCancelarClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnExcluirClicked(object sender, EventArgs e)
    {
        if (_animal == null) return;

        try
        {
            bool podeExcluir = await App.Db.PodeExcluirAnimal(_animal.Id);

            if (!podeExcluir)
            {
                bool forcarExclusao = await DisplayAlert("Atenção!",
                    $"O animal '{_animal.Nome}' possui vínculos com clientes. Deseja excluir mesmo assim? Isso também excluirá todos os vínculos.",
                    "Sim, excluir tudo", "Cancelar");

                if (!forcarExclusao) return;

                await App.Db.DeleteVinculosDoAnimal(_animal.Id);
            }
            else
            {
                bool confirmar = await DisplayAlert("Confirmar exclusão",
                    $"Deseja realmente excluir o animal '{_animal.Nome}'?",
                    "Sim", "Não");

                if (!confirmar) return;
            }

            await App.Db.DeleteAnimal(_animal.Id);
            await DisplayAlert("Sucesso", "Animal excluído com sucesso!", "OK");

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Não foi possível excluir o animal: {ex.Message}", "OK");
        }
    }
}
