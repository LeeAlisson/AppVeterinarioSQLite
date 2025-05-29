using AppVeterinarioSQLite.Models;
using System.Collections.ObjectModel;

namespace AppVeterinarioSQLite.Views;

public partial class EspeciesPage : ContentPage
{
    private ObservableCollection<Especie> _especies;

    public EspeciesPage()
    {
        InitializeComponent();
        _especies = new ObservableCollection<Especie>();
        collectionViewEspecies.ItemsSource = _especies;
        refreshView.Command = new Command(async () => await CarregarEspecies());
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarEspecies();
    }

    private async Task CarregarEspecies()
    {
        try
        {
            refreshView.IsRefreshing = true;

            var especies = await App.Db.GetAll();
            _especies.Clear();

            foreach (var especie in especies)
            {
                _especies.Add(especie);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Não foi possível carregar as espécies: {ex.Message}", "OK");
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
            await CarregarEspecies();
        }
    }

    private async void OnPesquisarClicked(object sender, EventArgs e)
    {
        try
        {
            string textoPesquisa = entryPesquisa.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(textoPesquisa))
            {
                await CarregarEspecies();
                return;
            }

            var especies = await App.Db.Search(textoPesquisa);
            _especies.Clear();

            foreach (var especie in especies)
            {
                _especies.Add(especie);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro na pesquisa: {ex.Message}", "OK");
        }
    }

    private async void OnEspecieSelecionada(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Especie especieSelecionada)
        {
            ((CollectionView)sender).SelectedItem = null;

            var parametros = new Dictionary<string, object>
            {
                { "especie", especieSelecionada }
            };

            await Shell.Current.GoToAsync("especies/detalhes", parametros);
        }
    }

    private async void OnAdicionarEspecieClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("especies/detalhes");
    }

    private async void OnEditarEspecie(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.BindingContext is Especie especie)
        {
            var parametros = new Dictionary<string, object>
            {
                { "especie", especie }
            };

            await Shell.Current.GoToAsync("especies/detalhes", parametros);
        }
    }

    private async void OnExcluirEspecie(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.BindingContext is Especie especie)
        {
            try
            {
                bool podeExcluir = await App.Db.PodeExcluirEspecie(especie.Id);

                if (!podeExcluir)
                {
                    bool forcarExclusao = await DisplayAlert("Atenção!",
                        $"A espécie '{especie.Nome}' possui animais vinculados. Deseja excluir mesmo assim? Isso também excluirá todos os animais desta espécie e seus vínculos.",
                        "Sim, excluir tudo", "Cancelar");

                    if (!forcarExclusao) return;

                    var animais = await App.Db.GetAnimaisByEspecie(especie.Id);

                    foreach (var animal in animais)
                    {
                        await App.Db.DeleteVinculosDoAnimal(animal.Id);
                    }

                    foreach (var animal in animais)
                    {
                        await App.Db.DeleteAnimal(animal.Id);
                    }
                }
                else
                {
                    bool confirmar = await DisplayAlert("Confirmar exclusão",
                        $"Deseja realmente excluir a espécie '{especie.Nome}'?",
                        "Sim", "Não");

                    if (!confirmar) return;
                }

                await App.Db.Delete(especie.Id);
                await DisplayAlert("Sucesso", "Espécie excluída com sucesso!", "OK");
                await CarregarEspecies();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Não foi possível excluir a espécie: {ex.Message}", "OK");
            }
        }
    }
}
