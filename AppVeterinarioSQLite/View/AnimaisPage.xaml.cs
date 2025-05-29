using AppVeterinarioSQLite.Models;
using System.Collections.ObjectModel;

namespace AppVeterinarioSQLite.Views;

public partial class AnimaisPage : ContentPage
{
    private ObservableCollection<Animal> _animais;
    private List<Especie> _especies;

    public AnimaisPage()
    {
        InitializeComponent();
        _animais = new ObservableCollection<Animal>();
        collectionViewAnimais.ItemsSource = _animais;
        refreshView.Command = new Command(async () => await CarregarAnimais());
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarEspecies();
        await CarregarAnimais();
    }

    private async Task CarregarEspecies()
    {
        try
        {
            _especies = await App.Db.GetAll();

            pickerEspecie.Items.Clear();
            pickerEspecie.Items.Add("Todas as espécies");

            foreach (var especie in _especies)
            {
                pickerEspecie.Items.Add(especie.Nome);
            }

            pickerEspecie.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Não foi possível carregar as espécies: {ex.Message}", "OK");
        }
    }

    private async Task CarregarAnimais(int? especieId = null)
    {
        try
        {
            refreshView.IsRefreshing = true;

            var animais = await App.Db.GetAllAnimais();

            if (especieId.HasValue)
            {
                animais = animais.Where(a => a.EspecieId == especieId.Value).ToList();
            }

            _animais.Clear();

            foreach (var animal in animais)
            {
                _animais.Add(animal);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Não foi possível carregar os animais: {ex.Message}", "OK");
        }
        finally
        {
            refreshView.IsRefreshing = false;
        }
    }

    private async void OnPesquisaTextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.NewTextValue))
        {
            await CarregarAnimais();
        }
    }

    private async void OnPesquisarClicked(object sender, EventArgs e)
    {
        try
        {
            string textoPesquisa = entryPesquisa.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(textoPesquisa))
            {
                await CarregarAnimais();
                return;
            }

            var animais = await App.Db.SearchAnimais(textoPesquisa);
            _animais.Clear();

            foreach (var animal in animais)
            {
                _animais.Add(animal);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro na pesquisa: {ex.Message}", "OK");
        }
    }

    private async void OnEspecieFiltroChanged(object sender, EventArgs e)
    {
        if (pickerEspecie.SelectedIndex <= 0)
        {
            await CarregarAnimais();
        }
        else
        {
            int selectedIndex = pickerEspecie.SelectedIndex - 1;

            if (selectedIndex >= 0 && selectedIndex < _especies.Count)
            {
                int especieId = _especies[selectedIndex].Id;
                await CarregarAnimais(especieId);
            }
        }
    }

    private async void OnLimparFiltroClicked(object sender, EventArgs e)
    {
        pickerEspecie.SelectedIndex = 0;
        entryPesquisa.Text = string.Empty;
        await CarregarAnimais();
    }

    private async void OnAnimalSelecionado(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Animal animalSelecionado)
        {
            ((CollectionView)sender).SelectedItem = null;

            var parametros = new Dictionary<string, object>
            {
                { "animal", animalSelecionado }
            };

            await Shell.Current.GoToAsync("animais/detalhes", parametros);
        }
    }

    private async void OnAdicionarAnimalClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("animais/detalhes");
    }

    private async void OnEditarAnimal(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.BindingContext is Animal animal)
        {
            var parametros = new Dictionary<string, object>
            {
                { "animal", animal }
            };

            await Shell.Current.GoToAsync("animais/detalhes", parametros);
        }
    }

    private async void OnExcluirAnimal(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.BindingContext is Animal animal)
        {
            try
            {
                bool podeExcluir = await App.Db.PodeExcluirAnimal(animal.Id);

                if (!podeExcluir)
                {
                    bool forcarExclusao = await DisplayAlert("Atenção!",
                        $"O animal '{animal.Nome}' possui vínculos com clientes. Deseja excluir mesmo assim? Isso também excluirá todos os vínculos.",
                        "Sim, excluir tudo", "Cancelar");

                    if (!forcarExclusao) return;

                    // Deletar todos os vínculos do animal
                    await App.Db.DeleteVinculosDoAnimal(animal.Id);
                }
                else
                {
                    bool confirmar = await DisplayAlert("Confirmar exclusão",
                        $"Deseja realmente excluir o animal '{animal.Nome}'?",
                        "Sim", "Não");

                    if (!confirmar) return;
                }

                await App.Db.DeleteAnimal(animal.Id);
                await DisplayAlert("Sucesso", "Animal excluído com sucesso!", "OK");
                await CarregarAnimais();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Não foi possível excluir o animal: {ex.Message}", "OK");
            }
        }
    }
}
