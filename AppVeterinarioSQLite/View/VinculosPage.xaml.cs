using AppVeterinarioSQLite.Models;
using System.Collections.ObjectModel;

namespace AppVeterinarioSQLite.Views;

public partial class VinculosPage : ContentPage
{
    private ObservableCollection<AnimalCliente> _vinculos;

    public VinculosPage()
    {
        InitializeComponent();
        _vinculos = new ObservableCollection<AnimalCliente>();
        collectionViewVinculos.ItemsSource = _vinculos;
        refreshView.Command = new Command(async () => await CarregarVinculos());
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarVinculos();
    }

    private async Task CarregarVinculos()
    {
        try
        {
            refreshView.IsRefreshing = true;

            var vinculos = await App.Db.GetAllVinculos();
            _vinculos.Clear();

            foreach (var vinculo in vinculos)
            {
                _vinculos.Add(vinculo);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"N�o foi poss�vel carregar os v�nculos: {ex.Message}", "OK");
        }
        finally
        {
            refreshView.IsRefreshing = false;
        }
    }

    private async void OnVinculoSelecionado(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is AnimalCliente vinculoSelecionado)
        {
            ((CollectionView)sender).SelectedItem = null;

            await DisplayAlert("Detalhes do V�nculo",
                $"Cliente: {vinculoSelecionado.ClienteNome}\nAnimal: {vinculoSelecionado.AnimalNome}",
                "OK");
        }
    }

    private async void OnAdicionarVinculoClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("vinculos/detalhes");
    }

    private async void OnExcluirVinculo(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.BindingContext is AnimalCliente vinculo)
        {
            bool confirmar = await DisplayAlert("Confirmar exclus�o",
                $"Deseja realmente excluir o v�nculo entre '{vinculo.ClienteNome}' e '{vinculo.AnimalNome}'?",
                "Sim", "N�o");

            if (confirmar)
            {
                try
                {
                    await App.Db.DeleteAnimalCliente(vinculo.ClienteId, vinculo.AnimalId);
                    await DisplayAlert("Sucesso", "V�nculo exclu�do com sucesso!", "OK");
                    await CarregarVinculos();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Erro", $"N�o foi poss�vel excluir o v�nculo: {ex.Message}", "OK");
                }
            }
        }
    }
}
